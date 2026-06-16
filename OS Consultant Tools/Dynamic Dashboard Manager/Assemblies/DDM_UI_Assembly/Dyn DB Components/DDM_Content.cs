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

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    public class DDM_Content
    {

        //Params
        private const string Param_CubeViewName = "IV_DDM_App_CV_Name";

        private const string Param_DashboardName = "IV_1a_DDM_App_Content_DB";

        private const string CV_DashboardName = "1a_DDM_App_Content_CV";

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
            var wfUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si);
            var ProfileKey = wfUnitPk.ProfileKey;
            int curr_Profile_ID = DDM_Support.get_CurrProfileID(si, ProfileKey);
			

            int menuOptionID = DDM_Support.get_SelectedMenu(si, customSubstVarsAlreadyResolved);
 BRApi.ErrorLog.LogMessage(si, $"Attempting to load Dashboard Rows menuOptionID : {menuOptionID}");
            var config_Menu_DT = DDM_Support.get_ConfigMenu(si, menuOptionID);
           var dashboardName = "emb_Dynamic_DDM_App_Content_DB" ;
            var cubeViewName = "Default";

            if (config_Menu_DT.Rows.Count > 0)
            {
                var optType = config_Menu_DT.Rows[0]["LayoutType"].ToString();
// BRApi.ErrorLog.LogMessage(si, $"Attempting Opttype : {optType}");
                if (optType == "1")
                {
					 BRApi.ErrorLog.LogMessage(si, $"Attempting to load Dashboard Rows >0 : {dashboardName}");
                    dashboardName = config_Menu_DT.Rows[0]["DB_Name"].ToString();
                }
                else if (optType == "2")
                {
                    dashboardName = CV_DashboardName;
                    cubeViewName = config_Menu_DT.Rows[0]["CV_Name"].ToString();
                }
            }
			var dynComponents = new WsDynamicComponentCollection();
			dynComponents = api.GetDynamicComponentsForDynamicDashboard(si, workspace, dynamicDashboardEx, string.Empty, null, TriStateBool.Unknown, WsDynamicItemStateType.Unknown);
			// BRApi.ErrorLog.LogMessage(si, $"Hit Here in content{dynamicDashboardEx.DynamicDashboard.Name} - {dynComponents.Components.Count}");
			var dynDBComponent = new WsDynamicDbrdCompMemberEx();

			if (dynamicDashboardEx.DynamicDashboard.Name == "DDM_App_Content_DB")
			{
				 BRApi.ErrorLog.LogMessage(si, $"Attempting to load Dashboard: {dashboardName}");
				dynDBComponent = dynComponents.GetComponentUsingBasedOnName("emb_Dynamic_DDM_App_Content_DB");
				dynDBComponent.DynamicComponentEx.DynamicComponent.Component.EmbeddedDashboardName = dashboardName;
			}
			else
			{
				dynDBComponent = dynComponents.GetComponentUsingBasedOnName("cv_DDM_Dynamic");
				var CV_XmlData = new XElement("cv_DDM_Dynamic");
				CV_XmlData = XElement.Parse(dynDBComponent.DynamicComponentEx.DynamicComponent.Component.XmlData);
				CV_XmlData.SetElementValue("CubeViewName",cubeViewName);
				dynDBComponent.DynamicComponentEx.DynamicComponent.Component.XmlData = CV_XmlData.ToString();
			}	

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

    }
}