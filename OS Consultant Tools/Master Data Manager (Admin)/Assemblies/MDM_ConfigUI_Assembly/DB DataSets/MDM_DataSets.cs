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
using Workspace.OSConsTools.GBL_UI_Assembly;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.MDM_DataSets
{
    /// <summary>
    /// Provides dropdown and grid data for the Master Data Manager (Admin) dashboards.
    /// </summary>
    public class MainClass
    {
        #region "Global Variables"
        private SessionInfo si;
        private BRGlobals globals;
        private object api;
        private DashboardDataSetArgs args;
        private readonly string defaultStringVal = "0";
        #endregion

        public object Main(SessionInfo si, BRGlobals globals, object api, DashboardDataSetArgs args)
        {
            try
            {
                BRApi.ErrorLog.LogMessage(si, $"MDM_DataSets hit: {args.DataSetName}");
                this.si      = si;
                this.globals = globals;
                this.api     = api;
                this.args    = args;

                switch (args.FunctionType)
                {
                    case DashboardDataSetFunctionType.GetDataSetNames:
                        break;

                    case DashboardDataSetFunctionType.GetDataSet:
                        // --- Dimension Config ---
                        if (args.DataSetName.XFEqualsIgnoreCase("get_MDM_OSDimensions"))
                            return get_MDM_OSDimensions();
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_MDM_DimConfigs"))
                            return get_MDM_DimConfigs();

                        // --- Integration Config ---
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_MDM_IntegrationConfigs"))
                            return get_MDM_IntegrationConfigs();
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_MDM_IntegrationDirections"))
                            return get_MDM_EnumList("IntegrationDirection");
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_MDM_SourceTypes"))
                            return get_MDM_EnumList("IntegrationSourceType");

                        // --- Approval Workflow Config ---
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_MDM_ApprovalWorkflows"))
                            return get_MDM_ApprovalWorkflows();
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_MDM_ApprovalSteps"))
                            return get_MDM_ApprovalSteps();
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_MDM_ChangeTypes"))
                            return get_MDM_EnumList("ChangeType");

                        // --- Validation Rule Config ---
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_MDM_ValidationRules"))
                            return get_MDM_ValidationRules();
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_MDM_RuleTypes"))
                            return get_MDM_EnumList("RuleType");
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_MDM_Severities"))
                            return get_MDM_EnumList("RuleSeverity");

                        // --- Access Config ---
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_MDM_AccessConfigs"))
                            return get_MDM_AccessConfigs();

                        // --- Audit Log ---
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_MDM_AuditLog"))
                            return get_MDM_AuditLog();

                        // --- Status list (shared) ---
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_MDM_Statuses"))
                            return get_MDM_Statuses();
                        break;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #region "Dimension Config Datasets"
        /// <summary>Returns all OS dimensions visible to the current user.</summary>
        private DataTable get_MDM_OSDimensions()
        {
            var dt     = new DataTable("MDM_OSDimensions");
            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                var helper = new SQL_GBL_Get_DataSets(si, conn);
                var sqa    = new SqlDataAdapter();
                var sql    = @"
SELECT DimTypeID AS DimID, Name AS DimName
FROM   Dimension
ORDER  BY Name";
                helper.Fill_Get_GBL_DT(si, sqa, dt, sql);
            }
            return dt;
        }

        /// <summary>Returns all configured MDM dimensions.</summary>
        private DataTable get_MDM_DimConfigs()
        {
            var dt     = new DataTable("MDM_DimConfigs");
            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                var helper = new SQL_GBL_Get_DataSets(si, conn);
                var sqa    = new SqlDataAdapter();
                var sql    = @"
SELECT DimConfigID, DimName, Descr, FeatureType, Status, CreateDate, CreateUser, UpdateDate, UpdateUser
FROM   MDM_DimConfig
ORDER  BY DimName";
                helper.Fill_Get_GBL_DT(si, sqa, dt, sql);
            }
            return dt;
        }
        #endregion

        #region "Integration Config Datasets"
        private DataTable get_MDM_IntegrationConfigs()
        {
            var dt     = new DataTable("MDM_IntegrationConfigs");
            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                var helper = new SQL_GBL_Get_DataSets(si, conn);
                var sqa    = new SqlDataAdapter();
                var dimID  = args.NameValuePairs.XFGetValue("IV_MDM_DimConfigID", "0");
                var sql    = @"
SELECT ic.IntConfigID, ic.Name, ic.DimConfigID, dc.DimName, ic.Direction, ic.SourceType,
       ic.Descr, ic.Status, ic.CreateDate, ic.CreateUser, ic.UpdateDate, ic.UpdateUser
FROM   MDM_IntegrationConfig ic
JOIN   MDM_DimConfig dc ON dc.DimConfigID = ic.DimConfigID
WHERE  (@DimConfigID = 0 OR ic.DimConfigID = @DimConfigID)
ORDER  BY ic.Name";
                var sqlparams = new[] { new SqlParameter("@DimConfigID", SqlDbType.Int)
                    { Value = int.TryParse(dimID, out var d) ? d : 0 } };
                helper.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
            }
            return dt;
        }
        #endregion

        #region "Approval Workflow Datasets"
        private DataTable get_MDM_ApprovalWorkflows()
        {
            var dt     = new DataTable("MDM_ApprovalWorkflows");
            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                var helper = new SQL_GBL_Get_DataSets(si, conn);
                var sqa    = new SqlDataAdapter();
                var dimID  = args.NameValuePairs.XFGetValue("IV_MDM_DimConfigID", "0");
                var sql    = @"
SELECT aw.WorkflowID, aw.Name, aw.DimConfigID, dc.DimName, aw.ChangeType,
       aw.Descr, aw.Status, aw.CreateDate, aw.CreateUser, aw.UpdateDate, aw.UpdateUser
FROM   MDM_ApprovalWorkflow aw
JOIN   MDM_DimConfig dc ON dc.DimConfigID = aw.DimConfigID
WHERE  (@DimConfigID = 0 OR aw.DimConfigID = @DimConfigID)
ORDER  BY aw.Name";
                var sqlparams = new[] { new SqlParameter("@DimConfigID", SqlDbType.Int)
                    { Value = int.TryParse(dimID, out var d) ? d : 0 } };
                helper.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
            }
            return dt;
        }

        private DataTable get_MDM_ApprovalSteps()
        {
            var dt     = new DataTable("MDM_ApprovalSteps");
            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                var helper     = new SQL_GBL_Get_DataSets(si, conn);
                var sqa        = new SqlDataAdapter();
                var workflowID = args.NameValuePairs.XFGetValue("IV_MDM_ApprWFID", "0");
                var sql        = @"
SELECT StepID, WorkflowID, StepOrder, Name, Assignee, Status, CreateDate, CreateUser, UpdateDate, UpdateUser
FROM   MDM_ApprovalStep
WHERE  (@WorkflowID = 0 OR WorkflowID = @WorkflowID)
ORDER  BY StepOrder";
                var sqlparams = new[] { new SqlParameter("@WorkflowID", SqlDbType.Int)
                    { Value = int.TryParse(workflowID, out var w) ? w : 0 } };
                helper.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
            }
            return dt;
        }
        #endregion

        #region "Validation Rule Datasets"
        private DataTable get_MDM_ValidationRules()
        {
            var dt     = new DataTable("MDM_ValidationRules");
            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                var helper = new SQL_GBL_Get_DataSets(si, conn);
                var sqa    = new SqlDataAdapter();
                var dimID  = args.NameValuePairs.XFGetValue("IV_MDM_DimConfigID", "0");
                var sql    = @"
SELECT vr.ValidationRuleID, vr.DimConfigID, dc.DimName, vr.Name, vr.RuleType, vr.Severity,
       vr.ConfigJSON, vr.Status, vr.CreateDate, vr.CreateUser, vr.UpdateDate, vr.UpdateUser
FROM   MDM_ValidationRule vr
JOIN   MDM_DimConfig dc ON dc.DimConfigID = vr.DimConfigID
WHERE  (@DimConfigID = 0 OR vr.DimConfigID = @DimConfigID)
ORDER  BY vr.Name";
                var sqlparams = new[] { new SqlParameter("@DimConfigID", SqlDbType.Int)
                    { Value = int.TryParse(dimID, out var d) ? d : 0 } };
                helper.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
            }
            return dt;
        }
        #endregion

        #region "Access Config Datasets"
        private DataTable get_MDM_AccessConfigs()
        {
            var dt     = new DataTable("MDM_AccessConfigs");
            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                var helper = new SQL_GBL_Get_DataSets(si, conn);
                var sqa    = new SqlDataAdapter();
                var dimID  = args.NameValuePairs.XFGetValue("IV_MDM_DimConfigID", "0");
                var sql    = @"
SELECT ac.AccessConfigID, ac.DimConfigID, dc.DimName, ac.GroupName, ac.Role,
       ac.Status, ac.CreateDate, ac.CreateUser
FROM   MDM_AccessConfig ac
JOIN   MDM_DimConfig dc ON dc.DimConfigID = ac.DimConfigID
WHERE  (@DimConfigID = 0 OR ac.DimConfigID = @DimConfigID)
ORDER  BY ac.GroupName";
                var sqlparams = new[] { new SqlParameter("@DimConfigID", SqlDbType.Int)
                    { Value = int.TryParse(dimID, out var d) ? d : 0 } };
                helper.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
            }
            return dt;
        }
        #endregion

        #region "Audit Log Dataset"
        private DataTable get_MDM_AuditLog()
        {
            var dt     = new DataTable("MDM_AuditLog");
            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                var helper = new SQL_GBL_Get_DataSets(si, conn);
                var sqa    = new SqlDataAdapter();
                var sql    = @"
SELECT al.AuditLogID, al.EventType, al.DimConfigID, dc.DimName,
       al.ObjectType, al.ObjectID, al.ChangedBy, al.ChangedDate, al.Notes
FROM   MDM_AuditLog al
LEFT JOIN MDM_DimConfig dc ON dc.DimConfigID = al.DimConfigID
ORDER  BY al.ChangedDate DESC";
                helper.Fill_Get_GBL_DT(si, sqa, dt, sql);
            }
            return dt;
        }
        #endregion

        #region "Shared / Enum Datasets"
        private DataTable get_MDM_Statuses()
        {
            var dt = new DataTable("MDM_Statuses");
            dt.Columns.Add("StatusID",   typeof(int));
            dt.Columns.Add("StatusName", typeof(string));
            dt.Rows.Add(1, "Active");
            dt.Rows.Add(0, "Inactive");
            return dt;
        }

        /// <summary>
        /// Builds a DataTable from a named MDM enum for use in dropdowns.
        /// Supported enum names: IntegrationDirection, IntegrationSourceType,
        /// ChangeType, RuleType, RuleSeverity.
        /// </summary>
        private DataTable get_MDM_EnumList(string enumName)
        {
            var dt = new DataTable($"MDM_{enumName}");
            dt.Columns.Add("ID",   typeof(int));
            dt.Columns.Add("Name", typeof(string));

            switch (enumName)
            {
                case "IntegrationDirection":
                    dt.Rows.Add((int)MDM_ConfigHelpers.IntegrationDirection.Upstream,     "Upstream");
                    dt.Rows.Add((int)MDM_ConfigHelpers.IntegrationDirection.Downstream,   "Downstream");
                    dt.Rows.Add((int)MDM_ConfigHelpers.IntegrationDirection.Bidirectional,"Bidirectional");
                    break;

                case "IntegrationSourceType":
                    dt.Rows.Add((int)MDM_ConfigHelpers.IntegrationSourceType.SQL,      "SQL");
                    dt.Rows.Add((int)MDM_ConfigHelpers.IntegrationSourceType.FlatFile, "Flat File");
                    dt.Rows.Add((int)MDM_ConfigHelpers.IntegrationSourceType.API,      "API");
                    break;

                case "ChangeType":
                    dt.Rows.Add((int)MDM_ConfigHelpers.ChangeType.Add,    "Add");
                    dt.Rows.Add((int)MDM_ConfigHelpers.ChangeType.Edit,   "Edit");
                    dt.Rows.Add((int)MDM_ConfigHelpers.ChangeType.Move,   "Move");
                    dt.Rows.Add((int)MDM_ConfigHelpers.ChangeType.Retire, "Retire");
                    break;

                case "RuleType":
                    dt.Rows.Add((int)MDM_ConfigHelpers.RuleType.Uniqueness,           "Uniqueness");
                    dt.Rows.Add((int)MDM_ConfigHelpers.RuleType.NamingConvention,     "Naming Convention");
                    dt.Rows.Add((int)MDM_ConfigHelpers.RuleType.RequiredAttribute,    "Required Attribute");
                    dt.Rows.Add((int)MDM_ConfigHelpers.RuleType.HierarchyConstraint,  "Hierarchy Constraint");
                    dt.Rows.Add((int)MDM_ConfigHelpers.RuleType.CrossDimConsistency,  "Cross-Dim Consistency");
                    dt.Rows.Add((int)MDM_ConfigHelpers.RuleType.CustomSQL,            "Custom SQL");
                    break;

                case "RuleSeverity":
                    dt.Rows.Add((int)MDM_ConfigHelpers.RuleSeverity.Error,   "Error");
                    dt.Rows.Add((int)MDM_ConfigHelpers.RuleSeverity.Warning, "Warning");
                    dt.Rows.Add((int)MDM_ConfigHelpers.RuleSeverity.Info,    "Info");
                    break;
            }

            return dt;
        }
        #endregion
    }
}
