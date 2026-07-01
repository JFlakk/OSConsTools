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

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
	public class DDM_DynDBSvc : IWsasDynamicDashboardsV800
	{
        public WsDynamicDashboardEx GetEmbeddedDynamicDashboard(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace, DashboardMaintUnit maintUnit,
            WsDynamicComponentEx parentDynamicComponentEx, Dashboard storedDashboard, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            try
            {
                if (api != null)
                {
					var SA_DB = customSubstVarsAlreadyResolved.XFGetValue("StandAlone_DB","NA");
//					foreach (KeyValuePair<string, string> substvar in customSubstVarsAlreadyResolved)
//        			{
//						BRApi.ErrorLog.LogMessage(si,$"Hit {substvar.Key} - {substvar.Value}");
//					}
					
					BRApi.ErrorLog.LogMessage(si,$"Hit Dyn DB Svc - {SA_DB} - {parentDynamicComponentEx.AncestorNameSuffix} - {customSubstVarsAlreadyResolved.Count}");
		           BRApi.ErrorLog.LogMessage(si, $"DEBUG: Switching on Dashboard Name: [{storedDashboard.Name}]");
					return storedDashboard.Name switch
		            {
						// configurable header items
		                //"DDM_App_Header" 
						//"DDM_App_Hdr_C2"  => DDM_Header.get_DynamicHdr(si, api, workspace, maintUnit, parentDynamicComponentEx, storedDashboard, customSubstVarsAlreadyResolved),	
		                // Dynamic Content
		                "DDM_App_Content_DB" => DDM_Content.get_DynamicContent(si, api, workspace, maintUnit, parentDynamicComponentEx, storedDashboard, customSubstVarsAlreadyResolved),
		                "DDM_App_Content_CV" => DDM_Content.get_DynamicContent(si, api, workspace, maintUnit, parentDynamicComponentEx, storedDashboard, customSubstVarsAlreadyResolved),
                        "DDM_App_Content_B_DB" => DDM_Content.get_DynamicContent(si, api, workspace, maintUnit, parentDynamicComponentEx, storedDashboard, customSubstVarsAlreadyResolved),
                        "DDM_App_Content_T_DB" => DDM_Content.get_DynamicContent(si, api, workspace, maintUnit, parentDynamicComponentEx, storedDashboard, customSubstVarsAlreadyResolved),
                        "DDM_App_Content_R_DB" => DDM_Content.get_DynamicContent(si, api, workspace, maintUnit, parentDynamicComponentEx, storedDashboard, customSubstVarsAlreadyResolved),
                        "DDM_App_Content_L_DB" => DDM_Content.get_DynamicContent(si, api, workspace, maintUnit, parentDynamicComponentEx, storedDashboard, customSubstVarsAlreadyResolved),
                        "DDM_App_Content_TL_DB" => DDM_Content.get_DynamicContent(si, api, workspace, maintUnit, parentDynamicComponentEx, storedDashboard, customSubstVarsAlreadyResolved),
                        "DDM_App_Content_TR_DB" => DDM_Content.get_DynamicContent(si, api, workspace, maintUnit, parentDynamicComponentEx, storedDashboard, customSubstVarsAlreadyResolved),
                        "DDM_App_Content_BL_DB" => DDM_Content.get_DynamicContent(si, api, workspace, maintUnit, parentDynamicComponentEx, storedDashboard, customSubstVarsAlreadyResolved),
                        "DDM_App_Content_BR_DB" => DDM_Content.get_DynamicContent(si, api, workspace, maintUnit, parentDynamicComponentEx, storedDashboard, customSubstVarsAlreadyResolved),

		                _ => api.GetEmbeddedDynamicDashboard(si, workspace, parentDynamicComponentEx, storedDashboard, string.Empty, null, TriStateBool.TrueValue, WsDynamicItemStateType.EntireObject)
		            };
				}

                return null;
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        public WsDynamicComponentCollection GetDynamicComponentsForDynamicDashboard(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace,
            DashboardMaintUnit maintUnit, WsDynamicDashboardEx dynamicDashboardEx, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            try
            {
				BRApi.ErrorLog.LogMessage(si,$"Hit Get Dyn Components {dynamicDashboardEx.DynamicDashboard.Name.ToString()}");
                if (api != null)
                {
		            return dynamicDashboardEx.DynamicDashboard.BasedOnName switch
		            {
		               //"0b_DDM_App_Header_ConfigItems" => DDM_Header.get_DynamicHdrComponents(si, api, workspace, maintUnit, dynamicDashboardEx, customSubstVarsAlreadyResolved),
		               // "0b2_DDM_App_Header_Config_Btn" 
						"DDM_App_Hdr_C2C1" => DDM_Header.get_DynamicHdrComponents(si, api, workspace, maintUnit, dynamicDashboardEx, customSubstVarsAlreadyResolved),		                
						// Dynamic Content
		                "DDM_App_Content_DB" => DDM_Content.get_DynamicComponentContent(si, api, workspace, maintUnit, dynamicDashboardEx, customSubstVarsAlreadyResolved),
		                "DDM_App_Content_CV" => DDM_Content.get_DynamicComponentContent(si, api, workspace, maintUnit, dynamicDashboardEx, customSubstVarsAlreadyResolved),
		                "DDM_App_Content_B_DB" => DDM_Content.get_DynamicComponentContent(si, api, workspace, maintUnit, dynamicDashboardEx, customSubstVarsAlreadyResolved),
		                "DDM_App_Content_T_DB" => DDM_Content.get_DynamicComponentContent(si, api, workspace, maintUnit, dynamicDashboardEx, customSubstVarsAlreadyResolved),
		                "DDM_App_Content_R_DB" => DDM_Content.get_DynamicComponentContent(si, api, workspace, maintUnit, dynamicDashboardEx, customSubstVarsAlreadyResolved),
		                "DDM_App_Content_L_DB" => DDM_Content.get_DynamicComponentContent(si, api, workspace, maintUnit, dynamicDashboardEx, customSubstVarsAlreadyResolved),
		                "DDM_App_Content_TL_DB" => DDM_Content.get_DynamicComponentContent(si, api, workspace, maintUnit, dynamicDashboardEx, customSubstVarsAlreadyResolved),
		                "DDM_App_Content_TR_DB" => DDM_Content.get_DynamicComponentContent(si, api, workspace, maintUnit, dynamicDashboardEx, customSubstVarsAlreadyResolved),
		                "DDM_App_Content_BL_DB" => DDM_Content.get_DynamicComponentContent(si, api, workspace, maintUnit, dynamicDashboardEx, customSubstVarsAlreadyResolved),
		                "DDM_App_Content_BR_DB" => DDM_Content.get_DynamicComponentContent(si, api, workspace, maintUnit, dynamicDashboardEx, customSubstVarsAlreadyResolved),
						
		                _ => api.GetDynamicComponentsForDynamicDashboard(si, workspace, dynamicDashboardEx, string.Empty, null, TriStateBool.TrueValue, WsDynamicItemStateType.MinimalWithTemplateParameters)
		            };
				}

                return null;
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        public WsDynamicAdapterCollection GetDynamicAdaptersForDynamicComponent(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace,
            DashboardMaintUnit maintUnit, WsDynamicComponentEx dynamicComponentEx, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            try
            {
                if (api != null)
                {
                    return api.GetDynamicAdaptersForDynamicComponent(si, workspace, dynamicComponentEx, string.Empty, null, TriStateBool.Unknown, WsDynamicItemStateType.Unknown);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        public WsDynamicCubeViewEx GetDynamicCubeViewForDynamicAdapter(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace,
            DashboardMaintUnit maintUnit, WsDynamicAdapterEx dynamicAdapterEx, CubeViewItem storedCubeViewItem, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            try
            {
                if (api != null)
                {
                    return api.GetDynamicCubeViewForDynamicAdapter(si, workspace, dynamicAdapterEx, storedCubeViewItem, string.Empty, null, TriStateBool.Unknown, WsDynamicItemStateType.Unknown);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        public WsDynamicParameterCollection GetDynamicParametersForDynamicComponent(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace,
            DashboardMaintUnit maintUnit, WsDynamicComponentEx dynamicComponentEx, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            try
            {
                if (api != null)
                {
                    return api.GetDynamicParametersForDynamicComponent(si, workspace, dynamicComponentEx,string.Empty,dynamicComponentEx.TemplateSubstVars, TriStateBool.TrueValue, WsDynamicItemStateType.MinimalWithTemplateParameters);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }
	}
}
