// =====================================================================================
// REFERENCE ONLY - not compiled (lives under docs/, outside the Assemblies glob).
// FMM Cube-Load Engine: ONE config-driven engine that replaces the per-module,
// hardcoded Load_Reqs_to_Cube copies in RMW. Reads FMM_CubeLoadConfig + ColMap
// (see FMM_ConfigTables.sql), builds a bulk DataBuffer, and writes once per POV.
//
// WHAT IT FIXES vs RMW:
//   * mapping is DATA (FMM_CubeLoadColMap), not VB literals -> finance users edit it in a grid
//   * DELTA mode: skip POVs whose source rows are unchanged (hash vs FMM_CubeLoadWatermark)
//   * DICTIONARY lookup instead of DataTable.Select-per-cell (RMW's O(cells x rows))
//   * keeps the good part - one api.Data.SetDataBuffer per POV (bulk, not per-cell)
//
// Call from a DM CustomCalculate step or a FinCustCalc function, once per POV entity/time.
// Replace __PLACEHOLDER__ helpers with your GBL equivalents.
// =====================================================================================
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;
using OneStream.Finance.Engine;
using OneStream.Shared.Common;
using OneStream.Shared.Engine;

namespace Workspace.OSConsTools.FMM_UI_Assembly
{
    public sealed class FMM_CubeLoadEngine
    {
        // ---- config records loaded once per run ----
        private sealed class LoadConfig
        {
            public int LoadID;
            public string SourceTable, TargetScenarioExpr, TargetView, OriginExpr, EntityScopeExpr, TimeScopeExpr;
            public int LoadMode;         // 1=FullReplace, 2=Delta
            public bool ClearUnmatched;
        }
        private sealed class ColMap
        {
            public string SourceColumn, TargetDimType, TargetMemberExpr;
            public bool IsAmount;
        }

        /// <summary>
        /// Load one POV (entity/time/scenario already resolved by the caller/DM step) from the
        /// configured staging table into the cube. Returns rows written (0 if skipped by delta).
        /// </summary>
        public int LoadPov(SessionInfo si, object apiObj, int loadId, string entity, string time, string scenario)
        {
            var api = (FinanceRulesApi)apiObj;   // shape per your FinCustCalc api arg
            var cfg = ReadConfig(si, loadId);
            var map = ReadColMap(si, loadId);
            string povKey = $"{entity}|{scenario}|{time}";

            // ---- DELTA: skip unchanged POVs ------------------------------------------------
            if (cfg.LoadMode == 2)
            {
                var hash = ComputeSourceHash(si, cfg.SourceTable, entity, time, scenario);
                if (hash == ReadWatermark(si, loadId, povKey))
                {
                    // BRApi.ErrorLog.LogMessage(si, $"CubeLoad {loadId} POV {povKey} unchanged - skipped");
                    return 0;
                }
            }

            // ---- read the source slice ONCE -----------------------------------------------
            var src = ReadSourceSlice(si, cfg, entity, time, scenario);   // DataTable

            // ---- build the destination buffer from the col map ----------------------------
            var destBuffer = new DataBuffer();
            var amountCol = map.FirstOrDefault(m => m.IsAmount)?.SourceColumn;

            foreach (DataRow row in src.Rows)
            {
                var cellPk = new DataBufferCellPk();
                cellPk.SetEntity(api, entity);
                cellPk.SetScenario(api, scenario);
                cellPk.SetTime(api, time);
                cellPk.SetView(api, cfg.TargetView);
                if (!string.IsNullOrEmpty(cfg.OriginExpr))
                    cellPk.SetOrigin(api, Resolve(cfg.OriginExpr, row));

                // Data-driven dimension assignment - the whole point. No hardcoded SetAccount("...").
                foreach (var m in map)
                {
                    if (m.IsAmount) continue;
                    string member = Resolve(m.TargetMemberExpr, row);   // literal, {col}, or |!token!|
                    ApplyDim(api, cellPk, m.TargetDimType, member);
                }

                var cell = new DataBufferCell(cellPk);
                double amt = amountCol != null && row[amountCol] != DBNull.Value
                             ? Convert.ToDouble(row[amountCol]) : 0d;
                cell.UpdateValue(null, amt, DataCellStatus.IsRealData, api.CalcStatusCache);
                destBuffer.SetCell(si, cell);
            }

            // ---- one bulk write per POV (keep RMW's efficient part) ------------------------
            var destInfo = new DataBufferInfo();  // set cube/scope per your api
            api.Data.SetDataBuffer(destBuffer, destInfo, /* clearCalcData */ false);

            // ---- optional self-reconciling clear of cells no longer in source -------------
            if (cfg.ClearUnmatched)
                ClearUnmatched(si, api, cfg, destBuffer, entity, time, scenario);

            if (cfg.LoadMode == 2)
                WriteWatermark(si, loadId, povKey, ComputeSourceHash(si, cfg.SourceTable, entity, time, scenario));

            return src.Rows.Count;
        }

        // ---- dimension dispatch (data-driven) ---------------------------------------------
        private static void ApplyDim(FinanceRulesApi api, DataBufferCellPk pk, string dimType, string member)
        {
            switch (dimType.ToUpperInvariant())
            {
                case "ACCOUNT": pk.SetAccount(api, member); break;
                case "FLOW":    pk.SetFlow(api, member); break;
                case "IC":      pk.SetIC(api, member); break;
                case "UD1":     pk.SetUD1(api, member); break;
                case "UD2":     pk.SetUD2(api, member); break;
                case "UD3":     pk.SetUD3(api, member); break;
                case "UD4":     pk.SetUD4(api, member); break;
                case "UD5":     pk.SetUD5(api, member); break;
                case "UD6":     pk.SetUD6(api, member); break;
                case "UD7":     pk.SetUD7(api, member); break;
                case "UD8":     pk.SetUD8(api, member); break;
                // Entity/Scenario/Time/View come from the POV, not the row.
            }
        }

        // Resolve a TargetMemberExpr: literal member, "{Column}" to use the row value, or leave token as-is.
        private static string Resolve(string expr, DataRow row)
        {
            if (string.IsNullOrEmpty(expr)) return string.Empty;
            if (expr.StartsWith("{") && expr.EndsWith("}"))
            {
                var col = expr.Trim('{', '}');
                return row.Table.Columns.Contains(col) && row[col] != DBNull.Value ? row[col].ToString() : string.Empty;
            }
            return expr;   // literal (or a |!token!| the DM step already substituted)
        }

        // ---- config + source reads (GBL helpers) ------------------------------------------
        private LoadConfig ReadConfig(SessionInfo si, int loadId)
        {
            var dt = QueryApp(si, "SELECT * FROM FMM_CubeLoadConfig WHERE LoadID=@id",
                              new SqlParameter("@id", loadId));
            var r = dt.Rows[0];
            return new LoadConfig {
                LoadID = loadId,
                SourceTable = r["SourceTable"].ToString(),
                TargetScenarioExpr = r["TargetScenarioExpr"]?.ToString(),
                TargetView = r["TargetViewExpr"].ToString(),
                OriginExpr = r["OriginExpr"]?.ToString(),
                EntityScopeExpr = r["EntityScopeExpr"]?.ToString(),
                TimeScopeExpr = r["TimeScopeExpr"]?.ToString(),
                LoadMode = Convert.ToInt32(r["LoadMode"]),
                ClearUnmatched = Convert.ToBoolean(r["ClearUnmatched"])
            };
        }

        private List<ColMap> ReadColMap(SessionInfo si, int loadId)
        {
            var dt = QueryApp(si, "SELECT * FROM FMM_CubeLoadColMap WHERE LoadID=@id ORDER BY SortOrder",
                              new SqlParameter("@id", loadId));
            return dt.AsEnumerable().Select(r => new ColMap {
                SourceColumn = r["SourceColumn"].ToString(),
                TargetDimType = r["TargetDimType"].ToString(),
                TargetMemberExpr = r["TargetMemberExpr"]?.ToString(),
                IsAmount = Convert.ToBoolean(r["IsAmount"])
            }).ToList();
        }

        private DataTable ReadSourceSlice(SessionInfo si, LoadConfig cfg, string entity, string time, string scenario)
        {
            // Parameterized - NOT string-concatenated IN(...) like RMW.
            var sql = $"SELECT * FROM {cfg.SourceTable} WITH (NOLOCK) " +
                      "WHERE Entity=@e AND WFTime_Name=@t AND WFScenario_Name=@s";
            return QueryApp(si, sql,
                new SqlParameter("@e", entity), new SqlParameter("@t", time), new SqlParameter("@s", scenario));
        }

        private string ComputeSourceHash(SessionInfo si, string table, string entity, string time, string scenario)
        {
            var sql = $"SELECT CONVERT(varchar(64), HASHBYTES('SHA2_256', " +
                      $"(SELECT * FROM {table} WITH (NOLOCK) WHERE Entity=@e AND WFTime_Name=@t AND WFScenario_Name=@s " +
                      "FOR XML RAW)), 2) AS h";
            var dt = QueryApp(si, sql,
                new SqlParameter("@e", entity), new SqlParameter("@t", time), new SqlParameter("@s", scenario));
            return dt.Rows.Count > 0 ? dt.Rows[0]["h"]?.ToString() ?? "" : "";
        }

        private string ReadWatermark(SessionInfo si, int loadId, string povKey)
        {
            var dt = QueryApp(si, "SELECT CONVERT(varchar(64),RowHash,2) AS h FROM FMM_CubeLoadWatermark WHERE LoadID=@id AND POVKey=@k",
                              new SqlParameter("@id", loadId), new SqlParameter("@k", povKey));
            return dt.Rows.Count > 0 ? dt.Rows[0]["h"]?.ToString() ?? "" : "";
        }

        private void WriteWatermark(SessionInfo si, int loadId, string povKey, string hash)
        {
            ExecApp(si, @"MERGE FMM_CubeLoadWatermark AS t
                          USING (SELECT @id AS LoadID, @k AS POVKey) AS s ON (t.LoadID=s.LoadID AND t.POVKey=s.POVKey)
                          WHEN MATCHED THEN UPDATE SET RowHash=CONVERT(binary(32),@h,2), LastLoaded=GETDATE()
                          WHEN NOT MATCHED THEN INSERT (LoadID,POVKey,RowHash,LastLoaded)
                              VALUES (@id,@k,CONVERT(binary(32),@h,2),GETDATE());",
                    new SqlParameter("@id", loadId), new SqlParameter("@k", povKey), new SqlParameter("@h", hash));
        }

        private void ClearUnmatched(SessionInfo si, FinanceRulesApi api, LoadConfig cfg, DataBuffer dest,
                                    string entity, string time, string scenario)
        {
            // Fetch current cube slice with REMOVEZeros, zero any cell not present in dest, one SetDataBuffer.
            // (Same self-reconciling pattern RMW uses - kept, just centralized.)
        }

        // ---- thin DB helpers (delegate to your GBL command builder) ------------------------
        private static DataTable QueryApp(SessionInfo si, string sql, params SqlParameter[] p)
        {
            var dt = new DataTable();
            var conn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var c = new SqlConnection(conn.ConnectionString))
            {
                var loader = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, c);
                loader.Fill_Get_GBL_DT(si, new SqlDataAdapter(), dt, sql, p);
            }
            return dt;
        }
        private static void ExecApp(SessionInfo si, string sql, params SqlParameter[] p)
        {
            var conn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var c = new SqlConnection(conn.ConnectionString))
            {
                c.Open();
                var cmd = new SqlCommand(sql, c);
                cmd.Parameters.AddRange(p);
                cmd.ExecuteNonQuery();
            }
        }
    }
}

/* -------------------------------------------------------------------------------------
   HOW THE OTHER FOUR ENGINES REUSE THIS SHAPE:
     Cube->Table  : FDX read (FdxExecuteCubeView) -> map FdxColumn->TargetColumn (FMM_CubeExtractColMap)
                    -> SqlBulkCopy to TargetTable; delta by IsKey columns. Mirror image of this class.
     Approval     : read FMM_WorkflowTransitions for (state,action)->newState instead of a VB dictionary;
                    apply with ONE set-based UPDATE ... FROM (VALUES ...) and ONE batched FMM_AuditLog insert.
     Validation   : read FMM_ValidationRules, run each (SQL/CubeView/Expression) as async jobs (OSDAI pattern),
                    write FMM_ValidationResults; parse any XML once.
     Modeling     : same bulk-DataBuffer build, but driver/target coords come from XFC_*Cycle_Param_Values,
                    and per-cell DataTable.Select is replaced by a Dictionary keyed on the join columns.
   One engine + one config table per construct = configurable for finance users, optimal by construction.
--------------------------------------------------------------------------------------- */
