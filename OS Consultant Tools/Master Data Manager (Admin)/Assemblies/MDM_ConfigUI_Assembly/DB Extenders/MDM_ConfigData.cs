using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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
using Workspace.OSConsTools.GBL_UI_Assembly;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.MDM_ConfigData
{
    public class MainClass
    {
        #region "Global Variables"
        private SessionInfo si;
        private BRGlobals globals;
        private object api;
        private DashboardExtenderArgs args;
        private StringBuilder debugString;

        // Dim Config
        public Dictionary<string, string> gbl_DimConfig_Dict    { get; set; } = new Dictionary<string, string>();
        public bool gbl_Dup_DimConfig                           { get; set; } = false;
        public int  gbl_DimConfigID                             { get; set; }
        public int  gbl_CurrDimConfigID                         { get; set; }

        // Integration Config
        public Dictionary<string, string> gbl_IntConfig_Dict    { get; set; } = new Dictionary<string, string>();
        public bool gbl_Dup_IntConfig                           { get; set; } = false;
        public int  gbl_IntConfigID                             { get; set; }
        public int  gbl_CurrIntConfigID                         { get; set; }

        // Approval Workflow
        public Dictionary<string, string> gbl_ApprWF_Dict       { get; set; } = new Dictionary<string, string>();
        public bool gbl_Dup_ApprWF                              { get; set; } = false;
        public int  gbl_ApprWFID                                { get; set; }
        public int  gbl_CurrApprWFID                            { get; set; }

        // Approval Step
        public Dictionary<string, string> gbl_ApprStep_Dict     { get; set; } = new Dictionary<string, string>();
        public bool gbl_Dup_ApprStep                            { get; set; } = false;
        public int  gbl_ApprStepID                              { get; set; }
        public int  gbl_CurrApprStepID                          { get; set; }

        // Validation Rule
        public Dictionary<string, string> gbl_ValRule_Dict      { get; set; } = new Dictionary<string, string>();
        public bool gbl_Dup_ValRule                             { get; set; } = false;
        public int  gbl_ValRuleID                               { get; set; }
        public int  gbl_CurrValRuleID                           { get; set; }

        // Access Config
        public Dictionary<string, string> gbl_Access_Dict       { get; set; } = new Dictionary<string, string>();
        public bool gbl_Dup_Access                              { get; set; } = false;
        public int  gbl_AccessID                                { get; set; }
        public int  gbl_CurrAccessID                            { get; set; }
        #endregion

        public object Main(SessionInfo si, BRGlobals globals, object api, DashboardExtenderArgs args)
        {
            try
            {
                this.si      = si;
                this.globals = globals;
                this.api     = api;
                this.args    = args;

                switch (args.FunctionType)
                {
                    case DashboardExtenderFunctionType.SqlTableEditorSaveData:
                        var saveResult = new XFSqlTableEditorSaveDataTaskResult();
                        return SaveData(ref saveResult);

                    case DashboardExtenderFunctionType.LoadDashboard:
                        return LoadDashboard();

                    case DashboardExtenderFunctionType.ComponentSelectionChanged:
                        return OnSelectionChanged();
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #region "Save Data"
        private XFSqlTableEditorSaveDataTaskResult SaveData(ref XFSqlTableEditorSaveDataTaskResult result)
        {
            result.IsOK = true;

            var tableName = args.SqlTableEditorSaveDataTaskInfo?.TableName ?? string.Empty;

            if (tableName.XFEqualsIgnoreCase("MDM_DimConfig"))
            {
                return SaveDimConfig(ref result);
            }
            else if (tableName.XFEqualsIgnoreCase("MDM_IntegrationConfig"))
            {
                return SaveIntegrationConfig(ref result);
            }
            else if (tableName.XFEqualsIgnoreCase("MDM_ApprovalWorkflow"))
            {
                return SaveApprovalWorkflow(ref result);
            }
            else if (tableName.XFEqualsIgnoreCase("MDM_ApprovalStep"))
            {
                return SaveApprovalStep(ref result);
            }
            else if (tableName.XFEqualsIgnoreCase("MDM_ValidationRule"))
            {
                return SaveValidationRule(ref result);
            }
            else if (tableName.XFEqualsIgnoreCase("MDM_AccessConfig"))
            {
                return SaveAccessConfig(ref result);
            }

            return result;
        }

        private XFSqlTableEditorSaveDataTaskResult SaveDimConfig(ref XFSqlTableEditorSaveDataTaskResult result)
        {
            var saveTypeStr = args.NameValuePairs.XFGetValue("IV_MDM_DimConfig_AddUpdate", "Add");
            var saveType    = saveTypeStr.XFEqualsIgnoreCase("Update")
                ? MDM_ConfigHelpers.SaveType.Update
                : MDM_ConfigHelpers.SaveType.Add;

            var mappings = MDM_ConfigHelpers.DimConfigRegistry.Configs[saveType].ParameterMappings;
            var vals     = ExtractParamValues(mappings);

            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                conn.Open();
                if (saveType == MDM_ConfigHelpers.SaveType.Add)
                {
                    var sql = @"
INSERT INTO MDM_DimConfig (DimName, Descr, FeatureType, Status, CreateDate, CreateUser)
VALUES (@DimName, @Descr, @FeatureType, @Status, GETDATE(), @User)";
                    ExecuteNonQuery(conn, sql,
                        Param("@DimName",     vals["DimName"]),
                        Param("@Descr",       vals["Descr"]),
                        Param("@FeatureType", vals["FeatureType"]),
                        Param("@Status",      vals["Status"]),
                        Param("@User",        si.AuthToken.UserName));
                }
                else
                {
                    var id  = args.NameValuePairs.XFGetValue("BL_MDM_DimConfigID", "0");
                    var sql = @"
UPDATE MDM_DimConfig
SET    Descr = @Descr, FeatureType = @FeatureType, Status = @Status,
       UpdateDate = GETDATE(), UpdateUser = @User
WHERE  DimConfigID = @DimConfigID";
                    ExecuteNonQuery(conn, sql,
                        Param("@Descr",       vals["Descr"]),
                        Param("@FeatureType", vals["FeatureType"]),
                        Param("@Status",      vals["Status"]),
                        Param("@User",        si.AuthToken.UserName),
                        ParamInt("@DimConfigID", int.Parse(id)));
                }
            }

            return result;
        }

        private XFSqlTableEditorSaveDataTaskResult SaveIntegrationConfig(ref XFSqlTableEditorSaveDataTaskResult result)
        {
            var saveTypeStr = args.NameValuePairs.XFGetValue("IV_MDM_IntConfig_AddUpdate", "Add");
            var saveType    = saveTypeStr.XFEqualsIgnoreCase("Update")
                ? MDM_ConfigHelpers.SaveType.Update
                : MDM_ConfigHelpers.SaveType.Add;

            var mappings = MDM_ConfigHelpers.IntegrationConfigRegistry.Configs[saveType].ParameterMappings;
            var vals     = ExtractParamValues(mappings);

            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                conn.Open();
                if (saveType == MDM_ConfigHelpers.SaveType.Add)
                {
                    var sql = @"
INSERT INTO MDM_IntegrationConfig (Name, DimConfigID, Direction, SourceType, ConnString, Descr, Status, CreateDate, CreateUser)
VALUES (@Name, @DimConfigID, @Direction, @SourceType, @ConnString, @Descr, @Status, GETDATE(), @User)";
                    ExecuteNonQuery(conn, sql,
                        Param("@Name",       vals["Name"]),
                        ParamInt("@DimConfigID", int.TryParse(vals["DimConfigID"], out var dcId) ? dcId : 0),
                        ParamInt("@Direction",   int.TryParse(vals["Direction"],   out var dir)  ? dir  : 0),
                        ParamInt("@SourceType",  int.TryParse(vals["SourceType"],  out var st)   ? st   : 0),
                        Param("@ConnString", vals["ConnString"]),
                        Param("@Descr",      vals["Descr"]),
                        ParamInt("@Status",  int.TryParse(vals["Status"], out var stat) ? stat : 1),
                        Param("@User",       si.AuthToken.UserName));
                }
                else
                {
                    var id  = args.NameValuePairs.XFGetValue("BL_MDM_IntConfigID", "0");
                    var sql = @"
UPDATE MDM_IntegrationConfig
SET    Direction = @Direction, SourceType = @SourceType, ConnString = @ConnString,
       Descr = @Descr, Status = @Status, UpdateDate = GETDATE(), UpdateUser = @User
WHERE  IntConfigID = @IntConfigID";
                    ExecuteNonQuery(conn, sql,
                        ParamInt("@Direction",   int.TryParse(vals["Direction"],  out var dir) ? dir : 0),
                        ParamInt("@SourceType",  int.TryParse(vals["SourceType"], out var st)  ? st  : 0),
                        Param("@ConnString", vals["ConnString"]),
                        Param("@Descr",      vals["Descr"]),
                        ParamInt("@Status",  int.TryParse(vals["Status"], out var stat) ? stat : 1),
                        Param("@User",       si.AuthToken.UserName),
                        ParamInt("@IntConfigID", int.Parse(id)));
                }
            }

            return result;
        }

        private XFSqlTableEditorSaveDataTaskResult SaveApprovalWorkflow(ref XFSqlTableEditorSaveDataTaskResult result)
        {
            var saveTypeStr = args.NameValuePairs.XFGetValue("IV_MDM_ApprWF_AddUpdate", "Add");
            var saveType    = saveTypeStr.XFEqualsIgnoreCase("Update")
                ? MDM_ConfigHelpers.SaveType.Update
                : MDM_ConfigHelpers.SaveType.Add;

            var mappings = MDM_ConfigHelpers.ApprovalWorkflowRegistry.Configs[saveType].ParameterMappings;
            var vals     = ExtractParamValues(mappings);

            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                conn.Open();
                if (saveType == MDM_ConfigHelpers.SaveType.Add)
                {
                    var sql = @"
INSERT INTO MDM_ApprovalWorkflow (Name, DimConfigID, ChangeType, Descr, Status, CreateDate, CreateUser)
VALUES (@Name, @DimConfigID, @ChangeType, @Descr, @Status, GETDATE(), @User)";
                    ExecuteNonQuery(conn, sql,
                        Param("@Name",       vals["Name"]),
                        ParamInt("@DimConfigID", int.TryParse(vals["DimConfigID"], out var dcId) ? dcId : 0),
                        ParamInt("@ChangeType",  int.TryParse(vals["ChangeType"],  out var ct)   ? ct   : 0),
                        Param("@Descr",      vals["Descr"]),
                        ParamInt("@Status",  int.TryParse(vals["Status"], out var stat) ? stat : 1),
                        Param("@User",       si.AuthToken.UserName));
                }
                else
                {
                    var id  = args.NameValuePairs.XFGetValue("BL_MDM_ApprWFID", "0");
                    var sql = @"
UPDATE MDM_ApprovalWorkflow
SET    ChangeType = @ChangeType, Descr = @Descr, Status = @Status,
       UpdateDate = GETDATE(), UpdateUser = @User
WHERE  WorkflowID = @WorkflowID";
                    ExecuteNonQuery(conn, sql,
                        ParamInt("@ChangeType", int.TryParse(vals["ChangeType"], out var ct)   ? ct   : 0),
                        Param("@Descr",         vals["Descr"]),
                        ParamInt("@Status",     int.TryParse(vals["Status"],     out var stat) ? stat : 1),
                        Param("@User",          si.AuthToken.UserName),
                        ParamInt("@WorkflowID", int.Parse(id)));
                }
            }

            return result;
        }

        private XFSqlTableEditorSaveDataTaskResult SaveApprovalStep(ref XFSqlTableEditorSaveDataTaskResult result)
        {
            var saveTypeStr = args.NameValuePairs.XFGetValue("IV_MDM_ApprStep_AddUpdate", "Add");
            var saveType    = saveTypeStr.XFEqualsIgnoreCase("Update")
                ? MDM_ConfigHelpers.SaveType.Update
                : MDM_ConfigHelpers.SaveType.Add;

            var mappings = MDM_ConfigHelpers.ApprovalStepRegistry.Configs[saveType].ParameterMappings;
            var vals     = ExtractParamValues(mappings);

            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                conn.Open();
                if (saveType == MDM_ConfigHelpers.SaveType.Add)
                {
                    var sql = @"
INSERT INTO MDM_ApprovalStep (WorkflowID, StepOrder, Name, Assignee, Status, CreateDate, CreateUser)
VALUES (@WorkflowID, @StepOrder, @Name, @Assignee, @Status, GETDATE(), @User)";
                    ExecuteNonQuery(conn, sql,
                        ParamInt("@WorkflowID", int.TryParse(vals["WorkflowID"], out var wfId) ? wfId : 0),
                        ParamInt("@StepOrder",  int.TryParse(vals["StepOrder"],  out var so)   ? so   : 0),
                        Param("@Name",     vals["Name"]),
                        Param("@Assignee", vals["Assignee"]),
                        ParamInt("@Status", int.TryParse(vals["Status"], out var stat) ? stat : 1),
                        Param("@User",     si.AuthToken.UserName));
                }
                else
                {
                    var id  = args.NameValuePairs.XFGetValue("BL_MDM_ApprStepID", "0");
                    var sql = @"
UPDATE MDM_ApprovalStep
SET    StepOrder = @StepOrder, Name = @Name, Assignee = @Assignee, Status = @Status,
       UpdateDate = GETDATE(), UpdateUser = @User
WHERE  StepID = @StepID";
                    ExecuteNonQuery(conn, sql,
                        ParamInt("@StepOrder", int.TryParse(vals["StepOrder"], out var so)   ? so   : 0),
                        Param("@Name",     vals["Name"]),
                        Param("@Assignee", vals["Assignee"]),
                        ParamInt("@Status", int.TryParse(vals["Status"], out var stat) ? stat : 1),
                        Param("@User",     si.AuthToken.UserName),
                        ParamInt("@StepID", int.Parse(id)));
                }
            }

            return result;
        }

        private XFSqlTableEditorSaveDataTaskResult SaveValidationRule(ref XFSqlTableEditorSaveDataTaskResult result)
        {
            var saveTypeStr = args.NameValuePairs.XFGetValue("IV_MDM_ValRule_AddUpdate", "Add");
            var saveType    = saveTypeStr.XFEqualsIgnoreCase("Update")
                ? MDM_ConfigHelpers.SaveType.Update
                : MDM_ConfigHelpers.SaveType.Add;

            var mappings = MDM_ConfigHelpers.ValidationRuleRegistry.Configs[saveType].ParameterMappings;
            var vals     = ExtractParamValues(mappings);

            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                conn.Open();
                if (saveType == MDM_ConfigHelpers.SaveType.Add)
                {
                    var sql = @"
INSERT INTO MDM_ValidationRule (DimConfigID, Name, RuleType, Severity, ConfigJSON, Status, CreateDate, CreateUser)
VALUES (@DimConfigID, @Name, @RuleType, @Severity, @ConfigJSON, @Status, GETDATE(), @User)";
                    ExecuteNonQuery(conn, sql,
                        ParamInt("@DimConfigID", int.TryParse(vals["DimConfigID"], out var dcId) ? dcId : 0),
                        Param("@Name",       vals["Name"]),
                        ParamInt("@RuleType",  int.TryParse(vals["RuleType"],  out var rt) ? rt : 0),
                        ParamInt("@Severity",  int.TryParse(vals["Severity"],  out var sv) ? sv : 0),
                        Param("@ConfigJSON", vals["ConfigJSON"]),
                        ParamInt("@Status",  int.TryParse(vals["Status"],  out var stat) ? stat : 1),
                        Param("@User",       si.AuthToken.UserName));
                }
                else
                {
                    var id  = args.NameValuePairs.XFGetValue("BL_MDM_ValRuleID", "0");
                    var sql = @"
UPDATE MDM_ValidationRule
SET    Name = @Name, RuleType = @RuleType, Severity = @Severity, ConfigJSON = @ConfigJSON,
       Status = @Status, UpdateDate = GETDATE(), UpdateUser = @User
WHERE  ValidationRuleID = @ValidationRuleID";
                    ExecuteNonQuery(conn, sql,
                        Param("@Name",       vals["Name"]),
                        ParamInt("@RuleType",  int.TryParse(vals["RuleType"],  out var rt) ? rt : 0),
                        ParamInt("@Severity",  int.TryParse(vals["Severity"],  out var sv) ? sv : 0),
                        Param("@ConfigJSON", vals["ConfigJSON"]),
                        ParamInt("@Status",  int.TryParse(vals["Status"],  out var stat) ? stat : 1),
                        Param("@User",       si.AuthToken.UserName),
                        ParamInt("@ValidationRuleID", int.Parse(id)));
                }
            }

            return result;
        }

        private XFSqlTableEditorSaveDataTaskResult SaveAccessConfig(ref XFSqlTableEditorSaveDataTaskResult result)
        {
            var saveTypeStr = args.NameValuePairs.XFGetValue("IV_MDM_Access_AddUpdate", "Add");
            var saveType    = saveTypeStr.XFEqualsIgnoreCase("Update")
                ? MDM_ConfigHelpers.SaveType.Update
                : MDM_ConfigHelpers.SaveType.Add;

            var mappings = MDM_ConfigHelpers.AccessConfigRegistry.Configs[saveType].ParameterMappings;
            var vals     = ExtractParamValues(mappings);

            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                conn.Open();
                if (saveType == MDM_ConfigHelpers.SaveType.Add)
                {
                    var sql = @"
INSERT INTO MDM_AccessConfig (DimConfigID, GroupName, Role, Status, CreateDate, CreateUser)
VALUES (@DimConfigID, @GroupName, @Role, @Status, GETDATE(), @User)";
                    ExecuteNonQuery(conn, sql,
                        ParamInt("@DimConfigID", int.TryParse(vals["DimConfigID"], out var dcId) ? dcId : 0),
                        Param("@GroupName", vals["GroupName"]),
                        Param("@Role",      vals["Role"]),
                        ParamInt("@Status", int.TryParse(vals["Status"], out var stat) ? stat : 1),
                        Param("@User",      si.AuthToken.UserName));
                }
                else
                {
                    var id  = args.NameValuePairs.XFGetValue("BL_MDM_AccessID", "0");
                    var sql = @"
UPDATE MDM_AccessConfig
SET    Role = @Role, Status = @Status, UpdateDate = GETDATE(), UpdateUser = @User
WHERE  AccessConfigID = @AccessConfigID";
                    ExecuteNonQuery(conn, sql,
                        Param("@Role",   vals["Role"]),
                        ParamInt("@Status",        int.TryParse(vals["Status"], out var stat) ? stat : 1),
                        Param("@User",   si.AuthToken.UserName),
                        ParamInt("@AccessConfigID", int.Parse(id)));
                }
            }

            return result;
        }
        #endregion

        #region "Load Dashboard"
        private XFLoadDashboardTaskResult LoadDashboard()
        {
            var result = new XFLoadDashboardTaskResult { ChangeCustomSubstVarsInDashboard = false };

            if (args.LoadDashboardTaskInfo.Reason == LoadDashboardReasonType.Initialize
                && args.LoadDashboardTaskInfo.Action == LoadDashboardActionType.BeforeFirstGetParameters)
            {
                result.ChangeCustomSubstVarsInDashboard = true;
                result.ModifiedCustomSubstVars = new Dictionary<string, string>();

                var gblHelpers = new GBL_Helpers();
                gblHelpers.UpdateCustomSubstVar(ref result, globals,
                    "IV_MDM_Admin_User", si.AuthToken.UserName);
            }

            return result;
        }
        #endregion

        #region "Selection Changed"
        private XFSelectionChangedTaskResult OnSelectionChanged()
        {
            var result = new XFSelectionChangedTaskResult
            {
                IsOK                                  = true,
                ShowMessageBox                        = false,
                Message                               = string.Empty,
                ChangeSelectionChangedUIActionInDashboard = false
            };
            return result;
        }
        #endregion

        #region "Helpers"
        private Dictionary<string, string> ExtractParamValues(Dictionary<int, Dictionary<string, string>> mappings)
        {
            var vals = new Dictionary<string, string>();
            foreach (var kvp in mappings)
            {
                foreach (var inner in kvp.Value)
                {
                    var uiParam = inner.Key;
                    var colName = inner.Value;
                    vals[colName] = args.NameValuePairs.XFGetValue(uiParam, string.Empty);
                }
            }
            return vals;
        }

        private static SqlParameter Param(string name, string value)
            => new SqlParameter(name, SqlDbType.NVarChar) { Value = (object)value ?? DBNull.Value };

        private static SqlParameter ParamInt(string name, int value)
            => new SqlParameter(name, SqlDbType.Int) { Value = value };

        private static void ExecuteNonQuery(SqlConnection conn, string sql, params SqlParameter[] parms)
        {
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddRange(parms);
            cmd.ExecuteNonQuery();
        }
        #endregion
    }
}
