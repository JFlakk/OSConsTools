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
	public class DDM_ConfigHelpers
	{
		#region "Config Setup"
		public class LayoutConfig 
		{
			public string Config_DashboardName { get; init; }
			
		    public string DashboardName { get; init; }
		
		    public Dictionary<int, Dictionary<string, string>> ParameterMappings { get; init; }
		}

		public static class LayoutRegistry
		{
		    public static readonly Dictionary<LayoutType, LayoutConfig> Configs = new()
		    {
		        [LayoutType.Dashboard] = new LayoutConfig 
		        {
		            Config_DashboardName = "DDM_LayoutConfig_DB",
					DashboardName = "DDM_App_Content_DB",
		            ParameterMappings = new() 
		            { 
		                { 0, new Dictionary<string, string> { { "IV_DDM_MenuLayout_SortOrder", "SortOrder" } } },
						{ 1, new Dictionary<string, string> { { "IV_DDM_MenuLayout_Name", "Name" } } },
						{ 2, new Dictionary<string, string> { { "IV_DDM_MenuLayout_DB_Name", "DB_Name" } } },
						{ 3, new Dictionary<string, string> { { "DL_DDM_MenuLayout_Status","Status" } } }
		            }
		        },
		        [LayoutType.CubeView] = new LayoutConfig 
		        {
		            DashboardName = "DDM_LayoutConfig_CV",
		            ParameterMappings = new() 
		            { 
		                { 0, new Dictionary<string, string> { { "IV_DDM_MenuLayout_SortOrder", "SortOrder" } } },
						{ 1, new Dictionary<string, string> { { "IV_DDM_MenuLayout_Name", "Name" } } },
						{ 2, new Dictionary<string, string> { { "IV_DDM_MenuLayout_CV_Name", "CV_Name" } } },
						{ 3, new Dictionary<string, string> { { "DL_DDM_MenuLayout_Status","Status" } } }
		            }
		        },
		        [LayoutType.Dashboard_TopBottom] = new LayoutConfig 
		        {
		            DashboardName = "DDM_LayoutConfig_TB_DB",
		            ParameterMappings = new() 
		            { 
		                { 0, new Dictionary<string, string> { { "IV_DDM_MenuLayout_SortOrder", "SortOrder" } } },
						{ 1, new Dictionary<string, string> { { "IV_DDM_MenuLayout_Name", "Name" } } },
		                { 2, new Dictionary<string, string> { { "IV_DDM_MenuLayout_T_Height", "T_Height" } } },
						{ 3, new Dictionary<string, string> { { "DL_DDM_MenuLayout_T_Content_Type", "T_ContentType" } } },
						{ 4, new Dictionary<string, string> { { "DL_DDM_MenuLayout_T_Name", "T_Name" } } },
						{ 5, new Dictionary<string, string> { { "DL_DDM_MenuLayout_B_ContentType", "B_ContentType" } } },
						{ 6, new Dictionary<string, string> { { "DL_DDM_MenuLayout_B_Name", "B_Name" } } },
						{ 7, new Dictionary<string, string> { { "DL_DDM_MenuLayout_Status","Status" } } }
		            }
		        },
		        [LayoutType.Dashboard_LeftRight] = new LayoutConfig 
		        {
		            DashboardName = "DDM_LayoutConfig_LR_DB",
		            ParameterMappings = new() 
		            { 
		                { 0, new Dictionary<string, string> { { "IV_DDM_MenuLayout_L_Width", "L_Width" } } },
						{ 1, new Dictionary<string, string> { { "DL_DDM_MenuLayout_L_ContentType", "L_ContentType" } } },
						{ 2, new Dictionary<string, string> { { "DL_DDM_MenuLayout_L_Name", "L_Name" } } },
						{ 3, new Dictionary<string, string> { { "DL_DDM_MenuLayout_R_ContentType", "R_ContentType" } } },
						{ 4, new Dictionary<string, string> { { "DL_DDM_MenuLayout_R_Name", "R_Name" } } }
		            }
		        },
		        [LayoutType.Dashboard_2Top1Bottom] = new LayoutConfig 
		        {
		            DashboardName = "DDM_LayoutConfig_2T1B_DB",
		            ParameterMappings = new() 
		            { 
		                { 0, new Dictionary<string, string> { { "IV_DDM_MenuLayout_L_Width", "L_Width" } } },
						{ 1, new Dictionary<string, string> { { "DL_DDM_MenuLayout_TL_ContentType", "TL_ContentType" } } },
						{ 2, new Dictionary<string, string> { { "DL_DDM_MenuLayout_TL_Name", "TL_Name" } } },
						{ 3, new Dictionary<string, string> { { "DL_DDM_MenuLayout_CV_Name_Left", "CV_Name_Left" } } },
						{ 4, new Dictionary<string, string> { { "DL_DDM_MenuLayout_Right_Content_Type", "Right_Option_Type" } } },
						{ 5, new Dictionary<string, string> { { "DL_DDM_MenuLayout_DB_Name_Right", "DB_Name_Right" } } },
						{ 6, new Dictionary<string, string> { { "DL_DDM_MenuLayout_CV_Name_Right", "CV_Name_Right" } } }
		            }
		        },
		        [LayoutType.Dashboard_1Top2Bottom] = new LayoutConfig 
		        {
		            DashboardName = "DDM_LayoutConfig_1T2B_DB",
		            ParameterMappings = new() 
		            { 
		                { 0, new Dictionary<string, string> { { "IV_DDM_MenuLayout_Left_Width", "L_Width" } } },
						{ 1, new Dictionary<string, string> { { "DL_DDM_MenuLayout_Left_Content_Type", "L_ContentType" } } },
						{ 2, new Dictionary<string, string> { { "DL_DDM_MenuLayout_DB_Name_Left", "DB_Name_Left" } } },
						{ 3, new Dictionary<string, string> { { "DL_DDM_MenuLayout_CV_Name_Left", "CV_Name_Left" } } },
						{ 4, new Dictionary<string, string> { { "DL_DDM_MenuLayout_Right_Content_Type", "R_ContentType" } } },
						{ 5, new Dictionary<string, string> { { "DL_DDM_MenuLayout_DB_Name_Right", "DB_Name_Right" } } },
						{ 6, new Dictionary<string, string> { { "DL_DDM_MenuLayout_CV_Name_Right", "CV_Name_Right" } } }
		            }
		        },
		        [LayoutType.Dashboard_2Left1Right] = new LayoutConfig 
		        {
		            DashboardName = "DDM_LayoutConfig_2L1R_DB",
		            ParameterMappings = new() 
		            { 
		                { 0, new Dictionary<string, string> { { "IV_DDM_MenuLayout_Left_Width", "L_Width" } } },
						{ 1, new Dictionary<string, string> { { "DL_DDM_MenuLayout_Left_Content_Type", "L_ContentType" } } },
						{ 2, new Dictionary<string, string> { { "DL_DDM_MenuLayout_DB_Name_Left", "DB_Name_Left" } } },
						{ 3, new Dictionary<string, string> { { "DL_DDM_MenuLayout_CV_Name_Left", "CV_Name_Left" } } },
						{ 4, new Dictionary<string, string> { { "DL_DDM_MenuLayout_Right_Content_Type", "R_ContentType" } } },
						{ 5, new Dictionary<string, string> { { "DL_DDM_MenuLayout_DB_Name_Right", "DB_Name_Right" } } },
						{ 6, new Dictionary<string, string> { { "DL_DDM_MenuLayout_CV_Name_Right", "CV_Name_Right" } } }
		            }
		        },
		        [LayoutType.Dashboard_1Left2Right] = new LayoutConfig 
		        {
		            DashboardName = "DDM_LayoutConfig_1L2R_DB",
		            ParameterMappings = new() 
		            { 
		                { 0, new Dictionary<string, string> { { "IV_DDM_MenuLayout_Left_Width", "L_Width" } } },
						{ 1, new Dictionary<string, string> { { "DL_DDM_MenuLayout_Left_Content_Type", "L_ContentType" } } },
						{ 2, new Dictionary<string, string> { { "DL_DDM_MenuLayout_DB_Name_Left", "DB_Name_Left" } } },
						{ 3, new Dictionary<string, string> { { "DL_DDM_MenuLayout_CV_Name_Left", "CV_Name_Left" } } },
						{ 4, new Dictionary<string, string> { { "DL_DDM_MenuLayout_Right_Content_Type", "R_ContentType" } } },
						{ 5, new Dictionary<string, string> { { "DL_DDM_MenuLayout_DB_Name_Right", "DB_Name_Right" } } },
						{ 6, new Dictionary<string, string> { { "DL_DDM_MenuLayout_CV_Name_Right", "CV_Name_Right" } } }
		            }
		        },
		        [LayoutType.Dashboard_2x2] = new LayoutConfig 
		        {
		            DashboardName = "DDM_LayoutConfig_2x2_DB",
		            ParameterMappings = new() 
		            { 
		                { 0, new Dictionary<string, string> { { "IV_DDM_MenuLayout_Left_Width", "L_Width" } } },
						{ 1, new Dictionary<string, string> { { "DL_DDM_MenuLayout_Left_Content_Type", "L_ContentType" } } },
						{ 2, new Dictionary<string, string> { { "DL_DDM_MenuLayout_DB_Name_Left", "DB_Name_Left" } } },
						{ 3, new Dictionary<string, string> { { "DL_DDM_MenuLayout_CV_Name_Left", "CV_Name_Left" } } },
						{ 4, new Dictionary<string, string> { { "DL_DDM_MenuLayout_Right_Content_Type", "R_ContentType" } } },
						{ 5, new Dictionary<string, string> { { "DL_DDM_MenuLayout_DB_Name_Right", "DB_Name_Right" } } },
						{ 6, new Dictionary<string, string> { { "DL_DDM_MenuLayout_CV_Name_Right", "CV_Name_Right" } } }
		            }
		        },
		        [LayoutType.Dashboard_CustomDB] = new LayoutConfig 
		        {
		            DashboardName = "DDM_LayoutConfig_CustomDB",
		            ParameterMappings = new() 
		            { 
		                { 0, new Dictionary<string, string> { { "IV_DDM_MenuLayout_Left_Width", "L_Width" } } },
						{ 1, new Dictionary<string, string> { { "DL_DDM_MenuLayout_Left_Content_Type", "L_ContentType" } } },
						{ 2, new Dictionary<string, string> { { "DL_DDM_MenuLayout_DB_Name_Left", "DB_Name_Left" } } },
						{ 3, new Dictionary<string, string> { { "DL_DDM_MenuLayout_CV_Name_Left", "CV_Name_Left" } } },
						{ 4, new Dictionary<string, string> { { "DL_DDM_MenuLayout_Right_Content_Type", "R_ContentType" } } },
						{ 5, new Dictionary<string, string> { { "DL_DDM_MenuLayout_DB_Name_Right", "DB_Name_Right" } } },
						{ 6, new Dictionary<string, string> { { "DL_DDM_MenuLayout_CV_Name_Right", "CV_Name_Right" } } }
		            }
		        },
		    };
		}
		
		public class HdrConfig 
		{
		    public string DashboardName { get; init; }
		
		    public Dictionary<int, Dictionary<string, string>> ParameterMappings { get; init; }
		}

		public static class HdrRegistry
		{
		    public static readonly Dictionary<HdrType, HdrConfig> Configs = new()
		    {
		        [HdrType.Filter] = new HdrConfig
		        {
		            DashboardName = "DDM_HdrConfig_Fltr",
		            ParameterMappings = new() 
		            { 
		                { 0, new Dictionary<string, string> { { "IV_DDM_Hdr_Name", "Name" } } },
						{ 1, new Dictionary<string, string> { { "DL_DDM_Hdr_Fltr_Type", "Fltr_Type" } } },
						{ 2, new Dictionary<string, string> { { "DL_DDM_Hdr_Fltr_DimType", "Fltr_DimType" } } },
						{ 3, new Dictionary<string, string> { { "BL_DDM_Hdr_Fltr_DimName", "Fltr_DimName" } } },
						{ 4, new Dictionary<string, string> { { "IV_DDM_Hdr_DependencyTier", "Fltr_DependencyTier" } } },
					//	{ 5, new Dictionary<string, string> { { "IV_DDM_Hdr_Fltr_Btn_Lbl", "Fltr_MFB" } } },
					//	{ 6, new Dictionary<string, string> { { "IV_DDM_Hdr_Fltr_Btn_Lbl", "Fltr_Default" } } },
						{ 7, new Dictionary<string, string> { { "IV_DDM_Hdr_Fltr_Btn", "Fltr_Btn" } } },
						{ 8, new Dictionary<string, string> { { "IV_DDM_Hdr_Fltr_Btn_Lbl", "Fltr_Btn_Lbl" } } },
						{ 9, new Dictionary<string, string> { { "IV_DDM_Hdr_Fltr_Btn_ToolTip", "Fltr_Btn_ToolTip" } } }
						
						
		            }
		        },
			
		        [HdrType.Button] = new HdrConfig 
		        {
		            DashboardName = "DDM_HdrConfig_Btn",
		            ParameterMappings = new() 
		            { 
		                { 0, new Dictionary<string, string> { { "IV_DDM_Hdr_Name", "Name" } } },
						{ 1, new Dictionary<string, string> { { "DL_DDM_Hdr_Btn_Type", "Btn_Type" } } },
						{ 2, new Dictionary<string, string> { { "IV_DDM_Hdr_Btn_Lbl", "Btn_Lbl" } } },
						{ 3, new Dictionary<string, string> { { "IV_DDM_Hdr_Btn_ToolTip", "Btn_ToolTip" } } }
						
		            }
		        }
		    };
		}
		#endregion
		
        public object Test(SessionInfo si)
        {
            try
            {
                return null;
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }
		
		public LayoutConfig Get_LayoutConfig(int layoutTypeintValue)
		{
			var layoutType = (LayoutType)layoutTypeintValue;

            if (LayoutRegistry.Configs.TryGetValue(layoutType, out var config))
            {
				return config;
			}
			return null;
		}
		
		public HdrConfig Get_HdrTypeConfig(int optionintValue)
		{
			var HdrType = (HdrType)optionintValue;
			
			if (HdrRegistry.Configs.TryGetValue(HdrType, out var config))
            {
				return config;
			}
			return null;
		}
		
		public enum DDMType {
			None = 0,
		    WFProfile = 1,
		    StandAlone = 2
		}
		
		public enum LayoutType {
			None = 0,
		    Dashboard = 1,
		    CubeView = 2,
		    Dashboard_TopBottom = 3,
		    Dashboard_LeftRight = 4,
			Dashboard_2Top1Bottom = 5,
			Dashboard_1Top2Bottom = 6,
		    Dashboard_2Left1Right = 7,
		    Dashboard_1Left2Right = 8,
		    Dashboard_2x2 = 9,
			Dashboard_CustomDB = 10
		}
		
		public enum HdrType {
			None = 0,
		    Filter = 1,
		    Button = 2
		}
		
		public enum HdrBtnType {
		    Standard = 1,
		    FileExplorer = 2,
			FileUpload = 3, 
			Workflow = 4
		}
		public enum DBPaneContents {
			None = 0,
		    Dashboard = 1,
		    CubeView = 2
		}
		
		public enum HdrFilterType {
			None = 0,
		    Dim = 1,
		    SQL_BoundList = 2,
			BR_BoundList = 3,
			Delimited_List = 4,
			Input_Value = 5, 
			Literal_Value = 6
		}

		public enum HdrBtn_ActionPOVType {
			None = 0,
		    NoAction = 1,
		    ChangePOV = 2,
			ChangeWF = 3,
			ChangePOVandWF = 4
		}
	
		public enum HdrBtn_ActionSaveType {
			None = 0,
		    NoAction = 1,
		    SaveDataComps = 2,
			PromptSaveDataComps = 3,
			SaveDataAllComps = 4,
			PromptSaveDataAllComps = 5,
			SaveDataCompsSaveFiles = 6,
			PromptSaveDataCompsSaveFiles = 7,
			SaveDataAllCompsSaveFiles = 8,
			PromptSaveDataAllCompsSaveFiles = 9,
			SaveAllFiles = 10
		}
		
		public enum HdrBtn_ActionServerTaskType {
			None = 0,
			NoTask = 1,
			ExeDBExtBR_GenServer = 2,
			ExeDBExtBR_StgServer = 3,
			ExeDBExtBR_ConsServer = 4,
			ExeDBExtBR_DMServer = 5,
			ExeFinCustCalcBR = 6,
			ExeDMSeq = 7,
			Calc = 8,
			ForceCalc = 9,
			CalcWLogging = 10,
			ForceCalcWLogging = 11,
			Trans = 12,
			ForceTrans = 13,
			TransWLogging = 14,
			ForceTransWLogging = 15,
			Cons = 16,
			ForceCons = 17,
			ConsWLogging = 18,
			ForceConsWLogging = 19
		}
	
		public enum HdrBtn_ActionUIChangedType {
			None = 0,
			NoAction = 1,
			Redraw = 2,
			Refresh = 3,
			CloseDialog = 4,
			CloseDialogOK = 5,
			CloseDialogCancel = 6,
			CloseAllDialogs = 7,
			OpenDialog = 8,
			OpenDialogRedraw = 9,
			OpenDialogRefresh = 10,
			OpenDialogNoBtns = 11,
			OpenDialogNoBtnsApplyChgsRedraw = 12,
			OpenDialogNoBtnsApplyChgsRefresh = 13,
			OpenDialogNoBtnsRedraw = 14,
			OpenDialogNoBtnsRefresh = 15,
			OpenDialogApplyChgsRedraw = 16,
			OpenDialogApplyChgsRefresh = 17,
			OpenDialogApplyChgsRedrawOk = 18,
			OpenDialogApplyChgsRefreshOk = 19
		}
		
		public enum HdrBtn_ActionNavType {
			None = 0,
		    NoAction = 1,
		    OpenFile = 2,
			OpenPage = 3,
			OpenWebSite = 4
		}
		public enum HdrDimType {
			Entity = 0,
		    Time = 1,
		    Scenario = 2,
			Account = 5, 
			Flow = 6,
			UD1 = 9,
			UD2 = 10,
			UD3 = 11,
			UD4 = 12,
			UD5 = 13,
			UD6 = 14,
			UD7 = 15,
			UD8 = 16,
		}
	}
}
