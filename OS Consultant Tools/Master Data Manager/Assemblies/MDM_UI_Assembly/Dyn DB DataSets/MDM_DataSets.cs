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
using Workspace.OSConsTools.MDM_ConfigUI_Assembly;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.MDM_DataSets
{
    /// <summary>
    /// Provides all dataset queries for the Master Data Manager end-user workspace.
    /// Covers: nav menu, dimension/member data, hierarchy tree, change requests,
    /// approver inbox, and report datasets.
    /// </summary>
    public class MainClass
    {
        #region "Global Variables"
        private SessionInfo si;
        private BRGlobals globals;
        private object api;
        private DashboardDataSetArgs args;
        #endregion

        public object Main(SessionInfo si, BRGlobals globals, object api, DashboardDataSetArgs args)
        {
            try
            {
                BRApi.ErrorLog.LogMessage(si, $"MDM_DataSets (user) hit: {args.DataSetName}");
                this.si      = si;
                this.globals = globals;
                this.api     = api;
                this.args    = args;

                switch (args.FunctionType)
                {
                    case DashboardDataSetFunctionType.GetDataSetNames:
                        break;

                    case DashboardDataSetFunctionType.GetDataSet:

                        // Navigation
                        if (args.DataSetName.XFEqualsIgnoreCase("Get_App_Menu"))
                            return Get_App_Menu();

                        // --- Feature 1: Dimension & Member Maintenance ---
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_MDM_UserDimensions"))
                            return Get_MDM_UserDimensions();
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_MDM_HierarchyTree"))
                            return Get_MDM_HierarchyTree();
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_MDM_MemberDetail"))
                            return Get_MDM_MemberDetail();

                        // --- Feature 2: Integrations ---
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_MDM_IntegrationRuns"))
                            return Get_MDM_IntegrationRuns();
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_MDM_IntegrationRunDetail"))
                            return Get_MDM_IntegrationRunDetail();

                        // --- Feature 3: Approval Processes ---
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_MDM_MyRequests"))
                            return Get_MDM_MyRequests();
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_MDM_ApproverInbox"))
                            return Get_MDM_ApproverInbox();
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_MDM_ChangeRequestDetail"))
                            return Get_MDM_ChangeRequestDetail();
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_MDM_ChangeRequestAudit"))
                            return Get_MDM_ChangeRequestAudit();

                        // --- Feature 4: Validations ---
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_MDM_ValidationResults"))
                            return Get_MDM_ValidationResults();

                        // --- Feature 6: Reports ---
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_MDM_Rpt_PendingChanges"))
                            return Get_MDM_Rpt_PendingChanges();
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_MDM_Rpt_ApprovalCycleTime"))
                            return Get_MDM_Rpt_ApprovalCycleTime();
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_MDM_Rpt_IntegrationSummary"))
                            return Get_MDM_Rpt_IntegrationSummary();
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_MDM_Rpt_ValidationExceptions"))
                            return Get_MDM_Rpt_ValidationExceptions();
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_MDM_Rpt_MemberChangeHistory"))
                            return Get_MDM_Rpt_MemberChangeHistory();
                        break;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #region "Navigation"
        private DataTable Get_App_Menu()
        {
            var dt     = new DataTable("MDM_AppMenu");
            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                var helper = new SQL_GBL_Get_DataSets(si, conn);
                var sqa    = new SqlDataAdapter();
                var sql    = @"
SELECT MenuOptionID AS ID, MenuName AS Name, SortOrder, IconName
FROM   MDM_MenuOption
WHERE  Status = 1
ORDER  BY SortOrder";
                helper.Fill_Get_GBL_DT(si, sqa, dt, sql);
            }
            return dt;
        }
        #endregion

        #region "Feature 1 — Dimension & Member Maintenance"

        /// <summary>Returns dimensions the current user has at least Submitter access to.</summary>
        private DataTable Get_MDM_UserDimensions()
        {
            var dt     = new DataTable("MDM_UserDimensions");
            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                var helper = new SQL_GBL_Get_DataSets(si, conn);
                var sqa    = new SqlDataAdapter();
                var sql    = @"
SELECT DISTINCT dc.DimConfigID, dc.DimName, dc.Descr
FROM   MDM_DimConfig    dc
JOIN   MDM_AccessConfig ac ON ac.DimConfigID = dc.DimConfigID
WHERE  dc.Status = 1
  AND  ac.Status = 1
  AND  IS_MEMBER(ac.GroupName) = 1
ORDER  BY dc.DimName";
                helper.Fill_Get_GBL_DT(si, sqa, dt, sql);
            }
            return dt;
        }

        /// <summary>Returns the hierarchy tree for the selected dimension, using MDM_ReorgSvc.</summary>
        private DataTable Get_MDM_HierarchyTree()
        {
            var dimName    = args.NameValuePairs.XFGetValue(MDM_Support.Param_SelDim,    string.Empty);
            var rootMember = args.NameValuePairs.XFGetValue(MDM_Support.Param_SelMember, string.Empty);
            return MDM_ReorgSvc.GetHierarchyTree(si, dimName, rootMember);
        }

        /// <summary>Returns the full property set for a single selected member.</summary>
        private DataTable Get_MDM_MemberDetail()
        {
            var dt     = new DataTable("MDM_MemberDetail");
            var dimName    = args.NameValuePairs.XFGetValue(MDM_Support.Param_SelDim,    string.Empty);
            var memberName = args.NameValuePairs.XFGetValue(MDM_Support.Param_SelMember, string.Empty);

            if (string.IsNullOrEmpty(dimName) || string.IsNullOrEmpty(memberName))
                return dt;

            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                var helper = new SQL_GBL_Get_DataSets(si, conn);
                var sqa    = new SqlDataAdapter();
                // Query the platform's member table for the staging record (if any) or live data.
                var sql    = @"
SELECT m.Name, m.ParentName, m.Description, m.SortOrder,
       m.IsEnabled, m.U1, m.U2, m.U3, m.U4, m.U5, m.U6, m.U7, m.U8
FROM   Member m
JOIN   Dimension d ON d.DimTypeID = m.DimTypeID
WHERE  d.Name = @DimName
  AND  m.Name = @MemberName";
                var sqlparams = new[]
                {
                    new SqlParameter("@DimName",    SqlDbType.NVarChar) { Value = dimName    },
                    new SqlParameter("@MemberName", SqlDbType.NVarChar) { Value = memberName }
                };
                helper.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
            }
            return dt;
        }
        #endregion

        #region "Feature 2 — Integrations"

        private DataTable Get_MDM_IntegrationRuns()
        {
            var dt     = new DataTable("MDM_IntegrationRuns");
            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                var helper = new SQL_GBL_Get_DataSets(si, conn);
                var sqa    = new SqlDataAdapter();
                var dimID  = args.NameValuePairs.XFGetValue("IV_MDM_DimConfigID", "0");
                var sql    = @"
SELECT r.RunID, ic.Name AS IntegrationName, dc.DimName, r.Direction,
       r.StartDate, r.EndDate, r.Status,
       r.RecordsProcessed, r.RecordsMatched, r.RecordsFailed,
       r.RunBy
FROM   MDM_IntegrationRunLog r
JOIN   MDM_IntegrationConfig  ic ON ic.IntConfigID  = r.IntConfigID
JOIN   MDM_DimConfig          dc ON dc.DimConfigID  = ic.DimConfigID
WHERE  (@DimConfigID = 0 OR ic.DimConfigID = @DimConfigID)
ORDER  BY r.StartDate DESC";
                var sqlparams = new[] { new SqlParameter("@DimConfigID", SqlDbType.Int)
                    { Value = int.TryParse(dimID, out var d) ? d : 0 } };
                helper.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
            }
            return dt;
        }

        private DataTable Get_MDM_IntegrationRunDetail()
        {
            var dt     = new DataTable("MDM_IntegrationRunDetail");
            var runIDStr = args.NameValuePairs.XFGetValue("IV_MDM_RunID", "0");
            if (!int.TryParse(runIDStr, out int runID) || runID <= 0) return dt;

            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                var helper = new SQL_GBL_Get_DataSets(si, conn);
                var sqa    = new SqlDataAdapter();
                var sql    = @"
SELECT DetailID, RunID, MemberName, Action, SourceValue, TargetValue, Status, ErrorMessage
FROM   MDM_IntegrationRunDetail
WHERE  RunID = @RunID
ORDER  BY DetailID";
                var sqlparams = new[] { new SqlParameter("@RunID", SqlDbType.Int) { Value = runID } };
                helper.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
            }
            return dt;
        }
        #endregion

        #region "Feature 3 — Approval Processes"

        /// <summary>Returns all change requests submitted by the current user.</summary>
        private DataTable Get_MDM_MyRequests()
        {
            var dt     = new DataTable("MDM_MyRequests");
            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                var helper = new SQL_GBL_Get_DataSets(si, conn);
                var sqa    = new SqlDataAdapter();
                var sql    = @"
SELECT cr.ChangeRequestID, dc.DimName, cr.ChangeType, cr.Status,
       cr.SubmittedBy, cr.SubmittedDate, cr.UpdatedDate,
       (SELECT COUNT(*) FROM MDM_ChangeRequestStep cs WHERE cs.ChangeRequestID = cr.ChangeRequestID) AS StepCount,
       (SELECT COUNT(*) FROM MDM_ChangeRequestStep cs WHERE cs.ChangeRequestID = cr.ChangeRequestID AND cs.CompletedDate IS NOT NULL) AS StepsCompleted
FROM   MDM_ChangeRequest cr
JOIN   MDM_DimConfig     dc ON dc.DimConfigID = cr.DimConfigID
WHERE  cr.SubmittedBy = @User
ORDER  BY cr.SubmittedDate DESC";
                var sqlparams = new[] { new SqlParameter("@User", SqlDbType.NVarChar) { Value = si.AuthToken.UserName } };
                helper.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
            }
            return dt;
        }

        /// <summary>
        /// Returns change requests whose current in-flight step is assigned to the current user.
        /// This is the approver's inbox.
        /// </summary>
        private DataTable Get_MDM_ApproverInbox()
        {
            var dt     = new DataTable("MDM_ApproverInbox");
            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                var helper = new SQL_GBL_Get_DataSets(si, conn);
                var sqa    = new SqlDataAdapter();
                var sql    = @"
SELECT cr.ChangeRequestID, dc.DimName, cr.ChangeType, cr.Status,
       cr.SubmittedBy, cr.SubmittedDate,
       cs.StepOrder, cs.AssignedDate,
       s.Name AS StepName
FROM   MDM_ChangeRequest     cr
JOIN   MDM_DimConfig         dc ON dc.DimConfigID    = cr.DimConfigID
JOIN   MDM_ChangeRequestStep cs ON cs.ChangeRequestID = cr.ChangeRequestID
                                AND cs.CompletedDate IS NULL
JOIN   MDM_ApprovalStep      s  ON s.StepID           = cs.StepID
WHERE  cs.AssignedTo = @User
  AND  cr.Status IN (@Submitted, @InReview)
ORDER  BY cr.SubmittedDate ASC";
                var sqlparams = new[]
                {
                    new SqlParameter("@User",      SqlDbType.NVarChar) { Value = si.AuthToken.UserName },
                    new SqlParameter("@Submitted", SqlDbType.Int)      { Value = (int)MDM_ConfigHelpers.ApprovalStatus.Submitted },
                    new SqlParameter("@InReview",  SqlDbType.Int)      { Value = (int)MDM_ConfigHelpers.ApprovalStatus.InReview  }
                };
                helper.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
            }
            return dt;
        }

        /// <summary>Returns the full detail payload + current step for a single change request.</summary>
        private DataTable Get_MDM_ChangeRequestDetail()
        {
            var dt     = new DataTable("MDM_ChangeRequestDetail");
            var reqIDStr = args.NameValuePairs.XFGetValue(MDM_Support.Param_SelChangeReq, "0");
            if (!int.TryParse(reqIDStr, out int reqID) || reqID <= 0) return dt;

            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                var helper = new SQL_GBL_Get_DataSets(si, conn);
                var sqa    = new SqlDataAdapter();
                var sql    = @"
SELECT cr.ChangeRequestID, dc.DimName, cr.ChangeType, cr.Payload, cr.Status,
       cr.SubmittedBy, cr.SubmittedDate, cr.UpdatedBy, cr.UpdatedDate,
       cs.StepOrder, s.Name AS CurrentStepName, cs.AssignedTo AS CurrentAssignee
FROM   MDM_ChangeRequest     cr
JOIN   MDM_DimConfig         dc ON dc.DimConfigID    = cr.DimConfigID
LEFT JOIN MDM_ChangeRequestStep cs ON cs.ChangeRequestID = cr.ChangeRequestID
                                   AND cs.CompletedDate IS NULL
LEFT JOIN MDM_ApprovalStep      s  ON s.StepID           = cs.StepID
WHERE  cr.ChangeRequestID = @ReqID";
                var sqlparams = new[] { new SqlParameter("@ReqID", SqlDbType.Int) { Value = reqID } };
                helper.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
            }
            return dt;
        }

        /// <summary>Returns the full audit trail for a change request.</summary>
        private DataTable Get_MDM_ChangeRequestAudit()
        {
            var dt     = new DataTable("MDM_ChangeRequestAudit");
            var reqIDStr = args.NameValuePairs.XFGetValue(MDM_Support.Param_SelChangeReq, "0");
            if (!int.TryParse(reqIDStr, out int reqID) || reqID <= 0) return dt;

            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                var helper = new SQL_GBL_Get_DataSets(si, conn);
                var sqa    = new SqlDataAdapter();
                var sql    = @"
SELECT AuditID, ChangeRequestID, Action, ActionBy, ActionDate, Comment
FROM   MDM_ChangeRequestAudit
WHERE  ChangeRequestID = @ReqID
ORDER  BY ActionDate ASC";
                var sqlparams = new[] { new SqlParameter("@ReqID", SqlDbType.Int) { Value = reqID } };
                helper.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
            }
            return dt;
        }
        #endregion

        #region "Feature 4 — Validations"

        private DataTable Get_MDM_ValidationResults()
        {
            var dt     = new DataTable("MDM_ValidationResults");
            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                var helper = new SQL_GBL_Get_DataSets(si, conn);
                var sqa    = new SqlDataAdapter();
                var dimID  = args.NameValuePairs.XFGetValue("IV_MDM_DimConfigID", "0");
                var sql    = @"
SELECT vres.ResultID, vres.ValidationRuleID, vr.Name AS RuleName,
       vr.RuleType, vr.Severity,
       dc.DimName, vres.MemberName, vres.ViolationDesc,
       vres.RunDate, vres.Status
FROM   MDM_ValidationResult vres
JOIN   MDM_ValidationRule   vr  ON vr.ValidationRuleID = vres.ValidationRuleID
JOIN   MDM_DimConfig        dc  ON dc.DimConfigID      = vr.DimConfigID
WHERE  (@DimConfigID = 0 OR vr.DimConfigID = @DimConfigID)
  AND  vres.Status = 'Open'
ORDER  BY vr.Severity, vres.RunDate DESC";
                var sqlparams = new[] { new SqlParameter("@DimConfigID", SqlDbType.Int)
                    { Value = int.TryParse(dimID, out var d) ? d : 0 } };
                helper.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
            }
            return dt;
        }
        #endregion

        #region "Feature 6 — Reports"

        /// <summary>Report: all in-flight change requests, by dimension, status, submitter, and age.</summary>
        private DataTable Get_MDM_Rpt_PendingChanges()
        {
            var dt     = new DataTable("MDM_Rpt_PendingChanges");
            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                var helper = new SQL_GBL_Get_DataSets(si, conn);
                var sqa    = new SqlDataAdapter();
                var sql    = @"
SELECT cr.ChangeRequestID, dc.DimName, cr.ChangeType, cr.Status,
       cr.SubmittedBy, cr.SubmittedDate,
       DATEDIFF(day, cr.SubmittedDate, GETDATE()) AS AgeDays,
       s.Name AS CurrentStep, cs.AssignedTo AS CurrentAssignee
FROM   MDM_ChangeRequest     cr
JOIN   MDM_DimConfig         dc ON dc.DimConfigID    = cr.DimConfigID
LEFT JOIN MDM_ChangeRequestStep cs ON cs.ChangeRequestID = cr.ChangeRequestID AND cs.CompletedDate IS NULL
LEFT JOIN MDM_ApprovalStep      s  ON s.StepID           = cs.StepID
WHERE  cr.Status NOT IN (@Applied, @Withdrawn, @Rejected)
ORDER  BY cr.SubmittedDate ASC";
                var sqlparams = new[]
                {
                    new SqlParameter("@Applied",   SqlDbType.Int) { Value = (int)MDM_ConfigHelpers.ApprovalStatus.Applied   },
                    new SqlParameter("@Withdrawn", SqlDbType.Int) { Value = (int)MDM_ConfigHelpers.ApprovalStatus.Withdrawn },
                    new SqlParameter("@Rejected",  SqlDbType.Int) { Value = (int)MDM_ConfigHelpers.ApprovalStatus.Rejected  }
                };
                helper.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
            }
            return dt;
        }

        /// <summary>Report: average approval cycle time per step, dim, and approver.</summary>
        private DataTable Get_MDM_Rpt_ApprovalCycleTime()
        {
            var dt     = new DataTable("MDM_Rpt_ApprovalCycleTime");
            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                var helper = new SQL_GBL_Get_DataSets(si, conn);
                var sqa    = new SqlDataAdapter();
                var sql    = @"
SELECT dc.DimName, s.Name AS StepName, cs.AssignedTo,
       COUNT(*)                                                      AS RequestCount,
       AVG(DATEDIFF(hour, cs.AssignedDate, cs.CompletedDate)) / 24.0 AS AvgDays,
       MAX(DATEDIFF(hour, cs.AssignedDate, cs.CompletedDate)) / 24.0 AS MaxDays
FROM   MDM_ChangeRequestStep cs
JOIN   MDM_ApprovalStep      s  ON s.StepID           = cs.StepID
JOIN   MDM_ChangeRequest     cr ON cr.ChangeRequestID = cs.ChangeRequestID
JOIN   MDM_DimConfig         dc ON dc.DimConfigID     = cr.DimConfigID
WHERE  cs.CompletedDate IS NOT NULL
GROUP  BY dc.DimName, s.Name, cs.AssignedTo
ORDER  BY dc.DimName, s.Name";
                helper.Fill_Get_GBL_DT(si, sqa, dt, sql);
            }
            return dt;
        }

        /// <summary>Report: integration run summary — records processed/matched/failed over time.</summary>
        private DataTable Get_MDM_Rpt_IntegrationSummary()
        {
            var dt     = new DataTable("MDM_Rpt_IntegrationSummary");
            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                var helper = new SQL_GBL_Get_DataSets(si, conn);
                var sqa    = new SqlDataAdapter();
                var sql    = @"
SELECT ic.Name AS IntegrationName, dc.DimName, r.Direction,
       CAST(r.StartDate AS DATE) AS RunDate,
       SUM(r.RecordsProcessed) AS Processed,
       SUM(r.RecordsMatched)   AS Matched,
       SUM(r.RecordsFailed)    AS Failed
FROM   MDM_IntegrationRunLog r
JOIN   MDM_IntegrationConfig  ic ON ic.IntConfigID = r.IntConfigID
JOIN   MDM_DimConfig          dc ON dc.DimConfigID = ic.DimConfigID
GROUP  BY ic.Name, dc.DimName, r.Direction, CAST(r.StartDate AS DATE)
ORDER  BY RunDate DESC, ic.Name";
                helper.Fill_Get_GBL_DT(si, sqa, dt, sql);
            }
            return dt;
        }

        /// <summary>Report: open validation violations by rule type, severity, and dimension.</summary>
        private DataTable Get_MDM_Rpt_ValidationExceptions()
        {
            var dt     = new DataTable("MDM_Rpt_ValidationExceptions");
            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                var helper = new SQL_GBL_Get_DataSets(si, conn);
                var sqa    = new SqlDataAdapter();
                var sql    = @"
SELECT dc.DimName, vr.RuleType, vr.Severity, vr.Name AS RuleName,
       COUNT(*) AS ViolationCount,
       MAX(vres.RunDate) AS LastRunDate
FROM   MDM_ValidationResult vres
JOIN   MDM_ValidationRule   vr  ON vr.ValidationRuleID = vres.ValidationRuleID
JOIN   MDM_DimConfig        dc  ON dc.DimConfigID      = vr.DimConfigID
WHERE  vres.Status = 'Open'
GROUP  BY dc.DimName, vr.RuleType, vr.Severity, vr.Name
ORDER  BY vr.Severity, dc.DimName, vr.Name";
                helper.Fill_Get_GBL_DT(si, sqa, dt, sql);
            }
            return dt;
        }

        /// <summary>Report: full audit trail for a specific member across all change requests.</summary>
        private DataTable Get_MDM_Rpt_MemberChangeHistory()
        {
            var dt         = new DataTable("MDM_Rpt_MemberChangeHistory");
            var memberName = args.NameValuePairs.XFGetValue(MDM_Support.Param_SelMember, string.Empty);
            if (string.IsNullOrEmpty(memberName)) return dt;

            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                var helper = new SQL_GBL_Get_DataSets(si, conn);
                var sqa    = new SqlDataAdapter();
                // Payload contains the member name — search via LIKE for simplicity.
                var sql    = @"
SELECT cr.ChangeRequestID, dc.DimName, cr.ChangeType, cr.Status,
       cr.SubmittedBy, cr.SubmittedDate, cr.UpdatedDate, cr.Payload
FROM   MDM_ChangeRequest cr
JOIN   MDM_DimConfig     dc ON dc.DimConfigID = cr.DimConfigID
WHERE  cr.Payload LIKE @MemberPattern
ORDER  BY cr.SubmittedDate DESC";
                var sqlparams = new[] { new SqlParameter("@MemberPattern", SqlDbType.NVarChar)
                    { Value = $"%\"{memberName}\"%".Replace("'","''") } };
                helper.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
            }
            return dt;
        }
        #endregion
    }
}
