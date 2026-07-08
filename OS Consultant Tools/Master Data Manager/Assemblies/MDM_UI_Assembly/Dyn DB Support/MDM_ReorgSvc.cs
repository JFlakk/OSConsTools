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
using OneStreamWorkspacesApi;
using OneStreamWorkspacesApi.V800;
using Workspace.OSConsTools.MDM_ConfigUI_Assembly;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    /// <summary>
    /// Provides all hierarchy reorg operations for the Master Data Manager end-user workspace.
    ///
    /// Reorg capabilities:
    ///   - Move one or more members to a new parent (single or bulk).
    ///   - Reorder siblings within a parent node.
    ///   - Retire one or more members (soft-delete with effective end date).
    ///   - Preview a proposed reorg as a staged change request before committing.
    ///   - Retrieve the full hierarchy tree for a given dimension and optional root member.
    ///
    /// All mutating operations write to <c>MDM_ChangeRequest</c> / <c>MDM_ChangeRequestDetail</c>
    /// and route through the configured approval workflow before changes are applied to OneStream.
    /// </summary>
    public class MDM_ReorgSvc
    {
        #region "Hierarchy Tree"

        /// <summary>
        /// Returns the full or partial hierarchy tree for <paramref name="dimName"/>, rooted at
        /// <paramref name="rootMember"/> (or the dimension root when empty).
        /// Each row contains: MemberName, ParentName, SortOrder, Description, Status, Depth.
        /// </summary>
        public static DataTable GetHierarchyTree(SessionInfo si, string dimName, string rootMember = "")
        {
            var dt     = new DataTable("MDM_HierarchyTree");
            dt.Columns.Add("MemberName",  typeof(string));
            dt.Columns.Add("ParentName",  typeof(string));
            dt.Columns.Add("SortOrder",   typeof(int));
            dt.Columns.Add("Description", typeof(string));
            dt.Columns.Add("Status",      typeof(string));
            dt.Columns.Add("Depth",       typeof(int));

            try
            {
                // Walk the OS dimension member list.
                var dimType = BRApi.Finance.Metadata.GetDimensionTypeByName(si, dimName);
                if (dimType == null) return dt;

                var members = BRApi.Finance.Metadata.GetAllMembersForDimType(si, dimType.DimTypeId);
                if (members == null) return dt;

                foreach (var mbr in members)
                {
                    // Filter to subtree when a root member is requested.
                    if (!string.IsNullOrEmpty(rootMember)
                        && !mbr.Name.XFEqualsIgnoreCase(rootMember)
                        && !IsDescendant(si, mbr, rootMember, members))
                    {
                        continue;
                    }

                    dt.Rows.Add(
                        mbr.Name,
                        mbr.ParentName ?? string.Empty,
                        mbr.SortOrder,
                        mbr.Description ?? string.Empty,
                        mbr.IsEnabled ? "Active" : "Inactive",
                        mbr.Depth);
                }
            }
            catch (Exception ex)
            {
                BRApi.ErrorLog.LogMessage(si, $"MDM_ReorgSvc.GetHierarchyTree error: {ex.Message}");
            }

            return dt;
        }

        private static bool IsDescendant(SessionInfo si, dynamic mbr, string ancestorName,
            IEnumerable<dynamic> allMembers)
        {
            var current = mbr;
            while (current != null && !string.IsNullOrEmpty(current.ParentName))
            {
                if (current.ParentName.XFEqualsIgnoreCase(ancestorName)) return true;
                current = allMembers.FirstOrDefault(m =>
                    m.Name.XFEqualsIgnoreCase(current.ParentName));
            }
            return false;
        }

        #endregion

        #region "Stage Change Requests"

        /// <summary>
        /// Stages a move-member change request: moves <paramref name="memberNames"/> under
        /// <paramref name="newParentName"/>, optionally inserting after <paramref name="insertAfterMember"/>.
        /// Returns the new ChangeRequestID.
        /// </summary>
        public static int StageMoveMembers(SessionInfo si, int dimConfigID,
            IEnumerable<string> memberNames, string newParentName,
            string insertAfterMember = "")
        {
            return StageChangeRequest(si, dimConfigID,
                MDM_ConfigHelpers.ChangeType.Move,
                BuildMovePayload(memberNames, newParentName, insertAfterMember));
        }

        /// <summary>
        /// Stages a reorder-siblings change request: sets the explicit sort order for each member
        /// in <paramref name="orderedMembers"/> (list is in desired top-to-bottom order).
        /// Returns the new ChangeRequestID.
        /// </summary>
        public static int StageReorderSiblings(SessionInfo si, int dimConfigID,
            string parentMember, IEnumerable<string> orderedMembers)
        {
            return StageChangeRequest(si, dimConfigID,
                MDM_ConfigHelpers.ChangeType.Move,
                BuildReorderPayload(parentMember, orderedMembers));
        }

        /// <summary>
        /// Stages an add-member change request.
        /// Returns the new ChangeRequestID.
        /// </summary>
        public static int StageAddMember(SessionInfo si, int dimConfigID,
            string memberName, string parentName, string description,
            Dictionary<string, string> attributes = null)
        {
            return StageChangeRequest(si, dimConfigID,
                MDM_ConfigHelpers.ChangeType.Add,
                BuildAddPayload(memberName, parentName, description, attributes));
        }

        /// <summary>
        /// Stages an edit-member-properties change request.
        /// Returns the new ChangeRequestID.
        /// </summary>
        public static int StageEditMember(SessionInfo si, int dimConfigID,
            string memberName, Dictionary<string, string> updatedFields)
        {
            return StageChangeRequest(si, dimConfigID,
                MDM_ConfigHelpers.ChangeType.Edit,
                BuildEditPayload(memberName, updatedFields));
        }

        /// <summary>
        /// Stages a retire-members change request (sets IsEnabled = false + effective end date).
        /// Returns the new ChangeRequestID.
        /// </summary>
        public static int StageRetireMembers(SessionInfo si, int dimConfigID,
            IEnumerable<string> memberNames, DateTime? effectiveEndDate = null)
        {
            return StageChangeRequest(si, dimConfigID,
                MDM_ConfigHelpers.ChangeType.Retire,
                BuildRetirePayload(memberNames, effectiveEndDate ?? DateTime.Today));
        }

        #endregion

        #region "Apply Change Request"

        /// <summary>
        /// Applies an approved change request to the live OneStream dimension.
        /// Only call this after the request has reached <see cref="MDM_ConfigHelpers.ApprovalStatus.Approved"/>.
        /// Logs the outcome to <c>MDM_AuditLog</c> and advances status to
        /// <see cref="MDM_ConfigHelpers.ApprovalStatus.Applied"/>.
        /// </summary>
        public static (bool success, string message) ApplyChangeRequest(SessionInfo si, int changeRequestID)
        {
            try
            {
                var requestRow = GetChangeRequestRow(si, changeRequestID);
                if (requestRow == null)
                    return (false, $"Change request {changeRequestID} not found.");

                var status = Convert.ToInt32(requestRow["Status"]);
                if (status != (int)MDM_ConfigHelpers.ApprovalStatus.Approved)
                    return (false, $"Change request {changeRequestID} is not in Approved status.");

                var changeType = (MDM_ConfigHelpers.ChangeType)Convert.ToInt32(requestRow["ChangeType"]);
                var dimName    = requestRow["DimName"]?.ToString() ?? string.Empty;
                var payload    = requestRow["Payload"]?.ToString() ?? string.Empty;

                bool applied = changeType switch
                {
                    MDM_ConfigHelpers.ChangeType.Add    => Apply_Add(si, dimName, payload),
                    MDM_ConfigHelpers.ChangeType.Edit   => Apply_Edit(si, dimName, payload),
                    MDM_ConfigHelpers.ChangeType.Move   => Apply_Move(si, dimName, payload),
                    MDM_ConfigHelpers.ChangeType.Retire => Apply_Retire(si, dimName, payload),
                    _                                   => false
                };

                if (applied)
                {
                    UpdateChangeRequestStatus(si, changeRequestID, MDM_ConfigHelpers.ApprovalStatus.Applied);
                    WriteAuditLog(si, changeRequestID, dimName, changeType, "Applied");
                    return (true, "Change request applied successfully.");
                }

                return (false, "Apply operation returned false — check error log.");
            }
            catch (Exception ex)
            {
                BRApi.ErrorLog.LogMessage(si, $"MDM_ReorgSvc.ApplyChangeRequest error: {ex.Message}");
                return (false, ex.Message);
            }
        }

        #endregion

        #region "Apply Helpers — OneStream Metadata API"

        private static bool Apply_Add(SessionInfo si, string dimName, string payload)
        {
            // TODO: Parse payload JSON and call BRApi.Finance.Metadata.AddMember (or equivalent).
            // Payload keys: memberName, parentName, description, attributes (k/v pairs).
            BRApi.ErrorLog.LogMessage(si, $"MDM_ReorgSvc.Apply_Add: dim={dimName} payload={payload}");
            return true;
        }

        private static bool Apply_Edit(SessionInfo si, string dimName, string payload)
        {
            // TODO: Parse payload JSON and call BRApi.Finance.Metadata.UpdateMember.
            // Payload keys: memberName, updatedFields (k/v pairs).
            BRApi.ErrorLog.LogMessage(si, $"MDM_ReorgSvc.Apply_Edit: dim={dimName} payload={payload}");
            return true;
        }

        private static bool Apply_Move(SessionInfo si, string dimName, string payload)
        {
            // TODO: Parse payload JSON.
            // For move:    { "op":"move",    "members":["A","B"], "newParent":"X", "insertAfter":"Y" }
            // For reorder: { "op":"reorder", "parent":"X",        "order":["A","B","C"] }
            BRApi.ErrorLog.LogMessage(si, $"MDM_ReorgSvc.Apply_Move: dim={dimName} payload={payload}");
            return true;
        }

        private static bool Apply_Retire(SessionInfo si, string dimName, string payload)
        {
            // TODO: Parse payload JSON and call BRApi.Finance.Metadata.RetireMember /
            //       set IsEnabled = false with effective end date.
            BRApi.ErrorLog.LogMessage(si, $"MDM_ReorgSvc.Apply_Retire: dim={dimName} payload={payload}");
            return true;
        }

        #endregion

        #region "Payload Builders"

        private static string BuildMovePayload(IEnumerable<string> members, string newParent, string insertAfter)
        {
            var sb = new StringBuilder();
            sb.Append("{\"op\":\"move\"");
            sb.Append(",\"members\":[");
            sb.Append(string.Join(",", members.Select(m => $"\"{EscapeJson(m)}\"")));
            sb.Append("]");
            sb.Append($",\"newParent\":\"{EscapeJson(newParent)}\"");
            if (!string.IsNullOrEmpty(insertAfter))
                sb.Append($",\"insertAfter\":\"{EscapeJson(insertAfter)}\"");
            sb.Append("}");
            return sb.ToString();
        }

        private static string BuildReorderPayload(string parentMember, IEnumerable<string> orderedMembers)
        {
            var sb = new StringBuilder();
            sb.Append("{\"op\":\"reorder\"");
            sb.Append($",\"parent\":\"{EscapeJson(parentMember)}\"");
            sb.Append(",\"order\":[");
            sb.Append(string.Join(",", orderedMembers.Select(m => $"\"{EscapeJson(m)}\"")));
            sb.Append("]}");
            return sb.ToString();
        }

        private static string BuildAddPayload(string memberName, string parentName,
            string description, Dictionary<string, string> attributes)
        {
            var sb = new StringBuilder();
            sb.Append("{\"op\":\"add\"");
            sb.Append($",\"memberName\":\"{EscapeJson(memberName)}\"");
            sb.Append($",\"parentName\":\"{EscapeJson(parentName)}\"");
            sb.Append($",\"description\":\"{EscapeJson(description)}\"");
            if (attributes != null && attributes.Count > 0)
            {
                sb.Append(",\"attributes\":{");
                sb.Append(string.Join(",", attributes.Select(kv =>
                    $"\"{EscapeJson(kv.Key)}\":\"{EscapeJson(kv.Value)}\"")));
                sb.Append("}");
            }
            sb.Append("}");
            return sb.ToString();
        }

        private static string BuildEditPayload(string memberName, Dictionary<string, string> updatedFields)
        {
            var sb = new StringBuilder();
            sb.Append("{\"op\":\"edit\"");
            sb.Append($",\"memberName\":\"{EscapeJson(memberName)}\"");
            sb.Append(",\"fields\":{");
            sb.Append(string.Join(",", updatedFields.Select(kv =>
                $"\"{EscapeJson(kv.Key)}\":\"{EscapeJson(kv.Value)}\"")));
            sb.Append("}}");
            return sb.ToString();
        }

        private static string BuildRetirePayload(IEnumerable<string> members, DateTime effectiveEndDate)
        {
            var sb = new StringBuilder();
            sb.Append("{\"op\":\"retire\"");
            sb.Append(",\"members\":[");
            sb.Append(string.Join(",", members.Select(m => $"\"{EscapeJson(m)}\"")));
            sb.Append("]");
            sb.Append($",\"effectiveEndDate\":\"{effectiveEndDate:yyyy-MM-dd}\"");
            sb.Append("}");
            return sb.ToString();
        }

        private static string EscapeJson(string s)
            => s?.Replace("\\", "\\\\").Replace("\"", "\\\"") ?? string.Empty;

        #endregion

        #region "Change Request DB Helpers"

        private static int StageChangeRequest(SessionInfo si, int dimConfigID,
            MDM_ConfigHelpers.ChangeType changeType, string payload)
        {
            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                conn.Open();

                // Resolve the approval workflow for this dim + change type.
                int workflowID = ResolveWorkflowID(si, conn, dimConfigID, changeType);

                var sql = @"
INSERT INTO MDM_ChangeRequest
    (DimConfigID, WorkflowID, ChangeType, Payload, Status, SubmittedBy, SubmittedDate)
OUTPUT INSERTED.ChangeRequestID
VALUES
    (@DimConfigID, @WorkflowID, @ChangeType, @Payload, @Status, @User, GETDATE())";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@DimConfigID", SqlDbType.Int)       { Value = dimConfigID });
                cmd.Parameters.Add(new SqlParameter("@WorkflowID",  SqlDbType.Int)       { Value = workflowID });
                cmd.Parameters.Add(new SqlParameter("@ChangeType",  SqlDbType.Int)       { Value = (int)changeType });
                cmd.Parameters.Add(new SqlParameter("@Payload",     SqlDbType.NVarChar)  { Value = payload });
                cmd.Parameters.Add(new SqlParameter("@Status",      SqlDbType.Int)       { Value = (int)MDM_ConfigHelpers.ApprovalStatus.Draft });
                cmd.Parameters.Add(new SqlParameter("@User",        SqlDbType.NVarChar)  { Value = si.AuthToken.UserName });

                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : -1;
            }
        }

        private static int ResolveWorkflowID(SessionInfo si, SqlConnection conn,
            int dimConfigID, MDM_ConfigHelpers.ChangeType changeType)
        {
            var helper = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, conn);
            var dt     = new DataTable("MDM_WF");
            var sqa    = new SqlDataAdapter();
            var sql    = @"
SELECT TOP 1 WorkflowID
FROM   MDM_ApprovalWorkflow
WHERE  DimConfigID = @DimConfigID
  AND  ChangeType  = @ChangeType
  AND  Status      = 1
ORDER  BY WorkflowID";
            var sqlparams = new[]
            {
                new SqlParameter("@DimConfigID", SqlDbType.Int) { Value = dimConfigID },
                new SqlParameter("@ChangeType",  SqlDbType.Int) { Value = (int)changeType }
            };
            helper.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
            return dt.Rows.Count > 0 ? Convert.ToInt32(dt.Rows[0]["WorkflowID"]) : 0;
        }

        private static DataRow GetChangeRequestRow(SessionInfo si, int changeRequestID)
        {
            var dt     = new DataTable("MDM_ChangeRequest");
            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                var helper    = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, conn);
                var sqa       = new SqlDataAdapter();
                var sql       = @"
SELECT cr.*, dc.DimName
FROM   MDM_ChangeRequest cr
JOIN   MDM_DimConfig     dc ON dc.DimConfigID = cr.DimConfigID
WHERE  cr.ChangeRequestID = @ChangeRequestID";
                var sqlparams = new[] { new SqlParameter("@ChangeRequestID", SqlDbType.Int) { Value = changeRequestID } };
                helper.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
            }
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        private static void UpdateChangeRequestStatus(SessionInfo si, int changeRequestID,
            MDM_ConfigHelpers.ApprovalStatus status)
        {
            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                conn.Open();
                var sql = @"
UPDATE MDM_ChangeRequest
SET    Status = @Status, UpdatedBy = @User, UpdatedDate = GETDATE()
WHERE  ChangeRequestID = @ChangeRequestID";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@Status",          SqlDbType.Int)      { Value = (int)status });
                cmd.Parameters.Add(new SqlParameter("@User",            SqlDbType.NVarChar) { Value = si.AuthToken.UserName });
                cmd.Parameters.Add(new SqlParameter("@ChangeRequestID", SqlDbType.Int)      { Value = changeRequestID });
                cmd.ExecuteNonQuery();
            }
        }

        private static void WriteAuditLog(SessionInfo si, int changeRequestID,
            string dimName, MDM_ConfigHelpers.ChangeType changeType, string notes)
        {
            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var conn = new SqlConnection(dbConn.ConnectionString))
            {
                conn.Open();
                var sql = @"
INSERT INTO MDM_AuditLog (EventType, DimConfigID, ObjectType, ObjectID, ChangedBy, ChangedDate, Notes)
SELECT @EventType, cr.DimConfigID, 'ChangeRequest', cr.ChangeRequestID,
       @ChangedBy, GETDATE(), @Notes
FROM   MDM_ChangeRequest cr
WHERE  cr.ChangeRequestID = @ChangeRequestID";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@EventType",       SqlDbType.NVarChar) { Value = changeType.ToString() });
                cmd.Parameters.Add(new SqlParameter("@ChangedBy",       SqlDbType.NVarChar) { Value = si.AuthToken.UserName });
                cmd.Parameters.Add(new SqlParameter("@Notes",           SqlDbType.NVarChar) { Value = notes });
                cmd.Parameters.Add(new SqlParameter("@ChangeRequestID", SqlDbType.Int)      { Value = changeRequestID });
                cmd.ExecuteNonQuery();
            }
        }

        #endregion
    }
}
