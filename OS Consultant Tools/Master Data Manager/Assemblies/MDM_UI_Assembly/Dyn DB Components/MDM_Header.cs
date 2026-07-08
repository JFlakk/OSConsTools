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
using OneStreamWorkspacesApi;
using OneStreamWorkspacesApi.V800;
using OneStreamWorkspacesApi.V820;
using Workspace.OSConsTools.MDM_ConfigUI_Assembly;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    /// <summary>
    /// Resolves the dynamic header bar for the MDM end-user workspace.
    /// Mirrors <c>DDM_Header</c> in the Dynamic Dashboard Manager.
    /// The header exposes dimension selector and context filters that are propagated
    /// as subst vars to the active content pane.
    /// </summary>
    public class MDM_Header
    {
        /// <summary>
        /// Returns the <see cref="WsDynamicDashboardEx"/> for the header pane.
        /// </summary>
        internal static WsDynamicDashboardEx get_DynamicHdr(
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
                BRApi.ErrorLog.LogMessage(si,
                    $"MDM_Header.get_DynamicHdr: storedDB={storedDashboard.Name}");

                return api.GetEmbeddedDynamicDashboard(si, workspace, parentDynamicComponentEx,
                    storedDashboard, string.Empty, null,
                    TriStateBool.TrueValue, WsDynamicItemStateType.EntireObject);
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        /// <summary>
        /// Returns the <see cref="WsDynamicComponentCollection"/> for the header pane.
        /// The header renders dimension-selector dropdowns and context filter components
        /// whose values are propagated to the content area via subst vars.
        /// </summary>
        internal static WsDynamicComponentCollection get_DynamicHdrComponents(
            SessionInfo si,
            IWsasDynamicDashboardsApiV800 api,
            DashboardWorkspace workspace,
            DashboardMaintUnit maintUnit,
            WsDynamicDashboardEx dynamicDashboardEx,
            Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            try
            {
                BRApi.ErrorLog.LogMessage(si,
                    $"MDM_Header.get_DynamicHdrComponents: base={dynamicDashboardEx.DynamicDashboard.BasedOnName}");

                return api.GetDynamicComponentsForDynamicDashboard(si, workspace,
                    dynamicDashboardEx, string.Empty, null,
                    TriStateBool.TrueValue, WsDynamicItemStateType.MinimalWithTemplateParameters);
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
    }
}
