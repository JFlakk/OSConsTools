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

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.DDM_ConfigLoadDB
{
    public class MainClass
    {
		#region "Global Params"
			private SessionInfo si;
	        private BRGlobals globals;
	        private object api;
	        private DashboardExtenderArgs args;
			private readonly GBL_Helpers gblHelpers = new GBL_Helpers();
		#endregion
		
		#region "Dictionary Setup"
		private string MainMenuParam = "DL_DDM_SetupOptions";
		private string menuLayoutParam = "DL_FMM_Cube_Config_Options";
        private Dictionary<string, string> paramMap = new Dictionary<string, string>()
        {
            {"BL_DDM_WFPtrv", "IV_DDM_WFPtrv"},
            {"BL_DDM_ConfigID", "IV_DDM_ConfigID"},
            {"BL_FMM_Table_Cube_ID", "IV_FMM_Cube_ID"},
            {"BL_FMM_Act_ID", "IV_FMM_Act_ID"},
            {"BL_FMM_Table_Act_ID", "IV_FMM_Act_ID"},
            {"BL_FMM_Model_ID", "IV_FMM_Model_ID"},
            {"BL_FMM_Model_Grp_Seq_ID", "IV_FMM_Model_Grp_Seq_ID"},
            {"BL_FMM_Model_Grp_ID","IV_FMM_Model_Grp_ID"}
        };
        // key string is dialog name, string array is list of IVs associated to textboxes that should be set to empty strings
        private Dictionary<string, string[]> clearTextBoxDict = new Dictionary<string, string[]>() {
            {"0b1b2b2_DDM_Config_Content_New", new string[] {"IV_FMM_Model_Name"}},
            {"1_FMM_Cube_Config_Dialog_Add", new string[] {"IV_FMM_Cube_Descr"}},
            {"1_FMM_Model_Grp_Dialog_Add", new string[] {"IV_FMM_Model_Grp_Name"}},
            {"1_FMM_Model_Grp_Seq_Dialog_Add", new string[] {"IV_FMM_Model_Grp_Seq_Name"}},

        };
		private Dictionary<string, Dictionary<int, string[]>> HierarchyDict = new Dictionary<string, Dictionary<int, string[]>>();
		
        private Dictionary<int, string[]> ConfigWFPDB = new Dictionary<int, string[]>()
        {
			{0, new string[] {"IV_DDM_MenuLayoutConfig_AddUpdate"}},
            {1, new string[] {"DL_DDM_Type"}},
            {2, new string[] {"BL_DDM_WFPRoot"}},
            {3, new string[] {"BL_DDM_WFPScenType"}},
			{4, new string[] {"IV_DDM_WFPtrv"}},
			{5, new string[] {"IV_DDM_ConfigID"}},
			{6, new string[] {"BL_DDM_MenuLayoutConfig"}},
			{7, new string[] {"DL_DDM_LayoutConfigType"}}
        };
        private Dictionary<int, string[]> ConfigOPDB = new Dictionary<int, string[]>()
        {
            {0, new string[] {"DL_DDM_Type"}},
            {1, new string[] {"BL_DDM_Root_WFProfiles"}},
            {2, new string[] {"BL_DDM_Scen_Type"}},
			{3, new string[] {"IV_DDM_trv_WF_Profile"}},
			{4, new string[] {"IV_DDM_Config_ID"}},
			{5, new string[] {"IV_DDM_Config_ID"}},
			{6, new string[] {"IV_DDM_Config_ID"}}
        };
		#endregion
        public object Main(SessionInfo si, BRGlobals globals, object api, DashboardExtenderArgs args)
        {
			HierarchyDict.Add("DDM_ConfigWFP",ConfigWFPDB);
			HierarchyDict.Add("DDM_ConfigOPDB",ConfigOPDB);
            try
            {
				this.si = si;
                this.globals = globals;
                this.api = api;
                this.args = args;
                switch (args.FunctionType)
                {
                    case DashboardExtenderFunctionType.LoadDashboard:
                        if (args.FunctionName.XFEqualsIgnoreCase("LoadDDM_AdminDB"))
                        {
                            var loadDbTaskResult = LoadAdminDB(string.Empty,ref args);
							foreach (var substVar in loadDbTaskResult.ModifiedCustomSubstVars)
							{
								//BRApi.ErrorLog.LogMessage(si, $"Available Var: {substVar.Key} = {substVar.Value}");
							}
                            return loadDbTaskResult;
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


        #region "Class Helper Functions"	
        #region "Load Dashboard"
        private XFLoadDashboardTaskResult LoadAdminDB(string RunType, ref DashboardExtenderArgs args)
        {
            var loadDbTaskResult = new XFLoadDashboardTaskResult
            {
                ChangeCustomSubstVarsInDashboard = true
            };

            clearParams(ref args, ref loadDbTaskResult);
            setParams(ref args, ref loadDbTaskResult);
            updateShowHide(ref args, ref loadDbTaskResult);

            return loadDbTaskResult;
        }
		#endregion
		
        private void updateShowHide(ref DashboardExtenderArgs args, ref XFLoadDashboardTaskResult taskResult)
        {
            string showHideIVName = "IV_DDM_ShowHide_MenuBtn";
            string showBtnVisibleName = "IV_DDM_DispShow_MenuBtn";
            string hideBtnVisibleName = "IV_DDM_DispHide_MenuBtn";
            string menuWidthIV = "IV_DDM_MenuWidth";


            var ARCustomSubst = args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved;

            string showHideIVVal = ARCustomSubst.XFGetValue(showHideIVName, "");

            if (showHideIVVal == "Hide")
            {
                UpdateCustomSubstVar(ref taskResult, showBtnVisibleName, "True");
                UpdateCustomSubstVar(ref taskResult, hideBtnVisibleName, "False");
                UpdateCustomSubstVar(ref taskResult, menuWidthIV, "0");
            }
            else if (showHideIVVal == "Show")
            {
                UpdateCustomSubstVar(ref taskResult, showBtnVisibleName, "False");
                UpdateCustomSubstVar(ref taskResult, hideBtnVisibleName, "True");
                UpdateCustomSubstVar(ref taskResult, menuWidthIV, "Auto");
            }
        }
		
        private void clearParams(ref DashboardExtenderArgs args, ref XFLoadDashboardTaskResult taskResult)
        {

            // get dialog name
            string DialogSelection = args.PrimaryDashboard.Name;

            // if dialog dashboard name is contained within the clearTextBoxDict, set all underlying textbox params to empty strings
            if (clearTextBoxDict.ContainsKey(DialogSelection))
            {
                foreach (string param in clearTextBoxDict[DialogSelection])
                {
                    UpdateCustomSubstVar(ref taskResult, param, "");
                }
            }
        }
		
        private void setParams(ref DashboardExtenderArgs args, ref XFLoadDashboardTaskResult taskResult)
        {
            string dialogSelection = args.PrimaryDashboard.Name;

            string mainMenuSelection = !string.IsNullOrEmpty(args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue(MainMenuParam))
                ? args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue(MainMenuParam)
                : args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue(MainMenuParam);

            string cubeSubMenuSelection = !string.IsNullOrEmpty(args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue(menuLayoutParam))
                ? args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue(menuLayoutParam)
                : args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue(menuLayoutParam);

            string selectedDashboard = mainMenuSelection;
            selectedDashboard = HierarchyDict.ContainsKey(dialogSelection) ? dialogSelection : selectedDashboard;

            var arCustomSubst = args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved;
            var prCustomSubst = args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun;

            if (HierarchyDict.ContainsKey(selectedDashboard))
            {
                Dictionary<int, string[]> dependencyDict = HierarchyDict[selectedDashboard];
                bool priorDependencyChanged = false;

                foreach (int dependencyDepth in dependencyDict.Keys)
                {
                    foreach (string param in dependencyDict[dependencyDepth])
                    {
                        bool arContainsKey = arCustomSubst.ContainsKey(param);
                        bool prContainsKey = prCustomSubst.ContainsKey(param);
                        string arVal = arCustomSubst.XFGetValue(param);
                        string prVal = prCustomSubst.XFGetValue(param);
                        string mappedParam = paramMap.ContainsKey(param) ? paramMap[param] : string.Empty;

                        if (!priorDependencyChanged)
                        {
                            if (mappedParam != string.Empty)
                            {
                                string arMappedVal = arCustomSubst.XFGetValue(mappedParam, string.Empty);
                                string prMappedVal = prCustomSubst.XFGetValue(mappedParam, string.Empty);

                                if (prContainsKey && isValidParamValue(prVal) && isValidParamValue(prMappedVal))
                                {
                                    if (prVal != prMappedVal)
                                    {
                                        priorDependencyChanged = true;
                                    }

                                    gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, param, prVal);
                                    gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, mappedParam, prVal);
                                }
                                else if (arContainsKey && isValidParamValue(arVal) && isValidParamValue(arMappedVal))
                                {
                                    if (arVal != arMappedVal)
                                    {
                                        priorDependencyChanged = true;
                                    }

                                    gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, param, arVal);
                                    gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, mappedParam, arVal);
                                }
                                else if (arContainsKey && isValidParamValue(arVal))
                                {
                                    gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, param, arVal);
                                    gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, mappedParam, arVal);
                                }
                                else if (prContainsKey && isValidParamValue(prVal))
                                {
                                    gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, param, prVal);
                                    gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, mappedParam, prVal);
                                }
                                else
                                {
                                    string paramDefault = getDefaultParam(param, taskResult.ModifiedCustomSubstVars);
                                    gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, param, paramDefault);
                                    gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, mappedParam, paramDefault);
                                }
                            }
                            else
                            {
                                if (arContainsKey && isValidParamValue(arVal))
                                {
                                    gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, param, arVal);
                                }
                                else if (prContainsKey && isValidParamValue(prVal))
                                {
                                    gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, param, prVal);
                                }
                                else
                                {
                                    gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, param, getDefaultParam(param, taskResult.ModifiedCustomSubstVars));
                                }
                            }
                        }
                        else
                        {
                            string paramDefault = getDefaultParam(param, taskResult.ModifiedCustomSubstVars);
                            gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, param, paramDefault);

                            if (mappedParam != string.Empty)
                            {
                                gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, mappedParam, paramDefault);
                            }
                        }
					//ExecuteSpecificRefreshLogic(selectedDashboard,mappedParam, ref taskResult);	
                    }
                }
            }
        }
		
        private bool isValidParamValue(string value)
        {
            if (value == null || value == string.Empty)
            {
                return false;
            }
            try
            {
                return Int32.Parse(value) > 0;
            }
            catch
            {
                // if we can't convert the param to an integer, that likely means it's a string value
                // there is currently no logic to tell whether a string param is a valid value, so return true by default
                return true;
            }
		}
        #region "Setup Helpers"

        private String get_Default_Root_Profile(SessionInfo si, BRGlobals globals, object api, DashboardExtenderArgs db_args)
        {
            try
            {
                var curr_Root_Profile = db_args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue("BL_DDM_Root_WF_Profiles", "NA");

                if (curr_Root_Profile == "NA")
                {
                    var root_Profile_DT = new DataTable("root_profile");

                    var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                        // Create a new DataTable

                        var sqa = new SqlDataAdapter();
                        // Define the select query and sqlparams
                        var sql = @"SELECT ProfileName 
        							FROM WorkflowProfileHierarchy
        							WHERE HierarchyLevel = 1
									AND IsTemplate = 0 ";

                        // Create an array of SqlParameter objects
                        var sqlparams = new SqlParameter[]
                        {
                        };

                        sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, root_Profile_DT, sql, sqlparams);
                    }
                    // Check if the DataTable has any rows
                    if (root_Profile_DT.Rows.Count > 0)
                    {
                        // Retrieve the value from the desired column
                        return root_Profile_DT.Rows[0]["ProfileName"].ToString();
                    }
                    else
                    {
                        return "NA";
                    }
                }
                else
                {
                    return curr_Root_Profile;
                }
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }

        }

        private String get_Default_WF_Profile(SessionInfo si, BRGlobals globals, object api, DashboardExtenderArgs db_args, Boolean new_root_Profile, string root_ProfileName)
        {
            try
            {
                if (new_root_Profile == false)
                {
                    return db_args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue("IV_DDM_trv_WF_Profile", "NA");
                }
                else
                {
                    var wf_Profile_DT = new DataTable("wf_profile");

                    var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                        // Create a new DataTable

                        var sqa = new SqlDataAdapter();
                        // Define the select query and sqlparams
                        var sql = @"SELECT prof.ProfileKey,prof.ProfileName
									FROM WorkflowProfileHierarchy prof
									WHERE prof.HierarchyLevel = 1
								    AND prof.IsTemplate = 0
									AND prof.ProfileName = @root_ProfileName";

                        // Create an array of SqlParameter objects
                        var sqlparams = new SqlParameter[]
                        {
                        new SqlParameter("@root_ProfileName", SqlDbType.NVarChar,100) { Value = root_ProfileName }
                        };

                        sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, wf_Profile_DT, sql, sqlparams);
                    }
                    // Check if the DataTable has any rows
                    if (wf_Profile_DT.Rows.Count > 0)
                    {
                        // Retrieve the value from the desired column
                        return wf_Profile_DT.Rows[0]["ProfileKey"].ToString();
                    }
                    else
                    {
                        return "NA";
                    }
                }

            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }


        }
		
        private void UpdateCustomSubstVar(ref XFLoadDashboardTaskResult result, string key, string value)
        {
            if (result.ModifiedCustomSubstVars.ContainsKey(key))
            {
                result.ModifiedCustomSubstVars.XFSetValue(key, value);
                globals.SetStringValue(key, value);
            }
            else
            {
                result.ModifiedCustomSubstVars.Add(key, value);
                globals.SetStringValue(key, value);
            }
        }
        private string getDefaultParam(string param, Dictionary<string, string> customsubstvars)
        {

            // replace IV with associated BL
            // IV should only be passed in if it's a value coming from a SQL Table Editor
            if (param.Contains("IV_"))
            {
                param = param.Replace("IV_", "BL_");
            }


            DashboardParamDisplayInfo paramInfo = BRApi.Dashboards.Parameters.GetParameterDisplayInfo(si, false, customsubstvars, args.PrimaryDashboard.WorkspaceID, param);
            //			BRApi.ErrorLog.LogMessage(si, "testing stuff for " + param + ": " + paramInfo?.ComboBoxItemsForBoundList?.Count);
            //BRApi.ErrorLog.LogMessage(si, "testing stuff 2: " + param + " " + paramInfo?.ListBoxItemsForBoundList?.Count);
            if (paramInfo?.ComboBoxItemsForBoundList?.Count > 0)
            {
				//var value = paramInfo.ComboBoxItemsForBoundList.FirstOrDefault(kvp => kvp.Value == "Test").Key;
                //				BRApi.ErrorLog.LogMessage(si,"Hit " + paramInfo.ComboBoxItemsForBoundList.First().Value.ToString());
                return paramInfo.ComboBoxItemsForBoundList.First().Value.ToString();
            }

            return "0";

        }
        #endregion
        #endregion
    }
}