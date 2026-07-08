using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.CSharp;
using OneStream.Finance.Database;
using OneStream.Finance.Engine;
using OneStream.Shared.Common;
using OneStream.Shared.Database;
using OneStream.Shared.Engine;
using OneStream.Shared.Wcf;
using OneStream.Stage.Database;
using OneStream.Stage.Engine;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.MDM_Config_Migration
{
    public class MainClass
    {
        public object Main(SessionInfo si, BRGlobals globals, object api, DashboardExtenderArgs args)
        {
            try
            {
                switch (args.FunctionType)
                {
                    case DashboardExtenderFunctionType.LoadDashboard:
                        if (args.FunctionName.XFEqualsIgnoreCase("MDM_Migration_Load"))
                        {
                            if (args.LoadDashboardTaskInfo.Reason == LoadDashboardReasonType.Initialize
                                && args.LoadDashboardTaskInfo.Action == LoadDashboardActionType.BeforeFirstGetParameters)
                            {
                                var result = new XFLoadDashboardTaskResult
                                {
                                    ChangeCustomSubstVarsInDashboard = false,
                                    ModifiedCustomSubstVars          = null
                                };
                                return result;
                            }
                        }
                        break;

                    case DashboardExtenderFunctionType.ComponentSelectionChanged:
                        if (args.FunctionName.XFEqualsIgnoreCase("MDM_Migration_Export"))
                        {
                            return Export_MDM_Config(si, args);
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("MDM_Migration_Import"))
                        {
                            return Import_MDM_Config(si, args);
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

        #region "Export"
        /// <summary>
        /// Exports all MDM configuration tables to an XML string stored in the user document folder.
        /// Mirrors the FMM_ConfigMigration export pattern.
        /// </summary>
        private XFSelectionChangedTaskResult Export_MDM_Config(SessionInfo si, DashboardExtenderArgs args)
        {
            var result = new XFSelectionChangedTaskResult
            {
                IsOK           = true,
                ShowMessageBox = true,
                Message        = "MDM configuration export is not yet implemented.",
                ChangeSelectionChangedUIActionInDashboard = false
            };

            // TODO: Query MDM_DimConfig, MDM_IntegrationConfig, MDM_ApprovalWorkflow,
            //       MDM_ApprovalStep, MDM_ValidationRule, MDM_AccessConfig and serialize to XML.
            //       Write to BRApi.Utilities.GetUserDocumentFolder(si).

            return result;
        }
        #endregion

        #region "Import"
        /// <summary>
        /// Imports MDM configuration from an XML file in the user document folder.
        /// Mirrors the FMM_ConfigMigration import pattern.
        /// </summary>
        private XFSelectionChangedTaskResult Import_MDM_Config(SessionInfo si, DashboardExtenderArgs args)
        {
            var result = new XFSelectionChangedTaskResult
            {
                IsOK           = true,
                ShowMessageBox = true,
                Message        = "MDM configuration import is not yet implemented.",
                ChangeSelectionChangedUIActionInDashboard = false
            };

            // TODO: Read XML from user document folder, deserialize, and upsert into MDM config tables.

            return result;
        }
        #endregion
    }
}
