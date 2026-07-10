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

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardStringFunction.MDM_ConfigUI
{
    public class MainClass
    {
        #region "Global Variables"
        private SessionInfo si;
        private BRGlobals globals;
        private object api;
        private DashboardStringFunctionArgs args;
        #endregion

        public object Main(SessionInfo si, BRGlobals globals, object api, DashboardStringFunctionArgs args)
        {
            try
            {
                this.si      = si;
                this.globals = globals;
                this.api     = api;
                this.args    = args;

                if (args.FunctionName.XFEqualsIgnoreCase("Get_Clean_Username"))
                {
                    return StringHelper.RemoveSystemCharacters(si.AuthToken.UserName, true, false);
                }
                else if (args.FunctionName.XFEqualsIgnoreCase("Get_MDM_ColFormat"))
                {
                    var curr_TED = args.NameValuePairs.XFGetValue("curr_TED");
                    var curr_DB  = args.NameValuePairs.XFGetValue("curr_DB");
                    var col      = args.NameValuePairs.XFGetValue("col");
                    return Get_MDM_ColFormat(curr_TED, curr_DB, col);
                }
                else if (args.FunctionName.XFEqualsIgnoreCase("Get_MenuDB"))
                {
                    return Get_MenuDB();
                }
                else if (args.FunctionName.XFEqualsIgnoreCase("Get_AdminContentDB"))
                {
                    return Get_AdminContentDB();
                }
                else if (args.FunctionName.XFEqualsIgnoreCase("Get_MDM_Config_IsVisible"))
                {
                    return Get_MDM_Config_IsVisible();
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #region "Dashboard Routing"
        /// <summary>
        /// Returns the admin navigation dashboard for the current setup-options selection.
        /// Mirrors DDM's <c>Get_MenuDB</c>.
        /// </summary>
        private string Get_MenuDB()
        {
            var selStr = args.NameValuePairs.XFGetValue("DL_MDM_SetupOptions", "1");
            if (!int.TryParse(selStr, out int featureIndex))
            {
                featureIndex = 1;
            }

            return featureIndex switch
            {
                1 => "MDM_AdminContent_DimConfig",
                2 => "MDM_AdminContent_IntConfig",
                3 => "MDM_AdminContent_ApprWF",
                4 => "MDM_AdminContent_ValRule",
                5 => "MDM_AdminContent_Admin",
                6 => "MDM_AdminContent_Reports",
                _ => "MDM_AdminContent_DimConfig"
            };
        }

        /// <summary>
        /// Returns the admin content dashboard name stored in the current subst var.
        /// Used when the content pane needs to read the IV directly.
        /// </summary>
        private string Get_AdminContentDB()
        {
            return args.NameValuePairs.XFGetValue("IV_MDM_AdminContent_DB", "MDM_AdminContent_DimConfig");
        }
        #endregion

        #region "Column Format"
        /// <summary>
        /// Returns a column-format string for SQL table editor columns in MDM admin dashboards.
        /// Mirrors DDM's <c>Get_DDM_ColFormat</c>.
        /// </summary>
        private string Get_MDM_ColFormat(string curr_TED, string curr_DB, string col)
        {
            // Default: editable text
            var format = "Text";

            if (string.IsNullOrEmpty(curr_TED) || string.IsNullOrEmpty(col))
            {
                return format;
            }

            // Status columns → dropdown
            if (col.XFEqualsIgnoreCase("Status"))
            {
                return "DropDown_MDM_Statuses";
            }

            // Feature-area specific overrides
            if (curr_TED.XFEqualsIgnoreCase("MDM_DimConfig"))
            {
                if (col.XFEqualsIgnoreCase("FeatureType"))
                    return "DropDown_MDM_FeatureTypes";
            }
            else if (curr_TED.XFEqualsIgnoreCase("MDM_IntegrationConfig"))
            {
                if (col.XFEqualsIgnoreCase("Direction"))
                    return "DropDown_MDM_IntegrationDirections";
                if (col.XFEqualsIgnoreCase("SourceType"))
                    return "DropDown_MDM_SourceTypes";
            }
            else if (curr_TED.XFEqualsIgnoreCase("MDM_ApprovalWorkflow"))
            {
                if (col.XFEqualsIgnoreCase("ChangeType"))
                    return "DropDown_MDM_ChangeTypes";
            }
            else if (curr_TED.XFEqualsIgnoreCase("MDM_ValidationRule"))
            {
                if (col.XFEqualsIgnoreCase("RuleType"))
                    return "DropDown_MDM_RuleTypes";
                if (col.XFEqualsIgnoreCase("Severity"))
                    return "DropDown_MDM_Severities";
            }

            return format;
        }
        #endregion

        #region "Visibility"
        /// <summary>
        /// Returns "1" (visible) or "0" (hidden) for admin-panel components based on the current
        /// setup-options selection. Mirrors DDM's <c>Get_DDM_Config_IsVisible</c>.
        /// </summary>
        private string Get_MDM_Config_IsVisible()
        {
            var componentName = args.NameValuePairs.XFGetValue("ComponentName", string.Empty);
            var selStr        = args.NameValuePairs.XFGetValue("DL_MDM_SetupOptions", "1");

            if (!int.TryParse(selStr, out int featureIndex))
            {
                featureIndex = 1;
            }

            // Map component prefixes to the feature index they belong to.
            var visibilityMap = new Dictionary<string, int>
            {
                { "MDM_DimConfig",  1 },
                { "MDM_IntConfig",  2 },
                { "MDM_ApprWF",     3 },
                { "MDM_ApprStep",   3 },
                { "MDM_ValRule",    4 },
                { "MDM_Access",     5 },
                { "MDM_Admin",      5 },
                { "MDM_Report",     6 }
            };

            foreach (var kv in visibilityMap)
            {
                if (componentName.StartsWith(kv.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return featureIndex == kv.Value ? "1" : "0";
                }
            }

            return "1";
        }
        #endregion
    }
}
