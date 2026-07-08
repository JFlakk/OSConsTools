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
using OneStreamWorkspacesApi;
using OneStreamWorkspacesApi.V800;
using OneStreamWorkspacesApi.V820;
using Workspace.OSConsTools.MDM_ConfigUI_Assembly;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    /// <summary>
    /// Resolves dynamic dashboard content and components for the MDM end-user workspace.
    /// Mirrors <c>DDM_Content</c> in the Dynamic Dashboard Manager.
    /// </summary>
    public class MDM_Content
    {
        private const string DefaultCubeViewName = "Default";

        /// <summary>
        /// Returns the <see cref="WsDynamicDashboardEx"/> for a content pane, selecting the
        /// correct embedded dashboard based on the current menu selection's layout config.
        /// </summary>
        internal static WsDynamicDashboardEx get_DynamicContent(
            SessionInfo si,
            IWsasDynamicDashboardsApiV800 api,
            DashboardWorkspace workspace,
            DashboardMaintUnit maintUnit,
            WsDynamicComponentEx parentDynamicComponentEx,
            Dashboard storedDashboard,
            Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            try
            {
                var configMenuRow = MDM_Support.get_ConfigMenuRow(si, customSubstVarsAlreadyResolved);
                var paneBinding   = MDM_Support.get_PaneBinding(si, configMenuRow, storedDashboard.Name);

                BRApi.ErrorLog.LogMessage(si,
                    $"MDM_Content.get_DynamicContent: pane={storedDashboard.Name} " +
                    $"contentType={paneBinding.ContentType} dash={paneBinding.DashboardName}");

                if (paneBinding.ContentType == MDM_ConfigHelpers.DBPaneContents.CubeView)
                {
                    // Hand off to the CV shell; caller sets CubeViewName from paneBinding.CubeViewName.
                    return api.GetEmbeddedDynamicDashboard(si, workspace, parentDynamicComponentEx,
                        storedDashboard, paneBinding.CubeViewName, null,
                        TriStateBool.TrueValue, WsDynamicItemStateType.EntireObject);
                }

                // Dashboard content — embed the resolved dashboard name.
                return api.GetEmbeddedDynamicDashboard(si, workspace, parentDynamicComponentEx,
                    storedDashboard, paneBinding.DashboardName, null,
                    TriStateBool.TrueValue, WsDynamicItemStateType.EntireObject);
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        /// <summary>
        /// Returns the <see cref="WsDynamicComponentCollection"/> for a content pane,
        /// driven by the resolved layout dashboard name.
        /// </summary>
        internal static WsDynamicComponentCollection get_DynamicComponentContent(
            SessionInfo si,
            IWsasDynamicDashboardsApiV800 api,
            DashboardWorkspace workspace,
            DashboardMaintUnit maintUnit,
            WsDynamicDashboardEx dynamicDashboardEx,
            Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            try
            {
                var configMenuRow = MDM_Support.get_ConfigMenuRow(si, customSubstVarsAlreadyResolved);
                var dbName        = configMenuRow != null
                    ? configMenuRow["DB_Name"]?.ToString() ?? string.Empty
                    : string.Empty;

                BRApi.ErrorLog.LogMessage(si,
                    $"MDM_Content.get_DynamicComponentContent: base={dynamicDashboardEx.DynamicDashboard.BasedOnName} dbName={dbName}");

                return api.GetDynamicComponentsForDynamicDashboard(si, workspace,
                    dynamicDashboardEx, dbName, null,
                    TriStateBool.TrueValue, WsDynamicItemStateType.MinimalWithTemplateParameters);
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
    }
}
