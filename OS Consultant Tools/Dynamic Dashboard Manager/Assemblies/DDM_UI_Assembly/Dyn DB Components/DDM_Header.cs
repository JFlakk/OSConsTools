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
using Microsoft.Data.SqlClient;
using Workspace.OSConsTools.DDM_ConfigUI_Assembly;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    public class DDM_Header
    {

        //Template Parameters

        // header temp params
        private const string template_MbrList_cbxbtn_BoundParam = "MbrList_cbxbtn_BoundParam";
		private const string template_MbrList_Default = "MbrList_Default";
		private const string template_MbrList_Cube = "MbrList_Cube";
		private const string template_MbrList_Dim = "MbrList_Dim";
		private const string template_MbrList_Filter = "MbrList_Filter";
		private const string template_MbrList_btn_Visible = "btn_Visible";
		private const string template_MbrList_btn_Text = "btn_Text";
			//"IV_DDM_Hdr_Fltr_Btn_Lbl";
			// "IV_DDM_Hdr_Fltr_Btn_Lbl";
		//	"IV_DDM_App_Generic_DBExt_1_Text";
			
			//"btn_Text";
		private const string template_MbrList_btn_ToolTip = "btn_ToolTip";
			//"btn_ToolTip";
		private const string template_MbrList_cbx_Visible = "cbx_Visible";
		private const string template_MbrList_cbx_Text = "cbx_Text";
		private const string template_MbrList_cbx_ToolTip = "cbx_ToolTip";
		private const string template_MbrList_txt_Visible = "txt_Visible";
		private const string template_MbrList_txt_Text = "txt_Text";
		private const string template_MbrList_txt_ToolTip = "txt_ToolTip";
		private const string template_MbrList_txt_BoundParam = "MbrList_txt_BoundParam";
        private const string TmpParam_HeaderItemAction = "HeaderItemAction";
        private const string TmpParam_HeaderItemIcon = "HeaderItemIcon";

        // Regular Parameter

        // header regular params
        private const string Param_HeaderAction = "IV_DDM_SelectedHeaderAction";
        private const string Param_HeaderTest = "IV_DDM_HDR_Comp";

        public static Dictionary<string, DashboardComponentType> dashboardTypeResolver = new Dictionary<string, DashboardComponentType>() {
            {"Btn", DashboardComponentType.Button},
            {"Cbx", DashboardComponentType.ComboBox},
            {"Txt", DashboardComponentType.TextBox}
        };

        public static Dictionary<string, XFSelectionChangedTaskType> serverTaskTypeResolver = new Dictionary<string, XFSelectionChangedTaskType>() {
            {"General", XFSelectionChangedTaskType.ExecuteDashboardExtenderBusinessRule},
            {"Stage", XFSelectionChangedTaskType.ExecuteDashboardExtenderBRStageServer},
            {"Data Management Server", XFSelectionChangedTaskType.ExecuteDashboardExtenderBRDataMgmtServer},
            {"Finance Custom Calc BR", XFSelectionChangedTaskType.ExecuteFinanceCustomCalculateBR},
            {"Data Management Sequence", XFSelectionChangedTaskType.ExecuteDataManagementSequence},
            {"Calculate", XFSelectionChangedTaskType.Calculate},
            {"Force Calculate", XFSelectionChangedTaskType.ForceCalculate},
            {"Calculate w/ Logging", XFSelectionChangedTaskType.CalculateWithLogging},
            {"Force Calculate w/ Logging", XFSelectionChangedTaskType.ForceCalculateWithLogging},
            {"Translate", XFSelectionChangedTaskType.Translate},
            {"Force Translate", XFSelectionChangedTaskType.ForceTranslate},
            {"Translate w/ Logging", XFSelectionChangedTaskType.TranslateWithLogging},
            {"Force Translate w/ Logging", XFSelectionChangedTaskType.ForceTranslateWithLogging},
            {"Consolidate", XFSelectionChangedTaskType.Consolidate},
            {"Force Consolidate", XFSelectionChangedTaskType.ForceConsolidate},
            {"Consolidate w/ Logging", XFSelectionChangedTaskType.ConsolidateWithLogging},
            {"Force Consolidate w/ Logging", XFSelectionChangedTaskType.ForceConsolidateWithLogging},

        };

        public object Main(SessionInfo si)
        {
            try
            {
                return null;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }


        // Technically, this will update all IVs with associated MLs
        internal static XFSelectionChangedTaskResult OnTextEntered(SessionInfo si, DashboardExtenderArgs args)
        {
            var taskResult = new XFSelectionChangedTaskResult() { ChangeCustomSubstVarsInDashboard = true };

            //update all text box IVs to their respective MLs
            Dictionary<string, string> IVs = args.SelectionChangedTaskInfo.CustomSubstVars.Where(x => x.Key.Contains("IV") && x.Key.Contains("Selection")).ToDictionary<string, string>();
            foreach (string IV in IVs.Keys)
            {
                taskResult.ModifiedCustomSubstVars.Add(IV.Replace("IV", "ML"), IVs[IV]);
            }

            return taskResult;
        }

        // menu label
        internal static WsDynamicDashboardEx get_DynamicHdr(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace, DashboardMaintUnit maintUnit,
            WsDynamicComponentEx parentDynamicComponentEx, Dashboard storedDashboard, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
			var repeatArg_List = new List<WsDynamicComponentRepeatArgs>();
			var dt = DDM_Support.get_HeaderItems(si, customSubstVarsAlreadyResolved,1);
			// BRApi.ErrorLog.LogMessage(si,$"Hit Here get hdr : {dt.Rows.Count} ");
			foreach (DataRow row in dt.Rows)
            {
				var templateSubstVars = new Dictionary<string, string>();
				// Fltr_DimType is stored as an int in DDM_DynDBHdrConfig; resolve to its enum name
				var dimTypeValue = Convert.ToInt32(row["Fltr_DimType"]);
				var dimType = Enum.GetName(typeof(DDM_ConfigHelpers.HdrDimType), dimTypeValue);
				BRApi.ErrorLog.LogMessage(si,$"Hit Here dyn hdr 120 : {dimType} ");
                if (!templateSubstVars.ContainsKey(template_MbrList_cbxbtn_BoundParam))
                {
                    templateSubstVars.Add(template_MbrList_cbxbtn_BoundParam, $"ML_DDM_App_{dimType}_Mbr_List");
                }
                if (!templateSubstVars.ContainsKey(template_MbrList_Default))
                {
                    templateSubstVars.Add(template_MbrList_Default, row["Fltr_Default"].ToString());
                }
                if (!templateSubstVars.ContainsKey(template_MbrList_Cube))
                {
                    templateSubstVars.Add(template_MbrList_Cube, "Army");
                }
                if (!templateSubstVars.ContainsKey(template_MbrList_Dim))
                {
//BRApi.ErrorLog.LogMessage(si,$"Hit Here dyn hdr 135 : {dimType} ");
                    templateSubstVars.Add(template_MbrList_Dim, row["Fltr_DimName"].ToString());
                }
                if (!templateSubstVars.ContainsKey(template_MbrList_Filter))
                {
//BRApi.ErrorLog.LogMessage(si,$"Hit Here dyn hdr 140 : {dimType} ");
                    templateSubstVars.Add(template_MbrList_Filter, row["Fltr_MFB"].ToString());
                }
                if (!templateSubstVars.ContainsKey(template_MbrList_btn_Visible))
                {
// BRApi.ErrorLog.LogMessage(si,$"Hit Here dyn hdr 145 : {dimType} ");
                  templateSubstVars.Add(template_MbrList_btn_Visible, Convert.ToBoolean(row["Fltr_Btn"]) ? "True" : "False");
					
                }		
                if (!templateSubstVars.ContainsKey(template_MbrList_btn_Text))
                {
                    templateSubstVars.Add(template_MbrList_btn_Text, row["Fltr_BtnLbl"].ToString());
                }	
                if (!templateSubstVars.ContainsKey(template_MbrList_btn_ToolTip))
                {
					BRApi.ErrorLog.LogMessage(si,$"Hit Here dyn hdr ToolTip");
                    templateSubstVars.Add(template_MbrList_btn_ToolTip, row["Fltr_BtnToolTip"].ToString());
                }	
                if (!templateSubstVars.ContainsKey(template_MbrList_cbx_Visible))
                {
                 // templateSubstVars.Add(template_MbrList_cbx_Visible, Convert.ToBoolean(row["Fltr_Cbx"]) ? "True" : "False");
                }
                if (!templateSubstVars.ContainsKey(template_MbrList_cbx_Text))
                {
// BRApi.ErrorLog.LogMessage(si,$"Hit Here dyn hdr 159 : {dimType} ");
                    templateSubstVars.Add(template_MbrList_cbx_Text, row["Fltr_CbxLbl"].ToString());
                }	
                if (!templateSubstVars.ContainsKey(template_MbrList_cbx_ToolTip))
                {
                    templateSubstVars.Add(template_MbrList_cbx_ToolTip, row["Fltr_CbxToolTip"].ToString());
                }	
                if (!templateSubstVars.ContainsKey(template_MbrList_txt_Visible))
                {
                 // templateSubstVars.Add(template_MbrList_txt_Visible, Convert.ToBoolean(row["Fltr_Txt"]) ? "True" : "False");
                }
                if (!templateSubstVars.ContainsKey(template_MbrList_txt_Text))
                {
                    templateSubstVars.Add(template_MbrList_txt_Text, row["Fltr_TxtLbl"].ToString());
                }
                if (!templateSubstVars.ContainsKey(template_MbrList_txt_ToolTip))
                {
                    templateSubstVars.Add(template_MbrList_txt_ToolTip, row["Fltr_TxtToolTip"].ToString());
                }			
                if (!templateSubstVars.ContainsKey(template_MbrList_txt_BoundParam))
                {
 // BRApi.ErrorLog.LogMessage(si,$"Hit Here dyn hdr 180 : {dimType} ");
                    templateSubstVars.Add(template_MbrList_txt_BoundParam, $"ML_DDM_App_{dimType}MbrList");
                }
				repeatArg_List.Add(new WsDynamicComponentRepeatArgs(dimType,templateSubstVars));
 BRApi.ErrorLog.LogMessage(si,$"Hit Dyn hdr 181: {dimType} - {row["Fltr_Btn"].ToString()} - {row["Fltr_BtnLbl"].ToString()} - {row["Fltr_BtnToolTip"].ToString()}");
			}

        	var dynamicDashboardEx = api.GetEmbeddedDynamicDashboard(si, workspace, parentDynamicComponentEx, storedDashboard, string.Empty,null, TriStateBool.TrueValue, WsDynamicItemStateType.EntireObject);

        	dynamicDashboardEx.DynamicDashboard.Tag = repeatArg_List;

        	api.SaveDynamicDashboardState(si, parentDynamicComponentEx.DynamicComponent, dynamicDashboardEx, WsDynamicItemStateType.MinimalWithTemplateParameters);

            return dynamicDashboardEx;
        }
		
        internal static WsDynamicComponentCollection get_DynamicHdrRepeatedComponents(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace,
            DashboardMaintUnit maintUnit, WsDynamicDashboardEx dynamicDashboardEx, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {	
			var repeatArg_List = dynamicDashboardEx.DynamicDashboard.Tag as List<WsDynamicComponentRepeatArgs>;
			var dynCompRepeated_dynDashboard = new WsDynamicComponentCollection();
			dynCompRepeated_dynDashboard = api.GetDynamicComponentsRepeatedForDynamicDashboard(si,workspace,dynamicDashboardEx,repeatArg_List,TriStateBool.TrueValue,WsDynamicItemStateType.EntireObject);
            // Loop through the repeated components to find Dashboards
            foreach (var comp in dynCompRepeated_dynDashboard.Components)
            {
                if (comp.DynamicComponentEx.DynamicComponent != null && comp.DynamicComponentEx.DynamicComponent.Component != null)
                {
                    var componentType = comp.DynamicComponentEx.DynamicComponent.Component.DashboardComponentType.ToString();
					var dashboardName = comp.DynamicComponentEx.DynamicComponent.Component.Name;
					
					BRApi.ErrorLog.LogMessage(si, $"Hit {componentType}");
					
					var DyynDashBoard = new WsDynamicDashboardEx();
					var DB = new WsDynamicDashboard(dynCompRepeated_dynDashboard.ParentDashboard.DynamicDashboard);
					//DB.Name = dashboardName;
					BRApi.ErrorLog.LogMessage(si, $"Hit cnt {comp.DynamicComponentEx.DynamicComponent.Component.EmbeddedDashboardName} - {comp.DynamicComponentEx.DynamicComponent.Component.TemplateParameterValues}");
                    if (DB.ComponentTemplateRepeatItems != null)
                    {
                        foreach (var storedCompTmplateRpt in DB.ComponentTemplateRepeatItems)
                        {
							BRApi.ErrorLog.LogMessage(si, $"Hit Template {storedCompTmplateRpt}");
                            if (storedCompTmplateRpt.TemplateParameterValues != null)
                            {
								 BRApi.ErrorLog.LogMessage(si, $"Hit Template Params {storedCompTmplateRpt.TemplateParameterValues.ToString()}");
                                 foreach (var paramValues in storedCompTmplateRpt.TemplateParameterValues)
                                 {
                                     BRApi.ErrorLog.LogMessage(si, $"TemplateParameter: {paramValues} =");
                                 }
                            }
                            else
                            {
                                BRApi.ErrorLog.LogMessage(si, "TemplateParameterValues is null.");
                            }
                        }
                    }
                    else
                    {
                        BRApi.ErrorLog.LogMessage(si, "ComponentTemplateRepeatItems is null.");
                    }
                    if (!string.IsNullOrEmpty(componentType))
                    {
//						api.GetStoredComponentsForDynamicDashboard
                        var tempComp_List = api.GetStoredComponentsForDynamicDashboard(si, workspace, dynamicDashboardEx.DynamicDashboard) as List<DashboardDbrdCompMemberEx>;

                        BRApi.ErrorLog.LogMessage(si, $"Found Dashboard Component: {componentType} - {tempComp_List.Count}");

                        foreach (var storedComp in tempComp_List)
                        {
                            var compName = storedComp.Component?.Name ?? "null";
                            var compType = storedComp.Component?.DashboardComponentType.ToString() ?? "null";
                            BRApi.ErrorLog.LogMessage(si, $"Stored Component - Name: {compName}, Type: {compType}");
                        }
                        // You can add your logic here to process the dashboard as needed
                    }
                }
            }
			return dynCompRepeated_dynDashboard;
		}

        internal static WsDynamicComponentCollection get_DynamicHdrComponents(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace,
            DashboardMaintUnit maintUnit, WsDynamicDashboardEx dynamicDashboardEx, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            var componentCollection = api.GetDynamicComponentsForDynamicDashboard(si, workspace, dynamicDashboardEx, String.Empty, null, TriStateBool.TrueValue, WsDynamicItemStateType.EntireObject);
		//	BRApi.ErrorLog.LogMessage(si,$"JF Dyn DB here {dynamicDashboardEx.DynamicDashboard.Name}");
            // add header items — fetch filter and button rows separately, then combine
            var filterDt = DDM_Support.get_HeaderItems(si, customSubstVarsAlreadyResolved, (int)DDM_ConfigHelpers.HdrType.Filter);
            var buttonDt = DDM_Support.get_HeaderItems(si, customSubstVarsAlreadyResolved, (int)DDM_ConfigHelpers.HdrType.Button);
            var tempColl = addHeaderItems(filterDt, buttonDt, si, workspace, api, dynamicDashboardEx, maintUnit);
// BRApi.ErrorLog.LogMessage(si, $"Stored Component Rows: {dt.Rows.Count}");
            foreach (var item in tempColl.Components)
			{
			BRApi.ErrorLog.LogMessage(si, $"Item: {item.DynamicComponentEx.DynamicComponent.BasedOnName.ToString()}");
				BRApi.ErrorLog.LogMessage(si, $"Item: {item.DynamicComponentEx.DynamicComponent.IsDynamic.ToString()}");
				BRApi.ErrorLog.LogMessage(si, $"Item: {item.DynamicComponentEx.DynamicComponent.Component.BoundParameterName.ToString()}");
				BRApi.ErrorLog.LogMessage(si, $"Item: {item.DynamicComponentEx.DynamicComponent.Component.Name.ToString()}");
                componentCollection.Components.Add(item);
            }

            // TODO: update header to be a grid with items spaced evenly horizontally
return componentCollection;
        }

        #region "Dynamic DB Helper Functions"
        private static XElement buildButtonXML(string btnType)
        {
            var tempXML = new XElement("XFButtonDefinition");
            tempXML.Add(new XElement("ImageFileSourceType"));
            tempXML.Add(new XElement("ImageUrlOrFullFileName"));
            tempXML.Add(new XElement("PageNumber"));
            tempXML.Add(new XElement("ExcelSheet"));
            tempXML.Add(new XElement("ExcelNamedRange"));
            if (btnType == "Filter")
            {
                tempXML.SetAttributeValue("ButtonType", "SelectMemberDialog");
                tempXML.Add(new XElement("SelectMemberInfo"));
                tempXML.Element("SelectMemberInfo").Add(new XElement("DimTypeName"));
                tempXML.Element("SelectMemberInfo").Add(new XElement("UseAllDimsForDimType", false));
                tempXML.Element("SelectMemberInfo").Add(new XElement("DimName"));
                tempXML.Element("SelectMemberInfo").Add(new XElement("CubeName"));
                tempXML.Element("SelectMemberInfo").Add(new XElement("MemberFilter"));
            }

            return tempXML;
        }

        // --- Null-safe column read helpers (added for new-schema refactor) ---
        // Returns the column value as a trimmed string, or string.Empty when the
        // column is missing or DBNull. Prevents the "column does not belong to table"
        // and NullReference failures the legacy direct row[...] reads were prone to.
        private static string GetStr(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName))
            {
                return string.Empty;
            }
            var val = row[columnName];
            return val == DBNull.Value ? string.Empty : val.ToString();
        }

        // Resolves an int-coded column to its enum name for the given enum type.
        // Returns string.Empty when the column is missing, DBNull, non-numeric, or
        // out of range for the enum.
        private static string GetEnumName(DataRow row, string columnName, Type enumType)
        {
            if (row == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
            {
                return string.Empty;
            }
            int value;
            if (!int.TryParse(row[columnName].ToString(), out value))
            {
                return string.Empty;
            }
            var name = Enum.GetName(enumType, value);
            return name ?? string.Empty;
        }
        
        // Orchestrates filter and button header items into a single component collection.
        private static WsDynamicComponentCollection addHeaderItems(
            DataTable filterItems, DataTable buttonItems,
            SessionInfo si, DashboardWorkspace ws, IWsasDynamicDashboardsApiV800 api,
            WsDynamicDashboardEx dynamicDashboardEx, DashboardMaintUnit maintUnit)
        {
            var wsDynCompMembers = new List<WsDynamicDbrdCompMemberEx>();
            wsDynCompMembers.AddRange(addFilterItems(filterItems, si, ws, api, dynamicDashboardEx, maintUnit));
            wsDynCompMembers.AddRange(addButtonItems(buttonItems, si, ws, api, dynamicDashboardEx, maintUnit));
            return new WsDynamicComponentCollection(dynamicDashboardEx, wsDynCompMembers);
        }

        // Builds dynamic components for each Filter-type header row.
        // Handles Btn / Cbx / Txt sub-components, line separators between rows,
        // and appends a text-entry refresh button when any row contains a TextBox.
        //
        // Expected DDM_DynDBHdrConfig columns for Filter rows (HdrType = 1):
        //   Fltr_DimType        int   → DDM_ConfigHelpers.HdrDimType enum (Entity=0, Time=1, Scenario=2, ...)
        //   Fltr_DimName        nvarchar  dimension name used for member-select dialog
        //   Fltr_Default        nvarchar  default selected member
        //   Fltr_MFB            nvarchar  member filter base expression
        //   Fltr_Btn            bit   show the member-select button (1=yes)
        //   Fltr_BtnLbl         nvarchar  label text for the button
        //   Fltr_BtnToolTip     nvarchar  tooltip for the button
        //   Fltr_Cbx            bit   show the combo-box (1=yes)
        //   Fltr_CbxLbl         nvarchar  label text for the combo-box
        //   Fltr_CbxToolTip     nvarchar  tooltip for the combo-box
        //   Fltr_Txt            bit   show the text-box (1=yes)
        //   Fltr_TxtLbl         nvarchar  label text for the text-box
        //   Fltr_TxtToolTip     nvarchar  tooltip for the text-box
        //   Fltr_BtnCbxBoundParam nvarchar  ML parameter name bound to Btn/Cbx (empty → use Txt param)
        //   Fltr_TxtBoundParam  nvarchar  IV parameter name bound to Txt
        private static List<WsDynamicDbrdCompMemberEx> addFilterItems(
            DataTable filterItems, SessionInfo si, DashboardWorkspace ws,
            IWsasDynamicDashboardsApiV800 api, WsDynamicDashboardEx dynamicDashboardEx,
            DashboardMaintUnit maintUnit)
        {
            var wsDynCompMembers = new List<WsDynamicDbrdCompMemberEx>();
            var tempCompMember = new WsDynamicDbrdCompMember();
            bool containsTxtBox = false;
            int iteration = 1;
            int rowCount = filterItems.Rows.Count;

            foreach (DataRow row in filterItems.Rows)
            {
                var dimTypeKeyValue = Convert.ToInt32(row["Fltr_DimType"]);
                var dimType = Enum.GetName(typeof(DDM_ConfigHelpers.HdrDimType), dimTypeKeyValue);

                var stored_param = new DashboardParamDisplayInfo();
                var new_param = new WsDynamicParameter();
                if (row["Fltr_BtnCbxBoundParam"].ToString() != string.Empty)
                {
                    stored_param = BRApi.Dashboards.Parameters.GetParameterDisplayInfo(si, false, null, ws.WorkspaceID, $"{ws.NamespacePrefix}.ML_DDM_App_MbrList{dimType}");
                    new_param = new WsDynamicParameter(true, stored_param.Parameter, stored_param.Parameter.UniqueID, stored_param.Parameter.Name, ws.Name);
                    new_param.Parameter = new DashboardParameter();
                    new_param.Parameter.UniqueID = Guid.NewGuid();
                    new_param.Parameter.Name = $"{stored_param.Parameter.Name}";
                    new_param.Parameter.ParameterType = DashboardParamType.MemberList;
                    new_param.Parameter.DimTypeName = dimType;
                    new_param.Parameter.CubeName = "Army";
                    new_param.Parameter.MemberFilter = row["Fltr_MFB"].ToString();
                    new_param.Parameter.DimName = row["Fltr_DimName"].ToString();
                }
                else
                {
                    new_param = new WsDynamicParameter(true, stored_param.Parameter, Guid.NewGuid(), row["Fltr_TxtBoundParam"].ToString(), ws.Name);
                    new_param.Parameter = new DashboardParameter();
                    new_param.Parameter.Name = row["Fltr_TxtBoundParam"].ToString();
                    new_param.Parameter.ParameterType = DashboardParamType.InputValue;
                }

                BRApi.ErrorLog.LogMessage(si, $"Hit Hdr filter items :{filterItems.Rows.Count}");

                var templateSubstVars = new Dictionary<string, string>();

                foreach (string colSuffix in dashboardTypeResolver.Keys)
                {
                    BRApi.ErrorLog.LogMessage(si, $"Hit 2 {colSuffix}");

                    string colName = "Fltr_" + colSuffix;
                    bool isEnabled = false;

                    if (filterItems.Columns.Contains(colName) && row[colName] != DBNull.Value)
                    {
                        isEnabled = Convert.ToBoolean(row[colName]);
                    }

                    if (!isEnabled) continue;

                    BRApi.ErrorLog.LogMessage(si, "Hit 3");

                    var compDefinition = new XElement("XFCompDefinition");

                    if (!templateSubstVars.ContainsKey(template_MbrList_cbxbtn_BoundParam))
                        templateSubstVars.Add(template_MbrList_cbxbtn_BoundParam, stored_param.Parameter.Name);
                    if (!templateSubstVars.ContainsKey(template_MbrList_Default))
                        templateSubstVars.Add(template_MbrList_Default, row["Fltr_Default"].ToString());
                    if (!templateSubstVars.ContainsKey(template_MbrList_Cube))
                        templateSubstVars.Add(template_MbrList_Cube, "Army");
                    if (!templateSubstVars.ContainsKey(template_MbrList_Dim))
                        templateSubstVars.Add(template_MbrList_Dim, row["Fltr_DimName"].ToString());
                    if (!templateSubstVars.ContainsKey(template_MbrList_Filter))
                        templateSubstVars.Add(template_MbrList_Filter, row["Fltr_MFB"].ToString());

                    BRApi.ErrorLog.LogMessage(si, $"Hit 3.5 JM{colSuffix}");

                    string storedCompName = colSuffix.ToLower() == "btn"
                        ? $"{colSuffix.ToLower()}_DDM_App_MbrList{dimType}"
                        : $"{colSuffix.ToLower()}_DDM_App_MbrList";

                    var storedComponent = api.GetStoredComponentForDynamicDashboard(si, ws, dynamicDashboardEx.DynamicDashboard, storedCompName);
                    var tempComp = api.GetDynamicComponentForDynamicDashboard(si, ws, dynamicDashboardEx, storedComponent.Component, string.Empty, null, TriStateBool.TrueValue, WsDynamicItemStateType.EntireObject);

                    tempComp.DynamicComponent.Component.ApplyParamValueToCurrentDbrd = true;
                    tempComp.DynamicComponent.Component.DashboardComponentType = dashboardTypeResolver[colSuffix];
                    tempComp.DynamicComponent.Component.Text = row["Fltr_" + colSuffix + "Lbl"].ToString();
                    tempComp.DynamicComponent.Component.ToolTip = row["Fltr_" + colSuffix + "ToolTip"].ToString();
                    tempComp.DynamicComponent.Component.Name = storedComponent.Component.Name;

                    if (colSuffix == "Btn")
                    {
                        if (!string.IsNullOrEmpty(tempComp.DynamicComponent.Component.XmlData))
                            compDefinition = XElement.Parse(tempComp.DynamicComponent.Component.XmlData);
                        else
                            compDefinition = buildButtonXML("Filter");

                        tempComp.DynamicComponent.Component.SelectionChangedUIActionType = XFSelectionChangedUIActionType.Refresh;
                        tempComp.DynamicComponent.Component.DashboardsToRedraw = "DDM Dynamic App Dashboard"; // TODO: Update to use row btn dashboard refresh if necessary

                        compDefinition.SetAttributeValue("ButtonType", "SelectMemberDialog"); // TODO: Check if DashboardComponentType.MemberSelectDialog is the same thing
                        if (compDefinition.Element("SelectMemberInfo") == null)
                            compDefinition.Add(new XElement("SelectMemberInfo"));
                        compDefinition.Element("SelectMemberInfo").SetElementValue("DimTypeName", dimType);
                        compDefinition.Element("SelectMemberInfo").SetElementValue("DimName", row["Fltr_DimName"].ToString());
                        compDefinition.Element("SelectMemberInfo").SetElementValue("CubeName", "Army");
                        compDefinition.Element("SelectMemberInfo").SetElementValue("MemberFilter", row["Fltr_MFB"].ToString());
                        compDefinition.SetElementValue("ImageFileSourceType", "DashboardFile");
                        compDefinition.SetElementValue("ImageUrlOrFullFileName", "Std_DB_Search.png"); // TODO: Add a col for allowing image input

                        tempComp.DynamicComponent.Component.XmlData = compDefinition.ToString();
                    }

                    if (!templateSubstVars.ContainsKey(template_MbrList_cbxbtn_BoundParam))
                    {
                        BRApi.ErrorLog.LogMessage(si, "Hit 3");
                        templateSubstVars.Add(template_MbrList_cbxbtn_BoundParam, stored_param.Parameter.Name);
                    }
                    else
                    {
                        templateSubstVars[template_MbrList_cbxbtn_BoundParam] = stored_param.Parameter.Name;
                    }

                    BRApi.ErrorLog.LogMessage(si, "Hit 3.5");
                    WsDynamicComponentEx filterCompEx = api.GetDynamicComponentForDynamicDashboard(si, ws, dynamicDashboardEx, tempComp.DynamicComponent.Component, dimType, templateSubstVars, TriStateBool.TrueValue, WsDynamicItemStateType.EntireObject);

                    BRApi.ErrorLog.LogMessage(si, $"Hit 3.6 {filterCompEx.DynamicComponent.Component.Name}");
                    wsDynCompMembers.Add(new WsDynamicDbrdCompMemberEx(tempCompMember, filterCompEx));
                    BRApi.ErrorLog.LogMessage(si, "Hit 3.7");

                    if (colSuffix == "Txt")
                        containsTxtBox = true;
                }

                BRApi.ErrorLog.LogMessage(si, "Hit 4");

                // Add a visual line separator between filter rows (not after the last one)
                // line component is really just an image component that shows a line /shrug
                if (iteration < rowCount)
                {
                    DashboardComponent tempLine = EngineDashboardComponents.GetComponent(api.DbConnAppOrFW, ws.UniqueID, maintUnit.UniqueID, "img_Line", false, true);
                    WsDynamicComponentEx tempLineCompEx = api.GetDynamicComponentForDynamicDashboard(si, ws, dynamicDashboardEx, tempLine, "line", null, TriStateBool.TrueValue, WsDynamicItemStateType.EntireObject);
                    wsDynCompMembers.Add(new WsDynamicDbrdCompMemberEx(tempCompMember, tempLineCompEx));
                }

                iteration++;
            }

            // If any filter row contained a text box, append a text-entry refresh button
            if (containsTxtBox)
            {
                var txtCompMember = new WsDynamicDbrdCompMember();
                DashboardComponent txtEntryComp = EngineDashboardComponents.GetComponent(api.DbConnAppOrFW, ws.UniqueID, maintUnit.UniqueID, "btn_DDM_EnterText", false, true);
                WsDynamicComponentEx txtEntryCompEx = api.GetDynamicComponentForDynamicDashboard(si, ws, dynamicDashboardEx, txtEntryComp, "TextEntry", null, TriStateBool.TrueValue, WsDynamicItemStateType.EntireObject);
                wsDynCompMembers.Add(new WsDynamicDbrdCompMemberEx(txtCompMember, txtEntryCompEx));
            }

            return wsDynCompMembers;
        }

        // Builds dynamic components for each Button-type header row.
        //
        // Expected DDM_DynDBHdrConfig columns for Button rows (HdrType = 2):
        //   Btn_Lbl                    nvarchar  button label text
        //   Btn_ToolTip                nvarchar  tooltip text
        //   Btn_ImageURL               nvarchar  dashboard-file image name (e.g. "Std_DB_Save.png")
        //   Btn_ActionServerTask       int       → DDM_ConfigHelpers.HdrBtn_ActionServerTaskType enum
        //   Btn_ActionServerTaskArgs   nvarchar  BR name / sequence name / calc args
        //   Btn_ActionSave             int       → XFSelectionChangedSaveType enum (nullable)
        //   Btn_ActionSaveArgs         nvarchar  save action arguments (nullable)
        //   Btn_ActionPOV              int       → XFSelectionChangedPovActionType enum (nullable)
        //   Btn_ActionPOVArgs          nvarchar  POV action arguments (nullable)
        //   Btn_ActionNav              int       → XFSelectionChangedNavigationType enum (nullable)
        //   Btn_ActionNavArgs          nvarchar  navigation arguments, e.g. URL or page name (nullable)
        //   Btn_ActionBoundParam       nvarchar  parameter name to set on click (nullable)
        //   Btn_ActionParamValue       nvarchar  value to assign to the bound parameter (nullable)
        //   Btn_ActionParamApply       nvarchar  "True"/"False" — apply param to current dashboard (nullable)
        //   Btn_ActionUIChanged        int       → XFSelectionChangedUIActionType enum (nullable, overrides Btn_Type)
        //   Btn_ActionUIChangedDBRedraw nvarchar dashboards to redraw (nullable)
        //   Btn_ActionUIChangedDBShow  nvarchar  dashboards to show (nullable)
        //   Btn_ActionUIChangedDBHide  nvarchar  dashboards to hide (nullable)
        //   Btn_ActionUIDialogOpen     nvarchar  dashboard to open as a dialog (nullable)
        //   Btn_ActionUIDialogInitParams nvarchar initial parameter values for the dialog (nullable)
        //   Btn_ActionUIDialogInputParamMap  nvarchar  input param map for dialog (nullable)
        //   Btn_ActionUIDialogOutputParamMap nvarchar output param map for dialog (nullable)
        //   Btn_Type                   int       legacy → DDM_ConfigHelpers.HdrBtnType enum (used when Btn_ActionUIChanged is null)
        private static List<WsDynamicDbrdCompMemberEx> addButtonItems(
            DataTable buttonItems, SessionInfo si, DashboardWorkspace ws,
            IWsasDynamicDashboardsApiV800 api, WsDynamicDashboardEx dynamicDashboardEx,
            DashboardMaintUnit maintUnit)
        {
            var wsDynCompMembers = new List<WsDynamicDbrdCompMemberEx>();
            var tempCompMember = new WsDynamicDbrdCompMember();

            foreach (DataRow row in buttonItems.Rows)
            {
                var templateSubstVars = new Dictionary<string, string>();
                var storedComponent = api.GetStoredComponentForDynamicDashboard(si, ws, dynamicDashboardEx.DynamicDashboard, "btn_DDM_Generic");
                var tempComp = api.GetDynamicComponentForDynamicDashboard(si, ws, dynamicDashboardEx, storedComponent.Component, string.Empty, null, TriStateBool.TrueValue, WsDynamicItemStateType.EntireObject);

                tempComp.DynamicComponent.Component.DashboardComponentType = DashboardComponentType.Button;

                XElement compDefinition;
                if (!string.IsNullOrEmpty(tempComp.DynamicComponent.Component.XmlData))
                    compDefinition = XElement.Parse(tempComp.DynamicComponent.Component.XmlData);
                else
                    compDefinition = buildButtonXML("Button");

                // --- Label / ToolTip / Image ---
                tempComp.DynamicComponent.Component.Text = GetStr(row, "Btn_Lbl");
                tempComp.DynamicComponent.Component.ToolTip = GetStr(row, "Btn_ToolTip");
                compDefinition.SetElementValue("ImageFileSourceType", "DashboardFile");
                compDefinition.SetElementValue("ImageUrlOrFullFileName", GetStr(row, "Btn_ImageURL"));

                // --- Server task ---
                string btnServerTaskName = GetEnumName(row, "Btn_ActionServerTask", typeof(DDM_ConfigHelpers.HdrBtn_ActionServerTaskType));
                tempComp.DynamicComponent.Component.SelectionChangedTaskType =
                    (!string.IsNullOrEmpty(btnServerTaskName) && serverTaskTypeResolver.ContainsKey(btnServerTaskName))
                        ? serverTaskTypeResolver[btnServerTaskName]
                        : XFSelectionChangedTaskType.ExecuteDashboardExtenderBRConsServer;
                tempComp.DynamicComponent.Component.SelectionChangedTaskArgs = GetStr(row, "Btn_ActionServerTaskArgs");

                // --- Save action ---
                if (row.Table.Columns.Contains("Btn_ActionSave") && row["Btn_ActionSave"] != DBNull.Value)
                {
                    tempComp.DynamicComponent.Component.SelectionChangedSaveType =
                        (XFSelectionChangedSaveType)Convert.ToInt32(row["Btn_ActionSave"]);
                    tempComp.DynamicComponent.Component.SelectionChangedSaveArgs = GetStr(row, "Btn_ActionSaveArgs");
                }

                // --- POV action ---
                if (row.Table.Columns.Contains("Btn_ActionPOV") && row["Btn_ActionPOV"] != DBNull.Value)
                {
                    tempComp.DynamicComponent.Component.SelectionChangedPovActionType =
                        (XFSelectionChangedPovActionType)Convert.ToInt32(row["Btn_ActionPOV"]);
                    tempComp.DynamicComponent.Component.SelectionChangedPovArgs = GetStr(row, "Btn_ActionPOVArgs");
                }

                // --- Navigation action ---
                if (row.Table.Columns.Contains("Btn_ActionNav") && row["Btn_ActionNav"] != DBNull.Value)
                {
                    tempComp.DynamicComponent.Component.SelectionChangedNavigationType =
                        (XFSelectionChangedNavigationType)Convert.ToInt32(row["Btn_ActionNav"]);
                    tempComp.DynamicComponent.Component.SelectionChangedNavigationArgs = GetStr(row, "Btn_ActionNavArgs");
                }

                // --- Apply-parameter action ---
                if (!string.IsNullOrEmpty(GetStr(row, "Btn_ActionBoundParam")))
                {
                    tempComp.DynamicComponent.Component.BoundParameterName = GetStr(row, "Btn_ActionBoundParam");
                    tempComp.DynamicComponent.Component.ParamValueForButtonClick = GetStr(row, "Btn_ActionParamValue");
                    if (row.Table.Columns.Contains("Btn_ActionParamApply") && !string.IsNullOrEmpty(GetStr(row, "Btn_ActionParamApply")))
                    {
                        tempComp.DynamicComponent.Component.ApplyParamValueToCurrentDbrd =
                            GetStr(row, "Btn_ActionParamApply").XFEqualsIgnoreCase("True");
                    }
                }

                // --- UI-changed action (Btn_ActionUIChanged overrides legacy Btn_Type) ---
                if (row.Table.Columns.Contains("Btn_ActionUIChanged") && row["Btn_ActionUIChanged"] != DBNull.Value)
                {
                    tempComp.DynamicComponent.Component.SelectionChangedUIActionType =
                        (XFSelectionChangedUIActionType)Convert.ToInt32(row["Btn_ActionUIChanged"]);
                }
                else
                {
                    // Fall back to legacy Btn_Type behavior (int -> enum name; Complete_WF opens a dialog)
                    string btnTypeName = GetEnumName(row, "Btn_Type", typeof(DDM_ConfigHelpers.HdrBtnType));
                    tempComp.DynamicComponent.Component.SelectionChangedUIActionType =
                        (btnTypeName == "Complete_WF")
                            ? XFSelectionChangedUIActionType.OpenDialogApplyChangesAndRefresh
                            : XFSelectionChangedUIActionType.Refresh;
                }

                tempComp.DynamicComponent.Component.DashboardsToRedraw = GetStr(row, "Btn_ActionUIChangedDBRedraw");
                tempComp.DynamicComponent.Component.DashboardsToShow = GetStr(row, "Btn_ActionUIChangedDBShow");
                tempComp.DynamicComponent.Component.DashboardsToHide = GetStr(row, "Btn_ActionUIChangedDBHide");
                tempComp.DynamicComponent.Component.DashboardForDialog = GetStr(row, "Btn_ActionUIDialogOpen");
                tempComp.DynamicComponent.Component.DlgInitialParameterValues = GetStr(row, "Btn_ActionUIDialogInitParams");
                tempComp.DynamicComponent.Component.DlgInputParameterMap = GetStr(row, "Btn_ActionUIDialogInputParamMap");
                tempComp.DynamicComponent.Component.DlgOutputParameterMap = GetStr(row, "Btn_ActionUIDialogOutputParamMap");

                tempComp.DynamicComponent.Component.XmlData = compDefinition.ToString();

                WsDynamicComponentEx buttonCompEx = api.GetDynamicComponentForDynamicDashboard(si, ws, dynamicDashboardEx, tempComp.DynamicComponent.Component, string.Empty, templateSubstVars, TriStateBool.TrueValue, WsDynamicItemStateType.EntireObject);
                wsDynCompMembers.Add(new WsDynamicDbrdCompMemberEx(tempCompMember, buttonCompEx));

                BRApi.ErrorLog.LogMessage(si, "Hit 5");
            }

            return wsDynCompMembers;
        }

#endregion
    }
}