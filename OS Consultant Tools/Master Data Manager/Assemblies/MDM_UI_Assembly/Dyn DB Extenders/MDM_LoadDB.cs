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

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.MDM_LoadDB
{
    /// <summary>
    /// DashboardExtender for the MDM end-user workspace shell.
    /// Handles initial dashboard load, show/hide of the side nav, and menu option selection.
    ///
    /// Also handles user-side actions:
    ///   - Submit change request (stage + submit for approval).
    ///   - Approve / Reject / Reassign a change request.
    ///   - Withdraw a change request.
    ///   - Apply an approved change request to the live dimension.
    /// </summary>
    public class MainClass
    {
        #region "Global Params"
        private SessionInfo si;
        private BRGlobals globals;
        private object api;
        private DashboardExtenderArgs args;
        private readonly GBL_Helpers gblHelpers = new GBL_Helpers();

        private string MainMenuParam        = MDM_Support.Param_AppMenu;
        private string showHideIVName       = "IV_MDM_App_ShowHide_MenuBtn";
        private string showBtnVisibleName   = "IV_MDM_App_DispShow_MenuBtn";
        private string hideBtnVisibleName   = "IV_MDM_App_DispHide_MenuBtn";
        private string menuWidthIV          = "IV_MDM_App_MenuWidth";
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
                    case DashboardExtenderFunctionType.LoadDashboard:
                        if (args.FunctionName.XFEqualsIgnoreCase("MDM_LoadDB"))
                            return LoadDB(ref args);
                        break;

                    case DashboardExtenderFunctionType.ComponentSelectionChanged:
                        // Menu selection
                        if (args.SelectionChangedTaskInfo?.ComponentName
                                ?.XFEqualsIgnoreCase(MainMenuParam) == true)
                            return OnMenuChanged(ref args);

                        // Show/hide toggle
                        if (args.SelectionChangedTaskInfo?.ComponentName
                                ?.XFEqualsIgnoreCase(showHideIVName) == true)
                            return OnShowHideChanged(ref args);

                        // Approval actions
                        if (args.FunctionName.XFEqualsIgnoreCase("MDM_Submit"))
                            return OnSubmitChangeRequest(ref args);
                        if (args.FunctionName.XFEqualsIgnoreCase("MDM_Approve"))
                            return OnApproveChangeRequest(ref args);
                        if (args.FunctionName.XFEqualsIgnoreCase("MDM_Reject"))
                            return OnRejectChangeRequest(ref args);
                        if (args.FunctionName.XFEqualsIgnoreCase("MDM_Reassign"))
                            return OnReassignChangeRequest(ref args);
                        if (args.FunctionName.XFEqualsIgnoreCase("MDM_Withdraw"))
                            return OnWithdrawChangeRequest(ref args);
                        if (args.FunctionName.XFEqualsIgnoreCase("MDM_Apply"))
                            return OnApplyChangeRequest(ref args);
                        break;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #region "Load Dashboard"
        private XFLoadDashboardTaskResult LoadDB(ref DashboardExtenderArgs args)
        {
            var result = new XFLoadDashboardTaskResult { ChangeCustomSubstVarsInDashboard = true };
            setInitialParams(ref args, ref result);
            updateShowHide(ref args, ref result);
            setMenuOption(ref args, ref result);
            return result;
        }

        private void setInitialParams(ref DashboardExtenderArgs args, ref XFLoadDashboardTaskResult result)
        {
            if (args.LoadDashboardTaskInfo.Reason != LoadDashboardReasonType.Initialize
                || args.LoadDashboardTaskInfo.Action != LoadDashboardActionType.BeforeFirstGetParameters)
                return;

            gblHelpers.UpdateCustomSubstVar(ref result, globals, "IV_MDM_App_User",     si.AuthToken.UserName);
            gblHelpers.UpdateCustomSubstVar(ref result, globals, MainMenuParam,          "1");
            gblHelpers.UpdateCustomSubstVar(ref result, globals, showHideIVName,         "Show");
            gblHelpers.UpdateCustomSubstVar(ref result, globals, showBtnVisibleName,     "0");
            gblHelpers.UpdateCustomSubstVar(ref result, globals, hideBtnVisibleName,     "1");
            gblHelpers.UpdateCustomSubstVar(ref result, globals, menuWidthIV,            "220");
            gblHelpers.UpdateCustomSubstVar(ref result, globals, MDM_Support.Param_SelDim,      string.Empty);
            gblHelpers.UpdateCustomSubstVar(ref result, globals, MDM_Support.Param_SelMember,   string.Empty);
            gblHelpers.UpdateCustomSubstVar(ref result, globals, MDM_Support.Param_SelChangeReq, "0");
        }

        private void updateShowHide(ref DashboardExtenderArgs args, ref XFLoadDashboardTaskResult result)
        {
            var showHide = args.NameValuePairs.XFGetValue(showHideIVName, "Show");
            bool isShow  = showHide.XFEqualsIgnoreCase("Show");
            gblHelpers.UpdateCustomSubstVar(ref result, globals, showBtnVisibleName, isShow ? "0" : "1");
            gblHelpers.UpdateCustomSubstVar(ref result, globals, hideBtnVisibleName, isShow ? "1" : "0");
            gblHelpers.UpdateCustomSubstVar(ref result, globals, menuWidthIV,        isShow ? "220" : "0");
        }

        private void setMenuOption(ref DashboardExtenderArgs args, ref XFLoadDashboardTaskResult result)
        {
            var menuVal = args.NameValuePairs.XFGetValue(MainMenuParam, "1");
            gblHelpers.UpdateCustomSubstVar(ref result, globals, MainMenuParam, menuVal);
        }
        #endregion

        #region "Menu Selection Changed"
        private XFSelectionChangedTaskResult OnMenuChanged(ref DashboardExtenderArgs args)
        {
            var result = new XFSelectionChangedTaskResult
            {
                IsOK                                      = true,
                ShowMessageBox                            = false,
                ChangeSelectionChangedUIActionInDashboard = true,
                ModifiedCustomSubstVars                   = new Dictionary<string, string>()
            };

            var selectedVal = args.SelectionChangedTaskInfo?.SelectedValue?.ToString() ?? "1";
            gblHelpers.UpdateCustomSubstVar(ref result, MainMenuParam, selectedVal);
            // Reset selection context when switching tabs.
            gblHelpers.UpdateCustomSubstVar(ref result, MDM_Support.Param_SelMember,    string.Empty);
            gblHelpers.UpdateCustomSubstVar(ref result, MDM_Support.Param_SelChangeReq, "0");

            return result;
        }

        private XFSelectionChangedTaskResult OnShowHideChanged(ref DashboardExtenderArgs args)
        {
            var result = new XFSelectionChangedTaskResult
            {
                IsOK                                      = true,
                ShowMessageBox                            = false,
                ChangeSelectionChangedUIActionInDashboard = true,
                ModifiedCustomSubstVars                   = new Dictionary<string, string>()
            };

            var current  = args.NameValuePairs.XFGetValue(showHideIVName, "Show");
            var newState = current.XFEqualsIgnoreCase("Show") ? "Hide" : "Show";
            bool isShow  = newState.XFEqualsIgnoreCase("Show");

            gblHelpers.UpdateCustomSubstVar(ref result, showHideIVName,     newState);
            gblHelpers.UpdateCustomSubstVar(ref result, showBtnVisibleName, isShow ? "0" : "1");
            gblHelpers.UpdateCustomSubstVar(ref result, hideBtnVisibleName, isShow ? "1" : "0");
            gblHelpers.UpdateCustomSubstVar(ref result, menuWidthIV,        isShow ? "220" : "0");

            return result;
        }
        #endregion

        #region "Approval Actions"

        /// <summary>Advances a draft change request to Submitted, notifying the first approver.</summary>
        private XFSelectionChangedTaskResult OnSubmitChangeRequest(ref DashboardExtenderArgs args)
        {
            var result  = MakeSelectionResult();
            var reqIDStr = args.NameValuePairs.XFGetValue(MDM_Support.Param_SelChangeReq, "0");
            if (!int.TryParse(reqIDStr, out int reqID) || reqID <= 0)
                return Fail(result, "No change request selected.");

            UpdateChangeRequestStatus(reqID, MDM_ConfigHelpers.ApprovalStatus.Submitted);
            WriteStepAudit(reqID, "Submitted", args.NameValuePairs.XFGetValue("IV_MDM_ApprComment", string.Empty));

            result.ShowMessageBox = true;
            result.Message        = "Change request submitted for approval.";
            return result;
        }

        /// <summary>Approves the current step; advances to the next step or to Approved.</summary>
        private XFSelectionChangedTaskResult OnApproveChangeRequest(ref DashboardExtenderArgs args)
        {
            var result   = MakeSelectionResult();
            var reqIDStr = args.NameValuePairs.XFGetValue(MDM_Support.Param_SelChangeReq, "0");
            if (!int.TryParse(reqIDStr, out int reqID) || reqID <= 0)
                return Fail(result, "No change request selected.");

            var comment = args.NameValuePairs.XFGetValue("IV_MDM_ApprComment", string.Empty);
            bool hasNextStep = AdvanceApprovalStep(reqID, approved: true);

            if (!hasNextStep)
            {
                // All steps complete — mark as fully Approved.
                UpdateChangeRequestStatus(reqID, MDM_ConfigHelpers.ApprovalStatus.Approved);
                result.Message = "Change request fully approved.";
            }
            else
            {
                UpdateChangeRequestStatus(reqID, MDM_ConfigHelpers.ApprovalStatus.InReview);
                result.Message = "Step approved. Forwarded to next approver.";
            }

            WriteStepAudit(reqID, "Approved", comment);
            result.ShowMessageBox = true;
            return result;
        }

        /// <summary>Rejects the change request; returns it to the submitter.</summary>
        private XFSelectionChangedTaskResult OnRejectChangeRequest(ref DashboardExtenderArgs args)
        {
            var result   = MakeSelectionResult();
            var reqIDStr = args.NameValuePairs.XFGetValue(MDM_Support.Param_SelChangeReq, "0");
            if (!int.TryParse(reqIDStr, out int reqID) || reqID <= 0)
                return Fail(result, "No change request selected.");

            var comment = args.NameValuePairs.XFGetValue("IV_MDM_ApprComment", string.Empty);
            if (string.IsNullOrWhiteSpace(comment))
                return Fail(result, "A rejection comment is required.");

            UpdateChangeRequestStatus(reqID, MDM_ConfigHelpers.ApprovalStatus.Rejected);
            WriteStepAudit(reqID, "Rejected", comment);

            result.ShowMessageBox = true;
            result.Message        = "Change request rejected.";
            return result;
        }

        /// <summary>Reassigns the current step's approver.</summary>
        private XFSelectionChangedTaskResult OnReassignChangeRequest(ref DashboardExtenderArgs args)
        {
            var result      = MakeSelectionResult();
            var reqIDStr    = args.NameValuePairs.XFGetValue(MDM_Support.Param_SelChangeReq, "0");
            var newAssignee = args.NameValuePairs.XFGetValue("IV_MDM_NewAssignee", string.Empty);
            if (!int.TryParse(reqIDStr, out int reqID) || reqID <= 0)
                return Fail(result, "No change request selected.");
            if (string.IsNullOrWhiteSpace(newAssignee))
                return Fail(result, "A new assignee is required for reassignment.");

            ReassignCurrentStep(reqID, newAssignee);
            WriteStepAudit(reqID, $"Reassigned to {newAssignee}",
                args.NameValuePairs.XFGetValue("IV_MDM_ApprComment", string.Empty));

            result.ShowMessageBox = true;
            result.Message        = $"Change request reassigned to {newAssignee}.";
            return result;
        }

        /// <summary>Withdraws a Draft or Submitted change request back to the submitter.</summary>
        private XFSelectionChangedTaskResult OnWithdrawChangeRequest(ref DashboardExtenderArgs args)
        {
            var result   = MakeSelectionResult();
            var reqIDStr = args.NameValuePairs.XFGetValue(MDM_Support.Param_SelChangeReq, "0");
            if (!int.TryParse(reqIDStr, out int reqID) || reqID <= 0)
                return Fail(result, "No change request selected.");

            UpdateChangeRequestStatus(reqID, MDM_ConfigHelpers.ApprovalStatus.Withdrawn);
            WriteStepAudit(reqID, "Withdrawn",
                args.NameValuePairs.XFGetValue("IV_MDM_ApprComment", string.Empty));

            result.ShowMessageBox = true;
            result.Message        = "Change request withdrawn.";
            return result;
        }

        /// <summary>Applies an Approved change request to the live OneStream dimension.</summary>
        private XFSelectionChangedTaskResult OnApplyChangeRequest(ref DashboardExtenderArgs args)
        {
            var result   = MakeSelectionResult();
            var reqIDStr = args.NameValuePairs.XFGetValue(MDM_Support.Param_SelChangeReq, "0");
            if (!int.TryParse(reqIDStr, out int reqID) || reqID <= 0)
                return Fail(result, "No change request selected.");

            var (success, message) = MDM_ReorgSvc.ApplyChangeRequest(si, reqID);
            result.ShowMessageBox  = true;
            result.Message         = message;
            result.IsOK            = success;
            return result;
        }
        #endregion

        #region "Approval DB Helpers"

        private void UpdateChangeRequestStatus(int reqID, MDM_ConfigHelpers.ApprovalStatus status)
        {
            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                conn.Open();
                var sql = @"
UPDATE MDM_ChangeRequest
SET    Status = @Status, UpdatedBy = @User, UpdatedDate = GETDATE()
WHERE  ChangeRequestID = @ReqID";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.Int)      { Value = (int)status });
                cmd.Parameters.Add(new SqlParameter("@User",   SqlDbType.NVarChar) { Value = si.AuthToken.UserName });
                cmd.Parameters.Add(new SqlParameter("@ReqID",  SqlDbType.Int)      { Value = reqID });
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Advances to the next approval step. Returns true when more steps remain,
        /// false when the final step was the current one.
        /// </summary>
        private bool AdvanceApprovalStep(int reqID, bool approved)
        {
            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                conn.Open();

                // Mark the current in-progress step as complete.
                var markDone = @"
UPDATE MDM_ChangeRequestStep
SET    CompletedDate = GETDATE(), CompletedBy = @User, Decision = @Decision
WHERE  ChangeRequestID = @ReqID
  AND  CompletedDate IS NULL
  AND  Decision IS NULL";
                using (var cmd = new SqlCommand(markDone, conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@User",     SqlDbType.NVarChar) { Value = si.AuthToken.UserName });
                    cmd.Parameters.Add(new SqlParameter("@Decision", SqlDbType.NVarChar) { Value = approved ? "Approved" : "Rejected" });
                    cmd.Parameters.Add(new SqlParameter("@ReqID",    SqlDbType.Int)      { Value = reqID });
                    cmd.ExecuteNonQuery();
                }

                if (!approved) return false;

                // Check whether a next step exists.
                var nextStepSql = @"
INSERT INTO MDM_ChangeRequestStep (ChangeRequestID, StepID, StepOrder, AssignedTo, AssignedDate)
SELECT TOP 1 @ReqID, s.StepID, s.StepOrder, s.Assignee, GETDATE()
FROM   MDM_ApprovalStep s
JOIN   MDM_ChangeRequest cr ON cr.WorkflowID = s.WorkflowID
WHERE  cr.ChangeRequestID = @ReqID
  AND  s.StepOrder > (
        SELECT ISNULL(MAX(cs2.StepOrder), 0)
        FROM   MDM_ChangeRequestStep cs2
        WHERE  cs2.ChangeRequestID = @ReqID
  )
  AND  s.Status = 1
ORDER  BY s.StepOrder";
                using (var cmd = new SqlCommand(nextStepSql, conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@ReqID", SqlDbType.Int) { Value = reqID });
                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0;
                }
            }
        }

        private void ReassignCurrentStep(int reqID, string newAssignee)
        {
            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                conn.Open();
                var sql = @"
UPDATE MDM_ChangeRequestStep
SET    AssignedTo = @Assignee
WHERE  ChangeRequestID = @ReqID
  AND  CompletedDate IS NULL";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@Assignee", SqlDbType.NVarChar) { Value = newAssignee });
                cmd.Parameters.Add(new SqlParameter("@ReqID",    SqlDbType.Int)      { Value = reqID });
                cmd.ExecuteNonQuery();
            }
        }

        private void WriteStepAudit(int reqID, string action, string comment)
        {
            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                conn.Open();
                var sql = @"
INSERT INTO MDM_ChangeRequestAudit (ChangeRequestID, Action, ActionBy, ActionDate, Comment)
VALUES (@ReqID, @Action, @User, GETDATE(), @Comment)";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@ReqID",   SqlDbType.Int)      { Value = reqID });
                cmd.Parameters.Add(new SqlParameter("@Action",  SqlDbType.NVarChar) { Value = action });
                cmd.Parameters.Add(new SqlParameter("@User",    SqlDbType.NVarChar) { Value = si.AuthToken.UserName });
                cmd.Parameters.Add(new SqlParameter("@Comment", SqlDbType.NVarChar) { Value = (object)comment ?? DBNull.Value });
                cmd.ExecuteNonQuery();
            }
        }
        #endregion

        #region "Helpers"
        private static XFSelectionChangedTaskResult MakeSelectionResult() =>
            new XFSelectionChangedTaskResult
            {
                IsOK                                      = true,
                ShowMessageBox                            = false,
                Message                                   = string.Empty,
                ChangeSelectionChangedUIActionInDashboard = true,
                ModifiedCustomSubstVars                   = new Dictionary<string, string>()
            };

        private static XFSelectionChangedTaskResult Fail(XFSelectionChangedTaskResult r, string msg)
        {
            r.IsOK           = false;
            r.ShowMessageBox = true;
            r.Message        = msg;
            return r;
        }
        #endregion
    }
}
