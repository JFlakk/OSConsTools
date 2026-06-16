using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
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

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.FMM_ConfigLoadDB
{
    public class MainClass
    {
        private string MainMenuParam = "DL_FMM_SetupOptions";

        private Dictionary<string, string> paramMap = new Dictionary<string, string>()
        {
            {"BL_FMM_ModelGrpSeqConfigID", "IV_FMM_ModelGrpSeqID"},
            {"BL_FMM_ModelGrpConfigID", "IV_FMM_ModelGrpID"},
            {"BL_FMM_UnitConfigID","IV_FMM_UnitConfigID"}
        };

        private Dictionary<string, Dictionary<int, string[]>> HierarchyDict = new Dictionary<string, Dictionary<int, string[]>>();

        private Dictionary<int, string[]> CubeConfig = new Dictionary<int, string[]>()
        {
            {0, new string[] {"IV_FMM_CubeConfig_AddUpdate"}},
            {1, new string[] {"BL_FMM_CubeConfigID"}}
			//Added - Devlin
//            {2, new string[] {"IV_FMM_CalcConfig_MbrListCalc"}},
//            {3, new string[] {"IV_FMM_CalcConfig_MbrList1Calc"}},
//            {4, new string[] {"IV_FMM_CalcConfig_MbrList2Calc"}},
//            {5, new string[] {"IV_FMM_CalcConfig_MbrList3Calc"}},
//            {6, new string[] {"IV_FMM_CalcConfig_MbrList4Calc"}}
        };

        private Dictionary<int, string[]> UnitAcctConfig = new Dictionary<int, string[]>()
        {
            {0, new string[] {"BL_FMM_CubeConfigID_Table"}},
            {1, new string[] {"BL_FMM_ActConfigID_Table"}},
            {2, new string[] {"IV_FMM_UnitID"}},
            {3, new string[] {"IV_FMM_AcctConfig_AddUpdate"}}
        };

        private Dictionary<int, string[]> CustTableConfig = new Dictionary<int, string[]>()
        {
            {0, new string[] {"IV_FMM_CustTableConfig_AddUpdate"}},
            {1, new string[] {"BL_FMM_CustTableConfigID"}}
        };

        private Dictionary<int, string[]> CustTableAssign = new Dictionary<int, string[]>()
        {
            {0, new string[] {"IV_FMM_CustTableConfig_AddUpdate"}},
            {1, new string[] {"BL_FMM_CustTableID"}}
        };


        private Dictionary<int, string[]> ApprConfig = new Dictionary<int, string[]>()
        {
            {0, new string[] {"BL_FMM_CubeConfigID"}},
            {1, new string[] {"IV_FMM_ApprConfigID"}}
        };

        private Dictionary<int, string[]> UIConfig = new Dictionary<int, string[]>()
        {
            {0, new string[] {"BL_FMM_CubeConfigID_Table"}},
            {1, new string[] {"BL_FMM_ActConfigID_Table"}}
        };

        private Dictionary<int, string[]> DataValConfig = new Dictionary<int, string[]>()
        {
            {0, new string[] {"BL_FMM_CubeConfigID_Table"}},
            {1, new string[] {"BL_FMM_ActConfigID_Table"}}
        };

        private Dictionary<int, string[]> ModelConfig = new Dictionary<int, string[]>()
        {
			{0, new string[] {"IV_FMM_ModelConfig_AddUpdate"}},
            {1, new string[] {"BL_FMM_CubeConfigID"}},
            {2, new string[] {"BL_FMM_ActConfigID"}},
            {3, new string[] {"BL_FMM_ModelConfigID"}},
            {4, new string[] {"BL_FMM_CalcConfigID"}},
			//Added - Devlin
            {5, new string[] {"IV_FMM_CalcConfig_MbrListCalc"}},
            {6, new string[] {"IV_FMM_CalcConfig_MbrList1Calc"}},
            {7, new string[] {"IV_FMM_CalcConfig_MbrList2Calc"}},
            {8, new string[] {"IV_FMM_CalcConfig_MbrList3Calc"}},
            {9, new string[] {"IV_FMM_CalcConfig_MbrList4Calc"}}
        };

        private Dictionary<int, string[]> BuildModelGroup = new Dictionary<int, string[]>()
        {
            {0, new string[] {"BL_FMM_CubeConfigID"}},
            {1, new string[] {"BL_FMM_ModelGrpConfigID"}}
        };

        private Dictionary<int, string[]> BuildModelGroupSeq = new Dictionary<int, string[]>()
        {
            {0, new string[] {"BL_FMM_CubeConfigID"}},
            {1, new string[] {"BL_FMM_ModelGrpSeqConfigID"}}
        };

        private Dictionary<int, string[]> CopyModel = new Dictionary<int, string[]>()
        {
            {0, new string[] {"BL_FMM_CubeConfigID_Src"}},
            {1, new string[] {"BL_FMM_ActConfigID_Src"}},
            {2, new string[] {"BL_FMM_Src_ModelConfigID"}},
            {3, new string[] {"BL_FMM_Src_CalcConfigIDs"}},
            {4, new string[] {"BL_FMM_Tgt_CubeConfigID"}},
            {5, new string[] {"BL_FMM_Tgt_ActConfigID"}},
            {6, new string[] {"BL_FMM_Tgt_ModelConfigID"}}
        };
		

        private SessionInfo si;
        private BRGlobals globals;
        private object api;
        private DashboardExtenderArgs args;
        private readonly GBL_Helpers gblHelpers = new GBL_Helpers();

        public object Main(SessionInfo si, BRGlobals globals, object api, DashboardExtenderArgs args)
        {
            HierarchyDict.Add("FMM_CubeConfig", CubeConfig);
            HierarchyDict.Add("FMM_UnitAcctConfig", UnitAcctConfig);
            HierarchyDict.Add("FMM_CustTableConfig", CustTableConfig);
            HierarchyDict.Add("FMM_CustTableAssign", CustTableAssign);
            HierarchyDict.Add("FMM_ApprConfig", ApprConfig);
            HierarchyDict.Add("FMM_UIConfig", UIConfig);
            HierarchyDict.Add("FMM_DataValConfig", DataValConfig);
            HierarchyDict.Add("FMM_ModelConfig", ModelConfig);
            HierarchyDict.Add("FMM_ModelGrp", BuildModelGroup);
            HierarchyDict.Add("FMM_ModelGrpSeq", BuildModelGroupSeq);
            HierarchyDict.Add("FMM_ModelConfigDialog_Copy", CopyModel);

            try
            {
                this.si = si;
                this.globals = globals;
                this.api = api;
                this.args = args;

                switch (args.FunctionType)
                {
                    case DashboardExtenderFunctionType.LoadDashboard:
                        if (args.FunctionName.XFEqualsIgnoreCase("Load_FMM_DB"))
                        {
                            var loadDbTaskResult = Load_Dashboard(string.Empty, ref args);
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

        private XFLoadDashboardTaskResult Load_Dashboard(string runType, ref DashboardExtenderArgs args)
        {
            var loadDbTaskResult = new XFLoadDashboardTaskResult
            {
                ChangeCustomSubstVarsInDashboard = true
            };
            setParams(ref args, ref loadDbTaskResult);
            updateShowHide(ref args, ref loadDbTaskResult);

            return loadDbTaskResult;
        }

        private XFLoadDashboardTaskResult Get_CalcType(XFLoadDashboardTaskResult loadDbTaskResult)
        {
            var xfLoadDbTaskResult = loadDbTaskResult;
            var calcTypeTable = new DataTable("CalcType");
            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);

            try
            {
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sqlGblGetDatasets = new SQL_GBL_Get_DataSets(si, connection);
                    var adapter = new SqlDataAdapter();
                    var sql = @"SELECT CalcType
                                FROM FMM_CubeConfig Cb
                                JOIN FMM_ActConfig Act
                                    ON Cb.CubeConfigID = Act.CubeConfigID
                                WHERE Cb.CubeConfigID = @CubeConfigID
                                    AND Act.ActConfigID = @ActConfigID";

                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@CubeConfigID", SqlDbType.Int)
                        {
                            Value = Convert.ToInt32(xfLoadDbTaskResult.ModifiedCustomSubstVars.XFGetValue("BL_FMM_CubeConfigID", "0"))
                        },
                        new SqlParameter("@ActConfigID", SqlDbType.Int)
                        {
                            Value = Convert.ToInt32(xfLoadDbTaskResult.ModifiedCustomSubstVars.XFGetValue("IV_FMM_ActConfigID", "0"))
                        }
                    };

                    sqlGblGetDatasets.Fill_Get_GBL_DT(si, adapter, calcTypeTable, sql, parameters);
                }
            }
            catch
            {
            }

            if (calcTypeTable.Rows.Count > 0)
            {
                gblHelpers.UpdateCustomSubstVar(ref xfLoadDbTaskResult, globals, "DL_FMM_CalcType", calcTypeTable.Rows[0]["CalcType"].ToString());
            }
            else
            {
                gblHelpers.UpdateCustomSubstVar(ref xfLoadDbTaskResult, globals, "DL_FMM_CalcType", "Table");
            }

            return xfLoadDbTaskResult;
        }

        private void setupUpdateModelDialog(ref XFLoadDashboardTaskResult taskResult)
        {
            string modelName = string.Empty;
            int CubeConfigID = 0;
            int activityID = 0;
            int ModelConfigID = 0;

            var modelTable = new DataTable("Models");
            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);

            try
            {
                CubeConfigID = Convert.ToInt32(taskResult.ModifiedCustomSubstVars.XFGetValue("BL_FMM_CubeConfigID"));
                activityID = Convert.ToInt32(taskResult.ModifiedCustomSubstVars.XFGetValue("IV_FMM_ActConfigID"));
                ModelConfigID = Convert.ToInt32(taskResult.ModifiedCustomSubstVars.XFGetValue("IV_FMM_ModelConfigID"));
            }
            catch
            {
            }

            try
            {
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sqlGblGetDatasets = new SQL_GBL_Get_DataSets(si, connection);
                    var adapter = new SqlDataAdapter();
                    var sql = @"SELECT *
                                FROM FMM_ModelConfig
                                WHERE CubeConfigID = @CubeConfigID
                                    AND ActConfigID = @ActConfigID
                                    AND ModelConfigID = @ModelConfigID";

                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = CubeConfigID },
                        new SqlParameter("@ActConfigID", SqlDbType.Int) { Value = activityID },
                        new SqlParameter("@ModelConfigID", SqlDbType.Int) { Value = ModelConfigID },
                    };

                    sqlGblGetDatasets.Fill_Get_GBL_DT(si, adapter, modelTable, sql, parameters);
                }
            }
            catch
            {
            }

            if (modelTable.Rows.Count > 0)
            {
                modelName = modelTable.Rows[0]["Name"].ToString();
            }

            gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, "IV_FMM_ModelConfigName", modelName);
        }

        private void updateShowHide(ref DashboardExtenderArgs args, ref XFLoadDashboardTaskResult taskResult)
        {
            string showHideIVName = "IV_FMM_ShowHide_MenuBtn";
            string showBtnVisibleName = "IV_FMM_DispShow_MenuBtn";
            string hideBtnVisibleName = "IV_FMM_DispHide_MenuBtn";
            string menuWidthIV = "IV_FMM_MenuWidth";

            var arCustomSubst = args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved;
            string showHideIVVal = arCustomSubst.XFGetValue(showHideIVName, string.Empty);

            if (showHideIVVal == "Hide")
            {
                gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, showBtnVisibleName, "True");
                gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, hideBtnVisibleName, "False");
                gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, menuWidthIV, "0");
            }
            else if (showHideIVVal == "Show")
            {
                gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, showBtnVisibleName, "False");
                gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, hideBtnVisibleName, "True");
                gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, menuWidthIV, "Auto");
            }
        }

        private void setParams(ref DashboardExtenderArgs args, ref XFLoadDashboardTaskResult taskResult)
        {
            var dialogSelection = args.PrimaryDashboard.Name;

            string mainMenuSelection = !string.IsNullOrEmpty(args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue(MainMenuParam))
                ? args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue(MainMenuParam)
                : args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue(MainMenuParam);

            string selectedDashboard = mainMenuSelection;

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
                        ExecuteSpecificRefreshLogic(selectedDashboard, param, ref taskResult);
//						UpdateSpecificRefreshLogic(selectedDashboard, param, ref taskResult);
                    }
                }
            }
        }

        private void ExecuteSpecificRefreshLogic(string dashboard, string mappedParam, ref XFLoadDashboardTaskResult taskResult)
        {
            BRApi.ErrorLog.LogMessage(si, $"Hit {dashboard} - {mappedParam}");
            if (mappedParam == "BL_FMM_CubeConfigID" && dashboard == "FMM_CubeConfig")
            {
                Load_CubeConfig(ref taskResult);
            }

            if (mappedParam == "BL_FMM_ModelConfigID" && dashboard == "FMM_ModelConfig")
            {
				Load_ModelConfig(ref taskResult);
            }

            if (mappedParam == "IV_FMM_ActConfigID" && dashboard == "FMM_ApprConfig_Config")
            {
                Get_CalcType(taskResult);
            }

            if (mappedParam == "BL_FMM_CustTableConfigID" && dashboard == "FMM_CustTableConfig")
            {
                Load_CustTableConfig(ref taskResult);
            }

            if (mappedParam == "IV_FMM_ModelConfigID" && dashboard == "3_FMM_ModelConfigDialog_Update")
            {
                setupUpdateModelDialog(ref taskResult);
            }
        }

        private void Load_CubeConfig(ref XFLoadDashboardTaskResult loadDbTaskResult)
        {
            var modifiedVars = loadDbTaskResult.ModifiedCustomSubstVars;
            FMM_ConfigHelpers.SetCubeConfigParams(si, ref modifiedVars);
            loadDbTaskResult.ModifiedCustomSubstVars = modifiedVars;
            foreach (var kvp in modifiedVars)
            {
                // Replace 'Console.WriteLine' with your specific logging method (e.g., Log.Info)
                BRApi.ErrorLog.LogMessage(si, $"Key: {kvp.Key}, Value: {kvp.Value}");
            }
        }

        private void Load_AcctConfig(ref XFLoadDashboardTaskResult loadDbTaskResult)
        {
            //FMM_ConfigHelpers.SetCubeConfigParams(si, loadDbTaskResult.ModifiedCustomSubstVars);
        }

        private void Load_CustTableConfig(ref XFLoadDashboardTaskResult loadDbTaskResult)
        {
            var modifiedVars = loadDbTaskResult.ModifiedCustomSubstVars;
            FMM_ConfigHelpers.SetCustTableConfigParams(si, ref modifiedVars);
            loadDbTaskResult.ModifiedCustomSubstVars = modifiedVars;
            foreach (var kvp in modifiedVars)
            {
                // Replace 'Console.WriteLine' with your specific logging method (e.g., Log.Info)
                BRApi.ErrorLog.LogMessage(si, $"Key: {kvp.Key}, Value: {kvp.Value}");
            }
        }
		
        private void Load_ModelConfig(ref XFLoadDashboardTaskResult loadDbTaskResult)
        {
            var modifiedVars = loadDbTaskResult.ModifiedCustomSubstVars;
            FMM_ConfigHelpers.SetModelConfigParams(si, ref modifiedVars);
            loadDbTaskResult.ModifiedCustomSubstVars = modifiedVars;
            foreach (var kvp in modifiedVars)
            {
                // Replace 'Console.WriteLine' with your specific logging method (e.g., Log.Info)
                BRApi.ErrorLog.LogMessage(si, $"Key: {kvp.Key}, Value: {kvp.Value}");
            }
        }

        private string getDefaultParam(string param, Dictionary<string, string> customSubstVars)
        {
            if (param.Contains("IV_"))
            {
                if (param.XFContainsIgnoreCase("_AddUpdate"))
                {
                    return "Update";
                }
                param = param.Replace("IV_", "BL_");
            }

            DashboardParamDisplayInfo paramInfo = BRApi.Dashboards.Parameters.GetParameterDisplayInfo(si, false, customSubstVars, args.PrimaryDashboard.WorkspaceID, param);
            if (paramInfo?.ComboBoxItemsForBoundList?.Count > 0)
            {
                return paramInfo.ComboBoxItemsForBoundList.First().Value.ToString();
            }

            return "0";
        }

        private bool isValidParamValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            try
            {
                return int.Parse(value) > 0;
            }
            catch
            {
                return true;
            }
        }
		
		
		// Potentially Delete
//        private void UpdateSpecificRefreshLogic(string dashboard, string mappedParam, ref XFLoadDashboardTaskResult taskResult)
//        {
//            BRApi.ErrorLog.LogMessage(si, $"Hit Line 516 {mappedParam} - {dashboard}");
//			//Added - Devlin
//			if (mappedParam == " BL_FMM_CalcConfigID" && dashboard == "FMM_ModelConfig")
//			{
//				UpdateDetailVisibility(ref taskResult);
//			}
//		}
		
		//Added - Devlin
//		private void UpdateDetailVisibility(ref XFLoadDashboardTaskResult taskResult)
//		{
//            BRApi.ErrorLog.LogMessage(si, $"Hit Line 528");
//			string chkValue = "False";
			
//			foreach(var var in args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved)
//		    {
//		        BRApi.ErrorLog.LogMessage(si, $"Line 533 Resolved Var: {var.Key}, Value: {var.Value}");
//		    }
			
			
//			string visibilityState = chkValue.XFContainsIgnoreCase("True") ? "True" : "False";
			
//			if (taskResult.ModifiedCustomSubstVars.ContainsKey("vShow_FMM_CalcConfig_MbrList1"))
//			{
//				taskResult.ModifiedCustomSubstVars["vShow_FMM_CalcConfig_MbrList1"] = visibilityState;
//			}
//			else
//			{
//				taskResult.ModifiedCustomSubstVars.Add("vShow_FMM_CalcConfig_MbrList1",visibilityState);
//			}
			
//            BRApi.ErrorLog.LogMessage(si, $"Hit Line 554 {chkValue}");
//            BRApi.ErrorLog.LogMessage(si, $"Hit Line 555 {visibilityState}");
//		}
		
    }
}
