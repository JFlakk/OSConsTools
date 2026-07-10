Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.Text.RegularExpressions
Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic
Imports OneStream.Finance.Database
Imports OneStream.Finance.Engine
Imports OneStream.Shared.Common
Imports OneStream.Shared.Database
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Wcf
Imports OneStream.Stage.Database
Imports OneStream.Stage.Engine

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.BUDFM_SolutionHelper
	Public Class MainClass
		' ---- legacy class variables (BudFM_SolutionHelper) ----
		Dim rpUtils As New BUDFM_RP_Utilities
		Dim mShowMessage As String = "Please revert or save changes before navigating away from this page"
		Dim mblnEnableSavePrompt As Boolean = False
		Dim mBlnLogSavePromptErrors As Boolean = False
		' Finance custom-calc target for the 3 remaining ExecuteCustomCalculate
		' call sites (staff-symbol functions -- they need the finance engine's
		' api, so they cannot be direct calls). Points at the assembly's
		' name-discovered Finance BR (Finance/BUDFM_CustomCalc.vb). If 9.3
		' rejects workspace-qualified names here, fall back to the legacy
		' inline rule name "USCG_BudFm_Utilities".
		Private Const FINANCE_CALC_RULE As String = "Workspace.Current.BUDFM_Assembly.BUDFM_CustomCalc"

		Public si As SessionInfo
		Public globals As BRGlobals
		Public api As Object
		Public args As DashboardExtenderArgs

		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Try
				Me.si = si : Me.globals = globals : Me.api = api : Me.args = args
				rpUtils.Main(si, globals, api, New ExtenderArgs())
				Dim fn As String = args.FunctionName

				If args.FunctionType = DashboardExtenderFunctionType.ComponentSelectionChanged AndAlso IsRouting(fn) Then
					Return OnSelectionChanged()
				End If

				Select Case fn
					' suffix-family -> one method keyed on RPAppr
					Case "ClearEXPLine_BS", "ClearEXPLine_F", "ClearEXPLine_MERHCF", "ClearEXPLine_MOSP", "ClearEXPLine_PCI", "ClearEXPLine_RD", "ClearEXPLine_RP"
						Return Me.ClearEXPLine(ApprFromFn(fn))
					' suffix-family -> one method keyed on RPAppr
					Case "DeleteEXPLine_BS", "DeleteEXPLine_F", "DeleteEXPLine_MERHCF", "DeleteEXPLine_MOSP", "DeleteEXPLine_PCI", "DeleteEXPLine_RD", "DeleteEXPLine_RP"
						Return Me.DeleteEXPLine(ApprFromFn(fn))
					' suffix-family -> one method keyed on RPAppr
					Case "EditEXPLine_F", "EditEXPLine_PCI", "EditEXPLine_RD"
						Return Me.EditEXPLine(ApprFromFn(fn))
					' suffix-family -> one method keyed on RPAppr
					Case "EditHIST_PCI", "EditHIST_RD"
						Return Me.EditHIST(ApprFromFn(fn))
					' suffix-family -> one method keyed on RPAppr
					Case "EditRP_Page1_F", "EditRP_Page1_OS", "EditRP_Page1_PCI", "EditRP_Page1_RD"
						Return Me.EditRP_Page1(ApprFromFn(fn))
					' suffix-family -> one method keyed on RPAppr
					Case "OnCbxBtnClick_GEN_BS", "OnCbxBtnClick_GEN_F", "OnCbxBtnClick_GEN_MERHCF", "OnCbxBtnClick_GEN_MOSP", "OnCbxBtnClick_GEN_PCI", "OnCbxBtnClick_GEN_RD", "OnCbxBtnClick_GEN_RP"
						Return Me.OnCbxBtnClick_GEN(ApprFromFn(fn))
					' suffix-family -> one method keyed on RPAppr
					Case "OnCbxRP_Expense_Selected_BS", "OnCbxRP_Expense_Selected_F", "OnCbxRP_Expense_Selected_MERHCF", "OnCbxRP_Expense_Selected_MOSP", "OnCbxRP_Expense_Selected_PCI", "OnCbxRP_Expense_Selected_RD", "OnCbxRP_Expense_Selected_RP"
						Return Me.OnCbxRP_Expense_Selected(ApprFromFn(fn))
					Case "AddMod" : Return Me.AddMod()
					Case "AddModHierachyMember" : Return Me.AddModHierachyMember()
					Case "ClearNonBLTLine_OS" : Return Me.ClearNonBLTLine_OS()
					Case "Consol_WFScenario" : Return Me.Consol_WFScenario()
					Case "CopyBilletsToDestination_OS" : Return Me.CopyBilletsToDestination_OS()
					Case "CopyRPAttributes" : Return Me.CopyRPAttributes()
					Case "CopyRPAttributesNew" : Return Me.CopyRPAttributesNew()
					Case "CreateInitialFYABVModHierarchy", "CreateInitialFYABVMODHierarchy" : Return Me.CreateInitialFYABVModHierarchy()
					Case "CreateInitialFYModHierarchy", "CreateInitialFYMODHierarchy" : Return Me.CreateInitialFYModHierarchy()
					Case "CreateNewRPAsExtension" : Return Me.CreateNewRPAsExtension()
					Case "CreateNewRPFromScratch" : Return Me.CreateNewRPFromScratch()
					Case "CreateRPs" : Return Me.CreateRPs()
					Case "CreateWorkingVersionOfRP" : Return Me.CreateWorkingVersionOfRP()
					Case "CurrentScenarioManagement" : Return Me.CurrentScenarioManagement()
					Case "DefualtYesOrNo" : Return Me.DefualtYesOrNo()
					Case "DeleteBLTLine_OS" : Return Me.DeleteBLTLine_OS()
					Case "DeleteBLTLine_OS_Mass" : Return Me.DeleteBLTLine_OS_Mass()
					Case "DeleteBilletList" : Return Me.DeleteBilletList()
					Case "DeleteModHierarchyMember" : Return Me.DeleteModHierarchyMember()
					Case "DeleteNonBLTLine_OS" : Return Me.DeleteNonBLTLine_OS()
					Case "DeleteSupportingDoc" : Return Me.DeleteSupportingDoc()
					Case "DownloadSupportingDoc" : Return Me.DownloadSupportingDoc()
					Case "EditBLTLine_Mass_OS" : Return Me.EditBLTLine_Mass_OS()
					Case "EditBLTLine_OS" : Return Me.EditBLTLine_OS()
					Case "EditEXPLine" : Return Me.EditEXPLine()
					Case "EditNBLTLine_OS" : Return Me.EditNBLTLine_OS()
					Case "EditRP_Page1" : Return Me.EditRP_Page1()
					Case "EditRP_Page2" : Return Me.EditRP_Page2()
					Case "EditRP_Page2_OS" : Return Me.EditRP_Page2_OS()
					Case "EditRP_Page3" : Return Me.EditRP_Page3()
					Case "EditRP_Page3_ConstrWords_PCI" : Return Me.EditRP_Page3_ConstrWords_PCI()
					Case "EditRP_Page3_EndItemsWords_PCI" : Return Me.EditRP_Page3_EndItemsWords_PCI()
					Case "EditRP_Page3_ProqAcqWords_PCI" : Return Me.EditRP_Page3_ProqAcqWords_PCI()
					Case "EditRP_Page3_RD" : Return Me.EditRP_Page3_RD()
					Case "EditScenarioSecurity" : Return Me.EditScenarioSecurity()
					Case "GetFirstScenario" : Return Me.GetFirstScenario()
					Case "LoadWFDashboard" : Return Me.LoadWFDashboard()
					Case "RefreshRPAttributes" : Return Me.RefreshRPAttributes()
					Case "ModComments" : Return Me.ModComments()
					Case "Mod_OMBJ_CJ_Comments" : Return Me.Mod_OMBJ_CJ_Comments()
					Case "MoveRelationshipMember" : Return Me.MoveRelationshipMember()
					Case "OnCbxBtnClick_GEN" : Return Me.OnCbxBtnClick_GEN()
					Case "OnCbxRP_BilletOPFAC_Selected" : Return Me.OnCbxRP_BilletOPFAC_Selected()
					Case "OnCbxRP_BilletReserveType_Selected" : Return Me.OnCbxRP_BilletReserveType_Selected()
					Case "OnCbxRP_BilletType_Selected" : Return Me.OnCbxRP_BilletType_Selected()
					Case "OnCbxRP_Billet_Selected" : Return Me.OnCbxRP_Billet_Selected()
					Case "OnCbxRP_BuildOut_Lease_Selected" : Return Me.OnCbxRP_BuildOut_Lease_Selected()
					Case "OnCbxRP_GradeType_Selected" : Return Me.OnCbxRP_GradeType_Selected()
					Case "OnCbxRP_Lease_Selected" : Return Me.OnCbxRP_Lease_Selected()
					Case "OnCbxRP_NBLT_RequestedItem_Tier1_Selected" : Return Me.OnCbxRP_NBLT_RequestedItem_Tier1_Selected()
					Case "OnCbxRP_NonBillet_Selected" : Return Me.OnCbxRP_NonBillet_Selected()
					Case "OnCbxRP_PPE_Selected" : Return Me.OnCbxRP_PPE_Selected()
					Case "OnCbxRP_SetDefault_NonBilletATU" : Return Me.OnCbxRP_SetDefault_NonBilletATU()
					Case "OnCbxRP_SetDefault_NonBilletPPA" : Return Me.OnCbxRP_SetDefault_NonBilletPPA()
					Case "OnCbxRP_SpcCode_Selected" : Return Me.OnCbxRP_SpcCode_Selected()
					Case "OnCbxRP_Utilities_Selected" : Return Me.OnCbxRP_Utilities_Selected()
					Case "OnConcReviewBtnClick" : Return Me.OnConcReviewBtnClick()
					Case "RetrieveModComments" : Return Me.RetrieveModComments()
					Case "ReviseMemDescription" : Return Me.ReviseMemDescription()
					Case "RpToModMapping" : Return Me.RpToModMapping()
					Case "SaveRPStatusWithComments" : Return Me.SaveRPStatusWithComments()
					Case "SearchRPsandSetDashboard" : Return Me.SearchRPsandSetDashboard()
					Case "SetDynamicParameters" : Return Me.SetDynamicParameters()
					Case "SetLiteralParamValue" : Return Me.SetLiteralParamValue()
					Case "SetPPADefaults" : Return Me.SetPPADefaults()
					Case "SetRPStatus" : Return Me.SetRPStatus()
					Case "StaffSymbolConcReview_AddStaffSymbol" : Return Me.StaffSymbolConcReview_AddStaffSymbol()
					Case "StaffSymbolConcReview_AutoPopulate" : Return Me.StaffSymbolConcReview_AutoPopulate()
					Case "UpdateRPsWithComment" : Return Me.UpdateRPsWithComment()
					Case "UploadSupportingDoc" : Return Me.UploadSupportingDoc()
					Case "WorkflowComplete" : Return Me.WorkflowComplete()
					Case "WorkflowRevert" : Return Me.WorkflowRevert()
					' ---- actions called only by non-OS appropriation dashboards ----
					Case "ClearBLTLine_OS" : Return Me.ClearBLTLine_OS()
					Case "OnBtnClick_GEN" : Return Me.OnBtnClick_GEN()
					Case "OnCbxBtnClick_RPCreate" : Return Me.OnCbxBtnClick_RPCreate()
					Case "OnCbxRP_BudgetCat_Selected" : Return Me.OnCbxRP_BudgetCat_Selected()
					Case "OnCreateBtnClick" : Return Me.OnCreateBtnClick()
					Case "OnHeaderBtnClick_GEN" : Return Me.OnHeaderBtnClick_GEN()
					Case "OnHeaderRP_Billet_Selected" : Return Me.OnHeaderRP_Billet_Selected()
					Case "OnHeaderRP_NonBillet_Selected" : Return Me.OnHeaderRP_NonBillet_Selected()
					Case "OnReportingBtnClick" : Return Me.OnReportingBtnClick()
					Case "Refresh" : Return Me.Refresh()
					Case "Refresh_PPA_Extractor" : Return Me.Refresh_PPA_Extractor()
					Case "Revert_OS_B" : Return Me.Revert_OS_B()
					Case "RollForward" : Return Me.RollForward()
					Case "UpdateTextValue" : Return Me.UpdateTextValue()
					Case "Update_RP_TermBillet" : Return Me.Update_RP_TermBillet()
					Case Else : Return Nothing
				End Select
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Function

		Private Function IsRouting(ByVal fn As String) As Boolean
			Return fn.XFEqualsIgnoreCase("HeaderSelectionChanged") OrElse fn.XFEqualsIgnoreCase("EditRPSelectionChanged") _
				OrElse fn.XFEqualsIgnoreCase("BilletSelectionChanged") OrElse fn.XFEqualsIgnoreCase("CostEstSelectionChanged") _
				OrElse fn.XFEqualsIgnoreCase("SetContentSelectionChanged") OrElse fn.XFEqualsIgnoreCase("RptSelectionChanged") _
				OrElse fn.XFEqualsIgnoreCase("WFNavigation")
		End Function

		' appropriation code parsed from the trailing suffix of a collapsed-family FunctionName
		Private Function ApprFromFn(ByVal fn As String) As String
			Dim i As Integer = fn.LastIndexOf("_"c)
			Return If(i >= 0, fn.Substring(i + 1), String.Empty)
		End Function

		Private Function NormalizeRoutingAppn(ByVal appn As String, Optional ByVal fallback As String = "OS") As String
			Dim normalized As String = BUDFM_AttributeSupport.NormalizeAppn(appn)
			If String.IsNullOrWhiteSpace(normalized) Then Return BUDFM_AttributeSupport.NormalizeAppn(fallback)
			Return normalized
		End Function

		Private Function ResolveRoutingAppnForRP(ByVal rpName As String, Optional ByVal fallbackAppn As String = "OS") As String
			Dim fallback As String = NormalizeRoutingAppn(fallbackAppn)
			If String.IsNullOrWhiteSpace(rpName) Then Return fallback
			Try
				Return NormalizeRoutingAppn(rpUtils.Get_RP_Appropriation(si, rpName), fallback)
			Catch
				Return fallback
			End Try
		End Function

		Private Sub SetRoutingNumber(ByVal vars As Dictionary(Of String, String), ByVal appn As String, ByVal rpNumber As String)
			BUDFM_AttributeSupport.SetRoutingParamValue(vars, BUDFM_AttributeSupport.RoutingNumberKey, NormalizeRoutingAppn(appn), rpNumber)
		End Sub

		Private Sub SetRoutingContent(ByVal vars As Dictionary(Of String, String), ByVal appn As String, ByVal content As String)
			BUDFM_AttributeSupport.SetRoutingParamValue(vars, BUDFM_AttributeSupport.RoutingContentKey, NormalizeRoutingAppn(appn), content)
		End Sub

		Private Sub SetRoutingPageCompat(ByVal vars As Dictionary(Of String, String), ByVal appn As String, ByVal contentPage As String)
			BUDFM_AttributeSupport.SetRoutingPageCompatValues(vars, NormalizeRoutingAppn(appn), contentPage)
		End Sub

		Private Sub SetRoutingFrame(ByVal vars As Dictionary(Of String, String), ByVal appn As String, ByVal frame As String)
			BUDFM_AttributeSupport.SetRoutingParamValue(vars, BUDFM_AttributeSupport.RoutingContentFrameKey, NormalizeRoutingAppn(appn), frame)
		End Sub

		' Legacy dependency note (staged cleanup):
		' This extender still writes a large set of attribute UI params that are OS-suffixed
		' (e.g., prm_BLT_*_OS / prm_NBLT_*_OS). These are not canonical routing keys and are
		' intentionally left unchanged in this pass. Routing keys are now APPN-scoped via helpers.

		' ---- content router + attribute refresh gate (delegates to FERBE_AttributeSupport) ----
		Private Function OnSelectionChanged() As Object
			Dim r As New XFSelectionChangedTaskResult()
			Dim content As String    = args.NameValuePairs.XFGetValue("Content", String.Empty)
			Dim rpNumber As String   = args.NameValuePairs.XFGetValue("RPName", String.Empty)
			Dim wfScenario As String = args.NameValuePairs.XFGetValue("WFScenario", String.Empty)
			Dim wfTime As String     = args.NameValuePairs.XFGetValue("WFTime", String.Empty)
			Dim subcontent As String = args.NameValuePairs.XFGetValue("subcontent", String.Empty)
			Dim readEdit As String   = args.NameValuePairs.XFGetValue("readEdit", String.Empty)
			Dim rpAppr As String     = args.NameValuePairs.XFGetValue("APPN_Content", String.Empty)
			BRApi.State.SetSessionState(si, False, ClientModuleType.Unknown, "", "", "", "", rpNumber, si.XfBytes)
			If Not String.IsNullOrEmpty(rpNumber) AndAlso rpNumber <> "None" Then
				BUDFM_AttributeSupport.SetRPContentRoutingVars(si, globals, r.ModifiedCustomSubstVars, readEdit, content, subcontent, rpAppr, rpNumber, String.Empty, wfScenario, wfTime)
				r.ChangeCustomSubstVarsInDashboard = True
			End If
			Return r
		End Function


		' User-driven refresh: wipe the attribute session cache and re-run the
		' content routing with forceRefresh so every attribute param repopulates
		' straight from the database. Wired to btn_RefreshAttributes_OS.
		Private Function RefreshRPAttributes() As Object
			Dim r As New XFSelectionChangedTaskResult()
			Dim content As String    = args.NameValuePairs.XFGetValue("Content", String.Empty)
			Dim rpNumber As String   = args.NameValuePairs.XFGetValue("RPName", String.Empty)
			Dim wfScenario As String = args.NameValuePairs.XFGetValue("WFScenario", String.Empty)
			Dim wfTime As String     = args.NameValuePairs.XFGetValue("WFTime", String.Empty)
			Dim subcontent As String = args.NameValuePairs.XFGetValue("subcontent", String.Empty)
			Dim readEdit As String   = args.NameValuePairs.XFGetValue("readEdit", String.Empty)
			Dim rpAppr As String     = args.NameValuePairs.XFGetValue("APPN_Content", String.Empty)
			Dim liNumber As String   = args.NameValuePairs.XFGetValue("LINumber", String.Empty)
			BUDFM_AttributeSupport.ClearAttributeCache(si)
			If Not String.IsNullOrEmpty(rpNumber) AndAlso rpNumber <> "None" Then
				BUDFM_AttributeSupport.SetRPContentRoutingVars(si, globals, r.ModifiedCustomSubstVars, readEdit, content, subcontent, rpAppr, rpNumber, liNumber, wfScenario, wfTime, True)
				r.ChangeCustomSubstVarsInDashboard = True
			End If
			r.IsOK = True
			Return r
		End Function

		' ===== ported action functions (TODO: fill bodies from BudFM_SolutionHelper source) =====
		Private Function ClearEXPLine(ByVal rpAppr As String) As Object
			' Variant bodies kept verbatim per appropriation (collapse later
			' only where a diff proves the variants identical).
			Select Case rpAppr
				Case "BS"
					' ==== ported verbatim from BudFM_SolutionHelper.ClearEXPLine_BS ====

		Dim wfTime As String = args.NameValuePairs("WFTime")
		Dim wfScenario As String = args.NameValuePairs("WFScenario")
		Dim wfCube As String = args.NameValuePairs("WFCube")
		Dim RPName As String = args.NameValuePairs("RPName")
		Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
		Dim LineItemNum As String = args.NameValuePairs("LineItemNum") 
		Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
		Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")

		If  String.IsNullOrEmpty (LineItemNum) Then 
			Throw New Exception("Please choose a Line Item") 
		End If
		RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, LineItemNum )

		'Storing the Annotation text for the attributes in a generic string
		Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U7#None:U8#None"		
		Dim scriptGenericsDescr As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_Input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U7#None:U8#None"	
		Dim LineItemNumInt As Integer = LineItemNum.Substring(9,2).XFConvertToInt	

		ClearExpense(si, args, wfScenario, wfCube, wfTime, RP_Entity, rpName, LineItemNum,  LineItemNumInt, scriptGenerics, scriptGenericsDescr)
		
		Dim params As New Dictionary(Of String, String) 
			params.Add("prm_EXP_RequestedItem_Tier1_BS", String.Empty) 		
			params.Add("prm_EXP_Description_Tier2_BS", String.Empty)
			params.Add("prm_EXP_Description_Tier2_Input_BS", String.Empty)
			params.Add("prm_EXP_POC_BS", String.Empty)
			params.Add("prm_EXP_SupportingDoc_BS", String.Empty)
			params.Add("prm_EXP_DollarKValue_BS", String.Empty)
			params.Add("prm_EXP_RecurringNonRecurring_BS", String.Empty)
			params.Add("prm_EXP_ATU_BS", String.Empty)
			params.Add("prm_EXP_PPA_BS", String.Empty)
			params.Add("prm_EXP_UII_BS", String.Empty)
			params.Add("prm_EXP_ObjectClass_BS", String.Empty)
			
		Return SetFieldValues(si,  params ,True, "" & RPName & " " & LineItemNum & " Successfully Cleared")
										
				Case "F"
					' ==== ported verbatim from BudFM_SolutionHelper.ClearEXPLine_F ====
			Dim wfTime As String = args.NameValuePairs("WFTime")
			Dim wfScenario As String = args.NameValuePairs("WFScenario")
			Dim wfCube As String = args.NameValuePairs("WFCube")
			Dim RPName As String = args.NameValuePairs("RPName")
			Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
			Dim LineItemNum As String = args.NameValuePairs("LineItemNum") 
			Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
			Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")

			If  String.IsNullOrEmpty (LineItemNum) Then 
				Throw New Exception("Please choose a Line Item") 
			End If
			RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, LineItemNum )
					
				
			'Storing the Annotation text for the attributes in a generic line item string,( without line number) 
			Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U7#None:U8#None"		
			Dim scriptGenericsDescr As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_Input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U7#None:U8#None"	
			Dim LineItemNumInt As Integer = LineItemNum.Substring(9,2).XFConvertToInt	

			ClearExpense(si, args, wfScenario, wfCube, wfTime, RP_Entity, rpName, LineItemNum,  LineItemNumInt, scriptgenerics, scriptGenericsDescr)
			
			Dim params As New Dictionary(Of String, String) 
				params.Add("prm_EXP_RequestedItem_Tier1_F",		String.Empty) 		
				params.Add("prm_EXP_Description_Tier2_F",		String.Empty)
				params.Add("prm_EXP_Description_Tier2_Input_F", String.Empty)
				params.Add("prm_EXP_POC_F", 					String.Empty)
				params.Add("prm_EXP_SupportingDoc_F", 			String.Empty)
				params.Add("prm_EXP_DollarKValue_F", 			String.Empty)
				params.Add("prm_EXP_BY_Obligations_F", 			String.Empty)
				params.Add("prm_EXP_BY_Plus1_Obligations_F", 	String.Empty)
				params.Add("prm_EXP_BY_Plus2_Obligations_F", 	String.Empty)
				params.Add("prm_EXP_RecurringNonRecurring_F", 	String.Empty)
				params.Add("prm_EXP_ATU_F", 					String.Empty)
				params.Add("prm_EXP_PPA_F", 					String.Empty)
				params.Add("prm_EXP_UII_F", 					String.Empty)
				params.Add("prm_EXP_ObjectClass_F",				String.Empty)
			
			Return SetFieldValues(si,  params ,True, "" & RPName & " " & LineItemNum & " Successfully Cleared")
										
				Case "MERHCF"
					' ==== ported verbatim from BudFM_SolutionHelper.ClearEXPLine_MERHCF ====
		Dim wfTime As String = args.NameValuePairs("WFTime")
		Dim wfScenario As String = args.NameValuePairs("WFScenario")
		Dim wfCube As String = args.NameValuePairs("WFCube")
		Dim RPName As String = args.NameValuePairs("RPName")
		Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
		Dim LineItemNum As String = args.NameValuePairs("LineItemNum") 
		Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
		Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")

		If  String.IsNullOrEmpty (LineItemNum) Then 
			Throw New Exception("Please choose a Line Item") 
		End If
		RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, LineItemNum )

		'Storing the Annotation text for the attributes in a generic string
		Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U7#None:U8#None"		
		Dim scriptGenericsDescr As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_Input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U7#None:U8#None"	
		Dim LineItemNumInt As Integer = LineItemNum.Substring(9,2).XFConvertToInt	

		ClearExpense(si, args, wfScenario, wfCube, wfTime, RP_Entity, rpName, LineItemNum,  LineItemNumInt, scriptGenerics, scriptGenericsDescr)
		
		Dim params As New Dictionary(Of String, String) 
			params.Add("prm_EXP_RequestedItem_Tier1_BS", String.Empty) 		
			params.Add("prm_EXP_Description_Tier2_BS", String.Empty)
			params.Add("prm_EXP_Description_Tier2_Input_BS", String.Empty)
			params.Add("prm_EXP_POC_BS", String.Empty)
			params.Add("prm_EXP_SupportingDoc_BS", String.Empty)
			params.Add("prm_EXP_DollarKValue_BS", String.Empty)
			params.Add("prm_EXP_RecurringNonRecurring_BS", String.Empty)
			params.Add("prm_EXP_ATU_BS", String.Empty)
			params.Add("prm_EXP_PPA_BS", String.Empty)
			params.Add("prm_EXP_UII_BS", String.Empty)
			params.Add("prm_EXP_ObjectClass_BS", String.Empty)
			
		Return SetFieldValues(si,  params ,True, "" & RPName & " " & LineItemNum & " Successfully Cleared")
								
				Case "MOSP"
					' ==== ported verbatim from BudFM_SolutionHelper.ClearEXPLine_MOSP ====

		Dim wfTime As String = args.NameValuePairs("WFTime")
		Dim wfScenario As String = args.NameValuePairs("WFScenario")
		Dim wfCube As String = args.NameValuePairs("WFCube")
		Dim RPName As String = args.NameValuePairs("RPName")
		Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
		Dim LineItemNum As String = args.NameValuePairs("LineItemNum") 
		Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
		Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")

		If  String.IsNullOrEmpty (LineItemNum) Then 
			Throw New Exception("Please choose a Line Item") 
		End If
		RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, LineItemNum )

		'Storing the Annotation text for the attributes in a generic string
		Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U7#None:U8#None"		
		Dim scriptGenericsDescr As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_Input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U7#None:U8#None"	
		Dim LineItemNumInt As Integer = LineItemNum.Substring(9,2).XFConvertToInt	

		ClearExpense(si, args, wfScenario, wfCube, wfTime, RP_Entity, rpName, LineItemNum,  LineItemNumInt, scriptGenerics, scriptGenericsDescr)
		
		Dim params As New Dictionary(Of String, String) 
			params.Add("prm_EXP_RequestedItem_Tier1_MOSP", String.Empty) 		
			params.Add("prm_EXP_Description_Tier2_MOSP", String.Empty)
			params.Add("prm_EXP_Description_Tier2_Input_MOSP", String.Empty)
			params.Add("prm_EXP_POC_MOSP", String.Empty)
			params.Add("prm_EXP_SupportingDoc_MOSP", String.Empty)
			params.Add("prm_EXP_DollarKValue_MOSP", String.Empty)
			params.Add("prm_EXP_RecurringNonRecurring_MOSP", String.Empty)
			params.Add("prm_EXP_ATU_MOSP", String.Empty)
			params.Add("prm_EXP_PPA_MOSP", String.Empty)
			params.Add("prm_EXP_UII_MOSP", String.Empty)
			params.Add("prm_EXP_ObjectClass_MOSP", String.Empty)
			
		Return SetFieldValues(si,  params ,True, "" & RPName & " " & LineItemNum & " Successfully Cleared")
		
				Case "PCI"
					' ==== ported verbatim from BudFM_SolutionHelper.ClearEXPLine_PCI ====

			Dim wfTime As String = args.NameValuePairs("WFTime")
			Dim wfScenario As String = args.NameValuePairs("WFScenario")
			Dim wfCube As String = args.NameValuePairs("WFCube")
			Dim RPName As String = args.NameValuePairs("RPName")
			Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
			Dim LineItemNum As String = args.NameValuePairs("LineItemNum") 
			Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
			Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")

			If  String.IsNullOrEmpty (LineItemNum) Then 
				Throw New Exception("Please choose a Line Item") 
			End If
			RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, LineItemNum)
					
			'Storing the Annotation text for the attributes in a generic line item string,( without line number) 
			Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U7#None:U8#None"		
			Dim scriptGenericsDescr As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_Input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U7#None:U8#None"	
			Dim LineItemNumInt As Integer = LineItemNum.Substring(9,2).XFConvertToInt	


			ClearExpense_PCI(si, args, wfScenario, wfCube, wfTime, RP_Entity, rpName, LineItemNum,  LineItemNumInt, scriptgenerics, scriptGenericsDescr)
			
			Dim params As New Dictionary(Of String, String) 
				params.Add("prm_EXP_RequestedItem_Tier1_PCI",	 String.Empty) 		
				params.Add("prm_EXP_Description_Tier2_PCI",		 String.Empty)
				params.Add("prm_EXP_Description_Tier2_Input_PCI", String.Empty)
				params.Add("prm_EXP_POC_PCI", 					 String.Empty)
				params.Add("prm_EXP_SupportingDoc_PCI", 			 String.Empty)
				params.Add("prm_EXP_DollarKValue_PCI", 			 String.Empty)
				params.Add("prm_EXP_BY_Obligations_PCI", 		 String.Empty)
				params.Add("prm_EXP_BY_Plus1_Obligations_PCI", 	 String.Empty)
				params.Add("prm_EXP_BY_Plus2_Obligations_PCI", 	 String.Empty)
				params.Add("prm_EXP_BY_Plus3_Obligations_PCI", 	 String.Empty)
				params.Add("prm_EXP_BY_Plus4_Obligations_PCI", 	 String.Empty)
				params.Add("prm_EXP_RecurringNonRecurring_PCI", 	 String.Empty)
				params.Add("prm_EXP_ATU_PCI", 					 String.Empty)
				params.Add("prm_EXP_PPA_Selection_PCI", 			 String.Empty)
				params.Add("prm_EXP_UII_PCI", 					 String.Empty)
				params.Add("prm_EXP_ObjectClass_PCI",			 String.Empty)
			
			Return SetFieldValues(si,  params ,True, "" & RPName & " " & LineItemNum & " Successfully Cleared")
										
				Case "RD"
					' ==== ported verbatim from BudFM_SolutionHelper.ClearEXPLine_RD ====
			Dim wfTime As String = args.NameValuePairs("WFTime")
			Dim wfScenario As String = args.NameValuePairs("WFScenario")
			Dim wfCube As String = args.NameValuePairs("WFCube")
			Dim RPName As String = args.NameValuePairs("RPName")
			Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
			Dim LineItemNum As String = args.NameValuePairs("LineItemNum") 
			Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
			Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")

			If  String.IsNullOrEmpty (LineItemNum) Then 
				Throw New Exception("Please choose a Line Item") 
			End If
			
			RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, LineItemNum )

			'Storing the Annotation text for the attributes in a generic line item string,( without line number) 
			Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U7#None:U8#None"		
			Dim scriptGenericsDescr As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_Input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U7#None:U8#None"	
			Dim LineItemNumInt As Integer = LineItemNum.Substring(9,2).XFConvertToInt	

			ClearExpense_RD(si, args, wfScenario, wfCube, wfTime, RP_Entity, rpName, LineItemNum,  LineItemNumInt, scriptgenerics, scriptGenericsDescr)
			
			Dim params As New Dictionary(Of String, String) 
				params.Add("prm_EXP_RequestedItem_Tier1_RD",		String.Empty) 		
				params.Add("prm_EXP_Description_Tier2_RD",		String.Empty)
				params.Add("prm_EXP_Description_Tier2_Input_RD", String.Empty)
				params.Add("prm_EXP_POC_RD", 					String.Empty)
				params.Add("prm_EXP_SupportingDoc_RD", 			String.Empty)
				params.Add("prm_EXP_DollarKValue_RD", 			String.Empty)
				params.Add("prm_EXP_BY_Obligations_RD", 			String.Empty)
				params.Add("prm_EXP_BY_Plus1_Obligations_RD", 	String.Empty)
				params.Add("prm_EXP_BY_Plus2_Obligations_RD", 	String.Empty)
				params.Add("prm_EXP_RecurringNonRecurring_RD", 	String.Empty)
				params.Add("prm_EXP_ATU_RD", 					String.Empty)
				params.Add("prm_EXP_PPA_RD", 					String.Empty)
				params.Add("prm_EXP_UII_RD", 					String.Empty)
				params.Add("prm_EXP_ObjectClass_RD",				String.Empty)
			
			Return SetFieldValues(si,  params ,True, "" & RPName & " " & LineItemNum & " Successfully Cleared")
										
				Case "RP"
					' ==== ported verbatim from BudFM_SolutionHelper.ClearEXPLine_RP ====
		Dim wfTime As String = args.NameValuePairs("WFTime")
		Dim wfScenario As String = args.NameValuePairs("WFScenario")
		Dim wfCube As String = args.NameValuePairs("WFCube")
		Dim RPName As String = args.NameValuePairs("RPName")
		Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
		Dim LineItemNum As String = args.NameValuePairs("LineItemNum") 
		Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
		Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")

		If  String.IsNullOrEmpty (LineItemNum) Then 
			Throw New Exception("Please choose a Line Item") 
		End If
		RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, LineItemNum )

		'Storing the Annotation text for the attributes in a generic string
		Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U7#None:U8#None"		
		Dim scriptGenericsDescr As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_Input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U7#None:U8#None"
		Dim LineItemNumInt As Integer = LineItemNum.Substring(9,2).XFConvertToInt	

		ClearExpense(si, args, wfScenario, wfCube, wfTime, RP_Entity, rpName, LineItemNum,  LineItemNumInt, scriptGenerics, scriptGenericsDescr)
		
		Dim params As New Dictionary(Of String, String) 
			params.Add("prm_EXP_RequestedItem_Tier1_RP", String.Empty) 		
			params.Add("prm_EXP_Description_Tier2_RP", String.Empty)
			params.Add("prm_EXP_Description_Tier2_Input_RP", String.Empty)
			params.Add("prm_EXP_POC_RP", String.Empty)
			params.Add("prm_EXP_SupportingDoc_RP", String.Empty)
			params.Add("prm_EXP_DollarKValue_RP", String.Empty)
			params.Add("prm_EXP_RecurringNonRecurring_RP", String.Empty)
			params.Add("prm_EXP_ATU_RP", String.Empty)
			params.Add("prm_EXP_PPA_RP", String.Empty)
			params.Add("prm_EXP_UII_RP", String.Empty)
			params.Add("prm_EXP_ObjectClass_RP", String.Empty)
			
		Return SetFieldValues(si,  params ,True, "" & RPName & " " & LineItemNum & " Successfully Cleared")
										
				Case Else
					Throw New XFException(si, New Exception("ClearEXPLine: unknown appropriation '" & rpAppr & "'"))
			End Select
			Return Nothing
		End Function
		Private Function DeleteEXPLine(ByVal rpAppr As String) As Object
			' Variant bodies kept verbatim per appropriation (collapse later
			' only where a diff proves the variants identical).
			Select Case rpAppr
				Case "BS"
					' ==== ported verbatim from BudFM_SolutionHelper.DeleteEXPLine_BS ====
		Dim wfTime As String = args.NameValuePairs("WFTime")
		Dim wfScenario As String = args.NameValuePairs("WFScenario")
		Dim wfCube As String = args.NameValuePairs("WFCube")
		Dim RPName As String = args.NameValuePairs("RPName")
		Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
		Dim LineItemNum As String = args.NameValuePairs("LineItemNum") 
		Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
		Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")

		If  String.IsNullOrEmpty (LineItemNum) Then 
			Throw New Exception("Please choose a Line Item") 
		End If
		RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, LineItemNum )

		Dim LineItemNumInt As Integer = LineItemNum.Substring(12,2).XFConvertToInt	
		Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U7#None:U8#None"			
		Dim scriptGenericsDescr As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_Input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U7#None:U8#None"	
		Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
						
		Dim std_LineItemsDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "Std_LineItems")
		Dim total_Expense_Line_ItemsId As Integer = BRApi.Finance.Members.GetMemberId(si, dimtypeId.UD6, "Total_Expense_LineItems")
		Dim number_of_Expenses As Integer = 0
		
		
		'Find number of Expenses
		Dim ud6LineItemMems As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si,std_LineItemsDimPk, total_Expense_Line_ItemsId, Nothing)
		If Not ud6lineItemMems Is Nothing Then
			For Each ud6objLineItem As Member In ud6LineItemMems
				'Get the Line Item member Name
				Dim ud6LineItemName As String = ud6objLineItem.Name	
				Dim objDataCellInfoUsingMemberScript As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si,wfCube,"A#Requested_Item_Tier1:" & scriptGenerics &":U6#" & ud6LineItemName)
				Dim requested_Item_Tier1 As String = objDataCellInfoUsingMemberScript.DataCellEx.DataCellAnnotation
					
				If (Not requested_Item_Tier1.XFEqualsIgnoreCase("")) Then	
					number_of_Expenses = number_of_Expenses+1
				End If							
			Next
		End If
		
		If number_of_Expenses <> 1 AndAlso  number_of_Expenses > LineItemNumInt  Then	
			
			Dim strExpensesMax As String = number_of_Expenses.ToString()
			If number_of_Expenses <10 Then 
				strExpensesMax = "0"&number_of_Expenses.ToString()
			End If
			
			'clear current expense
			ClearExpense(si, args, wfScenario, wfCube, wfTime, RP_Entity, rpName, LineItemNum,  LineItemNumInt, scriptGenerics, scriptGenericsDescr)	
			
			'copy from one plus
			Do
				Dim strLineItem As String = LineItemNumInt.ToString()
				Dim strLineItemplusone As String = LineItemNumInt+1.ToString()
				If LineItemNumInt <10 Then 
					strLineItem = "0"&LineItemNumInt.ToString()
				End If
				If LineItemNumInt+1 <10 Then 
					strLineItemplusone = "0"&LineItemNumInt+1.ToString()
				End If

				CopyExpenseAllFields( si, args, wfCube, wfTime, wfScenario, RP_Entity, rpName, "ExpLineItem_" & strLineItemplusone, "ExpLineItem_" & strLineItem )
				'brapi.ErrorLog.LogMessage(si, "copy " & strLineItemplusone & " to " & strLineItem)
				LineItemNumInt=LineItemNumInt+1
				
			Loop While LineItemNumInt <> number_of_Expenses
			
			'clear last expense
			
			ClearExpense(si, args, wfScenario, wfCube, wfTime, RP_Entity, rpName, "ExpLineItem_" & strExpensesMax,  number_of_Expenses, scriptGenerics, scriptGenericsDescr)	
			
		Else If  number_of_Expenses < LineItemNumInt  Then
			Throw New Exception("Cannot delete Line Item")
		Else 'number_of_Expenses = 1 so just clear it

			ClearExpense(si, args, wfScenario, wfCube, wfTime, RP_Entity, rpName, LineItemNum,  LineItemNumInt, scriptGenerics, scriptGenericsDescr)	
		End If
				
		selectionChangedTaskResult = Me.RefreshSelectedLineItem_BS(si, wfCube, wfTime, wfScenario, RPName, LineItemNum )
		Return selectionChangedTaskResult

				Case "F"
					' ==== ported verbatim from BudFM_SolutionHelper.DeleteEXPLine_F ====
			Dim wfTime As String = args.NameValuePairs("WFTime")
			Dim wfScenario As String = args.NameValuePairs("WFScenario")
			Dim wfCube As String = args.NameValuePairs("WFCube")
			Dim RPName As String = args.NameValuePairs("RPName")
			Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
			Dim LineItemNum As String = args.NameValuePairs("LineItemNum") 
			Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
			Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")

			If  String.IsNullOrEmpty (LineItemNum) Then 
				Throw New Exception("Please choose a Line Item") 
			End If
			RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, LineItemNum )

			Dim number_of_Expenses As Integer = 0
			Dim LineItemNumInt As Integer = LineItemNum.Substring(12,2).XFConvertToInt	
			
			' Form script generics string without line number 
			Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U7#None:U8#None"			
			Dim scriptGenericsDescr As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_Input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U7#None:U8#None"	
								
			Dim std_LineItemsDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "Std_LineItems")
			Dim total_Expense_Line_ItemsId As Integer = BRApi.Finance.Members.GetMemberId(si, dimtypeId.UD6, "Total_Expense_LineItems")
			
			
			'Find number of Expenses
			Dim ud6LineItemMems As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si,std_LineItemsDimPk, total_Expense_Line_ItemsId, Nothing)
			If Not ud6lineItemMems Is Nothing Then
				For Each ud6objLineItem As Member In ud6LineItemMems
					'Get the Line Item member Name
					Dim ud6LineItemName As String = ud6objLineItem.Name	
					Dim objDataCellInfoUsingMemberScript As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si,wfCube,"A#Requested_Item_Tier1:" & scriptGenerics &":U6#" & ud6LineItemName)
					Dim requested_Item_Tier1 As String = objDataCellInfoUsingMemberScript.DataCellEx.DataCellAnnotation
						
					If (Not requested_Item_Tier1.XFEqualsIgnoreCase("")) Then	
						number_of_Expenses = number_of_Expenses+1
					End If							
				Next
			End If
			
			If number_of_Expenses <> 1 AndAlso  number_of_Expenses > LineItemNumInt  Then	
				
				Dim strExpensesMax As String = number_of_Expenses.ToString()
				If number_of_Expenses <10 Then 
					strExpensesMax = "0"&number_of_Expenses.ToString()
				End If
				
				'clear current expense
				ClearExpense(si, args, wfScenario, wfCube, wfTime, RP_Entity, rpName, LineItemNum,  LineItemNumInt, scriptGenerics, scriptGenericsDescr)	
				'copy from one plus
				Do
					Dim strLineItem As String = LineItemNumInt.ToString()
					Dim strLineItemplusone As String = LineItemNumInt+1.ToString()
					If LineItemNumInt <10 Then 
						strLineItem = "0"&LineItemNumInt.ToString()
					End If
					If LineItemNumInt+1 <10 Then 
						strLineItemplusone = "0"&LineItemNumInt+1.ToString()
					End If

					CopyExpenseAllFields( si, args, wfCube, wfTime, wfScenario, RP_Entity, rpName, "ExpLineItem_" & strLineItemplusone, "ExpLineItem_" & strLineItem )
	
					LineItemNumInt=LineItemNumInt+1
					
				Loop While LineItemNumInt <> number_of_Expenses
				
				'clear last expense				
				ClearExpense(si, args, wfScenario, wfCube, wfTime, RP_Entity, rpName, "ExpLineItem_" & strExpensesMax,  number_of_Expenses, scriptGenerics, scriptGenericsDescr)	
				
			Else If  number_of_Expenses < LineItemNumInt  Then
				Throw New Exception("Cannot delete Line Item")
			Else 'number_of_Expenses = 1 so just clear it

				ClearExpense(si, args, wfScenario, wfCube, wfTime, RP_Entity, rpName, LineItemNum,  LineItemNumInt, scriptGenerics, scriptGenericsDescr)	
			End If
			
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
			selectionChangedTaskResult = Me.RefreshSelectedLineItem_F(si, wfCube, wfTime, wfScenario, RPName, LineItemNum )
			Return selectionChangedTaskResult

				Case "MERHCF"
					' ==== ported verbatim from BudFM_SolutionHelper.DeleteEXPLine_MERHCF ====
		Dim wfTime As String = args.NameValuePairs("WFTime")
		Dim wfScenario As String = args.NameValuePairs("WFScenario")
		Dim wfCube As String = args.NameValuePairs("WFCube")
		Dim RPName As String = args.NameValuePairs("RPName")
		Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
		Dim LineItemNum As String = args.NameValuePairs("LineItemNum") 
		Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
		Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")

		If  String.IsNullOrEmpty (LineItemNum) Then 
			Throw New Exception("Please choose a Line Item") 
		End If
		RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, LineItemNum )

		Dim number_of_Expenses As Integer = 0
		Dim LineItemNumInt As Integer = LineItemNum.Substring(12,2).XFConvertToInt	
		Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U7#None:U8#None"			
		Dim scriptGenericsDescr As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_Input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U7#None:U8#None"	
						
		Dim std_LineItemsDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "Std_LineItems")
		Dim total_Expense_Line_ItemsId As Integer = BRApi.Finance.Members.GetMemberId(si, dimtypeId.UD6, "Total_Expense_LineItems")
		
		
		'Find number of Expenses
		Dim ud6LineItemMems As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si,std_LineItemsDimPk, total_Expense_Line_ItemsId, Nothing)
		If Not ud6lineItemMems Is Nothing Then
			For Each ud6objLineItem As Member In ud6LineItemMems
				'Get the Line Item member Name
				Dim ud6LineItemName As String = ud6objLineItem.Name	
				Dim objDataCellInfoUsingMemberScript As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si,wfCube,"A#Requested_Item_Tier1:" & scriptGenerics &":U6#" & ud6LineItemName)
				Dim requested_Item_Tier1 As String = objDataCellInfoUsingMemberScript.DataCellEx.DataCellAnnotation
					
				If (Not requested_Item_Tier1.XFEqualsIgnoreCase("")) Then	
					number_of_Expenses = number_of_Expenses+1
				End If							
			Next
		End If

		If number_of_Expenses <> 1 AndAlso  number_of_Expenses > LineItemNumInt  Then	
			
			Dim strExpensesMax As String = number_of_Expenses.ToString()
			If number_of_Expenses <10 Then 
				strExpensesMax = "0"&number_of_Expenses.ToString()
			End If
			
			'clear current expense
			ClearExpense(si, args, wfScenario, wfCube, wfTime, RP_Entity, rpName, LineItemNum,  LineItemNumInt, scriptGenerics, scriptGenericsDescr)	
			
			'copy from one plus
			Do
				Dim strLineItem As String = LineItemNumInt.ToString()
				Dim strLineItemplusone As String = LineItemNumInt+1.ToString()
				If LineItemNumInt <10 Then 
					strLineItem = "0"&LineItemNumInt.ToString()
				End If
				If LineItemNumInt+1 <10 Then 
					strLineItemplusone = "0"&LineItemNumInt+1.ToString()
				End If

				CopyExpenseAllFields( si, args, wfCube, wfTime, wfScenario, RP_Entity, rpName, "ExpLineItem_" & strLineItemplusone, "ExpLineItem_" & strLineItem )
				'brapi.ErrorLog.LogMessage(si, "copy " & strLineItemplusone & " to " & strLineItem)
				LineItemNumInt=LineItemNumInt+1
				
			Loop While LineItemNumInt <> number_of_Expenses
			
			'clear last expense
			
			ClearExpense(si, args, wfScenario, wfCube, wfTime, RP_Entity, rpName, "ExpLineItem_" & strExpensesMax,  number_of_Expenses, scriptGenerics, scriptGenericsDescr)	
			
		Else If  number_of_Expenses < LineItemNumInt  Then
			Throw New Exception("Cannot delete Line Item")
		Else 'number_of_Expenses = 1 so just clear it

			ClearExpense(si, args, wfScenario, wfCube, wfTime, RP_Entity, rpName, LineItemNum,  LineItemNumInt, scriptGenerics, scriptGenericsDescr)	
		End If

		Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
		selectionChangedTaskResult = Me.RefreshSelectedLineItem_MERHCF(si, wfCube, wfTime, wfScenario, RPName, LineItemNum )
		Return selectionChangedTaskResult

'		Dim params As New Dictionary(Of String, String) 
'			params.Add("prm_EXP_RequestedItem_Tier1_BS", String.Empty) 		
'			params.Add("prm_EXP_Description_Tier2_BS", String.Empty)
'			params.Add("prm_EXP_Description_Tier2_Input_BS", String.Empty)
'			params.Add("prm_EXP_POC_BS", String.Empty)
'			params.Add("prm_EXP_SupportingDoc_BS", String.Empty)
'			params.Add("prm_EXP_DollarKValue_BS", String.Empty)
'			params.Add("prm_EXP_RecurringNonRecurring_BS", String.Empty)
'			params.Add("prm_EXP_ATU_BS", String.Empty)
'			params.Add("prm_EXP_PPA_BS", String.Empty)
'			params.Add("prm_EXP_UII_BS", String.Empty)
'			params.Add("prm_EXP_ObjectClass_BS", String.Empty)			
'		Return SetFieldValues(si,  params ,True, "" & RPName & " " & LineItemNum & " Successfully Cleared")	
				Case "MOSP"
					' ==== ported verbatim from BudFM_SolutionHelper.DeleteEXPLine_MOSP ====
		Dim wfTime As String = args.NameValuePairs("WFTime")
		Dim wfScenario As String = args.NameValuePairs("WFScenario")
		Dim wfCube As String = args.NameValuePairs("WFCube")
		Dim RPName As String = args.NameValuePairs("RPName")
		Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
		Dim LineItemNum As String = args.NameValuePairs("LineItemNum") 
		Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
		Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")

		If  String.IsNullOrEmpty (LineItemNum) Then 
			Throw New Exception("Please choose a Line Item") 
		End If
		RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, LineItemNum )

		Dim LineItemNumInt As Integer = LineItemNum.Substring(12,2).XFConvertToInt	
		Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U7#None:U8#None"			
		Dim scriptGenericsDescr As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_Input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U7#None:U8#None"	
						
		Dim std_LineItemsDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "Std_LineItems")
		Dim total_Expense_Line_ItemsId As Integer = BRApi.Finance.Members.GetMemberId(si, dimtypeId.UD6, "Total_Expense_LineItems")
		Dim number_of_Expenses As Integer = 0
		
		
		'Find number of Expenses
		Dim ud6LineItemMems As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si,std_LineItemsDimPk, total_Expense_Line_ItemsId, Nothing)
		If Not ud6lineItemMems Is Nothing Then
			For Each ud6objLineItem As Member In ud6LineItemMems
				'Get the Line Item member Name
				Dim ud6LineItemName As String = ud6objLineItem.Name	
				Dim objDataCellInfoUsingMemberScript As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si,wfCube,"A#Requested_Item_Tier1:" & scriptGenerics &":U6#" & ud6LineItemName)
				Dim requested_Item_Tier1 As String = objDataCellInfoUsingMemberScript.DataCellEx.DataCellAnnotation
					
				If (Not requested_Item_Tier1.XFEqualsIgnoreCase("")) Then	
					number_of_Expenses = number_of_Expenses+1
				End If							
			Next
		End If
		
		If number_of_Expenses <> 1 AndAlso  number_of_Expenses > LineItemNumInt  Then	
			
			Dim strExpensesMax As String = number_of_Expenses.ToString()
			If number_of_Expenses <10 Then 
				strExpensesMax = "0"&number_of_Expenses.ToString()
			End If
			
			'clear current expense
			ClearExpense(si, args, wfScenario, wfCube, wfTime, RP_Entity, rpName, LineItemNum,  LineItemNumInt, scriptGenerics, scriptGenericsDescr)	
			
			'copy from one plus
			Do
				Dim strLineItem As String = LineItemNumInt.ToString()
				Dim strLineItemplusone As String = LineItemNumInt+1.ToString()
				If LineItemNumInt <10 Then 
					strLineItem = "0"&LineItemNumInt.ToString()
				End If
				If LineItemNumInt+1 <10 Then 
					strLineItemplusone = "0"&LineItemNumInt+1.ToString()
				End If

				CopyExpenseAllFields( si, args, wfCube, wfTime, wfScenario, RP_Entity, rpName, "ExpLineItem_" & strLineItemplusone, "ExpLineItem_" & strLineItem )
				'brapi.ErrorLog.LogMessage(si, "copy " & strLineItemplusone & " to " & strLineItem)
				LineItemNumInt=LineItemNumInt+1
				
			Loop While LineItemNumInt <> number_of_Expenses
			
			'clear last expense
			
			ClearExpense(si, args, wfScenario, wfCube, wfTime, RP_Entity, rpName, "ExpLineItem_" & strExpensesMax,  number_of_Expenses, scriptGenerics, scriptGenericsDescr)	
			
		Else If  number_of_Expenses < LineItemNumInt  Then
			Throw New Exception("Cannot delete Line Item")
		Else 'number_of_Expenses = 1 so just clear it

			ClearExpense(si, args, wfScenario, wfCube, wfTime, RP_Entity, rpName, LineItemNum,  LineItemNumInt, scriptGenerics, scriptGenericsDescr)	
		End If
		
		Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
		selectionChangedTaskResult = Me.RefreshSelectedLineItem_MOSP(si, wfCube, wfTime, wfScenario, RPName, LineItemNum )
		Return selectionChangedTaskResult

'		Dim params As New Dictionary(Of String, String) 
'			params.Add("prm_EXP_RequestedItem_Tier1_MOSP", String.Empty) 		
'			params.Add("prm_EXP_Description_Tier2_MOSP", String.Empty)
'			params.Add("prm_EXP_Description_Tier2_Input_MOSP", String.Empty)
'			params.Add("prm_EXP_POC_MOSP", String.Empty)
'			params.Add("prm_EXP_SupportingDoc_MOSP", String.Empty)
'			params.Add("prm_EXP_DollarKValue_MOSP", String.Empty)
'			params.Add("prm_EXP_RecurringNonRecurring_MOSP", String.Empty)
'			params.Add("prm_EXP_ATU_MOSP", String.Empty)
'			params.Add("prm_EXP_PPA_MOSP", String.Empty)
'			params.Add("prm_EXP_UII_MOSP", String.Empty)
'			params.Add("prm_EXP_ObjectClass_MOSP", String.Empty)			
'		Return SetFieldValues(si,  params ,True, "" & RPName & " " & LineItemNum & " Successfully Cleared")	
	
				Case "PCI"
					' ==== ported verbatim from BudFM_SolutionHelper.DeleteEXPLine_PCI ====
			Dim wfTime As String = args.NameValuePairs("WFTime")
			Dim wfScenario As String = args.NameValuePairs("WFScenario")
			Dim wfCube As String = args.NameValuePairs("WFCube")
			Dim RPName As String = args.NameValuePairs("RPName")
			Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
			Dim LineItemNum As String = args.NameValuePairs("LineItemNum") 
			Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
			Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")

			If  String.IsNullOrEmpty (LineItemNum) Then 
				Throw New Exception("Please choose a Line Item") 
			End If
			RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, LineItemNum)

			Dim LineItemNumInt As Integer = LineItemNum.Substring(12,2).XFConvertToInt	
			
			' Form script generics string without line number 
			Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U7#None:U8#None"			
			Dim scriptGenericsDescr As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_Input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U7#None:U8#None"	
								
			Dim std_LineItemsDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "Std_LineItems")
			Dim total_Expense_Line_ItemsId As Integer = BRApi.Finance.Members.GetMemberId(si, dimtypeId.UD6, "Total_Expense_LineItems")
			
			
			'Find number of Expenses
			Dim number_of_Expenses As Integer = 0
			Dim ud6LineItemMems As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si,std_LineItemsDimPk, total_Expense_Line_ItemsId, Nothing)
			If Not ud6lineItemMems Is Nothing Then
				For Each ud6objLineItem As Member In ud6LineItemMems
					'Get the Line Item member Name
					Dim ud6LineItemName As String = ud6objLineItem.Name	
					Dim objDataCellInfoUsingMemberScript As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si,wfCube,"A#Requested_Item_Tier1:" & scriptGenerics &":U6#" & ud6LineItemName)
					Dim requested_Item_Tier1 As String = objDataCellInfoUsingMemberScript.DataCellEx.DataCellAnnotation
						
					If (Not requested_Item_Tier1.XFEqualsIgnoreCase("")) Then	
						number_of_Expenses = number_of_Expenses+1
					End If							
				Next
			End If
			
			If number_of_Expenses <> 1 AndAlso  number_of_Expenses > LineItemNumInt  Then	
				
				Dim strExpensesMax As String = number_of_Expenses.ToString()
				If number_of_Expenses <10 Then 
					strExpensesMax = "0"&number_of_Expenses.ToString()
				End If
				
				'clear current expense
				ClearExpense_PCI(si, args, wfScenario, wfCube, wfTime, RP_Entity, rpName, LineItemNum,  LineItemNumInt, scriptGenerics, scriptGenericsDescr)
				
				Do
					Dim strLineItem As String = LineItemNumInt.ToString()
					Dim strLineItemplusone As String = LineItemNumInt+1.ToString()
					If LineItemNumInt <10 Then 
						strLineItem = "0"&LineItemNumInt.ToString()
					End If
					If LineItemNumInt+1 <10 Then 
						strLineItemplusone = "0"&LineItemNumInt+1.ToString()
					End If

					CopyExpenseAllFields_PCI(si, args, wfCube, wfTime, wfScenario, RP_Entity, rpName, "ExpLineItem_" & strLineItemplusone, "ExpLineItem_" & strLineItem )
	
					LineItemNumInt=LineItemNumInt+1
					
				Loop While LineItemNumInt <> number_of_Expenses
				
				'clear last expense				
				ClearExpense_PCI(si, args, wfScenario, wfCube, wfTime, RP_Entity, rpName, "ExpLineItem_" & strExpensesMax,  number_of_Expenses, scriptGenerics, scriptGenericsDescr)	
				
			Else If  number_of_Expenses < LineItemNumInt  Then
				Throw New Exception("Cannot delete Line Item")
			Else 'number_of_Expenses = 1 so just clear it

				ClearExpense_PCI(si, args, wfScenario, wfCube, wfTime, RP_Entity, rpName, LineItemNum,  LineItemNumInt, scriptGenerics, scriptGenericsDescr)	
			End If
			
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
			selectionChangedTaskResult = Me.RefreshSelectedLineItem_PCI(si, wfCube, wfTime, wfScenario, RPName, LineItemNum )
			Return selectionChangedTaskResult

				Case "RD"
					' ==== ported verbatim from BudFM_SolutionHelper.DeleteEXPLine_RD ====
			Dim wfTime As String = args.NameValuePairs("WFTime")
			Dim wfScenario As String = args.NameValuePairs("WFScenario")
			Dim wfCube As String = args.NameValuePairs("WFCube")
			Dim RPName As String = args.NameValuePairs("RPName")
			Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
			Dim LineItemNum As String = args.NameValuePairs("LineItemNum") 
			Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
			Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")

			If  String.IsNullOrEmpty (LineItemNum) Then 
				Throw New Exception("Please choose a Line Item") 
			End If
			
			RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, LineItemNum )

			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()

			
			' Form script generics string without line number 
			Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U7#None:U8#None"			
			Dim scriptGenericsDescr As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_Input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U7#None:U8#None"	
								
			Dim std_LineItemsDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "Std_LineItems")
			Dim total_Expense_Line_ItemsId As Integer = BRApi.Finance.Members.GetMemberId(si, dimtypeId.UD6, "Total_Expense_LineItems")
			
			
			'Find number of Expenses
			Dim number_of_Expenses As Integer = 0
			Dim ud6LineItemMems As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si,std_LineItemsDimPk, total_Expense_Line_ItemsId, Nothing)
			If Not ud6lineItemMems Is Nothing Then
				For Each ud6objLineItem As Member In ud6LineItemMems
					'Get the Line Item member Name
					Dim ud6LineItemName As String = ud6objLineItem.Name	
					Dim objDataCellInfoUsingMemberScript As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si,wfCube,"A#Requested_Item_Tier1:" & scriptGenerics &":U6#" & ud6LineItemName)
					Dim requested_Item_Tier1 As String = objDataCellInfoUsingMemberScript.DataCellEx.DataCellAnnotation
						
					If (Not requested_Item_Tier1.XFEqualsIgnoreCase("")) Then	
						number_of_Expenses = number_of_Expenses+1
					End If							
				Next
			End If
			
			Dim LineItemNumInt As Integer = LineItemNum.Substring(12,2).XFConvertToInt	

			If number_of_Expenses <> 1 AndAlso  number_of_Expenses > LineItemNumInt  Then	
				
				Dim strExpensesMax As String = number_of_Expenses.ToString()
				If number_of_Expenses <10 Then 
					strExpensesMax = "0"&number_of_Expenses.ToString()
				End If
				
				'clear current expense
				ClearExpense_RD(si, args, wfScenario, wfCube, wfTime, RP_Entity, rpName, LineItemNum,  LineItemNumInt, scriptGenerics, scriptGenericsDescr)	
				'copy from one plus
				Do
					Dim strLineItem As String = LineItemNumInt.ToString()
					Dim strLineItemplusone As String = LineItemNumInt+1.ToString()
					If LineItemNumInt <10 Then 
						strLineItem = "0"&LineItemNumInt.ToString()
					End If
					If LineItemNumInt+1 <10 Then 
						strLineItemplusone = "0"&LineItemNumInt+1.ToString()
					End If

					CopyExpenseAllFields_RD( si, args, wfCube, wfTime, wfScenario, RP_Entity, rpName, "ExpLineItem_" & strLineItemplusone, "ExpLineItem_" & strLineItem )
	
					LineItemNumInt=LineItemNumInt+1
					
				Loop While LineItemNumInt <> number_of_Expenses
				
				'clear last expense				
				ClearExpense_RD(si, args, wfScenario, wfCube, wfTime, RP_Entity, rpName, "ExpLineItem_" & strExpensesMax,  number_of_Expenses, scriptGenerics, scriptGenericsDescr)	
				
			Else If  number_of_Expenses < LineItemNumInt  Then
				Throw New Exception("Cannot delete Line Item")
			Else 'number_of_Expenses = 1 so just clear it

				ClearExpense_RD(si, args, wfScenario, wfCube, wfTime, RP_Entity, rpName, LineItemNum,  LineItemNumInt, scriptGenerics, scriptGenericsDescr)	
			End If
			
			selectionChangedTaskResult = Me.RefreshSelectedLineItem_RD(si, wfCube, wfTime, wfScenario, RPName, LineItemNum )
			Return selectionChangedTaskResult

				Case "RP"
					' ==== ported verbatim from BudFM_SolutionHelper.DeleteEXPLine_RP ====

		Dim wfTime As String = args.NameValuePairs("WFTime")
		Dim wfScenario As String = args.NameValuePairs("WFScenario")
		Dim wfCube As String = args.NameValuePairs("WFCube")
		Dim RPName As String = args.NameValuePairs("RPName")
		Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
		Dim LineItemNum As String = args.NameValuePairs("LineItemNum") 
		Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
		Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")

		If  String.IsNullOrEmpty (LineItemNum) Then 
			Throw New Exception("Please choose a Line Item") 
		End If
		RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, LineItemNum )

		Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U7#None:U8#None"			
		Dim scriptGenericsDescr As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_Input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U7#None:U8#None"	
						
		Dim std_LineItemsDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "Std_LineItems")
		Dim total_Expense_Line_ItemsId As Integer = BRApi.Finance.Members.GetMemberId(si, dimtypeId.UD6, "Total_Expense_LineItems")
		Dim number_of_Expenses As Integer = 0
		Dim LineItemNumInt As Integer = LineItemNum.Substring(12,2).XFConvertToInt	
		
		
		'Find number of Expenses
		Dim ud6LineItemMems As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si,std_LineItemsDimPk, total_Expense_Line_ItemsId, Nothing)
		If Not ud6lineItemMems Is Nothing Then
			For Each ud6objLineItem As Member In ud6LineItemMems
				'Get the Line Item member Name
				Dim ud6LineItemName As String = ud6objLineItem.Name	
				Dim objDataCellInfoUsingMemberScript As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si,wfCube,"A#Requested_Item_Tier1:" & scriptGenerics &":U6#" & ud6LineItemName)
				Dim requested_Item_Tier1 As String = objDataCellInfoUsingMemberScript.DataCellEx.DataCellAnnotation
					
				If (Not requested_Item_Tier1.XFEqualsIgnoreCase("")) Then	
					number_of_Expenses = number_of_Expenses+1
				End If							
			Next
		End If
		
		If number_of_Expenses <> 1 AndAlso  number_of_Expenses > LineItemNumInt  Then	
			
			Dim strExpensesMax As String = number_of_Expenses.ToString()
			If number_of_Expenses <10 Then 
				strExpensesMax = "0"&number_of_Expenses.ToString()
			End If
			
			'clear current expense
			ClearExpense(si, args, wfScenario, wfCube, wfTime, RP_Entity, rpName, LineItemNum,  LineItemNumInt, scriptGenerics, scriptGenericsDescr)	
			
			'copy from one plus
			Do
				Dim strLineItem As String = LineItemNumInt.ToString()
				Dim strLineItemplusone As String = LineItemNumInt+1.ToString()
				If LineItemNumInt <10 Then 
					strLineItem = "0"&LineItemNumInt.ToString()
				End If
				If LineItemNumInt+1 <10 Then 
					strLineItemplusone = "0"&LineItemNumInt+1.ToString()
				End If

				CopyExpenseAllFields( si, args, wfCube, wfTime, wfScenario, RP_Entity, rpName, "ExpLineItem_" & strLineItemplusone, "ExpLineItem_" & strLineItem )
				'brapi.ErrorLog.LogMessage(si, "copy " & strLineItemplusone & " to " & strLineItem)
				LineItemNumInt=LineItemNumInt+1
				
			Loop While LineItemNumInt <> number_of_Expenses
			
			'clear last expense
			
			ClearExpense(si, args, wfScenario, wfCube, wfTime, RP_Entity, rpName, "ExpLineItem_" & strExpensesMax,  number_of_Expenses, scriptGenerics, scriptGenericsDescr)	
			
		Else If  number_of_Expenses < LineItemNumInt  Then
			Throw New Exception("Cannot delete Line Item")
		Else 'number_of_Expenses = 1 so just clear it

			ClearExpense(si, args, wfScenario, wfCube, wfTime, RP_Entity, rpName, LineItemNum,  LineItemNumInt, scriptGenerics, scriptGenericsDescr)	
		End If

		Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
		selectionChangedTaskResult = Me.RefreshSelectedLineItem_RP(si, wfCube, wfTime, wfScenario, RPName, LineItemNum )
		Return selectionChangedTaskResult
		
'		Dim params As New Dictionary(Of String, String) 
'			params.Add("prm_EXP_RequestedItem_Tier1_RP", String.Empty) 		
'			params.Add("prm_EXP_Description_Tier2_RP", String.Empty)
'			params.Add("prm_EXP_Description_Tier2_Input_RP", String.Empty)
'			params.Add("prm_EXP_POC_RP", String.Empty)
'			params.Add("prm_EXP_SupportingDoc_RP", String.Empty)
'			params.Add("prm_EXP_DollarKValue_RP", String.Empty)
'			params.Add("prm_EXP_RecurringNonRecurring_RP", String.Empty)
'			params.Add("prm_EXP_ATU_RP", String.Empty)
'			params.Add("prm_EXP_PPA_RP", String.Empty)
'			params.Add("prm_EXP_UII_RP", String.Empty)
'			params.Add("prm_EXP_ObjectClass_RP", String.Empty)			
'		Return SetFieldValues(si,  params ,True, "" & RPName & " " & LineItemNum & " Successfully Cleared")	
				Case Else
					Throw New XFException(si, New Exception("DeleteEXPLine: unknown appropriation '" & rpAppr & "'"))
			End Select
			Return Nothing
		End Function
		Private Function EditEXPLine(ByVal rpAppr As String) As Object
			' Variant bodies kept verbatim per appropriation (collapse later
			' only where a diff proves the variants identical).
			Select Case rpAppr
				Case "F"
					' ==== ported verbatim from BudFM_SolutionHelper.EditEXPLine_F ====
		Dim wfTime As String = args.NameValuePairs("WFTime")
		Dim wfScenario As String = args.NameValuePairs("WFScenario")
		Dim wfCube As String = args.NameValuePairs("WFCube")
		Dim RPName As String = args.NameValuePairs("RPName")
		Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
		Dim LineItemNum As String = args.NameValuePairs("LineItemNum") 
		Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
		Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")

		If  String.IsNullOrEmpty (LineItemNum) Then 
			Throw New Exception("Please choose a Line Item") 
		End If
		RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, LineItemNum )
						
		Dim requested_Item_Tier1 As String = args.NameValuePairs("Requested_Item_Tier1") '|!prm_NBLT_RequestedItem_Tier1!|
		Dim requested_Item_Tier1Split As List(Of String) = StringHelper.SplitString(requested_Item_Tier1, "_")
		Dim requested_ItemNum As Integer = requested_Item_Tier1Split(0).XFConvertToInt
		Dim description_Tier2 As String = args.NameValuePairs("Description_Tier2") '|!prm_NBLT_Description_Tier2!|
		Dim description_Tier2_ToUse As String = String.Empty
		'If requested_ItemNum >=400, we need to potentially determine which base Tier2 member to use since they will be entering a custom description
		If requested_ItemNum >=400
			If description_Tier2.XFContainsIgnoreCase("_1") Or description_Tier2 = "" Then
				description_Tier2_ToUse = requested_ItemNum & "0_1"
			Else
				description_Tier2_ToUse = description_Tier2
			End If
		Else 'requested_ItemNum <400
				description_Tier2_ToUse = description_Tier2
		End If							
		Dim description_Tier2_Input As String = args.NameValuePairs("Description_Tier2_Input") '|!prm_NBLT_Description_Tier2_Input!|
		Dim description_Tier2_Input_ToUse As String = String.Empty
		'If the requested_ItemNum >=400 , they must be usign a canned member so we should grab the description from that member, If not, then use what they entered
		If requested_ItemNum < 400
			description_Tier2_Input_ToUse = BRApi.Finance.Members.GetMember(si, dimtypeid.UD5, description_Tier2).Description
		Else
			description_Tier2_Input_ToUse = description_Tier2_Input
		End If						
		Dim pOC As String = args.NameValuePairs("POC") '|!prm_NBLT_POC!|
		Dim dollarK_Value As String = args.NameValuePairs("DollarK_Value") '|!prm_NBLT_DollarKValue!|		
		Dim r_NR As String = args.NameValuePairs("R_NR") '|!prm_NBLT_RecurringNonRecurring!|
		Dim aTU As String = args.NameValuePairs("ATU") '|!prm_NBLT_ATU!|
		Dim aTU_NoUnit As String=String.Empty
		If aTU <> ""
			aTU_NoUnit = aTU & "_NoUnit" 'Add the ATU and _NoUnit together to get the base level unit to store it at
		End If
		Dim pPAscriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"			
		Dim pPA As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#PPA:" & pPAscriptGenerics).DataCellEx.DataCellAnnotation			
		
		'Throw an error if a PPA has not been selected
		Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
		If pPA.XFEqualsIgnoreCase("")
				selectionChangedTaskResult.IsOK = True
				selectionChangedTaskResult.ShowMessageBox = True
				selectionChangedTaskResult.Message = " PPA is blank.  Please go back to Page 1 and select a PPA."
				Return selectionChangedTaskResult	
		End If
		
		Dim uII As String = args.NameValuePairs("UII") '|!prm_NBLT_UII!|
		Dim object_Class As String = args.NameValuePairs("Object_Class") '|!prm_NBLT_ObjectClass!|
							
		Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#" & LineItemNum & ":U7#None:U8#None"		
		
		Dim scriptGenericsDescr As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_Input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U6#" & LineItemNum & ":U7#None:U8#None"	
		'Create a new list of memberscript and value
		Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)
		
		'In this part, we are writing the annotations to the database
		lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Requested_Item_Tier1:" 											& scriptGenerics, 		0, True, requested_Item_Tier1))
		lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Description_Tier2:" 											& scriptGenerics, 		0, True, description_Tier2_ToUse))
		lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "U5#" 							& description_Tier2_ToUse & ":" 	& scriptGenericsDescr, 	0, True, description_Tier2_Input_ToUse))
		lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#POC:" 															& scriptGenerics, 		0, True, pOC))
		lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#DollarK_Value:" 												& scriptGenerics, 		0, True, dollarK_Value))
		lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#R_NR:" 															& scriptGenerics, 		0, True, r_NR))
		lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#ATU:" 															& scriptGenerics, 		0, True, aTU_NoUnit))
		lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#PPA:" 															& scriptGenerics, 		0, True, pPA))
		lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#UII:" 															& scriptGenerics, 		0, True, uII))
		lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Object_Class:" 													& scriptGenerics, 		0, True, object_Class))
							
		
		'********Allocation Drivers Storage********									
		'For those attributes that are also a dimension, we will also store a 1 in that dimension member that is selected so we can find it in a data buffer for the cost calc	
		Me.NBAllocationsCalc(si, args, RP_Entity, RPName, wfTime, LineItemNum, pPA, uII, object_Class, aTU_NoUnit)		
		
		'Write the annotations to the database
		Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)							
					
					
	 	'Show a message box that the RP was successfully created
		selectionChangedTaskResult.IsOK = True
		selectionChangedTaskResult.ShowMessageBox = True
		selectionChangedTaskResult.Message = "" & GetDescription(si,RPName) & " " & GetUD6Description(si,LineItemNum) & " Successfully Updated"
	 	Return selectionChangedTaskResult
		

				Case "PCI"
					' ==== ported verbatim from BudFM_SolutionHelper.EditEXPLine_PCI ====
			Dim wfTime As String = args.NameValuePairs("WFTime")
			Dim wfTimeNext1 As String = (wfTime.XFConvertToInt + 1).ToString
			Dim wfTimeNext2 As String = (wfTime.XFConvertToInt + 2).ToString
			Dim wfTimeNext3 As String = (wfTime.XFConvertToInt + 3).ToString
			Dim wfTimeNext4 As String = (wfTime.XFConvertToInt + 4).ToString
			Dim wfScenario As String = args.NameValuePairs("WFScenario")
			Dim wfCube As String = args.NameValuePairs("WFCube")
			Dim RPName As String = args.NameValuePairs("RPName")
			Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
			Dim LineItemNum As String = args.NameValuePairs("LineItemNum") 
			Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
			Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")
			Dim PPA As String = args.NameValuePairs("PPA_Selection") 
			Dim returnMessage As String = GetDescription(si,RPName) & " Successfully Updated"

			RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, LineItemNum)

			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
											
			' First Check if PPA is already selected for this RP. 
			' If Not, force the use To make the PPA selection before an expense line item can be saved
			
			If PPA = "" Then
				selectionChangedTaskResult.IsOK = True
				selectionChangedTaskResult.ShowMessageBox = True
				selectionChangedTaskResult.Message = "PPA needs to be selected first. Please select a PPA."
				Return selectionChangedTaskResult	
			End If 
						
			' Set Script Generics which is common for all line items. Get  PPA, UII and STU that were set as page 1 . (Please note: UD6 is line item number)
			Dim scriptGenericsExpenses As String = "Cb#" & wfCube & ":E#" & RP_Entity & ":C#Local:S#" & wfScenario & ":T#" & wfTime & ":V#Periodic:A#DollarK_Value:F#" & RPName & ":O#Top:I#None:U1#" & PPA & ":U2#Total_Investment:U3#Total_NonPay_Related:U4#PCI_NoUnit:U5#Total_CostLine:U6#No_ExpLineItem:U7#None:U8#None"
			Dim scriptGenericsObligations As String = "Cb#" & wfCube & ":E#" & RP_Entity & ":C#Local:S#" & wfScenario & ":V#Periodic:F#" & RPName & ":O#Top:I#None:U1#" & PPA & ":U2#Total_Investment:U3#Total_NonPay_Related:U4#PCI_NoUnit:U5#Total_CostLine:U6#No_ExpLineItem:U7#None:U8#FundRem_04"
										
'			' PPA is alredy selected and change comment already continue with saving line item 	
			Dim programExpense	As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, scriptGenericsExpenses).DataCellEx.DataCell.CellAmount
			Dim BY_Obligations 			As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "T#" & wfTime & ":A#BY_Obligations:" & scriptGenericsObligations).DataCellEx.DataCell.CellAmount
			Dim BY_Plus1_Obligations 	As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "T#" & wfTimeNext1 & ":A#By_Plus1_Obligations:" & scriptGenericsObligations).DataCellEx.DataCell.CellAmount		
			Dim BY_Plus2_Obligations 	As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "T#" & wfTimeNext2 & ":A#By_Plus2_Obligations:" & scriptGenericsObligations).DataCellEx.DataCell.CellAmount
			Dim BY_Plus3_Obligations 	As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "T#" & wfTimeNext3 & ":A#By_Plus3_Obligations:" & scriptGenericsObligations).DataCellEx.DataCell.CellAmount
			Dim BY_Plus4_Obligations 	As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "T#" & wfTimeNext4 & ":A#By_Plus4_Obligations:" & scriptGenericsObligations).DataCellEx.DataCell.CellAmount			
			Dim totalObligations As Decimal = (BY_Obligations + BY_Plus1_Obligations + BY_Plus2_Obligations + BY_Plus3_Obligations + BY_Plus4_Obligations)

			'Throw a warning if the program expenses don't equal the Obligations profile		
			If programExpense <> totalObligations Then 
				returnMessage = "WARNING: Total Program amount not equal to the sum of all BY Obligations." & vbNewLine & vbNewLine & "Please correct before proceeding."
			End If
			
			'Calculate the data cells for expense and Obligations by multiplying the input cells by $1000	
			Dim povInfo As New Dictionary(Of String, String) 
			povInfo.Add("Cube", wfCube)
			povInfo.Add("Consolidation", "Local")
			povInfo.Add("Scenario", wfScenario)
			povInfo.Add("View", "Periodic")
			povInfo.Add("Entity", rp_Entity)		
			
			globals.SetStringValue("rpName", rpName) 			
			
			povInfo.Add("Time", wfTime)	
			brapi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si, "USCG_RP_CostCalc_Expense", "Calc_PCI_ExpensesAndObligations", povInfo, customcalculatetimetype.MemberFilter)	
			
			povInfo.Remove("Time")
			povInfo.Add("Time", wfTimeNext1)	
			brapi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si, "USCG_RP_CostCalc_Expense", "Calc_PCI_ExpensesAndObligations", povInfo, customcalculatetimetype.MemberFilter)
			
			povInfo.Remove("Time")
			povInfo.Add("Time", wfTimeNext2)	
			brapi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si, "USCG_RP_CostCalc_Expense", "Calc_PCI_ExpensesAndObligations", povInfo, customcalculatetimetype.MemberFilter)
							
			povInfo.Remove("Time")				
			povInfo.Add("Time", wfTimeNext3)	
			brapi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si, "USCG_RP_CostCalc_Expense", "Calc_PCI_ExpensesAndObligations", povInfo, customcalculatetimetype.MemberFilter)
			
			povInfo.Remove("Time")
			povInfo.Add("Time", wfTimeNext4)	
			brapi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si, "USCG_RP_CostCalc_Expense", "Calc_PCI_ExpensesAndObligations", povInfo, customcalculatetimetype.MemberFilter)
			
			'Show a message box that the RP line item successfully saved			
			selectionChangedTaskResult.IsOK = True
			selectionChangedTaskResult.ShowMessageBox = True
			selectionChangedTaskResult.Message = returnMessage
		 	Return selectionChangedTaskResult
			
				Case "RD"
					' ==== ported verbatim from BudFM_SolutionHelper.EditEXPLine_RD ====
			Dim wfTime As String = args.NameValuePairs("WFTime")
			Dim wfTimeNext1 As String = (wfTime.XFConvertToInt + 1).ToString
			Dim wfTimeNext2 As String = (wfTime.XFConvertToInt + 2).ToString
			Dim wfTimeNext3 As String = (wfTime.XFConvertToInt + 3).ToString
			Dim wfTimeNext4 As String = (wfTime.XFConvertToInt + 4).ToString
			Dim wfScenario As String = args.NameValuePairs("WFScenario")
			Dim wfCube As String = args.NameValuePairs("WFCube")
			Dim RPName As String = args.NameValuePairs("RPName")
			Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
			Dim LineItemNum As String = args.NameValuePairs("LineItemNum") 
			Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
			Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")
			Dim returnMessage As String = GetDescription(si,RPName) & " Successfully Updated"

			RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, LineItemNum)

			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
			
			'Get the PPA from the RP Name
			Dim PPA As String = String.Empty
			
			If RPName = String.Empty Then
				selectionChangedTaskResult.IsOK = True
				selectionChangedTaskResult.ShowMessageBox = True
				selectionChangedTaskResult.Message = "RP needs to be selected first. Please select an RP."
				Return selectionChangedTaskResult	
			Else 
				PPA = RPName.Substring(3, RPName.Length - 3)
			End If 
						
			' Set Script Generics which is common for all line items. Get  PPA, UII and STU that were set as page 1 . (Please note: UD6 is line item number)
			Dim scriptGenericsExpenses As String = "Cb#" & wfCube & ":E#" & RP_Entity & ":C#Local:S#" & wfScenario & ":T#" & wfTime & ":V#Periodic:A#DollarK_Value:F#" & RPName & ":O#Top:I#None:U1#" & PPA & ":U2#Total_Investment:U3#Total_NonPay_Related:U4#RD_NoUnit:U5#Total_CostLine:U6#No_ExpLineItem:U7#None:U8#None"
			Dim scriptGenericsObligations As String = "Cb#" & wfCube & ":E#" & RP_Entity & ":C#Local:S#" & wfScenario & ":V#Periodic:F#" & RPName & ":O#Top:I#None:U1#" & PPA & ":U2#Total_Investment:U3#Total_NonPay_Related:U4#RD_NoUnit:U5#Total_CostLine:U6#No_ExpLineItem:U7#None:U8#FundRem_02"
										
'			' PPA is alredy selected and change comment already continue with saving line item 	
			Dim programExpense	As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, scriptGenericsExpenses).DataCellEx.DataCell.CellAmount
			Dim BY_Obligations 			As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "T#" & wfTime & ":A#BY_Obligations:" & scriptGenericsObligations).DataCellEx.DataCell.CellAmount
			Dim BY_Plus1_Obligations 	As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "T#" & wfTimeNext1 & ":A#By_Plus1_Obligations:" & scriptGenericsObligations).DataCellEx.DataCell.CellAmount		
			Dim BY_Plus2_Obligations 	As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "T#" & wfTimeNext2 & ":A#By_Plus2_Obligations:" & scriptGenericsObligations).DataCellEx.DataCell.CellAmount			
			Dim totalObligations As Decimal = (BY_Obligations + BY_Plus1_Obligations + BY_Plus2_Obligations)

			'Throw a warning if the program expenses don't equal the Obligations profile		
			If programExpense <> totalObligations Then 
				returnMessage = "WARNING: Total Program amount not equal to the sum of all BY Obligations." & vbNewLine & vbNewLine & "Please correct before proceeding."
			End If
			
			'Calculate the data cells for expense and Obligations by multiplying the input cells by $1000	
			Dim povInfo As New Dictionary(Of String, String) 
			povInfo.Add("Cube", wfCube)
			povInfo.Add("Consolidation", "Local")
			povInfo.Add("Scenario", wfScenario)
			povInfo.Add("View", "Periodic")
			povInfo.Add("Entity", rp_Entity)		
			
			globals.SetStringValue("rpName", rpName) 			
			
			povInfo.Add("Time", wfTime)	
			brapi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si, "USCG_RP_CostCalc_Expense", "Calc_RD_ExpensesAndObligations", povInfo, customcalculatetimetype.MemberFilter)	
			
			povInfo.Remove("Time")
			povInfo.Add("Time", wfTimeNext1)	
			brapi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si, "USCG_RP_CostCalc_Expense", "Calc_RD_ExpensesAndObligations", povInfo, customcalculatetimetype.MemberFilter)
			
			povInfo.Remove("Time")
			povInfo.Add("Time", wfTimeNext2)	
			brapi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si, "USCG_RP_CostCalc_Expense", "Calc_RD_ExpensesAndObligations", povInfo, customcalculatetimetype.MemberFilter)
							
			povInfo.Remove("Time")
			povInfo.Add("Time", wfTimeNext3)	
			brapi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si, "USCG_RP_CostCalc_Expense", "Calc_RD_ExpensesAndObligations", povInfo, customcalculatetimetype.MemberFilter)
			
			povInfo.Remove("Time")
			povInfo.Add("Time", wfTimeNext4)	
			brapi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si, "USCG_RP_CostCalc_Expense", "Calc_RD_ExpensesAndObligations", povInfo, customcalculatetimetype.MemberFilter)
							
			'Show a message box that the RP line item successfully saved			
			selectionChangedTaskResult.IsOK = True
			selectionChangedTaskResult.ShowMessageBox = True
			selectionChangedTaskResult.Message = returnMessage
		 	Return selectionChangedTaskResult
			
				Case Else
					Throw New XFException(si, New Exception("EditEXPLine: unknown appropriation '" & rpAppr & "'"))
			End Select
			Return Nothing
		End Function
		Private Function EditHIST(ByVal rpAppr As String) As Object
			' Variant bodies kept verbatim per appropriation (collapse later
			' only where a diff proves the variants identical).
			Select Case rpAppr
				Case "PCI"
					' ==== ported verbatim from BudFM_SolutionHelper.EditHIST_PCI ====
			Dim wfTime As String = args.NameValuePairs("WFTime")
			Dim wfTimePrior1 As String = (wfTime.XFConvertToInt - 1).ToString
			Dim wfTimePrior2 As String = (wfTime.XFConvertToInt - 2).ToString
			Dim wfScenario As String = args.NameValuePairs("WFScenario")
			Dim wfCube As String = args.NameValuePairs("WFCube")
			Dim RPName As String = args.NameValuePairs("RPName")
			Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
			Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
			Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")

			RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, "")
						
			'Calculate the data cells for expense and Obligations by multiplying the input cells by $1000	
			Dim povInfo As New Dictionary(Of String, String) 
			povInfo.Add("Cube", wfCube)
			povInfo.Add("Consolidation", "Local")
			povInfo.Add("Scenario", wfScenario)
			povInfo.Add("View", "Periodic")
			povInfo.Add("Entity", rp_Entity)		
			
			globals.SetStringValue("rpName", rpName) 			
			
			povInfo.Add("Time", wfTimePrior2)	
			brapi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si, "USCG_RP_CostCalc_Expense", "Calc_BudAuthAndOblInp", povInfo, customcalculatetimetype.MemberFilter)
							
			povInfo.Remove("Time")
			povInfo.Add("Time", wfTimePrior1)	
			brapi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si, "USCG_RP_CostCalc_Expense", "Calc_BudAuthAndOblInp", povInfo, customcalculatetimetype.MemberFilter)
			
			povInfo.Remove("Time")
			povInfo.Add("Time", wfTime)	
			brapi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si, "USCG_RP_CostCalc_Expense", "Calc_BudAuthAndOblInp", povInfo, customcalculatetimetype.MemberFilter)	
			
'			Show a message box that the RP line item successfully saved
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()					
			selectionChangedTaskResult.IsOK = True
			selectionChangedTaskResult.ShowMessageBox = True
			selectionChangedTaskResult.Message = "" & GetDescription(si,RPName) & " Successfully Updated"
		 	Return selectionChangedTaskResult
						
				Case "RD"
					' ==== ported verbatim from BudFM_SolutionHelper.EditHIST_RD ====
			Dim wfTime As String = args.NameValuePairs("WFTime")
			Dim wfTimePrior1 As String = (wfTime.XFConvertToInt - 1).ToString
			Dim wfTimePrior2 As String = (wfTime.XFConvertToInt - 2).ToString
			Dim wfScenario As String = args.NameValuePairs("WFScenario")
			Dim wfCube As String = args.NameValuePairs("WFCube")
			Dim RPName As String = args.NameValuePairs("RPName")
			Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
			Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
			Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")

			RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, "")
			
			'Calculate the data cells for expense and Obligations by multiplying the input cells by $1000	
			Dim povInfo As New Dictionary(Of String, String) 
			povInfo.Add("Cube", wfCube)
			povInfo.Add("Consolidation", "Local")
			povInfo.Add("Scenario", wfScenario)
			povInfo.Add("View", "Periodic")
			povInfo.Add("Entity", rp_Entity)		
			
			globals.SetStringValue("rpName", rpName) 			
			
			povInfo.Add("Time", wfTimePrior2)	
			brapi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si, "USCG_RP_CostCalc_Expense", "Calc_BudAuthAndOblInp", povInfo, customcalculatetimetype.MemberFilter)
							
			povInfo.Remove("Time")
			povInfo.Add("Time", wfTimePrior1)	
			brapi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si, "USCG_RP_CostCalc_Expense", "Calc_BudAuthAndOblInp", povInfo, customcalculatetimetype.MemberFilter)
			
			povInfo.Remove("Time")
			povInfo.Add("Time", wfTime)	
			brapi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si, "USCG_RP_CostCalc_Expense", "Calc_BudAuthAndOblInp", povInfo, customcalculatetimetype.MemberFilter)	
			
'			Show a message box that the RP line item successfully saved
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()					
			selectionChangedTaskResult.IsOK = True
			selectionChangedTaskResult.ShowMessageBox = True
			selectionChangedTaskResult.Message = "" & GetDescription(si,RPName) & " Successfully Updated"
		 	Return selectionChangedTaskResult
						
				Case Else
					Throw New XFException(si, New Exception("EditHIST: unknown appropriation '" & rpAppr & "'"))
			End Select
			Return Nothing
		End Function
		Private Function EditRP_Page1(ByVal rpAppr As String) As Object
			' Variant bodies kept verbatim per appropriation (collapse later
			' only where a diff proves the variants identical).
			Select Case rpAppr
				Case "F"
					' ==== ported verbatim from BudFM_SolutionHelper.EditRP_Page1_F ====

			Dim wfTime As String = args.NameValuePairs("WFTime")
			Dim wfScenario As String = args.NameValuePairs("WFScenario")
			Dim wfCube As String = args.NameValuePairs("WFCube")
			Dim RPName As String = args.NameValuePairs("RPName")
			Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
			Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
			Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")

			RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, "")
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
			' Set the generic script  which is common for all the expense line items
			Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & 
											":V#Annotation:F#" & RPName & 
											":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
											
			' First, check if PPA is selected. If not force the user to select PPA before saving 
			Dim PPA  = 	args.NameValuePairs("PPA")
			If PPA = "" Then
				'Show a message one screen 
				selectionChangedTaskResult.IsOK = False
				selectionChangedTaskResult.ShowMessageBox = True
				selectionChangedTaskResult.Message = "Please choose PPA before saving"
				Return selectionChangedTaskResult	
			End If
				
			' Check if another PPA was selected saved before is different from current selection 
			' If so, we need to reset PPA for each line items that were already created with the newly selected PPA and clear all the costs 
			Dim PriorPPA As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#PPA:" & scriptGenerics).DataCellEx.DataCellAnnotation
			Dim LineItemPPAResetNeeded As Boolean = False 
			If (Not PriorPPA = "") And 
			   (Not PPA = PriorPPA) Then
				LineItemPPAResetNeeded = True
			End If
			
			
			' Create a new list of memberscript and value and add memebers
			Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)
			
			' Create a new MemberScriptAndValue for each parameter and add to the list
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#PPA:" & scriptGenerics, 0, True, PPA))	
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#ATU:" & scriptGenerics, 0, True, args.NameValuePairs("ATU")))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#UII:" & scriptGenerics, 0, True, args.NameValuePairs("UII")))
			'Write the annotations to the database
			Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)			
			
			If LineItemPPAResetNeeded = True Then
				' Clear all the cost cacls 
				Me.Clear_Single_RP_EXP_Cost(si,args, WfTime, RP_Entity, RPName)
				'Reset the PPA for all expense line items with newly selected PPA
				updateLineItemsPPA(si, args, wfCube, wfTime, WfScenario, RP_Entity, RPName, PPA)
				' Reset allocations for all line in the data wich includes newly updated PPA for all expense line items
				updateAllocationsforLineItems(si, args, wfCube, wfTime, WfScenario, RP_Entity, RPName)
			End If
			
		 	'Show a message box that the RP was successfully updated
			selectionChangedTaskResult.IsOK = True
			selectionChangedTaskResult.ShowMessageBox = True
			selectionChangedTaskResult.Message = "" & GetDescription(si,RPName) & " Successfully Updated"
		 	Return selectionChangedTaskResult		
					
				Case "OS"
					' ==== ported verbatim from BudFM_SolutionHelper.EditRP_Page1_OS ====

			Dim wfTime As String = args.NameValuePairs("WFTime")
			Dim wfScenario As String = args.NameValuePairs("WFScenario")
			Dim wfCube As String = args.NameValuePairs("WFCube")
			Dim RPName As String = args.NameValuePairs("RPName")
			Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
			Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
			Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")
			
			Dim Content_OS As String = args.NameValuePairs("Content_OS")
			Dim Content_EditRP_OS As String = args.NameValuePairs("Content_EditRP_OS")
			

			RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, "")
			
				Dim number_of_Billets As String = args.NameValuePairs("Number_of_Billets")
				Dim add_General_Detail As String = args.NameValuePairs("Add_General_Detail")
				Dim increase_Decrease As String = args.NameValuePairs("Increase_Decrease") 'If Increase, continue on.  If Decrease, force fill the Lease, Utility, and ICASS fields to No.
				Dim part_of_Reprogramming As String = args.NameValuePairs("Part_of_Reprogramming")
				Dim personnel_Qtrs As String = args.NameValuePairs("Personnel_Qtrs")	
				Dim os_Qtrs As String = args.NameValuePairs("OS_Qtrs")				
					
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"						
					
					'Create a new list of memberscript and value
					Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)
					
					'Add the member scripts to the list and store as 0 No data annotations
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Number_of_Billets:" 		& scriptGenerics, 0, True, number_of_Billets))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Add_General_Detail:" 		& scriptGenerics, 0, True, add_General_Detail))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Increase_Decrease:" 		& scriptGenerics, 0, True, increase_Decrease))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Part_of_Reprogramming:" 	& scriptGenerics, 0, True, part_of_Reprogramming))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Personnel_Qtrs:" 			& scriptGenerics, 0, True, personnel_Qtrs))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#OS_Qtrs:" 					& scriptGenerics, 0, True, os_Qtrs))							
					
					'Write the annotations to the database
					Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)			
					
					RunPostSaveStepsForRP(globals, si, wfcube, RP_Entity, wfscenario, wftime, RPName)
					
				 	'Show a message box that the RP was successfully updated
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					SetRoutingPageCompat(selectionChangedTaskResult.ModifiedCustomSubstVars, rpAppr, Content_EditRP_OS)
					SetRoutingContent(selectionChangedTaskResult.ModifiedCustomSubstVars, rpAppr, Content_OS)
					selectionChangedTaskResult.IsOK = True
					selectionChangedTaskResult.ShowMessageBox = True
					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					selectionChangedTaskResult.Message = "" & GetDescription(si,RPName) & " Successfully Updated"
				 	Return selectionChangedTaskResult		
					
				
			
				Case "PCI"
					' ==== ported verbatim from BudFM_SolutionHelper.EditRP_Page1_PCI ====
			Dim wfTime As String = args.NameValuePairs("WFTime")
			Dim wfScenario As String = args.NameValuePairs("WFScenario")
			Dim wfCube As String = args.NameValuePairs("WFCube")
			Dim RPName As String = args.NameValuePairs("RPName")
			Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
			Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
			Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")

			RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, "")

			Dim personnel_Qtrs As String = args.NameValuePairs("Personnel_Qtrs")				
			Dim ppa_Level1 As String = args.NameValuePairs("PPA_Level1")
			Dim ppa_Level2 As String = args.NameValuePairs("PPA_Level2")
						
			
			Dim scriptGenerics As String = "Cb#" & wfCube & ":E#" & RP_Entity & ":C#Local:S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"						
			
			'Create a new list of memberscript and value
			Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)
							
			'Create a new MemberScriptAndValue for each parameter and add to the list
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#PPA_Level1_PCI:" & scriptGenerics, 0, True, ppa_Level1))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#PPA_Level2_PCI:" & scriptGenerics, 0, True, ppa_Level2))
									
							
			Dim PriorPPA1 As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#PPA_Level1_PCI:" & scriptGenerics).DataCellEx.DataCellAnnotation
			Dim PriorPPA2 As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#PPA_Level2_PCI:" & scriptGenerics).DataCellEx.DataCellAnnotation

			Dim LineItemPPAResetNeeded As Boolean = False 
			If Not String.CompareOrdinal(ppa_Level1,PriorPPA1) Or Not String.CompareOrdinal(ppa_Level2,PriorPPA2) Then
				Me.Clear_Single_RP_EXP_Cost(si,args, WfTime, RP_Entity, RPName)
				'Reset the PPA for all expense line items with newly selected PPA
					updateLineItemsPPA(si, args, wfCube, wfTime, WfScenario, RP_Entity, RPName, ppa_Level2)
					' Reset allocations for all line in the data wich includes newly updated PPA for all expense line items
					updateAllocationsforLineItems(si, args, wfCube, wfTime, WfScenario, RP_Entity, RPName)
			End If
			
			'Write the annotations to the database
			Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)			
			
		 	'Show a message box that the RP was successfully updated
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
			selectionChangedTaskResult.IsOK = True
			selectionChangedTaskResult.ShowMessageBox = True
			selectionChangedTaskResult.Message = "" & GetDescription(si,RPName) & " Successfully Updated"
		 	Return selectionChangedTaskResult		
			
						
					
				Case "RD"
					' ==== ported verbatim from BudFM_SolutionHelper.EditRP_Page1_RD ====
			Dim wfTime As String = args.NameValuePairs("WFTime")
			Dim wfScenario As String = args.NameValuePairs("WFScenario")
			Dim wfCube As String = args.NameValuePairs("WFCube")
			Dim RPName As String = args.NameValuePairs("RPName")
			Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
			Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
			Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")

			RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, "")

			
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()

			' Set the generic script  which is common for all the expense line items
				Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & 
												":V#Annotation:F#" & RPName & 
												":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"						

				
				' First, check if PPA is selected. If not force the user to select PPA before saving 								
				Dim PPA  = 	args.NameValuePairs("PPA")
				If PPA = "" Then
					'Show a message one screen 
					selectionChangedTaskResult.IsOK = False
					selectionChangedTaskResult.ShowMessageBox = True
					selectionChangedTaskResult.Message = "Please choose PPA before saving"
					Return selectionChangedTaskResult	
				End If

				
				' Check if another PPA was selected saved before is different from current selection 
				' If so, we need to reset PPA for each line items that were already created with the newly selected PPA and clear all the costs 
				Dim PPA_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#PPA:" & scriptGenerics)
				Dim PriorPPA As String = PPA_Info.DataCellEx.DataCellAnnotation
				Dim LineItemPPAResetNeeded As Boolean = False 
				If (Not PriorPPA = "") And 
				   (Not PPA = PriorPPA) Then
					LineItemPPAResetNeeded = True
				End If
				
				
				' Create a new list of memberscript and value and add memebers
				Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)
				
				' Create a new MemberScriptAndValue for each parameter and add to the list
				lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#PPA:" & scriptGenerics, 0, True, PPA))						
				lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#ATU:" & scriptGenerics, 0, True, args.NameValuePairs("ATU")))						
				lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#UII:" & scriptGenerics, 0, True, args.NameValuePairs("UII")))																
				
				'Write the annotations to the database
				Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)			
				
				If LineItemPPAResetNeeded = True Then
					' Clear all the cost cacls 
					Me.Clear_Single_RP_EXP_Cost(si,args, WfTime, RP_Entity, RPName)
					'Reset the PPA for all expense line items with newly selected PPA
					updateLineItemsPPA(si, args, wfCube, wfTime, WfScenario, RP_Entity, RPName, PPA)
					' Reset allocations for all line in the data wich includes newly updated PPA for all expense line items
					updateAllocationsforLineItems(si, args, wfCube, wfTime, WfScenario, RP_Entity, RPName)
				End If
				
			 	'Show a message box that the RP was successfully updated
				selectionChangedTaskResult.IsOK = True
				selectionChangedTaskResult.ShowMessageBox = True
				selectionChangedTaskResult.Message = "" & GetDescription(si,RPName) & " Successfully Updated"
			 	Return selectionChangedTaskResult		
				
			
				
				Case Else
					Throw New XFException(si, New Exception("EditRP_Page1: unknown appropriation '" & rpAppr & "'"))
			End Select
			Return Nothing
		End Function
		Private Function OnCbxBtnClick_GEN(ByVal rpAppr As String) As Object
			' Variant bodies kept verbatim per appropriation (collapse later
			' only where a diff proves the variants identical).
			Select Case rpAppr
				Case "BS"
					' ==== ported verbatim from BudFM_SolutionHelper.OnCbxBtnClick_GEN_BS ====
					
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
										
					'Get the component name
					Dim componentName As String = args.ComponentInfo.Component.Name
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")

					' If No RP is selected, nothing to do
					If RPName = "" Then 
						Return Nothing
					End If
					
					Dim routingAppn As String = ResolveRoutingAppnForRP(RPName, args.NameValuePairs.XFGetValue("APPN_Content", "OS"))
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)							
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"			
					
					'using a global function to avoid using brapi functions too many times and use api.data.calculate via a finance rule instead
					'set the script generics and parent account to be used in the global function
					globals.SetStringValue("scriptGenerics", scriptGenerics)
					globals.SetStringValue("parAccount", "RP_Attributes")					

					'Set a generic dictionary as an argument in the rule below
					Dim Dictionary As New Dictionary(Of String, String)
					
						BUDFM_AttributeSupport.GetRPAttributes(si, globals)
					
					If Not globals.GetObject("attributeDict") Is Nothing
					
						Dim attributeDict As Dictionary(Of String, String) = globals.GetObject("attributeDict")
							
						'Set the parameters for the combo boxes in the RP Dashboard Page2
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_FYRelatedRp1_BS", 				attributeDict.GetValueOrEmpty("FY_Related_RP1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_FYRelatedRp2_BS", 				attributeDict.GetValueOrEmpty("FY_Related_RP2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_FYRelatedRp3_BS", 				attributeDict.GetValueOrEmpty("FY_Related_RP3"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_OlderRelatedRp1_BS", 				attributeDict.GetValueOrEmpty("Older_Related_RP1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_OlderRelatedRp2_BS", 				attributeDict.GetValueOrEmpty("Older_Related_RP2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_OlderRelatedRp3_BS", 				attributeDict.GetValueOrEmpty("Older_Related_RP3"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOffice1_BS", 					attributeDict.GetValueOrEmpty("Lead_Office1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOffice2_BS", 					attributeDict.GetValueOrEmpty("Lead_Office2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOffice3_BS", 					attributeDict.GetValueOrEmpty("Lead_Office3"))					
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePOC1_BS", 				attributeDict.GetValueOrEmpty("Lead_Office_POC1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePOC2_BS", 				attributeDict.GetValueOrEmpty("Lead_Office_POC2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePOC3_BS", 				attributeDict.GetValueOrEmpty("Lead_Office_POC3"))					
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePhone1_BS", 			attributeDict.GetValueOrEmpty("Lead_Office_Phone1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePhone2_BS", 			attributeDict.GetValueOrEmpty("Lead_Office_Phone2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePhone3_BS", 			attributeDict.GetValueOrEmpty("Lead_Office_Phone3"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_IE_K_BS", 						attributeDict.GetValueOrEmpty("Initial_Estimate"))						
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_IE_Base_Funding_BS", 				attributeDict.GetValueOrEmpty("Base_Funding"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_IE_Base_Funding_Comments_BS",		attributeDict.GetValueOrEmpty("Base_Funding_Comments"))	
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_IE_R_Base_BS", 					attributeDict.GetValueOrEmpty("Recurring_Base_Estimate"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_IE_R_Base_Comments_BS", 			attributeDict.GetValueOrEmpty("Recurring_Base_Comments"))
						
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_ExecSummary_BS", 					attributeDict.GetValueOrEmpty("Exec_Summary"))	
								
						'Set the parameters for the combo boxes in the RP Dashboard Page3
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Page3_AffectOthers_BS", 			attributeDict.GetValueOrEmpty("Affect_Others"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Page3_Alignment_BS", 				attributeDict.GetValueOrEmpty("Alignment"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Page3_DenialImpact_BS", 			attributeDict.GetValueOrEmpty("Denial_Impact"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Page3_FundingImpact_BS", 			attributeDict.GetValueOrEmpty("Funding_Impact"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Page3_Problem_BS", 				attributeDict.GetValueOrEmpty("Problem"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Page3_ROI_BS", 					attributeDict.GetValueOrEmpty("ROI"))
					
					End If 'Not globals.GetObject("attributeDict") Is Nothing

					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					Return selectionChangedTaskResult						
				'	End Select	
					
				Case "F"
					' ==== ported verbatim from BudFM_SolutionHelper.OnCbxBtnClick_GEN_F ====
						
						'Get Time from current Workflow
						Dim wfTime As String = args.NameValuePairs("WFTime")
						Dim wfScenario As String = args.NameValuePairs("WFScenario")
						Dim wfCube As String = args.NameValuePairs("WFCube")
											
						'Get the component name
						Dim componentName As String = args.ComponentInfo.Component.Name
						Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
						Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")

						' If No RP is selected, nothing to do
						If RPName = "" Then 
							Return Nothing
						End If
						
						Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)		
						Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"								
				
						'using a global function to avoid using brapi functions too many times and use api.data.calculate via a finance rule instead
						'set the script generics and parent account to be used in the global function
						globals.SetStringValue("scriptGenerics", scriptGenerics)
						globals.SetStringValue("parAccount", "RP_Attributes")					

						'Set a generic dictionary as an argument in the rule below
						Dim Dictionary As New Dictionary(Of String, String)
						
							BUDFM_AttributeSupport.GetRPAttributes(si, globals)
						
						If Not globals.GetObject("attributeDict") Is Nothing
						
							Dim attributeDict As Dictionary(Of String, String) = globals.GetObject("attributeDict")
										
							#Region "Page1"  'Set Page1 Content 
								Dim PPA As String = attributeDict.GetValueOrEmpty("PPA")
								Dim ATU As String = attributeDict.GetValueOrEmpty("ATU")
								If ATU = "" Then
									' For the very first time (after  RP is created) when the user navigates to Edit RP page,
									' this  will  the Case. 
									' Since the ATU Is always the same For F Approptiation, We will set it once
									ATU = "F"
								End If 
								Dim UII As String = attributeDict.GetValueOrEmpty("UII")
								If UII = "" Then
									' For the very first time ( after  RP is created) when the user navigates to Edit RP page,
									' this will  the Case. 
									' Since the UII Is always the same ( i.e No UII) For F Approptiation, We will set it once
									UII = "NoInvestment"
								End If 
								
								'Set thethe values for bound parameters associated with combo boxes, text boxes ..etc
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_PPA_F", PPA)
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_ATU_F", ATU)
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_UII_F", UII)
								
							#End Region
							
							#Region "Page2"
											
			'					'Set the parameters for the combo boxes in the RP Dashboard Page2	
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_FYRelatedRp1_F", 				attributeDict.GetValueOrEmpty("FY_Related_RP1"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_FYRelatedRp2_F", 				attributeDict.GetValueOrEmpty("FY_Related_RP2"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_FYRelatedRp3_F", 				attributeDict.GetValueOrEmpty("FY_Related_RP3"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_OlderRelatedRp1_F", 			attributeDict.GetValueOrEmpty("Older_Related_RP1"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_OlderRelatedRp2_F", 			attributeDict.GetValueOrEmpty("Older_Related_RP2"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_OlderRelatedRp3_F", 			attributeDict.GetValueOrEmpty("Older_Related_RP3"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOffice1_F", 				attributeDict.GetValueOrEmpty("Lead_Office1"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOffice2_F", 				attributeDict.GetValueOrEmpty("Lead_Office2"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOffice3_F", 				attributeDict.GetValueOrEmpty("Lead_Office3"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePOC1_F", 			attributeDict.GetValueOrEmpty("Lead_Office_POC1"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePOC2_F", 			attributeDict.GetValueOrEmpty("Lead_Office_POC2"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePOC3_F", 			attributeDict.GetValueOrEmpty("Lead_Office_POC3"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePhone1_F", 			attributeDict.GetValueOrEmpty("Lead_Office_Phone1"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePhone2_F", 			attributeDict.GetValueOrEmpty("Lead_Office_Phone2"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePhone3_F", 			attributeDict.GetValueOrEmpty("Lead_Office_Phone3"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_IE_K_F", 						attributeDict.GetValueOrEmpty("Initial_Estimate"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_IE_Base_Funding_F", 			attributeDict.GetValueOrEmpty("Base_Funding"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_IE_Base_Funding_Comments_F", 	attributeDict.GetValueOrEmpty("Base_Funding_Comments"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_IE_R_Base_F",					attributeDict.GetValueOrEmpty("Recurring_Base_Estimate"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_IE_R_Base_Comments_F",		attributeDict.GetValueOrEmpty("Recurring_Base_Comments"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_ExecSummary_F", 				attributeDict.GetValueOrEmpty("Exec_Summary"))

							#End Region  'Set Page2 Content
															
						End If 'Not globals.GetObject("attributeDict") Is Nothing
								
						selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
						Return selectionChangedTaskResult						
					'	End Select	
				Case "MERHCF"
					' ==== ported verbatim from BudFM_SolutionHelper.OnCbxBtnClick_GEN_MERHCF ====
					
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
										
					'Get the component name
					Dim componentName As String = args.ComponentInfo.Component.Name
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")

					' If No RP is selected, nothing to do
					If RPName = "" Then 
						Return Nothing
					End If
					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)													
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"								
					
					'using a global function to avoid using brapi functions too many times and use api.data.calculate via a finance rule instead
					'set the script generics and parent account to be used in the global function
					globals.SetStringValue("scriptGenerics", scriptGenerics)
					globals.SetStringValue("parAccount", "RP_Attributes")					

					'Set a generic dictionary as an argument in the rule below
					Dim Dictionary As New Dictionary(Of String, String)
					
						BUDFM_AttributeSupport.GetRPAttributes(si, globals)
					
					If Not globals.GetObject("attributeDict") Is Nothing
					
						Dim attributeDict As Dictionary(Of String, String) = globals.GetObject("attributeDict")
					
						'Set the parameters for the combo boxes in the RP Dashboard Page2
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_FYRelatedRp1_MERHCF", 			attributeDict.GetValueOrEmpty("FY_Related_RP1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_FYRelatedRp2_MERHCF", 			attributeDict.GetValueOrEmpty("FY_Related_RP2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_FYRelatedRp3_MERHCF", 			attributeDict.GetValueOrEmpty("FY_Related_RP3"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_OlderRelatedRp1_MERHCF", 			attributeDict.GetValueOrEmpty("Older_Related_RP1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_OlderRelatedRp2_MERHCF", 			attributeDict.GetValueOrEmpty("Older_Related_RP2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_OlderRelatedRp3_MERHCF", 			attributeDict.GetValueOrEmpty("Older_Related_RP3"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOffice1_MERHCF", 				attributeDict.GetValueOrEmpty("Lead_Office1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOffice2_MERHCF", 				attributeDict.GetValueOrEmpty("Lead_Office2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOffice3_MERHCF", 				attributeDict.GetValueOrEmpty("Lead_Office3"))				
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePOC1_MERHCF", 			attributeDict.GetValueOrEmpty("Lead_Office_POC1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePOC2_MERHCF", 			attributeDict.GetValueOrEmpty("Lead_Office_POC2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePOC3_MERHCF", 			attributeDict.GetValueOrEmpty("Lead_Office_POC3"))					
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePhone1_MERHCF", 		attributeDict.GetValueOrEmpty("Lead_Office_Phone1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePhone2_MERHCF", 		attributeDict.GetValueOrEmpty("Lead_Office_Phone2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePhone3_MERHCF", 		attributeDict.GetValueOrEmpty("Lead_Office_Phone3"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_IE_K_MERHCF", 					attributeDict.GetValueOrEmpty("Initial_Estimate"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_IE_Base_Funding_MERHCF", 			attributeDict.GetValueOrEmpty("Base_Funding"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_IE_Base_Funding_Comments_MERHCF", attributeDict.GetValueOrEmpty("Base_Funding_Comments"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_IE_R_Base_MERHCF", 				attributeDict.GetValueOrEmpty("Recurring_Base_Estimate"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_IE_R_Base_Comments_MERHCF", 		attributeDict.GetValueOrEmpty("Recurring_Base_Comments"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_ExecSummary_MERHCF", 				attributeDict.GetValueOrEmpty("Exec_Summary"))

					End If 'Not globals.GetObject("attributeDict") Is Nothing
									
					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					Return selectionChangedTaskResult						
				'	End Select	
					
				Case "MOSP"
					' ==== ported verbatim from BudFM_SolutionHelper.OnCbxBtnClick_GEN_MOSP ====
					
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
										
					'Get the component name
					Dim componentName As String = args.ComponentInfo.Component.Name
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")

					' If No RP is selected, nothing to do
					If RPName = "" Then 
						Return Nothing
					End If
					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)						
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"								
					
					'using a global function to avoid using brapi functions too many times and use api.data.calculate via a finance rule instead
					'set the script generics and parent account to be used in the global function
					globals.SetStringValue("scriptGenerics", scriptGenerics)
					globals.SetStringValue("parAccount", "RP_Attributes")					

					'Set a generic dictionary as an argument in the rule below
					Dim Dictionary As New Dictionary(Of String, String)
					
						BUDFM_AttributeSupport.GetRPAttributes(si, globals)
					
					If Not globals.GetObject("attributeDict") Is Nothing
					
						Dim attributeDict As Dictionary(Of String, String) = globals.GetObject("attributeDict")
									
						'Set the parameters for the combo boxes in the RP Dashboard Page2
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_FYRelatedRp1_MOSP", 				attributeDict.GetValueOrEmpty("FY_Related_RP1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_FYRelatedRp2_MOSP", 				attributeDict.GetValueOrEmpty("FY_Related_RP2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_FYRelatedRp3_MOSP", 				attributeDict.GetValueOrEmpty("FY_Related_RP3"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_OlderRelatedRp1_MOSP", 			attributeDict.GetValueOrEmpty("Older_Related_RP1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_OlderRelatedRp2_MOSP", 			attributeDict.GetValueOrEmpty("Older_Related_RP2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_OlderRelatedRp3_MOSP", 			attributeDict.GetValueOrEmpty("Older_Related_RP3"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOffice1_MOSP", 				attributeDict.GetValueOrEmpty("Lead_Office1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOffice2_MOSP", 				attributeDict.GetValueOrEmpty("Lead_Office2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOffice3_MOSP", 				attributeDict.GetValueOrEmpty("Lead_Office3"))					
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePOC1_MOSP", 			attributeDict.GetValueOrEmpty("Lead_Office_POC1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePOC2_MOSP", 			attributeDict.GetValueOrEmpty("Lead_Office_POC2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePOC3_MOSP", 			attributeDict.GetValueOrEmpty("Lead_Office_POC3"))					
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePhone1_MOSP", 			attributeDict.GetValueOrEmpty("Lead_Office_Phone1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePhone2_MOSP", 			attributeDict.GetValueOrEmpty("Lead_Office_Phone2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePhone3_MOSP", 			attributeDict.GetValueOrEmpty("Lead_Office_Phone3"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_IE_K_MOSP", 						attributeDict.GetValueOrEmpty("Initial_Estimate"))	
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_IE_Base_Funding_MOSP", 			attributeDict.GetValueOrEmpty("Base_Funding"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_IE_Base_Funding_Comments_MOSP", 	attributeDict.GetValueOrEmpty("Base_Funding_Comments"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_IE_R_Base_MOSP", 					attributeDict.GetValueOrEmpty("Recurring_Base_Estimate"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_IE_R_Base_Comments_MOSP", 		attributeDict.GetValueOrEmpty("Recurring_Base_Comments"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_ExecSummary_MOSP", 				attributeDict.GetValueOrEmpty("Exec_Summary"))
						
					End If 'Not globals.GetObject("attributeDict") Is Nothing
								
					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					Return selectionChangedTaskResult						
				'	End Select	
					
				Case "PCI"
					' ==== ported verbatim from BudFM_SolutionHelper.OnCbxBtnClick_GEN_PCI ====
					
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
					'Dim RP_Entity As String = args.NameValuePairs("WFText1")
					Dim project_Number As String = args.NameValuePairs("Project_Number")
					
					'Get the component name
					Dim componentName As String = args.ComponentInfo.Component.Name
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					' If No RP is selected, nothing to do
					If RPName = "" Then 
						Return Nothing
					End If
					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)						
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"								
		
					'using a global function to avoid using brapi functions too many times and use api.data.calculate via a finance rule instead
					'set the script generics and parent account to be used in the global function
					globals.SetStringValue("scriptGenerics", scriptGenerics)
					globals.SetStringValue("parAccount", "RP_Attributes")					

					'Set a generic dictionary as an argument in the rule below
					Dim Dictionary As New Dictionary(Of String, String)
					
						BUDFM_AttributeSupport.GetRPAttributes(si, globals)
					
					If Not globals.GetObject("attributeDict") Is Nothing
					
						Dim attributeDict As Dictionary(Of String, String) = globals.GetObject("attributeDict")
												
						'Logic to show different dashboard depending on appropriation type
						Dim dBToShow_EditRPPage3 As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_EditRP_Page3_ProcAcq_PCI")
						If (attributeDict.GetValueOrEmpty("PPA_Level1_PCI") = "PCI_SFATON") Then
							dbToShow_EditRPPage3 = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_EditRP_Page3_Constr_PCI")
						End If
						
						If ((attributeDict.GetValueOrEmpty("PPA_Level1_PCI") = "PCI_OTHER") And (attributeDict.GetValueOrEmpty("PPA_Level2_PCI") = "PCI_OTHER_ES")) Then
							dbToShow_EditRPPage3 = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_EditRP_Page3_EndItems_PCI")
						End If
						
						'Set Parameters for dashboard change based on appropriation type
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Content_EditRP_Page3_PCI", dBToShow_EditRPPage3)
									
						'Set the parameters for the combo boxes in the RP Dashboard Page1
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_PPA_Level1_PCI", 					attributeDict.GetValueOrEmpty("PPA_Level1_PCI"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_PPA_Level2_PCI", 					attributeDict.GetValueOrEmpty("PPA_Level2_PCI"))
						
						'Set the parameters for the combo boxes in the RP Dashboard Page2
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_FYRelatedRp1_PCI", 				attributeDict.GetValueOrEmpty("FY_Related_RP1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_FYRelatedRp2_PCI", 				attributeDict.GetValueOrEmpty("FY_Related_RP2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_FYRelatedRp3_PCI", 				attributeDict.GetValueOrEmpty("FY_Related_RP3"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_OlderRelatedRp1_PCI", 			attributeDict.GetValueOrEmpty("Older_Related_RP1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_OlderRelatedRp2_PCI", 			attributeDict.GetValueOrEmpty("Older_Related_RP2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_OlderRelatedRp3_PCI", 			attributeDict.GetValueOrEmpty("Older_Related_RP3"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOffice1_PCI", 				attributeDict.GetValueOrEmpty("Lead_Office1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOffice2_PCI", 				attributeDict.GetValueOrEmpty("Lead_Office2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOffice3_PCI", 				attributeDict.GetValueOrEmpty("Lead_Office3"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePOC1_PCI", 				attributeDict.GetValueOrEmpty("Lead_Office_POC1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePOC2_PCI", 				attributeDict.GetValueOrEmpty("Lead_Office_POC2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePOC3_PCI", 				attributeDict.GetValueOrEmpty("Lead_Office_POC3"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePhone1_PCI", 			attributeDict.GetValueOrEmpty("Lead_Office_Phone1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePhone2_PCI", 			attributeDict.GetValueOrEmpty("Lead_Office_Phone2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePhone3_PCI", 			attributeDict.GetValueOrEmpty("Lead_Office_Phone3"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_IE_K_PCI", 						attributeDict.GetValueOrEmpty("Initial_Estimate"))				
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_IE_Base_Funding_PCI", 			attributeDict.GetValueOrEmpty("Base_Funding"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_IE_Base_Funding_Comments_PCI", 	attributeDict.GetValueOrEmpty("Base_Funding_Comments"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_IE_R_Base_PCI", 					attributeDict.GetValueOrEmpty("Recurring_Base_Estimate"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_IE_R_Base_Comments_PCI", 			attributeDict.GetValueOrEmpty("Recurring_Base_Comments"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_ExecSummary_PCI", 				attributeDict.GetValueOrEmpty("Exec_Summary"))
												
						'Set the parameters for the combo boxes in the RP Dashboard Page3
						'Generic Page3
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Investment_Description_PCI", 		attributeDict.GetValueOrEmpty("Invest_Desc_PCI"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Justification_PCI", 				attributeDict.GetValueOrEmpty("Justification_PCI"))
						
						'Proq/Acq Words Page 3
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_KeyMilestones_PY_PCI", 			attributeDict.GetValueOrEmpty("KeyMilestones_PY_PCI"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_KeyMilestones_CY_PCI", 			attributeDict.GetValueOrEmpty("KeyMilestones_CY_PCI"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_KeyMilestones_BY_PCI", 			attributeDict.GetValueOrEmpty("KeyMilestones_BY_PCI"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_SigChanges_PCI", 					attributeDict.GetValueOrEmpty("SignificantChanges_PCI"))
									
					End If 'Not globals.GetObject("attributeDict") Is Nothing
										
					'Constr Words - has to go in a separate section due to UD8 being used to denote project number
					Dim scriptGenericsWUD8 As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#" & project_Number
					
					'using a global function to avoid using brapi functions too many times and use api.data.calculate via a finance rule instead
					'set the script generics and parent account to be used in the global function
					globals.SetStringValue("scriptGenerics", scriptGenericsWUD8)
					globals.SetStringValue("parAccount", "RP_Attributes")					

					'Set a generic dictionary as an argument in the rule below
					Dim ConstrDictionary As New Dictionary(Of String, String)
					
						BUDFM_AttributeSupport.GetRPAttributes(si, globals)
					
					If Not globals.GetObject("attributeDict") Is Nothing
					
						Dim attributeDictConstr As Dictionary(Of String, String) = globals.GetObject("attributeDict")
											
						'Constr Words
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Project_FundReq_PCI", 			attributeDictConstr.GetValueOrEmpty("Project_FundReq_PCI"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Project_Description_PCI", 		attributeDictConstr.GetValueOrEmpty("Project_Desc_PCI"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Project_Justification_PCI", 		attributeDictConstr.GetValueOrEmpty("Project_Justification_PCI"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Project_Impact_PCI", 				attributeDictConstr.GetValueOrEmpty("Project_Impact_PCI"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Project_ContrSolic_PCI", 			attributeDictConstr.GetValueOrEmpty("Project_ContrSolic_PCI"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Project_DBConstrAward_PCI", 		attributeDictConstr.GetValueOrEmpty("Project_DBConstrAward_PCI"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Project_ConstrStart_PCI", 		attributeDictConstr.GetValueOrEmpty("Project_ConstrStart_PCI"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Project_ConstrComplete_PCI", 		attributeDictConstr.GetValueOrEmpty("Project_ConstrComplete_PCI"))
						
					End If 'Not globals.GetObject("attributeDict") Is Nothing
										
					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					Return selectionChangedTaskResult	
					
				Case "RD"
					' ==== ported verbatim from BudFM_SolutionHelper.OnCbxBtnClick_GEN_RD ====
						
						'Get Time from current Workflow
						Dim wfTime As String = args.NameValuePairs("WFTime")
						Dim wfScenario As String = args.NameValuePairs("WFScenario")
						Dim wfCube As String = args.NameValuePairs("WFCube")
											
						'Get the component name
						Dim componentName As String = args.ComponentInfo.Component.Name
						Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
						Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")

						' If No RP is selected, nothing to do
						If RPName = "" Then 
							Return Nothing
						End If
						
						Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)							
						Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"								
				
						'using a global function to avoid using brapi functions too many times and use api.data.calculate via a finance rule instead
						'set the script generics and parent account to be used in the global function
						globals.SetStringValue("scriptGenerics", scriptGenerics)
						globals.SetStringValue("parAccount", "RP_Attributes")					

						'Set a generic dictionary as an argument in the rule below
						Dim Dictionary As New Dictionary(Of String, String)
						
							BUDFM_AttributeSupport.GetRPAttributes(si, globals)
						
						If Not globals.GetObject("attributeDict") Is Nothing
						
							Dim attributeDict As Dictionary(Of String, String) = globals.GetObject("attributeDict")
								
						#Region "Page1"  'Set Page1 Content 
							'
							' RP Page 1
							' Get info for the RP Annotation Accounts using a member script 
							' 
							Dim PPA As String = attributeDict.GetValueOrEmpty("PPA")

							Dim ATU As String = attributeDict.GetValueOrEmpty("ATU")
							If ATU = "" Then
								' For the very first time (after  RP is created) when the user navigates to Edit RP page,
								' this  will  the Case. 
								' Since the ATU Is always the same For RD Approptiation, We will set it once
								ATU = "RD"
							End If 

							Dim UII As String = attributeDict.GetValueOrEmpty("UII")
							If UII = "" Then
								' For the very first time ( after  RP is created) when the user navigates to Edit RP page,
								' this will  the Case. 
								' Since the UII Is always the same ( i.e No UII) For RD Approptiation, We will set it once
								UII = "NoInvestment"
							End If 
							
							'Set thethe values for bound parameters associated with combo boxes, text boxes ..etc
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_PPA_RD", PPA)
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_ATU_RD", ATU)
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_UII_RD", UII)
							
						#End Region
						
						#Region "Page2"
						
										
		'					'Set the parameters for the combo boxes in the RP Dashboard Page2
		
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_FYRelatedRp1_RD", 			attributeDict.GetValueOrEmpty("FY_Related_RP1"))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_FYRelatedRp2_RD", 			attributeDict.GetValueOrEmpty("FY_Related_RP2"))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_FYRelatedRp3_RD", 			attributeDict.GetValueOrEmpty("FY_Related_RP3"))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_OlderRelatedRp1_RD", 			attributeDict.GetValueOrEmpty("Older_Related_RP1"))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_OlderRelatedRp2_RD", 			attributeDict.GetValueOrEmpty("Older_Related_RP2"))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_OlderRelatedRp3_RD", 			attributeDict.GetValueOrEmpty("Older_Related_RP3"))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOffice1_RD", 				attributeDict.GetValueOrEmpty("Lead_Office1"))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOffice2_RD", 				attributeDict.GetValueOrEmpty("Lead_Office2"))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOffice3_RD", 				attributeDict.GetValueOrEmpty("Lead_Office3"))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePOC1_RD", 			attributeDict.GetValueOrEmpty("Lead_Office_POC1"))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePOC2_RD", 			attributeDict.GetValueOrEmpty("Lead_Office_POC2"))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePOC3_RD", 			attributeDict.GetValueOrEmpty("Lead_Office_POC3"))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePhone1_RD", 		attributeDict.GetValueOrEmpty("Lead_Office_Phone1"))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePhone2_RD", 		attributeDict.GetValueOrEmpty("Lead_Office_Phone2"))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePhone3_RD", 		attributeDict.GetValueOrEmpty("Lead_Office_Phone3"))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Initial_Estimate_RD", 		attributeDict.GetValueOrEmpty("Initial_Estimate"))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Base_Funding_RD", 			attributeDict.GetValueOrEmpty("Base_Funding"))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Base_Funding_Comments_RD", 	attributeDict.GetValueOrEmpty("Base_Funding_Comments"))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Recurring_Base_Estimate_RD",	attributeDict.GetValueOrEmpty("Recurring_Base_Estimate"))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Recurring_Base_Comments_RD",	attributeDict.GetValueOrEmpty("Recurring_Base_Comments"))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_ExecSummary_RD", 				attributeDict.GetValueOrEmpty("Exec_Summary"))

		
						#End Region  'Set Page2 Content
							
						
						#Region "Page3" 'Set Page3 Content
						
						
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Project_Name_RD", 		"")
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Project_Description_RD", 	attributeDict.GetValueOrEmpty("Project_Description"))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Problem_RD", 				attributeDict.GetValueOrEmpty("Problem"))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Solution_RD", 			attributeDict.GetValueOrEmpty("Solution"))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Justification_RD", 		attributeDict.GetValueOrEmpty("Justification"))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Impact_On_Performance_RD",attributeDict.GetValueOrEmpty("Impact_On_Performance"))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Type_Of_Research_RD", 	attributeDict.GetValueOrEmpty("Type_Of_Research"))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Tech_Readiness_Level_RD", attributeDict.GetValueOrEmpty("Tech_Readiness_Level"))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Transition_Plans_RD", 	attributeDict.GetValueOrEmpty("Transition_Plans"))
						
						#End Region
							
						End If 'Not globals.GetObject("attributeDict") Is Nothing
								
						selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
						Return selectionChangedTaskResult						
					'	End Select	
				Case "RP"
					' ==== ported verbatim from BudFM_SolutionHelper.OnCbxBtnClick_GEN_RP ====
					
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
										
					'Get the component name
					Dim componentName As String = args.ComponentInfo.Component.Name
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					' If No RP is selected, nothing to do
					If RPName = "" Then 
						Return Nothing
					End If
					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)												
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"								
					
					'using a global function to avoid using brapi functions too many times and use api.data.calculate via a finance rule instead
					'set the script generics and parent account to be used in the global function
					globals.SetStringValue("scriptGenerics", scriptGenerics)
					globals.SetStringValue("parAccount", "RP_Attributes")					

					'Set a generic dictionary as an argument in the rule below
					Dim Dictionary As New Dictionary(Of String, String)
					
						BUDFM_AttributeSupport.GetRPAttributes(si, globals)
					
					If Not globals.GetObject("attributeDict") Is Nothing
					
						Dim attributeDict As Dictionary(Of String, String) = globals.GetObject("attributeDict")
										
						'Set the parameters for the combo boxes in the RP Dashboard Page2
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_FYRelatedRp1_RP", 				attributeDict.GetValueOrEmpty("FY_Related_RP1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_FYRelatedRp2_RP", 				attributeDict.GetValueOrEmpty("FY_Related_RP2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_FYRelatedRp3_RP", 				attributeDict.GetValueOrEmpty("FY_Related_RP3"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_OlderRelatedRp1_RP", 				attributeDict.GetValueOrEmpty("Older_Related_RP1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_OlderRelatedRp2_RP", 				attributeDict.GetValueOrEmpty("Older_Related_RP2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_OlderRelatedRp3_RP", 				attributeDict.GetValueOrEmpty("Older_Related_RP3"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOffice1_RP", 					attributeDict.GetValueOrEmpty("Lead_Office1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOffice2_RP", 					attributeDict.GetValueOrEmpty("Lead_Office2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOffice3_RP", 					attributeDict.GetValueOrEmpty("Lead_Office3"))					
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePOC1_RP", 				attributeDict.GetValueOrEmpty("Lead_Office_POC1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePOC2_RP", 				attributeDict.GetValueOrEmpty("Lead_Office_POC2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePOC3_RP", 				attributeDict.GetValueOrEmpty("Lead_Office_POC3"))					
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePhone1_RP", 			attributeDict.GetValueOrEmpty("Lead_Office_Phone1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePhone2_RP", 			attributeDict.GetValueOrEmpty("Lead_Office_Phone2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePhone3_RP", 			attributeDict.GetValueOrEmpty("Lead_Office_Phone3"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_IE_K_RP", 						attributeDict.GetValueOrEmpty("Initial_Estimate"))	
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_IE_Base_Funding_RP", 				attributeDict.GetValueOrEmpty("Base_Funding"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_IE_Base_Funding_Comments_RP", 	attributeDict.GetValueOrEmpty("Base_Funding_Comments"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_IE_R_Base_RP", 					attributeDict.GetValueOrEmpty("Recurring_Base_Estimate"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_IE_R_Base_Comments_RP", 			attributeDict.GetValueOrEmpty("Recurring_Base_Comments"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_ExecSummary_RP", 					attributeDict.GetValueOrEmpty("Exec_Summary"))
		
					End If 'Not globals.GetObject("attributeDict") Is Nothing
								
					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					Return selectionChangedTaskResult						
				'	End Select	
					
				Case Else
					Throw New XFException(si, New Exception("OnCbxBtnClick_GEN: unknown appropriation '" & rpAppr & "'"))
			End Select
			Return Nothing
		End Function
		Private Function OnCbxRP_Expense_Selected(ByVal rpAppr As String) As Object
			' Variant bodies kept verbatim per appropriation (collapse later
			' only where a diff proves the variants identical).
			Select Case rpAppr
				Case "BS"
					' ==== ported verbatim from BudFM_SolutionHelper.OnCbxRP_Expense_Selected_BS ====
			
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
										
					'Get the component name
					Dim componentName As String = args.ComponentInfo.Component.Name
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")

					' If No RP is selected, nothing to do
					If RPName = "" Then 
						Return Nothing
					End If
					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)					
					
					Dim LINumber As String = args.NameValuePairs.XFGetValue("LINumber")
					
					'Logic to set the default line item when the Billet screen is opened
					Dim LINumberToSet As String = String.Empty
					If LINumber.Length > 0 Then
						LINumberToSet = LINumber	

					Else
						LINumberToSet = "EXPLineItem_01"

					End If
						'set the line item based on the above logic							
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_LineItemNumber_BS", LINumberToSet)					
					
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumberToSet & ":U7#None:U8#None"			
					Dim scriptGenericsDescr As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_Input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U6#"& LINumberToSet & ":U7#None:U8#None"				
		
					'using a global function to avoid using brapi functions too many times and use api.data.calculate via a finance rule instead
					'set the script generics and parent account to be used in the global function
					globals.SetStringValue("scriptGenerics", scriptGenerics)
					globals.SetStringValue("parAccount", "Expense_LineItem_BS")					

					'Set a generic dictionary as an argument in the rule below
					Dim Dictionary As New Dictionary(Of String, String)
					
						BUDFM_AttributeSupport.GetRPAttributes(si, globals)
					
					If Not globals.GetObject("attributeDict") Is Nothing
					
						Dim attributeDict As Dictionary(Of String, String) = globals.GetObject("attributeDict")
							
						'Get info for the Expense
						Dim Requested_Item_Cost_Line As String = attributeDict.GetValueOrEmpty("Requested_Item_Tier1")					
						'Get the ItemNum to use to find the description Input account
						Dim requested_ItemNum As Integer
						If (Not Requested_Item_Cost_Line = "") 
							Dim requested_Item_Tier1Split As List(Of String) = StringHelper.SplitString(Requested_Item_Cost_Line, "_")
							requested_ItemNum = requested_Item_Tier1Split(0).XFConvertToInt
						End If
						
						'Derive ATU from ATU_NoUnit since we stored it as a base but they chose a parent
						Dim ATU_NoUnit As String = attributeDict.GetValueOrEmpty("ATU")	
						Dim ATU As String = String.Empty
						'If it already has a value, derive the parent member from the stored NoUnit child
						If ATU_NoUnit.Length > 0
							ATU = ATU_NoUnit.Substring(0, ATU_NoUnit.Length - 7)
						'If it doesn't have a value, return the default value
						Else					
						End If					
						
						'Set Parameters for NonBillet info_section
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_RequestedItem_Tier1_BS", 			Requested_Item_Cost_Line)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_ATU_BS", 							ATU)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_Description_Tier2_BS", 			attributeDict.GetValueOrEmpty("Description_Tier2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_Description_Tier2_Input_BS", 		BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "U5#" & requested_ItemNum & "0_1:" 		& scriptGenericsDescr).DataCellEx.DataCellAnnotation)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_POC_BS", 							attributeDict.GetValueOrEmpty("POC"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_DollarKValue_BS", 				attributeDict.GetValueOrEmpty("DollarK_Value"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_RecurringNonRecurring_BS", 		attributeDict.GetValueOrEmpty("R_NR"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_PPA_BS", 							attributeDict.GetValueOrEmpty("PPA"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_UII_BS", 							attributeDict.GetValueOrEmpty("UII"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_ObjectClass_BS", 					attributeDict.GetValueOrEmpty("Object_Class"))
						
					End If 'globals.GetObject("attributeDict") Is Nothing
					
					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True		
					Return selectionChangedTaskResult
					
				Case "F"
					' ==== ported verbatim from BudFM_SolutionHelper.OnCbxRP_Expense_Selected_F ====
				
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
										
					'Get the component name
					Dim componentName As String = args.ComponentInfo.Component.Name
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					' If No RP is selected, nothing to do
					If RPName = "" Then 
						Return Nothing
					End If
					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)							
					Dim LINumber As String = args.NameValuePairs.XFGetValue("LINumber")
					
					'Logic to set the default line item when the Billet screen is opened
					Dim LINumberToSet As String = String.Empty
					If LINumber.Length > 0 Then
						LINumberToSet = LINumber	
					Else
						LINumberToSet = "EXPLineItem_01"
					End If
					
					'set the line item based on the above logic							
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_LineItemNumber_F", LINumberToSet)	
					
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumberToSet & ":U7#None:U8#None"			
					Dim scriptGenericsDescr As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_Input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U6#"& LINumberToSet & ":U7#None:U8#None"				
		
					'using a global function to avoid using brapi functions too many times and use api.data.calculate via a finance rule instead
					'set the script generics and parent account to be used in the global function
					globals.SetStringValue("scriptGenerics", scriptGenerics)
					globals.SetStringValue("parAccount", "Expense_LineItem_RD")					

					'Set a generic dictionary as an argument in the rule below
					Dim Dictionary As New Dictionary(Of String, String)
					
						BUDFM_AttributeSupport.GetRPAttributes(si, globals)
					
					If Not globals.GetObject("attributeDict") Is Nothing
					
						Dim attributeDict As Dictionary(Of String, String) = globals.GetObject("attributeDict")
									
						'Get info for the Expense
						Dim Requested_Item_Cost_Line As String = attributeDict.GetValueOrEmpty("Requested_Item_Tier1")
						'Get the ItemNum to use to find the description Input account
						Dim requested_ItemNum As Integer
						If (Not Requested_Item_Cost_Line = "") 
							Dim requested_Item_Tier1Split As List(Of String) = StringHelper.SplitString(Requested_Item_Cost_Line, "_")
							requested_ItemNum = requested_Item_Tier1Split(0).XFConvertToInt
						End If
						
						'Derive ATU from ATU_NoUnit since we stored it as a base but they chose a parent
						Dim ATU_NoUnit As String = attributeDict.GetValueOrEmpty("ATU")		
						Dim ATU As String = String.Empty
						'If it already has a value, derive the parent member from the stored NoUnit child
						If ATU_NoUnit.Length > 0
							ATU = ATU_NoUnit.Substring(0, ATU_NoUnit.Length - 7)
						'If it doesn't have a value, return the default value
						Else					
						End If		
						
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_RequestedItem_Tier1_F", 		Requested_Item_Cost_Line)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_Description_Tier2_F", 		attributeDict.GetValueOrEmpty("Description_Tier2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_Description_Tier2_Input_F", 	BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "U5#" & requested_ItemNum & "0_1:" & scriptGenericsDescr).DataCellEx.DataCellAnnotation)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_POC_F", 						attributeDict.GetValueOrEmpty("POC"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_DollarKValue_F", 				attributeDict.GetValueOrEmpty("DollarK_Value"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_RecurringNonRecurring_F", 	attributeDict.GetValueOrEmpty("R_NR"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_ATU_F", 						ATU)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_PPA_F", 						attributeDict.GetValueOrEmpty("PPA"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_UII_F", 						attributeDict.GetValueOrEmpty("UII"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_ObjectClass_F", 				attributeDict.GetValueOrEmpty("Object_Class"))
						
					End If 'Not globals.GetObject("attributeDict") Is Nothing
								
					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True						
					Return selectionChangedTaskResult
					
				Case "MERHCF"
					' ==== ported verbatim from BudFM_SolutionHelper.OnCbxRP_Expense_Selected_MERHCF ====
				
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
										
					'Get the component name
					Dim componentName As String = args.ComponentInfo.Component.Name
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")

					' If No RP is selected, nothing to do
					If RPName = "" Then 
						Return Nothing
					End If
					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)						
					Dim LINumber As String = args.NameValuePairs.XFGetValue("LINumber")
					
					'Logic to set the default line item when the Billet screen is opened
					Dim LINumberToSet As String = String.Empty
					If LINumber.Length > 0 Then
						LINumberToSet = LINumber	

					Else
						LINumberToSet = "EXPLineItem_01"

					End If
					
					'set the line item based on the above logic							
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_LineItemNumber_MERHCF", LINumberToSet)						
					
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumberToSet & ":U7#None:U8#None"			
					Dim scriptGenericsDescr As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_Input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U6#"& LINumberToSet & ":U7#None:U8#None"				
		
					'using a global function to avoid using brapi functions too many times and use api.data.calculate via a finance rule instead
					'set the script generics and parent account to be used in the global function
					globals.SetStringValue("scriptGenerics", scriptGenerics)
					globals.SetStringValue("parAccount", "Expense_LineItem_RD")					

					'Set a generic dictionary as an argument in the rule below
					Dim Dictionary As New Dictionary(Of String, String)
					
						BUDFM_AttributeSupport.GetRPAttributes(si, globals)
					
					If Not globals.GetObject("attributeDict") Is Nothing
					
						Dim attributeDict As Dictionary(Of String, String) = globals.GetObject("attributeDict")
									
						'Get info for the Expense
						Dim Requested_Item_Cost_Line As String = attributeDict.GetValueOrEmpty("Requested_Item_Tier1")
						'Get the ItemNum to use to find the description Input account
						Dim requested_ItemNum As Integer
						If (Not Requested_Item_Cost_Line = "") 
							Dim requested_Item_Tier1Split As List(Of String) = StringHelper.SplitString(Requested_Item_Cost_Line, "_")
							requested_ItemNum = requested_Item_Tier1Split(0).XFConvertToInt
						End If
						
						'Derive ATU from ATU_NoUnit since we stored it as a base but they chose a parent
						Dim ATU_NoUnit As String = attributeDict.GetValueOrEmpty("ATU")		
						Dim ATU As String = String.Empty
						'If it already has a value, derive the parent member from the stored NoUnit child
						If ATU_NoUnit.Length > 0
							ATU = ATU_NoUnit.Substring(0, ATU_NoUnit.Length - 7)
						'If it doesn't have a value, return the default value
						Else					
						End If		
						
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_RequestedItem_Tier1_MERHCF", 		Requested_Item_Cost_Line)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_Description_Tier2_MERHCF", 		attributeDict.GetValueOrEmpty("Description_Tier2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_Description_Tier2_Input_MERHCF", 	BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "U5#" & requested_ItemNum & "0_1:" & scriptGenericsDescr).DataCellEx.DataCellAnnotation)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_POC_MERHCF",						attributeDict.GetValueOrEmpty("POC"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_DollarKValue_MERHCF", 			attributeDict.GetValueOrEmpty("DollarK_Value"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_RecurringNonRecurring_MERHCF", 	attributeDict.GetValueOrEmpty("R_NR"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_ATU_MERHCF", 						ATU)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_PPA_MERHCF", 						attributeDict.GetValueOrEmpty("PPA"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_UII_MERHCF", 						attributeDict.GetValueOrEmpty("UII"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_ObjectClass_MERHCF", 				attributeDict.GetValueOrEmpty("Object_Class"))
					
					End If 'Not globals.GetObject("attributeDict") Is Nothing
								
					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					Return selectionChangedTaskResult
					
				Case "MOSP"
					' ==== ported verbatim from BudFM_SolutionHelper.OnCbxRP_Expense_Selected_MOSP ====
				
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
										
					'Get the component name
					Dim componentName As String = args.ComponentInfo.Component.Name
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					' If No RP is selected, nothing to do
					If RPName = "" Then 
						Return Nothing
					End If
					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)					
					
					Dim LINumber As String = args.NameValuePairs.XFGetValue("LINumber")
					
					'Logic to set the default line item when the Billet screen is opened
					Dim LINumberToSet As String = String.Empty
					If LINumber.Length > 0 Then
						LINumberToSet = LINumber	

					Else
						LINumberToSet = "EXPLineItem_01"

					End If
					
					'set the line item based on the above logic							
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_LineItemNumber_MOSP", LINumberToSet)						
					
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumberToSet & ":U7#None:U8#None"			
					Dim scriptGenericsDescr As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_Input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U6#"& LINumberToSet & ":U7#None:U8#None"				
		
					'using a global function to avoid using brapi functions too many times and use api.data.calculate via a finance rule instead
					'set the script generics and parent account to be used in the global function
					globals.SetStringValue("scriptGenerics", scriptGenerics)
					globals.SetStringValue("parAccount", "Expense_LineItem_RD")					

					'Set a generic dictionary as an argument in the rule below
					Dim Dictionary As New Dictionary(Of String, String)
					
						BUDFM_AttributeSupport.GetRPAttributes(si, globals)
					
					If Not globals.GetObject("attributeDict") Is Nothing
					
						Dim attributeDict As Dictionary(Of String, String) = globals.GetObject("attributeDict")
									
						'Get info for the Expense
						Dim Requested_Item_Cost_Line As String = attributeDict.GetValueOrEmpty("Requested_Item_Tier1")
						'Get the ItemNum to use to find the description Input account
						Dim requested_ItemNum As Integer
						If (Not Requested_Item_Cost_Line = "") 
							Dim requested_Item_Tier1Split As List(Of String) = StringHelper.SplitString(Requested_Item_Cost_Line, "_")
							requested_ItemNum = requested_Item_Tier1Split(0).XFConvertToInt
						End If
						
						'Derive ATU from ATU_NoUnit since we stored it as a base but they chose a parent
						Dim ATU_NoUnit As String = attributeDict.GetValueOrEmpty("ATU")		
						Dim ATU As String = String.Empty
						'If it already has a value, derive the parent member from the stored NoUnit child
						If ATU_NoUnit.Length > 0
							ATU = ATU_NoUnit.Substring(0, ATU_NoUnit.Length - 7)
						'If it doesn't have a value, return the default value
						Else					
						End If		
						
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_RequestedItem_Tier1_MOSP", 		Requested_Item_Cost_Line)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_Description_Tier2_MOSP", 			attributeDict.GetValueOrEmpty("Description_Tier2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_Description_Tier2_Input_MOSP", 	BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "U5#" & requested_ItemNum & "0_1:" & scriptGenericsDescr).DataCellEx.DataCellAnnotation)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_POC_MOSP", 						attributeDict.GetValueOrEmpty("POC"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_DollarKValue_MOSP", 				attributeDict.GetValueOrEmpty("DollarK_Value"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_RecurringNonRecurring_MOSP", 		attributeDict.GetValueOrEmpty("R_NR"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_ATU_MOSP", 						ATU)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_PPA_MOSP", 						attributeDict.GetValueOrEmpty("PPA"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_UII_MOSP", 						attributeDict.GetValueOrEmpty("UII"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_ObjectClass_MOSP", 				attributeDict.GetValueOrEmpty("Object_Class"))
						
					End If 'Not globals.GetObject("attributeDict") Is Nothing
								
					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True						
					Return selectionChangedTaskResult
					
				Case "PCI"
					' ==== ported verbatim from BudFM_SolutionHelper.OnCbxRP_Expense_Selected_PCI ====
				
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					Dim returnValue As String = String.Empty

					' If No RP is selected, nothing to do
					If String.IsNullOrEmpty(RPName) Then Return returnValue
					
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()				
							
					'Remove the first three digits from the RP as the should be YY_ and that should give you the PPA
					Dim PPA As String = RPName.Substring(3, RPName.Length - 3)
					'Get the memberId from the name
					Dim PPAMemberID As Integer = BRApi.Finance.Members.GetMemberInfo(si, dimtypeid.UD1, PPA).Member.MemberId
					Dim objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "Std_PPA")
					Dim PPAMemberHasChildren As Boolean = BRApi.Finance.Members.HasChildren(si, objDimPk, PPAMemberID)
					
					'If the member has children, return a member filter with the children, else return the member itself
					If PPAMemberHasChildren 
						'Get a list of children and return the first one
						Dim PPAChildren As List(Of Member) = BRApi.Finance.Members.GetChildren(si, objDimPk, PPAMemberID)
						returnValue = PPAChildren(0).Name
					Else 
						returnValue = PPA
					End If
					
					'set the line item based on the above logic							
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_PPA_Selection_PCI", returnValue)								
					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					Return selectionChangedTaskResult
					
				Case "RD"
					' ==== ported verbatim from BudFM_SolutionHelper.OnCbxRP_Expense_Selected_RD ====
	
			
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
										
					'Get the component name
					Dim componentName As String = args.ComponentInfo.Component.Name
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					' If No RP is selected, nothing to do
					If RPName = "" Then 
						Return Nothing
					End If
					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)		
					
					Dim LINumber As String = args.NameValuePairs.XFGetValue("LINumber")
					
					'Logic to set the default line item when the Billet screen is opened
					Dim LINumberToSet As String = String.Empty
					If LINumber.Length > 0 Then
						LINumberToSet = LINumber	

					Else
						LINumberToSet = "EXPLineItem_01"

					End If
					
					'set the line item based on the above logic							
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_LineItemNumber_RD", LINumberToSet)	
								
					Dim scriptGenerics  = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumberToSet & ":U7#None:U8#None"			
					Dim scriptGenericsDescr As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_Input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U6#"& LINumberToSet & ":U7#None:U8#None"				
		
					'using a global function to avoid using brapi functions too many times and use api.data.calculate via a finance rule instead
					'set the script generics and parent account to be used in the global function
					globals.SetStringValue("scriptGenerics", scriptGenerics)
					globals.SetStringValue("parAccount", "Expense_LineItem_RD")					

					'Set a generic dictionary as an argument in the rule below
					Dim Dictionary As New Dictionary(Of String, String)
					
						BUDFM_AttributeSupport.GetRPAttributes(si, globals)
					
					If Not globals.GetObject("attributeDict") Is Nothing
					
						Dim attributeDict As Dictionary(Of String, String) = globals.GetObject("attributeDict")
								
						'Get info for the Non-Billet
						Dim Requested_Item_Cost_Line As String = attributeDict.GetValueOrEmpty("Requested_Item_Tier1")
						
						'Get the ItemNum to use to find the description Input account
						Dim requested_ItemNum As Integer
						If (Not Requested_Item_Cost_Line = "") 
							Dim requested_Item_Tier1Split As List(Of String) = StringHelper.SplitString(Requested_Item_Cost_Line, "_")
							requested_ItemNum = requested_Item_Tier1Split(0).XFConvertToInt
						End If
						
						'Derive ATU from ATU_NoUnit since we stored it as a base but they chose a parent
						Dim ATU_NoUnit As String = attributeDict.GetValueOrEmpty("ATU")
						Dim ATU As String = String.Empty
						
						'If it already has a value, derive the parent member from the stored NoUnit child
						If ATU_NoUnit.Length > 0
							ATU = ATU_NoUnit.Substring(0, ATU_NoUnit.Length - 7)
						'If it doesn't have a value, return the default value
						Else
							
						End If
							
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_RequestedItem_Tier1_RD", 		Requested_Item_Cost_Line)
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_Description_Tier2_RD",		attributeDict.GetValueOrEmpty("Description_Tier2"))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_Description_Tier2_Input_RD",	BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "U5#"& requested_ItemNum & "0_1:" 	& scriptGenericsDescr).DataCellEx.DataCellAnnotation)
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_POC_RD", 						attributeDict.GetValueOrEmpty("POC"))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_DollarKValue_RD", 			attributeDict.GetValueOrEmpty("DollarK_Value"))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_BY_Obligations_RD", 			attributeDict.GetValueOrEmpty("BY_Obligations"))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_BY_Plus1_Obligations_RD", 	attributeDict.GetValueOrEmpty("By_Plus1_Obligations"))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_BY_Plus2_Obligations_RD", 	attributeDict.GetValueOrEmpty("By_Plus2_Obligations"))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_RecurringNonRecurring_RD",	attributeDict.GetValueOrEmpty("R_NR"))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_ATU_RD", 						ATU)
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_PPA_RD", 						attributeDict.GetValueOrEmpty("PPA"))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_UII_RD", 						attributeDict.GetValueOrEmpty("UII"))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_ObjectClass_RD", 				attributeDict.GetValueOrEmpty("Object_Class"))

						End If 'Not globals.GetObject("attributeDict") Is Nothing
								
						selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
						Return selectionChangedTaskResult
					
				Case "RP"
					' ==== ported verbatim from BudFM_SolutionHelper.OnCbxRP_Expense_Selected_RP ====
				
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
										
					'Get the component name
					Dim componentName As String = args.ComponentInfo.Component.Name
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					' If No RP is selected, nothing to do
					If RPName = "" Then 
						Return Nothing
					End If
					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)					
					
					Dim LINumber As String = args.NameValuePairs.XFGetValue("LINumber")
					
					'Logic to set the default line item when the Billet screen is opened
					Dim LINumberToSet As String = String.Empty
					If LINumber.Length > 0 Then
						LINumberToSet = LINumber	

					Else
						LINumberToSet = "EXPLineItem_01"

					End If
						'set the line item based on the above logic							
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_LineItemNumber_RP", LINumberToSet)	
					
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumberToSet & ":U7#None:U8#None"			
					Dim scriptGenericsDescr As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_Input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U6#"& LINumberToSet & ":U7#None:U8#None"				
		
					'using a global function to avoid using brapi functions too many times and use api.data.calculate via a finance rule instead
					'set the script generics and parent account to be used in the global function
					globals.SetStringValue("scriptGenerics", scriptGenerics)
					globals.SetStringValue("parAccount", "Expense_LineItem_RD")					

					'Set a generic dictionary as an argument in the rule below
					Dim Dictionary As New Dictionary(Of String, String)
					
						BUDFM_AttributeSupport.GetRPAttributes(si, globals)
					
					If Not globals.GetObject("attributeDict") Is Nothing
					
						Dim attributeDict As Dictionary(Of String, String) = globals.GetObject("attributeDict")
									
						'Get info for the Expense
						Dim Requested_Item_Cost_Line As String = attributeDict.GetValueOrEmpty("Requested_Item_Tier1")
						'Get the ItemNum to use to find the description Input account
						Dim requested_ItemNum As Integer
						If (Not Requested_Item_Cost_Line = "") 
							Dim requested_Item_Tier1Split As List(Of String) = StringHelper.SplitString(Requested_Item_Cost_Line, "_")
							requested_ItemNum = requested_Item_Tier1Split(0).XFConvertToInt
						End If
						
						'Derive ATU from ATU_NoUnit since we stored it as a base but they chose a parent
						Dim ATU_NoUnit As String = attributeDict.GetValueOrEmpty("ATU")		
						Dim ATU As String = String.Empty
						'If it already has a value, derive the parent member from the stored NoUnit child
						If ATU_NoUnit.Length > 0
							ATU = ATU_NoUnit.Substring(0, ATU_NoUnit.Length - 7)
						'If it doesn't have a value, return the default value
						Else					
						End If		
						
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_RequestedItem_Tier1_RP", 		Requested_Item_Cost_Line)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_Description_Tier2_RP", 		attributeDict.GetValueOrEmpty("Description_Tier2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_Description_Tier2_Input_RP", 	BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "U5#" & requested_ItemNum & "0_1:" & scriptGenericsDescr).DataCellEx.DataCellAnnotation)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_POC_RP", 						attributeDict.GetValueOrEmpty("POC"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_DollarKValue_RP",				attributeDict.GetValueOrEmpty("DollarK_Value"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_RecurringNonRecurring_RP", 	attributeDict.GetValueOrEmpty("R_NR"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_ATU_RP", 						ATU)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_PPA_RP", 						attributeDict.GetValueOrEmpty("PPA"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_UII_RP", 						attributeDict.GetValueOrEmpty("UII"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_ObjectClass_RP", 				attributeDict.GetValueOrEmpty("Object_Class"))
						
					End If 'Not globals.GetObject("attributeDict") Is Nothing
								
					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True						
					Return selectionChangedTaskResult
					
				Case Else
					Throw New XFException(si, New Exception("OnCbxRP_Expense_Selected: unknown appropriation '" & rpAppr & "'"))
			End Select
			Return Nothing
		End Function
		Private Function AddMod() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.AddMod ====
						'Dim Username as String = api.si.username
						Dim Workflow As WorkFlowInitInfo = BRApi.Workflow.General.GetUserWorkflowInitInfo(si) 
						Dim WorkflowUnit As WOrkflowUnitInfo = Workflow.GetSelectedWorkflowUnitInfo()
						Dim WorkflowProfileName As String = WorkflowUnit.ProfileName
						Dim dimensionName As String = "Std_Flow"
						Dim PCRType As String = args.NameValuePairs("PCRType")
						Dim PCRNumber As String = args.NameValuePairs("PCRNumber")
						Dim SpecialCode As String = args.NameValuePairs("SpecialCode")
						Dim parentName As String
						Dim memberName As String 
						Dim PCRMember As String = String.Empty
						'Dim memberDesc As String = String.Empty
						Dim memberDesc As String = args.NameValuePairs("ModName")
						Dim memberDescLength As Integer = memberDesc.Length
						Dim modParent As String = args.NameValuePairs("ModParent")
						Dim annTermMod As String = args.NameValuePairs("AnnTermMod")
						Dim Scenario As String = WorkflowUnit.ScenarioName
						Dim WFTime As String = WorkflowUnit.TimeName
						Dim WFTimeParse As List(Of String) = StringHelper.SplitString(WFTime,"0")
						Dim ModTime As String = WFTimeParse(1)
						
				'PCRType Validations
				If annTermMod.Length = 0 Then
					Throw New Exception("Modification Ann/Term: " & environment.NewLine & "Must Select Is this an Ann/Term Mod." & environment.NewLine)
				Else If annTermMod.XFEqualsIgnoreCase("No") Then
						If PCRType.Length = 0 Then
						   Throw New Exception("Modification Type: " & environment.NewLine & "Must Select a Modification Type." & environment.NewLine)
						End If
						If PCRType.XFEqualsIgnoreCase("Price") Then 
								PCRMember = "PRI"
							Else If PCRType.XFEqualsIgnoreCase("Program") Then 
								PCRMember = "PGM" 
							Else If PCRType.XFEqualsIgnoreCase("Transfer") Then 
							    PCRMember = "TXF"
							Else If PCRType.XFEqualsIgnoreCase("Technical") Then 
							    PCRMember = "TCH"
						End If
				Else
					If annTermMod.XFEqualsIgnoreCase("Annualization")
						PCRMember = "ANN"
					Else If annTermMod.XFEqualsIgnoreCase("Termination")
						PCRMember = "TRM"
					End If
				End If						 
				
				'Mod Number Validation
						If StringHelper.ContainsLetter(PCRNumber) = True Then
							Throw New Exception("Modification Number Format: " & environment.NewLine & "Only numeric values may be entered for the Modification Number." & environment.NewLine)
						Else If PCRNumber.Contains("_") Or PCRNumber.Contains("$") Or PCRNumber.Contains("%") Or PCRNumber.Contains("&") Or PCRNumber.Contains("(") Or PCRNumber.Contains(")") _
							 Or PCRNumber.Contains("~") Or PCRNumber.Contains("`")  Or PCRNumber.Contains("'") Or PCRNumber.Contains(":") Or PCRNumber.Contains(".") Or PCRNumber.Contains(" ") Then
							Throw New Exception("Modification Number Format: " & environment.NewLine & "No special characters may be used for the Modification Number." & environment.NewLine)
						Else If PCRNumber.Length <> 4 Then
							Throw New Exception("Modification Number Length: " & environment.NewLine & "Must use 4 digits for the Modification Number." & environment.NewLine)	
					    Else If PCRNumber.StartsWith("0") Then
							Throw New Exception("Modification Number Bounds: " & environment.NewLine & "Modification Number must be between 1000-9999." & environment.NewLine)
					   	End If
											
					'Existing Mod Validation	
						Dim ExistingModName As String
						Dim ExistingModNameParsed As List(Of String)
						Dim ExistingModNum As String
						Dim ExistingModSpecialCode As String
						Dim memberFilter As String = String.Empty
						Dim standardMemberFilter As String = "F#USCG_FY" & ModTime & "_Mods.Base.where(Text5 DoesNotContain Archive_)"
						Dim abvMemberFilter As String = "F#USCG_ABV_FY" & ModTime & "_Mods.Base.where(Text5 DoesNotContain Archive_)"
						If modParent.XFContainsIgnoreCase("USCG_ABV")
							memberFilter = abvMemberFilter
						Else 'must be standard
							memberFilter = standardMemberFilter
						End If						
						Dim ExistingMods As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"Std_Flow", memberFilter, True)
						
						For Each ExistingMod As MemberInfo In ExistingMods
							ExistingModName = ExistingMod.Member.Name
							ExistingModNameParsed = StringHelper.SplitString(ExistingMod.Member.Name,"_")
							ExistingModNum = ExistingModNameParsed(2)
							If PCRNumber = ExistingModNum
								If (ExistingModNameParsed.Count = 5) Or (ExistingModNameParsed.Count = 6)
									ExistingModSpecialCode = ExistingModNameParsed(4)
								Else 
									ExistingModSpecialCode = ""
								End If	
								If (SpecialCode = ExistingModSpecialCode) Or (ExistingModSpecialCode.StartsWith("Prior"))
									Dim ExistingModDesc As String = ExistingMod.Description
									Throw New Exception("Modification Number: " & Environment.NewLine & "Modification number/special code combination already exists in " & ExistingModName & " - " & ExistingModDesc & ".")
								End If
							End If
						Next
						
						
				'Modification Name Validation
						If memberDescLength = 0 Then
							Throw New Exception("Modification Name: " & environment.NewLine & "Must enter a name for the Modification." & environment.NewLine)
					   	Else If memberDescLength > 80 Then
							Throw New Exception("Modification Name: " & environment.NewLine & "Length of Modification Name must be 80 characters or less." & environment.NewLine)
						End If
						
						
					'Parentheses Checking
						'Mismatching parentheses causes issues in XFBR BR for Justification CJ reports
						Dim ParenthesesCount As Integer = 0
						For Length As Integer = 0 To (memberDescLength - 1)
							If memberDesc(Length) = "(" Then
								ParenthesesCount = ParenthesesCount + 1
							Else If memberDesc(Length) = ")" Then
								ParenthesesCount = ParenthesesCount - 1
							End If
							
							If ParenthesesCount < 0 Then
								Throw New Exception("Modification Name Format: " & environment.NewLine & "Please ensure the parentheses format is correct." & environment.NewLine)
							Else If (Length = memberDescLength - 1) And (ParenthesesCount <> 0) Then
								Throw New Exception("Modification Name Format: " & environment.NewLine & "Please ensure all open parentheses have matching close parentheses." & environment.NewLine)
							End If
							'brapi.ErrorLog.LogMessage(si,"Parentheses Count: " & ParenthesesCount)
						Next	
						
						
				'Special Code Validation
						If ((SpecialCode.Length <> 0) And (SpecialCode.Length <> 2) And (SpecialCode.Length <> 3))
							Throw New Exception("Special Code: " & environment.NewLine & "Must use 2 or 3 digits for the Special Code." & environment.NewLine)
						Else If StringHelper.ContainsLowerCaseLetter(SpecialCode) Then
							Throw New Exception("Special Code: " & environment.NewLine & "Remove lower case letters in Special Code." & environment.NewLine)
						Else If SpecialCode.Contains("_") Or SpecialCode.Contains("$") Or SpecialCode.Contains("%") Or SpecialCode.Contains("&") Or SpecialCode.Contains("(") Or SpecialCode.Contains(")") _
							Or SpecialCode.Contains("~") Or SpecialCode.Contains("`")  Or SpecialCode.Contains("'") Or SpecialCode.Contains(":") Or SpecialCode.Contains(".") Or SpecialCode.Contains(" ") Then
							Throw New Exception("Special Code: " & environment.NewLine & "Remove spaces and/or special characters from Special Code." & environment.NewLine)
						Else If SpecialCode.Equals("AO") Or SpecialCode.Equals("OIG") Or SpecialCode.Equals("CBP") Or SpecialCode.Equals("ICE") Or SpecialCode.Equals("TSA") Or SpecialCode.Equals("OHA") _
							Or SpecialCode.Equals("ST") Or SpecialCode.Equals("DHS") Or SpecialCode.Equals("PBR") Or SpecialCode.Equals("ABV") Or SpecialCode.Equals("PRI") Or SpecialCode.Equals("PGM") _
							Or SpecialCode.Equals("TXF") Or SpecialCode.Equals("TCH") Or SpecialCode.Equals("ANN") Or SpecialCode.Equals("TRM") Or SpecialCode.Equals("WCF") Then
						 	Throw New Exception("Special Code: " & environment.NewLine & "Special Code not allowed." & environment.NewLine)
						'Currently not doing AG or PBR Scenarios
'						Else If ((SpecialCode.Length = 0) And (Scenario.StartsWith("AG_"))) Then
'							membername = "USCG_ABV" & PCRMember & "_" & PCRNumber & "_" & ModTime
'							parentname = modParent
'						Else If (((SpecialCode.Length = 2) Or (SpecialCode.Length = 3)) And (Scenario.StartsWith("AG_"))) Then
'							membername = "USCG_ABV" & PCRMember & "_" & PCRNumber & "_" & ModTime & "_" & SpecialCode
'							parentname = modParent
'						Else If ((SpecialCode.Length = 0) And (Scenario.StartsWith("PBR_"))) Then
'							membername = "USCG_PBR" & PCRMember & "_" & PCRNumber & "_" & ModTime
'							parentname = modParent
'						Else If (((SpecialCode.Length = 2) Or (SpecialCode.Length = 3)) And (Scenario.StartsWith("PBR_"))) Then
'							membername = "USCG_PBR" & PCRMember & "_" & PCRNumber & "_" & ModTime & "_" & SpecialCode
'							parentname = modParent
						Else If ((SpecialCode.Length = 0) And (modParent.XFContainsIgnoreCase("USCG_ABV"))) Then
							membername = "USCG_ABV" & PCRMember & "_" & PCRNumber & "_" & ModTime
							parentname = modParent
						Else If (((SpecialCode.Length = 2) Or (SpecialCode.Length = 3)) And (modParent.XFContainsIgnoreCase("USCG_ABV"))) Then
							membername = "USCG_ABV" & PCRMember & "_" & PCRNumber & "_" & ModTime & "_" & SpecialCode
							parentname = modParent
						Else If SpecialCode.Length = 0
							membername = "USCG_" & PCRMember & "_" & PCRNumber & "_" & ModTime
							parentname = modParent
						Else
							membername = "USCG_" & PCRMember & "_" & PCRNumber & "_" & ModTime & "_" & SpecialCode
							parentname = modParent
						End If
												
					'Add Modification to the Dimension with the text 8 value being "Mod"			
						rputils.Create_ModHierMem(si, membername, memberDesc, parentname, "Mod")
						
						Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
						selectionChangedTaskResult.IsOK = True
						selectionChangedTaskResult.ShowMessageBox = True
						selectionChangedTaskResult.Message = PCRType & " Modification Added: " & environment.NewLine & "The Member: " & membername & " was added to the system and is ready for use." & environment.NewLine
						selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = False
						selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = Nothing
						selectionChangedTaskResult.ChangeSelectionChangedNavigationInDashboard = False
						selectionChangedTaskResult.ModifiedSelectionChangedNavigationInfo = Nothing
						selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = False
						selectionChangedTaskResult.ModifiedCustomSubstVars = Nothing
						selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = False
						selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard = Nothing							
						Return selectionChangedTaskResult							
			Return Nothing
		End Function
		Private Function AddModHierachyMember() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.AddModHierachyMember ====
						
						'Create the New Member
						'First get the selected parent member
						Dim parent As String = args.NameValuePairs("Parent")
						Dim newMemberTitle As String = args.NameValuePairs("MemberTitle")
						
						'Derive the new member name from the parent.  Need to revise this Logic, but working for demo
						Dim newMemberTier As String
						Dim newMemberSequence As String
						Dim newMemberPrefix As String
						If parent.XFContainsIgnoreCase("Tier")
							Dim strDelimiter As String = "_" 
							Dim fields As List(Of String) = StringHelper.SplitString(parent, strDelimiter, StageConstants.ParserDefaults.DefaultQuoteCharacter)
							Dim fieldsCount As Integer = fields.Count
							Dim parentTier As String = fields(fieldsCount-2)
							Dim parentSequence As String = fields(fieldsCount-1)
							Dim parentTierAndSequenceLen As Integer = ("_" & parentTier & "_" & parentSequence).Length
							newMemberPrefix = parent.remove(parentTierAndSequenceLen)
							newMemberTier = "_Tier" & (parentTier.Substring(4,2).XFConvertToInt + 1).ToString("00")
							'need to make the below dynamic based on the sequences that already exist.  Use logic from RP sequence
							'Establish the list of existing sequences used
							Dim usedSequencesList As New List (Of String)
							Dim BudFm_FlowDim As OneStream.Shared.Wcf.Dim = BRApi.Finance.Dim.GetDim(si, "Std_Flow")			
							Dim existingRPMemList As List (Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BudFm_FlowDim.DimPk, "F#" & parent & ".Descendants.Where((Text8 DoesNotContain 'Mod') AND (Text8 DoesNotContain 'RP_FY') AND (Name Contains '" & newMemberTier & "'))", True)
														
							'If the existing list is not nothing, create the existing list
							If (Not existingRPMemList Is Nothing AndAlso existingRPMemList.Count <> 0) Then
								For Each existingRPMem As MemberInfo In existingRPMemList
									'Get the sequence number from the RP and add it to the list
									Dim uniqueId As String = existingRPMem.Member.Name.Substring(existingRPMem.Member.Name.Length-2,2)
									'Add it to the list
								'brapi.ErrorLog.LogMessage(si, "existingRPMem=" & existingRPMem.Member.Name)
									usedSequencesList.Add(uniqueId)
								Next 
								
								'Sort the list and get the last number in it and add a 1 to this because it will be the next number to assign			
								usedSequencesList.Sort()
								Dim currLastSequence As Integer = usedSequencesList.Last().XFConvertToInt()
								newMemberSequence = "_" & (currLastSequence + 1).ToString("00")
								
							Else 'The existing list for this year is nothing so start with 01
								newMemberSequence = "_" & 1.ToString("00")
							End If
													
						Else	'parent name does not contain Tier
							'Establish the list of existing sequences used
							Dim usedSequencesList As New List (Of String)
							Dim BudFm_FlowDim As OneStream.Shared.Wcf.Dim = BRApi.Finance.Dim.GetDim(si, "Std_Flow")			
							Dim existingRPMemList As List (Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BudFm_FlowDim.DimPk, "F#" & parent & ".Descendants.Where((Text8 DoesNotContain 'Mod') AND (Text8 DoesNotContain 'RP_FY'))", True)
														
							'If the existing list is not nothing, create the existing list
							If (Not existingRPMemList Is Nothing AndAlso existingRPMemList.Count <> 0) Then
								For Each existingRPMem As MemberInfo In existingRPMemList
									'Get the sequence number from the RP and add it to the list
									Dim uniqueId As String = existingRPMem.Member.Name.Substring(existingRPMem.Member.Name.Length-2,2)
									'Add it to the list
									usedSequencesList.Add(uniqueId)
								Next 
								
								'Sort the list and get the last number in it and add a 1 to this because it will be the next number to assign			
								usedSequencesList.Sort()
								Dim currLastSequence As Integer = usedSequencesList.Last().XFConvertToInt()
								newMemberSequence = "_" & (currLastSequence + 1).ToString("00")
								
							Else 'The existing list for this year is nothing so start with 01
								newMemberSequence = "_" & 1.ToString("00")
							End If			
							
							newMemberPrefix = parent
							newMemberTier = "_Tier01"
						End If
						
						'Create the new member by combining the components
						Dim newMemberName As String = newMemberPrefix & newMemberTier & newMemberSequence					
						rputils.Create_ModHierMem(si, newMemberName, newMemberTitle, parent, String.Empty)
												
						'Show a message box that the Member was successfully created
						Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
						selectionChangedTaskResult.IsOK = True
						selectionChangedTaskResult.ShowMessageBox = True
						selectionChangedTaskResult.Message = newMemberTitle & " Member successfully created"
						Return selectionChangedTaskResult
						 
			Return Nothing
		End Function
		Private Function ClearNonBLTLine_OS() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.ClearNonBLTLine_OS ====

		Dim wfTime As String = args.NameValuePairs("WFTime")
		Dim wfScenario As String = args.NameValuePairs("WFScenario")
		Dim wfCube As String = args.NameValuePairs("WFCube")
		Dim RPName As String = args.NameValuePairs("RPName")
		Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
		Dim LineItemNum As String = args.NameValuePairs("LineItemNum") '|!prm_NBLT_LineItemNumber!|
		Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
		Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")

		If  String.IsNullOrEmpty (LineItemNum) Then 
			Throw New Exception("Please choose a Line Item") 
		End If

		RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, LineItemNum )

		'Storing the Annotation text for the attributes in a generic string
		
		Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U7#None:U8#None"		
		Dim LineItemNumInt As Integer = LineItemNum.Substring(9,2).XFConvertToInt	

		ClearNonBillet(si, args, wfScenario, wfCube, wfTime, RP_Entity, rpName, LineItemNum,  LineItemNumInt, scriptgenerics)
		
		Dim params As New Dictionary(Of String, String) 
			params.Add("prm_NBLT_LineItemNumber_OS", String.Empty) 
			params.Add("prm_NBLT_RequestedItem_Tier1_OS", String.Empty) 		
			params.Add("prm_NBLT_Description_Tier2_OS", String.Empty)
			params.Add("prm_NBLT_Description_Tier2_Input_OS", String.Empty)
			params.Add("prm_NBLT_POC_OS", String.Empty)
			params.Add("prm_NBLT_SupportingDoc_OS", String.Empty)
			params.Add("prm_NBLT_DollarKValue_OS", String.Empty)
			params.Add("prm_NBLT_RecurringNonRecurring_OS", String.Empty)
			params.Add("prm_NBLT_ATU_OS", String.Empty)
			params.Add("prm_NBLT_PPA_OS", String.Empty)
			params.Add("prm_NBLT_UII_OS", String.Empty)
			params.Add("prm_NBLT_ObjectClass_OS", String.Empty)
			params.Add("prm_Content_OS","OS_Billets_NonAddEditNon_04d")
			
		Return SetFieldValues(si,  params ,True, "" & RPName & " " & LineItemNum & " Successfully Cleared")
								
		
			Return Nothing
		End Function
		Private Function Consol_WFScenario() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.Consol_WFScenario ====
	
							'Implement Load Dashboard logic here.
							
							If args.LoadDashboardTaskInfo.Reason = LoadDashboardReasonType.Initialize And args.LoadDashboardTaskInfo.Action = LoadDashboardActionType.BeforeFirstGetParameters Then
				
								Dim loadDashboardTaskResult As New XFLoadDashboardTaskResult()
								Dim params As New Dictionary(Of String, String)  
								
								brapi.Utilities.StartDataMgmtSequence(si, "Consol_WFScenario", params)	
								
								loadDashboardTaskResult.ChangeCustomSubstVarsInDashboard = False
								loadDashboardTaskResult.ModifiedCustomSubstVars = Nothing
								Return loadDashboardTaskResult
								
							End If
										
			Return Nothing
		End Function
		Private Function CopyBilletsToDestination_OS() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.CopyBilletsToDestination_OS ====

		Dim wfTime As String = args.NameValuePairs("WFTime")
		Dim wfScenario As String = args.NameValuePairs("WFScenario")
		Dim wfCube As String = args.NameValuePairs("WFCube")
		Dim RPName As String = args.NameValuePairs("RPName")
		Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
		Dim LINumberSource As String = args.NameValuePairs.XFGetValue("LINumberSource")
		Dim LINumberDestination As String = args.NameValuePairs.XFGetValue("LINumberDestination")
		Dim PositionNumber As String = args.NameValuePairs.XFGetValue("PositionNumber")
		Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
		Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")
		
		If  String.IsNullOrEmpty(LINumberSource) Then 
			Throw New Exception("Please choose a Source Line Item") 
		End If

		Dim nbrOfDestinationBillets As Integer = 1
		Dim BilletName As String = String.Empty
		Dim lstOfBillets As New List(Of String)
		
		'identify how many destination billets were selected since destination billets can now be 1-to-many
		If String.IsNullOrEmpty(LINumberDestination) Then 
			Throw New Exception("Please choose a Destination Line Item")
		Else
			For Each chr As Char In LINumberDestination
				If chr = "," Then  'the string of billets are separated by a space and a comma
					nbrOfDestinationBillets+=1
					lstOfBillets.Add(BilletName)
					BilletName = String.Empty
				Else If chr <> " " Then  'strip the blank preceeding the billet name
					BilletName = BilletName + chr
				End If
			Next
			'add the last billet to the list object since no more comma characters exist in the string
			lstOfBillets.Add(BilletName)
			
			If nbrOfDestinationBillets > 5 Then
				Throw New Exception("Limit exceeded, please reduce selected number of Destination Billets to 5 or less")
			End If
		End If

		For Each billet As String In lstOfBillets
			RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, billet)
			'run the function to copy the source LI to the destination LI
			Me.CopyBilletAllFields(si, globals, args, wfCube, wfTime, wfScenario, RP_Entity, RPName, LINumberSource, billet)
			LINumberDestination = billet
		Next
		
		'Show a message box that the Billet was successfully updated, however the RefreshSelectedBillet_OS return message with have dominance
		Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
		selectionChangedTaskResult.IsOK = True
		selectionChangedTaskResult.ShowMessageBox = True
		
		Dim stringmessage As String =  "" & GetDescription(si,RPName) & " " & GetUD6Description(si,LINumberSource) & " Successfully Copied to " & GetUD6Description(si,LINumberDestination) & ""
		
		selectionChangedTaskResult = Me.RefreshSelectedBillet_OS(si, args, globals, wfCube, wfTime, wfScenario, RPName, LINumberDestination, stringmessage)
		Return selectionChangedTaskResult
						
			Return Nothing
		End Function
		Private Function CopyRPAttributes() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.CopyRPAttributes ====
						
						Dim wfTime As String = args.NameValuePairs("WFTime")
						Dim wfScenario As String = args.NameValuePairs("WFScenario")
						Dim wfCube As String = args.NameValuePairs("WFCube")
						Dim SourceRPName As String = args.NameValuePairs("RPNumberSrc")
						Dim TargetRPName As String = args.NameValuePairs("RPNumberDest")
						Dim createWV As Boolean = False
						
						'First, check if the RP is in Edit Mode, then only continue
						If Not rpUtils.Is_RP_Editable(si, TargetRPName)
							Throw New Exception( TargetRPName & " is set to View Only.  No edits can be made.")
						End If
						
						Dim TargetRPEntity As String = rpUtils.Get_RP_Entity(si, TargetRPName)
						
						'
						' Copy attributes From source RP To target RP. It involves the follwing steps
						' 1. Delete annotations of target RP
						' 2. Copy annotations from source RP to target RP
						' 3. Delete all the allocations and calculated costs and (i.e data records) of target RP
						' 4. Copy all the calculated costs (i.e data records) from source RP to target RP
						' 
						rpUtils.Copy_RP_Attributes(
													si,
													wfCube,
													wfScenario, ' Source Scenario
													wfScenario,	' Target Scenario				
													SourceRPName,
													TargetRPName,
													createWV
													)
						
						'Show a message box that the RP was successfully created
						Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
						selectionChangedTaskResult.IsOK = True
						selectionChangedTaskResult.ShowMessageBox = True
						selectionChangedTaskResult.Message = "RP attributes successfully copied from  " & 
															SourceRPName & " to " & TargetRPName &
															" and calculated RP cost"
						Return selectionChangedTaskResult
							 
			Return Nothing
		End Function
		Private Function CopyRPAttributesNew() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.CopyRPAttributesNew ====
								
					 			Dim wfCube As String = args.NameValuePairs("WFCube")
								
								Dim sourceScenario As String = args.NameValuePairs("SourceScenario")
								Dim targetScenario As String = args.NameValuePairs("WFScenario")
								
								Dim targetWfTime As String = args.NameValuePairs("WFTime")
								Dim sourceWfTime As String = "20" & sourceScenario.Substring((sourceScenario.length-2),2)
								
								Dim SourceRPName As String = args.NameValuePairs("RPNumberSrc")
								Dim TargetRPName As String = args.NameValuePairs("RPNumberDest")

								'First, check if the RP is in Edit Mode, then only continue
								If Not rpUtils.Is_RP_Editable(si, TargetRPName)
									Throw New Exception( TargetRPName & " is set to View Only.  No edits can be made.")
								End If
								
								'Compare the number of billets are equal, then only continue
								
								'Source
								Dim SourceRPEntity As String = rpUtils.Get_RP_Entity(si, SourceRPName)
								Dim sourceNumOfBillets As Integer = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Number_of_Billets:E#" & SourceRPEntity & ":S#" & sourceScenario & ":T#" & sourceWfTime & ":V#Annotation:F#" & SourceRPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").DataCellEx.DataCellAnnotation.XFConvertToInt
								
								'Target
								Dim TargetRPEntity As String = rpUtils.Get_RP_Entity(si, TargetRPName)
								Dim targetNumOfBillets As Integer = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Number_of_Billets:E#" & TargetRPEntity & ":S#" & targetScenario & ":T#" & targetWfTime & ":V#Annotation:F#" & TargetRPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").DataCellEx.DataCellAnnotation.XFConvertToInt
								Dim targetNumOfBilletsAnno As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Number_of_Billets:E#" & TargetRPEntity & ":S#" & targetScenario & ":T#" & targetWfTime & ":V#Annotation:F#" & TargetRPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").DataCellEx.DataCellAnnotation	

								'Check target number of billets is not blank and display a message if it is blank
								 If targetNumOfBilletsAnno = "" Then
									Throw New Exception("There is no number of billets specified for the target RP, so the data cannot be copied. Please fill out a number of billets for the target RP before attempting to copy data into it.")
								 End If								

								' Copy attributes From source RP To target RP. It involves the following steps
								' 1. Delete annotations of target RP
								' 2. Copy annotations from source RP to target RP
								' 3. Delete all the allocations and calculated costs and (i.e data records) of target RP
								' 4. Copy all the calculated costs (i.e data records) from source RP to target RP
								' 5. Calculate the date for the Target not just copy run step below 
								
							    Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
								
								selectionChangedTaskResult.IsOK = True
								selectionChangedTaskResult.ShowMessageBox = True
								
								Dim createWV As Boolean = False
								
								rpUtils.Copy_RP_Attributes(si,
															wfCube,
															sourceScenario,
															targetScenario,
															SourceRPName,
															TargetRPName,
															createWV)
								
							  Dim rpName As String = TargetRPName		 
															 
							'Call the calculate step in the Data Management for the Target RP.
							  
								Dim params As New Dictionary(Of String, String)
								params.Add("prm_Number", rpName)
								params.Add("rpEntity", TargetRPEntity)
							    params.Add("WFTime", targetWfTime) 
							    BRAPI.Utilities.StartDataMgmtSequence(si, "Calc_Single_RP", params)
								
							  'Check if the number of billets source > the target if true display a message along with alerting how many billets were copied. Otherwise the regular message. 						
								
                                If sourceNumOfBillets > targetNumOfBillets Then 
									
							    	selectionChangedTaskResult.Message = "Billets up to line item " & targetNumOfBillets & " have been copied out of " & sourceNumOfBillets & " line items and RP cost has been calculated. To copy all billets, ensure that the number of billets selection on Edit RP Page 1 is the same for both the source and target RP."
								
		                        Else
									  
								   	selectionChangedTaskResult.Message =   "RP attributes successfully copied From  " & 
																	       SourceRPName & " to " & TargetRPName &
																	        " and calculated RP cost"
								End If
																		
								Return selectionChangedTaskResult
								
			Return Nothing
		End Function
		Private Function CreateInitialFYABVModHierarchy() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.CreateInitialFYABVMODHierarchy ====
						
						'Get Time from current Workflow
						Dim wfTime As String = args.NameValuePairs("WFTime")
						Dim wfTime_YY = rpUtils.Get_WFTime_YY(si, wfTime)
						
						'First, check to see if the initial FY## ABV MOD Hierarchy already exists and if so, then just return a message saying so						
'						Throw New Exception("FY" & wfTime_YY & "Initial MOD Hierarchy was already started")
						
						'Create New Members
						'Create the FY##_ABV_Mods Parent
						Dim fYYY_MODsName As String = "USCG_ABV_FY" & wfTime_YY & "_Mods"
						Dim fYYY_MODsTitle As String = "FY" & wfTime_YY & " Above Guidance Modifications"
						Dim total_MODsName As String = "Total_ABV_Mods"						
						rputils.Create_ModHierMem(si, fYYY_MODsName, fYYY_MODsTitle, total_MODsName, String.Empty)
						
						
						#Region "Create the Children"
						
								Dim fYYY_OSName As String = "USCG_ABVOS_" & wfTime_YY
								Dim fYYY_OSTitle As String = " O & S"
								rputils.Create_ModHierMem(si, fYYY_OSName, fYYY_OSTitle, fYYY_MODsName, String.Empty)
								
								Dim fYYY_PCIName As String = "USCG_ABVPCI_" & wfTime_YY
								Dim fYYY_PCITitle As String = "Procurement, Construction and Improvements"
								rputils.Create_ModHierMem(si, fYYY_PCIName, fYYY_PCITitle, fYYY_MODsName, String.Empty)
								
								Dim fYYY_RDName As String = "USCG_ABVRD_" & wfTime_YY
								Dim fYYY_RDTitle As String = "Research & Development"
								rputils.Create_ModHierMem(si, fYYY_RDName, fYYY_RDTitle, fYYY_MODsName, String.Empty)
								
								Dim fYYY_RPName As String = "USCG_ABVRP_" & wfTime_YY
								Dim fYYY_RPTitle As String = "Retired Pay"
								rputils.Create_ModHierMem(si, fYYY_RPName, fYYY_RPTitle, fYYY_MODsName, String.Empty)
								
								Dim fYYY_MOSPName As String = "USCG_ABVMOSP_" & wfTime_YY
								Dim fYYY_MOSPTitle As String = "Maritime Oil Spill Program"
								rputils.Create_ModHierMem(si, fYYY_MOSPName, fYYY_MOSPTitle, fYYY_MODsName, String.Empty)
								
								Dim fYYY_FName As String = "USCG_ABVF_" & wfTime_YY
								Dim fYYY_FTitle As String = "Funds"
								rputils.Create_ModHierMem(si, fYYY_FName, fYYY_FTitle, fYYY_MODsName, String.Empty)
								
								Dim fYYY_MERHCFName As String = "USCG_ABVMERHCF_" & wfTime_YY
								Dim fYYY_MERHCFTitle As String = "Medicare-Eligible Retiree Health Care Fund Contribution"
								rputils.Create_ModHierMem(si, fYYY_MERHCFName, fYYY_MERHCFTitle, fYYY_MODsName, String.Empty)
								
								Dim fYYY_BSName As String = "USCG_ABVBS_" & wfTime_YY
								Dim fYYY_BSTitle As String = "Boat Safety"
								rputils.Create_ModHierMem(si, fYYY_BSName, fYYY_BSTitle, fYYY_MODsName, String.Empty)
						#End Region
				
						'Show a message box that the RP was successfully created
						Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
						selectionChangedTaskResult.IsOK = True
						selectionChangedTaskResult.ShowMessageBox = True
						selectionChangedTaskResult.Message = "Initial FY" & wfTime_YY & " Above Guidance Mod hierarchy successfully created"
						Return selectionChangedTaskResult
						 
			Return Nothing
		End Function
		Private Function CreateInitialFYModHierarchy() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.CreateInitialFYMODHierarchy ====
						
						'Get Time from current Workflow
						Dim wfTime As String = args.NameValuePairs("WFTime")
						Dim wfTime_YY = rpUtils.Get_WFTime_YY(si, wfTime)
						
						'First, check to see if the initial FY## MOD Hierarchy already exists and if so, then just return a message saying so						
'						Throw New Exception("FY" & wfTime_YY & "Initial MOD Hierarchy was already started")
						
						'Create New Members
						'Create the FY##_Mods Parent
						Dim fYYY_MODsName As String = "USCG_FY" & wfTime_YY & "_Mods"
						Dim fYYY_MODsTitle As String = "FY" & wfTime_YY & " Modifications"
						Dim total_MODsName As String = "Total_Standard_Mods"						
						rputils.Create_ModHierMem(si, fYYY_MODsName, fYYY_MODsTitle, total_MODsName, String.Empty)
						
						#Region "Create Discretionary, Mandatory, & O&S Members" 
						
								Dim fYYY_DCRName As String = "USCG_DCR_" & wfTime_YY
								Dim fYYY_DCRTitle As String = "Total Discretionary"
								rputils.Create_ModHierMem(si, fYYY_DCRName, fYYY_DCRTitle, fYYY_MODsName, String.Empty)
								
								Dim fYYY_MNDName As String = "USCG_MND_" & wfTime_YY
								Dim fYYY_MNDTitle As String = "Total Mandatory"
								rputils.Create_ModHierMem(si, fYYY_MNDName, fYYY_MNDTitle, fYYY_MODsName, String.Empty)
								
								Dim fYYY_OSName As String = "USCG_OS_" & wfTime_YY
								Dim fYYY_OSTitle As String = "Operation & Support Request"
								rputils.Create_ModHierMem(si, fYYY_OSName, fYYY_OSTitle, fYYY_DCRName, String.Empty)
								
																												
						#End Region
						
						'Create the Technical, Transfer, Pricing, Program & Other Appropriation Members	
						#Region "Create the Pricing Members"
								Dim fYYY_PRIName As String = "USCG_PRI_" & wfTime_YY
								Dim fYYY_PRITitle As String = "Pricing Changes"
								rputils.Create_ModHierMem(si, fYYY_PRIName, fYYY_PRITitle, fYYY_OSName, String.Empty)
												
								Dim fYYY_PRIIncrName As String = "USCG_PRI_Incr_" & wfTime_YY
								Dim fYYY_PRIIncrTitle As String = "Increases"
								rputils.Create_ModHierMem(si, fYYY_PRIIncrName, fYYY_PRIIncrTitle, fYYY_PRIName, String.Empty)
												
								Dim fYYY_ManPersEntitName As String = "USCG_PRI_Incr_" & wfTime_YY & "_Tier01_01"
								Dim fYYY_ManPersEntitTitle As String = "Mandatory Personnel Entitlements"
								rputils.Create_ModHierMem(si, fYYY_ManPersEntitName, fYYY_ManPersEntitTitle, fYYY_PRIIncrName, String.Empty)
												
								Dim fYYY_PayInflationName As String = "Std_FactorSet_PayInflation_" & wfTime_YY
								Dim fYYY_PayInflationTitle As String = "Civilian Pay Raise Total"
								rputils.Create_ModHierMem(si, fYYY_PayInflationName, fYYY_PayInflationTitle, fYYY_ManPersEntitName, "Mod")
												
								Dim fYYY_MilpayInflationName As String = "Std_FactorSet_MilpayInflation_" & wfTime_YY
								Dim fYYY_MilpayInflationTitle As String = "Military Pay Raise Total"
								rputils.Create_ModHierMem(si, fYYY_MilpayInflationName, fYYY_MilpayInflationTitle, fYYY_ManPersEntitName, "Mod")
												
								Dim fYYY_PYAnnPayRaiseName As String = "PY_Annualization_PayRaise_" & wfTime_YY
								Dim fYYY_PYAnnPayRaiseTitle As String = "Annualization of Prior Year Pay Raise"
								rputils.Create_ModHierMem(si, fYYY_PYAnnPayRaiseName, fYYY_PYAnnPayRaiseTitle, fYYY_ManPersEntitName, "Mod")
												
								Dim fYYY_PYAnnMilPayRaiseName As String = "PY_Annualization_MilitaryPayRaise_" & wfTime_YY
								Dim fYYY_PYAnnMilPayRaiseTitle As String = "Annualization of  Prior Year Military Pay Raise"
								rputils.Create_ModHierMem(si, fYYY_PYAnnMilPayRaiseName, fYYY_PYAnnMilPayRaiseTitle, fYYY_ManPersEntitName, "Mod")
												
								Dim fYYY_NonPayInflationName As String = "Std_FactorSet_NonPayInflation_" & wfTime_YY
								Dim fYYY_NonPayInflationTitle As String = "Non-Pay Inflation"
								rputils.Create_ModHierMem(si, fYYY_NonPayInflationName, fYYY_NonPayInflationTitle, fYYY_PRIIncrName, "Mod")
												
								Dim fYYY_PRIDecrName As String = "USCG_PRI_Decr_" & wfTime_YY
								Dim fYYY_PRIDecrTitle As String = "Decreases"
								rputils.Create_ModHierMem(si, fYYY_PRIDecrName, fYYY_PRIDecrTitle, fYYY_PRIName, String.Empty)
								
						#End Region
						#Region "Create the Program Members"
								Dim fYYY_PGMName As String = "USCG_PGM_" & wfTime_YY
								Dim fYYY_PGMTitle As String = "Program Changes"
								rputils.Create_ModHierMem(si, fYYY_PGMName, fYYY_PGMTitle, fYYY_OSName, String.Empty)	
												
								Dim fYYY_PGMIncrName As String = "USCG_PGM_Incr_" & wfTime_YY
								Dim fYYY_PGMIncrTitle As String = "Increases"
								rputils.Create_ModHierMem(si, fYYY_PGMIncrName, fYYY_PGMIncrTitle, fYYY_PGMName, String.Empty)
												
								Dim fYYY_PGMDecrName As String = "USCG_PGM_Decr_" & wfTime_YY
								Dim fYYY_PGMDecrTitle As String = "Decreases"
								rputils.Create_ModHierMem(si, fYYY_PGMDecrName, fYYY_PGMDecrTitle, fYYY_PGMName, String.Empty)
						#End Region
						#Region "Create the Other Members"
								'SPA 5/24/23 Commenting these out because I don't think we want these in our hierarchy.  TCH and TXF would only be in MOD names
'								Dim fYYY_TCHName As String = "USCG_TCH_" & wfTime_YY
'								Dim fYYY_TCHTitle As String = "FY" & wfTime_YY & " Technical"
'								rputils.Create_ModHierMem(si, fYYY_TCHName, fYYY_TCHTitle, fYYY_MODsName, String.Empty)
								
'								Dim fYYY_TXFName As String = "USCG_TXF_" & wfTime_YY
'								Dim fYYY_TXFTitle As String = "FY" & wfTime_YY & " Transfer"
'								rputils.Create_ModHierMem(si, fYYY_TXFName, fYYY_TXFTitle, fYYY_MODsName, String.Empty)
								
								Dim fYYY_PCIName As String = "USCG_PCI_" & wfTime_YY
								Dim fYYY_PCITitle As String = "PC&I"
								rputils.Create_ModHierMem(si, fYYY_PCIName, fYYY_PCITitle, fYYY_DCRName, String.Empty)
								
								Dim fYYY_RDName As String = "USCG_RD_" & wfTime_YY
								Dim fYYY_RDTitle As String = "R&D"
								rputils.Create_ModHierMem(si, fYYY_RDName, fYYY_RDTitle, fYYY_DCRName, String.Empty)

								Dim fYYY_MERHCFName As String = "USCG_MERHCF_" & wfTime_YY
								Dim fYYY_MERHCFTitle As String = "MERHCFC"
								rputils.Create_ModHierMem(si, fYYY_MERHCFName, fYYY_MERHCFTitle, fYYY_DCRName, String.Empty)
								
								Dim fYYY_RPName As String = "USCG_RP_" & wfTime_YY
								Dim fYYY_RPTitle As String = "RP"
								rputils.Create_ModHierMem(si, fYYY_RPName, fYYY_RPTitle, fYYY_MNDName, String.Empty)
								
								Dim fYYY_MOSPName As String = "USCG_MOSP_" & wfTime_YY
								Dim fYYY_MOSPTitle As String = "MOSP"
								rputils.Create_ModHierMem(si, fYYY_MOSPName, fYYY_MOSPTitle, fYYY_MNDName, String.Empty)
								
								Dim fYYY_FName As String = "USCG_F_" & wfTime_YY
								Dim fYYY_FTitle As String = "Funds"
								rputils.Create_ModHierMem(si, fYYY_FName, fYYY_FTitle, fYYY_MNDName, String.Empty)

								Dim fYYY_BSName As String = "USCG_BS_" & wfTime_YY
								Dim fYYY_BSTitle As String = "BS"
								rputils.Create_ModHierMem(si, fYYY_BSName, fYYY_BSTitle, fYYY_MNDName, String.Empty)


						#End Region
				
						'Show a message box that the RP was successfully created
						Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
						selectionChangedTaskResult.IsOK = True
						selectionChangedTaskResult.ShowMessageBox = True
						selectionChangedTaskResult.Message = "Initial FY" & wfTime_YY & " Mod hierarchy successfully created"
						Return selectionChangedTaskResult
						 
			Return Nothing
		End Function
		Private Function CreateNewRPAsExtension() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.CreateNewRPAsExtension ====
						
						Dim wfTime As String = args.NameValuePairs("WFTime")
						Dim SourceRPName As String = args.NameValuePairs("RPNumber")
						Dim NewRPTitle As String = args.NameValuePairs("RPTitle")
						Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
						
						Try 
							Dim RPName = rpUtils.Create_New_RP_AsExtention(si, WFTime, SourceRPName, NewRPTitle)
							Dim RPAppr = rpUtils.Get_RP_Appropriation(si, SourceRPName)
					
							'Show a message box that the RP was successfully created
					
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = True
							selectionChangedTaskResult.Message = "Resource Proposal" & 
							environment.NewLine	& "'" & GetDescription(si,RPName) & "'" &
							environment.NewLine	& "Successfully Created" & 
							environment.NewLine	&
							environment.NewLine	& "Please ensure you fill out Page 1, Page 2, and Page 3 (if applicable) before proceeding to enter and calculate costs."
							'set the RP to show
							SetRoutingNumber(selectionChangedTaskResult.ModifiedCustomSubstVars, RPAppr, RPName)
							SetRoutingPageCompat(selectionChangedTaskResult.ModifiedCustomSubstVars, RPAppr, RPAppr & "_RP_Page1")
							SetRoutingContent(selectionChangedTaskResult.ModifiedCustomSubstVars, RPAppr, RPAppr & "_RP_Content")
						Catch
							'selectionChangedTaskResult.IsOK = False
							'selectionChangedTaskResult.ShowMessageBox = True
							'selectionChangedTaskResult.Message = "Please choose an RP with _00 extension as source" 
							'selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Content_OS","04a2_BDF_RP_Dashboard_Content_CreateRP_OS")
							Throw New Exception( "Please choose an RP with _00 extension as source" )
						End Try
						
						selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
						Return selectionChangedTaskResult
			Return Nothing
		End Function
		Private Function CreateNewRPFromScratch() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.CreateNewRPFromScratch ====
						
						'Get Time from current Workflow
						Dim wfTime As String = args.NameValuePairs("WFTime")
						Dim RPEntity As String = args.NameValuePairs("RPLeadDirect")
						Dim RPAppr As String = args.NameValuePairs("RPAppr")
						Dim RPBudCat As String = args.NameValuePairs("RPBudCat")
						Dim RPTitle As String = args.NameValuePairs("RPTitle")
					   	
						Dim RPName = rpUtils.Create_New_RP_FromScrartch(
																	si ,
																	WFTime ,
																	RPAppr ,
																	RPEntity,
																	RPBudCat,
																	RPTitle)					
						 
						'Show a message box that the RP was successfully created
						Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
						selectionChangedTaskResult.IsOK = True
						selectionChangedTaskResult.ShowMessageBox = True
						selectionChangedTaskResult.Message = "Resource Proposal" & 
						environment.NewLine	& "'" & GetDescription(si,RPName) & "'" &
						environment.NewLine	& "Successfully Created" & 
						environment.NewLine	&
						environment.NewLine	& "Please ensure you fill out Page 1, Page 2, and Page 3 (if applicable) before proceeding to enter and calculate costs."
						'set the RP to show
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Number_" & RPAppr, RPName)
						selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
						Return selectionChangedTaskResult
						 
			Return Nothing
		End Function
		Private Function CreateRPs() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.CreateRPs ====
			Dim wfTime As String = args.NameValuePairs("WFTime")
			Dim wfScenario As String = args.NameValuePairs("WFScenario")
			Dim RPEntity As String = args.NameValuePairs("RPLeadDirect")
			Dim RPAppr As String = args.NameValuePairs("RPAppr")
			Dim RPBudCat As String = args.NameValuePairs("RPBudCat")
			Dim wfTimeYY = rpUtils.Get_WFTime_YY(si, wfTime)
			Dim PriorYearWorkScenParam As String = "WorkScen_FY" + (wfTimeYY-1).ToString
			Dim PriorYearScenario As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, PriorYearWorkScenParam)
	
			If PriorYearScenario = Nothing Then
				'display a message box that says a PriorYearScenario was not selected
				Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
				selectionChangedTaskResult.IsOK = True
				selectionChangedTaskResult.ShowMessageBox = True
				selectionChangedTaskResult.Message = "Please Select a Prior Year Scenario."
				selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
				Return selectionChangedTaskResult
			Else
				'=======================================================================================
				'create an RP for each ud1 member in the list that has a Text1=CreateRPForMe as well as copy
				'certain DataAttachment records from the previous year's RP if year is greater than 2026 
				'=======================================================================================	
				
				'check if the RP flow members exist under "FY##_RP" parent, if not, create it
				Dim RPParentName As String = "FY" & wfTimeYY & "_RP"
				Dim flowDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "Std_Flow")
				Dim flowFilter As String = "F#" + RPParentName + ".Children"
				Dim flowMbrList As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Std_Flow", flowFilter, True)
				Dim flowMbrString As String = String.Empty
							
				'=======================================================================================
				'create a string of RPs to search for existance before attempting to create the RP
				'=======================================================================================
				For Each flowMbr As MemberInfo In flowMbrList
					flowMbrString = flowMbrString + "," + flowMbr.Member.Name.ToString
				Next

				'=======================================================================================
				'create a list of ud1 members
				'=======================================================================================
				Dim ud1DimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "Std_PPA")
				Dim UD1Filter As String = "U1#" + RPAppr + ".Descendants.Where(Text1 Contains CreateRPForMe)"
				Dim UD1MbrList As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Std_PPA", UD1Filter, True)

				Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,dimtypeid.Time, wfTime)
				
				Dim strPreExistingRPs As String = String.Empty
				Dim strCreatedRPs As String = String.Empty
				Dim strFirstRP As String = String.Empty
				Dim ppa_Level1 As String = String.Empty
				Dim ppa_Level2 As String = String.Empty
				Dim i As Integer = 1
				Dim k As Integer = 1
				
				If wfTime >= "2022" Then
					'=======================================================================================
					'loop through all UD1 members where Text1=CreateRPForMe
					'=======================================================================================
					For Each ud1mbr As MemberInfo In UD1MbrList
						
						#Region "Loop through all UD1 members where Text1=CreateRPForMe"
						Dim RPName As String = wfTimeYY + "_" + ud1mbr.Member.Name.ToString
						Dim Ud1mmbrname As String = ud1mbr.Member.Name
						Dim Ud1mmbrId As Integer = BRApi.Finance.Members.GetMemberId(si,dimtypeid.UD1, Ud1mmbrname)
						Dim bValue As Boolean = BRApi.Finance.UD.InUse(si, dimTypeId.UD1, Ud1mmbrId, DimConstants.Unknown, wfTimeId)
						Dim RPNameLength As Integer = RPName.Length
						Dim RPDesc As String = ud1mbr.Member.Description
						Dim PriorYearTime As String = (wfTime-1).ToString
						Dim PriorYearRPName As String = PriorYearTime.Substring(2,2) + RPName.Substring(2,RPNameLength-2)
						Dim rdAccountPPA As String = String.Empty
 						Dim rdAccountATU As String = "RD"
						Dim rdAccountUII As String = "NoInvestment"
						
						'if the RP does not exist then create it
						If (flowMbrString.Contains(RPName)) And (bValue) Then
							'======================================================
							'add the pre-existing RP to the "pre-existing" RP list
							'======================================================
							strPreExistingRPs = strPreExistingRPs + vbTab + "'" + RPName + " - " + RPDesc + "'" + vbCrLf
						Else
							If (RPAppr = "PCI") And (bValue) Or (RPAppr = "RD") And (bValue) Then
								#Region "Create the RP (the Flow member)"
								Dim RPText8Value As String = rpUtils.Generate_RP_LongName(si, wfTime, RPEntity, RPAppr, RPBudCat, "9999", "00")
								rpUtils.Create_RP(si, RPText8Value, RPDesc, RPName, RPParentName)
								
								'========================================
								'add the new RP to the "created" RP list
								'========================================
								strCreatedRPs = strCreatedRPs + vbTab + "'" + RPName + " - " + RPDesc + "'" + vbCrLf
								If k=1 Then
									strFirstRP = RPName
								End If
								k+=1
								#End Region 'Create the RP (the Flow member)
								
								#Region "Create Page1 DataAttachment table records (PPA,ATU,UII) for PCI or RD"
								'=======================================================================================
								'when CreateRPForMe flag is not on a base-level member(for PCI), define the ppa level 1 and 2 values MANUALLY for the new RP
								'=======================================================================================
								'assign the values
								If ud1mbr.Member.Name.ToString.StartsWith("RD_") Then 
									rdAccountPPA = ud1mbr.Member.Name.ToString
								Else If ud1mbr.Member.Name.ToString.StartsWith("PCI_") Then 
									If ud1mbr.Member.Name.ToString = "PCI_VES_ISVS" Then 
										ppa_Level1 = "PCI_VES"
										ppa_Level2 = "PCI_VES_ISVS"
									Else If ud1mbr.Member.Name.ToString = "PCI_OTHER_C4ISR" Then 
										ppa_Level1 = "PCI_OTHER"
										ppa_Level2 = "PCI_OTHER_C4ISR"
									Else
										'parse the member name to derive the ppa level 1 and 2 values by finding the 2nd occurance
										'of "_", assuming the 1st "_" is the 4th character (index=3), where indexing is starting with 0
										Dim temp1 As Integer = ud1mbr.Member.Name.ToString.IndexOf("_",4)
										Dim temp2 As String = ud1mbr.Member.Description
										ppa_Level1 = ud1mbr.Member.Name.ToString.Substring(0,temp1)
										ppa_Level2 = ud1mbr.Member.Name.ToString
									End If
								End If


								'write the values to the DataAttachment table
								Dim page1ScriptGenerics As String = "Cb#BudFm:E#" & RPEntity & ":C#Local:S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
								Dim page1List_MemberScriptAndValue As New List(Of MemberScriptAndValue)
							
								If ud1mbr.Member.Name.ToString.StartsWith("RD_") Then 
									page1List_MemberScriptAndValue.Add(New MemberScriptAndValue("BudFm", "A#PPA:" & page1ScriptGenerics, 0, True, rdAccountPPA))
									page1List_MemberScriptAndValue.Add(New MemberScriptAndValue("BudFm", "A#ATU:" & page1ScriptGenerics, 0, True, rdAccountATU))
									page1List_MemberScriptAndValue.Add(New MemberScriptAndValue("BudFm", "A#UII:" & page1ScriptGenerics, 0, True, rdAccountUII))
								Else If ud1mbr.Member.Name.ToString.StartsWith("PCI_") Then 
									page1List_MemberScriptAndValue.Add(New MemberScriptAndValue("BudFm", "A#PPA_Level1_PCI:" & page1ScriptGenerics, 0, True, ppa_Level1))
									page1List_MemberScriptAndValue.Add(New MemberScriptAndValue("BudFm", "A#PPA_Level2_PCI:" & page1ScriptGenerics, 0, True, ppa_Level2))
								End If
								Dim page1Obj_XFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, page1List_MemberScriptAndValue)
							
								#End Region 'Create Page1 DataAttachment table records (PPA,ATU,UII) for PCI or RD
								
							End If  'If RPAppr = "PCI" Or RPAppr = "RD" Then
							
							
							If RPAppr = "RD"  Then
								#Region "Copy Page3 Data Attachments - Project Schedule for RD"
								
								Dim page3RD_getcellPriorYrScript As String = "A#" & "Project_Description" & ":E#" & RPEntity & ":C#Local:S#" & PriorYearScenario & ":T#" & PriorYearTime & ":V#Annotation:F#" & PriorYearRPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
								Dim page3RDList_MemberScriptAndValue As New List(Of MemberScriptAndValue)
								Dim page3RD_cellInfo As New DataCellInfoUsingMemberScript
								Dim page3RD_cellValue As String = String.Empty
								Dim page3RD_ObjXFResult As New XFResult
								
								'get Prior Year RP annotation value
								page3RD_cellInfo = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, "BudFm", page3RD_getcellPriorYrScript)
								page3RD_cellValue = page3RD_cellInfo.DataCellEx.DataCellAnnotation
										
								'write Current Year RP annotation value
								Dim page3RD_setcellCurrYrScript As String = "A#" & "Project_Description" & ":E#" & RPEntity & ":C#Local:S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
								page3RDList_MemberScriptAndValue.Add(New MemberScriptAndValue("BudFm", page3RD_setcellCurrYrScript, 0, True, page3RD_cellValue))
								page3RD_ObjXFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, page3RDList_MemberScriptAndValue)
								
								
								Dim projSchedList As List(Of String) = New List(Of String) From {
									"ProjectMilestone_BY_RD",
									"ProjectMilestone_CY_RD",
									"ProjectStartDt_BY_RD",
									"ProjectStartDt_CY_RD",
									"ProjectEndDt_BY_RD",
									"ProjectEndDt_CY_RD",
									"TRL_Levels_BY_RD",
									"TRL_Levels_CY_RD"
								}
								
								'copy the prior year's RP required DataAttachment records to the new RP
								Dim lstMemberScriptAndValue4 As New List (Of MemberScriptAndValue)
								Dim objXFResult4 As New XFResult
								Dim getcellScript4 As String = String.Empty
								Dim setcellScript4 As String = String.Empty
								Dim cellInfo4 As New DataCellInfoUsingMemberScript
								Dim cellValue4 As String = String.Empty
								Dim newAccount4 As String = String.Empty
								Dim newProjSchAccount4 As String = String.Empty
								Dim r As Integer = 1
								
								For Each projSchAcctMbr As String In projSchedList
									Dim ud8CommentList As List(Of String) = New List(Of String)()
									ud8CommentList.Add("Comment_01")
									ud8CommentList.Add("Comment_02")
									ud8CommentList.Add("Comment_03")
									ud8CommentList.Add("Comment_04")
									ud8CommentList.Add("Comment_05")
									ud8CommentList.Add("Comment_06")
									ud8CommentList.Add("Comment_07")
									ud8CommentList.Add("Comment_08")
									ud8CommentList.Add("Comment_09")
									ud8CommentList.Add("Comment_10")
									ud8CommentList.Add("Comment_11")
									ud8CommentList.Add("Comment_12")
									ud8CommentList.Add("Comment_13")
									ud8CommentList.Add("Comment_14")
									ud8CommentList.Add("Comment_15")

									If projSchAcctMbr.Contains("BY") Then
										newProjSchAccount4 = projSchAcctMbr.Replace("BY","CY")
										For Each commentNbr In ud8CommentList
											getcellScript4 = "A#" & projSchAcctMbr & ":E#" & RPEntity & ":C#Local:S#" & PriorYearScenario & ":T#" & PriorYearTime & ":V#Annotation:F#" & PriorYearRPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#" & commentNbr
											cellInfo4 = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, "BudFm", getcellScript4)
											cellValue4 = cellInfo4.DataCellEx.DataCellAnnotation
											setcellScript4 = "A#" & newProjSchAccount4 & ":E#" & RPEntity & ":C#Local:S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#" & commentNbr
											lstMemberScriptAndValue4.Add(New MemberScriptAndValue("BudFm", setcellScript4, 0, True, cellValue4))
										Next
									Else If projSchAcctMbr.Contains("CY") Then
										newProjSchAccount4 = projSchAcctMbr.Replace("CY","PY")
										For Each commentNbr In ud8CommentList
											getcellScript4 = "A#" & projSchAcctMbr & ":E#" & RPEntity & ":C#Local:S#" & PriorYearScenario & ":T#" & PriorYearTime & ":V#Annotation:F#" & PriorYearRPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#" & commentNbr
											cellInfo4 = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, "BudFm", getcellScript4)
											cellValue4 = cellInfo4.DataCellEx.DataCellAnnotation
											setcellScript4 = "A#" & newProjSchAccount4 & ":E#" & RPEntity & ":C#Local:S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#" & commentNbr
											lstMemberScriptAndValue4.Add(New MemberScriptAndValue("BudFm", setcellScript4, 0, True, cellValue4))
										Next
									End If

								
									r+=1
								Next  'For Each projSchAcctMbr As String In projSchedList
								
								'write the projectSched annotations to the database
								objXFResult4 = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue4)

								#End Region 'copy Page3 Data Attachments - Project Schedule for RD
								
							End If  'If RPAppr = "RD" Then							
							
							If RPAppr = "PCI" Then
								#Region "Copy Page2 Data Attachments and some Page 3 Misc - for PCI"
								'=======================================================================================
								'create a string of DataAttachment record accounts to copy to the new RP
								'=======================================================================================
								Dim dataAttachRecordsList As List(Of String) = New List(Of String) From {
									"Lead_Office1",
									"Lead_Office2",
									"Lead_Office3",
									"Exec_Summary",
									"Older_Related_RP1",
									"Older_Related_RP2",
									"Older_Related_RP3",
									"Invest_Desc_PCI",
									"Justification_PCI",
									"KeyMilestones_BY_PCI",
									"KeyMilestones_CY_PCI"
								}

								'copy the prior year's RP required DataAttachment records to the new RP
								Dim lstMemberScriptAndValue As New List (Of MemberScriptAndValue)
								Dim objXFResultAllOthers As New XFResult
								Dim getcellScript As String = String.Empty
								Dim setcellScript As String = String.Empty
								Dim cellInfo As New DataCellInfoUsingMemberScript
								Dim cellValue As String = String.Empty
								Dim newAccount As String = String.Empty
								Dim m As Integer = 1
							
								For Each acctMbr As String In dataAttachRecordsList
									Select Case acctMbr
										Case "KeyMilestones_BY_PCI"  'Case1: move BY to CY
											newAccount = "KeyMilestones_CY_PCI"
										Case "KeyMilestones_CY_PCI"  'Case2: move CY to PY
											newAccount = "KeyMilestones_PY_PCI"
										Case Else  'Case3: otherwise the account member remains the same
											newAccount = acctMbr
									End Select
								
									'get Prior Year RP annotation value
									getcellScript = "A#" & acctMbr & ":E#" & RPEntity & ":C#Local:S#" & PriorYearScenario & ":T#" & PriorYearTime & ":V#Annotation:F#" & PriorYearRPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
									cellInfo = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, "BudFm", getcellScript)
									cellValue = cellInfo.DataCellEx.DataCellAnnotation
										
									'write Current Year RP annotation value
									setcellScript = "A#" & newAccount & ":E#" & RPEntity & ":C#Local:S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
									'add the member scripts to the list and store as an annotation
									lstMemberScriptAndValue.Add(New MemberScriptAndValue("BudFm", setcellScript, 0, True, cellValue))
									m+=1
								Next
							
								'write the annotations to the database
								objXFResultAllOthers = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)
							
								#End Region 'Copy Page2 Data Attachments and some Page 3 Misc - for PCI
								
								#Region "Copy Page3 Data Attachments - OverallInvestmentFunding - for PCI"
								'=======================================================================================
								'create a string of overall investment funding accounts to copy to the new RP
								'=======================================================================================
								Dim overallInvestmentFundingList As List(Of String) = New List(Of String) From {
									"OverallInvestFunding_OS_BY_PCI",
									"OverallInvestFunding_OS_CY_PCI",
									"OverallInvestFunding_OS_PY_PCI",
									"OverallInvestFunding_PCI_BY_PCI",
									"OverallInvestFunding_PCI_CY_PCI",
									"OverallInvestFunding_PCI_PY_PCI",
									"OverallInvestFunding_RD_BY_PCI",
									"OverallInvestFunding_RD_CY_PCI",
									"OverallInvestFunding_RD_PY_PCI",
									"OverallInvestFunding_Legacy_BY_PCI",
									"OverallInvestFunding_Legacy_CY_PCI",
									"OverallInvestFunding_Legacy_PY_PCI",
									"OverallInvestFunding_TotalFunding_BY_PCI",
									"OverallInvestFunding_TotalFunding_CY_PCI",
									"OverallInvestFunding_TotalFunding_PY_PCI",
									"OverallInvestFunding_Obligations_BY_PCI",
									"OverallInvestFunding_Obligations_CY_PCI",
									"OverallInvestFunding_Obligations_PY_PCI",
									"OverallInvestFunding_Expends_BY_PCI",
									"OverallInvestFunding_Expends_CY_PCI",
									"OverallInvestFunding_Expends_PY_PCI"
								}

								Dim lstMemberScriptAndValue2 As New List (Of MemberScriptAndValue)
								Dim objXFResultAllOthers2 As New XFResult
								Dim getcellScript2 As String = String.Empty
								Dim setcellScript2 As String = String.Empty
								Dim cellInfo2 As New DataCellInfoUsingMemberScript
								Dim cellValue2 As String = String.Empty
								Dim newFundingAccount As String = String.Empty
								Dim getcellScriptPriorYrs As String = String.Empty
								Dim cellInfoPriorYrs As New DataCellInfoUsingMemberScript
								Dim cellValuePriorYrs As String = String.Empty
								Dim decNewPriorYearsAmt As Decimal = 0
								Dim decPYAmt As Decimal = 0
								Dim o As Integer = 1
						
								For Each fundingAcctMbr As String In overallInvestmentFundingList
								
									If fundingAcctMbr.Contains("BY") Then
										newFundingAccount = fundingAcctMbr.Replace("BY","CY")
									Else If fundingAcctMbr.Contains("CY") Then
										newFundingAccount = fundingAcctMbr.Replace("CY","PY")
									Else If fundingAcctMbr.Contains("PY") Then
										newFundingAccount = fundingAcctMbr.Replace("PY","PriorYears")
										getcellScriptPriorYrs = "A#" & newFundingAccount & ":E#" & RPEntity & ":C#Local:S#" & PriorYearScenario & ":T#" & PriorYearTime & ":V#Annotation:F#" & PriorYearRPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
										cellInfoPriorYrs = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, "BudFm", getcellScriptPriorYrs)
										cellValuePriorYrs = cellInfoPriorYrs.DataCellEx.DataCellAnnotation.Replace("$","").Replace(",","")
										
										'first, initialize the numeric variable with the prior year's RP prior year value, THEN we will add
										'the prior year's RP PY value, and FINALLY save it to the current year's RP prior year value
										If cellValuePriorYrs.Length = 0 Then
											decNewPriorYearsAmt = 0
										Else
											decNewPriorYearsAmt = cellValuePriorYrs.XFConvertToDecimal()
										End If
									Else
										newFundingAccount = fundingAcctMbr
									End If
							
							
									getcellScript2 = "A#" & fundingAcctMbr & ":E#" & RPEntity & ":C#Local:S#" & PriorYearScenario & ":T#" & PriorYearTime & ":V#Annotation:F#" & PriorYearRPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
									cellInfo2 = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, "BudFm", getcellScript2)
									cellValue2 = cellInfo2.DataCellEx.DataCellAnnotation.Replace("$","").Replace(",","")
							
									'add PY to PriorYears depending on which of the two fields contain values
									If fundingAcctMbr.EndsWith("PY_PCI") Then
									
										If cellValue2.Length = 0 Then
											decPYAmt = 0
										Else
											decPYAmt = cellValue2.XFConvertToDecimal()
										End If
								
										'add PY amt to PriorYears amt and convert it back to a string
										If cellValuePriorYrs.Length > 0 And cellValue2.Length > 0 Then
											decNewPriorYearsAmt = decNewPriorYearsAmt + decPYAmt
											cellValuePriorYrs = "$" & decNewPriorYearsAmt.ToString("N0")
									
										Else If cellValuePriorYrs.Length > 0 And cellValue2.Length = 0 Then
											cellValuePriorYrs = "$" & decNewPriorYearsAmt.ToString("N0")
										
										Else If cellValuePriorYrs.Length = 0 And cellValue2.Length > 0 Then
											cellValuePriorYrs = "$" & decPYAmt.ToString("N0")
									
										Else
											cellValuePriorYrs = ""
									
										End If
								
								
										'save the new combined PriorYears amt to the current year's RP prior year's field
										setcellScript2 = "A#" & newFundingAccount & ":E#" & RPEntity & ":C#Local:S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
										lstMemberScriptAndValue2.Add(New MemberScriptAndValue("BudFm", setcellScript2, 0, True, cellValuePriorYrs))
								
									Else
										setcellScript2 = "A#" & newFundingAccount & ":E#" & RPEntity & ":C#Local:S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
										lstMemberScriptAndValue2.Add(New MemberScriptAndValue("BudFm", setcellScript2, 0, True, "$" & cellValue2.XFConvertToDecimal().ToString("N0")))
									End If
									o+=1
									
								Next  'For Each fundingAcctMbr As String In overallInvestmentFundingList
						
								'write the funding amount annotations to the database
								objXFResultAllOthers2 = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue2)
						
								#End Region 'Copy Page3 Data Attachments - OverallInvestmentFunding - for PCI
								
								#Region "Copy Page3 Data Attachments - Investment Schedule - for PCI"
								'=======================================================================================
								'create a string of investment schedule account annotations to copy to the new RP
								'=======================================================================================
								Dim investSchedList As List(Of String) = New List(Of String) From {
									"InvestSched_BY_Desc_PCI",
									"DesignWork_BY_Init_PCI",
									"DesignWork_BY_Comp_PCI",
									"ProjectWork_BY_Init_PCI",
									"ProjectWork_BY_Comp_PCI",
									"InvestSched_CY_Desc_PCI",
									"DesignWork_CY_Init_PCI",
									"DesignWork_CY_Comp_PCI",
									"ProjectWork_CY_Init_PCI",
									"ProjectWork_CY_Comp_PCI"
								}
						
								Dim lstMemberScriptAndValue3 As New List (Of MemberScriptAndValue)
								Dim objXFResultAllOthers3 As New XFResult
								Dim getcellScript3 As String = String.Empty
								Dim setcellScript3 As String = String.Empty
								Dim cellInfo3 As New DataCellInfoUsingMemberScript
								Dim cellValue3 As String = String.Empty
								Dim newInvSchAccount3 As String = String.Empty
								Dim getcellScriptPriorYrs3 As String = String.Empty
								Dim cellInfoPriorYrs3 As New DataCellInfoUsingMemberScript
								Dim cellValuePriorYrs3 As String = String.Empty
								Dim p As Integer = 1
						
								For Each invSchAcctMbr As String In investSchedList
									Dim ud8CommentList As List(Of String) = New List(Of String)()
									ud8CommentList.Add("Comment_01")
									ud8CommentList.Add("Comment_02")
									ud8CommentList.Add("Comment_03")
									ud8CommentList.Add("Comment_04")
									ud8CommentList.Add("Comment_05")
							
									If invSchAcctMbr.Contains("BY") Then
										newInvSchAccount3 = invSchAcctMbr.Replace("BY","CY")
										For Each commentNbr In ud8CommentList
											getcellScript3 = "A#" & invSchAcctMbr & ":E#" & RPEntity & ":C#Local:S#" & PriorYearScenario & ":T#" & PriorYearTime & ":V#Annotation:F#" & PriorYearRPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#" & commentNbr
											cellInfo3 = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, "BudFm", getcellScript3)
											cellValue3 = cellInfo3.DataCellEx.DataCellAnnotation
											setcellScript3 = "A#" & newInvSchAccount3 & ":E#" & RPEntity & ":C#Local:S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#" & commentNbr
											lstMemberScriptAndValue3.Add(New MemberScriptAndValue("BudFm", setcellScript3, 0, True, cellValue3))
										Next
									Else If invSchAcctMbr.Contains("CY") Then
										newInvSchAccount3 = invSchAcctMbr.Replace("CY","PY")
										For Each commentNbr In ud8CommentList
											getcellScript3 = "A#" & invSchAcctMbr & ":E#" & RPEntity & ":C#Local:S#" & PriorYearScenario & ":T#" & PriorYearTime & ":V#Annotation:F#" & PriorYearRPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#" & commentNbr
											cellInfo3 = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, "BudFm", getcellScript3)
											cellValue3 = cellInfo3.DataCellEx.DataCellAnnotation
											setcellScript3 = "A#" & newInvSchAccount3 & ":E#" & RPEntity & ":C#Local:S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#" & commentNbr
											lstMemberScriptAndValue3.Add(New MemberScriptAndValue("BudFm", setcellScript3, 0, True, cellValue3))
										Next
									End If
									p+=1
								Next  'For Each invSchAcctMbr As String In investSchedList
						
								'write the investSched annotations to the database
								objXFResultAllOthers3 = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue3)
						
								#End Region 'Copy Page3 Data Attachments - Investment Schedule - for PCI
								
							End If  'If RPAppr = "PCI" Then
						
						End If  'line188---If flowMbrString.Contains(RPName) Then
						
						i+=1
						#End Region 'loop through all UD1 members where Text1=CreateRPForMe
						
					Next  'For Each ud1mbr As MemberInfo In UD1MbrList

					'=======================================================================================
					'display a message box that the RPs were successfully created
					'=======================================================================================
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					selectionChangedTaskResult.IsOK = True
					selectionChangedTaskResult.ShowMessageBox = True
					selectionChangedTaskResult.Message = "Resource Proposals:" & 
					environment.NewLine	& strCreatedRPs &
					"Successfully Created!" & 
					environment.NewLine	&
					environment.NewLine	& "These RPs already exist:" & 
					environment.NewLine	& strPreExistingRPs &
					environment.NewLine	& "Please ensure you fill out Page 1, Page 2, and Page 3 (if applicable) for each proposal before proceeding to enter and calculate costs."
					'set the RP to show
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Number_" & RPAppr, strFirstRP)
					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					
					Return selectionChangedTaskResult
				End If  'wfTime > "2026"

			End If   'Else PriorYearScenario is populated
			Return Nothing
			Return Nothing
		End Function
		Private Function CreateWorkingVersionOfRP() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.CreateWorkingVersionOfRP ====
						
						Dim wfTime As String = args.NameValuePairs("WFTime")
						Dim wfScenario As String = args.NameValuePairs("WFScenario")
						Dim wfCube As String = args.NameValuePairs("WFCube")
						Dim SourceRPName As String = args.NameValuePairs("RPNumber")

						' Check if source RP is already a working version. Continue only if it is not
						If rpUtils.Is_Working_Version(si, SourceRPName) Then
							Throw New Exception ("RP selected is already a working version")
						End If

						' Copy attributes From source RP To target RP. It involves teh follwing steps
						' 1. Create a workign version (if does not exist) of  source RP
						' 2. Copy annotations from source RP to working version o
						' 3. Copy all the calculated costs (i.e data records) from source RP to workign version 
											
						Dim WvRPName As String = rpUtils.Create_WorkingVersion_of_RP(
																					si,
																					wfCube,
																					wfScenario, ' Source Scenario
																					wfScenario,	' Target Scenario				
																					SourceRPName
																					)

						Dim WvRPEntity =  rpUtils.Get_RP_Entity(si, WvRPName)								
						Dim RPAppr = rpUtils.Get_RP_Appropriation(si, WvRPName)
																				
						'Show a message box that the RP was successfully created
						Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
						selectionChangedTaskResult.IsOK = True
						selectionChangedTaskResult.ShowMessageBox = True
						selectionChangedTaskResult.Message = "Working Version" & 
						environment.NewLine	& "'" & GetDescription(si,WvRPName) & "'" &
						environment.NewLine	& "Successfully Created"
						SetRoutingNumber(selectionChangedTaskResult.ModifiedCustomSubstVars, RPAppr, WvRPName)
						'brapi.ErrorLog.LogMessage(si,"WvRPName : " & WvRPName)
						'brapi.ErrorLog.LogMessage(si,"WvRPEntity : " & WvRPEntity)
						SetRoutingPageCompat(selectionChangedTaskResult.ModifiedCustomSubstVars, RPAppr, RPAppr & "_RP_Page1")
						SetRoutingContent(selectionChangedTaskResult.ModifiedCustomSubstVars, RPAppr, RPAppr & "_RP_Content")
						selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
						Return selectionChangedTaskResult
							 
			Return Nothing
		End Function
		Private Function CurrentScenarioManagement() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.CurrentScenarioManagement ====
						
						'Get Time from current Workflow
						Dim wfYear As String = args.NameValuePairs("WFYear")
						
						'Get the updated working scenario
						Dim updatedyyMinusTwo As String = args.NameValuePairs("UpdatedYYMinusTwo")
						Dim updatedyyMinusOne As String = args.NameValuePairs("UpdatedyyMinusOne")
						Dim updatedyy As String = args.NameValuePairs("Updatedyy")
						Dim updatedyyPlusOne As String = args.NameValuePairs("UpdatedyyPlusOne")
						Dim updatedyyPlusTwo As String = args.NameValuePairs("UpdatedyyPlusTwo")
						Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()

							'If the updated working scenario is not blank, set the Workcen_FYXX default parameter to the updated working scenario
							'BY-2
							If (Not updatedyyMinusTwo = "") Then	
								Dim wfYearMinusTwo As String = (wfYear.XFConvertToInt - 2).ToString.Substring(2)
								BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "WorkScen_FY" & wfYearMinusTwo, updatedyyMinusTwo)
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_CurrScenYYMinusTwo_ADM", String.Empty)
							
							End If
				
							'BY-1
							If (Not updatedyyMinusOne = "") Then
								Dim wfYearMinusOne As String = (wfYear.XFConvertToInt - 1).ToString.Substring(2)
								BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "WorkScen_FY" & wfYearMinusOne, updatedyyMinusOne)
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_CurrScenYYMinusOne_ADM", String.Empty)
						
							End If	
							
							'BY
							If (Not updatedyy = "") Then
								Dim wfYearBY As String = (wfYear.XFConvertToInt).ToString.Substring(2)
								BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "WorkScen_FY" & wfYearBY, updatedyy)
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_CurrScenYY_ADM", String.Empty)
	
							End If
							
							'BY+1
							If (Not updatedyyPlusOne = "") Then
								Dim wfYearPlusOne As String = (wfYear.XFConvertToInt + 1).ToString.Substring(2)
								BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "WorkScen_FY" & wfYearPlusOne, updatedyyPlusOne)
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_CurrScenYYPlusOne_ADM", String.Empty)
	
							End If	
							
							'BY+2
							If (Not updatedyyPlusTwo = "") Then
								Dim wfYearPlusTwo As String = (wfYear.XFConvertToInt + 2).ToString.Substring(2)
								BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "WorkScen_FY" & wfYearPlusTwo, updatedyyPlusTwo)
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_CurrScenYYPlusTwo_ADM", String.Empty)

							End If
							
						'Clear Combo Boxes and Set Parameter Values
						selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
						Return selectionChangedTaskResult

			Return Nothing
		End Function
		Private Function DefualtYesOrNo() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.DefualtYesOrNo ====
	 
	 
	 		Dim WFTime As String = args.NameValuePairs("WFTime")
			Dim WFScenario As String = args.NameValuePairs("WFScenario")
			Dim Entity As String = args.NameValuePairs("Entity")
			Dim RPname As String = args.NameValuePairs("RPname")

	 
	 		Dim params As New Dictionary(Of String, String) 
			params.Add("WFTime", WFTime)
			params.Add("WFScenario", WFScenario) 
			params.Add("Entity", Entity) 
			params.Add("RPname", RPname)

			
			
			brapi.Utilities.StartDataMgmtSequence(si, "Mass_No_Delete", params)
				

			Return Nothing
		End Function
		Private Function DeleteBLTLine_OS() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.DeleteBLTLine_OS ====
					
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
					Dim RPName As String = args.NameValuePairs("RPName")
					Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
					Dim LineItemNum As String = args.NameValuePairs("LineItemNum") 
					Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
					Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")

					If  String.IsNullOrEmpty (LineItemNum) Then 
						Throw New Exception("Please choose a Line Item") 
					End If
					
					RunPreSaveStepsForRP_BLT_NBLT_Deletion(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, LineItemNum)
					'RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, LineItemNum)
				
						'added this evaluation to deal with billet over 99 since they would have 12 characters vs. 11			
						Dim LineItemNumIntLength As Integer = LineItemNum.Length
						Dim LineItemNumInt As Integer
						If LineItemNumIntLength = 11
							LineItemNumInt = LineItemNum.Substring(9,2).XFConvertToInt
						Else If LineItemNumIntLength = 12
							LineItemNumInt = LineItemNum.Substring(9,3).XFConvertToInt
						End If
						
						Using dbConnApp As DBConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							
						'1) First, Delete the current line item from the data attachment table
						Dim deleteSql As New Text.StringBuilder  				
						deleteSql.Append("Delete ")		
						deleteSql.Append("From dbo.DataAttachment ")
						deleteSql.Append(" Where Cube = '" & wfCube & "' ")
						deleteSql.Append(" AND Time = '" & wfTime & "' ")
						deleteSql.Append(" AND Scenario = '" & wfScenario & "' ")	
						deleteSql.Append(" AND Entity = '" & RP_Entity & "' ")
						deleteSql.Append(" AND Flow = '" & RPName & "' ")
						deleteSql.Append(" AND UD6 = '" & LineItemNum & "' ")
						'execute the query 
						BRApi.Database.ExecuteSql(dbConnApp, deleteSql.ToString, False)
						
'						2) Next, Update the line items to move them down 1. E.g. LineItem_02 becomes LineItem_01
						Dim updateSql As New Text.StringBuilder 
						updateSql.Append("Update ")	
	            		updateSql.Append(" dbo.DataAttachment ")
						updateSql.Append(" set UD6 = Replace(UD6, substring(UD6, 10, 3), format((Convert(INT, substring(UD6, 10, 3))-1), '0#')) ")
						updateSql.Append(" Where Cube = '" & wfCube & "' ")
						updateSql.Append(" AND Time = '" & wfTime & "' ")
						updateSql.Append(" AND Scenario = '" & wfScenario & "' ")	
						updateSql.Append(" AND Entity = '" & RP_Entity & "' ")
						updateSql.Append(" AND Flow = '" & RPName & "' ")
						updateSql.Append(" AND substring(UD6, 0, 10) = 'LineItem_' ")
						updateSql.Append(" AND Convert(INT, substring(UD6, 10, 3)) > " & LineItemNumInt & " ")
						'execute the update query 
						BRApi.Database.ExecuteSql(dbConnApp, updateSql.ToString, False)
						
						'3)Update the actual stored data using a finance business rule						
						Dim povInfo As New Dictionary(Of String, String) 
						povInfo.Add("Cube", wfCube)
						povInfo.Add("Consolidation", "Local")
						povInfo.Add("Scenario", wfScenario)
						povInfo.Add("View", "Periodic")
						povInfo.Add("Entity", rp_Entity)
						povInfo.Add("Time", wfTime)
						
						globals.SetStringValue("WFTime", wfTime) 
						globals.SetStringValue("rpName", rpName) 		
						globals.SetStringValue("LineItemNum", LineItemNum)
						
						brapi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si, "USCG_RP_CostCalc_Billet", "DeleteBillet", povInfo, customcalculatetimetype.MemberFilter)
						
						'Show a message box that the Billet was successfully updated						
						Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
						Dim stringmessage As String = "" & GetDescription(si,RPName) & " " & GetUD6Description(si,LineItemNum) & " Successfully Deleted"
						
						selectionChangedTaskResult = Me.RefreshSelectedBillet_OS(si, args, globals, wfCube, wfTime, wfScenario, RPName, LineItemNum, stringmessage)						
						Return selectionChangedTaskResult
							
					End Using
						
			Return Nothing
		End Function
		Private Function DeleteBLTLine_OS_Mass() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.DeleteBLTLine_OS_Mass ====
					
					
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
					Dim RPName As String = args.NameValuePairs("RPName")
					Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)
				    Dim BilletsList As String =  args.NameValuePairs("billet_delete")
					Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
					Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")
                  	Dim billetNumber As New List(Of Integer)
                    Dim billetD As String
			   	    Dim i As Integer
    			    Dim number_of_Billets As Integer = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Number_of_Billets:E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").DataCellEx.DataCellAnnotation.XFConvertToInt
			      
				    Dim LineItemNum As List (Of String)  = StringHelper.SplitString(BilletsList,",")
	
					For i =  0 To LineItemNum.Count -1
                      
						billetD = LineItemNum.Item(i)
			         	Dim numberPt As Integer = CInt(billetD.Substring(7))
					 	billetNumber.Add(numberPt)
					 
				   	Next
				
                 	billetNumber.Sort()
                 	billetNumber.Reverse()
				 
					
					RunPreSaveStepsForRP_BLT_NBLT_Deletion(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, "NA_LineItem")
					
					
				 	For Each num As Integer In billetNumber
						If Len(num.ToString()) = 1 Then
					  	billetD = "LineItem_0" & num.ToString()
					Else
					  	billetD = "LineItem_" & num.ToString()
				    End If 
				  
				 
		            DeleteMassBillets(si,globals,args, RP_Entity , RPName, wfCube,  wfScenario, wfTime,description_ChangeLog,reason_ChangeLog, billetD)
             			
				 	Next
				   
					
					Dim params As New Dictionary(Of String, String) 
					params.Add("WFTime", WFTime)
					params.Add("WFScenario", WFScenario) 
					params.Add("Entity", RP_Entity) 
					params.Add("RPname", RPName)

					brapi.Utilities.StartDataMgmtSequence(si, "Mass_No_Delete", params)
             						
					'Show a message box that the Billet was successfully updated						
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Reason_ChangeLog_OS", "")
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Description_ChangeLog_OS", "")

					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = True
					selectionChangedTaskResult.IsOK = True
					
					selectionChangedTaskResult.ShowMessageBox = True
					selectionChangedTaskResult.Message = "Billets " &  BilletsList & " have been deleted in RP " & RPName 
				
					Return selectionChangedTaskResult
				 	
			Return Nothing
		End Function
		Private Function DeleteBilletList() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.DeleteBilletList ====
		
				
			'This is a specific function to generate the list of billets to be deleted
			Dim wfTime As String = args.NameValuePairs("wfTime")
			Dim wfCube As String = args.NameValuePairs("wfCube")
			Dim wfScenario As String = args.NameValuePairs("wfScenario")
			Dim RPName As String = args.NameValuePairs("RPName")
			Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)	
			Dim LineItemNum As String = args.NameValuePairs("LineItemNum")
		    Dim selectionChangeTaskResult As New XFSelectionChangedTaskResult
			
		    'Get number of billets in RP from Edit RP Page 1
          	Dim numOfBillets As Integer = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Number_of_Billets:E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").DataCellEx.DataCellAnnotation.XFConvertToInt
			
   		    Dim billetDeleteList As New List (Of String)	
			Dim billetDeleteString  = ""
			Dim billetDeleteString2 = ""
		 

			
   		  	For i As Int32  = 1 To numOfBillets
				
				Dim billetNum As String = ""
		
				If i < 10 Then
			
					billetNum = "LineItem_0" & i.ToString
								 
				Else 
				    
					billetNum = "LineItem_" & i.ToString
							

				End If
						 							
					
				Dim attribute As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Billets_Yes_No_Input:E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Periodic:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#" & billetNum & ":U7#None:U8#None")
				Dim attributeValue As Integer = attribute.DataCellEx.DataCell.CellAmount
				
				If attributeValue = 1 Then 
						
					billetDeleteList.Add(billetNum)
					
				End If		 
						 
			Next i
            
 			'Looping throught the LineItem list. Changing the LineItem_ name into the desrcription name Billet_ and separating it by commas.
        	For Each billetname As String In billetDeleteList
	        	
				If billetDeleteString = ""
				 	billetDeleteString = Environment.NewLine & "Billet " & billetname.Split("_")(1)
				Else  
					billetDeleteString = billetDeleteString & ","  & environment.NewLine & "Billet " &  billetname.Split("_")(1) 
				End If
				 	 			
		 	 Next 
		  
			If billetDeleteString = ""
				Throw New Exception( "No billets are selected to be deleted. Please specify which billets should be deleted.") 
			End If		
			  
                      
			selectionChangeTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Mass_BLT_Delete",billetDeleteString)
		
			selectionChangeTaskResult.IsOK = True
			selectionChangeTaskResult.ShowMessageBox = True
			selectionChangeTaskResult.ChangeCustomSubstVarsInDashboard = True
			
		    'Throw an error message if no billets were selected to be deleted
						
	
			BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prm_Mass_BLT_Delete", billetDeleteString) 
			
			Return selectionChangeTaskResult
				

			Return Nothing
		End Function
		Private Function DeleteModHierarchyMember() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.DeleteModHierarchyMember ====
						
						Dim selectedMember As String = args.NameValuePairs("selectedMember")	
						Dim targetDimVal As String = "Std_Flow"		
						Dim targetDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, targetDimVal)		
						Dim selectedMemberPk As memberPk = BRApi.Finance.Members.GetMember(si, TargetDimPk.DimTypeId, selectedMember).MemberPk
						
						'Need to throw error message in here and kickout if the member has children that those need to be removed first
						'first check to see if the member has children
						Dim selectedMemberHasChildren As Boolean = BRApi.Finance.Members.HasChildren(si, targetDimPk, selectedMemberPk.MemberId)
						
						If selectedMemberHasChildren Then
							Dim selectedMemberDescr As String = BRApi.Finance.Members.GetMember(si, dimTypeId.Flow, selectedMember).Description			
							Throw New Exception(selectedMemberDescr & " has children and cannot be deleted until those relationships are removed.") 			
						Else 'Member does not have children so delete the member
							'Remove the Member
							BRApi.Finance.MemberAdmin.RemoveMember(si, targetDimPk, selectedMemberPk)
						End If
						
			Return Nothing
		End Function
		Private Function DeleteNonBLTLine_OS() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.DeleteNonBLTLine_OS ====
					
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
					Dim RPName As String = args.NameValuePairs("RPName")
					Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
					Dim LineItemNum As String = args.NameValuePairs("LineItemNum") 
					Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
					Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")

					If  String.IsNullOrEmpty (LineItemNum) Then 
						Throw New Exception("Please choose a Line Item") 
					End If
					
					'RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, LineItemNum)
					RunPreSaveStepsForRP_BLT_NBLT_Deletion(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, LineItemNum)
										
						'added this evaluation to deal with cost estimate over 99 since they would have 14 characters vs. 13		
						Dim LineItemNumIntLength As Integer = LineItemNum.Length
						Dim LineItemNumInt As Integer
						If LineItemNumIntLength = 13
							LineItemNumInt = LineItemNum.Substring(11,2).XFConvertToInt
						Else If LineItemNumIntLength = 14
							LineItemNumInt = LineItemNum.Substring(11,3).XFConvertToInt
						End If
						
						Using dbConnApp As DBConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							
						'1) First, Delete the current line item from the data attachment table
						Dim deleteSql As New Text.StringBuilder  				
						deleteSql.Append("Delete ")		
						deleteSql.Append("From dbo.DataAttachment ")
						deleteSql.Append(" Where Cube = '" & wfCube & "' ")
						deleteSql.Append(" AND Time = '" & wfTime & "' ")
						deleteSql.Append(" AND Scenario = '" & wfScenario & "' ")	
						deleteSql.Append(" AND Entity = '" & RP_Entity & "' ")
						deleteSql.Append(" AND Flow = '" & RPName & "' ")
						deleteSql.Append(" AND UD6 = '" & LineItemNum & "' ")
						'execute the query 
						'brapi.ErrorLog.LogMessage(si, deleteSql.ToString)
						BRApi.Database.ExecuteSql(dbConnApp, deleteSql.ToString, False)
						
'						2) Next, Update the line items to move them down 1. E.g. LineItem_02 becomes LineItem_01
						Dim updateSql As New Text.StringBuilder 
						updateSql.Append("Update ")	
	            		updateSql.Append(" dbo.DataAttachment ")
						updateSql.Append(" set UD6 = Replace(UD6, substring(UD6, 12, 3), format((Convert(INT, substring(UD6, 12, 3))-1), '0#')) ")
						updateSql.Append(" Where Cube = '" & wfCube & "' ")
						updateSql.Append(" AND Time = '" & wfTime & "' ")
						updateSql.Append(" AND Scenario = '" & wfScenario & "' ")	
						updateSql.Append(" AND Entity = '" & RP_Entity & "' ")
						updateSql.Append(" AND Flow = '" & RPName & "' ")
						updateSql.Append(" AND substring(UD6, 0, 12) = 'NBLineItem_' ")
						updateSql.Append(" AND Convert(INT, substring(UD6, 12, 3)) > " & LineItemNumInt & " ")
						'execute the update query 
						'brapi.ErrorLog.LogMessage(si, updateSql.ToString)
						BRApi.Database.ExecuteSql(dbConnApp, updateSql.ToString, False)
						
						'3)Update the actual stored data using a finance business rule						
						Dim povInfo As New Dictionary(Of String, String) 
						povInfo.Add("Cube", wfCube)
						povInfo.Add("Consolidation", "Local")
						povInfo.Add("Scenario", wfScenario)
						povInfo.Add("View", "Periodic")
						povInfo.Add("Entity", rp_Entity)
						povInfo.Add("Time", wfTime)
						
						globals.SetStringValue("WFTime", wfTime) 
						globals.SetStringValue("rpName", rpName) 		
						globals.SetStringValue("LineItemNum", LineItemNum)
						
						brapi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si, "USCG_RP_CostCalc_NonBillet", "DeleteNonBillet", povInfo, customcalculatetimetype.MemberFilter)
						
						'Show a message box that the Billet was successfully updated						
						Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
						Dim stringMessage As String = "" & GetDescription(si,RPName) & " " & GetUD6Description(si,LineItemNum) & " Successfully Deleted"
						selectionChangedTaskResult = Me.RefreshSelectedLineItem_OS(si, globals, wfCube, wfTime, wfScenario, RPName, LineItemNum, stringMessage )			
						Return selectionChangedTaskResult
							
					End Using
						
			Return Nothing
		End Function
		Private Function DeleteSupportingDoc() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.DeleteSupportingDoc ====

			'Get the RPName
			Dim RPName As String = args.NameValuePairs("RPName")
			
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()					
			'First, check if the RP is in Edit Mode or View Only Mode 
			If Not rpUtils.Is_RP_Editable(si, RPName)	
					'Mode is view only so do nothing and show the user a message that states its in view only mode
					selectionChangedTaskResult.IsOK = True
					selectionChangedTaskResult.ShowMessageBox = True
					selectionChangedTaskResult.Message = "" & GetDescription(si,RPName) & " Is set to View Only.  No edits can be made."
					Return selectionChangedTaskResult	
												
			Else 'Mode is edit so udpate the RP		
				
			'Get the unique Id from the supporting document
			Dim uniqueID As String = args.NameValuePairs("UniqueID")
			If String.IsNullOrEmpty(uniqueID) Then Throw New Exception("No Document Selected")
				
			Dim sql As New Text.StringBuilder
			sql.Append("DELETE FROM dbo.DataAttachment ")
            sql.Append("WHERE UniqueID = '" & uniqueID & "' ")
			
				Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
	            Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp, sql.ToString, True)
				
			
					'Mode is view only so do nothing and show the user a message that states its in view only mode
					selectionChangedTaskResult.IsOK = True
					selectionChangedTaskResult.ShowMessageBox = True
					selectionChangedTaskResult.Message = "Supporting Document Successfully Deleted"
					Return selectionChangedTaskResult	
				
				End Using
				
			End If 'rpMode
			
			Return Nothing
		End Function
		Private Function DownloadSupportingDoc() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.DownloadSupportingDoc ====
			
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
						
			'Get the unique Id from the supporting document
			Dim uniqueID As String = args.NameValuePairs("UniqueID")
			If String.IsNullOrEmpty(uniqueID) Then Throw New Exception("No Document Selected")
			
            Dim sql As New Text.StringBuilder
            sql.Append("SELECT FileName, FileBytes, UniqueID ")
            sql.Append("FROM dbo.DataAttachment WITH(NOLOCK) ")
            sql.Append("WHERE UniqueID = '" & uniqueID & "' ")
						
			Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
            Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp, sql.ToString, True)
                        If dt IsNot Nothing AndAlso dt.Rows.Count = 1 Then
                                    'Check to see if file as compressed
                                    Dim fileName As String = dt.Rows(0)("FileName")
                                    Dim fileBytes As Byte() = dt.Rows(0)("FileBytes")

                                    If Me.FileCanCompress(si, fileName) Then
                                                Try
                                                            fileBytes = CryptoManager.BytesDecompress(si, fileBytes)
                                                Catch
                                                            'Do nothing, file was not compressed
                                                End Try
                                    End If
									
                        BRApi.Utilities.SaveFileBytesToUserTempFolder(si, si.UserName, fileName, fileBytes)
						
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = False
							selectionchangedtaskresult.Message = " "
						
							Return selectionChangedTaskResult
                        Else
							
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = True
							selectionchangedtaskresult.Message = "File does not exist and may have been deleted."
						
							Return selectionChangedTaskResult
                        End If
            End Using
			
			Return Nothing
		End Function
		Private Function EditBLTLine_Mass_OS() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.EditBLTLine_Mass_OS ====
					
					Dim disableCode As Boolean = True

					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
					Dim RPName As String = args.NameValuePairs("RPName")
					Dim LineItemNum As String = String.Empty
					Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)
					Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
					Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")					
					Dim increase_Decrease As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Increase_Decrease:E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").DataCellEx.DataCellAnnotation
					
					LineItemNum = "N/A" 'for change/comment log
					RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, LineItemNum)
					
					'Loop through each billet line item in RP as defined by the # of billets attribute on Edit RP Page 1
					Dim editRPScriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"		
					Dim strNumOfBillets As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Number_of_Billets:" & editRPScriptGenerics).DataCellEx.DataCellAnnotation
					Dim numOfBillets As Integer = CInt(strNumOfBillets)
					
					For billetNum As Integer = 1 To numOfBillets
						If billetNum < 10 Then
							LineItemNum = "LineItem_0" & billetNum 
						Else
							LineItemNum = "LineItem_" & billetNum
						End If
						
						'Storing the Annotation text for the attributes in a generic string
						Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#" & LineItemNum & ":U7#None:U8#None"	
						
						'Starting the same as the standard EditBLTLine_OS function, but removing any code that pulls in parameters since the spreadsheet saves directly to the POVs
						'However, we'll still need to pull those attributes in because they factor into the allocation and headcount calculations below. So they'll be changed from parameter read-ins to GetDataCells
						
						If IsOSPG1Empty(globals, si, wfCube,RP_Entity,wfScenario,wfTime,RPName) Then Throw New Exception("Empty attributes in Page 1. All attributes on Page 1 must have a value to save this page.")
						
						If disableCode = False Then
						
							'BRApi.ErrorLog.LogMessage(si, "mass billets allocation code running")
							
							'Getting values of attributes (post save on the update all billets dashboard) from their POVs in order to use them in calcs later
							Dim billet_Type As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Billet_Type:" & scriptGenerics).DataCellEx.DataCellAnnotation
							If  String.IsNullOrEmpty(billet_Type) Then Throw New Exception("Please choose Military/Civilian")
							Dim billet_UII As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Billet_UII:" & scriptGenerics).DataCellEx.DataCellAnnotation
							Dim billet_Object_Class As String = String.Empty 'leaving this empty because we don't have a parameter for it at this time
							Dim grade_Rank As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Grade_Rank:" & scriptGenerics).DataCellEx.DataCellAnnotation
							
							Dim oPFAC As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#OPFAC:" & scriptGenerics).DataCellEx.DataCellAnnotation				
							Dim oPFACID As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD4, oPFAC)
							Dim oPFAC_PPA As String = BRApi.Finance.UD.Text(si, dimTypeId.UD4, oPFACID, 1, 0, 0)
							
							Dim billet_ATU As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Billet_ATU:" & scriptGenerics).DataCellEx.DataCellAnnotation
							Dim billet_ATU_NoUnit As String = String.Empty
							If billet_ATU <> ""
								billet_ATU_NoUnit = billet_ATU 'Since the billet ATU will already be no unit as it's being selected from a list of NoUnits as opposed to ATU children on the new billet screen (same goes for similar code below)
							End If	
							
							Dim	pPE_PPA As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#PPE_PPA:" & scriptGenerics).DataCellEx.DataCellAnnotation				
							Dim pPE_ATU As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#PPE_ATU:" & scriptGenerics).DataCellEx.DataCellAnnotation
							Dim ppe_ATU_NoUnit As String=String.Empty
							If pPE_ATU <> ""
								ppe_ATU_NoUnit = pPE_ATU
							End If
							
							Dim UTL_PPA As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Utilities_PPA:" & scriptGenerics).DataCellEx.DataCellAnnotation
							Dim UTL_ATU As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Utilities_ATU:" & scriptGenerics).DataCellEx.DataCellAnnotation
							Dim UTL_ATU_NoUnit As String=String.Empty
							If UTL_ATU <> ""
								UTL_ATU_NoUnit = UTL_ATU 
							End If
							
							Dim lease_PPA As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Lease_PPA:" & scriptGenerics).DataCellEx.DataCellAnnotation
							Dim lease_ATU As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Lease_ATU:" & scriptGenerics).DataCellEx.DataCellAnnotation
							Dim lease_ATU_NoUnit As String=String.Empty
							If lease_ATU <> ""
								lease_ATU_NoUnit = lease_ATU
							End If
							
							Dim spe_Code_Occu_Series As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Spe_Code_Occu_Series:" & scriptGenerics).DataCellEx.DataCellAnnotation
							Dim CodeId As Integer = BRApi.Finance.Members.GetMemberId(si, dimtypeid.UD3, spe_Code_Occu_Series)
							Dim SpecialtyCodeText2 As String = BRApi.Finance.UD.Text(si, dimtype.UD3.Id, CodeId, 2, DimConstants.Unknown, DimConstants.Unknown)
							Dim pilot As String = SpecialtyCodeText2
							
							Dim aD_Reserve As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#AD_Reserve:" & scriptGenerics).DataCellEx.DataCellAnnotation
							Dim reserve_Type As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Reserve_Type:" & scriptGenerics).DataCellEx.DataCellAnnotation
							Dim cONUS_OCONUS As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#CONUS_OCONUS:" & scriptGenerics).DataCellEx.DataCellAnnotation
							
											
							
							'Write logic to determine whether to use OPFAC PPA or UII PPA
							Dim ppa_Option As String
							Dim billet_UII_ID As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, billet_UII)
							Dim billet_UII_PPA As String = BRApi.Finance.UD.Text(si, dimTypeId.UD2, billet_UII_ID, 1, 0, 0)
							If (billet_UII_PPA <> "" And Not billet_UII_PPA.Contains(",")) Then
								ppa_Option = billet_UII_PPA
							Else
								ppa_Option = oPFAC_PPA
							End If
								
							'Create a new list of memberscript and value to save the variables that aren't being directly saved in the spreadsheet and are instead derived in the single billet save code
							Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)
							
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Billet_PPA:" 						& scriptGenerics, 0, True, ppa_Option))
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Billet_Object_Class:" 				& scriptGenerics, 0, True, billet_Object_Class))
							
							
							'********Allocation Drivers Storage********									
							'For those attributes that are also a dimension, we will also store a 1 in that dimension member that is selected so we can find it in a data buffer for the cost
							Me.AllocationsCalc(si, globals, args, RP_Entity, RPName, wfCube, wfScenario, wfTime, LineItemNum, ppa_Option, billet_UII, billet_Object_Class, billet_ATU_NoUnit, pPE_PPA, ppe_ATU_NoUnit, UTL_PPA, UTL_ATU_NoUnit, lease_PPA, lease_ATU_NoUnit)							
								
							'********Headcount Reporting Storage********
							Dim hcScriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Periodic:F#" & RPName & ":O#Forms:I#None:U6#" & LineItemNum & ":U7#None:U8#None"			
							
							'set the Aviator variable
							Dim aviator As String = String.Empty
							If pilot = "Y"
								aviator = "Aviator"
							ElseIf pilot = "N"
								aviator = "NA_Aviator"
							End If
							
							'Set the military employment type variable
							Dim milEmpType As String = String.Empty
							If aD_Reserve.XFEqualsIgnoreCase("Active_Duty")
								milEmpType = aD_Reserve
							ElseIf aD_Reserve.XFEqualsIgnoreCase("Reserve")
								milEmpType = reserve_Type
							Else 
								milEmpType = "NA_Military_Employment_Type"
							End If
							
							'Run the Headcount Calc
							Me.HeadcountCalc(si, globals, args, RP_Entity, RPName, wfCube, wfScenario, wfTime, LineItemNum, grade_Rank, milEmpType, spe_Code_Occu_Series, cONUS_OCONUS, aviator)
							
							'Write the annotations that were not saved in the cube view to the database for this line item
							Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)	
							
						Else
							'BRApi.ErrorLog.LogMessage(si, "mass billets allocation code NOT running, clear running instead")
							
							'Need to clear out the previous allocations when Save button on Update All Billets dashboard is clicked
							'Me.AllocationsClear(si, globals, args, RP_Entity, RPName, wfCube, wfScenario, wfTime, LineItemNum)
							Me.AllocationsClearSpdshtBillets(si, globals, args, RP_Entity, RPName, wfCube, wfScenario, wfTime, LineItemNum)
							'Me.HeadcountClear(si, globals, args, RP_Entity, RPName, wfCube, wfScenario, wfTime, LineItemNum)
							Me.HeadcountClearSpdshtBillets(si, globals, args, RP_Entity, RPName, wfCube, wfScenario, wfTime, LineItemNum)
							
						End If
						
					Next
					
				 	'Show a message box that the Billet was successfully updated
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					SetRoutingContent(selectionChangedTaskResult.ModifiedCustomSubstVars, ResolveRoutingAppnForRP(RPName, args.NameValuePairs.XFGetValue("APPN_Content", "OS")), "04c_BDF_RP_Dashboard_Content_NonAddEditBillets_OS")
					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					selectionChangedTaskResult.IsOK = True
					selectionChangedTaskResult.ShowMessageBox = True
					selectionChangedTaskResult.Message = "RP " & GetDescription(si,RPname)	 & " billets successfully updated. Please refresh screen after clicking OK."
				 	Return selectionChangedTaskResult
					
			Return Nothing
		End Function
		Private Function EditBLTLine_OS() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.EditBLTLine_OS ====

					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
					Dim RPName As String = args.NameValuePairs("RPName")
					Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
					Dim routingAppn As String = ResolveRoutingAppnForRP(RPName, args.NameValuePairs.XFGetValue("APPN_Content", "OS"))
					Dim LineItemNum As String = args.NameValuePairs("LineItemNum") 
					Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
					Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")					
					Dim increase_Decrease As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Increase_Decrease:E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").DataCellEx.DataCellAnnotation

					If  String.IsNullOrEmpty (LineItemNum) Then 
						Throw New Exception("Please choose a Line Item") 
					End If
					
					If IsOSPG1Empty(globals, si, wfCube,RP_Entity,wfScenario,wfTime,RPName) Then Throw New Exception("Empty attributes in Page 1. All attributes on Page 1 must have a value to save this page.")

					Dim billet_Type As String = args.NameValuePairs("Billet_Type") 										'|!prm_BLT_BilletType!|
					If  String.IsNullOrEmpty(billet_Type) Then Throw New Exception("Please choose Military/Civilian")
					Dim grade_Type As String = args.NameValuePairs("Grade_Type") 										'|!prm_BLT_GradeType!|
					Dim grade_Rank As String = args.NameValuePairs("Grade_Rank")  										'|!prm_BLT_GradeRank!|
					Dim aD_Reserve As String = args.NameValuePairs("AD_Reserve") 										'|!prm_BLT_ADReserve!|
					Dim reserve_Type As String = args.NameValuePairs("Reserve_Type") 									'|!prm_BLT_ReserveType!|
					Dim spe_Code_Occu_Series As String = args.NameValuePairs("Spe_Code_Occu_Series") 					'|!prm_BLT_SpcCodeOccSeries!|
					Dim CodeId As Integer = BRApi.Finance.Members.GetMemberId(si, dimtypeid.UD3, spe_Code_Occu_Series)
					Dim SpecialtyCodeText2 As String = BRApi.Finance.UD.Text(si, dimtype.UD3.Id, CodeId, 2, DimConstants.Unknown, DimConstants.Unknown)
					Dim cONUS_OCONUS As String = args.NameValuePairs("ConusOConus") 									'|!prm_BLT_ConusOConus!|
					Dim pilot As String = SpecialtyCodeText2													        'Assigning Specialty Code Text2 value to Pilot
					Dim electronic_Flight_Bag As String = args.NameValuePairs("Electronic_Flight_Bag") 					'|!prm_BLT_ElectronicFlightBag!|
					Dim term_Billet As String = args.NameValuePairs("Term_Billet") 										'|!prm_BLT_TermBillet!|
					Dim pPE_Type As String = args.NameValuePairs("PPE_Type") 											'|!prm_BLT_PPEType!|
					Dim	pPE_PPA As String = args.NameValuePairs("PPE_PPA") 												'|!prm_BLT_PPE_PPA!|						
					Dim pPE_ATU As String = args.NameValuePairs("PPE_ATU") 												'|!prm_BLT_PPE_ATU!|
					Dim ppe_ATU_NoUnit As String=String.Empty
					If pPE_ATU <> ""
						ppe_ATU_NoUnit = pPE_ATU & "_NoUnit" 'Add the ATU and _NoUnit together to get the base level unit to store it at
					End If
					Dim build_Out_Choice As String = args.NameValuePairs("Build_Out_Choice") 							'|!prm_BLT_Build_Out!|
					Dim iCASS_Costs As String = args.NameValuePairs("ICASS_Costs") 										'|!prm_BLT_ICASSType!|
					Dim position_Number As String = args.NameValuePairs("Position_Number") 								'|!prm_BLT_PositionNumber!|
					
					'Position number should only be filled out for Decreases in the RAP stage.  If filled out in RAP and Increase, throw and error
					'If (position_Number.Length > 0 And wfScenario.XFContainsIgnoreCase("RAP_") And increase_Decrease.XFEqualsIgnoreCase("I")) Then Throw New Exception("Position Number should not be filled in for Increase RPs (See Page 1) in the RAP Scenario. Please clear the Position Number and save.")
						
					Dim position_Title As String = args.NameValuePairs("Position_Title") 								'|!prm_BLT_PositionTitle!|
					Dim billet_ATU As String = args.NameValuePairs("Billet_ATU") 										'|!prm_BLT_ATU!|
					Dim billet_ATU_NoUnit As String = String.Empty
					If billet_ATU <> ""
						billet_ATU_NoUnit=billet_ATU & "_NoUnit" 'Add the ATU and _NoUnit together to get the base level unit to store it at
					End If
					Dim billet_UII As String = args.NameValuePairs("Billet_UII") 										'|!prm_BLT_UII!|
					Dim billet_Object_Class As String = String.Empty 'leaving this empty because we don't have a parameter for it at this time
					Dim oPFAC As String = args.NameValuePairs("OPFAC") 													'|!prm_BLT_OPFACS!|						
					Dim oPFACID As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD4, oPFAC)
					Dim oPFAC_PPA As String = BRApi.Finance.UD.Text(si, dimTypeId.UD4, oPFACID, 1, 0, 0)
					Dim detached_Duty As String = args.NameValuePairs("Detached_Duty") 									'|!prm_BLT_DetachedDuty!|
					Dim detached_Duty_Location As String = args.NameValuePairs("Detached_Duty_Location") 				'|!prm_BLT_DutyLocation!|
					Dim background_Investigation_Type As String = args.NameValuePairs("Background_Investigation_Type") 	'|!prm_BLT_BIType!|
					Dim Acquisition_Project As String = args.NameValuePairs("Acquisition_Project") 						'|!prm_BLT_Acq_Project!|
					Dim lease_Choice As String = args.NameValuePairs("Lease_Choice") 									'|!prm_BLT_Lease!|
					Dim lease_PPA As String = args.NameValuePairs("Lease_PPA") 											'|!prm_BLT_Lease_PPA_OS!|
					Dim lease_ATU As String = args.NameValuePairs("Lease_ATU") 											'|!prm_BLT_Lease_ATU_OS!|												'|!prm_BLT_UTL_ATU!|
					Dim lease_ATU_NoUnit As String=String.Empty
					If lease_ATU <> ""
						lease_ATU_NoUnit = lease_ATU & "_NoUnit" 'Add the ATU and _NoUnit together to get the base level unit to store it at
					End If
					Dim furniture_Reqd As String = args.NameValuePairs("Furniture_Reqd") 								'|!prm_BLT_Furniture!|
					Dim utilities_Reqd As String = args.NameValuePairs("Utilities_Reqd") 								'|!prm_BLT_Utilities!|
					Dim computer_Type As String = args.NameValuePairs("Computer_Type") 									'|!prm_BLT_Computer_Type!|
					Dim lineItem_Comment As String = args.NameValuePairs("LineItem_Comment") 							'|!prm_BLT_Comment!|
					Dim UTL_PPA As String = args.NameValuePairs("UTL_PPA") 												'|!prm_BLT_UTL_PPA!|
					Dim UTL_ATU As String = args.NameValuePairs("UTL_ATU") 												'|!prm_BLT_UTL_ATU!|
					Dim UTL_ATU_NoUnit As String=String.Empty
					If UTL_ATU <> ""
						UTL_ATU_NoUnit = UTL_ATU & "_NoUnit" 'Add the ATU and _NoUnit together to get the base level unit to store it at
					End If
						
					If  String.IsNullOrEmpty (term_Billet) Then 
                        Throw New Exception("Please choose Perm / Term") 
                    End If

					RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, LineItemNum )					
						
					'Write logic to determine whether to use OPFAC PPA or UII PPA
					Dim ppa_Option As String
					Dim billet_UII_ID As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, billet_UII)
					Dim billet_UII_PPA As String = BRApi.Finance.UD.Text(si, dimTypeId.UD2, billet_UII_ID, 1, 0, 0)
					If (billet_UII_PPA <> "" And Not billet_UII_PPA.Contains(",")) Then
						ppa_Option = billet_UII_PPA
					Else
						ppa_Option = oPFAC_PPA
					End If
					
					'Storing the Annotation text for the attributes in a generic string
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#" & LineItemNum & ":U7#None:U8#None"								
					
					'Create a new list of memberscript and value
					Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)
					
					'*********Attribute Annotation Storage********
					'Add the member scripts to the list and store as 0 No data annotations
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Billet_Type:" 						& scriptGenerics, 0, True, billet_Type))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Grade_Type:" 						& scriptGenerics, 0, True, grade_Type))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Grade_Rank:" 						& scriptGenerics, 0, True, grade_Rank))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#AD_Reserve:" 						& scriptGenerics, 0, True, aD_Reserve))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Reserve_Type:" 						& scriptGenerics, 0, True, reserve_Type))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Spe_Code_Occu_Series:" 				& scriptGenerics, 0, True, spe_Code_Occu_Series))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Pilot:" 							& scriptGenerics, 0, True, pilot))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Electronic_Flight_Bag:" 			& scriptGenerics, 0, True, electronic_Flight_Bag))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Term_Billet:" 						& scriptGenerics, 0, True, term_Billet))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#PPE_Type:" 							& scriptGenerics, 0, True, pPE_Type))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#PPE_PPA:" 							& scriptGenerics, 0, True, pPE_PPA))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#PPE_ATU:" 							& scriptGenerics, 0, True, ppe_ATU_NoUnit))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Build_Out_Choice:" 					& scriptGenerics, 0, True, build_Out_Choice))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#ICASS_Costs:" 						& scriptGenerics, 0, True, iCASS_Costs))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Position_Number:" 					& scriptGenerics, 0, True, position_Number))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Position_Title:" 					& scriptGenerics, 0, True, position_Title))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Billet_ATU:" 						& scriptGenerics, 0, True, billet_ATU_NoUnit))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Billet_UII:" 						& scriptGenerics, 0, True, billet_UII))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Billet_PPA:" 						& scriptGenerics, 0, True, ppa_Option))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Billet_Object_Class:" 				& scriptGenerics, 0, True, billet_Object_Class))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#CONUS_OCONUS:" 						& scriptGenerics, 0, True, cONUS_OCONUS))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#OPFAC:" 							& scriptGenerics, 0, True, oPFAC))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Detached_Duty:" 					& scriptGenerics, 0, True, detached_Duty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Detached_Duty_Location:" 			& scriptGenerics, 0, True, detached_Duty_Location))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Background_Investigation_Type:" 	& scriptGenerics, 0, True, background_Investigation_Type))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Lease_Choice:" 						& scriptGenerics, 0, True, lease_Choice))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Lease_PPA:" 						& scriptGenerics, 0, True, lease_PPA))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Lease_ATU:" 						& scriptGenerics, 0, True, lease_ATU_NoUnit))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Furniture_Reqd:" 					& scriptGenerics, 0, True, furniture_Reqd))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Utilities_Reqd:" 					& scriptGenerics, 0, True, utilities_Reqd))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Computer_Type:" 					& scriptGenerics, 0, True, computer_Type))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#LineItem_Comment:" 					& scriptGenerics, 0, True, lineItem_Comment))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Utilities_PPA:" 					& scriptGenerics, 0, True, UTL_PPA))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Utilities_ATU:" 					& scriptGenerics, 0, True, UTL_ATU_NoUnit))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Acquisition_Project:" 				& scriptGenerics, 0, True, Acquisition_Project))	
						
					
'							'********Allocation Drivers Storage********									
'							'For those attributes that are also a dimension, we will also store a 1 in that dimension member that is selected so we can find it in a data buffer for the cost calc	
					Me.AllocationsCalc(si, globals, args, RP_Entity, RPName, wfCube, wfScenario, wfTime, LineItemNum, ppa_Option, billet_UII, billet_Object_Class, billet_ATU_NoUnit, pPE_PPA, ppe_ATU_NoUnit, UTL_PPA, UTL_ATU_NoUnit, lease_PPA, lease_ATU_NoUnit)							
								
						
					'********Headcount Reporting Storage********
					Dim hcScriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Periodic:F#" & RPName & ":O#Forms:I#None:U6#" & LineItemNum & ":U7#None:U8#None"			
					
					'set the Aviator variable
					Dim aviator As String = String.Empty
					If pilot = "Y"
						aviator = "Aviator"
					ElseIf pilot = "N"
						aviator = "NA_Aviator"
					End If
					
					'Set the military employment type variable
					Dim milEmpType As String = String.Empty
					If aD_Reserve.XFEqualsIgnoreCase("Active_Duty")
						milEmpType = aD_Reserve
					ElseIf aD_Reserve.XFEqualsIgnoreCase("Reserve")
						milEmpType = reserve_Type
					Else 
						milEmpType = "NA_Military_Employment_Type"
					End If
						
					
					'Run the Headcount Calc
					Me.HeadcountCalc(si, globals, args, RP_Entity, RPName, wfCube, wfScenario, wfTime, LineItemNum, grade_Rank, milEmpType, spe_Code_Occu_Series, cONUS_OCONUS, aviator)
					
					'Get PPE Type Description set on save --- Steve B
					Dim PPE_Typedescription As String = String.Empty
					Dim loopCounter As Integer = 0
						
					If pPE_Type.Length = 0
							PPE_Typedescription = ""
					Else
							
						Dim selectedArray() As String = pPE_Type.Replace(" ", "").Split(",")
						Dim types As List(Of String) = selectedArray.ToList()
						
						For Each ppetype As String In types
							If loopCounter = 0 Then
							
								PPE_Typedescription = BRApi.Finance.Members.GetMember(si, dimtypeid.UD8, ppetype).Description 
							
							Else
								
								PPE_Typedescription = PPE_Typedescription & ", " & BRApi.Finance.Members.GetMember(si, dimtypeid.UD8, ppetype).Description
								
							End If
							
							loopCounter+=1
						
						  Next
							
					End If
					
					'Write the annotations to the database
					Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)	
				 	'Show a message box that the Billet was successfully updated
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					SetRoutingContent(selectionChangedTaskResult.ModifiedCustomSubstVars, routingAppn, "OS_RP_OSDynamicCopy")
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_PPEType_Descr_OS", 				PPE_Typedescription)
					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					selectionChangedTaskResult.IsOK = True
					selectionChangedTaskResult.ShowMessageBox = True
					selectionChangedTaskResult.Message = "" & GetDescription(si,RPname)	 & " " & GetUD6Description(si,LineItemNum) & " Successfully Updated"
				 	Return selectionChangedTaskResult
					
'						End If ' Edit Mode
					
			Return Nothing
		End Function
		Private Function EditEXPLine() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.EditEXPLine ====

					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
					Dim RPName As String = args.NameValuePairs("RPName")
					Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
					Dim routingAppn As String = ResolveRoutingAppnForRP(RPName, args.NameValuePairs.XFGetValue("APPN_Content", "OS"))
					Dim LineItemNum As String = args.NameValuePairs("LineItemNum") 
					Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
					Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")

					If  String.IsNullOrEmpty (LineItemNum) Then 
						Throw New Exception("Please choose a Line Item") 
					End If
					RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, LineItemNum )

					Dim requested_Item_Tier1 As String = args.NameValuePairs("Requested_Item_Tier1") '|!prm_NBLT_RequestedItem_Tier1!|
					Dim requested_Item_Tier1Split As List(Of String) = StringHelper.SplitString(requested_Item_Tier1, "_")
					Dim requested_ItemNum As Integer = requested_Item_Tier1Split(0).XFConvertToInt
					Dim description_Tier2 As String = args.NameValuePairs("Description_Tier2") '|!prm_NBLT_Description_Tier2!|
					Dim description_Tier2_ToUse As String = String.Empty
					'If requested_ItemNum >=400, we need to potentially determine which base Tier2 member to use since they will be entering a custom description
					If requested_ItemNum >=400
						If description_Tier2.XFContainsIgnoreCase("_1") Or description_Tier2 = "" Then
							description_Tier2_ToUse = requested_ItemNum & "0_1"
						Else
							description_Tier2_ToUse = description_Tier2
						End If
					Else 'requested_ItemNum <400
							description_Tier2_ToUse = description_Tier2
					End If							
					Dim description_Tier2_Input As String = args.NameValuePairs("Description_Tier2_Input") '|!prm_NBLT_Description_Tier2_Input!|
					Dim description_Tier2_Input_ToUse As String = String.Empty
					'If the requested_ItemNum >=400 , they must be usign a canned member so we should grab the description from that member, If not, then use what they entered
					If requested_ItemNum < 400
						description_Tier2_Input_ToUse = BRApi.Finance.Members.GetMember(si, dimtypeid.UD5, description_Tier2).Description
					Else
						description_Tier2_Input_ToUse = description_Tier2_Input
					End If						
					Dim pOC As String = args.NameValuePairs("POC") '|!prm_NBLT_POC!|
					Dim dollarK_Value As String = args.NameValuePairs("DollarK_Value") '|!prm_NBLT_DollarKValue!|		
					Dim r_NR As String = args.NameValuePairs("R_NR") '|!prm_NBLT_RecurringNonRecurring!|
					Dim aTU As String = args.NameValuePairs("ATU") '|!prm_NBLT_ATU!|
					Dim aTU_NoUnit As String=String.Empty
					If aTU <> ""
						aTU_NoUnit = aTU & "_NoUnit" 'Add the ATU and _NoUnit together to get the base level unit to store it at
					End If
					Dim pPA As String = args.NameValuePairs("PPA") '|!prm_NBLT_PPA!|
					Dim uII As String = args.NameValuePairs("UII") '|!prm_NBLT_UII!|
					Dim object_Class As String = args.NameValuePairs("Object_Class") '|!prm_NBLT_ObjectClass!|
							
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#" & LineItemNum & ":U7#None:U8#None"			
					Dim scriptGenericsDescr As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_Input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U6#" & LineItemNum & ":U7#None:U8#None"	
					'Create a new list of memberscript and value
					Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)
					
					'In this part, we are writing the annotations to the database
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Requested_Item_Tier1:" 											& scriptGenerics, 		0, True, requested_Item_Tier1))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Description_Tier2:" 											& scriptGenerics, 		0, True, description_Tier2_ToUse))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "U5#" 							& description_Tier2_ToUse & ":" 	& scriptGenericsDescr, 	0, True, description_Tier2_Input_ToUse))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#POC:" 															& scriptGenerics, 		0, True, pOC))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#DollarK_Value:" 												& scriptGenerics, 		0, True, dollarK_Value))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#R_NR:" 															& scriptGenerics, 		0, True, r_NR))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#ATU:" 															& scriptGenerics, 		0, True, aTU_NoUnit))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#PPA:" 															& scriptGenerics, 		0, True, pPA))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#UII:" 															& scriptGenerics, 		0, True, uII))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Object_Class:" 													& scriptGenerics, 		0, True, object_Class))
							
							
							
					'********Allocation Drivers Storage********									
					'For those attributes that are also a dimension, we will also store a 1 in that dimension member that is selected so we can find it in a data buffer for the cost calc	
					Me.NBAllocationsCalc(si, args, RP_Entity, RPName, wfTime, LineItemNum, pPA, uII, object_Class, aTU_NoUnit)		
					
					'Write the annotations to the database
					Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)							
								
								
				 	'Show a message box that the RP was successfully created
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					selectionChangedTaskResult.IsOK = True
					selectionChangedTaskResult.ShowMessageBox = True
					selectionChangedTaskResult.Message = "" & GetDescription(si,RPName) & " " & LineItemNum & " Successfully Updated"
				 	Return selectionChangedTaskResult
												
			Return Nothing
		End Function
		Private Function EditNBLTLine_OS() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.EditNBLTLine_OS ====

					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
					Dim RPName As String = args.NameValuePairs("RPName")
					Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
					Dim LineItemNum As String = args.NameValuePairs("LineItemNum") 
					Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
					Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")

					Dim Content_OS As String = args.NameValuePairs.XFGetValue("Content_OS")
					
					If  String.IsNullOrEmpty (LineItemNum) Then 
						Throw New Exception("Please choose a Line Item") 
					End If
					
					If IsOSPG1Empty(globals, si, wfCube,RP_Entity,wfScenario,wfTime,RPName) Then Throw New Exception("Empty attributes in Page 1. All attributes on Page 1 must have a value to save this page.")

					RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, LineItemNum )
					
					Dim requested_Item_Tier1 As String = args.NameValuePairs("Requested_Item_Tier1") '|!prm_NBLT_RequestedItem_Tier1!|
 					If String.IsNullOrEmpty(requested_Item_Tier1) Then Throw New Exception("Please choose Requested Item - Cost Line")
					Dim requested_Item_Tier1Split As List(Of String) = StringHelper.SplitString(requested_Item_Tier1, "_")
					Dim requested_ItemNum As Integer = requested_Item_Tier1Split(0).XFConvertToInt
					Dim description_Tier2 As String = args.NameValuePairs("Description_Tier2") '|!prm_NBLT_Description_Tier2!|
					Dim description_Tier2_ToUse As String = String.Empty
					'If requested_ItemNum >=400, we need to potentially determine which base Tier2 member to use since they will be entering a custom description
					If requested_ItemNum >=400
						If description_Tier2.XFContainsIgnoreCase("_1") Or description_Tier2 = "" Then
							description_Tier2_ToUse = requested_ItemNum & "0_1"
						Else
							description_Tier2_ToUse = description_Tier2
						End If
					Else 'requested_ItemNum <400
							description_Tier2_ToUse = description_Tier2
					End If							
					Dim description_Tier2_Input As String = args.NameValuePairs("Description_Tier2_Input") '|!prm_NBLT_Description_Tier2_Input!|
					Dim description_Tier2_Input_ToUse As String = String.Empty
					'If the requested_ItemNum >=400 , they must be usign a canned member so we should grab the description from that member, If not, then use what they entered
					If requested_ItemNum < 400
						description_Tier2_Input_ToUse = BRApi.Finance.Members.GetMember(si, dimtypeid.UD5, description_Tier2).Description
					Else
						description_Tier2_Input_ToUse = description_Tier2_Input
					End If						
					Dim pOC As String = args.NameValuePairs("POC") '|!prm_NBLT_POC!|
'						Dim reference_Doc As String = args.NameValuePairs("Reference_Doc") '|!prm_NBLT_SupportingDoc!|
					Dim dollarK_Value As String = args.NameValuePairs("DollarK_Value") '|!prm_NBLT_DollarKValue!|
						'Check for symbols
					Dim r As New Regex("^[0-9]")
					If Not r.IsMatch(dollarK_Value) Then 
						dollarK_Value = Regex.Replace(dollarK_Value, "[^0-9\.\-/]", "")
						If String.IsNullOrEmpty(dollarK_Value) Then 
							Throw New Exception ("Invalid $K Value")
						End If
					End If
					
					Dim r_NR As String = args.NameValuePairs("R_NR") '|!prm_NBLT_RecurringNonRecurring!|
					Dim aTU As String = args.NameValuePairs("ATU") '|!prm_NBLT_ATU!|
					Dim aTU_NoUnit As String=String.Empty
					If aTU <> ""
						aTU_NoUnit = aTU & "_NoUnit" 'Add the ATU and _NoUnit together to get the base level unit to store it at
					End If
					Dim pPA As String = args.NameValuePairs("PPA") '|!prm_NBLT_PPA_OS!|

					Dim uII As String = args.NameValuePairs("UII") '|!prm_NBLT_UII!|
					Dim object_Class As String = args.NameValuePairs("Object_Class") '|!prm_NBLT_ObjectClass!|


					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#" & LineItemNum & ":U7#None:U8#None"		
					Dim scriptGenericsDescr As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_Input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U6#" & LineItemNum & ":U7#None:U8#None"	
					'Create a new list of memberscript and value
					Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)
					
					'*********Attribute Annotation Storage********
					'Add the member scripts to the list and store as 0 No data annotations
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Requested_Item_Tier1:" 		& scriptGenerics, 										0, True, requested_Item_Tier1))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Description_Tier2:" 		& scriptGenerics, 										0, True, description_Tier2_ToUse))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "U5#" 							& description_Tier2_ToUse & ":" & scriptGenericsDescr, 	0, True, description_Tier2_Input_ToUse))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#POC:" 						& scriptGenerics, 										0, True, pOC))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#DollarK_Value:" 			& scriptGenerics, 										0, True, dollarK_Value))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#R_NR:" 						& scriptGenerics, 										0, True, r_NR))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#ATU:" 						& scriptGenerics, 										0, True, aTU_NoUnit))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#PPA:" 						& scriptGenerics, 										0, True, pPA))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#UII:" 						& scriptGenerics, 										0, True, uII))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Object_Class:" 				& scriptGenerics, 										0, True, object_Class))
											
					'********Allocation Drivers Storage********									
					'For those attributes that are also a dimension, we will also store a 1 in that dimension member that is selected so we can find it in a data buffer for the cost calc	
					Me.NBAllocationsCalc(si, args, RP_Entity, RPName, wfTime, LineItemNum, pPA, uII, object_Class, aTU_NoUnit)		
					
					'Write the annotations to the database
					Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)							
								
								
				 	'Show a message box that the RP was successfully created
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					selectionChangedTaskResult.IsOK = True
					selectionChangedTaskResult.ShowMessageBox = True
					SetRoutingContent(selectionChangedTaskResult.ModifiedCustomSubstVars, routingAppn, Content_OS)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_LineItemNumber_OS_Copy", LineItemNum)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_LineItemNumber_OS", LineItemNum)
					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True	
					selectionChangedTaskResult.Message = "" & GetDescription(si,RPName) & " " & GetUD6Description(si,LineItemNum) & " Successfully Updated"
				 	Return selectionChangedTaskResult
							
			Return Nothing
		End Function
		Private Function EditRP_Page1() As Object
			' No un-suffixed EditRP_Page1 exists in the legacy rule -- only the
			' appropriation-suffixed family (handled above). Dead dispatch entry
			' kept for safety; remove once confirmed nothing sends the bare name.
			Return Nothing
		End Function
		Private Function EditRP_Page2() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.EditRP_Page2 ====
			Dim wfTime As String = args.NameValuePairs("WFTime")
			Dim wfScenario As String = args.NameValuePairs("WFScenario")
			Dim wfCube As String = args.NameValuePairs("WFCube")
			Dim RPName As String = args.NameValuePairs("RPName")
			Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
			Dim routingAppn As String = ResolveRoutingAppnForRP(RPName, args.NameValuePairs.XFGetValue("APPN_Content", "OS"))
			Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
			Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")

			RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, "")

			Dim fY_Related_RP1 As String = args.NameValuePairs("FY_Related_RP1")
			Dim fY_Related_RP2 As String = args.NameValuePairs("FY_Related_RP2")
			Dim fY_Related_RP3 As String = args.NameValuePairs("FY_Related_RP3")
			Dim older_Related_RP1 As String = args.NameValuePairs("Older_Related_RP1")
			Dim older_Related_RP2 As String = args.NameValuePairs("Older_Related_RP2")
			Dim older_Related_RP3 As String = args.NameValuePairs("Older_Related_RP3")
			Dim lead_Office1 As String = args.NameValuePairs("Lead_Office1")
			Dim lead_Office2 As String = args.NameValuePairs("Lead_Office2")
			Dim lead_Office3 As String = args.NameValuePairs("Lead_Office3")
			Dim lead_Office_POC1 As String = args.NameValuePairs("Lead_Office_POC1")
			Dim lead_Office_POC2 As String = args.NameValuePairs("Lead_Office_POC2")
			Dim lead_Office_POC3 As String = args.NameValuePairs("Lead_Office_POC3")
			Dim lead_Office_Phone1 As String = args.NameValuePairs("Lead_Office_Phone1")
			Dim lead_Office_Phone2 As String = args.NameValuePairs("Lead_Office_Phone2")
			Dim lead_Office_Phone3 As String = args.NameValuePairs("Lead_Office_Phone3")
			Dim InitialE As String = args.NameValuePairs("InitialE")
			Dim baseFunding As String = args.NameValuePairs("BaseFunding")
			Dim baseFundingComment As String = args.NameValuePairs("BaseFundingComment")
			Dim exec_Summary As String = args.NameValuePairs("Exec_Summary")
							
			Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"						
			
			'Create a new list of memberscript and value
			Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)
			
			'Add the member scripts to the list and store as 0 No data annotations
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#FY_Related_RP1:" 			& scriptGenerics, 0, True, fY_Related_RP1))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#FY_Related_RP2:" 			& scriptGenerics, 0, True, fY_Related_RP2))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#FY_Related_RP3:" 			& scriptGenerics, 0, True, fY_Related_RP3))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Older_Related_RP1:" 		& scriptGenerics, 0, True, older_Related_RP1))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Older_Related_RP2:" 		& scriptGenerics, 0, True, older_Related_RP2))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Older_Related_RP3:" 		& scriptGenerics, 0, True, older_Related_RP3))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Lead_Office1:" 				& scriptGenerics, 0, True, lead_Office1))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Lead_Office2:" 				& scriptGenerics, 0, True, lead_Office2))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Lead_Office3:" 				& scriptGenerics, 0, True, lead_Office3))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Lead_Office_POC1:" 			& scriptGenerics, 0, True, lead_Office_POC1))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Lead_Office_POC2:" 			& scriptGenerics, 0, True, lead_Office_POC2))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Lead_Office_POC3:" 			& scriptGenerics, 0, True, lead_Office_POC3))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Lead_Office_Phone1:" 		& scriptGenerics, 0, True, lead_Office_Phone1))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Lead_Office_Phone2:" 		& scriptGenerics, 0, True, lead_Office_Phone2))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Lead_Office_Phone3:" 		& scriptGenerics, 0, True, lead_Office_Phone3))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Exec_Summary:" 				& scriptGenerics, 0, True, exec_Summary))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Initial_Estimate:" 			& scriptGenerics, 0, True, InitialE))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Base_Funding:" 				& scriptGenerics, 0, True, baseFunding))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Base_Funding_Comments:" 	& scriptGenerics, 0, True, baseFundingComment))
								
			Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)
			
		 	'Show a message box that the RP was successfully updated
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
			selectionChangedTaskResult.IsOK = True
			selectionChangedTaskResult.ShowMessageBox = True
			selectionChangedTaskResult.Message = "" & GetDescription(si,RPName) & " Successfully Updated"
		 	Return selectionChangedTaskResult	
			
'						End If 'Edit Mode						
					
			Return Nothing
		End Function
		Private Function EditRP_Page2_OS() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.EditRP_Page2_OS ====
					
				Dim wfTime As String = args.NameValuePairs("WFTime")
				Dim wfScenario As String = args.NameValuePairs("WFScenario")
				Dim wfCube As String = args.NameValuePairs("WFCube")
				Dim RPName As String = args.NameValuePairs("RPName")
				Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)	

				Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
				Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")
				
				Dim Content_OS As String = args.NameValuePairs("Content_OS")
			    Dim Content_EditRP_OS As String = args.NameValuePairs("Content_EditRP_OS")
				

				RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, "")

				Dim fY_Related_RP1 As String = args.NameValuePairs("FY_Related_RP1")
				Dim fY_Related_RP2 As String = args.NameValuePairs("FY_Related_RP2")
				Dim fY_Related_RP3 As String = args.NameValuePairs("FY_Related_RP3")
				Dim older_Related_RP1 As String = args.NameValuePairs("Older_Related_RP1")
				Dim older_Related_RP2 As String = args.NameValuePairs("Older_Related_RP2")
				Dim older_Related_RP3 As String = args.NameValuePairs("Older_Related_RP3")
				Dim lead_Office1 As String = args.NameValuePairs("Lead_Office1")
				Dim lead_Office2 As String = args.NameValuePairs("Lead_Office2")
				Dim lead_Office3 As String = args.NameValuePairs("Lead_Office3")
				Dim lead_Office_POC1 As String = args.NameValuePairs("Lead_Office_POC1")
				Dim lead_Office_POC2 As String = args.NameValuePairs("Lead_Office_POC2")
				Dim lead_Office_POC3 As String = args.NameValuePairs("Lead_Office_POC3")
				Dim lead_Office_Phone1 As String = args.NameValuePairs("Lead_Office_Phone1")
				Dim lead_Office_Phone2 As String = args.NameValuePairs("Lead_Office_Phone2")
				Dim lead_Office_Phone3 As String = args.NameValuePairs("Lead_Office_Phone3")
				Dim exec_Summary As String = args.NameValuePairs("Exec_Summary")
				Dim InitialE As String = args.NameValuePairs("InitialE")
				Dim InitialEMIL As String = args.NameValuePairs("InitialEMIL")
				Dim InitialECIV As String = args.NameValuePairs("InitialECIV")
				Dim baseFunding As String = args.NameValuePairs("BaseFunding")
				Dim baseFundingComment As String = args.NameValuePairs("BaseFundingComment")
				Dim baseFundingMIL As  String = args.NameValuePairs("BaseFundingMIL")
				Dim baseFundingCIV As  String = args.NameValuePairs("BaseFundingCIV")
					
				Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"						
				Dim ConcReviewScriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Comment_01"

				
				'Create a new list of memberscript and value
				Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)
				
				'if lead office exists, add both annotation and periodic values (office text and data=1) to memberscriptandvalue for StaffSymbol_ConcReview and Comment_01
				If Not lead_Office1.Length = 0 Then
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#StaffSymbol_ConcReview:" & ConcReviewScriptGenerics & ":V#Annotation", 0, True, lead_Office1))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#StaffSymbol_ConcReview:" & ConcReviewScriptGenerics & ":V#Periodic", 1, False, lead_Office1))
				End If
				
				'Add the member scripts to the list and store as 0 No data annotations
				lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#FY_Related_RP1:" 			& scriptGenerics, 0, True, fY_Related_RP1))
				lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#FY_Related_RP2:"			& scriptGenerics, 0, True, fY_Related_RP2))
				lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#FY_Related_RP3:" 			& scriptGenerics, 0, True, fY_Related_RP3))
				lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Older_Related_RP1:" 		& scriptGenerics, 0, True, older_Related_RP1))
				lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Older_Related_RP2:" 		& scriptGenerics, 0, True, older_Related_RP2))
				lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Older_Related_RP3:" 		& scriptGenerics, 0, True, older_Related_RP3))
				lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Lead_Office1:" 				& scriptGenerics, 0, True, lead_Office1))
				lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Lead_Office2:" 				& scriptGenerics, 0, True, lead_Office2))
				lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Lead_Office3:" 				& scriptGenerics, 0, True, lead_Office3))
				lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Lead_Office_POC1:" 			& scriptGenerics, 0, True, lead_Office_POC1))
				lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Lead_Office_POC2:" 			& scriptGenerics, 0, True, lead_Office_POC2))
				lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Lead_Office_POC3:" 			& scriptGenerics, 0, True, lead_Office_POC3))
				lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Lead_Office_Phone1:" 		& scriptGenerics, 0, True, lead_Office_Phone1))
				lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Lead_Office_Phone2:"		& scriptGenerics, 0, True, lead_Office_Phone2))
				lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Lead_Office_Phone3:" 		& scriptGenerics, 0, True, lead_Office_Phone3))							
				lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Exec_Summary:" 				& scriptGenerics, 0, True, exec_Summary))
				lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Initial_Estimate:" 			& scriptGenerics, 0, True, InitialE))
				lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Initial_Estimate_MIL_FTP:" 	& scriptGenerics, 0, True, InitialEMIL))
				lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Initial_Estimate_CIV_FTP:" 	& scriptGenerics, 0, True, InitialECIV))
				lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Base_Funding:" 				& scriptGenerics, 0, True, baseFunding))
				lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Base_Funding_Comments:" 	& scriptGenerics, 0, True, baseFundingComment))
				lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Base_Funding_MIL_FTP:" 		& scriptGenerics, 0, True, baseFundingMIL))
				lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Base_Funding_CIV_FTP:" 		& scriptGenerics, 0, True, baseFundingCIV))
				
				
				'Write the annotations to the database
				Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)
				
				RunPostSaveStepsForRP(globals, si, wfcube, RP_Entity, wfscenario, wftime, RPName)
				
			 	'Show a message box that the RP was successfully updated
				Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
				SetRoutingPageCompat(selectionChangedTaskResult.ModifiedCustomSubstVars, routingAppn, Content_EditRP_OS)
				SetRoutingContent(selectionChangedTaskResult.ModifiedCustomSubstVars, routingAppn, Content_OS)
				selectionChangedTaskResult.IsOK = True
				selectionChangedTaskResult.ShowMessageBox = True
				selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
				selectionChangedTaskResult.Message = "" & GetDescription(si,RPName) & " Successfully Updated"
			 	Return selectionChangedTaskResult	
				
			
			Return Nothing
		End Function
		Private Function EditRP_Page3() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.EditRP_Page3 ====
			Dim wfTime As String = args.NameValuePairs("WFTime")
			Dim wfScenario As String = args.NameValuePairs("WFScenario")
			Dim wfCube As String = args.NameValuePairs("WFCube")
			Dim RPName As String = args.NameValuePairs("RPName")
			Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
			Dim routingAppn As String = ResolveRoutingAppnForRP(RPName, args.NameValuePairs.XFGetValue("APPN_Content", "OS"))
			Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
			Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")
			
			Dim Content_OS As String = args.NameValuePairs("Content_OS")
			Dim Content_EditRP_OS As String = args.NameValuePairs("Content_EditRP_OS")

			RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, "")

				
			Dim affect_Others As String = args.NameValuePairs("AffectOthers")
			Dim alignment As String = args.NameValuePairs("Alignment")
			Dim denial_Impact As String = args.NameValuePairs("DenialImpact")
			Dim funding_Impact As String = args.NameValuePairs("FundingImpact")
			Dim problem As String = args.NameValuePairs("Problem")
			Dim rOI As String = args.NameValuePairs("ROI")
						
			Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"						
			
			'Create a new list of memberscript and value
			Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)
			
			'Add the member scripts to the list and store as 0 No data annotations
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Affect_Others:" 	& scriptGenerics, 0, True, affect_Others))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Alignment:" 		& scriptGenerics, 0, True, alignment))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Denial_Impact:" 	& scriptGenerics, 0, True, denial_Impact))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Funding_Impact:" 	& scriptGenerics, 0, True, funding_Impact))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Problem:" 			& scriptGenerics, 0, True, problem))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#ROI:" 				& scriptGenerics, 0, True, rOI))
			
	
			
			'Write the annotations to the database
			Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)
			
			RunPostSaveStepsForRP(globals, si, wfcube, RP_Entity, wfscenario, wftime, RPName)
			
		 	'Show a message box that the RP was successfully updated
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
			SetRoutingPageCompat(selectionChangedTaskResult.ModifiedCustomSubstVars, routingAppn, Content_EditRP_OS)
			SetRoutingContent(selectionChangedTaskResult.ModifiedCustomSubstVars, routingAppn, Content_OS)
			selectionChangedTaskResult.IsOK = True
			selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
			selectionChangedTaskResult.ShowMessageBox = True
			selectionChangedTaskResult.Message = "" & GetDescription(si,RPName) & " Successfully Updated"
		 	Return selectionChangedTaskResult	
			
					
			Return Nothing
		End Function
		Private Function EditRP_Page3_ConstrWords_PCI() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.EditRP_Page3_ConstrWords_PCI ====
			Dim wfTime As String = args.NameValuePairs("WFTime")
			Dim wfScenario As String = args.NameValuePairs("WFScenario")
			Dim wfCube As String = args.NameValuePairs("WFCube")
			Dim RPName As String = args.NameValuePairs("RPName")
			Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
			Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
			Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")

			RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, "")

			Dim invest_Desc As String = args.NameValuePairs("Invest_Desc")
			Dim justification As String = args.NameValuePairs("Justification")
			Dim project_Number As String = args.NameValuePairs("Project_Number")
			Dim project_FundReq As String = args.NameValuePairs("Project_FundReq")
			Dim project_Desc As String = args.NameValuePairs("Project_Desc")
			Dim project_Justification As String = args.NameValuePairs("Project_Justification")
			Dim project_Impact As String = args.NameValuePairs("Project_Impact")
			Dim project_ContrSolic As String = args.NameValuePairs("Project_ContrSolic")
			Dim project_DBConstrAward As String = args.NameValuePairs("Project_DBConstrAward")
			Dim project_ConstrStart As String = args.NameValuePairs("Project_ConstrStart")
			Dim project_ConstrComplete As String = args.NameValuePairs("Project_ConstrComplete")

			Dim scriptGenerics As String = "Cb#" & wfCube & ":E#" & RP_Entity & ":C#Local:S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"						
			Dim scriptGenericsWUD8 As String = "Cb#" & wfCube & ":E#" & RP_Entity & ":C#Local:S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#" & project_Number
			
			'Create a new list of memberscript and value
			Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)
			
			'Edit attribute accounts and Add it to the list
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Invest_Desc_PCI:" & 			scriptGenerics, 	0, True, invest_Desc))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Justification_PCI:" & 			scriptGenerics, 	0, True, justification))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Project_FundReq_PCI:" & 		scriptGenericsWUD8, 0, True, project_FundReq))								
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Project_Desc_PCI:" & 			scriptGenericsWUD8, 0, True, project_Desc))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Project_Justification_PCI:" & 	scriptGenericsWUD8, 0, True, project_Justification))								
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Project_Impact_PCI:" & 			scriptGenericsWUD8, 0, True, project_Impact))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Project_ContrSolic_PCI:" & 		scriptGenericsWUD8, 0, True, project_ContrSolic))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Project_DBConstrAward_PCI:" & 	scriptGenericsWUD8, 0, True, project_DBConstrAward))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Project_ConstrStart_PCI:" & 	scriptGenericsWUD8, 0, True, project_ConstrStart))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Project_ConstrComplete_PCI:" & 	scriptGenericsWUD8, 0, True, project_ConstrComplete))
											
			
			'Write the annotations to the database
			Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)
			
		 	'Show a message box that the RP was successfully updated
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
			selectionChangedTaskResult.IsOK = True
			selectionChangedTaskResult.ShowMessageBox = True
			selectionChangedTaskResult.Message = "" & GetDescription(si,RPName) & " Successfully Updated"
		 	Return selectionChangedTaskResult	
									
			Return Nothing
		End Function
		Private Function EditRP_Page3_EndItemsWords_PCI() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.EditRP_Page3_EndItemsWords_PCI ====
		Dim wfTime As String = args.NameValuePairs("WFTime")
		Dim wfScenario As String = args.NameValuePairs("WFScenario")
		Dim wfCube As String = args.NameValuePairs("WFCube")
		Dim RPName As String = args.NameValuePairs("RPName")
		Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
		Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
		Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")

		RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, "")
		Dim invest_Desc As String = args.NameValuePairs("Invest_Desc")
		Dim scriptGenerics As String = "Cb#" & wfCube & ":E#" & RP_Entity & ":C#Local:S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"						
			
		'Create a new list of memberscript and value
		Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)
		
		'Edit attribute accounts and Add it to the list
		lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Invest_Desc_PCI:" & scriptGenerics, 0, True, invest_Desc))
							
		'Write the annotations to the database
		Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)
		
	 	'Show a message box that the RP was successfully updated
		Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
		selectionChangedTaskResult.IsOK = True
		selectionChangedTaskResult.ShowMessageBox = True
		selectionChangedTaskResult.Message = "" & GetDescription(si,RPName) & " Successfully Updated"
	 	Return selectionChangedTaskResult	
		
			Return Nothing
		End Function
		Private Function EditRP_Page3_ProqAcqWords_PCI() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.EditRP_Page3_ProqAcqWords_PCI ====
			Dim wfTime As String = args.NameValuePairs("WFTime")
			Dim wfScenario As String = args.NameValuePairs("WFScenario")
			Dim wfCube As String = args.NameValuePairs("WFCube")
			Dim RPName As String = args.NameValuePairs("RPName")
			Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
			Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
			Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")

			RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, "")
			Dim invest_Desc As String = args.NameValuePairs("Invest_Desc")
			Dim justification As String = args.NameValuePairs("Justification")
			Dim keyMilestones_PY As String = args.NameValuePairs("KeyMilestones_PY")
			Dim keyMilestones_CY As String = args.NameValuePairs("KeyMilestones_CY")
			Dim keyMilestones_BY As String = args.NameValuePairs("KeyMilestones_BY")
			Dim sig_Changes As String = args.NameValuePairs("SigChanges")
			
			Dim scriptGenerics As String = "Cb#" & wfCube & ":E#" & RP_Entity & ":C#Local:S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"						
			
			'Create a new list of memberscript and value
			Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)
			
			'Edit attribute accounts and Add it to the list
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Invest_Desc_PCI:" & 		scriptGenerics, 0, True, invest_Desc))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Justification_PCI:" & 		scriptGenerics, 0, True, justification))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#KeyMilestones_PY_PCI:" & 	scriptGenerics, 0, True, keyMilestones_PY))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#KeyMilestones_CY_PCI:" & 	scriptGenerics, 0, True, keyMilestones_CY))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#KeyMilestones_BY_PCI:" & 	scriptGenerics, 0, True, keyMilestones_BY))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#SignificantChanges_PCI:" & 	scriptGenerics, 0, True, sig_Changes))							
							
					
			'Write the annotations to the database
			Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)
			
		 	'Show a message box that the RP was successfully updated
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
			selectionChangedTaskResult.IsOK = True
			selectionChangedTaskResult.ShowMessageBox = True
			selectionChangedTaskResult.Message = "" & GetDescription(si,RPName) & " Successfully Updated"
		 	Return selectionChangedTaskResult	
			
			
			Return Nothing
		End Function
		Private Function EditRP_Page3_RD() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.EditRP_Page3_RD ====
			Dim wfTime As String = args.NameValuePairs("WFTime")
			Dim wfScenario As String = args.NameValuePairs("WFScenario")
			Dim wfCube As String = args.NameValuePairs("WFCube")
			Dim RPName As String = args.NameValuePairs("RPName")
			Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
			Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
			Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")

			RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, "")

								
			Dim scriptGenerics As String = 	"E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & 
											":V#Annotation:F#" & RPName & 
											":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"						
			
			'Create a new list of memberscript and value
			Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)

			' Create a new MemberScriptAndValue for each parameter and add to the list
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube,"A#Project_Name:" 			& scriptGenerics, 0, True, args.NameValuePairs("Project_Name")))						
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube,"A#20PY:" 					& scriptGenerics, 0, True, args.NameValuePairs("20PY")))						
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube,"A#20CY:" 					& scriptGenerics, 0, True, args.NameValuePairs("20CY")))						
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube,"A#20BY:" 					& scriptGenerics, 0, True, args.NameValuePairs("20BY")))						
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube,"A#Project_Description:" 	& scriptGenerics, 0, True, args.NameValuePairs("Project_Description")))						
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube,"A#Problem:" 				& scriptGenerics, 0, True, args.NameValuePairs("Problem")))						
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube,"A#Justification:" 			& scriptGenerics, 0, True, args.NameValuePairs("Justification")))						
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube,"A#Solution:" 				& scriptGenerics, 0, True, args.NameValuePairs("Solution")))						
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube,"A#Impact_On_Performance:" 	& scriptGenerics, 0, True, args.NameValuePairs("Impact_On_Performance")))						
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube,"A#Type_Of_Research:" 		& scriptGenerics, 0, True, args.NameValuePairs("Type_Of_Research")))						
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube,"A#Tech_Readiness_Level:" 	& scriptGenerics, 0, True, args.NameValuePairs("Tech_Readiness_Level")))						
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube,"A#Transition_PLans:" 		& scriptGenerics, 0, True, args.NameValuePairs("Transition_Plans")))						
								
			'Write the annotations to the database
			Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)
			
		 	'Show a message box that the RP was successfully updated
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
			selectionChangedTaskResult.IsOK = True
			selectionChangedTaskResult.ShowMessageBox = True
			selectionChangedTaskResult.Message = "" & GetDescription(si,RPName) & " Successfully Updated"
		 	Return selectionChangedTaskResult	
	
			Return Nothing
		End Function
		Private Function EditScenarioSecurity() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.EditScenarioSecurity ====
						
						'Get Time from current Workflow
						Dim wfYear As String = args.NameValuePairs("WFYear")
						Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
						
						'BY-2
						'Get variables selected by the user for the BYMinusTwo scenario
						Dim selectedReadGroupBYMinusTwo = args.NameValuePairs("SelectedReadGroupBYMinusTwo")
						'brapi.ErrorLog.LogMessage(si, selectedReadGroupBYMinusTwo & " SelectedReadGroupBYMinusTwo")
						Dim selectedWriteGroupBYMinusTwo = args.NameValuePairs("SelectedWriteGroupBYMinusTwo")
						'brapi.ErrorLog.LogMessage(si, selectedWriteGroupBYMinusTwo & " SelectedWriteGroupBYMinusTwo")
						
						'Get the variables for the BYMinusTwo scenario
						Dim wfYearMinusTwo As String = (wfYear.XFConvertToInt - 2).ToString.Substring(2)
						Dim workScenBYMinusTwo As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "WorkScen_FY" & wfYearMinusTwo)
						
						'Run the Helper Function to update the BYMinusTwoScenario
						Me.EditScenarioSecurityHelper(globals, si, selectedReadGroupBYMinusTwo, selectedWriteGroupBYMinusTwo, workScenBYMinusTwo)
						
						'Clear combo box
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_ScenarioReadGroupBYMinus2_ADM", String.Empty)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_ScenarioWriteGroupBYMinus2_ADM", String.Empty)
						
											
						'BY-1
						'Get the variables selected by the user for the BYMinusOne scenario
						Dim selectedReadGroupBYMinusOne = args.NameValuePairs("SelectedReadGroupBYMinusOne")
						Dim selectedWriteGroupBYMinusOne = args.NameValuePairs("SelectedWriteGroupBYMinusOne")
						
						'Get the variables for the BYMinusOne scenario
						Dim wfYearMinusOne As String = (wfYear.XFConvertToInt - 1).ToString.Substring(2)
						Dim workScenBYMinusOne As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "WorkScen_FY" & wfYearMinusOne)
						
						'Run the Helper Function to update the BYMinusOneScenario
						Me.EditScenarioSecurityHelper(globals, si, selectedReadGroupBYMinusOne, selectedWriteGroupBYMinusOne, workScenBYMinusOne)
						
						'Clear combo box
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_ScenarioReadGroupBYMinus1_ADM", String.Empty)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_ScenarioWriteGroupBYMinus1_ADM", String.Empty)
						
						'BY
						'Get the variables selected by the user for the BY scenario
						Dim selectedReadGroupBY = args.NameValuePairs("SelectedReadGroupBY")
						Dim selectedWriteGroupBY = args.NameValuePairs("SelectedWriteGroupBY")
						
						'Get the variables for the BYMinusOne scenario
						Dim wfYearBY As String = (wfYear.XFConvertToInt).ToString.Substring(2)
						Dim workScenBY As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "WorkScen_FY" & wfYearBY)
						
						'Run the Helper Function to update the BYMinusTwoScenario
						Me.EditScenarioSecurityHelper(globals, si, selectedReadGroupBY, selectedWriteGroupBY, workScenBY)
						
						'Clear combo box
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_ScenarioReadGroupBY_ADM", String.Empty)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_ScenarioWriteGroupBY_ADM", String.Empty)
												
						'BY+1
						'Get the variables selected by the user for the BYPlusOne scenario
						Dim selectedReadGroupBYPlusOne = args.NameValuePairs("SelectedReadGroupBYPlusOne")
						Dim selectedWriteGroupBYPlusOne = args.NameValuePairs("SelectedWriteGroupBYPlusOne")
						
						'Get the variables for the BYPlusOne scenario
						Dim wfYearPlusOne As String = (wfYear.XFConvertToInt + 1).ToString.Substring(2)
						Dim workScenBYPlusOne As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "WorkScen_FY" & wfYearPlusOne)
						
						'Run the Helper Function to update the BYPlusOne scenario
						Me.EditScenarioSecurityHelper(globals, si, selectedReadGroupBYPlusOne, selectedWriteGroupBYPlusOne, workScenBYPlusOne)
 
						'Clear combo box
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_ScenarioReadGroupBYPlus1_ADM", String.Empty)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_ScenarioWriteGroupBYPlus1_ADM", String.Empty)
						
						
						'BY+2
						'Get the variables selected by the user for the BYPlusTwo scenario
						Dim selectedReadGroupBYPlusTwo = args.NameValuePairs("SelectedReadGroupBYPlusTwo")
						Dim selectedWriteGroupBYPlusTwo = args.NameValuePairs("SelectedWriteGroupBYPlusTwo")
						
						'Get the variables for the BYPlusOne scenario
						Dim wfYearPlusTwo As String = (wfYear.XFConvertToInt + 2).ToString.Substring(2)
						Dim workScenBYPlusTwo As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "WorkScen_FY" & wfYearPlusTwo)
						
						'Run the Helper Function to update the BYPlusOne scenario
						Me.EditScenarioSecurityHelper(globals, si, selectedReadGroupBYPlusTwo, selectedWriteGroupBYPlusTwo, workScenBYPlusTwo)
						
						'Clear combo box
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_ScenarioReadGroupBYPlus2_ADM", String.Empty)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_ScenarioWriteGroupBYPlus2_ADM", String.Empty)
						
					
						'Show a message box that Scenario Security was successfully updated
						selectionChangedTaskResult.IsOK = True
						selectionChangedTaskResult.ShowMessageBox = True
						selectionChangedTaskResult.Message = "Scenario security was updated successfully."
						selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
						Return selectionChangedTaskResult
						
			Return Nothing
		End Function
		Private Function GetFirstScenario() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.GetFirstScenario ====
    
			'Dim MemberFilterScript As String = "F#Total_RPs.Base"
		Dim wfCurrentYear As String = args.NameValuePairs("WFTime")
		Dim wfCurrentYearTwoDigit As Integer = wfCurrentYear.XFConvertToInt - 2000
		Dim wfPriorYearTwoDigit As Integer = wfCurrentYearTwoDigit - 1
		Dim wfScenario As String = args.NameValuePairs("WFScenario")
		Dim ScenarioMemberFilterScript As String  = ""
		
		'If prior to 2026, customing the filters to allow flexibility for historical data loads
		If wfCurrentYear.XFConvertToInt <2024 Then
			If wfScenario.XFContainsIgnoreCase("Enacted_FY") Then
				ScenarioMemberFilterScript = "S#Enacted_FY" & wfPriorYearTwoDigit
			Else
				ScenarioMemberFilterScript = " "
			End If
		Else If wfCurrentYear.XFConvertToInt =2024 Then
			If (wfScenario.XFContainsIgnoreCase("RAP_FY") Or wfScenario.XFContainsIgnoreCase("OMBJ_FY"))Then
				ScenarioMemberFilterScript = " "
			Else If wfScenario.XFContainsIgnoreCase("CJ_FY") Then
				ScenarioMemberFilterScript = "S#Enacted_FY" & wfPriorYearTwoDigit
			Else If wfScenario.XFContainsIgnoreCase("Enacted_FY") Then
				ScenarioMemberFilterScript = "S#Enacted_FY" & wfPriorYearTwoDigit & ", S#CJ_FY" & wfCurrentYearTwoDigit
			End If
		Else
			'2025 or after so restrict the scenarios that can be rolled forward depending on the WF Scenario
			If wfScenario.XFContainsIgnoreCase("RAP_") Then
				' This is the case of rolling forward from prior year
				'  Valid options for for source scenarios are prior year's CJ_FY<PriorYear> and Enacted_<PriorYear>
				ScenarioMemberFilterScript = "S#Enacted_FY" & wfPriorYearTwoDigit & ", S#CJ_FY" & wfPriorYearTwoDigit & ", S#OMBJ_FY" & wfPriorYearTwoDigit & ", S#RAP_FY" & wfPriorYearTwoDigit
			Else If wfScenario.XFContainsIgnoreCase("OMBJ_")
				' This is the case of rolling forward from within current budget year
				'  Valid options for for source scenarios are current budget year's RAP_<CurrentYear>
				ScenarioMemberFilterScript = "S#RAP_FY" & wfCurrentYearTwoDigit & ",S#Enacted_FY" & wfPriorYearTwoDigit & ", S#CJ_FY" & wfPriorYearTwoDigit & ", S#OMBJ_FY" & wfPriorYearTwoDigit
				
			Else If wfScenario.XFContainsIgnoreCase("CJ_")
				' This is the case of rolling forward from within current budget year
				'  Valid options for for source scenarios are cuurent budget year's OMBJ_<CurrentYear>
				ScenarioMemberFilterScript = "S#OMBJ_FY" & wfCurrentYearTwoDigit & ",S#Enacted_FY" & wfPriorYearTwoDigit & ", S#CJ_FY" & wfPriorYearTwoDigit
				
			Else If wfScenario.XFContainsIgnoreCase("Enacted_")
				' This is the case of rolling forward from within current budget year
				'  Valid options for for source scenarios are cuurent budget year's CJ_<CurrentYear>
				ScenarioMemberFilterScript = "S#CJ_FY" & wfCurrentYearTwoDigit & ",S#Enacted_FY" & wfPriorYearTwoDigit
			End If
			
		End If
		
		Dim textSplit As List(Of String) = StringHelper.SplitString(ScenarioMemberFilterScript,",")
		     'Brapi.ErrorLog.LogMessage(si,textSplit(0).Substring(2))
		     'Return textSplit(0).Substring(2)
		
		
		

			
		
			
			Dim selectionChangeTaskResult As New XFSelectionChangedTaskResult
			selectionChangeTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Rollforward_BaseAndAnnTerm_Source_ADM",textSplit(0).Substring(2))											

			selectionChangeTaskResult.IsOK = True
			selectionChangeTaskResult.ShowMessageBox = False
			selectionChangeTaskResult.ChangeCustomSubstVarsInDashboard = True
		'	selectionChangeTaskResult.ModifiedCustomSubstVars = NameValuePairs
			Return selectionChangeTaskResult
					
		
								
		     
	     
			Return Nothing
		End Function
		Private Function LoadWFDashboard() As Object
			' ==== ported from BudFm_SolutionHelper_UX (LoadDashboard arm): on entry to the
			' published WF dashboard, restore the remembered RP from session state and
			' route the content vars; otherwise land on the default view. ====
			Dim content As String = args.NameValuePairs.XFGetValue("Content", String.Empty)
			Dim wfScenario As String = args.NameValuePairs.XFGetValue("WFScenario", String.Empty)
			Dim wfTime As String = args.NameValuePairs.XFGetValue("WFTime", String.Empty)
			Dim appn_content As String = args.NameValuePairs.XFGetValue("APPN_Content", String.Empty)
			If content = "|!prm_Content_OS!|" Then
				content = "OS_RP_CreateRP"
			End If

			If args.LoadDashboardTaskInfo.Reason = LoadDashboardReasonType.Initialize Then
				BRApi.State.SetSessionState(si, False, ClientModuleType.Unknown, "", "", "dashState", "dashState", String.Empty, si.XfBytes)

				If args.LoadDashboardTaskInfo.Action = LoadDashboardActionType.BeforeFirstGetParameters Then
					Dim userState As XFUserState = BRApi.State.GetSessionState(si, False, ClientModuleType.Unknown, String.Empty, String.Empty, String.Empty, String.Empty)
					Dim loadDashboardTaskResult As New XFLoadDashboardTaskResult()
					loadDashboardTaskResult.ChangeCustomSubstVarsInDashboard = True

					If (userState IsNot Nothing) AndAlso (Not String.IsNullOrEmpty(userState.TextValue)) Then
						Dim flowNavLink As String = userState.TextValue
						Try
							Dim RPAppr As String = rpUtils.Get_RP_Appropriation(si, flowNavLink)
						Catch ex As Exception
							loadDashboardTaskResult.ChangeCustomSubstVarsInDashboard = False
							Return loadDashboardTaskResult
						End Try
						BUDFM_AttributeSupport.SetRPContentRoutingVars(si, globals, loadDashboardTaskResult.ModifiedCustomSubstVars, "ReadOnly", content, String.Empty, appn_content, flowNavLink, String.Empty, wfScenario, wfTime)
						Return loadDashboardTaskResult
					Else
						' No remembered RP -- default landing view, routing vars cleared
						BUDFM_AttributeSupport.SetRPContentRoutingVars(si, globals, loadDashboardTaskResult.ModifiedCustomSubstVars, "ReadOnly", content, String.Empty, appn_content, String.Empty, String.Empty, wfScenario, wfTime)
						Return loadDashboardTaskResult
					End If
				End If
			End If
			Return Nothing
		End Function
		Private Function ModComments() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.ModComments ====
			
			Dim wfTime As String = args.NameValuePairs("WFTime")
			Dim wfScenario As String = args.NameValuePairs("WFScenario")
			Dim wfCube As String = args.NameValuePairs("WFCube")
			Dim ModName As String = args.NameValuePairs("ModName")
	
			Dim DHS_Commentary As String = args.NameValuePairs("DHS_Commentary")
									
			Dim scriptGenerics As String = "S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & ModName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			
			'Create a new list of memberscript and value
			Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)
			
			'Add the member scripts to the list and store as 0 No data annotations
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#DHS_Commentary:" 	& scriptGenerics, 0, True, DHS_Commentary))
			
			'Write the annotations to the database
			Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)
			
			'Show a message box that the RP was successfully updated
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
			selectionChangedTaskResult.IsOK = True
			selectionChangedTaskResult.ShowMessageBox = True
			selectionChangedTaskResult.Message = "" & GetDescription(si,ModName) & " Successfully Updated"
		 	Return selectionChangedTaskResult
			
								
			Return Nothing
		End Function
		Private Function Mod_OMBJ_CJ_Comments() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.Mod_OMBJ_CJ_Comments ====
			Dim wfTime As String = args.NameValuePairs("WFTime")
			Dim wfScenario As String = args.NameValuePairs("WFScenario")
			Dim wfCube As String = args.NameValuePairs("WFCube")
			
			Dim ModName As String = args.NameValuePairs("ModName")
			Dim Justification_Commentary As String = args.NameValuePairs("Justification_Commentary")
			Dim Performance_Commentary As String = args.NameValuePairs("Performance_Commentary")
			Dim Description_Commentary As String = args.NameValuePairs("Description_Commentary")
			
			Dim scriptGenerics As String = "S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & ModName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			
			'Create a new list of memberscript and value
			Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)
			'Add the member scripts to the list and store as 0 No data annotations
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Mod_DescriptionOfItem:" 	& scriptGenerics, 0, True, Description_Commentary))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Mod_Justifications:" 	& scriptGenerics, 0, True, Justification_Commentary))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Mod_ImpactOnPerformance:" 	& scriptGenerics, 0, True, Performance_Commentary))
			
			'Write the annotations to the database
			Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)
			
			'Show a message box that the RP was successfully updated
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
			selectionChangedTaskResult.IsOK = True
			selectionChangedTaskResult.ShowMessageBox = True
			selectionChangedTaskResult.Message = "" & GetDescription(si,ModName) & " Successfully Updated"
		 	
			Return selectionChangedTaskResult

			Return Nothing
		End Function
		Private Function MoveRelationshipMember() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.MoveRelationshipMember ====
								
						Dim targetDimVal As String = "Std_Flow"						
						Dim targetDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, targetDimVal)
						Dim selectedMember As String = args.NameValuePairs("selectedMember")	
						Dim selectedMemberID As Integer = BRApi.Finance.Members.GetMemberId(si, targetDimPk.DimTypeId, selectedMember)
						Dim selectedSibling As String = args.NameValuePairs("selectedSibling")
						Dim selectedSiblingId As Integer = BRApi.Finance.Members.GetMemberId(si, targetDimPk.DimTypeId, selectedSibling)
						'Get the parent from the selected member.  This should only have one parent in our hierarchy
						Dim parents As List(Of Member) = BRApi.Finance.Members.GetParents(si, targetDimPk, brapi.Finance.Members.GetMemberId(si, dimTypeId.Flow, selectedMember), False)
						Dim parentId As Integer = parents(0).MemberId
						
						'Relationship
						Dim relPk As New RelationshipPk(targetDimPk.DimTypeId, ParentID, selectedMemberID)
						Dim rel As New Relationship(relPk, targetDimPk.DimId, RelationshipMovementType.InsertAfterSibling, 1)
						Dim relInfo As New RelationshipInfo(rel, Nothing)
						Dim relPosOpt As New RelationshipPositionOptions(RelationshipMovementType.InsertAfterSibling, selectedSiblingId)
						
						'Save the Member Relationship inserted after the selected sibling
						BRApi.Finance.MemberAdmin.SaveRelationshipInfo(si, relInfo, relPosOpt)
						'BRApi.Finance.MemberAdmin.CopyOrMoveRelationships(si, TargetDimPk, relationshipPks, newParentId, False, relationshipPositionOptions)
						
			Return Nothing
		End Function
		Private Function OnCbxBtnClick_GEN() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.OnCbxBtnClick_GEN ====
					 
'					 'Get the dashboard selected and use it to determine which section to run
'					Dim dbdSelected As String = args.NameValuePairs("DbdSelected")					 
					
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
										
					'Get the component name
					Dim componentName As String = args.ComponentInfo.Component.Name
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					
					Dim Content_EditRP_OS As String = args.NameValuePairs.XFGetValue("Content_EditRP_OS")
					Dim Content_OS As String = args.NameValuePairs.XFGetValue("Content_OS")
					
					' If No RP is selected, nothing to do
					If RPName = "" Then 
						Return Nothing
					End If
					
					Dim RPChanged As Boolean = False
					Dim routingAppn As String = ResolveRoutingAppnForRP(RPName, args.NameValuePairs.XFGetValue("APPN_Content", "OS"))
					Dim RPNameCopy As String = args.NameValuePairs.XFGetValue("RPNameCopy")
					If Not String.IsNullOrEmpty(RPNameCopy) AndAlso RPNameCopy<> RPName AndAlso RPNameCopy <> "None" Then 
						RPChanged= True

						If CheckSaveState(si, globals, args) Then
							'Throw New Exception(mShowMessage)
							SetRoutingNumber(selectionChangedTaskResult.ModifiedCustomSubstVars, routingAppn, RPNameCopy)
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Number_OS_Copy", RPNameCopy)
							If Not String.IsNullOrEmpty(Content_EditRP_OS) Then 
								SetRoutingPageCompat(selectionChangedTaskResult.ModifiedCustomSubstVars, routingAppn, Content_EditRP_OS)
							Else
								SetRoutingPageCompat(selectionChangedTaskResult.ModifiedCustomSubstVars, routingAppn, routingAppn & "_RP_Page1")
							End If
							SetRoutingContent(selectionChangedTaskResult.ModifiedCustomSubstVars, routingAppn, routingAppn & "_RP_Content")
							selectionChangedTaskResult.IsOK = False
							selectionChangedTaskResult.ShowMessageBox = True
							selectionChangedTaskResult.Message = mShowMessage
							selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True								
							Return selectionChangedTaskResult								
						End If
					End If
					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)												
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"								
							
					'using a global function to avoid using brapi functions too many times and use api.data.calculate via a finance rule instead
					'set the script generics and parent account to be used in the global function
					globals.SetStringValue("scriptGenerics", scriptGenerics)
					globals.SetStringValue("parAccount", "RP_Attributes")					

					'Set a generic dictionary as an argument in the rule below
					Dim Dictionary As New Dictionary(Of String, String)
					
						BUDFM_AttributeSupport.GetRPAttributes(si, globals)
					
					If Not globals.GetObject("attributeDict") Is Nothing
					
						Dim attributeDict As Dictionary(Of String, String) = globals.GetObject("attributeDict")
							
						'Set the parameters for the combo boxes in the RP Dashboard Page1
						'Set the defaults for General Detail and O&S and Personnel Qtrs if not stored
						Dim Add_General_Detail As String = String.Empty
						Dim Add_General_DetailSaved As String = attributeDict.GetValueOrEmpty("Add_General_Detail")
						
						If String.IsNullOrEmpty(Add_General_DetailSaved)
							Add_General_Detail = "Y"
						Else 
							Add_General_Detail = Add_General_DetailSaved
						End If
						
						Dim Personnel_Qtrs As String = String.Empty
						Dim Personnel_QtrsSaved As String = attributeDict.GetValueOrEmpty("Personnel_Qtrs")
						
						If String.IsNullOrEmpty(Personnel_QtrsSaved)
							Personnel_Qtrs = BRApi.Dashboards.Parameters.GetParameterDisplayInfo(si, False, Nothing, "prm_BLT_NumberOfPersonnelQtrs_OS").Parameter.DefaultValue
						Else 
							Personnel_Qtrs = Personnel_QtrsSaved
						End If
						
						Dim OS_Qtrs As String = String.Empty
						Dim OS_QtrsSaved As String = attributeDict.GetValueOrEmpty("OS_Qtrs")
						
						If String.IsNullOrEmpty(OS_QtrsSaved)
							OS_Qtrs = BRApi.Dashboards.Parameters.GetParameterDisplayInfo(si, False, Nothing, "prm_NBLT_NumberOfOSQtrs_OS").Parameter.DefaultValue
						Else 
							OS_Qtrs = OS_QtrsSaved
						End If
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Number_OS_Copy", 						RPName)							
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_NumberOfBillets_OS", 				attributeDict.GetValueOrEmpty("Number_of_Billets"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_AutoAddGenDetail_OS", 			Add_General_Detail)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_IncreaseDecrease_OS", 			attributeDict.GetValueOrEmpty("Increase_Decrease"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_PartOfReprogramming_OS", 			attributeDict.GetValueOrEmpty("Part_of_Reprogramming"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_NumberOfPersonnelQtrs_OS", 		Personnel_Qtrs)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_NumberOfOSQtrs_OS", 				OS_Qtrs)
						
						'Set the parameters for the combo boxes in the RP Dashboard Page2
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_FYRelatedRp1_OS", 					attributeDict.GetValueOrEmpty("FY_Related_RP1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_FYRelatedRp2_OS", 					attributeDict.GetValueOrEmpty("FY_Related_RP2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_FYRelatedRp3_OS", 					attributeDict.GetValueOrEmpty("FY_Related_RP3"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_OlderRelatedRp1_OS", 					attributeDict.GetValueOrEmpty("Older_Related_RP1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_OlderRelatedRp2_OS", 					attributeDict.GetValueOrEmpty("Older_Related_RP2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_OlderRelatedRp3_OS", 					attributeDict.GetValueOrEmpty("Older_Related_RP3"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOffice1_OS", 						attributeDict.GetValueOrEmpty("Lead_Office1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOffice2_OS", 						attributeDict.GetValueOrEmpty("Lead_Office2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOffice3_OS", 						attributeDict.GetValueOrEmpty("Lead_Office3"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePOC1_OS", 					attributeDict.GetValueOrEmpty("Lead_Office_POC1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePOC2_OS", 					attributeDict.GetValueOrEmpty("Lead_Office_POC2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePOC3_OS", 					attributeDict.GetValueOrEmpty("Lead_Office_POC3"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePhone1_OS", 				attributeDict.GetValueOrEmpty("Lead_Office_Phone1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePhone2_OS", 				attributeDict.GetValueOrEmpty("Lead_Office_Phone2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePhone3_OS", 				attributeDict.GetValueOrEmpty("Lead_Office_Phone3"))					
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_ExecSummary_OS", 						attributeDict.GetValueOrEmpty("Exec_Summary"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_IE_K_OS", 						attributeDict.GetValueOrEmpty("Initial_Estimate"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_IE_MIL_OS", 						attributeDict.GetValueOrEmpty("Initial_Estimate_MIL_FTP"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_IE_CIV_OS", 						attributeDict.GetValueOrEmpty("Initial_Estimate_CIV_FTP"))			
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_IE_Base_Funding_OS", 				attributeDict.GetValueOrEmpty("Base_Funding"))			
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_IE_Base_Funding_Comments_OS", 		attributeDict.GetValueOrEmpty("Base_Funding_Comments"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_CBF_MIL_OS", 						attributeDict.GetValueOrEmpty("Base_Funding_MIL_FTP"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_CBF_CIV_OS", 						attributeDict.GetValueOrEmpty("Base_Funding_CIV_FTP"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_IE_R_Base_OS", 					attributeDict.GetValueOrEmpty("Recurring_Base_Estimate"))					
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_R_Base_Comments_OS", 				attributeDict.GetValueOrEmpty("Recurring_Base_Comments"))	
						                                                                                                                
						'Set the parameters for the combo boxes in the RP Dashboard Page3 (MSN added this 01/20/23)                     
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Page3_AffectOthers_OS", 				attributeDict.GetValueOrEmpty("Affect_Others"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Page3_Alignment_OS", 					attributeDict.GetValueOrEmpty("Alignment"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Page3_DenialImpact_OS", 				attributeDict.GetValueOrEmpty("Denial_Impact"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Page3_FundingImpact_OS", 				attributeDict.GetValueOrEmpty("Funding_Impact"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Page3_Problem_OS", 					attributeDict.GetValueOrEmpty("Problem"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Page3_ROI_OS", 						attributeDict.GetValueOrEmpty("ROI"))
						If Not String.IsNullOrEmpty(Content_EditRP_OS) Then 
							SetRoutingPageCompat(selectionChangedTaskResult.ModifiedCustomSubstVars, routingAppn, routingAppn & "_RP_Page1")
						End If 
						SetRoutingContent(selectionChangedTaskResult.ModifiedCustomSubstVars, routingAppn, routingAppn & "_RP_Content")
					End If 'Not globals.GetObject("attributeDict") Is Nothing
										
					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					Return selectionChangedTaskResult
					
			Return Nothing
		End Function
		Private Function OnCbxRP_BilletOPFAC_Selected() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.OnCbxRP_BilletOPFAC_Selected ====
			
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
					
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					
					'Get the variables passed in
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					Dim LINumber As String = args.NameValuePairs.XFGetValue("LINumber")
					Dim OPFAC As String = args.NameValuePairs("OPFAC")
					Dim OPFACLength As Integer = OPFAC.Length
					
					' If No RP is selected, nothing to do
					If RPName = "" Then
						BRApi.State.SetSessionState(si, False, ClientModuletype.Unknown,"","","dashState","dashState","Edit", si.XfBytes)
						Return Nothing
					End If
					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)
					
					'Getting intersected data value such as RP and Line Item
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumber & ":U7#None:U8#None"						
		
					'Assign variables for stored attributes
					Dim UII_Saved As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Billet_UII:" & scriptGenerics).DataCellEx.DataCellAnnotation
					Dim Term_Billet_Saved As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Term_Billet:" & scriptGenerics).DataCellEx.DataCellAnnotation
			
					'Assign variables to be set
                    Dim UII As String = String.Empty 
                    Dim Term_Billet As String = String.Empty 
					
					'Set the UII default if nothing already saved
					If UII_Saved.Length > 0
						UII = UII_Saved
					Else
'						'Checking OPFAC Combo Box value user selected
						If OPFACLength >= 2 
							If OPFAC.Substring(0,2).XFEqualsIgnoreCase("49")
								UII = String.Empty
							Else 
								If OPFACLength >= 10									
									If OPFAC.Substring(0,10).XFEqualsIgnoreCase("98_70098_6")
										UII = String.Empty
									Else 
										UII = "NoInvestment"
									End If
								Else 
									UII = "NoInvestment"
								End If	
							End If
					    Else 
							UII = "NoInvestment"
						End If					
					 End If	
					 
					'Set the Term Billet default if nothing already saved
					If Term_Billet_Saved.Length > 0
						Term_Billet = Term_Billet_Saved
					Else
'						'Checking OPFAC Combo Box value user selected
						If OPFACLength >= 2							
							Dim OPFACFirstTwo As String = OPFAC.Substring(0,2)
							Select Case OPFACFirstTwo
							Case "28","41","45"
								Term_Billet = String.Empty
							Case Else								
								If OPFACLength >= 10 			
									Dim OPFACFirstTen As String = OPFAC.Substring(0,10)
									Select Case OPFACFirstTen
										Case "98_70098_9"
											Term_Billet = String.Empty
										Case Else
											Term_Billet = "Perm"
									End Select
								Else 
										Term_Billet = "Perm"
								End If									
							End Select								
						Else 
							Term_Billet = "Perm"
						End If
					End If
					
					
					Dim oPFACID As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD4, oPFAC)				
                    
					'Text3
					Dim Text3 As String = BRApi.Finance.UD.Text(si, dimTypeId.UD4, oPFACID, 3, 0, 0)
					If  Not String.IsNullOrEmpty(Text3) Then
				
						Dim Text3Split As List(Of String) = StringHelper.SplitString(Text3,"|")	
						If Text3Split.Count = 7 Then 
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Build_Out_OS",  	Text3Split(0))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Lease_OS",  		Text3Split(1))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Lease_ATU_OS",  	Text3Split(2))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Lease_PPA_OS",  	Text3Split(3))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Utilities_OS",  	Text3Split(4))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_UTL_ATU_OS",  	Text3Split(5))
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_UTL_PPA_OS",  	Text3Split(6))
						End If 
					End If 					
					'Text3

					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_UII_OS", UII)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_ConusOConus_OS", BRApi.Finance.UD.Text(si, dimTypeId.UD4, oPFACID, 2, 0, 0))	
                    selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_TermBillet_OS", Term_Billet)
					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					
					Return selectionChangedTaskResult
					
			Return Nothing
		End Function
		Private Function OnCbxRP_BilletReserveType_Selected() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.OnCbxRP_BilletReserveType_Selected ====
			
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
								
					'Get the component name
					Dim componentName As String = args.ComponentInfo.Component.Name
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					
					' If No RP is selected, nothing to do
					If RPName = "" Then 
						Return Nothing
					End If
					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)
					Dim LINumber As String = args.NameValuePairs.XFGetValue("LINumber")                 
					
					'Getting intersected data value such as RP and Line Item
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumber & ":U7#None:U8#None"						
										
					'Assign variable for stored RT value
                    Dim Reserve_Type_Saved As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Reserve_Type:" & scriptGenerics).DataCellEx.DataCellAnnotation
					
                    Dim Reserve_Type As String = String.Empty 
					
					Dim AD_Reserve As String = args.NameValuePairs("AD_Reserve")				
					
					If Reserve_Type_Saved.Length > 0
						Reserve_Type = Reserve_Type_Saved
					Else
						If AD_Reserve.XFEqualsIgnoreCase("Active_Duty")
							Reserve_Type = "NA_Reserve"
					    Else 						
							Reserve_Type = "High"	
						End If
					 End If					

                    selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_ReserveType_OS", Reserve_Type)
					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					
					Return selectionChangedTaskResult
					
			Return Nothing
		End Function
		Private Function OnCbxRP_BilletType_Selected() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.OnCbxRP_BilletType_Selected ====
			
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
								
					'Get the component name
					Dim componentName As String = args.ComponentInfo.Component.Name
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					
					'Get the NVP's passed through
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					Dim LINumber As String = args.NameValuePairs.XFGetValue("LINumber")
					Dim Billet_Type As String = args.NameValuePairs("Billet_Type")
					
					' If No RP is selected, nothing to do
					If RPName = "" Then 
						Return Nothing
					End If
					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)		
					
					If IsOSPG1Empty(globals, si, wfCube,RP_Entity,wfScenario,wfTime,RPName) Then Throw New Exception("Empty attributes in Page 1. All attributes on Page 1 must have a value to save this page.")

					'Create the Generic scripts
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumber & ":U7#None:U8#None"		
					Dim scriptGenericsNoLine As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"						
										
					'using a global function to avoid using brapi functions too many times and use api.data.calculate via a finance rule instead
					'set the script generics and parent account to be used in the global function
					globals.SetStringValue("scriptGenerics", scriptGenerics)
					globals.SetStringValue("parAccount", "Billet_LineItem_Data")					

					'Set a generic dictionary as an argument in the rule below
					Dim Dictionary As New Dictionary(Of String, String)
					BRapi.ErrorLog.LogMessage(si,"Hit Before")
					
						BUDFM_AttributeSupport.GetRPAttributes(si, globals)
						BRapi.ErrorLog.LogMessage(si,"Hit After")
					
					If Not globals.GetObject("attributeDict") Is Nothing
						
						Dim attributeDict As Dictionary(Of String, String) = globals.GetObject("attributeDict")
										
						'Assign variables for stored values to evaluate
	                    Dim Reserve_Type_Saved As String = attributeDict.GetValueOrEmpty("Reserve_Type")
	                    Dim PPE_Type_Saved As String = attributeDict.GetValueOrEmpty("PPE_Type")
	                    Dim PPE_PPA_Saved As String = attributeDict.GetValueOrEmpty("Billet_PPA")  
						Dim PPE_ATU_Saved As String = attributeDict.GetValueOrEmpty("Billet_ATU")
		                Dim Detached_Duty_Saved As String = attributeDict.GetValueOrEmpty("Detached_Duty")
		                Dim ICASS_Costs_Saved As String = attributeDict.GetValueOrEmpty("ICASS_Costs") 
						Dim BI_Type_Saved As String = attributeDict.GetValueOrEmpty("Background_Investigation_Type")
			            
						'Create variables to be set based on whether or not variables above have stored values already
	                    Dim Reserve_Type As String = String.Empty 
	                    Dim AD_Reserve As String = String.Empty 
						Dim PPE_Type As String = String.Empty			
	                    Dim PPE_PPA As String = String.Empty 
	                    Dim PPE_ATU As String = String.Empty 	
						Dim Detached_Duty As String = String.Empty	
						Dim ICASS_Costs As String = String.Empty
						Dim BI_Type As String = String.Empty						
											 
						'Add section for generic defaults if new billet
						'these will only populate if the user selects a billet type
						If (Not Billet_Type.XFEqualsIgnoreCase(""))
																				
							'Detached Duty Logic: default to No
							If Detached_Duty_Saved.Length > 0
								Detached_Duty = Detached_Duty_Saved
							Else								
								Detached_Duty = BRApi.Dashboards.Parameters.GetParameterDisplayInfo(si, False, Dictionary, "prm_BLT_DetachedDuty_OS").Parameter.DefaultValue
							End If
							
							'ICASS Logic: default to No
							If ICASS_Costs_Saved.Length > 0
								ICASS_Costs = ICASS_Costs_Saved
							Else
								ICASS_Costs = BRApi.Dashboards.Parameters.GetParameterDisplayInfo(si, False, Dictionary, "prm_BLT_ICASSType_OS").Parameter.DefaultValue
							End If
							
							'BI_Type Logic: default Background Investigation Type to Normal
							If BI_Type_Saved.Length > 0
								BI_Type = BI_Type_Saved
							Else
								BI_Type = BRApi.Dashboards.Parameters.GetParameterDisplayInfo(si, False, Dictionary, "prm_BLT_BIType_OS").Parameter.DefaultValue
							End If

							'DZ--20231212--DHSUSCG-1509--change made per Jennifers request to allow ppe/ppa/atu selections when civilian is selected
							If PPE_Type_Saved.Length > 0 Or PPE_PPA_Saved.Length > 0 Or PPE_ATU_Saved.Length > 0 Then
								PPE_Type = PPE_Type_Saved
								PPE_PPA = PPE_PPA_Saved
								PPE_ATU = PPE_ATU_Saved
							End If
							If Billet_Type.XFEqualsIgnoreCase("Military") Then
								If Reserve_Type_Saved.Length > 0 Then
									Reserve_Type = Reserve_Type_Saved
								End If
							Else 'Billet_Type is Civilian
								If PPE_Type_Saved.Length < 1 Then
									PPE_Type = BRApi.Dashboards.Parameters.GetParameterDisplayInfo(si, False, Dictionary, "prm_BLT_PPEType_OS").Parameter.DefaultValue
									PPE_PPA = BRApi.Dashboards.Parameters.GetParameterDisplayInfo(si, False, Dictionary, "prm_BLT_PPE_PPA_OS").Parameter.DefaultValue
									PPE_ATU = BRApi.Dashboards.Parameters.GetParameterDisplayInfo(si, False, Dictionary, "prm_BLT_PPE_ATU_OS").Parameter.DefaultValue
								End If
								If PPE_Type.XFContainsIgnoreCase("NA_PPE_Type") Then
									PPE_PPA = BRApi.Dashboards.Parameters.GetParameterDisplayInfo(si, False, Dictionary, "prm_BLT_PPE_PPA_OS").Parameter.DefaultValue
									PPE_ATU = BRApi.Dashboards.Parameters.GetParameterDisplayInfo(si, False, Dictionary, "prm_BLT_PPE_ATU_OS").Parameter.DefaultValue
								End If									
								Reserve_Type = BRApi.Dashboards.Parameters.GetParameterDisplayInfo(si, False, Dictionary, "prm_BLT_ReserveType_OS").Parameter.DefaultValue
								AD_Reserve = BRApi.Dashboards.Parameters.GetParameterDisplayInfo(si, False, Dictionary, "prm_BLT_ADReserve_OS").Parameter.DefaultValue
							End If


							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_ReserveType_OS", Reserve_Type)
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_ADReserve_OS", AD_Reserve)
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_PPEType_OS", PPE_Type)
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_PPE_PPA_OS", PPE_PPA)
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_PPE_ATU_OS", PPE_ATU)		
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_DetachedDuty_OS", Detached_Duty)
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_ICASSType_OS", ICASS_Costs)
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_BIType_OS", BI_Type)					

						End If

					End If 'Not globals.GetObject("attributeDict") Is Nothing

					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					Return selectionChangedTaskResult


			Return Nothing
		End Function
		Private Function OnCbxRP_Billet_Selected() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.OnCbxRP_Billet_Selected ====
					 
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					If RPName = "" Then
						Return Nothing
					End If
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)				
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
				'	brapi.ErrorLog.LogMessage(si, "RP Name " & RPName)
					#Region "New variables"
					Dim Content_EditRP_OS As String = args.NameValuePairs.XFGetValue("Content_EditRP_OS")
					Dim Content_OS As String = args.NameValuePairs.XFGetValue("Content_OS")
					Dim LINumber As String = args.NameValuePairs.XFGetValue("LINumber")
					Dim LINumberCopy As String = args.NameValuePairs.XFGetValue("BLTCopy")
					Dim LINumberToSet As String = String.Empty
					Dim RPNameCopy As String = args.NameValuePairs.XFGetValue("RPNameCopy")
					#End Region 
									
					#Region "New - Check if the RP has changed"			
					If Not String.IsNullOrEmpty(RPNameCopy) AndAlso RPNameCopy<> RPName AndAlso RPNameCopy <> "None" Then ' new

'						If CheckSaveState(si, globals, args) Then
'							'set RP Name back to what it was
'							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Number_OS", RPNameCopy)
'							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Number_OS_Copy", RPNameCopy)
'							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Content_OS","OS_Billets_Main_04c")
'							selectionChangedTaskResult.IsOK = False
'							selectionChangedTaskResult.ShowMessageBox = True
'							selectionChangedTaskResult.Message = mShowMessage
'							selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True								
'							Return selectionChangedTaskResult
							
'						Else
							'load new RP and default Line item to 01
						   	selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Number_OS_Copy", RPName )
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_LineItemNumber_OS",  "LineItem_01")
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_LineItemNumber_OS_Copy",  "LineItem_01")
							SetRoutingContent(selectionChangedTaskResult.ModifiedCustomSubstVars, ResolveRoutingAppnForRP(RPName, args.NameValuePairs.XFGetValue("APPN_Content", "OS")), Content_OS)
						
						'End If
					End If
					
					#End Region
					
										'Logic to set the default line item when the Billet screen is opened
					If LINumber.Length > 0 Then
						'Get the number of billets and integer from the line item member to compare and return appropriate line item per the RP selected
						Dim rightChars As Integer = LINumber.Substring(9,2).XFConvertToInt			
						
						Dim number_of_Billets As Integer = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Number_of_Billets:E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").DataCellEx.DataCellAnnotation.XFConvertToInt
						
						If  rightChars > number_of_Billets
							LINumberToSet = "LineItem_01"	
						Else
							LINumberToSet = LINumber	
						End If
					Else
						LINumberToSet = "LineItem_01"
					End If	
					
					#Region "New - Check if the LINE NUMBER has changed"		
					If Not String.IsNullOrEmpty(LINumberCopy) AndAlso LINumber <> LINumberCopy Then 
'						If CheckSaveState(si, globals, args) Then
						
'							'set LineItem back to what it was
'							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_LineItemNumber_OS", LINumberCopy)
'							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_LineItemNumber_OS_Copy", LINumberCopy)
'							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Content_OS","OS_Billets_Main_04c")
'							selectionChangedTaskResult.IsOK = False
'							selectionChangedTaskResult.ShowMessageBox = True
'							selectionChangedTaskResult.Message = mShowMessage
'							selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True								
'							Return selectionChangedTaskResult
							
'						Else
							'load new line item and update 
						   	selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Number_OS_Copy", RPName )
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_LineItemNumber_OS",  LINumber)
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_LineItemNumber_OS_Copy",  LINumber)
							SetRoutingContent(selectionChangedTaskResult.ModifiedCustomSubstVars, ResolveRoutingAppnForRP(RPName, args.NameValuePairs.XFGetValue("APPN_Content", "OS")), Content_OS)
						
						'End If
					End If
					#End Region	
					
					
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumberToSet & ":U7#None:U8#None"						
											
					'using a global function to avoid using brapi functions too many times and use api.data.calculate via a finance rule instead
					'set the script generics and parent account to be used in the global function
					globals.SetStringValue("scriptGenerics", scriptGenerics)
					globals.SetStringValue("parAccount", "Billet_LineItem_Data")					

					'Set a generic dictionary as an argument in the rule below
					Dim Dictionary As New Dictionary(Of String, String)
					
						BUDFM_AttributeSupport.GetRPAttributes(si, globals)
					
					If Not globals.GetObject("attributeDict") Is Nothing
					
						Dim attributeDict As Dictionary(Of String, String) = globals.GetObject("attributeDict")
							
						'For the ATU creteria, we need to derive the parent ATU since we store it in NoUnit
						'Derive Billet_ATU from Billet_ATU_NoUnit since we stored it as a base but they chose a parentDim Billet_ATU_NoUnit As String = Billet_ATU_NoUnit_Info
						Dim Billet_ATU_NoUnit As String = attributeDict.GetValueOrEmpty("Billet_ATU")
						Dim Billet_ATU As String = String.Empty
						If Billet_ATU_NoUnit.Length > 0
							Billet_ATU = Billet_ATU_NoUnit.Substring(0, Billet_ATU_NoUnit.Length - 7)
							'If nothing Return Zero
						End If
						
						'Derive PPE_ATU from PPE_ATU_NoUnit since we stored it as a base but they chose a parent
						Dim PPE_ATU_NoUnit As String = attributeDict.GetValueOrEmpty("PPE_ATU")	
						Dim PPE_ATU As String = String.Empty
						If PPE_ATU_NoUnit.Length > 0
							PPE_ATU = PPE_ATU_NoUnit.Substring(0, PPE_ATU_NoUnit.Length - 7)
							'If nothing Return Zero
						End If	
												
'						'Derive lease_ATU from lease_ATU_NoUnit since we stored it as a base but they chose a parent
						Dim lease_ATU_NoUnit As String = attributeDict.GetValueOrEmpty("Lease_ATU")	
						Dim lease_ATU As String = String.Empty
						If lease_ATU_NoUnit.Length > 0
							lease_ATU = lease_ATU_NoUnit.Substring(0, lease_ATU_NoUnit.Length - 7)
							'If nothing Return Zero
						End If	
						
						'Derive UTL_ATU from UTL_ATU_NoUnit since we stored it as a base but they chose a parent
						Dim UTL_ATU_NoUnit As String = attributeDict.GetValueOrEmpty("Utilities_ATU")	
						Dim UTL_ATU As String = String.Empty
						If UTL_ATU_NoUnit.Length > 0
							UTL_ATU = UTL_ATU_NoUnit.Substring(0, UTL_ATU_NoUnit.Length - 7)
							'If nothing Return Zero
						End If			
						
						'set the line item based on the above logic
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_LineItemNumber_OS", LINumberToSet)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_LineItemNumber_OS_Copy", LINumberToSet)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_PPE_ATU_OS", PPE_ATU)	
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Lease_ATU_OS", lease_ATU)	
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_ATU_OS", Billet_ATU)	
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_UTL_ATU_OS", UTL_ATU)		
							
						'For all other billet attributes, just return what was stored
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_BilletType_OS", 			attributeDict.GetValueOrEmpty("Billet_Type"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_GradeType_OS", 			attributeDict.GetValueOrEmpty("Grade_Type"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_GradeRank_OS", 			attributeDict.GetValueOrEmpty("Grade_Rank"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_ADReserve_OS", 			attributeDict.GetValueOrEmpty("AD_Reserve"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_ReserveType_OS", 			attributeDict.GetValueOrEmpty("Reserve_Type"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_SpcCodeOccSeries_OS", 	attributeDict.GetValueOrEmpty("Spe_Code_Occu_Series"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Pilot_OS", 				attributeDict.GetValueOrEmpty("Pilot"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_ElectronicFlightBag_OS", 	attributeDict.GetValueOrEmpty("Electronic_Flight_Bag"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_PositionNumber_OS", 		attributeDict.GetValueOrEmpty("Position_Number"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_PositionTitle_OS", 		attributeDict.GetValueOrEmpty("Position_Title"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_OPFACS_OS", 				attributeDict.GetValueOrEmpty("OPFAC"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_UII_OS", 					attributeDict.GetValueOrEmpty("Billet_UII"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_ConusOConus_OS", 			attributeDict.GetValueOrEmpty("CONUS_OCONUS"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_DetachedDuty_OS", 		attributeDict.GetValueOrEmpty("Detached_Duty"))								
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_DutyLocation_OS", 		attributeDict.GetValueOrEmpty("Detached_Duty_Location"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_TermBillet_OS", 			attributeDict.GetValueOrEmpty("Term_Billet"))
						
						Dim PPE_Typedescription As String = String.Empty
						Dim loopCounter As Integer = 0
						
						If attributeDict.GetValueOrEmpty("PPE_Type").Length = 0
							PPE_Typedescription = ""
						Else
							
							Dim selectedArray() As String = attributeDict.GetValueOrEmpty("PPE_Type").Replace(" ", "").Split(",")
							Dim types As List(Of String) = selectedArray.ToList()
						
							For Each ppetype As String In types
								If loopCounter = 0 Then
							
									PPE_Typedescription = BRApi.Finance.Members.GetMember(si, dimtypeid.UD8, ppetype).Description 
							
								Else
								
									PPE_Typedescription = PPE_Typedescription & ", " & BRApi.Finance.Members.GetMember(si, dimtypeid.UD8, ppetype).Description
								
								End If
							
								loopCounter+=1
						
						   Next
						
						
						End If
						
					
					
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_PPEType_OS", 				attributeDict.GetValueOrEmpty("PPE_Type"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_PPEType_Descr_OS", 				PPE_Typedescription)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_PPE_PPA_OS", 				attributeDict.GetValueOrEmpty("PPE_PPA"))										
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Build_Out_OS", 			attributeDict.GetValueOrEmpty("Build_Out_Choice"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_ICASSType_OS", 			attributeDict.GetValueOrEmpty("ICASS_Costs"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_BIType_OS", 				attributeDict.GetValueOrEmpty("Background_Investigation_Type"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Acq_Project_OS", 			attributeDict.GetValueOrEmpty("Acquisition_Project"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Lease_OS", 				attributeDict.GetValueOrEmpty("Lease_Choice"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Lease_PPA_OS", 			attributeDict.GetValueOrEmpty("Lease_PPA"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Furniture_OS", 			attributeDict.GetValueOrEmpty("Furniture_Reqd"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Utilities_OS", 			attributeDict.GetValueOrEmpty("Utilities_Reqd"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Computer_Type_OS", 		attributeDict.GetValueOrEmpty("Computer_Type"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Comment_OS", 				attributeDict.GetValueOrEmpty("LineItem_Comment"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_UTL_PPA_OS", 				attributeDict.GetValueOrEmpty("Utilities_PPA"))
						
						
						SetRoutingContent(selectionChangedTaskResult.ModifiedCustomSubstVars, ResolveRoutingAppnForRP(RPName, args.NameValuePairs.XFGetValue("APPN_Content", "OS")), Content_OS)
						
					End If 'Not globals.GetObject("attributeDict") Is Nothing
											
					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True	
					Return selectionChangedTaskResult						
					'End Select													
					
			Return Nothing
		End Function
		Private Function OnCbxRP_BuildOut_Lease_Selected() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.OnCbxRP_BuildOut_Lease_Selected ====
									
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
								
					'Get the component name
					Dim componentName As String = args.ComponentInfo.Component.Name
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					' If No RP is selected, nothing to do
					If RPName = "" Then 
						Return Nothing
					End If
					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)
					Dim LINumber As String = args.NameValuePairs.XFGetValue("LINumber")
					
					'Getting intersected data value such as RP and Line Item
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumber & ":U7#None:U8#None"						
										
					'Assign variables for Lease Choice
                    Dim Lease_Choice_Saved As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "Lease_Choice:" & scriptGenerics).DataCellEx.DataCellAnnotation
                    
		   			Dim Lease_Choice As String = String.Empty	 
					
					Dim Build_Out_Choice As String = args.NameValuePairs("Build_Out_Choice")
					
					If Lease_Choice_Saved.Length > 0 
                       Lease_Choice = Lease_Choice_Saved
					   
					Else
						'Get the NA value when PPE Required field has value of No
						If Build_Out_Choice.XFEqualsIgnoreCase("Y")
                           	Lease_Choice = "Lease_No"
  							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_UTL_PPA_OS", "NA_PPA")       
							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_UTL_ATU_OS", "NA_ATU")  
					
						End If
					
					 End If					
					 
                    selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Lease_OS", Lease_Choice)       
					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					
					Return selectionChangedTaskResult
					
			Return Nothing
		End Function
		Private Function OnCbxRP_GradeType_Selected() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.OnCbxRP_GradeType_Selected ====
					
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
								
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					
					' If No RP is selected, nothing to do
					If RPName = "" Then 
						Return Nothing
					End If
					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)					

					Dim LINumber As String = args.NameValuePairs.XFGetValue("LINumber")
                 					
					'Getting intersected data value such as RP and Line Item
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumber & ":U7#None:U8#None"						
										
					'Assign variable for stored Reserve_Type value
                    Dim Grade_Rank_Saved As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Grade_Rank:" & scriptGenerics).DataCellEx.DataCellAnnotation
					
                    Dim Grade_Rank As String = String.Empty 					
					Dim Grade_Type As String = args.NameValuePairs("Grade_Type")				
					
'					If Grade_Rank_Saved.Length > 0
'						Grade_Rank = Grade_Rank_Saved

'					Else						
						Select Case Grade_Type
							Case "ES"
								Grade_Rank = "ES_00"
							Case "AD"
								Grade_Rank = "AD_00"
							Case "AL"
								Grade_Rank = "AL_00"
						End Select
					
'					 End If		

                    selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_GradeRank_OS", Grade_Rank)
					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					
					Return selectionChangedTaskResult
					
					
			Return Nothing
		End Function
		Private Function OnCbxRP_Lease_Selected() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.OnCbxRP_Lease_Selected ====
									
					'Get Time from current Workflow
					'{BudFM_SolutionHelper}{OnCbxRP_Utilities_Selected}{WFTime=2024, WFScenario=RPSeeding_FY24, WFCube=BudEx, RPName=|!prm_Number_OS!|, LINumber=|!prm_BLT_LineItemNumber_OS!|, Lease=|!prm_BLT_Lease_OS!|}
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
								
					'Get the component name
					Dim componentName As String = args.ComponentInfo.Component.Name
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					
					' If No RP is selected, nothing to do
					If RPName = "" Then 
						Return Nothing
					End If
					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)
					Dim LINumber As String = args.NameValuePairs.XFGetValue("LINumber")
                 
					'Getting intersected data value such as RP and Line Item
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumber & ":U7#None:U8#None"						
										
					'Assign variables for PPA and ATU that were saved
                    Dim lease_PPA_Saved As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Lease_PPA:" & scriptGenerics).DataCellEx.DataCellAnnotation
                    Dim lease_PPA As String = String.Empty 
                    Dim lease_ATU_Saved As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Lease_ATU:" & scriptGenerics).DataCellEx.DataCellAnnotation
                    Dim lease_ATU As String = String.Empty 
					Dim lease_Reqd As String = args.NameValuePairs("Lease")

					'If PPA and ATU value stored at account dimension, then assign that value
					If lease_PPA_Saved.Length > 0
						   lease_PPA = lease_PPA_Saved
					Else
						'Get the NA value when Utility field has value of No
						If Not lease_Reqd.XFEqualsIgnoreCase("Lease_Direct")
							lease_PPA = "NA_PPA" 
						End If
					 End If							 
					 
					If lease_ATU_Saved.Length > 0
						   lease_ATU = lease_ATU_Saved
					Else
						'Get the NA value when Utility field has value of No
						If Not lease_Reqd.XFEqualsIgnoreCase("Lease_Direct")
							lease_ATU = "NA_ATU" 
						End If
					 End If		
					'Set the Lease PPA and ATU 
                    selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Lease_PPA_OS", lease_PPA)
                    selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Lease_ATU_OS", lease_ATU)
					 
					'If Lease = Munro, then Utilities should default to N and the utilties ATU and PPA should default to NA, else do nothing
					If lease_Reqd.XFEqualsIgnoreCase("Lease_Munro")
	                    Dim UTL_Saved As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Utilities_Reqd:" & scriptGenerics).DataCellEx.DataCellAnnotation
	                    Dim UTL_PPA_Saved As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Utilities_PPA:" & scriptGenerics).DataCellEx.DataCellAnnotation
						Dim UTL_ATU_Saved As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Utilities_ATU:" & scriptGenerics).DataCellEx.DataCellAnnotation
						
						Dim UTL_Reqd As String = String.Empty
	                    Dim UTL_PPA As String = String.Empty 
	                    Dim UTL_ATU As String = String.Empty 						
					
						'If Utilities has a saved value, then just return that and the PPA/ATU already saved
						If UTL_Saved.Length > 0
							UTL_Reqd = UTL_Saved
							UTL_PPA = UTL_PPA_Saved
							UTL_ATU = UTL_ATU_Saved						   
						Else
							UTL_Reqd = "N"
							UTL_PPA = "NA_PPA" 
							UTL_ATU = "NA_ATU"
						 End If					

	                    selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Utilities_OS", UTL_Reqd)
	                    selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_UTL_PPA_OS", UTL_PPA)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_UTL_ATU_OS", UTL_ATU)
						
					End If

					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True					
					Return selectionChangedTaskResult
					
			Return Nothing
		End Function
		Private Function OnCbxRP_NBLT_RequestedItem_Tier1_Selected() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.OnCbxRP_NBLT_RequestedItem_Tier1_Selected ====
			
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
								
					'Get the NVP's passed through
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					
					' If No RP is selected, nothing to do
					If RPName = "" Then 
						Return Nothing
					End If
					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)		
					
					'For Requested item Tier1 we just want to force users to fill out page 1 before proceeding.  If page 1 is filled, we do nothing
					If IsOSPG1Empty(globals, si, wfCube,RP_Entity,wfScenario,wfTime,RPName) Then Throw New Exception("Empty attributes in Page 1. All attributes on Page 1 must have a value to save this page.")
				
					
			Return Nothing
		End Function
		Private Function OnCbxRP_NonBillet_Selected() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.OnCbxRP_NonBillet_Selected ====
				
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")				
					'Get the component name
					Dim componentName As String = args.ComponentInfo.Component.Name
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					
					Dim Content_EditRP_OS As String = args.NameValuePairs.XFGetValue("Content_EditRP_OS")
					Dim Content_OS As String = args.NameValuePairs.XFGetValue("Content_OS")

					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					' If No RP is selected, nothing to do
					If RPName = "" Then 
						Return Nothing
					End If
					Dim RPChanged As Boolean = False
					Dim RPNameCopy As String = args.NameValuePairs.XFGetValue("RPNameCopy")
					If Not String.IsNullOrEmpty(RPNameCopy) AndAlso RPNameCopy<> RPName AndAlso RPNameCopy <> "None" Then 
						RPChanged= True
'						If CheckSaveState(si, globals, args) Then
'							'Throw New Exception(mShowMessage)
'							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Number_OS", RPNameCopy)
'							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Number_OS_Copy", RPNameCopy)
'							If Not String.IsNullOrEmpty(Content_EditRP_OS) Then 
'								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Content_EditRP_OS", Content_EditRP_OS)
'							End If
'							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Content_OS","OS_Billets_AddEditNon_04d")
							
'							'selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Content_EditRP_OS", "042b2b_BDF_RP_Dashboard_Content_EditPageRP_Page2_OS")
'							'selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Content_OS","OS_RP_Content")
'							selectionChangedTaskResult.IsOK = False
'							selectionChangedTaskResult.ShowMessageBox = True
'							selectionChangedTaskResult.Message = mShowMessage
'							selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True								
'							Return selectionChangedTaskResult								
'						End If
					End If
			
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)	
					
					Dim LINumber As String = args.NameValuePairs.XFGetValue("NBLT")
					Dim LINumberCopy As String = args.NameValuePairs.XFGetValue("NBLTCopy")

					
					If RPChanged Then
						LINumber ="NBLineItem_01"
						LINumberCopy= ""
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_LineItemNumber_OS_Copy", LINumber)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_LineItemNumber_OS", LINumber)
					End If			

					Dim LINumberToSet As String = String.Empty
				
					If LINumber.Length > 0 Then
						LINumberToSet = LINumber	
					Else
						LINumberToSet = "NBLineItem_01"
						'load the first time
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_LineItemNumber_OS_Copy", LINumberToSet)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_LineItemNumber_OS", LINumberToSet)
						
					End If
											
					If Not String.IsNullOrEmpty(LINumberCopy) AndAlso LINumber <> LINumberCopy Then 

'						If CheckSaveState(si, globals, args) Then
'						'Set it back to how it was
'							LINumberToSet = LINumberCopy
'							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_LineItemNumber_OS", LINumberToSet)
'							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_LineItemNumber_OS_Copy", LINumberToSet)
'							If Not String.IsNullOrEmpty(Content_EditRP_OS) Then 
'								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Content_EditRP_OS", Content_EditRP_OS)
'							End If
'							selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Content_OS","OS_Billets_AddEditNon_04d")
'							selectionChangedTaskResult.IsOK = False
'							selectionChangedTaskResult.ShowMessageBox = True
'							selectionChangedTaskResult.Message = mShowMessage
'							selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True								
'							Return selectionChangedTaskResult	
							
'						Else
							'load new data
						   	selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_LineItemNumber_OS_Copy", LINumberToSet)

						'End If
					End If		


					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumberToSet & ":U7#None:U8#None"			
					Dim scriptGenericsDescr As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_Input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U6#"& LINumberToSet & ":U7#None:U8#None"				
					
							'using a global function to avoid using brapi functions too many times and use api.data.calculate via a finance rule instead
							'set the script generics and parent account to be used in the global function
							globals.SetStringValue("scriptGenerics", scriptGenerics)
							globals.SetStringValue("parAccount", "NonBillet_LineItem_Data")					
		
							'Set a generic dictionary as an argument in the rule below
							Dim Dictionary As New Dictionary(Of String, String)
							
								BUDFM_AttributeSupport.GetRPAttributes(si, globals)
							
								
							If Not globals.GetObject("attributeDict") Is Nothing
							
								Dim attributeDict As Dictionary(Of String, String) = globals.GetObject("attributeDict")
									
	'							'Get info for the Non-Billet
								Dim Requested_Item_Cost_Line As String = attributeDict.GetValueOrEmpty("Requested_Item_Tier1")
								'Get the ItemNum to use to find the description Input account
								Dim requested_ItemNum As Integer
								If (Not Requested_Item_Cost_Line = "") 
									
									Dim requested_Item_Tier1Split As List(Of String) = StringHelper.SplitString(Requested_Item_Cost_Line, "_")
									requested_ItemNum = requested_Item_Tier1Split(0).XFConvertToInt
								End If	
								
								'Derive ATU from ATU_NoUnit since we stored it as a base but they chose a parent
								Dim ATU_NoUnit As String = attributeDict.GetValueOrEmpty("ATU")	

								Dim ATU As String = String.Empty
								'If it already has a value, derive the parent member from the stored NoUnit child
								If ATU_NoUnit.Length > 0	
									ATU = ATU_NoUnit.Substring(0, ATU_NoUnit.Length - 7)
								Else
									
								End If
								
								'Set Parameters for NonBillet info_section
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Number_OS_Copy",                      RPName)
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_RequestedItem_Tier1_OS", 		Requested_Item_Cost_Line)
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_ATU_OS", 						ATU)						
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_Description_Tier2_OS", 			attributeDict.GetValueOrEmpty("Description_Tier2"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_Description_Tier2_Input_OS", 	BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "U5#" & requested_ItemNum & "0_1:" 		& scriptGenericsDescr).DataCellEx.DataCellAnnotation)
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_POC_OS", 						attributeDict.GetValueOrEmpty("POC"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_DollarKValue_OS", 				attributeDict.GetValueOrEmpty("DollarK_Value"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_RecurringNonRecurring_OS", 		attributeDict.GetValueOrEmpty("R_NR"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_PPA_OS", 						attributeDict.GetValueOrEmpty("PPA"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_UII_OS", 						attributeDict.GetValueOrEmpty("UII"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_ObjectClass_OS", 				attributeDict.GetValueOrEmpty("Object_Class"))
								
								If Not String.IsNullOrEmpty(Content_EditRP_OS) Then 
									SetRoutingPageCompat(selectionChangedTaskResult.ModifiedCustomSubstVars, ResolveRoutingAppnForRP(RPName, args.NameValuePairs.XFGetValue("APPN_Content", "OS")), Content_EditRP_OS)
								End If
								SetRoutingContent(selectionChangedTaskResult.ModifiedCustomSubstVars, ResolveRoutingAppnForRP(RPName, args.NameValuePairs.XFGetValue("APPN_Content", "OS")), Content_OS)
							
							End If 'globals.GetObject("attributeDict") Is Nothing
							
							selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True					
							Return selectionChangedTaskResult
							
					
					'Logic to set the default line item when the Billet screen is opened
					
					
			Return Nothing
		End Function
		Private Function OnCbxRP_PPE_Selected() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.OnCbxRP_PPE_Selected ====
									
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
								
					'Get the component name
					Dim componentName As String = args.ComponentInfo.Component.Name
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					' If No RP is selected, nothing to do
					If RPName = "" Then 
						Return Nothing
					End If
					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)
					Dim LINumber As String = args.NameValuePairs.XFGetValue("LINumber")                 
					
					'Getting intersected data value such as RP and Line Item
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumber & ":U7#None:U8#None"						
					
					'Assign variables for PPA and ATU that were saved 
                    Dim PPE_PPA_Saved As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Billet_PPA:" & scriptGenerics).DataCellEx.DataCellAnnotation  
					Dim PPE_ATU_Saved As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Billet_ATU:" & scriptGenerics).DataCellEx.DataCellAnnotation  
		            		
                    Dim PPE_PPA As String = String.Empty 
                    Dim PPE_ATU As String = String.Empty 
					
					'If PPA and ATU value stored at account dimension, then assign that value
					If PPE_PPA_Saved.Length > 0 Or PPE_ATU_Saved.Length > 0
							PPE_PPA = PPE_PPA_Saved
						   	PPE_ATU = PPE_ATU_Saved
					Else
							PPE_PPA = "NA_PPA" 
							PPE_ATU = "NA_ATU"
					 End If					
					 
                    selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_PPE_PPA_OS", PPE_PPA)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_PPE_ATU_OS", PPE_ATU)
					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					
					Return selectionChangedTaskResult
					
			Return Nothing
		End Function
		Private Function OnCbxRP_SetDefault_NonBilletATU() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.OnCbxRP_SetDefault_NonBilletATU ====
						
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
										
					'Get the component name
					Dim componentName As String = args.ComponentInfo.Component.Name
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					' If No RP is selected, nothing to do
					If RPName = "" Then 
						Return Nothing
					End If
					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)
					Dim LINumber As String = args.NameValuePairs.XFGetValue("LINumber")
					Dim costLine As String = args.NameValuePairs.XFGetValue("CostLine")
					
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumber & ":U7#None:U8#None"						
		
					'Get info for the Non-Billet
					
					'Derive ATU from ATU_NoUnit since we stored it as a base but they chose a parent
					Dim ATU_NoUnit As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#ATU:" & scriptGenerics).DataCellEx.DataCellAnnotation
					
					Dim ATU As String = String.Empty	
					Dim ppaToSet As String = String.Empty		
					Dim uiiToSet As String = String.Empty			
					
					'If it already has a value, derive the parent member from the stored NoUnit child
					If ATU_NoUnit.Length > 0
						ATU = ATU_NoUnit.Substring(0, ATU_NoUnit.Length - 7)
					Else
						'If it doesn't have a value, return the default value for ATU and PPA
						ATU = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "E#NA:S#" & wfScenario & ":T#" & wfTime & ":A#None:V#Assumptions:O#Forms:I#None:F#None:U1#None:U2#None:U3#None:U4#No_ATU:U5#" & costLine & ":U6#None:U7#None:U8#None").DataCellEx.DataCellAnnotation
					 	ppaToSet = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "E#NA:S#" & wfScenario & ":T#" & wfTime & ":A#None:V#Assumptions:O#Forms:I#None:F#None:U1#NO_PPA:U2#None:U3#None:U4#None:U5#" & costLine & ":U6#None:U7#None:U8#None").DataCellEx.DataCellAnnotation
						uiiToSet = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "E#NA:S#" & wfScenario & ":T#" & wfTime & ":A#None:V#Assumptions:O#Forms:I#None:F#None:U1#None:U2#NoInvestment:U3#None:U4#None:U5#" & costLine & ":U6#None:U7#None:U8#None").DataCellEx.DataCellAnnotation
					
					End If					
					
					'Set Parameters for Billet info_section 
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_ATU_OS", ATU)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_PPA_OS", ppaToSet)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_UII_OS", uiiToSet)
					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					
					Return selectionChangedTaskResult
					
			Return Nothing
		End Function
		Private Function OnCbxRP_SetDefault_NonBilletPPA() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.OnCbxRP_SetDefault_NonBilletPPA ====
		
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
										
					'Set the selectionChangedTaskResult variable
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")

					' If No RP is selected, nothing to do
					If RPName = "" Then 
						Return Nothing
					End If
					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)
					Dim LINumber As String = args.NameValuePairs.XFGetValue("LINumber")
					Dim costLine As String = args.NameValuePairs.XFGetValue("CostLine")
					
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumber & ":U7#None:U8#None"						
		
					'Get info for the Non-Billet
					
					'Get the ppa for the Non Billet if there has already been one saved for this Non Billet
					Dim ppa As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#PPA:" & scriptGenerics).DataCellEx.DataCellAnnotation
					
					Dim ppaToSet As String = String.Empty
					'If it already has a value, then just return the saved value
					If ppa.Length > 0
						ppaToSet = ppa
					'If it doesn't have a value, return the default value
					Else
						ppaToSet = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "E#NA:S#" & wfScenario & ":T#" & wfTime & ":A#None:V#Assumptions:O#Forms:I#None:F#None:U1#NO_PPA:U2#None:U3#None:U4#None:U5#" & costLine & ":U6#None:U7#None:U8#None").DataCellEx.DataCellAnnotation
											
					End If					
					
					'Set Parameters for Billet info_section 
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_PPA_OS", ppaToSet)
					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					
					Return selectionChangedTaskResult
					
			Return Nothing
		End Function
		Private Function OnCbxRP_SpcCode_Selected() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.OnCbxRP_SpcCode_Selected ====
			
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
					Dim RP_Entity As String = args.NameValuePairs("WFText1")
								
					'Get the component name
					Dim componentName As String = args.ComponentInfo.Component.Name
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					'If RPName is blank, return a None for RP_Entity
					If RPName = ""
						RP_Entity = "None"
					End If
					'BRApi.ErrorLog.LogMessage(si, "RPName: " & RPName)
					Dim LINumber As String = args.NameValuePairs.XFGetValue("LINumber")
					
					'Getting intersected data value such as RP and Line Item
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumber & ":U7#None:U8#None"						
										
					'Assign variable for stored Flight Bag value and Increase/Decrease
                    Dim Electronic_Flight_Bag_Saved As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Electronic_Flight_Bag:" & scriptGenerics).DataCellEx.DataCellAnnotation	
					Dim increase_Decrease As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Increase_Decrease:" & scriptGenerics).DataCellEx.DataCellAnnotation
					
					Dim Specialty_Code As String = args.NameValuePairs("Spe_Code_Occu_Series")
					Dim CodeId As Integer = BRApi.Finance.Members.GetMemberId(si, dimtypeid.UD8, Specialty_Code)
					Dim SpecialtyCodeText2 As String = BRApi.Finance.UD.Text(si, dimtype.UD8.Id, CodeId, 2, DimConstants.Unknown, DimConstants.Unknown)

					Dim Electronic_Flight_Bag As String = String.Empty
					
					If Electronic_Flight_Bag_Saved.Length > 0
						Electronic_Flight_Bag = Electronic_Flight_Bag_Saved

					Else
						If Increase_Decrease.XFEqualsIgnoreCase("I") And SpecialtyCodeText2.XFEqualsIgnoreCase("Y")		
							Electronic_Flight_Bag = "Y"
					    Else 
						
							Electronic_Flight_Bag = "NA"
						End If
					
					 End If					

                    selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_ElectronicFlightBag_OS", Electronic_Flight_Bag)
					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					
					Return selectionChangedTaskResult
					
			Return Nothing
		End Function
		Private Function OnCbxRP_Utilities_Selected() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.OnCbxRP_Utilities_Selected ====
									
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
								
					'Get the component name
					Dim componentName As String = args.ComponentInfo.Component.Name
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					
					' If No RP is selected, nothing to do
					If RPName = "" Then 
						Return Nothing
					End If
					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)
					Dim LINumber As String = args.NameValuePairs.XFGetValue("LINumber")
                 
					
					'Getting intersected data value such as RP and Line Item
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumber & ":U7#None:U8#None"						
										
					'Assign variables for PPA and ATU that were saved
                    Dim UTL_PPA_Saved As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Utilities_PPA:" & scriptGenerics).DataCellEx.DataCellAnnotation
					Dim UTL_ATU_Saved As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Utilities_ATU:" & scriptGenerics).DataCellEx.DataCellAnnotation
					
                    Dim UTL_PPA As String = String.Empty 
                    Dim UTL_ATU As String = String.Empty 
					
					Dim Utilities_Reqd As String = args.NameValuePairs("Utilities_Reqd")
				
					'If PPA and ATU value stored at account dimension, then assign that value
					If UTL_PPA_Saved.Length > 0 Or UTL_ATU_Saved.Length > 0
						   UTL_PPA = UTL_PPA_Saved
						   UTL_ATU = UTL_ATU_Saved
					   
					Else
						'Get the NA value when Utility field has value of No
						If Utilities_Reqd.XFEqualsIgnoreCase("N")
							UTL_PPA = "NA_PPA" 
							UTL_ATU = "NA_ATU"

						End If
					
					 End If					

                    selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_UTL_PPA_OS", UTL_PPA)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_UTL_ATU_OS", UTL_ATU)
					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					
					Return selectionChangedTaskResult
					
			Return Nothing
		End Function
		Private Function OnConcReviewBtnClick() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.OnConcReviewBtnClick ====
					 Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()     
					 'Check if saved and then update session state appropriately
					If CheckSaveState(si, globals, args) Then
						'Throw New Exception(mShowMessage)
					End If
						BRApi.State.SetSessionState(si, False, ClientModuletype.Unknown, "", "", "dashState", "dashState", "ConcReview", si.XfBytes)
			Return Nothing
		End Function
		Private Function RetrieveModComments() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.RetrieveModComments ====
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
					
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					
					Dim ModName As String = args.NameValuePairs.XFGetValue("ModName")
					
					Dim scriptGenerics As String = "S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & ModName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
					
					Dim modComments As String  = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, scriptGenerics & ":A#DHS_Commentary").DataCellEx.DataCellAnnotation
					Dim descComments As String  = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, scriptGenerics & ":A#Mod_DescriptionOfItem").DataCellEx.DataCellAnnotation
					Dim justComments As String  = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, scriptGenerics & ":A#Mod_Justifications").DataCellEx.DataCellAnnotation
					Dim iopComments As String  = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, scriptGenerics & ":A#Mod_ImpactOnPerformance").DataCellEx.DataCellAnnotation

 
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Mod_DHS_Commentary_ADM", modComments)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Mod_DescriptionOfItem_Content", descComments)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Mod_Justifications_Content", justComments)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Mod_ImpactOnPerformance_Content", iopComments)
					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					
					Return selectionChangedTaskResult
				
			Return Nothing
		End Function
		Private Function ReviseMemDescription() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.ReviseMemDescription ====
			
					Dim memberName As String = args.NameValuePairs("MemberName")
					Dim newMemberDescr As String = args.NameValuePairs("NewMemberDescr")									
					
					'Get all the necessary objects to change flow properties
					Dim memInfo As MemberInfo = BRApi.Finance.Members.GetMemberInfo(si, dimTypeId.Flow, memberName, True)
					Dim memId As Integer = memInfo.Member.MemberId
					Dim std_FlowDim As OneStream.Shared.Wcf.Dim = BRApi.Finance.Dim.GetDim(si, "Std_Flow")
					Dim memPk As New MemberPk(std_FlowDim.DimPk.DimTypeId, memId)
					Dim memToUpdate As New Member(memPk,memberName,newMemberDescr,std_FlowDim.DimPk.DimId)
					Dim memVarProps As VaryingMemberProperties = memInfo.Properties
					Dim memToUpdateInfo As New MemberInfo(memToUpdate,memVarProps,Nothing,std_FlowDim, DimConstants.Unknown)
					Dim memMemberProperties As FlowVMProperties = memToUpdateInfo.GetFlowProperties()
					
					'Save the member description
					BRApi.Finance.MemberAdmin.SaveMemberInfo(si, memToUpdateInfo, True, True, False, False)
					
									
			Return Nothing
		End Function
		Private Function RpToModMapping() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.RpToModMapping ====
												
						Dim wfTime As String = TimeDimHelper.GetNameFromId(si.WorkflowClusterPk.TimeKey)
						Dim wfYY As String = rpUtils.Get_WFTime_YY(si, wfTime)
						Dim wfYYPrior1 As String = (wfYY - 1).ToString
						Dim wfYYPrior2 As String = (wfYY - 2).ToString
						Dim wfYYPrior3 As String = (wfYY - 3).ToString
						Dim wfYYPrior4 As String = (wfYY - 4).ToString
						Dim wfScenarioId As Integer = si.WorkflowClusterPk.ScenarioKey
						Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, wfScenarioId)
						Dim wfScenarioTypId As Integer = BRApi.Finance.Scenario.GetScenarioType(si, wfScenarioId).Id
						Dim wfTimeId As Integer = si.WorkflowClusterPk.TimeKey
						
						Dim wfCube As String = args.NameValuePairs("wfCube")
						Dim selectedMod As String = args.NameValuePairs("selectedMod")	
						Dim selectedModId As Integer = Brapi.Finance.Members.GetMemberId(si, dimtypeid.Flow, selectedMod)
						
						Dim std_FlowDim As String = "Std_Flow"
						Dim std_FlowDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, std_FlowDim)									
						Dim BudFm_FlowDim As OneStream.Shared.Wcf.Dim = BRApi.Finance.Dim.GetDim(si, std_FlowDim)						
																		
						'Get Whether selected mod is a descendant of OS, PC&I, R&D, etc.							
						'Is OS Descendant?
						Dim OS_ParentId As Integer = Brapi.Finance.Members.GetMemberId(si, dimtypeid.Flow, "USCG_OS_" & wfYY)
						Dim isOSdescendant As Boolean = BRApi.Finance.Members.IsDescendant(si, std_FlowDimPk, OS_ParentId, selectedModId)
								
						'Is PCI Descendant?
						Dim PCI_ParentId As Integer = Brapi.Finance.Members.GetMemberId(si, dimtypeid.Flow, "USCG_PCI_" & wfYY)
						Dim isPCIdescendant As Boolean = BRApi.Finance.Members.IsDescendant(si, std_FlowDimPk, PCI_ParentId, selectedModId)
						
						'Is RD Descendant?
						Dim RD_ParentId As Integer = Brapi.Finance.Members.GetMemberId(si, dimtypeid.Flow, "USCG_RD_" & wfYY)
						Dim isRDdescendant As Boolean = BRApi.Finance.Members.IsDescendant(si, std_FlowDimPk, RD_ParentId, selectedModId)
						
						'Is MERHCF Descendant
						Dim MERHCF_ParentId As Integer = Brapi.Finance.Members.GetMemberId(si, dimtypeid.Flow, "USCG_MERHCF_" & wfYY)
						Dim isMERHCFdescendant As Boolean = BRApi.Finance.Members.IsDescendant(si, std_FlowDimPk, MERHCF_ParentId, selectedModId)
						
						'Is RP Descendant?
						Dim RP_ParentId As Integer = Brapi.Finance.Members.GetMemberId(si, dimtypeid.Flow, "USCG_RP_" & wfYY)
						Dim isRPdescendant As Boolean = BRApi.Finance.Members.IsDescendant(si, std_FlowDimPk, RP_ParentId, selectedModId)
						
						'Is MOSP Descendant?
						Dim MOSP_ParentId As Integer = Brapi.Finance.Members.GetMemberId(si, dimtypeid.Flow, "USCG_MOSP_" & wfYY)
						Dim isMOSPdescendant As Boolean = BRApi.Finance.Members.IsDescendant(si, std_FlowDimPk, MOSP_ParentId, selectedModId)
						
						'Is F Descendant?
						Dim F_ParentId As Integer = Brapi.Finance.Members.GetMemberId(si, dimtypeid.Flow, "USCG_F_" & wfYY)
						Dim isFdescendant As Boolean = BRApi.Finance.Members.IsDescendant(si, std_FlowDimPk, F_ParentId, selectedModId)
						
						'Is BS Descendant?
						Dim BS_ParentId As Integer = Brapi.Finance.Members.GetMemberId(si, dimtypeid.Flow, "USCG_BS_" & wfYY)
						Dim isBSdescendant As Boolean = BRApi.Finance.Members.IsDescendant(si, std_FlowDimPk, BS_ParentId, selectedModId)	
										
						'Above Guidance
						'Is ABVOS Descendant?
						Dim ABVOS_ParentId As Integer = Brapi.Finance.Members.GetMemberId(si, dimtypeid.Flow, "USCG_ABVOS_" & wfYY)
						Dim isABVOSdescendant As Boolean = BRApi.Finance.Members.IsDescendant(si, std_FlowDimPk, ABVOS_ParentId, selectedModId)
						
						'Is ABVPCI Descendant?
						Dim ABVPCI_ParentId As Integer = Brapi.Finance.Members.GetMemberId(si, dimtypeid.Flow, "USCG_ABVPCI_" & wfYY)
						Dim isABVPCIdescendant As Boolean = BRApi.Finance.Members.IsDescendant(si, std_FlowDimPk, ABVPCI_ParentId, selectedModId)
						
						'Is ABVRD Descendant?
						Dim ABVRD_ParentId As Integer = Brapi.Finance.Members.GetMemberId(si, dimtypeid.Flow, "USCG_ABVRD_" & wfYY)
						Dim isABVRDdescendant As Boolean = BRApi.Finance.Members.IsDescendant(si, std_FlowDimPk, ABVRD_ParentId, selectedModId)
						
						'Is ABVMERHCF Descendant
						Dim ABVMERHCF_ParentId As Integer = Brapi.Finance.Members.GetMemberId(si, dimtypeid.Flow, "USCG_ABVMERHCF_" & wfYY)
						Dim isABVMERHCFdescendant As Boolean = BRApi.Finance.Members.IsDescendant(si, std_FlowDimPk, ABVMERHCF_ParentId, selectedModId)
						
						'Is ABVRP Descendant?
						Dim ABVRP_ParentId As Integer = Brapi.Finance.Members.GetMemberId(si, dimtypeid.Flow, "USCG_ABVRP_" & wfYY)
						Dim isABVRPdescendant As Boolean = BRApi.Finance.Members.IsDescendant(si, std_FlowDimPk, ABVRP_ParentId, selectedModId)
						
						'Is ABVMOSP Descendant?
						Dim ABVMOSP_ParentId As Integer = Brapi.Finance.Members.GetMemberId(si, dimtypeid.Flow, "USCG_ABVMOSP_" & wfYY)
						Dim isABVMOSPdescendant As Boolean = BRApi.Finance.Members.IsDescendant(si, std_FlowDimPk, ABVMOSP_ParentId, selectedModId)
						
						'Is ABVF Descendant?
						Dim ABVF_ParentId As Integer = Brapi.Finance.Members.GetMemberId(si, dimtypeid.Flow, "USCG_ABVF_" & wfYY)
						Dim isABVFdescendant As Boolean = BRApi.Finance.Members.IsDescendant(si, std_FlowDimPk, ABVF_ParentId, selectedModId)
						
						'Is ABVBS Descendant?
						Dim ABVBS_ParentId As Integer = Brapi.Finance.Members.GetMemberId(si, dimtypeid.Flow, "USCG_ABVBS_" & wfYY)
						Dim isABVBSdescendant As Boolean = BRApi.Finance.Members.IsDescendant(si, std_FlowDimPk, ABVBS_ParentId, selectedModId)
		
						'declare the memberfilter variable to populate depending on the appropriation type
						Dim memberFilter As New Text.StringBuilder
						
						Select Case True
						Case (isOSdescendant Or isMERHCFdescendant Or isRPdescendant Or isMOSPdescendant Or isFdescendant Or isBSdescendant Or isABVOSdescendant Or isABVMERHCFdescendant Or isABVRPdescendant Or isABVMOSPdescendant Or isABVFdescendant Or isABVBSdescendant)
							memberFilter.Append("F#FY" & wfYY & "_RPs.Base.Where((Text7 = '') OR (Text7 = " & selectedMod & "))")
						Case (isPCIdescendant Or isRDdescendant Or isFdescendant Or isABVPCIdescendant Or isABVRDdescendant Or isABVFdescendant)
							memberFilter.Append("F#FY" & wfYY & "_RPs.Base.Where((Text7 = '') OR (Text7 = " & selectedMod & ")),")	
							memberFilter.Append("F#FY" & wfYYPrior1 & "_RPs.Base.Where((Text7 = '') OR (Text7 = " & selectedMod & ")),")	
							memberFilter.Append("F#FY" & wfYYPrior2 & "_RPs.Base.Where((Text7 = '') OR (Text7 = " & selectedMod & ")),")
							memberFilter.Append("F#FY" & wfYYPrior3 & "_RPs.Base.Where((Text7 = '') OR (Text7 = " & selectedMod & ")),")
							memberFilter.Append("F#FY" & wfYYPrior4 & "_RPs.Base.Where((Text7 = '') OR (Text7 = " & selectedMod & ")),")
						End Select
						
						
						Dim rpMembers As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BudFm_FlowDim.DimPk, memberFilter.ToString, True)
							For Each rpMember As MemberInfo In rpMembers
								
								Dim rpNameVal As String = rpMember.Member.Name
								Dim memberScript As String = "S#" & wfScenario & ":T#" & wfTime & ":E#Total_Lead_Office:C#Aggregated:V#Annotation:A#None:F#" & rpNameVal & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"	
								Dim assignRPToModVal As String  = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, memberScript).DataCellEx.DataCellAnnotation								
								Dim rpVMProp As VaryingMemberProperties = BRApi.Finance.Members.ReadMemberPropertiesNoCache(si, Dimtype.Flow.Id, rpNameVal)
								Dim rpProp As FlowVMProperties = rpVMProp.GetFlowProperties
								Dim currentModAssignmentVal As String =	rpProp.Text7.GetStoredValue(wfScenarioTypId, wfTimeId)
								Dim RPText1 As String = rpProp.Text1.GetStoredValue(wfScenarioTypId, wfTimeId)'contains the Status
													
								'If prev mod assigned but now RP is set to No Data, clear the relationship
								If (Not String.IsNullOrWhiteSpace(currentModAssignmentVal) And String.IsNullOrWhiteSpace(assignRPToModVal))
									Dim relPkList As New List(Of RelationshipPk)									
									Dim memId As Integer = BRApi.Finance.Members.GetMemberId(si, std_FlowDimPk.DimTypeId, rpNameVal)
									Dim parentId As Integer = BRApi.Finance.Members.GetMemberId(si, std_FlowDimPk.DimTypeId, currentModAssignmentVal)
									Dim relPk As New RelationshipPk(std_FlowDimPk.DimTypeId, parentId, memId)
									relPkList.Add(relPk)
									'Remove the Mod-RP relationship
									Brapi.Finance.MemberAdmin.RemoveRelationships(si, std_FlowDimPk, relPkList, True)
									rpProp.Text7.RemoveStoredPropertyItem(wfScenarioTypId, wfTimeId)
									'remove the Mod name in the Text 7 value of the RP Member
									BRApi.Finance.MemberAdmin.SaveMemberInfo(si, False, rpMember.Member, True, rpVMProp, False, New List(Of MemberDescription), TriStateBool.FalseValue)		
								End If					
																			
								'If assignRPToModVal is set to Yes, then add the RP as a child of the selectedMod
								'removing the Status as the prior year RPs would not have a Status and need to be used for PC&I and R&D, etc.  The member list in the cube view should filter the appropriate RPs anyway
'								If (assignRPToModVal.XFEqualsIgnoreCase("Yes") And String.IsNullOrWhiteSpace(currentModAssignmentVal) And Not selectedMod.XFContainsIgnoreCase("USCG_ABV") And RPText1.XFContainsIgnoreCase("Status_03"))
								If (assignRPToModVal.XFEqualsIgnoreCase("Yes") And String.IsNullOrWhiteSpace(currentModAssignmentVal))
									'set the text 7 value to the selectedMod
									rpProp.Text7.SetStoredValue(wfScenarioTypId, wfTimeId, selectedMod)	
									BRApi.Finance.MemberAdmin.SaveMemberInfo(si, False, rpMember.Member, True, rpVMProp, False, New List(Of MemberDescription), TriStateBool.FalseValue)
									'Get the parent and Child Id's
									Dim ParentID As Integer = BRApi.Finance.Members.GetMemberId(si, std_FlowDimPk.DimTypeId, selectedMod)
									Dim childId As Integer = BRApi.Finance.Members.GetMemberId(si, std_FlowDimPk.DimTypeId, rpNameVal)	
									
									'Relationship
									Dim relPk As New RelationshipPk(std_FlowDimPk.DimTypeId, ParentID, childId)
									Dim rel As New Relationship(relPk, std_FlowDimPk.DimId, RelationshipMovementType.InsertAsLastSibling, 1)

									Dim relInfo As New RelationshipInfo(rel, Nothing)
									Dim relPosOpt As New RelationshipPositionOptions()
									
									'Save the Member Relationship
									BRApi.Finance.MemberAdmin.SaveRelationshipInfo(si, relInfo, relPosOpt)
									
									
								Else If (assignRPToModVal.XFEqualsIgnoreCase("Yes") And String.IsNullOrWhiteSpace(currentModAssignmentVal)  And selectedMod.XFContainsIgnoreCase("USCG_ABV") And RPText1.XFContainsIgnoreCase("Status_04"))
									'set the text 7 value to the selectedMod
									rpProp.Text7.SetStoredValue(wfScenarioTypId, wfTimeId, selectedMod)	
									BRApi.Finance.MemberAdmin.SaveMemberInfo(si, False, rpMember.Member, True, rpVMProp, False, New List(Of MemberDescription), TriStateBool.FalseValue)
									'Get the parent and Child Id's
									Dim ParentID As Integer = BRApi.Finance.Members.GetMemberId(si, std_FlowDimPk.DimTypeId, selectedMod)
									Dim childId As Integer = BRApi.Finance.Members.GetMemberId(si, std_FlowDimPk.DimTypeId, rpNameVal)	
									
									'Relationship
									Dim relPk As New RelationshipPk(std_FlowDimPk.DimTypeId, ParentID, childId)
									Dim rel As New Relationship(relPk, std_FlowDimPk.DimId, RelationshipMovementType.InsertAsLastSibling, 1)

									Dim relInfo As New RelationshipInfo(rel, Nothing)
									Dim relPosOpt As New RelationshipPositionOptions()
									
									'Save the Member Relationship
									BRApi.Finance.MemberAdmin.SaveRelationshipInfo(si, relInfo, relPosOpt)
									
								End If
								
							Next
			Return Nothing
		End Function
		Private Function SaveRPStatusWithComments() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.SaveRPStatusWithComments ====
			'{BudFM_SolutionHelper}{SaveRPStatusWithComments}{WFTime=[|WFTime|],ChangeText=[|!prm_Description_ChangeLog_ADM!|],ChangeReason=[|!prm_Reason_ChangeLog_ADM!|], WFScenario=[|WFScenario|], WFCube=[|WFCube|], RPListFilter=[|!prm_EditRp_SaveComments_RPList_ADM!|]}
					
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
					Dim RPlistFilter As String = args.NameValuePairs("RPListFilter")
					
					Dim ChangeText As String = args.NameValuePairs("ChangeText")
					Dim ChangeReason As String = args.NameValuePairs("ChangeReason")
					'BRApi.ErrorLog.LogMessage(si, ChangeText & vbCrLf & ChangeReason)
					
					Dim scriptGenerics As String = "E#NA:S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"	

					'Create a new list of memberscript and value
					Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)
					
					RPlistFilter = RPlistFilter.Replace(" ","")
					Dim arr() As String = RPlistFilter.Split(",")
					Dim vals As List (Of String) = arr.ToList()
					Dim newRPlistFilter As String = ""
					
					For Each RP As String In vals
						If newRPlistFilter = "" Then
							newRPlistFilter = "F#" & rp
						Else
							If Not newRPlistFilter.Contains(RP) Then
								newRPlistFilter = newRPlistFilter & ",F#" & RP
							End If 
						End If 	
					Next	
						
					Dim RPList As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Std_Flow" , newRPlistFilter,True)							 

					For Each RPInfo As MemberInfo In RPList
					
					'For Each RP As String In vals
					
    					Dim RPName As String = RPInfo.Member.Name
    					Dim RPDescription As String =  String.Empty
					    Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)
						
						Dim scriptGenerics_Status As String = scriptGenerics & ":A#RPStatus"		& ":F#" & RPName
						Dim scriptGenerics_Status_Clog As String = "E#" & RP_Entity & ":F#" & RPName & ":S#" & wfScenario & ":T#" & wfTime & ":A#Description_Changelog:V#Annotation:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#RP_Status_Change:U7#None:U8#" & ChangeReason
						
						Dim scriptGenerics_BudCat As String	= scriptGenerics & ":A#RPBudCatDescr" 	& ":F#" & RPName
						Dim scriptGenerics_BudCat_Clog As String ="E#" & RP_Entity & ":F#" & RPName &":S#" & wfScenario & ":T#" & wfTime & ":A#Description_Changelog:V#Annotation:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#RP_BudCat_Change:U7#None:U8#" & ChangeReason
						
						Dim updatedStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, scriptGenerics_Status).DataCellEx.DataCellAnnotation
						Dim updatedBudCat As String =  BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, scriptGenerics_BudCat).DataCellEx.DataCellAnnotation
						
						' Make update to RP if any of the below have values , otherwise we have nothing to do for this RP
						If  ((Not updatedStatus = "") Or
							 (Not updatedBudCat = "")) Then
							 
							Dim BudFm_FlowDim As OneStream.Shared.Wcf.Dim = BRApi.Finance.Dim.GetDim(si, "Std_Flow")
							Dim RPId As Integer = RPInfo.Member.MemberId
							Dim RPPk As New MemberPk(BudFm_FlowDim.DimPk.DimTypeId, RPId)
							Dim RPMemberInfo As MemberInfo = BRApi.Finance.Members.GetMemberInfo(si, dimtypeid.Flow, RPId, True)
							RPDescription = RPInfo.Member.Description
							
							' Set all the necessary objects to change flow properties
							Dim RPToUpdate As New Member(RPPk,RPName,RPDescription,BudFm_FlowDim.DimPk.DimId)
							Dim RPVarProps As VaryingMemberProperties = RPMemberInfo.Properties
							Dim RPToUpdateInfo As New MemberInfo(RPToUpdate,RPVarProps,Nothing,BudFm_FlowDim, DimConstants.Unknown)
							Dim RPMemberProperties As FlowVMProperties = RPToUpdateInfo.GetFlowProperties()

							Dim wfPk As WorkflowUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si)
							Dim ScenarioTypeId As Integer = BRApi.Finance.Scenario.GetScenarioType(si, wfPk.ScenarioKey).Id
							

							' Get the current Text1 value and extact individual items							
							Dim CurrentText1Value As String = RPMemberProperties.Text1.GetStoredValue(ScenarioTypeId, wfPk.TimeKey)
							Dim CurrentText1Split As List(Of String) = StringHelper.SplitString(CurrentText1Value,"|")		
							Dim CurrentStatus As String  = CurrentText1Split(0)
							Dim CurrMode As String  = CurrentText1Split(1)
							Dim CurrCCReq As String = CurrentText1Split(2)
							
							' Set the placeholders construct new text 1 value 
							Dim NewText1Value As String = ""
							Dim NewStatus As String = ""

							' Get updated status. If it is not empty set it to new status  and clear the cell annotation
							If (Not updatedStatus = "") Then
								NewStatus = updatedStatus
'								RPMemberProperties.Text1.SetStoredValue(DimConstants.Unknown, DimConstants.Unknown, updatedStatus)
								lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, scriptGenerics_Status, 0, True, ""))
								
								lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, scriptGenerics_Status_Clog , 0, True, ChangeText))
									
							Else
								NewStatus = CurrentStatus								
							End If
							
							' Construct the new Text1 value and set it 
							NewText1Value = NewStatus & "|" & CurrMode & "|" & CurrCCReq					
							RPMemberProperties.Text1.SetStoredValue(ScenarioTypeId, wfPk.TimeKey,  NewText1Value)
							
							' Get the current Text8 value and extact individual items							
							Dim CurrentText8Value As String = RPMemberProperties.Text8.GetStoredValue(DimConstants.Unknown,DimConstants.Unknown)
							Dim CurrentText8Split As List(Of String) = StringHelper.SplitString(CurrentText8Value,"_")		
							Dim CurrentSplit0 As String  = CurrentText8Split(0)
							Dim CurrentSplit1 As String  = CurrentText8Split(1)
							Dim CurrentSplit2 As String  = CurrentText8Split(2)
							Dim CurrentSplit3 As String  = CurrentText8Split(3)
							Dim CurrentSplit4 As String  = CurrentText8Split(4)
							Dim CurrentBudCat As String  = CurrentText8Split(5)
							Dim CurrentSplit6 As String  = CurrentText8Split(6)
							Dim CurrentSplit7 As String  = CurrentText8Split(7)
							
							' Set the placeholders construct new text 8 value 
							Dim NewText8Value As String = ""
							Dim NewBudCat As String = ""
							
							' Get updated BudCat. If it is not empty set it to new BudCat  and clear the cell annotation
							If (Not updatedBudCat = "") Then
								NewBudCat = updatedBudCat
			                    lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, scriptGenerics_BudCat, 0, True, ""))
								
								lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, scriptGenerics_BudCat_Clog , 0, True, ChangeText))
							
							Else
								NewBudCat = CurrentBudCat								
							End If
							
							' Construct the new Text8 value and set it 
							NewText8Value = CurrentSplit0 & "_" & CurrentSplit1 & "_" & CurrentSplit2 & "_" & CurrentSplit3 & "_" & CurrentSplit4 & "_" & NewBudCat & "_" & CurrentSplit6 & "_" & CurrentSplit7 			
							RPMemberProperties.Text8.SetStoredValue(DimConstants.Unknown,DimConstants.Unknown,NewText8Value)
							
							'Save (We need To Set the isNew flag To False As we are Not creating a New RP here)
							BRApi.Finance.MemberAdmin.SaveMemberInfo(si, RPToUpdateInfo, True, True, False, False)
						
						End If
					Next

					'Write the annotations to the database
					Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)	
					
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Reason_ChangeLog_ADM", "")
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Description_ChangeLog_ADM", "")
					
					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = True
					selectionChangedTaskResult.IsOK = True
					
					
					Return selectionChangedTaskResult
					
										
			Return Nothing
		End Function
		Private Function SearchRPsandSetDashboard() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.SearchRPsandSetDashboard ====
							Dim SearchKeyword As String = args.NameValuePairs("SearchQuery")
							Dim Content As String = args.NameValuePairs("Content")
							
							Brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prmRPSearchQuery", SearchKeyword)
							
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							Dim routingAppn As String = NormalizeRoutingAppn(args.NameValuePairs.XFGetValue("APPN_Content", "OS"))
							SetRoutingContent(selectionChangedTaskResult.ModifiedCustomSubstVars, routingAppn, Content)
							selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
							Return selectionChangedTaskResult
			Return Nothing
		End Function
		Private Function SetDynamicParameters() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.SetDynamicParameters ====
				Dim ButtonName As String = args.NameValuePairs("Button")
				
				
				
				If ButtonName = "btn_ViewAllBillets_OS" Then
					BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prm_Content_AddEditBillets_NonEditRP_OS", "OS_Billets_Main_04c1b") 

				End If 
				
				If ButtonName = "btn_CreateNewBillet_OS" Then
				BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prm_Content_AddEditBillets_NonEditRP_OS", "OS_Billets_Main_04c1a") 

				End If 
				
				If ButtonName = "btn_LeaseUtility_Report_OS" Then
				BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prm_Content_AddEditBillets_NonEditRP_OS", "OS_Billets_Main_04c1d") 

				End If 
				
				If ButtonName = "btn_UpdateAllBillets_OS" Then
				BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prm_Content_AddEditBillets_NonEditRP_OS", "OS_Billets_Main_04c1c") 

				End If 
				
				
			Return Nothing
		End Function
		Private Function SetLiteralParamValue() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.SetLiteralParamValue ====
							Dim SearchKeyword As String = args.NameValuePairs("SearchQuery")
							Brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prmRPSearchQuery", SearchKeyword)
			Return Nothing
		End Function
		Private Function SetPPADefaults() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.SetPPADefaults ====

			'find the selected member from the first combo box and update its Text property with the multi-selected values from the second combo box
			Dim wfTime As String = args.NameValuePairs("WFTime")
			Dim wfScenario As String = args.NameValuePairs("WFScenario")
			Dim wfCube As String = args.NameValuePairs("WFCube")
			Dim UDDim As Integer = args.NameValuePairs("UDDim")
			'Adding the value of 8 to UDDim is a way of dynamically converting the UD1 through UD8 dimension
			'parameter value into the DimTypeId stored In the application database Dim table
			'   reference: select DimTypeId, DimId, Name from dbo.Dim order by 1,2
			Dim UDDimType As Integer = UDDim + 8
			Dim UDMbr As String = args.NameValuePairs("UDMbr")
			Dim TextProperty As String = args.NameValuePairs("TextProperty")
			Dim NewTextValue As String = args.NameValuePairs("NewTextValue")
			
			Dim xMbrId As Integer = BRApi.Finance.Members.GetMemberId(si, UDDimType, UDMbr)
			Dim xMbrInfo As MemberInfo = BRApi.Finance.Members.GetMemberInfo(si, UDDimType, xMbrId, True)
			Dim xMbrProperties As UDVMProperties = xMbrInfo.GetUDProperties()

			If TextProperty = 1 Then
				xMbrProperties.Text1.SetStoredValue(DimConstants.Unknown, DimConstants.Unknown, NewTextValue)
			ElseIf TextProperty = 2 Then
				xMbrProperties.Text2.SetStoredValue(DimConstants.Unknown, DimConstants.Unknown, NewTextValue)
			ElseIf TextProperty = 3 Then
				xMbrProperties.Text3.SetStoredValue(DimConstants.Unknown, DimConstants.Unknown, NewTextValue)
			ElseIf TextProperty = 4 Then
				xMbrProperties.Text4.SetStoredValue(DimConstants.Unknown, DimConstants.Unknown, NewTextValue)
			ElseIf TextProperty = 5 Then
				xMbrProperties.Text5.SetStoredValue(DimConstants.Unknown, DimConstants.Unknown, NewTextValue)
			ElseIf TextProperty = 6 Then
				xMbrProperties.Text6.SetStoredValue(DimConstants.Unknown, DimConstants.Unknown, NewTextValue)
			ElseIf TextProperty = 7 Then
				xMbrProperties.Text7.SetStoredValue(DimConstants.Unknown, DimConstants.Unknown, NewTextValue)
			ElseIf TextProperty = 8 Then
				xMbrProperties.Text8.SetStoredValue(DimConstants.Unknown, DimConstants.Unknown, NewTextValue)
			End If

			Return Nothing
		End Function
		Private Function SetRPStatus() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.SetRPStatus ====
			
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
					Dim RPlistFilter As String = args.NameValuePairs("RPListFilter")	
										
					
					Dim scriptGenerics As String = "E#NA:S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"	

					'Create a new list of memberscript and value
					Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)

					Dim RPList As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Std_Flow" , RPListFilter,True)							 

					For Each RPInfo As MemberInfo In RPList
					
    					Dim RPName As String = RPInfo.Member.Name
    					Dim RPDescription As String =  String.Empty
					
						Dim scriptGenerics_Title As String 	= scriptGenerics & ":A#RPTitle" 		& ":F#" & RPName				
						Dim scriptGenerics_Status As String = scriptGenerics & ":A#RPStatus"		& ":F#" & RPName	
						Dim scriptGenerics_Mode As String	= scriptGenerics & ":A#RPMode"			& ":F#" & RPName	
						Dim scriptGenerics_CCReq As String	= scriptGenerics & ":A#RPCCComentReq" 	& ":F#" & RPName
						Dim scriptGenerics_BudCat As String	= scriptGenerics & ":A#RPBudCatDescr" 	& ":F#" & RPName
						Dim scriptGenerics_RPFundingAvailability As String	= scriptGenerics & ":A#RPFundingAvailability" 	& ":F#" & RPName

						Dim updatedTitle As String =  BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, scriptGenerics_Title).DataCellEx.DataCellAnnotation
						Dim updatedStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, scriptGenerics_Status).DataCellEx.DataCellAnnotation
						Dim updatedMode As String =   BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, scriptGenerics_Mode).DataCellEx.DataCellAnnotation
						Dim updatedCCReq As String =  BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, scriptGenerics_CCReq).DataCellEx.DataCellAnnotation
						Dim updatedBudCat As String =  BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, scriptGenerics_BudCat).DataCellEx.DataCellAnnotation
						Dim updatedFundingAvailability As String =  BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, scriptGenerics_RPFundingAvailability).DataCellEx.DataCellAnnotation
						
						' Make update to RP if any of the below have values , otherwise we have nothing to do for this RP
						If  ((Not updatedTitle = "")  Or 
							 (Not updatedStatus = "") Or
							 (Not updatedMode = "")   Or
							 (Not updatedCCReq = "")  Or
							 (Not updatedBudCat = "") Or
							 (Not updatedFundingAvailability = "")) Then

							Dim BudFm_FlowDim As OneStream.Shared.Wcf.Dim = BRApi.Finance.Dim.GetDim(si, "Std_Flow")
							Dim RPId As Integer = RPInfo.Member.MemberId
							Dim RPPk As New MemberPk(BudFm_FlowDim.DimPk.DimTypeId, RPId)
							Dim RPMemberInfo As MemberInfo = BRApi.Finance.Members.GetMemberInfo(si, dimtypeid.Flow, RPId, True)

							' Get updated RP Title. If it is not empty set it as DEscription and and clear the cell annotation
							If (Not updatedTitle = "") Then
								RPDescription = updatedTitle
								lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, scriptGenerics_Title, 0, True, ""))		
							Else
								RPDescription = RPInfo.Member.Description
							End If
								
							' Set all the necessary objects to change flow properties
							Dim RPToUpdate As New Member(RPPk,RPName,RPDescription,BudFm_FlowDim.DimPk.DimId)
							Dim RPVarProps As VaryingMemberProperties = RPMemberInfo.Properties
							Dim RPToUpdateInfo As New MemberInfo(RPToUpdate,RPVarProps,Nothing,BudFm_FlowDim, DimConstants.Unknown)
							Dim RPMemberProperties As FlowVMProperties = RPToUpdateInfo.GetFlowProperties()

							Dim wfPk As WorkflowUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si)
							Dim ScenarioTypeId As Integer = BRApi.Finance.Scenario.GetScenarioType(si, wfPk.ScenarioKey).Id
							

							' Get the current Text1 value and extact individual items
							Dim CurrentText1Value As String = RPMemberProperties.Text1.GetStoredValue(ScenarioTypeId, wfPk.TimeKey)
							
							If Not CurrentText1Value.Contains("|") Then
								
								'do nothing
							
							Else
								
								Dim CurrentText1Split As List(Of String) = StringHelper.SplitString(CurrentText1Value,"|")		
								Dim CurrentStatus As String  = CurrentText1Split(0)
								Dim CurrMode As String  = CurrentText1Split(1)
								Dim CurrCCReq As String = CurrentText1Split(2)
								
								' Set the placeholders construct new text 1 value 
								Dim NewText1Value As String = ""
								Dim NewStatus As String = ""
								Dim NewMode As String = ""
								Dim NewCCReq As String = ""
								
								' Get updated status. If it is not empty set it to new status  and clear the cell annotation
								If (Not updatedStatus = "") Then
									NewStatus = updatedStatus
	'								RPMemberProperties.Text1.SetStoredValue(DimConstants.Unknown, DimConstants.Unknown, updatedStatus)
									lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, scriptGenerics_Status, 0, True, ""))
								Else
									NewStatus = CurrentStatus								
								End If

								' Get updated mode. If it is not empty set it to new mode  and clear the cell annotation
								If (Not updatedMode = "") Then
									NewMode = updatedMode
	'								RPMemberProperties.Text2.SetStoredValue(DimConstants.Unknown, DimConstants.Unknown, updatedMode)
									lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, scriptGenerics_Mode, 0, True, ""))
								Else
									NewMode = CurrMode
								End If
								
								' Get updated CCReq value. If it is not empty set it to new CCReq and clear the cell annotation
								If (Not updatedCCReq = "") Then
									NewCCReq = updatedCCReq
	'								RPMemberProperties.Text3.SetStoredValue(DimConstants.Unknown, DimConstants.Unknown, updatedCCReq)
									lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, scriptGenerics_CCReq, 0, True, ""))
								Else
									NewCCReq = CurrCCReq								
								End If
								
								' Construct the new Text1 value and set it 
								NewText1Value = NewStatus & "|" & NewMode & "|"	& NewCCReq					
								RPMemberProperties.Text1.SetStoredValue(ScenarioTypeId, wfPk.TimeKey,  NewText1Value)
							
							End If 
							
							' Get the current Text2 value and extact individual items							
							Dim CurrentFundingAvailability As String = RPMemberProperties.Text2.GetStoredValue(DimConstants.Unknown, DimConstants.Unknown)
'							
							' Set the placeholders construct new text 2 value 
							Dim NewText2Value As String = ""
							Dim NewFundingAvailability As String = ""
							
							' Get updated Funding Availability. If it is not empty set it to new Funding Availability  and clear the cell annotation
							If (Not updatedFundingAvailability = "") Then
								NewFundingAvailability = updatedFundingAvailability
			                    lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, scriptGenerics_RPFundingAvailability, 0, True, ""))
							Else
								NewFundingAvailability = CurrentFundingAvailability								
							End If
							
							
							' Construct the new Text2 value and set it 
							NewText2Value = NewFundingAvailability			
							RPMemberProperties.Text2.SetStoredValue(DimConstants.Unknown,DimConstants.Unknown,NewText2Value)
							
							' Get the current Text8 value and extact individual items							
							Dim CurrentText8Value As String = RPMemberProperties.Text8.GetStoredValue(DimConstants.Unknown,DimConstants.Unknown)
							Dim CurrentText8Split As List(Of String) = StringHelper.SplitString(CurrentText8Value,"_")		
							Dim CurrentSplit0 As String  = CurrentText8Split(0)
							Dim CurrentSplit1 As String  = CurrentText8Split(1)
							Dim CurrentSplit2 As String  = CurrentText8Split(2)
							Dim CurrentSplit3 As String  = CurrentText8Split(3)
							Dim CurrentSplit4 As String  = CurrentText8Split(4)
							Dim CurrentBudCat As String  = CurrentText8Split(5)
							Dim CurrentSplit6 As String  = CurrentText8Split(6)
							Dim CurrentSplit7 As String  = CurrentText8Split(7)
							
							' Set the placeholders construct new text 8 value 
							Dim NewText8Value As String = ""
							Dim NewBudCat As String = ""
							
							' Get updated BudCat. If it is not empty set it to new BudCat  and clear the cell annotation
							If (Not updatedBudCat = "") Then
								NewBudCat = updatedBudCat
			                    lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, scriptGenerics_BudCat, 0, True, ""))
							Else
								NewBudCat = CurrentBudCat								
							End If
							
							' Construct the new Text8 value and set it 
							NewText8Value = CurrentSplit0 & "_" & CurrentSplit1 & "_" & CurrentSplit2 & "_" & CurrentSplit3 & "_" & CurrentSplit4 & "_" & NewBudCat & "_" & CurrentSplit6 & "_" & CurrentSplit7 			
							RPMemberProperties.Text8.SetStoredValue(DimConstants.Unknown,DimConstants.Unknown,NewText8Value)

							'Save (We need To Set the isNew flag To False As we are Not creating a New RP here)
							BRApi.Finance.MemberAdmin.SaveMemberInfo(si, RPToUpdateInfo, True, True, False, False)
						
						End If
					Next

					'Write the annotations to the database
					Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)				
					
										
			Return Nothing
		End Function
		Private Function StaffSymbolConcReview_AddStaffSymbol() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.StaffSymbolConcReview_AddStaffSymbol ====
			Dim wfCube As String = args.NameValuePairs("WFCube")
			Dim wfTime As String = args.NameValuePairs("WFTime")
			Dim wfScenario As String = args.NameValuePairs("WFScenario")
			Dim wfProfile As String = args.NameValuePairs("WFProfile")
			Dim RPAppr As String = args.NameValuePairs("RPAppr")
			Dim RPName As String = args.NameValuePairs("RPName")
			Dim StaffSymbol As String = args.NameValuePairs("StaffSymbol")
			Dim RPEntity As String = rpUtils.Get_RP_Entity(si, RPName)
			Dim wfTimeYY = rpUtils.Get_WFTime_YY(si, wfTime)
			
			'when the lead office was saved on EditRP-Page2 logic to A#Lead_Office1/U8#None, it was also saved to A#StaffSymbol_ConcReview/Comment_01
			Dim GetAnnotationScriptGenerics As String = "E#" & RPEntity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":C#Local:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Comment_01"
			Dim LeadOffice1 As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#StaffSymbol_ConcReview:" + GetAnnotationScriptGenerics).DataCellEx.DataCellAnnotation
			Dim LeadOfficeConcursFlag As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#C_N__ConcReview:" + GetAnnotationScriptGenerics).DataCellEx.DataCellAnnotation
			
			If Not LeadOfficeConcursFlag = "C" Then
				Throw New Exception("Lead Office must concur before adding a new Office")
			Else
				Dim povInfo As New Dictionary(Of String, String) 
				povInfo.Add("Cube", wfCube)
				povInfo.Add("Time", wfTime)
				povInfo.Add("Scenario", wfScenario)
				povInfo.Add("Profile", wfProfile)
				povInfo.Add("Appr", RPAppr)
				povInfo.Add("Name", RPName)
				povInfo.Add("Entity", RPEntity)
				povInfo.Add("StaffSymbol", StaffSymbol)
				
				globals.SetStringValue("WFProfile", wfProfile)
				globals.SetStringValue("Appr", RPAppr)
				globals.SetStringValue("Name", RPName)
				globals.SetStringValue("StaffSymbol", StaffSymbol)
				BRApi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si, FINANCE_CALC_RULE, "AddStaffSymbolRow", povInfo, 0)
			End If
			
			Return Nothing
		End Function
		Private Function StaffSymbolConcReview_AutoPopulate() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.StaffSymbolConcReview_AutoPopulate ====
		Dim wfCube As String = args.NameValuePairs("WFCube")
		Dim wfTime As String = args.NameValuePairs("WFTime")
		Dim wfScenario As String = args.NameValuePairs("WFScenario")
		Dim wfProfile As String = args.NameValuePairs("WFProfile")
		Dim RPAppr As String = args.NameValuePairs("RPAppr")
		Dim RPName As String = args.NameValuePairs("RPName")
		Dim RPEntity As String = rpUtils.Get_RP_Entity(si, RPName)
		Dim wfTimeYY = rpUtils.Get_WFTime_YY(si, wfTime)
		
		
		'if the year is >=2028
		If CInt(wfTime) >=2028 Then
			
			'Get ccrTF  (True or False) to determine if auto-populate has been run or not.
			Dim ccrTF As String = "E#" & RPEntity & ":S#" & wfScenario & ":T#" & wfTime & ":A#CCR_TF:V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim ccrTFValue As String = String.Empty
			Dim ccrdataCell As DataCellInfoUsingMemberScript = brapi.Finance.Data.GetDataCellUsingMemberScript(si, wfcube, ccrTF)
		
				'If ccr has been run (True), don't run autopopulate, stop code and print message
				If  ccrDataCell IsNot Nothing AndAlso ccrDataCell.DataCellEx IsNot Nothing AndAlso ccrdataCell.DataCellEx.DataCell IsNot Nothing Then
					ccrTFValue = ccrDataCell.DataCellEx.DataCellAnnotation
					
				End If
				
				If ccrTFValue.XFEqualsIgnoreCase("True") Then				
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					selectionChangedTaskResult.IsOK = True
					selectionChangedTaskResult.ShowMessageBox = True
					selectionChangedTaskResult.Message = "CCR Auto-population has already been run. To avoid potential data issues, the system prevents users from re-running Auto-population. To add new offices, please add them manually."
					Return selectionChangedTaskResult
					
				Else
				'when the lead office was saved on EditRP-Page2 logic to A#Lead_Office1/U8#None, it was also saved to A#StaffSymbol_ConcReview/Comment_01
				Dim GetAnnotationScriptGenerics As String = "E#" & RPEntity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":C#Local:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Comment_01"
				Dim LeadOfficeConcursFlag As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#C_N__ConcReview:" + GetAnnotationScriptGenerics).DataCellEx.DataCellAnnotation
				
					If Not LeadOfficeConcursFlag = "C" Then
						Throw New Exception("Lead Office must concur before auto-populating Offices")
					Else
						Dim povInfo As New Dictionary(Of String, String) 
						povInfo.Add("Cube", wfCube)
						povInfo.Add("Time", wfTime)
						povInfo.Add("Scenario", wfScenario)
						povInfo.Add("Profile", wfProfile)
						povInfo.Add("Appr", RPAppr)
						povInfo.Add("Name", RPName)
						povInfo.Add("Entity", RPEntity)
						
						globals.SetStringValue("WFProfile", wfProfile)
						globals.SetStringValue("Appr", RPAppr)
						globals.SetStringValue("Name", RPName)
						BRApi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si, FINANCE_CALC_RULE, "AutoPopulateStaffSymbolsNew", povInfo, 0) 'customcalculatetimetype.MemberFilter
					
						Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)
						RunPostSaveStepsForRP(globals, si, wfcube, RP_Entity, wfscenario, wftime, RPName)
						
						'When StaffSymbolConcReview_AutoPopulate is run the first time, set A#CCR_TF to True
						Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#CCR_TF:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"	
						
						'Create a new list of memberscript and value
						Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)
						
						'Add the member scripts to the list and store as "True" annotation
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#CCR_TF:" & scriptGenerics, 0, True, "True"))
						
						Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)
						
						Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
						selectionChangedTaskResult.IsOK = True
						selectionChangedTaskResult.ShowMessageBox = True
						selectionChangedTaskResult.Message = "The lead office list has been auto-populated based on your calculated RP. If expected lead offices did not populate, " & _
						"please navigate To Edit RP Page 1, Calculate the RP, And Return To Concurrent Review To press the auto-population button again." & Environment.NewLine & Environment.NewLine & _
						"Once complete, additional offices can be added via the Add New Office button."
						selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = False
						Return selectionChangedTaskResult
								
						
				End If
			End If
		Else
					
			'Pre-2028
			'when the lead office was saved on EditRP-Page2 logic to A#Lead_Office1/U8#None, it was also saved to A#StaffSymbol_ConcReview/Comment_01
			Dim GetAnnotationScriptGenerics As String = "E#" & RPEntity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":C#Local:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Comment_01"
			Dim LeadOfficeConcursFlag As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#C_N__ConcReview:" + GetAnnotationScriptGenerics).DataCellEx.DataCellAnnotation
			
			If Not LeadOfficeConcursFlag = "C" Then
				Throw New Exception("Lead Office must concur before auto-populating Offices")
			Else
				Dim povInfo As New Dictionary(Of String, String)
				povInfo.Add("Cube", wfCube)
				povInfo.Add("Time", wfTime)
				povInfo.Add("Scenario", wfScenario)
				povInfo.Add("Profile", wfProfile)
				povInfo.Add("Appr", RPAppr)
				povInfo.Add("Name", RPName)
				povInfo.Add("Entity", RPEntity)
				
				globals.SetStringValue("WFProfile", wfProfile)
				globals.SetStringValue("Appr", RPAppr)
				globals.SetStringValue("Name", RPName)
				BRApi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si, FINANCE_CALC_RULE, "AutoPopulateStaffSymbolsNew", povInfo, 0)'customcalculatetimetype.MemberFilter
			
				Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)
				RunPostSaveStepsForRP(globals, si, wfcube, RP_Entity, wfscenario, wftime, RPName)
				
				Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
				selectionChangedTaskResult.IsOK = True
				selectionChangedTaskResult.ShowMessageBox = True
				selectionChangedTaskResult.Message = "The lead office list has been auto-populated based on your calculated RP. If expected lead offices did not populate, " & _
				"please navigate To Edit RP Page 1, Calculate the RP, And Return To Concurrent Review To press the auto-population button again." & Environment.NewLine & Environment.NewLine & _
				"Once complete, additional offices can be added via the Add New Office button."
				selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = False
				Return selectionChangedTaskResult
			End If
		End If	
			Return Nothing
		End Function
		Private Function UpdateRPsWithComment() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.UpdateRPsWithComment ====
				
				Dim wfTime As String = args.NameValuePairs("wfTime")
				
				Dim wfCube As String = args.NameValuePairs("wfCube")
				
				Dim wfScenario As String = args.NameValuePairs("wfScenario")
				
				Dim RPlistFilter As String = args.NameValuePairs("RPListFilter")
				
				Dim scriptGenerics As String = "E#NA:S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"	
			
		    	Dim selectionChangeTaskResult As New XFSelectionChangedTaskResult
			
				'Create a list of memberscript and value (RPs in the cube view)
				'Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)
				
				Dim RPList As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Std_Flow", RPListFilter , True)
				
				Dim RPStatusUpdateList As New List (Of String)
				
				Dim RPBudCatUpdateList As New List (Of String)
				
				Dim RPNamesStatusUpdatedstrt As String = Environment.NewLine & "RP Status Updates: " 
				Dim RPNamesStatusUpdated As String = ""
				Dim RPNamesStatusUpdatedcmbined As String = ""
				
				Dim RPNamesBudCatUpdatedstrt As String = Environment.NewLine & "BudCat Updates: "
				Dim RPNamesBudCatUpdated As String = ""
				Dim RPNamesBudCatUpdatedcmbined As String = ""
				
				'Dim statusUpdatesFound As Boolean = False
				
				'Dim budCatUpdatesFound As Boolean = False
								
				'Get the RPs and any updates made, from the cube view.
				For Each RPInfo As MemberInfo In RPList
				
					Dim RPName As String = RPInfo.Member.Name
					
					Dim scriptGenerics_Status As String = scriptGenerics & ":A#RPStatus"		& ":F#" & RPName
					
					Dim scriptGenerics_BudCat As String	= scriptGenerics & ":A#RPBudCatDescr" 	& ":F#" & RPName			
									
					Dim updatedStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, scriptGenerics_Status).DataCellEx.DataCellAnnotation
					
					Dim updatedBudCat As String =  BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, scriptGenerics_BudCat).DataCellEx.DataCellAnnotation
					
						
					If Not String.IsNullOrEmpty(updatedStatus) OrElse Not String.IsNullOrEmpty(updatedBudCat) Then
									
						If Not String.IsNullOrEmpty(updatedStatus) Then	
							
							'statusUpdatesFound = True
							
							If RPNamesStatusUpdated = "" Then
								RPNamesStatusUpdated = RPName
							Else
								RPNamesStatusUpdated = RPNamesStatusUpdated & ", " & RPName
							End If
								
						End If 
								
						If Not String.IsNullOrEmpty(updatedBudCat) Then

							'budCatUpdatesFound = True
									
							If RPNamesBudCatUpdated = "" Then
								RPNamesBudCatUpdated = RPName									
							Else
								RPNamesBudCatUpdated = RPNamesBudCatUpdated & ", " & RPName
							End If
						End If
					End If 
				Next		
												
				If String.IsNullOrEmpty(RPNamesStatusUpdated) AndAlso String.IsNullOrEmpty(RPNamesBudCatUpdated)Then
					Throw New Exception("No RP Status or Budget Category updates were made. Please choose the updates to be saved.") 
				End If

				selectionChangeTaskResult.IsOK = True
				selectionChangeTaskResult.ShowMessageBox = True
				selectionChangeTaskResult.ChangeCustomSubstVarsInDashboard = True
				
				RPNamesStatusUpdatedcmbined = RPNamesStatusUpdatedstrt & RPNamesStatusUpdated
				RPNamesBudCatUpdatedcmbined = RPNamesBudCatUpdatedstrt & RPNamesBudCatUpdated
				
				Dim rplstupdated As String = RPNamesStatusUpdated & "," & RPNamesBudCatUpdated
				
				BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prm_EditRp_SaveComments_RPStatus_ADM", RPNamesStatusUpdatedcmbined)
 
				BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prm_EditRp_SaveComments_BudCat_ADM", RPNamesBudCatUpdatedcmbined)				

				BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prm_EditRp_SaveComments_RPList_ADM", rplstupdated)
				
				Return selectionChangeTaskResult
	
			Return Nothing
		End Function
		Private Function UploadSupportingDoc() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.UploadSupportingDoc ====

			'get the file the user uploaded and define the fileshare prefix
			Dim userSelectedFilePath As String = args.NameValuePairs.XFGetValue("FilePath")
			'Get Time from current Workflow
			Dim wfTime As String = args.NameValuePairs("WFTime")
			Dim wfScenario As String = args.NameValuePairs("WFScenario")
			Dim wfCube As String = args.NameValuePairs("WFCube")
			Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
			Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					

			Dim LineItemNum As String = args.NameValuePairs("LineItemNum") 
			Dim fileSharePrefix As String = "C:\OneStreamShare\FileShare"
			
			'First, check if the RP is in Edit Mode or View Only Mode 
			If Not rputils.Is_RP_Editable(si, RPName) 				
					'Mode is view only so do nothing and show the user a message that states its in view only mode
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					selectionChangedTaskResult.IsOK = True
					selectionChangedTaskResult.ShowMessageBox = True
					selectionChangedTaskResult.Message = "" & GetDescription(si,RPName) & " Is set to View Only.  No edits can be made."
					Return selectionChangedTaskResult	
												
			Else 'Mode is edit so udpate the RP		
								
	'			'get the file to confirm it exists
				Dim fileContent As XFFileEx = BRApi.FileSystem.GetFile(si, fileSystemLocation.FileShare, userSelectedFilePath, True, False, False, SharedConstants.Unknown, Nothing, True)
		
				If fileContent Is Nothing Then
					
					'log to the error log if the file is not found
					'brapi.ErrorLog.LogMessage(si, "File Not Found")
					
				Else
				
					Using dt As DataTable = GetSupportDocDataTableCV(si, True)
						Dim dr As DataRow = dt.NewRow   
						
						dr("UniqueID")  = Guid.NewGuid
						dr("Cube")		= wfCube
						dr("Entity")    = RP_Entity
						dr("Parent")    = ""
						dr("Cons")		= "USD"
						dr("Scenario")  = wfScenario
						dr("Time")		= wfTime
						dr("Account")	= "Reference_Doc"
						dr("Flow")	    = RPName
						dr("Origin")    = "Forms"
						dr("IC")		= "None"
						dr("UD1")		= "None"
						dr("UD2")		= "None"
						dr("UD3")		= "None"
						dr("UD4")		= "None"
						dr("UD5")		= "None"
						dr("UD6")		= LineItemNum
						dr("UD7")		= "None"
						dr("UD8")		= "None"

						dr("Title")					= ""
						dr("AttachmentType")		= DataAttachmentType.Annotation
						dr("CreatedUserName")		= si.UserName
						dr("CreatedTimestamp")		= DateTime.UtcNow
						dr("LastEditedUserName")    = si.UserName
						dr("LastEditedTimestamp")   = DateTime.UtcNow
						dr("Text")					= "Supporting Doc Attached"
						dr("FileName")				= fileContent.XFFile.FileInfo.Name					
						
						'Add logic for compression
						dr("FileBytes")				= fileContent.XFFile.ContentFileBytes
						dt.Rows.Add(dr)
						BRApi.Database.SaveCustomDataTable(si, "App", "dbo.DataAttachment", dt, False)
								
						'Delete the Loaded File
						BRApi.FileSystem.DeleteFile(si, fileSystemLocation.FileShare, userSelectedFilePath)
								
						
					End Using
								
				
				Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
				selectionChangedTaskResult.IsOK = True
				selectionChangedTaskResult.ShowMessageBox = True
				selectionchangedtaskresult.Message = "Supporting Document Loaded Successfully"
		
				Return selectionChangedTaskResult
				
			End If 'fileContent Is Nothing Then
				
			End If 'Edit Mode
			
			Return Nothing
		End Function
		Private Function WorkflowComplete() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.WorkflowComplete ====
		
				'Initialize method level variables
				Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
				Dim noUpdateMsg As New Text.StringBuilder
				Dim noUpdateCount As Integer = 0

				'Check the Workflow status of the parent (We can't calculate plan if the parent is certified)
				Dim wfRegParent As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetParent(si, si.WorkflowClusterPk)
				Dim wfRegParentPk As New WorkflowUnitClusterPk(wfRegParent.ProfileKey, si.WorkflowClusterPk.ScenarioKey, si.WorkflowClusterPk.TimeKey)
				Dim wfRegParentStatus As WorkflowInfo = BRApi.Workflow.Status.GetWorkflowStatus(si, wfRegParentPk, False)												
				If Not wfRegParentStatus.AllTasksCompleted Then															

					Dim curProfile As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)
						
					'Update workflow to COMPLETED
					Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, si.WorkflowClusterPk)
					BRApi.Workflow.Status.SetWorkflowStatus(si, si.WorkflowClusterPk, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("Plan Workflow Completed: {0}", wfClusterDesc), "", "User clicked [Complete Workflow]", Guid.Empty)					
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					selectionChangedTaskResult.IsOK = True							
					selectionChangedTaskResult.ShowMessageBox = False						

				Else
					'Parent Certified, cannot update workflow
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = False
					selectionChangedTaskResult.IsOK = True							
					selectionChangedTaskResult.ShowMessageBox = True
					selectionChangedTaskResult.Message = "Workflow NOT Completed: Parent Workflow has been Completed."												
				End If	
				
				Return selectionChangedTaskResult
				
			Return Nothing
		End Function
		Private Function WorkflowRevert() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.WorkflowRevert ====
	
				'Initialize method level variables
				Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
				Dim noUpdateMsg As New Text.StringBuilder
				Dim noUpdateCount As Integer = 0

				'Check the Workflow status of the parent (We can't calculate plan if the parent is certified)
				Dim wfRegParent As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetParent(si, si.WorkflowClusterPk)
				Dim wfRegParentPk As New WorkflowUnitClusterPk(wfRegParent.ProfileKey, si.WorkflowClusterPk.ScenarioKey, si.WorkflowClusterPk.TimeKey)
				Dim wfRegParentStatus As WorkflowInfo = BRApi.Workflow.Status.GetWorkflowStatus(si, wfRegParentPk, False)												
				If (Not wfRegParentStatus.AllTasksCompleted) Then															
					'Update the workspace workflow to INPROCESS
					Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, si.WorkflowClusterPk)
					BRApi.Workflow.Status.SetWorkflowStatus(si, si.WorkflowClusterPk, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("Capital Plan Workflow Reverted: {0}", wfClusterDesc), "", "User clicked [Revert Workflow]", Guid.Empty)
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					selectionChangedTaskResult.IsOK = True							
					selectionChangedTaskResult.ShowMessageBox = False
					
				Else
					'Parent Certified, cannot update workflow
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = False
					selectionChangedTaskResult.IsOK = True							
					selectionChangedTaskResult.ShowMessageBox = True
					selectionChangedTaskResult.Message = "Workflow NOT Reverted: Parent Workflow has been Completed."												
				End If	

				
				Return selectionChangedTaskResult								
									
			Return Nothing
		End Function
		Private Function ClearBLTLine_OS() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.ClearBLTLine_OS (called by non-OS appropriation dashboards) ====
					
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
					Dim RPName As String = args.NameValuePairs("RPName")
					Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
					Dim LineItemNum As String = args.NameValuePairs("LineItemNum") 
					Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
					Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")

					If  String.IsNullOrEmpty (LineItemNum) Then 
						Throw New Exception("Please choose a Line Item") 
					End If
					
					RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, LineItemNum)
				
						'Storing the Annotation text for the attributes in a generic string
						Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U7#None:U8#None"		
						Dim hcScriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Periodic:F#" & RPName & ":O#Top:I#Top:U1#HCRptg_UD1:U2#HCRptg_UD2:U3#HCRptg_UD3:U4#HCRptg_UD4:U5#HCRptg_UD5:U7#None:U8#None"							
						
							
						'********LOGIC FOR CLEARING LINE********
						Dim LineItemNumInt As Integer = LineItemNum.Substring(9,2).XFConvertToInt	
						Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
						
						'Run the clear billet function
						Me.ClearBillet(si, globals, args, wfCube, wfScenario, wfTime, RP_Entity, RPName, LineItemNum, LineItemNumInt, scriptGenerics)
						'brapi.ErrorLog.LogMessage(si, "9 ClearBillet 2 " )
							'Show a message box that the Billet was successfully updated
						
						Dim Stringmessage As String = "" & GetDescription(si,RPName) & " " & GetUD6Description(si,LineItemNum) & " Successfully Cleared"	
							
						selectionChangedTaskResult = Me.RefreshSelectedBillet_OS(si, args, globals, wfCube, wfTime, wfScenario, RPName, LineItemNum, Stringmessage)						
						Return selectionChangedTaskResult
								
			Return Nothing
		End Function
		Private Function OnBtnClick_GEN() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.OnBtnClick_GEN (called by non-OS appropriation dashboards) ====
					 Dim BlnLogErrors As Boolean = True
					 'Check values are saved by comparing parameters to current RP
					 'Page 1
					Dim billets As String = args.NameValuePairs.XFGetValue("Billets", String.Empty)
					Dim autoAdd As String = args.NameValuePairs.XFGetValue("AutoAdd", String.Empty)
					Dim increDecre As String = args.NameValuePairs.XFGetValue("IncreDecre", String.Empty)
					Dim reprogramming As String = args.NameValuePairs.XFGetValue("Reprogramming", String.Empty)
					Dim personnelQuarters As String = args.NameValuePairs.XFGetValue("PersonnelQuarters", String.Empty)
					Dim OMQuarters As String = args.NameValuePairs.XFGetValue("O&MQuarters", String.Empty)
					'Dim ActiveDashboard As String = args.NameValuePairs.XFGetValue("ActiveDashboard", String.Empty)

					'Page 2
					Dim leadOffice1 As String = args.NameValuePairs.XFGetValue("LeadOffice1", String.Empty)
					Dim leadOffice2 As String = args.NameValuePairs.XFGetValue("LeadOffice2", String.Empty)
					Dim leadOffice3 As String = args.NameValuePairs.XFGetValue("LeadOffice3", String.Empty)
					Dim leadOfficePOC1 As String = args.NameValuePairs.XFGetValue("LeadOfficePOC1", String.Empty)
					Dim leadOfficePOC2 As String = args.NameValuePairs.XFGetValue("LeadOfficePOC2", String.Empty)
					Dim leadOfficePOC3 As String = args.NameValuePairs.XFGetValue("LeadOfficePOC3", String.Empty)
					Dim leadOfficePhone1 As String = args.NameValuePairs.XFGetValue("LeadOfficePhone1", String.Empty)
					Dim leadOfficePhone2 As String = args.NameValuePairs.XFGetValue("LeadOfficePhone2", String.Empty)
					Dim leadOfficePhone3 As String = args.NameValuePairs.XFGetValue("LeadOfficePhone3", String.Empty)
					Dim initialEstimate As String = args.NameValuePairs.XFGetValue("InitialEstimate", String.Empty)
					Dim initialEstimateMil As String = args.NameValuePairs.XFGetValue("InitialEstimateMil", String.Empty)
					Dim initialEstimateCiv As String = args.NameValuePairs.XFGetValue("InitialEstimateCiv", String.Empty)
					Dim baseFunding As String = args.NameValuePairs.XFGetValue("BaseFunding", String.Empty)
					Dim baseFundingMil As String = args.NameValuePairs.XFGetValue("BaseFundingMil", String.Empty)
					Dim baseFundingCiv As String = args.NameValuePairs.XFGetValue("BaseFundingCiv", String.Empty)
					Dim baseFundingComments As String = args.NameValuePairs.XFGetValue("BaseFundingComments", String.Empty)
					Dim relatedRP1 As String = args.NameValuePairs.XFGetValue("FYRelatedRP1", String.Empty)
					Dim relatedRP2 As String = args.NameValuePairs.XFGetValue("FYRelatedRP2", String.Empty)
					Dim relatedRP3 As String = args.NameValuePairs.XFGetValue("FYRelatedRP3", String.Empty)
					Dim olderRelatedRP1 As String = args.NameValuePairs.XFGetValue("OlderRelatedRP1", String.Empty)
					Dim olderRelatedRP2 As String = args.NameValuePairs.XFGetValue("OlderRelatedRP2", String.Empty)
					Dim olderRelatedRP3 As String = args.NameValuePairs.XFGetValue("OlderRelatedRP3", String.Empty)
					Dim execSummary As String = args.NameValuePairs.XFGetValue("ExecSummary", String.Empty)
					Dim Content_OS As String = args.NameValuePairs.XFGetValue("Content_OS", String.Empty)
					Dim Content_EditRP_OS As String = args.NameValuePairs.XFGetValue("Content_EditRP_OS", String.Empty)

					'Page 3
					Dim problem As String = args.NameValuePairs.XFGetValue("Problem", String.Empty)
					Dim fundingImpact As String = args.NameValuePairs.XFGetValue("FundingImpact", String.Empty)
					Dim denialImpact As String = args.NameValuePairs.XFGetValue("DenialImpact", String.Empty)
					Dim affectOthers As String = args.NameValuePairs.XFGetValue("AffectOthers", String.Empty)
					Dim ROI As String = args.NameValuePairs.XFGetValue("ROI", String.Empty)
					Dim alignment As String = args.NameValuePairs.XFGetValue("Alignment", String.Empty)

					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs.XFGetValue("WFTime", String.Empty)
					Dim wfScenario As String = args.NameValuePairs.XFGetValue("WFScenario", String.Empty)
					Dim wfCube As String = args.NameValuePairs.XFGetValue("WFCube", String.Empty)
					Dim Entity As String = args.NameValuePairs.XFGetValue("Entity", String.Empty)

					'Get the component name
					Dim componentName As String = args.ComponentInfo.Component.Name
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName", String.Empty)
					
					' If No RP is selected, nothing to do
					If RPName = "" Then 
						Return Nothing
					End If
					
					If Content_EditRP_OS = "OS_Billets_MassDelete_04c1c1" Then
						
						Dim params As New Dictionary(Of String, String) 
						params.Add("WFTime", WFTime)
						params.Add("WFScenario", WFScenario) 
						params.Add("Entity", Entity) 
						params.Add("RPname", RPname)

						brapi.Utilities.StartDataMgmtSequence(si, "Mass_No_Delete", params)
					
					End If 
					
					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)												
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"								

					'using a global function to avoid using brapi functions too many times and use api.data.calculate via a finance rule instead
					'set the script generics and parent account to be used in the global function
					globals.SetStringValue("scriptGenerics", scriptGenerics)
					globals.SetStringValue("parAccount", "RP_Attributes")				

					'Set a generic dictionary as an argument in the rule below
					Dim Dictionary As New Dictionary(Of String, String)
					
						BUDFM_AttributeSupport.GetRPAttributes(si, globals)
					
					If Not globals.GetObject("attributeDict") Is Nothing AndAlso rpUtils.Is_RP_Editable(si, RPName)
						
						Dim attributeDict As Dictionary(Of String, String) = globals.GetObject("attributeDict")
						Dim allKeys As New Text.StringBuilder
						'For Each i As String In attributeDict.Keys
						'	allKeys.Append(i & ", ")
						'Next
						allKeys.Append(billets & ", " & autoAdd & ", " & increDecre & ", " & reprogramming & ", " & personnelQuarters & ", " & OMQuarters)
If billets<> attributeDict.GetValueOrEmpty("Number_of_Billets").ToString Then 

End If
						Dim values As New Text.StringBuilder
						values.Append(attributeDict.GetValueOrEmpty("Number_of_Billets") & ", " & attributeDict.GetValueOrEmpty("Add_General_Detail") & ", " & attributeDict.GetValueOrEmpty("Increase_Decrease") & ", " & attributeDict.GetValueOrEmpty("Part_of_Reprogramming") & ", " & attributeDict.GetValueOrEmpty("Personnel_Qtrs") & ", " & attributeDict.GetValueOrEmpty("OS_Qtrs"))
						'Check if saved
						Try
							
							If mblnEnableSavePrompt AndAlso Not (billets = attributeDict.GetValueOrEmpty("Number_of_Billets").ToString) And 
							(autoAdd = attributeDict.GetValueOrEmpty("Add_General_Detail").ToString) And 
							(increDecre = attributeDict.GetValueOrEmpty("Increase_Decrease").ToString) And 
							(reprogramming = attributeDict.GetValueOrEmpty("Part_of_Reprogramming").ToString) And 
							(personnelQuarters = attributeDict.GetValueOrEmpty("Personnel_Qtrs").ToString) And 
							(OMQuarters = attributeDict.GetValueOrEmpty("OS_Qtrs").ToString) And 	
							(leadOffice1 = attributeDict.GetValueOrEmpty("Lead_Office1").ToString) And
							(leadOffice2 = attributeDict.GetValueOrEmpty("Lead_Office2").ToString) And
							(leadOffice3 = attributeDict.GetValueOrEmpty("Lead_Office3").ToString) And
							(leadOfficePOC1 = attributeDict.GetValueOrEmpty("Lead_Office_POC1").ToString) And
							(leadOfficePOC2 = attributeDict.GetValueOrEmpty("Lead_Office_POC2").ToString) And
							(leadOfficePOC3 = attributeDict.GetValueOrEmpty("Lead_Office_POC3").ToString) And
							(leadOfficePhone1 = attributeDict.GetValueOrEmpty("Lead_Office_Phone1").ToString) And
							(leadOfficePhone2 = attributeDict.GetValueOrEmpty("Lead_Office_Phone2").ToString) And
							(leadOfficePhone3 = attributeDict.GetValueOrEmpty("Lead_Office_Phone3").ToString) And
							(initialEstimate = attributeDict.GetValueOrEmpty("Initial_Estimate").ToString) And
							(initialEstimateMil = attributeDict.GetValueOrEmpty("Initial_Estimate_MIL_FTP").ToString) And
							(initialEstimateCiv = attributeDict.GetValueOrEmpty("Initial_Estimate_CIV_FTP").ToString) And
							(baseFunding = attributeDict.GetValueOrEmpty("Base_Funding").ToString) And
							(baseFundingMil = attributeDict.GetValueOrEmpty("Base_Funding_MIL_FTP").ToString) And
							(baseFundingCiv = attributeDict.GetValueOrEmpty("Base_Funding_CIV_FTP").ToString) And
							(baseFundingComments = attributeDict.GetValueOrEmpty("Base_Funding_Comments").ToString) And
							(relatedRP1 = attributeDict.GetValueOrEmpty("FY_Related_RP1").ToString) And
							(relatedRP2 = attributeDict.GetValueOrEmpty("FY_Related_RP2").ToString) And
							(relatedRP3 = attributeDict.GetValueOrEmpty("FY_Related_RP3").ToString) And
							(olderRelatedRP1 = attributeDict.GetValueOrEmpty("Older_Related_RP1").ToString) And
							(olderRelatedRP2 = attributeDict.GetValueOrEmpty("Older_Related_RP2").ToString) And
							(olderRelatedRP3 = attributeDict.GetValueOrEmpty("Older_Related_RP3").ToString) And
							(execSummary = attributeDict.GetValueOrEmpty("Exec_Summary").ToString) And
							(problem = attributeDict.GetValueOrEmpty("Problem").ToString) And
							(fundingImpact = attributeDict.GetValueOrEmpty("Funding_Impact").ToString) And
							(denialImpact = attributeDict.GetValueOrEmpty("Denial_Impact").ToString) And
							(affectOthers = attributeDict.GetValueOrEmpty("Affect_Others").ToString) And
							(ROI = attributeDict.GetValueOrEmpty("ROI").ToString) And
							(alignment = attributeDict.GetValueOrEmpty("Alignment").ToString) Then
							
	
								#Region "Log"

If mBlnLogSavePromptErrors Then
 
If billets 				<> attributeDict.GetValueOrEmpty("Number_of_Billets").ToString        Then 
	BRApi.ErrorLog.LogMessage(si, "billets 				" & vbcrlf & billets 					& " , " & vbcrlf & attributeDict.GetValueOrEmpty("Number_of_Billets").ToString        ) 
End If
If autoAdd 				<> attributeDict.GetValueOrEmpty("Add_General_Detail").ToString       Then 
	BRApi.ErrorLog.LogMessage(si, "autoAdd 				" & vbcrlf & autoAdd 					& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Add_General_Detail").ToString       ) 
End If
If increDecre 				<> attributeDict.GetValueOrEmpty("Increase_Decrease").ToString        Then 
	BRApi.ErrorLog.LogMessage(si, "increDecreQQ 			" &  vbcrlf & increDecre 					& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Increase_Decrease").ToString        ) 
End If
If reprogramming 			<> attributeDict.GetValueOrEmpty("Part_of_Reprogramming").ToString    Then 
	BRApi.ErrorLog.LogMessage(si, "reprogramming			" &  vbcrlf & reprogramming 				& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Part_of_Reprogramming").ToString    ) 
End If
If personnelQuarters 		<> attributeDict.GetValueOrEmpty("Personnel_Qtrs").ToString           Then 
	BRApi.ErrorLog.LogMessage(si, "personnelQuarters		" &  vbcrlf & personnelQuarters 			& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Personnel_Qtrs").ToString           ) 
End If
If OMQuarters 				<> attributeDict.GetValueOrEmpty("OS_Qtrs").ToString                  Then 
	BRApi.ErrorLog.LogMessage(si, "OMQuarters 			" &  vbcrlf & OMQuarters 					& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("OS_Qtrs").ToString                  ) 
End If
If leadOffice1 			<> attributeDict.GetValueOrEmpty("Lead_Office1").ToString             Then 
	BRApi.ErrorLog.LogMessage(si, "leadOffice1 			" &  vbcrlf & leadOffice1 				& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Lead_Office1").ToString             ) 
End If
If leadOffice2 			<> attributeDict.GetValueOrEmpty("Lead_Office2").ToString             Then 
	BRApi.ErrorLog.LogMessage(si, "leadOffice2 			" &  vbcrlf & leadOffice2 				& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Lead_Office2").ToString             ) 
End If
If leadOffice3 			<> attributeDict.GetValueOrEmpty("Lead_Office3").ToString             Then 
	BRApi.ErrorLog.LogMessage(si, "leadOffice3 			" &  vbcrlf & leadOffice3 				& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Lead_Office3").ToString             ) 
End If
If leadOfficePOC1 			<> attributeDict.GetValueOrEmpty("Lead_Office_POC1").ToString         Then 
	BRApi.ErrorLog.LogMessage(si, "leadOfficePOC1 		" &  vbcrlf & leadOfficePOC1 				& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Lead_Office_POC1").ToString         ) 
End If
If leadOfficePOC2 			<> attributeDict.GetValueOrEmpty("Lead_Office_POC2").ToString         Then 
	BRApi.ErrorLog.LogMessage(si, "leadOfficePOC2 		" &  vbcrlf & leadOfficePOC2 				& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Lead_Office_POC2").ToString         ) 
End If
If leadOfficePOC3 			<> attributeDict.GetValueOrEmpty("Lead_Office_POC3").ToString         Then 
	BRApi.ErrorLog.LogMessage(si, "leadOfficePOC3 		" &  vbcrlf & leadOfficePOC3 				& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Lead_Office_POC3").ToString         ) 
End If
If leadOfficePhone1 		<> attributeDict.GetValueOrEmpty("Lead_Office_Phone1").ToString       Then 
	BRApi.ErrorLog.LogMessage(si, "leadOfficePhone1		" &  vbcrlf & leadOfficePhone1			& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Lead_Office_Phone1").ToString       ) 
End If
If leadOfficePhone2 		<> attributeDict.GetValueOrEmpty("Lead_Office_Phone2").ToString       Then 
	BRApi.ErrorLog.LogMessage(si, "leadOfficePhone2		" &  vbcrlf & leadOfficePhone2			& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Lead_Office_Phone2").ToString       ) 
End If
If leadOfficePhone3 		<> attributeDict.GetValueOrEmpty("Lead_Office_Phone3").ToString       Then 
	BRApi.ErrorLog.LogMessage(si, "leadOfficePhone3		" &  vbcrlf & leadOfficePhone3			& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Lead_Office_Phone3").ToString       ) 
End If
If initialEstimate 		<> attributeDict.GetValueOrEmpty("Initial_Estimate").ToString         Then 
	BRApi.ErrorLog.LogMessage(si, "initialEstimate 		" &  vbcrlf & initialEstimate 			& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Initial_Estimate").ToString         ) 
End If
If initialEstimateMil 		<> attributeDict.GetValueOrEmpty("Initial_Estimate_MIL_FTP").ToString Then 
	BRApi.ErrorLog.LogMessage(si, "initialEstimateMil 	" &  vbcrlf & initialEstimateMil 			& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Initial_Estimate_MIL_FTP").ToString ) 
End If
If initialEstimateCiv 		<> attributeDict.GetValueOrEmpty("Initial_Estimate_CIV_FTP").ToString Then 
	BRApi.ErrorLog.LogMessage(si, "initialEstimateCiv 	" &  vbcrlf & initialEstimateCiv 			& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Initial_Estimate_CIV_FTP").ToString ) 
End If
If baseFunding 			<> attributeDict.GetValueOrEmpty("Base_Funding").ToString             Then 
	BRApi.ErrorLog.LogMessage(si, "baseFunding 			" &  vbcrlf & baseFunding 				& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Base_Funding").ToString             ) 
End If
If baseFundingMil 			<> attributeDict.GetValueOrEmpty("Base_Funding_MIL_FTP").ToString     Then 
	BRApi.ErrorLog.LogMessage(si, "baseFundingMil 		" &  vbcrlf & baseFundingMil 				& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Base_Funding_MIL_FTP").ToString     ) 
End If
If baseFundingCiv 			<> attributeDict.GetValueOrEmpty("Base_Funding_CIV_FTP").ToString     Then 
	BRApi.ErrorLog.LogMessage(si, "baseFundingCiv 		" &  vbcrlf & baseFundingCiv 				& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Base_Funding_CIV_FTP").ToString     ) 
End If
If baseFundingComments 	<> attributeDict.GetValueOrEmpty("Base_Funding_Comments").ToString    Then 
	BRApi.ErrorLog.LogMessage(si, "baseFundingComments 	" &  vbcrlf & baseFundingComments 		& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Base_Funding_Comments").ToString    ) 
End If
If relatedRP1 				<> attributeDict.GetValueOrEmpty("FY_Related_RP1").ToString           Then 
	BRApi.ErrorLog.LogMessage(si, "relatedRP1 			" &  vbcrlf & relatedRP1 					& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("FY_Related_RP1").ToString           ) 
End If
If relatedRP2 				<> attributeDict.GetValueOrEmpty("FY_Related_RP2").ToString           Then 
	BRApi.ErrorLog.LogMessage(si, "relatedRP2 			" &  vbcrlf & relatedRP2 					& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("FY_Related_RP2").ToString           ) 
End If
If relatedRP3 				<> attributeDict.GetValueOrEmpty("FY_Related_RP3").ToString           Then 
	BRApi.ErrorLog.LogMessage(si, "relatedRP3 			" &  vbcrlf & relatedRP3 					& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("FY_Related_RP3").ToString           ) 
End If
If olderRelatedRP1 		<> attributeDict.GetValueOrEmpty("Older_Related_RP1").ToString        Then 
	BRApi.ErrorLog.LogMessage(si, "olderRelatedRP1 		" &  vbcrlf & olderRelatedRP1 			& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Older_Related_RP1").ToString        ) 
End If
If olderRelatedRP2 		<> attributeDict.GetValueOrEmpty("Older_Related_RP2").ToString        Then 
	BRApi.ErrorLog.LogMessage(si, "olderRelatedRP2 		" &  vbcrlf & olderRelatedRP2 			& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Older_Related_RP2").ToString        ) 
End If
If olderRelatedRP3 		<> attributeDict.GetValueOrEmpty("Older_Related_RP3").ToString        Then 
	BRApi.ErrorLog.LogMessage(si, "olderRelatedRP3 		" &  vbcrlf & olderRelatedRP3 			& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Older_Related_RP3").ToString        ) 
End If
If execSummary 			<> attributeDict.GetValueOrEmpty("Exec_Summary").ToString             Then 
	BRApi.ErrorLog.LogMessage(si, "execSummary 			" &  vbcrlf & execSummary 				& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Exec_Summary").ToString             ) 
End If
If problem 				<> attributeDict.GetValueOrEmpty("Problem").ToString                  Then 
	BRApi.ErrorLog.LogMessage(si, "problem 				" &  vbcrlf & problem 					& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Problem").ToString                  ) 
End If
If fundingImpact 			<> attributeDict.GetValueOrEmpty("Funding_Impact").ToString           Then 
	BRApi.ErrorLog.LogMessage(si, "fundingImpact			" &  vbcrlf & fundingImpact 				& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Funding_Impact").ToString           ) 
End If
If denialImpact 			<> attributeDict.GetValueOrEmpty("Denial_Impact").ToString            Then 
	BRApi.ErrorLog.LogMessage(si, "denialImpact			" &  vbcrlf & denialImpact				& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Denial_Impact").ToString            ) 
End If
If affectOthers 			<> attributeDict.GetValueOrEmpty("Affect_Others").ToString            Then 
	BRApi.ErrorLog.LogMessage(si, "affectOthers			" &  vbcrlf & affectOthers				& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Affect_Others").ToString            ) 
End If
If ROI 					<> attributeDict.GetValueOrEmpty("ROI").ToString                      Then 
	BRApi.ErrorLog.LogMessage(si, "ROI 					" &  vbcrlf & ROI 						& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("ROI").ToString                      ) 
End If
If alignment 				<> attributeDict.GetValueOrEmpty("Alignment").ToString                Then 
	BRApi.ErrorLog.LogMessage(si, "alignment				" &  vbcrlf & alignment 					& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Alignment").ToString                ) 
End If
End If
#End Region								
								Throw New Exception("Please revert or save changes before navigating away from this page")
							
							End If 
									
								'Set the parameters for the combo boxes in the RP Dashboard Page1
								'Set the defaults for General Detail and O&S and Personnel Qtrs if not stored
								Dim Add_General_Detail As String = String.Empty
								Dim Add_General_DetailSaved As String = attributeDict.GetValueOrEmpty("Add_General_Detail")
								
								If String.IsNullOrEmpty(Add_General_DetailSaved)
									Add_General_Detail = "Y"
								Else 
									Add_General_Detail = Add_General_DetailSaved
								End If
								
								Dim Personnel_Qtrs As String = String.Empty
								Dim Personnel_QtrsSaved As String = attributeDict.GetValueOrEmpty("Personnel_Qtrs")
								
								If String.IsNullOrEmpty(Personnel_QtrsSaved)
									Personnel_Qtrs = BRApi.Dashboards.Parameters.GetParameterDisplayInfo(si, False, Nothing, "prm_BLT_NumberOfPersonnelQtrs_OS").Parameter.DefaultValue
								Else 
									Personnel_Qtrs = Personnel_QtrsSaved
								End If
								
								Dim OS_Qtrs As String = String.Empty
								Dim OS_QtrsSaved As String = attributeDict.GetValueOrEmpty("OS_Qtrs")
								
								If String.IsNullOrEmpty(OS_QtrsSaved)
									OS_Qtrs = BRApi.Dashboards.Parameters.GetParameterDisplayInfo(si, False, Nothing, "prm_NBLT_NumberOfOSQtrs_OS").Parameter.DefaultValue
								Else 
									OS_Qtrs = OS_QtrsSaved
								End If
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Number_OS_Copy", 						RPName)							
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_NumberOfBillets_OS", 				attributeDict.GetValueOrEmpty("Number_of_Billets"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_AutoAddGenDetail_OS", 			Add_General_Detail)
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_IncreaseDecrease_OS", 			attributeDict.GetValueOrEmpty("Increase_Decrease"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_PartOfReprogramming_OS", 			attributeDict.GetValueOrEmpty("Part_of_Reprogramming"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_NumberOfPersonnelQtrs_OS", 		Personnel_Qtrs)
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_NumberOfOSQtrs_OS", 				OS_Qtrs)
								
								'Set the parameters for the combo boxes in the RP Dashboard Page2
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_FYRelatedRp1_OS", 					attributeDict.GetValueOrEmpty("FY_Related_RP1"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_FYRelatedRp2_OS", 					attributeDict.GetValueOrEmpty("FY_Related_RP2"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_FYRelatedRp3_OS", 					attributeDict.GetValueOrEmpty("FY_Related_RP3"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_OlderRelatedRp1_OS", 					attributeDict.GetValueOrEmpty("Older_Related_RP1"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_OlderRelatedRp2_OS", 					attributeDict.GetValueOrEmpty("Older_Related_RP2"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_OlderRelatedRp3_OS", 					attributeDict.GetValueOrEmpty("Older_Related_RP3"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOffice1_OS", 						attributeDict.GetValueOrEmpty("Lead_Office1"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOffice2_OS", 						attributeDict.GetValueOrEmpty("Lead_Office2"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOffice3_OS", 						attributeDict.GetValueOrEmpty("Lead_Office3"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePOC1_OS", 					attributeDict.GetValueOrEmpty("Lead_Office_POC1"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePOC2_OS", 					attributeDict.GetValueOrEmpty("Lead_Office_POC2"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePOC3_OS", 					attributeDict.GetValueOrEmpty("Lead_Office_POC3"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePhone1_OS", 				attributeDict.GetValueOrEmpty("Lead_Office_Phone1"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePhone2_OS", 				attributeDict.GetValueOrEmpty("Lead_Office_Phone2"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePhone3_OS", 				attributeDict.GetValueOrEmpty("Lead_Office_Phone3"))					
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_ExecSummary_OS", 						attributeDict.GetValueOrEmpty("Exec_Summary"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_IE_K_OS", 						attributeDict.GetValueOrEmpty("Initial_Estimate"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_IE_MIL_OS", 						attributeDict.GetValueOrEmpty("Initial_Estimate_MIL_FTP"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_IE_CIV_OS", 						attributeDict.GetValueOrEmpty("Initial_Estimate_CIV_FTP"))			
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_IE_Base_Funding_OS", 				attributeDict.GetValueOrEmpty("Base_Funding"))			
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_IE_Base_Funding_Comments_OS", 		attributeDict.GetValueOrEmpty("Base_Funding_Comments"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_CBF_MIL_OS", 						attributeDict.GetValueOrEmpty("Base_Funding_MIL_FTP"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_CBF_CIV_OS", 						attributeDict.GetValueOrEmpty("Base_Funding_CIV_FTP"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_IE_R_Base_OS", 					attributeDict.GetValueOrEmpty("Recurring_Base_Estimate"))					
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_R_Base_Comments_OS", 				attributeDict.GetValueOrEmpty("Recurring_Base_Comments"))	
								
								'Set the parameters for the combo boxes in the RP Dashboard Page3 (MSN added this 01/20/23)
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Page3_AffectOthers_OS", 				attributeDict.GetValueOrEmpty("Affect_Others"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Page3_Alignment_OS", 					attributeDict.GetValueOrEmpty("Alignment"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Page3_DenialImpact_OS", 				attributeDict.GetValueOrEmpty("Denial_Impact"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Page3_FundingImpact_OS", 				attributeDict.GetValueOrEmpty("Funding_Impact"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Page3_Problem_OS", 					attributeDict.GetValueOrEmpty("Problem"))
								selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Page3_ROI_OS", 						attributeDict.GetValueOrEmpty("ROI"))
								SetRoutingPageCompat(selectionChangedTaskResult.ModifiedCustomSubstVars, ResolveRoutingAppnForRP(RPName, args.NameValuePairs.XFGetValue("APPN_Content", "OS")), Content_EditRP_OS)
								SetRoutingContent(selectionChangedTaskResult.ModifiedCustomSubstVars, ResolveRoutingAppnForRP(RPName, args.NameValuePairs.XFGetValue("APPN_Content", "OS")), Content_OS)
							
												
								
						
						Catch ex As Exception
							Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
						End Try
					Else 
						
						SetRoutingPageCompat(selectionChangedTaskResult.ModifiedCustomSubstVars, ResolveRoutingAppnForRP(RPName, args.NameValuePairs.XFGetValue("APPN_Content", "OS")), Content_EditRP_OS)
						SetRoutingContent(selectionChangedTaskResult.ModifiedCustomSubstVars, ResolveRoutingAppnForRP(RPName, args.NameValuePairs.XFGetValue("APPN_Content", "OS")), Content_OS)						
					End If 'Not globals.GetObject("attributeDict") Is Nothing
							
					
					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					Return selectionChangedTaskResult
					
			Return Nothing
		End Function
		Private Function OnCbxBtnClick_RPCreate() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.OnCbxBtnClick_RPCreate (called by non-OS appropriation dashboards) ====
					
						Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
						Dim routingAppn As String = NormalizeRoutingAppn(args.NameValuePairs.XFGetValue("Appropriation", args.NameValuePairs.XFGetValue("APPN_Content", "OS")))
						Dim routedContent As String = args.NameValuePairs.XFGetValue("Content_OS", args.NameValuePairs.XFGetValue("Content", String.Empty))
						SetRoutingContent(selectionChangedTaskResult.ModifiedCustomSubstVars, routingAppn, routedContent)
						selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
						Return selectionChangedTaskResult
							 
			Return Nothing
		End Function
		Private Function OnCbxRP_BudgetCat_Selected() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.OnCbxRP_BudgetCat_Selected (called by non-OS appropriation dashboards) ====
                     Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
		             Dim Appropriation As String = args.NameValuePairs("Appropriation")
		             Dim Budget_Category As String = String.Empty 

		             If Appropriation.XFEqualsIgnoreCase("OS")
			         		Budget_Category = "I"
					 Else 
							Budget_Category = "NA"
					 End If					

                selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BudgetCategory_OS", Budget_Category)
				selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
							Return selectionChangedTaskResult
					
			Return Nothing
		End Function
		Private Function OnCreateBtnClick() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.OnCreateBtnClick (called by non-OS appropriation dashboards) ====
					 'Check if saved and then update session state appropriately
					 Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()     
					 If CheckSaveState(si, globals, args) Then
						'Throw New Exception(mShowMessage)
					End If

					 BRApi.State.SetSessionState(si, False, ClientModuletype.Unknown, "", "", "dashState", "dashState", "Create", si.XfBytes)
			Return Nothing
		End Function
		Private Function OnHeaderBtnClick_GEN() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.OnHeaderBtnClick_GEN (called by non-OS appropriation dashboards) ====
					 
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
					
					'Get component name
					Dim componentName As String = args.ComponentInfo.Component.Name
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					Dim RPNameCopy As String = args.NameValuePairs.XFGetValue("RPNameCopy")
					
					' If No RP is selected, nothing to do
					If RPName = "" Then
						'Update session state appropriately, even if no RP selected
						BRApi.State.SetSessionState(si, False, ClientModuletype.Unknown,"","","dashState","dashState","Edit", si.XfBytes)
						Return Nothing
					End If
					
					If CheckSaveState(si, globals, args) Then
						'Throw New Exception(mShowMessage)
					End If

					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)							
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"								
					
					'using a global function to avoid using brapi functions too many times and use api.data.calculate via a finance rule instead
					'set the script generics and parent account to be used in the global function
					globals.SetStringValue("scriptGenerics", scriptGenerics)
					globals.SetStringValue("parAccount", "RP_Attributes")					
					

					'Set a generic dictionary as an argument in the rule below
					Dim Dictionary As New Dictionary(Of String, String)
					
						BUDFM_AttributeSupport.GetRPAttributes(si, globals)
					
					If Not globals.GetObject("attributeDict") Is Nothing
						
						Dim attributeDict As Dictionary(Of String, String) = globals.GetObject("attributeDict")
							
						'Set the parameters for the combo boxes in the RP Dashboard Page1
						'Set the defaults for General Detail and O&S and Personnel Qtrs if not stored
						Dim Add_General_Detail As String = String.Empty
						Dim Add_General_DetailSaved As String = attributeDict.GetValueOrEmpty("Add_General_Detail")
						
						If String.IsNullOrEmpty(Add_General_DetailSaved)
							Add_General_Detail = "Y"
						Else 
							Add_General_Detail = Add_General_DetailSaved
						End If
						
						Dim Personnel_Qtrs As String = String.Empty
						Dim Personnel_QtrsSaved As String = attributeDict.GetValueOrEmpty("Personnel_Qtrs")
						
						If String.IsNullOrEmpty(Personnel_QtrsSaved)
							Personnel_Qtrs = BRApi.Dashboards.Parameters.GetParameterDisplayInfo(si, False, Nothing, "prm_BLT_NumberOfPersonnelQtrs_OS").Parameter.DefaultValue
						Else 
							Personnel_Qtrs = Personnel_QtrsSaved
						End If
						
						Dim OS_Qtrs As String = String.Empty
						Dim OS_QtrsSaved As String = attributeDict.GetValueOrEmpty("OS_Qtrs")
						
						If String.IsNullOrEmpty(OS_QtrsSaved)
							OS_Qtrs = BRApi.Dashboards.Parameters.GetParameterDisplayInfo(si, False, Nothing, "prm_NBLT_NumberOfOSQtrs_OS").Parameter.DefaultValue
						Else 
							OS_Qtrs = OS_QtrsSaved
						End If
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Number_OS_Copy",                      RPName)							
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_NumberOfBillets_OS", 				attributeDict.GetValueOrEmpty("Number_of_Billets"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_AutoAddGenDetail_OS", 			Add_General_Detail)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_IncreaseDecrease_OS", 			attributeDict.GetValueOrEmpty("Increase_Decrease"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_PartOfReprogramming_OS", 			attributeDict.GetValueOrEmpty("Part_of_Reprogramming"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_NumberOfPersonnelQtrs_OS", 		Personnel_Qtrs)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_NumberOfOSQtrs_OS", 				OS_Qtrs)
						
						'Set the parameters for the combo boxes in the RP Dashboard Page2
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_FYRelatedRp1_OS", 					attributeDict.GetValueOrEmpty("FY_Related_RP1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_FYRelatedRp2_OS", 					attributeDict.GetValueOrEmpty("FY_Related_RP2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_FYRelatedRp3_OS", 					attributeDict.GetValueOrEmpty("FY_Related_RP3"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_OlderRelatedRp1_OS", 					attributeDict.GetValueOrEmpty("Older_Related_RP1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_OlderRelatedRp2_OS", 					attributeDict.GetValueOrEmpty("Older_Related_RP2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_OlderRelatedRp3_OS", 					attributeDict.GetValueOrEmpty("Older_Related_RP3"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOffice1_OS", 						attributeDict.GetValueOrEmpty("Lead_Office1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOffice2_OS", 						attributeDict.GetValueOrEmpty("Lead_Office2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOffice3_OS", 						attributeDict.GetValueOrEmpty("Lead_Office3"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePOC1_OS", 					attributeDict.GetValueOrEmpty("Lead_Office_POC1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePOC2_OS", 					attributeDict.GetValueOrEmpty("Lead_Office_POC2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePOC3_OS", 					attributeDict.GetValueOrEmpty("Lead_Office_POC3"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePhone1_OS", 				attributeDict.GetValueOrEmpty("Lead_Office_Phone1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePhone2_OS", 				attributeDict.GetValueOrEmpty("Lead_Office_Phone2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePhone3_OS", 				attributeDict.GetValueOrEmpty("Lead_Office_Phone3"))					
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_ExecSummary_OS", 						attributeDict.GetValueOrEmpty("Exec_Summary"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_IE_K_OS", 						attributeDict.GetValueOrEmpty("Initial_Estimate"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_IE_MIL_OS", 						attributeDict.GetValueOrEmpty("Initial_Estimate_MIL_FTP"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_IE_CIV_OS", 						attributeDict.GetValueOrEmpty("Initial_Estimate_CIV_FTP"))			
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_IE_Base_Funding_OS", 				attributeDict.GetValueOrEmpty("Base_Funding"))			
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_IE_Base_Funding_Comments_OS", 		attributeDict.GetValueOrEmpty("Base_Funding_Comments"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_CBF_MIL_OS", 						attributeDict.GetValueOrEmpty("Base_Funding_MIL_FTP"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_CBF_CIV_OS", 						attributeDict.GetValueOrEmpty("Base_Funding_CIV_FTP"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_IE_R_Base_OS", 					attributeDict.GetValueOrEmpty("Recurring_Base_Estimate"))					
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_R_Base_Comments_OS", 				attributeDict.GetValueOrEmpty("Recurring_Base_Comments"))	
						
						'Set the parameters for the combo boxes in the RP Dashboard Page3 (MSN added this 01/20/23)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Page3_AffectOthers_OS", 				attributeDict.GetValueOrEmpty("Affect_Others"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Page3_Alignment_OS", 					attributeDict.GetValueOrEmpty("Alignment"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Page3_DenialImpact_OS", 				attributeDict.GetValueOrEmpty("Denial_Impact"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Page3_FundingImpact_OS", 				attributeDict.GetValueOrEmpty("Funding_Impact"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Page3_Problem_OS", 					attributeDict.GetValueOrEmpty("Problem"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Page3_ROI_OS", 						attributeDict.GetValueOrEmpty("ROI"))

					End If 'Not globals.GetObject("attributeDict") Is Nothing
					
					'Update session state appropriately
					BRApi.State.SetSessionState(si, False, ClientModuletype.Unknown,"","","dashState","dashState","Edit", si.XfBytes)
					
					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					Return selectionChangedTaskResult
					
			Return Nothing
		End Function
		Private Function OnHeaderRP_Billet_Selected() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.OnHeaderRP_Billet_Selected (called by non-OS appropriation dashboards) ====
					 
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
										
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					Dim LINumber As String = args.NameValuePairs.XFGetValue("LINumber")	
					Dim CheckAction As Boolean = args.NameValuePairs.XFGetValue("CheckSaveAction")
					Dim LINumberCopy As String = args.NameValuePairs.XFGetValue("BLTCopy")	
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					Dim CurrentView  As String = args.NameValuePairs("CurrentView")
					' If No RP is selected, nothing to do
					If RPName = "" Then
						'Update session state appropriately, even if no RP selected
						BRApi.State.SetSessionState(si, False, ClientModuletype.Unknown,"","","dashState","dashState","AddEditBillets", si.XfBytes)
						Return Nothing
					End If
					
					If CheckAction AndAlso CheckSaveState(si, globals, args) Then
						'Throw New Exception(mShowMessage)
					End If
					
					LINumber= "LineItem_01"
					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)

					'Logic to set the default line item when the Billet screen is opened
					Dim LINumberToSet As String = String.Empty
					If LINumber.Length > 0 Then
						'Get the number of billets and integer from the line item member to compare and return appropriate line item per the RP selected
						Dim rightChars As Integer = LINumber.Substring(9,2).XFConvertToInt			
						
						Dim number_of_Billets As Integer = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Number_of_Billets:E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").DataCellEx.DataCellAnnotation.XFConvertToInt
						
						If  rightChars > number_of_Billets
							LINumberToSet = "LineItem_01"	
							
						Else
							LINumberToSet = LINumber	
							
						End If
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_LineItemNumber_OS_Copy", LINumber)	
					Else
						LINumberToSet = "LineItem_01"
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_LineItemNumber_OS_Copy", "")

					End If					
					
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumberToSet & ":U7#None:U8#None"						
											
					'using a global function to avoid using brapi functions too many times and use api.data.calculate via a finance rule instead
					'set the script generics and parent account to be used in the global function
					globals.SetStringValue("scriptGenerics", scriptGenerics)
					globals.SetStringValue("parAccount", "Billet_LineItem_Data")					

					'Set a generic dictionary as an argument in the rule below
					Dim Dictionary As New Dictionary(Of String, String)
					
						BUDFM_AttributeSupport.GetRPAttributes(si, globals)
					
					If Not globals.GetObject("attributeDict") Is Nothing
					
						Dim attributeDict As Dictionary(Of String, String) = globals.GetObject("attributeDict")
							
						'For the ATU creteria, we need to derive the parent ATU since we store it in NoUnit
						'Derive Billet_ATU from Billet_ATU_NoUnit since we stored it as a base but they chose a parentDim Billet_ATU_NoUnit As String = Billet_ATU_NoUnit_Info
						Dim Billet_ATU_NoUnit As String = attributeDict.GetValueOrEmpty("Billet_ATU")
						Dim Billet_ATU As String = String.Empty
						If Billet_ATU_NoUnit.Length > 0
							Billet_ATU = Billet_ATU_NoUnit.Substring(0, Billet_ATU_NoUnit.Length - 7)
							'If nothing Return Zero
						End If
						
						'Derive PPE_ATU from PPE_ATU_NoUnit since we stored it as a base but they chose a parent
						Dim PPE_ATU_NoUnit As String = attributeDict.GetValueOrEmpty("PPE_ATU")	
						Dim PPE_ATU As String = String.Empty
						If PPE_ATU_NoUnit.Length > 0
							PPE_ATU = PPE_ATU_NoUnit.Substring(0, PPE_ATU_NoUnit.Length - 7)
							'If nothing Return Zero
						End If	
												
'						'Derive lease_ATU from lease_ATU_NoUnit since we stored it as a base but they chose a parent
						Dim lease_ATU_NoUnit As String = attributeDict.GetValueOrEmpty("Lease_ATU")	
						Dim lease_ATU As String = String.Empty
						If lease_ATU_NoUnit.Length > 0
							lease_ATU = lease_ATU_NoUnit.Substring(0, lease_ATU_NoUnit.Length - 7)
							'If nothing Return Zero
						End If	
						
						'Derive UTL_ATU from UTL_ATU_NoUnit since we stored it as a base but they chose a parent
						Dim UTL_ATU_NoUnit As String = attributeDict.GetValueOrEmpty("Utilities_ATU")	
						Dim UTL_ATU As String = String.Empty
						If UTL_ATU_NoUnit.Length > 0
							UTL_ATU = UTL_ATU_NoUnit.Substring(0, UTL_ATU_NoUnit.Length - 7)
							'If nothing Return Zero
						End If			
						
						'set the line item based on the above logic
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_LineItemNumber_OS", LINumberToSet)
					    selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Number_OS_Copy",RPName)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_PPE_ATU_OS", PPE_ATU)	
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Lease_ATU_OS", lease_ATU)	
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_ATU_OS", Billet_ATU)	
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_UTL_ATU_OS", UTL_ATU)	
							
						'For all other billet attributes, just return what was stored
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_BilletType_OS", 			attributeDict.GetValueOrEmpty("Billet_Type"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_GradeType_OS", 			attributeDict.GetValueOrEmpty("Grade_Type"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_GradeRank_OS", 			attributeDict.GetValueOrEmpty("Grade_Rank"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_ADReserve_OS", 			attributeDict.GetValueOrEmpty("AD_Reserve"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_ReserveType_OS", 			attributeDict.GetValueOrEmpty("Reserve_Type"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_SpcCodeOccSeries_OS", 	attributeDict.GetValueOrEmpty("Spe_Code_Occu_Series"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Pilot_OS", 				attributeDict.GetValueOrEmpty("Pilot"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_ElectronicFlightBag_OS", 	attributeDict.GetValueOrEmpty("Electronic_Flight_Bag"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_PositionNumber_OS", 		attributeDict.GetValueOrEmpty("Position_Number"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_PositionTitle_OS", 		attributeDict.GetValueOrEmpty("Position_Title"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_OPFACS_OS", 				attributeDict.GetValueOrEmpty("OPFAC"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_UII_OS", 					attributeDict.GetValueOrEmpty("Billet_UII"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_ConusOConus_OS", 			attributeDict.GetValueOrEmpty("CONUS_OCONUS"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_DetachedDuty_OS", 		attributeDict.GetValueOrEmpty("Detached_Duty"))								
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_DutyLocation_OS", 		attributeDict.GetValueOrEmpty("Detached_Duty_Location"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_TermBillet_OS", 			attributeDict.GetValueOrEmpty("Term_Billet"))
						
						Dim PPE_Typedescription As String = String.Empty
						Dim loopCounter As Integer = 0
						
						If attributeDict.GetValueOrEmpty("PPE_Type").Length = 0
							PPE_Typedescription = ""
						Else
							
							Dim selectedArray() As String = attributeDict.GetValueOrEmpty("PPE_Type").Replace(" ", "").Split(",")
							Dim types As List(Of String) = selectedArray.ToList()
						
							For Each ppetype As String In types
								If loopCounter = 0 Then
							
									PPE_Typedescription = BRApi.Finance.Members.GetMember(si, dimtypeid.UD8, ppetype).Description 
							
								Else
								
									PPE_Typedescription = PPE_Typedescription & ", " & BRApi.Finance.Members.GetMember(si, dimtypeid.UD8, ppetype).Description
								
								End If
							
								loopCounter+=1
						
						   Next
						
						
						End If
						
					
					
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_PPEType_OS", 				attributeDict.GetValueOrEmpty("PPE_Type"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_PPEType_Descr_OS", 				PPE_Typedescription)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_PPE_PPA_OS", 				attributeDict.GetValueOrEmpty("PPE_PPA"))										
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Build_Out_OS", 			attributeDict.GetValueOrEmpty("Build_Out_Choice"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_ICASSType_OS", 			attributeDict.GetValueOrEmpty("ICASS_Costs"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_BIType_OS", 				attributeDict.GetValueOrEmpty("Background_Investigation_Type"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Acq_Project_OS", 			attributeDict.GetValueOrEmpty("Acquisition_Project"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Lease_OS", 				attributeDict.GetValueOrEmpty("Lease_Choice"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Lease_PPA_OS", 			attributeDict.GetValueOrEmpty("Lease_PPA"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Furniture_OS", 			attributeDict.GetValueOrEmpty("Furniture_Reqd"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Utilities_OS", 			attributeDict.GetValueOrEmpty("Utilities_Reqd"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Computer_Type_OS", 		attributeDict.GetValueOrEmpty("Computer_Type"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Comment_OS", 				attributeDict.GetValueOrEmpty("LineItem_Comment"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_UTL_PPA_OS", 				attributeDict.GetValueOrEmpty("Utilities_PPA"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Content_AddEditBillets_NonEditRP_OS", "OS_Billets_Main_04c1a")	

						'' reset the dyanmic parameter for the create new rp when billets buttons is clicked.
					    Brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prm_Content_AddEditBillets_NonEditRP_OS", CurrentView) 
					End If 'Not globals.GetObject("attributeDict") Is Nothing
					
					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					'Update session state appropriately
					BRApi.State.SetSessionState(si, False, ClientModuletype.Unknown,"","","dashState","dashState","AddEditBillets", si.XfBytes)
					Return selectionChangedTaskResult						
					'End Select													
					
			Return Nothing
		End Function
		Private Function OnHeaderRP_NonBillet_Selected() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.OnHeaderRP_NonBillet_Selected (called by non-OS appropriation dashboards) ====
				
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
				
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
		
					Dim wfCube As String = args.NameValuePairs("WFCube")
					
					
					'Get the component name
					Dim componentName As String = args.ComponentInfo.Component.Name
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
						
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					Dim LINumberCopy As String = args.NameValuePairs.XFGetValue("BLTCopy")	
					Dim LINumber As String = args.NameValuePairs.XFGetValue("NBLT")
					

					If RPName = "" Then
						'Update session state appropriately, even if no RP selected
						BRApi.State.SetSessionState(si, False, ClientModuletype.Unknown,"","","dashState","dashState","AddEditNonBillets", si.XfBytes)
						Return Nothing
					End If	
					
					If CheckSaveState(si, globals, args) Then
						'Throw New Exception(mShowMessage)
					End If
					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)					
					
					
					'Logic to set the default line item when the Billet screen is opened
					Dim LINumberToSet As String = String.Empty

					LINumberToSet = "NBLineItem_01"
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_LineItemNumber_OS", LINumberToSet)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_LineItemNumber_OS_Copy", LINumberToSet)
					
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumberToSet & ":U7#None:U8#None"			
					Dim scriptGenericsDescr As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_Input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U6#"& LINumberToSet & ":U7#None:U8#None"				
					
					'using a global function to avoid using brapi functions too many times and use api.data.calculate via a finance rule instead
					'set the script generics and parent account to be used in the global function
					globals.SetStringValue("scriptGenerics", scriptGenerics)
					globals.SetStringValue("parAccount", "NonBillet_LineItem_Data")				

					'Set a generic dictionary as an argument in the rule below
					Dim Dictionary As New Dictionary(Of String, String)
					
						BUDFM_AttributeSupport.GetRPAttributes(si, globals)
					
					If Not globals.GetObject("attributeDict") Is Nothing
					
						Dim attributeDict As Dictionary(Of String, String) = globals.GetObject("attributeDict")
							
	'					'Get info for the Non-Billet

						Dim Requested_Item_Cost_Line As String = attributeDict.GetValueOrEmpty("Requested_Item_Tier1")

						'Get the ItemNum to use to find the description Input account
						Dim requested_ItemNum As Integer
						If (Not Requested_Item_Cost_Line = "") 
							Dim requested_Item_Tier1Split As List(Of String) = StringHelper.SplitString(Requested_Item_Cost_Line, "_")
							requested_ItemNum = requested_Item_Tier1Split(0).XFConvertToInt
						End If	
						
						'Derive ATU from ATU_NoUnit since we stored it as a base but they chose a parent
						Dim ATU_NoUnit As String = attributeDict.GetValueOrEmpty("ATU")	
						Dim ATU As String = String.Empty
						'If it already has a value, derive the parent member from the stored NoUnit child
						If ATU_NoUnit.Length > 0
							ATU = ATU_NoUnit.Substring(0, ATU_NoUnit.Length - 7)
						Else
						End If
						
						'Set Parameters for NonBillet info_section
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Number_OS_Copy",                      RPName)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_RequestedItem_Tier1_OS", 		Requested_Item_Cost_Line)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_ATU_OS", 						ATU)						
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_Description_Tier2_OS", 			attributeDict.GetValueOrEmpty("Description_Tier2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_Description_Tier2_Input_OS", 	BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "U5#" & requested_ItemNum & "0_1:" 		& scriptGenericsDescr).DataCellEx.DataCellAnnotation)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_POC_OS", 						attributeDict.GetValueOrEmpty("POC"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_DollarKValue_OS", 				attributeDict.GetValueOrEmpty("DollarK_Value"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_RecurringNonRecurring_OS", 		attributeDict.GetValueOrEmpty("R_NR"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_PPA_OS", 						attributeDict.GetValueOrEmpty("PPA"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_UII_OS", 						attributeDict.GetValueOrEmpty("UII"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_ObjectClass_OS", 				attributeDict.GetValueOrEmpty("Object_Class"))
					
					End If 'globals.GetObject("attributeDict") Is Nothing
					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					'Update session state appropriately
					BRApi.State.SetSessionState(si, False, ClientModuletype.Unknown,"","","dashState","dashState","AddEditNonBillets", si.XfBytes)
					Return selectionChangedTaskResult
					
			Return Nothing
		End Function
		Private Function OnReportingBtnClick() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.OnReportingBtnClick (called by non-OS appropriation dashboards) ====
					 Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()     
					 'Check if saved and then update session state appropriately
					If CheckSaveState(si, globals, args) Then
						'Throw New Exception(mShowMessage)
					End If
					 BRApi.State.SetSessionState(si, False, ClientModuletype.Unknown, "", "", "dashState", "dashState", "Reporting", si.XfBytes)
			Return Nothing
		End Function
		Private Function Refresh() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.Refresh (called by non-OS appropriation dashboards) ====
					 
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
					
					
					
					'Get component name
					Dim componentName As String = args.ComponentInfo.Component.Name
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")

					' If No RP is selected, nothing to do
					If RPName = "" Then
						'Update session state appropriately, even if no RP selected
						BRApi.State.SetSessionState(si, False, ClientModuletype.Unknown,"","","dashState","dashState","Edit", si.XfBytes)
						Return Nothing
					End If
					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)							
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"								
					
					'using a global function to avoid using brapi functions too many times and use api.data.calculate via a finance rule instead
					'set the script generics and parent account to be used in the global function
					globals.SetStringValue("scriptGenerics", scriptGenerics)
					globals.SetStringValue("parAccount", "RP_Attributes")					
					

					'Set a generic dictionary as an argument in the rule below
					Dim Dictionary As New Dictionary(Of String, String)
					
						BUDFM_AttributeSupport.GetRPAttributes(si, globals)
					
					If Not globals.GetObject("attributeDict") Is Nothing

						Dim attributeDict As Dictionary(Of String, String) = globals.GetObject("attributeDict")
							
						'Set the parameters for the combo boxes in the RP Dashboard Page1
						'Set the defaults for General Detail and O&S and Personnel Qtrs if not stored
						Dim Add_General_Detail As String = String.Empty
						Dim Add_General_DetailSaved As String = attributeDict.GetValueOrEmpty("Add_General_Detail")
						
						If String.IsNullOrEmpty(Add_General_DetailSaved)
							Add_General_Detail = "Y"
						Else 
							Add_General_Detail = Add_General_DetailSaved
						End If
						
						Dim Personnel_Qtrs As String = String.Empty
						Dim Personnel_QtrsSaved As String = attributeDict.GetValueOrEmpty("Personnel_Qtrs")
						
						If String.IsNullOrEmpty(Personnel_QtrsSaved)
							Personnel_Qtrs = BRApi.Dashboards.Parameters.GetParameterDisplayInfo(si, False, Nothing, "prm_BLT_NumberOfPersonnelQtrs_OS").Parameter.DefaultValue
						Else 
							Personnel_Qtrs = Personnel_QtrsSaved
						End If
						
						Dim OS_Qtrs As String = String.Empty
						Dim OS_QtrsSaved As String = attributeDict.GetValueOrEmpty("OS_Qtrs")
						
						If String.IsNullOrEmpty(OS_QtrsSaved)
							OS_Qtrs = BRApi.Dashboards.Parameters.GetParameterDisplayInfo(si, False, Nothing, "prm_NBLT_NumberOfOSQtrs_OS").Parameter.DefaultValue
						Else 
							OS_Qtrs = OS_QtrsSaved
						End If
												
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_NumberOfBillets_OS", 				attributeDict.GetValueOrEmpty("Number_of_Billets"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_AutoAddGenDetail_OS", 			Add_General_Detail)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_IncreaseDecrease_OS", 			attributeDict.GetValueOrEmpty("Increase_Decrease"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_PartOfReprogramming_OS", 			attributeDict.GetValueOrEmpty("Part_of_Reprogramming"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_NumberOfPersonnelQtrs_OS", 		Personnel_Qtrs)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_NumberOfOSQtrs_OS", 				OS_Qtrs)
						
						'Set the parameters for the combo boxes in the RP Dashboard Page2
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_FYRelatedRp1_OS", 					attributeDict.GetValueOrEmpty("FY_Related_RP1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_FYRelatedRp2_OS", 					attributeDict.GetValueOrEmpty("FY_Related_RP2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_FYRelatedRp3_OS", 					attributeDict.GetValueOrEmpty("FY_Related_RP3"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_OlderRelatedRp1_OS", 					attributeDict.GetValueOrEmpty("Older_Related_RP1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_OlderRelatedRp2_OS", 					attributeDict.GetValueOrEmpty("Older_Related_RP2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_OlderRelatedRp3_OS", 					attributeDict.GetValueOrEmpty("Older_Related_RP3"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOffice1_OS", 						attributeDict.GetValueOrEmpty("Lead_Office1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOffice2_OS", 						attributeDict.GetValueOrEmpty("Lead_Office2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOffice3_OS", 						attributeDict.GetValueOrEmpty("Lead_Office3"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePOC1_OS", 					attributeDict.GetValueOrEmpty("Lead_Office_POC1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePOC2_OS", 					attributeDict.GetValueOrEmpty("Lead_Office_POC2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePOC3_OS", 					attributeDict.GetValueOrEmpty("Lead_Office_POC3"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePhone1_OS", 				attributeDict.GetValueOrEmpty("Lead_Office_Phone1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePhone2_OS", 				attributeDict.GetValueOrEmpty("Lead_Office_Phone2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePhone3_OS", 				attributeDict.GetValueOrEmpty("Lead_Office_Phone3"))					
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_ExecSummary_OS", 						attributeDict.GetValueOrEmpty("Exec_Summary"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_IE_K_OS", 						attributeDict.GetValueOrEmpty("Initial_Estimate"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_IE_MIL_OS", 						attributeDict.GetValueOrEmpty("Initial_Estimate_MIL_FTP"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_IE_CIV_OS", 						attributeDict.GetValueOrEmpty("Initial_Estimate_CIV_FTP"))			
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_IE_Base_Funding_OS", 				attributeDict.GetValueOrEmpty("Base_Funding"))			
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_IE_Base_Funding_Comments_OS", 		attributeDict.GetValueOrEmpty("Base_Funding_Comments"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_CBF_MIL_OS", 						attributeDict.GetValueOrEmpty("Base_Funding_MIL_FTP"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_CBF_CIV_OS", 						attributeDict.GetValueOrEmpty("Base_Funding_CIV_FTP"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_IE_R_Base_OS", 					attributeDict.GetValueOrEmpty("Recurring_Base_Estimate"))					
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_R_Base_Comments_OS", 				attributeDict.GetValueOrEmpty("Recurring_Base_Comments"))	
						
						'Set the parameters for the combo boxes in the RP Dashboard Page3 (MSN added this 01/20/23)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Page3_AffectOthers_OS", 				attributeDict.GetValueOrEmpty("Affect_Others"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Page3_Alignment_OS", 					attributeDict.GetValueOrEmpty("Alignment"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Page3_DenialImpact_OS", 				attributeDict.GetValueOrEmpty("Denial_Impact"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Page3_FundingImpact_OS", 				attributeDict.GetValueOrEmpty("Funding_Impact"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Page3_Problem_OS", 					attributeDict.GetValueOrEmpty("Problem"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Page3_ROI_OS", 						attributeDict.GetValueOrEmpty("ROI"))
						SetRoutingPageCompat(selectionChangedTaskResult.ModifiedCustomSubstVars, routingAppn, routingAppn & "_RP_Page1")
						SetRoutingContent(selectionChangedTaskResult.ModifiedCustomSubstVars, routingAppn, routingAppn & "_RP_Content")

					End If 'Not globals.GetObject("attributeDict") Is Nothing
					
					'Update session state appropriately
					BRApi.State.SetSessionState(si, False, ClientModuletype.Unknown,"","","dashState","dashState","Edit", si.XfBytes)
					
					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					Return selectionChangedTaskResult
					
			Return Nothing
		End Function
		Private Function Refresh_PPA_Extractor() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.Refresh_PPA_Extractor (called by non-OS appropriation dashboards) ====
			
			Dim columnSelection As String = args.NameValuePairs("ColumnSelection")
			
			Return Nothing
		End Function
		Private Function Revert_OS_B() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.Revert_OS_B (called by non-OS appropriation dashboards) ====
					 
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
					
					'Get component name
					Dim componentName As String = args.ComponentInfo.Component.Name
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")

					' If No RP is selected, nothing to do
					If RPName = "" Then
						'Update session state appropriately, even if no RP selected
						BRApi.State.SetSessionState(si, False, ClientModuletype.Unknown,"","","dashState","dashState","Edit", si.XfBytes)
						Return Nothing
					End If

					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)							
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"								
					
					'using a global function to avoid using brapi functions too many times and use api.data.calculate via a finance rule instead
					'set the script generics and parent account to be used in the global function
					globals.SetStringValue("scriptGenerics", scriptGenerics)
					globals.SetStringValue("parAccount", "RP_Attributes")					
					

					'Set a generic dictionary as an argument in the rule below
					Dim Dictionary As New Dictionary(Of String, String)
					
						BUDFM_AttributeSupport.GetRPAttributes(si, globals)
					
					If Not globals.GetObject("attributeDict") Is Nothing
						
						Dim attributeDict As Dictionary(Of String, String) = globals.GetObject("attributeDict")
							
						'Set the parameters for the combo boxes in the RP Dashboard Page1
						'Set the defaults for General Detail and O&S and Personnel Qtrs if not stored
						Dim Add_General_Detail As String = String.Empty
						Dim Add_General_DetailSaved As String = attributeDict.GetValueOrEmpty("Add_General_Detail")
						
						If String.IsNullOrEmpty(Add_General_DetailSaved)
							Add_General_Detail = "Y"
						Else 
							Add_General_Detail = Add_General_DetailSaved
						End If
						
						Dim Personnel_Qtrs As String = String.Empty
						Dim Personnel_QtrsSaved As String = attributeDict.GetValueOrEmpty("Personnel_Qtrs")
						
						If String.IsNullOrEmpty(Personnel_QtrsSaved)
							Personnel_Qtrs = BRApi.Dashboards.Parameters.GetParameterDisplayInfo(si, False, Nothing, "prm_BLT_NumberOfPersonnelQtrs_OS").Parameter.DefaultValue
						Else 
							Personnel_Qtrs = Personnel_QtrsSaved
						End If
						
						Dim OS_Qtrs As String = String.Empty
						Dim OS_QtrsSaved As String = attributeDict.GetValueOrEmpty("OS_Qtrs")
						
						If String.IsNullOrEmpty(OS_QtrsSaved)
							OS_Qtrs = BRApi.Dashboards.Parameters.GetParameterDisplayInfo(si, False, Nothing, "prm_NBLT_NumberOfOSQtrs_OS").Parameter.DefaultValue
						Else 
							OS_Qtrs = OS_QtrsSaved
						End If
												
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_NumberOfBillets_OS", 				attributeDict.GetValueOrEmpty("Number_of_Billets"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_AutoAddGenDetail_OS", 			Add_General_Detail)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_IncreaseDecrease_OS", 			attributeDict.GetValueOrEmpty("Increase_Decrease"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_PartOfReprogramming_OS", 			attributeDict.GetValueOrEmpty("Part_of_Reprogramming"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_NumberOfPersonnelQtrs_OS", 		Personnel_Qtrs)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_NumberOfOSQtrs_OS", 				OS_Qtrs)
						
						'Set the parameters for the combo boxes in the RP Dashboard Page2
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_FYRelatedRp1_OS", 					attributeDict.GetValueOrEmpty("FY_Related_RP1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_FYRelatedRp2_OS", 					attributeDict.GetValueOrEmpty("FY_Related_RP2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_FYRelatedRp3_OS", 					attributeDict.GetValueOrEmpty("FY_Related_RP3"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_OlderRelatedRp1_OS", 					attributeDict.GetValueOrEmpty("Older_Related_RP1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_OlderRelatedRp2_OS", 					attributeDict.GetValueOrEmpty("Older_Related_RP2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_OlderRelatedRp3_OS", 					attributeDict.GetValueOrEmpty("Older_Related_RP3"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOffice1_OS", 						attributeDict.GetValueOrEmpty("Lead_Office1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOffice2_OS", 						attributeDict.GetValueOrEmpty("Lead_Office2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOffice3_OS", 						attributeDict.GetValueOrEmpty("Lead_Office3"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePOC1_OS", 					attributeDict.GetValueOrEmpty("Lead_Office_POC1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePOC2_OS", 					attributeDict.GetValueOrEmpty("Lead_Office_POC2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePOC3_OS", 					attributeDict.GetValueOrEmpty("Lead_Office_POC3"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePhone1_OS", 				attributeDict.GetValueOrEmpty("Lead_Office_Phone1"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePhone2_OS", 				attributeDict.GetValueOrEmpty("Lead_Office_Phone2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_LeadOfficePhone3_OS", 				attributeDict.GetValueOrEmpty("Lead_Office_Phone3"))					
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_ExecSummary_OS", 						attributeDict.GetValueOrEmpty("Exec_Summary"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_IE_K_OS", 						attributeDict.GetValueOrEmpty("Initial_Estimate"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_IE_MIL_OS", 						attributeDict.GetValueOrEmpty("Initial_Estimate_MIL_FTP"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_IE_CIV_OS", 						attributeDict.GetValueOrEmpty("Initial_Estimate_CIV_FTP"))			
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_IE_Base_Funding_OS", 				attributeDict.GetValueOrEmpty("Base_Funding"))			
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_IE_Base_Funding_Comments_OS", 		attributeDict.GetValueOrEmpty("Base_Funding_Comments"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_CBF_MIL_OS", 						attributeDict.GetValueOrEmpty("Base_Funding_MIL_FTP"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_CBF_CIV_OS", 						attributeDict.GetValueOrEmpty("Base_Funding_CIV_FTP"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_IE_R_Base_OS", 					attributeDict.GetValueOrEmpty("Recurring_Base_Estimate"))					
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_R_Base_Comments_OS", 				attributeDict.GetValueOrEmpty("Recurring_Base_Comments"))	
						
						'Set the parameters for the combo boxes in the RP Dashboard Page3 (MSN added this 01/20/23)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Page3_AffectOthers_OS", 				attributeDict.GetValueOrEmpty("Affect_Others"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Page3_Alignment_OS", 					attributeDict.GetValueOrEmpty("Alignment"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Page3_DenialImpact_OS", 				attributeDict.GetValueOrEmpty("Denial_Impact"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Page3_FundingImpact_OS", 				attributeDict.GetValueOrEmpty("Funding_Impact"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Page3_Problem_OS", 					attributeDict.GetValueOrEmpty("Problem"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Page3_ROI_OS", 						attributeDict.GetValueOrEmpty("ROI"))
						SetRoutingPageCompat(selectionChangedTaskResult.ModifiedCustomSubstVars, routingAppn, routingAppn & "_RP_Page1")
						SetRoutingContent(selectionChangedTaskResult.ModifiedCustomSubstVars, routingAppn, routingAppn & "_RP_Content")

					End If 'Not globals.GetObject("attributeDict") Is Nothing
					
					'Update session state appropriately
					BRApi.State.SetSessionState(si, False, ClientModuletype.Unknown,"","","dashState","dashState","Edit", si.XfBytes)
					
					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					Return selectionChangedTaskResult												
					
			Return Nothing
		End Function
		Private Function RollForward() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.RollForward (called by non-OS appropriation dashboards) ====
	
			Dim FromScenario As String = args.NameValuePairs("FromScenario")					
			Dim FromScenario_Split As List(Of String) = StringHelper.SplitString(FromScenario, "_")
			Dim FromYear As String = FromScenario_Split(1).Substring(2,2) + 2000

			Dim ToScenario As String = args.NameValuePairs("ToScenario")
			Dim ToYear As String = args.NameValuePairs("ToYear")
			
			Dim FromYearInt As Integer = FromYear.XFConvertToInt
			Dim ToYearInt As Integer = ToYear.XFConvertToInt
			
			Dim RPParent As String = "FY" & ToYearInt-2000 & "_RPs"
			'brapi.ErrorLog.LogMessage(si, "RollOver Function Called" & FromScenario & " " & ToScenario & " " & FromYear & " " & ToYear & " " & RPParent)
			
			Dim params As New Dictionary(Of String, String) 
			params.Add("RF_Cube", "BudFm")
			params.Add("RF_SourceScenario", FromScenario) 
			params.Add("RF_SourceYear", FromYear) 
			params.Add("RF_TargetScenario", ToScenario) 
			params.Add("RF_TargetYear", ToYear) 
			params.Add("RF_RPParent", RPParent) 
			
			
			If FromScenario.XFEqualsIgnoreCase(ToScenario) Then
				' Cannot roll forward from to the same scenario
				Throw New Exception ("From-Scenario and To-Scenario cannot be the same")
			End If
			
			If FromYearInt > ToYearInt Then				
				' Not allowed, you can only for forward to future
				Throw New Exception ("From-Year cannot be later than To-Year")
		
			Else If (ToYearInt-FromYearInt) > 1 Then
				' Not allowed you only roll forward to next year from the From-Year
				Throw New Exception ("Roll Forward is allowed only into Next budget Year")
			
			Else If FromYearInt = ToYearInt Then
				' Roll Forward to the next Scenario in the same year 
				brapi.Utilities.StartDataMgmtSequence(si, "RollForward_BudFm_NextStepInBY", params)	
				
			Else 
				' Roll forward to next year
				brapi.Utilities.StartDataMgmtSequence(si, "RollForward_BudFm_NextFY", params)	
				
			End If
							
			Return Nothing
		End Function
		Private Function UpdateTextValue() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.UpdateTextValue (called by non-OS appropriation dashboards) ====
						Dim updateSql As New Text.StringBuilder 
						Using dbConnApp As DBConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						updateSql.Append("Update ")	
	            		updateSql.Append(" dbo.DataAttachment ")
						updateSql.Append(" set Text = '76' ")
						updateSql.Append(" Where Cube = 'BudFm' ")
						updateSql.Append(" AND Time = '2026' ")
						updateSql.Append(" AND Scenario = 'CJ_FY26' ")	
						
						updateSql.Append(" AND Flow = '26_2035_00' ")
						updateSql.Append(" AND Text = 'SAM JONES' ")
					
						'execute the update query 
						BRApi.Database.ExecuteSql(dbConnApp, updateSql.ToString, False)
					
					End Using
			Return Nothing
		End Function
		Private Function Update_RP_TermBillet() As Object
			' ==== ported verbatim from BudFM_SolutionHelper.Update_RP_TermBillet (called by non-OS appropriation dashboards) ====
									
	Try
'        Dim sql As New Text.StringBuilder
'			sql.Append("UPDATE dbo.DataAttachment ")
'			sql.Append(" SET Text = 'Perm' ")
'           sql.Append(" WHERE Cube = 'BudFm' ")
'			sql.Append(" AND Scenario = 'RAP_FY26'")
'           sql.Append(" AND Flow = '26_2050_00' ")
'			sql.Append(" AND Account  like '%Term_Billet%' ")
'			sql.Append(" AND UD6 like '%LineItem_02%' ")
'			sql.Append(" and Text like '%Term_Na%' ")
			
  
         'This one to update all records that have a Term_Na in text column and Account = Term_Billet setting the Text column to Perm
	      Dim sql As New Text.StringBuilder
			sql.Append("UPDATE dbo.DataAttachment ")
			sql.Append(" SET Text = 'Perm' ")
            sql.Append(" WHERE Cube = 'BudFm' ")
			sql.Append(" AND Account  like '%Term_Billet%' ")
			sql.Append(" and Text like '%Term_Na%' ")

			Dim sqlStmt As String = sql.ToString
			BrApi.ErrorLog.LogMessage (si, sqlStmt)
							
			Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
	        	Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp, sqlStmt, True)
			End Using
		Return Nothing
		
		
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
			Return Nothing
		End Function


		' ===== helper defs carried verbatim from BudFM_SolutionHelper =====
		
				
#Region "HelperFunctions"


		Private Function EditScenarioSecurityHelper(ByVal globals As BRGlobals, ByVal si As SessionInfo, ByVal SelectedReadGroup As String, ByVal SelectedWriteGroup As String, ByVal workScen As String)
			Try
				
				'Create Selected group name variables
				Dim SelectedReadGroupName As String = ""
				Dim SelectedWriteGroupName As String = ""
				
				'Define roles from parameters
				Dim grpAllUsers As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Security_BudFm_s_AllUsers")
				Dim grpOfficeUsers As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Security_BudFm_s_OfficeandPowerUsers")
				Dim grpPowerUsers As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Security_BudFm_s_PowerUsers")
				Dim grpFmExecutionUsers As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Security_BudFm_s_AllUsers_InclExecution")
				
				'Assign Selected Read group name variables
				If SelectedReadGroup = "All Users" Then
					If (workScen.Contains("Enacted")) Or (workScen.Contains("CJ")) Then
						SelectedReadGroupName = grpFmExecutionUsers 'grpAllUsers
					Else
						SelectedReadGroupName = grpAllUsers 'grpAllUsers
					End If 
					
				Else If SelectedReadGroup = "Office Users and Power Users" Then
					SelectedReadGroupName = grpOfficeUsers
					
				Else If SelectedReadGroup = "Power Users Only" Then
					SelectedReadGroupName = grpPowerUsers
									
				End If
				
				'Assign Selected Write group name variables	
				If SelectedWriteGroup = "Office Users and Power Users" Then
					SelectedWriteGroupName = grpOfficeUsers
					
				Else If SelectedWriteGroup = "Power Users Only" Then
					SelectedWriteGroupName = grpPowerUsers
					
				End If
				'Get Unique ID for current groups
				Dim guidCurrentReadGroup As Guid = BRApi.Finance.Members.GetMember(si, 2, workScen).ReadDataGroupUniqueID
				Dim guidCurrentWriteGroup As Guid = BRApi.Finance.Members.GetMember(si, 2, workScen).ReadWriteDataGroupUniqueID
				
				'Get Unique ID for selected groups
				Dim guidSelectedReadGroup As Guid = BRApi.Security.Admin.GetGroup(si, SelectedReadGroupName).Group.UniqueID
				Dim guidSelectedWriteGroup As Guid = BRApi.Security.Admin.GetGroup(si, SelectedWriteGroupName).Group.UniqueID
										
				'Get the Unique ID from the Auditor, OfficeUser, and PowerUser roles
				Dim guidAuditors As Guid = BRApi.Security.Admin.GetGroup(si, grpAllUsers).Group.UniqueID
				Dim guidExec As Guid = BRApi.Security.Admin.GetGroup(si, grpFmExecutionUsers).Group.UniqueID
				Dim guidOfficeUsers As Guid = BRApi.Security.Admin.GetGroup(si, grpOfficeUsers).Group.UniqueID
				Dim guidPowerUsers As Guid = BRApi.Security.Admin.GetGroup(si, grpPowerUsers).Group.UniqueID

				'Determine if there is an update to either the Read Group or the Write Group
				Dim updateStatusRead As Boolean = True
				Dim updateStatusWrite As Boolean = True
				
				If SelectedReadGroup = String.Empty Then
					updateStatusRead = False
				Else If guidCurrentReadGroup = guidSelectedReadGroup Then
					updateStatusRead = False	
				Else 
					updateStatusRead = True	
				End If
				
				If SelectedWriteGroup = String.Empty Then
					updateStatusWrite = False	
				Else If guidCurrentWriteGroup = guidSelectedWriteGroup Then
					updateStatusWrite = False							
				Else 
					updateStatusWrite = True							
				End If
				
				'If updates were made, proceed to make the update
				If updateStatusRead = True Or updateStatusWrite = True Then
					Dim BudFm_ScenarioDim As OneStream.Shared.Wcf.Dim = BRApi.Finance.Dim.GetDim(si, "BudFm_Scenario")
					Dim scenarioMember As Member = BRApi.Finance.Members.GetMember(si, 2, workScen)
					Dim scenarioName As String = scenarioMember.Name
					Dim scenarioID As Integer = scenarioMember.MemberId
					Dim scenarioPk As New MemberPk(BudFm_ScenarioDim.DimPk.DimTypeId, scenarioID)
					Dim scenarioMemberInfo As MemberInfo = BRApi.Finance.Members.GetMemberInfo(si, dimtypeid.Scenario, scenarioID, True)
					Dim scenarioDesc As String = scenarioMember.Description
					Dim scenarioToUpdate As New Member(scenarioPk, scenarioName, scenarioDesc, BudFm_ScenarioDim.DimPk.DimId)
					Dim scenarioVarProps As VaryingMemberProperties = scenarioMemberInfo.Properties
					Dim scenarioToUpdateInfo As New MemberInfo(scenarioToUpdate, scenarioVarProps, Nothing, BudFm_ScenarioDim, DimConstants.Unknown)
					Dim scenarioMemberProperties As ScenarioVMProperties = scenarioToUpdateInfo.GetScenarioProperties()
					Dim writeableMbr As New WritableMember(scenarioMember)
									
					'Set the New "Read Data" Group
					If updateStatusRead = True
						writeableMbr.ReadDataGroupUniqueID = guidSelectedReadGroup
						
					Else 
						
					End If
					
					'Set the New "Read and Write Data" Group and "Manage Data" Group						
					If updateStatusWrite = True Then
						writeableMbr.ReadWriteDataGroupUniqueID = guidSelectedWriteGroup							
						scenarioMemberProperties.ManageDataGroup.SetStoredValue(guidSelectedWriteGroup)
					End If
					
					'Save the member
					Dim isExisting As TriStateBool = TriStateBool.FalseValue
					BRapi.Finance.MemberAdmin.SaveMemberInfo(si, True, writeableMbr, True, scenarioVarProps, False, New List(Of MemberDescription), isExisting)
					
				End If
				Return Nothing
			Catch ex As Exception				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
       Private Function IsOSPG1Empty(ByVal globals As BRGlobals, ByVal si As SessionInfo, ByVal wfCube As String, ByVal RP_Entity As String, ByVal wfScenario As String, ByVal wfTime As String, ByVal RPName As String)
			Try
			
				Dim PG1scriptGenerics As String 		= "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim PG1sNumber_of_Billets As String 	= ""
				Dim PG1Add_General_Detail As String 	= ""
				Dim PG1Increase_Decrease As String 		= ""
				Dim PG1Part_of_Reprogramming As String 	= ""
				Dim PG1Personnel_Qtrs As String 		= ""
				Dim PGOS_Qtrs1 As String 				= ""
	
				globals.SetStringValue("scriptGenerics", PG1scriptGenerics)
				globals.SetStringValue("parAccount", "RP_Attributes")					
	
				'Set a generic dictionary as an argument in the rule below
				Dim Dictionary As New Dictionary(Of String, String)
				
					BUDFM_AttributeSupport.GetRPAttributes(si, globals)
				
				If Not globals.GetObject("attributeDict") Is Nothing
				
				Dim attributeDict As Dictionary(Of String, String) = globals.GetObject("attributeDict")
						
					'Set the parameters for the combo boxes in the RP Dashboard Page1
					PG1sNumber_of_Billets=  	attributeDict.GetValueOrEmpty("Number_of_Billets")
					PG1Add_General_Detail =		attributeDict.GetValueOrEmpty("Add_General_Detail")
					PG1Increase_Decrease=		attributeDict.GetValueOrEmpty("Increase_Decrease")
					PG1Part_of_Reprogramming=	attributeDict.GetValueOrEmpty("Part_of_Reprogramming")
					PG1Personnel_Qtrs =			attributeDict.GetValueOrEmpty("Personnel_Qtrs")
					PGOS_Qtrs1=					attributeDict.GetValueOrEmpty("OS_Qtrs")
					
				End If	
			
				Return String.IsNullOrEmpty(PG1sNumber_of_Billets)	OrElse String.IsNullOrEmpty(PG1Add_General_Detail)	OrElse String.IsNullOrEmpty(PG1Increase_Decrease)	OrElse String.IsNullOrEmpty(PG1Part_of_Reprogramming)	OrElse String.IsNullOrEmpty(PG1Personnel_Qtrs)	OrElse String.IsNullOrEmpty(PGOS_Qtrs1)		
			Catch ex As Exception				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function


		Private Function GetDescription(ByVal si As SessionInfo, ByVal RPname As String)
			Try
				
				Dim RPMemId As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Flow, RPname)
				If RPmemId = -1 Then
					Throw New Exception ("RP does not exist " &RPname)
				End If
				Dim RPMemberInfo As MemberInfo = BRApi.Finance.Members.GetMemberInfo(si, dimtypeid.Flow, RPMemId, True)
				Return RPMemberInfo.NameandDescription

			Catch ex As Exception				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Private Function GetUD6Description(ByVal si As SessionInfo, ByVal RPname As String)
			Try
				
				Dim RPMemId As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD6, RPname)
				If RPmemId = -1 Then
					Throw New Exception ("RP does not exist " &RPname)
				End If
				Dim RPMemberInfo As MemberInfo = BRApi.Finance.Members.GetMemberInfo(si, dimtypeid.UD6, RPMemId, True)
				Return RPMemberInfo.Description

			Catch ex As Exception				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Private Function CostCalc(ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs, ByVal rp_Entity As String, ByVal rpName As String, ByVal wfTime As Integer)
			Try
				     '**********Updated for OS***********
				Dim params As New Dictionary(Of String, String) 
				params.Add("rpEntity", rp_Entity)
				params.Add("prm_Number", rpName) 
				params.Add("WFTime", wfTime) 				
				
				brapi.Utilities.StartDataMgmtSequence(si, "Calc_Single_RP", params)		
					
							
				Return Nothing
			Catch ex As Exception				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Private Function CostClear(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal args As DashboardExtenderArgs, ByVal rp_Entity As String, ByVal rpName As String, 
						ByVal wfCube As String, ByVal wfScenario As String, ByVal wfTime As Integer, ByVal LineItemNum As String)
			Try
					     
				Dim povInfo As New Dictionary(Of String, String) 
				povInfo.Add("Cube", wfCube)
				povInfo.Add("Consolidation", "Local")
				povInfo.Add("Scenario", wfScenario)
				povInfo.Add("View", "Periodic")
				povInfo.Add("Entity", rp_Entity)
				povInfo.Add("Time", wfTime)		
				
				'stopped using this approach on 7/9/23 when implemented Execution via custom calc call below
'				brapi.Utilities.StartDataMgmtSequence(si, "Clear_Single_RP_LI_Cost", params)

				globals.SetStringValue("rpEntity", rp_Entity) 
				globals.SetStringValue("rpName", rpName) 
				globals.SetStringValue("WFTime", wfTime) 		
				globals.SetStringValue("LineItemNum", LineItemNum)		
				
				brapi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si, "USCG_RP_CostCalc_Billet", "ClearLICost", povInfo, customcalculatetimetype.MemberFilter)	
				
				Return Nothing
			Catch ex As Exception				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Private Function NonBilletCostClear(ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs, ByVal rp_Entity As String, ByVal rpName As String, 
						ByVal wfTime As Integer, ByVal LineItemNum As String)
			Try
			     '**********Updated for OS***********
				Dim params As New Dictionary(Of String, String) 
				params.Add("rpEntity", rp_Entity)
				params.Add("rpName", rpName) 
				params.Add("WFTime", wfTime) 		
				params.Add("LineItemNum", LineItemNum)		
				
				brapi.Utilities.StartDataMgmtSequence(si, "Clear_Single_RP_LI_NBCost", params)		
							
				Return Nothing
			Catch ex As Exception				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
				
		Private Function CopyBilletAllFields(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal args As DashboardExtenderArgs, ByVal wfCube As String, ByVal wfTime As String, 
						ByVal wfScenario As String, ByVal RP_Entity As String, ByVal rpName As String, ByVal LINumberSource As String, ByVal LINumberDestination As String)
			Try
				
						'Storing the Annotation text for the attributes in a generic string
						Dim scriptGenericsSource As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumberSource & ":U7#None:U8#None"						
						Dim scriptGenericsDestination As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumberDestination & ":U7#None:U8#None"						
						
						'using a global function to avoid using brapi functions too many times and use api.data.calculate via a finance rule instead
						'set the script generics and parent account to be used in the global function
						globals.SetStringValue("scriptGenerics", scriptGenericsSource)
						globals.SetStringValue("parAccount", "Billet_LineItem_Data")					

						'Set a generic dictionary as an argument in the rule below
						Dim Dictionary As New Dictionary(Of String, String)
						
						BUDFM_AttributeSupport.GetRPAttributes(si, globals)
							
						'Create a new list of memberscript and value
						Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)
							
						If Not globals.GetObject("attributeDict") Is Nothing
						
							Dim attributeDict As Dictionary(Of String, String) = globals.GetObject("attributeDict")
							
							'Create the script for the destination billet and add it to the list
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Billet_Type:" 					& scriptGenericsDestination, 0, True, attributeDict.GetValueOrEmpty("Billet_Type")))
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Grade_Type:" 					& scriptGenericsDestination, 0, True, attributeDict.GetValueOrEmpty("Grade_Type")))
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Grade_Rank:" 					& scriptGenericsDestination, 0, True, attributeDict.GetValueOrEmpty("Grade_Rank")))
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#AD_Reserve:" 					& scriptGenericsDestination, 0, True, attributeDict.GetValueOrEmpty("AD_Reserve")))
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Reserve_Type:" 					& scriptGenericsDestination, 0, True, attributeDict.GetValueOrEmpty("Reserve_Type")))
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Spe_Code_Occu_Series:" 			& scriptGenericsDestination, 0, True, attributeDict.GetValueOrEmpty("Spe_Code_Occu_Series")))
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Pilot:" 						& scriptGenericsDestination, 0, True, attributeDict.GetValueOrEmpty("Pilot")))
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Electronic_Flight_Bag:" 		& scriptGenericsDestination, 0, True, attributeDict.GetValueOrEmpty("Electronic_Flight_Bag")))
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Position_Number:" 				& scriptGenericsDestination, 0, True, attributeDict.GetValueOrEmpty("Position_Number")))
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Position_Title:" 				& scriptGenericsDestination, 0, True, attributeDict.GetValueOrEmpty("Position_Title")))
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Billet_ATU:" 					& scriptGenericsDestination, 0, True, attributeDict.GetValueOrEmpty("Billet_ATU")))
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#OPFAC:" 						& scriptGenericsDestination, 0, True, attributeDict.GetValueOrEmpty("OPFAC")))	
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Billet_PPA:" 					& scriptGenericsDestination, 0, True, attributeDict.GetValueOrEmpty("Billet_PPA")))
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Billet_UII:" 					& scriptGenericsDestination, 0, True, attributeDict.GetValueOrEmpty("Billet_UII")))
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Billet_Object_Class:" 			& scriptGenericsDestination, 0, True, attributeDict.GetValueOrEmpty("Billet_Object_Class")))
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#CONUS_OCONUS:" 					& scriptGenericsDestination, 0, True, attributeDict.GetValueOrEmpty("CONUS_OCONUS")))
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Detached_Duty:" 				& scriptGenericsDestination, 0, True, attributeDict.GetValueOrEmpty("Detached_Duty")))
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Detached_Duty_Location:" 		& scriptGenericsDestination, 0, True, attributeDict.GetValueOrEmpty("Detached_Duty_Location")))
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Term_Billet:" 					& scriptGenericsDestination, 0, True, attributeDict.GetValueOrEmpty("Term_Billet")))
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#PPE_Type:" 						& scriptGenericsDestination, 0, True, attributeDict.GetValueOrEmpty("PPE_Type")))	
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#PPE_PPA:" 						& scriptGenericsDestination, 0, True, attributeDict.GetValueOrEmpty("PPE_PPA")))
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#PPE_ATU:" 						& scriptGenericsDestination, 0, True, attributeDict.GetValueOrEmpty("PPE_ATU")))
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Build_Out_Choice:" 				& scriptGenericsDestination, 0, True, attributeDict.GetValueOrEmpty("Build_Out_Choice")))
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#ICASS_Costs:" 					& scriptGenericsDestination, 0, True, attributeDict.GetValueOrEmpty("ICASS_Costs")))
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Background_Investigation_Type:" & scriptGenericsDestination, 0, True, attributeDict.GetValueOrEmpty("Background_Investigation_Type")))
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Acquisition_Project:" 			& scriptGenericsDestination, 0, True, attributeDict.GetValueOrEmpty("Acquisition_Project")))
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Lease_Choice:" 					& scriptGenericsDestination, 0, True, attributeDict.GetValueOrEmpty("Lease_Choice")))
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Lease_PPA:" 					& scriptGenericsDestination, 0, True, attributeDict.GetValueOrEmpty("Lease_PPA")))
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Lease_ATU:" 					& scriptGenericsDestination, 0, True, attributeDict.GetValueOrEmpty("Lease_ATU")))
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Furniture_Reqd:" 				& scriptGenericsDestination, 0, True, attributeDict.GetValueOrEmpty("Furniture_Reqd")))
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Utilities_Reqd:" 				& scriptGenericsDestination, 0, True, attributeDict.GetValueOrEmpty("Utilities_Reqd")))
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Computer_Type:" 				& scriptGenericsDestination, 0, True, attributeDict.GetValueOrEmpty("Computer_Type")))
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#LineItem_Comment:" 				& scriptGenericsDestination, 0, True, attributeDict.GetValueOrEmpty("LineItem_Comment")))
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Utilities_PPA:" 				& scriptGenericsDestination, 0, True, attributeDict.GetValueOrEmpty("Utilities_PPA")))
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Utilities_ATU:" 				& scriptGenericsDestination, 0, True, attributeDict.GetValueOrEmpty("Utilities_ATU")))
										
							
	'						'********Allocation Drivers Storage********									
	'						'For those attributes that are also a dimension, we will also store a 1 in that dimension member that is selected so we can find it in a data buffer for the cost calc	
							Me.AllocationsCalc(si, globals, args, RP_Entity, RPName, wfCube, wfScenario, wfTime, 
												LINumberDestination, 
												attributeDict.GetValueOrEmpty("Billet_PPA"), 
												attributeDict.GetValueOrEmpty("Billet_UII"), 
												attributeDict.GetValueOrEmpty("Billet_Object_Class"), 
												attributeDict.GetValueOrEmpty("Billet_ATU"), 
												attributeDict.GetValueOrEmpty("PPE_PPA"), 
												attributeDict.GetValueOrEmpty("PPE_ATU"), 
												attributeDict.GetValueOrEmpty("Utilities_PPA"), 
												attributeDict.GetValueOrEmpty("Utilities_ATU"), 
												attributeDict.GetValueOrEmpty("Lease_PPA"), 
												attributeDict.GetValueOrEmpty("Lease_ATU"))	
								
								
							'********Headcount Reporting Storage********
							Dim hcScriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Periodic:F#" & RPName & ":O#Forms:I#None:U6#" & LINumberDestination & ":U7#None:U8#None"			
							
							'set the Aviator variable
							Dim aviator As String = String.Empty
							If attributeDict.GetValueOrEmpty("Pilot") = "Y"
								aviator = "Aviator"
							ElseIf attributeDict.GetValueOrEmpty("Pilot") = "N"
								aviator = "NA_Aviator"
							End If
							
							'Set the military employment type variable
							Dim milEmpType As String = String.Empty
							If attributeDict.GetValueOrEmpty("AD_Reserve").XFEqualsIgnoreCase("Active_Duty")
								milEmpType = attributeDict.GetValueOrEmpty("AD_Reserve")
							ElseIf attributeDict.GetValueOrEmpty("AD_Reserve").XFEqualsIgnoreCase("Reserve")
								milEmpType = attributeDict.GetValueOrEmpty("Reserve_Type")
							Else 
								milEmpType = "NA_Military_Employment_Type"
							End If
							
							'get the increase_decrease value
							Dim increase_Decrease As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Increase_Decrease:E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").DataCellEx.DataCellAnnotation

							'Run the Headcount Calc
							Me.HeadcountCalc(si, globals, args, RP_Entity, RPName, wfCube, wfScenario, wfTime, LINumberDestination, attributeDict.GetValueOrEmpty("Grade_Rank"), milEmpType, attributeDict.GetValueOrEmpty("Spe_Code_Occu_Series"), attributeDict.GetValueOrEmpty("CONUS_OCONUS"), aviator)
							
								
							'Write the annotations to the database
							Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)		
									
						End If 'Not globals.GetObject("attributeDict") Is Nothing
								
							
				Return Nothing
			Catch ex As Exception				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		
		Private Function DeleteMassBillets(ByVal si As SessionInfo,ByVal globals As BRGlobals,ByVal args As DashboardExtenderArgs, ByVal RP_Entity As String, ByVal RPName As String, 
						ByVal wfCube As String, ByVal wfScenario As String, ByVal wfTime As Integer, ByVal description_ChangeLog As String,ByVal reason_ChangeLog As String, ByVal billetD As String)
						
						
						'RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog,billetD)
						'RunPreSaveStepsForRP_BLT_Deletion(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, billetD)
						Dim LineItemNumIntLength As Integer = billetD.Length
						Dim LineItemNumInt As Integer
						If LineItemNumIntLength = 11
							LineItemNumInt = billetD.Substring(9,2).XFConvertToInt
						Else If LineItemNumIntLength = 12
							LineItemNumInt = billetD.Substring(9,3).XFConvertToInt
						End If
						
						Using dbConnApp As DBConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							
						'1) First, Delete the current line item from the data attachment table
						Dim deleteSql As New Text.StringBuilder  				
						deleteSql.Append("Delete ")		
						deleteSql.Append("From dbo.DataAttachment ")
						deleteSql.Append(" Where Cube = '" & wfCube & "' ")
						deleteSql.Append(" AND Time = '" & wfTime & "' ")
						deleteSql.Append(" AND Scenario = '" & wfScenario & "' ")	
						deleteSql.Append(" AND Entity = '" & RP_Entity & "' ")
						deleteSql.Append(" AND Flow = '" & RPName & "' ")
						deleteSql.Append(" AND UD6 = '" & billetD & "' ")
						'execute the query 
						BRApi.Database.ExecuteSql(dbConnApp, deleteSql.ToString, False)
						
'						2) Next, Update the line items to move them down 1. E.g. LineItem_02 becomes LineItem_01
						Dim updateSql As New Text.StringBuilder 
						updateSql.Append("Update ")	
	            		updateSql.Append(" dbo.DataAttachment ")
						updateSql.Append(" set UD6 = Replace(UD6, substring(UD6, 10, 3), format((Convert(INT, substring(UD6, 10, 3))-1), '0#')) ")
						updateSql.Append(" Where Cube = '" & wfCube & "' ")
						updateSql.Append(" AND Time = '" & wfTime & "' ")
						updateSql.Append(" AND Scenario = '" & wfScenario & "' ")	
						updateSql.Append(" AND Entity = '" & RP_Entity & "' ")
						updateSql.Append(" AND Flow = '" & RPName & "' ")
						updateSql.Append(" AND substring(UD6, 0, 10) = 'LineItem_' ")
						updateSql.Append(" AND Convert(INT, substring(UD6, 10, 3)) > " & LineItemNumInt & " ")
						'execute the update query 
					      BRApi.Database.ExecuteSql(dbConnApp, updateSql.ToString, False)
''					
						
						
						'3)Update the actual stored data using a finance business rule						
						Dim povInfo As New Dictionary(Of String, String) 
						povInfo.Add("Cube", wfCube)
						povInfo.Add("Consolidation", "Local")
						povInfo.Add("Scenario", wfScenario)
						povInfo.Add("View", "Periodic")
						povInfo.Add("Entity", rp_Entity)
						povInfo.Add("Time", wfTime)
						
						globals.SetStringValue("WFTime", wfTime) 
						globals.SetStringValue("rpName", rpName) 		
						globals.SetStringValue("LineItemNum", billetD)
						
						brapi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si, "USCG_RP_CostCalc_Billet", "DeleteBillet", povInfo, customcalculatetimetype.MemberFilter)
						

                   End Using
				
						
			Return Nothing
		End Function
		
		Private Function CopyNonBilletAllFields(ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs, ByVal wfCube As String, ByVal wfTime As String, 
						ByVal wfScenario As String, ByVal RP_Entity As String, ByVal rpName As String, ByVal LINumberSource As String, ByVal LINumberDestination As String)
			Try

						
						Dim scriptGenerics As String      = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumberSource & ":U7#None:U8#None"		
						Dim scriptGenericsDescr As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_Input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U7#None:U8#None"						
						
						Dim requested_Item_Tier1 As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, 			"A#Requested_Item_Tier1:" 											& scriptGenerics 			& ":U6#"& LINumberSource).DataCellEx.DataCellAnnotation						
						Dim description_Tier2_ToUse As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, 		"A#Description_Tier2:" 												& scriptGenerics 			& ":U6#"& LINumberSource).DataCellEx.DataCellAnnotation								
						Dim description_Tier2_Input_ToUse As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, 	"U5#" 						& description_Tier2_ToUse & ":" 		& scriptGenericsDescr 		& ":U6#"& LINumberSource).DataCellEx.DataCellAnnotation										
						Dim pOC As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, 							"A#POC:" 															& scriptGenerics 			& ":U6#"& LINumberSource).DataCellEx.DataCellAnnotation						
						Dim dollarK_Value As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, 					"A#DollarK_Value:" 													& scriptGenerics 			& ":U6#"& LINumberSource).DataCellEx.DataCellAnnotation						
						Dim r_NR As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, 							"A#R_NR:" 															& scriptGenerics 			& ":U6#"& LINumberSource).DataCellEx.DataCellAnnotation						
						Dim aTU_NoUnit As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, 						"A#ATU:" 															& scriptGenerics 			& ":U6#"& LINumberSource).DataCellEx.DataCellAnnotation						
						Dim pPA As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, 							"A#PPA:" 															& scriptGenerics 			& ":U6#"& LINumberSource).DataCellEx.DataCellAnnotation						
						Dim uII As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, 							"A#UII:" 															& scriptGenerics 			& ":U6#"& LINumberSource).DataCellEx.DataCellAnnotation						
						Dim object_Class As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, 					"A#Object_Class:" 													& scriptGenerics 			& ":U6#"& LINumberSource).DataCellEx.DataCellAnnotation
											
						
						'Create a new list of memberscript and value
						Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)
						
						'Create the script for the destination non-billet and add it to the list
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Requested_Item_Tier1:" 											& scriptGenerics 		& ":U6#" & LINumberDestination, 0, True, requested_Item_Tier1))
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Description_Tier2:" 											& scriptGenerics 		& ":U6#" & LINumberDestination, 0, True, description_Tier2_ToUse))
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "U5#" 							& description_Tier2_ToUse & ":" 	& scriptGenericsDescr 	& ":U6#" & LINumberDestination, 0, True, description_Tier2_Input_ToUse))
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#POC:" 															& scriptGenerics 		& ":U6#" & LINumberDestination, 0, True, pOC))
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#DollarK_Value:" 												& scriptGenerics 		& ":U6#" & LINumberDestination, 0, True, dollarK_Value))
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#R_NR:" 															& scriptGenerics 		& ":U6#" & LINumberDestination, 0, True, r_NR))
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#ATU:" 															& scriptGenerics 		& ":U6#" & LINumberDestination, 0, True, aTU_NoUnit))
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#PPA:" 															& scriptGenerics 		& ":U6#" & LINumberDestination, 0, True, pPA))
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#UII:" 															& scriptGenerics 		& ":U6#" & LINumberDestination, 0, True, uII))
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Object_Class:" 													& scriptGenerics 		& ":U6#" & LINumberDestination, 0, True, object_Class))
															
								
'						'********Allocation Drivers Storage********									
'						'For those attributes that are also a dimension, we will also store a 1 in that dimension member that is selected so we can find it in a data buffer for the cost calc	
						Me.NBAllocationsCalc(si, args, RP_Entity, RPName, wfTime, LINumberDestination, pPA, uII, object_Class, aTU_NoUnit)		
						
						'Files
						Dim strRefDocType As String = "Reference_Doc"						
						Dim sqlUpdate As New Text.StringBuilder                                                       
							sqlUpdate.Append("Update dbo.DataAttachment ")
							sqlUpdate.Append(" set UD6 = '" & LINumberDestination & "' ")
							sqlUpdate.Append(" WHERE Time = '" & wfTime & "' ")
							sqlUpdate.Append(" AND Flow = '" & rpName & "' ")
							sqlUpdate.Append(" AND Scenario = '" & wfScenario & "' ")
							sqlUpdate.Append(" AND UD6 = '" & LINumberSource & "' ")
							sqlUpdate.Append(" AND Account = '" & strRefDocType & "' ")
						
						Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
							Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp, sqlUpdate.ToString, False)
						End Using 

						'Write the annotations to the database
						Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)
												
				Return Nothing
			Catch ex As Exception				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Private Function ClearBillet(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal args As DashboardExtenderArgs, ByVal wfCube As String, ByVal wfScenario As String, ByVal wfTime As String, 
						ByVal RP_Entity As String, ByVal rpName As String, ByVal LineItemNum As String, ByVal LineItemNumInt As Integer, ByVal scriptgenerics As String)
			Try
							
	
					'Create a new list of memberscript and value
					Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)
					
					'Create the script for the billet and add it to the list
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Billet_Type:" 					& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Grade_Type:" 					& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Grade_Rank:" 					& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#AD_Reserve:" 					& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Reserve_Type:" 					& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Spe_Code_Occu_Series:" 			& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Pilot:" 						& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Electronic_Flight_Bag:" 		& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Term_Billet:" 					& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#PPE_Type:" 						& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))	
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#PPE_PPA:" 						& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#PPE_ATU:" 						& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Build_Out_Choice:" 				& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#ICASS_Costs:" 					& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Position_Number:" 				& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Position_Title:" 				& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Billet_ATU:" 					& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#CONUS_OCONUS:" 					& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Billet_UII:" 					& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Billet_PPA:" 					& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Billet_Object_Class:" 			& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#CONUS_OCONUS:" 					& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#OPFAC:" 						& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Detached_Duty:" 				& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Detached_Duty_Location:" 		& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Background_Investigation_Type:" & scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Lease_Choice:" 					& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Lease_PPA:" 					& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Lease_ATU:" 					& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Acquisition_Project:" 			& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Furniture_Reqd:" 				& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Utilities_Reqd:" 				& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Computer_Type:" 				& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#LineItem_Comment:" 				& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Utilities_PPA:" 				& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Utilities_ATU:" 				& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
												
					'********Allocation Drivers Storage********									
					'For those attributes that are also a dimension, we will clear that dimension member that was selected
					Me.AllocationsClear(si, globals, args, RP_Entity, RPName, wfCube, WFScenario, wfTime, LineItemNum)						
								
						
					'********Headcount Reporting Storage********							
					'Clear the Headcount
					Me.HeadcountClear(si, globals, args, RP_Entity, RPName, wfCube, wfScenario, wfTime, LineItemNum)
					
					
					'********Cost Storage********							
					'Clear the Cost
					Me.CostClear(si, globals, args, RP_Entity, RPName, wfCube, wfScenario, wfTime, LineItemNum)							
							
'					'Reduce the number of billets by one.  Commenting this out per meeting with Ranga on 10/4/2022
'					'Edit A#Number_of_Billets and Add it to the list
'					Dim Number_of_Billets_mbrScriptAndValue As New MemberScriptAndValue(wfCube, "A#Number_of_Billets:" & scriptGenerics & ":U5#None", 0, True, (LineItemNumInt-1).ToString)
'						lstMemberScriptAndValue.Add(Number_of_Billets_mbrScriptAndValue)					
					
					'Write the annotations to the database
					Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)		
										
							
				Return Nothing
			Catch ex As Exception				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Private Function ClearNonBillet(ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs, ByVal wfScenario As String, ByVal wfCube As String, 
						ByVal wfTime As String, ByVal rp_Entity As String, ByVal rpName As String, ByVal LineItemNum As String, ByVal LineItemNumInt As Integer, 
						ByVal scriptgenerics As String)

		Try
			
					'Create a new list of memberscript and value
					Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)
					
					'Create the script for the non-billet and add it to the list
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Requested_Item_Tier1:" 												& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Description_Tier2:" 												& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					'lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#" 								& description_Tier2_ToUse & ":" 	& scriptGenerics, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#POC:" 																& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Reference_Doc:" 													& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#DollarK_Value:" 													& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#R_NR:" 																& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#ATU:" 																& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#PPA:" 																& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#UII:" 																& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))         
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Object_Class:" 														& scriptGenerics & ":U6#" & LineItemNum, 0, True, String.Empty))
	
					'********Cost Storage********							
					'Clear the Cost							
					Me.NonBilletCostClear(si, args, rp_Entity, RPName, wfTime, LineItemNum)
					Dim strRefDocType As String = "Reference_Doc"

					'brapi.ErrorLog.LogMessage(si, "uniqueID " &uniqueID)
					'Delete Files
					Dim sqlDelete As New Text.StringBuilder
					sqlDelete.Append("DELETE FROM dbo.DataAttachment ")
            		sqlDelete.Append("WHERE Cube = '" & wfCube & "' ")
					sqlDelete.Append("AND Time = '" & wfTime & "' ")
					sqlDelete.Append("AND Flow = '" & rpName & "' ")
					sqlDelete.Append("AND Scenario = '" & wfScenario & "' ")
					sqlDelete.Append("AND UD6 = '" & LineItemNum & "' ")
					sqlDelete.Append("AND Account = '" & strRefDocType & "' ")
					
					Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
	        		   	Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp, sqlDelete.ToString, True)
					End Using
					
					'Write the annotations to the database
					Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)
													
				Return Nothing
			Catch ex As Exception				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
			
		Private Function AllocationsCalc(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal args As DashboardExtenderArgs, ByVal rp_Entity As String, ByVal rpName As String, 
						ByVal wfCube As String, ByVal wfScenario As String, ByVal wfTime As Integer, ByVal LineItemNum As String, ByVal billet_PPA As String, ByVal billet_UII As String,
						ByVal billet_Object_Class As String, ByVal billet_ATU_NoUnit As String, ByVal pPE_PPA As String, ByVal ppe_ATU_NoUnit As String, 
						ByVal UTL_PPA As String, ByVal UTL_ATU_NoUnit As String, ByVal lease_PPA As String, ByVal lease_ATU_NoUnit As String)
			Try
	
				Dim povInfo As New Dictionary(Of String, String) 
				povInfo.Add("Cube", wfCube)
				povInfo.Add("Consolidation", "Local")
				povInfo.Add("Scenario", wfScenario)
				povInfo.Add("View", "Periodic")
				povInfo.Add("Entity", rp_Entity)
				povInfo.Add("Time", wfTime)
				
				'stopped using this approach on 7/9/23 when implemented Execution via custom calc call below
				'brapi.Utilities.StartDataMgmtSequence(si, "Calc_Single_RP_LI_Allocations", params)	

				
				globals.SetStringValue("rpName", rpName) 
				globals.SetStringValue("WFTime", wfTime) 		
				globals.SetStringValue("LineItemNum", LineItemNum)
				globals.SetStringValue("billet_PPA", billet_PPA)
				globals.SetStringValue("billet_UII", billet_UII)
				globals.SetStringValue("billet_Object_Class", billet_Object_Class)
				globals.SetStringValue("billet_ATU_NoUnit", billet_ATU_NoUnit)
				globals.SetStringValue("pPE_PPA", pPE_PPA)
				globals.SetStringValue("ppe_ATU_NoUnit", ppe_ATU_NoUnit)
				globals.SetStringValue("UTL_PPA", UTL_PPA)
				globals.SetStringValue("UTL_ATU_NoUnit", UTL_ATU_NoUnit)
				globals.SetStringValue("lease_PPA", lease_PPA)
				globals.SetStringValue("lease_ATU_NoUnit", lease_ATU_NoUnit)
				
				brapi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si, "USCG_RP_CostCalc_Billet", "CalculateLIAllocations", povInfo, customcalculatetimetype.MemberFilter)
					
							
				Return Nothing
			Catch ex As Exception				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Private Function NBAllocationsCalc(ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs, ByVal rp_Entity As String, ByVal rpName As String, 
						ByVal wfTime As Integer, ByVal LineItemNum As String, ByVal pPA As String, ByVal uII As String,	ByVal object_Class As String, 
						ByVal aTU_NoUnit As String)
			Try

				Dim params As New Dictionary(Of String, String) 
				params.Add("rpEntity", rp_Entity)
				params.Add("rpName", rpName) 
				params.Add("WFTime", wfTime) 		
				params.Add("LineItemNum", LineItemNum)
				params.Add("PPA", pPA)
				params.Add("UII", uII)
				params.Add("Object_Class", object_Class)
				params.Add("ATU_NoUnit", aTU_NoUnit)
				
				brapi.Utilities.StartDataMgmtSequence(si, "Calc_Single_RP_LI_NBAllocations", params)		
					
							
				Return Nothing
			Catch ex As Exception				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Private Function AllocationsClear(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal args As DashboardExtenderArgs, ByVal rp_Entity As String, ByVal rpName As String, 
						ByVal wfCube As String, ByVal wfScenario As String, ByVal wfTime As Integer, ByVal LineItemNum As String)
			Try

				Dim povInfo As New Dictionary(Of String, String) 
				povInfo.Add("Cube", wfCube)
				povInfo.Add("Consolidation", "Local")
				povInfo.Add("Scenario", wfScenario)
				povInfo.Add("View", "Periodic")
				povInfo.Add("Entity", rp_Entity)
				povInfo.Add("Time", wfTime)
				
				'stopped using this approach on 7/9/23 when implemented Execution via custom calc call below
'				brapi.Utilities.StartDataMgmtSequence(si, "Clear_Single_RP_LI_Allocations", params)	

				globals.SetStringValue("rpEntity", rp_Entity)
				globals.SetStringValue("rpName", rpName) 
				globals.SetStringValue("WFTime", wfTime) 		
				globals.SetStringValue("LineItemNum", LineItemNum)	
				brapi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si, "USCG_RP_CostCalc_Billet", "ClearLIAllocations", povInfo, customcalculatetimetype.MemberFilter)
					
							
				Return Nothing
			Catch ex As Exception				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Private Function AllocationsClearSpdshtBillets(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal args As DashboardExtenderArgs, ByVal rp_Entity As String, ByVal rpName As String, 
						ByVal wfCube As String, ByVal wfScenario As String, ByVal wfTime As Integer, ByVal LineItemNum As String)
			Try

				Dim povInfo As New Dictionary(Of String, String) 
				povInfo.Add("Cube", wfCube)
				povInfo.Add("Consolidation", "Local")
				povInfo.Add("Scenario", wfScenario)
				povInfo.Add("View", "Periodic")
				povInfo.Add("Entity", rp_Entity)
				povInfo.Add("Time", wfTime)
				
				'stopped using this approach on 7/9/23 when implemented Execution via custom calc call below
'				brapi.Utilities.StartDataMgmtSequence(si, "Clear_Single_RP_LI_Allocations", params)	

				globals.SetStringValue("rpEntity", rp_Entity)
				globals.SetStringValue("rpName", rpName) 
				globals.SetStringValue("WFTime", wfTime) 		
				globals.SetStringValue("LineItemNum", LineItemNum)	
				brapi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si, "USCG_RP_CostCalc_Billet", "ClearLIAllocationsSpdshtBillets", povInfo, customcalculatetimetype.MemberFilter)
					
							
				Return Nothing
			Catch ex As Exception				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function		
		
		Private Function HeadcountCalc(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal args As DashboardExtenderArgs, ByVal rp_Entity As String, ByVal rpName As String, 
						ByVal wfCube As String, ByVal wfScenario As String, ByVal wfTime As Integer, ByVal LineItemNum As String, ByVal grade_Rank As String, ByVal milEmpType As String, 
						ByVal spe_Code_Occu_Series As String, ByVal cONUS_OCONUS As String, ByVal aviator As String)
			Try
					
				Dim povInfo As New Dictionary(Of String, String) 
				povInfo.Add("Cube", wfCube)
				povInfo.Add("Consolidation", "Local")
				povInfo.Add("Scenario", wfScenario)
				povInfo.Add("View", "Periodic")
				povInfo.Add("Entity", rp_Entity)
				povInfo.Add("Time", wfTime)
				
'				brapi.Utilities.StartDataMgmtSequence(si, "Calc_Single_RP_LI_Headcount", params)


				globals.SetStringValue("rpEntity", rp_Entity) 
				globals.SetStringValue("rpName", rpName) 
				globals.SetStringValue("WFTime", wfTime) 		
				globals.SetStringValue("LineItemNum", LineItemNum)
				globals.SetStringValue("grade_Rank", grade_Rank)
				globals.SetStringValue("milEmpType", milEmpType)
				globals.SetStringValue("spe_Code_Occu_Series", spe_Code_Occu_Series)
				globals.SetStringValue("cONUS_OCONUS", cONUS_OCONUS)
				globals.SetStringValue("aviator", aviator)

				brapi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si, "USCG_RP_CostCalc_Billet", "CalculateLIHeadcount", povInfo, customcalculatetimetype.MemberFilter)
					
							
				Return Nothing
			Catch ex As Exception				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Private Function HeadcountClear(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal args As DashboardExtenderArgs, ByVal rp_Entity As String, ByVal rpName As String, 
						ByVal wfCube As String, ByVal wfScenario As String, ByVal wfTime As Integer, ByVal LineItemNum As String)
			Try
							
				Dim povInfo As New Dictionary(Of String, String) 
				povInfo.Add("Cube", wfCube)
				povInfo.Add("Consolidation", "Local")
				povInfo.Add("Scenario", wfScenario)
				povInfo.Add("View", "Periodic")
				povInfo.Add("Entity", rp_Entity)
				povInfo.Add("Time", wfTime)				
				
'				brapi.Utilities.StartDataMgmtSequence(si, "Clear_Single_RP_LI_Headcount", params)	

				globals.SetStringValue("rpEntity", rp_Entity) 
				globals.SetStringValue("rpName", rpName) 
				globals.SetStringValue("WFTime", wfTime) 		
				globals.SetStringValue("LineItemNum", LineItemNum)
				brapi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si, "USCG_RP_CostCalc_Billet", "ClearLIHeadcount", povInfo, customcalculatetimetype.MemberFilter)	
					
							
				Return Nothing
			Catch ex As Exception				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Private Function HeadcountClearSpdshtBillets(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal args As DashboardExtenderArgs, ByVal rp_Entity As String, ByVal rpName As String, 
						ByVal wfCube As String, ByVal wfScenario As String, ByVal wfTime As Integer, ByVal LineItemNum As String)
			Try
							
				Dim povInfo As New Dictionary(Of String, String) 
				povInfo.Add("Cube", wfCube)
				povInfo.Add("Consolidation", "Local")
				povInfo.Add("Scenario", wfScenario)
				povInfo.Add("View", "Periodic")
				povInfo.Add("Entity", rp_Entity)
				povInfo.Add("Time", wfTime)				

				globals.SetStringValue("rpEntity", rp_Entity) 
				globals.SetStringValue("rpName", rpName) 
				globals.SetStringValue("WFTime", wfTime) 		
				globals.SetStringValue("LineItemNum", LineItemNum)
				
'				BRApi.ErrorLog.LogMessage(si, "Custom Calc Headcount Clear POV Info: " & Environment.NewLine & _
'											  "Cube: " & wfCube & Environment.NewLine & _
'											  "WF Scenario: " & wfScenario & Environment.NewLine & _
'											  "Entity: " & rp_Entity & Environment.NewLine & _
'											  "Time: " & wfTime & Environment.NewLine & _
'											  "Global RP Entity: " & globals.GetStringValue("rpEntity") & Environment.NewLine & _
'											  "Global RP Name : " & globals.GetStringValue("rpName") & Environment.NewLine & _
'											  "Global WF Time: " & globals.GetStringValue("WFTime") & Environment.NewLine & _
'											  "Global Line Item Num: " & globals.GetStringValue("LineItemNum"))
				
				brapi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si, "USCG_RP_CostCalc_Billet", "ClearLIHeadcountSpdshtBillets", povInfo, customcalculatetimetype.MemberFilter)	
					
							
				Return Nothing
			Catch ex As Exception				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function		
		
		Private Function GetSupportDocDataTableCV(ByVal si As SessionInfo, ByVal includeFileBytes As Boolean) As DataTable
			Try

				Dim sql As New Text.StringBuilder                                                  
				If includeFileBytes Then
					sql.Append("Select * ")
				Else     
					sql.Append("Select ")
					sql.Append("UniqueID, ")
					sql.Append("Cube, ")
					sql.Append("Entity, ")
					sql.Append("Parent, ")
					sql.Append("Cons, ")
					sql.Append("Scenario, ")
					sql.Append("Time, ")
					sql.Append("Account, ")
					sql.Append("Flow, ")
					sql.Append("Origin, ")
					sql.Append("IC, ")
					sql.Append("UD1, ")
					sql.Append("UD2, ")
					sql.Append("UD3, ")
					sql.Append("UD4, ")
					sql.Append("UD5, ")
					sql.Append("UD6, ")
					sql.Append("UD7, ")
					sql.Append("UD8, ")
					sql.Append("Title, ")
					sql.Append("AttachmentType, ")
					sql.Append("CreatedUserName, ")
					sql.Append("CreatedTimestamp, ")
					sql.Append("LastEditedUserName, ")
					sql.Append("LastEditedTimestamp, ")
					sql.Append("Text, ")
					sql.Append("FileName, ")                  
				End If
				sql.Append("From dbo.DataAttachment With (NOLOCK) ")

				Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
					Return BRApi.Database.ExecuteSql(dbConnApp, sql.ToString, False)
				End Using                               

				Catch ex As Exception
					Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try                       
		End Function
	
		Private Function FileCanCompress(ByVal si As SessionInfo, ByVal fileName As String) As Boolean
                                    Try

                                                'Disabling compression in PV4.1.0_SV107
                                                'Re-enabling compression for txt/pdf in PV4.1.0_SV108
                                                Dim canCompress As Boolean = True

                                                'Check file Extension (Office files should be compressed, they are already ZIP files)
                                                If fileName.XFContainsIgnoreCase(".xlsx") Then
                                                            canCompress = False
                                                ElseIf fileName.XFContainsIgnoreCase(".docx") Then
                                                            canCompress = False
                                                ElseIf fileName.XFContainsIgnoreCase(".pptx") Then
                                                            canCompress = False
                                                End If

                                                Return canCompress

                                    Catch ex As Exception
                                                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
                                    End Try
                        End Function
			
		Private Function SetFieldValues(ByVal si As SessionInfo, ByVal params As Dictionary(Of String, String),ByVal showMessageBox As Boolean, 
						ByVal pstrMessage As String ) As XFSelectionChangedTaskResult
						
			Try

				Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
				For Each param As KeyValuePair(Of String, String) In params
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue(param.Key, param.Value )
					
					'brapi.ErrorLog.LogMessage(si, "Updated " & param.Key & " To: " & param.Value  )
				Next  
				
				selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
				selectionChangedTaskResult.IsOK = True
				selectionChangedTaskResult.ShowMessageBox = showMessageBox
				selectionChangedTaskResult.Message = pstrMessage	
				Return  selectionChangedTaskResult
			Catch ex As Exception				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Private Function SetChangeLogComment(ByVal si As SessionInfo, ByVal Cube As String, ByVal Entity As String, ByVal Scenario As String, ByVal Time As String,
						ByVal Flow As String, ByVal UD6 As String,  ByVal Text As String)
											
		
			Try
				SetChangeLogComment(si, Cube, Entity, Scenario, Time, Flow, UD6, "None", Text )
				Return Nothing
				Catch ex As Exception
					Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try                       
		End Function

		Private Function SetChangeLogComment(ByVal si As SessionInfo, ByVal Cube As String, ByVal Entity As String, ByVal Scenario As String, ByVal Time As String,
						ByVal Flow As String, ByVal UD6 As String, ByVal UD8 As String, ByVal Text As String)
											
		
			Try
		
				Using dt As DataTable = GetSupportDocDataTableCV(si, True)
					Dim dr As DataRow = dt.NewRow   
					
					dr("UniqueID")  = Guid.NewGuid
					dr("Cube")		= Cube
					dr("Entity")    = Entity
					dr("Parent")    = ""
					dr("Cons")		= "USD"
					dr("Scenario")  = Scenario
					dr("Time")		= Time
					dr("Account")	= "Description_ChangeLog"
					dr("Flow")	    = Flow
					dr("Origin")    = "Forms"
					dr("IC")		= "None"
					dr("UD1")		= "None"
					dr("UD2")		= "None"
					dr("UD3")		= "None"
					dr("UD4")		= "None"
					dr("UD5")		= "None"
					dr("UD6")		= UD6
					dr("UD7")		= "None"
					dr("UD8")		= UD8

					dr("Title")					= ""
					dr("AttachmentType")		= DataAttachmentType.Annotation
					dr("CreatedUserName")		= si.UserName
					dr("CreatedTimestamp")		= DateTime.UtcNow
					dr("LastEditedUserName")    = si.UserName
					dr("LastEditedTimestamp")   = DateTime.UtcNow
					dr("FileName")				= ""
					dr("Text")					= Text
					
					dt.Rows.Add(dr)
					BRApi.Database.SaveCustomDataTable(si, "App", "dbo.DataAttachment", dt, False)
							
					
				End Using
				
				Return Nothing
				Catch ex As Exception
					Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try                       
		End Function

		Private Function SetChangeLogComment_BLT_NBLT_Deletion(ByVal si As SessionInfo, ByVal Cube As String, ByVal Entity As String, ByVal Scenario As String, ByVal Time As String,
						ByVal Flow As String, ByVal UD6 As String, ByVal UD8 As String, ByVal Text As String)
											
		
			Try
		
				Using dt As DataTable = GetSupportDocDataTableCV(si, True)
					Dim dr As DataRow = dt.NewRow   
					
					dr("UniqueID")  = Guid.NewGuid
					dr("Cube")		= Cube
					dr("Entity")    = Entity
					dr("Parent")    = ""
					dr("Cons")		= "USD"
					dr("Scenario")  = Scenario
					dr("Time")		= Time
					dr("Account")	= "Description_ChangeLog"
					dr("Flow")	    = Flow
					dr("Origin")    = "Forms"
					dr("IC")		= "None"
					dr("UD1")		= "None"
					dr("UD2")		= "None"
					dr("UD3")		= "None"
					dr("UD4")		= "None"
					dr("UD5")		= "None"
					dr("UD6")		= "NA_LineItem"
					dr("UD7")		= "None"
					dr("UD8")		= UD8

					dr("Title")					= ""
					dr("AttachmentType")		= DataAttachmentType.Annotation
					dr("CreatedUserName")		= si.UserName
					dr("CreatedTimestamp")		= DateTime.UtcNow
					dr("LastEditedUserName")    = si.UserName
					dr("LastEditedTimestamp")   = DateTime.UtcNow
					dr("FileName")				= ""
					dr("Text")					= Text
					
					dt.Rows.Add(dr)
					BRApi.Database.SaveCustomDataTable(si, "App", "dbo.DataAttachment", dt, False)
							
					
				End Using
				
				Return Nothing
				Catch ex As Exception
					Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try                       
		End Function
		
Private Function UpdateLastEditedTimestamp(ByVal si As SessionInfo, ByVal Cube As String, ByVal Entity As String, ByVal Scenario As String, ByVal Time As String,
						ByVal Flow As String)
											
		Try
	
			'First try to update the timestamp , if the record does not exist, insert 	
			
			Dim sqlWhere As New Text.StringBuilder
				sqlWhere.Append(" WHERE Cube = '" & Cube & "' ")
				sqlWhere.Append(" AND Time = '" & Time & "' ")
				sqlWhere.Append(" AND Entity = '" & Entity & "' ")
				sqlWhere.Append(" AND Flow = '" & Flow & "' ")
				sqlWhere.Append(" AND Scenario = '" & Scenario & "' ")
				sqlWhere.Append(" AND Account = 'RPAudit' ")		
				
			Dim sqlSelect As String =  "SELECT 'x' FROM dbo.DataAttachment " & sqlWhere.ToString				
				
			Dim numRows As Integer = 0
			Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
	        	Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp, sqlSelect, True)
	            numRows = dt.Rows.Count
			End Using	

			If numRows = 1 Then		
				 Dim sqlUpdate As String =  
				 		"Update dbo.DataAttachment " & 
						" set Text = 'Audited' " &
						", LastEditedUserName = '" & si.UserName & "' " &
						", LastEditedTimestamp = '" & DateTime.UtcNow & "' " &
 						sqlWhere.ToString
						
				Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
		        	Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp, sqlUpdate, True)
				End Using	
			
			Else If numRows < 1 Then
				Using dt As DataTable = GetSupportDocDataTableCV(si, True)
					Dim dr As DataRow = dt.NewRow   
					
					dr("UniqueID")  = Guid.NewGuid
					dr("Cube")		= Cube
					dr("Entity")    = Entity
					dr("Parent")    = ""
					dr("Cons")		= "USD"
					dr("Scenario")  = Scenario
					dr("Time")		= Time
					dr("Account")	= "RPAudit"
					dr("Flow")	    = Flow
					dr("Origin")    = "Forms"
					dr("IC")		= "None"
					dr("UD1")		= "None"
					dr("UD2")		= "None"
					dr("UD3")		= "None"
					dr("UD4")		= "None"
					dr("UD5")		= "None"
					dr("UD6")		= "none"
					dr("UD7")		= "None"
					dr("UD8")		= "None"
					dr("Title")					= ""
					dr("AttachmentType")		= DataAttachmentType.Annotation
					dr("CreatedUserName")		= si.UserName
					dr("CreatedTimestamp")		= DateTime.UtcNow
					dr("LastEditedUserName")    = si.UserName
					dr("LastEditedTimestamp")   = DateTime.UtcNow
					dr("FileName")				= ""
					dr("Text")					= "Audited"
					
					dt.Rows.Add(dr)
					BRApi.Database.SaveCustomDataTable(si, "App", "dbo.DataAttachment", dt, False)									
				End Using
			End If
			
			Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		End Try                       
	End Function

Private Function UpdateRpCompletionStatusFunction(
					ByVal si As SessionInfo, 
					ByVal Cube As String, 
					ByVal Entity As String, 
					ByVal Scenario As String, 
					ByVal Time As String,
					ByVal Flow As String,
					ByVal RPCompletenessText As String)
						
		Try
				
			Dim sqlWhere As New Text.StringBuilder
				sqlWhere.Append(" Where Cube = '" & Cube & "' ")
				sqlWhere.Append(" AND Time = '" & Time & "' ")
				sqlWhere.Append(" AND Entity = '" & Entity & "' ")
				sqlWhere.Append(" AND Flow = '" & Flow & "' ")
				sqlWhere.Append(" AND Scenario = '" & Scenario & "' ")
				sqlWhere.Append(" AND Account = 'RPCompleteness' ")	
				
			
			Dim sqlSelect As String = "SELECT * FROM dbo.DataAttachment" & sqlWhere.ToString
			'brapi.ErrorLog.LogMessage(si, sqlSelect)
			Dim numRows As Integer = 0
			Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp, sqlSelect, True)
	            numRows = dt.Rows.Count
			End Using
			
			If numRows > 0 Then
				Dim sqlUpdate As String = 
				"Update dbo.DataAttachment " &
				" set Text = '" & RPCompletenessText & "' " &
				sqlWhere.ToString
				'First establish app connection, then update the Completeness Text
				Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
		        	BRApi.Database.ExecuteSql(dbConnApp, sqlUpdate, True)
				End Using	
			
			Else 
				Using dt As DataTable = GetSupportDocDataTableCV(si, True)
					Dim dr As DataRow = dt.NewRow   
					
					dr("UniqueID")  = Guid.NewGuid
					dr("Cube")		= Cube
					dr("Entity")    = Entity
					dr("Parent")    = ""
					dr("Cons")		= "USD"
					dr("Scenario")  = Scenario
					dr("Time")		= Time
					dr("Account")	= "RPCompleteness"
					dr("Flow")	    = Flow
					dr("Origin")    = "Forms"
					dr("IC")		= "None"
					dr("UD1")		= "None"
					dr("UD2")		= "None"
					dr("UD3")		= "None"
					dr("UD4")		= "None"
					dr("UD5")		= "None"
					dr("UD6")		= "none"
					dr("UD7")		= "None"
					dr("UD8")		= "None"
					dr("Title")					= ""
					dr("AttachmentType")		= DataAttachmentType.Annotation
					dr("CreatedUserName")		= si.UserName
					dr("CreatedTimestamp")		= DateTime.UtcNow
					dr("LastEditedUserName")    = si.UserName
					dr("LastEditedTimestamp")   = DateTime.UtcNow
					dr("FileName")				= ""
					dr("Text")					= RPCompletenessText
					
					dt.Rows.Add(dr)
					BRApi.Database.SaveCustomDataTable(si, "App", "dbo.DataAttachment", dt, False)
				End Using
			End If			
			
			Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try		
		End Function	
	
		Private Function RefreshSelectedLineItem_OS (ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal wfCube As String, ByVal wfTime As String, ByVal wfScenario As String, 
						ByVal RPName As String, ByVal LINumber As String, ByVal stringmessage As String) ' *** Updated for OS *** 
			Try
'					'
										
'					'Get the component name
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)		
								
					'Logic to set the default line item when the Billet screen is opened
					Dim LINumberToSet As String = String.Empty
					If LINumber.Length > 0 Then
						LINumberToSet = LINumber	

					Else
						LINumberToSet = "NBLineItem_01"

					End If
						'set the line item based on the above logic							
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_LineItemNumber_OS", LINumberToSet)
					
					
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumberToSet & ":U7#None:U8#None"			
					Dim scriptGenericsDescr As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_Input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U6#"& LINumberToSet & ":U7#None:U8#None"				
			
					'using a global function to avoid using brapi functions too many times and use api.data.calculate via a finance rule instead
					'set the script generics and parent account to be used in the global function
					globals.SetStringValue("scriptGenerics", scriptGenerics)
					globals.SetStringValue("parAccount", "NonBillet_LineItem_Data")					

					'Set a generic dictionary as an argument in the rule below
					Dim Dictionary As New Dictionary(Of String, String)
					
						BUDFM_AttributeSupport.GetRPAttributes(si, globals)
					
					If Not globals.GetObject("attributeDict") Is Nothing
					
						Dim attributeDict As Dictionary(Of String, String) = globals.GetObject("attributeDict")
							
	'					'Get info for the Non-Billet
						Dim Requested_Item_Cost_Line As String = attributeDict.GetValueOrEmpty("Requested_Item_Tier1")
						'Get the ItemNum to use to find the description Input account
						Dim requested_ItemNum As Integer
						If (Not Requested_Item_Cost_Line = "") 
							Dim requested_Item_Tier1Split As List(Of String) = StringHelper.SplitString(Requested_Item_Cost_Line, "_")
							requested_ItemNum = requested_Item_Tier1Split(0).XFConvertToInt
						End If	
						
						'Derive ATU from ATU_NoUnit since we stored it as a base but they chose a parent
						Dim ATU_NoUnit As String = attributeDict.GetValueOrEmpty("ATU")	
						Dim ATU As String = String.Empty
						'If it already has a value, derive the parent member from the stored NoUnit child
						If ATU_NoUnit.Length > 0
							ATU = ATU_NoUnit.Substring(0, ATU_NoUnit.Length - 7)
						Else
						End If
						
						'Set Parameters for NonBillet info_section
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_RequestedItem_Tier1_OS", 		Requested_Item_Cost_Line)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_ATU_OS", 						ATU)						
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_Description_Tier2_OS", 			attributeDict.GetValueOrEmpty("Description_Tier2"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_Description_Tier2_Input_OS", 	BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "U5#" & requested_ItemNum & "0_1:" 		& scriptGenericsDescr).DataCellEx.DataCellAnnotation)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_POC_OS", 						attributeDict.GetValueOrEmpty("POC"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_DollarKValue_OS", 				attributeDict.GetValueOrEmpty("DollarK_Value"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_RecurringNonRecurring_OS", 		attributeDict.GetValueOrEmpty("R_NR"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_PPA_OS", 						attributeDict.GetValueOrEmpty("PPA"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_UII_OS", 						attributeDict.GetValueOrEmpty("UII"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_NBLT_ObjectClass_OS", 				attributeDict.GetValueOrEmpty("Object_Class"))
						Dim routingAppn As String = ResolveRoutingAppnForRP(RPName, args.NameValuePairs.XFGetValue("APPN_Content", "OS"))
						SetRoutingContent(selectionChangedTaskResult.ModifiedCustomSubstVars, routingAppn, "OS_Billets_NonAddEditNon_04d")
					
						selectionChangedTaskResult.ShowMessageBox = True
						selectionChangedTaskResult.Message = stringmessage
						
						
					End If 'globals.GetObject("attributeDict") Is Nothing
					
					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True					
					Return selectionChangedTaskResult
				Catch ex As Exception
					Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try                       
	
		End Function
		
		Private Function RefreshSelectedBillet_OS (ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs, ByVal globals As BRGlobals, ByVal wfCube As String, 
						ByVal wfTime As String, ByVal wfScenario As String, 
						ByVal RPName As String, ByVal LINumber As String, ByVal stringmessage As String) ' *** Updated for BS *** 
			Try
				
					'Get the component name
					Dim componentName As String = args.ComponentInfo.Component.Name
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					
					' If No RP is selected, nothing to do
					If RPName = "" Then 
						Return Nothing
					End If
					
					Dim routingAppn As String = ResolveRoutingAppnForRP(RPName, args.NameValuePairs.XFGetValue("APPN_Content", "OS"))
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)	
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumber & ":U7#None:U8#None"
										
					'using a global function to avoid using brapi functions too many times and use api.data.calculate via a finance rule instead
					'set the script generics and parent account to be used in the global function
					globals.SetStringValue("scriptGenerics", scriptGenerics)
					globals.SetStringValue("parAccount", "Billet_LineItem_Data")					

					'Set a generic dictionary as an argument in the rule below
					Dim Dictionary As New Dictionary(Of String, String)
					
					BUDFM_AttributeSupport.GetRPAttributes(si, globals)
					
					If Not globals.GetObject("attributeDict") Is Nothing
					
						Dim attributeDict As Dictionary(Of String, String) = globals.GetObject("attributeDict")
							
						'For the ATU criteria, we need to derive the parent ATU since we store it in NoUnit
						'Derive Billet_ATU from Billet_ATU_NoUnit since we stored it as a base but they chose a parentDim Billet_ATU_NoUnit As String = Billet_ATU_NoUnit_Info
						Dim Billet_ATU_NoUnit As String = attributeDict.GetValueOrEmpty("Billet_ATU")
						Dim Billet_ATU As String = String.Empty
						If Billet_ATU_NoUnit.Length > 0
							Billet_ATU = Billet_ATU_NoUnit.Substring(0, Billet_ATU_NoUnit.Length - 7)
							'If nothing Return Zero
						End If
						
						'Derive PPE_ATU from PPE_ATU_NoUnit since we stored it as a base but they chose a parent
						Dim PPE_ATU_NoUnit As String = attributeDict.GetValueOrEmpty("PPE_ATU")	
						Dim PPE_ATU As String = String.Empty
						If PPE_ATU_NoUnit.Length > 0
							PPE_ATU = PPE_ATU_NoUnit.Substring(0, PPE_ATU_NoUnit.Length - 7)
							'If nothing Return Zero
						End If	
												
'						'Derive lease_ATU from lease_ATU_NoUnit since we stored it as a base but they chose a parent
						Dim lease_ATU_NoUnit As String = attributeDict.GetValueOrEmpty("Lease_ATU")	
						Dim lease_ATU As String = String.Empty
						If lease_ATU_NoUnit.Length > 0
							lease_ATU = lease_ATU_NoUnit.Substring(0, lease_ATU_NoUnit.Length - 7)
							'If nothing Return Zero
						End If	
						
						'Derive UTL_ATU from UTL_ATU_NoUnit since we stored it as a base but they chose a parent
						Dim UTL_ATU_NoUnit As String = attributeDict.GetValueOrEmpty("Utilities_ATU")	
						Dim UTL_ATU As String = String.Empty
						If UTL_ATU_NoUnit.Length > 0
							UTL_ATU = UTL_ATU_NoUnit.Substring(0, UTL_ATU_NoUnit.Length - 7)
							'If nothing Return Zero
						End If			
						
						'set the line item based on the above logic
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_LineItemNumber_OS", LINumber)
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Destination_LineItemNumber_OS", String.Empty)
					
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_PPE_ATU_OS", PPE_ATU)	
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Lease_ATU_OS", lease_ATU)	
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_ATU_OS", Billet_ATU)	
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_UTL_ATU_OS", UTL_ATU)		
						'For all other billet attributes, just return what was stored
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_BilletType_OS", 			attributeDict.GetValueOrEmpty("Billet_Type"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_GradeType_OS", 			attributeDict.GetValueOrEmpty("Grade_Type"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_GradeRank_OS", 			attributeDict.GetValueOrEmpty("Grade_Rank"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_ADReserve_OS", 			attributeDict.GetValueOrEmpty("AD_Reserve"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_ReserveType_OS", 			attributeDict.GetValueOrEmpty("Reserve_Type"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_SpcCodeOccSeries_OS", 	attributeDict.GetValueOrEmpty("Spe_Code_Occu_Series"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Pilot_OS", 				attributeDict.GetValueOrEmpty("Pilot"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_ElectronicFlightBag_OS", 	attributeDict.GetValueOrEmpty("Electronic_Flight_Bag"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_PositionNumber_OS", 		attributeDict.GetValueOrEmpty("Position_Number"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_PositionTitle_OS", 		attributeDict.GetValueOrEmpty("Position_Title"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_OPFACS_OS", 				attributeDict.GetValueOrEmpty("OPFAC"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_UII_OS", 					attributeDict.GetValueOrEmpty("Billet_UII"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_ConusOConus_OS", 			attributeDict.GetValueOrEmpty("CONUS_OCONUS"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_DetachedDuty_OS", 		attributeDict.GetValueOrEmpty("Detached_Duty"))
				
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_DutyLocation_OS", 		attributeDict.GetValueOrEmpty("Detached_Duty_Location"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_TermBillet_OS", 			attributeDict.GetValueOrEmpty("Term_Billet"))
						
						Dim PPE_Typedescription As String = String.Empty
						Dim loopCounter As Integer = 0
						
						If attributeDict.GetValueOrEmpty("PPE_Type").Length = 0
							PPE_Typedescription = ""
						Else
							
							Dim selectedArray() As String = attributeDict.GetValueOrEmpty("PPE_Type").Replace(" ", "").Split(",")
							Dim types As List(Of String) = selectedArray.ToList()
						
							For Each ppetype As String In types
								If loopCounter = 0 Then
							
									PPE_Typedescription = BRApi.Finance.Members.GetMember(si, dimtypeid.UD8, ppetype).Description 
							
								Else
								
									PPE_Typedescription = PPE_Typedescription & ", " & BRApi.Finance.Members.GetMember(si, dimtypeid.UD8, ppetype).Description
								
								End If
							
								loopCounter+=1
						
						   Next
						
						
						End If
						
					
					
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_PPEType_OS", 				attributeDict.GetValueOrEmpty("PPE_Type"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_PPEType_Descr_OS", 				PPE_Typedescription)	
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_PPE_PPA_OS", 				attributeDict.GetValueOrEmpty("PPE_PPA"))										
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Build_Out_OS", 			attributeDict.GetValueOrEmpty("Build_Out_Choice"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_ICASSType_OS", 			attributeDict.GetValueOrEmpty("ICASS_Costs"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_BIType_OS", 				attributeDict.GetValueOrEmpty("Background_Investigation_Type"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Acq_Project_OS", 			attributeDict.GetValueOrEmpty("Acquisition_Project"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Lease_OS", 				attributeDict.GetValueOrEmpty("Lease_Choice"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Lease_PPA_OS", 			attributeDict.GetValueOrEmpty("Lease_PPA"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Furniture_OS", 			attributeDict.GetValueOrEmpty("Furniture_Reqd"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Utilities_OS", 			attributeDict.GetValueOrEmpty("Utilities_Reqd"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Computer_Type_OS", 		attributeDict.GetValueOrEmpty("Computer_Type"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_Comment_OS", 				attributeDict.GetValueOrEmpty("LineItem_Comment"))
						selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_UTL_PPA_OS", 				attributeDict.GetValueOrEmpty("Utilities_PPA"))
						
						SetRoutingContent(selectionChangedTaskResult.ModifiedCustomSubstVars, routingAppn, "OS_RP_OSDynamicCopy")
						SetRoutingFrame(selectionChangedTaskResult.ModifiedCustomSubstVars, routingAppn, routingAppn & "_RP_Frame")
						
						
						selectionChangedTaskResult.ShowMessageBox = True
						selectionChangedTaskResult.Message = stringmessage
						
					End If 'Not globals.GetObject("attributeDict") Is Nothing
											
					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					Return selectionChangedTaskResult						
					'End Select						
		
				Catch ex As Exception
					Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try                       
	
		End Function
		
		
		Private Function RefreshSelectedLineItem_BS (ByVal si As SessionInfo, ByVal wfCube As String, ByVal wfTime As String, ByVal wfScenario As String, 
						ByVal RPName As String, ByVal LINumber As String) ' *** Updated for BS *** 
			Try
'										
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)																			
					Dim scriptGenerics  = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumber & ":U7#None:U8#None"			
					Dim scriptGenericsDescr As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_Input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U6#"& LINumber & ":U7#None:U8#None"				
		
					'Get info for the Expense
					Dim Requested_Item_Cost_Line_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Requested_Item_Tier1:" & scriptGenerics)
					Dim Requested_Item_Cost_Line As String = Requested_Item_Cost_Line_Info.DataCellEx.DataCellAnnotation
					
					'Get the ItemNum to use to find the description Input account
					Dim requested_ItemNum As Integer
					If (Not Requested_Item_Cost_Line = "") 
						Dim requested_Item_Tier1Split As List(Of String) = StringHelper.SplitString(Requested_Item_Cost_Line, "_")
						requested_ItemNum = requested_Item_Tier1Split(0).XFConvertToInt
					End If
					
					'Derive ATU from ATU_NoUnit since we stored it as a base but they chose a parent
					Dim ATU_NoUnit_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#ATU:" & scriptGenerics)
					Dim ATU_NoUnit As String = ATU_NoUnit_Info.DataCellEx.DataCellAnnotation	
					Dim ATU As String = String.Empty
					
					'If it already has a value, derive the parent member from the stored NoUnit child
					If ATU_NoUnit.Length > 0
						ATU = ATU_NoUnit.Substring(0, ATU_NoUnit.Length - 7)
					'If it doesn't have a value, return the default value
					Else
	'										
					End If
		
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_LineItemNumber_BS", 			LINumber)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_RequestedItem_Tier1_BS", 		Requested_Item_Cost_Line)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_Description_Tier2_BS",		BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Description_Tier2:" 				& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_Description_Tier2_Input_BS",	BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "U5#"& requested_ItemNum & "0_1:" 	& scriptGenericsDescr).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_POC_BS", 						BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#POC:" 							& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_DollarKValue_BS", 			BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#DollarK_Value:" 					& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_RecurringNonRecurring_BS",	BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#R_NR:" 							& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_ATU_BS", 						ATU)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_PPA_BS", 						BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#PPA:" 							& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_UII_BS", 						BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#UII:" 							& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_ObjectClass_BS", 				BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Object_Class:" 					& scriptGenerics).DataCellEx.DataCellAnnotation)

					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					Return selectionChangedTaskResult
		
				Catch ex As Exception
					Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try                       
	
		End Function

				Private Function RefreshSelectedLineItem_MERHCF (ByVal si As SessionInfo, ByVal wfCube As String, ByVal wfTime As String, ByVal wfScenario As String, 
						ByVal RPName As String, ByVal LINumber As String) ' *** Updated for BS *** 
			Try
'										
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)																		
					Dim scriptGenerics  = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumber & ":U7#None:U8#None"			
					Dim scriptGenericsDescr As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_Input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U6#"& LINumber & ":U7#None:U8#None"				
		
					'Get info for the Expense
					Dim Requested_Item_Cost_Line_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Requested_Item_Tier1:" & scriptGenerics)
					Dim Requested_Item_Cost_Line As String = Requested_Item_Cost_Line_Info.DataCellEx.DataCellAnnotation
					
					'Get the ItemNum to use to find the description Input account
					Dim requested_ItemNum As Integer
					If (Not Requested_Item_Cost_Line = "") 
						Dim requested_Item_Tier1Split As List(Of String) = StringHelper.SplitString(Requested_Item_Cost_Line, "_")
						requested_ItemNum = requested_Item_Tier1Split(0).XFConvertToInt
					End If
					
					'Derive ATU from ATU_NoUnit since we stored it as a base but they chose a parent
					Dim ATU_NoUnit_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#ATU:" & scriptGenerics)
					Dim ATU_NoUnit As String = ATU_NoUnit_Info.DataCellEx.DataCellAnnotation	
					Dim ATU As String = String.Empty
					
					'If it already has a value, derive the parent member from the stored NoUnit child
					If ATU_NoUnit.Length > 0
						ATU = ATU_NoUnit.Substring(0, ATU_NoUnit.Length - 7)
					'If it doesn't have a value, return the default value
					Else
	'										
					End If
		
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_LineItemNumber_MERHCF", 			LINumber)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_RequestedItem_Tier1_MERHCF", 		Requested_Item_Cost_Line)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_Description_Tier2_MERHCF",		BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Description_Tier2:" 				& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_Description_Tier2_Input_MERHCF",	BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "U5#"& requested_ItemNum & "0_1:" 	& scriptGenericsDescr).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_POC_MERHCF", 						BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#POC:" 							& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_DollarKValue_MERHCF", 			BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#DollarK_Value:" 					& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_RecurringNonRecurring_MERHCF",	BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#R_NR:" 							& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_ATU_MERHCF", 						ATU)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_PPA_MERHCF", 						BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#PPA:" 							& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_UII_MERHCF", 						BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#UII:" 							& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_ObjectClass_MERHCF", 				BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Object_Class:" 					& scriptGenerics).DataCellEx.DataCellAnnotation)

					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					Return selectionChangedTaskResult
		
				Catch ex As Exception
					Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try                       
	
		End Function

				Private Function RefreshSelectedLineItem_MOSP (ByVal si As SessionInfo, ByVal wfCube As String, ByVal wfTime As String, ByVal wfScenario As String, 
						ByVal RPName As String, ByVal LINumber As String) ' *** Updated for BS *** 
			Try
'										
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)																				
					Dim scriptGenerics  = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumber & ":U7#None:U8#None"			
					Dim scriptGenericsDescr As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_Input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U6#"& LINumber & ":U7#None:U8#None"				
		
					'Get info for the Expense
					Dim Requested_Item_Cost_Line_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Requested_Item_Tier1:" & scriptGenerics)
					Dim Requested_Item_Cost_Line As String = Requested_Item_Cost_Line_Info.DataCellEx.DataCellAnnotation
					
					'Get the ItemNum to use to find the description Input account
					Dim requested_ItemNum As Integer
					If (Not Requested_Item_Cost_Line = "") 
						Dim requested_Item_Tier1Split As List(Of String) = StringHelper.SplitString(Requested_Item_Cost_Line, "_")
						requested_ItemNum = requested_Item_Tier1Split(0).XFConvertToInt
					End If
					
					'Derive ATU from ATU_NoUnit since we stored it as a base but they chose a parent
					Dim ATU_NoUnit_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#ATU:" & scriptGenerics)
					Dim ATU_NoUnit As String = ATU_NoUnit_Info.DataCellEx.DataCellAnnotation	
					Dim ATU As String = String.Empty
					
					'If it already has a value, derive the parent member from the stored NoUnit child
					If ATU_NoUnit.Length > 0
						ATU = ATU_NoUnit.Substring(0, ATU_NoUnit.Length - 7)
					'If it doesn't have a value, return the default value
					Else
	'										
					End If
		
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_LineItemNumber_MOSP", 			LINumber)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_RequestedItem_Tier1_MOSP", 		Requested_Item_Cost_Line)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_Description_Tier2_MOSP",			BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Description_Tier2:" 				& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_Description_Tier2_Input_MOSP",	BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "U5#"& requested_ItemNum & "0_1:" 	& scriptGenericsDescr).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_POC_MOSP", 						BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#POC:" 							& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_DollarKValue_MOSP", 				BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#DollarK_Value:" 					& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_RecurringNonRecurring_MOSP",		BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#R_NR:" 							& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_ATU_MOSP", 						ATU)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_PPA_MOSP", 						BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#PPA:" 							& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_UII_MOSP", 						BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#UII:" 							& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_ObjectClass_MOSP", 				BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Object_Class:" 					& scriptGenerics).DataCellEx.DataCellAnnotation)

					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					Return selectionChangedTaskResult
		
				Catch ex As Exception
					Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try                       
	
		End Function

				Private Function RefreshSelectedLineItem_RP (ByVal si As SessionInfo, ByVal wfCube As String, ByVal wfTime As String, ByVal wfScenario As String, 
						ByVal RPName As String, ByVal LINumber As String) ' *** Updated for BS *** 
			Try
'										
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)		
					
'					Dim RPSplit As List(Of String) = StringHelper.SplitString(RPName,"_")
'					Dim RP_Entity As String = "LO_" & RPSplit(3)
																		
					Dim scriptGenerics  = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumber & ":U7#None:U8#None"			
					Dim scriptGenericsDescr As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_Input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U6#"& LINumber & ":U7#None:U8#None"				
		
					'Get info for the Expense
					Dim Requested_Item_Cost_Line_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Requested_Item_Tier1:" & scriptGenerics)
					Dim Requested_Item_Cost_Line As String = Requested_Item_Cost_Line_Info.DataCellEx.DataCellAnnotation
					
					'Get the ItemNum to use to find the description Input account
					Dim requested_ItemNum As Integer
					If (Not Requested_Item_Cost_Line = "") 
						Dim requested_Item_Tier1Split As List(Of String) = StringHelper.SplitString(Requested_Item_Cost_Line, "_")
						requested_ItemNum = requested_Item_Tier1Split(0).XFConvertToInt
					End If
					
					'Derive ATU from ATU_NoUnit since we stored it as a base but they chose a parent
					Dim ATU_NoUnit_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#ATU:" & scriptGenerics)
					Dim ATU_NoUnit As String = ATU_NoUnit_Info.DataCellEx.DataCellAnnotation	
					Dim ATU As String = String.Empty
					
					'If it already has a value, derive the parent member from the stored NoUnit child
					If ATU_NoUnit.Length > 0
						ATU = ATU_NoUnit.Substring(0, ATU_NoUnit.Length - 7)
					'If it doesn't have a value, return the default value
					Else
	'										
					End If
		
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_LineItemNumber_RP", 			LINumber)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_RequestedItem_Tier1_RP", 		Requested_Item_Cost_Line)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_Description_Tier2_RP",		BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Description_Tier2:" 				& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_Description_Tier2_Input_RP",	BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "U5#"& requested_ItemNum & "0_1:" 	& scriptGenericsDescr).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_POC_RP", 						BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#POC:" 							& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_DollarKValue_RP", 			BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#DollarK_Value:" 					& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_RecurringNonRecurring_RP",	BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#R_NR:" 							& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_ATU_RP", 						ATU)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_PPA_RP", 						BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#PPA:" 							& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_UII_RP", 						BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#UII:" 							& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_ObjectClass_RP", 				BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Object_Class:" 					& scriptGenerics).DataCellEx.DataCellAnnotation)

					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					Return selectionChangedTaskResult
		
				Catch ex As Exception
					Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try                       
	
		End Function

		
		Private Function RefreshSelectedLineItem_F (ByVal si As SessionInfo, ByVal wfCube As String, ByVal wfTime As String, ByVal wfScenario As String, 
						ByVal RPName As String, ByVal LINumber As String)
			Try
'														
'					'Get the component name
'					Dim componentName As String = args.ComponentInfo.Component.Name
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)																		
					Dim scriptGenerics  = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumber & ":U7#None:U8#None"			
					Dim scriptGenericsDescr As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_Input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U6#"& LINumber & ":U7#None:U8#None"				
		
					'Get info for the Non-Billet
					Dim Requested_Item_Cost_Line_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Requested_Item_Tier1:" & scriptGenerics)
					Dim Requested_Item_Cost_Line As String = Requested_Item_Cost_Line_Info.DataCellEx.DataCellAnnotation
					
					'Get the ItemNum to use to find the description Input account
					Dim requested_ItemNum As Integer
					If (Not Requested_Item_Cost_Line = "") 
						Dim requested_Item_Tier1Split As List(Of String) = StringHelper.SplitString(Requested_Item_Cost_Line, "_")
						requested_ItemNum = requested_Item_Tier1Split(0).XFConvertToInt
					End If
					
					'Derive ATU from ATU_NoUnit since we stored it as a base but they chose a parent
					Dim ATU_NoUnit_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#ATU:" & scriptGenerics)
					Dim ATU_NoUnit As String = ATU_NoUnit_Info.DataCellEx.DataCellAnnotation	
					Dim ATU As String = String.Empty
					
					'If it already has a value, derive the parent member from the stored NoUnit child
					If ATU_NoUnit.Length > 0
						ATU = ATU_NoUnit.Substring(0, ATU_NoUnit.Length - 7)
					'If it doesn't have a value, return the default value
					Else
	'									
					End If
		
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_LineItemNumber_F", 			LINumber)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_RequestedItem_Tier1_F", 		Requested_Item_Cost_Line)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_Description_Tier2_F",		BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Description_Tier2:" 				& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_Description_Tier2_Input_F",	BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "U5#"& requested_ItemNum & "0_1:" 	& scriptGenericsDescr).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_POC_F", 						BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#POC:" 							& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_DollarKValue_F", 			BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#DollarK_Value:" 					& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_BY_Obligations_F", 			BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#BY_Obligations:" 				& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_BY_Plus1_Obligations_F", 	BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#BY_Plus1_Obligations:"			& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_BY_Plus2_Obligations_F", 	BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#BY_Plus2_Obligations:" 			& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_RecurringNonRecurring_F",	BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#R_NR:" 							& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_ATU_F", 						ATU)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_PPA_F", 						BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#PPA:" 							& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_UII_F", 						BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#UII:" 							& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_ObjectClass_F", 				BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Object_Class:" 					& scriptGenerics).DataCellEx.DataCellAnnotation)

					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					Return selectionChangedTaskResult
		
				Catch ex As Exception
					Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try                       
	
		End Function
		
		Private Function RefreshSelectedLineItem_RD (ByVal si As SessionInfo, ByVal wfCube As String, ByVal wfTime As String, ByVal wfScenario As String, 
						ByVal RPName As String, ByVal LINumber As String) ' *** Updated for RD *** 
			Try
														
'					'Get the component name
'					Dim componentName As String = args.ComponentInfo.Component.Name
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()

					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)											
					Dim scriptGenerics  = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumber & ":U7#None:U8#None"			
					Dim scriptGenericsDescr As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_Input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U6#"& LINumber & ":U7#None:U8#None"				
		
					'Get info for the Non-Billet
					Dim Requested_Item_Cost_Line_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Requested_Item_Tier1:" & scriptGenerics)
					Dim Requested_Item_Cost_Line As String = Requested_Item_Cost_Line_Info.DataCellEx.DataCellAnnotation
					
					'Get the ItemNum to use to find the description Input account
					Dim requested_ItemNum As Integer
					If (Not Requested_Item_Cost_Line = "") 
						Dim requested_Item_Tier1Split As List(Of String) = StringHelper.SplitString(Requested_Item_Cost_Line, "_")
						requested_ItemNum = requested_Item_Tier1Split(0).XFConvertToInt
					End If
					
					'Derive ATU from ATU_NoUnit since we stored it as a base but they chose a parent
					Dim ATU_NoUnit_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#ATU:" & scriptGenerics)
					Dim ATU_NoUnit As String = ATU_NoUnit_Info.DataCellEx.DataCellAnnotation	
					Dim ATU As String = String.Empty
					
					'If it already has a value, derive the parent member from the stored NoUnit child
					If ATU_NoUnit.Length > 0
						ATU = ATU_NoUnit.Substring(0, ATU_NoUnit.Length - 7)
					'If it doesn't have a value, return the default value
					Else
						
					End If

		
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_LineItemNumber_RD", 			LINumber)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_RequestedItem_Tier1_RD", 		Requested_Item_Cost_Line)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_Description_Tier2_RD",		BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Description_Tier2:" 				& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_Description_Tier2_Input_RD",	BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "U5#"& requested_ItemNum & "0_1:" 	& scriptGenericsDescr).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_POC_RD", 						BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#POC:" 							& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_DollarKValue_RD", 			BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#DollarK_Value:" 					& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_BY_Obligations_RD", 			BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#BY_Obligations:" 				& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_BY_Plus1_Obligations_RD", 	BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#BY_Plus1_Obligations:"			& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_BY_Plus2_Obligations_RD", 	BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#BY_Plus2_Obligations:" 			& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_RecurringNonRecurring_RD",	BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#R_NR:" 							& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_ATU_RD", 						ATU)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_PPA_RD", 						BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#PPA:" 							& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_UII_RD", 						BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#UII:" 							& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_ObjectClass_RD", 				BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Object_Class:" 					& scriptGenerics).DataCellEx.DataCellAnnotation)

					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					Return selectionChangedTaskResult
		
				Catch ex As Exception
					Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try                       
	
		End Function
		
		Private Function RefreshSelectedLineItem_PCI (ByVal si As SessionInfo, ByVal wfCube As String, ByVal wfTime As String, ByVal wfScenario As String, 
						ByVal RPName As String, ByVal LINumber As String) ' *** Updated for RD *** 
			Try
'					
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)	
					Dim scriptGenerics  = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumber & ":U7#None:U8#None"			
					Dim scriptGenericsDescr As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_Input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U6#"& LINumber & ":U7#None:U8#None"				
		
					'Get info for the Non-Billet
					Dim Requested_Item_Cost_Line_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Requested_Item_Tier1:" & scriptGenerics)
					Dim Requested_Item_Cost_Line As String = Requested_Item_Cost_Line_Info.DataCellEx.DataCellAnnotation
					
					'Get the ItemNum to use to find the description Input account
					Dim requested_ItemNum As Integer
					If (Not Requested_Item_Cost_Line = "") 
						Dim requested_Item_Tier1Split As List(Of String) = StringHelper.SplitString(Requested_Item_Cost_Line, "_")
						requested_ItemNum = requested_Item_Tier1Split(0).XFConvertToInt
					End If
					
					'Derive ATU from ATU_NoUnit since we stored it as a base but they chose a parent
					Dim ATU_NoUnit_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#ATU:" & scriptGenerics)
					Dim ATU_NoUnit As String = ATU_NoUnit_Info.DataCellEx.DataCellAnnotation	
					Dim ATU As String = String.Empty
					
					'If it already has a value, derive the parent member from the stored NoUnit child
					If ATU_NoUnit.Length > 0
						ATU = ATU_NoUnit.Substring(0, ATU_NoUnit.Length - 7)
					'If it doesn't have a value, return the default value
					Else
	'					Dim atuAllocDefaults_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "E#NA:A#" & costLine & ":V#Assumptions:O#Forms:I#None:F#None:U1#None:U2#None:U3#None:U4#No_ATU:U5#None:U6#None:U7#None:U8#None")
	'					Dim atuAllocDefaults As String = atuAllocDefaults_Info.DataCellEx.DataCellAnnotation
	'						ATU = atuAllocDefaults						
					End If

		
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_LineItemNumber_PCI", 			LINumber)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_RequestedItem_Tier1_PCI", 		Requested_Item_Cost_Line)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_Description_Tier2_PCI",		BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Description_Tier2:" 				& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_Description_Tier2_Input_PCI",	BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "U5#"& requested_ItemNum & "0_1:" 	& scriptGenericsDescr).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_POC_PCI", 						BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#POC:" 							& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_DollarKValue_PCI", 			BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#DollarK_Value:" 					& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_BY_Obligations_PCI", 			BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#BY_Obligations:" 				& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_BY_Plus1_Obligations_PCI", 	BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#BY_Plus1_Obligations:"			& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_BY_Plus2_Obligations_PCI", 	BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#BY_Plus2_Obligations:" 			& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_BY_Plus3_Obligations_PCI", 	BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#BY_Plus3_Obligations:"			& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_BY_Plus4_Obligations_PCI", 	BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#BY_Plus4_Obligations:" 			& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_RecurringNonRecurring_PCI",	BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#R_NR:" 							& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_ATU_PCI", 						ATU)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_PPA_PCI", 						BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#PPA:" 							& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_UII_PCI", 						BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#UII:" 							& scriptGenerics).DataCellEx.DataCellAnnotation)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_EXP_ObjectClass_PCI", 				BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Object_Class:" 					& scriptGenerics).DataCellEx.DataCellAnnotation)

					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					Return selectionChangedTaskResult
		
				Catch ex As Exception
					Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try                       
	
		End Function
		
		Private Function CopyExpenseAllFields(ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs, ByVal wfCube As String, 
						ByVal wfTime As String, ByVal wfScenario As String, ByVal RP_Entity As String, ByVal rpName As String, 
						ByVal LINumberSource As String, ByVal LINumberDestination As String)
			Try

					Dim scriptGenerics As String      = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumberSource & ":U7#None:U8#None"		
					Dim scriptGenericsDescr As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_Input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U7#None:U8#None"						
					
					Dim requested_Item_Tier1 As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, 			"A#Requested_Item_Tier1:"										& scriptGenerics 		& ":U6#"& LINumberSource).DataCellEx.DataCellAnnotation
					Dim description_Tier2_ToUse As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, 		"A#Description_Tier2:" 											& scriptGenerics 		& ":U6#"& LINumberSource).DataCellEx.DataCellAnnotation
					Dim description_Tier2_Input_ToUse As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, 	"U5#" 						& description_Tier2_ToUse & ":" 	& scriptGenericsDescr 	& ":U6#"& LINumberSource).DataCellEx.DataCellAnnotation
					Dim pOC As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, 							"A#POC:" 														& scriptGenerics 		& ":U6#"& LINumberSource).DataCellEx.DataCellAnnotation
					Dim reference_Doc As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, 					"A#Reference_Doc:" 												& scriptGenerics 		& ":U6#"& LINumberSource).DataCellEx.DataCellAnnotation
					Dim dollarK_Value As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, 					"A#DollarK_Value:" 												& scriptGenerics 		& ":U6#"& LINumberSource).DataCellEx.DataCellAnnotation
					Dim r_NR As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, 							"A#R_NR:" 														& scriptGenerics 		& ":U6#"& LINumberSource).DataCellEx.DataCellAnnotation
					Dim aTU_NoUnit = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, 								"A#ATU:" 														& scriptGenerics 		& ":U6#"& LINumberSource).DataCellEx.DataCellAnnotation
					Dim pPA As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, 							"A#PPA:" 														& scriptGenerics 		& ":U6#"& LINumberSource).DataCellEx.DataCellAnnotation
					Dim uII As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, 							"A#UII:" 														& scriptGenerics 		& ":U6#"& LINumberSource).DataCellEx.DataCellAnnotation
					Dim object_Class As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, 					"A#Object_Class:" 												& scriptGenerics 		& ":U6#"& LINumberSource).DataCellEx.DataCellAnnotation
						
					'Create a new list of memberscript and value
					Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)
					
					'Create the script for the expenses and add it to the list
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Requested_Item_Tier1:" 	& scriptGenerics 										& ":U6#" & LINumberDestination, 0, True, requested_Item_Tier1))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Description_Tier2:" 	& scriptGenerics 										& ":U6#" & LINumberDestination, 0, True, description_Tier2_ToUse))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "U5#" 						& description_Tier2_ToUse & ":" & scriptGenericsDescr 	& ":U6#" & LINumberDestination, 0, True, description_Tier2_Input_ToUse))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#POC:" 					& scriptGenerics 										& ":U6#" & LINumberDestination, 0, True, pOC))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#DollarK_Value:" 		& scriptGenerics 										& ":U6#" & LINumberDestination, 0, True, dollarK_Value))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#R_NR:"					& scriptGenerics 										& ":U6#" & LINumberDestination, 0, True, r_NR))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#ATU:" 					& scriptGenerics 										& ":U6#" & LINumberDestination, 0, True, aTU_NoUnit))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#PPA:" 					& scriptGenerics 										& ":U6#" & LINumberDestination, 0, True, pPA))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#UII:" 					& scriptGenerics 										& ":U6#" & LINumberDestination, 0, True, uII))
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Object_Class:" 			& scriptGenerics 										& ":U6#" & LINumberDestination, 0, True, object_Class))
														
						
'					'********Allocation Drivers Storage********									
'					'For those attributes that are also a dimension, we will also store a 1 in that dimension member that is selected so we can find it in a data buffer for the cost calc	
					Me.NBAllocationsCalc(si, args, RP_Entity, RPName, wfTime, LINumberDestination, pPA, uII, object_Class, aTU_NoUnit)		
					
					'Files
					Dim strRefDocType As String = "Reference_Doc"						
					Dim sqlUpdate As New Text.StringBuilder                                                       
						sqlUpdate.Append("Update dbo.DataAttachment ")
						sqlUpdate.Append(" set UD6 = '" & LINumberDestination & "' ")
						sqlUpdate.Append(" WHERE Time = '" & wfTime & "' ")
						sqlUpdate.Append(" AND Flow = '" & rpName & "' ")
						sqlUpdate.Append(" AND Scenario = '" & wfScenario & "' ")
						sqlUpdate.Append(" AND UD6 = '" & LINumberSource & "' ")
						sqlUpdate.Append(" AND Account = '" & strRefDocType & "' ")
					
					Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
						Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp, sqlUpdate.ToString, False)
					End Using 

					'Write the annotations to the database
					Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)
												
				Return Nothing
			Catch ex As Exception				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Private Function ClearExpense(ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs, ByVal wfScenario As String, 
						ByVal wfCube As String, ByVal wfTime As String, ByVal rp_Entity As String, ByVal rpName As String, 
						ByVal LineItemNum As String, ByVal LineItemNumInt As Integer, ByVal scriptGenerics As String, ByVal scriptGenericsDescr As String)

		Try
			
				'Get the tier 2 description				
				Dim description_Tier2 As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "T#" & wfTime & ":A#Description_Tier2:" & scriptGenerics & ":U6#" & LineItemNum).DataCellEx.DataCellAnnotation

				'Create a new list of memberscript and value
				Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)
				
				'Create the script for the expenses and add it to the list
				lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Requested_Item_Tier1:" 										& scriptGenerics 		& ":U6#" & LineItemNum, 0, True, String.Empty))
				lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Description_Tier2:" 										& scriptGenerics 		& ":U6#" & LineItemNum, 0, True, String.Empty))
				lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "U5#" 							& description_Tier2 & ":" 		& scriptGenericsDescr 	& ":U6#" & LineItemNum, 0, True, String.Empty))
				lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#POC:" 														& scriptGenerics 		& ":U6#" & LineItemNum, 0, True, String.Empty))
				lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Reference_Doc:" 											& scriptGenerics 		& ":U6#" & LineItemNum, 0, True, String.Empty))
				lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#DollarK_Value:" 											& scriptGenerics 		& ":U6#" & LineItemNum, 0, True, String.Empty))
				lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#R_NR:" 														& scriptGenerics 		& ":U6#" & LineItemNum, 0, True, String.Empty))
				lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#ATU:" 														& scriptGenerics 		& ":U6#" & LineItemNum, 0, True, String.Empty))
				lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#PPA:" 														& scriptGenerics 		& ":U6#" & LineItemNum, 0, True, String.Empty))
				lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#UII:" 														& scriptGenerics 		& ":U6#" & LineItemNum, 0, True, String.Empty))
				lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Object_Class:" 												& scriptGenerics 		& ":U6#" & LineItemNum, 0, True, String.Empty))
	
				'********Cost Storage********							
				'Clear the Cost							
				Me.ExpenseCostClear(si, args, rp_Entity, RPName, wfTime, LineItemNum)
				Dim strRefDocType As String = "Reference_Doc"

				'Delete Files
				Dim sqlDelete As New Text.StringBuilder
				sqlDelete.Append("DELETE FROM dbo.DataAttachment ")
            	sqlDelete.Append("WHERE Cube = '" & wfCube & "' ")
				sqlDelete.Append("AND Time = '" & wfTime & "' ")
				sqlDelete.Append("AND Flow = '" & rpName & "' ")
				sqlDelete.Append("AND Scenario = '" & wfScenario & "' ")
				sqlDelete.Append("AND UD6 = '" & LineItemNum & "' ")
				sqlDelete.Append("AND Account = '" & strRefDocType & "' ")
				
					Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
	        	    	Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp, sqlDelete.ToString, True)
					End Using
				
				'Write the annotations to the database
				Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)
													
				Return Nothing
			Catch ex As Exception				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
			
		Private Function CopyExpenseAllFields_RD(ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs, ByVal wfCube As String, 
						ByVal wfTime As String, ByVal wfScenario As String, ByVal RP_Entity As String, ByVal rpName As String, 
						ByVal LINumberSource As String, ByVal LINumberDestination As String)
			Try
				
						Dim scriptGenerics_Src			As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumberSource &":U7#None:U8#None"		
						Dim scriptGenerics_Dest			As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumberDestination &":U7#None:U8#None"		

						' Get all the prorties for source line item
						Dim Requested_Item_Tier1 	As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Requested_Item_Tier1:"	& scriptGenerics_Src).DataCellEx.DataCellAnnotation
						Dim Description_Tier2 		As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Description_Tier2:" 		& scriptGenerics_Src).DataCellEx.DataCellAnnotation
						Dim POC 					As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#POC:" 					& scriptGenerics_Src).DataCellEx.DataCellAnnotation
						Dim Reference_Doc			As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Reference_Doc:" 			& scriptGenerics_Src).DataCellEx.DataCellAnnotation
						Dim DoallrK_Value			As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#DollarK_Value:" 			& scriptGenerics_Src).DataCellEx.DataCellAnnotation
						Dim By_Obligations			As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#BY_Obligations:" 		& scriptGenerics_Src).DataCellEx.DataCellAnnotation
						Dim By_Plus1_Obligations	As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#BY_Plus1_Obligations:" 	& scriptGenerics_Src).DataCellEx.DataCellAnnotation
						Dim By_Plus2_Obligations	As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#BY_Plus2_Obligations:" 	& scriptGenerics_Src).DataCellEx.DataCellAnnotation
						Dim R_NR					As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#R_NR:" 					& scriptGenerics_Src).DataCellEx.DataCellAnnotation
						Dim ATU_NoUnit				As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#ATU:" 					& scriptGenerics_Src).DataCellEx.DataCellAnnotation
						Dim PPA						As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#PPA:" 					& scriptGenerics_Src).DataCellEx.DataCellAnnotation
						Dim UII						As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#UII:" 					& scriptGenerics_Src).DataCellEx.DataCellAnnotation
						Dim Object_Class			As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Object_Class:" 			& scriptGenerics_Src).DataCellEx.DataCellAnnotation
												
	
						' Formulate the script generic description sorce and destination (Based on Description_Tier2 above)
						Dim scriptGenericsDescr_Src 		As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U6#"& LINumberSource &":U7#None:U8#None"								
						Dim scriptGenericsDescr_Dest 		As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U6#"& LINumberDestination &":U7#None:U8#None"						
						Dim Description_Tier2_Input 		As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "U5#" & Description_Tier2 & ":" & scriptGenericsDescr_Src).DataCellEx.DataCellAnnotation
								
						'Create a new list of memberscript and set it for the target line item
						Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Requested_Item_Tier1:" 		& scriptGenerics_Dest, 0, True, Requested_Item_Tier1))
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Description_Tier2:" 		& scriptGenerics_Dest, 0, True, Description_Tier2))
'						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Description_Tier2_Input:" 	& scriptGenerics_Dest, 0, True, Description_Tier2_Input))
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#POC:" 						& scriptGenerics_Dest, 0, True, POC))
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Reference_Doc:" 			& scriptGenerics_Dest, 0, True, Reference_Doc))
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#DollarK_Value:" 			& scriptGenerics_Dest, 0, True, DoallrK_Value))
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#BY_Obligations:" 			& scriptGenerics_Dest, 0, True, By_Obligations))
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#BY_Plus1_Obligations:"		& scriptGenerics_Dest, 0, True, By_Plus1_Obligations))
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#BY_Plus2_Obligations:"		& scriptGenerics_Dest, 0, True, By_Plus2_Obligations))
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#R_NR:" 						& scriptGenerics_Dest, 0, True, R_NR))
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#ATU:" 						& scriptGenerics_Dest, 0, True, ATU_NoUnit))
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#PPA:" 						& scriptGenerics_Dest, 0, True, PPA))
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#UII:" 						& scriptGenerics_Dest, 0, True, UII))
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Object_Class:"				& scriptGenerics_Dest, 0, True, Object_Class))
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "U5#" & Description_Tier2 & ":"	& scriptGenericsDescr_Dest, 0, True, Description_Tier2_Input))
								
						'Files
						Dim strRefDocType As String = "Reference_Doc"						
						Dim sqlUpdate As New Text.StringBuilder                                                       
							sqlUpdate.Append("Update dbo.DataAttachment ")
							sqlUpdate.Append(" set UD6 = '" & LINumberDestination & "' ")
							sqlUpdate.Append(" WHERE Time = '" & wfTime & "' ")
							sqlUpdate.Append(" AND Flow = '" & rpName & "' ")
							sqlUpdate.Append(" AND Scenario = '" & wfScenario & "' ")
							sqlUpdate.Append(" AND UD6 = '" & LINumberSource & "' ")
							sqlUpdate.Append(" AND Account = '" & strRefDocType & "' ")
						
						Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
							Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp, sqlUpdate.ToString, False)
						End Using 

						'Write the annotations to the database
						Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)

						' Calculate the allocations for the destination line item
						Me.Calc_Single_RP_LI_EXP_Allocations(si, args, RP_Entity, RPName, wfTime, LINumberDestination, PPA, UII, object_Class, ATU_NoUnit)		
												
				Return Nothing
			Catch ex As Exception				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Private Function ClearExpense_RD(ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs, ByVal wfScenario As String, 
						ByVal wfCube As String, ByVal wfTime As String, ByVal rp_Entity As String, ByVal rpName As String, 
						ByVal LineItemNum As String, ByVal LineItemNumInt As Integer, 
						ByVal ScriptGenerics As String, ByVal ScriptGenericsDescr As String)

		Try

			' First add line number for the scriptgenerics
			ScriptGenerics = ScriptGenerics & ":U6#" & LineItemNum 
			ScriptGenericsDescr = ScriptGenericsDescr & ":U6#" & LineItemNum 
			
			'Create a new list of memberscript and value
			Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)
			'but first, get the description stored at the _1 member
			Dim description_Tier2_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "T#" & wfTime & ":A#Description_Tier2:" & scriptGenerics & ":U6#" & LineItemNum)
			Dim description_Tier2 As String = description_Tier2_Info.DataCellEx.DataCellAnnotation	
			
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Requested_Item_Tier1:"				& scriptGenerics, 0, True, String.Empty))  
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Description_Tier2:"					& scriptGenerics, 0, True, String.Empty))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "U5#" & Description_Tier2 & ":"		& scriptGenericsDescr, 0, True, String.Empty))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#POC:" 								& scriptGenerics, 0, True, String.Empty))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#PPA:" 								& scriptGenerics, 0, True, String.Empty))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#ATU:" 								& scriptGenerics, 0, True, String.Empty))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#UII:" 								& scriptGenerics, 0, True, String.Empty))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Object_Class:" 						& scriptGenerics, 0, True, String.Empty))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#R_NR:" 								& scriptGenerics, 0, True, String.Empty))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Reference_Doc:" 					& scriptGenerics, 0, True, String.Empty))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#DollarK_Value:" 					& scriptGenerics, 0, True, String.Empty))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#BY_Obligations:" 					& scriptGenerics, 0, True, String.Empty))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#BY_Plus1_Obligations:" 				& scriptGenerics, 0, True, String.Empty))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#BY_Plus2_Obligations:" 				& scriptGenerics, 0, True, String.Empty))																			
			
			Dim strRefDocType As String = "Reference_Doc"

			'Delete Files
			Dim sqlDelete As New Text.StringBuilder
			sqlDelete.Append("DELETE FROM dbo.DataAttachment ")
			sqlDelete.Append("WHERE Cube = '" & wfCube & "' ")
			sqlDelete.Append("AND Time = '" & wfTime & "' ")
			sqlDelete.Append("AND Flow = '" & rpName & "' ")
			sqlDelete.Append("AND Scenario = '" & wfScenario & "' ")
			sqlDelete.Append("AND UD6 = '" & LineItemNum & "' ")
			sqlDelete.Append("AND Account = '" & strRefDocType & "' ")
			
			Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
		    	Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp, sqlDelete.ToString, True)
			End Using
			
			'Write the annotations to the database
			Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)
			
			'********Cost Storage********							
			'Clear the Allocation								
			Me.Clear_Single_RP_LI_EXP_Allocations(si, args, rp_Entity, rpName,wfTime, LineItemNum)
			'Clear the cost
			Me.Clear_Single_RP_LI_EXP_Cost(si, args, rp_Entity, RPName, wfTime, LineItemNum)
									
			Return Nothing
			
			Catch ex As Exception				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
			
		Private Function CopyExpenseAllFields_PCI(ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs, ByVal wfCube As String, 
						ByVal wfTime As String, ByVal wfScenario As String, ByVal RP_Entity As String, ByVal rpName As String, 
						ByVal LINumberSource As String, ByVal LINumberDestination As String)
			Try
				
						Dim scriptGenerics_Src			As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumberSource &":U7#None:U8#None"		
						Dim scriptGenerics_Dest			As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumberDestination &":U7#None:U8#None"		

						' Get all the prorties for source line item
						Dim Requested_Item_Tier1 	As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Requested_Item_Tier1:"	& scriptGenerics_Src).DataCellEx.DataCellAnnotation
						Dim Description_Tier2 		As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Description_Tier2:" 		& scriptGenerics_Src).DataCellEx.DataCellAnnotation
						Dim POC 					As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#POC:" 					& scriptGenerics_Src).DataCellEx.DataCellAnnotation
						Dim Reference_Doc			As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Reference_Doc:" 			& scriptGenerics_Src).DataCellEx.DataCellAnnotation
						Dim DoallrK_Value			As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#DollarK_Value:" 			& scriptGenerics_Src).DataCellEx.DataCellAnnotation
						Dim By_Obligations			As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#BY_Obligations:" 		& scriptGenerics_Src).DataCellEx.DataCellAnnotation
						Dim By_Plus1_Obligations	As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#BY_Plus1_Obligations:" 	& scriptGenerics_Src).DataCellEx.DataCellAnnotation
						Dim By_Plus2_Obligations	As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#BY_Plus2_Obligations:" 	& scriptGenerics_Src).DataCellEx.DataCellAnnotation
						Dim By_Plus3_Obligations	As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#BY_Plus3_Obligations:" 	& scriptGenerics_Src).DataCellEx.DataCellAnnotation
						Dim By_Plus4_Obligations	As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#BY_Plus4_Obligations:" 	& scriptGenerics_Src).DataCellEx.DataCellAnnotation
						Dim R_NR					As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#R_NR:" 					& scriptGenerics_Src).DataCellEx.DataCellAnnotation
						Dim ATU_NoUnit				As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#ATU:" 					& scriptGenerics_Src).DataCellEx.DataCellAnnotation
						Dim PPA						As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#PPA:" 					& scriptGenerics_Src).DataCellEx.DataCellAnnotation
						Dim UII						As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#UII:" 					& scriptGenerics_Src).DataCellEx.DataCellAnnotation
						Dim Object_Class			As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Object_Class:" 			& scriptGenerics_Src).DataCellEx.DataCellAnnotation
						
						' Formulate the script generic description sorce and destination (Based on Description_Tier2 above)
						Dim scriptGenericsDescr_Src 		As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U6#"& LINumberSource &":U7#None:U8#None"						
'						Dim scriptGenericsDescr_Dest 		As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#" & Description_Tier2 & ":U6#"& LINumberDestination &":U7#None:U8#None"						
						Dim scriptGenericsDescr_Dest 		As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U6#"& LINumberDestination &":U7#None:U8#None"						

						Dim Description_Tier2_Input 		As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "U5#" & Description_Tier2 & ":" & scriptGenericsDescr_Src).DataCellEx.DataCellAnnotation
															
						'Create a new list of memberscript and set it for the target line item
						Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Requested_Item_Tier1:" 		& scriptGenerics_Dest, 0, True, Requested_Item_Tier1))
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Description_Tier2:" 		& scriptGenerics_Dest, 0, True, Description_Tier2))
'						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Description_Tier2_Input:" 	& scriptGenerics_Dest, 0, True, Description_Tier2_Input))
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#POC:" 						& scriptGenerics_Dest, 0, True, POC))
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Reference_Doc:" 			& scriptGenerics_Dest, 0, True, Reference_Doc))
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#DollarK_Value:" 			& scriptGenerics_Dest, 0, True, DoallrK_Value))
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#BY_Obligations:" 			& scriptGenerics_Dest, 0, True, By_Obligations))
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#BY_Plus1_Obligations:"		& scriptGenerics_Dest, 0, True, By_Plus1_Obligations))
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#BY_Plus2_Obligations:"		& scriptGenerics_Dest, 0, True, By_Plus2_Obligations))
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#BY_Plus3_Obligations:"		& scriptGenerics_Dest, 0, True, By_Plus3_Obligations))
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#BY_Plus4_Obligations:"		& scriptGenerics_Dest, 0, True, By_Plus4_Obligations))
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#R_NR:" 						& scriptGenerics_Dest, 0, True, R_NR))
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#ATU:" 						& scriptGenerics_Dest, 0, True, ATU_NoUnit))
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#PPA:" 						& scriptGenerics_Dest, 0, True, PPA))
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#UII:" 						& scriptGenerics_Dest, 0, True, UII))
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Object_Class:"				& scriptGenerics_Dest, 0, True, Object_Class))
						lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "U5#" & Description_Tier2 & ":"	& scriptGenericsDescr_Dest, 0, True, Description_Tier2_Input))
								
						'Files
						Dim strRefDocType As String = "Reference_Doc"						
						Dim sqlUpdate As New Text.StringBuilder                                                       
							sqlUpdate.Append("Update dbo.DataAttachment ")
							sqlUpdate.Append(" set UD6 = '" & LINumberDestination & "' ")
							sqlUpdate.Append(" WHERE Time = '" & wfTime & "' ")
							sqlUpdate.Append(" AND Flow = '" & rpName & "' ")
							sqlUpdate.Append(" AND Scenario = '" & wfScenario & "' ")
							sqlUpdate.Append(" AND UD6 = '" & LINumberSource & "' ")
							sqlUpdate.Append(" AND Account = '" & strRefDocType & "' ")
						
						Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
							Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp, sqlUpdate.ToString, False)
						End Using 

						'Write the annotations to the database
						Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)

						'********Allocation Drivers Storage********					
						' Calculate the allocations for the destination line item
						Me.Calc_Single_RP_LI_EXP_Allocations(si, args, RP_Entity, RPName, wfTime, LINumberDestination, PPA, UII, object_Class, ATU_NoUnit)		
												
				Return Nothing
			Catch ex As Exception				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
				
		Private Function ClearExpense_PCI(ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs, ByVal wfScenario As String, 
						ByVal wfCube As String, ByVal wfTime As String, ByVal rp_Entity As String, ByVal rpName As String, 
						ByVal LineItemNum As String, ByVal LineItemNumInt As Integer, 
						ByVal ScriptGenerics As String, ByVal ScriptGenericsDescr As String)

		Try

			' First add line number for the scriptgenerics
			ScriptGenerics = ScriptGenerics & ":U6#" & LineItemNum 
			ScriptGenericsDescr = ScriptGenericsDescr & ":U6#" & LineItemNum 
			
			'Create a new list of memberscript and value
			Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)
			'but first, get the description stored at the _1 member
			Dim description_Tier2_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "T#" & wfTime & ":A#Description_Tier2:" & scriptGenerics & ":U6#" & LineItemNum)
			Dim description_Tier2 As String = description_Tier2_Info.DataCellEx.DataCellAnnotation	
			
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Requested_Item_Tier1:"				& scriptGenerics, 0, True, String.Empty))  
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Description_Tier2:"					& scriptGenerics, 0, True, String.Empty))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "U5#" & Description_Tier2 & ":"		& scriptGenericsDescr, 0, True, String.Empty))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#POC:" 								& scriptGenerics, 0, True, String.Empty))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#PPA:" 								& scriptGenerics, 0, True, String.Empty))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#ATU:" 								& scriptGenerics, 0, True, String.Empty))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#UII:" 								& scriptGenerics, 0, True, String.Empty))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Object_Class:" 						& scriptGenerics, 0, True, String.Empty))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#R_NR:" 								& scriptGenerics, 0, True, String.Empty))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Reference_Doc:" 					& scriptGenerics, 0, True, String.Empty))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#DollarK_Value:" 					& scriptGenerics, 0, True, String.Empty))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#BY_Obligations:" 					& scriptGenerics, 0, True, String.Empty))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#By_Plus1_Obligations:" 				& scriptGenerics, 0, True, String.Empty))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#By_Plus2_Obligations:" 				& scriptGenerics, 0, True, String.Empty))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#By_Plus3_Obligations:" 				& scriptGenerics, 0, True, String.Empty))
			lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#By_Plus4_Obligations:" 				& scriptGenerics, 0, True, String.Empty))
																																			
			
			Dim strRefDocType As String = "Reference_Doc"

			'Delete Files
			Dim sqlDelete As New Text.StringBuilder
			sqlDelete.Append("DELETE FROM dbo.DataAttachment ")
			sqlDelete.Append("WHERE Cube = '" & wfCube & "' ")
			sqlDelete.Append("AND Time = '" & wfTime & "' ")
			sqlDelete.Append("AND Flow = '" & rpName & "' ")
			sqlDelete.Append("AND Scenario = '" & wfScenario & "' ")
			sqlDelete.Append("AND UD6 = '" & LineItemNum & "' ")
			sqlDelete.Append("AND Account = '" & strRefDocType & "' ")
			
			Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
		    	Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp, sqlDelete.ToString, True)
			End Using
			
			'Write the annotations to the database
			Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)
			
			'********Cost Storage********									

			'Clear the Allocation 							
			Me.Clear_Single_RP_LI_EXP_Allocations(si, args, rp_Entity, rpName,wfTime, LineItemNum)
			'Clear the cost
			Me.Clear_Single_RP_LI_EXP_Cost(si, args, rp_Entity, RPName, wfTime, LineItemNum)
									
			Return Nothing
			
			Catch ex As Exception				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Private Function ExpenseCostClear(ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs, ByVal rp_Entity As String, 
						ByVal rpName As String, ByVal wfTime As Integer, ByVal LineItemNum As String)
			Try
			
				Dim params As New Dictionary(Of String, String)  
				params.Add("rpEntity", rp_Entity)
				params.Add("rpName", rpName) 
				params.Add("WFTime", wfTime) 		
				params.Add("LineItemNum", LineItemNum)		
				
				brapi.Utilities.StartDataMgmtSequence(si, "Clear_Single_RP_LI_NBCost", params)		
							
				Return Nothing
			Catch ex As Exception				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Private Function Clear_Single_RP_EXP_Cost(ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs,  ByVal wfTime As Integer, 
						ByVal rp_Entity As String, ByVal rpName As String)
			Try
				
				Dim params As New Dictionary(Of String, String)  
				params.Add("rpEntity", rp_Entity)
				params.Add("rpName", rpName) 
				params.Add("WFTime", wfTime) 		
				
				brapi.Utilities.StartDataMgmtSequence(si, "Clear_Single_RP_EXP_Cost", params)		
							
				Return Nothing
			Catch ex As Exception				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Private Function updateAllocationsforLineItems (ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs, ByVal wfCube As String, 
						ByVal wfTime As String, ByVal wfScenario As String, ByVal RP_Entity As String, ByVal RPName As String) 
			Try
				' Form script generics string without line number 
				Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U7#None:U8#None"			
				Dim scriptGenericsDescr As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_Input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U7#None:U8#None"	
									
				Dim std_LineItemsDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "Std_LineItems")
				Dim total_Expense_Line_ItemsId As Integer = BRApi.Finance.Members.GetMemberId(si, dimtypeId.UD6, "Total_Expense_LineItems")
				
				' Create a new list of memberscript and value and add memebers
				Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)
								
				'Find number of Expense line items that are already enetered and set the PPA for them and set the PPA 
				Dim ud6LineItemMems As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si,std_LineItemsDimPk, total_Expense_Line_ItemsId, Nothing)
				
				If Not ud6lineItemMems Is Nothing Then
					For Each ud6objLineItem As Member In ud6LineItemMems
						'Get the Line Item member Name
						Dim ud6LineItemName As String = ud6objLineItem.Name	
						Dim scriptLineItem  = scriptGenerics &":U6#" & ud6LineItemName
						Dim objDataCellInfoUsingMemberScript As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si,wfCube,"A#Requested_Item_Tier1:" & scriptLineItem)
						Dim requested_Item_Tier1 As String = objDataCellInfoUsingMemberScript.DataCellEx.DataCellAnnotation
							
						If (Not requested_Item_Tier1.XFEqualsIgnoreCase("")) Then	
							' Set expense allocations for each line item														
							Me.Calc_Single_RP_LI_EXP_Allocations(si, args, RP_Entity, RPName, wfTime, ud6LineItemName, 
								BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#PPA:"& scriptLineItem).DataCellEx.DataCellAnnotation,
								BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#UII:"& scriptLineItem).DataCellEx.DataCellAnnotation,
								BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Object_Class:"& scriptLineItem).DataCellEx.DataCellAnnotation,
								BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#ATU:"& scriptLineItem).DataCellEx.DataCellAnnotation
								)		
							
						End If							
					Next
				End If
				'Write the annotations to the database
				Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)			
				
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try                       
		End Function
			
		Private Function Calc_Single_RP_LI_EXP_Allocations(ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs, ByVal rp_Entity As String, 
						ByVal rpName As String, ByVal wfTime As Integer, ByVal LineItemNum As String, ByVal pPA As String, ByVal uII As String,
						ByVal object_Class As String, ByVal aTU_NoUnit As String)
			Try
				
				Dim params As New Dictionary(Of String, String)  
				params.Add("rpEntity", rp_Entity) 
				params.Add("rpName", rpName) 
				params.Add("WFTime", wfTime) 		
				params.Add("LineItemNum", LineItemNum)
				params.Add("PPA", pPA)
				params.Add("UII", uII)
				params.Add("Object_Class", object_Class)
				params.Add("ATU_NoUnit", aTU_NoUnit)
				
				brapi.Utilities.StartDataMgmtSequence(si, "Calc_Single_RP_LI_EXP_Allocations", params)							
							
				Return Nothing
			Catch ex As Exception			
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function	

		Private Function Clear_Single_RP_LI_EXP_Allocations(ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs, ByVal rp_Entity As String, 
						ByVal rpName As String, ByVal wfTime As Integer, ByVal LineItemNum As String)
						
			Try

				Dim params As New Dictionary(Of String, String)  
				params.Add("rpEntity", rp_Entity) 
				params.Add("rpName", rpName) 
				params.Add("WFTime", wfTime) 		
				params.Add("LineItemNum", LineItemNum)
				
				brapi.Utilities.StartDataMgmtSequence(si, "Clear_Single_RP_LI_EXP_Allocations", params)							
							
				Return Nothing
			Catch ex As Exception			
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function	
				
		Private Function Clear_Single_RP_LI_EXP_Cost(ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs, ByVal rp_Entity As String, 
						ByVal rpName As String, ByVal wfTime As Integer, ByVal LineItemNum As String)
			Try
				
				Dim params As New Dictionary(Of String, String)  
				params.Add("rpEntity", rp_Entity)
				params.Add("rpName", rpName) 
				params.Add("WFTime", wfTime) 		
				params.Add("LineItemNum", LineItemNum)		
				
				brapi.Utilities.StartDataMgmtSequence(si, "Clear_Single_RP_LI_EXP_Cost", params)		
							
				Return Nothing
			Catch ex As Exception				
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Private Function updateLineItemsPPA (ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs, ByVal wfCube As String, ByVal wfTime As String, 
						ByVal wfScenario As String, ByVal RP_Entity As String, ByVal RPName As String, ByVal PPA As String)
			Try
				' Form script generics string without line number 
				Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U7#None:U8#None"			
												
				Dim scriptGenericsDescr As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_Input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U7#None:U8#None"	
									
				Dim std_LineItemsDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "Std_LineItems")
				Dim total_Expense_Line_ItemsId As Integer = BRApi.Finance.Members.GetMemberId(si, dimtypeId.UD6, "Total_Expense_LineItems")
				
				' Create a new list of memberscript and value and add memebers
				Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)
								
				'Find number of Expense line items that are already enetered and set the PPA for them and set the PPA 
				Dim ud6LineItemMems As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si,std_LineItemsDimPk, total_Expense_Line_ItemsId, Nothing)
				If Not ud6lineItemMems Is Nothing Then
					For Each ud6objLineItem As Member In ud6LineItemMems
						'Get the Line Item member Name
						Dim ud6LineItemName As String = ud6objLineItem.Name	
						Dim objDataCellInfoUsingMemberScript As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si,wfCube,"A#Requested_Item_Tier1:" & scriptGenerics &":U6#" & ud6LineItemName)
						Dim requested_Item_Tier1 As String = objDataCellInfoUsingMemberScript.DataCellEx.DataCellAnnotation
							
						If (Not requested_Item_Tier1.XFEqualsIgnoreCase("")) Then	
							' Create a new MemberScriptAndValue for each parameter and add to the list
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#PPA:" & scriptGenerics & ":U6#" & ud6LineItemName, 0, True, PPA))
							lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#UII:" & scriptGenerics & ":U6#" & ud6LineItemName, 0, True, ""))
							
							' TODO Run the allocations again
							Me.Calc_Single_RP_LI_EXP_Allocations(si, args, RP_Entity, RPName, wfTime, ud6LineItemName, PPA, "", "", "")		
							
						End If							
					Next
				End If
				'Write the annotations to the database
				Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)			
				
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try                       
		End Function
				
		Private Function CheckSaveState(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal args As DashboardExtenderArgs, Optional ByVal CheckSave As Boolean = True) As Boolean 'Check if data within the current dashboard (determined by session state) has been saved. If saved, Return Nothing, otherwise throws an exception.
					
					'Get Time from current Workflow.

					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
										
					Dim RPName As String = args.NameValuePairs("RPName")
					Dim LINumber As String =  args.NameValuePairs("LINumber")
					Dim LINumberToSet As String = ""
					Dim LINumberToSet22 As String = ""
					Dim Username As String = si.username
					Dim BlnLogErrors As Boolean = False
					
					'Return False here will disable all save prompts #disablesaveprompt #saveprompt
					Return False
					
					' If No RP is selected, nothing to do
					If RPName = "" Then 
						Return False
					End If
					
					
					
					Dim RPChanged As Boolean = False
					Dim RPNameCopy As String = args.NameValuePairs.XFGetValue("RPNameCopy")
					
					Dim BlnDifferentYearRPs As Boolean = String.CompareOrdinal(left(RPNameCopy,2),left(RPName,2))
					If BlnDifferentYearRPs Then
						Return False
					End If
					
					If Not String.IsNullOrEmpty(RPNameCopy) AndAlso RPNameCopy<> RPName AndAlso RPNameCopy<>"None"  Then 
						RPChanged= True
						RPName = RPNameCopy
					End If
					
					'When changing scenarios, old RP year will not be same so Save Prompt must be skipped
					
					
					'Do not run if RO is not in Edit Mode or CheckSave is false
					If Not CheckSave OrElse (Not String.IsNullOrEmpty(RPName) AndAlso  Not rputils.Is_RP_Editable(si, RPName)) Then
						Return False
					End If
					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)
					'Perform save check based what dashboard the user is currently in, as determined by session state
					'No check for create, reporting, and review because there is nothing to save in those dashboards
					Select Case BRApi.State.GetSessionState(si, False, ClientModuletype.Unknown,"", "","dashState","dashState").TextValue
						Case "Create"
							Return False
						Case "Edit"
							#Region "Edit"
							'Check values are saved by comparing parameters to current RP
							Dim billets As String = args.NameValuePairs("Billets")
							Dim autoAdd As String = args.NameValuePairs("AutoAdd")
							Dim increDecre As String = args.NameValuePairs("IncreDecre")
							Dim reprogramming As String = args.NameValuePairs("Reprogramming")
							Dim personnelQuarters As String = args.NameValuePairs("PersonnelQuarters")
							Dim OMQuarters As String = args.NameValuePairs("O&MQuarters")
								
							Dim leadOffice1 As String = args.NameValuePairs("LeadOffice1")
							Dim leadOffice2 As String = args.NameValuePairs("LeadOffice2")
							Dim leadOffice3 As String = args.NameValuePairs("LeadOffice3")
							Dim leadOfficePOC1 As String = args.NameValuePairs("LeadOfficePOC1")
							Dim leadOfficePOC2 As String = args.NameValuePairs("LeadOfficePOC2")
							Dim leadOfficePOC3 As String = args.NameValuePairs("LeadOfficePOC3")
							Dim leadOfficePhone1 As String = args.NameValuePairs("LeadOfficePhone1")
							Dim leadOfficePhone2 As String = args.NameValuePairs("LeadOfficePhone2")
							Dim leadOfficePhone3 As String = args.NameValuePairs("LeadOfficePhone3")
							Dim initialEstimate As String = args.NameValuePairs("InitialEstimate")
							Dim initialEstimateMil As String = args.NameValuePairs("InitialEstimateMil")
							Dim initialEstimateCiv As String = args.NameValuePairs("InitialEstimateCiv")
							Dim baseFunding As String = args.NameValuePairs("BaseFunding")
							Dim baseFundingMil As String = args.NameValuePairs("BaseFundingMil")
							Dim baseFundingCiv As String = args.NameValuePairs("BaseFundingCiv")
							Dim baseFundingComments As String = args.NameValuePairs("BaseFundingComments")
							Dim relatedRP1 As String = args.NameValuePairs("FYRelatedRP1")
							Dim relatedRP2 As String = args.NameValuePairs("FYRelatedRP2")
							Dim relatedRP3 As String = args.NameValuePairs("FYRelatedRP3")
							Dim olderRelatedRP1 As String = args.NameValuePairs("OlderRelatedRP1")
							Dim olderRelatedRP2 As String = args.NameValuePairs("OlderRelatedRP2")
							Dim olderRelatedRP3 As String = args.NameValuePairs("OlderRelatedRP3")
							Dim execSummary As String = args.NameValuePairs("ExecSummary")
							 
							Dim problem As String = args.NameValuePairs("Problem")
							Dim fundingImpact As String = args.NameValuePairs("FundingImpact")
							Dim denialImpact As String = args.NameValuePairs("DenialImpact")
							Dim affectOthers As String = args.NameValuePairs("AffectOthers")
							Dim ROI As String = args.NameValuePairs("ROI")
							Dim alignment As String = args.NameValuePairs("Alignment")			 
																		
							Dim scriptGenericsEdit As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"								
									
							'using a global function to avoid using brapi functions too many times and use api.data.calculate via a finance rule instead
							'set the script generics and parent account to be used in the global function
							globals.SetStringValue("scriptGenerics", scriptGenericsEdit)
							globals.SetStringValue("parAccount", "RP_Attributes")				

							'Set a generic dictionary as an argument in the rule below
							Dim DictionaryEdit As New Dictionary(Of String, String)
							
								BUDFM_AttributeSupport.GetRPAttributes(si, globals)
							
							If Not globals.GetObject("attributeDict") Is Nothing
								
								Dim attributeDict As Dictionary(Of String, String) = globals.GetObject("attributeDict")

#Region "Log"

If BlnLogErrors Then
 
If billets 				<> attributeDict.GetValueOrEmpty("Number_of_Billets").ToString        Then 
	BRApi.ErrorLog.LogMessage(si, "billets 				" & vbcrlf & billets 					& " , " & vbcrlf & attributeDict.GetValueOrEmpty("Number_of_Billets").ToString        ) 
End If
If autoAdd 				<> attributeDict.GetValueOrEmpty("Add_General_Detail").ToString       Then 
	BRApi.ErrorLog.LogMessage(si, "autoAdd 				" & vbcrlf & autoAdd 					& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Add_General_Detail").ToString       ) 
End If
If increDecre 				<> attributeDict.GetValueOrEmpty("Increase_Decrease").ToString        Then 
	BRApi.ErrorLog.LogMessage(si, "increDecreQQ 			" &  vbcrlf & increDecre 					& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Increase_Decrease").ToString        ) 
End If
If reprogramming 			<> attributeDict.GetValueOrEmpty("Part_of_Reprogramming").ToString    Then 
	BRApi.ErrorLog.LogMessage(si, "reprogramming			" &  vbcrlf & reprogramming 				& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Part_of_Reprogramming").ToString    ) 
End If
If personnelQuarters 		<> attributeDict.GetValueOrEmpty("Personnel_Qtrs").ToString           Then 
	BRApi.ErrorLog.LogMessage(si, "personnelQuarters		" &  vbcrlf & personnelQuarters 			& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Personnel_Qtrs").ToString           ) 
End If
If OMQuarters 				<> attributeDict.GetValueOrEmpty("OS_Qtrs").ToString                  Then 
	BRApi.ErrorLog.LogMessage(si, "OMQuarters 			" &  vbcrlf & OMQuarters 					& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("OS_Qtrs").ToString                  ) 
End If
If leadOffice1 			<> attributeDict.GetValueOrEmpty("Lead_Office1").ToString             Then 
	BRApi.ErrorLog.LogMessage(si, "leadOffice1 			" &  vbcrlf & leadOffice1 				& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Lead_Office1").ToString             ) 
End If
If leadOffice2 			<> attributeDict.GetValueOrEmpty("Lead_Office2").ToString             Then 
	BRApi.ErrorLog.LogMessage(si, "leadOffice2 			" &  vbcrlf & leadOffice2 				& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Lead_Office2").ToString             ) 
End If
If leadOffice3 			<> attributeDict.GetValueOrEmpty("Lead_Office3").ToString             Then 
	BRApi.ErrorLog.LogMessage(si, "leadOffice3 			" &  vbcrlf & leadOffice3 				& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Lead_Office3").ToString             ) 
End If
If leadOfficePOC1 			<> attributeDict.GetValueOrEmpty("Lead_Office_POC1").ToString         Then 
	BRApi.ErrorLog.LogMessage(si, "leadOfficePOC1 		" &  vbcrlf & leadOfficePOC1 				& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Lead_Office_POC1").ToString         ) 
End If
If leadOfficePOC2 			<> attributeDict.GetValueOrEmpty("Lead_Office_POC2").ToString         Then 
	BRApi.ErrorLog.LogMessage(si, "leadOfficePOC2 		" &  vbcrlf & leadOfficePOC2 				& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Lead_Office_POC2").ToString         ) 
End If
If leadOfficePOC3 			<> attributeDict.GetValueOrEmpty("Lead_Office_POC3").ToString         Then 
	BRApi.ErrorLog.LogMessage(si, "leadOfficePOC3 		" &  vbcrlf & leadOfficePOC3 				& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Lead_Office_POC3").ToString         ) 
End If
If leadOfficePhone1 		<> attributeDict.GetValueOrEmpty("Lead_Office_Phone1").ToString       Then 
	BRApi.ErrorLog.LogMessage(si, "leadOfficePhone1		" &  vbcrlf & leadOfficePhone1			& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Lead_Office_Phone1").ToString       ) 
End If
If leadOfficePhone2 		<> attributeDict.GetValueOrEmpty("Lead_Office_Phone2").ToString       Then 
	BRApi.ErrorLog.LogMessage(si, "leadOfficePhone2		" &  vbcrlf & leadOfficePhone2			& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Lead_Office_Phone2").ToString       ) 
End If
If leadOfficePhone3 		<> attributeDict.GetValueOrEmpty("Lead_Office_Phone3").ToString       Then 
	BRApi.ErrorLog.LogMessage(si, "leadOfficePhone3		" &  vbcrlf & leadOfficePhone3			& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Lead_Office_Phone3").ToString       ) 
End If
If initialEstimate 		<> attributeDict.GetValueOrEmpty("Initial_Estimate").ToString         Then 
	BRApi.ErrorLog.LogMessage(si, "initialEstimate 		" &  vbcrlf & initialEstimate 			& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Initial_Estimate").ToString         ) 
End If
If initialEstimateMil 		<> attributeDict.GetValueOrEmpty("Initial_Estimate_MIL_FTP").ToString Then 
	BRApi.ErrorLog.LogMessage(si, "initialEstimateMil 	" &  vbcrlf & initialEstimateMil 			& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Initial_Estimate_MIL_FTP").ToString ) 
End If
If initialEstimateCiv 		<> attributeDict.GetValueOrEmpty("Initial_Estimate_CIV_FTP").ToString Then 
	BRApi.ErrorLog.LogMessage(si, "initialEstimateCiv 	" &  vbcrlf & initialEstimateCiv 			& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Initial_Estimate_CIV_FTP").ToString ) 
End If
If baseFunding 			<> attributeDict.GetValueOrEmpty("Base_Funding").ToString             Then 
	BRApi.ErrorLog.LogMessage(si, "baseFunding 			" &  vbcrlf & baseFunding 				& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Base_Funding").ToString             ) 
End If
If baseFundingMil 			<> attributeDict.GetValueOrEmpty("Base_Funding_MIL_FTP").ToString     Then 
	BRApi.ErrorLog.LogMessage(si, "baseFundingMil 		" &  vbcrlf & baseFundingMil 				& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Base_Funding_MIL_FTP").ToString     ) 
End If
If baseFundingCiv 			<> attributeDict.GetValueOrEmpty("Base_Funding_CIV_FTP").ToString     Then 
	BRApi.ErrorLog.LogMessage(si, "baseFundingCiv 		" &  vbcrlf & baseFundingCiv 				& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Base_Funding_CIV_FTP").ToString     ) 
End If
If baseFundingComments 	<> attributeDict.GetValueOrEmpty("Base_Funding_Comments").ToString    Then 
	BRApi.ErrorLog.LogMessage(si, "baseFundingComments 	" &  vbcrlf & baseFundingComments 		& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Base_Funding_Comments").ToString    ) 
End If
If relatedRP1 				<> attributeDict.GetValueOrEmpty("FY_Related_RP1").ToString           Then 
	BRApi.ErrorLog.LogMessage(si, "relatedRP1 			" &  vbcrlf & relatedRP1 					& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("FY_Related_RP1").ToString           ) 
End If
If relatedRP2 				<> attributeDict.GetValueOrEmpty("FY_Related_RP2").ToString           Then 
	BRApi.ErrorLog.LogMessage(si, "relatedRP2 			" &  vbcrlf & relatedRP2 					& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("FY_Related_RP2").ToString           ) 
End If
If relatedRP3 				<> attributeDict.GetValueOrEmpty("FY_Related_RP3").ToString           Then 
	BRApi.ErrorLog.LogMessage(si, "relatedRP3 			" &  vbcrlf & relatedRP3 					& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("FY_Related_RP3").ToString           ) 
End If
If olderRelatedRP1 		<> attributeDict.GetValueOrEmpty("Older_Related_RP1").ToString        Then 
	BRApi.ErrorLog.LogMessage(si, "olderRelatedRP1 		" &  vbcrlf & olderRelatedRP1 			& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Older_Related_RP1").ToString        ) 
End If
If olderRelatedRP2 		<> attributeDict.GetValueOrEmpty("Older_Related_RP2").ToString        Then 
	BRApi.ErrorLog.LogMessage(si, "olderRelatedRP2 		" &  vbcrlf & olderRelatedRP2 			& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Older_Related_RP2").ToString        ) 
End If
If olderRelatedRP3 		<> attributeDict.GetValueOrEmpty("Older_Related_RP3").ToString        Then 
	BRApi.ErrorLog.LogMessage(si, "olderRelatedRP3 		" &  vbcrlf & olderRelatedRP3 			& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Older_Related_RP3").ToString        ) 
End If
If execSummary 			<> attributeDict.GetValueOrEmpty("Exec_Summary").ToString             Then 
	BRApi.ErrorLog.LogMessage(si, "execSummary 			" &  vbcrlf & execSummary 				& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Exec_Summary").ToString             ) 
End If
If problem 				<> attributeDict.GetValueOrEmpty("Problem").ToString                  Then 
	BRApi.ErrorLog.LogMessage(si, "problem 				" &  vbcrlf & problem 					& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Problem").ToString                  ) 
End If
If fundingImpact 			<> attributeDict.GetValueOrEmpty("Funding_Impact").ToString           Then 
	BRApi.ErrorLog.LogMessage(si, "fundingImpact			" &  vbcrlf & fundingImpact 				& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Funding_Impact").ToString           ) 
End If
If denialImpact 			<> attributeDict.GetValueOrEmpty("Denial_Impact").ToString            Then 
	BRApi.ErrorLog.LogMessage(si, "denialImpact			" &  vbcrlf & denialImpact				& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Denial_Impact").ToString            ) 
End If
If affectOthers 			<> attributeDict.GetValueOrEmpty("Affect_Others").ToString            Then 
	BRApi.ErrorLog.LogMessage(si, "affectOthers			" &  vbcrlf & affectOthers				& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Affect_Others").ToString            ) 
End If
If ROI 					<> attributeDict.GetValueOrEmpty("ROI").ToString                      Then 
	BRApi.ErrorLog.LogMessage(si, "ROI 					" &  vbcrlf & ROI 						& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("ROI").ToString                      ) 
End If
If alignment 				<> attributeDict.GetValueOrEmpty("Alignment").ToString                Then 
	BRApi.ErrorLog.LogMessage(si, "alignment				" &  vbcrlf & alignment 					& " , " &  vbcrlf & attributeDict.GetValueOrEmpty("Alignment").ToString                ) 
End If
End If
#End Region								
								
								Try
									If (billets = attributeDict.GetValueOrEmpty("Number_of_Billets").ToString) And 
									(autoAdd = attributeDict.GetValueOrEmpty("Add_General_Detail").ToString) And 
									(increDecre = attributeDict.GetValueOrEmpty("Increase_Decrease").ToString) And 
									(reprogramming = attributeDict.GetValueOrEmpty("Part_of_Reprogramming").ToString) And 
									(personnelQuarters = attributeDict.GetValueOrEmpty("Personnel_Qtrs").ToString) And 
									(OMQuarters = attributeDict.GetValueOrEmpty("OS_Qtrs").ToString) And 
									(leadOffice1 = attributeDict.GetValueOrEmpty("Lead_Office1").ToString) And
									(leadOffice2 = attributeDict.GetValueOrEmpty("Lead_Office2").ToString) And
									(leadOffice3 = attributeDict.GetValueOrEmpty("Lead_Office3").ToString) And
									(leadOfficePOC1 = attributeDict.GetValueOrEmpty("Lead_Office_POC1").ToString) And
									(leadOfficePOC2 = attributeDict.GetValueOrEmpty("Lead_Office_POC2").ToString) And
									(leadOfficePOC3 = attributeDict.GetValueOrEmpty("Lead_Office_POC3").ToString) And
									(leadOfficePhone1 = attributeDict.GetValueOrEmpty("Lead_Office_Phone1").ToString) And
									(leadOfficePhone2 = attributeDict.GetValueOrEmpty("Lead_Office_Phone2").ToString) And
									(leadOfficePhone3 = attributeDict.GetValueOrEmpty("Lead_Office_Phone3").ToString) And
									(initialEstimate = attributeDict.GetValueOrEmpty("Initial_Estimate").ToString) And
									(initialEstimateMil = attributeDict.GetValueOrEmpty("Initial_Estimate_MIL_FTP").ToString) And
									(initialEstimateCiv = attributeDict.GetValueOrEmpty("Initial_Estimate_CIV_FTP").ToString) And
									(baseFunding = attributeDict.GetValueOrEmpty("Base_Funding").ToString) And
									(baseFundingMil = attributeDict.GetValueOrEmpty("Base_Funding_MIL_FTP").ToString) And
									(baseFundingCiv = attributeDict.GetValueOrEmpty("Base_Funding_CIV_FTP").ToString) And
									(baseFundingComments = attributeDict.GetValueOrEmpty("Base_Funding_Comments").ToString) And
									(relatedRP1 = attributeDict.GetValueOrEmpty("FY_Related_RP1").ToString) And
									(relatedRP2 = attributeDict.GetValueOrEmpty("FY_Related_RP2").ToString) And
									(relatedRP3 = attributeDict.GetValueOrEmpty("FY_Related_RP3").ToString) And
									(olderRelatedRP1 = attributeDict.GetValueOrEmpty("Older_Related_RP1").ToString) And
									(olderRelatedRP2 = attributeDict.GetValueOrEmpty("Older_Related_RP2").ToString) And
									(olderRelatedRP3 = attributeDict.GetValueOrEmpty("Older_Related_RP3").ToString) And
									(execSummary = attributeDict.GetValueOrEmpty("Exec_Summary").ToString) And
									(problem = attributeDict.GetValueOrEmpty("Problem").ToString) And
									(fundingImpact = attributeDict.GetValueOrEmpty("Funding_Impact").ToString) And
									(denialImpact = attributeDict.GetValueOrEmpty("Denial_Impact").ToString) And
									(affectOthers = attributeDict.GetValueOrEmpty("Affect_Others").ToString) And
									(ROI = attributeDict.GetValueOrEmpty("ROI").ToString) And
									(alignment = attributeDict.GetValueOrEmpty("Alignment").ToString) Then
										Return False
									Else
										Return True
									End If
								Catch ex As Exception
									Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
								End Try
							End If
							#End Region
						Case "AddEditBillets"
							#Region "AddEditBillets"
							'Check values are saved by comparing parameters to current RP
							 Dim positionNumber As String = args.NameValuePairs("PositionNumber")
							 Dim positionTitle As String = args.NameValuePairs("PositionTitle")
							 Dim lineItemComment As String = args.NameValuePairs("LineItemComment")
							 Dim billetType As String = args.NameValuePairs("BilletType")
							 Dim gradeType As String = args.NameValuePairs("GradeType")
							 Dim gradeRank As String = args.NameValuePairs("GradeRank")
							 Dim ADReserve As String = args.NameValuePairs("ADReserve")
							 Dim reserveType As String = args.NameValuePairs("ReserveType")
							 Dim speCodeOccuSeries As String = args.NameValuePairs("SpeCodeOccuSeries")
							 
							 Dim billetATU As String = args.NameValuePairs("BilletATU")
							 Dim OPFAC As String = args.NameValuePairs("OPFAC")
							 Dim UII As String = args.NameValuePairs("BilletUII")
							 Dim conusOConus As String = args.NameValuePairs("ConusOConus")
							 Dim detachedDuty As String = args.NameValuePairs("DetachedDuty")
							 Dim detachedDutyLocation As String = args.NameValuePairs("DetachedDutyLocation")
							 Dim ICASSCosts As String = args.NameValuePairs("ICASSCosts")
							 
							 Dim PPEType As String = args.NameValuePairs("PPEType")
							 Dim PPEPPA As String = args.NameValuePairs("PPEPPA")
							 Dim PPEATU As String = args.NameValuePairs("PPEATU")
							 Dim electronicFlightBag As String = args.NameValuePairs("ElectronicFlightBag")
							 Dim acquisitionProject As String = args.NameValuePairs("AcquisitionProject")
							 Dim termBillet As String = args.NameValuePairs("TermBillet")
							 Dim backgroundInvestigationType As String = args.NameValuePairs("BackgroundInvestigationType")
							 Dim computerType As String = args.NameValuePairs("ComputerType")
							 
							 Dim buildOutChoice As String = args.NameValuePairs("BuildOutChoice")
							 Dim leaseChoice As String = args.NameValuePairs("LeaseChoice")
							 Dim leasePPA As String = args.NameValuePairs("LeasePPA")
							 Dim leaseATU As String = args.NameValuePairs("LeaseATU")
							 Dim furnitureReqd As String = args.NameValuePairs("FurnitureReqd")
							 Dim utilitiesReqd As String = args.NameValuePairs("UtilitiesReqd")
							 Dim utilitiesPPA As String = args.NameValuePairs("UtilitiesPPA")
							 Dim utilitiesATU As String = args.NameValuePairs("UtilitiesATU")
							 Dim LINumberCopy = args.NameValuePairs("BLTCopy")
					
							'Logic to set the default line item when the Billet screen is opened
							'Dim LINumberToSet As String = String.Empty
							
							If LINumber.Length > 0 Then
								
								If Not String.IsNullOrEmpty(LINumberCopy) AndAlso LINumberCopy<> LINumber Then
									LINumber = LINumberCopy	
								Else
									LINumber  = LINumber	
								End If

								'Get the number of billets and integer from the line item member to compare and return appropriate line item per the RP selected
								Dim rightChars As Integer = LINumber.Substring(9,2).XFConvertToInt			
								
								Dim number_of_Billets As Integer = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Number_of_Billets:E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").DataCellEx.DataCellAnnotation.XFConvertToInt
								
								If  rightChars > number_of_Billets
									LINumberToSet = "LineItem_01"	
								Else
									LINumberToSet = LINumber	
								End If
							Else
								LINumberToSet = "LineItem_01"
						
							End If
									
							Dim scriptGenericsBillet As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumberToSet & ":U7#None:U8#None"		
							'using a global function to avoid using brapi functions too many times and use api.data.calculate via a finance rule instead
							'set the script generics and parent account to be used in the global function
							globals.SetStringValue("scriptGenerics", scriptGenericsBillet)
							globals.SetStringValue("parAccount", "Billet_LineItem_Data")				

							'Set a generic dictionary as an argument in the rule below
							Dim DictionaryBillet As New Dictionary(Of String, String)
							
								BUDFM_AttributeSupport.GetRPAttributes(si, globals)
							
							If Not globals.GetObject("attributeDict") Is Nothing
								
								Dim attributeDict As Dictionary(Of String, String) = globals.GetObject("attributeDict")
								
								'For the ATU creteria, we need to derive the parent ATU since we store it in NoUnit
								'Derive Billet_ATU from Billet_ATU_NoUnit since we stored it as a base but they chose a parentDim Billet_ATU_NoUnit As String = Billet_ATU_NoUnit_Info
								Dim Billet_ATU_NoUnit As String = attributeDict.GetValueOrEmpty("Billet_ATU")
								Dim Billet_ATU As String = String.Empty
								If Billet_ATU_NoUnit.Length > 0
									Billet_ATU = Billet_ATU_NoUnit.Substring(0, Billet_ATU_NoUnit.Length - 7)
									'If nothing Return Zero
								End If
								
								'Derive PPE_ATU from PPE_ATU_NoUnit since we stored it as a base but they chose a parent
								Dim PPE_ATU_NoUnit As String = attributeDict.GetValueOrEmpty("PPE_ATU")	
								Dim PPE_ATU As String = String.Empty
								If PPE_ATU_NoUnit.Length > 0
									PPE_ATU = PPE_ATU_NoUnit.Substring(0, PPE_ATU_NoUnit.Length - 7)
									'If nothing Return Zero
								End If	
														
		'						'Derive lease_ATU from lease_ATU_NoUnit since we stored it as a base but they chose a parent
								Dim lease_ATU_NoUnit As String = attributeDict.GetValueOrEmpty("Lease_ATU")	
								Dim lease_ATU As String = String.Empty
								If lease_ATU_NoUnit.Length > 0
									lease_ATU = lease_ATU_NoUnit.Substring(0, lease_ATU_NoUnit.Length - 7)
									'If nothing Return Zero
								End If	
								
								'Derive UTL_ATU from UTL_ATU_NoUnit since we stored it as a base but they chose a parent
								Dim UTL_ATU_NoUnit As String = attributeDict.GetValueOrEmpty("Utilities_ATU")	
								Dim UTL_ATU As String = String.Empty
								If UTL_ATU_NoUnit.Length > 0
									UTL_ATU = UTL_ATU_NoUnit.Substring(0, UTL_ATU_NoUnit.Length - 7)
									'If nothing Return Zero
								End If	
								
						'Check if saved

#Region "log"
If BlnLogErrors Then

If  positionNumber   <> attributeDict.GetValueOrEmpty("Position_Number").ToString   Then 
BRApi.ErrorLog.LogMessage(si, " positionNumber  " & vbcrlf &  positionNumber   &  vbcrlf & attributeDict.GetValueOrEmpty("Position_Number").ToString   ) 
End If
If  positionTitle   <> attributeDict.GetValueOrEmpty("Position_Title").ToString   Then 
BRApi.ErrorLog.LogMessage(si, " positionTitle  " & vbcrlf &  positionTitle   &  vbcrlf & attributeDict.GetValueOrEmpty("Position_Title").ToString   )
 End If
If  lineItemComment   <> attributeDict.GetValueOrEmpty("LineItem_Comment").ToString   Then 
BRApi.ErrorLog.LogMessage(si, " lineItemComment  " & vbcrlf &  lineItemComment   &  vbcrlf & attributeDict.GetValueOrEmpty("LineItem_Comment").ToString   )
 End If
If  billetType   <> attributeDict.GetValueOrEmpty("Billet_Type").ToString   Then 
BRApi.ErrorLog.LogMessage(si, " billetType  " & vbcrlf &  billetType   &  vbcrlf & attributeDict.GetValueOrEmpty("Billet_Type").ToString   )
 End If
If  gradeType   <> attributeDict.GetValueOrEmpty("Grade_Type").ToString   Then 
BRApi.ErrorLog.LogMessage(si, " gradeType  " & vbcrlf &  gradeType   &  vbcrlf & attributeDict.GetValueOrEmpty("Grade_Type").ToString   )
 End If
If  gradeRank   <> attributeDict.GetValueOrEmpty("Grade_Rank").ToString   Then 
BRApi.ErrorLog.LogMessage(si, " gradeRank  " & vbcrlf &  gradeRank   &  vbcrlf & attributeDict.GetValueOrEmpty("Grade_Rank").ToString   ) 

End If
If  ADReserve   <> attributeDict.GetValueOrEmpty("AD_Reserve").ToString  Then 
BRApi.ErrorLog.LogMessage(si, " ADReserve  " & vbcrlf &  ADReserve   &  vbcrlf & attributeDict.GetValueOrEmpty("AD_Reserve").ToString  )
 End If
If  reserveType   <> attributeDict.GetValueOrEmpty("Reserve_Type").ToString  Then 
BRApi.ErrorLog.LogMessage(si, " reserveType  " & vbcrlf &  reserveType   &  vbcrlf & attributeDict.GetValueOrEmpty("Reserve_Type").ToString  ) 
End If
If  speCodeOccuSeries   <> attributeDict.GetValueOrEmpty("Spe_Code_Occu_Series").ToString  Then 
BRApi.ErrorLog.LogMessage(si, " speCodeOccuSeries  " & vbcrlf &  speCodeOccuSeries   &  vbcrlf & attributeDict.GetValueOrEmpty("Spe_Code_Occu_Series").ToString  ) 
End If
If  billetATU   <> Billet_ATU   Then 
BRApi.ErrorLog.LogMessage(si, " billetATU  " & vbcrlf &  billetATU   &  vbcrlf & Billet_ATU   ) 
End If
If  OPFAC   <> attributeDict.GetValueOrEmpty("OPFAC").ToString   Then 
BRApi.ErrorLog.LogMessage(si, " OPFAC  " & vbcrlf &  OPFAC   &  vbcrlf & attributeDict.GetValueOrEmpty("OPFAC").ToString   ) 
End If
If  UII   <> attributeDict.GetValueOrEmpty("Billet_UII").ToString   Then 
BRApi.ErrorLog.LogMessage(si, " UII  " & vbcrlf &  UII   &  vbcrlf & attributeDict.GetValueOrEmpty("Billet_UII").ToString   ) 
End If
If  conusOConus   <> attributeDict.GetValueOrEmpty("CONUS_OCONUS").ToString   Then 
BRApi.ErrorLog.LogMessage(si, " conusOConus  " & vbcrlf &  conusOConus   &  vbcrlf & attributeDict.GetValueOrEmpty("CONUS_OCONUS").ToString   ) 
End If
If  detachedDuty   <> attributeDict.GetValueOrEmpty("Detached_Duty").ToString   Then 
BRApi.ErrorLog.LogMessage(si, " detachedDuty  " & vbcrlf &  detachedDuty   &  vbcrlf & attributeDict.GetValueOrEmpty("Detached_Duty").ToString   ) 
End If
If  detachedDutyLocation   <> attributeDict.GetValueOrEmpty("Detached_Duty_Location").ToString   Then
 BRApi.ErrorLog.LogMessage(si, " detachedDutyLocation  " & vbcrlf &  detachedDutyLocation   &  vbcrlf & attributeDict.GetValueOrEmpty("Detached_Duty_Location").ToString   ) 
End If
If  ICASSCosts   <> attributeDict.GetValueOrEmpty("ICASS_Costs").ToString  Then 
BRApi.ErrorLog.LogMessage(si, " ICASSCosts  " & vbcrlf &  ICASSCosts   &  vbcrlf & attributeDict.GetValueOrEmpty("ICASS_Costs").ToString  ) 
End If
If  PPEType   <> attributeDict.GetValueOrEmpty("PPE_Type").ToString  Then 
BRApi.ErrorLog.LogMessage(si, " PPEType  " & vbcrlf &  PPEType   &  vbcrlf & attributeDict.GetValueOrEmpty("PPE_Type").ToString  ) 
End If
If  PPEPPA   <> attributeDict.GetValueOrEmpty("PPE_PPA").ToString  Then 
BRApi.ErrorLog.LogMessage(si, " PPEPPA  " & vbcrlf &  PPEPPA   &  vbcrlf & attributeDict.GetValueOrEmpty("PPE_PPA").ToString  ) 
End If
If  PPEATU   <> PPE_ATU  Then 
BRApi.ErrorLog.LogMessage(si, " PPEATU  " & vbcrlf &  PPEATU   &  vbcrlf & PPE_ATU  ) 
End If
If  electronicFlightBag   <> attributeDict.GetValueOrEmpty("Electronic_Flight_Bag").ToString  Then 
BRApi.ErrorLog.LogMessage(si, " electronicFlightBag  " & vbcrlf &  electronicFlightBag   &  vbcrlf & attributeDict.GetValueOrEmpty("Electronic_Flight_Bag").ToString  ) 
End If
If  acquisitionProject   <> attributeDict.GetValueOrEmpty("Acquisition_Project").ToString  Then 
BRApi.ErrorLog.LogMessage(si, " acquisitionProject  " & vbcrlf &  acquisitionProject   &  vbcrlf & attributeDict.GetValueOrEmpty("Acquisition_Project").ToString  ) 
End If
If  termBillet   <> attributeDict.GetValueOrEmpty("Term_Billet").ToString  Then 
BRApi.ErrorLog.LogMessage(si, " termBillet  " & vbcrlf &  termBillet   &  vbcrlf & attributeDict.GetValueOrEmpty("Term_Billet").ToString  ) 
End If
If  backgroundInvestigationType   <> attributeDict.GetValueOrEmpty("Background_Investigation_Type").ToString  Then 
BRApi.ErrorLog.LogMessage(si, " backgroundInvestigationType  " & vbcrlf &  backgroundInvestigationType   &  vbcrlf & attributeDict.GetValueOrEmpty("Background_Investigation_Type").ToString  ) 
End If
If  computerType   <> attributeDict.GetValueOrEmpty("Computer_Type").ToString  Then 
BRApi.ErrorLog.LogMessage(si, " computerType  " & vbcrlf &  computerType   &  vbcrlf & attributeDict.GetValueOrEmpty("Computer_Type").ToString  ) 
End If
If  buildOutChoice   <> attributeDict.GetValueOrEmpty("Build_Out_Choice").ToString  Then 
BRApi.ErrorLog.LogMessage(si, " buildOutChoice  " & vbcrlf &  buildOutChoice   &  vbcrlf & attributeDict.GetValueOrEmpty("Build_Out_Choice").ToString  ) 
End If
If  leaseChoice   <> attributeDict.GetValueOrEmpty("Lease_Choice").ToString  Then 
BRApi.ErrorLog.LogMessage(si, " leaseChoice  " & vbcrlf &  leaseChoice   &  vbcrlf & attributeDict.GetValueOrEmpty("Lease_Choice").ToString  ) 
End If
If  leasePPA   <> attributeDict.GetValueOrEmpty("Lease_PPA").ToString  Then 
BRApi.ErrorLog.LogMessage(si, " leasePPA  " & vbcrlf &  leasePPA   &  vbcrlf & attributeDict.GetValueOrEmpty("Lease_PPA").ToString  ) 
End If
If  leaseATU   <> lease_ATU  Then 
BRApi.ErrorLog.LogMessage(si, " leaseATU  " & vbcrlf &  leaseATU   &  vbcrlf & lease_ATU  ) 
End If
If  furnitureReqd   <> attributeDict.GetValueOrEmpty("Furniture_Reqd").ToString  Then 
BRApi.ErrorLog.LogMessage(si, " furnitureReqd  " & vbcrlf &  furnitureReqd   &  vbcrlf & attributeDict.GetValueOrEmpty("Furniture_Reqd").ToString  ) 
End If
If  utilitiesReqd   <> attributeDict.GetValueOrEmpty("Utilities_Reqd").ToString  Then 
BRApi.ErrorLog.LogMessage(si, " utilitiesReqd  " & vbcrlf &  utilitiesReqd   &  vbcrlf & attributeDict.GetValueOrEmpty("Utilities_Reqd").ToString  ) 
End If
If  utilitiesPPA   <> attributeDict.GetValueOrEmpty("Utilities_PPA").ToString  Then 
BRApi.ErrorLog.LogMessage(si, " utilitiesPPA  " & vbcrlf &  utilitiesPPA   &  vbcrlf & attributeDict.GetValueOrEmpty("Utilities_PPA").ToString  ) 
End If
If  utilitiesATU   <> UTL_ATU Then 
BRApi.ErrorLog.LogMessage(si, " utilitiesATU  " & vbcrlf &  utilitiesATU   &  vbcrlf & UTL_ATU ) 
End If
End If
#End Region ' log


								Try
									If (positionNumber = attributeDict.GetValueOrEmpty("Position_Number").ToString) And 
									(positionTitle = attributeDict.GetValueOrEmpty("Position_Title").ToString) And 
									(lineItemComment = attributeDict.GetValueOrEmpty("LineItem_Comment").ToString) And 
									(billetType = attributeDict.GetValueOrEmpty("Billet_Type").ToString) And 
									(gradeType = attributeDict.GetValueOrEmpty("Grade_Type").ToString) And 
									(gradeRank = attributeDict.GetValueOrEmpty("Grade_Rank").ToString) And 
									(ADReserve = attributeDict.GetValueOrEmpty("AD_Reserve").ToString) And
									(reserveType = attributeDict.GetValueOrEmpty("Reserve_Type").ToString) And
									(speCodeOccuSeries = attributeDict.GetValueOrEmpty("Spe_Code_Occu_Series").ToString) And
									(billetATU = Billet_ATU) And 
									(OPFAC = attributeDict.GetValueOrEmpty("OPFAC").ToString) And 
									(UII = attributeDict.GetValueOrEmpty("Billet_UII").ToString) And 
									(conusOConus = attributeDict.GetValueOrEmpty("CONUS_OCONUS").ToString) And 
									(detachedDuty = attributeDict.GetValueOrEmpty("Detached_Duty").ToString) And 
									(detachedDutyLocation = attributeDict.GetValueOrEmpty("Detached_Duty_Location").ToString) And 
									(ICASSCosts = attributeDict.GetValueOrEmpty("ICASS_Costs").ToString) And
									(PPEType = attributeDict.GetValueOrEmpty("PPE_Type").ToString) And
									(PPEPPA = attributeDict.GetValueOrEmpty("PPE_PPA").ToString) And
									(PPEATU = PPE_ATU) And
									(electronicFlightBag = attributeDict.GetValueOrEmpty("Electronic_Flight_Bag").ToString) And
									(acquisitionProject = attributeDict.GetValueOrEmpty("Acquisition_Project").ToString) And
									(termBillet = attributeDict.GetValueOrEmpty("Term_Billet").ToString) And
									(backgroundInvestigationType = attributeDict.GetValueOrEmpty("Background_Investigation_Type").ToString) And
									(computerType = attributeDict.GetValueOrEmpty("Computer_Type").ToString) And
									(buildOutChoice = attributeDict.GetValueOrEmpty("Build_Out_Choice").ToString) And
									(leaseChoice = attributeDict.GetValueOrEmpty("Lease_Choice").ToString) And
									(leasePPA = attributeDict.GetValueOrEmpty("Lease_PPA").ToString) And
									(leaseATU = lease_ATU) And
									(furnitureReqd = attributeDict.GetValueOrEmpty("Furniture_Reqd").ToString) And
									(utilitiesReqd = attributeDict.GetValueOrEmpty("Utilities_Reqd").ToString) And
									(utilitiesPPA = attributeDict.GetValueOrEmpty("Utilities_PPA").ToString) And
									(utilitiesATU = UTL_ATU) Then
										Return False
									Else
										Return True
									End If
								Catch ex As Exception
									Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
								End Try
							End If
							#End Region
						Case "AddEditNonBillets"
							#Region "AddEditNonBillets"
							Dim componentName As String = args.ComponentInfo.Component.Name
							'Check values are saved by comparing parameters to current RP
							Dim requestedItemTier1 As String = args.NameValuePairs("RequestedItemTier1")
							Dim POC As String = args.NameValuePairs("POC")
							Dim dollarKValue As String = args.NameValuePairs("DollarKValue")
							Dim recurringNonRecurring As String = args.NameValuePairs("RNR")
							Dim ATU As String = args.NameValuePairs("ATU")
							Dim PPA As String = args.NameValuePairs("PPA")
							Dim UII As String = args.NameValuePairs("UII")
							Dim OC As String = args.NameValuePairs("OC")
							Dim D1 As String = args.NameValuePairs("D1")
							Dim D2 As String = args.NameValuePairs("D2")	
							LINumber  =  args.NameValuePairs("NBLT")
							Dim LINumberCopy As String = args.NameValuePairs("NBLTCopy")

							If String.IsNullOrEmpty(LINumber) AndAlso String.IsNullOrEmpty (LINumberCopy) Then 
								Return True
							End If
							
							'Logic to set the default line item when the Billet screen is opened
							'Dim LINumberToSet As String = String.Empty
							
							If Not String.IsNullOrEmpty(LINumber) Then
								If (Not String.IsNullOrEmpty(LINumberCopy)) AndAlso (LINumberCopy <> LINumber) Then
									LINumberToSet = LINumberCopy.ToString	
								Else
									LINumberToSet  = LINumber	
								End If
							Else
								LINumberToSet = "NBLineItem_01"
							End If

							Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumberToSet & ":U7#None:U8#None"			
							Dim scriptGenericsDescr As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#Description_Tier2_Input:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U6#"& LINumberToSet & ":U7#None:U8#None"				
							
							'using a global function to avoid using brapi functions too many times and use api.data.calculate via a finance rule instead
							'set the script generics and parent account to be used in the global function
							globals.SetStringValue("scriptGenerics", scriptGenerics)
							globals.SetStringValue("parAccount", "NonBillet_LineItem_Data")					
			
							'Set a generic dictionary as an argument in the rule below
							Dim Dictionary As New Dictionary(Of String, String)
							
								BUDFM_AttributeSupport.GetRPAttributes(si, globals)
							
							If Not globals.GetObject("attributeDict") Is Nothing
							
								Dim attributeDict As Dictionary(Of String, String) = globals.GetObject("attributeDict")
									
	'							'Get info for the Non-Billet
								Dim Requested_Item_Cost_Line As String = attributeDict.GetValueOrEmpty("Requested_Item_Tier1")
								'Get the ItemNum to use to find the description Input account
								Dim requested_ItemNum As Integer
								Dim DescriptionMatches As Boolean = False
								If (Not Requested_Item_Cost_Line = "") 
									Dim requested_Item_Tier1Split As List(Of String) = StringHelper.SplitString(Requested_Item_Cost_Line, "_")
									requested_ItemNum = requested_Item_Tier1Split(0).XFConvertToInt
									If requested_ItemNum >=400
										'Show Text Box 
										If D2 <>  BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "U5#" & requested_ItemNum & "0_1:" 		& scriptGenericsDescr).DataCellEx.DataCellAnnotation Then 	
											'Error
										Else
											DescriptionMatches = True
										End If
									Else
										'Show Combo Box 
										If D1 <>  attributeDict.GetValueOrEmpty("Description_Tier2").ToString Then 	
											'Error
										Else
											DescriptionMatches = True
										End If									
									End If
								Else
									DescriptionMatches = True	
								End If	
							
								'Derive ATU from ATU_NoUnit since we stored it as a base but they chose a parent
								Dim ATU_NoUnit As String = attributeDict.GetValueOrEmpty("ATU")	
								
								Dim ATUDB As String = String.Empty
								'If it already has a value, derive the parent member from the stored NoUnit child
								If ATU_NoUnit.Length > 0
									ATUDB = ATU_NoUnit.Substring(0, ATU_NoUnit.Length - 7)
								End If
															
								#Region "Description Comparasion"
		
								'Dim requested_Item_Tier1 As String = Requested_Item_Cost_Line
								'Dim DescriptionMatches As Boolean = False
 								'If Not String.IsNullOrEmpty(requested_Item_Tier1) Then 
								'	Dim requested_Item_Tier1Split As List(Of String) = StringHelper.SplitString(requested_Item_Tier1, "_")
								'	Dim ReqItemSplit As List(Of String) = StringHelper.SplitString(Requested_Item_Cost_Line, "_")
								'	Dim ReqItemNum As Integer = ReqItemSplit(0).XFConvertToInt
								'	If ReqItemNum >=400
								'		'Show Text Box 
								'		If D2 <>  BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "U5#" & requested_ItemNum & "0_1:" 		& scriptGenericsDescr).DataCellEx.DataCellAnnotation Then 	
								'			'Error
								'		Else
								'			DescriptionMatches = True
								'		End If
								'	Else
								'		'Show Combo Box 
								'		If D1 <>  attributeDict.GetValueOrEmpty("Description_Tier2").ToString Then 	
								'			'Error
								'		Else
								'			DescriptionMatches = True
								'		End If									
								'	End If
								'Else
								'	DescriptionMatches = True		
								'End If
									
								#End Region 
								
								#Region "Log"
								If BlnLogErrors Then
	
								If requestedItemTier1 <>  Requested_Item_Cost_Line Then 	
									Brapi.ErrorLog.LogMessage(si, "Mismatch requestedItemTier1 : " &						requestedItemTier1)
									Brapi.ErrorLog.LogMessage(si, "Mismatch Requested_Item_Cost_Line: " &		Requested_Item_Cost_Line)
								End If
								
								If POC <>  attributeDict.GetValueOrEmpty("POC").ToString Then 	
									Brapi.ErrorLog.LogMessage(si, "Mismatch POC : " &						POC)
									Brapi.ErrorLog.LogMessage(si, "Mismatch attributeDict.GetValueOrEmpty(POC).ToString: " &		attributeDict.GetValueOrEmpty("POC").ToString)
								End If
								
								If dollarKValue <>  attributeDict.GetValueOrEmpty("DollarK_Value").ToString Then 	
									Brapi.ErrorLog.LogMessage(si, "Mismatch dollarKValue : " &						dollarKValue)
									Brapi.ErrorLog.LogMessage(si, "Mismatch attributeDict.GetValueOrEmpty(DollarK_Value).ToString: " &		attributeDict.GetValueOrEmpty("DollarK_Value").ToString)
								End If
								If recurringNonRecurring <>  attributeDict.GetValueOrEmpty("R_NR").ToString Then 	
									Brapi.ErrorLog.LogMessage(si, "Mismatch recurringNonRecurring : " &						recurringNonRecurring)
									Brapi.ErrorLog.LogMessage(si, "Mismatch attributeDict.GetValueOrEmpty(R_NR).ToString: " &		attributeDict.GetValueOrEmpty("R_NR").ToString)
								End If
								If ATU <>  ATUDB Then 	
									Brapi.ErrorLog.LogMessage(si, "Mismatch ATU : " &						ATU)
									Brapi.ErrorLog.LogMessage(si, "Mismatch ATUDB: " &		ATUDB)
								End If
								If PPA <>  attributeDict.GetValueOrEmpty("PPA").ToString Then 	
									Brapi.ErrorLog.LogMessage(si, "Mismatch PPA : " &						PPA)
									Brapi.ErrorLog.LogMessage(si, "Mismatch attributeDict.GetValueOrEmpty(PPA).ToString: " &		attributeDict.GetValueOrEmpty("PPA").ToString)
								End If
								If UII <>  attributeDict.GetValueOrEmpty("UII").ToString Then 	
									Brapi.ErrorLog.LogMessage(si, "Mismatch UII : " &						UII)
									Brapi.ErrorLog.LogMessage(si, "Mismatch attributeDict.GetValueOrEmpty(UII).ToString: " &		attributeDict.GetValueOrEmpty("UII").ToString)
								End If
								If OC <>  attributeDict.GetValueOrEmpty("Object_Class").ToString Then 	
									Brapi.ErrorLog.LogMessage(si, "Mismatch OC : " &						OC)
									Brapi.ErrorLog.LogMessage(si, "Mismatch attributeDict.GetValueOrEmpty(Object_Class).ToString: " &		attributeDict.GetValueOrEmpty("Object_Class").ToString)
								End If
								End If
								#End Region ' log
								Try
									If (requestedItemTier1 = Requested_Item_Cost_Line And 
										POC = attributeDict.GetValueOrEmpty("POC").ToString) And
										(dollarKValue = attributeDict.GetValueOrEmpty("DollarK_Value").ToString) And
										(recurringNonRecurring = attributeDict.GetValueOrEmpty("R_NR").ToString) And
										(ATU = ATUDB) And
										(PPA = attributeDict.GetValueOrEmpty("PPA").ToString) And
										(UII = attributeDict.GetValueOrEmpty("UII").ToString) And 
										(OC = attributeDict.GetValueOrEmpty("Object_Class").ToString) And
										DescriptionMatches Then
										Return False
									Else
										Return True 
									End If
								Catch ex As Exception
									Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
								End Try
							Else 

							End If
							#End Region
						Case "Reporting"
							Return False
						Case "ConcReview"
							Return False
					End Select
					Return Nothing
			End Function

Public Sub RunPreSaveStepsForRP(
					ByVal si As SessionInfo,
					ByVal Cube As String,
					ByVal Scenario As String,
					ByVal Time As String,
					ByVal RPName As String,
					ByVal Reason_ChangeLog As String,
					ByVal Description_Changelog As String,
					ByVal LineItem As String)					
	Try
'		' 1. Check User Role  
'		' 2. Make sure RP Is Is edit mode
'		' 3. If change comment is required, make sure comment is entered
'		' 4. Log change comment, if required

'		'Ensure user is able to make changes
		Dim UserReadOnly = rpUtils.Is_Read_Only(si)
		If UserReadOnly
			Throw New Exception ("User group is View Only. No edits can be made.")
		End If

'		'Get the RPName and other parameters
		Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
		If Not rpUtils.Is_RP_Editable(si, RPName)
			Throw New Exception (RPName & " is set to View Only. No edits can be made.")
		End If
				
'		Dim commentRequired As String = BRApi.Finance.Flow.Text(si, rpId, 3, DimConstants.Unknown, DimConstants.Unknown)
	 
'		If commentRequired.XFEqualsIgnoreCase("CC_02") Then						
		If rpUtils.Is_RP_CC_Required(si, RPName)	
			If Reason_ChangeLog.XFEqualsIgnoreCase("") Then
				Throw New Exception("Change Reason is empty, please choose one from the list")				
			Else If (description_ChangeLog.XFEqualsIgnoreCase("") And reason_ChangeLog.XFContainsIgnoreCase("OTH") )  Then
				Throw New Exception("Please enter a description for change.")
			End If				
			
			Me.SetChangeLogComment(si, Cube, RP_Entity, Scenario, Time, RPName, LineItem, reason_ChangeLog, description_ChangeLog)												
							
		End If 'commentRequired		
		
		UpdateLastEditedTimestamp(si, Cube, RP_Entity, Scenario, Time, RPName)
			
		
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Sub


Public Sub RunPreSaveStepsForRP_BLT_NBLT_Deletion(
					ByVal si As SessionInfo,
					ByVal Cube As String,
					ByVal Scenario As String,
					ByVal Time As String,
					ByVal RPName As String,
					ByVal Reason_ChangeLog As String,
					ByVal Description_Changelog As String,
					ByVal LineItem As String)					
	Try
'		' 1. Check User Role  
'		' 2. Make sure RP Is Is edit mode
'		' 3. If change comment is required, make sure comment is entered
'		' 4. Log change comment, if required

'		'Ensure user is able to make changes
		Dim UserReadOnly = rpUtils.Is_Read_Only(si)
		If UserReadOnly
			Throw New Exception ("User group is View Only. No edits can be made.")
		End If

'		'Get the RPName and other parameters
		Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
		If Not rpUtils.Is_RP_Editable(si, RPName)
			Throw New Exception (RPName & " is set to View Only. No edits can be made.")
		End If
				
'		Dim commentRequired As String = BRApi.Finance.Flow.Text(si, rpId, 3, DimConstants.Unknown, DimConstants.Unknown)
	 
'		If commentRequired.XFEqualsIgnoreCase("CC_02") Then						
		If rpUtils.Is_RP_CC_Required(si, RPName)	
			If Reason_ChangeLog.XFEqualsIgnoreCase("") Then
				Throw New Exception("Change Reason is empty, please choose one from the list")				
			Else If (description_ChangeLog.XFEqualsIgnoreCase("") And reason_ChangeLog.XFContainsIgnoreCase("OTH") )  Then
				Throw New Exception("Please enter a description for change.")
			End If				
			
			'Me.SetChangeLogComment(si, Cube, RP_Entity, Scenario, Time, RPName, LineItem, reason_ChangeLog, description_ChangeLog)												
			Me.SetChangeLogComment_BLT_NBLT_Deletion(si, Cube, RP_Entity, Scenario, Time, RPName, LineItem, reason_ChangeLog, description_ChangeLog)	
			
		End If 'commentRequired		
		
		UpdateLastEditedTimestamp(si, Cube, RP_Entity, Scenario, Time, RPName)
			
		
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Sub

Public Sub RunPostSaveStepsForRP(
					ByVal globals As BRGlobals, 
					ByVal si As SessionInfo, 
					ByVal wfCube As String,
					ByVal RP_Entity As String,
					ByVal wfScenario As String, 
					ByVal wfTime As String, 
					ByVal RPName As String)
		Try
			'Get attributes
			Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"								
			Dim scriptGenericsCCR As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Comment_01"
			
			
					'using a global function to avoid using brapi functions too many times and use api.data.calculate via a finance rule instead
					'set the script generics and parent account to be used in the global function
					globals.SetStringValue("scriptGenerics", scriptGenerics)
					globals.SetStringValue("parAccount", "RP_Attributes")

					'Set a generic dictionary as an argument in the rule below
					Dim Dictionary As New Dictionary(Of String, String)
					
						BUDFM_AttributeSupport.GetRPAttributes(si, globals)
					
					If Not globals.GetObject("attributeDict") Is Nothing
					
						Dim attributeDict As Dictionary(Of String, String) = globals.GetObject("attributeDict")
						Dim scriptGenericsPeriodic As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Periodic:A#Funding:F#" & RPName & ":O#Top:I#Top:U1#Total_Appropriations:U2#Total_Investment:U3#Total_ObjectClass:U4#Total_ATU:U6#Top_UD6_LineItem:U7#None:U8#None"
						Dim RPCost As DataCellInfoUsingMemberScript = brapi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "U5#Total_CostLine:" & scriptGenericsPeriodic)
						Dim RPCost_Status As String = RPCost.DataCellEx.DataCell.CellAmount.ToString
						Dim CCR As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#C_N__ConcReview:" & scriptGenericsCCR)
						Dim CCR_Status = CCR.DataCellEx.DataCellAnnotation
						
						'If any fields on Page 1 are not filled in, return Red/"Incomplete". 			
						If  attributeDict.GetValueOrEmpty("Number_of_Billets") = String.Empty Or _
							attributeDict.GetValueOrEmpty("Add_General_Detail") = String.Empty Or _
							attributeDict.GetValueOrEmpty("Increase_Decrease") = String.Empty Or _
							attributeDict.GetValueOrEmpty("Part_of_Reprogramming") = String.Empty Or _
							attributeDict.GetValueOrEmpty("Personnel_Qtrs") = String.Empty Or _
							attributeDict.GetValueOrEmpty("OS_Qtrs") = String.Empty Then
							UpdateRpCompletionStatusFunction(si, wfcube, RP_Entity, wfscenario, wfTime, RPName, "Incomplete") 							
					
						'If Page 1 is completed, but Page 2 or Page 3 have missing fields and/or the RP Cost is 0, and the Concurrent Review Lead Office has not Concurred return "Not Calculated".	
						Else If attributeDict.GetValueOrEmpty("Number_of_Billets") <> String.Empty AndAlso _ 'Page 1
							attributeDict.GetValueOrEmpty("Add_General_Detail") <> String.Empty AndAlso _
							attributeDict.GetValueOrEmpty("Increase_Decrease") <> String.Empty AndAlso _
							attributeDict.GetValueOrEmpty("Part_of_Reprogramming") <> String.Empty AndAlso _
							attributeDict.GetValueOrEmpty("Personnel_Qtrs") <> String.Empty AndAlso _
							attributeDict.GetValueOrEmpty("OS_Qtrs") <> String.Empty AndAlso _
							attributeDict.GetValueOrEmpty("Lead_Office1") = String.Empty Or _ 'Page 2
							attributeDict.GetValueOrEmpty("Lead_Office_POC1") = String.Empty Or _
							attributeDict.GetValueOrEmpty("Lead_Office_Phone1") = String.Empty Or _
							attributeDict.GetValueOrEmpty("Initial_Estimate") = String.Empty Or _
							attributeDict.GetValueOrEmpty("Initial_Estimate_MIL_FTP") = String.Empty Or _
							attributeDict.GetValueOrEmpty("Initial_Estimate_CIV_FTP") = String.Empty Or _
							attributeDict.GetValueOrEmpty("Base_Funding") = String.Empty Or _
							attributeDict.GetValueOrEmpty("Base_Funding_Comments") = String.Empty Or _
							attributeDict.GetValueOrEmpty("Base_Funding_MIL_FTP") = String.Empty Or _
							attributeDict.GetValueOrEmpty("Base_Funding_CIV_FTP") = String.Empty Or _
							attributeDict.GetValueOrEmpty("Exec_Summary") = String.Empty Or _ 
							attributeDict.GetValueOrEmpty("Problem") = String.Empty Or _'Page 3
							attributeDict.GetValueOrEmpty("Funding_Impact") = String.Empty Or _
							attributeDict.GetValueOrEmpty("Denial_Impact") = String.Empty Or _
							attributeDict.GetValueOrEmpty("Affect_Others") = String.Empty Or _
							RPCost_Status = 0 Or _ 'RP Cost
							CCR_Status <> "C" Then
							UpdateRpCompletionStatusFunction(si, wfcube, RP_Entity, wfscenario, wfTime, RPName, "Not Calculated")

						Else
							'If all required fields on Page 1,2, and 3 are completed, the RP Cost is calculated, and the CCR has Concurred, return "Complete"
							UpdateRpCompletionStatusFunction(si, wfcube, RP_Entity, wfscenario, wfTime, RPName, "Complete")
							
						End If
					End If
		Catch 
		End Try
End Sub



#End Region

	

	End Class
End Namespace
