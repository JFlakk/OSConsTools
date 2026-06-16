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
            // add header items
            var dt = DDM_Support.get_HeaderItems(si, customSubstVarsAlreadyResolved,1);
            var tempColl = addHeaderItems(ref dt, si, workspace, api, dynamicDashboardEx, maintUnit);
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
        
        private static WsDynamicComponentCollection addHeaderItems(ref DataTable headerItems, SessionInfo si, DashboardWorkspace ws, IWsasDynamicDashboardsApiV800 api, WsDynamicDashboardEx dynamicDashboardEx, DashboardMaintUnit maintUnit)
        {

            var wsDynCompMembers = new List<WsDynamicDbrdCompMemberEx>();
            var storedComponent = new DashboardDbrdCompMemberEx();
            var nameSuffix = string.Empty;

            var templateSubstVars = new Dictionary<string, string>();

            int iteration = 1;
            bool containsTxtBox = false;
            int rowCount = headerItems.Rows.Count;

            foreach (DataRow row in headerItems.Rows)
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
                    //new_param.Parameter.Name = row["Fltr_BtnCbx_BoundParam"].ToString();
                    new_param.Parameter.Name = $"{stored_param.Parameter.Name}";
                    new_param.Parameter.ParameterType = DashboardParamType.MemberList;
                    new_param.Parameter.DimTypeName = dimType;
                    new_param.Parameter.CubeName = "Army";
                    new_param.Parameter.MemberFilter = row["Fltr_MFB"].ToString();
                    new_param.Parameter.DimName = row["Fltr_DimName"].ToString();
                    //tempComp.DynamicComponent.Component.BoundParameterName = new_param.Parameter.Name;
                }
                else
                {
                    new_param = new WsDynamicParameter(true, stored_param.Parameter, Guid.NewGuid(), row["Fltr_TxtBoundParam"].ToString(), ws.Name);
                    new_param.Parameter = new DashboardParameter();
                    new_param.Parameter.Name = row["Fltr_TxtBoundParam"].ToString();
                    new_param.Parameter.ParameterType = DashboardParamType.InputValue;
                    //tempComp.DynamicComponent.Component.BoundParameterName = new_param.Parameter.Name;
                }
				BRApi.ErrorLog.LogMessage(si,$"Hit Hdr items :{headerItems.Rows.Count}");
				templateSubstVars.Clear();
                var tempCompMember = new WsDynamicDbrdCompMember();
                var compDefinition = new XElement("XFCompDefinition");
				var tempComp = new WsDynamicComponentEx();

                var baseSearch = string.Empty;

                var optTypeValue = Convert.ToInt32(row["HdrType"]);
				//BRApi.ErrorLog.LogMessage(si,$"Hit: {optType}");
				var optType = Enum.GetName(typeof(DDM_ConfigHelpers.HdrType), optTypeValue);

                var storedCompName_servertaskbtn = "btn_DDM_App_Complete_WF";


                // based on item type, switch to define the header item
                switch (optType)
                {
                    case "Filter":
                        baseSearch += "Fltr";
						BRApi.ErrorLog.LogMessage(si,$"Hit {baseSearch}");

                        foreach (string colSuffix in dashboardTypeResolver.Keys)
                        {
						BRApi.ErrorLog.LogMessage(si,$"Hit 2 {colSuffix}.tostring()");
							
							string colName = baseSearch + "_" + colSuffix;
								bool isEnabled = false;
								
								// Check if column exists and is not null
								if (headerItems.Columns.Contains(colName) && row[colName] != DBNull.Value)
								{
								    isEnabled = Convert.ToBoolean(row[colName]);
								}
								
								if (isEnabled)
								{
								   
								
							
                           // if ((bool)row[baseSearch + "_" + colSuffix])
                            {
														BRApi.ErrorLog.LogMessage(si,"Hit 3");

								if (!templateSubstVars.ContainsKey(template_MbrList_cbxbtn_BoundParam))
                                {
                                    templateSubstVars.Add(template_MbrList_cbxbtn_BoundParam, stored_param.Parameter.Name);
                                }
                                if (!templateSubstVars.ContainsKey(template_MbrList_Default))
                                {
                                    templateSubstVars.Add(template_MbrList_Default, row[baseSearch + "_Default"].ToString());
                                }
                                if (!templateSubstVars.ContainsKey(template_MbrList_Cube))
                                {
                                    templateSubstVars.Add(template_MbrList_Cube, "Army");
                                }
                                if (!templateSubstVars.ContainsKey(template_MbrList_Dim))
                                {
                                    templateSubstVars.Add(template_MbrList_Dim, row[baseSearch + "_DimName"].ToString());
                                }
                                if (!templateSubstVars.ContainsKey(template_MbrList_Filter))
                                {
                                    templateSubstVars.Add(template_MbrList_Filter, row[baseSearch + "_MFB"].ToString());
                                }
								 BRApi.ErrorLog.LogMessage(si,$"Hit 3.5 JM{colSuffix}");
								var storedCompName = string.Empty;
								if (colSuffix.ToLower() == "btn")
								{
									storedCompName = $"{colSuffix.ToLower()}_DDM_App_MbrList{dimType}";
								}
								else
								{
									storedCompName = $"{colSuffix.ToLower()}_DDM_App_MbrList";
								}
								storedComponent = api.GetStoredComponentForDynamicDashboard(si, ws, dynamicDashboardEx.DynamicDashboard, storedCompName);
   
								tempComp = api.GetDynamicComponentForDynamicDashboard(si, ws, dynamicDashboardEx, storedComponent.Component, string.Empty, null, TriStateBool.TrueValue, WsDynamicItemStateType.EntireObject);
                                //tempComp.DynamicComponent.Component.BoundParameterName = new_param.Parameter.Name;
								//BRApi.ErrorLog.LogMessage(si,$"Hit 3.6 JM {storedComponent.Component.Name}");
								tempComp.DynamicComponent.Component.ApplyParamValueToCurrentDbrd = true;
								//BRApi.ErrorLog.LogMessage(si,"Hit 3.7 JM");
                                //								tempComp.DisplayFormat = "|!LV_Std_btn_Format!|";

                                tempComp.DynamicComponent.Component.DashboardComponentType = dashboardTypeResolver[colSuffix];
								//BRApi.ErrorLog.LogMessage(si,"Hit 3.8 JM");
                                tempComp.DynamicComponent.Component.Text = row[baseSearch + "_" + colSuffix + "Lbl"].ToString();
                                tempComp.DynamicComponent.Component.ToolTip = row[baseSearch + "_" + colSuffix + "ToolTip"].ToString();
                                tempComp.DynamicComponent.Component.Name = storedComponent.Component.Name;
//BRApi.ErrorLog.LogMessage(si,"Hit 2");
                                if (colSuffix == "Btn")
                                {
                                    if (!String.IsNullOrEmpty(tempComp.DynamicComponent.Component.XmlData))
                                    {
                                        compDefinition = XElement.Parse(tempComp.DynamicComponent.Component.XmlData);
                                    }
                                    else
                                    {
                                        compDefinition = buildButtonXML(optType);
                                    }

                                    tempComp.DynamicComponent.Component.SelectionChangedUIActionType = XFSelectionChangedUIActionType.Refresh;
                                    tempComp.DynamicComponent.Component.DashboardsToRedraw = "DDM Dynamic App Dashboard"; //TODO: Update to use row btn dashboard refresh if necessary

                                    compDefinition.SetAttributeValue("ButtonType", "SelectMemberDialog"); // TODO: Check if DashboardComponentType.MemberSelectDialog is the same thing as this. Would likely still need to update through XML though for underlying

                                    if (compDefinition.Element("SelectMemberInfo") == null)
                                    {
                                        compDefinition.Add(new XElement("SelectMemberInfo"));
                                    }
                                    compDefinition.Element("SelectMemberInfo").SetElementValue("DimTypeName", dimType);
                                    compDefinition.Element("SelectMemberInfo").SetElementValue("DimName", row[baseSearch + "_DimName"].ToString());
                                    compDefinition.Element("SelectMemberInfo").SetElementValue("CubeName", "Army");
                                    compDefinition.Element("SelectMemberInfo").SetElementValue("MemberFilter", row[baseSearch + "_MFB"].ToString());
                                    compDefinition.SetElementValue("ImageFileSourceType", "DashboardFile");
                                    compDefinition.SetElementValue("ImageUrlOrFullFileName", "Std_DB_Search.png"); // TODO: Add a col for allowing image input
                                }
//BRApi.ErrorLog.LogMessage(si,"Hit 3");
                                if (compDefinition != null)
                                {
                                    tempComp.DynamicComponent.Component.XmlData = compDefinition.ToString();
                                }

                                nameSuffix = "Dyn_1";

                                if (!templateSubstVars.ContainsKey(template_MbrList_cbxbtn_BoundParam))
                                {
BRApi.ErrorLog.LogMessage(si,"Hit 3");
                                    templateSubstVars.Add(template_MbrList_cbxbtn_BoundParam, stored_param.Parameter.Name);
                                }
                                else
                                {
                                    templateSubstVars[template_MbrList_cbxbtn_BoundParam] = stored_param.Parameter.Name;
                                }
BRApi.ErrorLog.LogMessage(si,"Hit 3.5");
                                WsDynamicComponentEx filterCompEx = api.GetDynamicComponentForDynamicDashboard(si, ws,dynamicDashboardEx, tempComp.DynamicComponent.Component, dimType, templateSubstVars, TriStateBool.TrueValue, WsDynamicItemStateType.EntireObject);
                             
								BRApi.ErrorLog.LogMessage(si,$"Hit 3.6 {filterCompEx.DynamicComponent.Component.Name.ToString()}");
								wsDynCompMembers.Add(new WsDynamicDbrdCompMemberEx(tempCompMember, filterCompEx));
 BRApi.ErrorLog.LogMessage(si,"Hit 3.7");
                                 //add a secondary button component to handle text entry
                                if (colSuffix == "Txt")
                                {
									//BRApi.ErrorLog.LogMessage(si,"Hit 3.6");
                                    containsTxtBox = true;
                                }
								
                            }
							
                        }}

BRApi.ErrorLog.LogMessage(si,"Hit 4");
                        break;
                    case "Button":
                        baseSearch += "Btn";
                        storedComponent = api.GetStoredComponentForDynamicDashboard(si, ws, dynamicDashboardEx.DynamicDashboard, "btn_DDM_Generic");
                        tempComp = api.GetDynamicComponentForDynamicDashboard(si, ws, dynamicDashboardEx, storedComponent.Component, string.Empty, null, TriStateBool.TrueValue, WsDynamicItemStateType.EntireObject);

                        tempComp.DynamicComponent.Component.DashboardComponentType = DashboardComponentType.Button;

                        if (!String.IsNullOrEmpty(tempComp.DynamicComponent.Component.XmlData))
                        {
                            compDefinition = XElement.Parse(tempComp.DynamicComponent.Component.XmlData);
                        }
                        else
                        {
                            compDefinition = buildButtonXML(optType);
                        }

                        // --- Label / ToolTip / Image (schema: Btn_Lbl, Btn_ToolTip, Btn_ImageURL) ---
                        tempComp.DynamicComponent.Component.Text = GetStr(row, "Btn_Lbl");
                        tempComp.DynamicComponent.Component.ToolTip = GetStr(row, "Btn_ToolTip");
                        compDefinition.SetElementValue("ImageFileSourceType", "DashboardFile");
                        compDefinition.SetElementValue("ImageUrlOrFullFileName", GetStr(row, "Btn_ImageURL"));

                        // --- Server task (schema: Btn_ActionServerTask int -> enum name, Btn_ActionServerTaskArgs) ---
                        string btnServerTaskName = GetEnumName(row, "Btn_ActionServerTask", typeof(DDM_ConfigHelpers.HdrBtn_ActionServerTaskType));
                        tempComp.DynamicComponent.Component.SelectionChangedTaskType =
                            (!string.IsNullOrEmpty(btnServerTaskName) && serverTaskTypeResolver.ContainsKey(btnServerTaskName))
                                ? serverTaskTypeResolver[btnServerTaskName]
                                : XFSelectionChangedTaskType.ExecuteDashboardExtenderBRConsServer;
                        tempComp.DynamicComponent.Component.SelectionChangedTaskArgs = GetStr(row, "Btn_ActionServerTaskArgs");

                        // --- Save action (schema: Btn_ActionSave int -> XFSelectionChangedSaveDataType, Btn_ActionSaveArgs) ---
                        if (row.Table.Columns.Contains("Btn_ActionSave") && row["Btn_ActionSave"] != DBNull.Value)
                        {
                            tempComp.DynamicComponent.Component.SelectionChangedSaveType =
                                (XFSelectionChangedSaveType)Convert.ToInt32(row["Btn_ActionSave"]);
                            tempComp.DynamicComponent.Component.SelectionChangedSaveArgs = GetStr(row, "Btn_ActionSaveArgs");
                        }

                        // --- POV action (schema: Btn_ActionPOV int -> XFSelectionChangedPovActionType, Btn_ActionPOVArgs) ---
                        if (row.Table.Columns.Contains("Btn_ActionPOV") && row["Btn_ActionPOV"] != DBNull.Value)
                        {
                            tempComp.DynamicComponent.Component.SelectionChangedPovActionType =
                                (XFSelectionChangedPovActionType)Convert.ToInt32(row["Btn_ActionPOV"]);
                            tempComp.DynamicComponent.Component.SelectionChangedPovArgs = GetStr(row, "Btn_ActionPOVArgs");
                        }

                        // --- Navigation action (schema: Btn_ActionNav int -> XFSelectionChangedNavigationType, Btn_ActionNavArgs) ---
                        if (row.Table.Columns.Contains("Btn_ActionNav") && row["Btn_ActionNav"] != DBNull.Value)
                        {
                            tempComp.DynamicComponent.Component.SelectionChangedNavigationType =
                                (XFSelectionChangedNavigationType)Convert.ToInt32(row["Btn_ActionNav"]);
                            tempComp.DynamicComponent.Component.SelectionChangedNavigationArgs = GetStr(row, "Btn_ActionNavArgs");
                        }

                        // --- Apply-parameter action (schema: Btn_ActionBoundParam / Value / Apply) ---
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

                        // --- UI-changed action (schema: Btn_ActionUIChanged int -> XFSelectionChangedUIActionType,
                        //     Btn_ActionUIChangedDBRedraw, plus dialog open + param maps) ---
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

                        if (compDefinition != null)
                        {
                            tempComp.DynamicComponent.Component.XmlData = compDefinition.ToString();
                        }

                        WsDynamicComponentEx buttonCompEx = api.GetDynamicComponentForDynamicDashboard(si, ws, dynamicDashboardEx, tempComp.DynamicComponent.Component, nameSuffix, templateSubstVars, TriStateBool.TrueValue, WsDynamicItemStateType.EntireObject);
                        wsDynCompMembers.Add(new WsDynamicDbrdCompMemberEx(tempCompMember, buttonCompEx));

BRApi.ErrorLog.LogMessage(si,"Hit 5");
                        break;
                    default:
                        continue;
                        break;

                }



                // if there are items after this item and the current item is not a button, add a line
                // line component is really just an image component that shows a line /shrug
                // TODO: Figure out how to skip the line if it's a btn comp
                if (iteration < rowCount && optType != "Button")
                {
                    // add line image component
                    string storedImageCompName = "img_Line";

                    DashboardComponent tempLine = EngineDashboardComponents.GetComponent(api.DbConnAppOrFW, ws.UniqueID, maintUnit.UniqueID, storedImageCompName, false, true);
                    WsDynamicComponentEx tempLineCompEx = api.GetDynamicComponentForDynamicDashboard(si, ws, dynamicDashboardEx, tempLine, "line", null, TriStateBool.TrueValue, WsDynamicItemStateType.EntireObject);

                    WsDynamicDbrdCompMemberEx tempLineDynCompMemberEx = new WsDynamicDbrdCompMemberEx(tempCompMember, tempLineCompEx);
                    wsDynCompMembers.Add(tempLineDynCompMemberEx);

                }


                iteration++;

            }


            // if any items contained a text box add a refresh button on the end
            if (containsTxtBox)
            {
                string storedCompName_textEntered = "btn_DDM_EnterText";
                WsDynamicDbrdCompMember tempCompMember = new WsDynamicDbrdCompMember(); // can adjust dashboard info here

                DashboardComponent txtEntryComp = EngineDashboardComponents.GetComponent(api.DbConnAppOrFW, ws.UniqueID, maintUnit.UniqueID, storedCompName_textEntered, false, true);

                WsDynamicComponentEx txtEntryCompEx = api.GetDynamicComponentForDynamicDashboard(si, ws, dynamicDashboardEx, txtEntryComp, "TextEntry", null, TriStateBool.TrueValue, WsDynamicItemStateType.EntireObject);
                wsDynCompMembers.Add(new WsDynamicDbrdCompMemberEx(tempCompMember, txtEntryCompEx));
            }


            return new WsDynamicComponentCollection(dynamicDashboardEx, wsDynCompMembers);
        }

#endregion
    }
}