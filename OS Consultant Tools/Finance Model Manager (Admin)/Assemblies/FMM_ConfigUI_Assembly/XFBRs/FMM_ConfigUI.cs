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

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardStringFunction.FMM_ConfigUI
{
    public class MainClass
    {
#region "Global Variables"
        private SessionInfo si;
        private BRGlobals globals;
        private object api;
        private DashboardStringFunctionArgs args;
#endregion
        public object Main(SessionInfo si, BRGlobals globals, object api, DashboardStringFunctionArgs args)
        {
            try
            {
                this.si = si;
                this.globals = globals;
                this.api = api;
                this.args = args;
                if (args.FunctionName.XFEqualsIgnoreCase("Get_Clean_Username"))
                {
                    // Get the User Document Folder with the Clean Name (Consistent with Platform Folder Naming)
                    return StringHelper.RemoveSystemCharacters(si.AuthToken.UserName, true, false);
                }
                else if (args.FunctionName.XFEqualsIgnoreCase("Get_CalcColFormat"))
                {
                    return Get_CalcColFormat();
                }
                else if (args.FunctionName.XFEqualsIgnoreCase("Get_CubeConfigDB"))
                {
                    return Get_CubeConfigDB();
                }
                else if (args.FunctionName.XFEqualsIgnoreCase("Get_AcctConfigDB"))
                {
                    return Get_AcctConfigDB();
                }
                else if (args.FunctionName.XFEqualsIgnoreCase("Get_CustTableConfigDB"))
                {
                    return Get_CustTableDB();
                }
                else if (args.FunctionName.XFEqualsIgnoreCase("Get_CustTableIndexConfigDB"))
                {
                    return Get_CustTableDB();
                }
                else if (args.FunctionName.XFEqualsIgnoreCase("Get_ModelDB"))
                {
                    return Get_ModelDB();
                }
                else if (args.FunctionName.XFEqualsIgnoreCase("Get_CalcConfigDB"))
                {
                    return Get_CalcDB();
                }
                else if (args.FunctionName.XFEqualsIgnoreCase("Get_DBObjectVisibleEnabled"))
                {
                    return Get_DBObjectVisibleEnabled();
                }
                else if (args.FunctionName.XFEqualsIgnoreCase("Get_DBMbrListObjectVisibleEnabled"))
                {
					BRApi.ErrorLog.LogMessage(si, $"Calling Get_DBMbrListObjectVisibleEnabled");
                    return Get_DBMbrListObjectVisibleEnabled();
                }
				
                return null;

            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
#region "XFBR Dyn DB"
        private string Get_CubeConfigDB()
        {
            var cubeConfigType = args.NameValuePairs.XFGetValue("CubeConfigType", "NA");
            var cubeConfigID = args.NameValuePairs.XFGetValue("CubeConfigID", "0");
            var currDB = args.NameValuePairs.XFGetValue("currDB", "NA");
            var isUpdate = cubeConfigType.XFEqualsIgnoreCase("Update");
            if (currDB.XFEqualsIgnoreCase("FMM_CubeConfig_C2"))
            {
                int.TryParse(cubeConfigID, out var cubeconfigID);

                if (isUpdate)
                {
                    if (cubeconfigID == 0)
                    {
                        return "FMM_CubeConfig_C2_Blank";
                    }
                    return "FMM_CubeConfig_C2_Update";
                }
                return "FMM_CubeConfig_C2_Add";
            }
            else
            {
                if (isUpdate)
                {
                    return $"{currDB}_Update";
                }
                return $"{currDB}_Add";
            }
        }

        private string Get_CustTableDB()
        {
            var CustTableConfigType = args.NameValuePairs.XFGetValue("custTableConfigType", "NA");
            var CustTableID = args.NameValuePairs.XFGetValue("custTableID", "0");
            var currDB = args.NameValuePairs.XFGetValue("currDB", "NA");
            if (currDB.XFEqualsIgnoreCase("FMM_CustTableDef_C2"))
            {
                var isUpdate = CustTableConfigType.XFEqualsIgnoreCase("Update");
                int.TryParse(CustTableID, out var custTableId);

                if (isUpdate && custTableId == 0)
                {
                    return "FMM_CustTableDef_C2_Blank";
                }
                return "FMM_CustTableDef_C2_AddUpdate";
            }
            else
            {
                var isUpdate = CustTableConfigType.XFEqualsIgnoreCase("Update");

                if (isUpdate)
                {
                    return $"{currDB}_Update";
                }

                return $"{currDB}_Add";
            }
        }

        private string Get_AcctConfigDB()
        {
            var acctConfigType = args.NameValuePairs.XFGetValue("acctConfigType", "NA");
            var unitID = args.NameValuePairs.XFGetValue("UnitID", "0");
            var acctID = args.NameValuePairs.XFGetValue("AcctID", "0");
            var currDB = args.NameValuePairs.XFGetValue("currDB", "NA");
            if (currDB.XFEqualsIgnoreCase("FMM_UnitAcctConfig_C2R2") || currDB.XFEqualsIgnoreCase("FMM_UnitAcctConfig_C2R2C2"))
            {
                var isUpdate = acctConfigType.XFEqualsIgnoreCase("Update");
                int.TryParse(unitID, out var intUnitID);

                if (isUpdate && intUnitID == 0)
                {
                    return $"{currDB}_Blank";
                }

                return $"{currDB}_AddUpdate";
            }
            else
            {
                var isUpdate = acctConfigType.XFEqualsIgnoreCase("Update");

                if (isUpdate)
                {
                    return $"{currDB}_Update";
                }

                return $"{currDB}_Add";
            }
            return currDB;
        }

        private string Get_UIConfigDB()
        {
            var CalcAddUpdate = args.NameValuePairs.XFGetValue("CalcAddUpdate", "NA");
            var CalcType = args.NameValuePairs.XFGetValue("CalcType", "NA");
            var calcConfigID = args.NameValuePairs.XFGetValue("CalcConfigID", "0");
            var currDB = args.NameValuePairs.XFGetValue("currDB", "NA");
            if (currDB.XFEqualsIgnoreCase("FMM_ModelConfigC2C2"))
            {
                var isUpdate = CalcAddUpdate.XFEqualsIgnoreCase("Update");
                int.TryParse(calcConfigID, out var calcconfigID);

                if (isUpdate && calcconfigID == 0)
                {
                    return "FMM_ModelConfigC2C2_Blank";
                }

                return "FMM_ModelConfigC2C2_AddUpdate";
            }
            return currDB;
        }
        private string Get_ApprConfigDB()
        {
            var CalcAddUpdate = args.NameValuePairs.XFGetValue("CalcAddUpdate", "NA");
            var CalcType = args.NameValuePairs.XFGetValue("CalcType", "NA");
            var calcConfigID = args.NameValuePairs.XFGetValue("CalcConfigID", "0");
            var currDB = args.NameValuePairs.XFGetValue("currDB", "NA");
            if (currDB.XFEqualsIgnoreCase("FMM_ModelConfigC2C2"))
            {
                var isUpdate = CalcAddUpdate.XFEqualsIgnoreCase("Update");
                int.TryParse(calcConfigID, out var calcconfigID);

                if (isUpdate && calcconfigID == 0)
                {
                    return "FMM_ModelConfigC2C2_Blank";
                }

                return "FMM_ModelConfigC2C2_AddUpdate";
            }
            return currDB;
        }

        private string Get_ModelDB()
        {
            var ModelType = args.NameValuePairs.XFGetValue("ModelType", "NA");
            var ModelConfigID = args.NameValuePairs.XFGetValue("ModelConfigID", "0");
            var currDB = args.NameValuePairs.XFGetValue("currDB", "NA");
            if (currDB.XFEqualsIgnoreCase("FMM_ModelConfigC1R2"))
            {
                var isUpdate = ModelType.XFEqualsIgnoreCase("Update");
                int.TryParse(ModelConfigID, out var modelConfigID);

                if (isUpdate && modelConfigID == 0)
                {
                    return "FMM_ModelConfigC1R2_Blank";
                }

                return "FMM_ModelConfigC1R2_AddUpdate";
            }
            else
            {
                var isUpdate = ModelType.XFEqualsIgnoreCase("Update");

                if (isUpdate)
                {
                    return $"{currDB}_Update";
                }

                return $"{currDB}_Add";
            }
            return currDB;
        }


        private string Get_CalcDB()
        {
            var calcType = args.NameValuePairs.XFGetValue("CalcType", "0").XFConvertToInt();
            var calcConfig = FMM_ConfigHelpers.Get_CalcConfigType(calcType);
            if (calcConfig != null)
            {
                return calcConfig.DashboardName;
            }
            else
            {
                return string.Empty;
            }
        }
#endregion
		
        private string Get_CalcColFormat()
        {
			var currTED = args.NameValuePairs.XFGetValue("currTED","NA");
            var currModelType = args.NameValuePairs.XFGetValue("currModelType","NA");
            var col = args.NameValuePairs.XFGetValue("col","0");
//            if (currTED.Equals("FMM_CalcConfig", StringComparison.OrdinalIgnoreCase))
//            {
//                if (int.TryParse(col, out int colIndex) && colIndex > 0)
//                {
//                    var modelTypeKey = currModelType.Equals("Table", StringComparison.OrdinalIgnoreCase) ? "Table" : "Cube";

//                    if (ModelColumnFormatter.CalcConfigColumns.TryGetValue(modelTypeKey, out var columns) && colIndex <= columns.Length)
//                    {
//                        return columns[colIndex - 1].ToString();
//                    }
//                }
//            }
            if (currTED.Equals("Dest", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(col, out int colIndex) && colIndex > 0)
                {
//                    var modelTypeKey = currModelType.Equals("Table", StringComparison.OrdinalIgnoreCase) ? "Table" : "Cube";
					// Added - Devlin
					string modelTypeKey = "Table";
			        if (currModelType.Equals("Table", StringComparison.OrdinalIgnoreCase)) { modelTypeKey = "Table"; }
			        else if (currModelType.Equals("Cube", StringComparison.OrdinalIgnoreCase)) { modelTypeKey = "Cube"; }
			        else if (currModelType.Equals("CubeToTable", StringComparison.OrdinalIgnoreCase)) { modelTypeKey = "CubeToTable"; }
			        else if (currModelType.Equals("BRTabletoCube", StringComparison.OrdinalIgnoreCase)) { modelTypeKey = "BRTabletoCube"; }
			        else if (currModelType.Equals("Consol", StringComparison.OrdinalIgnoreCase)) { modelTypeKey = "Consol"; }
					

                    if (ModelColumnFormatter.DestCellColumns.TryGetValue(modelTypeKey, out var columns) && colIndex <= columns.Length)
                    {
                        return columns[colIndex - 1].ToString();
                    }
                }
            }
            else if (currTED.Equals("Src", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(col, out int colIndex) && colIndex > 0)
                {
//                    var modelTypeKey = currModelType.Equals("Table", StringComparison.OrdinalIgnoreCase) ? "Table" : "Cube";
					// Added - Devlin
					string modelTypeKey = "Table";
					if (currModelType.Equals("Table", StringComparison.OrdinalIgnoreCase)) { modelTypeKey = "Table"; }
			        else if (currModelType.Equals("Cube", StringComparison.OrdinalIgnoreCase)) { modelTypeKey = "Cube"; }
			        else if (currModelType.Equals("CubeToTable", StringComparison.OrdinalIgnoreCase)) { modelTypeKey = "CubeToTable"; }
			        else if (currModelType.Equals("BRTabletoCube", StringComparison.OrdinalIgnoreCase)) { modelTypeKey = "BRTabletoCube"; }
			        else if (currModelType.Equals("Consol", StringComparison.OrdinalIgnoreCase)) { modelTypeKey = "Consol"; }

                    if (ModelColumnFormatter.SrcCellColumns.TryGetValue(modelTypeKey, out var columns) && colIndex <= columns.Length)
                    {
                        return columns[colIndex - 1].ToString();
                    }
                }
            }
            return null;
        }

        private string Get_DBObjectVisibleEnabled()
        {
            var objName= args.NameValuePairs.XFGetValue("ObjectName","NA");
			switch (objName.ToLower())
		    {
		        case "activityconfig":
		            

		
		        case "calcunits":

		
		        case "customtabledef":

		
		        case "na":


		
		        default:
					return "False";
		    }
		
		    // Final fallback
		    return "False";
        }
		
		private string Get_DBMbrListObjectVisibleEnabled()
		{
			//Get value of each check box
			string CubeCalcConfigObject = args.NameValuePairs.XFGetValue("CubeCalcConfigObject","NA");
			string MbrListCalcYesNo = args.NameValuePairs.XFGetValue("MbrListCalcYesNo","False");
			string MbrListCalc1YesNo = args.NameValuePairs.XFGetValue("MbrListCalc1YesNo","False");
			string MbrListCalc2YesNo = args.NameValuePairs.XFGetValue("MbrListCalc2YesNo","False");
			string MbrListCalc3YesNo = args.NameValuePairs.XFGetValue("MbrListCalc3YesNo","False");
			string MbrListCalc4YesNo = args.NameValuePairs.XFGetValue("MbrListCalc4YesNo","False");
			string result = string.Empty;
			
			switch (CubeCalcConfigObject)
			{
				case "chk_FMM_CalcConfig_MbrList1Calc":
					result = MbrListCalcYesNo;
				break;
					
				// Show objects in FMM_CalcConfig_Cube_R1R2C2R1
				case "cbx_FMM_CalcConfig_MbrList1DimType":
				case "cbx_FMM_CalcConfig_MbrList1Dim":
				case "txt_FMM_CalcConfig_MbrList1Filter":
				case "chk_FMM_CalcConfig_BRCalc":
				case "txt_FMM_CalcConfig_BRName":
				case "chk_FMM_CalcConfig_MbrList2Calc":
					result = (MbrListCalcYesNo.XFEqualsIgnoreCase("True") && MbrListCalc1YesNo.XFEqualsIgnoreCase("True")) ? "True" : "False";
				break;
					
				// Show objects in FMM_CalcConfig_Cube_R1R2C2R2
				case "cbx_FMM_CalcConfig_MbrList2DimType":
				case "cbx_FMM_CalcConfig_MbrList2Dim":
				case "txt_FMM_CalcConfig_MbrList2Filter":
				case "chk_FMM_CalcConfig_MbrList3Calc":
					result = (MbrListCalcYesNo.XFEqualsIgnoreCase("True") && MbrListCalc1YesNo.XFEqualsIgnoreCase("True") && MbrListCalc2YesNo.XFEqualsIgnoreCase("True")) ? "True" : "False";
				break;
					
				// Show objects in FMM_CalcConfig_Cube_R1R2C2R3
				case "cbx_FMM_CalcConfig_MbrList3DimType":
				case "cbx_FMM_CalcConfig_MbrList3Dim":
				case "txt_FMM_CalcConfig_MbrList3Filter":
				case "chk_FMM_CalcConfig_MbrList4Calc":
					result = (MbrListCalcYesNo.XFEqualsIgnoreCase("True") && MbrListCalc1YesNo.XFEqualsIgnoreCase("True") && MbrListCalc2YesNo.XFEqualsIgnoreCase("True") && MbrListCalc3YesNo.XFEqualsIgnoreCase("True")) ? "True" : "False";
				break;
					
				// Show objects in FMM_CalcConfig_Cube_R1R2C2R4
				case "cbx_FMM_CalcConfig_MbrList4DimType":
				case "cbx_FMM_CalcConfig_MbrList4Dim":
				case "txt_FMM_CalcConfig_MbrList4Filter":
					result = (MbrListCalcYesNo.XFEqualsIgnoreCase("True") && MbrListCalc1YesNo.XFEqualsIgnoreCase("True") && MbrListCalc2YesNo.XFEqualsIgnoreCase("True") && MbrListCalc3YesNo.XFEqualsIgnoreCase("True") && MbrListCalc4YesNo.XFEqualsIgnoreCase("True")) ? "True" : "False";
				break;
					
				default:
					result = "False";
					break;
			}
			
			return result;
			
			
		}
		
        public class ColumnConfig
        {
            public string ColumnName { get; set; }
            public string Description { get; set; }
            public bool IsVisible { get; set; }
            public string Width { get; set; } = "Auto";
            public bool AllowUpdates { get; set; } = true;
            public string DefaultValue { get; set; }
            public string ParameterName { get; set; }

            public override string ToString()
            {
                var parts = new List<string>();
                if (!AllowUpdates) parts.Add("AllowUpdates = False");
                if (!string.IsNullOrEmpty(ColumnName)) parts.Add($"ColumnName = {ColumnName}");
                if (!string.IsNullOrEmpty(DefaultValue)) parts.Add($"DefaultValue = {DefaultValue}");
                if (!string.IsNullOrEmpty(Description)) parts.Add($"Description = {Description}");
                if (!string.IsNullOrEmpty(Width)) parts.Add($"Width = {Width}");
                if (!string.IsNullOrEmpty(ParameterName)) parts.Add($"ParameterName = {ParameterName}");
                if (IsVisible) parts.Add("IsVisible = True");

                return string.Join(", ", parts);
            }
        }

        public class ModelColumnFormatter
        {
//            public static readonly Dictionary<string, ColumnConfig[]> CalcConfigColumns = new Dictionary<string, ColumnConfig[]>
//            {
//                { "Table", new ColumnConfig[]
//                    {
//                        new ColumnConfig { ColumnName = "CubeConfigID", IsVisible = false, AllowUpdates = false, DefaultValue = "|!BL_FMM_CubeConfigID!|" },
//                        new ColumnConfig { ColumnName = "ActConfigID", IsVisible = false, AllowUpdates = false, DefaultValue = "|!IV_FMM_ActConfigID!|" },
//                        new ColumnConfig { ColumnName = "ModelConfigID", IsVisible = false, AllowUpdates = false, DefaultValue = "|!IV_FMM_ModelConfigID!|" },
//                        new ColumnConfig { ColumnName = "CalcConfigID", IsVisible = false, AllowUpdates = false},
//                        new ColumnConfig { ColumnName = "Sequence", Description = "Seq", IsVisible = true },
//                        new ColumnConfig { ColumnName = "Name", Description = "[Calc Name]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "Explanation", Description = "[Calc Explanation]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "Condition", Description = "[Conditional Logic]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "MultiDimAlloc", Description = "[Multi-Dim Alloc]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "BRCalc", Description = "[BR Calc]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "BRCalcName", Description = "[BR Calc Name]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "TimePhase", Description = "[Time Phasing]", IsVisible = true, ParameterName = "DL_FMM_CalcConfig_Time_Phasing" },
//                        new ColumnConfig { ColumnName = "InputFrequency", Description = "[Input Freq]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "Status", Description = "[Status]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "CreateDate", Description = "[Create Date]", IsVisible = true, AllowUpdates = false },
//                        new ColumnConfig { ColumnName = "CreateUser", Description = "[Create User]", IsVisible = true, AllowUpdates = false },
//                        new ColumnConfig { ColumnName = "UpdateDate", Description = "[Update Date]", IsVisible = true, AllowUpdates = false },
//                        new ColumnConfig { ColumnName = "UpdateUser", Description = "[Update User]", IsVisible = true, AllowUpdates = false }
//                    }
//                },
//                { "Consol", new ColumnConfig[]
//                    {
//                        new ColumnConfig { ColumnName = "CubeConfigID", IsVisible = false, AllowUpdates = false, DefaultValue = "|!BL_FMM_CubeConfigID!|" },
//                        new ColumnConfig { ColumnName = "ActConfigID", IsVisible = false, AllowUpdates = false, DefaultValue = "|!IV_FMM_ActConfigID!|" },
//                        new ColumnConfig { ColumnName = "ModelConfigID", IsVisible = false, AllowUpdates = false, DefaultValue = "|!IV_FMM_ModelConfigID!|" },
//                        new ColumnConfig { ColumnName = "CalcConfigID", IsVisible = false, AllowUpdates = false},
//                        new ColumnConfig { ColumnName = "Sequence", Description = "Seq", IsVisible = true },
//                        new ColumnConfig { ColumnName = "Name", Description = "[Calc Name]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "Condition", Description = "[Conditional Logic]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "Explanation", Description = "[Calc Explanation]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "MultiDimAlloc", Description = "[Multi-Dim Alloc]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "MbrList_Calc", Description = "[Mbr List Calc?]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "MbrList_1_Dim", Description = "[Mbr List 1 Dim]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "MbrList_1_Filter", Description = "[Mbr List 1 Filter]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "MbrList_1_DimType", IsVisible = false },
//                        new ColumnConfig { ColumnName = "MbrList_1_Filter", IsVisible = false },
//                        new ColumnConfig { ColumnName = "MbrList_2_Dim", Description = "[Mbr List 2 Dim]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "MbrList_2_Filter", Description = "[Mbr List 2 Filter]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "MbrList_2_DimType", IsVisible = false },
//                        new ColumnConfig { ColumnName = "MbrList_2_Filter", IsVisible = false },
//                        new ColumnConfig { ColumnName = "MbrList_3_Dim", Description = "[Mbr List 3 Dim]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "MbrList_3_Filter", Description = "[Mbr List 3 Filter]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "MbrList_3_DimType", IsVisible = false },
//                        new ColumnConfig { ColumnName = "MbrList_3_Filter", IsVisible = false },
//                        new ColumnConfig { ColumnName = "MbrList_4_Dim", Description = "[Mbr List 4 Dim]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "MbrList_4_Filter", Description = "[Mbr List 4 Filter]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "MbrList_4_DimType", IsVisible = false },
//                        new ColumnConfig { ColumnName = "MbrList_4_Filter", IsVisible = false },
//                        new ColumnConfig { ColumnName = "BR_Calc", Description = "[BR Calc?]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "BR_Calc_Name", Description = "[BR Calc Name]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "Status", Description = "[Status]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "Create_Date", Description = "[Create Date]", IsVisible = true, AllowUpdates = false },
//                        new ColumnConfig { ColumnName = "Create_User", Description = "[Create User]", IsVisible = true, AllowUpdates = false },
//                        new ColumnConfig { ColumnName = "Update_Date", Description = "[Update Date]", IsVisible = true, AllowUpdates = false },
//                        new ColumnConfig { ColumnName = "Update_User", Description = "[Update User]", IsVisible = true, AllowUpdates = false }
//                    }
//                },
//                { "BRTabletoCube", new ColumnConfig[]
//                    {
//                        new ColumnConfig { ColumnName = "CubeConfigID", IsVisible = false, AllowUpdates = false, DefaultValue = "|!BL_FMM_CubeConfigID!|" },
//                        new ColumnConfig { ColumnName = "ActConfigID", IsVisible = false, AllowUpdates = false, DefaultValue = "|!IV_FMM_ActConfigID!|" },
//                        new ColumnConfig { ColumnName = "ModelConfigID", IsVisible = false, AllowUpdates = false, DefaultValue = "|!IV_FMM_ModelConfigID!|" },
//                        new ColumnConfig { ColumnName = "CalcConfigID", IsVisible = false, AllowUpdates = false},
//                        new ColumnConfig { ColumnName = "Sequence", Description = "Seq", IsVisible = true },
//                        new ColumnConfig { ColumnName = "Name", Description = "[Calc Name]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "Condition", Description = "[Conditional Logic]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "Explanation", Description = "[Calc Explanation]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "MultiDimAlloc", Description = "[Multi-Dim Alloc]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "MbrList_Calc", Description = "[Mbr List Calc?]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "MbrList_1_Dim", Description = "[Mbr List 1 Dim]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "MbrList_1_Filter", Description = "[Mbr List 1 Filter]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "MbrList_1_DimType", IsVisible = false },
//                        new ColumnConfig { ColumnName = "MbrList_1_Filter", IsVisible = false },
//                        new ColumnConfig { ColumnName = "MbrList_2_Dim", Description = "[Mbr List 2 Dim]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "MbrList_2_Filter", Description = "[Mbr List 2 Filter]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "MbrList_2_DimType", IsVisible = false },
//                        new ColumnConfig { ColumnName = "MbrList_2_Filter", IsVisible = false },
//                        new ColumnConfig { ColumnName = "MbrList_3_Dim", Description = "[Mbr List 3 Dim]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "MbrList_3_Filter", Description = "[Mbr List 3 Filter]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "MbrList_3_DimType", IsVisible = false },
//                        new ColumnConfig { ColumnName = "MbrList_3_Filter", IsVisible = false },
//                        new ColumnConfig { ColumnName = "MbrList_4_Dim", Description = "[Mbr List 4 Dim]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "MbrList_4_Filter", Description = "[Mbr List 4 Filter]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "MbrList_4_DimType", IsVisible = false },
//                        new ColumnConfig { ColumnName = "MbrList_4_Filter", IsVisible = false },
//                        new ColumnConfig { ColumnName = "BR_Calc", Description = "[BR Calc?]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "BR_Calc_Name", Description = "[BR Calc Name]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "Status", Description = "[Status]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "Create_Date", Description = "[Create Date]", IsVisible = true, AllowUpdates = false },
//                        new ColumnConfig { ColumnName = "Create_User", Description = "[Create User]", IsVisible = true, AllowUpdates = false },
//                        new ColumnConfig { ColumnName = "Update_Date", Description = "[Update Date]", IsVisible = true, AllowUpdates = false },
//                        new ColumnConfig { ColumnName = "Update_User", Description = "[Update User]", IsVisible = true, AllowUpdates = false }
//                    }
//                },
//                { "BRCubetoTable", new ColumnConfig[]
//                    {
//                        new ColumnConfig { ColumnName = "CubeConfigID", IsVisible = false, AllowUpdates = false, DefaultValue = "|!BL_FMM_CubeConfigID!|" },
//                        new ColumnConfig { ColumnName = "ActConfigID", IsVisible = false, AllowUpdates = false, DefaultValue = "|!IV_FMM_ActConfigID!|" },
//                        new ColumnConfig { ColumnName = "ModelConfigID", IsVisible = false, AllowUpdates = false, DefaultValue = "|!IV_FMM_ModelConfigID!|" },
//                        new ColumnConfig { ColumnName = "CalcConfigID", IsVisible = false, AllowUpdates = false},
//                        new ColumnConfig { ColumnName = "Sequence", Description = "Seq", IsVisible = true },
//                        new ColumnConfig { ColumnName = "Name", Description = "[Calc Name]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "Condition", Description = "[Conditional Logic]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "Explanation", Description = "[Calc Explanation]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "MultiDimAlloc", Description = "[Multi-Dim Alloc]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "MbrList_Calc", Description = "[Mbr List Calc?]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "MbrList_1_Dim", Description = "[Mbr List 1 Dim]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "MbrList_1_Filter", Description = "[Mbr List 1 Filter]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "MbrList_1_DimType", IsVisible = false },
//                        new ColumnConfig { ColumnName = "MbrList_1_Filter", IsVisible = false },
//                        new ColumnConfig { ColumnName = "MbrList_2_Dim", Description = "[Mbr List 2 Dim]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "MbrList_2_Filter", Description = "[Mbr List 2 Filter]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "MbrList_2_DimType", IsVisible = false },
//                        new ColumnConfig { ColumnName = "MbrList_2_Filter", IsVisible = false },
//                        new ColumnConfig { ColumnName = "MbrList_3_Dim", Description = "[Mbr List 3 Dim]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "MbrList_3_Filter", Description = "[Mbr List 3 Filter]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "MbrList_3_DimType", IsVisible = false },
//                        new ColumnConfig { ColumnName = "MbrList_3_Filter", IsVisible = false },
//                        new ColumnConfig { ColumnName = "MbrList_4_Dim", Description = "[Mbr List 4 Dim]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "MbrList_4_Filter", Description = "[Mbr List 4 Filter]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "MbrList_4_DimType", IsVisible = false },
//                        new ColumnConfig { ColumnName = "MbrList_4_Filter", IsVisible = false },
//                        new ColumnConfig { ColumnName = "BR_Calc", Description = "[BR Calc?]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "BR_Calc_Name", Description = "[BR Calc Name]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "Status", Description = "[Status]", IsVisible = true },
//                        new ColumnConfig { ColumnName = "Create_Date", Description = "[Create Date]", IsVisible = true, AllowUpdates = false },
//                        new ColumnConfig { ColumnName = "Create_User", Description = "[Create User]", IsVisible = true, AllowUpdates = false },
//                        new ColumnConfig { ColumnName = "Update_Date", Description = "[Update Date]", IsVisible = true, AllowUpdates = false },
//                        new ColumnConfig { ColumnName = "Update_User", Description = "[Update User]", IsVisible = true, AllowUpdates = false }
//                    }
//                }
//            };

            public static readonly Dictionary<string, ColumnConfig[]> DestCellColumns = new Dictionary<string, ColumnConfig[]>
            {
                { "Table", new ColumnConfig[]
                    {
                        new ColumnConfig { ColumnName = "CubeConfigID", IsVisible = false },
                        new ColumnConfig { ColumnName = "ActConfigID", IsVisible = false },
                        new ColumnConfig { ColumnName = "ModelConfigID", IsVisible = false },
                        new ColumnConfig { ColumnName = "CalcConfigID", IsVisible = false },
                        new ColumnConfig { ColumnName = "DestConfigID", IsVisible = false },
                        new ColumnConfig { ColumnName = "Location", Description = "Target Location", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UoM", Description = "[Unit of Measure]", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Acct", Description = "Account", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "View", Description = "View", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "IC", Description = "IC", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Flow", Description = "Flow", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD1", Description = "UD1", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD2", Description = "UD2", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD3", Description = "UD3", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD4", Description = "UD4", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD5", Description = "UD5", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD6", Description = "UD6", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD7", Description = "UD7", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD8", Description = "UD8", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "TimeFilter", Description = "Time Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "ConditionalFilter", Description = "Conditional Filter", Width = "Auto", IsVisible = true },
			            // Add more placeholders if needed
			        }
                },
                { "Cube", new ColumnConfig[]
                    {
                        new ColumnConfig { ColumnName = "CubeConfigID", IsVisible = false },
                        new ColumnConfig { ColumnName = "ActConfigID", IsVisible = false },
                        new ColumnConfig { ColumnName = "ModelConfigID", IsVisible = false },
                        new ColumnConfig { ColumnName = "CalcConfigID", IsVisible = false },
                        new ColumnConfig { ColumnName = "DestConfigID", IsVisible = false },
                        new ColumnConfig { ColumnName = "Acct", Description = "Account", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "View", Description = "View", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Origin", Description = "Origin", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "IC", Description = "IC", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Flow", Description = "Flow", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD1", Description = "UD1", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD2", Description = "UD2", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD3", Description = "UD3", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD4", Description = "UD4", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD5", Description = "UD5", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD6", Description = "UD6", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD7", Description = "UD7", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD8", Description = "UD8", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Cube", Description = "Cube", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "EntFilter", Description = "Entity Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "ParentFilter", Description = "Parent Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "ConsFilter", Description = "Consolidation Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "ScenFilter", Description = "Scenario Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "TimeFilter", Description = "Time Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "AcctFilter", Description = "Account Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "OriginFilter", Description = "Origin Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "FlowFilter", Description = "Flow Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD1Filter", Description = "UD1 Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD2Filter", Description = "UD2 Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD3Filter", Description = "UD3 Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD4Filter", Description = "UD4 Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD5Filter", Description = "UD5 Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD6Filter", Description = "UD6 Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD7Filter", Description = "UD7 Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD8Filter", Description = "UD8 Filter", Width = "Auto", IsVisible = true },
			            // Add more placeholders if needed
			        }
                },
                { "Consol", new ColumnConfig[]
                    {
                        new ColumnConfig { ColumnName = "CubeConfigID", IsVisible = false },
                        new ColumnConfig { ColumnName = "ActConfigID", IsVisible = false },
                        new ColumnConfig { ColumnName = "ModelConfigID", IsVisible = false },
                        new ColumnConfig { ColumnName = "CalcConfigID", IsVisible = false },
                        new ColumnConfig { ColumnName = "DestConfigID", IsVisible = false },
                        new ColumnConfig { ColumnName = "Cube", Description = "Cube", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "EntFilter", Description = "Entity Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "ParentFilter", Description = "Parent Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "ConsFilter", Description = "Consolidation Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "ScenFilter", Description = "Scenario Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "TimeFilter", Description = "Time Filter", Width = "Auto", IsVisible = true }
                    }
                },
                { "BRTabletoCube", new ColumnConfig[]
                    {
                        new ColumnConfig { ColumnName = "CubeConfigID", IsVisible = false },
                        new ColumnConfig { ColumnName = "ActConfigID", IsVisible = false },
                        new ColumnConfig { ColumnName = "ModelConfigID", IsVisible = false },
                        new ColumnConfig { ColumnName = "CalcConfigID", IsVisible = false },
                        new ColumnConfig { ColumnName = "DestConfigID", IsVisible = false },
                        new ColumnConfig { ColumnName = "Acct", Description = "Account", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "View", Description = "View", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Origin", Description = "Origin", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "IC", Description = "IC", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Flow", Description = "Flow", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD1", Description = "UD1", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD2", Description = "UD2", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD3", Description = "UD3", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD4", Description = "UD4", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD5", Description = "UD5", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD6", Description = "UD6", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD7", Description = "UD7", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD8", Description = "UD8", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Cube", Description = "Cube", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "EntFilter", Description = "Entity Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "ParentFilter", Description = "Parent Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "ConsFilter", Description = "Consolidation Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "ScenFilter", Description = "Scenario Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "TimeFilter", Description = "Time Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "AcctFilter", Description = "Account Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "OriginFilter", Description = "Origin Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "FlowFilter", Description = "Flow Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD1Filter", Description = "UD1 Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD2Filter", Description = "UD2 Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD3Filter", Description = "UD3 Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD4Filter", Description = "UD4 Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD5Filter", Description = "UD5 Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD6Filter", Description = "UD6 Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD7Filter", Description = "UD7 Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD8Filter", Description = "UD8 Filter", Width = "Auto", IsVisible = true },
			            // Add more placeholders if needed
			        }
                },
                { "BRTabletoCube", new ColumnConfig[]
                    {
                        new ColumnConfig { ColumnName = "CubeConfigID", IsVisible = false },
                        new ColumnConfig { ColumnName = "ActConfigID", IsVisible = false },
                        new ColumnConfig { ColumnName = "ModelConfigID", IsVisible = false },
                        new ColumnConfig { ColumnName = "CalcConfigID", IsVisible = false },
                        new ColumnConfig { ColumnName = "DestConfigID", IsVisible = false },
                        new ColumnConfig { ColumnName = "Acct", Description = "Account", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "View", Description = "View", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Origin", Description = "Origin", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "IC", Description = "IC", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Flow", Description = "Flow", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD1", Description = "UD1", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD2", Description = "UD2", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD3", Description = "UD3", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD4", Description = "UD4", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD5", Description = "UD5", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD6", Description = "UD6", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD7", Description = "UD7", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD8", Description = "UD8", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Cube", Description = "Cube", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "EntFilter", Description = "Entity Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "ParentFilter", Description = "Parent Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "ConsFilter", Description = "Consolidation Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "ScenFilter", Description = "Scenario Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "TimeFilter", Description = "Time Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "AcctFilter", Description = "Account Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "OriginFilter", Description = "Origin Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "FlowFilter", Description = "Flow Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD1Filter", Description = "UD1 Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD2Filter", Description = "UD2 Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD3Filter", Description = "UD3 Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD4Filter", Description = "UD4 Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD5Filter", Description = "UD5 Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD6Filter", Description = "UD6 Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD7Filter", Description = "UD7 Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD8Filter", Description = "UD8 Filter", Width = "Auto", IsVisible = true },
			            // Add more placeholders if needed
			        }
                }
            };
            public static readonly Dictionary<string, ColumnConfig[]> SrcCellColumns = new Dictionary<string, ColumnConfig[]>
            {
                { "Table", new ColumnConfig[]
                    {
                        new ColumnConfig { ColumnName = "Src_Cell_ID", IsVisible = false },
                        new ColumnConfig { ColumnName = "CubeConfigID", IsVisible = false, DefaultValue = "|!BL_FMM_CubeConfigID!|" },
                        new ColumnConfig { ColumnName = "ActConfigID", IsVisible = false, DefaultValue = "|!IV_FMM_ActConfigID!|" },
                        new ColumnConfig { ColumnName = "ModelConfigID", IsVisible = false, DefaultValue = "|!IV_FMM_ModelConfigID!|" },
                        new ColumnConfig { ColumnName = "CalcConfigID", IsVisible = false, DefaultValue = "|!IV_FMM_CalcConfigID!|" },
                        new ColumnConfig { ColumnName = "Src_Order", Description = "Order", IsVisible = true },
                        new ColumnConfig { ColumnName = "Src_Type", Description = "[Source/Calc Type]", ParameterName = "DL_FMM_Table_Calc_Src", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Src_Item", Description = "[Source/Calc Item]", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Open_Parens", Description = "(", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Math_Operator", Description = "Op", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Table_Calc_Expression", Description = "[Calc Expression]", Width = "200", IsVisible = true },
                        new ColumnConfig { ColumnName = "Close_Parens", Description = ")", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Table_Join_Expression", Description = "[Join Expression]", Width = "200", IsVisible = true },
                        new ColumnConfig { ColumnName = "Table_Filter_Expression", Description = "[Filter Expression]", Width = "200", IsVisible = true },
                        new ColumnConfig { ColumnName = "MapType", Description = "[Map Type]", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Map_Source", Description = "[Map Source]", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "MapLogic", Description = "[Map Logic]", Width = "Auto", IsVisible = true },
			            // Add more placeholders if needed
			        }
                },
                { "Cube", new ColumnConfig[]
                    {
                        new ColumnConfig { ColumnName = "Cell_ID", IsVisible = false },
                        new ColumnConfig { ColumnName = "CubeConfigID", IsVisible = false, DefaultValue = "|!BL_FMM_CubeConfigID!|" },
                        new ColumnConfig { ColumnName = "ActConfigID", IsVisible = false, DefaultValue = "|!IV_FMM_ActConfigID!|" },
                        new ColumnConfig { ColumnName = "ModelConfigID", IsVisible = false, DefaultValue = "|!IV_FMM_ModelConfigID!|" },
                        new ColumnConfig { ColumnName = "CalcConfigID", IsVisible = false, DefaultValue = "|!IV_FMM_CalcConfigID!|" },
                        new ColumnConfig { ColumnName = "Src_Order", Description = "Order", IsVisible = true },
                        new ColumnConfig { ColumnName = "Open_Parens", Description = "(", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Math_Operator", Description = "Op", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Entity", Description = "Entity", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Cons", Description = "Cons", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Scenario", Description = "Scenario", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Time", Description = "Time", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Origin", Description = "Origin", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "IC", Description = "IC", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "View", Description = "View", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Acct", Description = "Account", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Flow", Description = "Flow", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD1", Description = "UD1", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD2", Description = "UD2", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD3", Description = "UD3", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD4", Description = "UD4", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD5", Description = "UD5", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD6", Description = "UD6", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD7", Description = "UD7", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD8", Description = "UD8", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Close_Parens", Description = ")", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Src_Type", Description = "[Source Type]", ParameterName = "DL_FMM_Cube_Calc_Source", DefaultValue = "[Stored Cell]", Width = "Auto", IsVisible = true },
			            // Add more placeholders if needed
			        }
                }
            };
        }
    }
}