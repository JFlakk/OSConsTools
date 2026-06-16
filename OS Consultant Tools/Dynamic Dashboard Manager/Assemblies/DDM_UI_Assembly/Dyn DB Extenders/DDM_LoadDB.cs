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
using OneStreamWorkspacesApi.V800;


namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.DDM_LoadDB
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
                        if (args.FunctionName.XFEqualsIgnoreCase("DDM_LoadDB"))
                        {
							//BRApi.ErrorLog.LogMessage(si,$"Hit ComponentSelectionChanged {args.LoadDashboardTaskInfo.Reason.ToString()} - {args.LoadDashboardTaskInfo.Action.ToString()}");
                            if (args.LoadDashboardTaskInfo.Reason == LoadDashboardReasonType.ComponentSelectionChanged && args.LoadDashboardTaskInfo.Action == LoadDashboardActionType.BeforeGetDashboardDisplayInfo)
                            {
                           }
                            // Implement Load Dashboard logic here.
                            if (args.LoadDashboardTaskInfo.Reason == LoadDashboardReasonType.Initialize && args.LoadDashboardTaskInfo.Action == LoadDashboardActionType.BeforeFirstGetParameters)
                            {
								//BRApi.ErrorLog.LogMessage(si,$"Hit Initial Load");
                                var load_Dashboard_Task_Result = Load_Dashboard(si, globals, api, args, "Initial");
                                return load_Dashboard_Task_Result;
                            }
                            else if (args.LoadDashboardTaskInfo.Reason == LoadDashboardReasonType.ComponentSelectionChanged && args.LoadDashboardTaskInfo.Action != LoadDashboardActionType.BeforeGetDashboardDisplayInfo)
                            {
								//BRApi.ErrorLog.LogMessage(si,"Hit ComponentSelectionChanged Last");
                                var load_Dashboard_Task_Result = Load_Dashboard(si, globals, api, args, "Post-Initial-Set-SubstVars");
                                return load_Dashboard_Task_Result;
                            }
                            else if (args.LoadDashboardTaskInfo.Reason == LoadDashboardReasonType.ComponentSelectionChanged && args.LoadDashboardTaskInfo.Action == LoadDashboardActionType.BeforeGetDashboardDisplayInfo)
                            {
								//BRApi.ErrorLog.LogMessage(si,"Hit ComponentSelectionChanged");
                                var load_Dashboard_Task_Result = Load_Dashboard(si, globals, api, args, "Post-Initial");
                                return load_Dashboard_Task_Result;
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
        private XFLoadDashboardTaskResult Load_Dashboard(SessionInfo si, BRGlobals globals, object api, DashboardExtenderArgs db_args, string RunType)
        {
            var wfUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si);
            var Load_Dashboard_Task_Result = new XFLoadDashboardTaskResult();
            if (RunType.XFEqualsIgnoreCase("Initial"))
            {
			//	BRApi.ErrorLog.LogMessage(si, $"Hit HERE Initial {db_args.PrimaryDashboard.Name} - { db_args.ComponentInfo.WorkspaceName} - {wfUnitPk.ToString()}");
                Load_Dashboard_Task_Result.ChangeCustomSubstVarsInDashboard = true;
				Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("IV_DDM_App_Show_Hide_Menu_Btn","Show");
				Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("IV_DDM_App_Display_Show_Menu_Btn","False");
				Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("IV_DDM_App_Display_Hide_Menu_Btn","True");
				Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("IV_DDM_App_Menu_Width","Auto");
                Load_Dashboard_Task_Result = Get_Default_Menu_Options(si, globals, api, db_args, Load_Dashboard_Task_Result, wfUnitPk.ProfileKey, "Initial");
                //Load_Dashboard_Task_Result = Get_Default_Menu_Option(si,globals,api,db_args,Load_Dashboard_Task_Result);
                //Get Default Cube Settings
                //Get Default Cube
                //Get Default Scenario Type
                //Get Default Ent Mbr Filter
                //Get Default Agg vs Consol

            }
			else if (RunType.XFEqualsIgnoreCase("Post-Initial-Set-SubstVars"))
            {
							foreach (var kvp in db_args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues)
			{
			    BRApi.ErrorLog.LogMessage(si,$"hit Resolved {kvp.Key} - {kvp.Value}");
			}
			
							foreach (var kvp in db_args.SelectionChangedTaskInfo.CustomSubstVars)
			{
			    BRApi.ErrorLog.LogMessage(si,$"hit Prior {kvp.Key} - {kvp.Value}");
			}
                Load_Dashboard_Task_Result.ChangeCustomSubstVarsInDashboard = true;
				//Checks current value of Show Menu param against prior run
				var show_hide_val = db_args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue("IV_DDM_App_Show_Hide_Menu_Btn");
//				var new_display_show_val = db_args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue("IV_DDM_App_Show_Hide_Menu_Btn");
//				var prior_run_show_val = db_args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("IV_DDM_App_Display_Show_Menu_Btn");

				if (!string.IsNullOrEmpty(show_hide_val))
				{
				    if (show_hide_val == "Hide")
				    {
				        UpdateCustomSubstVar(Load_Dashboard_Task_Result, "IV_DDM_App_Display_Show_Menu_Btn", "True");
				        UpdateCustomSubstVar(Load_Dashboard_Task_Result, "IV_DDM_App_Display_Hide_Menu_Btn", "False");
				        UpdateCustomSubstVar(Load_Dashboard_Task_Result, "IV_DDM_App_Menu_Width", "0");
				    }
				    else if (show_hide_val == "Show")
				    {
				        UpdateCustomSubstVar(Load_Dashboard_Task_Result, "IV_DDM_App_Display_Show_Menu_Btn", "False");
				        UpdateCustomSubstVar(Load_Dashboard_Task_Result, "IV_DDM_App_Display_Hide_Menu_Btn", "True");
				        UpdateCustomSubstVar(Load_Dashboard_Task_Result, "IV_DDM_App_Menu_Width", "Auto");
				    }
				}
                Load_Dashboard_Task_Result = Get_Default_Menu_Options(si, globals, api, db_args, Load_Dashboard_Task_Result, wfUnitPk.ProfileKey, "Post-Initial");
            }
            else if (RunType.XFEqualsIgnoreCase("Post-Initial"))
            {
                Load_Dashboard_Task_Result.ChangeCustomSubstVarsInDashboard = true;
				//Checks current value of Show Menu param against prior run
				var show_hide_val = db_args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue("IV_DDM_App_Show_Hide_Menu_Btn");
//				var new_display_show_val = db_args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue("IV_DDM_App_Show_Hide_Menu_Btn");
//				var prior_run_show_val = db_args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("IV_DDM_App_Display_Show_Menu_Btn");

				if (!string.IsNullOrEmpty(show_hide_val))
				{
				    if (show_hide_val == "Hide")
				    {
				        UpdateCustomSubstVar(Load_Dashboard_Task_Result, "IV_DDM_App_Display_Show_Menu_Btn", "True");
				        UpdateCustomSubstVar(Load_Dashboard_Task_Result, "IV_DDM_App_Display_Hide_Menu_Btn", "False");
				        UpdateCustomSubstVar(Load_Dashboard_Task_Result, "IV_DDM_App_Menu_Width", "0");
				    }
				    else if (show_hide_val == "Show")
				    {
				        UpdateCustomSubstVar(Load_Dashboard_Task_Result, "IV_DDM_App_Display_Show_Menu_Btn", "False");
				        UpdateCustomSubstVar(Load_Dashboard_Task_Result, "IV_DDM_App_Display_Hide_Menu_Btn", "True");
				        UpdateCustomSubstVar(Load_Dashboard_Task_Result, "IV_DDM_App_Menu_Width", "Auto");
				    }
				}
                Load_Dashboard_Task_Result = Get_Default_Menu_Options(si, globals, api, db_args, Load_Dashboard_Task_Result, wfUnitPk.ProfileKey, "Post-Initial");
            }

            return Load_Dashboard_Task_Result;

        }
		#endregion
		
        #region "Setup Helpers"
        private XFLoadDashboardTaskResult Get_Default_Menu_Options(SessionInfo si, BRGlobals globals, object api, DashboardExtenderArgs args, XFLoadDashboardTaskResult Load_Dashboard_Task_Result, Guid ProfileKey, String RunType)
        {
            var XF_Load_Dashboard_Task_Result = new XFLoadDashboardTaskResult();
            XF_Load_Dashboard_Task_Result = Load_Dashboard_Task_Result;

            var dt = new DataTable("WFP_Config");

            var menu_option = args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("BL_DDM_App_Menu", String.Empty);

            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                // Create a new DataTable

                var sqa = new SqlDataAdapter();
                // Define the select query and parameters
                var sql = @"Select Menu.DynDBConfigID,Menu.DynDBMenuID,Menu.Name,
							Menu.LayoutType, Menu.CustomDBHdr_Name,
							Menu.CustomDBContent_Name,
							Menu.DB_Name,Menu.CV_Name
							FROM DDM_DynDBConfig Cnfg
							JOIN DDM_DynDBMenuLayoutConfig Menu
							ON Cnfg.DynDBConfigID = Menu.DynDBConfigID
							WHERE Cnfg.WFPKey = @OS_WFProfileKey ";
                if (menu_option != String.Empty)
                {
                    sql += @"AND Menu.DynDBMenuID = @DDM_Menu_ID ";
                }

                sql += @"ORDER BY SortOrder";
                // Initialize the list of parameters
                var sqlparams = new SqlParameter[]
                {
                    new SqlParameter("@OS_WFProfileKey", SqlDbType.UniqueIdentifier) { Value = ProfileKey }
                };

                // Add the second parameter conditionally
                if (!string.IsNullOrEmpty(menu_option))
                {
                    sqlparams = sqlparams.Append(new SqlParameter("@DDM_Menu_ID", SqlDbType.Int) { Value = Convert.ToInt32(menu_option) }).ToArray();
                }

                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
            }
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];

                 UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, "BL_DDM_App_Menu", row["DynDBMenuID"].ToString());
  
            }
			

            return XF_Load_Dashboard_Task_Result;
        }


        private void UpdateCustomSubstVar(XFLoadDashboardTaskResult result, string key, string value)
        {
            if (result.ModifiedCustomSubstVars.ContainsKey(key))
            {
                result.ModifiedCustomSubstVars.XFSetValue(key, value);
            }
            else
            {
                result.ModifiedCustomSubstVars.Add(key, value);
            }
        }

        #endregion

    }
}