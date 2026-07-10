using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using OneStream.Finance.Database;
using OneStream.Finance.Engine;
using OneStream.Shared.Common;
using OneStream.Shared.Database;
using OneStream.Shared.Engine;
using OneStream.Shared.Wcf;
using OneStream.Stage.Database;
using OneStream.Stage.Engine;
using Workspace.OSConsTools.MDM_ConfigUI_Assembly;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardStringFunction.MDM_UI
{
    public class MainClass
    {
        #region "Global Variables"
        private SessionInfo si;
        private BRGlobals globals;
        private object api;
        private DashboardStringFunctionArgs args;
        #endregion

        // Default layout dashboard when the menu row cannot be resolved.
        private const string DefaultDashboard = "MDM_App_Content_DB";

        public object Main(SessionInfo si, BRGlobals globals, object api, DashboardStringFunctionArgs args)
        {
            try
            {
                this.si      = si;
                this.globals = globals;
                this.api     = api;
                this.args    = args;

                if (args.FunctionName.XFEqualsIgnoreCase("Get_LayoutDB"))
                    return Get_LayoutDB();

                if (args.FunctionName.XFEqualsIgnoreCase("Get_Clean_Username"))
                    return StringHelper.RemoveSystemCharacters(si.AuthToken.UserName, true, false);

                if (args.FunctionName.XFEqualsIgnoreCase("Get_MDM_ApprovalStatus"))
                    return Get_MDM_ApprovalStatus();

                if (args.FunctionName.XFEqualsIgnoreCase("Get_MDM_ChangeTypeLabel"))
                    return Get_MDM_ChangeTypeLabel();

                return null;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #region "Layout Dashboard Resolver"
        /// <summary>
        /// Resolves the correct layout dashboard for the content area, keyed to the selected
        /// <c>BL_MDM_AppMenu</c> option. Mirrors <c>DDM_UI.Get_LayoutDB</c>.
        /// </summary>
        private string Get_LayoutDB()
        {
            // Allow an explicit LayoutType override for testing.
            var layoutTypeOverride = args.NameValuePairs.XFGetValue("LayoutType", string.Empty);
            if (!string.IsNullOrEmpty(layoutTypeOverride)
                && int.TryParse(layoutTypeOverride, out int overrideLayoutType))
            {
                var dbName = args.NameValuePairs.XFGetValue("DB_Name", string.Empty);
                var cvName = args.NameValuePairs.XFGetValue("CV_Name", string.Empty);
                BRApi.ErrorLog.LogMessage(si,
                    $"MDM_UI.Get_LayoutDB: LayoutType override={overrideLayoutType} DB_Name={dbName}");
                return Resolve_Layout_Dashboard(overrideLayoutType, dbName, cvName);
            }

            // DB-driven: look up the selected menu's layout config.
            var configMenuRow = MDM_Support.get_ConfigMenuRow(si, args.NameValuePairs);
            if (configMenuRow == null)
            {
                BRApi.ErrorLog.LogMessage(si, "MDM_UI.Get_LayoutDB: no config row found; using default.");
                return DefaultDashboard;
            }

            var paneBinding = MDM_Support.get_PaneBinding(si, configMenuRow,
                args.NameValuePairs.XFGetValue("currDB", DefaultDashboard));

            BRApi.ErrorLog.LogMessage(si,
                $"MDM_UI.Get_LayoutDB: resolved to '{paneBinding.DashboardName}'");
            return paneBinding.DashboardName;
        }

        private string Resolve_Layout_Dashboard(int layoutTypeInt, string dbName, string cvName)
        {
            return (MDM_ConfigHelpers.LayoutType)layoutTypeInt switch
            {
                MDM_ConfigHelpers.LayoutType.Dashboard or
                MDM_ConfigHelpers.LayoutType.Dashboard_CustomDB    => string.IsNullOrEmpty(dbName)
                                                                        ? DefaultDashboard
                                                                        : dbName,
                MDM_ConfigHelpers.LayoutType.CubeView              => "MDM_App_Content_CV",
                MDM_ConfigHelpers.LayoutType.None                  => DefaultDashboard,
                MDM_ConfigHelpers.LayoutType.Dashboard_TopBottom   => "MDM_App_Content_TB_DB",
                MDM_ConfigHelpers.LayoutType.Dashboard_LeftRight   => "MDM_App_Content_LR_DB",
                MDM_ConfigHelpers.LayoutType.Dashboard_2Top1Bottom => "MDM_App_Content_2T1B_DB",
                MDM_ConfigHelpers.LayoutType.Dashboard_1Top2Bottom => "MDM_App_Content_1T2B_DB",
                MDM_ConfigHelpers.LayoutType.Dashboard_2Left1Right => "MDM_App_Content_2L1R_DB",
                MDM_ConfigHelpers.LayoutType.Dashboard_1Left2Right => "MDM_App_Content_1L2R_DB",
                MDM_ConfigHelpers.LayoutType.Dashboard_2x2        => "MDM_App_Content_2x2_DB",
                _                                                  => DefaultDashboard
            };
        }
        #endregion

        #region "Display Helpers"
        /// <summary>Returns a human-readable label for the current change request's ApprovalStatus.</summary>
        private string Get_MDM_ApprovalStatus()
        {
            var statusStr = args.NameValuePairs.XFGetValue("IV_MDM_ApprStatus", "0");
            if (!int.TryParse(statusStr, out int statusInt)) return string.Empty;

            return (MDM_ConfigHelpers.ApprovalStatus)statusInt switch
            {
                MDM_ConfigHelpers.ApprovalStatus.Draft      => "Draft",
                MDM_ConfigHelpers.ApprovalStatus.Submitted  => "Submitted",
                MDM_ConfigHelpers.ApprovalStatus.InReview   => "In Review",
                MDM_ConfigHelpers.ApprovalStatus.Approved   => "Approved",
                MDM_ConfigHelpers.ApprovalStatus.Rejected   => "Rejected",
                MDM_ConfigHelpers.ApprovalStatus.Applied    => "Applied",
                MDM_ConfigHelpers.ApprovalStatus.Withdrawn  => "Withdrawn",
                _                                           => statusStr
            };
        }

        /// <summary>Returns a human-readable label for a ChangeType integer.</summary>
        private string Get_MDM_ChangeTypeLabel()
        {
            var typeStr = args.NameValuePairs.XFGetValue("IV_MDM_ChangeType", "0");
            if (!int.TryParse(typeStr, out int typeInt)) return string.Empty;

            return (MDM_ConfigHelpers.ChangeType)typeInt switch
            {
                MDM_ConfigHelpers.ChangeType.Add    => "Add Member",
                MDM_ConfigHelpers.ChangeType.Edit   => "Edit Member",
                MDM_ConfigHelpers.ChangeType.Move   => "Move / Reorder",
                MDM_ConfigHelpers.ChangeType.Retire => "Retire Member",
                _                                   => typeStr
            };
        }
        #endregion
    }
}
