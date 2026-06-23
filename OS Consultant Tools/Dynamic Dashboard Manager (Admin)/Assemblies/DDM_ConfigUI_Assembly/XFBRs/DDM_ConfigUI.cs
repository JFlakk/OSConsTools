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

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardStringFunction.DDM_ConfigUI
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
                else if (args.FunctionName.XFEqualsIgnoreCase("Get_DDM_ColFormat"))
                {
                    var curr_TED = args.NameValuePairs.XFGetValue("curr_TED");
                    var curr_DB = args.NameValuePairs.XFGetValue("curr_DB");
                    var col = args.NameValuePairs.XFGetValue("col");
                    return Get_DDM_ColFormat(curr_TED, curr_DB, col);
                }
                else if (args.FunctionName.XFEqualsIgnoreCase("Get_MenuDB"))
                {
                    return Get_MenuDB();
                }
				else if (args.FunctionName.XFEqualsIgnoreCase("Get_DDM_LayoutConfigDB"))
				{
					return Get_DDM_LayoutConfigDB();
				}
                else if (args.FunctionName.XFEqualsIgnoreCase("Get_HdrDB"))
                {
                    return Get_HdrDB();
                }
				else if (args.FunctionName.XFEqualsIgnoreCase("Get_ConfigHdrTypeDB"))
				{
					BRApi.ErrorLog.LogMessage(si,"hit: Get_ConfigHdrTypeDB");
					return Get_ConfigHdrTypeDB();
				}	
				else if (args.FunctionName.XFEqualsIgnoreCase("Get_DDM_Config_IsVisible"))
				{
					BRApi.ErrorLog.LogMessage(si,"hit");
					return this.Get_DDM_Config_IsVisible();
				}	
				
				else if (args.FunctionName.XFEqualsIgnoreCase("Get_DDM_Config_IsVisibleHdr"))
				{
					
					return this.Get_DDM_Config_IsVisibleHdr();
				}	

                return null;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        public static string Get_DDM_ColFormat(string curr_ted, string curr_DB, string col)
        {
            if (curr_ted.Equals("Config", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(col, out int colIndex) && colIndex > 0)
                {
                    if (DDMColFormatter.ConfigColumns.TryGetValue(curr_DB, out var columns) && colIndex <= columns.Length)
                    {
                        return columns[colIndex - 1].ToString();
                    }
                }
            }
            return null;
        }
		
        private string Get_MenuDB()
        {
            var menuLayoutConfigType = args.NameValuePairs.XFGetValue("MenuLayoutConfigType", "NA");
            var configID = args.NameValuePairs.XFGetValue("ConfigID", "0");
			var configMenuID = args.NameValuePairs.XFGetValue("ConfigMenuID", "0");
            var currDB = args.NameValuePairs.XFGetValue("currDB", "NA");
            if (currDB.XFEqualsIgnoreCase("DDM_MenuLayoutConfig"))
            {
                int.TryParse(configID, out var configId);

                if (configId == 0)
                {
                    return "DDM_MenuLayoutConfig_Blank";
                }

                return "DDM_MenuLayoutConfig_AddUpdate";
            }
			else if (currDB.XFEqualsIgnoreCase("DDM_MenuLayoutConfig_C2"))
            {
				var isUpdate = menuLayoutConfigType.XFEqualsIgnoreCase("Update");
                int.TryParse(configMenuID, out var configMenuId);
			
                if (isUpdate && configMenuId == 0)
                {
                    return "DDM_MenuLayoutConfig_C2_Blank";
                }
                return "DDM_MenuLayoutConfig_C2_AddUpdate";
            }
			else if (currDB.XFEqualsIgnoreCase("DDM_MenuLayoutConfig_C2R4"))
            {
				var isUpdate = menuLayoutConfigType.XFEqualsIgnoreCase("Update");
                int.TryParse(configMenuID, out var configMenuId);
			
                if (isUpdate && configMenuId == 0)
                {
                    return "DDM_MenuLayoutConfig_C2R4_Update";
                }
                return "DDM_MenuLayoutConfig_C2R4_Add";
            }
            return currDB;
        }

		public string Get_DDM_LayoutConfigDB()
		{
			var layoutType = args.NameValuePairs.XFGetValue("LayoutType","1").XFConvertToInt();
			var configHelpers = new DDM_ConfigHelpers();
			var layoutConfig = configHelpers.Get_LayoutConfig(layoutType);
			BRApi.ErrorLog.LogMessage(si,$"hit LAYU {layoutConfig.Config_DashboardName}");
			if (layoutConfig != null)
			{
			    return layoutConfig.Config_DashboardName;
			}
			else
			{
			    return string.Empty;
			}
		}
		
        private string Get_HdrDB()
        {
            var hdrConfigType = args.NameValuePairs.XFGetValue("HdrConfigType", "NA");
			var configMenuID = args.NameValuePairs.XFGetValue("ConfigMenuID", "-1");
			BRApi.ErrorLog.LogMessage(si,$"Hit Config {configMenuID}");
            var currDB = args.NameValuePairs.XFGetValue("currDB", "NA");
            if (currDB.XFEqualsIgnoreCase("DDM_HdrConfig"))
            {
                int.TryParse(configMenuID, out var configmenuId);

                if (configmenuId == 0)
                {
                    return "DDM_HdrConfig_Blank";
                }

                return "DDM_HdrConfig_AddUpdate";
            }
			else if (currDB.XFEqualsIgnoreCase("DDM_HdrConfig_C2"))
            {
				var isUpdate = hdrConfigType.XFEqualsIgnoreCase("Update");
                int.TryParse(configMenuID, out var configMenuId);
			
                if (isUpdate && configMenuId == 0)
                {
                    return $"{currDB}_Blank";
                }
                return $"{currDB}_AddUpdate";
            }
            return currDB;
        }
		
		
		private string Get_ConfigHdrTypeDB()
		{
			var HdrType = args.NameValuePairs.XFGetValue("HdrType","-1").XFConvertToInt();
			var configHelpers = new DDM_ConfigHelpers();
			var HdrConfig = configHelpers.Get_HdrTypeConfig(HdrType);
			if (HdrConfig != null)
			{
							BRApi.ErrorLog.LogMessage(si,$"Hit Hdr Type {HdrType} - {HdrConfig.DashboardName}");
			    return HdrConfig.DashboardName;
			}
			else
			{
			    return string.Empty;
			}
		}
		
		public string Get_DDM_Config_IsVisible()
		{
			var checkType = args.NameValuePairs.XFGetValue("checkType","NA");

			switch (checkType)
		    {
		        case "paneContent":
				{
					var paneVal = args.NameValuePairs.XFGetValue("dbPaneContentType", "0").XFConvertToInt();
					var txtBoxType = args.NameValuePairs.XFGetValue("txtBoxType","NA");
					var isVisible = (paneVal == (int)DDM_ConfigHelpers.DBPaneContents.CubeView && txtBoxType == "CV") ||
                         			(paneVal == (int)DDM_ConfigHelpers.DBPaneContents.Dashboard && txtBoxType == "DB");
					BRApi.ErrorLog.LogMessage(si,$"Hit {isVisible}");
		            return isVisible.ToString();
				}
		        case "filterCheckbox":
				{
					return string.Empty;
				}

		        case "btnCbxBoundParam":
				{
					return string.Empty;
				}
		        default:
		            // If we don't recognize the type, return empty immediately
		            return string.Empty;
		    }
		}
		
		public string Get_DDM_Config_IsVisibleHdr()
{
    var fltrHdrType = args.NameValuePairs.XFGetValue("HdrType", "NA");

    return fltrHdrType switch
    {
        "1"  => "True",
         "2" or "3" or "4" => "False",
        _          => "True" // Default case
    };
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

        public class DDMColFormatter
        {
            public static readonly Dictionary<string, ColumnConfig[]> ConfigColumns = new Dictionary<string, ColumnConfig[]>
            {
                { "DDM_ConfigWFP", new ColumnConfig[]
                    {
                        new ColumnConfig { ColumnName = "DynDBConfigID", IsVisible = false, AllowUpdates = false},
                        new ColumnConfig { ColumnName = "WFPKey", Description = "[Profile Name]", IsVisible = true, AllowUpdates = false, DefaultValue = "|!IV_DDM_WFPtrv!|", ParameterName = "BL_DDM_WFPNames" },
                        new ColumnConfig { ColumnName = "WFPStepType", Description = "[Profile Step Type]", IsVisible = true, AllowUpdates = false, ParameterName = "DL_DDM_WFPStepType" },
                        new ColumnConfig { ColumnName = "Status", Description = "[Status]", IsVisible = true },
                        new ColumnConfig { ColumnName = "CreateDate", Description = "[Create Date]", IsVisible = true, AllowUpdates = false },
                        new ColumnConfig { ColumnName = "CreateUser", Description = "[Create User]", IsVisible = true, AllowUpdates = false },
                        new ColumnConfig { ColumnName = "UpdateDate", Description = "[Update Date]", IsVisible = true, AllowUpdates = false },
                        new ColumnConfig { ColumnName = "UpdateUser", Description = "[Update User]", IsVisible = true, AllowUpdates = false }
                    }
                },
                { "DDM_ConfigOPDB", new ColumnConfig[]
                    {
                        new ColumnConfig { ColumnName = "Cube_ID", IsVisible = false, AllowUpdates = false, DefaultValue = "|!IV_FMM_Cube_ID!|" },
                        new ColumnConfig { ColumnName = "Act_ID", IsVisible = false, AllowUpdates = false, DefaultValue = "|!IV_FMM_Act_ID!|" },
                        new ColumnConfig { ColumnName = "Model_ID", IsVisible = false, AllowUpdates = false, DefaultValue = "|!IV_FMM_Model_ID!|" },
                        new ColumnConfig { ColumnName = "Sequence", Description = "Seq", IsVisible = true },
                        new ColumnConfig { ColumnName = "Name", Description = "[Calc Name]", IsVisible = true },
                        new ColumnConfig { ColumnName = "Condition", Description = "[Conditional Logic]", IsVisible = true },
                        new ColumnConfig { ColumnName = "Explanation", Description = "[Calc Explanation]", IsVisible = true },
                        new ColumnConfig { ColumnName = "MultiDim_Alloc", Description = "[Multi-Dim Alloc]", IsVisible = true },
                        new ColumnConfig { ColumnName = "MbrList_Calc", Description = "[Mbr List Calc?]", IsVisible = true },
                        new ColumnConfig { ColumnName = "MbrList_1_Dim", Description = "[Mbr List 1 Dim]", IsVisible = true },
                        new ColumnConfig { ColumnName = "MbrList_1_Filter", Description = "[Mbr List 1 Filter]", IsVisible = true },
                        new ColumnConfig { ColumnName = "MbrList_1_DimType", IsVisible = false },
                        new ColumnConfig { ColumnName = "MbrList_1_Filter", IsVisible = false },
                        new ColumnConfig { ColumnName = "MbrList_2_Dim", Description = "[Mbr List 2 Dim]", IsVisible = true },
                        new ColumnConfig { ColumnName = "MbrList_2_Filter", Description = "[Mbr List 2 Filter]", IsVisible = true },
                        new ColumnConfig { ColumnName = "MbrList_2_DimType", IsVisible = false },
                        new ColumnConfig { ColumnName = "MbrList_2_Filter", IsVisible = false },
                        new ColumnConfig { ColumnName = "MbrList_3_Dim", Description = "[Mbr List 3 Dim]", IsVisible = true },
                        new ColumnConfig { ColumnName = "MbrList_3_Filter", Description = "[Mbr List 3 Filter]", IsVisible = true },
                        new ColumnConfig { ColumnName = "MbrList_3_DimType", IsVisible = false },
                        new ColumnConfig { ColumnName = "MbrList_3_Filter", IsVisible = false },
                        new ColumnConfig { ColumnName = "MbrList_4_Dim", Description = "[Mbr List 4 Dim]", IsVisible = true },
                        new ColumnConfig { ColumnName = "MbrList_4_Filter", Description = "[Mbr List 4 Filter]", IsVisible = true },
                        new ColumnConfig { ColumnName = "MbrList_4_DimType", IsVisible = false },
                        new ColumnConfig { ColumnName = "MbrList_4_Filter", IsVisible = false },
                        new ColumnConfig { ColumnName = "BR_Calc", Description = "[BR Calc?]", IsVisible = true },
                        new ColumnConfig { ColumnName = "BR_Calc_Name", Description = "[BR Calc Name]", IsVisible = true },
                        new ColumnConfig { ColumnName = "Status", Description = "[Status]", IsVisible = true },
                        new ColumnConfig { ColumnName = "Create_Date", Description = "[Create Date]", IsVisible = true, AllowUpdates = false },
                        new ColumnConfig { ColumnName = "CreateUser", Description = "[Create User]", IsVisible = true, AllowUpdates = false },
                        new ColumnConfig { ColumnName = "Update_Date", Description = "[Update Date]", IsVisible = true, AllowUpdates = false },
                        new ColumnConfig { ColumnName = "UpdateUser", Description = "[Update User]", IsVisible = true, AllowUpdates = false }
                    }
                }
            };
        }
    }
}