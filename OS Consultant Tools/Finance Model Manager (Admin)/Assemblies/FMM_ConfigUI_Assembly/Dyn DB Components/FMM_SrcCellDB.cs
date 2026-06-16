using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.CSharp;
using Microsoft.Data.SqlClient;
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
    public class FMM_SrcCellDB
    {
        // Database table that will contain our objects
        public string TableName { get; } = "FMM_SrcCellConfig";

        // SessionInfo instance
        public SessionInfo si { get; }

        // constructor
        public FMM_SrcCellDB(SessionInfo si)
        {
            this.si = si;
        }
		
        public List<string> GetRequiredColumnsByCalcType(int calcType)
        {
            var columns = new List<string>();

            columns.AddRange(new[] {
                "SrcCellConfigID", "CubeConfigID", "ActConfigID", "ModelConfigID", "CalcConfigID","Type","Item",
                "SrcOrder","CreateDate","CreateUser","UpdateDate","UpdateUser"
            });

            var srcConfig = FMM_ConfigHelpers.Get_SrcConfigType(calcType);

            if (srcConfig != null && srcConfig.ParameterMappings != null)
            {
                foreach (var mapping in srcConfig.ParameterMappings.Values)
                {
                    foreach (var dbColumn in mapping.Values)
                    {
                        if (!columns.Contains(dbColumn) && !string.IsNullOrEmpty(dbColumn))
                        {
                            columns.Add(dbColumn);
                        }
                    }
                }
            }

            return columns;
        }
		
        public string GetSelectColumnsForCalcType(int calcType)
        {
            var columns = GetRequiredColumnsByCalcType(calcType);
            return string.Join(", ", columns);
        }

        public FMM_SrcCellModel GetSrcCell(int srcCellConfigID, int calcType)
        {
            try
            {
                var columns = GetSelectColumnsForCalcType(calcType);
                string sql = $"SELECT {columns} FROM {this.TableName} WHERE SrcCellConfigID = @SrcCellConfigID";

                List<DbParamInfo> paramList = new List<DbParamInfo> { new DbParamInfo("@SrcCellConfigID", srcCellConfigID) };

                using (DbConnInfoApp dbConn = BRApi.Database.CreateApplicationDbConnInfo(this.si))
                {
                    DataTable dt = BRApi.Database.ExecuteSql(dbConn, sql, paramList, false);
                    if (dt.Rows.Count < 1) return null;

                    DataRow dr = dt.Rows[0];
                    return MapDataRowToModel(dr, calcType);
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        public List<FMM_SrcCellModel> GetSrcCellsByCalcConfigID(int calcConfigID, int calcType)
        {
            try
            {
				if (string.IsNullOrWhiteSpace(calcConfigID.XFToString()))
				{
					calcConfigID = 0;
				}
                var columns = GetSelectColumnsForCalcType(calcType);
                var sql = $"SELECT {columns} FROM {this.TableName} WHERE CalcConfigID = @CalcConfigID";
                BRApi.ErrorLog.LogMessage(si, $"Hit {sql} - {calcConfigID}");

                List<DbParamInfo> paramList = new List<DbParamInfo> { new DbParamInfo("@CalcConfigID", calcConfigID) };

                using (DbConnInfoApp dbConn = BRApi.Database.CreateApplicationDbConnInfo(this.si))
                {
                    DataTable dt = BRApi.Database.ExecuteSql(dbConn, sql, paramList, false);
                    List<FMM_SrcCellModel> result = new List<FMM_SrcCellModel>();

                    foreach (DataRow dr in dt.Rows)
                    {
                        result.Add(MapDataRowToModel(dr, calcType));
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        public void Merge(List<FMM_SrcCellModel> models, int calcType)
        {
            if (models == null || models.Count == 0) return;

            try
            {
                var mergeTable = BuildMergeTable(models, calcType);

                var CalcConfigID = models[0].calcConfigID;
                var selectColumns = GetSelectColumnsForCalcType(calcType);
                var currentSql = $"SELECT {selectColumns} FROM {this.TableName} WHERE CalcConfigID = @CalcConfigID";

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(this.si);

                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(this.si, connection);
                    var sqa = new SqlDataAdapter();
                    var currentTable = new DataTable();

                    var sqlparams = new[]
                    {
                        new SqlParameter("@CalcConfigID", SqlDbType.Int) { Value = CalcConfigID }
                    };

                    cmdBuilder.FillDataTable(this.si, sqa, currentTable, currentSql, sqlparams);

                    // Set primary key if Cell_ID column exists in the result set
                    if (currentTable.Columns.Contains("SrcCellConfigID") && currentTable.Columns["SrcCellConfigID"] != null)
                    {
                        currentTable.PrimaryKey = new[] { currentTable.Columns["SrcCellConfigID"]! };
                    }

                    // Merge the new/updated records with existing data
                    // preserveChanges=false: Overwrite existing rows with new values (upsert behavior)
                    currentTable.Merge(mergeTable, false, MissingSchemaAction.Add);

                    cmdBuilder.UpdateTable(this.si, this.TableName, currentTable, sqa);
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Merge a single source cell using the generic SQA process
        /// </summary>
        public void Merge(FMM_SrcCellModel model, int calcType)
        {
            this.Merge(new List<FMM_SrcCellModel> { model }, calcType);
        }

        private DataTable BuildMergeTable(IEnumerable<FMM_SrcCellModel> models, int calcType)
        {
            var columns = GetRequiredColumnsByCalcType(calcType);
            DataTable dt = new DataTable(this.TableName);

            foreach (var col in columns)
            {
                dt.Columns.Add(col, typeof(object));
            }

            foreach (var model in models)
            {
                DataRow row = dt.NewRow();

                foreach (var columnName in columns)
                {
                    var prop =
                        model.GetType().GetProperty(columnName,
                            System.Reflection.BindingFlags.Instance |
                            System.Reflection.BindingFlags.Public |
                            System.Reflection.BindingFlags.IgnoreCase)
                        ?? model.GetType().GetProperty(columnName.Replace("_", string.Empty),
                            System.Reflection.BindingFlags.Instance |
                            System.Reflection.BindingFlags.Public |
                            System.Reflection.BindingFlags.IgnoreCase);

                    if (prop == null) continue;

                    var value = prop.GetValue(model);
                    SetColumnValue(row, columnName, value);
                }

                dt.Rows.Add(row);
            }

            return dt;
        }

        private void SetColumnValue(DataRow row, string columnName, object value)
        {
            if (!row.Table.Columns.Contains(columnName)) return;
            row[columnName] = value ?? DBNull.Value;
        }

        /// <summary>
        /// Maps a DataRow to FMM_SrcCellModel, safely handling null values
        /// Overload without calcType - uses default parameterless constructor
        /// </summary>
        private FMM_SrcCellModel MapDataRowToModel(DataRow dr)
        {
            return MapDataRowToModel(dr, 0); // 0 = CalcType.None, will use empty enabled properties list
        }

        /// <summary>
        /// Maps a DataRow to FMM_SrcCellModel, safely handling null values
        /// </summary>
        private FMM_SrcCellModel MapDataRowToModel(DataRow dr, int calcType)
        {
            // Get enabled properties from configuration
            var enabledProperties = FMM_ConfigHelpers.GetEnabledSrcProperties(calcType);

            var model = new FMM_SrcCellModel(enabledProperties)
            {
                cubeConfigID = dr.Field<int>("CubeConfigID"),
                actConfigID = dr.Field<int>("ActConfigID"),
                modelConfigID = dr.Field<int>("ModelConfigID"),
                calcConfigID = dr.Field<int>("CalcConfigID"),
                srcCellConfigID = dr.Field<int>("SrcCellConfigID"),
                srcOrder = dr.Field<int?>("SrcOrder") ?? 0,
                type = dr.Field<string>("Type") ?? string.Empty,
                item = dr.Field<string>("Item") ?? string.Empty
            };


            // Set dimension fields only if they exist in the column set and are enabled
            if (enabledProperties.Contains("Entity") && dr.Table.Columns.Contains("Entity"))
                model.entity = dr.Field<string>("Entity");
            if (enabledProperties.Contains("Cons") && dr.Table.Columns.Contains("Cons"))
                model.cons = dr.Field<string>("Cons");
            if (enabledProperties.Contains("Scenario") && dr.Table.Columns.Contains("Scenario"))
                model.scenario = dr.Field<string>("Scenario");
            if (enabledProperties.Contains("Time") && dr.Table.Columns.Contains("Time"))
                model.time = dr.Field<string>("Time");
            if (enabledProperties.Contains("View") && dr.Table.Columns.Contains("View"))
                model.view = dr.Field<string>("View");
            if (enabledProperties.Contains("Acct") && dr.Table.Columns.Contains("Acct"))
                model.acct = dr.Field<string>("Acct");
            if (enabledProperties.Contains("IC") && dr.Table.Columns.Contains("IC"))
                model.ic = dr.Field<string>("IC");
            if (enabledProperties.Contains("Origin") && dr.Table.Columns.Contains("Origin"))
                model.origin = dr.Field<string>("Origin");
            if (enabledProperties.Contains("Flow") && dr.Table.Columns.Contains("Flow"))
                model.flow = dr.Field<string>("Flow");
            if (enabledProperties.Contains("UD1") && dr.Table.Columns.Contains("UD1"))
                model.ud1 = dr.Field<string>("UD1");
            if (enabledProperties.Contains("UD2") && dr.Table.Columns.Contains("UD2"))
                model.ud2 = dr.Field<string>("UD2");
            if (enabledProperties.Contains("UD3") && dr.Table.Columns.Contains("UD3"))
                model.ud3 = dr.Field<string>("UD3");
            if (enabledProperties.Contains("UD4") && dr.Table.Columns.Contains("UD4"))
                model.ud4 = dr.Field<string>("UD4");
            if (enabledProperties.Contains("UD5") && dr.Table.Columns.Contains("UD5"))
                model.ud5 = dr.Field<string>("UD5");
            if (enabledProperties.Contains("UD6") && dr.Table.Columns.Contains("UD6"))
                model.ud6 = dr.Field<string>("UD6");
            if (enabledProperties.Contains("UD7") && dr.Table.Columns.Contains("UD7"))
                model.ud7 = dr.Field<string>("UD7");
            if (enabledProperties.Contains("UD8") && dr.Table.Columns.Contains("UD8"))
                model.ud8 = dr.Field<string>("UD8");

            return model;
        }

        /// <summary>
        /// Delete a source cell record by Cell_ID
        /// </summary>
        public void Delete(int cellId)
        {
            try
            {
                string sql = $"DELETE FROM {this.TableName} WHERE Cell_ID = @cellID";
                List<DbParamInfo> paramList = new List<DbParamInfo> { new DbParamInfo("@cellID", cellId) };

                using (DbConnInfoApp dbConn = BRApi.Database.CreateApplicationDbConnInfo(this.si))
                {
                    BRApi.Database.ExecuteActionQuery(dbConn, sql, paramList, false, true);
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Delete a source cell record by Cell_ID (string representation)
        /// </summary>
        public void Delete(string cellIdStr)
        {
            if (int.TryParse(cellIdStr, out int cellId))
            {
                this.Delete(cellId);
            }
            else
            {
                throw new XFException(si, new ArgumentException($"Invalid Cell_ID format: {cellIdStr}"));
            }
        }

        /// <summary>
        /// Delete a source cell record by model instance
        /// </summary>
        public void Delete(FMM_SrcCellModel model)
        {
            this.Delete(model.srcCellConfigID);
        }

        /// <summary>
        /// Create a default configuration for a workspace
        /// </summary>
        public void CreateDefaultConfig(Guid workspaceId, Dictionary<string, string> customSubstVars)
        {
            // Build a minimal starter row into a DataTable and push it through the GBL SQA helpers.
            // CalcType 1 = Table; adjust if other defaults are required.
            try
            {
                _ = workspaceId; // reserved for future workspace-specific defaults
                var CubeConfigID = customSubstVars.XFGetValue("BL_FMM_CubeConfigID", "0");
                var ActConfigID = customSubstVars.XFGetValue("BL_FMM_ActConfigID", "0");
                var ModelConfigID = customSubstVars.XFGetValue("BL_FMM_ModelConfigID", "0");
                var CalcConfigID = customSubstVars.XFGetValue("BL_FMM_CalcConfigID", "0").XFConvertToInt();

                const int defaultCalcType = 1;

                using (var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(this.si))
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();

                    // Pull current set (schema) using SQA builder
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(this.si, connection);
                    var sqa = new SqlDataAdapter();
                    var currentTable = new DataTable();

                    var selectColumns = GetSelectColumnsForCalcType(defaultCalcType);
                    if (!selectColumns.Split(',').Select(c => c.Trim()).Contains("SrcCellConfigID", StringComparer.OrdinalIgnoreCase))
                    {
                        selectColumns = $"SrcCellConfigID, {selectColumns}";
                    }

                    var currentSql = $"SELECT {selectColumns} FROM {this.TableName} WHERE CalcConfigID = @CalcConfigID";
                    var sqlparams = new[] { new SqlParameter("@CalcConfigID", SqlDbType.Int) { Value = CalcConfigID } };
					BRApi.ErrorLog.LogMessage(si,$"SQL {currentSql}");
                    cmdBuilder.FillDataTable(this.si, sqa, currentTable, currentSql, sqlparams);

                    if (currentTable.Columns.Contains("SrcCellConfigID"))
                    {
                        currentTable.PrimaryKey = new[] { currentTable.Columns["SrcCellConfigID"]! };
                    }

                    var now = DateTime.Now;
                    var user = this.si?.UserName ?? string.Empty;

                    // Create the starter row with minimal required fields
                    var row = currentTable.NewRow();
					SetColumnValue(row, "SrcCellConfigID",1);
                    SetColumnValue(row, "CubeConfigID", CubeConfigID);
                    SetColumnValue(row, "ActConfigID", ActConfigID);
                    SetColumnValue(row, "ModelConfigID", ModelConfigID);
                    SetColumnValue(row, "CalcConfigID", CalcConfigID);
                    SetColumnValue(row, "SrcOrder", 1);
                    SetColumnValue(row, "Type", "Test");
                    SetColumnValue(row, "Item", string.Empty);
                    SetColumnValue(row, "CreateDate", DateTime.Now);
                    SetColumnValue(row, "CreateUser", si.UserName);
                    SetColumnValue(row, "UpdateDate", DateTime.Now);
                    SetColumnValue(row, "UpdateUser", si.UserName);
BRApi.ErrorLog.LogMessage(si,$"Hit {CalcConfigID}");
                    currentTable.Rows.Add(row);

BRApi.ErrorLog.LogMessage(si,$"Hit 2: {CalcConfigID} - {this.TableName}");
                    cmdBuilder.UpdateTable(this.si, this.TableName, currentTable, sqa);
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }
    }
}
