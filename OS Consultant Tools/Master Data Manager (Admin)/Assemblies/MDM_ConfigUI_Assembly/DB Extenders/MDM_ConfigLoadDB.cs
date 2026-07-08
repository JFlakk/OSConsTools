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

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.MDM_ConfigLoadDB
{
    public class MainClass
    {
        /// <summary>
        /// The setup-options dropdown component that drives the admin content area.
        /// Mirrors <c>DL_FMM_SetupOptions</c> / <c>sp_DL_DDM_SetupOptions</c> in FMM and DDM.
        /// </summary>
        private string MainMenuParam = "DL_MDM_SetupOptions";

        // Maps a parent-list component to the IV that should receive its selected ID.
        private Dictionary<string, string> paramMap = new Dictionary<string, string>()
        {
            { "BL_MDM_DimConfigID",   "IV_MDM_DimConfigID"   },
            { "BL_MDM_IntConfigID",   "IV_MDM_IntConfigID"   },
            { "BL_MDM_ApprWFID",      "IV_MDM_ApprWFID"      },
            { "BL_MDM_ApprStepID",    "IV_MDM_ApprStepID"    },
            { "BL_MDM_ValRuleID",     "IV_MDM_ValRuleID"      },
            { "BL_MDM_AccessID",      "IV_MDM_AccessID"      }
        };

        // Routes setup-options values to the content dashboard they should display.
        private Dictionary<int, string[]> SetupOptionsRouting = new Dictionary<int, string[]>()
        {
            // Feature 1 — Dimension Maintenance
            { 1, new string[] { "MDM_AdminContent_DimConfig"    } },
            // Feature 2 — Integrations
            { 2, new string[] { "MDM_AdminContent_IntConfig"    } },
            // Feature 3 — Approvals
            { 3, new string[] { "MDM_AdminContent_ApprWF"       } },
            // Feature 4 — Validations
            { 4, new string[] { "MDM_AdminContent_ValRule"      } },
            // Feature 5 — Admin Maintenance
            { 5, new string[] { "MDM_AdminContent_Admin"        } },
            // Feature 6 — Reports
            { 6, new string[] { "MDM_AdminContent_Reports"      } }
        };

        #region "Global Variables"
        private SessionInfo si;
        private BRGlobals globals;
        private object api;
        private DashboardExtenderArgs args;
        private readonly GBL_Helpers gblHelpers = new GBL_Helpers();
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
                        if (args.FunctionName.XFEqualsIgnoreCase("MDM_ConfigLoadDB"))
                        {
                            return LoadDB(ref args);
                        }
                        break;

                    case DashboardExtenderFunctionType.ComponentSelectionChanged:
                        if (args.SelectionChangedTaskInfo?.ComponentName?.XFEqualsIgnoreCase(MainMenuParam) == true)
                        {
                            return OnSetupOptionsChanged(ref args);
                        }
                        // Propagate parent-list selection to its corresponding IV.
                        foreach (var kv in paramMap)
                        {
                            if (args.SelectionChangedTaskInfo?.ComponentName?.XFEqualsIgnoreCase(kv.Key) == true)
                            {
                                return OnParentListChanged(kv.Value, ref args);
                            }
                        }
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
            var result = new XFLoadDashboardTaskResult
            {
                ChangeCustomSubstVarsInDashboard = true,
                ModifiedCustomSubstVars          = new Dictionary<string, string>()
            };

            // Default to Feature 1 (Dim Maintenance) on first load.
            var currentSelection = args.NameValuePairs.XFGetValue(MainMenuParam, "1");
            if (!int.TryParse(currentSelection, out int featureIndex))
            {
                featureIndex = 1;
            }

            SetContentDashboard(featureIndex, ref result);

            return result;
        }
        #endregion

        #region "Selection Changed"
        private XFSelectionChangedTaskResult OnSetupOptionsChanged(ref DashboardExtenderArgs args)
        {
            var result = new XFSelectionChangedTaskResult
            {
                IsOK                                      = true,
                ShowMessageBox                            = false,
                ChangeSelectionChangedUIActionInDashboard = true,
                ModifiedCustomSubstVars                   = new Dictionary<string, string>()
            };

            var selStr = args.SelectionChangedTaskInfo?.SelectedValue?.ToString() ?? "1";
            if (!int.TryParse(selStr, out int featureIndex))
            {
                featureIndex = 1;
            }

            var loadResult = new XFLoadDashboardTaskResult
            {
                ChangeCustomSubstVarsInDashboard = true,
                ModifiedCustomSubstVars          = new Dictionary<string, string>()
            };
            SetContentDashboard(featureIndex, ref loadResult);

            foreach (var kv in loadResult.ModifiedCustomSubstVars)
            {
                result.ModifiedCustomSubstVars[kv.Key] = kv.Value;
            }

            return result;
        }

        private XFSelectionChangedTaskResult OnParentListChanged(string targetIV, ref DashboardExtenderArgs args)
        {
            var result = new XFSelectionChangedTaskResult
            {
                IsOK                                      = true,
                ShowMessageBox                            = false,
                ChangeSelectionChangedUIActionInDashboard = true,
                ModifiedCustomSubstVars                   = new Dictionary<string, string>()
            };

            var selectedID = args.SelectionChangedTaskInfo?.SelectedValue?.ToString() ?? "0";
            gblHelpers.UpdateCustomSubstVar(ref result, targetIV, selectedID);

            return result;
        }
        #endregion

        #region "Helpers"
        private void SetContentDashboard(int featureIndex, ref XFLoadDashboardTaskResult result)
        {
            if (SetupOptionsRouting.TryGetValue(featureIndex, out var routeInfo) && routeInfo.Length > 0)
            {
                gblHelpers.UpdateCustomSubstVar(ref result, globals,
                    "IV_MDM_AdminContent_DB", routeInfo[0]);
            }
            else
            {
                gblHelpers.UpdateCustomSubstVar(ref result, globals,
                    "IV_MDM_AdminContent_DB", "MDM_AdminContent_DimConfig");
            }

            gblHelpers.UpdateCustomSubstVar(ref result, globals,
                "IV_MDM_SetupOptions_Sel", featureIndex.ToString());
        }
        #endregion
    }
}
