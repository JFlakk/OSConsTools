// =====================================================================================
// REFERENCE ONLY - not compiled (lives under docs/, outside the Assemblies glob).
// DDM header-rebuild optimization: only rebuild the dynamic header when it actually changed.
//
// TWO LEVERS (use together):
//   Lever 1 (structural, do first) - REDRAW SCOPING. Split header vs content into separate
//     dynamic dashboards and redraw only what changed:
//       * member/filter pick  -> DashboardsToRedraw = "<content pane>"   (header NOT re-run)
//       * menu / layout change -> redraw the header (+ content)
//     Today DDM sets DashboardsToRedraw = "DDM Dynamic App Dashboard" (the whole top), so every
//     filter click rebuilds the header. Narrow that target and most rebuilds disappear with no
//     detection logic at all.
//
//   Lever 2 (belt-and-suspenders) - SIGNATURE SHORT-CIRCUIT. When the engine DOES call the build
//     service, give it a cheap early-out: if the header signature is unchanged, reuse saved state
//     instead of looping every row through addHeaderItems.
//
// CAVEAT: OneStream decides WHEN to call the dynamic service; you can short-circuit INSIDE it
//   (return saved/minimal state) but can't stop the call. Lever 1 controls the calls; Lever 2
//   makes the unavoidable ones cheap. Selections survive a skipped rebuild because they live in
//   ML_ subst vars, not in the header components.
// =====================================================================================
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using OneStream.Shared.Common;
using OneStream.Shared.Engine;

namespace Workspace.OSConsTools.DDM_UI_Assembly
{
    // Drop these helpers next to DDM_Support; call HeaderSignatureChanged(...) at the top of
    // DDM_Header.get_DynamicHdr / get_DynamicHdrComponents before doing any rebuild work.
    public static class DDM_HeaderRebuildGuard
    {
        // Subst var that carries the last-built signature across round-trips.
        private const string Param_HeaderSig = "IV_DDM_Hdr_Signature";

        /// <summary>
        /// True when the header must be rebuilt: the selected menu changed, or the header config
        /// rows for that menu changed (row count / max UpdateDate). False -> reuse the existing header.
        /// </summary>
        public static bool HeaderSignatureChanged(SessionInfo si, System.Collections.Generic.Dictionary<string, string> csv)
        {
            string menu = csv.XFGetValue(DDM_Support.Param_DashboardMenu, "1");   // BL_DDM_AppMenu
            string current = ComputeSignature(si, menu);
            string previous = csv.XFGetValue(Param_HeaderSig, string.Empty);

            // Also treat "no header built yet this session" as changed.
            return !string.Equals(current, previous, StringComparison.Ordinal);
        }

        /// <summary>
        /// Cheap signature = menuID | rowCount | maxUpdateDate over the header config for that menu.
        /// One tiny aggregate query - far cheaper than rebuilding N components every render.
        /// </summary>
        public static string ComputeSignature(SessionInfo si, string menuId)
        {
            var dt = new DataTable("hdr_sig");
            var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConn.ConnectionString))
            {
                var sqa = new SqlDataAdapter();
                var loader = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                // NOTE: swap DDM_DynDBHdrConfig / UpdateDate for your canonical header table + version col.
                var sql = @"SELECT COUNT(*) AS n, CONVERT(varchar(30), MAX(UpdateDate), 126) AS v
                            FROM DDM_DynDBHdrConfig
                            WHERE DynDBMenuID = @Menu";
                var p = new[] { new SqlParameter("@Menu", SqlDbType.Int) { Value = SafeInt(menuId) } };
                loader.Fill_Get_GBL_DT(si, sqa, dt, sql, p);
            }

            string n = "0", v = string.Empty;
            if (dt.Rows.Count > 0)
            {
                n = dt.Rows[0]["n"]?.ToString() ?? "0";
                v = dt.Rows[0]["v"] == DBNull.Value ? string.Empty : dt.Rows[0]["v"].ToString();
            }
            return $"{menuId}|{n}|{v}";
        }

        /// <summary>Persist the freshly-built signature so the next render can compare against it.</summary>
        public static void StampSignature(SessionInfo si, ref XFLoadDashboardTaskResult result,
                                          System.Collections.Generic.Dictionary<string, string> csv)
        {
            string menu = csv.XFGetValue(DDM_Support.Param_DashboardMenu, "1");
            string sig = ComputeSignature(si, menu);
            if (result.ModifiedCustomSubstVars.ContainsKey(Param_HeaderSig))
                result.ModifiedCustomSubstVars.XFSetValue(Param_HeaderSig, sig);
            else
                result.ModifiedCustomSubstVars.Add(Param_HeaderSig, sig);
        }

        private static int SafeInt(string s) => int.TryParse(s, out var i) ? i : 1;
    }
}

/* -------------------------------------------------------------------------------------
   WIRE-IN SKETCH (inside the dynamic build path, e.g. DDM_Header.get_DynamicHdrComponents):

     if (!DDM_HeaderRebuildGuard.HeaderSignatureChanged(si, customSubstVarsAlreadyResolved))
     {
         // Unchanged -> reuse the already-saved dynamic state instead of rebuilding every row.
         return api.GetDynamicComponentsForDynamicDashboard(
             si, workspace, dynamicDashboardEx, string.Empty, null,
             TriStateBool.TrueValue, WsDynamicItemStateType.MinimalWithTemplateParameters);
     }
     // Changed -> do the full addHeaderItems(...) build, then stamp the new signature in the
     // LoadDashboard extender's result so the next render can compare.

   REDRAW-SPLIT (the bigger win, in the SelectionChanged handler):
     - Filter / member pick:   comp.DashboardsToRedraw = "DDM_App_Content_<pane>";   // NOT the top
     - Menu / layout change:    comp.DashboardsToRedraw = "DDM Dynamic App Dashboard"; // rebuild header+content

   WHAT FORCES A REBUILD (captured by the signature):
     menu change (BL_DDM_AppMenu)                       -> YES
     header config edited in Admin (rowCount/UpdateDate)-> YES
     member selection (ML_*), show/hide, text entry     -> NO  (data within existing components)
--------------------------------------------------------------------------------------- */
