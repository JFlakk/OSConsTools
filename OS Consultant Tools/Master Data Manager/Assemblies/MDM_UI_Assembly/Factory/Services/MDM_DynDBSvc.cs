using System;
using System.Collections.Generic;
using OneStream.Shared.Common;
using OneStream.Shared.Database;
using OneStream.Shared.Engine;
using OneStream.Shared.Wcf;
using OneStreamWorkspacesApi;
using OneStreamWorkspacesApi.V800;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    public class MDM_DynDBSvc : IWsasDynamicDashboardsV800
    {
        public WsDynamicDashboardEx GetEmbeddedDynamicDashboard(
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
                if (api == null) return null;

                BRApi.ErrorLog.LogMessage(si,
                    $"MDM_DynDBSvc.GetEmbeddedDynamicDashboard: [{storedDashboard.Name}]");

                return storedDashboard.Name switch
                {
                    // Header pane
                    "MDM_App_Hdr_C2C1"   => MDM_Header.get_DynamicHdr(si, api, workspace, maintUnit,
                                                parentDynamicComponentEx, storedDashboard, customSubstVarsAlreadyResolved),

                    // Content panes — all single-pane variants
                    "MDM_App_Content_DB"    => MDM_Content.get_DynamicContent(si, api, workspace, maintUnit,
                                                parentDynamicComponentEx, storedDashboard, customSubstVarsAlreadyResolved),
                    "MDM_App_Content_B_DB"  => MDM_Content.get_DynamicContent(si, api, workspace, maintUnit,
                                                parentDynamicComponentEx, storedDashboard, customSubstVarsAlreadyResolved),
                    "MDM_App_Content_T_DB"  => MDM_Content.get_DynamicContent(si, api, workspace, maintUnit,
                                                parentDynamicComponentEx, storedDashboard, customSubstVarsAlreadyResolved),
                    "MDM_App_Content_L_DB"  => MDM_Content.get_DynamicContent(si, api, workspace, maintUnit,
                                                parentDynamicComponentEx, storedDashboard, customSubstVarsAlreadyResolved),
                    "MDM_App_Content_R_DB"  => MDM_Content.get_DynamicContent(si, api, workspace, maintUnit,
                                                parentDynamicComponentEx, storedDashboard, customSubstVarsAlreadyResolved),
                    "MDM_App_Content_TL_DB" => MDM_Content.get_DynamicContent(si, api, workspace, maintUnit,
                                                parentDynamicComponentEx, storedDashboard, customSubstVarsAlreadyResolved),
                    "MDM_App_Content_TR_DB" => MDM_Content.get_DynamicContent(si, api, workspace, maintUnit,
                                                parentDynamicComponentEx, storedDashboard, customSubstVarsAlreadyResolved),
                    "MDM_App_Content_BL_DB" => MDM_Content.get_DynamicContent(si, api, workspace, maintUnit,
                                                parentDynamicComponentEx, storedDashboard, customSubstVarsAlreadyResolved),
                    "MDM_App_Content_BR_DB" => MDM_Content.get_DynamicContent(si, api, workspace, maintUnit,
                                                parentDynamicComponentEx, storedDashboard, customSubstVarsAlreadyResolved),

                    _ => api.GetEmbeddedDynamicDashboard(si, workspace, parentDynamicComponentEx,
                             storedDashboard, string.Empty, null,
                             TriStateBool.TrueValue, WsDynamicItemStateType.EntireObject)
                };
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        public WsDynamicComponentCollection GetDynamicComponentsForDynamicDashboard(
            SessionInfo si,
            IWsasDynamicDashboardsApiV800 api,
            DashboardWorkspace workspace,
            DashboardMaintUnit maintUnit,
            WsDynamicDashboardEx dynamicDashboardEx,
            Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            try
            {
                if (api == null) return null;

                BRApi.ErrorLog.LogMessage(si,
                    $"MDM_DynDBSvc.GetDynamicComponentsForDynamicDashboard: [{dynamicDashboardEx.DynamicDashboard.BasedOnName}]");

                return dynamicDashboardEx.DynamicDashboard.BasedOnName switch
                {
                    // Header
                    "MDM_App_Hdr_C2C1"   => MDM_Header.get_DynamicHdrComponents(si, api, workspace, maintUnit,
                                                dynamicDashboardEx, customSubstVarsAlreadyResolved),

                    // Content panes
                    "MDM_App_Content_DB"    => MDM_Content.get_DynamicComponentContent(si, api, workspace, maintUnit,
                                                dynamicDashboardEx, customSubstVarsAlreadyResolved),
                    "MDM_App_Content_B_DB"  => MDM_Content.get_DynamicComponentContent(si, api, workspace, maintUnit,
                                                dynamicDashboardEx, customSubstVarsAlreadyResolved),
                    "MDM_App_Content_T_DB"  => MDM_Content.get_DynamicComponentContent(si, api, workspace, maintUnit,
                                                dynamicDashboardEx, customSubstVarsAlreadyResolved),
                    "MDM_App_Content_L_DB"  => MDM_Content.get_DynamicComponentContent(si, api, workspace, maintUnit,
                                                dynamicDashboardEx, customSubstVarsAlreadyResolved),
                    "MDM_App_Content_R_DB"  => MDM_Content.get_DynamicComponentContent(si, api, workspace, maintUnit,
                                                dynamicDashboardEx, customSubstVarsAlreadyResolved),
                    "MDM_App_Content_TL_DB" => MDM_Content.get_DynamicComponentContent(si, api, workspace, maintUnit,
                                                dynamicDashboardEx, customSubstVarsAlreadyResolved),
                    "MDM_App_Content_TR_DB" => MDM_Content.get_DynamicComponentContent(si, api, workspace, maintUnit,
                                                dynamicDashboardEx, customSubstVarsAlreadyResolved),
                    "MDM_App_Content_BL_DB" => MDM_Content.get_DynamicComponentContent(si, api, workspace, maintUnit,
                                                dynamicDashboardEx, customSubstVarsAlreadyResolved),
                    "MDM_App_Content_BR_DB" => MDM_Content.get_DynamicComponentContent(si, api, workspace, maintUnit,
                                                dynamicDashboardEx, customSubstVarsAlreadyResolved),

                    _ => api.GetDynamicComponentsForDynamicDashboard(si, workspace,
                             dynamicDashboardEx, string.Empty, null,
                             TriStateBool.TrueValue, WsDynamicItemStateType.MinimalWithTemplateParameters)
                };
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        public WsDynamicAdapterCollection GetDynamicAdaptersForDynamicComponent(
            SessionInfo si,
            IWsasDynamicDashboardsApiV800 api,
            DashboardWorkspace workspace,
            DashboardMaintUnit maintUnit,
            WsDynamicComponentEx dynamicComponentEx,
            Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            try
            {
                if (api == null) return null;
                return api.GetDynamicAdaptersForDynamicComponent(si, workspace, dynamicComponentEx,
                    string.Empty, null, TriStateBool.Unknown, WsDynamicItemStateType.Unknown);
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        public WsDynamicCubeViewEx GetDynamicCubeViewForDynamicAdapter(
            SessionInfo si,
            IWsasDynamicDashboardsApiV800 api,
            DashboardWorkspace workspace,
            DashboardMaintUnit maintUnit,
            WsDynamicAdapterEx dynamicAdapterEx,
            CubeViewItem storedCubeViewItem,
            Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            try
            {
                if (api == null) return null;
                return api.GetDynamicCubeViewForDynamicAdapter(si, workspace, dynamicAdapterEx,
                    storedCubeViewItem, string.Empty, null,
                    TriStateBool.Unknown, WsDynamicItemStateType.Unknown);
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        public WsDynamicParameterCollection GetDynamicParametersForDynamicComponent(
            SessionInfo si,
            IWsasDynamicDashboardsApiV800 api,
            DashboardWorkspace workspace,
            DashboardMaintUnit maintUnit,
            WsDynamicComponentEx dynamicComponentEx,
            Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            try
            {
                if (api == null) return null;
                return api.GetDynamicParametersForDynamicComponent(si, workspace, dynamicComponentEx,
                    string.Empty, dynamicComponentEx.TemplateSubstVars,
                    TriStateBool.TrueValue, WsDynamicItemStateType.MinimalWithTemplateParameters);
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }
    }
}
