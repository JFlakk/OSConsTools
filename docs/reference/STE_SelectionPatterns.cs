// =====================================================================================
// REFERENCE ONLY - not compiled (lives under docs/, outside the Assemblies glob).
// SQL Table Editor (STE / "TED") selection & parameter patterns for OneStream dashboards.
//
// Covers, in one Dashboard Extender BR:
//   Pattern A - Resolve a DEPENDENT parameter before the grid binds (the IV_->BL_ trick).
//   Pattern B - PERSISTENT MULTI-SELECT via a staging table (select 6, add more later, never lose them).
//   Pattern C - SINGLE-SELECT that survives a top-frame refresh (why the naive bind gets wiped).
//
// Idioms match FMM_ConfigData.cs / DDM_ConfigLoadDB.cs: GBL command builder, XFEditedDataRow,
// CustomSubstVars, ModifiedCustomSubstVars + ChangeCustomSubstVarsInDashboard.
// Replace the __PLACEHOLDER__ table/column/param names with your own.
//
// BINDING REALITY (verified from the workspace XML - all SqlTableEditor components):
//   * A TED's own boundParameterName is ALWAYS an IV_ when set (the grid emits ONE literal:
//     the clicked row's key / edited cell). 5 of the STEs bind IV_, ZERO bind BL_/DL_.
//   * It is often EMPTY (11 of them) - then you read every change from EditedDataRows on save.
//   * BL_ is the PARENT PICKER (a combo/BoundList choosing which config's rows to load),
//     NOT the grid's binding. IV_ has no list + no default-reselect, so a Refresh/Redraw wipes
//     it with nothing to restore from - which is exactly why selection must live in a table.
// =====================================================================================
using System;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;
using OneStream.Shared.Common;
using OneStream.Shared.Engine;
using OneStream.Shared.Wcf;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.STE_SelectionPatterns
{
    public class MainClass
    {
        private SessionInfo si;
        private BRGlobals globals;
        private object api;
        private DashboardExtenderArgs args;

        // --- names you wire to your dashboard (see BINDING REALITY note in the header) ---
        private const string Param_TedRowBinding  = "IV_SelectedRowID";       // the TED's OWN boundParameterName - a literal row key (or "" and use EditedDataRows)
        private const string Param_SelectedRowIDs = "IV_SelectedRowIDs";      // delimited accumulator, e.g. "12|15|22"
        private const string Param_ParentKey      = "BL_ParentConfigID";      // PARENT PICKER (a real BoundList), NOT the grid binding
        private const string Param_DependentChild = "BL_ChildOfParent";       // param whose list depends on the parent
        private const string SelectionTable       = "STE_RowSelection";       // staging table (see DDL note at bottom)

        public object Main(SessionInfo si, BRGlobals globals, object api, DashboardExtenderArgs args)
        {
            try
            {
                this.si = si; this.globals = globals; this.api = api; this.args = args;

                switch (args.FunctionType)
                {
                    case DashboardExtenderFunctionType.LoadDashboard:
                        return OnLoad();

                    case DashboardExtenderFunctionType.SqlTableEditorSaveData:
                        return OnSaveSelection();
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        // =============================================================================
        // PATTERN A - resolve a DEPENDENT parameter BEFORE the grid binds.
        //   The grid substitutes |!param!| once at bind and never re-runs the cascade,
        //   so a child param that depends on a parent must be resolved here, at
        //   BeforeFirstGetParameters, or it is empty when the grid's SQL runs.
        // =============================================================================
        private XFLoadDashboardTaskResult OnLoad()
        {
            var result = new XFLoadDashboardTaskResult { ChangeCustomSubstVarsInDashboard = true };

            // Only seed on the FIRST gather. Subsequent gathers already have the value.
            bool firstPass = args.LoadDashboardTaskInfo.Action == LoadDashboardActionType.BeforeFirstGetParameters;
            if (!firstPass) return result;

            var resolved = args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved;

            // Read the parent value the child depends on (already resolved this pass, or from prior run).
            string parentId = resolved.XFGetValue(Param_ParentKey,
                                 args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue(Param_ParentKey, "0"));

            // Resolve the dependent child's DEFAULT explicitly - the grid won't cascade it for you.
            // (This is the generalized form of DDM_ConfigLoadDB.getDefaultParam's IV_->BL_ trick:
            //  an IV_ has no bound list, so map to its BL_ twin and take the first bound item.)
            string childDefault = ResolveBoundListDefault(Param_DependentChild, resolved);
            UpsertSubstVar(result, Param_DependentChild, childDefault);

            // PATTERN C hook: re-assert the persisted single/multi selection so a refresh does NOT wipe it.
            // Rebuild the accumulator from the staging table (source of truth), not from the volatile grid.
            string persisted = ReadSelectedIdsFromTable(parentId);
            UpsertSubstVar(result, Param_SelectedRowIDs, persisted);

            return result;
        }

        // Map IV_->BL_ (an InputValue has no list), then take the bound list's first item as the default.
        private string ResolveBoundListDefault(string param, System.Collections.Generic.Dictionary<string, string> csv)
        {
            if (param.Contains("IV_")) param = param.Replace("IV_", "BL_");

            var info = BRApi.Dashboards.Parameters.GetParameterDisplayInfo(
                          si, false, csv, args.PrimaryDashboard.WorkspaceID, param);

            if (info?.ComboBoxItemsForBoundList?.Count > 0)
                return info.ComboBoxItemsForBoundList.First().Value.ToString();

            return string.Empty;
        }

        // =============================================================================
        // PATTERN B - PERSISTENT MULTI-SELECT via a staging table.
        //
        //   WHY the grid's own selection is a dead end:
        //   - A row-highlight selection is transient UI state; a redraw re-runs the SQL and drops it.
        //   - A bound BL_ parameter RE-EVALUATES against its freshly queried list every gather, so on
        //     refresh it snaps back to its default -> your selected value is wiped.
        //
        //   THE FIX: don't store selection in the grid or the bound param. Store it in a table the grid
        //   RE-READS every render. Give the STE SQL a "Selected" bit column via LEFT JOIN to that table
        //   (make it an editable checkbox). Then:
        //     - refresh re-reads the join   -> checkmarks always persist
        //     - selecting 6 then 3 more     -> just toggles 3 more rows; the set ACCUMULATES
        //     - downstream consumers read the SELECTED SET from the table (or a delimited param built from it)
        //
        //   STE column SQL (illustrative):
        //     SELECT r.RowID, r.Name, ...,
        //            CAST(CASE WHEN s.RowID IS NULL THEN 0 ELSE 1 END AS bit) AS Selected
        //     FROM   SourceRows r
        //     LEFT JOIN STE_RowSelection s
        //            ON s.RowID = r.RowID AND s.SessionKey = '|!IV_SessionKey!|'
        // =============================================================================
        private XFSqlTableEditorSaveDataTaskResult OnSaveSelection()
        {
            var result = new XFSqlTableEditorSaveDataTaskResult();
            var info   = args.SqlTableEditorSaveDataTaskInfo;

            // Scope key so different users/parents keep independent selection sets.
            string sessionKey = info.CustomSubstVars.XFGetValue("IV_SessionKey", si.UserName);
            string parentId   = info.CustomSubstVars.XFGetValue(Param_ParentKey, "0");

            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConn.ConnectionString))
            {
                connection.Open();
                var cmd = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);

                // UPSERT only the rows the user actually toggled. We do NOT clear the table first -
                // that is what makes the selection ACCUMULATE across visits instead of resetting.
                foreach (XFEditedDataRow xfRow in info.EditedDataRows)
                {
                    // Deletes here mean "row removed from the grid", not "deselect"; guard as needed.
                    if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Delete) continue;

                    var row      = xfRow.ModifiedDataRow;
                    long rowId   = Convert.ToInt64(row["RowID"]);
                    bool selected = row.Table.Columns.Contains("Selected")
                                    && row["Selected"] != DBNull.Value
                                    && Convert.ToBoolean(row["Selected"]);

                    if (selected)
                        UpsertSelection(cmd, connection, sessionKey, parentId, rowId);   // add / keep
                    else
                        RemoveSelection(cmd, connection, sessionKey, parentId, rowId);   // explicit deselect
                }
            }

            // Optional: publish the accumulated set as a delimited param for downstream (charts, saves, DM).
            // Do this in the LOAD extender via UpsertSubstVar(Param_SelectedRowIDs, ...) so it survives redraws.

            result.IsOK = true;
            return result;
        }

        // =============================================================================
        // Staging-table helpers (MERGE = upsert). Table DDL at the bottom of this file.
        // =============================================================================
        private void UpsertSelection(GBL_UI_Assembly.SQA_GBL_Command_Builder cmd, SqlConnection cn,
                                     string sessionKey, string parentId, long rowId)
        {
            var sql = $@"MERGE {SelectionTable} AS t
                         USING (SELECT @SessionKey AS SessionKey, @ParentId AS ParentId, @RowId AS RowID) AS s
                         ON (t.SessionKey = s.SessionKey AND t.ParentId = s.ParentId AND t.RowID = s.RowID)
                         WHEN NOT MATCHED THEN
                             INSERT (SessionKey, ParentId, RowID, UpdateUser, UpdateDate)
                             VALUES (s.SessionKey, s.ParentId, s.RowID, @User, GETDATE());";
            var p = new[]
            {
                new SqlParameter("@SessionKey", SqlDbType.NVarChar, 100) { Value = sessionKey },
                new SqlParameter("@ParentId",   SqlDbType.NVarChar, 50)  { Value = parentId },
                new SqlParameter("@RowId",      SqlDbType.BigInt)         { Value = rowId },
                new SqlParameter("@User",       SqlDbType.NVarChar, 100) { Value = si.UserName }
            };
            cmd.ExecuteNonQuery(si, sql, p);   // use your GBL builder's non-query entry point
        }

        private void RemoveSelection(GBL_UI_Assembly.SQA_GBL_Command_Builder cmd, SqlConnection cn,
                                     string sessionKey, string parentId, long rowId)
        {
            var sql = $@"DELETE FROM {SelectionTable}
                         WHERE SessionKey = @SessionKey AND ParentId = @ParentId AND RowID = @RowId;";
            var p = new[]
            {
                new SqlParameter("@SessionKey", SqlDbType.NVarChar, 100) { Value = sessionKey },
                new SqlParameter("@ParentId",   SqlDbType.NVarChar, 50)  { Value = parentId },
                new SqlParameter("@RowId",      SqlDbType.BigInt)         { Value = rowId }
            };
            cmd.ExecuteNonQuery(si, sql, p);
        }

        // Read the accumulated set back as a delimited string (e.g. "12|15|22") for a param default.
        private string ReadSelectedIdsFromTable(string parentId)
        {
            var dt = new DataTable();
            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConn.ConnectionString))
            {
                var sqa = new SqlDataAdapter();
                var loader = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                var sql = $@"SELECT RowID FROM {SelectionTable} WHERE ParentId = @ParentId ORDER BY RowID";
                var p = new[] { new SqlParameter("@ParentId", SqlDbType.NVarChar, 50) { Value = parentId } };
                loader.Fill_Get_GBL_DT(si, sqa, dt, sql, p);
            }
            return string.Join("|", dt.AsEnumerable().Select(r => r["RowID"].ToString()));
        }

        // Write a subst var so it survives the round-trip (mirrors DDM_LoadDB.UpdateCustomSubstVar).
        private void UpsertSubstVar(XFLoadDashboardTaskResult result, string key, string value)
        {
            if (result.ModifiedCustomSubstVars.ContainsKey(key)) result.ModifiedCustomSubstVars.XFSetValue(key, value);
            else                                                 result.ModifiedCustomSubstVars.Add(key, value);
            globals.SetStringValue(key, value);
        }
    }
}

/* -------------------------------------------------------------------------------------
   Staging table DDL (application DB). One row per selected grid row, scoped per user+parent.

   CREATE TABLE STE_RowSelection (
       SessionKey  NVARCHAR(100) NOT NULL,   -- si.UserName or an explicit IV_SessionKey
       ParentId    NVARCHAR(50)  NOT NULL,   -- the BL_ParentConfigID this selection belongs to
       RowID       BIGINT        NOT NULL,   -- the source row's key
       UpdateUser  NVARCHAR(100) NULL,
       UpdateDate  DATETIME      NULL,
       CONSTRAINT PK_STE_RowSelection PRIMARY KEY (SessionKey, ParentId, RowID)
   );

   Why this beats the bound-parameter approach:
   - Persistence is explicit and server-side, so a redraw/refresh CANNOT wipe it.
   - Accumulation is free: adding rows later just inserts more keys; the PK dedupes.
   - Multi-select is just N rows in the table; single-select is the same table capped at 1.
   - Any downstream (chart adapter, DM sequence, another dashboard) reads the same set -
     no fragile delimited subst var to keep in sync.
--------------------------------------------------------------------------------------- */
