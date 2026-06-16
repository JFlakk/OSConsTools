using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
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
using OpenXmlPowerTools;
using Workspace.OSConsTools.GBL_UI_Assembly;


namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.DDM_ConfigData
{
    public class MainClass
    {
        #region "Global Variables"
        /// <summary>
        /// Stores the sort order for each menu header option by ID.
        /// </summary>
        public Dictionary<int, int> GBL_Menu_Hdr_Options_SortOrder_Dict { get; set; } = new Dictionary<int, int>();

        /// <summary>
        /// Stores the sort order for each menu option by ID.
        /// </summary>
        public Dictionary<int, int> GBL_Menu_Order_Dict { get; set; } = new Dictionary<int, int>();

        /// <summary>
        /// Stores the name for each menu option by ID.
        /// </summary>
        public Dictionary<int, String> GBL_Menu_Name_Dict { get; set; } = new Dictionary<int, String>();

        /// <summary>
        /// Indicates if duplicate menu options exist.
        /// </summary>
        public bool Duplicate_Menu_Options { get; set; } = false;

        /// <summary>
        /// Indicates if duplicate menu option sort orders exist.
        /// </summary>
        public bool Duplicate_Menu_Options_SortOrder { get; set; } = false;

        /// <summary>
        /// Indicates if duplicate menu header option sort orders exist.
        /// </summary>
        public bool Duplicate_Menu_Hdr_Options_SortOrder { get; set; } = false;

        private SessionInfo si;
        private BRGlobals globals;
        private object api;
        private DashboardExtenderArgs args;
        private StringBuilder debugString;
        #endregion

        public object Main(SessionInfo si, BRGlobals globals, object api, DashboardExtenderArgs args)
        {
            try
            {

                this.si = si;
                this.globals = globals;
                this.api = api;
                this.args = args;
                switch (args.FunctionType)
                {
                    case DashboardExtenderFunctionType.SqlTableEditorSaveData:
                        var save_Data_Task_Result = new XFSqlTableEditorSaveDataTaskResult();
                        if (args.FunctionName.XFEqualsIgnoreCase("Save_Profile_Config_Rows"))
                        {
                            // Save the data rows for profile config.
                            var saveDataTaskResult = new XFSqlTableEditorSaveDataTaskResult();
                            saveDataTaskResult.IsOK = true;
                            saveDataTaskResult.ShowMessageBox = false;
                            saveDataTaskResult.Message = "";
                            saveDataTaskResult.CancelDefaultSave = false;
                            return saveDataTaskResult;
                        }
						break;

                    case DashboardExtenderFunctionType.ComponentSelectionChanged:
                        var changedResult = new XFSelectionChangedTaskResult();
                        if (args.FunctionName.XFEqualsIgnoreCase("ConfigWFP_SaveAdd"))
                        {
                            changedResult = ConfigWFP_SaveAdd();
							return changedResult;
                        }
						else if (args.FunctionName.XFEqualsIgnoreCase("MenuLayoutConfig_SaveAdd"))
						{
                            return this.MenuLayoutConfig_Save("Add");
                        }
						else if (args.FunctionName.XFEqualsIgnoreCase("MenuLayoutConfig_SaveUpdate"))
						{
                            return this.MenuLayoutConfig_Save("Update");
                        }
						else if (args.FunctionName.XFEqualsIgnoreCase("MenuLayoutConfig_Add"))
						{
                            return this.MenuLayoutConfig_Add();
                        }
						else if (args.FunctionName.XFEqualsIgnoreCase("Select_DDM_MenuLayoutConfig"))
						{
                            return this.Select_DDM_MenuLayoutConfig();
                        }
						else if (args.FunctionName.XFEqualsIgnoreCase("LayoutConfigType_Select"))
						{
                            return this.LayoutConfigType_Select();
                        }
						else if (args.FunctionName.XFEqualsIgnoreCase("MenuLayoutConfig_SaveAdd"))
						{
                            return this.MenuLayoutConfig_Save("Add");
                        }
						else if (args.FunctionName.XFEqualsIgnoreCase("Select_DDM_MenuLayout_Option_Type"))
						{
                            return this.Select_DDM_MenuLayout_Option_Type();
                        }
						else if (args.FunctionName.XFEqualsIgnoreCase("Select_Add_DDM_HdrConfigs"))
						{
                            return this.Select_Add_DDM_HdrConfigs();
                        }
						else if (args.FunctionName.XFEqualsIgnoreCase("Select_DDM_Config_Hdr_Ctrl"))
						{
                            return this.Select_DDM_Config_Hdr_Ctrl();
                        }
						else if (args.FunctionName.XFEqualsIgnoreCase("HdrConfigs_SaveAdd"))
						{

                            return this.HdrConfig_SaveAdd();
                        }
						break;
                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and rethrow exceptions for error handling.
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSqlTableEditorSaveDataTaskResult Save_DDM_Profile_Config_Rows()
        {
            try
            {
                var save_Data_Task_Result = new XFSqlTableEditorSaveDataTaskResult();
                var saveDataTaskInfo = args.SqlTableEditorSaveDataTaskInfo;
                int os_DDM_MenuID = 0;

                // Loop through each row in the table editor that was added or updated prior to hitting save
                foreach (XFEditedDataRow xfRow in saveDataTaskInfo.EditedDataRows)
                {
                    if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Insert)
                    {
                        // Handle insert logic for new profile config row.
                        var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                        using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                        {
                            connection.Open();
                            var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                            os_DDM_MenuID = sql_gbl_get_max_id.Get_Max_ID(si, "DDM_Config_Menu", "DDM_MenuID");
                        }
                        xfRow.ModifiedDataRow.SetValue("DDM_MenuID", os_DDM_MenuID, XFDataType.Int16);
                        xfRow.ModifiedDataRow.SetValue("Status", "In Process", XFDataType.Text);
                        xfRow.ModifiedDataRow.SetValue("Create_Date", DateTime.Now, XFDataType.DateTime);
                        xfRow.ModifiedDataRow.SetValue("CreateUser", si.UserName, XFDataType.Text);
                        xfRow.ModifiedDataRow.SetValue("Update_Date", DateTime.Now, XFDataType.DateTime);
                        xfRow.ModifiedDataRow.SetValue("UpdateUser", si.UserName, XFDataType.Text);
                    }
                    else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Update)
                    {
                        // Handle update logic for existing profile config row.
                        xfRow.ModifiedDataRow.SetValue("Update_Date", DateTime.Now, XFDataType.DateTime);
                        xfRow.ModifiedDataRow.SetValue("UpdateUser", si.UserName, XFDataType.Text);
                    }
                    else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Delete)
                    {
                        // Handle delete logic for profile config row if necessary.
                        xfRow.ModifiedDataRow.SetValue("Update_Date", DateTime.Now, XFDataType.DateTime);
                        xfRow.ModifiedDataRow.SetValue("UpdateUser", si.UserName, XFDataType.Text);
                    }
                }

                // Set return value
                save_Data_Task_Result.IsOK = true;
                save_Data_Task_Result.ShowMessageBox = false;
                save_Data_Task_Result.Message = String.Empty;
                if (Duplicate_Menu_Options == true || Duplicate_Menu_Options_SortOrder == true)
                {
                    save_Data_Task_Result.CancelDefaultSave = false;
                }
                else
                {
                    save_Data_Task_Result.CancelDefaultSave = true;
                }
                return save_Data_Task_Result;
            }
            catch (Exception ex)
            {
                // Log and rethrow exceptions for error handling.
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSelectionChangedTaskResult ConfigWFP_SaveAdd()
        {
            try
            {
                var save_Data_Task_Result = new XFSelectionChangedTaskResult();
				
                var wfpKey = Guid.Parse(args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_DDM_WFPtrv", Guid.Empty.ToString()));

                var newWFPConfigID = 0;
                if (newWFPConfigID == 0)
                {
                    var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                        connection.Open();

                        // Create a new DataTable
                        var sqa = new SqlDataAdapter();
                        var DDM_DynDBConfig_Count_DT = new DataTable();
                        // Define the select query and parameters
                        var sql = @"SELECT Count(*) as Count
                                    FROM DDM_DynDBConfig
                                    WHERE WFPKey = @wfpKey";

                        // Create an array of SqlParameter objects
                        var sqlparams = new SqlParameter[]
                        {
                        new SqlParameter("@wfpKey", SqlDbType.UniqueIdentifier) { Value = wfpKey }
                        };

                        sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, DDM_DynDBConfig_Count_DT, sql, sqlparams);

                        if (Convert.ToInt32(DDM_DynDBConfig_Count_DT.Rows[0]["Count"]) == 0)
						{

	                        // Example: Get the max ID for the "MCM_Calc_Config" table
							var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
	                        var DDM_DynDBConfig_DT = new DataTable();
	
	                        // Fill the DataTable with the current data from MCM_Dest_Cell
	                        sql = @"SELECT * 
									FROM DDM_DynDBConfig 
									WHERE DynDBConfigID = @DynDBConfigID";
	
	                        // Update the value of the existing sqlparams array
							sqlparams = new SqlParameter[]
	                        {
	                        new SqlParameter("@DynDBConfigID", SqlDbType.Int) { Value = newWFPConfigID }
	                        };
							
							cmdBuilder.FillDataTable(si, sqa, DDM_DynDBConfig_DT, sql, sqlparams);
	
	                        var wfpStepType_DT = new DataTable();
	                        sql = @"SELECT *
		                            FROM WorkflowProfileHierarchy
		                            WHERE ProfileKey = @WFPKey";
	                        sqlparams = new SqlParameter[]
	                        {
	                            new SqlParameter("@WFPKey", SqlDbType.UniqueIdentifier) { Value = wfpKey }
	                        };
	                        sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, wfpStepType_DT, sql, sqlparams);
	
	                        var newRow = DDM_DynDBConfig_DT.NewRow();
							newRow["DynDBType"] = 1;
	                        newRow["WFPKey"] = wfpKey;
	                        if (wfpStepType_DT.Rows.Count > 0)
	                        {
	                            newRow["WFPStepType"] = wfpStepType_DT.Rows[0]["ProfileType"];
	                        }
	                        newRow["Status"] = 1;
	                        newRow["CreateDate"] = DateTime.Now;
	                        newRow["CreateUser"] = si.UserName;
	                        newRow["UpdateDate"] = DateTime.Now;
	                        newRow["UpdateUser"] = si.UserName;
	                        // Set other column values for the new row as needed
	                        DDM_DynDBConfig_DT.Rows.Add(newRow);

							cmdBuilder.UpdateTable(si, "DDM_DynDBConfig", DDM_DynDBConfig_DT, sqa);
						}
                    }
                }

                return save_Data_Task_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSelectionChangedTaskResult MenuLayoutConfig_Add()
        {

            var selectResult = new XFSelectionChangedTaskResult();
            selectResult.ChangeCustomSubstVarsInDashboard = true;
            var gbl_helpers = new GBL_UI_Assembly.GBL_Helpers();
			var custsubstvars = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues;
			foreach (var substVar in custsubstvars)
			{
				gbl_helpers.UpdateCustomSubstVar(ref selectResult, substVar.Key, substVar.Value);
			}
            gbl_helpers.UpdateCustomSubstVar(ref selectResult, "BL_DDM_MenuLayoutConfig", string.Empty);
            return selectResult;
        }

        private XFSelectionChangedTaskResult LayoutConfigType_Select()
        {

            var selectResult = new XFSelectionChangedTaskResult();
            selectResult.ChangeCustomSubstVarsInDashboard = true;
            var gbl_helpers = new GBL_UI_Assembly.GBL_Helpers();
			var custsubstvars = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues;
			foreach (var substVar in custsubstvars)
			{
				gbl_helpers.UpdateCustomSubstVar(ref selectResult, substVar.Key, substVar.Value);
								BRApi.ErrorLog.LogMessage(si, $"Available Var: {substVar.Key} = {substVar.Value}");				
								
			}
            return selectResult;
        }
		
		private XFSelectionChangedTaskResult Select_Add_DDM_MenuLayoutConfig()
		{
		    try
		    {
		        var select_Result = new XFSelectionChangedTaskResult();
				select_Result.ChangeCustomSubstVarsInDashboard = true;
				var optionintValue = 2;
				UpdateCustomSubstVar(ref select_Result,"BL_DDM_MenuLayoutConfig",string.Empty);
	            DDM_ConfigHelpers.LayoutType layoutType = (DDM_ConfigHelpers.LayoutType)optionintValue;
	
	            // 3. Lookup in your LayoutRegistry class
	            if (DDM_ConfigHelpers.LayoutRegistry.Configs.TryGetValue(layoutType, out var config))
	            {
					// 4. Update the UI Subst Var so the Dashboard flips to the correct UI
	                UpdateCustomSubstVar(ref select_Result, "IV_DDM_MenuLayout_Option_Type", config.DashboardName);
				}
				return select_Result;
			}
            catch (Exception ex)
            {
                // Log and rethrow exceptions for error handling.
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
		}
		
		private XFSelectionChangedTaskResult Select_DDM_MenuLayoutConfig()
		{
		    try
		    {
		        var select_Result = new XFSelectionChangedTaskResult();
				select_Result.ChangeCustomSubstVarsInDashboard = true;
				BRApi.ErrorLog.LogMessage(si,$"Hit Select Menu Layout");
				select_Result.ModifiedCustomSubstVars = this.args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues;
				var existingMenuID = this.args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("BL_DDM_MenuLayoutConfig", "0").XFConvertToInt();
				var DDM_MenuLayoutConfigLayout_DT = new DataTable();
				UpdateCustomSubstVar(ref select_Result,"IV_DDM_MenuLayoutConfigUI","DDM_Config_AddUpdate");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
					var sqa = new SqlDataAdapter();
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    connection.Open();
	                var sql = @"SELECT *
	                        FROM DDM_DynDBMenuLayoutConfig
							WHERE DynDBMenuID = @DynDBMenuID";
	                var sqlparams = new SqlParameter[]
	                {
	                    new SqlParameter("@DynDBMenuID", SqlDbType.Int) { Value = existingMenuID }
	                };
	                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, DDM_MenuLayoutConfigLayout_DT, sql, sqlparams);
				}
				if (DDM_MenuLayoutConfigLayout_DT.Rows.Count > 0)
		        {
		            DataRow row = DDM_MenuLayoutConfigLayout_DT.Rows[0];
		
		            // 2. Extract Option_Type and Convert to Enum
		            DDM_ConfigHelpers.LayoutType layoutType = (DDM_ConfigHelpers.LayoutType)row["LayoutType"];
					UpdateCustomSubstVar(ref select_Result, "DL_DDM_MenuLayout_Type", row["LayoutType"].ToString());
		            // 3. Lookup in your LayoutRegistry class
		            if (DDM_ConfigHelpers.LayoutRegistry.Configs.TryGetValue(layoutType, out var config))
		            {
						foreach (var step in config.ParameterMappings)
						{
						    // The 'step.Value' is the inner Dictionary<string, string>
						    // It usually contains just one pair, but we loop to be safe
						    foreach (var map in step.Value)
						    {
						        string tgtParamName = map.Key;   // e.g. "IV_DDM_MenuLayout_Top_Height"
						        string columnName = map.Value;  // e.g. "Top_Height"
								UpdateCustomSubstVar(ref select_Result, tgtParamName, row[columnName].ToString());
							}
						}
					}
					
				}
				return select_Result;
			}
            catch (Exception ex)
            {
                // Log and rethrow exceptions for error handling.
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
		}
		
		private XFSelectionChangedTaskResult Select_DDM_MenuLayout_Option_Type()
		{
		    try
		    {
		        var select_Result = new XFSelectionChangedTaskResult();
				var DDM_MenuLayoutConfigLayout_DT = new DataTable();

				var optionintValue = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("DL_DDM_MenuLayout_Type", "0").XFConvertToInt();
				var histoptionintValue = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("DL_DDM_MenuLayout_Type", "0").XFConvertToInt();

				//UpdateCustomSubstVar(ref select_Result,"IV_DDM_Config_Menu_UI","0b1b2b2_DDM_Config_Content_NewUpdates");
BRApi.ErrorLog.LogMessage(si,$"Hit {optionintValue} - {histoptionintValue} - {args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_DDM_MenuLayout_Option_Type", "NA")} - {args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_DDM_MenuLayout_Option_Type", "NA")}");
	            // 2. Extract Option_Type and Convert to Enum
	            DDM_ConfigHelpers.LayoutType layoutType = (DDM_ConfigHelpers.LayoutType)optionintValue;
	
	            // 3. Lookup in your LayoutRegistry class
	            if (DDM_ConfigHelpers.LayoutRegistry.Configs.TryGetValue(layoutType, out var config))
	            {
BRApi.ErrorLog.LogMessage(si,$"Hit {config.DashboardName}");
					// 4. Update the UI Subst Var so the Dashboard flips to the correct UI
	                UpdateCustomSubstVar(ref select_Result, "IV_DDM_MenuLayout_Option_Type", config.DashboardName);
				}
				foreach (KeyValuePair<string, string> kvp in select_Result.ModifiedCustomSubstVars)
    {
        	BRApi.ErrorLog.LogMessage(si,$"Key: {kvp.Key} | Value: {kvp.Value}");
    }
				var uiAction = new XFSelectionChangedUIActionInfo();
				uiAction.SelectionChangedUIActionType = XFSelectionChangedUIActionType.Refresh;
				uiAction.DashboardsToRedraw = "0b1b2b_DDM_Config_Content";
				select_Result.ModifiedSelectionChangedUIActionInfo = uiAction;
				return select_Result;
			}
            catch (Exception ex)
            {
                // Log and rethrow exceptions for error handling.
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
		}


		private XFSelectionChangedTaskResult Select_Add_DDM_HdrConfigs()
		{
		    try
		    {
		        var select_Result = new XFSelectionChangedTaskResult();
				select_Result.ChangeCustomSubstVarsInDashboard = true;
				UpdateCustomSubstVar(ref select_Result,"BL_DDM_Config_Hdrs",string.Empty);
				return select_Result;
			}
            catch (Exception ex)
            {
                // Log and rethrow exceptions for error handling.
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
		}
		
		private XFSelectionChangedTaskResult Select_DDM_Config_Hdr_Ctrl()
		{
		    try
		    {
				BRApi.ErrorLog.LogMessage(si,$"Hit here 481");
		        var select_Result = new XFSelectionChangedTaskResult();
				var DDM_HdrConfigs_DT = new DataTable();
				var HdrConfigID = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("BL_DDM_Config_Hdrs").XFConvertToInt();
				select_Result.ChangeCustomSubstVarsInDashboard = true;
				UpdateCustomSubstVar(ref select_Result,"IV_DDM_Hdr_UI","DDM_HdrConfig_AddUpdate");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
					var sqa = new SqlDataAdapter();
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    connection.Open();
	                var sql = @"SELECT *
	                        FROM DDM_DynDBHdrConfig

							WHERE DynDBHdrID = @DynDBHdrID";
	                var sqlparams = new SqlParameter[]
	                {
	                    new SqlParameter("@DynDBHdrID", SqlDbType.Int) { Value = HdrConfigID }
	                };
	                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, DDM_HdrConfigs_DT, sql, sqlparams);
					BRApi.ErrorLog.LogMessage(si,$"Hit here 466 {sql}");
				}
					
				if (DDM_HdrConfigs_DT.Rows.Count > 0)
		        {
		            DataRow row = DDM_HdrConfigs_DT.Rows[0];
	
		            // 2. Extract Option_Type and Convert to Enum
		            DDM_ConfigHelpers.HdrType HdrType = (DDM_ConfigHelpers.HdrType)row["HdrType"];
		
		            // 3. Lookup in your LayoutRegistry class
		            if (DDM_ConfigHelpers.HdrRegistry.Configs.TryGetValue(HdrType, out var config))
		            {
		                // Retrieve the DashboardName from the class
		                string targetDashboard = config.DashboardName;
		BRApi.ErrorLog.LogMessage(si,$"Hit: {targetDashboard}");
		                // 4. Update the UI Subst Var so the Dashboard flips to the correct UI
		                UpdateCustomSubstVar(ref select_Result, "IV_DDM_Hdr_Option_Type", targetDashboard);
					}
				}
				return select_Result;
			}
            catch (Exception ex)
            {
                // Log and rethrow exceptions for error handling.
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
		}
		
        #region "Menu Layout Save"
		private XFSelectionChangedTaskResult MenuLayoutConfig_Save(string runType)
		{
		    try
		    {
				var existingMenuID = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("BL_DDM_MenuLayoutConfig", "0").XFConvertToInt();

		        var save_Result = new XFSelectionChangedTaskResult();
		        var configID = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_DDM_ConfigID").XFConvertToInt();
		        var menuName = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_DDM_MenuLayout_Name");
		        var sortOrder = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_DDM_Menu_SortOrder", "0").XFConvertToInt();
		        var layoutType = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("DL_DDM_LayoutConfigType", "0").XFConvertToInt();
		BRApi.ErrorLog.LogMessage(si,$"Hit ConfigID: {configID}, MenuName: {menuName}, SortOrder: {sortOrder}, layoutType:{layoutType} ");
		        // 1. Run Duplicate Check before proceeding
		        // We 'Initiate' to fill GBL_Menu dictionaries from the DB
		        Duplicate_Menu_Check(configID, "Initiate");
		        
		        // Logic to check if current name/sort order exists in the dictionaries 
		        // (Assuming Duplicate_Menu_Options/SortOrder are class-level booleans)
		        if (GBL_Menu_Name_Dict.Values.Contains(menuName) && runType == "Add")
		            throw new Exception("A menu with this name already exists.");
		
		        var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
		        using (SqlConnection connection = new SqlConnection(dbConnApp.ConnectionString))
		        {
		            connection.Open();
		            var sqa = new SqlDataAdapter();
		            var DDM_MenuLayoutConfigLayout_DT = new DataTable();
		        	var sqlSelect = @"SELECT * FROM DDM_DynDBMenuLayoutConfig WHERE DynDBMenuID = @DDM_MenuID";
					var sqlparams = new SqlParameter[]
	                {
	                };
					var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
		            DataRow row;
		            if (runType == "Add")
		            {
						var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
		                var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
		                var newID = sql_gbl_get_max_id.Get_Max_ID(si, "DDM_DynDBMenuLayoutConfig", "DynDBMenuID");
						var DDM_Config_DT = new DataTable();
						sqlparams = new SqlParameter[]
	                	{
							new SqlParameter("@DDM_MenuID", SqlDbType.Int) { Value = newID }
	                	};
		                
		                cmdBuilder.FillDataTable(si, sqa, DDM_MenuLayoutConfigLayout_DT, sqlSelect, sqlparams);
						sqlSelect = @"SELECT * FROM DDM_DynDBConfig WHERE DynDBConfigID = @DDM_ConfigID";
						sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, DDM_Config_DT, sqlSelect, new SqlParameter[] { new SqlParameter("@DDM_ConfigID", SqlDbType.Int) { Value = configID }});
		                row = DDM_MenuLayoutConfigLayout_DT.NewRow();
//						if (DDM_Config_DT.Rows.Count > 0)
//					    {
							
//					        var typeValue = DDM_Config_DT.Rows[0]["DynDBType"];
//					        DDM_ConfigHelpers.DDMType configType = (DDM_ConfigHelpers.DDMType)typeValue;
					        
//					        switch (configType)
//					        {
//					            case DDM_ConfigHelpers.DDMType.WFProfile:
//					                row["DynDBType"] = (int)DDM_ConfigHelpers.DDMType.WFProfile;
//					                row["ScenType"] = DDM_Config_DT.Rows[0]["ScenType"];
//									row["WFPKey"] = DDM_Config_DT.Rows[0]["WFPKey"];
//					                break;
					
//					            case DDM_ConfigHelpers.DDMType.StandAlone:
//					                row["DDM_Type"] = (int)DDM_ConfigHelpers.DDMType.StandAlone;
//					                // Add specific logic/column defaults for StandAlone here
//					                break;
//					        }
//					    }
		                row["DynDBMenuID"] = newID;
						row["Status"] = 1;
		                row["CreateDate"] = DateTime.Now;
		                row["CreateUser"] = si.UserName;
		                row["UpdateDate"] = DateTime.Now;
		                row["UpdateUser"] = si.UserName;
		                DDM_MenuLayoutConfigLayout_DT.Rows.Add(row);
		            }
		            else
		            {
		                cmdBuilder.FillDataTable(si, sqa, DDM_MenuLayoutConfigLayout_DT, sqlSelect, new SqlParameter[] { new SqlParameter("@Menu_ID", existingMenuID) });
		                if (DDM_MenuLayoutConfigLayout_DT.Rows.Count == 0) throw new Exception("Record not found.");
		                row = DDM_MenuLayoutConfigLayout_DT.Rows[0];
		            }
		
				//	BRApi.ErrorLog.LogMessage(si,$"Hit: {configID} - {layoutType} - {menuName} - {sortOrder}");
		            // Map standard fields
		            row["DynDBConfigID"] = configID;
		            row["Name"] = menuName;
		            row["SortOrder"] = sortOrder;
		            row["LayoutType"] = layoutType;
		            row["UpdateDate"] = DateTime.Now;
		            row["UpdateUser"] = si.UserName;
		BRApi.ErrorLog.LogMessage(si,$"Hit584");
		
					DDM_ConfigHelpers.LayoutType layouttype = (DDM_ConfigHelpers.LayoutType)layoutType;
					if (DDM_ConfigHelpers.LayoutRegistry.Configs.TryGetValue(layouttype, out var layoutConfig))
					{
						
					   //  2. Iterate through the mapping: Key = IV Name (Source), Value = DB Column (Target)
					     // ParameterMappings is Dictionary<int, Dictionary<string, string>>;
					    foreach (var group in layoutConfig.ParameterMappings.Values)
					    {
					        foreach (var mapping in group)
					        {
					            string sourceSubstVar = mapping.Key;   // e.g., "IV_DDM_MenuLayout_DB_Name"
					            string targetCol = mapping.Value; // e.g., "DB_Name"
					            	BRApi.ErrorLog.LogMessage(si,$"Hit597");
					            // Assign the value from CustomSubstVars to the DataRow
					            row[targetCol] = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue(sourceSubstVar, string.Empty);
					        }
							
					    }
					}
//		foreach (var group in layoutConfig.ParameterMappings.Values)
//{
//    foreach (var mapping in group)
//    {
//        string sourceSubstVar = mapping.Key;   
//        string targetCol = mapping.Value; 
        
//        // 1. Get the raw value from OneStream
//        string rawValue = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue(sourceSubstVar, string.Empty);
        
//        // 2. Identify the target column type
//        Type colType = DDM_MenuLayoutConfigLayout_DT.Columns[targetCol].DataType;

//        if (colType == typeof(int) || colType == typeof(Int32))
//        {
//            // If it's an integer column, try to parse. 
//            // If empty or invalid, use 0 (or DBNull.Value if you prefer)
//            int parsedInt;
//            if (int.TryParse(rawValue, out parsedInt))
//            {
//                row[targetCol] = parsedInt;
//            }
//            else
//            {
//                // To store a real NULL in the DB:
//                row[targetCol] = DBNull.Value; 
                
//                // OR to store a zero:
//                // row[targetCol] = 0; 
//            }
//        }
//        else
//        {
//            // For string/text columns, assign directly
//            row[targetCol] = rawValue;
//        }
//    }
//}
								
						
						
		            // 2. Perform Layout Validation and Column Clearing
						BRApi.ErrorLog.LogMessage(si,$"Hit Before:"); 
		            var valResult = Val_DDM_MenuLayoutConfigLayout(ref row);
		            if (!valResult.IsOK) return valResult;
				BRApi.ErrorLog.LogMessage(si,$"Hit: ");
		            // 3. Save to DB
					LogDataTable(DDM_MenuLayoutConfigLayout_DT,"Menu");
		            cmdBuilder.UpdateTable(si, "DDM_DynDBMenuLayoutConfig", DDM_MenuLayoutConfigLayout_DT, sqa);
		            save_Result.Message = "Save Successful.";
		        }
		BRApi.ErrorLog.LogMessage(si,$"Hit: ");
		        save_Result.IsOK = true;
		        save_Result.ShowMessageBox = true;
		        return save_Result;
				}
		    catch (Exception ex) { return new XFSelectionChangedTaskResult { IsOK = false, Message = ex.Message, ShowMessageBox = true }; }
		}
		
		#endregion
		
		
        #region "Header Controls Save"
	
		private XFSelectionChangedTaskResult HdrConfig_SaveAdd()
		{
			 
		    try
		    {
				BRApi.ErrorLog.LogMessage(si, $"HERE Save Header");
				var runType = "New";
				var existingHdrID = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("BL_DDM_HdrConfigs", "0").XFConvertToInt();
				
				if (existingHdrID > 0)
				{
					runType = "Update";
				}
		        var save_Result = new XFSelectionChangedTaskResult();
		        var configID = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_DDM_ConfigID", "0").XFConvertToInt();
		        var menuName = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_DDM_Hdr_Name", string.Empty);
		        var sortOrder = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_DDM_Hdr_SortOrder", "0").XFConvertToInt();
		        var layoutType = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("DL_DDM_Hdr_Type", "0").XFConvertToInt();
				var menuID = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("BL_DDM_MenuLayoutConfig", "0").XFConvertToInt();
		 BRApi.ErrorLog.LogMessage(si, $"HERE Save HDR Name {layoutType}");
		        // 1. Run Duplicate Check before proceeding
		        // We 'Initiate' to fill GBL_Menu dictionaries from the DB
		        Duplicate_Hdr_Check(menuID, "Initiate");
		        
		        // Logic to check if current name/sort order exists in the dictionaries 
		        // (Assuming Duplicate_Menu_Options/SortOrder are class-level booleans)
		        if (GBL_Menu_Name_Dict.Values.Contains(menuName) && runType == "New")
		            throw new Exception("A menu with this name already exists.");
		
		        var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
		        using (SqlConnection connection = new SqlConnection(dbConnApp.ConnectionString))
		        {
		            connection.Open();
		            var sqa = new SqlDataAdapter();
		            var DDM_HdrConfigs_DT = new DataTable();
		            		            var sqlSelect = "SELECT * FROM DDM_HdrConfigs WHERE DDM_HdrID = @DDM_HdrID";
					var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);		
		            DataRow row;
		            if (runType == "New")
		            {
BRApi.ErrorLog.LogMessage(si, $"HERE New run");
						var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
		                var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
		                var newID = sql_gbl_get_max_id.Get_Max_ID(si, "DDM_HdrConfigs", "DDM_HdrID");
						var DDM_Config_DT = new DataTable();
		                
		                cmdBuilder.FillDataTable(si, sqa, DDM_HdrConfigs_DT, sqlSelect, new SqlParameter[] { new SqlParameter("@DDM_HdrID", -1) });
						sqlSelect = "SELECT * FROM DDM_Config WHERE DDM_ConfigID = @DDM_ConfigID";
						sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, DDM_Config_DT, sqlSelect, new SqlParameter[] { new SqlParameter("@DDM_ConfigID", SqlDbType.Int) { Value = configID }});
		                row = DDM_HdrConfigs_DT.NewRow();
						DDM_HdrConfigs_DT.Rows.Add(row);
						if (DDM_Config_DT.Rows.Count > 0)
					    {
							
					        var typeValue = DDM_Config_DT.Rows[0]["DDM_Type"];
					       	row["DDM_Type"] = typeValue;
							DDM_ConfigHelpers.DDMType configType = (DDM_ConfigHelpers.DDMType)typeValue;
     
					        
					        switch (configType)
					        {
					            case DDM_ConfigHelpers.DDMType.WFProfile:
					                row["DDM_Type"] = (int)DDM_ConfigHelpers.DDMType.WFProfile;
					                row["ScenType"] = DDM_Config_DT.Rows[0]["ScenType"];
									row["ProfileKey"] = DDM_Config_DT.Rows[0]["ProfileKey"];
					                break;
					
					            case DDM_ConfigHelpers.DDMType.StandAlone:
					                row["DDM_Type"] = (int)DDM_ConfigHelpers.DDMType.StandAlone;
					                // Add specific logic/column defaults for StandAlone here
					                break;
					        }
					    }
						BRApi.ErrorLog.LogMessage(si, $"HERE New run New ID{newID}");
		                row["DDM_MenuID"] = menuID;
						row["DDM_ConfigID"] = configID;
						row["DDM_HdrID"] = newID;
						row["Status"] = "In Process";
		               
		            }
		            else
						
		            {
						
		                cmdBuilder.FillDataTable(si, sqa, DDM_HdrConfigs_DT, sqlSelect, new SqlParameter[] { new SqlParameter("@DDM_HdrID", existingHdrID) });
		                if (DDM_HdrConfigs_DT.Rows.Count == 0) throw new Exception("Record not found.");
		                row = DDM_HdrConfigs_DT.Rows[0];
		            }
		 BRApi.ErrorLog.LogMessage(si, $"HERE Save HDR 2 Name {menuName}");
		            // Map standard fields
		            row["Name"] = menuName;
		            row["SortOrder"] = sortOrder;
					row["Status"] = "In Process";
		           	row["Option_Type"] = layoutType;
					row["HdrType"] = layoutType;
		            row["CreateDate"] = DateTime.Now;
		            row["CreateUser"] = si.UserName;
		            row["UpdateDate"] = DateTime.Now;
		            row["UpdateUser"] = si.UserName;
		
 					DDM_ConfigHelpers.HdrType HdrType = (DDM_ConfigHelpers.HdrType)row["Option_Type"];	
		        	if (DDM_ConfigHelpers.HdrRegistry.Configs.TryGetValue(HdrType, out var HdrConfig))
					{
						foreach (var group in HdrConfig.ParameterMappings.Values)
{
    foreach (var mapping in group)
    {
        string sourceSubstVar = mapping.Key; 
        string targetCol = mapping.Value;
// Temporary Debug: List every variable available to the rule
foreach (var key in args.SelectionChangedTaskInfo.CustomSubstVars.Keys)
{
    BRApi.ErrorLog.LogMessage(si, $"Available Var: {key} | Value: {args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue(key)}");
}
        // Retrieve the value
        string varValue = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, false, sourceSubstVar);
	//args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue(sourceSubstVar);

        // Basic validation: Ensure the column exists in the row's table to prevent 'Column not found' errors
        if (row.Table.Columns.Contains(targetCol))
        {
            row[targetCol] = varValue;
        }
        else
        {
            BRApi.ErrorLog.LogMessage(si, $"Mapping Error: Column {targetCol} not found in target table.");
        }
    }
}
						
						
						
//					    foreach (var group in HdrConfig.ParameterMappings.Values)
//					    {
//					        foreach (var mapping in group)
//					        {
//					            string sourceSubstVar = mapping.Key; 
//					            string targetCol = mapping.Value;
// BRApi.ErrorLog.LogMessage(si, $"HERE Save HDR 3 {sourceSubstVar} - {targetCol}");
//					            row[targetCol] = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue(sourceSubstVar);
//								 BRApi.ErrorLog.LogMessage(si, $"HERE Save HDR 3 {sourceSubstVar} - {targetCol}");
//					        }
//					    }
					}
					 BRApi.ErrorLog.LogMessage(si, $"HERE Save HDR 3");
		            var valResult = Val_DDM_HdrConfigs(ref row);
		            if (!valResult.IsOK) return valResult;
					
					
					
				//	LogDataTable(DDM_HdrConfigs_DT,"Header");
		           // cmdBuilder.UpdateTable(si, "DDM_HdrConfigs", DDM_HdrConfigs_DT, sqa);
					
					try 
{
	BRApi.ErrorLog.LogMessage(si, $"FINAL CHECK: Row ID is {row["DDM_HdrID"]}, Row Status is {row["Status"]}");

    cmdBuilder.UpdateTable(si, "DDM_HdrConfigs", DDM_HdrConfigs_DT, sqa);
}
catch (Exception ex) 
{
    throw new Exception($"UpdateTable Failed. Table: DDM_HdrConfigs. Error: {ex.Message} | Inner: {ex.InnerException?.Message}");
}
					
		            save_Result.Message = "Save Successful.";
					BRApi.ErrorLog.LogMessage(si, "UpdateTableSimple Finished Successfully");
		        }
		
		        save_Result.IsOK = true;
		        save_Result.ShowMessageBox = true;
		        return save_Result;
		    }
		    catch (Exception ex) { return new XFSelectionChangedTaskResult { IsOK = false, Message = ex.Message, ShowMessageBox = true }; }
		}
		
		#endregion
		
		#region "Data Validation"
		

        #region "Duplicate Checks"
		/// <summary>
        /// Checks for duplicate menu options and sort orders.
        /// This method is used to identify duplicates during the save process.
        /// </summary>
        private void Duplicate_Menu_Check(int wfProfile_ID, String Dup_Process_Step, [Optional] String DDL_Process, [Optional] XFEditedDataRow Config_Menu_DataRow)
        {
            switch (Dup_Process_Step)
            {
                case "Initiate":
                    // Select rows from the table before any updates to rows are processed
                    {
                        var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                        using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                        {
                            var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                            connection.Open();

                            // Create a new DataTable
                            var sqa = new SqlDataAdapter();
                            var DDM_Config_Menu_DT = new DataTable();
                            // Define the select query and parameters
                            var sql = @"SELECT *
						       			FROM DDM_DynDBMenuLayoutConfig
						       			WHERE DynDBConfigID = @DDM_ConfigID";
                            // Create an array of SqlParameter objects
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@DDM_ConfigID", SqlDbType.Int) { Value = wfProfile_ID}
                            };

                            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, DDM_Config_Menu_DT, sql, sqlparams);

                            foreach (DataRow menu_Row in DDM_Config_Menu_DT.Rows)
                            {
                                int menu_ID = (int)menu_Row["DynDBMenuID"];
                                int sortOrder = (int)menu_Row["SortOrder"];
                                string menu_Name = (string)menu_Row["Name"];

                                GBL_Menu_Order_Dict.Add(menu_ID, sortOrder);
                                GBL_Menu_Name_Dict.Add(menu_ID, menu_Name);
                            }
                        }
                    }
                    break;
                case "Update Row":
                    // Check for duplicate menu options when the process step is "Initiate"
                    if (DDL_Process == "Insert")
                    {
                        int menuOptionID = (int)Config_Menu_DataRow.OriginalDataRow["DynDBMenuID"];
                        int sortOrder = (int)Config_Menu_DataRow.ModifiedDataRow["SortOrder"];
                        string menu_Name = (string)Config_Menu_DataRow.ModifiedDataRow["Name"];

                        GBL_Menu_Order_Dict.Add(menuOptionID, sortOrder);
                        GBL_Menu_Name_Dict.Add(menuOptionID, menu_Name);

                    }
                    else if (DDL_Process == "Update")
                    {
                        int menuOptionID = (int)Config_Menu_DataRow.OriginalDataRow["DynDBMenuID"];
                        int orig_sortOrder = (int)Config_Menu_DataRow.OriginalDataRow["SortOrder"];
                        int new_sortOrder = (int)Config_Menu_DataRow.ModifiedDataRow["SortOrder"];
                        string orig_menu_Name = (string)Config_Menu_DataRow.OriginalDataRow["Name"];
                        string new_menu_Name = (string)Config_Menu_DataRow.ModifiedDataRow["Name"];

                        if (orig_sortOrder != new_sortOrder)
                        {
                            GBL_Menu_Order_Dict.XFSetValue(menuOptionID, new_sortOrder);
                        }
                        if (orig_menu_Name != new_menu_Name)
                        {
                            GBL_Menu_Name_Dict.XFSetValue(menuOptionID, new_menu_Name);
                        }
                    }
                    else if (DDL_Process == "Delete")
                    {
                        // TODO: Implement logic to check for duplicate menu options based on DDL_Process value
                    }
                    break;
            }
            var dup_Menu_SortOrders = GBL_Menu_Order_Dict
                                                         .GroupBy(x => x.Value)
                                                         .Where(g => g.Count() > 1)
                                                         .Select(g => g.Key)
                                                         .ToList();
            foreach (var kvp in GBL_Menu_Order_Dict)
            {

            }
            if (dup_Menu_SortOrders.Count > 0)
            {
                Duplicate_Menu_Options_SortOrder = true;
            }
            else
            {
                Duplicate_Menu_Options_SortOrder = false;
            }
            var dup_Menu_Options = GBL_Menu_Name_Dict.GroupBy(x => x.Value)
                                                         .Where(g => g.Count() > 1)
                                                         .Select(g => g.Key)
                                                         .ToList();
            if (dup_Menu_Options.Count > 0)
            {
                Duplicate_Menu_Options = true;
            }
            else
            {
                Duplicate_Menu_Options = false;
            }
        }

        /// <summary>
        /// Checks for duplicate menu header options and sort orders.
        /// This method is used to identify duplicates for menu header options during the save process.
        /// </summary>
        private void Duplicate_Hdr_Check(int wfProfile_ID, String Dup_Process_Step, [Optional] String DDL_Process, [Optional] XFEditedDataRow Config_Menu_DataRow)
        {
            switch (Dup_Process_Step)
            {
                case "Initiate":
                    // Select rows from the table before any updates to rows are processed
                    {
                        var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                        using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                        {
                            var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                            connection.Open();

                            // Create a new DataTable
                            var sqa = new SqlDataAdapter();
                            var DDM_Config_Menu_DT = new DataTable();
                            // Define the select query and parameters
                            var sql = @"SELECT *
						       			FROM DDM_HdrConfigs
						       			WHERE DDM_ConfigID = @DDM_ConfigID";
                            // Create an array of SqlParameter objects
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@DDM_ConfigID", SqlDbType.Int) { Value = wfProfile_ID}
                            };

                            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, DDM_Config_Menu_DT, sql, sqlparams);

                            foreach (DataRow menu_Row in DDM_Config_Menu_DT.Rows)
                            {
                                int menu_ID = (int)menu_Row["DDM_MenuID"];
                                int sortOrder = (int)menu_Row["Order"];
                                string menu_Name = (string)menu_Row["Name"];

                                GBL_Menu_Order_Dict.Add(menu_ID, sortOrder);
                                GBL_Menu_Name_Dict.Add(menu_ID, menu_Name);
                            }
                        }
                    }
                    break;
                case "Update Row":
                    // Check for duplicate menu options when the process step is "Initiate"
                    if (DDL_Process == "Insert")
                    {
                        int menuOptionID = (int)Config_Menu_DataRow.OriginalDataRow["DDM_MenuID"];
                        int sortOrder = (int)Config_Menu_DataRow.ModifiedDataRow["DDM_Menu_Order"];
                        string menu_Name = (string)Config_Menu_DataRow.ModifiedDataRow["Name"];

                        GBL_Menu_Order_Dict.Add(menuOptionID, sortOrder);
                        GBL_Menu_Name_Dict.Add(menuOptionID, menu_Name);

                    }
                    else if (DDL_Process == "Update")
                    {
                        int menuOptionID = (int)Config_Menu_DataRow.OriginalDataRow["DDM_MenuID"];
                        int orig_sortOrder = (int)Config_Menu_DataRow.OriginalDataRow["Order"];
                        int new_sortOrder = (int)Config_Menu_DataRow.ModifiedDataRow["Order"];
                        string orig_menu_Name = (string)Config_Menu_DataRow.OriginalDataRow["Name"];
                        string new_menu_Name = (string)Config_Menu_DataRow.ModifiedDataRow["Name"];

                        if (orig_sortOrder != new_sortOrder)
                        {
                            GBL_Menu_Order_Dict.XFSetValue(menuOptionID, new_sortOrder);
                        }
                        if (orig_menu_Name != new_menu_Name)
                        {
                            GBL_Menu_Name_Dict.XFSetValue(menuOptionID, new_menu_Name);
                        }
                    }
                    else if (DDL_Process == "Delete")
                    {
                        // TODO: Implement logic to check for duplicate menu options based on DDL_Process value
                    }
                    break;
            }
            var dup_Menu_SortOrders = GBL_Menu_Order_Dict
                                                         .GroupBy(x => x.Value)
                                                         .Where(g => g.Count() > 1)
                                                         .Select(g => g.Key)
                                                         .ToList();
            foreach (var kvp in GBL_Menu_Order_Dict)
            {

            }

            if (dup_Menu_SortOrders.Count > 0)
            {
                Duplicate_Menu_Options_SortOrder = true;
            }
            else
            {
                Duplicate_Menu_Options_SortOrder = false;
            }
            var dup_Menu_Options = GBL_Menu_Name_Dict.GroupBy(x => x.Value)
                                                         .Where(g => g.Count() > 1)
                                                         .Select(g => g.Key)
                                                         .ToList();
            if (dup_Menu_Options.Count > 0)
            {
                Duplicate_Menu_Options = true;
            }
            else
            {
                Duplicate_Menu_Options = false;
            }
        }
		#endregion
		
		#region "Validate Columns"
//		public XFSelectionChangedTaskResult Val_DDM_MenuLayoutConfigLayout(ref DataRow row)
//{
//    var valResult = new XFSelectionChangedTaskResult { IsOK = true };

//    try
//    {
//	 BRApi.ErrorLog.LogMessage(si, $"DDM Validation Crash:");
//        // 1. SAFE ENUM PARSING
//        // Don't direct cast. Convert to string first to handle different underlying numeric types.
//        string rawLayoutValue = row["LayoutType"] != DBNull.Value ? row["LayoutType"].ToString() : "0";
//        if (!Enum.TryParse(rawLayoutValue, out DDM_ConfigHelpers.LayoutType layoutType))
//        {
//            throw new Exception($"Value '{rawLayoutValue}' is not a valid LayoutType.");
//        }

//        if (DDM_ConfigHelpers.LayoutRegistry.Configs.TryGetValue(layoutType, out var config))
//        {
//            // 2. BUILD WHITELIST
//            var requiredColumns = config.ParameterMappings.Values
//                .SelectMany(d => d.Values)
//                .ToHashSet();

//            var allLayoutColumns = new List<string> { "T_ContentType" };

//            // 3. PART A: VALIDATE REQUIRED (With Column Existence Check)
//            foreach (var col in requiredColumns)
//            {
//                // Verify column actually exists in the row's table schema
//                if (!row.Table.Columns.Contains(col))
//                {
//                    BRApi.ErrorLog.LogMessage(si, $"Validation Warning: Required column '{col}' missing from DataTable schema.");
//                    continue; 
//                }

//                if (row.IsNull(col) || string.IsNullOrWhiteSpace(row[col].ToString()))
//                {
//                    valResult.IsOK = false;
//                    valResult.Message = $"Validation Error: '{col}' is required for {layoutType}.";
//                    return valResult;
//                }
//            }

//            // 4. PART B: CLEAR OTHER COLUMNS (With Existence Check)
//            foreach (var col in allLayoutColumns)
//            {
//                // Only touch the column if it exists in the schema and isn't in the current whitelist
//                if (row.Table.Columns.Contains(col) && !requiredColumns.Contains(col))
//                {
//                    row[col] = DBNull.Value;
//                }
//            }
//        }
//    }
//    catch (Exception ex)
//    {
//        // CRITICAL: This is the only way to find the line number of a "Silent" failure
//        BRApi.ErrorLog.LogMessage(si, $"DDM Validation Crash: {ex.Message} \n {ex.StackTrace}");
        
//        valResult.IsOK = false;
//        valResult.Message = "Error during row validation: " + ex.Message;
//    }

//    return valResult;
//}
		
		
		
		
		
		public XFSelectionChangedTaskResult Val_DDM_MenuLayoutConfigLayout(ref DataRow row)
		{
		    var valResult = new XFSelectionChangedTaskResult { IsOK = true };
		
		    try
		    {
		        // Parse the Option Type from the row
		       // DDM_ConfigHelpers.LayoutType layoutType = (DDM_ConfigHelpers.LayoutType)row["LayoutType"];
					string layoutValue = row["LayoutType"] != DBNull.Value ? row["LayoutType"].ToString() : "0";
					DDM_ConfigHelpers.LayoutType layoutType;
					
					if (!Enum.TryParse(layoutValue, out layoutType))
					{
					    valResult.IsOK = false;
					    valResult.Message = $"Invalid Layout Type value: {layoutValue}";
					    return valResult;
						}
		        	if (DDM_ConfigHelpers.LayoutRegistry.Configs.TryGetValue(layoutType, out var config))
		        {
		            // 1. Identify "Whitelist" columns (Target DB columns)
		            var requiredColumns = config.ParameterMappings.Values
		                .SelectMany(d => d.Values)
		                .ToHashSet();
		
		            // 2. Define the pool of all potential layout columns to be cleaned if not in whitelist
		            // Add all your DB column names here that relate to specific layouts
		            var allLayoutColumns = new List<string> { 
		               
		            };
		
		            // 3. Part A: Validate Required
		            foreach (var col in requiredColumns)
		            {
		                if (row.IsNull(col) || string.IsNullOrWhiteSpace(row[col].ToString()))
		                {
		                    valResult.IsOK = false;
		                    valResult.Message = $"Validation Error: '{col}' is required for {layoutType}.";
		                    return valResult;
		                }
		            }
		
		            // 4. Part B: Clear other layout columns
		            foreach (var col in allLayoutColumns)
		            {
		                if (!requiredColumns.Contains(col))
		                {
		                    row[col] = DBNull.Value;
		                }
		            }
		        }
		    }
		    catch (Exception ex)
		    {
		        valResult.IsOK = false;
		        valResult.Message = "Error during row validation: " + ex.Message;
		    }
		
		    return valResult;
		}

		public XFSelectionChangedTaskResult Val_DDM_HdrConfigs(ref DataRow row)
		{
		    var valResult = new XFSelectionChangedTaskResult { IsOK = true, ShowMessageBox = true};
		
		    try
		    {
		        // Parse the Option Type from the row
		        DDM_ConfigHelpers.HdrType layoutType = (DDM_ConfigHelpers.HdrType)row["Option_Type"];
		
		        if (DDM_ConfigHelpers.HdrRegistry.Configs.TryGetValue(layoutType, out var config))
		        {
		            // 1. Identify "Whitelist" columns (Target DB columns)
		            var requiredColumns = config.ParameterMappings.Values
		                .SelectMany(d => d.Values)
		                .ToHashSet();
		
		            // 2. Define the pool of all potential layout columns to be cleaned if not in whitelist
		            // Add all your DB column names here that relate to specific layouts
		            var allLayoutColumns = new List<string> { 
		               
		            };
		
		            // 3. Part A: Validate Required
		            foreach (var col in requiredColumns)
		            {
		                if (row.IsNull(col) || string.IsNullOrWhiteSpace(row[col].ToString()))
		                {
		                    valResult.IsOK = false;
		                    valResult.Message = $"Validation Error: '{col}' is required for {layoutType}.";
		                    return valResult;
		                }
		            }
		
		            // 4. Part B: Clear other layout columns
		            foreach (var col in allLayoutColumns)
		            {
		                if (!requiredColumns.Contains(col))
		                {
		                    row[col] = DBNull.Value;
		                }
		            }
		        }
		    }
		    catch (Exception ex)
		    {
		        valResult.IsOK = false;
		        valResult.Message = "Error during row validation: " + ex.Message;
		    }
		
		    return valResult;
		}

		public void Val_Col_Populated(DataRow row, string colName, ref XFSelectionChangedTaskResult result)
		{
		    if (row.IsNull(colName) || string.IsNullOrEmpty(row[colName].ToString()))
		    {
		        // Pseudo-code: add your specific framework's error message
		        result.IsOK = false;
		        result.Message = $"{colName} is required for this layout.";
		    }
		}
		
		#endregion
		#endregion
			private void LogDataTable(DataTable dt, string label)
		{
		    try
		    {
		        var sb = new StringBuilder();
		        var columnList = string.Join(",", dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
		        sb.AppendLine($"{label}: rows={dt.Rows.Count}, cols={columnList}");
		        int idx = 0;
		        foreach (DataRow r in dt.Rows)
		        {
		            var rowValues = dt.Columns.Cast<DataColumn>()
		                .Select(c => $"{c.ColumnName}={FormatValue(r[c])}");
		            sb.AppendLine($"Row {idx} (state={r.RowState}): {string.Join("; ", rowValues)}");
		            idx++;
		        }
		        BRApi.ErrorLog.LogMessage(si, sb.ToString());
		    }
		    catch (Exception ex)
		    {
		        BRApi.ErrorLog.LogMessage(si, $"LogDataTable error: {ex.Message}");
		    }
		}

		private string FormatValue(object value)
		{
		    if (value == null || value == DBNull.Value) return "<null>";
		    if (value is DateTime dt) return dt.ToString("s", CultureInfo.InvariantCulture);
		    return value.ToString();
		}	
        private void UpdateCustomSubstVar(ref XFSelectionChangedTaskResult result, string key, string value)
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
    }
}