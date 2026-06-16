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
using OpenXmlPowerTools;
using Workspace.OSConsTools.GBL_UI_Assembly;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.FMM_ConfigData
{
    public class MainClass
    {
        #region "Global Variables"
        private SessionInfo si;
        private BRGlobals globals;
        private object api;
        private DashboardExtenderArgs args;
        private StringBuilder debugString;
        public string gbl_BalCalc { get; set; } = string.Empty;
        public string gbl_BalBufferCalc { get; set; } = string.Empty;
        public string gbl_UnbalBufferCalc { get; set; } = string.Empty;
        public string gbl_UnbalCalc { get; set; } = string.Empty;
        public string gbl_CalcLogic_Table { get; set; } = string.Empty;
        public int gbl_Table_SrcCell { get; set; }
        public int gbl_SrcCellConfig_Cnt { get; set; }
        public string gbl_ModelType { get; set; } = "Cube";
        public Dictionary<int, string> gbl_SrcCellConfig_Dict { get; set; } = new Dictionary<int, string>();
        public Dictionary<int, string> gbl_UnbalCalc_Dict { get; set; } = new Dictionary<int, string>();
        public Dictionary<int, string> gbl_SrcCellDrill_Dict { get; set; } = new Dictionary<int, string>();
        public Dictionary<string, string> gbl_ActConfig_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> gbl_CalcUnitConfig_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> gbl_UnitConfig_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> gbl_AcctConfig_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> gbl_Appr_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> gbl_ApprStep_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> gbl_Register_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> gbl_Col_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> gbl_Calc_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> gbl_CustTableDef_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> gbl_Models_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> gbl_ModelGrps_Dict { get; set; } = new Dictionary<string, string>();
        public bool gbl_Dup_ActConfig { get; set; } = false;
        public bool gbl_Dup_CalcUnitConfig { get; set; } = false;
        public bool gbl_Dup_UnitConfig { get; set; } = false;
        public bool gbl_Dup_AcctConfig { get; set; } = false;
        public bool gbl_Dup_Approval { get; set; } = false;
        public bool gbl_Dup_ApprStep { get; set; } = false;
        public bool gbl_Dup_RegConfig { get; set; } = false;
        public bool gbl_Dup_ColConfig { get; set; } = false;
        public bool gbl_Dup_CalcConfig { get; set; } = false;
        public bool gbl_Dup_CustTableDef { get; set; } = false;
        public bool gbl_Dup_Models { get; set; } = false;
        public bool gbl_Dup_ModelGrps { get; set; } = false;
        public int gbl_ActConfigID { get; set; }
        public int gbl_CurrActConfigID { get; set; }
        public int gbl_UnitID { get; set; }
        public int gbl_CurrUnitID { get; set; }
        public int gbl_AcctID { get; set; }
        public int gbl_CurrAcctID { get; set; }
        public int gbl_ApprID { get; set; }
        public int gbl_CurrApprID { get; set; }
        public int gbl_ApprStepID { get; set; }
        public int gbl_CurrApprStepID { get; set; }
        public int gbl_RegConfigID { get; set; }
        public int gbl_CurrRegConfigID { get; set; }
        public int gbl_ColID { get; set; }
        public int gbl_CurrColID { get; set; }
        public int gbl_ModelConfigID { get; set; }
        public int gbl_CurrModelConfigID { get; set; }
        public int gbl_CalcConfigID { get; set; }
        public int gbl_CurrCalcConfigID { get; set; }
        public int gbl_DestCellID { get; set; }
        public int gbl_CurrDestCellID { get; set; }
        public int gbl_SrcCellID { get; set; }
        public int gbl_CurrSrcCellID { get; set; }
        public int gbl_ModelGrpID { get; set; }
        public int gbl_CurrModelGrpID { get; set; }
        public int gbl_Model_Grp_Assign_ID { get; set; }
        public int gbl_CurrModel_Grp_Assign_ID { get; set; }
        public int gbl_CalcUnitID { get; set; }
        public int gbl_CurrCalcUnitID { get; set; }
        public int gbl_Calc_Unit_Assign_ID { get; set; }
        public int gbl_CurrCalc_Unit_Assign_ID { get; set; }
        #endregion

        public MainClass()
        {
        }

        public MainClass(SessionInfo si, BRGlobals globals)
        {
            this.si = si;
            this.globals = globals;
        }
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
                        var saveResult = new XFSqlTableEditorSaveDataTaskResult();
                        switch (args.FunctionName)
                        {
                            case var fn when fn.XFEqualsIgnoreCase("ActConfig_Save"):
                                saveResult = ActConfig_Save();
                                return saveResult;

                            case var fn when fn.XFEqualsIgnoreCase("CalcUnitConfig_Save"):
                                saveResult = CalcUnitConfig_Save();
                                return saveResult;

                            case var fn when fn.XFEqualsIgnoreCase("UnitConfig_Save"):
                                saveResult = UnitConfig_Save();
                                return saveResult;

                            case var fn when fn.XFEqualsIgnoreCase("UIColConfig_Save"):
                                saveResult = ColConfig_Save();
                                return saveResult;
                            case var fn when fn.XFEqualsIgnoreCase("CustTableDef_Save"):
                                saveResult = CustTableDef_Save();
                                return saveResult;
                        }
                        break;

                    case DashboardExtenderFunctionType.ComponentSelectionChanged:
                        var changed_Result = new XFSelectionChangedTaskResult();
                        switch (args.FunctionName)
                        {
                            case var fn when fn.XFEqualsIgnoreCase("SolutionTableSetup"):
                                changed_Result = SolutionTableSetup();
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("CubeConfig_SaveAdd"):
                                changed_Result = CubeConfig_Save("Add");
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("CubeConfig_SaveUpdate"):
                                changed_Result = CubeConfig_Save("Update");
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("CubeConfig_SaveDelete"):
                                changed_Result = CubeConfig_Save("Delete");
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("CubeConfig_Add"):
                                changed_Result = CubeConfig_Add();
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("CubeConfig_Select"):
                                changed_Result = CubeConfig_Select();
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("AcctConfig_SaveAdd"):
                                changed_Result = AcctConfig_Save("Add");
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("AcctConfig_Add"):
                                changed_Result = AcctConfig_Add();
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("AcctConfig_Select"):
                                changed_Result = AcctConfig_Select();
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("AcctConfig_SaveUpdate"):
                                changed_Result = AcctConfig_Save("Update");
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("CustTableConfig_SaveAdd"):
                                changed_Result = CustTableConfig_Save("Add");
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("CustTableConfig_SaveUpdate"):
                                changed_Result = CustTableConfig_Save("Update");
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("CustTable_Add"):
                                changed_Result = CustTable_Add();
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("CustTable_Select"):
                                changed_Result = CustTable_Select();
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("CalcConfig_SaveAdd"):
                                gbl_ModelType = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("DL_FMM_CalcType");
                                changed_Result = CalcConfig_Save("Add");
                                if (gbl_ModelType == "Cube")
                                {
                                    EvaluateCalcConfigSetup(gbl_CalcConfigID);
                                }
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("CalcConfig_SaveUpdate"):
                                gbl_ModelType = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("DL_FMM_CalcType");
                                changed_Result = CalcConfig_Save("Update");
                                if (gbl_ModelType == "Cube")
                                {
                                    EvaluateCalcConfigSetup(gbl_CalcConfigID);
                                }
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("DestCell_SaveUpdate"):
                                gbl_ModelType = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("DL_FMM_CalcType");
                                changed_Result = DestCell_Save("Update");
                                if (gbl_ModelType == "Cube")
                                {
                                    EvaluateCalcConfigSetup(gbl_CalcConfigID);
                                }
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Save_SrcCellConfig_Rows"):
                                var src_CellDb = new FMM_SrcCellDB(si);
                                gbl_ModelType = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("DL_FMM_CalcType");
                                changed_Result = Save_SrcCellConfig_Rows();
                                if (gbl_ModelType == "Cube")
                                {
                                    EvaluateCalcConfigSetup(gbl_CalcConfigID);
                                }
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Model_SaveAdd"):
                                changed_Result = Model_Save("Add");
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Model_SaveUpdate"):
                                changed_Result = Model_Save("Update");
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Model_SaveDelete"):
                                changed_Result = Model_Save("Delete");
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Model_Add"):
                                changed_Result = Model_Add();
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Model_Select"):
                                changed_Result = Model_Select();
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("ModelGrp_SaveAdd"):
                                changed_Result = ModelGrp_Save("Add");
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("ModelGrp_SaveUpdate"):
                                changed_Result = ModelGrp_Save("Update");
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("ModelGrpSeqConfig_SaveAdd"):
                                changed_Result = ModelGrpSeqConfig_Save("Add");
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("ModelGrpSeqConfig_SaveUpdate"):
                                changed_Result = ModelGrpSeqConfig_Save("Update");
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Save_New_Model_Grp_Model_Assignments"):
                                //changed_Result = ModelGrp_Assign_Save("Add");
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Save_New_Calc_Unit_Assign"):
                                //changed_Result = Calc_Unit_Assign_Save("Add");
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Save_New_ApprStep"):
                                changed_Result = ApprStep_Save("Add");
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Process_Bulk_Calc_Unit"):
                                changed_Result = Process_Bulk_Calc_Unit();
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Process_Copy_Cube_Config"):
                                Process_Copy_Cube_Config(ref changed_Result);
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Copy_RegConfig"):
                                changed_Result = Process_Bulk_Calc_Unit();
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Process_Copy_ActConfig"):
                                Process_Copy_ActConfig(ref changed_Result);
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Process_Model_Copy"):
                                changed_Result = Process_Bulk_Calc_Unit();
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Process_Calc_Copy"):
                                Process_Calc_Copy(ref changed_Result);
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Copy_Model_Grp_Config"):
                                changed_Result = Process_Bulk_Calc_Unit();
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Select_Add_FMM_CalcConfigID"):
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Select_FMM_CalcConfigID"):
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("SrcCellConfigCube_SaveUpdate"):
                                changed_Result = SrcCellConfigCube_Save("Update");
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("SrcCellConfigCube_SaveDelete"):
                                changed_Result = SrcCellConfigCube_Save("Delete");
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("SrcCellConfigCube_SaveAdd"):
                                changed_Result = SrcCellConfigCube_Save("Add");
                                return changed_Result;
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

        #region "Solution Setup"
        private XFSelectionChangedTaskResult SolutionTableSetup()
        {
            try
            {
                var selectionChangedTaskResult = new XFSelectionChangedTaskResult();

                // Create connection to application database
                using (DbConnInfo dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si))
                {
                    if (!DbSql.DoesTableExist(dbConnApp, "FMM_CubeConfig") &&
                        !DbSql.DoesTableExist(dbConnApp, "FMM_ActConfig") &&
                        !DbSql.DoesTableExist(dbConnApp, "XFW_PLP_ActivityClassItems") &&
                        !DbSql.DoesTableExist(dbConnApp, "XFW_PLP_ActivityTypes") &&
                        !DbSql.DoesTableExist(dbConnApp, "XFW_PLP_ExecutionList") &&
                        !DbSql.DoesTableExist(dbConnApp, "XFW_PLP_Plan") &&
                        !DbSql.DoesTableExist(dbConnApp, "XFW_PLP_Register") &&
                        !DbSql.DoesTableExist(dbConnApp, "XFW_PLP_RegisterDetail") &&
                        !DbSql.DoesTableExist(dbConnApp, "XFW_PLP_ControlListItems") &&
                        !DbSql.DoesTableExist(dbConnApp, "XFW_PLP_GlobalDrivers") &&
                        !DbSql.DoesTableExist(dbConnApp, "XFW_PLP_AuditLog") &&
                        !DbSql.DoesTableExist(dbConnApp, "XFW_PLP_RegisterActions"))
                    {
                        // FIRST TIME INSTALL
                        var setupFileName = "FMM_TableSetup.txt";
                        var fileResource = BRApi.Dashboards.FileResources.GetFileResource(si, false, Guid.Empty, setupFileName);

                        if (fileResource == null)
                        {
                            selectionChangedTaskResult.IsOK = false;
                            selectionChangedTaskResult.ShowMessageBox = true;
                            selectionChangedTaskResult.Message = $"Table Setup File ({setupFileName}) is missing or invalid.";
                            return selectionChangedTaskResult;
                        }

                        // CREATE THE SCHEMA
                        try
                        {
                            string sqlScript = System.Text.Encoding.UTF8.GetString(fileResource.FileBytes);
                            BRApi.Database.ExecuteActionQuery(dbConnApp, sqlScript, false, true);

                            selectionChangedTaskResult.IsOK = true;
                            selectionChangedTaskResult.ShowMessageBox = true;
                            selectionChangedTaskResult.Message = "Solution Tables Created";
                        }
                        catch (Exception)
                        {
                            selectionChangedTaskResult.IsOK = false;
                            selectionChangedTaskResult.ShowMessageBox = true;
                            selectionChangedTaskResult.Message = "Error: Solution Tables NOT Created";
                        }
                    }
                    else
                    {
                        selectionChangedTaskResult.IsOK = true;
                        selectionChangedTaskResult.ShowMessageBox = false;
                    }
                }

                return selectionChangedTaskResult;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #endregion

        #region "Cube Config Functions"
        private XFSelectionChangedTaskResult CubeConfig_Save(string runType)
        {
            try
            {
                var saveResult = new XFSelectionChangedTaskResult();
                var customSubstVars = args.SelectionChangedTaskInfo.CustomSubstVars;
                var customSubstVarsWithValues = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues;

                var Cube = customSubstVarsWithValues.XFGetValue("BL_FMM_CubeConfig_Name", customSubstVars.XFGetValue("IV_FMM_CubeConfig_Name", string.Empty));
                var ScenType = customSubstVars.XFGetValue("BL_FMM_CubeConfig_ScenType", customSubstVars.XFGetValue("IV_FMM_CubeConfig_ScenType", string.Empty));
                var cubeConfigID = customSubstVarsWithValues.XFGetValue("BL_FMM_CubeConfigID", "0").XFConvertToInt();

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (SqlConnection connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();
                    var FMM_CubeConfig_DT = new DataTable();

                    var sql = @"SELECT * 
							    FROM FMM_CubeConfig 
						        WHERE CubeConfigID = @CubeConfigID";
                    var sqlParams = new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = cubeConfigID } };

                    cmdBuilder.FillDataTable(si, sqa, FMM_CubeConfig_DT, sql, sqlParams);

                    DataRow dataRow = null;

                    if (runType == "Add")
                    {
                        dataRow = FMM_CubeConfig_DT.NewRow();
                        dataRow["Status"] = 1;
                        FMM_CubeConfig_DT.Rows.Add(dataRow);
                    }
                    else if (runType == "Update" && FMM_CubeConfig_DT.Rows.Count > 0)
                    {
                        dataRow = FMM_CubeConfig_DT.Rows[0];
                    }
                    else if (runType == "Delete" && FMM_CubeConfig_DT.Rows.Count > 0)
                    {
                        FMM_CubeConfig_DT.Rows[0].Delete();
                        cmdBuilder.UpdateTable(si, "FMM_CubeConfig", FMM_CubeConfig_DT, sqa);
                        saveResult.IsOK = true;
                        saveResult.Message = "Cube Config Deleted Successfully.";
                        saveResult.ShowMessageBox = true;
                        return saveResult;
                    }
                    else
                    {
                        saveResult.IsOK = false;
                        saveResult.Message = runType == "Delete" ? "Could not find the Cube Config record to delete." : "No matching records found.";
                        saveResult.ShowMessageBox = true;
                        return saveResult;
                    }

                    if (dataRow != null)
                    {
                        var saveType = runType == "Add" ? FMM_ConfigHelpers.SaveType.Add : FMM_ConfigHelpers.SaveType.Update;
                        MapConfigValues(ref dataRow, customSubstVars, (int)saveType, "Cube");
                        dataRow["UpdateDate"] = DateTime.Now;
                        dataRow["UpdateUser"] = si.UserName;
                        if (runType == "Add")
                        {
                            dataRow["CreateDate"] = DateTime.Now;
                            dataRow["CreateUser"] = si.UserName;
                        }
                        cmdBuilder.UpdateTable(si, "FMM_CubeConfig", FMM_CubeConfig_DT, sqa);
                        saveResult.IsOK = true;
                        saveResult.Message = runType == "Add" ? "New Cube Config Saved." : "Cube Config Updates Saved.";
                        saveResult.ShowMessageBox = true;
                    }
                }
                return saveResult;
            }
            catch (Exception ex)
            {
                return new XFSelectionChangedTaskResult
                {
                    IsOK = false,
                    Message = $"An error occurred: {ex.Message}",
                    ShowMessageBox = true
                };
            }
        }

        private XFSelectionChangedTaskResult CubeConfig_Add()
        {
            var selectResult = new XFSelectionChangedTaskResult();
            selectResult.ChangeCustomSubstVarsInDashboard = true;
            var SaveTypeintValue = 3;
            var gbl_helpers = new GBL_UI_Assembly.GBL_Helpers();
            gbl_helpers.UpdateCustomSubstVar(ref selectResult, "BL_FMM_CubeConfigID", string.Empty);
            FMM_ConfigHelpers.SaveType saveType = (FMM_ConfigHelpers.SaveType)SaveTypeintValue;
            if (FMM_ConfigHelpers.CubeConfigRegistry.Configs.TryGetValue(saveType, out var config))
            {
                foreach (var step in config.ParameterMappings)
                {
                    foreach (var map in step.Value)
                    {
                        gbl_helpers.UpdateCustomSubstVar(ref selectResult, map.Value, string.Empty);
                    }
                }
            }
            return selectResult;
        }

        private XFSelectionChangedTaskResult CubeConfig_Select()
        {
            var selectResult = new XFSelectionChangedTaskResult();
            selectResult.ChangeCustomSubstVarsInDashboard = true;
            var gbl_helpers = new GBL_UI_Assembly.GBL_Helpers();
            var existingCubeConfigID = this.args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("BL_FMM_CubeConfigID", "0").XFConvertToInt();

            gbl_helpers.UpdateCustomSubstVar(ref selectResult, "BL_FMM_CubeConfigID", existingCubeConfigID.XFToString());
            gbl_helpers.UpdateCustomSubstVar(ref selectResult, "IV_FMM_CubeConfig_AddUpdate", "Update");
            var modifiedVars = selectResult.ModifiedCustomSubstVars;
            FMM_ConfigHelpers.SetCubeConfigParams(si, ref modifiedVars);
            selectResult.ModifiedCustomSubstVars = modifiedVars;

            return selectResult;
        }

        #endregion

        #region "Activity Config Functions"
        private XFSqlTableEditorSaveDataTaskResult ActConfig_Save()
        {
            try
            {
                var saveResult = new XFSqlTableEditorSaveDataTaskResult();
                var saveTaskInfo = args.SqlTableEditorSaveDataTaskInfo;
                var actConfigID = 0;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();
                    var FMM_ActConfig_DT = new DataTable();
                    var cubeConfigID = saveTaskInfo.CustomSubstVars.XFGetValue("BL_FMM_CubeConfigID", "0").XFConvertToInt();
                    Dup_Config("Initiate", "ActConfig", cubeConfigID: cubeConfigID);

                    var sql = @"SELECT * 
                                FROM FMM_ActConfig
                                WHERE CubeConfigID = @CubeConfigID";
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = cubeConfigID }
                    };
                    cmdBuilder.FillDataTable(si, sqa, FMM_ActConfig_DT, sql, sqlparams);

                    foreach (XFEditedDataRow xfRow in saveTaskInfo.EditedDataRows)
                    {
                        if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Insert)
                        {
                            {
                                var new_DataRow = FMM_ActConfig_DT.NewRow();
                                new_DataRow["CubeConfigID"] = (int)xfRow.ModifiedDataRow["CubeConfigID"];
                                new_DataRow["Name"] = (string)xfRow.ModifiedDataRow.Items["Name"];
                                new_DataRow["Register"] = (bool)xfRow.ModifiedDataRow.Items["Register"];
                                new_DataRow["Status"] = 1;
                                new_DataRow["CreateDate"] = DateTime.Now;
                                new_DataRow["CreateUser"] = si.UserName;
                                new_DataRow["UpdateDate"] = DateTime.Now;
                                new_DataRow["UpdateUser"] = si.UserName;
                                FMM_ActConfig_DT.Rows.Add(new_DataRow);
                                Dup_Config("Update Row", "ActConfig", "Insert", modifiedDataRow: xfRow, cubeConfigID: cubeConfigID);
                            }
                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Update)
                        {
                            var rowsToUpdate = FMM_ActConfig_DT.Select($"ActConfigID = {(int)xfRow.ModifiedDataRow["ActConfigID"]}");
                            if (rowsToUpdate.Length > 0)
                            {
                                var rowToUpdate = rowsToUpdate[0];
                                rowToUpdate["Name"] = (string)xfRow.ModifiedDataRow["Name"];
                                rowToUpdate["Register"] = (bool)xfRow.ModifiedDataRow.Items["Register"];
                                rowToUpdate["Status"] = (int)xfRow.ModifiedDataRow["Status"];
                                rowToUpdate["UpdateDate"] = DateTime.Now;
                                rowToUpdate["UpdateUser"] = si.UserName;
                                Dup_Config("Update Row", "ActConfig", "Update", modifiedDataRow: xfRow, cubeConfigID: cubeConfigID);
                            }
                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Delete)
                        {
                            var rowsToDelete = FMM_ActConfig_DT.Select($"ActConfigID = {(int)xfRow.OriginalDataRow["ActConfigID"]}");
                            if (rowsToDelete.Length > 0)
                            {
                                foreach (var row in rowsToDelete)
                                {
                                    row.Delete();
                                    Dup_Config("Update Row", "ActConfig", "Delete", modifiedDataRow: xfRow, cubeConfigID: cubeConfigID);
                                }
                            }
                        }
                    }

                    var dup_ActConfig = gbl_ActConfig_Dict
                                             .GroupBy(x => x.Value)
                                             .Where(g => g.Count() > 1)
                                             .Select(g => g.Key)
                                             .ToList();

                    gbl_Dup_ActConfig = dup_ActConfig.Count > 0;

                    if (gbl_Dup_ActConfig)
                    {
                        saveResult.IsOK = false;
                        saveResult.ShowMessageBox = true;
                        saveResult.Message += "Duplicate Activity Config entries found during the operation.";
                    }
                    else
                    {
                        saveResult.IsOK = true;
                        saveResult.ShowMessageBox = false;
                        cmdBuilder.UpdateTable(si, "FMM_ActConfig", FMM_ActConfig_DT, sqa);
                    }
                }

                saveResult.CancelDefaultSave = true; // Note: Use True if we already saved the data rows in this Business Rule.

                return saveResult;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSelectionChangedTaskResult AcctConfig_Add()
        {
            var selectResult = new XFSelectionChangedTaskResult();
            selectResult.ChangeCustomSubstVarsInDashboard = true;
            var gbl_helpers = new GBL_UI_Assembly.GBL_Helpers();
            gbl_helpers.UpdateCustomSubstVar(ref selectResult, "BL_FMM_ActConfigID_Table", string.Empty);
            return selectResult;
        }

        private XFSelectionChangedTaskResult AcctConfig_Select()
        {
            var selectResult = new XFSelectionChangedTaskResult();
            selectResult.ChangeCustomSubstVarsInDashboard = true;
            var gbl_helpers = new GBL_UI_Assembly.GBL_Helpers();
            var existingActConfigID = this.args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("BL_FMM_ActConfigID_Table", "0").XFConvertToInt();

            gbl_helpers.UpdateCustomSubstVar(ref selectResult, "BL_FMM_ActConfigID_Table", existingActConfigID.XFToString());
            gbl_helpers.UpdateCustomSubstVar(ref selectResult, "IV_FMM_AcctConfig_AddUpdate", "Update");
            gbl_helpers.UpdateCustomSubstVar(ref selectResult, "IV_FMM_ActConfigID", existingActConfigID.XFToString());

            return selectResult;
        }

        private XFSelectionChangedTaskResult AcctConfig_Save(string runType)
        {
            try
            {
                var saveResult = new XFSelectionChangedTaskResult();
                var Cube = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("BL_FMM_All_Cube_Names", args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_CubeConfig_Name", string.Empty));
                var ScenType = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("BL_FMM_CubeConfig_ScenType", args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_CubeConfig_ScenType", string.Empty));

                int new_CubeConfigID = 0;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (SqlConnection connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    connection.Open();

                    var sqa = new SqlDataAdapter();
                    var FMM_CubeConfig_Count_DT = new DataTable();

                    var sql = @"SELECT Count(*) as Count
                                FROM FMM_CubeConfig
                                WHERE Cube = @Cube
                                AND ScenType = @ScenType";

                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@Cube", SqlDbType.NVarChar, 50) { Value = Cube },
                        new SqlParameter("@ScenType", SqlDbType.NVarChar, 20) { Value = ScenType }
                    };

                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_CubeConfig_Count_DT, sql, sqlparams);

                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var FMM_CubeConfig_DT = new DataTable();

                    sql = "SELECT * FROM FMM_CubeConfig WHERE CubeConfigID = @CubeConfigID";
                    sqlparams = new SqlParameter[]
                    {
                    };

                    if (runType == "Add")
                    {
                        new_CubeConfigID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_CubeConfig", "CubeConfigID");

                        sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = new_CubeConfigID }
                        };

                        cmdBuilder.FillDataTable(si, sqa, FMM_CubeConfig_DT, sql, sqlparams);

                        var new_DataRow = FMM_CubeConfig_DT.NewRow();
                        var SaveTypeintValue = 1;
                        FMM_ConfigHelpers.SaveType saveType = (FMM_ConfigHelpers.SaveType)SaveTypeintValue;
                        if (FMM_ConfigHelpers.CubeConfigRegistry.Configs.TryGetValue(saveType, out var config))
                        {
                            foreach (var step in config.ParameterMappings)
                            {
                                foreach (var map in step.Value)
                                {
                                    new_DataRow[map.Value] = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue(map.Key, string.Empty);
                                }
                            }
                        }

                        new_DataRow["CubeConfigID"] = new_CubeConfigID;
                        new_DataRow["Status"] = 1;
                        new_DataRow["CreateDate"] = DateTime.Now;
                        new_DataRow["CreateUser"] = si.UserName;
                        new_DataRow["UpdateDate"] = DateTime.Now;
                        new_DataRow["UpdateUser"] = si.UserName;

                        FMM_CubeConfig_DT.Rows.Add(new_DataRow);
                        cmdBuilder.UpdateTable(si, "FMM_CubeConfig", FMM_CubeConfig_DT, sqa);
                        saveResult.IsOK = true;
                        saveResult.Message = "New Cube Config Saved.";
                        saveResult.ShowMessageBox = true;
                    }
                    else if (Convert.ToInt32(FMM_CubeConfig_Count_DT.Rows[0]["Count"]) > 0 && runType == "Update")
                    {
                        var cubeStatus = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("DL_FMM_Status", string.Empty);
                        new_CubeConfigID = Convert.ToInt32(args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("BL_FMM_CubeConfigID", "0"));
                        sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = new_CubeConfigID }
                        };

                        cmdBuilder.FillDataTable(si, sqa, FMM_CubeConfig_DT, sql, sqlparams);

                        if (FMM_CubeConfig_DT.Rows.Count > 0)
                        {
                            var rowToUpdate = FMM_CubeConfig_DT.Rows[0];
                            var SaveTypeintValue = 2;
                            FMM_ConfigHelpers.SaveType saveType = (FMM_ConfigHelpers.SaveType)SaveTypeintValue;
                            if (FMM_ConfigHelpers.CubeConfigRegistry.Configs.TryGetValue(saveType, out var config))
                            {
                                foreach (var step in config.ParameterMappings)
                                {
                                    foreach (var map in step.Value)
                                    {
                                        rowToUpdate[map.Value] = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue(map.Key, string.Empty);
                                    }
                                }
                            }
                            rowToUpdate["UpdateDate"] = DateTime.Now;
                            rowToUpdate["UpdateUser"] = si.UserName;

                            cmdBuilder.UpdateTable(si, "FMM_CubeConfig", FMM_CubeConfig_DT, sqa);

                            saveResult.IsOK = true;
                            saveResult.Message = "Cube Config Updates Saved.";
                            saveResult.ShowMessageBox = true;
                        }
                    }
                    else if (Convert.ToInt32(FMM_CubeConfig_Count_DT.Rows[0]["Count"]) > 0 && runType == "Add")
                    {
                        saveResult.IsOK = false;
                        saveResult.Message = "Duplicated Cube and Scenario Type, Cube Config not saved.";
                        saveResult.ShowMessageBox = true;
                    }
                }

                return saveResult;
            }
            catch (Exception ex)
            {
                var errorResult = new XFSelectionChangedTaskResult
                {
                    IsOK = false,
                    Message = $"An error occurred: {ex.Message}",
                    ShowMessageBox = true
                };
                return errorResult;
            }
        }
        #endregion

        #region "Custom Table Config Functions"
        private XFSelectionChangedTaskResult CustTableConfig_Save(string runType)
        {
            try
            {
                var saveResult = new XFSelectionChangedTaskResult();
                var gbl_helpers = new GBL_UI_Assembly.GBL_Helpers();
                var uiErrorMessage = string.Empty;
                var uiErrorMessageList = new List<string>();

                var custTableID = 0;

                var sqlparams = new SqlParameter[]
                {
                };

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (SqlConnection connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    connection.Open();

                    var sqa = new SqlDataAdapter();

                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var FMM_CustTableConfig_DT = new DataTable();

                    var sql = @"SELECT * 
						    FROM FMM_CustTable 
							WHERE CustTableID = @CustTableID";

                    if (runType == "Add")
                    {
                        sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@CustTableID", SqlDbType.Int) { Value = custTableID }
                        };

                        cmdBuilder.FillDataTable(si, sqa, FMM_CustTableConfig_DT, sql, sqlparams);

                        var new_DataRow = FMM_CustTableConfig_DT.NewRow();
                        var SaveTypeintValue = 1;
                        FMM_ConfigHelpers.SaveType saveType = (FMM_ConfigHelpers.SaveType)SaveTypeintValue;
                        if (FMM_ConfigHelpers.CustTableConfigRegistry.Configs.TryGetValue(saveType, out var config))
                        {
                            foreach (var step in config.ParameterMappings)
                            {
                                foreach (var map in step.Value)
                                {
                                    var result = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue(map.Key, string.Empty);
                                    BRApi.ErrorLog.LogMessage(si, $"Hit: {map.Value} - {result}");
                                    if (new_DataRow.Table.Columns.Contains(map.Value))
                                    {
                                        var errorMessage = gbl_helpers.checkBlankValue(ref new_DataRow, map.Value, result);
                                        if (errorMessage == "Error")
                                        {
                                            uiErrorMessageList.Add(map.Value);
                                        }
                                        else if (errorMessage == "Success")
                                        {
                                            new_DataRow[map.Value] = result;
                                        }
                                    }
                                }
                            }
                        }

                        new_DataRow["CustTableID"] = custTableID;
                        new_DataRow["Status"] = 1;
                        new_DataRow["CreateDate"] = DateTime.Now;
                        new_DataRow["CreateUser"] = si.UserName;
                        new_DataRow["UpdateDate"] = DateTime.Now;
                        new_DataRow["UpdateUser"] = si.UserName;

                        if (uiErrorMessageList.Count > 0)
                        {
                            uiErrorMessage = "The following fields cannot be blank: " + string.Join(", ", uiErrorMessageList);
                            saveResult.IsOK = false;
                            saveResult.Message = uiErrorMessage;
                            saveResult.ShowMessageBox = true;
                            return saveResult;
                        }
                        foreach (DataColumn col in FMM_CustTableConfig_DT.Columns)
                        {
                            string colName = col.ColumnName;
                            string colType = col.DataType.Name;

                            // 3. Retrieve the value safely
                            var value = new_DataRow[colName];

                            // 4. Handle Nulls for the log display
                            string displayValue = (value == null || value == DBNull.Value) ? "[NULL / DBNull]" : value.ToString();

                            // 5. Write to OneStream Error Log
                            BRApi.ErrorLog.LogMessage(si, $"COLUMN: {colName.PadRight(20)} | TYPE: {colType.PadRight(10)} | VALUE: {displayValue}");
                        }
                        FMM_CustTableConfig_DT.Rows.Add(new_DataRow);
                        cmdBuilder.UpdateTable(si, "FMM_CustTable", FMM_CustTableConfig_DT, sqa);
                        saveResult.IsOK = true;
                        saveResult.Message = "New Cube Config Saved.";
                        saveResult.ShowMessageBox = true;
                    }
                    else if (runType == "Update")
                    {
                        custTableID = Convert.ToInt32(args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_custTableID", "0"));
                        sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@custTableID", SqlDbType.Int) { Value = custTableID }
                        };

                        cmdBuilder.FillDataTable(si, sqa, FMM_CustTableConfig_DT, sql, sqlparams);

                        if (FMM_CustTableConfig_DT.Rows.Count > 0)
                        {
                            var rowToUpdate = FMM_CustTableConfig_DT.Rows[0];
                            var SaveTypeintValue = 2;
                            FMM_ConfigHelpers.SaveType saveType = (FMM_ConfigHelpers.SaveType)SaveTypeintValue;
                            if (FMM_ConfigHelpers.CustTableConfigRegistry.Configs.TryGetValue(saveType, out var config))
                            {
                                foreach (var step in config.ParameterMappings)
                                {
                                    foreach (var map in step.Value)
                                    {
                                        rowToUpdate[map.Value] = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue(map.Key, string.Empty);
                                    }
                                }
                            }
                            rowToUpdate["UpdateDate"] = DateTime.Now;
                            rowToUpdate["UpdateUser"] = si.UserName;

                            cmdBuilder.UpdateTable(si, "FMM_CustTable", FMM_CustTableConfig_DT, sqa);

                            saveResult.IsOK = true;
                            saveResult.Message = "Cube Config Updates Saved.";
                            saveResult.ShowMessageBox = true;
                        }
                    }
                    else if (runType == "Delete")
                    {
                        custTableID = Convert.ToInt32(args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_custTableID", "0"));
                        sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@custTableID", SqlDbType.Int) { Value = custTableID }
                        };

                        cmdBuilder.FillDataTable(si, sqa, FMM_CustTableConfig_DT, sql, sqlparams);

                        if (FMM_CustTableConfig_DT.Rows.Count > 0)
                        {
                            // Mark the row as deleted
                            FMM_CustTableConfig_DT.Rows[0].Delete();

                            // Commit the deletion to the database
                            cmdBuilder.UpdateTable(si, "FMM_CustTable", FMM_CustTableConfig_DT, sqa);

                            saveResult.IsOK = true;
                            saveResult.Message = "Cube Config Deleted Successfully.";
                            saveResult.ShowMessageBox = true;
                        }
                        else
                        {
                            saveResult.IsOK = false;
                            saveResult.Message = "Could not find the Cube Config record to delete.";
                            saveResult.ShowMessageBox = true;
                        }
                    }
                }

                return saveResult;
            }
            catch (Exception ex)
            {
                return new XFSelectionChangedTaskResult
                {
                    IsOK = false,
                    Message = $"An error occurred: {ex.Message}",
                    ShowMessageBox = true
                };
            }
        }

        private XFSelectionChangedTaskResult CustTable_Add()
        {
            try
            {
                var selectResult = new XFSelectionChangedTaskResult();
                selectResult.ChangeCustomSubstVarsInDashboard = true;
                var SaveTypeintValue = 3;
                var gbl_helpers = new GBL_UI_Assembly.GBL_Helpers();
                gbl_helpers.UpdateCustomSubstVar(ref selectResult, "BL_FMM_CustTableConfigID", string.Empty);
                FMM_ConfigHelpers.SaveType saveType = (FMM_ConfigHelpers.SaveType)SaveTypeintValue;

                if (FMM_ConfigHelpers.CustTableConfigRegistry.Configs.TryGetValue(saveType, out var config))
                {
                    foreach (var step in config.ParameterMappings)
                    {
                        foreach (var map in step.Value)
                        {
                            gbl_helpers.UpdateCustomSubstVar(ref selectResult, map.Value, string.Empty);
                        }
                    }
                }
                return selectResult;
            }
            catch (Exception ex)
            {
                // Log and rethrow exceptions for error handling.
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSelectionChangedTaskResult CustTable_Select()
        {
            var selectResult = new XFSelectionChangedTaskResult();
            selectResult.ChangeCustomSubstVarsInDashboard = true;
            var gbl_helpers = new GBL_UI_Assembly.GBL_Helpers();
            var existingCustTableID = this.args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("BL_FMM_CustTableConfigID", "0").XFConvertToInt();
            gbl_helpers.UpdateCustomSubstVar(ref selectResult, "BL_FMM_CustTableConfigID", existingCustTableID.XFToString());
            gbl_helpers.UpdateCustomSubstVar(ref selectResult, "IV_FMM_CustTableConfig_AddUpdate", "Update");
            var modifiedVars = selectResult.ModifiedCustomSubstVars;
            FMM_ConfigHelpers.SetCustTableConfigParams(si, ref modifiedVars);
            selectResult.ModifiedCustomSubstVars = modifiedVars;

            return selectResult;
        }

        private XFSqlTableEditorSaveDataTaskResult CustTableDef_Save()
        {
            try
            {
                var saveResult = new XFSqlTableEditorSaveDataTaskResult();
                var custTableConfigID = args.SqlTableEditorSaveDataTaskInfo.CustomSubstVars.XFGetValue("BL_FMM_CustTableConfigID", "0").XFConvertToInt();

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();
                    var custTableDefDt = new DataTable();
                    Dup_Config("Initiate", "CustTableColConfig", custTableConfigID: custTableConfigID);

                    var sql = @"SELECT *
                                FROM FMM_CustTableDef
                                WHERE CustTableID = @CustTableID";
                    var sqlparams = new[]
                    {
                        new SqlParameter("@CustTableID", SqlDbType.Int) { Value = custTableConfigID }
                    };

                    cmdBuilder.FillDataTable(si, sqa, custTableDefDt, sql, sqlparams);

                    if (custTableDefDt.Columns.Contains("CustTableColID"))
                    {
                        custTableDefDt.PrimaryKey = new[] { custTableDefDt.Columns["CustTableColID"]! };
                    }

                    foreach (XFEditedDataRow xfRow in args.SqlTableEditorSaveDataTaskInfo.EditedDataRows)
                    {
                        if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Insert)
                        {
                            var newDataRow = custTableDefDt.NewRow();
                            newDataRow["CustTableID"] = custTableConfigID;
                            newDataRow["ColOrder"] = xfRow.ModifiedDataRow["ColOrder"];
                            newDataRow["Name"] = xfRow.ModifiedDataRow["Name"];
                            newDataRow["PrimaryKey"] = xfRow.ModifiedDataRow["PrimaryKey"];
                            newDataRow["DataType"] = xfRow.ModifiedDataRow["DataType"];
                            newDataRow["MaxLength"] = xfRow.ModifiedDataRow["MaxLength"];
                            newDataRow["Precision"] = xfRow.ModifiedDataRow["Precision"];
                            newDataRow["Scale"] = xfRow.ModifiedDataRow["Scale"];
                            newDataRow["IsNullable"] = xfRow.ModifiedDataRow["IsNullable"];
                            newDataRow["DefaultValue"] = xfRow.ModifiedDataRow["DefaultValue"];
                            newDataRow["Descr"] = xfRow.ModifiedDataRow["Descr"];
                            newDataRow["Status"] = xfRow.ModifiedDataRow["Status"] != DBNull.Value
                                ? xfRow.ModifiedDataRow["Status"]
                                : 1;
                            newDataRow["CreateDate"] = DateTime.Now;
                            newDataRow["CreateUser"] = si.UserName;
                            newDataRow["UpdateDate"] = DateTime.Now;
                            newDataRow["UpdateUser"] = si.UserName;
                            custTableDefDt.Rows.Add(newDataRow);
                            Dup_Config("Update Row", "CustTableDef", "Insert", modifiedDataRow: xfRow, custTableConfigID: custTableConfigID);
                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Update)
                        {
                            if (xfRow.ModifiedDataRow["CustTableColID"] == DBNull.Value)
                            {
                                continue;
                            }

                            var custTableColID = Convert.ToInt32(xfRow.ModifiedDataRow["CustTableColID"]);

                            var rowsToUpdate = custTableDefDt.Select($"CustTableColID = {custTableColID}");
                            if (rowsToUpdate.Length > 0)
                            {
                                var rowToUpdate = rowsToUpdate[0];
                                rowToUpdate["ColOrder"] = xfRow.ModifiedDataRow["ColOrder"];
                                rowToUpdate["Name"] = xfRow.ModifiedDataRow["Name"];
                                rowToUpdate["PrimaryKey"] = xfRow.ModifiedDataRow["PrimaryKey"];
                                rowToUpdate["DataType"] = xfRow.ModifiedDataRow["DataType"];
                                rowToUpdate["MaxLength"] = xfRow.ModifiedDataRow["MaxLength"];
                                rowToUpdate["Precision"] = xfRow.ModifiedDataRow["Precision"];
                                rowToUpdate["Scale"] = xfRow.ModifiedDataRow["Scale"];
                                rowToUpdate["IsNullable"] = xfRow.ModifiedDataRow["IsNullable"];
                                rowToUpdate["DefaultValue"] = xfRow.ModifiedDataRow["DefaultValue"];
                                rowToUpdate["Descr"] = xfRow.ModifiedDataRow["Descr"];
                                if (xfRow.ModifiedDataRow["Status"] != DBNull.Value)
                                {
                                    rowToUpdate["Status"] = xfRow.ModifiedDataRow["Status"];
                                }
                                rowToUpdate["UpdateDate"] = DateTime.Now;
                                rowToUpdate["UpdateUser"] = si.UserName;
                                Dup_Config("Update Row", "CustTableDef", "Update", modifiedDataRow: xfRow, custTableConfigID: custTableConfigID);
                            }
                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Delete)
                        {
                            if (xfRow.OriginalDataRow["CustTableColID"] == DBNull.Value)
                            {
                                continue;
                            }

                            var custTableColID = Convert.ToInt32(xfRow.OriginalDataRow["CustTableColID"]);

                            var rowsToDelete = custTableDefDt.Select($"CustTableColID = {custTableColID}");
                            if (rowsToDelete.Length > 0)
                            {
                                foreach (var row in rowsToDelete)
                                {
                                    row.Delete();
                                    Dup_Config("Update Row", "CustTableDef", "Delete", modifiedDataRow: xfRow, custTableConfigID: custTableConfigID);
                                }
                            }
                        }
                    }

                    var dupCustTableDef = gbl_CustTableDef_Dict
                        .GroupBy(x => x.Value)
                        .Where(g => g.Count() > 1)
                        .Select(g => g.Key)
                        .ToList();

                    gbl_Dup_CustTableDef = dupCustTableDef.Count > 0;

                    if (gbl_Dup_CustTableDef)
                    {
                        saveResult.IsOK = false;
                        saveResult.ShowMessageBox = true;
                        saveResult.Message += "Duplicate custom table column definitions found during the operation.";
                    }
                    else
                    {
                        cmdBuilder.UpdateTable(si, "FMM_CustTableDef", custTableDefDt, sqa);
                        saveResult.IsOK = true;
                        saveResult.ShowMessageBox = false;
                    }
                }

                saveResult.CancelDefaultSave = true;
                return saveResult;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
        #endregion

        #region "Model Config Functions"

        private XFSelectionChangedTaskResult Model_Add()
        {
            var selectResult = new XFSelectionChangedTaskResult();
            selectResult.ChangeCustomSubstVarsInDashboard = true;
            var SaveTypeintValue = 3;
            var gbl_helpers = new GBL_UI_Assembly.GBL_Helpers();
            gbl_helpers.UpdateCustomSubstVar(ref selectResult, "BL_FMM_ModelConfigID", string.Empty);
            FMM_ConfigHelpers.SaveType saveType = (FMM_ConfigHelpers.SaveType)SaveTypeintValue;
            if (FMM_ConfigHelpers.ModelConfigRegistry.Configs.TryGetValue(saveType, out var config))
            {
                foreach (var step in config.ParameterMappings)
                {
                    foreach (var map in step.Value)
                    {
                        gbl_helpers.UpdateCustomSubstVar(ref selectResult, map.Value, string.Empty);
                    }
                }
            }
            return selectResult;
        }

        private XFSelectionChangedTaskResult Model_Select()
        {
            var selectResult = new XFSelectionChangedTaskResult();
            selectResult.ChangeCustomSubstVarsInDashboard = true;
            var gbl_helpers = new GBL_UI_Assembly.GBL_Helpers();
            var existingModelConfigID = this.args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("BL_FMM_ModelConfigID", "0").XFConvertToInt();

            gbl_helpers.UpdateCustomSubstVar(ref selectResult, "BL_FMM_ModelConfigID", existingModelConfigID.XFToString());
            gbl_helpers.UpdateCustomSubstVar(ref selectResult, "IV_FMM_ModelConfig_AddUpdate", "Update");
            var modifiedVars = selectResult.ModifiedCustomSubstVars;
            FMM_ConfigHelpers.SetModelConfigParams(si, ref modifiedVars);
            selectResult.ModifiedCustomSubstVars = modifiedVars;

            return selectResult;
        }

        private XFSelectionChangedTaskResult Model_Save(string runType)
        {
            try
            {
                var saveResult = new XFSelectionChangedTaskResult();
                var customSubstVars = args.SelectionChangedTaskInfo.CustomSubstVars;
                var customSubstVarsWithValues = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues;

                var actConfigID = Convert.ToInt32(customSubstVars.XFGetValue("IV_FMM_ActConfigID", "0"));
                var cubeConfigID = customSubstVars.XFGetValue("BL_FMM_CubeConfigID", "0").XFConvertToInt();
                var modelConfigID = customSubstVarsWithValues.XFGetValue("BL_FMM_ModelConfigID", "0").XFConvertToInt();
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (SqlConnection connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();
                    var FMM_ModelConfig_DT = new DataTable();

                    Dup_Config("Initiate", "ModelConfig", cubeConfigID: cubeConfigID, actConfigID: actConfigID);

                    var sql = @"SELECT *
                                FROM FMM_ModelConfig
                                WHERE ModelConfigID = @ModelConfigID";
                    var sqlParams = new SqlParameter[]
                    {
                        new SqlParameter("@ModelConfigID", SqlDbType.Int) {Value = modelConfigID}
                    };

                    cmdBuilder.FillDataTable(si, sqa, FMM_ModelConfig_DT, sql, sqlParams);

                    DataRow dataRow = null;
                    DataRow originalDataRow = null;

                    if (runType == "Add")
                    {
                        dataRow = FMM_ModelConfig_DT.NewRow();
                        dataRow["CubeConfigID"] = cubeConfigID;
                        dataRow["ActConfigID"] = actConfigID;
                        dataRow["Status"] = 1;
                        FMM_ModelConfig_DT.Rows.Add(dataRow);
                    }
                    else if (runType == "Update")
                    {
                        dataRow = FMM_ModelConfig_DT.Rows[0];
                        originalDataRow = FMM_ModelConfig_DT.NewRow();
                        originalDataRow.ItemArray = (object[])dataRow.ItemArray.Clone();
                    }
                    else if (runType == "Delete")
                    {
                        dataRow = FMM_ModelConfig_DT.Rows[0];
                        Dup_Config("Update Row", "ModelConfig", "Delete", row: dataRow, cubeConfigID: cubeConfigID, actConfigID: actConfigID);
                        dataRow.Delete();
                        cmdBuilder.UpdateTable(si, "FMM_ModelConfig", FMM_ModelConfig_DT, sqa);
                        saveResult.IsOK = true;
                        saveResult.Message = "Model Deleted Successfully.";
                        saveResult.ShowMessageBox = true;
                        return saveResult;
                    }

                    if (dataRow != null)
                    {
                        var saveTypeValue = runType == "Add" ? 1 : 2;
                        MapConfigValues(ref dataRow, customSubstVars, saveTypeValue, "Model");
                        dataRow["UpdateDate"] = DateTime.Now;
                        dataRow["UpdateUser"] = si.UserName;

                        if (runType == "Add")
                        {
                            dataRow["CreateDate"] = DateTime.Now;
                            dataRow["CreateUser"] = si.UserName;
                            Dup_Config("Update Row", "Model", "Insert", row: dataRow, cubeConfigID: cubeConfigID, actConfigID: actConfigID);
                        }
                        else
                        {
                            Dup_Config("Update Row", "Model", "Update", row: dataRow, originalRow: originalDataRow, cubeConfigID: cubeConfigID, actConfigID: actConfigID);
                        }

                        var dupModels = gbl_Models_Dict
                            .GroupBy(x => x.Value)
                            .Where(g => g.Count() > 1)
                            .Select(g => g.Key)
                            .ToList();

                        gbl_Dup_Models = dupModels.Count > 0;

                        if (gbl_Dup_Models)
                        {
                            saveResult.IsOK = false;
                            saveResult.Message = "Duplicate Model entries found during the operation.";
                            saveResult.ShowMessageBox = true;
                            return saveResult;
                        }

                        cmdBuilder.UpdateTable(si, "FMM_ModelConfig", FMM_ModelConfig_DT, sqa);
                        saveResult.IsOK = true;
                        saveResult.Message = runType == "Add" ? "New Model Saved." : "Model Updates Saved.";
                        saveResult.ShowMessageBox = true;
                    }
                }

                return saveResult;
            }
            catch (Exception ex)
            {
                return new XFSelectionChangedTaskResult
                {
                    IsOK = false,
                    Message = $"An error occurred: {ex.Message}",
                    ShowMessageBox = true
                };
            }
        }
        #endregion
        #region "Custom Table Assignment Functions"
        #endregion
        #region "Save TED Inputs"

        private XFSqlTableEditorSaveDataTaskResult CalcUnitConfig_Save()
        {
            try
            {
                var saveResult = new XFSqlTableEditorSaveDataTaskResult();
                var saveTaskInfo = args.SqlTableEditorSaveDataTaskInfo;
                var calcUnitID = 0;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();
                    var FMM_CalcUnitConfig_DT = new DataTable();
                    var cubeConfigID = saveTaskInfo.CustomSubstVars.XFGetValue("BL_FMM_CubeConfigID", "0").XFConvertToInt();
                    Dup_Config("Initiate", "CalcUnitConfig", cubeConfigID: cubeConfigID);

                    var sql = @"SELECT * 
                                FROM FMM_CalcUnitConfig
                                WHERE CubeConfigID = @CubeConfigID";
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = cubeConfigID }
                    };
                    cmdBuilder.FillDataTable(si, sqa, FMM_CalcUnitConfig_DT, sql, sqlparams);

                    foreach (XFEditedDataRow xfRow in saveTaskInfo.EditedDataRows)
                    {
                        if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Insert)
                        {
                            var new_DataRow = FMM_CalcUnitConfig_DT.NewRow();
                            new_DataRow["CubeConfigID"] = (int)xfRow.ModifiedDataRow["CubeConfigID"];
                            new_DataRow["Entity_MFB"] = (string)xfRow.ModifiedDataRow.Items["Entity_MFB"];
                            new_DataRow["WFChannel"] = (string)xfRow.ModifiedDataRow.Items["WFChannel"];
                            new_DataRow["Status"] = "Build";
                            new_DataRow["CreateDate"] = DateTime.Now;
                            new_DataRow["CreateUser"] = si.UserName;
                            new_DataRow["UpdateDate"] = DateTime.Now;
                            new_DataRow["UpdateUser"] = si.UserName;
                            FMM_CalcUnitConfig_DT.Rows.Add(new_DataRow);
                            Dup_Config("Update Row", "CalcUnitConfig", "Insert", modifiedDataRow: xfRow, cubeConfigID: cubeConfigID);
                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Update)
                        {
                            var rowsToUpdate = FMM_CalcUnitConfig_DT.Select($"CalcUnitID = {(int)xfRow.ModifiedDataRow["CalcUnitID"]}");
                            if (rowsToUpdate.Length > 0)
                            {
                                var rowToUpdate = rowsToUpdate[0];
                                rowToUpdate["Entity_MFB"] = (string)xfRow.ModifiedDataRow.Items["Entity_MFB"];
                                rowToUpdate["WFChannel"] = (string)xfRow.ModifiedDataRow.Items["WFChannel"];
                                rowToUpdate["UpdateDate"] = DateTime.Now;
                                rowToUpdate["UpdateUser"] = si.UserName;
                                Dup_Config("Update Row", "CalcUnitConfig", "Update", modifiedDataRow: xfRow, cubeConfigID: cubeConfigID);
                            }
                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Delete)
                        {
                            var rowsToDelete = FMM_CalcUnitConfig_DT.Select($"CalcUnitID = {(int)xfRow.OriginalDataRow["CalcUnitID"]}");

                            if (rowsToDelete.Length > 0)
                            {
                                foreach (var row in rowsToDelete)
                                {
                                    Dup_Config("Update Row", "CalcUnitConfig", "Delete", modifiedDataRow: xfRow, cubeConfigID: cubeConfigID);
                                    row.Delete();
                                }
                            }
                        }
                    }
                    var dup_CalcUnitConfig = gbl_CalcUnitConfig_Dict
                                                 .GroupBy(x => x.Value)
                                                 .Where(g => g.Count() > 1)
                                                 .Select(g => g.Key)
                                                 .ToList();

                    gbl_Dup_CalcUnitConfig = dup_CalcUnitConfig.Count > 0;

                    if (gbl_Dup_CalcUnitConfig)
                    {
                        saveResult.IsOK = false;
                        saveResult.ShowMessageBox = true;
                        saveResult.Message += "Duplicate WF DU Config entries found during the operation.";
                    }
                    else
                    {
                        saveResult.IsOK = true;
                        saveResult.ShowMessageBox = false;

                        cmdBuilder.UpdateTable(si, "FMM_CalcUnitConfig", FMM_CalcUnitConfig_DT, sqa);
                    }
                }

                saveResult.CancelDefaultSave = true; // Note: Use True if we already saved the data rows in this Business Rule.

                return saveResult;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSelectionChangedTaskResult CalcConfig_Save(string runType)
        {
            try
            {
                var saveResult = new XFSelectionChangedTaskResult();
                var customSubstVarsWithValues = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues;

                var createNewDestCell = false;
                var cubeConfigID = customSubstVarsWithValues.XFGetValue("BL_FMM_CubeConfigID", "0").XFConvertToInt();
                var actConfigID = customSubstVarsWithValues.XFGetValue("IV_FMM_ActConfigID", "0").XFConvertToInt();
                var modelConfigID = customSubstVarsWithValues.XFGetValue("IV_FMM_ModelConfigID", "0").XFConvertToInt();
                var calcConfigID = customSubstVarsWithValues.XFGetValue("BL_FMM_CalcConfigs", "0").XFConvertToInt();

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();

                    var FMM_CalcConfig_DT = new DataTable();
                    var FMM_DestCell_DT = new DataTable();
                    //  Dup_Config("Initiate", "CalcConfig", ref saveResult, cubeConfigID: cubeConfigID, actConfigID: actConfigID, ModelConfigID: ModelConfigID);

                    var sql = @"SELECT * 
                                FROM FMM_CalcConfig 
                                WHERE CubeConfigID = @CubeConfigID 
                                AND ActConfigID = @ActConfigID 
                                AND ModelConfigID = @ModelConfigID";

                    var sqlparams = new[]
                    {
                    new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = cubeConfigID },
                    new SqlParameter("@ActConfigID", SqlDbType.Int) { Value = actConfigID },
                    new SqlParameter("@ModelConfigID", SqlDbType.Int) { Value = modelConfigID }
                    };

                    cmdBuilder.FillDataTable(si, sqa, FMM_CalcConfig_DT, sql, sqlparams);
                    FMM_CalcConfig_DT.PrimaryKey = new DataColumn[] { FMM_CalcConfig_DT.Columns["CalcConfigID"]! };

                    if (runType == "Add")
                    {
                        var sql_gbl_get_max_id = new SQL_GBL_Get_Max_ID(si, connection);

                        gbl_CalcConfigID = calcConfigID;

                        var new_config_Row = FMM_CalcConfig_DT.NewRow();

                        new_config_Row["Status"] = "Build";
                        new_config_Row["CreateDate"] = DateTime.Now;
                        new_config_Row["CreateUser"] = si.UserName;
                        new_config_Row["UpdateDate"] = DateTime.Now;
                        new_config_Row["UpdateUser"] = si.UserName;

                        FMM_CalcConfig_DT.Rows.Add(new_config_Row);
                        //  Dup_Config("Update Row", "CalcConfig", "Insert", row: new_config_Row, cubeConfigID: cubeConfigID, actConfigID: actConfigID, ModelConfigID: ModelConfigID);

                        var DestCell_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_DestCell", "DestCell_ID");

                        sql = @"SELECT * 
                            FROM FMM_DestCell 
                            WHERE CalcConfigID = @CalcConfigID 
                            AND DestCell_ID = @DestCell_ID";

                        sqlparams = new[]
                        {
                            new SqlParameter("@CalcConfigID", SqlDbType.Int) { Value = calcConfigID },
                            new SqlParameter("@DestCell_ID", SqlDbType.Int) { Value = DestCell_ID }
                        };

                        cmdBuilder.FillDataTable(si, sqa, FMM_DestCell_DT, sql, sqlparams);

                        var new_Row = FMM_DestCell_DT.NewRow();
                        new_Row["CubeConfigID"] = cubeConfigID;
                        new_Row["ActConfigID"] = actConfigID;
                        new_Row["ModelConfigID"] = modelConfigID;
                        new_Row["CalcConfigID"] = calcConfigID;
                        new_Row["DestCell_ID"] = DestCell_ID;

                        FMM_DestCell_DT.Rows.Add(new_Row);
                    }
                    else
                    {
                        calcConfigID = customSubstVarsWithValues.XFGetValue("BL_FMM_CalcConfigs", "0").XFConvertToInt();
                        var rowsToUpdate = FMM_CalcConfig_DT.Select($"CalcConfigID = {calcConfigID}");
                        if (rowsToUpdate.Length > 0)
                        {
                            var rowToUpdate = rowsToUpdate[0];
                            gbl_CalcConfigID = rowToUpdate["CalcConfigID"] != DBNull.Value ? Convert.ToInt32(rowToUpdate["CalcConfigID"]) : 0;

                            rowToUpdate["UpdateDate"] = DateTime.Now;
                            rowToUpdate["UpdateUser"] = si.UserName;
                            //Dup_Config("Update Row", "CalcConfig", "Update", row: rowToUpdate, cubeConfigID: cubeConfigID, actConfigID: actConfigID, ModelConfigID: ModelConfigID);
                        }
                    }

                    var dup_CalcConfig = gbl_Calc_Dict
                    .GroupBy(x => x.Value)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                    gbl_Dup_CalcConfig = dup_CalcConfig.Count > 0;

                    if (gbl_Dup_CalcConfig)
                    {
                        saveResult.IsOK = false;
                        saveResult.ShowMessageBox = true;
                        saveResult.Message += "Duplicate Unit Config entries found during the operation.";
                    }
                    else
                    {
                        saveResult.IsOK = true;
                        saveResult.ShowMessageBox = false;

                        var calcTypeValue = customSubstVarsWithValues.XFGetValue("DL_FMM_CalcType", "0").XFConvertToInt();
                        var rowToMap = FMM_CalcConfig_DT.Rows.Find(calcConfigID);
                        if (rowToMap != null)
                        {
                            var calcType = (FMM_ConfigHelpers.CalcType)calcTypeValue;
                            if (FMM_ConfigHelpers.CalcRegistry.Configs.TryGetValue(calcType, out var calcConfig))
                            {
                                foreach (var group in calcConfig.ParameterMappings.Values)
                                {
                                    foreach (var mapping in group)
                                    {
                                        var sourceSubstVar = mapping.Key;
                                        var targetCol = mapping.Value;
                                        rowToMap[targetCol] = customSubstVarsWithValues.XFGetValue(sourceSubstVar, string.Empty);
                                    }
                                }
                            }
                        }

                        cmdBuilder.UpdateTable(si, "FMM_CalcConfig", FMM_CalcConfig_DT, sqa);

                        if (createNewDestCell)
                        {
                            cmdBuilder.UpdateTable(si, "FMM_DestCell", FMM_DestCell_DT, sqa);
                        }
                    }
                }

                return saveResult;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
        private XFSelectionChangedTaskResult DestCell_Save(string runType)
        {
            try
            {
                var saveResult = new XFSelectionChangedTaskResult();
                var saveTaskInfo = args.SelectionChangedTaskInfo;

                gbl_CalcConfigID = saveTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_CalcConfigID", "0").XFConvertToInt();
                var CubeConfigID = saveTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("BL_FMM_CubeConfigID", "0").XFConvertToInt();
                var ActConfigID = saveTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_ActConfigID", "0").XFConvertToInt();
                var ModelConfigID = saveTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_ModelConfigID", "0").XFConvertToInt();

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();
                    var fmmDestCellDt = new DataTable();

                    var sql = @"SELECT * 
                    FROM FMM_DestCell 
                    WHERE CalcConfigID = @CalcConfigID";
                    var sqlparams = new[]
                    {
                new SqlParameter("@CalcConfigID", SqlDbType.Int) { Value = gbl_CalcConfigID }
                };

                    cmdBuilder.FillDataTable(si, sqa, fmmDestCellDt, sql, sqlparams);
                    fmmDestCellDt.PrimaryKey = new[] { fmmDestCellDt.Columns["DestCell_ID"]! };

                    var destRow = fmmDestCellDt.Rows.Cast<DataRow>().FirstOrDefault();
                    if (destRow == null)
                    {
                        saveResult.IsOK = false;
                        saveResult.ShowMessageBox = true;
                        saveResult.Message = "Destination cell record not found for update.";
                        return saveResult;
                    }

                    destRow["UpdateDate"] = DateTime.Now;
                    destRow["UpdateUser"] = si.UserName;

                    cmdBuilder.UpdateTable(si, "FMM_DestCell", fmmDestCellDt, sqa);
                }

                saveResult.IsOK = true;
                saveResult.ShowMessageBox = false;
                return saveResult;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
        private XFSelectionChangedTaskResult Save_SrcCellConfig_Rows()
        {
            try
            {
                var saveResult = new XFSelectionChangedTaskResult();
                var saveTaskInfo = args.SelectionChangedTaskInfo;

                gbl_CalcConfigID = saveTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_CalcConfigID", "0").XFConvertToInt();
                var CubeConfigID = saveTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("BL_FMM_CubeConfigID", "0").XFConvertToInt();
                var ActConfigID = saveTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_ActConfigID", "0").XFConvertToInt();
                var ModelConfigID = saveTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_ModelConfigID", "0").XFConvertToInt();

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();
                    var fmmSrcCellDt = new DataTable();

                    var sql = @"SELECT * 
                    FROM FMM_SrcCell 
                    WHERE CalcConfigID = @CalcConfigID";
                    var sqlparams = new[]
                    {
                new SqlParameter("@CalcConfigID", SqlDbType.Int) { Value = gbl_CalcConfigID }
                };

                    cmdBuilder.FillDataTable(si, sqa, fmmSrcCellDt, sql, sqlparams);
                    fmmSrcCellDt.PrimaryKey = new[] { fmmSrcCellDt.Columns["SrcCellConfig_ID"]! };

                    if (fmmSrcCellDt.Rows.Count == 0)
                    {
                        saveResult.IsOK = false;
                        saveResult.ShowMessageBox = true;
                        saveResult.Message = "Source cell records not found for update.";
                        return saveResult;
                    }

                    foreach (DataRow row in fmmSrcCellDt.Rows)
                    {
                        row["UpdateDate"] = DateTime.Now;
                        row["UpdateUser"] = si.UserName;
                    }

                    cmdBuilder.UpdateTable(si, "FMM_SrcCell", fmmSrcCellDt, sqa);
                }

                saveResult.IsOK = true;
                saveResult.ShowMessageBox = false;
                return saveResult;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSqlTableEditorSaveDataTaskResult UnitConfig_Save()
        {
            try
            {
                var saveResult = new XFSqlTableEditorSaveDataTaskResult();
                var saveTaskInfo = args.SqlTableEditorSaveDataTaskInfo;
                var UnitID = 0;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();
                    var FMM_UnitConfig_DT = new DataTable();
                    var cubeConfigID = saveTaskInfo.CustomSubstVars.XFGetValue("BL_FMM_CubeConfigID", "0").XFConvertToInt();
                    var actConfigID = saveTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_ActConfigID", "0").XFConvertToInt();
                    Dup_Config("Initiate", "UnitConfig", cubeConfigID: cubeConfigID, actConfigID: actConfigID);

                    var sql = @"SELECT * 
                                FROM FMM_UnitConfig
                                WHERE CubeConfigID = @CubeConfigID
                                AND ActConfigID = @ActConfigID";
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = cubeConfigID },
                        new SqlParameter("@ActConfigID", SqlDbType.Int) { Value = actConfigID }
                    };
                    cmdBuilder.FillDataTable(si, sqa, FMM_UnitConfig_DT, sql, sqlparams);

                    foreach (XFEditedDataRow xfRow in saveTaskInfo.EditedDataRows)
                    {
                        if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Insert)
                        {
                            var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                            UnitID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_UnitConfig", "UnitID");
                            var new_DataRow = FMM_UnitConfig_DT.NewRow();
                            new_DataRow["CubeConfigID"] = (int)xfRow.ModifiedDataRow["CubeConfigID"];
                            new_DataRow["ActConfigID"] = (int)xfRow.ModifiedDataRow["ActConfigID"];
                            new_DataRow["UnitID"] = UnitID;
                            new_DataRow["Name"] = (string)xfRow.ModifiedDataRow.Items["Name"];
                            new_DataRow["Status"] = (int)xfRow.ModifiedDataRow.Items["Status"];
                            new_DataRow["CreateDate"] = DateTime.Now;
                            new_DataRow["CreateUser"] = si.UserName;
                            new_DataRow["UpdateDate"] = DateTime.Now;
                            new_DataRow["UpdateUser"] = si.UserName;
                            FMM_UnitConfig_DT.Rows.Add(new_DataRow);
                            Dup_Config("Update Row", "UnitConfig", "Insert", modifiedDataRow: xfRow, cubeConfigID: cubeConfigID, actConfigID: actConfigID);
                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Update)
                        {
                            var rowsToUpdate = FMM_UnitConfig_DT.Select($"UnitID = {(int)xfRow.ModifiedDataRow["UnitID"]}");
                            if (rowsToUpdate.Length > 0)
                            {
                                var rowToUpdate = rowsToUpdate[0];
                                rowToUpdate["Name"] = (string)xfRow.ModifiedDataRow["Name"];
                                rowToUpdate["Status"] = (int)xfRow.ModifiedDataRow["Status"];
                                rowToUpdate["UpdateDate"] = DateTime.Now;
                                rowToUpdate["UpdateUser"] = si.UserName;
                                Dup_Config("Update Row", "UnitConfig", "Update", modifiedDataRow: xfRow, cubeConfigID: cubeConfigID, actConfigID: actConfigID);
                            }
                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Delete)
                        {
                            var rowsToDelete = FMM_UnitConfig_DT.Select($"UnitID = {(int)xfRow.OriginalDataRow["UnitID"]}");
                            if (rowsToDelete.Length > 0)
                            {
                                foreach (var row in rowsToDelete)
                                {
                                    row.Delete();
                                    Dup_Config("Update Row", "UnitConfig", "Delete", modifiedDataRow: xfRow, cubeConfigID: cubeConfigID, actConfigID: actConfigID);
                                }
                            }
                        }
                    }
                    var dup_UnitConfig = gbl_UnitConfig_Dict
                                         .GroupBy(x => x.Value)
                                         .Where(g => g.Count() > 1)
                                         .Select(g => g.Key)
                                         .ToList();

                    gbl_Dup_UnitConfig = dup_UnitConfig.Count > 0;

                    if (gbl_Dup_UnitConfig)
                    {
                        saveResult.IsOK = false;
                        saveResult.ShowMessageBox = true;
                        saveResult.Message += "Duplicate Unit Config entries found during the operation.";
                    }
                    else
                    {
                        saveResult.IsOK = true;
                        saveResult.ShowMessageBox = false;
                        cmdBuilder.UpdateTable(si, "FMM_UnitConfig", FMM_UnitConfig_DT, sqa);
                    }
                }

                saveResult.CancelDefaultSave = true; // Note: Use True if we already saved the data rows in this Business Rule.

                return saveResult;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }


        #endregion

        #region "Register Config TED Inputs"



        private XFSqlTableEditorSaveDataTaskResult ColConfig_Save()
        {
            try
            {
                var saveResult = new XFSqlTableEditorSaveDataTaskResult();
                var saveTaskInfo = args.SqlTableEditorSaveDataTaskInfo;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();
                    var FMM_Col_Config_DT = new DataTable();
                    var cubeConfigID = saveTaskInfo.CustomSubstVars.XFGetValue("BL_FMM_CubeConfigID", "0").XFConvertToInt();
                    var actConfigID = saveTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_ActConfigID", "0").XFConvertToInt();
                    var uiConfigID = saveTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_UIConfigID", "0").XFConvertToInt();
                    Dup_Config("Initiate", "ColConfig", cubeConfigID: cubeConfigID, actConfigID: actConfigID, uiConfigID: uiConfigID);

                    var sql = @"SELECT * 
                                FROM FMM_Col_Config
                                WHERE CubeConfigID = @CubeConfigID
                                AND ActConfigID = @ActConfigID
                                AND RegConfigID = @RegConfigID";
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = cubeConfigID },
                        new SqlParameter("@ActConfigID", SqlDbType.Int) { Value = actConfigID },
                        new SqlParameter("@RegConfigID", SqlDbType.Int) { Value = uiConfigID }
                    };
                    cmdBuilder.FillDataTable(si, sqa, FMM_Col_Config_DT, sql, sqlparams);

                    foreach (XFEditedDataRow xfRow in saveTaskInfo.EditedDataRows)
                    {
                        if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Update)
                        {
                            var rowsToUpdate = FMM_Col_Config_DT.Select($"Col_ID = {(int)xfRow.ModifiedDataRow["Col_ID"]}");
                            if (rowsToUpdate.Length > 0)
                            {
                                var rowToUpdate = rowsToUpdate[0];
                                rowToUpdate["InUse"] = (bool)xfRow.ModifiedDataRow["InUse"];
                                rowToUpdate["Required"] = (bool)xfRow.ModifiedDataRow["Required"];
                                rowToUpdate["Updates"] = (bool)xfRow.ModifiedDataRow["Updates"];
                                rowToUpdate["Alias"] = (string)xfRow.ModifiedDataRow["Alias"];
                                if (!(bool)xfRow.ModifiedDataRow["InUse"])
                                {
                                    rowToUpdate["Order"] = 99;
                                }
                                else
                                {
                                    rowToUpdate["Order"] = (int)xfRow.ModifiedDataRow["Order"];
                                }
                                rowToUpdate["Default"] = (string)xfRow.ModifiedDataRow["Default"];
                                rowToUpdate["Param"] = (string)xfRow.ModifiedDataRow["Param"];
                                rowToUpdate["Format"] = (string)xfRow.ModifiedDataRow["Format"];
                                rowToUpdate["FilterParam"] = (string)xfRow.ModifiedDataRow["FilterParam"];
                                rowToUpdate["UpdateDate"] = DateTime.Now;
                                rowToUpdate["UpdateUser"] = si.UserName;
                                Dup_Config("Update Row", "ColConfig", "Update", modifiedDataRow: xfRow, cubeConfigID: cubeConfigID, actConfigID: actConfigID, uiConfigID: uiConfigID);
                            }
                        }
                    }

                    var dup_Col_Config = gbl_Col_Dict
                                         .GroupBy(x => x.Value)
                                         .Where(g => g.Count() > 1)
                                         .Select(g => g.Key)
                                         .ToList();

                    gbl_Dup_ColConfig = dup_Col_Config.Count > 0;

                    if (gbl_Dup_ColConfig)
                    {
                        saveResult.IsOK = false;
                        saveResult.ShowMessageBox = true;
                        saveResult.Message += "Duplicate Col Config entries found during the operation.";
                    }
                    else
                    {
                        saveResult.IsOK = true;
                        saveResult.ShowMessageBox = false;
                        cmdBuilder.UpdateTable(si, "FMM_Col_Config", FMM_Col_Config_DT, sqa);
                    }
                }

                saveResult.CancelDefaultSave = true; // Note: Use True if we already saved the data rows in this Business Rule.

                return saveResult;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
        #endregion

        #region "Calc Save Helpers"	

        private void Update_SrcCellCols(DataRow destRow)
        {
            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                var sqlDataAdapter = new SqlDataAdapter();
                var fmmSrcCellDt = new DataTable();

                var sql = @"SELECT * FROM FMM_SrcCell WHERE CalcConfigID = @CalcConfigID ORDER BY SrcOrder";
                var parameters = new SqlParameter[] { new SqlParameter("@CalcConfigID", SqlDbType.Int) { Value = gbl_CalcConfigID } };

                connection.Open();
                cmdBuilder.FillDataTable(si, sqlDataAdapter, fmmSrcCellDt, sql, parameters);

                // Pre-build dimension token mappings
                var dimTokens = BuildDimensionTokenMap();
                var suppressedDims = new[] { "Entity", "Cons", "Scenario", "Time" };
                var coreDims = new[] { "Acct", "IC", "Origin", "Flow", "UD1", "UD2", "UD3", "UD4", "UD5", "UD6", "UD7", "UD8" };

                int srcCellCount = 0;
                foreach (DataRow srcRow in fmmSrcCellDt.Rows)
                {
                    srcCellCount++;
                    var srcCellDrillDown = new StringBuilder();
                    var balanceMap = new Dictionary<string, int>();
                    bool isBalanced = true;

                    // Process suppressed dimensions
                    foreach (var dim in suppressedDims)
                    {
                        if (srcCellDrillDown.Length > 0) srcCellDrillDown.Append(":");
                        srcCellDrillDown.Append(ExtractDimensionValue(srcRow, dim, dimTokens[dim]));
                    }

                    // Process core dimensions
                    var unbalFilterBuffer = new StringBuilder();
                    foreach (var dim in coreDims)
                    {
                        var srcValue = ExtractDimensionValue(srcRow, dim, dimTokens[dim]);
                        var destValue = ExtractDimensionValue(destRow, dim, dimTokens[dim]);
                        var destFilterValue = ExtractDimensionValue(destRow, $"OS_{dim}_Filter", dimTokens[dim]);

                        if (!string.IsNullOrEmpty(srcValue) && string.IsNullOrEmpty(destValue))
                        {
                            balanceMap[dim] = 1;
                            srcRow[$"Unbal_{dim}_Override"] = "Common";
                            isBalanced = false;
                            if (srcCellCount > 1) gbl_BalCalc = "Unbalanced";
                            else gbl_BalCalc = "UnbalAlloc";

                            if (srcCellDrillDown.Length > 0) srcCellDrillDown.Append(":");
                            srcCellDrillDown.Append(srcValue);
                        }
                        else if (!string.IsNullOrEmpty(destFilterValue))
                        {
                            if (unbalFilterBuffer.Length > 0) unbalFilterBuffer.Append(",");
                            unbalFilterBuffer.Append($"[{destFilterValue}]");
                        }
                        else
                        {
                            srcRow[$"Unbal_{dim}_Override"] = string.Empty;
                        }
                    }

                    // Handle dynamic calc
                    if ((string)srcRow["SrcType"] == "Dynamic Calc")
                    {
                        gbl_BalCalc = "UnbalAlloc";
                    }

                    // Update row with calculated values
                    var drillDownStr = srcCellDrillDown.ToString();
                    var srcCellId = (int)srcRow["CellID"];

                    gbl_SrcCellConfig_Dict[srcCellId] = $"{srcRow["OpenParens"]}|{srcRow["MathOperator"]} {drillDownStr}|{srcRow["CloseParens"]}";
                    gbl_SrcCellDrill_Dict[srcCellId] = drillDownStr;
                    gbl_UnbalCalc_Dict[srcCellId] = $"{srcRow["OpenParens"]}|{srcRow["MathOperator"]} -Calculation- |{srcRow["CloseParens"]}";

                    srcRow["SrcOrder"] = srcCellCount;
                    srcRow["Dyn_Calc_Script"] = drillDownStr;
                    srcRow["Unbal_SrcCellConfig_Buffer"] = drillDownStr;
                    srcRow["Unbal_SrcCellConfig_Buffer_Filter"] = unbalFilterBuffer.ToString();
                }

                cmdBuilder.UpdateTable(si, "FMM_SrcCell", fmmSrcCellDt, sqlDataAdapter);
            }
        }

        private Dictionary<string, List<string>> BuildDimensionTokenMap()
        {
            return new Dictionary<string, List<string>>
            {
            { "Entity", new List<string> { "E#" } },
            { "Cons", new List<string> { "C#" } },
            { "Scenario", new List<string> { "S#" } },
            { "Time", new List<string> { "T#" } },
            { "View", new List<string> { "V#" } },
            { "Acct", new List<string> { "A#" } },
            { "IC", new List<string> { "I#", "IC#" } },
            { "Origin", new List<string> { "O#" } },
            { "Flow", new List<string> { "F#" } },
            { "UD1", new List<string> { "UD1#", "U1#" } },
            { "UD2", new List<string> { "UD2#", "U2#" } },
            { "UD3", new List<string> { "UD3#", "U3#" } },
            { "UD4", new List<string> { "UD4#", "U4#" } },
            { "UD5", new List<string> { "UD5#", "U5#" } },
            { "UD6", new List<string> { "UD6#", "U6#" } },
            { "UD7", new List<string> { "UD7#", "U7#" } },
            { "UD8", new List<string> { "UD8#", "U8#" } }
            };
        }

        private string ExtractDimensionValue(DataRow row, string fieldName, List<string> tokens)
        {
            if (row[fieldName] == DBNull.Value)
                return string.Empty;

            var value = row[fieldName].ToString();
            return tokens.Any(token => value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0) ? value : string.Empty;
        }

        private void Update_Unbal_Src_Columns(DataRow dest_DataRow)
        {
            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            string unbal_SrcCellConfig_Buffer_Filter = string.Empty;
            int unbal_SrcCellConfig_Buffer_Filter_Cnt = 0;
            string override_Dest_Val = string.Empty;
            int override_Dest_Cnt = 0;
            int countSrcCells = 0;

            var countSql = @"SELECT COUNT(*) as Count 
                FROM FMM_SrcCell 
                WHERE CalcConfigID = @CalcConfigID";

            DataTable srcDt;
            DataTable countDt;

            var sql_paramList = new List<DbParamInfo>
            {
                new DbParamInfo("@CalcConfigID", gbl_CalcConfigID)
            };

            using (dbConnApp)
            {
                countDt = BRApi.Database.ExecuteSql(dbConnApp, countSql, sql_paramList, false);

                if (countDt.Rows[0]["Count"] != DBNull.Value)
                {
                    countSrcCells = Convert.ToInt32(countDt.Rows[0]["Count"]);
                }

                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();
                    var FMM_SrcCellConfig_DT = new DataTable();

                    var sql = @"SELECT * 
                                FROM FMM_SrcCell 
                                WHERE CalcConfigID = @CalcConfigID
                                ORDER BY SrcOrder";
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@CalcConfigID", SqlDbType.Int) { Value = gbl_CalcConfigID },
                    };

                    cmdBuilder.FillDataTable(si, sqa, FMM_SrcCellConfig_DT, sql, sqlparams);

                    FMM_SrcCellConfig_DT.PrimaryKey = new DataColumn[] { FMM_SrcCellConfig_DT.Columns["CellID"] };

                    foreach (DataRow src_DataRow in FMM_SrcCellConfig_DT.Rows)
                    {
                        unbal_SrcCellConfig_Buffer_Filter = string.Empty;
                        unbal_SrcCellConfig_Buffer_Filter_Cnt = 0;
                        override_Dest_Val = string.Empty;
                        override_Dest_Cnt = 0;



                        EvaluateDim("AcctFilter", "Acct", "A#", FMM_SrcCellConfig_DT, src_DataRow, ref unbal_SrcCellConfig_Buffer_Filter, ref unbal_SrcCellConfig_Buffer_Filter_Cnt, dest_DataRow, ref override_Dest_Val, ref override_Dest_Cnt);
                        EvaluateDim("OriginFilter", "Origin", "O#", FMM_SrcCellConfig_DT, src_DataRow, ref unbal_SrcCellConfig_Buffer_Filter, ref unbal_SrcCellConfig_Buffer_Filter_Cnt, dest_DataRow, ref override_Dest_Val, ref override_Dest_Cnt);
                        EvaluateDim("FlowFilter", "Flow", "F#", FMM_SrcCellConfig_DT, src_DataRow, ref unbal_SrcCellConfig_Buffer_Filter, ref unbal_SrcCellConfig_Buffer_Filter_Cnt, dest_DataRow, ref override_Dest_Val, ref override_Dest_Cnt);
                        EvaluateDim("ICFilter", "IC", "I#", FMM_SrcCellConfig_DT, src_DataRow, ref unbal_SrcCellConfig_Buffer_Filter, ref unbal_SrcCellConfig_Buffer_Filter_Cnt, dest_DataRow, ref override_Dest_Val, ref override_Dest_Cnt);
                        EvaluateDim("UD1_Filter", "UD1", "U1#", FMM_SrcCellConfig_DT, src_DataRow, ref unbal_SrcCellConfig_Buffer_Filter, ref unbal_SrcCellConfig_Buffer_Filter_Cnt, dest_DataRow, ref override_Dest_Val, ref override_Dest_Cnt);
                        EvaluateDim("UD2_Filter", "UD2", "U2#", FMM_SrcCellConfig_DT, src_DataRow, ref unbal_SrcCellConfig_Buffer_Filter, ref unbal_SrcCellConfig_Buffer_Filter_Cnt, dest_DataRow, ref override_Dest_Val, ref override_Dest_Cnt);
                        EvaluateDim("UD3_Filter", "UD3", "U3#", FMM_SrcCellConfig_DT, src_DataRow, ref unbal_SrcCellConfig_Buffer_Filter, ref unbal_SrcCellConfig_Buffer_Filter_Cnt, dest_DataRow, ref override_Dest_Val, ref override_Dest_Cnt);
                        EvaluateDim("UD4_Filter", "UD4", "U4#", FMM_SrcCellConfig_DT, src_DataRow, ref unbal_SrcCellConfig_Buffer_Filter, ref unbal_SrcCellConfig_Buffer_Filter_Cnt, dest_DataRow, ref override_Dest_Val, ref override_Dest_Cnt);
                        EvaluateDim("UD5_Filter", "UD5", "U5#", FMM_SrcCellConfig_DT, src_DataRow, ref unbal_SrcCellConfig_Buffer_Filter, ref unbal_SrcCellConfig_Buffer_Filter_Cnt, dest_DataRow, ref override_Dest_Val, ref override_Dest_Cnt);
                        EvaluateDim("UD6_Filter", "UD6", "U6#", FMM_SrcCellConfig_DT, src_DataRow, ref unbal_SrcCellConfig_Buffer_Filter, ref unbal_SrcCellConfig_Buffer_Filter_Cnt, dest_DataRow, ref override_Dest_Val, ref override_Dest_Cnt);
                        EvaluateDim("UD7_Filter", "UD7", "U7#", FMM_SrcCellConfig_DT, src_DataRow, ref unbal_SrcCellConfig_Buffer_Filter, ref unbal_SrcCellConfig_Buffer_Filter_Cnt, dest_DataRow, ref override_Dest_Val, ref override_Dest_Cnt);
                        EvaluateDim("UD8_Filter", "UD8", "U8#", FMM_SrcCellConfig_DT, src_DataRow, ref unbal_SrcCellConfig_Buffer_Filter, ref unbal_SrcCellConfig_Buffer_Filter_Cnt, dest_DataRow, ref override_Dest_Val, ref override_Dest_Cnt);
                        src_DataRow["UnbalBuffer_Filter"] = unbal_SrcCellConfig_Buffer_Filter;
                        src_DataRow["OverrideValue"] = override_Dest_Val;
                    }
                    cmdBuilder.UpdateTable(si, "FMM_SrcCell", FMM_SrcCellConfig_DT, sqa);
                }
            }
        }

        private void Update_SrcCellCols()
        {
            try
            {
                var dimToken = new Dictionary<string, List<string>>();
                var curr_Cube_Buffer_Filter = String.Empty;
                var src_Buffer_Filter = String.Empty;
                int curr_Cube_Buffer_FilterCnt = 0;
                int src_Buffer_FilterCnt = 0;

                string[] coreDimTypes = { "Acct", "IC", "Origin", "Flow", "UD1", "UD2", "UD3", "UD4", "UD5", "UD6", "UD7", "UD8" };
                #region "Core Dim Types"												
                foreach (var dimType in coreDimTypes)
                {
                    switch (dimType)
                    {
                        case "Acct":
                            dimToken[dimType] = new List<string> { "A#" };
                            break;
                        case "IC":
                            dimToken[dimType] = new List<string> { "I#", "IC#" };
                            break;
                        case "Origin":
                            dimToken[dimType] = new List<string> { "O#" };
                            break;
                        case "Flow":
                            dimToken[dimType] = new List<string> { "F#" };
                            break;
                        case "UD1":
                            dimToken[dimType] = new List<string> { "UD1#", "U1#" };
                            break;
                        case "UD2":
                            dimToken[dimType] = new List<string> { "UD2#", "U2#" };
                            break;
                        case "UD3":
                            dimToken[dimType] = new List<string> { "UD3#", "U3#" };
                            break;
                        case "UD4":
                            dimToken[dimType] = new List<string> { "UD4#", "U4#" };
                            break;
                        case "UD5":
                            dimToken[dimType] = new List<string> { "UD5#", "U5#" };
                            break;
                        case "UD6":
                            dimToken[dimType] = new List<string> { "UD6#", "U6#" };
                            break;
                        case "UD7":
                            dimToken[dimType] = new List<string> { "UD7#", "U7#" };
                            break;
                        case "UD8":
                            dimToken[dimType] = new List<string> { "UD8#", "U8#" };
                            break;
                    }
                }
                #endregion


                if (gbl_BalCalc != "DB Model")
                {
                    var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                        var sqa = new SqlDataAdapter();
                        var FMM_DestCell_DT = new DataTable();

                        var sql = @"SELECT * 
                                    FROM FMM_DestCell
                                    WHERE CalcConfigID = @CalcConfigID";

                        var sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@CalcConfigID", SqlDbType.Int) { Value = gbl_CalcConfigID }
                        };

                        connection.Open();

                        cmdBuilder.FillDataTable(si, sqa, FMM_DestCell_DT, sql, sqlparams);
                        foreach (DataRow FMM_DestCell_DT_Row in FMM_DestCell_DT.Rows)
                        {
                            foreach (var dimType in coreDimTypes)
                            {
                                string destField = "" + dimType;
                                string filter_Field = "OS_" + dimType + "_Filter";
                                string balanceKey = dimType;
                                string dimToken_1 = string.Empty;
                                string dimToken_2 = string.Empty;
                                List<string> dimTokens = dimToken[dimType];
                                if (dimTokens.Count == 1)
                                {
                                    dimToken_1 = dimTokens[0];
                                    dimToken_2 = dimTokens[0];
                                }
                                else if (dimTokens.Count > 1)
                                {
                                    dimToken_1 = dimTokens[0];
                                    dimToken_2 = dimTokens[1];
                                }

                                if (FMM_DestCell_DT_Row[destField] != DBNull.Value)
                                {
                                    string targetValue = FMM_DestCell_DT_Row[destField].ToString();
                                    if (targetValue.IndexOf(dimToken_1, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                        targetValue.IndexOf(dimToken_2, StringComparison.OrdinalIgnoreCase) >= 0)
                                    {
                                        curr_Cube_Buffer_Filter += (curr_Cube_Buffer_FilterCnt == 0 ? "[" : ",[") + targetValue + "]";
                                        curr_Cube_Buffer_FilterCnt++;
                                    }
                                }

                                if (FMM_DestCell_DT_Row[filter_Field] != DBNull.Value)
                                {
                                    string filterValue = FMM_DestCell_DT_Row[filter_Field].ToString();
                                    if (filterValue.IndexOf(dimToken_1, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                        filterValue.IndexOf(dimToken_2, StringComparison.OrdinalIgnoreCase) >= 0)
                                    {

                                        string patternStart = "|!MbrList_";
                                        string patternEnd = "_Filter.Name!|";

                                        if (filterValue.Contains(patternStart) && filterValue.Contains(patternEnd))
                                        {
                                            int startIndex = filterValue.IndexOf(patternStart) + patternStart.Length;
                                            int endIndex = filterValue.IndexOf(patternEnd, startIndex);

                                            string number = filterValue.Substring(startIndex, endIndex - startIndex);
                                            string memberListFilter = Get_CalcConfig_MbrList(number); // Assuming this is your function
                                            curr_Cube_Buffer_Filter += (curr_Cube_Buffer_FilterCnt == 0 ? "[" : ",[") + memberListFilter + "]";
                                            src_Buffer_Filter += (src_Buffer_FilterCnt == 0 ? "[" : ",[") + filterValue + "]";
                                            curr_Cube_Buffer_FilterCnt++;
                                            src_Buffer_FilterCnt++;
                                        }
                                        else
                                        {
                                            curr_Cube_Buffer_Filter += (curr_Cube_Buffer_FilterCnt == 0 ? "[" : ",[") + filterValue + "]";
                                            src_Buffer_Filter += (src_Buffer_FilterCnt == 0 ? "[" : ",[") + filterValue + "]";
                                            curr_Cube_Buffer_FilterCnt++;
                                            src_Buffer_FilterCnt++;
                                        }
                                    }
                                }
                            }
                            FMM_DestCell_DT_Row["Curr_Cube_Buffer_Filter"] = curr_Cube_Buffer_Filter;
                            FMM_DestCell_DT_Row["BufferFilter"] = src_Buffer_Filter;
                        }
                        cmdBuilder.UpdateTable(si, "FMM_DestCell", FMM_DestCell_DT, sqa);

                    }

                }

            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }

        }

        private void EvaluateCalcConfigSetup(int Curr_CalcConfigID)
        {
            try
            {
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();
                    var destCellDT = new DataTable();

                    var sql = @"SELECT * FROM FMM_DestCell WHERE CalcConfigID = @CalcConfigID";
                    var sqlparams = new SqlParameter[] { new SqlParameter("@CalcConfigID", SqlDbType.Int) { Value = Curr_CalcConfigID } };

                    cmdBuilder.FillDataTable(si, sqa, destCellDT, sql, sqlparams);

                    if (destCellDT.Rows.Count == 0)
                    {
                        return; // Exit early if no dest cells found
                    }

                    // Initialize once outside the loop
                    gbl_BalCalc = "Balanced";
                    gbl_SrcCellConfig_Dict.Clear();

                    foreach (DataRow destRow in destCellDT.Rows)
                    {
                        Update_SrcCellCols(destRow);
                        Update_SrcCellCols();
                        UpdateGlobals();

                        if (gbl_BalCalc == "Unbalanced" || gbl_BalCalc == "UnbalAlloc" ||
                            gbl_BalCalc == "ExtUnbalanced" || gbl_BalCalc == "Ext_UnbalAlloc")
                        {
                            Update_Unbal_Src_Columns(destRow);
                        }
                    }
                    Update_CalcConfig_Columns();
                    gbl_SrcCellConfig_Dict.Clear();
                }
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private void Update_CalcConfig_Columns()
        {
            try
            {
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();
                    var FMM_CalcConfig_DT = new DataTable();

                    var sql = @"SELECT * FROM FMM_CalcConfig WHERE CalcConfigID = @CalcConfigID";
                    var sqlparams = new[] { new SqlParameter("@CalcConfigID", SqlDbType.Int) { Value = gbl_CalcConfigID } };

                    cmdBuilder.FillDataTable(si, sqa, FMM_CalcConfig_DT, sql, sqlparams);

                    if (FMM_CalcConfig_DT.Rows.Count == 0)
                        return;

                    var now = DateTime.Now;
                    var userName = si.UserName;

                    foreach (DataRow row in FMM_CalcConfig_DT.Rows)
                    {
                        UpdateCalcConfigRow(row, now, userName);
                    }

                    cmdBuilder.UpdateTable(si, "FMM_CalcConfig", FMM_CalcConfig_DT, sqa);
                }
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private void UpdateCalcConfigRow(DataRow row, DateTime updateDate, string updateUser)
        {
            bool isMultiDim = row["MultiDimAlloc"] != DBNull.Value && Convert.ToBoolean(row["MultiDimAlloc"]);
            bool isMbrList = row["MbrList_Calc"] != DBNull.Value && Convert.ToBoolean(row["MbrList_Calc"]);
            bool isBRCalc = row["BRCalc"] != DBNull.Value && Convert.ToBoolean(row["BRCalc"]);

            if (isMultiDim)
            {
                gbl_BalCalc = "MultiDimAlloc";
                row["BalBuffer"] = gbl_BalCalc;
                row["Bal_Buffer_Calc"] = gbl_UnbalBufferCalc;
                row["UnbalCalc"] = gbl_UnbalCalc;
            }
            else if (isMbrList)
            {
                HandleMbrListCalc(row);
            }
            else if (isBRCalc)
            {
                gbl_BalCalc = "BRCalc";
                return; // No row updates for BRCalc
            }
            else if (gbl_BalCalc == "Balanced")
            {
                row["BalBuffer"] = "Balanced";
                row["Bal_Buffer_calc"] = gbl_BalBufferCalc;
                row["UnbalCalc"] = string.Empty;
            }
            else if (gbl_BalCalc == "DB Model")
            {
                row["BalBuffer"] = gbl_BalCalc;
                row["Table_Calc_Logic"] = gbl_CalcLogic_Table;
                row["Table_srcCellCount"] = gbl_Table_SrcCell;
            }
            else
            {
                row["BalBuffer"] = gbl_BalCalc;
                row["Bal_Buffer_calc"] = gbl_UnbalBufferCalc;
                row["UnbalCalc"] = gbl_UnbalCalc;
            }

            row["UpdateDate"] = updateDate;
            row["UpdateUser"] = updateUser;
        }

        private void HandleMbrListCalc(DataRow row)
        {
            if (gbl_BalCalc == "Unbalanced")
            {
                gbl_BalCalc = "ExtUnbalanced";
            }
            else if (gbl_BalCalc == "UnbalAlloc")
            {
                gbl_BalCalc = "Ext_UnbalAlloc";
            }

            row["BalBuffer"] = gbl_BalCalc;
            row["Bal_Buffer_Calc"] = gbl_UnbalBufferCalc;
            row["UnbalCalc"] = gbl_UnbalCalc;
        }

        private void UpdateGlobals()
        {
            try
            {
                var sortedDict = gbl_SrcCellConfig_Dict.OrderBy(entry => entry.Key);


                int calcIterations = 1;
                foreach (KeyValuePair<int, string> calcSegment in sortedDict)
                {
                    if (gbl_BalCalc != "DB Model")
                    {
                        gbl_BalBufferCalc += StringHelper.ReplaceString(calcSegment.Value, "|", string.Empty, true);
                        if (calcIterations == 1 && (gbl_BalCalc == "Unbalanced" || gbl_BalCalc == "UnbalAlloc"))
                        {
                            gbl_UnbalBufferCalc = gbl_SrcCellDrill_Dict[calcSegment.Key];
                            gbl_UnbalCalc += StringHelper.ReplaceString(StringHelper.ReplaceString(gbl_UnbalCalc_Dict[calcSegment.Key], "-Calculation-", "BalancedBuffer", true), "|", "", true);
                        }
                        else if (gbl_BalCalc == "Unbalanced" || gbl_BalCalc == "UnbalAlloc")
                        {
                            gbl_UnbalCalc += StringHelper.ReplaceString(StringHelper.ReplaceString(gbl_UnbalCalc_Dict[calcSegment.Key], "-Calculation-", "SrcBufferValue" + calcIterations.ToString(), true), "|", "", true);
                        }
                    }
                    else
                    {
                        gbl_UnbalBufferCalc = gbl_SrcCellDrill_Dict[calcSegment.Key];
                        if (gbl_UnbalBufferCalc.Contains("T#|DBModelYear|", StringComparison.OrdinalIgnoreCase))
                        {
                            gbl_CalcLogic_Table += StringHelper.ReplaceString(StringHelper.ReplaceString(gbl_UnbalCalc_Dict[calcSegment.Key], "Cube-Calculation-", "Annual_Cube", true), "|", "", true);
                        }
                        else
                        {
                            gbl_CalcLogic_Table += StringHelper.ReplaceString(StringHelper.ReplaceString(gbl_UnbalCalc_Dict[calcSegment.Key], "Cube-Calculation-", "Monthly_Cube", true), "|", "", true);
                        }
                    }
                    calcIterations++;
                }
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private void EvaluateDim(string destFilterColumn, string srcColumn, string filterPrefix, DataTable srcDt, DataRow srcDataRow, ref string bufferFilter, ref int bufferFilterCnt, DataRow destDataRow, ref string overrideDestVal, ref int overrideCnt)
        {
            try
            {
                var srcOrder = Convert.ToInt32(srcDataRow["SrcOrder"]);
                var srcValue = srcDataRow[srcColumn];
                var destValue = destDataRow[destFilterColumn];
                var destFilterValue = destDataRow["OS_" + srcColumn + "_Filter"];
                var dynCalcScript = srcDataRow["Dyn_Calc_Script"].ToString();
                var unbalOverride = srcDataRow["Unbal_" + srcColumn + "_Override"].ToString();

                // First condition: Handle Unbalanced scenarios
                if ((gbl_BalCalc == "Unbalanced" || (gbl_BalCalc == "UnbalAlloc" && srcOrder == 1)) &&
                    destValue != DBNull.Value && destValue.ToString().XFContainsIgnoreCase(filterPrefix))
                {
                    if (srcValue == DBNull.Value || !srcValue.ToString().XFContainsIgnoreCase(filterPrefix))
                    {
                        Add_to_Buffer_Filter(destValue.ToString(), ref bufferFilter, ref bufferFilterCnt);
                    }
                }
                // Second condition: Handle subsequent sources
                else if (srcValue == DBNull.Value || !srcValue.ToString().XFContainsIgnoreCase(filterPrefix))
                {
                    var previousRow = srcOrder > 1 ? srcDt.Rows[srcOrder - 2] : null;
                    if (previousRow != null)
                    {
                        var prevValue = previousRow[srcColumn];
                        if ((prevValue == DBNull.Value || !prevValue.ToString().XFContainsIgnoreCase(filterPrefix)) &&
                            destValue != DBNull.Value && destValue.ToString().XFContainsIgnoreCase(filterPrefix))
                        {
                            Add_to_Buffer_Filter(destValue.ToString(), ref bufferFilter, ref bufferFilterCnt);
                        }
                    }
                }

                // Override logic for non-first source orders
                if (srcOrder > 1)
                {
                    var destHasFilter = destValue != DBNull.Value && destValue.ToString().XFContainsIgnoreCase(filterPrefix);
                    var destHasFilterParam = destFilterValue != DBNull.Value && destFilterValue.ToString().XFContainsIgnoreCase(filterPrefix);
                    var dynCalcHasFilter = dynCalcScript.XFContainsIgnoreCase(filterPrefix);
                    var prevRow = srcDt.Rows[srcOrder - 2];
                    var prevValue = prevRow[srcColumn];
                    var prevHasFilter = prevValue != DBNull.Value && prevValue.ToString().XFContainsIgnoreCase(filterPrefix);
                    var srcIsEmpty = srcValue == DBNull.Value;
                    var unbalHasFilter = unbalOverride.XFContainsIgnoreCase(filterPrefix);

                    bool shouldAddOverride = (!destHasFilter && !destHasFilterParam && dynCalcHasFilter) ||
                               (prevHasFilter && srcIsEmpty && !unbalHasFilter);

                    if (shouldAddOverride)
                    {
                        overrideDestVal = overrideCnt > 0 ? overrideDestVal + "," + filterPrefix : filterPrefix;
                        overrideCnt++;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private void Add_to_Buffer_Filter(string value, ref string bufferFilter, ref int bufferFilterCnt)
        {
            if (bufferFilterCnt == 0)
            {
                bufferFilter = $"[{value}]";
            }
            else
            {
                bufferFilter += $",[{value}]";
            }
            bufferFilterCnt++;
        }

        private bool XFContains_Ignore_Case(string source, string toCheck)
        {
            return source?.IndexOf(toCheck, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public class MbrListDimChecker
        {
            private static readonly Dictionary<string, string> patterns = new Dictionary<string, string>
            {
                { "A#", "Account" },
                { "F#", "Flow" },
                { "U1#", "UD1" },
                { "U2#", "UD2" },
                { "U3#", "UD3" },
                { "U4#", "UD4" },
                { "U5#", "UD5" },
                { "U6#", "UD6" },
                { "U7#", "UD7" },
                { "U8#", "UD8" },
                { "UD1#", "UD1" },
                { "UD2#", "UD2" },
                { "UD3#", "UD3" },
                { "UD4#", "UD4" },
                { "UD5#", "UD5" },
                { "UD6#", "UD6" },
                { "UD7#", "UD7" },
                { "UD8#", "UD8" }
            };

            public string DimCheck(DataRow row, string columnName)
            {
                if (row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value)
                {
                    string columnValue = row[columnName].ToString();

                    foreach (var pattern in patterns)
                    {
                        if (columnValue.StartsWith(pattern.Key) && columnValue.Length > 1)
                        {
                            return pattern.Value;
                        }
                    }
                }

                return string.Empty;
            }
            public string Get_Src_Dest_Filter(DataRow row, string columnName)
            {
                if (row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value)
                {
                    string columnValue = row[columnName].ToString().Replace(".Children", ".Base");

                    return columnValue;
                }

                return string.Empty;
            }
        }

        private string Get_CalcConfig_MbrList(string number)
        {

            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                connection.Open();

                var sqa = new SqlDataAdapter();
                var FMM_CalcConfig_DT = new DataTable();
                var sql = @$"SELECT MbrList{number}Filter
                            FROM FMM_CalcConfig
                            WHERE CalcConfigID = @CalcConfigID";
                var sqlparams = new SqlParameter[]
                {
                    new SqlParameter("@CalcConfigID", SqlDbType.Int) { Value = gbl_CalcConfigID}
                };

                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_CalcConfig_DT, sql, sqlparams);

                foreach (DataRow CalcConfig_Row in FMM_CalcConfig_DT.Rows)
                {
                    return (string)CalcConfig_Row[$"MbrList{number}Filter"];
                }
            }


            return string.Empty;
        }


        #endregion

        private XFSelectionChangedTaskResult ModelGrp_Save(string runType)
        {
            try
            {
                var saveResult = new XFSelectionChangedTaskResult();
                var new_OS_Model_Grp_CubeConfigID = Convert.ToInt32(args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("BL_FMM_CubeConfigID", "0"));
                var new_Name = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_ModelConfigGrp_Name", "0");
                int new_ModelGrpConfigID = 0;
                if (new_ModelGrpConfigID == 0)
                {
                    var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        var sql_gbl_get_max_id = new SQL_GBL_Get_Max_ID(si, connection);
                        connection.Open();

                        new_ModelGrpConfigID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_ModelGrps", "ModelGrpConfigID");

                        var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                        var FMM_ModelGrps_DT = new DataTable();
                        var sqa = new SqlDataAdapter();

                        string sql = @"
                                        SELECT * 
                                        FROM FMM_ModelGrps 
                                        WHERE ModelGrpConfigID = @ModelGrpConfigID";


                        var sqlparams = new SqlParameter[]
                        {
                        new SqlParameter("@ModelGrpConfigID", SqlDbType.Int) { Value = new_ModelGrpConfigID }
                        };

                        cmdBuilder.FillDataTable(si, sqa, FMM_ModelGrps_DT, sql, sqlparams);

                        var new_DataRow = FMM_ModelGrps_DT.NewRow();
                        new_DataRow["CubeConfigID"] = new_OS_Model_Grp_CubeConfigID;
                        new_DataRow["ModelGrpConfigID"] = new_ModelGrpConfigID;
                        new_DataRow["Name"] = new_Name;
                        new_DataRow["Status"] = "Build";
                        new_DataRow["CreateDate"] = DateTime.Now;
                        new_DataRow["CreateUser"] = si.UserName;
                        new_DataRow["UpdateDate"] = DateTime.Now;
                        new_DataRow["UpdateUser"] = si.UserName;
                        FMM_ModelGrps_DT.Rows.Add(new_DataRow);

                        cmdBuilder.UpdateTable(si, "FMM_ModelGrps", FMM_ModelGrps_DT, sqa);
                    }
                }

                return saveResult;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSelectionChangedTaskResult ModelGrpSeqConfig_Save(string runType)
        {
            try
            {
                var saveResult = new XFSelectionChangedTaskResult();
                var new_CubeConfigID = Convert.ToInt32(args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("BL_FMM_CubeConfigID", "0"));
                var new_Name = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_ModelConfigGrp_Seq_Name", "0");
                int new_ModelGrpSeqConfigID = 0;
                if (new_ModelGrpSeqConfigID == 0)
                {
                    var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        var sql_gbl_get_max_id = new SQL_GBL_Get_Max_ID(si, connection);
                        connection.Open();

                        new_ModelGrpSeqConfigID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_ModelConfigGrp_Seqs", "ModelGrpSeqConfigID");

                        var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                        var FMM_ModelConfigGrp_Seqs_DT = new DataTable();
                        var sqa = new SqlDataAdapter();

                        string sql = @"
                                        SELECT * 
                                        FROM FMM_ModelConfigGrp_Seqs 
                                        WHERE ModelGrpSeqConfigID = @ModelGrpSeqConfigID";


                        var sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@ModelGrpSeqConfigID", SqlDbType.Int) { Value = new_ModelGrpSeqConfigID }
                        };

                        cmdBuilder.FillDataTable(si, sqa, FMM_ModelConfigGrp_Seqs_DT, sql, sqlparams);

                        var new_DataRow = FMM_ModelConfigGrp_Seqs_DT.NewRow();
                        new_DataRow["CubeConfigID"] = new_CubeConfigID;
                        new_DataRow["ModelGrpSeqConfigID"] = new_ModelGrpSeqConfigID;
                        new_DataRow["Name"] = new_Name;
                        new_DataRow["Status"] = "Build";
                        new_DataRow["CreateDate"] = DateTime.Now;
                        new_DataRow["CreateUser"] = si.UserName;
                        new_DataRow["UpdateDate"] = DateTime.Now;
                        new_DataRow["UpdateUser"] = si.UserName;
                        FMM_ModelConfigGrp_Seqs_DT.Rows.Add(new_DataRow);

                        cmdBuilder.UpdateTable(si, "FMM_ModelConfigGrp_Seqs", FMM_ModelConfigGrp_Seqs_DT, sqa);
                    }
                }

                return saveResult;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSelectionChangedTaskResult ModelGrpAssign_Save(string runType)
        {
            try
            {
                var saveResult = new XFSelectionChangedTaskResult();
                var custom_SubstVars = args.SelectionChangedTaskInfo.CustomSubstVars;
                var cubeConfigID = Convert.ToInt32(custom_SubstVars.XFGetValue("BL_FMM_CubeConfigID", "0"));
                var ModelGrpConfigID = Convert.ToInt32(custom_SubstVars.XFGetValue("IV_FMM_ModelConfigGrp_ID", "0"));
                var ModelConfigID_List = custom_SubstVars.XFGetValue("IV_FMM_ModelConfigID_Selection", "0");
                int new_OS_Model_Grp_Assign_ID = 0;
                if (ModelConfigID_List.Length > 0 && new_OS_Model_Grp_Assign_ID == 0)
                {
                    var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                        connection.Open();

                        new_OS_Model_Grp_Assign_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_ModelGrpAssign", "Model_Grp_Assign_ID");

                        var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                        var FMM_ModelGrpAssign_DT = new DataTable();
                        var sqa = new SqlDataAdapter();

                        string sql = @"
                                        SELECT * 
                                        FROM FMM_ModelGrpAssign
                                        WHERE CubeConfigID = @CubeConfigID
                                        AND ModelGrpConfigID = @ModelGrpConfigID";

                        var sqlparams = new SqlParameter[]
                        {
                        new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = cubeConfigID },
                        new SqlParameter("@ModelGrpConfigID", SqlDbType.Int) { Value = ModelGrpConfigID }
                        };

                        cmdBuilder.FillDataTable(si, sqa, FMM_ModelGrpAssign_DT, sql, sqlparams);

                        var ModelConfigIDs = ModelConfigID_List.Split(',');
                        bool isFirstIteration = true;
                        foreach (var ModelConfigID in ModelConfigIDs)
                        {
                            if (!isFirstIteration)
                            {
                                new_OS_Model_Grp_Assign_ID += 1;
                            }
                            isFirstIteration = false;
                            var new_DataRow = FMM_ModelGrpAssign_DT.NewRow();
                            new_DataRow["CubeConfigID"] = cubeConfigID;
                            new_DataRow["ModelGrpConfigID"] = ModelGrpConfigID;
                            new_DataRow["ModelConfigID"] = Convert.ToInt32(ModelConfigID.Trim());
                            new_DataRow["Model_Grp_Assign_ID"] = new_OS_Model_Grp_Assign_ID;
                            new_DataRow["Sequence"] = 0;
                            new_DataRow["Status"] = "Build";
                            new_DataRow["CreateDate"] = DateTime.Now;
                            new_DataRow["CreateUser"] = si.UserName;
                            new_DataRow["UpdateDate"] = DateTime.Now;
                            new_DataRow["UpdateUser"] = si.UserName;

                            FMM_ModelGrpAssign_DT.Rows.Add(new_DataRow);
                        }

                        cmdBuilder.UpdateTable(si, "FMM_ModelGrpAssign", FMM_ModelGrpAssign_DT, sqa);
                    }
                }

                return saveResult;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSelectionChangedTaskResult Save_Calc_Unit_Assign(string runType)
        {
            try
            {
                var saveResult = new XFSelectionChangedTaskResult();
                var custom_SubstVars = args.SelectionChangedTaskInfo.CustomSubstVars;
                var cubeConfigID = Convert.ToInt32(custom_SubstVars.XFGetValue("BL_FMM_CubeConfigID", "0"));
                var CalcUnitID_List = custom_SubstVars.XFGetValue("IV_FMM_CalcConfig_Unit_Selection", "0");
                var ModelGrpConfigID_List = custom_SubstVars.XFGetValue("IV_FMM_ModelConfigGrp_ID_Selection");
                var ModelGrpSeqConfigID = custom_SubstVars.XFGetValue("IV_FMM_ModelConfigGrp_Seq_ID");
                int new_Calc_Unit_Assign_ID = 0;
                if (ModelGrpConfigID_List.Length > 0 && new_Calc_Unit_Assign_ID == 0)
                {
                    var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                        connection.Open();

                        new_Calc_Unit_Assign_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_CalcConfig_Unit_Assign", "Calc_Unit_Assign_ID");

                        var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                        var FMM_CalcConfig_Unit_Assign_DT = new DataTable();
                        var sqa = new SqlDataAdapter();

                        var sql = @"SELECT * 
                                    FROM FMM_CalcConfig_Unit_Assign
                                    WHERE CubeConfigID = @CubeConfigID
                                    AND ModelGrpSeqConfigID = @ModelGrpSeqConfigID";

                        var sqlparams = new SqlParameter[]
                        {
                        new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = cubeConfigID },
                        new SqlParameter("@ModelGrpSeqConfigID", SqlDbType.Int) { Value = ModelGrpSeqConfigID }
                        };

                        cmdBuilder.FillDataTable(si, sqa, FMM_CalcConfig_Unit_Assign_DT, sql, sqlparams);

                        var modelGroupIds = ModelGrpConfigID_List.Split(',');
                        var WfDuIds = CalcUnitID_List.Split(',');
                        bool isFirstIteration = true;
                        foreach (var modelGroupId in modelGroupIds)
                        {
                            foreach (var WfDuId in WfDuIds)
                            {
                                if (!isFirstIteration)
                                {
                                    new_Calc_Unit_Assign_ID += 1;
                                }
                                isFirstIteration = false;
                                var new_DataRow = FMM_CalcConfig_Unit_Assign_DT.NewRow();
                                new_DataRow["CubeConfigID"] = cubeConfigID;
                                new_DataRow["ModelGrpConfigID"] = Convert.ToInt32(modelGroupId.Trim());
                                new_DataRow["CalcUnitID"] = Convert.ToInt32(WfDuId.Trim());
                                new_DataRow["ModelGrpSeqConfigID"] = ModelGrpSeqConfigID;
                                new_DataRow["Calc_Unit_Assign_ID"] = new_Calc_Unit_Assign_ID;
                                new_DataRow["Sequence"] = 0;
                                new_DataRow["Status"] = "Build";
                                new_DataRow["CreateDate"] = DateTime.Now;
                                new_DataRow["CreateUser"] = si.UserName;
                                new_DataRow["UpdateDate"] = DateTime.Now;
                                new_DataRow["UpdateUser"] = si.UserName;

                                FMM_CalcConfig_Unit_Assign_DT.Rows.Add(new_DataRow);
                            }
                        }

                        cmdBuilder.UpdateTable(si, "FMM_CalcConfig_Unit_Assign", FMM_CalcConfig_Unit_Assign_DT, sqa);
                    }
                }

                return saveResult;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSelectionChangedTaskResult ApprConfig_Save(string runType)
        {
            try
            {
                var saveResult = new XFSelectionChangedTaskResult();
                var saveTaskInfo = args.SelectionChangedTaskInfo;
                var ApprID = 0;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();
                    var FMM_ApprConfig_DT = new DataTable();
                    var cubeConfigID = saveTaskInfo.CustomSubstVars.XFGetValue("BL_FMM_CubeConfigID", "0").XFConvertToInt();
                    var actConfigID = saveTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_ActConfigID", "0").XFConvertToInt();
                    var new_ApprID = 0;
                    Dup_Config("Initiate", "ApprConfig", cubeConfigID: cubeConfigID);

                    var sql = @"SELECT * 
                                    FROM FMM_ApprConfigConfig
                                    WHERE CubeConfigID = @CubeConfigID";
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = cubeConfigID }
                    };
                    cmdBuilder.FillDataTable(si, sqa, FMM_ApprConfig_DT, sql, sqlparams);

                    if (runType == "Add")
                    {
                        sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = new_ApprID }
                        };

                        cmdBuilder.FillDataTable(si, sqa, FMM_ApprConfig_DT, sql, sqlparams);

                        var new_DataRow = FMM_ApprConfig_DT.NewRow();
                        var SaveTypeintValue = 1;
                        FMM_ConfigHelpers.SaveType saveType = (FMM_ConfigHelpers.SaveType)SaveTypeintValue;
                        if (FMM_ConfigHelpers.ApprConfigRegistry.Configs.TryGetValue(saveType, out var config))
                        {
                            foreach (var step in config.ParameterMappings)
                            {
                                foreach (var map in step.Value)
                                {
                                    new_DataRow[map.Value] = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue(map.Key, string.Empty);
                                }
                            }
                        }

                        new_DataRow["CubeConfigID"] = new_ApprID;
                        new_DataRow["Status"] = 1;
                        new_DataRow["CreateDate"] = DateTime.Now;
                        new_DataRow["CreateUser"] = si.UserName;
                        new_DataRow["UpdateDate"] = DateTime.Now;
                        new_DataRow["UpdateUser"] = si.UserName;

                        FMM_ApprConfig_DT.Rows.Add(new_DataRow);
                        cmdBuilder.UpdateTable(si, "FMM_ApprConfigConfig", FMM_ApprConfig_DT, sqa);
                        saveResult.IsOK = true;
                        saveResult.Message = "New Cube Config Saved.";
                        saveResult.ShowMessageBox = true;
                    }
                    //                    else if (Convert.ToInt32(FMM_CubeConfig_Count_DT.Rows[0]["Count"]) > 0 && runType == "Update")
                    //                    {
                    //                        var cubeStatus = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("DL_FMM_Status", string.Empty);
                    //                        new_CubeConfigID = Convert.ToInt32(args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("BL_FMM_CubeConfigID", "0"));
                    //                        sqlparams = new SqlParameter[]
                    //                        {
                    //                            new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = new_CubeConfigID }
                    //                        };

                    //                        cmdBuilder.FillDataTable(si, sqa, FMM_CubeConfig_DT, sql, sqlparams);

                    //                        if (FMM_CubeConfig_DT.Rows.Count > 0)
                    //                        {
                    //                            var rowToUpdate = FMM_CubeConfig_DT.Rows[0];
                    //                            var SaveTypeintValue = 2;
                    //                            FMM_ConfigHelpers.SaveType saveType = (FMM_ConfigHelpers.SaveType)SaveTypeintValue;
                    //                            if (FMM_ConfigHelpers.CubeConfigRegistry.Configs.TryGetValue(saveType, out var config))
                    //                            {
                    //                                foreach (var step in config.ParameterMappings)
                    //                                {
                    //                                    foreach (var map in step.Value)
                    //                                    {
                    //                                        rowToUpdate[map.Value] = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue(map.Key, string.Empty);
                    //                                    }
                    //                                }
                    //                            }
                    //                            rowToUpdate["UpdateDate"] = DateTime.Now;
                    //                            rowToUpdate["UpdateUser"] = si.UserName;

                    //                            cmdBuilder.UpdateTable(si, "FMM_CubeConfig", FMM_CubeConfig_DT, sqa);

                    //                            saveResult.IsOK = true;
                    //                            saveResult.Message = "Cube Config Updates Saved.";
                    //                            saveResult.ShowMessageBox = true;
                    //                        }
                    //                    }
                    //                    else if (Convert.ToInt32(FMM_CubeConfig_Count_DT.Rows[0]["Count"]) > 0 && runType == "Add")
                    //                    {
                    //                        saveResult.IsOK = false;
                    //                        saveResult.Message = "Duplicated Cube and Scenario Type, Cube Config not saved.";
                    //                        saveResult.ShowMessageBox = true;
                    //                    }
                }

                return saveResult;
            }
            catch (Exception ex)
            {
                var errorResult = new XFSelectionChangedTaskResult
                {
                    IsOK = false,
                    Message = $"An error occurred: {ex.Message}",
                    ShowMessageBox = true
                };
                return errorResult;
            }
        }
        private XFSelectionChangedTaskResult ApprStepConfig_Save()
        {
            try
            {
                var saveResult = new XFSelectionChangedTaskResult();
                var saveTaskInfo = args.SelectionChangedTaskInfo;
                var customSubstVars = saveTaskInfo.CustomSubstVars;
                var ApprStepID = 0;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();
                    var FMM_ApprStepConfig_DT = new DataTable();
                    var cubeConfigID = customSubstVars.XFGetValue("BL_FMM_CubeConfigID", "0").XFConvertToInt();
                    var apprID = customSubstVars.XFGetValue("IV_FMM_ApprConfigID", "0").XFConvertToInt();
                    Dup_Config("Initiate", "ApprStepConfig", cubeConfigID: cubeConfigID, apprID: apprID);

                    string sql = @"SELECT * 
                                    FROM FMM_ApprConfigStep_Config
                                    WHERE CubeConfigID = @CubeConfigID
                                    AND ApprID = @ApprID";
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = cubeConfigID },
                        new SqlParameter("@ApprID", SqlDbType.Int) { Value = apprID }
                    };
                    cmdBuilder.FillDataTable(si, sqa, FMM_ApprStepConfig_DT, sql, sqlparams);

                    //                    foreach (XFEditedDataRow xfRow in saveTaskInfo.EditedDataRows)
                    //                    {
                    //                        if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Insert)
                    //                        {
                    //                            var new_DataRow = FMM_ApprStepConfig_DT.NewRow();

                    //                            new_DataRow["CubeConfigID"] = (int)xfRow.ModifiedDataRow["CubeConfigID"];
                    //                            new_DataRow["ApprID"] = (int)xfRow.ModifiedDataRow["ApprID"];
                    //                            new_DataRow["ApprStepID"] = (int)xfRow.ModifiedDataRow["ApprStepID"];
                    //                            new_DataRow["WFProfile_Step"] = (string)xfRow.ModifiedDataRow["WFProfile_Step"];
                    //                            new_DataRow["StepNum"] = (int)xfRow.ModifiedDataRow["StepNum"];
                    //                            new_DataRow["UserGroup"] = (string)xfRow.ModifiedDataRow["UserGroup"];
                    //                            new_DataRow["Logic"] = (string)xfRow.ModifiedDataRow["Logic"];
                    //                            new_DataRow["Item"] = (string)xfRow.ModifiedDataRow["Item"];
                    //                            new_DataRow["Level"] = (int)xfRow.ModifiedDataRow["Level"];
                    //                            new_DataRow["ApprConfig"] = (int)xfRow.ModifiedDataRow["ApprConfig"];
                    //                            new_DataRow["InitStatus"] = (string)xfRow.ModifiedDataRow["InitStatus"];
                    //                            new_DataRow["ApprStatus"] = (string)xfRow.ModifiedDataRow["ApprStatus"];
                    //                            new_DataRow["RejStatus"] = (string)xfRow.ModifiedDataRow["RejStatus"];
                    //                            new_DataRow["Status"] = (string)xfRow.ModifiedDataRow["Status"];
                    //                            new_DataRow["CreateDate"] = DateTime.Now;
                    //                            new_DataRow["CreateUser"] = si.UserName;
                    //                            new_DataRow["UpdateDate"] = DateTime.Now;
                    //                            new_DataRow["UpdateUser"] = si.UserName;

                    //                            FMM_ApprStepConfig_DT.Rows.Add(new_DataRow);

                    //                            Dup_ApprStepConfig(CubeConfigID, ApprID, "Insert Row", ref saveResult, "Insert", xfRow);
                    //                        }
                    //                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Update)
                    //                        {
                    //                            var rowsToUpdate = FMM_ApprStepConfig_DT.Select($"ApprStepID = {(int)xfRow.ModifiedDataRow["ApprStepID"]}");
                    //                            if (rowsToUpdate.Length > 0)
                    //                            {
                    //                                var rowToUpdate = rowsToUpdate[0];
                    //                                rowToUpdate["StepNum"] = (int)xfRow.ModifiedDataRow["StepNum"];
                    //                                rowToUpdate["UserGroup"] = (string)xfRow.ModifiedDataRow["UserGroup"];
                    //                                rowToUpdate["Logic"] = (string)xfRow.ModifiedDataRow["Logic"];
                    //                                rowToUpdate["Item"] = (string)xfRow.ModifiedDataRow["Item"];
                    //                                rowToUpdate["Level"] = (int)xfRow.ModifiedDataRow["Level"];
                    //                                rowToUpdate["ApprConfig"] = (int)xfRow.ModifiedDataRow["ApprConfig"];
                    //                                rowToUpdate["InitStatus"] = (string)xfRow.ModifiedDataRow["InitStatus"];
                    //                                rowToUpdate["ApprStatus"] = (string)xfRow.ModifiedDataRow["ApprStatus"];
                    //                                rowToUpdate["RejStatus"] = (string)xfRow.ModifiedDataRow["RejStatus"];
                    //                                rowToUpdate["Status"] = (string)xfRow.ModifiedDataRow["Status"];
                    //                                rowToUpdate["UpdateDate"] = DateTime.Now;
                    //                                rowToUpdate["UpdateUser"] = si.UserName;
                    //                                Dup_ApprStepConfig(CubeConfigID, ApprID, "Update Row", ref saveResult, "Update", xfRow);
                    //                            }
                    //                        }
                    //                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Delete)
                    //                        {
                    //                            var rowsToDelete = FMM_ApprStepConfig_DT.Select($"ApprStepID = {(int)xfRow.OriginalDataRow["ApprStepID"]}");
                    //                            if (rowsToDelete.Length > 0)
                    //                            {
                    //                                foreach (var row in rowsToDelete)
                    //                                {
                    //                                    row.Delete();
                    //                                    Dup_ApprStepConfig(CubeConfigID, ApprID, "Update Row", ref saveResult, "Delete", xfRow);
                    //                                }
                    //                            }
                    //                        }
                    //                    }
                    //                    var dup_ApprSteps = gbl_ApprStep_Dict
                    //                                        .GroupBy(x => x.Value)
                    //                                        .Where(g => g.Count() > 1)
                    //                                        .Select(g => g.Key)
                    //                                        .ToList();

                    //                    gbl_Dup_ApprStep = dup_ApprSteps.Count > 0;

                    //                    if (gbl_Dup_ApprStep)
                    //                    {
                    //                        saveResult.IsOK = false;
                    //                        saveResult.ShowMessageBox = true;
                    //                        saveResult.Message += "Duplicate Activity Approval entries found during the operation.";
                    //                    }
                    //                    else
                    //                    {
                    //                        saveResult.IsOK = true;
                    //                        saveResult.ShowMessageBox = false;
                    //                        cmdBuilder.UpdateTable(si, "FMM_ApprConfigStep_Config", FMM_ApprStepConfig_DT, sqa, "CubeConfigID", "ApprID", "ApprStepID");
                    //                    }
                }

                ////                saveResult.CancelDefaultSave = true; // Note: Use True if we already saved the data rows in this Business Rule.

                return saveResult;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
        private XFSelectionChangedTaskResult ApprStep_Save(string runType)
        {
            try
            {
                var saveResult = new XFSelectionChangedTaskResult();
                var custom_SubstVars = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues;
                var cubeConfigID = Convert.ToInt32(custom_SubstVars.XFGetValue("BL_FMM_CubeConfigID", "0"));
                var ApprID = Convert.ToInt32(custom_SubstVars.XFGetValue("IV_FMM_ApprConfigID", "0"));
                var wfProfile_Step = custom_SubstVars.XFGetValue("IV_FMM_trv_ApprStep_WFProfile", string.Empty);

                var new_ApprStepID = 0;

                if (cubeConfigID > 0 && ApprID > 0 && wfProfile_Step != string.Empty && new_ApprStepID == 0)
                {
                    var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                        var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                        connection.Open();

                        new_ApprStepID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_ApprConfigStep_Config", "ApprStepID");

                        var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                        var FMM_ApprStepConfig_DT = new DataTable();
                        var FMM_ApprConfigStep_WFProfile_DT = new DataTable();
                        var FMM_CubeConfig_DT = new DataTable();
                        var FMM_Act_ApprStep_Config_DT = new DataTable(); // DataTable for FMM_Reg_Dtl_Cube_Map
                        var sqa = new SqlDataAdapter();

                        var sql = @"SELECT * 
                                    FROM FMM_ApprConfigStep_Config
                                    WHERE CubeConfigID = @CubeConfigID
                                    AND ApprID = @ApprID";

                        var sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = cubeConfigID },
                            new SqlParameter("@ApprID", SqlDbType.Int) { Value = ApprID }
                        };

                        cmdBuilder.FillDataTable(si, sqa, FMM_ApprStepConfig_DT, sql, sqlparams);




                        sql = @"SELECT * 
                                FROM FMM_CubeConfig
                                WHERE CubeConfigID = @CubeConfigID";

                        sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = cubeConfigID }
                        };

                        sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_CubeConfig_DT, sql, sqlparams);

                        var topCubeInfo = BRApi.Finance.Cubes.GetCubeInfo(si, FMM_CubeConfig_DT.Rows[0].Field<string>("Cube"));
                        var cubeScenarioType = ScenarioType.GetItem("Plan").Id;
                        var cubeScenarioTypeId = ScenarioTypeId.LongTerm;
                        var rootProfileName = topCubeInfo.GetTopLevelCubeWFPName(si, cubeScenarioType);
                        var wfProfileSuffixes = new Dictionary<ScenarioTypeId, string>();
                        wfProfileSuffixes = topCubeInfo.TopLevelCubeWFPSuffixes;
                        var wfProfileSuffix = wfProfileSuffixes.XFGetValue(cubeScenarioTypeId, string.Empty);

                        foreach (var entry in wfProfileSuffixes)
                        {
                            ScenarioTypeId id = entry.Key;
                            string suffix = entry.Value;

                        }

                        sql = @"WITH RecursiveCTE AS (
                                SELECT 
                                    prof.ProfileKey,
                                    prof.ProfileName, 
                                    CAST('00000000-0000-0000-0000-000000000000' AS uniqueidentifier) AS ParentProfileKey, 
                                    prof.HierarchyLevel, 
                                    prof.HierarchyIndex
                                FROM 
                                    WorkflowProfileHierarchy prof
                                WHERE 
                                    prof.HierarchyLevel = 1
                                    AND prof.IsTemplate = 0
                                    AND prof.ProfileName = @rootprofilename 
                                UNION ALL
                                SELECT 
                                    prof.ProfileKey,
                                    prof.ProfileName,
                                    prof.ParentProfileKey, 
                                    prof.HierarchyLevel, 
                                    prof.HierarchyIndex
                                FROM 
                                    WorkflowProfileHierarchy prof
                                INNER JOIN 
                                    RecursiveCTE rcte ON prof.ParentProfileKey = rcte.ProfileKey)
                            SELECT 
                                rcte.ProfileName, 
                                rcte.ProfileKey,
                                rcte.ParentProfileKey, 
                                parentProf.ProfileName as ParentProfileName, 
                                rcte.HierarchyLevel,
                                rcte.HierarchyIndex
                            FROM 
                                RecursiveCTE rcte
                            LEFT JOIN 
                                WorkflowProfileHierarchy parentProf ON rcte.ParentProfileKey = parentProf.ProfileKey
                            ORDER BY 
                                rcte.HierarchyLevel DESC, 
                                rcte.HierarchyIndex";

                        sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@rootprofilename", SqlDbType.NVarChar) { Value = rootProfileName }
                        };

                        sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_ApprConfigStep_WFProfile_DT, sql, sqlparams);

                        var filteredWFProfiles = from profile in FMM_ApprConfigStep_WFProfile_DT.AsEnumerable()
                                                 where profile["ProfileName"].ToString().Contains(wfProfile_Step)
                                                 select profile;

                        foreach (var profile in filteredWFProfiles)
                        {
                            var new_DataRow = FMM_ApprStepConfig_DT.NewRow();
                            new_DataRow["CubeConfigID"] = cubeConfigID;
                            new_DataRow["ApprID"] = ApprID;
                            new_DataRow["WFProfile_Step"] = profile["ProfileKey"];
                            new_DataRow["ApprStepID"] = new_ApprStepID++;
                            new_DataRow["StepNum"] = 0;
                            new_DataRow["UserGroup"] = "Test";
                            new_DataRow["Logic"] = "Test";
                            new_DataRow["Item"] = "Test";
                            new_DataRow["Level"] = 0;
                            new_DataRow["ApprConfig"] = 0;
                            new_DataRow["InitStatus"] = "Testy";
                            new_DataRow["ApprStatus"] = "Testy";
                            new_DataRow["RejStatus"] = "Testy";
                            new_DataRow["Status"] = "Build";
                            new_DataRow["CreateDate"] = DateTime.Now;
                            new_DataRow["CreateUser"] = si.UserName;
                            new_DataRow["UpdateDate"] = DateTime.Now;
                            new_DataRow["UpdateUser"] = si.UserName;

                            FMM_ApprStepConfig_DT.Rows.Add(new_DataRow);


                        }

                        cmdBuilder.UpdateTable(si, "FMM_ApprConfigStep_Config", FMM_ApprStepConfig_DT, sqa);


                    }
                }
                return saveResult;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSelectionChangedTaskResult UIConfig_Save(string runType)
        {
            try
            {
                var saveResult = new XFSelectionChangedTaskResult();
                var saveTaskInfo = args.SelectionChangedTaskInfo;
                var new_Reg_List = new List<int>();
                var uiConfigID = 0;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();
                    var FMM_UIConfig_DT = new DataTable();
                    var cubeConfigID = saveTaskInfo.CustomSubstVars.XFGetValue("BL_FMM_CubeConfigID", "0").XFConvertToInt();
                    var actConfigID = saveTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_ActConfigID", "0").XFConvertToInt();
                    Dup_Config("Initiate", "RegConfig", cubeConfigID: cubeConfigID, actConfigID: actConfigID);

                    var sql = @"SELECT * 
                                FROM FMM_UIConfig
                                WHERE CubeConfigID = @CubeConfigID
                                AND ActConfigID = @ActConfigID";
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = cubeConfigID },
                        new SqlParameter("@ActConfigID", SqlDbType.Int) { Value = actConfigID }
                    };
                    cmdBuilder.FillDataTable(si, sqa, FMM_UIConfig_DT, sql, sqlparams);
                }
                return saveResult;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
        private XFSelectionChangedTaskResult Process_Bulk_Calc_Unit()
        {
            try
            {
                var saveResult = new XFSelectionChangedTaskResult();
                var custom_SubstVars = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues;
                var cubeConfigID = Convert.ToInt32(custom_SubstVars.XFGetValue("BL_FMM_CubeConfigID", "0"));
                var Calc_Unit_Entity_MFB = custom_SubstVars.XFGetValue("IV_FMM_CalcConfig_Unit_Entity_MFB", "0");
                var WF_Channels = custom_SubstVars.XFGetValue("BL_FMM_WFChannels", "0");
                var new_CalcUnitID = 0;
                var loop_times = 0;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                    connection.Open();

                    new_CalcUnitID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_CalcUnitConfig", "CalcUnitID");

                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var FMM_CalcUnitConfig_DT = new DataTable();
                    var sqa = new SqlDataAdapter();

                    string sql = @"SELECT * 
                                   FROM FMM_CalcUnitConfig
                                   WHERE CubeConfigID = @CubeConfigID";

                    var sqlparams = new SqlParameter[]
                    {
                    new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = cubeConfigID }
                    };

                    cmdBuilder.FillDataTable(si, sqa, FMM_CalcUnitConfig_DT, sql, sqlparams);
                    var cubeInfo = BRApi.Finance.Cubes.GetCubeInfo(si, "Army_RMW_Consol");
                    var ent_DimID = cubeInfo.Cube.CubeDims.GetEntityDimId();
                    var cubeDim_Lookup = cubeInfo.Cube.CubeDims.CubeDimsDictionary.Values.ToLookup(dim => dim.DimId);
                    IEnumerable<CubeDim> ent_Dim = cubeDim_Lookup[ent_DimID];
                    var Calc_Unit_EntList = new List<MemberInfo>();
                    foreach (CubeDim dim in ent_Dim)
                    {
                        var dimTypeID = dim.CubeDimPk.DimTypeId;
                        var ent_DimPk = new DimPk(dimTypeID, ent_DimID);
                        Calc_Unit_EntList = BRApi.Finance.Members.GetMembersUsingFilter(si, ent_DimPk, Calc_Unit_Entity_MFB, true);
                    }

                    foreach (var entity in Calc_Unit_EntList)
                    {
                        var new_DataRow = FMM_CalcUnitConfig_DT.NewRow();
                        if (loop_times == 0)
                        {
                            new_DataRow["CalcUnitID"] = new_CalcUnitID;
                        }
                        {
                            new_DataRow["CalcUnitID"] = ++new_CalcUnitID;
                        }
                        new_DataRow["CubeConfigID"] = cubeConfigID;
                        new_DataRow["Entity_MFB"] = entity.Member.Name;
                        new_DataRow["WFChannel"] = WF_Channels;
                        new_DataRow["Status"] = "Build";
                        new_DataRow["CreateDate"] = DateTime.Now;
                        new_DataRow["CreateUser"] = si.UserName;
                        new_DataRow["UpdateDate"] = DateTime.Now;
                        new_DataRow["UpdateUser"] = si.UserName;

                        FMM_CalcUnitConfig_DT.Rows.Add(new_DataRow);
                        loop_times += 1;
                    }

                    cmdBuilder.UpdateTable(si, "FMM_CalcUnitConfig", FMM_CalcUnitConfig_DT, sqa);

                    return saveResult;
                }
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }

        }

        #region "Check Duplicates"
        private void Dup_Config(string dupProcStep, string configType, [Optional] string ddl_Process, [Optional] XFEditedDataRow modifiedDataRow, [Optional] DataRow row, [Optional] DataRow originalRow, int cubeConfigID = 0, int actConfigID = 0, int unitConfigID = 0, int acctConfigID = 0, int uiConfigID = 0, int custTableConfigID = 0, int modelConfigID = 0, int apprID = 0)
        {
            var config_Key = string.Empty;
            var sql = configType switch
            {
                "ModelConfig" => @"SELECT * FROM FMM_ModelConfig WHERE CubeConfigID = @CubeConfigID AND ActConfigID = @ActConfigID",
                "ActConfig" => @"SELECT * FROM FMM_ActConfig WHERE CubeConfigID = @CubeConfigID",
                "CalcUnitConfig" => @"SELECT * FROM FMM_CalcUnitConfig WHERE CubeConfigID = @CubeConfigID",
                "UnitConfig" => @"SELECT * FROM FMM_UnitConfig WHERE CubeConfigID = @CubeConfigID AND ActConfigID = @ActConfigID",
                "AcctConfig" => @"SELECT * FROM FMM_AcctConfig WHERE CubeConfigID = @CubeConfigID AND ActConfigID = @ActConfigID AND UnitID = @UnitID",
                "ColConfig" => @"SELECT * FROM FMM_Col_Config WHERE CubeConfigID = @CubeConfigID AND ActConfigID = @ActConfigID AND RegConfigID = @RegConfigID",
                "CalcConfig" => @"SELECT * FROM FMM_CalcConfig WHERE CubeConfigID = @CubeConfigID AND ActConfigID = @ActConfigID AND ModelConfigID = @ModelConfigID",
                "ApprConfig" => @"SELECT * FROM FMM_ApprConfigConfig WHERE CubeConfigID = @CubeConfigID",
                "ApprStepConfig" => @"SELECT * FROM FMM_ApprConfigStep_Config WHERE CubeConfigID = @CubeConfigID AND ApprID = @ApprID",
                "RegConfig" => @"SELECT * FROM FMM_UIConfig WHERE CubeConfigID = @CubeConfigID AND ActConfigID = @ActConfigID",
                "CustTableDef" => @"SELECT * FROM FMM_CustTableDef WHERE CustTableID = @CustTableID",
                _ => string.Empty
            };

            var sqlparams = configType switch
            {
                "Model" => new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = cubeConfigID }, new SqlParameter("@ActConfigID", SqlDbType.Int) { Value = actConfigID } },
                "ActConfig" => new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = cubeConfigID } },
                "CalcUnitConfig" => new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = cubeConfigID } },
                "UnitConfig" => new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = cubeConfigID }, new SqlParameter("@ActConfigID", SqlDbType.Int) { Value = actConfigID } },
                "AcctConfig" => new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = cubeConfigID }, new SqlParameter("@ActConfigID", SqlDbType.Int) { Value = actConfigID }, new SqlParameter("@UnitID", SqlDbType.Int) { Value = unitConfigID } },
                "ColConfig" => new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = cubeConfigID }, new SqlParameter("@ActConfigID", SqlDbType.Int) { Value = actConfigID }, new SqlParameter("@RegConfigID", SqlDbType.Int) { Value = uiConfigID } },
                "CalcConfig" => new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = cubeConfigID }, new SqlParameter("@ActConfigID", SqlDbType.Int) { Value = actConfigID }, new SqlParameter("@ModelConfigID", SqlDbType.Int) { Value = modelConfigID } },
                "ApprConfig" => new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = cubeConfigID } },
                "ApprStepConfig" => new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = cubeConfigID }, new SqlParameter("@ApprID", SqlDbType.Int) { Value = apprID } },
                "RegConfig" => new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = cubeConfigID }, new SqlParameter("@ActConfigID", SqlDbType.Int) { Value = actConfigID } },
                "CustTableDef" => new SqlParameter[] { new SqlParameter("@CustTableID", SqlDbType.Int) { Value = custTableConfigID } },
                _ => new SqlParameter[] { }
            };

            switch (dupProcStep)
            {
                case "Initiate":
                    var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                        connection.Open();

                        var sqa = new SqlDataAdapter();
                        var configDT = new DataTable();

                        sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, configDT, sql, sqlparams);

                        var configDict = GetConfigDict(configType);
                        foreach (DataRow configDataRow in configDT.Rows)
                        {
                            config_Key = BuildConfigKey(configType, cubeConfigID, actConfigID, unitConfigID, uiConfigID, custTableConfigID, row: configDataRow);
                            var config_Value = ExtractConfigValue(configType, row: configDataRow);
                            if (!configDict.ContainsKey(config_Key))
                            {
                                configDict.Add(config_Key, config_Value);
                            }
                        }
                    }
                    break;

                case "Update Row":
                    var originalEditedRow = modifiedDataRow?.OriginalDataRow;
                    var currentEditedRow = ddl_Process == "Delete"
                        ? modifiedDataRow?.OriginalDataRow
                        : modifiedDataRow?.ModifiedDataRow;
                    var currentRow = currentEditedRow == null
                        ? row ?? throw new ArgumentNullException(nameof(row), "Dup_Config update requires either XFEditedDataRow or DataRow.")
                        : null;
                    var previousRow = originalEditedRow == null ? originalRow ?? currentRow : null;
                    config_Key = BuildConfigKey(configType, cubeConfigID, actConfigID, unitConfigID, uiConfigID, custTableConfigID, row: currentRow, editedRow: currentEditedRow);
                    var newconfig_Value = ExtractConfigValue(configType, row: currentRow, editedRow: currentEditedRow);
                    var configDict2 = GetConfigDict(configType);

                    if (ddl_Process == "Insert")
                    {
                        configDict2.Add(config_Key, newconfig_Value);
                    }
                    else if (ddl_Process == "Update")
                    {
                        var originalKey = BuildConfigKey(configType, cubeConfigID, actConfigID, unitConfigID, uiConfigID, custTableConfigID, row: previousRow, editedRow: originalEditedRow);
                        var origconfig_Value = ExtractConfigValue(configType, row: previousRow, editedRow: originalEditedRow);
                        if (!originalKey.XFEqualsIgnoreCase(config_Key) && configDict2.ContainsKey(originalKey))
                        {
                            configDict2.Remove(originalKey);
                        }

                        if (origconfig_Value != newconfig_Value || !originalKey.XFEqualsIgnoreCase(config_Key))
                        {
                            if (configDict2.ContainsKey(config_Key))
                            {
                                configDict2.XFSetValue(config_Key, newconfig_Value);
                            }
                            else
                            {
                                configDict2.Add(config_Key, newconfig_Value);
                            }
                        }
                    }
                    else if (ddl_Process == "Delete")
                    {
                        if (configDict2.ContainsKey(config_Key))
                        {
                            configDict2.Remove(config_Key);
                        }
                    }
                    break;
            }
        }

        private string BuildConfigKey(string configType, int cubeConfigID, int actConfigID, int unitConfigID, int uiConfigID, int custTableConfigID, [Optional] DataRow row, [Optional] XFDataRow editedRow)
        {
            return configType switch
            {
                "ModelConfig" => $"{cubeConfigID}|{actConfigID}|{GetKeyValue("ModelConfigID")}",
                "ActConfig" => $"{cubeConfigID}|{GetKeyValue("ActConfigID")}",
                "CalcUnitConfig" => $"{cubeConfigID}|{GetKeyValue("CalcUnitConfigID")}",
                "UnitConfig" => $"{cubeConfigID}|{actConfigID}|{GetKeyValue("UnitID")}",
                "AcctConfig" => $"{cubeConfigID}|{actConfigID}|{unitConfigID}|{GetKeyValue("AcctID")}",
                "ColConfig" => $"{cubeConfigID}|{actConfigID}|{uiConfigID}|{GetKeyValue("Col_ID")}",
                "CalcConfig" => $"{cubeConfigID}|{actConfigID}|{GetKeyValue("ModelConfigID")}|{GetKeyValue("CalcConfigID")}",
                "ApprConfig" => $"{cubeConfigID}|{GetKeyValue("ApprID")}",
                "ApprStepConfig" => $"{cubeConfigID}|{GetKeyValue("ApprID")}|{GetKeyValue("ApprStepID")}",
                "UIConfig" => $"{cubeConfigID}|{actConfigID}|{GetKeyValue("RegConfigID")}",
                "CustTableDef" => $"{custTableConfigID}|{GetKeyValue("CustTableColID")}",
                _ => string.Empty
            };

            int GetKeyValue(string columnName)
            {
                if (row != null)
                {
                    return Convert.ToInt32(row[columnName]);
                }

                if (editedRow != null)
                {
                    return Convert.ToInt32(editedRow[columnName]);
                }

                throw new ArgumentNullException(nameof(row), "BuildConfigKey requires either a DataRow or XFDataRow.");
            }
        }

        private string ExtractConfigValue(string configType, [Optional] DataRow row, [Optional] XFDataRow editedRow)
        {
            object GetValue(string columnName)
            {
                if (row != null)
                {
                    return row[columnName];
                }

                if (editedRow != null)
                {
                    return editedRow[columnName];
                }

                throw new ArgumentNullException(nameof(row), "ExtractConfigValue requires either a DataRow or XFDataRow.");
            }

            string GetStringValue(string columnName)
            {
                var value = GetValue(columnName);
                return value == DBNull.Value ? string.Empty : value.ToString();
            }

            return configType switch
            {
                "Model" => GetStringValue("Name"),
                "ActConfig" => GetStringValue("Name"),
                "CalcUnitConfig" => $"{GetStringValue("Entity_MFB")}|{GetStringValue("WFChannel")}",
                "UnitConfig" => GetStringValue("Name"),
                "AcctConfig" => GetStringValue("Name"),
                "ColConfig" => GetStringValue("Order"),
                "CalcConfig" => GetStringValue("Name"),
                "ApprConfig" => GetStringValue("Name"),
                "ApprStepConfig" => $"{GetStringValue("WFProfile_Step")}|{GetStringValue("StepNum")}|{GetStringValue("UserGroup")}|{GetStringValue("Logic")}|{GetStringValue("Item")}|{GetStringValue("Level")}|{GetStringValue("ApprConfig")}|{GetStringValue("InitStatus")}|{GetStringValue("ApprStatus")}|{GetStringValue("RejStatus")}",
                "RegConfig" => GetStringValue("Name"),
                "CustTableDef" => $"{GetStringValue("ColOrder")}|{GetStringValue("Name")}|{GetStringValue("PrimaryKey")}|{GetStringValue("DataType")}|{GetStringValue("MaxLength")}|{GetStringValue("Precision")}|{GetStringValue("Scale")}|{GetStringValue("IsNullable")}|{GetStringValue("DefaultValue")}",
                _ => string.Empty
            };
        }

        private Dictionary<string, string> GetConfigDict(string configType)
        {
            return configType switch
            {
                "Model" => gbl_Models_Dict,
                "ActConfig" => gbl_ActConfig_Dict,
                "CalcUnitConfig" => gbl_CalcUnitConfig_Dict,
                "UnitConfig" => gbl_UnitConfig_Dict,
                "AcctConfig" => gbl_AcctConfig_Dict,
                "ColConfig" => gbl_Col_Dict,
                "CalcConfig" => gbl_Calc_Dict,
                "ApprConfig" => gbl_Appr_Dict,
                "ApprStepConfig" => gbl_ApprStep_Dict,
                "RegConfig" => gbl_Register_Dict,
                "CustTableDef" => gbl_CustTableDef_Dict,
                _ => new Dictionary<string, string>()
            };
        }
        #endregion

        #region "Col Helpers"

        public void Insert_Col_Default_Rows(int cubeConfigID, int actConfigID, int uiConfigID)
        {
            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                var sqlMaxIdGetter = new SQL_GBL_Get_Max_ID(si, connection);
                var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                var sqa = new SqlDataAdapter();
                var FMM_Col_Config_DT = new DataTable();
                connection.Open();
                int os_Col_ID = sqlMaxIdGetter.Get_Max_ID(si, "FMM_Col_Config", "Col_ID");
                string selectQuery_colSetup = @"
                    SELECT * 
                    FROM FMM_Col_Config 
                    WHERE CubeConfigID = @CubeConfigID
                    AND ActConfigID = @ActConfigID
                    AND RegConfigID = @RegConfigID";

                var parameters_colSetup = new SqlParameter[]
                {
                    new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = cubeConfigID },
                    new SqlParameter("@ActConfigID", SqlDbType.Int) { Value = actConfigID },
                    new SqlParameter("@RegConfigID", SqlDbType.Int) { Value = uiConfigID }
                };
                cmdBuilder.FillDataTable(si, sqa, FMM_Col_Config_DT, selectQuery_colSetup, parameters_colSetup);
                //var columnConfigDefaults = new ColumnConfigDefaults();
                //foreach (var columnConfig in columnConfigDefaults.DefaultColumns)

                cmdBuilder.UpdateTable(si, "FMM_Col_Config", FMM_Col_Config_DT, sqa);
            }
        }

        #endregion



        private XFSelectionChangedTaskResult Select_Add_FMM_CalcConfigID()
        {
            try
            {
                var selectResult = new XFSelectionChangedTaskResult();
                selectResult.ChangeCustomSubstVarsInDashboard = true;
                var optionintValue = 2;
                var gbl_helpers = new GBL_UI_Assembly.GBL_Helpers();
                gbl_helpers.UpdateCustomSubstVar(ref selectResult, "BL_DDM_Config_Menu", string.Empty);

                return selectResult;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSelectionChangedTaskResult ActConfig_Select()
        {
            try
            {
                var selectResult = new XFSelectionChangedTaskResult();
                selectResult.ChangeCustomSubstVarsInDashboard = true;
                var gbl_helpers = new GBL_UI_Assembly.GBL_Helpers();
                var existingCubeConfigID = this.args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("BL_FMM_CubeConfigID", "0").XFConvertToInt();

                gbl_helpers.UpdateCustomSubstVar(ref selectResult, "BL_FMM_CubeConfigID", existingCubeConfigID.XFToString());
                gbl_helpers.UpdateCustomSubstVar(ref selectResult, "IV_FMM_CubeConfig_AddUpdate", "Update");
                gbl_helpers.UpdateCustomSubstVar(ref selectResult, "BL_FMM_CubeConfigID", existingCubeConfigID.XFToString());
                // FMM_ConfigHelpers.SetCubeConfigParams(si, selectResult.ModifiedCustomSubstVars);

                return selectResult;
            }
            catch (Exception ex)
            {
                // Log and rethrow exceptions for error handling.
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }



        private XFDataRow SanitizeRow(XFDataRow dataRow)
        {
            foreach (string key in dataRow.Items.Keys)
            {
                if (dataRow.Items[key] == typeof(string))
                {
                    if (dataRow.Items[key] == null || String.Empty.Equals(dataRow.Items[key]))
                    {
                        dataRow.Items[key] = DBNull.Value;
                    }
                }
            }

            return dataRow;
        }

        #region "Copy Data Helpers"
        private static int GetRowIntValue(DataRow row, string columnName, int defaultValue = -1)
        {
            return row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value
                ? Convert.ToInt32(row[columnName])
                : defaultValue;
        }

        private static ILookup<int, DataRow> BuildRowLookup(DataTable table, string columnName)
        {
            return table.Rows
                .Cast<DataRow>()
                .Where(row => row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value)
                .ToLookup(row => Convert.ToInt32(row[columnName]));
        }

        private void Process_Copy_Cube_Config(ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            var src_CubeConfigID = Convert.ToInt32(args.NameValuePairs.XFGetValue("src_CubeConfigID", "0"));
            var tgt_CubeConfigID = Convert.ToInt32(args.NameValuePairs.XFGetValue("tgt_CubeConfigID", "0"));
            #region "Define Data Tables"
            var src_FMM_ActConfig_DT = new DataTable();
            var tgt_FMM_ActConfig_DT = new DataTable();
            var src_FMM_UnitConfig_DT = new DataTable();
            var tgt_FMM_UnitConfig_DT = new DataTable();
            var src_FMM_AcctConfig_DT = new DataTable();
            var tgt_FMM_AcctConfig_DT = new DataTable();
            var src_FMM_ApprConfig_DT = new DataTable();
            var tgt_FMM_ApprConfig_DT = new DataTable();
            var src_FMM_ApprStepConfig_DT = new DataTable();
            var tgt_FMM_ApprStepConfig_DT = new DataTable();
            var src_FMM_UIConfig_DT = new DataTable();
            var tgt_FMM_UIConfig_DT = new DataTable();
            var src_FMM_Col_Config_DT = new DataTable();
            var tgt_FMM_Col_Config_DT = new DataTable();
            var src_FMM_ModelConfig_DT = new DataTable();
            var tgt_FMM_ModelConfig_DT = new DataTable();
            var src_FMM_CalcConfig_DT = new DataTable();
            var tgt_FMM_CalcConfig_DT = new DataTable();
            var src_FMM_DestCell_DT = new DataTable();
            var tgt_FMM_DestCell_DT = new DataTable();
            var src_FMM_SrcCellConfig_DT = new DataTable();
            var tgt_FMM_SrcCellConfig_DT = new DataTable();
            var src_FMM_ModelGrps_DT = new DataTable();
            var tgt_FMM_ModelGrps_DT = new DataTable();
            var src_FMM_ModelGrpAssign_DT = new DataTable();
            var tgt_FMM_ModelGrpAssign_DT = new DataTable();
            var src_FMM_CalcUnitConfig_DT = new DataTable();
            var tgt_FMM_CalcUnitConfig_DT = new DataTable();
            var src_FMM_CalcConfig_Unit_Assign_DT = new DataTable();
            var tgt_FMM_CalcConfig_Unit_Assign_DT = new DataTable();
            var tgt_FMM_CubeConfig_DT = new DataTable();
            #endregion
            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                #region "Define SQL Adapter Classes"
                var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                var sqa = new SqlDataAdapter();
                var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                #endregion
                connection.Open();
                #region "Get FMM Data"
                get_FMM_ActConfig_Data(
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = src_CubeConfigID } },
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = tgt_CubeConfigID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_ActConfig_DT, ref tgt_FMM_ActConfig_DT, sql_gbl_get_max_id
                );

                get_FMM_UnitConfig_Data(
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = src_CubeConfigID } },
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = tgt_CubeConfigID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_UnitConfig_DT, ref tgt_FMM_UnitConfig_DT, sql_gbl_get_max_id
                );

                get_FMM_AcctConfig_Data(
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = src_CubeConfigID } },
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = tgt_CubeConfigID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_AcctConfig_DT, ref tgt_FMM_AcctConfig_DT, sql_gbl_get_max_id
                );

                get_FMM_ApprConfig_Data(
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = src_CubeConfigID } },
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = tgt_CubeConfigID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_ApprConfig_DT, ref tgt_FMM_ApprConfig_DT, sql_gbl_get_max_id
                );

                get_FMM_ApprConfigStep_Config_Data(
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = src_CubeConfigID } },
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = tgt_CubeConfigID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_ApprStepConfig_DT, ref tgt_FMM_ApprStepConfig_DT, sql_gbl_get_max_id
                );

                get_FMM_UIConfig_Data(
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = src_CubeConfigID } },
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = tgt_CubeConfigID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_UIConfig_DT, ref tgt_FMM_UIConfig_DT, sql_gbl_get_max_id
                );

                get_FMM_Col_Config_Data(
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = src_CubeConfigID } },
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = tgt_CubeConfigID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Col_Config_DT, ref tgt_FMM_Col_Config_DT, sql_gbl_get_max_id
                );

                get_FMM_ModelConfig_Data(
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = src_CubeConfigID } },
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = tgt_CubeConfigID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_ModelConfig_DT, ref tgt_FMM_ModelConfig_DT, sql_gbl_get_max_id
                );

                get_FMM_CalcConfig_Data(
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = src_CubeConfigID } },
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = tgt_CubeConfigID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_CalcConfig_DT, ref tgt_FMM_CalcConfig_DT, sql_gbl_get_max_id
                );

                get_FMM_DestCell_Data(
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = src_CubeConfigID } },
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = tgt_CubeConfigID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_DestCell_DT, ref tgt_FMM_DestCell_DT, sql_gbl_get_max_id
                );

                get_FMM_SrcCellConfig_Data(
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = src_CubeConfigID } },
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = tgt_CubeConfigID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_SrcCellConfig_DT, ref tgt_FMM_SrcCellConfig_DT, sql_gbl_get_max_id
                );

                get_FMM_ModelGrpConfigs_Data(
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = src_CubeConfigID } },
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = tgt_CubeConfigID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_ModelGrps_DT, ref tgt_FMM_ModelGrps_DT, sql_gbl_get_max_id
                );

                get_FMM_ModelGrpAssign_Data(
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = src_CubeConfigID } },
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = tgt_CubeConfigID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_ModelGrpAssign_DT, ref tgt_FMM_ModelGrpAssign_DT, sql_gbl_get_max_id
                );

                get_FMM_CalcUnitConfig_Data(
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = src_CubeConfigID } },
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = tgt_CubeConfigID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_CalcUnitConfig_DT, ref tgt_FMM_CalcUnitConfig_DT, sql_gbl_get_max_id
                );

                get_FMM_CalcConfig_Unit_Assign_Data(
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = src_CubeConfigID } },
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = tgt_CubeConfigID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_CalcConfig_Unit_Assign_DT, ref tgt_FMM_CalcConfig_Unit_Assign_DT, sql_gbl_get_max_id
                );
                #endregion

                var unitsByActConfigID = BuildRowLookup(src_FMM_UnitConfig_DT, "ActConfigID");
                var acctsByUnitId = BuildRowLookup(src_FMM_AcctConfig_DT, "UnitID");
                var regsByActConfigID = BuildRowLookup(src_FMM_UIConfig_DT, "ActConfigID");
                var colsByRegConfigId = BuildRowLookup(src_FMM_Col_Config_DT, "RegConfigID");
                var approvalsByActConfigID = BuildRowLookup(src_FMM_ApprConfig_DT, "ActConfigID");
                var apprStepsByApprId = BuildRowLookup(src_FMM_ApprStepConfig_DT, "ApprID");
                var modelsByActConfigID = BuildRowLookup(src_FMM_ModelConfig_DT, "ActConfigID");
                var calcsByModelConfigID = BuildRowLookup(src_FMM_CalcConfig_DT, "ModelConfigID");
                var destCellsByCalcConfigID = BuildRowLookup(src_FMM_DestCell_DT, "CalcConfigID");
                var srcCellsByCalcConfigID = BuildRowLookup(src_FMM_SrcCellConfig_DT, "CalcConfigID");

                #region "Copy Activity Data"
                foreach (DataRow activity_ConfigRow in src_FMM_ActConfig_DT.Rows)
                {
                    CopyActivities(activity_ConfigRow, ref tgt_FMM_ActConfig_DT, tgt_CubeConfigID, ref XFCopyTaskResult);
                    var curr_srcActivityID = GetRowIntValue(activity_ConfigRow, "ActConfigID");
                    foreach (DataRow UnitConfigRow in unitsByActConfigID[curr_srcActivityID])
                    {
                        var curr_srcUnitID = GetRowIntValue(UnitConfigRow, "UnitID");
                        CopyUnits(UnitConfigRow, ref tgt_FMM_UnitConfig_DT, tgt_CubeConfigID, ref XFCopyTaskResult);
                        foreach (DataRow AcctConfigRow in acctsByUnitId[curr_srcUnitID])
                        {
                            CopyAccts(AcctConfigRow, ref tgt_FMM_AcctConfig_DT, tgt_CubeConfigID, ref XFCopyTaskResult);
                        }
                    }
                    foreach (DataRow RegConfigRow in regsByActConfigID[curr_srcActivityID])
                    {
                        var curr_srcRegisterConfigID = GetRowIntValue(RegConfigRow, "RegConfigID");
                        Copy_RegConfig(RegConfigRow, ref tgt_FMM_UIConfig_DT, tgt_CubeConfigID, ref XFCopyTaskResult);
                        foreach (DataRow col_ConfigRow in colsByRegConfigId[curr_srcRegisterConfigID])
                        {
                            Copy_Col_Config(col_ConfigRow, ref tgt_FMM_Col_Config_DT, tgt_CubeConfigID, ref XFCopyTaskResult);
                        }
                    }
                    foreach (DataRow ConfigRow in approvalsByActConfigID[curr_srcActivityID])
                    {
                        var curr_srcApprovalID = GetRowIntValue(ConfigRow, "ApprID");
                        CopyApprovals(ConfigRow, ref tgt_FMM_ApprConfig_DT, tgt_CubeConfigID, ref XFCopyTaskResult);
                        foreach (DataRow approvalstep_ConfigRow in apprStepsByApprId[curr_srcApprovalID])
                        {
                            Copy_ApprSteps(approvalstep_ConfigRow, ref tgt_FMM_ApprStepConfig_DT, tgt_CubeConfigID, ref XFCopyTaskResult);
                        }
                    }
                    foreach (DataRow models_ConfigRow in modelsByActConfigID[curr_srcActivityID])
                    {
                        var curr_srcModelConfigID = GetRowIntValue(models_ConfigRow, "ModelConfigID");
                        CopyModels(models_ConfigRow, ref tgt_FMM_ModelConfig_DT, tgt_CubeConfigID, ref XFCopyTaskResult);
                        foreach (DataRow CalcConfigRow in calcsByModelConfigID[curr_srcModelConfigID])
                        {
                            var curr_srcCalcConfigID = GetRowIntValue(CalcConfigRow, "CalcConfigID");
                            CopyCalcs(CalcConfigRow, ref tgt_FMM_CalcConfig_DT, tgt_CubeConfigID, ref XFCopyTaskResult, "Calc Copy");
                            foreach (DataRow ConfigRow in destCellsByCalcConfigID[curr_srcCalcConfigID])
                            {
                                CopyCell(ConfigRow, ref tgt_FMM_DestCell_DT, tgt_CubeConfigID, ref XFCopyTaskResult);
                            }
                            foreach (DataRow src_ConfigRow in srcCellsByCalcConfigID[curr_srcCalcConfigID])
                            {
                                Copy_SrcCell(src_ConfigRow, ref tgt_FMM_SrcCellConfig_DT, tgt_CubeConfigID, ref XFCopyTaskResult);
                            }
                        }
                    }
                }
                #endregion
                foreach (var Model_GroupRow in src_FMM_ModelGrps_DT.Rows)
                {

                }
                foreach (var CalcUnitConfigRow in src_FMM_CalcUnitConfig_DT.Rows)
                {

                }
                cmdBuilder.UpdateTable(si, "FMM_ActConfig", tgt_FMM_ActConfig_DT, sqa);
                cmdBuilder.UpdateTable(si, "FMM_UnitConfig", tgt_FMM_UnitConfig_DT, sqa);
                cmdBuilder.UpdateTable(si, "FMM_AcctConfig", tgt_FMM_AcctConfig_DT, sqa);
                cmdBuilder.UpdateTable(si, "FMM_UIConfig", tgt_FMM_UIConfig_DT, sqa);
                cmdBuilder.UpdateTable(si, "FMM_Col_Config", tgt_FMM_Col_Config_DT, sqa);
                cmdBuilder.UpdateTable(si, "FMM_ApprConfigConfig", tgt_FMM_ApprConfig_DT, sqa);
                cmdBuilder.UpdateTable(si, "FMM_ApprConfigStep_Config", tgt_FMM_ApprStepConfig_DT, sqa);
                cmdBuilder.UpdateTable(si, "FMM_ModelConfig", tgt_FMM_ModelConfig_DT, sqa);
                cmdBuilder.UpdateTable(si, "FMM_CalcConfig", tgt_FMM_CalcConfig_DT, sqa);
                cmdBuilder.UpdateTable(si, "FMM_DestCell", tgt_FMM_DestCell_DT, sqa);
                cmdBuilder.UpdateTable(si, "FMM_SrcCell", tgt_FMM_SrcCellConfig_DT, sqa);
                cmdBuilder.UpdateTable(si, "FMM_ModelGrpAssign", tgt_FMM_ModelGrpAssign_DT, sqa);
                cmdBuilder.UpdateTable(si, "FMM_CalcUnitConfig", tgt_FMM_CalcUnitConfig_DT, sqa);
                cmdBuilder.UpdateTable(si, "FMM_CalcConfig_Unit_Assign", tgt_FMM_CalcConfig_Unit_Assign_DT, sqa);
                cmdBuilder.UpdateTable(si, "FMM_ModelGrps", tgt_FMM_ModelGrps_DT, sqa);
                cmdBuilder.UpdateTable(si, "FMM_CubeConfig", tgt_FMM_CubeConfig_DT, sqa);
            }
        }

        private void Process_Copy_ActConfig(ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            var src_CubeConfigID = Convert.ToInt32(args.NameValuePairs.XFGetValue("src_CubeConfigID", "0"));
            var tgt_CubeConfigID = Convert.ToInt32(args.NameValuePairs.XFGetValue("tgt_CubeConfigID", "0"));
            var src_ActConfigID = Convert.ToInt32(args.NameValuePairs.XFGetValue("src_ActConfigID", "0"));
            #region "Define Data Tables"
            var src_FMM_ActConfig_DT = new DataTable();
            var tgt_FMM_ActConfig_DT = new DataTable();
            var src_FMM_UnitConfig_DT = new DataTable();
            var tgt_FMM_UnitConfig_DT = new DataTable();
            var src_FMM_AcctConfig_DT = new DataTable();
            var tgt_FMM_AcctConfig_DT = new DataTable();
            var src_FMM_ApprConfig_DT = new DataTable();
            var tgt_FMM_ApprConfig_DT = new DataTable();
            var src_FMM_ApprStepConfig_DT = new DataTable();
            var tgt_FMM_ApprStepConfig_DT = new DataTable();
            var src_FMM_UIConfig_DT = new DataTable();
            var tgt_FMM_UIConfig_DT = new DataTable();
            var src_FMM_Col_Config_DT = new DataTable();
            var tgt_FMM_Col_Config_DT = new DataTable();
            var src_FMM_ModelConfig_DT = new DataTable();
            var tgt_FMM_ModelConfig_DT = new DataTable();
            var src_FMM_CalcConfig_DT = new DataTable();
            var tgt_FMM_CalcConfig_DT = new DataTable();
            var src_FMM_DestCell_DT = new DataTable();
            var tgt_FMM_DestCell_DT = new DataTable();
            var src_FMM_SrcCellConfig_DT = new DataTable();
            var tgt_FMM_SrcCellConfig_DT = new DataTable();
            var src_FMM_ModelGrps_DT = new DataTable();
            var tgt_FMM_ModelGrps_DT = new DataTable();
            var src_FMM_ModelGrpAssign_DT = new DataTable();
            var tgt_FMM_ModelGrpAssign_DT = new DataTable();
            var src_FMM_CalcUnitConfig_DT = new DataTable();
            var tgt_FMM_CalcUnitConfig_DT = new DataTable();
            var src_FMM_CalcConfig_Unit_Assign_DT = new DataTable();
            var tgt_FMM_CalcConfig_Unit_Assign_DT = new DataTable();
            var src_FMM_CubeConfig_DT = new DataTable();
            var tgt_FMM_CubeConfig_DT = new DataTable();
            #endregion
            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                #region "Define SQL Adapter Classes"
                var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                var sqa = new SqlDataAdapter();
                #endregion
                connection.Open();
                #region "Get FMM Data"
                get_FMM_ActConfig_Data(
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = src_CubeConfigID } },
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = tgt_CubeConfigID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_ActConfig_DT, ref tgt_FMM_ActConfig_DT, sql_gbl_get_max_id
                );

                get_FMM_UnitConfig_Data(
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = src_CubeConfigID } },
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = tgt_CubeConfigID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_UnitConfig_DT, ref tgt_FMM_UnitConfig_DT, sql_gbl_get_max_id
                );

                get_FMM_AcctConfig_Data(
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = src_CubeConfigID } },
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = tgt_CubeConfigID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_AcctConfig_DT, ref tgt_FMM_AcctConfig_DT, sql_gbl_get_max_id
                );

                get_FMM_ApprConfig_Data(
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = src_CubeConfigID } },
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = tgt_CubeConfigID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_ApprConfig_DT, ref tgt_FMM_ApprConfig_DT, sql_gbl_get_max_id
                );

                get_FMM_ApprConfigStep_Config_Data(
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = src_CubeConfigID } },
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = tgt_CubeConfigID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_ApprStepConfig_DT, ref tgt_FMM_ApprStepConfig_DT, sql_gbl_get_max_id
                );

                get_FMM_UIConfig_Data(
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = src_CubeConfigID } },
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = tgt_CubeConfigID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_UIConfig_DT, ref tgt_FMM_UIConfig_DT, sql_gbl_get_max_id
                );

                get_FMM_Col_Config_Data(
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = src_CubeConfigID } },
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = tgt_CubeConfigID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Col_Config_DT, ref tgt_FMM_Col_Config_DT, sql_gbl_get_max_id
                );

                get_FMM_ModelConfig_Data(
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = src_CubeConfigID } },
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = tgt_CubeConfigID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_ModelConfig_DT, ref tgt_FMM_ModelConfig_DT, sql_gbl_get_max_id
                );

                get_FMM_CalcConfig_Data(
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = src_CubeConfigID } },
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = tgt_CubeConfigID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_CalcConfig_DT, ref tgt_FMM_CalcConfig_DT, sql_gbl_get_max_id
                );

                get_FMM_DestCell_Data(
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = src_CubeConfigID } },
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = tgt_CubeConfigID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_DestCell_DT, ref tgt_FMM_DestCell_DT, sql_gbl_get_max_id
                );

                get_FMM_SrcCellConfig_Data(
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = src_CubeConfigID } },
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = tgt_CubeConfigID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_SrcCellConfig_DT, ref tgt_FMM_SrcCellConfig_DT, sql_gbl_get_max_id
                );

                get_FMM_ModelGrpConfigs_Data(
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = src_CubeConfigID } },
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = tgt_CubeConfigID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_ModelGrps_DT, ref tgt_FMM_ModelGrps_DT, sql_gbl_get_max_id
                );

                get_FMM_ModelGrpAssign_Data(
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = src_CubeConfigID } },
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = tgt_CubeConfigID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_ModelGrpAssign_DT, ref tgt_FMM_ModelGrpAssign_DT, sql_gbl_get_max_id
                );

                get_FMM_CalcUnitConfig_Data(
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = src_CubeConfigID } },
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = tgt_CubeConfigID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_CalcUnitConfig_DT, ref tgt_FMM_CalcUnitConfig_DT, sql_gbl_get_max_id
                );

                get_FMM_CalcConfig_Unit_Assign_Data(
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = src_CubeConfigID } },
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = tgt_CubeConfigID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_CalcConfig_Unit_Assign_DT, ref tgt_FMM_CalcConfig_Unit_Assign_DT, sql_gbl_get_max_id
                );
                #endregion

                var unitsByActConfigID = BuildRowLookup(src_FMM_UnitConfig_DT, "ActConfigID");
                var acctsByUnitId = BuildRowLookup(src_FMM_AcctConfig_DT, "UnitID");
                var regsByActConfigID = BuildRowLookup(src_FMM_UIConfig_DT, "ActConfigID");
                var colsByRegConfigId = BuildRowLookup(src_FMM_Col_Config_DT, "RegConfigID");
                var approvalsByActConfigID = BuildRowLookup(src_FMM_ApprConfig_DT, "ActConfigID");
                var apprStepsByApprId = BuildRowLookup(src_FMM_ApprStepConfig_DT, "ApprID");
                var modelsByActConfigID = BuildRowLookup(src_FMM_ModelConfig_DT, "ActConfigID");
                var calcsByModelConfigID = BuildRowLookup(src_FMM_CalcConfig_DT, "ModelConfigID");
                var destCellsByCalcConfigID = BuildRowLookup(src_FMM_DestCell_DT, "CalcConfigID");
                var srcCellsByCalcConfigID = BuildRowLookup(src_FMM_SrcCellConfig_DT, "CalcConfigID");

                #region "Copy Activity Data"
                foreach (DataRow activity_ConfigRow in src_FMM_ActConfig_DT.Rows)
                {
                    CopyActivities(activity_ConfigRow, ref tgt_FMM_ActConfig_DT, tgt_CubeConfigID, ref XFCopyTaskResult);
                    var curr_srcActivityID = GetRowIntValue(activity_ConfigRow, "ActConfigID");
                    foreach (DataRow UnitConfigRow in unitsByActConfigID[curr_srcActivityID])
                    {
                        var curr_srcUnitID = GetRowIntValue(UnitConfigRow, "UnitID");
                        CopyUnits(UnitConfigRow, ref tgt_FMM_UnitConfig_DT, tgt_CubeConfigID, ref XFCopyTaskResult);
                        foreach (DataRow AcctConfigRow in acctsByUnitId[curr_srcUnitID])
                        {
                            CopyAccts(AcctConfigRow, ref tgt_FMM_AcctConfig_DT, tgt_CubeConfigID, ref XFCopyTaskResult);
                        }
                    }
                    foreach (DataRow RegConfigRow in regsByActConfigID[curr_srcActivityID])
                    {
                        var curr_srcRegisterConfigID = GetRowIntValue(RegConfigRow, "RegConfigID");
                        Copy_RegConfig(RegConfigRow, ref tgt_FMM_UIConfig_DT, tgt_CubeConfigID, ref XFCopyTaskResult);
                        foreach (DataRow col_ConfigRow in colsByRegConfigId[curr_srcRegisterConfigID])
                        {
                            Copy_Col_Config(col_ConfigRow, ref tgt_FMM_Col_Config_DT, tgt_CubeConfigID, ref XFCopyTaskResult);
                        }
                    }
                    foreach (DataRow ConfigRow in approvalsByActConfigID[curr_srcActivityID])
                    {
                        var curr_srcApprovalID = GetRowIntValue(ConfigRow, "ApprID");
                        CopyApprovals(ConfigRow, ref tgt_FMM_ApprConfig_DT, tgt_CubeConfigID, ref XFCopyTaskResult);
                        foreach (DataRow approvalstep_ConfigRow in apprStepsByApprId[curr_srcApprovalID])
                        {
                            Copy_ApprSteps(approvalstep_ConfigRow, ref tgt_FMM_ApprStepConfig_DT, tgt_CubeConfigID, ref XFCopyTaskResult);
                        }
                    }
                    foreach (DataRow models_ConfigRow in modelsByActConfigID[curr_srcActivityID])
                    {
                        var curr_srcModelConfigID = GetRowIntValue(models_ConfigRow, "ModelConfigID");
                        CopyModels(models_ConfigRow, ref tgt_FMM_ModelConfig_DT, tgt_CubeConfigID, ref XFCopyTaskResult);
                        foreach (DataRow CalcConfigRow in calcsByModelConfigID[curr_srcModelConfigID])
                        {
                            var curr_srcCalcConfigID = GetRowIntValue(CalcConfigRow, "CalcConfigID");
                            CopyCalcs(CalcConfigRow, ref tgt_FMM_CalcConfig_DT, tgt_CubeConfigID, ref XFCopyTaskResult, "Calc Copy");
                            foreach (DataRow ConfigRow in destCellsByCalcConfigID[curr_srcCalcConfigID])
                            {
                                CopyCell(ConfigRow, ref tgt_FMM_DestCell_DT, tgt_CubeConfigID, ref XFCopyTaskResult);
                            }
                            foreach (DataRow src_ConfigRow in srcCellsByCalcConfigID[curr_srcCalcConfigID])
                            {
                                Copy_SrcCell(src_ConfigRow, ref tgt_FMM_SrcCellConfig_DT, tgt_CubeConfigID, ref XFCopyTaskResult);
                            }
                        }
                    }
                }
                #endregion
                foreach (var Model_GroupRow in src_FMM_ModelGrps_DT.Rows)
                {

                }
                foreach (var CalcUnitConfigRow in src_FMM_CalcUnitConfig_DT.Rows)
                {

                }

                cmdBuilder.UpdateTable(si, "FMM_ActConfig", tgt_FMM_ActConfig_DT, sqa);
                cmdBuilder.UpdateTable(si, "FMM_UnitConfig", tgt_FMM_UnitConfig_DT, sqa);
                cmdBuilder.UpdateTable(si, "FMM_AcctConfig", tgt_FMM_AcctConfig_DT, sqa);
                cmdBuilder.UpdateTable(si, "FMM_UIConfig", tgt_FMM_UIConfig_DT, sqa);
                cmdBuilder.UpdateTable(si, "FMM_Col_Config", tgt_FMM_Col_Config_DT, sqa);
                cmdBuilder.UpdateTable(si, "FMM_ApprConfigConfig", tgt_FMM_ApprConfig_DT, sqa);
                cmdBuilder.UpdateTable(si, "FMM_ApprConfigStep_Config", tgt_FMM_ApprStepConfig_DT, sqa);
                cmdBuilder.UpdateTable(si, "FMM_ModelConfig", tgt_FMM_ModelConfig_DT, sqa);
                cmdBuilder.UpdateTable(si, "FMM_CalcConfig", tgt_FMM_CalcConfig_DT, sqa);
                cmdBuilder.UpdateTable(si, "FMM_DestCell", tgt_FMM_DestCell_DT, sqa);
                cmdBuilder.UpdateTable(si, "FMM_SrcCell", tgt_FMM_SrcCellConfig_DT, sqa);
                cmdBuilder.UpdateTable(si, "FMM_ModelGrpAssign", tgt_FMM_ModelGrpAssign_DT, sqa);
                cmdBuilder.UpdateTable(si, "FMM_CalcUnitConfig", tgt_FMM_CalcUnitConfig_DT, sqa);
                cmdBuilder.UpdateTable(si, "FMM_CalcConfig_Unit_Assign", tgt_FMM_CalcConfig_Unit_Assign_DT, sqa);
                cmdBuilder.UpdateTable(si, "FMM_ModelGrps", tgt_FMM_ModelGrps_DT, sqa);
                cmdBuilder.UpdateTable(si, "FMM_CubeConfig", tgt_FMM_CubeConfig_DT, sqa);
            }
        }

        private XFSelectionChangedTaskResult Copy_Model_Config(XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            var srcActivities_DT = new DataTable("srcActivities");
            var srcWF_DataUnits_DT = new DataTable("srcWF_DataUnits");
            var srcRegisters_DT = new DataTable("srcRegisters");
            var srcApprovals_DT = new DataTable("srcApprovals");
            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                connection.Open();
                var srcActivities_sqlDataAdapter = new SqlDataAdapter();
                string srcActivities_selectQuery = @"
                    SELECT CubeConfigID, Name
                    FROM Cube
                    WHERE IsTopLevelCube = 1";
                var srcActivities_parameters = new SqlParameter[]
                {
                };
                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, srcActivities_sqlDataAdapter, srcActivities_DT, srcActivities_selectQuery, srcActivities_parameters);

            }
            return XFCopyTaskResult;
        }

        private void Process_Calc_Copy(ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            var src_CubeConfigID = Convert.ToInt32(args.NameValuePairs.XFGetValue("src_CubeConfigID", "0"));
            var tgt_CubeConfigID = Convert.ToInt32(args.NameValuePairs.XFGetValue("tgt_CubeConfigID", "0"));
            var src_ActConfigID = Convert.ToInt32(args.NameValuePairs.XFGetValue("src_ActConfigID", "0"));
            var tgt_ActConfigID = Convert.ToInt32(args.NameValuePairs.XFGetValue("tgt_ActConfigID", "0"));
            var src_ModelConfigID = Convert.ToInt32(args.NameValuePairs.XFGetValue("src_ModelConfigID", "0"));
            var tgt_ModelConfigID = Convert.ToInt32(args.NameValuePairs.XFGetValue("tgt_ModelConfigID", "0"));
            var src_CalcConfigIDs = Convert.ToInt32(args.NameValuePairs.XFGetValue("src_CalcConfigIDs", "0"));
            gbl_CurrActConfigID = tgt_ActConfigID;
            gbl_CurrModelConfigID = tgt_ModelConfigID;
            #region "Define Data Tables"
            var src_FMM_CalcConfig_DT = new DataTable();
            var tgt_FMM_CalcConfig_DT = new DataTable();
            var src_FMM_DestCell_DT = new DataTable();
            var tgt_FMM_DestCell_DT = new DataTable();
            var src_FMM_SrcCellConfig_DT = new DataTable();
            var tgt_FMM_SrcCellConfig_DT = new DataTable();
            #endregion
            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                #region "Define SQL Adapter Classes"
                var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                var sql_gbl_get_max_id = new SQL_GBL_Get_Max_ID(si, connection);
                var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                var sqa = new SqlDataAdapter();
                #endregion
                connection.Open();
                #region "Get MCM Data"

                get_FMM_CalcConfig_Data(
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = src_CubeConfigID },
                                         new SqlParameter("@ActConfigID", SqlDbType.Int) { Value = src_ActConfigID },
                                         new SqlParameter("@ModelConfigID", SqlDbType.Int) { Value = src_ModelConfigID },
                                         new SqlParameter("@CalcConfigID", SqlDbType.NVarChar) {Value = src_CalcConfigIDs }},
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = tgt_CubeConfigID },
                                         new SqlParameter("@ActConfigID", SqlDbType.Int) { Value = tgt_ActConfigID },
                                         new SqlParameter("@ModelConfigID", SqlDbType.Int) { Value = tgt_ModelConfigID }},
                    sql_gbl_get_datasets,
                    ref src_FMM_CalcConfig_DT, ref tgt_FMM_CalcConfig_DT, sql_gbl_get_max_id
                );

                get_FMM_DestCell_Data(
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = src_CubeConfigID },
                                         new SqlParameter("@ActConfigID", SqlDbType.Int) { Value = src_ActConfigID },
                                         new SqlParameter("@ModelConfigID", SqlDbType.Int) { Value = src_ModelConfigID },
                                         new SqlParameter("@CalcConfigID", SqlDbType.NVarChar) {Value = src_CalcConfigIDs }},
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = tgt_CubeConfigID },
                                         new SqlParameter("@ActConfigID", SqlDbType.Int) { Value = tgt_ActConfigID },
                                         new SqlParameter("@ModelConfigID", SqlDbType.Int) { Value = tgt_ModelConfigID }},
                    sql_gbl_get_datasets,
                    ref src_FMM_DestCell_DT, ref tgt_FMM_DestCell_DT, sql_gbl_get_max_id
                );

                get_FMM_SrcCellConfig_Data(
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = src_CubeConfigID },
                                         new SqlParameter("@ActConfigID", SqlDbType.Int) { Value = src_ActConfigID },
                                         new SqlParameter("@ModelConfigID", SqlDbType.Int) { Value = src_ModelConfigID },
                                         new SqlParameter("@CalcConfigID", SqlDbType.NVarChar) {Value = src_CalcConfigIDs }},
                    new SqlParameter[] { new SqlParameter("@CubeConfigID", SqlDbType.Int) { Value = tgt_CubeConfigID },
                                         new SqlParameter("@ActConfigID", SqlDbType.Int) { Value = tgt_ActConfigID },
                                         new SqlParameter("@ModelConfigID", SqlDbType.Int) { Value = tgt_ModelConfigID }},
                    sql_gbl_get_datasets,
                    ref src_FMM_SrcCellConfig_DT, ref tgt_FMM_SrcCellConfig_DT, sql_gbl_get_max_id
                );

                #endregion

                var destCellsByCalcConfigID = BuildRowLookup(src_FMM_DestCell_DT, "CalcConfigID");
                var srcCellsByCalcConfigID = BuildRowLookup(src_FMM_SrcCellConfig_DT, "CalcConfigID");

                #region "Copy Calc Data"
                foreach (DataRow CalcConfigRow in src_FMM_CalcConfig_DT.Rows)
                {
                    var curr_srcCalcConfigID = GetRowIntValue(CalcConfigRow, "CalcConfigID");
                    CopyCalcs(CalcConfigRow, ref tgt_FMM_CalcConfig_DT, tgt_CubeConfigID, ref XFCopyTaskResult, "Model Calc Copy");
                    foreach (DataRow ConfigRow in destCellsByCalcConfigID[curr_srcCalcConfigID])
                    {
                        CopyCell(ConfigRow, ref tgt_FMM_DestCell_DT, tgt_CubeConfigID, ref XFCopyTaskResult);
                    }
                    foreach (DataRow src_ConfigRow in srcCellsByCalcConfigID[curr_srcCalcConfigID])
                    {
                        Copy_SrcCell(src_ConfigRow, ref tgt_FMM_SrcCellConfig_DT, tgt_CubeConfigID, ref XFCopyTaskResult);
                    }
                }
                #endregion

                cmdBuilder.UpdateTable(si, "FMM_CalcConfig", tgt_FMM_CalcConfig_DT, sqa);
                cmdBuilder.UpdateTable(si, "FMM_DestCell", tgt_FMM_DestCell_DT, sqa);
                cmdBuilder.UpdateTable(si, "FMM_SrcCell", tgt_FMM_SrcCellConfig_DT, sqa);

            }
        }

        private void get_FMM_ActConfig_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, ref DataTable src_FMM_ActConfig_DT, ref DataTable tgt_FMM_ActConfig_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            var whereClause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"SELECT * FROM FMM_ActConfig {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_ActConfig_DT, sql, src_sqlparams);

            whereClause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"SELECT * FROM FMM_ActConfig {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, tgt_FMM_ActConfig_DT, sql, tgt_sqlparams);

            gbl_ActConfigID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_ActConfig", "ActConfigID");
        }

        private void get_FMM_UnitConfig_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, ref DataTable src_FMM_UnitConfig_DT, ref DataTable tgt_FMM_UnitConfig_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            var whereClause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"SELECT * FROM FMM_UnitConfig {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_UnitConfig_DT, sql, src_sqlparams);

            whereClause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"SELECT * FROM FMM_UnitConfig {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, tgt_FMM_UnitConfig_DT, sql, tgt_sqlparams);
            gbl_UnitID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_UnitConfig", "UnitID");
        }

        private void get_FMM_AcctConfig_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, ref DataTable src_FMM_AcctConfig_DT, ref DataTable tgt_FMM_AcctConfig_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            var whereClause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"SELECT * FROM FMM_AcctConfig {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_AcctConfig_DT, sql, src_sqlparams);

            whereClause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"SELECT * FROM FMM_AcctConfig {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, tgt_FMM_AcctConfig_DT, sql, tgt_sqlparams);
            gbl_AcctID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_AcctConfig", "AcctID");
        }

        private void get_FMM_ApprConfig_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, ref DataTable src_FMM_ApprConfig_DT, ref DataTable tgt_FMM_ApprConfig_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            var whereClause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"SELECT * FROM FMM_ApprConfigConfig {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_ApprConfig_DT, sql, src_sqlparams);

            whereClause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"SELECT * FROM FMM_ApprConfigConfig {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, tgt_FMM_ApprConfig_DT, sql, tgt_sqlparams);
            gbl_ApprID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_ApprConfigConfig", "ApprID");
        }

        private void get_FMM_ApprConfigStep_Config_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, ref DataTable src_FMM_ApprStepConfig_DT, ref DataTable tgt_FMM_ApprStepConfig_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            var whereClause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"SELECT * FROM FMM_ApprConfigStep_Config {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_ApprStepConfig_DT, sql, src_sqlparams);

            whereClause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"SELECT * FROM FMM_ApprConfigStep_Config {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, tgt_FMM_ApprStepConfig_DT, sql, tgt_sqlparams);

            gbl_ApprStepID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_ApprConfigStep_Config", "ApprStepID");
        }

        private void get_FMM_UIConfig_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, ref DataTable src_FMM_UIConfig_DT, ref DataTable tgt_FMM_UIConfig_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            var whereClause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"SELECT * FROM FMM_UIConfig {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_UIConfig_DT, sql, src_sqlparams);

            whereClause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"SELECT * FROM FMM_UIConfig {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, tgt_FMM_UIConfig_DT, sql, tgt_sqlparams);

            gbl_RegConfigID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_UIConfig", "RegConfigID");
        }

        private void get_FMM_Col_Config_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, ref DataTable src_FMM_Col_Config_DT, ref DataTable tgt_FMM_Col_Config_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            var whereClause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"SELECT * FROM FMM_Col_Config {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_Col_Config_DT, sql, src_sqlparams);

            whereClause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"SELECT * FROM FMM_Col_Config {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, tgt_FMM_Col_Config_DT, sql, tgt_sqlparams);

            gbl_ColID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Col_Config", "Col_ID");
        }

        private void get_FMM_ModelConfig_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, ref DataTable src_FMM_ModelConfig_DT, ref DataTable tgt_FMM_ModelConfig_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            var whereClause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"SELECT * FROM FMM_ModelConfig {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_ModelConfig_DT, sql, src_sqlparams);

            whereClause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"SELECT * FROM FMM_ModelConfig {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, tgt_FMM_ModelConfig_DT, sql, tgt_sqlparams);

            gbl_ModelConfigID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_ModelConfig", "ModelConfigID");
        }

        private void get_FMM_CalcConfig_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, ref DataTable src_FMM_CalcConfig_DT, ref DataTable tgt_FMM_CalcConfig_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            var whereClause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"SELECT * FROM FMM_CalcConfig {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_CalcConfig_DT, sql, src_sqlparams);

            whereClause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"SELECT * FROM FMM_CalcConfig {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, tgt_FMM_CalcConfig_DT, sql, tgt_sqlparams);

            gbl_CalcConfigID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_CalcConfig", "CalcConfigID");
        }

        private void get_FMM_DestCell_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, ref DataTable src_FMM_DestCell_DT, ref DataTable tgt_FMM_DestCell_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            var whereClause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"SELECT * FROM FMM_DestCell {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_DestCell_DT, sql, src_sqlparams);

            whereClause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"SELECT * FROM FMM_DestCell {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, tgt_FMM_DestCell_DT, sql, tgt_sqlparams);

            gbl_DestCellID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_DestCell", "DestCell_ID");
        }

        private void get_FMM_SrcCellConfig_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, ref DataTable src_FMM_SrcCellConfig_DT, ref DataTable tgt_FMM_SrcCellConfig_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            var whereClause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"SELECT * FROM FMM_SrcCell {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_SrcCellConfig_DT, sql, src_sqlparams);

            whereClause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"SELECT * FROM FMM_SrcCell {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, tgt_FMM_SrcCellConfig_DT, sql, tgt_sqlparams);

            gbl_SrcCellID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_SrcCell", "CellID");
        }

        private void get_FMM_ModelGrpConfigs_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, ref DataTable src_FMM_ModelGrps_DT, ref DataTable tgt_FMM_ModelGrps_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            var whereClause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"SELECT * FROM FMM_ModelGrps {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_ModelGrps_DT, sql, src_sqlparams);

            whereClause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"SELECT * FROM FMM_ModelGrps {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, tgt_FMM_ModelGrps_DT, sql, tgt_sqlparams);

            gbl_ModelGrpID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_ModelGrps", "ModelGrpConfigID");
        }

        private void get_FMM_ModelGrpAssign_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, ref DataTable src_FMM_ModelGrpAssign_DT, ref DataTable tgt_FMM_ModelGrpAssign_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            var whereClause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"SELECT * FROM FMM_ModelGrpAssign {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_ModelGrpAssign_DT, sql, src_sqlparams);

            whereClause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"SELECT * FROM FMM_ModelGrpAssign {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, tgt_FMM_ModelGrpAssign_DT, sql, tgt_sqlparams);

            gbl_Model_Grp_Assign_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_ModelGrpAssign", "Model_Grp_Assign_ID");
        }

        private void get_FMM_CalcUnitConfig_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, ref DataTable src_FMM_CalcUnitConfig_DT, ref DataTable tgt_FMM_CalcUnitConfig_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            var whereClause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"SELECT * FROM FMM_CalcUnitConfig {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_CalcUnitConfig_DT, sql, src_sqlparams);

            whereClause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"SELECT * FROM FMM_CalcUnitConfig {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, tgt_FMM_CalcUnitConfig_DT, sql, tgt_sqlparams);

            gbl_CalcUnitID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_CalcUnitConfig", "CalcUnitID");
        }

        private void get_FMM_CalcConfig_Unit_Assign_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, ref DataTable src_FMM_CalcConfig_Unit_Assign_DT, ref DataTable tgt_FMM_CalcConfig_Unit_Assign_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            var whereClause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"SELECT * FROM FMM_CalcConfig_Unit_Assign {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_CalcConfig_Unit_Assign_DT, sql, src_sqlparams);

            whereClause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"SELECT * FROM FMM_CalcConfig_Unit_Assign {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, tgt_FMM_CalcConfig_Unit_Assign_DT, sql, tgt_sqlparams);

            gbl_Calc_Unit_Assign_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_CalcConfig_Unit_Assign", "Calc_Unit_Assign_ID");
        }

        private void CopyActivities(DataRow src_FMM_ActConfig_Row, ref DataTable tgt_FMM_ActConfig_DT, int targetCubeConfigID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            var row_Status = "Build";
            bool isDuplicate = tgt_FMM_ActConfig_DT.AsEnumerable()
                .Any(row => row["Name"].ToString() == src_FMM_ActConfig_Row["Name"].ToString() &&
                            row["CalcType"].ToString() == src_FMM_ActConfig_Row["CalcType"].ToString());

            if (!isDuplicate)
            {
                gbl_ActConfigID += 1;
                gbl_CurrActConfigID = gbl_ActConfigID;
                DataRow new_DestDataRow = tgt_FMM_ActConfig_DT.NewRow();

                new_DestDataRow["CubeConfigID"] = targetCubeConfigID;
                new_DestDataRow["ActConfigID"] = gbl_ActConfigID;
                new_DestDataRow["Name"] = src_FMM_ActConfig_Row.Field<string>("Name") ?? string.Empty;
                new_DestDataRow["CalcType"] = src_FMM_ActConfig_Row.Field<string>("CalcType") ?? string.Empty;
                new_DestDataRow["Status"] = row_Status; // Set initial status as "Build"
                new_DestDataRow["CreateDate"] = DateTime.Now;
                new_DestDataRow["CreateUser"] = si.UserName; // Set the appropriate user context
                new_DestDataRow["UpdateDate"] = DateTime.Now;
                new_DestDataRow["UpdateUser"] = si.UserName; // Set the appropriate user context

                tgt_FMM_ActConfig_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                DataRow existing_DataRow = tgt_FMM_ActConfig_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_FMM_ActConfig_Row["Name"].ToString() &&
                                           row["CalcType"].ToString() == src_FMM_ActConfig_Row["CalcType"].ToString());
                if (existing_DataRow != null)
                {
                    gbl_CurrActConfigID = existing_DataRow.Field<int>("ActConfigID");
                    existing_DataRow["Status"] = row_Status; // Update status or other fields as needed
                    existing_DataRow["UpdateDate"] = DateTime.Now;
                    existing_DataRow["UpdateUser"] = si.UserName; // Set the appropriate user context
                }
            }
        }
        private void CopyUnits(DataRow src_FMM_UnitConfig_Row, ref DataTable tgt_FMM_UnitConfig_DT, int targetCubeConfigID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            bool isDuplicate = tgt_FMM_UnitConfig_DT.AsEnumerable()
                .Any(row => row["Name"].ToString() == src_FMM_UnitConfig_Row["Name"].ToString() &&
                            row["ActConfigID"].ToString() == gbl_CurrActConfigID.ToString());

            if (!isDuplicate)
            {
                gbl_UnitID += 1;
                gbl_CurrUnitID = gbl_UnitID;
                DataRow new_DestDataRow = tgt_FMM_UnitConfig_DT.NewRow();

                new_DestDataRow["CubeConfigID"] = targetCubeConfigID;
                new_DestDataRow["ActConfigID"] = gbl_CurrActConfigID;
                new_DestDataRow["UnitID"] = gbl_UnitID;
                new_DestDataRow["Name"] = src_FMM_UnitConfig_Row.Field<string>("Name") ?? string.Empty;
                new_DestDataRow["CreateDate"] = DateTime.Now;
                new_DestDataRow["CreateUser"] = si.UserName; // Set appropriate user context
                new_DestDataRow["UpdateDate"] = DateTime.Now;
                new_DestDataRow["UpdateUser"] = si.UserName; // Set appropriate user context

                tgt_FMM_UnitConfig_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                DataRow existing_DataRow = tgt_FMM_UnitConfig_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_FMM_UnitConfig_Row["Name"].ToString() &&
                                           row["ActConfigID"].ToString() == gbl_CurrActConfigID.ToString());

                if (existing_DataRow != null)
                {
                    gbl_CurrUnitID = existing_DataRow.Field<int>("UnitID");
                    existing_DataRow["UpdateDate"] = DateTime.Now;
                    existing_DataRow["UpdateUser"] = si.UserName; // Set appropriate user context
                }
            }
        }
        private void CopyAccts(DataRow src_FMM_AcctConfig_Row, ref DataTable tgt_FMM_AcctConfig_DT, int targetCubeConfigID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            bool isDuplicate = tgt_FMM_AcctConfig_DT.AsEnumerable()
                .Any(row => row["Name"].ToString() == src_FMM_AcctConfig_Row["Name"].ToString() &&
                            row["ActConfigID"].ToString() == gbl_CurrActConfigID.ToString() &&
                            row["UnitID"].ToString() == gbl_CurrUnitID.ToString());

            if (!isDuplicate)
            {
                gbl_AcctID += 1;
                gbl_CurrAcctID = gbl_AcctID;
                DataRow new_DestDataRow = tgt_FMM_AcctConfig_DT.NewRow();

                new_DestDataRow["CubeConfigID"] = targetCubeConfigID;
                new_DestDataRow["ActConfigID"] = gbl_CurrActConfigID;
                new_DestDataRow["UnitID"] = gbl_CurrUnitID;
                new_DestDataRow["AcctID"] = gbl_AcctID;
                new_DestDataRow["Name"] = src_FMM_AcctConfig_Row.Field<string>("AcctName") ?? string.Empty; // Handle nulls
                new_DestDataRow["MapLogic"] = src_FMM_AcctConfig_Row.Field<string>("MapLogic") ?? string.Empty; // Handle nulls
                new_DestDataRow["CreateDate"] = DateTime.Now;
                new_DestDataRow["CreateUser"] = si.UserName; new_DestDataRow["UpdateDate"] = DateTime.Now;
                new_DestDataRow["UpdateUser"] = si.UserName;
                tgt_FMM_AcctConfig_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                DataRow existing_DataRow = tgt_FMM_AcctConfig_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_FMM_AcctConfig_Row["Name"].ToString() &&
                                           row["ActConfigID"].ToString() == gbl_CurrActConfigID.ToString() &&
                                           row["UnitID"].ToString() == gbl_CurrUnitID.ToString());

                if (existing_DataRow != null)
                {
                    gbl_CurrAcctID = existing_DataRow.Field<int>("AcctID");

                    existing_DataRow["MapLogic"] = src_FMM_AcctConfig_Row.Field<string>("MapLogic") ?? existing_DataRow.Field<string>("MapLogic");
                    existing_DataRow["UpdateDate"] = DateTime.Now;
                    existing_DataRow["UpdateUser"] = si.UserName;
                }
            }
        }
        private void Copy_RegConfig(DataRow src_RegConfig_Row, ref DataTable tgt_RegConfig_DT, int targetCubeConfigID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            var row_Status = "Build";
            bool isDuplicate = tgt_RegConfig_DT.AsEnumerable()
                .Any(row => row["Name"].ToString() == src_RegConfig_Row["Name"].ToString() &&
                            row["ActConfigID"].ToString() == gbl_CurrActConfigID.ToString());

            if (!isDuplicate)
            {
                gbl_RegConfigID += 1;
                gbl_CurrRegConfigID = gbl_RegConfigID;
                DataRow new_DestDataRow = tgt_RegConfig_DT.NewRow();

                new_DestDataRow["CubeConfigID"] = targetCubeConfigID;
                new_DestDataRow["ActConfigID"] = gbl_CurrActConfigID;
                new_DestDataRow["RegConfigID"] = gbl_RegConfigID;
                new_DestDataRow["Name"] = src_RegConfig_Row.Field<string>("Name") ?? string.Empty; // Handle nulls
                new_DestDataRow["TimePhasing"] = src_RegConfig_Row.Field<string>("TimePhasing") ?? string.Empty; // Handle nulls
                new_DestDataRow["StartDtSrc"] = src_RegConfig_Row.Field<string>("StartDtSrc") ?? string.Empty; // Handle nulls
                new_DestDataRow["EndDtSrc"] = src_RegConfig_Row.Field<string>("EndDtSrc") ?? string.Empty; // Handle nulls
                new_DestDataRow["ApprConfig"] = src_RegConfig_Row.Field<string>("ApprConfig") ?? string.Empty; // Handle nulls
                new_DestDataRow["Status"] = row_Status; // Set initial status as appropriate
                new_DestDataRow["CreateDate"] = DateTime.Now;
                new_DestDataRow["CreateUser"] = si.UserName; // Set appropriate user context
                new_DestDataRow["UpdateDate"] = DateTime.Now;
                new_DestDataRow["UpdateUser"] = si.UserName; // Set appropriate user context

                tgt_RegConfig_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                DataRow existing_DataRow = tgt_RegConfig_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_RegConfig_Row["Name"].ToString() &&
                                           row["ActConfigID"].ToString() == gbl_CurrActConfigID.ToString());

                if (existing_DataRow != null)
                {
                    gbl_CurrRegConfigID = existing_DataRow.Field<int>("RegConfigID");

                    existing_DataRow["TimePhasing"] = src_RegConfig_Row.Field<string>("TimePhasing") ?? existing_DataRow.Field<string>("TimePhasing");
                    existing_DataRow["StartDtSrc"] = src_RegConfig_Row.Field<string>("StartDtSrc") ?? existing_DataRow.Field<string>("StartDtSrc");
                    existing_DataRow["EndDtSrc"] = src_RegConfig_Row.Field<string>("EndDtSrc") ?? existing_DataRow.Field<string>("EndDtSrc");
                    existing_DataRow["ApprConfig"] = src_RegConfig_Row.Field<string>("ApprConfig") ?? existing_DataRow.Field<string>("ApprConfig");
                    existing_DataRow["Status"] = row_Status;
                    existing_DataRow["UpdateDate"] = DateTime.Now;
                    existing_DataRow["UpdateUser"] = si.UserName; // Set appropriate user context
                }
            }
        }
        private void Copy_Col_Config(DataRow src_Col_Config_Row, ref DataTable tgt_Col_Config_DT, int targetCubeConfigID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            var row_Status = "Build";
            bool isDuplicate = tgt_Col_Config_DT.AsEnumerable()
                .Any(row => row["Name"].ToString() == src_Col_Config_Row["Name"].ToString() &&
                            row["ActConfigID"].ToString() == gbl_CurrActConfigID.ToString() &&
                            row["RegConfigID"].ToString() == gbl_CurrRegConfigID.ToString());

            if (!isDuplicate)
            {
                gbl_ColID += 1;
                gbl_CurrColID = gbl_ColID;
                DataRow new_DestDataRow = tgt_Col_Config_DT.NewRow();
                new_DestDataRow["CubeConfigID"] = targetCubeConfigID;
                new_DestDataRow["ActConfigID"] = gbl_CurrActConfigID;
                new_DestDataRow["RegConfigID"] = gbl_CurrRegConfigID;
                new_DestDataRow["Col_ID"] = gbl_ColID;
                new_DestDataRow["Name"] = src_Col_Config_Row.Field<string>("Name") ?? string.Empty;
                new_DestDataRow["InUse"] = src_Col_Config_Row.Field<bool?>("InUse") ?? false;
                new_DestDataRow["Required"] = src_Col_Config_Row.Field<bool?>("Required") ?? false;
                new_DestDataRow["Updates"] = src_Col_Config_Row.Field<bool?>("Updates") ?? false;
                new_DestDataRow["Alias"] = src_Col_Config_Row.Field<string>("Alias") ?? string.Empty;
                new_DestDataRow["Order"] = src_Col_Config_Row.Field<int?>("Order") ?? 0;
                new_DestDataRow["Default"] = src_Col_Config_Row.Field<string>("Default") ?? string.Empty;
                new_DestDataRow["Param"] = src_Col_Config_Row.Field<string>("Param") ?? string.Empty;
                new_DestDataRow["Format"] = src_Col_Config_Row.Field<string>("Format") ?? string.Empty;
                new_DestDataRow["Status"] = row_Status; // Set initial status as appropriate
                new_DestDataRow["CreateDate"] = DateTime.Now;
                new_DestDataRow["CreateUser"] = si.UserName; new_DestDataRow["UpdateDate"] = DateTime.Now;
                new_DestDataRow["UpdateUser"] = si.UserName;
                tgt_Col_Config_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                DataRow existing_DataRow = tgt_Col_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_Col_Config_Row["Name"].ToString() &&
                                    row["ActConfigID"].ToString() == gbl_CurrActConfigID.ToString() &&
                                    row["RegConfigID"].ToString() == gbl_CurrRegConfigID.ToString());

                if (existing_DataRow != null)
                {
                    gbl_CurrColID = existing_DataRow.Field<int>("Col_ID");
                    existing_DataRow["InUse"] = src_Col_Config_Row.Field<bool?>("InUse") ?? false;
                    existing_DataRow["Required"] = src_Col_Config_Row.Field<bool?>("Required") ?? false;
                    existing_DataRow["Updates"] = src_Col_Config_Row.Field<bool?>("Updates") ?? false;
                    existing_DataRow["Alias"] = src_Col_Config_Row.Field<string>("Alias") ?? string.Empty;
                    existing_DataRow["Order"] = src_Col_Config_Row.Field<int?>("Order") ?? 0;
                    existing_DataRow["Default"] = src_Col_Config_Row.Field<string>("Default") ?? string.Empty;
                    existing_DataRow["Param"] = src_Col_Config_Row.Field<string>("Param") ?? string.Empty;
                    existing_DataRow["Format"] = src_Col_Config_Row.Field<string>("Format") ?? string.Empty;
                    existing_DataRow["Status"] = row_Status; // Set initial status as appropriate
                    existing_DataRow["UpdateDate"] = DateTime.Now;
                    existing_DataRow["UpdateUser"] = si.UserName;
                }
            }
        }
        private void CopyApprovals(DataRow src_FMM_ApprConfig_Row, ref DataTable tgt_FMM_ApprConfig_DT, int targetCubeConfigID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            var row_Status = "Build";
            bool isDuplicate = tgt_FMM_ApprConfig_DT.AsEnumerable()
                .Any(row => row["Name"].ToString() == src_FMM_ApprConfig_Row["Name"].ToString() &&
                            row["ActConfigID"].ToString() == gbl_CurrActConfigID.ToString());

            if (!isDuplicate)
            {
                gbl_ApprID += 1;
                gbl_CurrApprID = gbl_ApprID;
                DataRow new_DestDataRow = tgt_FMM_ApprConfig_DT.NewRow();
                new_DestDataRow["CubeConfigID"] = targetCubeConfigID;
                new_DestDataRow["ActConfigID"] = gbl_CurrActConfigID;
                new_DestDataRow["ApprID"] = gbl_ApprID;
                new_DestDataRow["Name"] = src_FMM_ApprConfig_Row.Field<string>("Name") ?? string.Empty;
                new_DestDataRow["Status"] = row_Status; // Set initial status as appropriate
                new_DestDataRow["CreateDate"] = DateTime.Now;
                new_DestDataRow["CreateUser"] = si.UserName; new_DestDataRow["UpdateDate"] = DateTime.Now;
                new_DestDataRow["UpdateUser"] = si.UserName;
                tgt_FMM_ApprConfig_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                DataRow existing_DataRow = tgt_FMM_ApprConfig_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_FMM_ApprConfig_Row["Name"].ToString() &&
                                           row["ActConfigID"].ToString() == gbl_CurrActConfigID.ToString());

                if (existing_DataRow != null)
                {
                    gbl_CurrApprID = existing_DataRow.Field<int>("ApprID");
                    existing_DataRow["Status"] = row_Status; // Update status or other fields as needed
                    existing_DataRow["UpdateDate"] = DateTime.Now;
                    existing_DataRow["UpdateUser"] = si.UserName;
                }
            }
        }
        private void Copy_ApprSteps(DataRow src_FMM_ApprConfigStep_Config_Row, ref DataTable tgt_FMM_ApprStepConfig_DT, int targetCubeConfigID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            var row_Status = "Build";
            bool isDuplicate = tgt_FMM_ApprStepConfig_DT.AsEnumerable()
                .Any(row => row["StepNum"].ToString() == src_FMM_ApprConfigStep_Config_Row["StepNum"].ToString() &&
                            row["WFProfile_Step"].ToString() == src_FMM_ApprConfigStep_Config_Row["WFProfile_Step"].ToString() &&
                            row["ActConfigID"].ToString() == gbl_CurrActConfigID.ToString() &&
                            row["ApprID"].ToString() == gbl_CurrApprID.ToString());

            if (!isDuplicate)
            {
                gbl_ApprStepID += 1;
                gbl_CurrApprStepID = gbl_ApprStepID;
                DataRow new_DestDataRow = tgt_FMM_ApprStepConfig_DT.NewRow();
                new_DestDataRow["CubeConfigID"] = targetCubeConfigID;
                new_DestDataRow["ActConfigID"] = gbl_ActConfigID;
                new_DestDataRow["ApprID"] = gbl_ApprID;
                new_DestDataRow["ApprStepID"] = gbl_ApprStepID;
                new_DestDataRow["StepNum"] = src_FMM_ApprConfigStep_Config_Row.Field<int?>("StepNum") ?? 0;
                new_DestDataRow["WFProfile_Step"] = src_FMM_ApprConfigStep_Config_Row.Field<Guid?>("WFProfile_Step") ?? Guid.Empty;
                new_DestDataRow["UserGroup"] = src_FMM_ApprConfigStep_Config_Row.Field<string>("UserGroup") ?? string.Empty;
                new_DestDataRow["Logic"] = src_FMM_ApprConfigStep_Config_Row.Field<string>("Logic") ?? string.Empty;
                new_DestDataRow["Item"] = src_FMM_ApprConfigStep_Config_Row.Field<string>("Item") ?? string.Empty;
                new_DestDataRow["Level"] = src_FMM_ApprConfigStep_Config_Row.Field<int?>("Level") ?? 0;
                new_DestDataRow["ApprConfig"] = src_FMM_ApprConfigStep_Config_Row.Field<int?>("ApprConfig") ?? 0;
                new_DestDataRow["InitStatus"] = src_FMM_ApprConfigStep_Config_Row.Field<string>("InitStatus") ?? string.Empty;
                new_DestDataRow["ApprStatus"] = src_FMM_ApprConfigStep_Config_Row.Field<string>("ApprStatus") ?? string.Empty;
                new_DestDataRow["RejStatus"] = src_FMM_ApprConfigStep_Config_Row.Field<string>("RejStatus") ?? string.Empty;
                new_DestDataRow["Status"] = row_Status; // Set initial status as appropriate
                new_DestDataRow["CreateDate"] = DateTime.Now;
                new_DestDataRow["CreateUser"] = si.UserName; new_DestDataRow["UpdateDate"] = DateTime.Now;
                new_DestDataRow["UpdateUser"] = si.UserName;
                tgt_FMM_ApprStepConfig_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                DataRow existing_DataRow = tgt_FMM_ApprStepConfig_DT.AsEnumerable()
                    .FirstOrDefault(row => row["StepNum"].ToString() == src_FMM_ApprConfigStep_Config_Row["StepNum"].ToString() &&
                                           row["WFProfile_Step"].ToString() == src_FMM_ApprConfigStep_Config_Row["WFProfile_Step"].ToString() &&
                                           row["ActConfigID"].ToString() == gbl_CurrActConfigID.ToString() &&
                                           row["ApprID"].ToString() == gbl_CurrApprID.ToString());

                if (existing_DataRow != null)
                {
                    gbl_CurrApprStepID = existing_DataRow.Field<int>("ApprStepID");
                    existing_DataRow["UserGroup"] = src_FMM_ApprConfigStep_Config_Row.Field<string>("UserGroup") ?? string.Empty;
                    existing_DataRow["Logic"] = src_FMM_ApprConfigStep_Config_Row.Field<string>("Logic") ?? string.Empty;
                    existing_DataRow["Item"] = src_FMM_ApprConfigStep_Config_Row.Field<string>("Item") ?? string.Empty;
                    existing_DataRow["Level"] = src_FMM_ApprConfigStep_Config_Row.Field<int?>("Level") ?? 0;
                    existing_DataRow["ApprConfig"] = src_FMM_ApprConfigStep_Config_Row.Field<int?>("ApprConfig") ?? 0;
                    existing_DataRow["InitStatus"] = src_FMM_ApprConfigStep_Config_Row.Field<string>("InitStatus") ?? string.Empty;
                    existing_DataRow["ApprStatus"] = src_FMM_ApprConfigStep_Config_Row.Field<string>("ApprStatus") ?? string.Empty;
                    existing_DataRow["RejStatus"] = src_FMM_ApprConfigStep_Config_Row.Field<string>("RejStatus") ?? string.Empty;
                    existing_DataRow["Status"] = row_Status; // Set initial status as appropriate
                    existing_DataRow["UpdateDate"] = DateTime.Now;
                    existing_DataRow["UpdateUser"] = si.UserName;
                }
            }
        }
        private void CopyModels(DataRow src_Model_Config_Row, ref DataTable tgt_Model_Config_DT, int targetCubeConfigID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            var row_Status = "Build";
            bool isDuplicate = tgt_Model_Config_DT.AsEnumerable()
                .Any(row => row["Name"].ToString() == src_Model_Config_Row["Name"].ToString() &&
                            row["ActConfigID"].ToString() == gbl_CurrActConfigID.ToString());

            if (!isDuplicate)
            {
                gbl_ModelConfigID += 1;
                gbl_CurrModelConfigID = gbl_ModelConfigID;
                DataRow new_DestDataRow = tgt_Model_Config_DT.NewRow();
                new_DestDataRow["CubeConfigID"] = targetCubeConfigID;
                new_DestDataRow["ActConfigID"] = gbl_CurrActConfigID;
                new_DestDataRow["ModelConfigID"] = gbl_ModelConfigID;
                new_DestDataRow["Name"] = src_Model_Config_Row.Field<string>("Name") ?? string.Empty;
                new_DestDataRow["Status"] = row_Status; // Set initial status as appropriate
                new_DestDataRow["CreateDate"] = DateTime.Now;
                new_DestDataRow["CreateUser"] = si.UserName; new_DestDataRow["UpdateDate"] = DateTime.Now;
                new_DestDataRow["UpdateUser"] = si.UserName;
                tgt_Model_Config_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                DataRow existing_DataRow = tgt_Model_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_Model_Config_Row["Name"].ToString() &&
                                           row["ActConfigID"].ToString() == gbl_CurrActConfigID.ToString());

                if (existing_DataRow != null)
                {
                    gbl_CurrModelConfigID = existing_DataRow.Field<int>("ModelConfigID");
                    existing_DataRow["Status"] = row_Status; // Update status or other fields as needed
                    existing_DataRow["UpdateDate"] = DateTime.Now;
                    existing_DataRow["UpdateUser"] = si.UserName;
                }
            }
        }
        private void CopyCalcs(DataRow src_CalcConfig_Row, ref DataTable tgt_CalcConfig_DT, int targetCubeConfigID, ref XFSelectionChangedTaskResult XFCopyTaskResult, string runType)
        {
            var row_Status = "Build";
            bool isDuplicate = tgt_CalcConfig_DT.AsEnumerable()
                .Any(row => row["Name"].ToString() == src_CalcConfig_Row["Name"].ToString() &&
                            row["ActConfigID"].ToString() == gbl_CurrActConfigID.ToString() &&
                            row["ModelConfigID"].ToString() == gbl_CurrModelConfigID.ToString());

            if (runType == "Model Calc Copy" || !isDuplicate)
            {
                gbl_CalcConfigID += 1;
                gbl_CurrCalcConfigID = gbl_CalcConfigID;

                DataRow new_DestDataRow = tgt_CalcConfig_DT.NewRow();
                new_DestDataRow["CubeConfigID"] = targetCubeConfigID;
                new_DestDataRow["ActConfigID"] = gbl_CurrActConfigID;
                new_DestDataRow["ModelConfigID"] = gbl_CurrModelConfigID;
                new_DestDataRow["CalcConfigID"] = gbl_CalcConfigID;
                if (runType == "Model Calc Copy")
                {
                    string calcName = src_CalcConfig_Row.Field<string>("Name") ?? string.Empty;

                    new_DestDataRow["Name"] = calcName + " - Copy";
                }
                else
                {
                    new_DestDataRow["Name"] = src_CalcConfig_Row.Field<string>("Name") ?? string.Empty;
                }
                new_DestDataRow["Sequence"] = src_CalcConfig_Row.Field<int?>("Sequence") ?? 0;
                new_DestDataRow["CalcCondition"] = src_CalcConfig_Row.Field<string>("CalcCondition") ?? string.Empty;
                new_DestDataRow["CalcExplanation"] = src_CalcConfig_Row.Field<string>("CalcExplanation") ?? string.Empty;
                new_DestDataRow["BalancedBuffer"] = src_CalcConfig_Row.Field<string>("BalancedBuffer") ?? string.Empty;
                new_DestDataRow["bal_buffer_calc"] = src_CalcConfig_Row.Field<string>("bal_buffer_calc") ?? string.Empty;
                new_DestDataRow["UnbalCalc"] = src_CalcConfig_Row.Field<string>("UnbalCalc") ?? string.Empty;
                new_DestDataRow["Table_Calc_SQL_Logic"] = src_CalcConfig_Row.Field<string>("Table_Calc_SQL_Logic") ?? string.Empty;
                new_DestDataRow["TimePhasing"] = src_CalcConfig_Row.Field<string>("TimePhasing") ?? string.Empty;
                new_DestDataRow["InputFrequency"] = src_CalcConfig_Row.Field<string>("InputFrequency") ?? string.Empty;
                new_DestDataRow["MultiDimAlloc"] = src_CalcConfig_Row.Field<bool?>("MultiDimAlloc") ?? false;
                new_DestDataRow["BR_Calc"] = src_CalcConfig_Row.Field<bool?>("BR_Calc") ?? false;
                new_DestDataRow["BR_Calc_Name"] = src_CalcConfig_Row.Field<string>("BR_Calc_Name") ?? string.Empty;
                new_DestDataRow["Status"] = row_Status;
                new_DestDataRow["CreateDate"] = DateTime.Now;
                new_DestDataRow["CreateUser"] = si.UserName; new_DestDataRow["UpdateDate"] = DateTime.Now;
                new_DestDataRow["UpdateUser"] = si.UserName;
                tgt_CalcConfig_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                DataRow existing_DataRow = tgt_CalcConfig_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_CalcConfig_Row["Name"].ToString() &&
                                           row["ActConfigID"].ToString() == gbl_CurrActConfigID.ToString() &&
                                           row["ModelConfigID"].ToString() == gbl_CurrModelConfigID.ToString());

                if (existing_DataRow != null)
                {
                    gbl_CurrCalcConfigID = existing_DataRow.Field<int>("CalcConfigID");
                    existing_DataRow["Status"] = row_Status;
                    existing_DataRow["Sequence"] = src_CalcConfig_Row.Field<int?>("Sequence") ?? 0;
                    existing_DataRow["CalcCondition"] = src_CalcConfig_Row.Field<string>("CalcCondition") ?? string.Empty;
                    existing_DataRow["CalcExplanation"] = src_CalcConfig_Row.Field<string>("CalcExplanation") ?? string.Empty;
                    existing_DataRow["BalancedBuffer"] = src_CalcConfig_Row.Field<string>("BalancedBuffer") ?? string.Empty;
                    existing_DataRow["bal_buffer_calc"] = src_CalcConfig_Row.Field<string>("bal_buffer_calc") ?? string.Empty;
                    existing_DataRow["UnbalCalc"] = src_CalcConfig_Row.Field<string>("UnbalCalc") ?? string.Empty;
                    existing_DataRow["Table_Calc_SQL_Logic"] = src_CalcConfig_Row.Field<string>("Table_Calc_SQL_Logic") ?? string.Empty;
                    existing_DataRow["TimePhasing"] = src_CalcConfig_Row.Field<string>("TimePhasing") ?? string.Empty;
                    existing_DataRow["InputFrequency"] = src_CalcConfig_Row.Field<string>("InputFrequency") ?? string.Empty;
                    existing_DataRow["MultiDimAlloc"] = src_CalcConfig_Row.Field<bool?>("MultiDimAlloc") ?? false;
                    existing_DataRow["BR_Calc"] = src_CalcConfig_Row.Field<bool?>("BR_Calc") ?? false;
                    existing_DataRow["BR_Calc_Name"] = src_CalcConfig_Row.Field<string>("BR_Calc_Name") ?? string.Empty;
                    existing_DataRow["UpdateDate"] = DateTime.Now;
                    existing_DataRow["UpdateUser"] = si.UserName;
                }
            }
        }
        private void CopyCell(DataRow SrcCellConfig_Config_Row, ref DataTable tgt_Cell_Config_DT, int targetCubeConfigID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            bool isDuplicate = tgt_Cell_Config_DT.AsEnumerable()
                .Any(row => row["ActConfigID"].ToString() == gbl_CurrActConfigID.ToString() &&
                            row["ModelConfigID"].ToString() == gbl_CurrModelConfigID.ToString() &&
                            row["CalcConfigID"].ToString() == gbl_CurrCalcConfigID.ToString());

            if (!isDuplicate)
            {
                gbl_DestCellID += 1;
                gbl_CurrDestCellID = gbl_DestCellID;

                DataRow new_DestDataRow = tgt_Cell_Config_DT.NewRow();
                new_DestDataRow["CubeConfigID"] = targetCubeConfigID;
                new_DestDataRow["ActConfigID"] = gbl_CurrActConfigID;
                new_DestDataRow["ModelConfigID"] = gbl_CurrModelConfigID;
                new_DestDataRow["CalcConfigID"] = gbl_CurrCalcConfigID;
                new_DestDataRow["DestCell_ID"] = gbl_DestCellID;

                new_DestDataRow["Location"] = SrcCellConfig_Config_Row.Field<string>("Location") ?? string.Empty;
                new_DestDataRow["Calc_Plan_Units"] = SrcCellConfig_Config_Row.Field<string>("Calc_Plan_Units") ?? string.Empty;
                new_DestDataRow["Acct"] = SrcCellConfig_Config_Row.Field<string>("Acct") ?? string.Empty;
                new_DestDataRow["View"] = SrcCellConfig_Config_Row.Field<string>("View") ?? string.Empty;
                new_DestDataRow["Origin"] = SrcCellConfig_Config_Row.Field<string>("Origin") ?? string.Empty;
                new_DestDataRow["IC"] = SrcCellConfig_Config_Row.Field<string>("IC") ?? string.Empty;
                new_DestDataRow["Flow"] = SrcCellConfig_Config_Row.Field<string>("Flow") ?? string.Empty;
                new_DestDataRow["UD1"] = SrcCellConfig_Config_Row.Field<string>("UD1") ?? string.Empty;
                new_DestDataRow["UD2"] = SrcCellConfig_Config_Row.Field<string>("UD2") ?? string.Empty;
                new_DestDataRow["UD3"] = SrcCellConfig_Config_Row.Field<string>("UD3") ?? string.Empty;
                new_DestDataRow["UD4"] = SrcCellConfig_Config_Row.Field<string>("UD4") ?? string.Empty;
                new_DestDataRow["UD5"] = SrcCellConfig_Config_Row.Field<string>("UD5") ?? string.Empty;
                new_DestDataRow["UD6"] = SrcCellConfig_Config_Row.Field<string>("UD6") ?? string.Empty;
                new_DestDataRow["UD7"] = SrcCellConfig_Config_Row.Field<string>("UD7") ?? string.Empty;
                new_DestDataRow["UD8"] = SrcCellConfig_Config_Row.Field<string>("UD8") ?? string.Empty;
                new_DestDataRow["TimeFilter"] = SrcCellConfig_Config_Row.Field<string>("TimeFilter") ?? string.Empty;
                new_DestDataRow["AcctFilter"] = SrcCellConfig_Config_Row.Field<string>("AcctFilter") ?? string.Empty;
                new_DestDataRow["OriginFilter"] = SrcCellConfig_Config_Row.Field<string>("OriginFilter") ?? string.Empty;
                new_DestDataRow["ICFilter"] = SrcCellConfig_Config_Row.Field<string>("ICFilter") ?? string.Empty;
                new_DestDataRow["FlowFilter"] = SrcCellConfig_Config_Row.Field<string>("FlowFilter") ?? string.Empty;
                new_DestDataRow["UD1_Filter"] = SrcCellConfig_Config_Row.Field<string>("UD1_Filter") ?? string.Empty;
                new_DestDataRow["UD2_Filter"] = SrcCellConfig_Config_Row.Field<string>("UD2_Filter") ?? string.Empty;
                new_DestDataRow["UD3_Filter"] = SrcCellConfig_Config_Row.Field<string>("UD3_Filter") ?? string.Empty;
                new_DestDataRow["UD4_Filter"] = SrcCellConfig_Config_Row.Field<string>("UD4_Filter") ?? string.Empty;
                new_DestDataRow["UD5_Filter"] = SrcCellConfig_Config_Row.Field<string>("UD5_Filter") ?? string.Empty;
                new_DestDataRow["UD6_Filter"] = SrcCellConfig_Config_Row.Field<string>("UD6_Filter") ?? string.Empty;
                new_DestDataRow["UD7_Filter"] = SrcCellConfig_Config_Row.Field<string>("UD7_Filter") ?? string.Empty;
                new_DestDataRow["UD8_Filter"] = SrcCellConfig_Config_Row.Field<string>("UD8_Filter") ?? string.Empty;
                new_DestDataRow["ConditionalFilter"] = SrcCellConfig_Config_Row.Field<string>("ConditionalFilter") ?? string.Empty;
                new_DestDataRow["Curr_Cube_Buffer_Filter"] = SrcCellConfig_Config_Row.Field<string>("Curr_Cube_Buffer_Filter") ?? string.Empty;
                new_DestDataRow["BufferFilter"] = SrcCellConfig_Config_Row.Field<string>("BufferFilter") ?? string.Empty;
                new_DestDataRow["DestCell_Logic"] = SrcCellConfig_Config_Row.Field<string>("DestCell_Logic") ?? string.Empty;
                new_DestDataRow["SQL_Logic"] = SrcCellConfig_Config_Row.Field<string>("SQL_Logic") ?? string.Empty;

                tgt_Cell_Config_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                DataRow existing_DataRow = tgt_Cell_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["ActConfigID"].ToString() == gbl_CurrActConfigID.ToString() &&
                                           row["ModelConfigID"].ToString() == gbl_CurrModelConfigID.ToString() &&
                                           row["CalcConfigID"].ToString() == gbl_CurrCalcConfigID.ToString());

                if (existing_DataRow != null)
                {
                    gbl_CurrDestCellID = existing_DataRow.Field<int>("DestCell_ID");
                    existing_DataRow["Location"] = SrcCellConfig_Config_Row.Field<string>("Location") ?? string.Empty;
                    existing_DataRow["Calc_Plan_Units"] = SrcCellConfig_Config_Row.Field<string>("Calc_Plan_Units") ?? string.Empty;
                    existing_DataRow["Acct"] = SrcCellConfig_Config_Row.Field<string>("Acct") ?? string.Empty;
                    existing_DataRow["View"] = SrcCellConfig_Config_Row.Field<string>("View") ?? string.Empty;
                    existing_DataRow["Origin"] = SrcCellConfig_Config_Row.Field<string>("Origin") ?? string.Empty;
                    existing_DataRow["IC"] = SrcCellConfig_Config_Row.Field<string>("IC") ?? string.Empty;
                    existing_DataRow["Flow"] = SrcCellConfig_Config_Row.Field<string>("Flow") ?? string.Empty;
                    existing_DataRow["UD1"] = SrcCellConfig_Config_Row.Field<string>("UD1") ?? string.Empty;
                    existing_DataRow["UD2"] = SrcCellConfig_Config_Row.Field<string>("UD2") ?? string.Empty;
                    existing_DataRow["UD3"] = SrcCellConfig_Config_Row.Field<string>("UD3") ?? string.Empty;
                    existing_DataRow["UD4"] = SrcCellConfig_Config_Row.Field<string>("UD4") ?? string.Empty;
                    existing_DataRow["UD5"] = SrcCellConfig_Config_Row.Field<string>("UD5") ?? string.Empty;
                    existing_DataRow["UD6"] = SrcCellConfig_Config_Row.Field<string>("UD6") ?? string.Empty;
                    existing_DataRow["UD7"] = SrcCellConfig_Config_Row.Field<string>("UD7") ?? string.Empty;
                    existing_DataRow["UD8"] = SrcCellConfig_Config_Row.Field<string>("UD8") ?? string.Empty;
                    existing_DataRow["TimeFilter"] = SrcCellConfig_Config_Row.Field<string>("TimeFilter") ?? string.Empty;
                    existing_DataRow["AcctFilter"] = SrcCellConfig_Config_Row.Field<string>("AcctFilter") ?? string.Empty;
                    existing_DataRow["OriginFilter"] = SrcCellConfig_Config_Row.Field<string>("OriginFilter") ?? string.Empty;
                    existing_DataRow["ICFilter"] = SrcCellConfig_Config_Row.Field<string>("ICFilter") ?? string.Empty;
                    existing_DataRow["FlowFilter"] = SrcCellConfig_Config_Row.Field<string>("FlowFilter") ?? string.Empty;
                    existing_DataRow["UD1_Filter"] = SrcCellConfig_Config_Row.Field<string>("UD1_Filter") ?? string.Empty;
                    existing_DataRow["UD2_Filter"] = SrcCellConfig_Config_Row.Field<string>("UD2_Filter") ?? string.Empty;
                    existing_DataRow["UD3_Filter"] = SrcCellConfig_Config_Row.Field<string>("UD3_Filter") ?? string.Empty;
                    existing_DataRow["UD4_Filter"] = SrcCellConfig_Config_Row.Field<string>("UD4_Filter") ?? string.Empty;
                    existing_DataRow["UD5_Filter"] = SrcCellConfig_Config_Row.Field<string>("UD5_Filter") ?? string.Empty;
                    existing_DataRow["UD6_Filter"] = SrcCellConfig_Config_Row.Field<string>("UD6_Filter") ?? string.Empty;
                    existing_DataRow["UD7_Filter"] = SrcCellConfig_Config_Row.Field<string>("UD7_Filter") ?? string.Empty;
                    existing_DataRow["UD8_Filter"] = SrcCellConfig_Config_Row.Field<string>("UD8_Filter") ?? string.Empty;
                    existing_DataRow["ConditionalFilter"] = SrcCellConfig_Config_Row.Field<string>("ConditionalFilter") ?? string.Empty;
                    existing_DataRow["Curr_Cube_Buffer_Filter"] = SrcCellConfig_Config_Row.Field<string>("Curr_Cube_Buffer_Filter") ?? string.Empty;
                    existing_DataRow["BufferFilter"] = SrcCellConfig_Config_Row.Field<string>("BufferFilter") ?? string.Empty;
                    existing_DataRow["DestCell_Logic"] = SrcCellConfig_Config_Row.Field<string>("DestCell_Logic") ?? string.Empty;
                    existing_DataRow["SQL_Logic"] = SrcCellConfig_Config_Row.Field<string>("SQL_Logic") ?? string.Empty;
                }
            }
        }
        private void Copy_SrcCell(DataRow src_SrcCellConfig_Config_Row, ref DataTable tgt_SrcCellConfig_Config_DT, int targetCubeConfigID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            gbl_SrcCellID += 1;
            gbl_CurrSrcCellID = gbl_SrcCellID;
            bool isDuplicate = tgt_SrcCellConfig_Config_DT.AsEnumerable()
                .Any(row => row["ActConfigID"].ToString() == gbl_CurrActConfigID.ToString() &&
                            row["ModelConfigID"].ToString() == gbl_CurrModelConfigID.ToString() &&
                            row["CalcConfigID"].ToString() == gbl_CurrCalcConfigID.ToString() &&
                            row["CellID"].ToString() == gbl_CurrSrcCellID.ToString());

            if (!isDuplicate)
            {
                DataRow new_DestDataRow = tgt_SrcCellConfig_Config_DT.NewRow();

                new_DestDataRow["CubeConfigID"] = targetCubeConfigID;
                new_DestDataRow["ActConfigID"] = gbl_CurrActConfigID;
                new_DestDataRow["ModelConfigID"] = gbl_CurrModelConfigID;
                new_DestDataRow["CalcConfigID"] = gbl_CurrCalcConfigID;
                new_DestDataRow["CellID"] = gbl_SrcCellID;
                new_DestDataRow["SrcOrder"] = src_SrcCellConfig_Config_Row.Field<int?>("SrcOrder") ?? 0;
                new_DestDataRow["SrcType"] = src_SrcCellConfig_Config_Row.Field<string>("SrcType") ?? string.Empty;
                new_DestDataRow["SrcItem"] = src_SrcCellConfig_Config_Row.Field<string>("SrcItem") ?? string.Empty;
                new_DestDataRow["OpenParens"] = src_SrcCellConfig_Config_Row.Field<string>("OpenParens") ?? string.Empty;
                new_DestDataRow["MathOperator"] = src_SrcCellConfig_Config_Row.Field<string>("MathOperator") ?? string.Empty;
                new_DestDataRow["Entity"] = src_SrcCellConfig_Config_Row.Field<string>("Entity") ?? string.Empty;
                new_DestDataRow["Cons"] = src_SrcCellConfig_Config_Row.Field<string>("Cons") ?? string.Empty;
                new_DestDataRow["Scenario"] = src_SrcCellConfig_Config_Row.Field<string>("Scenario") ?? string.Empty;
                new_DestDataRow["Time"] = src_SrcCellConfig_Config_Row.Field<string>("Time") ?? string.Empty;
                new_DestDataRow["Origin"] = src_SrcCellConfig_Config_Row.Field<string>("Origin") ?? string.Empty;
                new_DestDataRow["IC"] = src_SrcCellConfig_Config_Row.Field<string>("IC") ?? string.Empty;
                new_DestDataRow["View"] = src_SrcCellConfig_Config_Row.Field<string>("View") ?? string.Empty;
                new_DestDataRow["Src_Plan_Units"] = src_SrcCellConfig_Config_Row.Field<string>("Src_Plan_Units") ?? string.Empty;
                new_DestDataRow["Acct"] = src_SrcCellConfig_Config_Row.Field<string>("Acct") ?? string.Empty;
                new_DestDataRow["Flow"] = src_SrcCellConfig_Config_Row.Field<string>("Flow") ?? string.Empty;
                new_DestDataRow["UD1"] = src_SrcCellConfig_Config_Row.Field<string>("UD1") ?? string.Empty;
                new_DestDataRow["UD2"] = src_SrcCellConfig_Config_Row.Field<string>("UD2") ?? string.Empty;
                new_DestDataRow["UD3"] = src_SrcCellConfig_Config_Row.Field<string>("UD3") ?? string.Empty;
                new_DestDataRow["UD4"] = src_SrcCellConfig_Config_Row.Field<string>("UD4") ?? string.Empty;
                new_DestDataRow["UD5"] = src_SrcCellConfig_Config_Row.Field<string>("UD5") ?? string.Empty;
                new_DestDataRow["UD6"] = src_SrcCellConfig_Config_Row.Field<string>("UD6") ?? string.Empty;
                new_DestDataRow["UD7"] = src_SrcCellConfig_Config_Row.Field<string>("UD7") ?? string.Empty;
                new_DestDataRow["UD8"] = src_SrcCellConfig_Config_Row.Field<string>("UD8") ?? string.Empty;
                new_DestDataRow["CloseParens"] = src_SrcCellConfig_Config_Row.Field<string>("CloseParens") ?? string.Empty;
                new_DestDataRow["Unbal_SrcCellConfig_Buffer"] = src_SrcCellConfig_Config_Row.Field<string>("Unbal_SrcCellConfig_Buffer") ?? string.Empty;
                new_DestDataRow["Unbal_Origin_Override"] = src_SrcCellConfig_Config_Row.Field<string>("Unbal_Origin_Override") ?? string.Empty;
                new_DestDataRow["Unbal_IC_Override"] = src_SrcCellConfig_Config_Row.Field<string>("Unbal_IC_Override") ?? string.Empty;
                new_DestDataRow["Unbal_Acct_Override"] = src_SrcCellConfig_Config_Row.Field<string>("Unbal_Acct_Override") ?? string.Empty;
                new_DestDataRow["Unbal_Flow_Override"] = src_SrcCellConfig_Config_Row.Field<string>("Unbal_Flow_Override") ?? string.Empty;
                new_DestDataRow["Unbal_UD1_Override"] = src_SrcCellConfig_Config_Row.Field<string>("Unbal_UD1_Override") ?? string.Empty;
                new_DestDataRow["Unbal_UD2_Override"] = src_SrcCellConfig_Config_Row.Field<string>("Unbal_UD2_Override") ?? string.Empty;
                new_DestDataRow["Unbal_UD3_Override"] = src_SrcCellConfig_Config_Row.Field<string>("Unbal_UD3_Override") ?? string.Empty;
                new_DestDataRow["Unbal_UD4_Override"] = src_SrcCellConfig_Config_Row.Field<string>("Unbal_UD4_Override") ?? string.Empty;
                new_DestDataRow["Unbal_UD5_Override"] = src_SrcCellConfig_Config_Row.Field<string>("Unbal_UD5_Override") ?? string.Empty;
                new_DestDataRow["Unbal_UD6_Override"] = src_SrcCellConfig_Config_Row.Field<string>("Unbal_UD6_Override") ?? string.Empty;
                new_DestDataRow["Unbal_UD7_Override"] = src_SrcCellConfig_Config_Row.Field<string>("Unbal_UD7_Override") ?? string.Empty;
                new_DestDataRow["Unbal_UD8_Override"] = src_SrcCellConfig_Config_Row.Field<string>("Unbal_UD8_Override") ?? string.Empty;
                new_DestDataRow["Unbal_SrcCellConfig_Buffer_Filter"] = src_SrcCellConfig_Config_Row.Field<string>("Unbal_SrcCellConfig_Buffer_Filter") ?? string.Empty;
                new_DestDataRow["Dyn_Calc_Script"] = src_SrcCellConfig_Config_Row.Field<string>("Dyn_Calc_Script") ?? string.Empty;
                new_DestDataRow["OverrideValue"] = src_SrcCellConfig_Config_Row.Field<string>("OverrideValue") ?? string.Empty;
                new_DestDataRow["Table_Calc_Expression"] = src_SrcCellConfig_Config_Row.Field<string>("Table_Calc_Expression") ?? string.Empty;
                new_DestDataRow["Table_Join_Expression"] = src_SrcCellConfig_Config_Row.Field<string>("Table_Join_Expression") ?? string.Empty;
                new_DestDataRow["Table_Filter_Expression"] = src_SrcCellConfig_Config_Row.Field<string>("Table_Filter_Expression") ?? string.Empty;
                new_DestDataRow["MapType"] = src_SrcCellConfig_Config_Row.Field<string>("MapType") ?? string.Empty;
                new_DestDataRow["MapSource"] = src_SrcCellConfig_Config_Row.Field<string>("MapSource") ?? string.Empty;
                new_DestDataRow["MapLogic"] = src_SrcCellConfig_Config_Row.Field<string>("MapLogic") ?? string.Empty;
                new_DestDataRow["Src_SQL_Stmt"] = src_SrcCellConfig_Config_Row.Field<string>("Src_SQL_Stmt") ?? string.Empty;
                new_DestDataRow["Use_Temp_Table"] = src_SrcCellConfig_Config_Row.Field<bool>("Use_Temp_Table");
                new_DestDataRow["Temp_Table_Name"] = src_SrcCellConfig_Config_Row.Field<string>("Temp_Table_Name") ?? string.Empty;
                new_DestDataRow["CreateDate"] = DateTime.Now;
                new_DestDataRow["CreateUser"] = si.UserName; // Set appropriate user context
                new_DestDataRow["UpdateDate"] = DateTime.Now;
                new_DestDataRow["UpdateUser"] = si.UserName; // Set appropriate user context

                tgt_SrcCellConfig_Config_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                DataRow existing_DataRow = tgt_SrcCellConfig_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["ActConfigID"].ToString() == gbl_CurrActConfigID.ToString() &&
                                           row["ModelConfigID"].ToString() == gbl_CurrModelConfigID.ToString() &&
                                           row["CalcConfigID"].ToString() == gbl_CurrCalcConfigID.ToString() &&
                                           row["CellID"].ToString() == gbl_CurrSrcCellID.ToString());

                if (existing_DataRow != null)
                {
                    gbl_CurrSrcCellID = Convert.ToInt32(existing_DataRow["CellID"].ToString());
                    existing_DataRow["SrcOrder"] = src_SrcCellConfig_Config_Row.Field<int?>("SrcOrder") ?? 0;
                    existing_DataRow["SrcType"] = src_SrcCellConfig_Config_Row.Field<string>("SrcType") ?? string.Empty;
                    existing_DataRow["SrcItem"] = src_SrcCellConfig_Config_Row.Field<string>("SrcItem") ?? string.Empty;
                    existing_DataRow["OpenParens"] = src_SrcCellConfig_Config_Row.Field<string>("OpenParens") ?? string.Empty;
                    existing_DataRow["MathOperator"] = src_SrcCellConfig_Config_Row.Field<string>("MathOperator") ?? string.Empty;
                    existing_DataRow["Entity"] = src_SrcCellConfig_Config_Row.Field<string>("Entity") ?? string.Empty;
                    existing_DataRow["Cons"] = src_SrcCellConfig_Config_Row.Field<string>("Cons") ?? string.Empty;
                    existing_DataRow["Scenario"] = src_SrcCellConfig_Config_Row.Field<string>("Scenario") ?? string.Empty;
                    existing_DataRow["Time"] = src_SrcCellConfig_Config_Row.Field<string>("Time") ?? string.Empty;
                    existing_DataRow["Origin"] = src_SrcCellConfig_Config_Row.Field<string>("Origin") ?? string.Empty;
                    existing_DataRow["IC"] = src_SrcCellConfig_Config_Row.Field<string>("IC") ?? string.Empty;
                    existing_DataRow["View"] = src_SrcCellConfig_Config_Row.Field<string>("View") ?? string.Empty;
                    existing_DataRow["Src_Plan_Units"] = src_SrcCellConfig_Config_Row.Field<string>("Src_Plan_Units") ?? string.Empty;
                    existing_DataRow["Acct"] = src_SrcCellConfig_Config_Row.Field<string>("Acct") ?? string.Empty;
                    existing_DataRow["Flow"] = src_SrcCellConfig_Config_Row.Field<string>("Flow") ?? string.Empty;
                    existing_DataRow["UD1"] = src_SrcCellConfig_Config_Row.Field<string>("UD1") ?? string.Empty;
                    existing_DataRow["UD2"] = src_SrcCellConfig_Config_Row.Field<string>("UD2") ?? string.Empty;
                    existing_DataRow["UD3"] = src_SrcCellConfig_Config_Row.Field<string>("UD3") ?? string.Empty;
                    existing_DataRow["UD4"] = src_SrcCellConfig_Config_Row.Field<string>("UD4") ?? string.Empty;
                    existing_DataRow["UD5"] = src_SrcCellConfig_Config_Row.Field<string>("UD5") ?? string.Empty;
                    existing_DataRow["UD6"] = src_SrcCellConfig_Config_Row.Field<string>("UD6") ?? string.Empty;
                    existing_DataRow["UD7"] = src_SrcCellConfig_Config_Row.Field<string>("UD7") ?? string.Empty;
                    existing_DataRow["UD8"] = src_SrcCellConfig_Config_Row.Field<string>("UD8") ?? string.Empty;
                    existing_DataRow["CloseParens"] = src_SrcCellConfig_Config_Row.Field<string>("CloseParens") ?? string.Empty;
                    existing_DataRow["Unbal_SrcCellConfig_Buffer"] = src_SrcCellConfig_Config_Row.Field<string>("Unbal_SrcCellConfig_Buffer") ?? string.Empty;
                    existing_DataRow["Unbal_Origin_Override"] = src_SrcCellConfig_Config_Row.Field<string>("Unbal_Origin_Override") ?? string.Empty;
                    existing_DataRow["Unbal_IC_Override"] = src_SrcCellConfig_Config_Row.Field<string>("Unbal_IC_Override") ?? string.Empty;
                    existing_DataRow["Unbal_Acct_Override"] = src_SrcCellConfig_Config_Row.Field<string>("Unbal_Acct_Override") ?? string.Empty;
                    existing_DataRow["Unbal_Flow_Override"] = src_SrcCellConfig_Config_Row.Field<string>("Unbal_Flow_Override") ?? string.Empty;
                    existing_DataRow["Unbal_UD1_Override"] = src_SrcCellConfig_Config_Row.Field<string>("Unbal_UD1_Override") ?? string.Empty;
                    existing_DataRow["Unbal_UD2_Override"] = src_SrcCellConfig_Config_Row.Field<string>("Unbal_UD2_Override") ?? string.Empty;
                    existing_DataRow["Unbal_UD3_Override"] = src_SrcCellConfig_Config_Row.Field<string>("Unbal_UD3_Override") ?? string.Empty;
                    existing_DataRow["Unbal_UD4_Override"] = src_SrcCellConfig_Config_Row.Field<string>("Unbal_UD4_Override") ?? string.Empty;
                    existing_DataRow["Unbal_UD5_Override"] = src_SrcCellConfig_Config_Row.Field<string>("Unbal_UD5_Override") ?? string.Empty;
                    existing_DataRow["Unbal_UD6_Override"] = src_SrcCellConfig_Config_Row.Field<string>("Unbal_UD6_Override") ?? string.Empty;
                    existing_DataRow["Unbal_UD7_Override"] = src_SrcCellConfig_Config_Row.Field<string>("Unbal_UD7_Override") ?? string.Empty;
                    existing_DataRow["Unbal_UD8_Override"] = src_SrcCellConfig_Config_Row.Field<string>("Unbal_UD8_Override") ?? string.Empty;
                    existing_DataRow["Unbal_SrcCellConfig_Buffer_Filter"] = src_SrcCellConfig_Config_Row.Field<string>("Unbal_SrcCellConfig_Buffer_Filter") ?? string.Empty;
                    existing_DataRow["Dyn_Calc_Script"] = src_SrcCellConfig_Config_Row.Field<string>("Dyn_Calc_Script") ?? string.Empty;
                    existing_DataRow["OverrideValue"] = src_SrcCellConfig_Config_Row.Field<string>("OverrideValue") ?? string.Empty;
                    existing_DataRow["Table_Calc_Expression"] = src_SrcCellConfig_Config_Row.Field<string>("Table_Calc_Expression") ?? string.Empty;
                    existing_DataRow["Table_Join_Expression"] = src_SrcCellConfig_Config_Row.Field<string>("Table_Join_Expression") ?? string.Empty;
                    existing_DataRow["Table_Filter_Expression"] = src_SrcCellConfig_Config_Row.Field<string>("Table_Filter_Expression") ?? string.Empty;
                    existing_DataRow["MapType"] = src_SrcCellConfig_Config_Row.Field<string>("MapType") ?? string.Empty;
                    existing_DataRow["MapSource"] = src_SrcCellConfig_Config_Row.Field<string>("MapSource") ?? string.Empty;
                    existing_DataRow["MapLogic"] = src_SrcCellConfig_Config_Row.Field<string>("MapLogic") ?? string.Empty;
                    existing_DataRow["Src_SQL_Stmt"] = src_SrcCellConfig_Config_Row.Field<string>("Src_SQL_Stmt") ?? string.Empty;
                    existing_DataRow["Use_Temp_Table"] = src_SrcCellConfig_Config_Row.Field<bool>("Use_Temp_Table");
                    existing_DataRow["Temp_Table_Name"] = src_SrcCellConfig_Config_Row.Field<string>("Temp_Table_Name") ?? string.Empty;
                    existing_DataRow["UpdateDate"] = DateTime.Now;
                    existing_DataRow["UpdateUser"] = si.UserName; // Set appropriate user context
                }
            }
        }
        private void Copy_ModelGrps(DataRow src_Model_Grp_Config_Row, ref DataTable tgt_Model_Grp_Config_DT, int targetCubeConfigID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            bool isDuplicate = tgt_Model_Grp_Config_DT.AsEnumerable()
                .Any(row => row["GroupName"].ToString() == src_Model_Grp_Config_Row["GroupName"].ToString() &&
                            row["GroupType"].ToString() == src_Model_Grp_Config_Row["GroupType"].ToString());

            if (!isDuplicate)
            {
                gbl_ModelGrpID += 1;
                gbl_CurrModelGrpID = gbl_ModelGrpID;
                DataRow new_DestDataRow = tgt_Model_Grp_Config_DT.NewRow();
                new_DestDataRow["CubeConfigID"] = targetCubeConfigID;
                new_DestDataRow["ActConfigID"] = targetCubeConfigID;
                new_DestDataRow["Model_Grp_Config_ID"] = gbl_ModelGrpID;
                new_DestDataRow["GroupName"] = src_Model_Grp_Config_Row["GroupName"].ToString();
                new_DestDataRow["GroupType"] = src_Model_Grp_Config_Row["GroupType"].ToString();
                new_DestDataRow["Description"] = src_Model_Grp_Config_Row["Description"].ToString();
                new_DestDataRow["Status"] = "Build"; // Set initial status as appropriate
                new_DestDataRow["CreateDate"] = DateTime.Now;
                new_DestDataRow["CreateUser"] = si.UserName;
                new_DestDataRow["UpdateDate"] = DateTime.Now;
                new_DestDataRow["UpdateUser"] = si.UserName;
                tgt_Model_Grp_Config_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                DataRow existing_DataRow = tgt_Model_Grp_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["GroupName"].ToString() == src_Model_Grp_Config_Row["GroupName"].ToString() &&
                                           row["GroupType"].ToString() == src_Model_Grp_Config_Row["GroupType"].ToString());

                if (existing_DataRow != null)
                {
                    gbl_CurrModelGrpID = Convert.ToInt32(existing_DataRow["Model_Grp_Config_ID"].ToString());
                    existing_DataRow["Description"] = src_Model_Grp_Config_Row["Description"].ToString(); // Update description or other fields as needed
                    existing_DataRow["Status"] = src_Model_Grp_Config_Row["Status"].ToString(); // Update status or other fields as needed
                    existing_DataRow["UpdateDate"] = DateTime.Now;
                    existing_DataRow["UpdateUser"] = si.UserName;
                }
            }
        }
        private void Copy_Model_Grp_Assign_Model(DataRow src_Model_Grp_Assign_Model_Row, ref DataTable tgt_Model_Grp_Assign_Model_DT, int targetCubeConfigID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            bool isDuplicate = tgt_Model_Grp_Assign_Model_DT.AsEnumerable()
                .Any(row => row["GroupName"].ToString() == src_Model_Grp_Assign_Model_Row["GroupName"].ToString() &&
                            row["ModelName"].ToString() == src_Model_Grp_Assign_Model_Row["ModelName"].ToString());

            if (!isDuplicate)
            {
                gbl_Model_Grp_Assign_ID += 1;
                gbl_CurrModel_Grp_Assign_ID = gbl_Model_Grp_Assign_ID;
                DataRow new_DestDataRow = tgt_Model_Grp_Assign_Model_DT.NewRow();
                new_DestDataRow["CubeConfigID"] = targetCubeConfigID;
                new_DestDataRow["ActConfigID"] = targetCubeConfigID;
                new_DestDataRow["Model_Grp_Assign_ID"] = gbl_Model_Grp_Assign_ID;
                new_DestDataRow["GroupName"] = src_Model_Grp_Assign_Model_Row["GroupName"].ToString();
                new_DestDataRow["ModelName"] = src_Model_Grp_Assign_Model_Row["ModelName"].ToString();
                new_DestDataRow["Description"] = src_Model_Grp_Assign_Model_Row["Description"].ToString();
                new_DestDataRow["Status"] = "Build"; // Set initial status as appropriate
                new_DestDataRow["CreateDate"] = DateTime.Now;
                new_DestDataRow["CreateUser"] = si.UserName;
                new_DestDataRow["UpdateDate"] = DateTime.Now;
                new_DestDataRow["UpdateUser"] = si.UserName;
                tgt_Model_Grp_Assign_Model_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                DataRow existing_DataRow = tgt_Model_Grp_Assign_Model_DT.AsEnumerable()
                    .FirstOrDefault(row => row["GroupName"].ToString() == src_Model_Grp_Assign_Model_Row["GroupName"].ToString() &&
                                           row["ModelName"].ToString() == src_Model_Grp_Assign_Model_Row["ModelName"].ToString());

                if (existing_DataRow != null)
                {
                    gbl_CurrModel_Grp_Assign_ID = Convert.ToInt32(existing_DataRow["Model_Grp_Assign_ID"].ToString());
                    existing_DataRow["Description"] = src_Model_Grp_Assign_Model_Row["Description"].ToString(); // Update description or other fields as needed
                    existing_DataRow["Status"] = src_Model_Grp_Assign_Model_Row["Status"].ToString(); // Update status or other fields as needed
                    existing_DataRow["UpdateDate"] = DateTime.Now;
                    existing_DataRow["UpdateUser"] = si.UserName;
                }
            }
        }
        private void Copy_CalcUnitConfig(DataRow src_CalcUnitConfig_Row, ref DataTable tgt_CalcUnitConfig_DT, int targetCubeConfigID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            bool isDuplicate = tgt_CalcUnitConfig_DT.AsEnumerable()
                .Any(row => row["DU_Name"].ToString() == src_CalcUnitConfig_Row["DU_Name"].ToString() &&
                            row["DU_Type"].ToString() == src_CalcUnitConfig_Row["DU_Type"].ToString());

            if (!isDuplicate)
            {
                gbl_CalcUnitID += 1;
                gbl_CurrCalcUnitID = gbl_CalcUnitID;
                DataRow new_DestDataRow = tgt_CalcUnitConfig_DT.NewRow();
                new_DestDataRow["CubeConfigID"] = targetCubeConfigID;
                new_DestDataRow["ActConfigID"] = targetCubeConfigID;
                new_DestDataRow["CalcUnitID"] = gbl_CalcUnitID;
                new_DestDataRow["DU_Name"] = src_CalcUnitConfig_Row["DU_Name"].ToString();
                new_DestDataRow["DU_Type"] = src_CalcUnitConfig_Row["DU_Type"].ToString();
                new_DestDataRow["Description"] = src_CalcUnitConfig_Row["Description"].ToString();
                new_DestDataRow["Status"] = "Build";
                new_DestDataRow["CreateDate"] = DateTime.Now;
                new_DestDataRow["CreateUser"] = si.UserName;
                new_DestDataRow["UpdateDate"] = DateTime.Now;
                new_DestDataRow["UpdateUser"] = si.UserName;
                tgt_CalcUnitConfig_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                DataRow existing_DataRow = tgt_CalcUnitConfig_DT.AsEnumerable()
                    .FirstOrDefault(row => row["DU_Name"].ToString() == src_CalcUnitConfig_Row["DU_Name"].ToString() &&
                                           row["DU_Type"].ToString() == src_CalcUnitConfig_Row["DU_Type"].ToString());

                if (existing_DataRow != null)
                {
                    gbl_CurrCalcUnitID = Convert.ToInt32(existing_DataRow["CalcUnitID"].ToString());
                    existing_DataRow["Description"] = src_CalcUnitConfig_Row["Description"].ToString(); // Update description or other fields as needed
                    existing_DataRow["Status"] = src_CalcUnitConfig_Row["Status"].ToString(); // Update status or other fields as needed
                    existing_DataRow["UpdateDate"] = DateTime.Now;
                    existing_DataRow["UpdateUser"] = si.UserName;
                }
            }
        }
        private void Copy_Calc_Unit_Assign(DataRow src_Calc_Unit_Assign_Row, ref DataTable tgt_Calc_Unit_Assign_DT, int targetCubeConfigID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            bool isDuplicate = tgt_Calc_Unit_Assign_DT.AsEnumerable()
                .Any(row => row["DU_Name"].ToString() == src_Calc_Unit_Assign_Row["DU_Name"].ToString() &&
                            row["Model_Grp_Name"].ToString() == src_Calc_Unit_Assign_Row["Model_Grp_Name"].ToString());

            if (!isDuplicate)
            {
                gbl_Calc_Unit_Assign_ID += 1;
                gbl_CurrCalc_Unit_Assign_ID = gbl_Calc_Unit_Assign_ID;
                DataRow new_DestDataRow = tgt_Calc_Unit_Assign_DT.NewRow();
                new_DestDataRow["CubeConfigID"] = targetCubeConfigID;
                new_DestDataRow["ActConfigID"] = targetCubeConfigID;
                new_DestDataRow["Calc_Unit_Assign_ID"] = gbl_Calc_Unit_Assign_ID;
                new_DestDataRow["DU_Name"] = src_Calc_Unit_Assign_Row["DU_Name"].ToString();
                new_DestDataRow["Model_Grp_Name"] = src_Calc_Unit_Assign_Row["Model_Grp_Name"].ToString();
                new_DestDataRow["Description"] = src_Calc_Unit_Assign_Row["Description"].ToString();
                new_DestDataRow["Status"] = "Build"; // Set initial status as appropriate
                new_DestDataRow["CreateDate"] = DateTime.Now;
                new_DestDataRow["CreateUser"] = si.UserName;
                new_DestDataRow["UpdateDate"] = DateTime.Now;
                new_DestDataRow["UpdateUser"] = si.UserName;
                tgt_Calc_Unit_Assign_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                DataRow existing_DataRow = tgt_Calc_Unit_Assign_DT.AsEnumerable()
                    .FirstOrDefault(row => row["DU_Name"].ToString() == src_Calc_Unit_Assign_Row["DU_Name"].ToString() &&
                                           row["Model_Grp_Name"].ToString() == src_Calc_Unit_Assign_Row["Model_Grp_Name"].ToString());

                if (existing_DataRow != null)
                {
                    gbl_CurrCalc_Unit_Assign_ID = Convert.ToInt32(existing_DataRow["Calc_Unit_Assign_ID"].ToString());
                    existing_DataRow["Description"] = src_Calc_Unit_Assign_Row["Description"].ToString(); // Update description or other fields as needed
                    existing_DataRow["Status"] = src_Calc_Unit_Assign_Row["Status"].ToString(); // Update status or other fields as needed
                    existing_DataRow["UpdateDate"] = DateTime.Now;
                    existing_DataRow["UpdateUser"] = si.UserName;
                }
            }
        }
        #endregion

        #region "Helper Functions"

        private void MapConfigValues(ref DataRow row, Dictionary<string, string> customSubstVars, int saveTypeValue, string configHelperType)
        {
            FMM_ConfigHelpers.SaveType saveType = (FMM_ConfigHelpers.SaveType)saveTypeValue;
            dynamic config = null;

            // Switch statement to get the appropriate config registry based on configHelperType
            switch (configHelperType)
            {
                case "CustTable":
                    if (FMM_ConfigHelpers.CustTableConfigRegistry.Configs.TryGetValue(saveType, out var custTableConfig))
                    {
                        config = custTableConfig;
                    }
                    break;

                case "ModelConfig":
                    if (FMM_ConfigHelpers.ModelConfigRegistry.Configs.TryGetValue(saveType, out var modelConfig))
                    {
                        config = modelConfig;
                    }
                    break;

                case "Cube":
                default:
                    if (FMM_ConfigHelpers.CubeConfigRegistry.Configs.TryGetValue(saveType, out var cubeConfig))
                    {
                        config = cubeConfig;
                    }
                    break;
            }

            if (config != null)
            {
                foreach (var step in config.ParameterMappings)
                {
                    foreach (var map in step.Value)
                    {
                        string value = customSubstVars.ContainsKey(map.Key) ? customSubstVars[map.Key] : string.Empty;
                        row[map.Value] = value;
                    }
                }
            }
        }

        private string Construct_Where_Clause(SqlParameter[] sqlparams)
        {
            if (sqlparams == null || sqlparams.Length == 0)
            {
                return string.Empty;
            }

            var whereClause = new List<string>();

            foreach (var param in sqlparams)
            {
                var columnName = param.ParameterName.TrimStart('@');

                if (param.Value == DBNull.Value || param.Value == null)
                {
                    whereClause.Add($"{columnName} IS NULL");
                }
                else
                {
                    if (param.SqlDbType == SqlDbType.NVarChar && param.Value is string stringValue)
                    {
                        if (stringValue.Contains(","))
                        {
                            var inClauseValues = string.Join(",", stringValue.Split(',')
                                                            .Select(value => $"'{value.Trim()}'"));
                            whereClause.Add($"{columnName} IN ({inClauseValues})");
                        }
                        else
                        {
                            whereClause.Add($"{columnName} = @{columnName}");
                        }
                    }
                    else
                    {
                        whereClause.Add($"{columnName} = @{columnName}");
                    }
                }
            }

            return "WHERE " + string.Join(" AND ", whereClause);
        }

        #endregion

        #region "Dynamic Component Helpers"
        private string getDynamicParamValue(string paramName, string defaultValue = null)
        {
            string paramValue = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues[this.buildDynamicParamName(paramName)];
            if (!string.IsNullOrEmpty(paramValue))
                return paramValue;
            return defaultValue;
        }

        private string getDynamicParamDisplayValue(string paramName)
        {
            return args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues[$"!{this.buildDynamicParamName(paramName)}!"];
        }

        private string buildDynamicParamName(string paramName)
        {
            return $"{paramName}_dynamic_{this.getDynamicSuffix(args.ComponentInfo.Component.Name)}";
        }

        private string getDynamicSuffix(string name)
        {
            if (!name.Contains("_dynamic_"))
                return string.Empty;
            return name.Split("_dynamic_")[1];
        }

        private Guid getDynamicParamGuid(string paramName)
        {
            string paramValue = this.getDynamicParamValue(paramName, Guid.Empty.ToString());
            if (string.IsNullOrEmpty(paramValue))
                return Guid.Empty;
            return Guid.Parse(paramValue);
        }

        private string getIdFromComponentName(string componentName)
        {
            return this.getDynamicSuffix(componentName);
        }
        #endregion

        #region "Src Cell Config Functions"
        private XFSelectionChangedTaskResult SrcCellConfigCube_Save(string runType)
        {
            try
            {
                var src_CellDb = new FMM_SrcCellDB(si);

                switch (runType.ToUpperInvariant())
                {
                    case "UPDATE":
                        {
                            var componentId = this.getIdFromComponentName(args.ComponentInfo.Component.Name);
                            var dynamicSuffix = this.getDynamicSuffix(args.ComponentInfo.Component.Name);
                            gbl_ModelType = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("DL_FMM_CalcType", "Cube");
                            gbl_CalcConfigID = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_CalcConfigID", "0").XFConvertToInt();

                            var srcCellModels = FMM_ConfigHelpers.BuildSrcCellModels(
                                args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues,
                                componentId,
                                dynamicSuffix,
                                (paramName, defaultValue) => this.getDynamicParamValue(paramName, defaultValue),
                                paramName => this.getDynamicParamGuid(paramName));

                            src_CellDb.Merge(srcCellModels, 1);

                            if (gbl_ModelType.XFEqualsIgnoreCase("Cube"))
                            {
                                EvaluateCalcConfigSetup(gbl_CalcConfigID);
                            }

                            return new XFSelectionChangedTaskResult
                            {
                                IsOK = true,
                                ShowMessageBox = false
                            };
                        }
                    case "DELETE":
                        {
                            gbl_ModelType = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("DL_FMM_CalcType", "Cube");
                            gbl_CalcConfigID = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_CalcConfigID", "0").XFConvertToInt();
                            src_CellDb.Delete(this.getIdFromComponentName(args.ComponentInfo.Component.Name));
                            if (gbl_ModelType.XFEqualsIgnoreCase("Cube"))
                            {
                                EvaluateCalcConfigSetup(gbl_CalcConfigID);
                            }
                            return new XFSelectionChangedTaskResult
                            {
                                ChangeCustomSubstVarsInDashboard = true,
                                IsOK = true,
                                ShowMessageBox = false
                            };
                        }
                    case "ADD":
                        {
                            gbl_ModelType ="Cube";
                            gbl_CalcConfigID = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("BL_FMM_CalcConfigID", "0").XFConvertToInt();
                            var customSubstVars = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues;
							BRApi.ErrorLog.LogMessage(si,$"Hit GBL {gbl_CalcConfigID}");
                            src_CellDb.CreateDefaultConfig(args.EmbeddedDashboard.WorkspaceID, customSubstVars);
                            if (gbl_ModelType.XFEqualsIgnoreCase("Cube"))
                            {
                                EvaluateCalcConfigSetup(gbl_CalcConfigID);
                            }
                            return new XFSelectionChangedTaskResult
                            {
                                ChangeCustomSubstVarsInDashboard = true,
                                IsOK = true,
                                ShowMessageBox = false
                            };
                        }
                    default:
                        return new XFSelectionChangedTaskResult
                        {
                            IsOK = false,
                            ShowMessageBox = true,
                            Message = $"Unknown runType '{runType}' for SrcCellConfigCube_Save."
                        };
                }
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
        #endregion
    }
}