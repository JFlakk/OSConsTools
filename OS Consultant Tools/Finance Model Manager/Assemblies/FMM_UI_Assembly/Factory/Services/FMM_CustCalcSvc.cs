using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.CSharp;
using OneStream.Finance.Database;
using OneStream.Finance.Engine;
using OneStream.Shared.Common;
using OneStream.Shared.Database;
using OneStream.Shared.Engine;
using OneStream.Shared.Wcf;
using OneStream.Stage.Database;
using OneStream.Stage.Engine;
using OneStreamWorkspacesApi;
using OneStreamWorkspacesApi.V800;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
	public class FMM_CustCalcSvc : IWsasFinanceCustomCalculateV800
	{
        public void CustomCalculate(SessionInfo si, BRGlobals brGlobals, FinanceRulesApi api, FinanceRulesArgs args)
        {
            try
            {
                var funcName = args?.CustomCalculateArgs?.FunctionName ?? string.Empty;

                // Only handle Table calc execution
                if (!funcName.XFEqualsIgnoreCase("ExecuteTableCalcs"))
                    return;

                using (var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si))
                {
                    // Load all active Table CalcConfig rows
                    var calcConfigSql = @"
                        SELECT cc.CalcConfigID, cc.Name, cc.Table_Calc_Logic, cc.Table_Calc_SQL_Logic,
                               cc.gbl_Status
                        FROM FMM_CalcConfig cc
                        WHERE cc.gbl_CalcLogic_Table = 1
                          AND cc.gbl_Status IN ('Active','Build')
                        ORDER BY cc.Sequence";

                    var dtCalcConfigs = BRApi.Database.ExecuteQuery(dbConn, false, calcConfigSql, null);

                    foreach (DataRow calcRow in dtCalcConfigs.Rows)
                    {
                        int calcConfigID = calcRow.Field<int>("CalcConfigID");
                        ExecuteTableCalc(si, dbConn, calcConfigID);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        private void ExecuteTableCalc(SessionInfo si, DbConnInfoApp dbConn, int calcConfigID)
        {
            // Load src cell config rows ordered by SrcOrder
            var srcSql = @"
                SELECT SrcCellConfigID, SrcOrder, Type, Item,
                       Table_Calc_Expression, Table_Join_Expression, Table_Filter_Expression,
                       Table_JoinType
                FROM FMM_SrcCellConfig
                WHERE CalcConfigID = @calcConfigID
                  AND (Type IS NOT NULL AND Type <> '')
                ORDER BY SrcOrder";

            var srcParams = new List<DbParamInfo>
            {
                new DbParamInfo("@calcConfigID", calcConfigID)
            };

            var dtSrc = BRApi.Database.ExecuteQuery(dbConn, false, srcSql, srcParams);

            if (dtSrc.Rows.Count == 0)
                return;

            // Build SELECT / FROM / JOIN / WHERE SQL from src cell configs
            var sqlBuilder = new System.Text.StringBuilder();
            DataRow primaryRow = dtSrc.Rows[0];
            string primaryTable = (primaryRow.Field<string>("Item") ?? string.Empty).Trim();
            string primaryCalcExpr = (primaryRow.Field<string>("Table_Calc_Expression") ?? string.Empty).Trim();
            string primaryFilter = (primaryRow.Field<string>("Table_Filter_Expression") ?? string.Empty).Trim();

            // SELECT clause: use CalcExpression of primary row or * fallback
            sqlBuilder.Append("SELECT ");
            sqlBuilder.AppendLine(string.IsNullOrWhiteSpace(primaryCalcExpr) ? "A.*" : primaryCalcExpr);
            sqlBuilder.AppendLine($"FROM {primaryTable} A");

            // JOIN clauses for each additional src row
            char tableAlias = 'B';
            for (int i = 1; i < dtSrc.Rows.Count; i++)
            {
                DataRow joinRow = dtSrc.Rows[i];
                string joinTable = (joinRow.Field<string>("Item") ?? string.Empty).Trim();
                string joinExpr = (joinRow.Field<string>("Table_Join_Expression") ?? string.Empty).Trim();
                string joinType = (joinRow.Field<string>("Table_JoinType") ?? string.Empty).Trim().ToUpperInvariant();

                // Default to INNER JOIN when not specified
                string joinKeyword = joinType switch
                {
                    "LEFT" or "2" => "LEFT JOIN",
                    "FULL OUTER" or "3" => "FULL OUTER JOIN",
                    _ => "INNER JOIN"
                };

                if (string.IsNullOrWhiteSpace(joinExpr))
                    joinExpr = "1=1"; // fallback — admin must configure a real expression

                sqlBuilder.AppendLine($"{joinKeyword} {joinTable} {tableAlias} ON {joinExpr}");
                tableAlias++;
            }

            // WHERE clause: use filter from the primary row if provided
            if (!string.IsNullOrWhiteSpace(primaryFilter))
                sqlBuilder.AppendLine($"WHERE {primaryFilter}");

            string execSql = sqlBuilder.ToString();

            // Execute the built SQL
            var dtResults = BRApi.Database.ExecuteQuery(dbConn, false, execSql, null);

            // Write results to the configured destination table
            WriteResultsToDestTable(si, dbConn, calcConfigID, dtResults);

            // Persist the generated SQL for audit / debugging
            var updateSql = @"
                UPDATE FMM_CalcConfig
                SET Table_Calc_SQL_Logic = @sqlLogic
                WHERE CalcConfigID = @calcConfigID";

            var updateParams = new List<DbParamInfo>
            {
                new DbParamInfo("@sqlLogic", execSql),
                new DbParamInfo("@calcConfigID", calcConfigID)
            };

            BRApi.Database.ExecuteActionQuery(dbConn, updateSql, updateParams, false, true);
        }

        private void WriteResultsToDestTable(SessionInfo si, DbConnInfoApp dbConn, int calcConfigID, DataTable dtResults)
        {
            if (dtResults == null || dtResults.Rows.Count == 0)
                return;

            // Retrieve the destination table name from the CalcConfig
            var destSql = @"
                SELECT DestTableName
                FROM FMM_DestCell
                WHERE CalcConfigID = @calcConfigID";

            var destParams = new List<DbParamInfo>
            {
                new DbParamInfo("@calcConfigID", calcConfigID)
            };

            var dtDest = BRApi.Database.ExecuteQuery(dbConn, false, destSql, destParams);

            if (dtDest.Rows.Count == 0)
                return;

            string destTableName = (dtDest.Rows[0].Field<string>("DestTableName") ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(destTableName))
                return;

            // Insert each result row into the destination custom table
            foreach (DataRow resultRow in dtResults.Rows)
            {
                var colNames = string.Join(", ", dtResults.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
                var paramNames = string.Join(", ", dtResults.Columns.Cast<DataColumn>().Select(c => "@" + c.ColumnName));

                var insertSql = $"INSERT INTO {destTableName} ({colNames}) VALUES ({paramNames})";
                var insertParams = dtResults.Columns.Cast<DataColumn>()
                    .Select(c => new DbParamInfo("@" + c.ColumnName, resultRow[c] ?? DBNull.Value))
                    .ToList();

                BRApi.Database.ExecuteActionQuery(dbConn, insertSql, insertParams, false, false);
            }
        }
	}
}
