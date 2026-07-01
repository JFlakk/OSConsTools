using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
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
using Microsoft.Data.SqlClient;
using Workspace.OSConsTools.DDM_ConfigUI_Assembly;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    public class DDM_Content
    {

        //Params
        private const string DefaultCubeViewName = "Default";

        public object Main(SessionInfo si)
        {
            try
            {
                // Orchestrate dashboard logic here
                return null;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }


//        internal static XFSelectionChangedTaskResult OnMenuSelectionChanged(SessionInfo si, DashboardExtenderArgs args)
//        {

//            var taskResult = new XFSelectionChangedTaskResult() { ChangeCustomSubstVarsInDashboard = true };

//            var wfUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si);
//            var ProfileKey = wfUnitPk.ProfileKey;
//            int configProfileID = DDM_Support.get_CurrProfileID(si, ProfileKey);

//            int menuOptionID = DDM_Support.get_SelectedMenu(si, args.SelectionChangedTaskInfo.CustomSubstVars);


//            Dictionary<string, string> ParamsToAdd = DDM_Support.get_ParamsToAdd(DDM_Support.get_HeaderItems(si, args.SelectionChangedTaskInfo.CustomSubstVars));

//            // get cube name based on SI.
//            int cubeID = si.PovDataCellPk.CubeId;
//            var cubeName = DDM_Support.get_CubeName(si, cubeID);

//            // add cubename IV
//            taskResult.ModifiedCustomSubstVars.Add(DDM_Support.Param_CubeName, cubeName);


//            foreach (string param in ParamsToAdd.Keys)
//            {
//                taskResult.ModifiedCustomSubstVars.Add(param, ParamsToAdd[param]);
//            }
//            return taskResult;
//        }

		internal static WsDynamicComponentCollection get_DynamicComponentContent(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace,
		    DashboardMaintUnit maintUnit, WsDynamicDashboardEx dynamicDashboardEx, Dictionary<string, string> customSubstVarsAlreadyResolved)
		{
		    var configMenuRow = DDM_Support.get_ConfigMenuRow(si, customSubstVarsAlreadyResolved);
		    var paneBinding = DDM_Support.get_PaneBinding(si, configMenuRow, dynamicDashboardEx.DynamicDashboard.BasedOnName);

		    var dynComponents = api.GetDynamicComponentsForDynamicDashboard(si, workspace, dynamicDashboardEx, string.Empty, null, TriStateBool.Unknown, WsDynamicItemStateType.Unknown);

			BRApi.ErrorLog.LogMessage(si, $"DDM content: BasedOnName={dynamicDashboardEx.DynamicDashboard.BasedOnName}, ContentType={paneBinding.ContentType}, DB={paneBinding.DashboardName}, CV={paneBinding.CubeViewName}");

			// When DDM_App_Content_DB is the single-pane container and the content type is
			// CubeView, redirect to DDM_App_Content_CV.  Its own dynamic service call will
			// bind the cube view name onto cv_DDM_Dynamic_App_Content.
			if (paneBinding.ContentType == DDM_ConfigHelpers.DBPaneContents.CubeView
			    && dynamicDashboardEx.DynamicDashboard.BasedOnName.XFEqualsIgnoreCase("DDM_App_Content_DB"))
			{
				return embed_Dashboard(si, api, dynComponents, dynamicDashboardEx, maintUnit, "DDM_App_Content_CV");
			}

			// When DDM_App_Content_CV (or a multi-pane pane dashboard) is rendered and the
			// content type is CubeView, bind the cube view name directly.
			if (paneBinding.ContentType == DDM_ConfigHelpers.DBPaneContents.CubeView)
			{
				bool bound = try_BindCubeView(dynComponents, paneBinding.CubeViewName);
				if (!bound)
				{
					try_BindCubeViewByName(dynComponents, "cv_DDM_Dynamic_App_Content", paneBinding.CubeViewName);
				}
				return dynComponents;
			}

			// Dashboard content: embed the target dashboard inside the pane container.
			return embed_Dashboard(si, api, dynComponents, dynamicDashboardEx, maintUnit, paneBinding.DashboardName);
		}

		private static WsDynamicComponentCollection embed_Dashboard(SessionInfo si, IWsasDynamicDashboardsApiV800 api, WsDynamicComponentCollection dynComponents,
		    WsDynamicDashboardEx dynamicDashboardEx, DashboardMaintUnit maintUnit, string dashboardName)
		{
			BRApi.ErrorLog.LogMessage(si, $"DDM embed: {dynamicDashboardEx.DynamicDashboard.Name} -> {dashboardName}");

			var compMbr = dynComponents.GetComponentUsingBasedOnName(dynamicDashboardEx.DynamicDashboard.Name);

		    if (compMbr?.DynamicComponentEx?.DynamicComponent?.Component != null)
		    {
		        compMbr.DynamicComponentEx.DynamicComponent.Component.EmbeddedDashboardName = dashboardName;
		        return dynComponents;
		    }

		    // No component exists in workspace setup, so inject one dynamically.
		    var storedCompName = $"Embedded {dashboardName}";
		    var storedComp = EngineDashboardComponents.GetComponent(
		        api.DbConnAppOrFW,
		        maintUnit.WorkspaceID,
		        maintUnit.UniqueID,
		        storedCompName,
		        false,
		        true
		    );

		    if (storedComp == null)
		    {
		        BRApi.ErrorLog.LogMessage(si, $"DDM: Could not find stored component [{storedCompName}] in maint unit.");
		        return dynComponents;
		    }

		    var newCompEx = api.GetDynamicComponentForDynamicDashboard(
		        si,
		        dynamicDashboardEx.DynamicDashboard.Workspace,
		        dynamicDashboardEx,
		        storedComp,
		        string.Empty,
		        null,
		        TriStateBool.TrueValue,
		        WsDynamicItemStateType.EntireObject
		    );

		    if (newCompEx?.DynamicComponent?.Component == null)
		    {
		        BRApi.ErrorLog.LogMessage(si, "DDM: newCompEx returned null component; cannot inject.");
		        return dynComponents;
		    }

		    newCompEx.DynamicComponent.Component.EmbeddedDashboardName = dashboardName;
		    newCompEx.DynamicComponent.Component.Name = storedComp.Name;

			var newMember = new WsDynamicDbrdCompMemberEx();
			newMember.DynamicComponentEx = newCompEx;

			dynComponents.Components.Add(newMember);

		    BRApi.ErrorLog.LogMessage(si, $"DDM: Injected new embedded component [{storedComp.Name}] -> [{dashboardName}]");
		    return dynComponents;
		}
		
        // menu label
        internal static WsDynamicDashboardEx get_DynamicContent(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace, DashboardMaintUnit maintUnit,
            WsDynamicComponentEx parentDynamicComponentEx, Dashboard storedDashboard, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            var repeatArgsList = new List<WsDynamicComponentRepeatArgs>();

            var dynamicDashboardEx = api.GetEmbeddedDynamicDashboard(si, workspace, parentDynamicComponentEx, storedDashboard, string.Empty, null, TriStateBool.TrueValue, WsDynamicItemStateType.MinimalWithTemplateParameters);

            dynamicDashboardEx.DynamicDashboard.Tag = repeatArgsList;

            api.SaveDynamicDashboardState(si, parentDynamicComponentEx.DynamicComponent, dynamicDashboardEx, WsDynamicItemStateType.MinimalWithTemplateParameters);

            return dynamicDashboardEx;
        }

        private static bool try_BindEmbeddedDashboard(WsDynamicComponentCollection dynComponents, string dashboardName)
        {
            if (string.IsNullOrEmpty(dashboardName) || dynComponents == null || dynComponents.Components == null)
            {
                return false;
            }

            foreach (var componentMember in dynComponents.Components)
            {
                var component = componentMember.DynamicComponentEx.DynamicComponent.Component;
                if (component != null && !string.IsNullOrEmpty(component.EmbeddedDashboardName))
                {
                    component.EmbeddedDashboardName = dashboardName;
                    return true;
                }
            }

            return false;
        }

//        private static bool try_BindEmbeddedDashboardByName(WsDynamicComponentCollection dynComponents, string basedOnName, string dashboardName)
//        {
//            if (string.IsNullOrEmpty(dashboardName) || dynComponents == null)
//            {
//                return false;
//            }

//            var dynComponent = dynComponents.GetComponentUsingBasedOnName(basedOnName);
//            if (dynComponent != null && dynComponent.DynamicComponentEx?.DynamicComponent?.Component != null)
//            {
//                dynComponent.DynamicComponentEx.DynamicComponent.Component.EmbeddedDashboardName = dashboardName;
//                return true;
//            }

//            return false;
//        }

        private static bool try_BindCubeView(WsDynamicComponentCollection dynComponents, string cubeViewName)
        {
            if (string.IsNullOrEmpty(cubeViewName))
            {
                cubeViewName = DefaultCubeViewName;
            }

            if (dynComponents == null || dynComponents.Components == null)
            {
                return false;
            }

            foreach (var componentMember in dynComponents.Components)
            {
                var component = componentMember.DynamicComponentEx.DynamicComponent.Component;
                if (component == null || string.IsNullOrEmpty(component.XmlData))
                {
                    continue;
                }

                try
                {
                    var xmlData = XElement.Parse(component.XmlData);
                    if (xmlData.Element("CubeViewName") != null)
                    {
                        xmlData.SetElementValue("CubeViewName", cubeViewName);
                        component.XmlData = xmlData.ToString();
                        return true;
                    }
                }
                catch
                {
                    // Not cube view XML, continue scanning components.
                }
            }

            return false;
        }

        private static bool try_BindCubeViewByName(WsDynamicComponentCollection dynComponents, string basedOnName, string cubeViewName)
        {
            if (dynComponents == null)
            {
                return false;
            }

            var dynComponent = dynComponents.GetComponentUsingBasedOnName(basedOnName);
            if (dynComponent == null || dynComponent.DynamicComponentEx?.DynamicComponent?.Component == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(cubeViewName))
            {
                cubeViewName = DefaultCubeViewName;
            }

            var xmlData = XElement.Parse(dynComponent.DynamicComponentEx.DynamicComponent.Component.XmlData);
            xmlData.SetElementValue("CubeViewName", cubeViewName);
            dynComponent.DynamicComponentEx.DynamicComponent.Component.XmlData = xmlData.ToString();
            return true;
        }

    }
}
