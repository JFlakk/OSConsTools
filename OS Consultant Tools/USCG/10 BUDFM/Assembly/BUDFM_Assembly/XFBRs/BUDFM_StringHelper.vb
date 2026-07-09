Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardStringFunction.BUDFM_StringHelper
	Public Class MainClass
		' legacy class state (BudFm_ParamHelper)
		Dim rpUtils As New BUDFM_RP_Utilities
		Private NotInheritable Class AppnToolbarConfig
			Public ReadOnly CreateRP As String
			Public ReadOnly RPContentEdit As String
			Public ReadOnly RPContentView As String
			Public ReadOnly BilletsMain As String
			Public ReadOnly BilletsAddEdit As String
			Public ReadOnly BilletsView As String
			Public ReadOnly Reporting As String
			Public ReadOnly ConcReview As String
			Public ReadOnly Fallback As String
			Public Sub New(ByVal createRP As String, ByVal rpContentEdit As String, ByVal rpContentView As String, ByVal billetsMain As String, ByVal billetsAddEdit As String, ByVal billetsView As String, ByVal reporting As String, ByVal concReview As String, ByVal fallback As String)
				Me.CreateRP = createRP
				Me.RPContentEdit = rpContentEdit
				Me.RPContentView = rpContentView
				Me.BilletsMain = billetsMain
				Me.BilletsAddEdit = billetsAddEdit
				Me.BilletsView = billetsView
				Me.Reporting = reporting
				Me.ConcReview = concReview
				Me.Fallback = fallback
			End Sub
		End Class
		Private Shared ReadOnly ToolbarConfigByAppn As New Dictionary(Of String, AppnToolbarConfig)(StringComparer.OrdinalIgnoreCase) From {
			{"OS", New AppnToolbarConfig("OS_RP_ToolbarCreateRP", "OS_RP_Toolbar_03b", "OS_RP_Toolbar_03bView", "OS_Billets_Toolbar", "OS_NonBillets_Toolbar", "OS_NonBillets_ToolbarView", "OS_Rpt_Toolbar", "OS_RP_ToolbarConcReview", "OS_RP_Toolbar_03")}
		}

		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
				rpUtils.Main(si, globals, api, New ExtenderArgs())
				Select Case True
					Case args.FunctionName.XFEqualsIgnoreCase("ResolveRPMode") : Return ResolveRPMode(si, args)
					Case args.FunctionName.XFEqualsIgnoreCase("RPControlState") : Return RPControlState(si, args)
					Case args.FunctionName.XFEqualsIgnoreCase("ButtonImageSwitcher") : Return ButtonImageSwitcher(si, args)
					Case args.FunctionName.XFEqualsIgnoreCase("GetModeDashboard") : Return GetModeDashboard(si, args)
					Case args.FunctionName.XFEqualsIgnoreCase("ColumnVisible") : Return String.Empty ' TODO port from BudEx_ParamHelper
					' Everything else runs the legacy BudFm_ParamHelper logic, ported
					' wholesale below (140 functions; dead duplicate block removed).
					Case Else : Return InnerMain(si, globals, api, args)
				End Select
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Function

		' Next mode from the clicked button + current mode. Security trumps the
		' click: a read-only user (GBL check) can never enter Edit.
		Private Function ResolveRPMode(ByVal si As SessionInfo, ByVal args As DashboardStringFunctionArgs) As String
			Dim btn As String = args.NameValuePairs.XFGetValue("Button", "")
			Dim mode As String = args.NameValuePairs.XFGetValue("Mode", "View")
			If Workspace.GBL.GBL_Assembly.GBL_Helpers.Is_Read_Only(si, "prm_Security_BudFm_r_Auditor") Then Return "View"
			Select Case True
				Case btn.XFContainsIgnoreCase("Edit") : Return "Edit"
				Case btn.XFContainsIgnoreCase("Save"), btn.XFContainsIgnoreCase("Cancel"), btn.XFContainsIgnoreCase("View") : Return "View"
				Case Else : Return If(mode.XFEqualsIgnoreCase("Edit"), "Edit", "View")
			End Select
		End Function

		' Editable state for a control, from the Mode param + security. Prop picks
		' the polarity: IsEnabled wants True when editable, ReadOnly wants False.
		Private Function RPControlState(ByVal si As SessionInfo, ByVal args As DashboardStringFunctionArgs) As String
			Dim mode As String = args.NameValuePairs.XFGetValue("Mode", "View")
			Dim prop As String = args.NameValuePairs.XFGetValue("Prop", "IsEnabled")
			Dim canEdit As Boolean = mode.XFEqualsIgnoreCase("Edit") AndAlso Not Workspace.GBL.GBL_Assembly.GBL_Helpers.Is_Read_Only(si, "prm_Security_BudFm_r_Auditor")
			If prop.XFEqualsIgnoreCase("ReadOnly") Then Return (Not canEdit).ToString()
			Return canEdit.ToString()
		End Function

		' Mode-twin embed picker: returns Base & "Edit"/"View" from the Mode param,
		' for EmbeddedDashboard bindings that swap a button strip (or any twin pair)
		' by mode. Security trumps the param — a read-only user always gets View.
		' Usage: XFBR(...BUDFM_StringHelper, GetModeDashboard, Base=[OS_RP_Content1], Mode=[|!prm_Mode_OS!|])
		Private Function GetModeDashboard(ByVal si As SessionInfo, ByVal args As DashboardStringFunctionArgs) As String
			Dim base As String = args.NameValuePairs.XFGetValue("Base", String.Empty)
			Dim mode As String = args.NameValuePairs.XFGetValue("Mode", "View")
			Dim edit As Boolean = mode.XFEqualsIgnoreCase("Edit") AndAlso Not Workspace.GBL.GBL_Assembly.GBL_Helpers.Is_Read_Only(si, "prm_Security_BudFm_r_Auditor")
			Return base & If(edit, "Edit", "View")
		End Function

		' Mode-driven button image. TODO: swap in the app's actual image names
		' when porting ButtonImageSwitcher from the inline rule.
		Private Function ButtonImageSwitcher(ByVal si As SessionInfo, ByVal args As DashboardStringFunctionArgs) As String
			Dim mode As String = args.NameValuePairs.XFGetValue("Mode", "View")
			Return If(mode.XFEqualsIgnoreCase("Edit"), "EditModeOn", "EditModeOff")
		End Function

		' ===== ported wholesale from BudFm_ParamHelper (dead dup block removed) =====
		Private Function InnerMain(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
				
#Region "GetActiveOrReserveList"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetActiveOrReserveList, billet_Type=|!prm_BLT_Billet_Type!|)
				If args.FunctionName.XFEqualsIgnoreCase("GetActiveOrReserveList") Then
					Dim billet_Type As String = args.NameValuePairs("billet_Type")
					'BRApi.ErrorLog.LogMessage(si, "billet_Type: " & billet_Type)
					If billet_Type.XFEqualsIgnoreCase("Military")
						Return "U8#Military_Employment_Type.Children.Remove(NA_Military_Employment_Type)"
					Else
						
						   Return "U8#NA_Military_Employment_Type"
							'Do Nothing as billet_Type must be Civilian
					End If
						
				End If

				
#End Region 'GetActiveOrReserve

#Region "MatchedRPList_MOSP"
						If args.FunctionName.XFEqualsIgnoreCase("MatchedRPList_MOSP") Then
							'Dim MemberFilterScript As String = "F#Total_RPs.Base"
							Dim MemberFilterScriptWF As String = "F#FY_2024_RPS.Base"
							If (args.NameValuePairs("SearchQuery") = "") Then
								'Return MemberFilterScript
								Return MemberFilterScriptWF
							Else
								MemberFilterScriptWF = MemberFilterScriptWF & ".Where((Name Contains [|!prmRPSearchQuery!|] and Name Contains MOSP) Or (Description Contains [|!prmRPSearchQuery!|] And Name Contains MOSP))"
								
								
							End If
								Return MemberFilterScriptWF
						End If

#End Region

#Region "GetAPPBCATOption"

				If args.FunctionName.XFEqualsIgnoreCase("GetAPPBCATOption") Then
					Dim Appropriation As String = args.NameValuePairs("Appropriation")
                    Dim Budget_Category As String = args.NameValuePairs("Budget_Category")
					'BRApi.ErrorLog.LogMessage(si, "billet_Type: " & billet_Type)
					If Appropriation.XFEqualsIgnoreCase("OS") 
						If Budget_Category.XFEqualsIgnoreCase("Value_Items")
							Return "1,2,3,4,NA"
						Else If Budget_Category.XFEqualsIgnoreCase("Display_Items")
					  		Return "BUDCAT I,BUDCAT II,BUDCAT III,BUDCAT IV"
						End If
					Else	
						Return "NA"
						
					End If
					  
				End If
				
#End Region 'GetAPPBCATOption

#Region "GetTermBillet_Year_Option"
		If args.FunctionName.XFEqualsIgnoreCase("GetTermBillet_Year_Option") Then
			'BRApi.ErrorLog.LogMessage(si, "Pass6")
					Dim Term_Billet As String = args.NameValuePairs("Term_Billet")
					If Term_Billet.XFEqualsIgnoreCase("Y")
							
							Return "T#Root.Children"
					Else 	
						'BRApi.ErrorLog.LogMessage(si, "TermBillet: " & Term_Billet)
						Return " "
							'Do Nothing as PPE_Required must be No
					End If
					
				End If
#End Region		

#Region "GetBuildOut_Lease_Option"
'		If args.FunctionName.XFEqualsIgnoreCase("GetBuildOut_Lease_Option") Then
'					Dim Build_Out As String = args.NameValuePairs("Build_Out")
'					If Build_Out.XFEqualsIgnoreCase("Y")
							
'							Return "U8#Total_Lease.Base.WHERE(Name Contains No)"
'					Else 	
'						'BRApi.ErrorLog.LogMessage(si, "BuildOut: " & Build_Out)
'						Return "U8#Total_Lease.Base"
'					End If
					
'				End If
#End Region

#Region "GetOPFAC_UII_Option"
		If args.FunctionName.XFEqualsIgnoreCase("GetOPFAC_UII_Option") Then
			        Dim wfCube As String = args.NameValuePairs("WFCube")
					Dim OPFAC As String = args.NameValuePairs("OPFAC")
                    Dim ATU As String = args.NameValuePairs("ATU")
                    Dim CompareText1 As String = "49"
                    Dim CompareText2 As String = "98_70098_6"
					
                    If (OPFAC.XFContainsIgnoreCase(CompareText1)) Or (OPFAC.XFContainsIgnoreCase(CompareText2))
							
					        'Return "U2#NoInvestment"
							Return "U2#Billet_Investments.Base"
					Else 	
						'BRApi.ErrorLog.LogMessage(si, "BuildOut: " & Build_Out)
						    
							Return "U2#NoInvestment,U2#Billet_Investments.Base.Remove(NoInvestment)"
						
					End If
					
				End If
#End Region

#Region "GetBIlletCivilianOption"

				If args.FunctionName.XFEqualsIgnoreCase("GetBIlletCivilianOption") Then
					Dim billet_Type As String = args.NameValuePairs("billet_Type")
                    Dim AD_Reserve As String = args.NameValuePairs("AD_Reserve")
					'BRApi.ErrorLog.LogMessage(si, "billet_Type: " & billet_Type)
					If billet_Type.XFEqualsIgnoreCase("Civilian") Or (billet_Type.XFEqualsIgnoreCase("Military") And AD_Reserve.XFEqualsIgnoreCase("Active_Duty"))
						Return "U8#NA_Reserve"
					Else
					  If billet_Type.XFEqualsIgnoreCase("Military") And AD_Reserve.XFEqualsIgnoreCase("Reserve")
						Return "U8#Reserve.Base.Remove(NA_Reserve)"
						
					End If
					  End If  
				End If
				
#End Region 'GetBilletCivilianOption

#Region "GetUTL_ATU_PPA"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetPPEType_ATU_PPA, Required=|!prm_BLT_PPERequired!|, Filter_Value=PPE_Type)
				If args.FunctionName.XFEqualsIgnoreCase("GetUTL_ATU_PPA") Then
					Dim Required As String = args.NameValuePairs("Required")
					Dim Filter_Value As String = args.NameValuePairs("Filter_Value")
									
					'BRApi.ErrorLog.LogMessage(si, "Filter_Value: " & Filter_Value)
					If Required.XFEqualsIgnoreCase("Y")
						If Filter_Value.XFEqualsIgnoreCase("PPA")
							Return "U1#OS.Base"
						Else If Filter_Value.XFEqualsIgnoreCase("ATU")
							Return "U4#Total_ATU.Children"
						End If
					Else 					
						If Filter_Value.XFEqualsIgnoreCase("PPA")
							Return "U1#NA_PPA"
						Else If Filter_Value.XFEqualsIgnoreCase("ATU")
							Return "U4#NA_ATU"
						End If
					End If
					
				End If


#End Region 'GetPPEType_ATU_PPA

#Region "GetPPEType"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetPPEType, Billet_Type=|!prm_BLT_BilletType_OS!|)
				If args.FunctionName.XFEqualsIgnoreCase("GetPPEType") Then
					Dim Billet_Type As String = args.NameValuePairs("Billet_Type")
									
		'BRApi.ErrorLog.LogMessage(si, "GetPPEType XFBR Ran")
					If Billet_Type.XFEqualsIgnoreCase("Military")
						Return "U8#Total_PPE.Children.Remove(NA_PPE_Type)"						
					Else 							
						Return "U8#NA_PPE_Type"
					End If
					
				End If


#End Region 'GetPPEType_ATU_PPA

#Region "GetPPE_ATU_PPA"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetPPE_ATU_PPA, Billet_Type=|!prm_BLT_BilletType_OS!|, PPE_Type=|!prm_BLT_PPEType_OS!|, Filter_Value=PPA)
				If args.FunctionName.XFEqualsIgnoreCase("GetPPE_ATU_PPA") Then
					Dim Billet_Type As String = args.NameValuePairs("Billet_Type")
					Dim PPE_Type As String = args.NameValuePairs("PPE_Type")
					Dim Filter_Value As String = args.NameValuePairs("Filter_Value")
									
					'BRApi.ErrorLog.LogMessage(si, "Filter_Value: " & Filter_Value)
					If Billet_Type.XFEqualsIgnoreCase("Military")
						'If PPE_Type is not blank, return a valid list of values
						If PPE_Type <> ""
							If Filter_Value.XFEqualsIgnoreCase("PPA")
								Return "U1#OS.Base"
							Else If Filter_Value.XFEqualsIgnoreCase("ATU")
								Return "U4#Total_ATU.Children"
							End If
						Else 'PPE_Type is blank
							If Filter_Value.XFEqualsIgnoreCase("PPA")
								Return "U1#NA_PPA"
							Else If Filter_Value.XFEqualsIgnoreCase("ATU")
								Return "U4#NA_ATU"
							End If
						End If 'PPE_Type <> ""
					Else 'Civilian						
						If Filter_Value.XFEqualsIgnoreCase("PPA")
							Return "U1#NA_PPA"
						Else If Filter_Value.XFEqualsIgnoreCase("ATU")
							Return "U4#NA_ATU"
						End If
					End If
					
				End If


#End Region 'GetPPEType_ATU_PPA

#Region "GetSourceAttributes"
								
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetSourceAttributes, WFTime=2024, WFScenario=RPSeeding_FY24, WFCube=BudEx, WFText1=,RPName=|!prm_Number!|, LINumber=|!prm_BLT_LineItemNumber!|, Filter_Value = LineItem_Comment)
				If args.FunctionName.XFEqualsIgnoreCase("GetSourceAttributes") Then	
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")

					' If RP Name is empty, nothing to do 
					If RPName = "" Then
						Return Nothing
					End If
					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)					
					Dim LINumber As String = args.NameValuePairs("LINumber")
					Dim Filter_Value As String = args.NameValuePairs("Filter_Value")	
					
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#" & LINumber & ":U7#None:U8#None"
													
					'Get info Billet Attribute					
					Select Case Filter_Value
					Case "Billet_ATU","PPE_ATU","Lease_ATU","Utilities_ATU"

						Dim attributeValueDataAttachmentList As DataAttachmentList = BRApi.Finance.Data.GetDataAttachments(si, "A#" & Filter_Value & ":" & scriptGenerics, False)
						Dim attributeValue As String = String.Empty
						For Each attributeValueDataAttachment As DataAttachment In attributeValueDataAttachmentList.Items
							attributeValue = attributeValueDataAttachment.Text
						Next
						
						If attributeValue <> ""
							'remove the '_NoUnit' from the stored ATU Name
							Return attributeValue.Substring(0, attributeValue.Length - 7)
						End If
					
					Case Else

						Dim attributeValueDataAttachmentList As DataAttachmentList = BRApi.Finance.Data.GetDataAttachments(si, "A#" & Filter_Value & ":" & scriptGenerics, False)
						Dim attributeValue As String = String.Empty
						For Each attributeValueDataAttachment As DataAttachment In attributeValueDataAttachmentList.Items
							attributeValue = attributeValueDataAttachment.Text
						Next
							
						Return attributeValue
					
					End Select
					
				End If
				
#End Region 'GetSourceBilletAttributes
			
#Region "GetPpaAllocTableUD8"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetPpaAllocTableUD8, cubeName=BudEx, allocTableUD5=Pay_Military_Inp)
				If args.FunctionName.XFEqualsIgnoreCase("GetPpaAllocTableUD8") Then
					Dim cubeName As String = args.NameValuePairs("cubeName")
					Dim time As String = args.NameValuePairs("time")
					Dim scenario As String = args.NameValuePairs("scenario")
					Dim allocTableUD5 As String = args.NameValuePairs("allocTableUD5")
					Dim ppaAllocTableTops As String = ":E#NA:V#Annotation:A#None:F#None:O#Forms:I#None:U1#NO_PPA:U2#None:U3#None:U4#None:U6#None:U7#None:U8#None"
					
			'brapi.ErrorLog.LogMessage(si, "ppaAllocTypeString = U5#" & allocTableUD5 & ppaAllocTableTops)
					Dim ppaAllocTypeInfo As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, cubeName, "S#" & scenario & ":T#" & time & ":U5#" & allocTableUD5 & ppaAllocTableTops)
					Dim ppaAllocType As String = ppaAllocTypeInfo.DataCellEx.DataCellAnnotation
			'brapi.ErrorLog.LogMessage(si, "ppaAllocType = " & ppaAllocType)
					
					If ppaAllocType.XFEqualsIgnoreCase("1")
						Return "U8#None"
					Else 'must be 2 or 3 so return the annnotation helper so the user cannot edit the cells due to them being Manual or OPFAC
						Return "U8#Annotation_Helper"
							
					End If
					
				End If

				
#End Region 'GetPpaAllocTableUD8	
				
#Region "GetAtuAllocTableUD8"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetAtuAllocTableUD8, cubeName=BudEx, allocTableUD5=Pay_Military_Inp)
				If args.FunctionName.XFEqualsIgnoreCase("GetAtuAllocTableUD8") Then
					Dim cubeName As String = args.NameValuePairs("cubeName")
					Dim time As String = args.NameValuePairs("time")
					Dim scenario As String = args.NameValuePairs("scenario")
					Dim allocTableUD5 As String = args.NameValuePairs("allocTableUD5")
					Dim atuAllocTableTops As String = ":E#NA:V#Annotation:A#None:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#No_ATU:U6#None:U7#None:U8#None"
					
					'brapi.ErrorLog.LogMessage(si, "ppaAllocTypeString = A#" & allocTableAccount & ppaAllocTableTops)
					Dim AtuAllocTypeInfo As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, cubeName, "S#" & scenario & ":T#" & time & ":U5#" & allocTableUD5 & atuAllocTableTops)
					Dim AtuAllocType As String = AtuAllocTypeInfo.DataCellEx.DataCellAnnotation
					
					If AtuAllocType.XFEqualsIgnoreCase("1")
						Return "U8#None"
					Else 'must be 2 or 3 so return the annnotation helper so the user cannot edit the cells due to them being Manual or OPFAC
						Return "U8#Annotation_Helper"
							
					End If
					
				End If

				
#End Region 'GetAtuAllocTableUD8	

#Region "GetOcAllocTableUD8"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetOcAllocTableUD8, cubeName=BudEx, allocTableUD5=Pay_Military_Inp)
				If args.FunctionName.XFEqualsIgnoreCase("GetOcAllocTableUD8") Then
					Dim cubeName As String = args.NameValuePairs("cubeName")
					Dim time As String = args.NameValuePairs("time")
					Dim scenario As String = args.NameValuePairs("scenario")
					Dim allocTableUD5 As String = args.NameValuePairs("allocTableUD5")
					Dim OCAllocTableTops As String = ":E#NA:V#Annotation:A#None:F#None:O#Forms:I#None:U1#None:U2#None:U3#No_ObjectClass:U4#None:U6#None:U7#None:U8#None"
					
					Dim OCAllocTypeInfo As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, cubeName, "S#" & scenario & ":T#" & time & ":U5#" & allocTableUD5 & OCAllocTableTops)
					Dim OCAllocType As String = OCAllocTypeInfo.DataCellEx.DataCellAnnotation
					
					If OCAllocType.XFEqualsIgnoreCase("1")
						Return "U8#None"
					Else 'must be 2 or 3 so return the annnotation helper so the user cannot edit the cells due to them being Manual or OPFAC
						Return "U8#Annotation_Helper"
							
					End If
					
				End If

#End Region 'GetOcAllocTableUD8

#Region "GetUIIAllocTableUD8"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetUIIAllocTableUD8, cubeName=BudEx, allocTableUD5=Pay_Military_Inp)
				If args.FunctionName.XFEqualsIgnoreCase("GetUIIAllocTableUD8") Then
					Dim cubeName As String = args.NameValuePairs("cubeName")
					Dim time As String = args.NameValuePairs("time")
					Dim scenario As String = args.NameValuePairs("scenario")
					Dim allocTableUD5 As String = args.NameValuePairs("allocTableUD5")
					Dim UIIAllocTableTops As String = ":E#NA:V#Annotation:A#None:F#None:O#Forms:I#None:U1#None:U2#NoInvestment:U3#None:U4#None:U6#None:U7#None:U8#None"
					
					Dim UIIAllocTypeInfo As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, cubeName, "S#" & scenario & ":T#" & time & ":U5#" & allocTableUD5 & UIIAllocTableTops)
					Dim UIIAllocType As String = UIIAllocTypeInfo.DataCellEx.DataCellAnnotation
					
					If UIIAllocType.XFEqualsIgnoreCase("1")
						Return "U8#None"
					Else 'must be 2 or 3 so return the annnotation helper so the user cannot edit the cells due to them being Manual or OPFAC
						Return "U8#Annotation_Helper"
							
					End If
					
				End If

#End Region 'GetUIIAllocTableUD8

#Region "GetPpaAllocTableUD8_NBLT"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetPpaAllocTableUD8_NBLT, cubeName=BudEx, allocTableUD5=Pay_Military_Inp)
				If args.FunctionName.XFEqualsIgnoreCase("GetPpaAllocTableUD8_NBLT") Then
					Dim cubeName As String = args.NameValuePairs("cubeName")
					Dim time As String = args.NameValuePairs("time")
					Dim scenario As String = args.NameValuePairs("scenario")
					Dim allocTableUD5 As String = args.NameValuePairs("allocTableUD5")
					Dim ppaAllocTableTops As String = ":E#NA:V#Annotation:A#None:F#None:O#Forms:I#None:U1#NO_PPA:U2#None:U3#None:U4#None:U6#None:U7#None:U8#None"
					
					'brapi.ErrorLog.LogMessage(si, "ppaAllocTypeString = A#" & allocTableAccount & ppaAllocTableTops)
					Dim ppaAllocTypeInfo As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, cubeName, "S#" & scenario & ":T#" & time & ":U5#" & allocTableUD5 & ppaAllocTableTops)
					Dim ppaAllocType As String = ppaAllocTypeInfo.DataCellEx.DataCellAnnotation
					
					If ppaAllocType.XFEqualsIgnoreCase("1")
						Return "U8#None"
					Else 'must be 2 or 3 so return the annnotation helper so the user cannot edit the cells due to them being Manual or OPFAC
						Return "U8#Annotation_Helper"
							
					End If
					
				End If

				
#End Region 'GetPpaAllocTableUD8_NBLT	
				
#Region "GetAtuAllocTableUD8_NBLT"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetAtuAllocTableUD8_NBLT, cubeName=BudEx, allocTableUD5=Pay_Military_Inp)
				If args.FunctionName.XFEqualsIgnoreCase("GetAtuAllocTableUD8_NBLT") Then
					Dim cubeName As String = args.NameValuePairs("cubeName")
					Dim time As String = args.NameValuePairs("time")
					Dim scenario As String = args.NameValuePairs("scenario")
					Dim allocTableUD5 As String = args.NameValuePairs("allocTableUD5")
					Dim atuAllocTableTops As String = ":E#NA:V#Annotation:A#None:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#No_ATU:U6#None:U7#None:U8#None"
					
					'brapi.ErrorLog.LogMessage(si, "ppaAllocTypeString = A#" & allocTableAccount & ppaAllocTableTops)
					Dim AtuAllocTypeInfo As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, cubeName, "S#" & scenario & ":T#" & time & ":U5#" & allocTableUD5 & atuAllocTableTops)
					Dim AtuAllocType As String = AtuAllocTypeInfo.DataCellEx.DataCellAnnotation
					
					If AtuAllocType.XFEqualsIgnoreCase("1")
						Return "U8#None"
					Else 'must be 3 so return the annnotation helper so the user cannot edit the cells due to them being Manual or OPFAC
						Return "U8#Annotation_Helper"
							
					End If
					
				End If

				
#End Region 'GetAtuAllocTableUD8_NBLT

#Region "GetOcAllocTableUD8_NBLT"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetOcAllocTableUD8_NBLT, cubeName=BudEx, allocTableUD5=Pay_Military_Inp)
				If args.FunctionName.XFEqualsIgnoreCase("GetOcAllocTableUD8_NBLT") Then
					Dim cubeName As String = args.NameValuePairs("cubeName")
					Dim time As String = args.NameValuePairs("time")
					Dim scenario As String = args.NameValuePairs("scenario")
					Dim allocTableUD5 As String = args.NameValuePairs("allocTableUD5")
					Dim OCAllocTableTops As String = ":E#NA:V#Annotation:A#None:F#None:O#Forms:I#None:U1#None:U2#None:U3#No_ObjectClass:U4#None:U6#None:U7#None:U8#None"
					
					Dim OCAllocTypeInfo As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, cubeName, "S#" & scenario & ":T#" & time & ":U5#" & allocTableUD5 & OCAllocTableTops)
					Dim OCAllocType As String = OCAllocTypeInfo.DataCellEx.DataCellAnnotation
					
					If OCAllocType.XFEqualsIgnoreCase("1")
						Return "U8#None"
					Else 'must be 3 so return the annnotation helper so the user cannot edit the cells due to them being Manual or OPFAC
						Return "U8#Annotation_Helper"
							
					End If
					
				End If

#End Region 'GetOcAllocTableUD8_NBLT

#Region "GetUIIAllocTableUD8_NBLT"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetUIIAllocTableUD8_NBLT, cubeName=BudEx, allocTableUD5=1000_1)
				If args.FunctionName.XFEqualsIgnoreCase("GetUIIAllocTableUD8_NBLT") Then
					Dim cubeName As String = args.NameValuePairs("cubeName")
					Dim time As String = args.NameValuePairs("time")
					Dim scenario As String = args.NameValuePairs("scenario")
					Dim allocTableUD5 As String = args.NameValuePairs("allocTableUD5")
					Dim UIIAllocTableTops As String = ":E#NA:V#Annotation:A#None:F#None:O#Forms:I#None:U1#None:U2#NoInvestment:U3#None:U4#None:U6#None:U7#None:U8#None"
					
					Dim UIIAllocTypeInfo As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, cubeName, "S#" & scenario & ":T#" & time & ":U5#" & allocTableUD5 & UIIAllocTableTops)
					Dim UIIAllocType As String = UIIAllocTypeInfo.DataCellEx.DataCellAnnotation	
					If UIIAllocType.XFEqualsIgnoreCase("1")
						Return "U8#None"
					Else 'must be  3 so return the annnotation helper so the user cannot edit the cells due to them being Manual or OPFAC
						Return "U8#Annotation_Helper"
							
					End If
					
				End If

#End Region 'GetUIIAllocTableUD8_NBLT

#Region "GetAtuAllocTableUD8_NBLTDefault"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetAtuAllocTableUD8_NBLTDefault, cubeName=BudEx, allocTableUD5=Pay_Military_Inp)
				If args.FunctionName.XFEqualsIgnoreCase("GetAtuAllocTableUD8_NBLTDefault") Then
					Dim cubeName As String = args.NameValuePairs("cubeName")
					Dim time As String = args.NameValuePairs("time")
					Dim scenario As String = args.NameValuePairs("scenario")
					Dim allocTableUD5 As String = args.NameValuePairs("allocTableUD5")
					Dim atuAllocTableTops As String = ":E#NA:V#Annotation:A#None:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#No_ATU:U6#None:U7#None:U8#None"
					
					'brapi.ErrorLog.LogMessage(si, "ppaAllocTypeString = A#" & allocTableAccount & ppaAllocTableTops)
					Dim AtuAllocTypeInfo As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, cubeName, "S#" & scenario & ":T#" & time & ":U5#" & allocTableUD5 & atuAllocTableTops)
					Dim AtuAllocType As String = AtuAllocTypeInfo.DataCellEx.DataCellAnnotation
					
					If AtuAllocType.XFEqualsIgnoreCase("2")
						Return "U8#None"
					Else 'must be 3 so return the annnotation helper so the user cannot edit the cells due to them being Manual or OPFAC
						Return "U8#Annotation_Helper"
							
					End If
					
				End If

				
#End Region 'GetAtuAllocTableUD8_NBLTDefault

#Region "GetPpaAllocTableUD8_NBLTDefault"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetPpaAllocTableUD8_NBLTDefault, cubeName=BudEx, allocTableUD5=Pay_Military_Inp)
				If args.FunctionName.XFEqualsIgnoreCase("GetPpaAllocTableUD8_NBLTDefault") Then
					Dim cubeName As String = args.NameValuePairs("cubeName")
					Dim time As String = args.NameValuePairs("time")
					Dim scenario As String = args.NameValuePairs("scenario")
					Dim allocTableUD5 As String = args.NameValuePairs("allocTableUD5")
					Dim ppaAllocTableTops As String = ":E#NA:V#Annotation:A#None:F#None:O#Forms:I#None:U1#NO_PPA:U2#None:U3#None:U4#None:U6#None:U7#None:U8#None"
					
					Dim ppaAllocTypeInfo As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, cubeName, "S#" & scenario & ":T#" & time & ":U5#" & allocTableUD5 & ppaAllocTableTops)
					Dim ppaAllocType As String = ppaAllocTypeInfo.DataCellEx.DataCellAnnotation
					
					If ppaAllocType.XFEqualsIgnoreCase("2")
						Return "U8#None"
					Else 'must be 3 so return the annnotation helper so the user cannot edit the cells due to them being Manual
						Return "U8#Annotation_Helper"
							
					End If
					
				End If

				
#End Region 'GetPpaAllocTableUD8_NBLTDefault

#Region "GetUiiAllocTableUD8_NBLTDefault"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetUiiAllocTableUD8_NBLTDefault, cubeName=BudEx, allocTableUD5=Pay_Military_Inp)
				If args.FunctionName.XFEqualsIgnoreCase("GetUiiAllocTableUD8_NBLTDefault") Then
					Dim cubeName As String = args.NameValuePairs("cubeName")
					Dim time As String = args.NameValuePairs("time")
					Dim scenario As String = args.NameValuePairs("scenario")
					Dim allocTableUD5 As String = args.NameValuePairs("allocTableUD5")
					Dim uiiAllocTableTops As String = ":E#NA:V#Annotation:A#None:F#None:O#Forms:I#None:U1#None:U2#NoInvestment:U3#None:U4#None:U6#None:U7#None:U8#None"
					
					Dim uiiAllocTypeInfo As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, cubeName, "S#" & scenario & ":T#" & time & ":U5#" & allocTableUD5 & uiiAllocTableTops)
					Dim uiiAllocType As String = uiiAllocTypeInfo.DataCellEx.DataCellAnnotation
					
					If uiiAllocType.XFEqualsIgnoreCase("2")
						Return "U8#None"
					Else 'must be 3 so return the annnotation helper so the user cannot edit the cells due to them being Manual
						Return "U8#Annotation_Helper"
							
					End If
					
				End If

				
#End Region 'GetUiiAllocTableUD8_NBLTDefault

#Region "GetOcAllocTableUD8_NBLTDefault"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetOcAllocTableUD8_NBLTDefault, cubeName=BudEx, allocTableUD5=Pay_Military_Inp)
				If args.FunctionName.XFEqualsIgnoreCase("GetOcAllocTableUD8_NBLTDefault") Then
					Dim cubeName As String = args.NameValuePairs("cubeName")
					Dim time As String = args.NameValuePairs("time")
					Dim scenario As String = args.NameValuePairs("scenario")
					Dim allocTableUD5 As String = args.NameValuePairs("allocTableUD5")
					Dim ocAllocTableTops As String = ":E#NA:V#Annotation:A#None:F#None:O#Forms:I#None:U1#None:U2#None:U3#No_ObjectClass:U4#None:U6#None:U7#None:U8#None"
					
					Dim ocAllocTypeInfo As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, cubeName, "S#" & scenario & ":T#" & time & ":U5#" & allocTableUD5 & ocAllocTableTops)
					Dim ocAllocType As String = ocAllocTypeInfo.DataCellEx.DataCellAnnotation
					
					If ocAllocType.XFEqualsIgnoreCase("2")
						Return "U8#None"
					Else 'must be 3 so return the annnotation helper so the user cannot edit the cells due to them being Manual
						Return "U8#Annotation_Helper"
							
					End If
					
				End If

				
#End Region 'GetOcAllocTableUD8_NBLTDefault

#Region "Set Line Item Cost"
				
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, SetLineItemCost, WFTime=2024, WFScenario=RPSeeding_FY24, WFCube=BudEx, RPName=|!prm_Number!|, LINumber = |!prm_BLT_LineItemNumber!|)
				If args.FunctionName.XFEqualsIgnoreCase("SetLineItemCost") Then
				
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfYear As String = String.Empty
					If wfTime.Length = 4
						wfYear = wfTime.Substring(2,2)
					End If
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")		
									
					'Paremters							
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					'Declare the format string
					Dim FormatString As String = "$K #,##0,.000"
					'Declare the variable to return
					Dim Billet_CostLine As String = String.Empty
					Dim TextToReturn As String = "FY"& wfYear & " Billet Cost: "
					
					'Return the Line Item Total if the RPName is not blank
					If (Not RPName.Length < 1)
						Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)
						Dim LINumber As String = args.NameValuePairs.XFGetValue("LINumber")																																									
						Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Periodic:A#Funding:F#" & RPName & ":O#Top:I#Top:U1#Total_Appropriations:U2#Total_Investment:U3#Total_ObjectClass:U4#Total_ATU:U6#"& LINumber & ":U7#None:U8#None"
						
						'Get info for the total
						Dim Billet_CostLine_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "U5#Billet_CostLine:" & scriptGenerics)
						Billet_CostLine = TextToReturn & Billet_CostLine_Info.DataCellEx.DataCell.CellAmount.ToString(FormatString)
						
					Else 
						'Just return 0
						Billet_CostLine = TextToReturn & (0).ToString(FormatString)
						
					End If
					
					Return Billet_CostLine
										
				End If
				
#End Region 'Set Line Item Cost

#Region "Set RP Total Cost"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, SetRPTotalCost, WFTime=2024, WFScenario=RPSeeding_FY24, WFCube=BudEx, RPName=|!prm_Number!|)
				If args.FunctionName.XFEqualsIgnoreCase("SetRPTotalCost") Then
				
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfYear As String = String.Empty
					If wfTime.Length = 4
						wfYear = wfTime.Substring(2,2)
					End If
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")			
									
					'Paremters							
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					'Declare the format string
					Dim FormatString As String = "$K #,##0,.000"
					'Declare the variables to return
					Dim Total_CostLine As String = String.Empty
					Dim TextToReturn As String = "Total FY"& wfYear & " RP Cost: "
					
					
					'Return the Total if the RPName is not blank
					If (Not RPName.Length < 1)
						Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)																										
						Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Periodic:A#Funding:F#" & RPName & ":O#Top:I#Top:U1#Total_Appropriations:U2#Total_Investment:U3#Total_ObjectClass:U4#Total_ATU:U6#Top_UD6_LineItem:U7#None:U8#None"
						
						'Get info for the total
						Dim Total_CostLine_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "U5#Total_CostLine:" & scriptGenerics)
						Total_CostLine = TextToReturn & Total_CostLine_Info.DataCellEx.DataCell.CellAmount.ToString(FormatString)
					
					Else 
						'Just return 0
						Total_CostLine = TextToReturn & (0).ToString(FormatString)
						
					End If
						
					Return Total_CostLine
										
				End If
#End Region 'Set Total RP Cost

#Region"Set Non Billet Total"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, SetNBLTCost, WFTime=2024, WFScenario=RPSeeding_FY24, WFCube=BudEx, RPName=|!prm_Number!|, LINumber=|!prm_NBLT_LineItemNumber!|)
				If args.FunctionName.XFEqualsIgnoreCase("SetNBLTCost") Then
				
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfYear As String = String.Empty
					If wfTime.Length = 4
						wfYear = wfTime.Substring(2,2)
					End If
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")	
									
					'Paremters							
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					'Declare the format string
					Dim FormatString As String = "$K #,##0,.000"
					'Declare the variable to return
					Dim NBLT_Total As String = String.Empty
					Dim TextToReturn As String = "FY"& wfYear & " Item Cost: "
					
					'Return the Line Item Total if the RPName is not blank
					If (Not RPName.Length < 1)
						Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)
						
						'Get the number of O&M quarters to display in the text to return
						Dim OS_Qtrs As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#OS_Qtrs:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").DataCellEx.DataCellAnnotation
						If OS_Qtrs.XFEqualsIgnoreCase("1")
							TextToReturn  = "FY"& wfYear & " Item Cost (" & OS_Qtrs & " O&M Qtr): "
						Else 
							TextToReturn  = "FY"& wfYear & " Item Cost (" & OS_Qtrs & " O&M Qtrs): "
						End If
						
						Dim LINumber As String = args.NameValuePairs.XFGetValue("LINumber")	
						'Get info for the total
						'Edit:
						NBLT_Total = TextToReturn & BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Periodic:A#Funding:F#" & RPName & ":O#Top:I#Top:U1#Total_Appropriations:U2#Total_Investment:U3#Total_ObjectClass:U4#Total_ATU:U5#NonBillet_CostLine:U6#"& LINumber & ":U7#None:U8#None").DataCellEx.DataCell.CellAmount.ToString(FormatString)
												
					Else 
						'Just return 0
						NBLT_Total = TextToReturn & (0).ToString(FormatString)
						
					End If
												
						Return NBLT_Total						
										
				End If

#End Region'Set NonBillet Total

#Region"Set Expense Total"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, SetExpenseCost, WFTime=2024, WFScenario=RPSeeding_FY24, WFCube=BudEx, RPName=|!prm_Number!|, LINumber=|!prm_NBLT_LineItemNumber!|)
				If args.FunctionName.XFEqualsIgnoreCase("SetExpenseCost") Then
				
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfYear As String = String.Empty
					If wfTime.Length = 4
						wfYear = wfTime.Substring(2,2)
					End If
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")	
									
					'Paremters							
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					'Declare the format string
					Dim FormatString As String = "$K #,##0,.000"
					'Declare the variable to return
					Dim NBLT_Total As String = String.Empty
					Dim TextToReturn As String = "FY"& wfYear & " Expense Cost: "
					
					'Return the Line Item Total if the RPName is not blank
					If (Not RPName.Length < 1)
						Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)
						Dim LINumber As String = args.NameValuePairs.XFGetValue("LINumber")					
																																												
						Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Periodic:A#Funding:F#" & RPName & ":O#Top:I#Top:U1#Total_Appropriations:U2#Total_Investment:U3#Total_ObjectClass:U4#Total_ATU:U6#"& LINumber & ":U7#None:U8#None"
						
						'Get info for the total
						'Edit:
						Dim NBLT_Total_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "U5#NonBillet_CostLine:" & scriptGenerics)
						NBLT_Total = TextToReturn & NBLT_Total_Info.DataCellEx.DataCell.CellAmount.ToString(FormatString)
						
					Else 
						'Just return 0
						NBLT_Total = TextToReturn & (0).ToString(FormatString)
						
					End If
						
						
						Return NBLT_Total
						
										
				End If

#End Region'Set NonBillet Total

#Region"GetRPLineItems"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetRPLineItems, WFTime=2024, WFScenario=RPSeeding_FY24, WFCube=BudEx, WFText1=, RPName=|!prm_Number!|)
				If args.FunctionName.XFEqualsIgnoreCase("GetRPLineItems") Then
				
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
					Dim RP_Entity As String = args.NameValuePairs("WFText1")			
						
					'Paremters							
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
'					brapi.ErrorLog.LogMessage(si,wfTime & "wfTime" )
'					brapi.ErrorLog.LogMessage(si,wfScenario  & "wfScenario")
'					brapi.ErrorLog.LogMessage(si,wfCube  & "wfCube")
'					brapi.ErrorLog.LogMessage(si,RP_Entity  & "RP_Entity")
'					brapi.ErrorLog.LogMessage(si,RPName & "RPName" )
					
					'Dim POVCellValue As String  =XFGetCell(True, "BudFm", "LO_DCMS", "", "Local", "RAP_FY25", "2025", "Annotation", "Number_of_Billets", "RP_FY_2025_USCG_DCMS_OS_3_9999P_XXXXX_01", "Forms", "None", "None", "None", "None", "None", "None", "None", "None", "Annotation_Helper")
					
					'brapi.ErrorLog.LogMessage(si,POVCellValue & "POVCellValue" )
'					'Get info for the total
'					Dim NBLT_Total_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#NonBillet_CostLine:" & scriptGenerics)
'					Dim NBLT_Total As String = NBLT_Total_Info.DataCellEx.DataCell.CellAmount.ToString("$#,###.00")
					
'					Return NBLT_Total
										
				End If

#End Region'Set NonBillet Total

#Region "GetNonBilletATUList"

If args.FunctionName.XFEqualsIgnoreCase("GetNonBilletATUList") Then
    
    Dim wfCube As String = args.NameValuePairs("WFCube")
    Dim wfTime As String = args.NameValuePairs("WFTime")
    Dim wfScenario As String = args.NameValuePairs("WFScenario")
    Dim req_Item As String = args.NameValuePairs("Req_Item")
	Dim return_Type As String = args.NameValuePairs.GetValueOrEmpty("Return_Type")
    
    ' Get Scenario and Time IDs for the InUse check (As per your example)
    Dim scenarioMbr As Member = BRApi.Finance.Members.GetMember(si, DimTypeId.Scenario, wfScenario)
    Dim scenarioTypeId As Integer = BRApi.Finance.Scenario.GetScenarioType(si, scenarioMbr.MemberId).Id
    Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si, DimTypeId.Time, wfTime)
    
    Dim req_ItemNum As Integer = 0
    If (Not String.IsNullOrEmpty(req_Item)) Then
        Dim req_Item_Split As List(Of String) = StringHelper.SplitString(req_Item, "_")
        req_ItemNum = req_Item_Split(0).XFConvertToInt
    End If
    
    Dim costLine As String = args.NameValuePairs("CostLine")
	'If the req_ItemNum >=400 it is a user input cost line so refer to the cost line "req_ItemNum & 0_1" member for the allocation type
    If req_ItemNum >= 400 Then
        costLine = req_ItemNum & "0_1"
		'Do nothing and use the costLine parameter value
    End If

    'Get the Default Allocation Member(s) from the Allocation Table
    Dim atuAllocType_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "S#" & wfScenario & ":T#" & wfTime & ":E#NA:A#None:V#Annotation:O#Forms:I#None:F#None:U1#None:U2#None:U3#None:U4#No_ATU:U5#" & costLine & ":U6#None:U7#None:U8#None")
	Dim atuAllocType As String = atuAllocType_Info.DataCellEx.DataCellAnnotation
    
    Dim atuAllocDefaults_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "S#" & wfScenario & ":T#" & wfTime & ":E#NA:A#None:V#Assumptions:O#Forms:I#None:F#None:U1#None:U2#None:U3#None:U4#No_ATU:U5#" & costLine & ":U6#None:U7#None:U8#None")
    Dim atuAllocDefaults As String = atuAllocDefaults_Info.DataCellEx.DataCellAnnotation
    
    ' Define the Filter String
    Dim baseFilter As String = ""
    Dim exclusions As String = ".Remove(PCI,RD,RP,MERHCF,MOSP,BS,F,No_ATU)"
	
    'If before 2026, just load the U4#Total_ATU.Children
    If wfTime.XFConvertToInt < 2026 Then
        baseFilter = "U4#Total_ATU.Children" & exclusions
    Else
		'look up the allocation type for this costLine
        If atuAllocType.XFEqualsIgnoreCase("1") Then
            Return " "
        Else If atuAllocType.XFEqualsIgnoreCase("2") Then
            baseFilter = "U4#" & atuAllocDefaults & ",U4#Total_ATU.Children" & exclusions
        Else If atuAllocType.XFEqualsIgnoreCase("3") Then
            baseFilter = "U4#Total_ATU.Children" & exclusions
        Else
            Return " "
        End If
    End If
    
    ' Process the List based on In Use
    Dim potentialMbrs As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "Std_ATU"), baseFilter, Nothing)
    Dim finalItems As New List(Of String)
    'Dim leadStringList As String = ""
    
    If Not potentialMbrs Is Nothing Then
        For Each mbrInfo As MemberInfo In potentialMbrs
            Dim u4Id As Integer = mbrInfo.Member.MemberId
            Dim u4Inuse As Boolean = BRApi.Finance.UD.InUse(si, DimTypeId.UD4, u4Id, scenarioTypeId, wfTimeId)
            
            If u4Inuse Then
				If return_Type.XFEqualsIgnoreCase("Display_Items") Then
					finalItems.Add(mbrInfo.Member.Description)
				Else
					finalItems.Add(mbrInfo.Member.Name)
				End If
			End If
		Next
	End If
	Return String.Join(",", finalItems)

End If

#End Region

#Region "GetNonBilletPPAList"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetNonBilletPPAList,  WFCube=BudEx, WFTime=2024,  WFScenario=RPSeeding_FY24, Req_Item=|!prm_NBLT_RequestedItem_Tier1!|, CostLine=|!prm_NBLT_Description_Tier2!|, Ud4ATU=|!prm_NBLT_ATU!|)
				If args.FunctionName.XFEqualsIgnoreCase("GetNonBilletPPAList") Then
					
					Dim wfCube As String = args.NameValuePairs("WFCube")
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim req_Item As String = args.NameValuePairs("Req_Item")
					Dim req_ItemNum As Integer
					If (Not req_Item = "")
						Dim req_Item_Split As List(Of String) = StringHelper.SplitString(req_Item, "_")
						req_ItemNum = req_Item_Split(0).XFConvertToInt
					End If
					Dim costLine As String = args.NameValuePairs("CostLine")
					'Get the AllocType from the Allocation Table
					'If the req_ItemNum >=400 it is a user input cost line so refer to the cost line "req_ItemNum & 0_1" member for the allocation type
					If req_ItemNum >=400
						costLine = req_ItemNum & "0_1"
					Else
							'Do nothing and use the costLine parameter value
					End If
					Dim ud4ATU As String = args.NameValuePairs("Ud4ATU")
					Dim ud4ATUId As Integer = BRApi.Finance.Members.GetMemberId(si, dimtypeid.UD4, ud4ATU)
					'Get the AllocType from the Allocation Table
					Dim ppaAllocType_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "S#" & wfScenario & ":T#" & wfTime & ":E#NA:A#None:V#Annotation:O#Forms:I#None:F#None:U1#NO_PPA:U2#None:U3#None:U4#None:U5#" & costLine & ":U6#None:U7#None:U8#None")
					Dim ppaAllocType As String = ppaAllocType_Info.DataCellEx.DataCellAnnotation	
					
					'Get the Default Allocation Member(s) from the UD4 ATU Text 1 field
					Dim ppaMemList As New List (Of String)
					Dim ud4Text1 As String = BRApi.Finance.UD.Text(si, dimTypeId.ud4, ud4ATUId, 1, DimConstants.Unknown, DimConstants.Unknown)
					Dim ud4Text1Split() As String = ud4Text1.Split(",")
					For Each ud4Mem As String In ud4Text1Split
						'replace blanks
						ppaMemList.Add("U1#" & ud4Mem.Replace(" ", ""))
					Next 			
					Dim ppaAllocDefaults As String = String.Join(",", ppaMemList)
					
					'If before 2026, just load the U1#Total_PPA.Base.Remove(NO_PPA)
					If wfTime <2026
						Return "U1#Total_Appropriations.Base.Remove(NO_PPA)"
					Else
						'Do nothing and look up the allocation type
					End If
														
					'look up the allocation type for this costLine and return the appropriate list
					If ppaAllocType.XFEqualsIgnoreCase("1")
						Return " "
					Else If ppaAllocType.XFEqualsIgnoreCase("2")
						Return ppaAllocDefaults
					Else If ppaAllocType.XFEqualsIgnoreCase("3")
						Return ppaAllocDefaults
					Else
						Return "U1#Total_Appropriations.Base.Remove(NO_PPA)" ' Note: MSN updated this from U1#Total_PPA to U1#Total_Appropriations on 10/02/2023
					End If
					
					
					
				End If

				
#End Region 'GetNonBilletPPAList

#Region "GetNonBilletUIIList"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetNonBilletUIIList,  WFCube=BudEx, WFTime=2024, WFScenario=RPSeeding_FY24, Req_Item=|!prm_NBLT_RequestedItem_Tier1!|, CostLine=|!prm_NBLT_Description_Tier2!|, Ud1PPA=|!prm_NBLT_PPA!|)
				If args.FunctionName.XFEqualsIgnoreCase("GetNonBilletUIIList") Then
					
					Dim wfCube As String = args.NameValuePairs("WFCube")
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim req_Item As String = args.NameValuePairs("Req_Item")
					Dim req_ItemNum As Integer
					If (Not req_Item = "")
						Dim req_Item_Split As List(Of String) = StringHelper.SplitString(req_Item, "_")
						req_ItemNum = req_Item_Split(0).XFConvertToInt
					End If
					Dim costLine As String = args.NameValuePairs("CostLine")
					'Get the AllocType from the Allocation Table
					'If the req_ItemNum >=400 it is a user input cost line so refer to the cost line "req_ItemNum & 0_1" member for the allocation type
					If req_ItemNum >=400
						costLine = req_ItemNum & "0_1"
					Else
							'Do nothing and use the costLine parameter value
					End If
					Dim ud1PPA As String = args.NameValuePairs("Ud1PPA")
					Dim ud1PPAId As Integer = BRApi.Finance.Members.GetMemberId(si, dimtypeid.UD1, ud1PPA)
					'Get the AllocType from the Allocation Table
					Dim uiiAllocType_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "S#" & wfScenario & ":T#" & wfTime & ":E#NA:A#None:V#Annotation:O#Forms:I#None:F#None:U1#None:U2#NoInvestment:U3#None:U4#None:U5#" & costLine & ":U6#None:U7#None:U8#None")
					Dim uiiAllocType As String = uiiAllocType_Info.DataCellEx.DataCellAnnotation		
			
					'Declare a new list for the uii Allocation Defaults to return
					Dim uiiAllocText1PPAs As New List (Of String)
					
					'Get the List of UD2 Members
					Dim objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "Std_Investment")
					Dim uiiMemList As List (Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U2#CostEstimate_Investments.Base.Remove(024_000006372_PPL)", True)
					
					For Each ud2Mem As MemberInfo In uiiMemList
						'Get the Text Field1 with the PPA Members
						Dim ud2Text1 As String = BRApi.Finance.UD.Text(si, dimTypeId.UD2, ud2Mem.Member.MemberId, 1, DimConstants.Unknown, DimConstants.Unknown)
						
						Dim ud2Text1Split() As String = ud2Text1.Split(",")
						For Each ud1Mem As String In ud2Text1Split
							If ud1Mem.Replace(" ", "") = ud1PPA
								'Add the uii to the list if it contains the selected PPA in the Text1 Field
								uiiAllocText1PPAs.Add("U2#" & ud2Mem.Member.Name)
							End If
						Next
					Next 			
					
					Dim uiiAllocDefaults As String = String.Join(",", uiiAllocText1PPAs)	
					
					'If before 2026, just load the U2#CostEstimate_Investments.Base
					If wfTime <2026
						Return "U2#CostEstimate_Investments.Base"
					Else
						'Do nothing and look up the allocation type
					End If
					
					'look up the allocation type for this costLine and return the appropriate list
					If uiiAllocType.XFEqualsIgnoreCase("1")
						Return " "
					Else If uiiAllocType.XFEqualsIgnoreCase("2")
						Return uiiAllocDefaults
					Else If uiiAllocType.XFEqualsIgnoreCase("3")
						Return uiiAllocDefaults
					Else						
						Return "U2#CostEstimate_Investments.Base"
					End If
					
					
					
				End If

				
#End Region 'GetNonBilletUIIList

#Region "GetNonBilletOCList"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetNonBilletOCList,  WFCube=BudEx, WFTime=2024, WFScenario=RPSeeding_FY24, Req_Item=|!prm_NBLT_RequestedItem_Tier1!|, CostLine=|!prm_NBLT_Description_Tier2!|)
				If args.FunctionName.XFEqualsIgnoreCase("GetNonBilletOCList") Then
					Dim wfCube As String = args.NameValuePairs("WFCube")
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim req_Item As String = args.NameValuePairs("Req_Item")
					Dim req_ItemNum As Integer
					If (Not req_Item = "")
						Dim req_Item_Split As List(Of String) = StringHelper.SplitString(req_Item, "_")
						req_ItemNum = req_Item_Split(0).XFConvertToInt
					End If
					Dim costLine As String = args.NameValuePairs("CostLine")
					'Get the AllocType from the Allocation Table
					'If the req_ItemNum >=400 it is a user input cost line so refer to the cost line "req_ItemNum & 0_1" member for the allocation type
					If req_ItemNum >=400
						costLine = req_ItemNum & "0_1"
					Else
							'Do nothing and use the costLine parameter value
					End If
					
					
					Dim ocAllocType_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "S#" & wfScenario & ":T#" & wfTime & ":E#NA:A#None:V#Annotation:O#Forms:I#None:F#None:U1#None:U2#None:U3#No_ObjectClass:U4#None:U5#" & costLine & ":U6#None:U7#None:U8#None")
					Dim ocAllocType As String = ocAllocType_Info.DataCellEx.DataCellAnnotation			
					
					'Get the Default Allocation Member(s) from the Allocation Table
					Dim ocAllocDefaults_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "S#" & wfScenario & ":T#" & wfTime & ":E#NA:A#None:V#Assumptions:O#Forms:I#None:F#None:U1#None:U2#None:U3#No_ObjectClass:U4#None:U5#" & costLine & ":U6#None:U7#None:U8#None")
					Dim ocAllocDefaults As String = ocAllocDefaults_Info.DataCellEx.DataCellAnnotation
					
'					'Check to see if my first characters are 999 because they are historical cost estimate items used prior to FY26, if so return the integer that is line item specific
                    If req_ItemNum >=9990
						ocAllocType = "3"
					Else
					
						'look up the allocation type for this costLine
						If ocAllocType.XFEqualsIgnoreCase("1")
							Return " "
						Else If ocAllocType.XFEqualsIgnoreCase("2")			
							Return "U3#" & ocAllocDefaults
						Else If ocAllocType.XFEqualsIgnoreCase("3")
							Return "U3#Total_ObjectClass.Base.Remove(No_ObjectClass)"
						Else			
							Return " "
						End If
			         End If
					
				   End If

				
#End Region 'GetNonBilletATUList

#Region "GetRPStatus"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetRPStatus, RPName=|!prm_Number!|)
				If args.FunctionName.XFEqualsIgnoreCase("GetRPStatus") Then
					
					Dim rpName As String = args.NameValuePairs("RPName")		
					If (Not rpName = "")
						Return rpUtils.Get_RP_Status_Description(si, rpName) 
					Else 
						Return ""
					End If 'RPName = ""
					
				End If
#End Region 

#Region "GetRPMode"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetRPMode, RPName=|!prm_Number!|, FilterValue=ModeDescription)
				If args.FunctionName.XFEqualsIgnoreCase("GetRPMode") Then
					
					Dim rpName As String = args.NameValuePairs("RPName")		
					Dim filterValue As String = args.NameValuePairs("FilterValue")
					If (Not rpName = "")
						Dim ModeDescription As String = rpUtils.Get_RP_Mode_Description(si, rpName) 
'							BRApi.Finance.Members.GetMember(si, dimtypeId.UD8, rpUtils.Get_RP_Mode(si, rpName)).Description
						Dim modeColor As String = String.Empty
						
						If rpUtils.Is_RP_Editable(si, rpName) Then
							modeColor = XFColors.Green.Name
						Else
							modeColor = XFColors.Red.Name
						End If
						
						If filterValue.XFEqualsIgnoreCase("ModeDescription") Then 
							Return modeDescription
						Else If filterValue.XFEqualsIgnoreCase("ModeColor") Then 
							Return  modeColor
						Else
							Return ""
						End If	
					Else 
						Return ""
					End If 'RPName = ""
					
				End If
#End Region 

#Region "GetUserInGroupForConcRev"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetUserInGroupForConcRev)
				If args.FunctionName.XFEqualsIgnoreCase("GetUserInGroupForConcRev") Then
					'Replace this with the name of the dashboard parameter containing the literal value of the security group that we want to see this
					Dim securityGroupName1 As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Security_")
					Dim securityGroupName2 As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Security_")
					If BRApi.Security.Authorization.IsUserInGroup(si, securityGroupName1) Or BRApi.Security.Authorization.IsUserInGroup(si, securityGroupName2) Then
						Return "True"
					Else
						Return "False"
					End If
				End If 'FunctionName				
					

#End Region

#Region "GetUserInGroupForRPAdmin"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetUserInGroupForRPAdmin)
				If args.FunctionName.XFEqualsIgnoreCase("GetUserInGroupForRPAdmin") Then
					'Replace this with the name of the dashboard parameter containing the literal value of the security group that we want to see this
					Dim securityGroupName1 As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Security_")
					If BRApi.Security.Authorization.IsUserInGroup(si, securityGroupName1) Then
						Return "True"
					Else
						Return "False"
					End If
				End If 'FunctionName					
					

#End Region

#Region "GetUserInGroupForWorkingVersion"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetUserInGroupForWorkingVersion)
				If args.FunctionName.XFEqualsIgnoreCase("GetUserInGroupForWorkingVersion") Then
					'Replace this with the name of the dashboard parameter containing the literal value of the security group that we want to see this
					Dim securityGroupName1 As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Security_BudFm_r_OfficeUserWV")
					If BRApi.Security.Authorization.IsUserInGroup(si, securityGroupName1) Then
						Return "True"
					Else
						Return "False"
					End If
				End If 'FunctionName				
					

#End Region


'Total_Directorate
'Report RP Scenario Progress 
'cbx_Reporting_RPScenarioStatus_LeadDirectorate 
'prm_LeadDirectorate_Reporting


#Region "GetInUseForDirectorate"
'			    XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetInUseForDirectorate, WFTime=|WFTime|)
'				prm_LeadDirectorate_Reporting
'				cbx_Reporting_RPScenarioStatus_LeadDirectorate
				'RP Scenario Progress report

				
				If args.FunctionName.XFEqualsIgnoreCase("GetInUseForDirectorate") Then
					
				     Dim budfm_Ud8ItemsDimPk As DimPk =  BRApi.Finance.Dim.GetDimPk(si,"Std_Reporting")
					 Dim Total_Directorate_List As Integer = BRApi.Finance.Members.GetMemberId(si,DimTypeId.UD8, "Total_Directorate")
					 
					Dim total_lead_Directorate As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si,budfm_Ud8ItemsDimPk, Total_Directorate_List, Nothing)
                  
					'Dim totalEntityLead As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si,budfm_EntityItemsDimPk, Total_LeadOffice, Nothing)
                    Dim scenarioKey As Integer = si.WorkflowClusterPk.ScenarioKey	
					Dim ScenarioName As String = BRApi.Finance.Members.GetMemberName(si, "2", scenarioKey)
					Dim LeadList As New List(Of String) 
					Dim leadStringList As String = ""
					Dim wfTime As String = args.NameValuePairs("WFTime")
					
					Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,dimtypeid.Time, wfTime)
					
					Dim objScenarioType As ScenarioType = BRApi.Finance.Scenario.GetScenarioType(si, scenarioKey)
		            Dim U8Inuse As Boolean = False
					
				   
					 For Each U8Lead As Member In total_lead_Directorate
							Dim U8ID As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.UD8.Id, U8Lead.Name)
							U8Inuse = BRApi.Finance.UD.InUse(si,dimTypeId.UD8,U8ID ,objScenarioType.Id, wfTimeId )	
						    Dim leadName As String = U8Lead.Description
							 
							If U8Inuse Then
								LeadList.Add(U8Lead.Description)
								'BRAPI.ErrorLog.LogMessage(si, leadName)
								' BRAPI.ErrorLog.LogMessage(si, "InUse " & U8Inuse.ToString & " " & U8Lead.ToString)
							  U8Inuse = False
							End If
					 Next
					LeadList.Remove("CG9")
				  
					For Each Ud8L As String  In LeadList
					  If leadStringList = "" Then
						  leadStringList = Ud8L
					  Else 
						 leadStringList = leadStringList & "," & Ud8L
					  End If
					  
					Next 	
			
				
					Return  leadStringList
				
		End If 			
					

#End Region

'Total_LeadOffice U8
' report Concurrent Clearance Matrix 
'cbx_Reporting_RPLeadOfficeSelection
'prm_RPLeadOfficeSelector

#Region "GetInUseForLeads"
'				Concurrent Clearance Matrix cbx_Reporting_RPLeadOfficeSelection prm_RPLeadOfficeSelector
'				XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetInUseForLeads, WFTime=|WFTime|, View=[Display_Items/Value_Items])

				
				If args.FunctionName.XFEqualsIgnoreCase("GetInUseForLeads") Then
					
				     Dim budfm_Ud8ItemsDimPk As DimPk =  BRApi.Finance.Dim.GetDimPk(si,"Std_Reporting")
					 Dim Total_Directorate_List As Integer = BRApi.Finance.Members.GetMemberId(si,DimTypeId.UD8, "Total_LeadOffice")
					 
					Dim total_lead_Directorate As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si,budfm_Ud8ItemsDimPk, Total_Directorate_List, Nothing)
                  
                    Dim scenarioKey As Integer = si.WorkflowClusterPk.ScenarioKey	
					Dim ScenarioName As String = BRApi.Finance.Members.GetMemberName(si, "2", scenarioKey)
					Dim LeadList As New List(Of String) 
					Dim leadStringList As String = ""
					Dim leadStringListDisplay As String = ""
					Dim wfTime As String = args.NameValuePairs("WFTime")
					
					Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,dimtypeid.Time, wfTime)
					
					Dim objScenarioType As ScenarioType = BRApi.Finance.Scenario.GetScenarioType(si, scenarioKey)
		            Dim U8Inuse As Boolean = False
					
				
					 For Each U8Lead As Member In total_lead_Directorate
							Dim U8ID As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.UD8.Id, U8Lead.Name)
							U8Inuse = BRApi.Finance.UD.InUse(si,dimTypeId.UD8,U8ID ,objScenarioType.Id, wfTimeId )	
						    Dim leadName As String = U8Lead.Description
							 
							If U8Inuse Then
								LeadList.Add(U8Lead.Name)
							    U8Inuse = False
							End If
					 Next
					LeadList.Remove("CG9")
				
					For Each Ud8L As String In LeadList
					  If leadStringList = "" Then
						  leadStringList = Ud8L
						  leadStringListDisplay = Ud8L.Replace("_", "-") 'subbing out the underscores for dashes for the display items
					  Else 
						 leadStringList = leadStringList & "," & Ud8L
						 leadStringListDisplay = leadStringListDisplay & "," & Ud8L.Replace("_", "-")
					  End If
					  
					Next 	
					
					Dim viewOption As String = args.NameValuePairs("View")
					
					If viewOption.XFEqualsIgnoreCase("Display_Items") Then
						Return leadStringListDisplay
					Else
						Return leadStringList
					End If
					
					
		End If 			
					

#End Region


			
#Region "GetScenarioReadDataGroup"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetScenarioReadDataGroup, wfYear=|WFYear|, Filter=BYScen)
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetScenarioReadDataGroup, wfYear=|WFYear|, Filter=BYScenMinusTwo)
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetScenarioReadDataGroup, wfYear=|WFYear|, Filter=BYScenMinusOne)
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetScenarioReadDataGroup, wfYear=|WFYear|, Filter=BYScenPlusOne)
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetScenarioReadDataGroup, wfYear=|WFYear|, Filter=BYScenPlusTwo)
				If args.FunctionName.XFEqualsIgnoreCase("GetScenarioReadDataGroup") Then
					
					Dim wfYear = args.NameValuePairs("WFYear")
					Dim filter = args.NameValuePairs("Filter")
					
					'Get the Current Working Scenario
					Dim currscenYear As String = String.empty
					If Filter = "BYScen" 
						currscenYear = wfYear.Substring(2)
					ElseIf Filter = "BYScenMinusTwo"
						currscenYear = (wfYear.Substring(2).XFConvertToInt - 2).ToString
					ElseIf Filter = "BYScenMinusOne"
						currscenYear = (wfYear.Substring(2).XFConvertToInt - 1).ToString
					ElseIf Filter = "BYScenPlusOne"
						currscenYear = (wfYear.Substring(2).XFConvertToInt + 1).ToString
					ElseIf Filter = "BYScenPlusTwo"
						currscenYear = (wfYear.Substring(2).XFConvertToInt + 2).ToString
					End If
						
					'Get the Read Data Group Unique ID from the Working Scenario	
					Dim currscen As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "WorkScen_FY" & currScenYear)
					Dim guidReadData As Guid = BRApi.Finance.Members.GetMember(si, 2, currscen).ReadDataGroupUniqueID
							
					'Define roles from parameters
					Dim grpAllUsers As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Security_BudFm_s_AllUsers")
					Dim grpOfficeUsers As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Security_BudFm_s_OfficeandPowerUsers")
					Dim grpPowerUsers As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Security_BudFm_s_PowerUsers")
					Dim grpAllUsersincEX As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Security_BudFm_s_AllUsers_InclExecution")
					
					'Get the Unique ID from the All Users, OfficeandPowerUsers, and PowerUsers scenario security groups
					Dim guidAllUsers As Guid = BRApi.Security.Admin.GetGroup(si, grpAllUsers).Group.UniqueID
					Dim guidAllUsersincEX As Guid = BRApi.Security.Admin.GetGroup(si, grpAllUsersincEX).Group.UniqueID
					Dim guidOfficeUsers As Guid = BRApi.Security.Admin.GetGroup(si, grpOfficeUsers).Group.UniqueID
					Dim guidPowerUsers As Guid = BRApi.Security.Admin.GetGroup(si, grpPowerUsers).Group.UniqueID
					
					'Compare the Read Data Group Unique ID of the Scenario to the UniqueID of the All Users, OfficeandPowerUsers, and PowerUsers scenario security groups and return a value
					If guidReadData = guidAllUsers Then						
						Return "All Users"
						
					Else If guidReadData = guidOfficeUsers Then						
						Return "Office Users and Power Users"
						
					Else If guidReadData = guidPowerUsers Then						
						Return "Power Users Only"
					
					Else If guidReadData = guidAllUsersincEX Then
						Return "All Users"
						
					Else						
						Return "Needs Defined"
					End If	
					
				End If

#End Region

#Region "GetScenarioWriteDataGroup"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetScenarioWriteDataGroup, wfYear=|WFYear|, Filter=BYScen)
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetScenarioWriteDataGroup, wfYear=|WFYear|, Filter=BYScenMinusTwo)
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetScenarioWriteDataGroup, wfYear=|WFYear|, Filter=BYScenMinusOne)
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetScenarioWriteDataGroup, wfYear=|WFYear|, Filter=BYScenPlusOne)
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetScenarioWriteDataGroup, wfYear=|WFYear|, Filter=BYScenPlusTwo)
				If args.FunctionName.XFEqualsIgnoreCase("GetScenarioWriteDataGroup") Then
					
					Dim wfYear = args.NameValuePairs("WFYear")
					Dim filter = args.NameValuePairs("Filter")
					
					'Get the Current Working Scenario
					Dim currscenYear As String = String.empty
					If Filter = "BYScen" 
						currscenYear = wfYear.Substring(2).ToString
					ElseIf Filter = "BYScenMinusTwo"
						currscenYear = (wfYear.Substring(2).XFConvertToInt - 2).ToString
					ElseIf Filter = "BYScenMinusOne"
						currscenYear = (wfYear.Substring(2).XFConvertToInt - 1).ToString
					ElseIf Filter = "BYScenPlusOne"
						currscenYear = (wfYear.Substring(2).XFConvertToInt + 1).ToString
					ElseIf Filter = "BYScenPlusTwo"
						currscenYear = (wfYear.Substring(2).XFConvertToInt + 2).ToString
					End If
					
					'Get the Read/Write Data Group Unique ID from the workflow scenario		
					Dim currscen As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "WorkScen_FY" & currScenYear)
					Dim guidWriteData As Guid = BRApi.Finance.Members.GetMember(si, 2, currscen).ReadWriteDataGroupUniqueID
										
					'Define roles from parameters
					Dim grpOfficeUsers As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Security_BudFm_s_OfficeandPowerUsers")
					Dim grpPowerUsers As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Security_BudFm_s_PowerUsers")
					
					'Get the Unique ID from the OfficeandPowersUsers and PowerUsers scenario security groups
					Dim guidOfficeUsers As Guid = BRApi.Security.Admin.GetGroup(si, grpOfficeUsers).Group.UniqueID
					Dim guidPowerUsers As Guid = BRApi.Security.Admin.GetGroup(si, grpPowerUsers).Group.UniqueID
					
					'Compare the Read/Write Data Group Unique ID of the Scenario to the UniqueID of the OfficeandPowersUsers and PowerUsers scenario security groups and return a value
					If guidWriteData = guidOfficeUsers Then						
						Return "Office Users and Power Users"
						
					Else If guidWriteData = guidPowerUsers Then						
						Return "Power Users Only"
						
					Else						
						Return "Needs Defined"
						
					End If
					
				End If 'FunctionName					
					

#End Region

#Region "GetLeadOffice"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetLeadOffice, LeadOffice=|!prm_LeadOffice!|)
				If args.FunctionName.XFEqualsIgnoreCase("GetLeadOffice") Then
					Dim leadOffice As String = args.NameValuePairs("LeadOffice")	
					
					If leadOffice <> "" Then
						Dim length As Integer = leadOffice.Length
	'brapi.ErrorLog.LogMessage(si, leadOffice.Substring(3,length-3))					
						Return leadOffice.Substring(3,length-3)
					Else
						Return "None"
					End If
					
				End If

				
#End Region 'GetLeadOffice	

#Region "SearchRPListCV"

	If args.FunctionName.XFEqualsIgnoreCase("SearchRPListCV") Then
		
		'Get RP Hierarchy from current year RPs
		Dim strWFYear As String = args.NameValuePairs("currYear")	
		Dim MemberFilterScript As String = "F#FY" & strWFYear &"_RP.Base"
		Dim leadOffice As String = args.NameValuePairs("LeadOffice")
		Dim entity As String = String.Empty
		Dim searchQuery As String = args.NameValuePairs("SearchQuery")
		
		'Derive the Lead Office Entity
		If leadOffice <> "" Then
			Dim length As Integer = leadOffice.Length				
			entity = leadOffice.Substring(3,length-3)
		Else
			entity = ""
		End If
		
		If searchQuery = "" Then	
			MemberFilterScript = MemberFilterScript & ".Where(Name Contains " & entity & ")"
		
		Else			
			MemberFilterScript = MemberFilterScript & ".Where((Name Contains [|!prmRPSearchQuery!|]) Or (Description Contains [|!prmRPSearchQuery!|]))"
		
		End If
		
		Return MemberFilterScript
		
	End If

#End Region

#Region "SearchRPListCV2"

	If args.FunctionName.XFEqualsIgnoreCase("SearchRPListCV2") Then
		
		'Get RP Hierarchy from current year RPs
		Dim strWFYear As String = args.NameValuePairs("currYear")	
		Dim MemberFilterScript As String = "F#FY" & strWFYear.Substring(2,2) &"_RP.Base"
		Dim MemberFilterScript_WV As String = "F#FY" & strWFYear.Substring(2,2) &"_RP_WV.Base"
		Dim appropriation As String = args.NameValuePairs("Appropriation")
		Dim entity As String = String.Empty
		Dim searchQuery As String = args.NameValuePairs("SearchQuery")	
		
		If searchQuery = "" Then
			MemberFilterScript = MemberFilterScript & ".Where( Text8 Contains _" & appropriation & "_ )," & MemberFilterScript_WV & ".Where( Text8 Contains _" & appropriation & "_ ),"
		
		Else
			MemberFilterScript = MemberFilterScript & 
									".Where( " & 
									         " ( Text8 Contains _" & appropriation & "_ ) and " &
											 " (( Text8 Contains " & searchQuery & " ) Or (Description Contains " & searchQuery & " )) " &
									"      ) ," & 
									MemberFilterScript_WV & 
									".Where( " & 
									         " ( Text8 Contains _" & appropriation & "_ ) and " &
											 " (( Text8 Contains " & searchQuery & " ) Or (Description Contains " & searchQuery & " )) " &
									"      ) "
		
		End If
		
		Return MemberFilterScript
		
	End If

#End Region

#Region "SearchRPListCV3"

	If args.FunctionName.XFEqualsIgnoreCase("SearchRPListCV3") Then
		
		'Get RP Hierarchy from current year RPs
		Dim strWFYear As String = args.NameValuePairs("currYear")	
		Dim MemberFilterScript As String = "F#FY" & strWFYear.Substring(2,2) &"_RP.Base"
		'Dim MemberFilterScript_WV As String = "F#FY" & strWFYear.Substring(2,2) &"_RP_WV.Base"
		Dim appropriation As String = args.NameValuePairs("Appropriation")
		Dim entity As String = String.Empty
		Dim searchQuery As String = args.NameValuePairs("SearchQuery")	
		
		If searchQuery = "" Then
			MemberFilterScript = MemberFilterScript & ".Where( Text8 Contains _" & appropriation & "_ )," & ".Where( Text8 Contains _" & appropriation & "_ ),"
		
		Else
			MemberFilterScript = MemberFilterScript & 
									".Where( " & 
									         " ( Text8 Contains _" & appropriation & "_ ) and " &
											 " (( Text8 Contains " & searchQuery & " ) Or (Description Contains " & searchQuery & " )) " &
									"      ) "
		
		End If
		
		Return MemberFilterScript
		
	End If

#End Region

#Region "GetT2DescripInpVisibility"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetT2DescrInpVisibility, FilterValue=txb, RequestedItem=[|!prm_NBLT_RequestedItem_Tier1!|])
				If args.FunctionName.XFEqualsIgnoreCase("GetT2DescrInpVisibility") Then
					Dim FilterValue As String = args.NameValuePairs("FilterValue")	
					Dim ReqItem As String = args.NameValuePairs("RequestedItem")	
					
					'If ReqItem is filled out, parse the string and evaluate the T1 Account
					If (Not ReqItem = "") Then
						Dim ReqItemSplit As List(Of String) = StringHelper.SplitString(reqItem, "_")
						Dim ReqItemNum As Integer = ReqItemSplit(0).XFConvertToInt
						
						'Return visible if the T1 Item >= 400 as below this can use the canned descriptions
						If ReqItemNum >=400
							'If text box artifact, visibility = true
							If FilterValue = "txb"
								Return "True"
							ElseIf FilterValue = "cbx"
								Return "False"
							End If
						Else
							'If text box artifact, visibility = false
							If FilterValue = "txb"
								Return "False"
							ElseIf FilterValue = "cbx"
								Return "True"
							End If
						End If 'ReqItemNum >=500
						Else
							Return "False"
					End If 'Not ReqItem = ""
					
				End If

				
#End Region 	
				
#Region "GetT2Descrip"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetT2Descrip, WFCube=[BudEx], WFScenario=[RPSeeding_FY24], WFTime=[2024], WFText1=[], Descr=[|MFAccount|], RP=[|!prm_Number!|], LINum=[NBLineItem_01])
				If args.FunctionName.XFEqualsIgnoreCase("GetT2Descrip") Then
					Dim wfCube As String = args.NameValuePairs("WFCube")
					Dim wfScen As String = args.NameValuePairs("WFScenario")
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim entity As String = args.NameValuePairs("WFText1")					
					Dim descr As String = args.NameValuePairs("Descr")	
					Dim rp As String = args.NameValuePairs("RP")
					Dim lINum As String = args.NameValuePairs("LINum")
					Dim descrInput As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "E#" & entity & ":C#Local:S#" & wfScen & ":T#" & wfTime & ":V#Annotation:A#None:F#" & rp & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#" & descr & ":U6#" & lINum & ":U7#None:U8#None").DataCellEx.DataCellAnnotation
					
					Return descrInput
					
				End If
				
#End Region 	
		
#Region "GetSupportDocName"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetSupportDocName, KeyValue=[|!UniqueID!|])				
				If args.FunctionName.XFEqualsIgnoreCase("GetSupportDocName") Then
					Dim keyValue As String = args.NameValuePairs("KeyValue")

					If keyValue <> String.Empty Then

	                 'Create the data table to return
	                 Dim sql As New Text.StringBuilder
	                 sql.Append($"SELECT FileName FROM dbo.DataAttachment WHERE UniqueID = '" & keyValue & "'")

	                  'Return the specified field value
					  Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
			          Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp, sql.ToString, True)
	                              If dt.Rows.Count = 1 Then
	                                          Return dt.Rows(0)(0).ToString
	                              Else
	                                          Dim message As String = "GetFieldValueUsingKey failed: could not find KeyField (UniqueID) Value (" & keyValue & ") SQL = " & sql.ToString
	                                          'BRApi.ErrorLog.LogMessage(si, message)
	                                          Return String.Empty
	                              End If
	                  End Using
	                Else
	                            Return "No Selection"
	                End If
				End If

				
				
#End Region 	

#Region "GetCleanUserName"
		
			'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetCleanUserName)	
             If args.FunctionName.XFEqualsIgnoreCase("GetCleanUserName") Then
                          'Get the User Document Folder with the Clean Name (Consistent with Platform Folder Naming)
                          Dim allowPeriods As Boolean = True
                          Dim allowSpaces As Boolean = False
                          Return StringHelper.RemoveSystemCharacters(si.AuthToken.UserName, allowPeriods, allowSpaces)
			End If
						  
#End Region 	

#Region "GetCCComReqForSave"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetCCComReqForSave, RPName=|!prm_Number!|, Button=CCReq)
				If args.FunctionName.XFEqualsIgnoreCase("GetCCComReqForSave") Then	
					Dim rpName As String = args.NameValuePairs("RPName")					
					Dim button As String = args.NameValuePairs("Button")	
					'Replace this with the name of the security group that we want to see this
					
					If (Not rpName = "")
						If Not (rpUtils.Is_RP_CC_required(si, rpName))
							If button.XFEqualsIgnoreCase("CCNotReq") Then Return "True"
							If button.XFEqualsIgnoreCase("CCReq") Then Return "False"
						Else			
							If button.XFEqualsIgnoreCase("CCNotReq") Then Return "False"
							If button.XFEqualsIgnoreCase("CCReq") Then Return "True"
						End If 'rpText3
					End If 'Not rpName = ""
						If button.XFEqualsIgnoreCase("CCNotReq") Then Return "True"
						If button.XFEqualsIgnoreCase("CCReq") Then Return "False"
				End If 'FunctionName					
					

#End Region

#Region "GetModeforEdit"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetModeforEdit, RPName=|!prm_Number_OS!|, WFScenario=|WFScenario|, WFTime=|WFTime|)
				If args.FunctionName.XFEqualsIgnoreCase("GetModeforEdit") Then	
					
					Dim rpName As String = args.NameValuePairs("RPName")
					Dim WFScenario As String = args.NameValuePairs("WFScenario")
					Dim WFTime As String = args.NameValuePairs("WFTime")
					
					Dim rpID As Integer = BRApi.Finance.Members.GetMemberId(si, DimtypeID.Flow, rpName)
					Dim rpTimeID As Integer = BRApi.Finance.Members.GetMemberId(si, DimtypeID.Time, WFTime)
					Dim WorkScenarioId As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, WFScenario)
					Dim WorkScenarioTypeId As Integer = BRApi.Finance.Scenario.GetScenarioType(si, WorkScenarioId).Id
					
					Dim rpTextValue As String = BRApi.Finance.Flow.Text(si, rpID, 1 , WorkScenarioTypeId, rpTimeID)
					
					If Not rpTextValue = "" Then
					
						Dim rptextsplit() As String = rpTextValue.Split ("|")
								
						Dim EditStatus As String = rptextsplit(1)
					
						If EditStatus = "Mode_02"
					
							Return "False"
					
						Else
						
							Return "True"
							
						End If 
					
					Else
						  
						  Return "False"
						  
					  End If 	  
						

				End If 'FunctionName					
					

#End Region

#Region "GetTargetRAPYears"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetTargetRAPYears, TgtScenario=|!prm_BudFm_TargetScenario!|)
				If args.FunctionName.XFEqualsIgnoreCase("GetTargetRAPYears") Then
					Dim tgtScenario As String = args.NameValuePairs("TgtScenario")
					'Get the target to copy the source data to based on the target scenario name
					Dim tgtScenarioY1 As Integer = tgtScenario.Substring(6,2).XFConvertToInt -3
					Dim tgtScenarioY2 As Integer = tgtScenarioY1 +1
					Dim tgtScenarioY3 As Integer = tgtScenarioY1 +2
					Dim tgtScenarioY4 As Integer = tgtScenarioY1 +3
					Dim tgtScenarioY5 As Integer = tgtScenarioY1 +4
					Dim tgtScenarioY6 As Integer = tgtScenarioY1 +5
					Dim tgtScenarioY7 As Integer = tgtScenarioY1 +6
					Dim tgtScenarioY8 As Integer = tgtScenarioY1 +7
					
					'Return the appended target scenario time filters together
					Return "T#20" & tgtScenarioY1  & _
							",T#20" & tgtScenarioY2 & _
							",T#20" & tgtScenarioY3 & _
							",T#20" & tgtScenarioY4 & _
							",T#20" & tgtScenarioY5 & _
							",T#20" & tgtScenarioY6 & _
							",T#20" & tgtScenarioY7 & _
							",T#20" & tgtScenarioY8
					
				End If
#End Region
				
#Region "GetUseInflationFactor"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetUseInflationFactor, cubeName=BudEx, povYear=T#YearNext4(2024))
				If args.FunctionName.XFEqualsIgnoreCase("GetUseInflationFactor") Then					
				End If

				
#End Region 'GetUseInflationFactor

#Region "SetActiveButtonColor"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, SetActiveButtonColor, CurrDbdName=[04f_BDF_RP_Dashboard_Content_ConcReview], DbdSelected=[|!prm_Content!|])
				If args.FunctionName.XFEqualsIgnoreCase("SetActiveButtonColor") Then
					BRAPi.Errorlog.LogMessage(si,"Hit selected")
					'Get the button selected the user just clicked on and the name of the dashboard the corresponds to the button
					Dim currDbdName As String = args.NameValuePairs("CurrDbdName")
					Dim dbdSelected As String = args.NameValuePairs("DbdSelected")
					
					'If the current dashboard name = the dashboard selected, then return green, else return dark blue
					If currDbdName=dbdSelected Then
						BRAPi.Errorlog.LogMessage(si,"Hit selected")
						Return "#FF16CA94"
					Else 
						Return "XFMediumDarkBlueText"
					End If
					
				End If 'FunctionName					
					

#End Region

#Region "SetActiveButtonColorRow2"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, SetActiveButtonColorRow2, CurrDbdName=[04a1_BDF_RP_Dashboard_Content_CreateRP], DbdSelected=[|!prm_Content!|])
				If args.FunctionName.XFEqualsIgnoreCase("SetActiveButtonColorRow2") Then
					'Get the button selected the user just clicked on and the name of the dashboard the corresponds to the button
					Dim currDbdName As String = args.NameValuePairs("CurrDbdName")
					Dim dbdSelected As String = args.NameValuePairs("DbdSelected")
					
					'If the current dashboard name = the dashboard selected, then return green, else return dark blue
					If currDbdName=dbdSelected Then
						Return "#FF16CA94"
					Else 
						Return "XFMediumDarkGray"
					End If
					
				End If 'FunctionName					
					

#End Region


#Region "SetActiveButtonColorBillet"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, SetActiveButtonColorBillet, CurrDbdName=[04f_BDF_RP_Dashboard_Content_ConcReview], DbdSelected=[|!prm_Content!|])
				If args.FunctionName.XFEqualsIgnoreCase("SetActiveButtonColorBillet") Then
					'Get the button selected the user just clicked on and the name of the dashboard the corresponds to the button
					Dim currDbdName As String = args.NameValuePairs("CurrDbdName")
					Dim dbdSelected As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_AddEditBillets_NonEditRP_OS")
					
					'If the current dashboard name = the dashboard selected, then return green, else return dark blue
					If currDbdName=dbdSelected Then
						Return "#FF16CA94"
						
					Else 
						Return "XFMediumDarkBlueText"
					
					End If
					
				End If 'FunctionName					
					

#End Region

#Region "PwrUserUpdateCreateFunctions"

	If args.FunctionName.XFEqualsIgnoreCase("PwrUserUpdateCreateFunctions") Then
		
		If BRApi.Security.Authorization.IsUserInGroup(si, "USCG_FERBE_BudFm_r_PowerUser") Then
		         Return True
				  
	     Else 
				  Return False
				
	     End If 
		 
   End If
	 
#End Region



	
#Region "GetEconTableUD8"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetEconTableUD8, cubeName=BudEx, ColTime=2024)
				If args.FunctionName.XFEqualsIgnoreCase("GetEconTableUD8") Then
					Dim cubeName As String = args.NameValuePairs("cubeName")
					Dim colTime As String = args.NameValuePairs("ColTime")
					Dim scenario As String = args.NameValuePairs("scenario")
					Dim useInflFactorString As String = ":E#NA:V#Annotation:A#Std_FactorSet_UseInflationFactor:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
					
					Dim useInflFactor As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, cubeName, "S#" & scenario & ":T#" & colTime & useInflFactorString).DataCellEx.DataCellAnnotation
					
					'If the inflation factor is set to Yes, then allow the user to input inflation percentages and cost in these years, else don't allow input
					If useInflFactor.XFEqualsIgnoreCase("Y")
						Return "U8#None"
					Else If useInflFactor.XFEqualsIgnoreCase("N")
						Return "U8#Annotation_Helper"
							
					End If
					
				End If

				
#End Region 'GetEconTableUD8	
				
#Region "GetCostTableUD8"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetCostTableUD8, cubeName=BudEx, ColTime=2024)
				If args.FunctionName.XFEqualsIgnoreCase("GetCostTableUD8") Then
					Dim cubeName As String = args.NameValuePairs("cubeName")
					Dim colTime As String = args.NameValuePairs("ColTime")
					Dim scenario As String = args.NameValuePairs("scenario")
					Dim useInflFactorString As String = ":E#NA:V#Annotation:A#Std_FactorSet_UseInflationFactor:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
					
					Dim useInflFactorInfo As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, cubeName, "S#" & scenario & ":T#" & colTime & useInflFactorString)
					Dim useInflFactor As String = useInflFactorInfo.DataCellEx.DataCellAnnotation
					
					'If the inflation factor is set to No, then then allow user to input costs, else don't allow them to enter costs
					If useInflFactor.XFEqualsIgnoreCase("N")
						Return "U8#None"
					Else If useInflFactor.XFEqualsIgnoreCase("Y")
						Return "U8#Annotation_Helper"
							
					End If
					
				End If

				
#End Region 'GetCostTableUD8	

#Region "EvalRPAppropriation"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, EvalRPAppropriation,  RPName=[|!prm_Number!|], Appropriation = Text)
				If args.FunctionName.XFEqualsIgnoreCase("EvalRPAppropriation") Then

					Dim RPName As String = args.NameValuePairs("RPName")
					Dim Appr As String = args.NameValuePairs("Appropriation")
					
					' Check if the Appropriation passed in as parameters is the same as 
					' the one emebedded in RP Name.					
					If Not String.IsNullOrEmpty(RPName) Then 
						Dim ApprfromRPName = rpUtils.Get_RP_Appropriation(si,RPName)
						If String.CompareOrdinal(ApprfromRPName.ToString.ToUpper,Appr.ToString.ToUpper)=0  Then
							Return True
						End If
					End If

'					If Not String.IsNullOrEmpty(RPName) Then 
'						Dim rpNameSplit As List(Of String) = StringHelper.SplitString(rpName, "_")
'							If rpNameSplit.Count>= 4 AndAlso String.CompareOrdinal(rpNameSplit(4).ToString.ToUpper,Appr.ToString.ToUpper)=0  Then 			
'								Return True
'							End If
'					End If		
					
					Return False
					
				End If
			
#End Region 'EvalRPAppropriation	
				
#Region "GetRPEntity"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetRPEntity, RPName=|!prm_Number_BSF!|)
				If args.FunctionName.XFEqualsIgnoreCase("GetRPEntity") Then
					Dim RPName As String = args.NameValuePairs("RPName")
					If RPName <> ""
						Return rpUtils.Get_RP_Entity(si,RPName)
					Else
						Return Nothing
					End If
					
					
				End If

				
#End Region 'GetRPEntity

#Region "GetYearAndQtrList_PCI"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetYearAndQtrList_PCI, WFTime=|WFTime|, Filter=PY)
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetYearAndQtrList_PCI, WFTime=|WFTime|, Filter=CY)
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetYearAndQtrList_PCI, WFTime=|WFTime|, Filter=BY)
				If args.FunctionName.XFEqualsIgnoreCase("GetYearAndQtrList_PCI") Then
					Dim WFYear As String = args.NameValuePairs("WFTime")
					Dim filter As String = args.NameValuePairs("Filter")
					'Convert string into integer
					Dim wfYear_Int As Integer = Convert.ToInt32(WFYear)
					'Return all four quarters of next five years
					Dim return_String As String = String.Empty
						Select Case filter
						Case = "PY"							
							For future_Year As Integer = wfYear_Int-22 To wfYear_Int+15
								For future_Qtr As Integer = 1 To 4
									return_String = return_String & "FY " & future_Year.ToString & " Q" & future_Qtr & ","
								Next
							Next
						Case = "CY"					
							For future_Year As Integer = wfYear_Int-22 To wfYear_Int+15
								For future_Qtr As Integer = 1 To 4
									return_String = return_String & "FY " & future_Year.ToString & " Q" & future_Qtr & ","
								Next
							Next
						Case = "BY"			
							For future_Year As Integer = wfYear_Int-22 To wfYear_Int+15
								For future_Qtr As Integer = 1 To 4
									return_String = return_String & "FY " & future_Year.ToString & " Q" & future_Qtr & ","
								Next
							Next
						End Select
					
					Return return_String
					
				End If

				
#End Region 'GetYearAndQtrList_PCI

#Region "GetYearAndQtrList_RD"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetYearAndQtrList_RD, WFTime=|WFTime|, Filter=PY)
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetYearAndQtrList_RD, WFTime=|WFTime|, Filter=CY)
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetYearAndQtrList_RD, WFTime=|WFTime|, Filter=BY)
				If args.FunctionName.XFEqualsIgnoreCase("GetYearAndQtrList_RD") Then
					Dim WFYear As String = args.NameValuePairs("WFTime")
					Dim filter As String = args.NameValuePairs("Filter")
					'Convert string into integer
					Dim wfYear_Int As Integer = Convert.ToInt32(WFYear)
					'Return all four quarters of next five years
					Dim return_String As String = String.Empty
						Select Case filter
						Case = "PY"							
							For future_Year As Integer = wfYear_Int-15 To wfYear_Int+15
								For future_Qtr As Integer = 1 To 4
									return_String = return_String & "FY " & future_Year.ToString & " Q" & future_Qtr & ","
								Next
							Next
						Case = "CY"					
							For future_Year As Integer = wfYear_Int-15 To wfYear_Int+15
								For future_Qtr As Integer = 1 To 4
									return_String = return_String & "FY " & future_Year.ToString & " Q" & future_Qtr & ","
								Next
							Next
						Case = "BY"			
							For future_Year As Integer = wfYear_Int-15 To wfYear_Int+15
								For future_Qtr As Integer = 1 To 4
									return_String = return_String & "FY " & future_Year.ToString & " Q" & future_Qtr & ","
								Next
							Next
						End Select
					
					Return return_String
					
				End If

				
#End Region 'GetYearAndQtrList_PCI

#Region "Get PPA List PCI"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetRPEntity, RPName=|!prm_Number_BSF!|)
				If args.FunctionName.XFEqualsIgnoreCase("GetPPAList_PCI") Then
					Dim ppa_Level2 As String = args.NameValuePairs("PPA_Level2")
					Dim return_String As String = String.Empty
					
					'BRApi.ErrorLog.LogMessage(si, "PPA Level 2 :" & ppa_Level2)
					
					If ppa_Level2 = "PCI_VES_ISVS"
						return_String = "U1#PCI_VES_ISVS.Children"
					Else If ppa_Level2 = "PCI_OTHER_C4ISR"
						return_String = "U1#PCI_OTHER_C4ISR.Children"
					Else
						return_String = "U1#" & ppa_Level2
					End If
					
					Return return_String
					
				End If

				
#End Region 'Get PPA List PCI

#Region "Get Tier 2 PPA PCI"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetRPEntity, RPName=|!prm_Number_PCI!|)
				If args.FunctionName.XFEqualsIgnoreCase("GetTier2PPA_PCI") Then
					Dim wfCube As String = args.NameValuePairs("WFCube")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim RPName As String = args.NameValuePairs("RPName")

					' If RP Name is empty, nothing to do 
					If RPName = "" Then
						Return Nothing
					End If					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)		
					Dim LINumber As String = args.NameValuePairs.XFGetValue("LineItemNumber")
					Dim return_String As String = String.Empty
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumber & ":U7#None:U8#None"
					
					Dim PPA_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#PPA:" & scriptGenerics)
					Dim PPA As String = PPA_Info.DataCellEx.DataCellAnnotation
					
					return_String = PPA
					
					Return return_String
					
				End If

				
#End Region 'Get Tier 2 PPA PCI

#Region "Get UII List PCI"

				'Gets list of applicable UIIs based on the PPA Level 1 and 2 selections
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetRPEntity, RPName=|!prm_Number_PCI!|)
				If args.FunctionName.XFEqualsIgnoreCase("GetUIIList_PCI") Then
					Dim wfCube As String = args.NameValuePairs("WFCube")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim RPName As String = args.NameValuePairs("RPName")

					' If RP Name is empty, nothing to do 
					If RPName = "" Then
						Return Nothing
					End If
					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)						
					Dim ppa_Level2 As String = args.NameValuePairs("PPA_Level2")
					Dim LINumber As String = args.NameValuePairs.XFGetValue("LineItemNumber")
					Dim return_String As String = String.Empty
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#"& LINumber & ":U7#None:U8#None"
					
					Dim PPA_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#PPA:" & scriptGenerics)
					Dim PPA As String = PPA_Info.DataCellEx.DataCellAnnotation
					
					return_String = PPA
					
					Return return_String
					
				End If

				
#End Region 'Get UII List PCI

#Region "GetBuildOutList"  

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetBuildOutList, RPName=|!prm_Number_OS!|, WFCube=BudEx, WFTime=2024, WFScenario=RPSeeding_FY24)
				If args.FunctionName.XFEqualsIgnoreCase("GetBuildOutList") Then
					'Get RP					
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					' If RP Name is empty, nothing to do 
					If RPName = "" Then
						Return Nothing
					End If					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)	
					Dim wfCube As String = args.NameValuePairs("WFCube")
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"	
									
					Dim Increase_Decrease_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Increase_Decrease:" & scriptGenerics)
										
					Dim increase_Decrease As String = Increase_Decrease_Info.DataCellEx.DataCellAnnotation
					
					If Increase_Decrease.XFEqualsIgnoreCase("I")
						Return "U8#Total_YesNo.Base.Remove(NA)"					
					Else 'must be Descrease so just return NA						
						Return "U8#NA"						
					End If
					
				End If
				
#End Region 'GetBuildOutList
#Region "GetUtilities" 

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetUtilities, RPName=|!prm_Number_OS!|, WFCube=BudEx, WFTime=2024, WFScenario=RPSeeding_FY24)
				If args.FunctionName.XFEqualsIgnoreCase("GetUtilities") Then
		'brapi.ErrorLog.LogMessage(si, "GetUtilitiesList XFBR Start " & DateTime.Now.Millisecond.ToString)
					'Get RP					
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					' If RP Name is empty, nothing to do 
					If RPName = "" Then
						Return Nothing
					End If					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)
					Dim wfCube As String = args.NameValuePairs("WFCube")
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"	
									
					Dim Increase_Decrease_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Increase_Decrease:" & scriptGenerics)
										
					Dim increase_Decrease As String = Increase_Decrease_Info.DataCellEx.DataCellAnnotation
					'BRApi.ErrorLog.LogMessage (si, Increase_Decrease)
				
					If Increase_Decrease.XFEqualsIgnoreCase("I")
		'brapi.ErrorLog.LogMessage(si, "GetUtilitiesList XFBR End" & DateTime.Now.Millisecond.ToString)
						Return "U8#Total_YesNo.Base.Remove(NA)"					
					Else 'must be Descrease so just return NA						
						Return "U8#No"						
					End If
					
				End If
								

#End Region 'GetUtilities

#Region "GetICASS"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetICASS, RPName=|!prm_Number_OS!|, WFCube=BudEx, WFTime=2024, WFScenario=RPSeeding_FY24)
				If args.FunctionName.XFEqualsIgnoreCase("GetICASS") Then
					'Get RP					
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					' If RP Name is empty, nothing to do 
					If RPName = "" Then
						Return Nothing
					End If					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)
					Dim wfCube As String = args.NameValuePairs("WFCube")
					Dim wfTime As String = args.NameValuePairs("WFTime") 
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"	
					
					Dim Increase_Decrease_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Increase_Decrease:" & scriptGenerics)
										
					Dim increase_Decrease As String = Increase_Decrease_Info.DataCellEx.DataCellAnnotation
				
					If Increase_Decrease.XFEqualsIgnoreCase("I")
							Return "U8#Total_ICASS.Children"
						Else
							Return "U8#No_ICASS"					
					End If
				End If								

#End Region 'GetICASS

#Region "GetTermBilletList"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetTermBilletList, RPName=|!prm_Number_OS!|, WFCube=BudEx, WFTime=2024, WFScenario=RPSeeding_FY24)
				If args.FunctionName.XFEqualsIgnoreCase("GetTermBilletList") Then
					'Get RP					
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					' If RP Name is empty, nothing to do 
					If RPName = "" Then
						Return Nothing
					End If					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)	
					Dim wfCube As String = args.NameValuePairs("WFCube")
					Dim wfTime As String = args.NameValuePairs("WFTime") 
					Dim wfScenario As String = args.NameValuePairs("WFScenario")					
					
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"	
					
					Dim Increase_Decrease_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Increase_Decrease:" & scriptGenerics)
					Dim increase_Decrease As String = Increase_Decrease_Info.DataCellEx.DataCellAnnotation
					
					If Increase_Decrease.XFEqualsIgnoreCase("I")
						Return "U8#Total_TermBillet.Children"
					Else						
						Return "U8#Term_NA"							
					End If		
				End If
				
#End Region 'GetTermBilletList

#Region "GetElectronicFlightBagList"

		'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetElectronicFlightBagList, RPName=|!prm_Number_OS!|, WFCube=BudEx, WFTime=2024, WFScenario=RPSeeding_FY24, Spe_Code_Occu_Series=|!prm_BLT_SpcCodeOccSeries_OS!|)
		If args.FunctionName.XFEqualsIgnoreCase("GetElectronicFlightBagList") Then
			
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					' If RP Name is empty, nothing to do 
					If RPName = "" Then
						Return Nothing
					End If					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)	
					Dim wfCube As String = args.NameValuePairs("WFCube")
					Dim wfTime As String = args.NameValuePairs("WFTime") 
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
                    Dim Specialty_Code As String = args.NameValuePairs("Spe_Code_Occu_Series")
				    Dim CodeId As Integer = BRApi.Finance.Members.GetMemberId(si, dimtypeid.UD8, Specialty_Code)
					Dim SpecialtyCodeText2 As String = BRApi.Finance.UD.Text(si, dimtype.UD8.Id, CodeId, 2, DimConstants.Unknown, DimConstants.Unknown)
					
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"	
					
					Dim increase_Decrease As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Increase_Decrease:" & scriptGenerics).DataCellEx.DataCellAnnotation

		
					If Increase_Decrease.XFEqualsIgnoreCase("I") And SpecialtyCodeText2.XFEqualsIgnoreCase("Y")
						Return "U8#Total_YesNo.Base.Remove(NA)"
					Else
						Return "U8#NA"
							'Do Nothing as Pilot_Type must be No
					End If
					
		End If
		
#End Region 'GetElectronicFlightBagList

#Region "GetExpenseUIIList (PCI)"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetNonBilletUIIList,  WFCube=BudEx, Req_Item=|!prm_NBLT_RequestedItem_Tier1!|, CostLine=|!prm_NBLT_Description_Tier2!|, Ud1PPA=|!prm_NBLT_PPA!|)
				If args.FunctionName.XFEqualsIgnoreCase("GetExpenseUIIList") Then
					
					Dim wfCube As String = args.NameValuePairs("WFCube")
					Dim ppa_Exp_Selection As String = args.NameValuePairs("PPA_Level2")
					Dim ppa_Exp_SelectionId As Integer = BRApi.Finance.Members.GetMemberId(si, dimtypeid.UD1, ppa_Exp_Selection)
					
					'Declare a new list for the uii Allocation Defaults to return
					Dim uiiAllocText1PPAs As New List (Of String)
					
					
					'Get the List of UD2 Members
					Dim objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "Std_Investment")
					Dim uiiMemList As List (Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U2#Total_Investment.Base", True)
					
					For Each ud2Mem As MemberInfo In uiiMemList
						'Get the Text Field1 with the PPA Members
						Dim ud2Text1 As String = BRApi.Finance.UD.Text(si, dimTypeId.UD2, ud2Mem.Member.MemberId, 1, DimConstants.Unknown, DimConstants.Unknown)
						
						If ud2Text1 = ppa_Exp_Selection Then
							Return ud2Mem.Member.Name
						Else If ud2Text1.Contains(",")
							Dim ud2Text1Split() As String = ud2Text1.Split(",")
							For Each ud1Mem As String In ud2Text1Split
								'BRApi.ErrorLog.LogMessage(si, "UD2 Member: " & ud2Mem.Member.Name & " & PPA Tag: " & ud1Mem)
								If ud1Mem.Replace(" ", "") = ppa_Exp_Selection
									'Add the uii to the list if it contains the selected PPA in the Text1 Field
									Return ud2Mem.Member.Name
								End If
							
							Next
							
						Else
						
						End If
						
					Next
					
				End If

				
#End Region 'GetExpenseUIIList (PCI)

#Region "GetLease" 

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetLease, RPName=|!prm_Number_OS!|, WFCube=BudEx, WFTime=2024, WFScenario=RPSeeding_FY24, Filter_Value=Display_Items)
				If args.FunctionName.XFEqualsIgnoreCase("GetLease") Then
					'Get RP					
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					' If RP Name is empty, nothing to do 
					If RPName = "" Then
						Return Nothing
					End If					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)	
					Dim wfCube As String = args.NameValuePairs("WFCube")
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim lease_Select As String = args.NameValuePairs("Lease_Selection")
					
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
					
					If lease_Select.XFEqualsIgnoreCase("Lease_CG")
						Return "U1#OS.Base"						
					Else
						Return "U1#NA_PPA"
						
					End If
					
				End If
								

#End Region 'GetLease

#Region "GetFurnitureList" 

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetFurnitureList, RPName=|!prm_Number_OS!|, WFCube=BudEx, WFTime=2024, WFScenario=RPSeeding_FY24)
				If args.FunctionName.XFEqualsIgnoreCase("GetFurnitureList") Then
			'Brapi.ErrorLog.LogMessage(si, "GetFurnitureList XFBR Start")
					'Get RP					
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					' If RP Name is empty, nothing to do 
					If RPName = "" Then
						Return Nothing
					End If					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)
					Dim wfCube As String = args.NameValuePairs("WFCube")
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					
					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"	
									
					Dim increase_Decrease As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Increase_Decrease:" & scriptGenerics).DataCellEx.DataCellAnnotation
							
					If Increase_Decrease.XFEqualsIgnoreCase("I")
						Return "U8#Total_YesNo.Base.Remove(NA)"
					Else
						Return "U8#NA"
					End If
					
			'Brapi.ErrorLog.LogMessage(si, "GetFurnitureList XFBR End")
			
				End If
								

#End Region 'GetFurnitureList

#Region "GetColumnList" 

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetColumnList, ColumnSelection=|!prm_ColumnTemplate_Extractor!|, View=Value_Items, WFCube=|WFCube|,  WFTime=|WFTime|,  WFScenario=|WFScenario|)
				If args.FunctionName.XFEqualsIgnoreCase("GetColumnList") Then
					Dim columns As String = args.NameValuePairs("ColumnSelection")
					'BRApi.ErrorLog.LogMessage(si, "Cube View Name Passed In: " & columns)
					Dim returnView As String = args.NameValuePairs("View")
					
					'Information to get member list
					Dim returnStringValues As String = String.Empty
					Dim returnStringDisplay As String = String.Empty
					Dim returnString As String = String.Empty
					Dim returnStringList As List(Of MemberInfo)
					Dim dimensionName As String = String.Empty
					Dim memberFilter As String = String.Empty
					
					If columns.Contains("Custom") Then
						
						Select Case columns
							Case "BDF_RP_PPA_Extractor_PPA_Custom_Columns"
								dimensionName = "Std_PPA"
								memberFilter = "U1#Total_Appropriations.Base"
								Dim loopCounter As Integer = 0
								returnStringList = BRApi.Finance.Metadata.GetMembersUsingFilter(si, dimensionName, memberFilter, True)
								For Each approp In returnStringList
									If loopCounter = 0 Then
										returnStringValues = approp.Member.Name
										returnStringDisplay = approp.Member.Description.Replace(",", "")
									Else
										returnStringValues = returnStringValues & ", " & approp.Member.Name
										returnStringDisplay = returnStringDisplay & ", " & approp.Member.Description.Replace(",", "")
									End If
									loopCounter+=1
								Next
								If returnView = "Value_Items" Then
									Return returnStringValues
								Else If returnView = "Display_Items"
									Return returnStringDisplay
								Else
									
								End If
								
							Case "BDF_RP_PPA_Extractor_UII_Custom_Columns"
								dimensionName = "Std_Investment"
								memberFilter = "U2#Alternate_Investment_Hierarchy.Base"
								Dim loopCounter As Integer = 0
								returnStringList = BRApi.Finance.Metadata.GetMembersUsingFilter(si, dimensionName, memberFilter, True)
								For Each uii In returnStringList
									If loopCounter = 0 Then
										returnStringValues = uii.Member.Name
										returnStringDisplay = uii.Member.Name & " - " & uii.Member.Description.Replace(",", "")
									Else
										returnStringValues = returnStringValues & ", " & uii.Member.Name
										returnStringDisplay = returnStringDisplay & ", " & uii.Member.Name & " - " & uii.Member.Description.Replace(",", "")
									End If
									loopCounter+=1
								Next
								If returnView = "Value_Items" Then
									Return returnStringValues
								Else If returnView = "Display_Items"
									Return returnStringDisplay
								Else
									
								End If
							
							Case "BDF_RP_PPA_Extractor_OC_Custom_Columns"
								dimensionName = "Std_ObjectClass"
								memberFilter = "U3#Total_ObjectClass.Base"
								Dim loopCounter As Integer = 0
								returnStringList = BRApi.Finance.Metadata.GetMembersUsingFilter(si, dimensionName, memberFilter, True)
								For Each objClass In returnStringList
									If loopCounter = 0 Then
										returnStringValues = objClass.Member.Name
										returnStringDisplay = objClass.Member.Description.Replace(",", "")
									Else
										returnStringValues = returnStringValues & ", " & objClass.Member.Name
										returnStringDisplay = returnStringDisplay & ", " & objClass.Member.Description.Replace(",", "")
									End If
									loopCounter+=1
								Next
								If returnView = "Value_Items" Then
									Return returnStringValues
								Else If returnView = "Display_Items"
									Return returnStringDisplay
								Else
									
								End If
								
							Case "BDF_RP_PPA_Extractor_ATU_Custom_Columns"
								
							    Dim wfCube As String = ""
							    Dim wfTime As String = ""
							    Dim wfScenario As String = ""

							    If args.NameValuePairs.ContainsKey("WFCube") Then wfCube = args.NameValuePairs("WFCube")
							    If args.NameValuePairs.ContainsKey("WFTime") Then wfTime = args.NameValuePairs("WFTime")
							    If args.NameValuePairs.ContainsKey("WFScenario") Then wfScenario = args.NameValuePairs("WFScenario")

							    ' Get Scenario and Time IDs (Required for the InUse API)
							    Dim scenarioMbr As Member = BRApi.Finance.Members.GetMember(si, DimTypeId.Scenario, wfScenario)
							    
							    ' Safety check to ensure the scenario was found
							    If Not scenarioMbr Is Nothing Then
							        Dim scenarioTypeId As Integer = BRApi.Finance.Scenario.GetScenarioType(si, scenarioMbr.MemberPk.MemberId).Id
							        Dim timeId As Integer = BRApi.Finance.Members.GetMemberId(si, DimTypeId.Time, wfTime)

							        ' Define the Member Filter
							        dimensionName = "Std_ATU"
							        memberFilter = "U4#Total_ATU.Children"
							        
							        ' Get the list of potential members
							        returnStringList = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, dimensionName), memberFilter, Nothing)
							        
							        Dim loopCounter As Integer = 0
							        If Not returnStringList Is Nothing Then
							            For Each atu In returnStringList
							                ' Perform the InUse Check
							                Dim bInUse As Boolean = BRApi.Finance.UD.InUse(si, DimTypeId.UD4, atu.Member.MemberPk.MemberId, scenarioTypeId, timeId)

							                If bInUse Then
							                    If loopCounter = 0 Then
							                        returnStringValues = atu.Member.Name
							                        returnStringDisplay = atu.Member.Name & " - " & atu.Member.Description.Replace(",", "")
							                    Else
							                        returnStringValues = returnStringValues & ", " & atu.Member.Name
							                        returnStringDisplay = returnStringDisplay & ", " & atu.Member.Name & " - " & atu.Member.Description.Replace(",", "")
							                    End If
							                    loopCounter += 1
							                End If
							            Next
							        End If
							    End If

							    ' 6. Return the formatted strings
							    If returnView.XFEqualsIgnoreCase("Value_Items") Then
							        Return returnStringValues
							    Else If returnView.XFEqualsIgnoreCase("Display_Items")
							        Return returnStringDisplay
							    End If
							
						End Select
						
					Else
						Return "NA"
						
					End If	
			
				End If
								

#End Region 'GetColumnList

#Region "GetRPList" 

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetRPList, RPRowOption = |!prm_RPRowOption!|, WFTime=2024)
				If args.FunctionName.XFEqualsIgnoreCase("GetRPList") Then
					Dim rpRowOption As String = args.NameValuePairs("RPRowOption")
					Dim wfTime As String = args.NameValuePairs("WFTime")
					
					If rpRowOption = "All" Then
						Return "FY" & wfTime.Substring(2,2) & "_RP.Base.Where(Name DoesNotContain '_WV') "
					Else 'Is custom
						Return "RP.List(|!prm_RPRowSelector!|)"

					End If	
						
				End If
								

#End Region 'GetRPList

#Region "GetAllOrCustomLeadOffices" 

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetAllOrCustomLeadOffices, LeadOfficeSelection=|!prm_RPLeadOfficeOption!|)
				If args.FunctionName.XFEqualsIgnoreCase("GetAllOrCustomLeadOffices") Then
					
					'If |!prm_RPLeadOfficeOption!| = All Then Return U8#Total_LeadOffice.Base
					Dim leadOfficeOption As String = args.NameValuePairs("LeadOfficeSelection")
					
					If leadOfficeOption.Contains("All") Then
						
						Return "U8#Total_LeadOffice.Base"
						
					Else
						
						Return "U8#Total_LeadOffice.List(|!prm_RPLeadOfficeSelector!|)"
						
					End If
					
					'If Custom Then Return U8#List(|!prm_RPLeadOfficeSelector!|)
			
				End If
				
#End Region 'GetLeadOfficeList
						
#Region "ShowHide_SelectionCbx" 

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, ShowHide_SelectionCbx, RowColOption= |!prm_RPRowOption!|)
				If args.FunctionName.XFEqualsIgnoreCase("ShowHide_SelectionCbx") Then
					Dim rowColOption As String = args.NameValuePairs("RowColOption")
					
					If rowColOption.Contains("Custom") Then
						Return "True"
					Else
						Return "False"

					End If	
						
				End If
								

#End Region 'ShowHide_SelectionCbx

#Region "ShowHide_SelectionCbx_RPScenarioStatus_Directorate" 

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, ShowHide_SelectionCbx_RPScenarioStatus_Directorate, selectionOption=|!prm_RPSelectionOption!|)
				If args.FunctionName.XFEqualsIgnoreCase("ShowHide_SelectionCbx_RPScenarioStatus_Directorate") Then
					Dim selectionOption As String = args.NameValuePairs("SelectionOption")
			
							
					If selectionOption.Contains("Directorate") Then 
						Return "True"

					Else 
						Return "False"

					End If	

				End If	
								

#End Region 'ShowHide_SelectionCbx_RPScenarioStatus_Directorate

#Region "ShowHide_SelectionCbx_RPScenarioStatus_LeadOffice" 

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, ShowHide_SelectionCbx_RPScenarioStatus_LeadOffice, selectionOption=|!prm_RPSelectionOption!|)
				If args.FunctionName.XFEqualsIgnoreCase("ShowHide_SelectionCbx_RPScenarioStatus_LeadOffice") Then
					Dim selectionOption As String = args.NameValuePairs("SelectionOption")
			
					If selectionOption.Contains("Office") Then
						Return "True"
					
					Else 
						Return "False"

					End If	

				End If	
								

#End Region 'ShowHide_SelectionCbx_RPScenarioStatus_LeadOffice

#Region "ShowHide_SelectionCbx_RPScenarioStatus_RPName" 

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, ShowHide_SelectionCbx_RPScenarioStatus_RPName, selectionOption=|!prm_RPSelectionOption!|)
				If args.FunctionName.XFEqualsIgnoreCase("ShowHide_SelectionCbx_RPScenarioStatus_RPName") Then
					Dim selectionOption As String = args.NameValuePairs("SelectionOption")
			
					If selectionOption.Contains("RPName") Then
						Return "True"
					
					Else 
						Return "False"

					End If	

				End If	
								

#End Region 'ShowHide_SelectionCbx_RPScenarioStatus_RPName

#Region "GetColSum" 

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, ShowHide_SelectionCbx, RowColOption= |!prm_RPRowOption!|)
				If args.FunctionName.XFEqualsIgnoreCase("GetColSum") Then
					Dim memberFilter As String = args.NameValuePairs("MemberFilter")
					Dim dimTypeAbbr As String = args.NameValuePairs("DimTypeAbbr")
					
					'BRApi.ErrorLog.LogMessage(si, "Member Filter: " & memberFilter)
					Dim selectedArray() As String = memberFilter.Replace(" ", "").Split(",")
					Dim selectedList As List (Of String) = selectedArray.ToList()
					Dim returnString As String = String.Empty
					Dim loopCounter As Integer = 0
					For Each member In selectedList
						'BRApi.ErrorLog.LogMessage(si, "Member In List: " & member)
						If loopCounter = 0 Then
							member = dimTypeAbbr & "#" & member
						Else
							member = "+" & dimTypeAbbr & "#" & member
						End If
						returnString = returnString & member
						loopCounter+=1
					Next
					
					'BRApi.ErrorLog.LogMessage(si, "Return String: " & returnString)
					Return returnString
					
				End If
								

#End Region 'GetColSum

#Region "SetPCIVariation"' ** SetPCIVariation** 

				 If args.FunctionName.XFEqualsIgnoreCase("SetPCIVariation") Then
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
					
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					' If RP Name is empty, nothing to do 
					If RPName = "" Then
						Return Nothing
					End If					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)										

					Dim scriptGenerics As String = "E#" & RP_Entity & ":C#Local:S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"								

					Dim ppa_Level1_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#PPA_Level1_PCI:" & scriptGenerics)
					Dim ppa_Level1 As String = ppa_Level1_Info.DataCellEx.DataCellAnnotation
					'BRApi.ErrorLog.LogMessage(si, "PPA Level 1 Script: " & "A#PPA_Level1_PCI:" & scriptGenerics & " and Value of PPA Level 1: " & ppa_Level1)
					Dim ppa_Level2_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#PPA_Level2_PCI:" & scriptGenerics)
					Dim ppa_Level2 As String = ppa_Level2_Info.DataCellEx.DataCellAnnotation

					'Logic to show different dashboard depending on appropriation type
					Dim PCIReport As String = "BDF_RP_SummaryBook_Proq.xfDoc.pdfBook"
					'
					If (ppa_Level1 = "PCI_SFATON") Then
						'Cons
						PCIReport = "BDF_RP_SummaryBook_Constr.xfDoc.pdfBook"
					End If
					
					If ((ppa_Level1 = "PCI_OTHER") And (ppa_Level2 = "PCI_OTHER_ES")) Then
						'End Items
						PCIReport = "BDF_RP_SummaryBook_End.xfDoc.pdfBook"
					End If
					Return PCIReport
				End If
#End Region

#Region "SetPCIVariationADM"' ** SetPCIVariation for ADM** 

				 If args.FunctionName.XFEqualsIgnoreCase("SetPCIVariationADM") Then
					'Get Time from current Workflow
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim wfCube As String = args.NameValuePairs("WFCube")
					
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					' If RP Name is empty, nothing to do 
					If RPName = "" Then
						Return Nothing
					End If					
					Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)										

					Dim scriptGenerics As String = "E#" & RP_Entity & ":C#Local:S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"								

					Dim ppa_Level1_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#PPA_Level1_PCI:" & scriptGenerics)
					Dim ppa_Level1 As String = ppa_Level1_Info.DataCellEx.DataCellAnnotation
					'BRApi.ErrorLog.LogMessage(si, "PPA Level 1 Script: " & "A#PPA_Level1_PCI:" & scriptGenerics & " and Value of PPA Level 1: " & ppa_Level1)
					Dim ppa_Level2_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#PPA_Level2_PCI:" & scriptGenerics)
					Dim ppa_Level2 As String = ppa_Level2_Info.DataCellEx.DataCellAnnotation

					'Logic to show different dashboard depending on appropriation type
					Dim PCIReport As String = "BDF_RP_SummaryBook_Proq_PCI_ADM.xfDoc.pdfBook"
					'
					If (ppa_Level1 = "PCI_SFATON") Then
						'Cons
						PCIReport = "BDF_RP_SummaryBook_Constr_PCI_ADM.xfDoc.pdfBook"
					End If
					
					If ((ppa_Level1 = "PCI_OTHER") And (ppa_Level2 = "PCI_OTHER_ES")) Then
						'End Items
						PCIReport = "BDF_RP_SummaryBook_End_PCI_ADM.xfDoc.pdfBook"
					End If
					Return PCIReport
				End If
#End Region

#Region "GetFundingBaseMembers" 

'				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetFundingBaseMembers, ParentMember = Funding)
'				If args.FunctionName.XFEqualsIgnoreCase("GetFundingBaseMembers") Then
'					Dim parentMemberName As String = args.NameValuePairs("ParentMember")
					
'					Dim acctDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "BudFm_Account")
'					Dim parentMemberId As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, parentMember)
'					Dim acctBaseMembers As List (Of Member) = BRApi.Finance.Members.GetBaseMembers(si, acctDimPk, parentMember, Nothing)
					
'					For Each acctBaseMembers As Member In acctBaseMembers
'						BRApi.ErrorLog.LogMessage(si, "Account Base member: " & acctBaseMember)
'					Next
					

'				End If
								

#End Region 'GetColSum

#Region "GetObjectClassVisible"
				'cbx_NBLT_ObjectClass_OS: IsVisible = XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetObjectClassVisible, WFCube=BudEx, WFTime=2024,  WFScenario=RPSeeding_FY24, CostLine=|!prm_NBLT_Description_Tier2_OS!|)
				
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetObjectClassVisible, WFCube=BudEx, CostLine=|!prm_NBLT_Description_Tier2!|) 
				If args.FunctionName.XFEqualsIgnoreCase("GetObjectClassVisible") Then
					Dim wfCube As String = args.NameValuePairs("WFCube")
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim wfScenario As String = args.NameValuePairs("WFScenario")
					Dim tier1Desc As String = args.NameValuePairs("Tier1Desc")
					Dim costLine As String = args.NameValuePairs("CostLine")
					'Do nothing if the tier1Desc is empty. Default is to not show the Object Class
					If String.IsNullOrEmpty(tier1Desc) Then Return "False"
						
					'Check if the tier 1 description only has one member, and if so, what the allocation type of that member is
					Dim tier1DescDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "Std_CostLine")
					Dim tier1DescId As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD5, tier1Desc)
					Dim tier1DescChildren As New List (Of Member)
					tier1DescChildren = BRApi.Finance.Members.GetChildren(si, tier1DescDimPk, tier1DescId)
					
					Dim objClassAllocType As String = String.Empty
					
					'Check to see if my first characters are 999 because they are historical cost estimate items used prior to FY26, if so return the integer that is line item specific
					If tier1Desc.Substring(0,3) = "999"
							objClassAllocType = "3"
		            Else
					
						If tier1DescChildren.Count = 1 Then
							Dim objClassAllocType_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "S#" & wfScenario & ":T#" & wfTime & ":E#NA:A#None:V#Annotation:O#Forms:I#None:F#None:U1#None:U2#None:U3#No_ObjectClass:U4#None:U5#" & tier1DescChildren(0).Name & ":U6#None:U7#None:U8#None")
							objClassAllocType = objClassAllocType_Info.DataCellEx.DataCellAnnotation
						Else
							'Checks the allocation type of the tier 2 description
							Dim objClassAllocType_Info As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "S#" & wfScenario & ":T#" & wfTime & ":E#NA:A#None:V#Annotation:O#Forms:I#None:F#None:U1#None:U2#None:U3#No_ObjectClass:U4#None:U5#" & costLine & ":U6#None:U7#None:U8#None")
							objClassAllocType = objClassAllocType_Info.DataCellEx.DataCellAnnotation	
						End If
					End If
					
					'Do nothing if the atuAllocType is empty. Default is to not show the Object Class
					If String.IsNullOrEmpty(objClassAllocType) Then Return "False"
					If objClassAllocType = "1" Then
						Return "False"
					Else
						Return "True"
					End If

				End If
			
#End Region 'GetObjectClassVisible	

#Region "GetESTTimeStamp" 

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetESTTimeStamp)
				If args.FunctionName.XFEqualsIgnoreCase("GetESTTimeStamp") Then
						
					Dim now As DateTime = Date.UtcNow
				    Dim easternzoneid As String = "Eastern Standard Time"
				    Dim easternZone As TimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(easternZoneId)
				    Dim convertTimeEST As String = TimeZoneInfo.ConvertTime(now, easternzone).ToString & " EST"	
					
					Return convertTimeEST
					
				End If
								
#End Region 'GetESTTimeStamp

#Region "GetTwoDigitYear" 

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetTwoDigitYear, WFTime=2024)
				If args.FunctionName.XFEqualsIgnoreCase("GetTwoDigitYear") Then
					Dim wfTime As String = args.NameValuePairs("WFTime")
					
					Return wfTime.Substring(2,2)
					
				End If
								

#End Region 'GetTwoDigitYear

#Region "GetTwoDigitYearPrior" 

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetTwoDigitYearPrior, WFTime=2024)
				If args.FunctionName.XFEqualsIgnoreCase("GetTwoDigitYearPrior") Then
					Dim wfTime As String = args.NameValuePairs("WFTime")
					
					Return (wfTime.Substring(2,2).XFConvertToInt - 1).ToString
					
				End If
								

#End Region 'GetTwoDigitYear

#Region "GetTwoDigitYearPrior2" 

                'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetTwoDigitYearPrior2, WFTime=2024)
                If args.FunctionName.XFEqualsIgnoreCase("GetTwoDigitYearPrior2") Then
                    Dim wfTime As String = args.NameValuePairs("WFTime")

                    Return (wfTime.Substring(2,2).XFConvertToInt - 2).ToString

                End If 

#End Region 'GetTwoDigitYearPrior2

#Region "GetTwoDigitYearFromScenario" 

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetTwoDigitYearFromScenario, Scenario=|!ScenarioName!|)
				If args.FunctionName.XFEqualsIgnoreCase("GetTwoDigitYearFromScenario") Then
					Dim scenario As String = args.NameValuePairs("Scenario")
					Dim year As String = String.Empty
					
					If scenario.Length > 0
						year = scenario.Substring(scenario.Length -2, 2)
					End If
					
					Return year
					
				End If
								

#End Region 'GetTwoDigitYear

#Region "GetScenarioBudgetYear" 

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetScenarioBudgetYear, Scenario=|<MyScenario>|)
				If args.FunctionName.XFEqualsIgnoreCase("GetScenarioBudgetYear") Then
					Dim Scenario As String = args.NameValuePairs("Scenario")
					If (Not Scenario.Length = 0)
						Dim Scenario_Split As List(Of String) = StringHelper.SplitString(Scenario, "_")
						Dim Scenario_BY As String = (Scenario_Split(1).Substring(2,2).XFConvertToInt + 2000).ToString
						Return Scenario_BY
					Else
						Return " "
					End If
					
				End If
								

#End Region 'GetScenarioBudgetYear

#Region "GetModDashboardName" 

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetModDashboardName, WFTime=2024, SelectedMember=|!prm_Mod_SelectedModHierachyName_ADM!|)
				If args.FunctionName.XFEqualsIgnoreCase("GetModDashboardName") Then
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim WFScenario As String = args.NameValuePairs("WFScenario")
					'BRApi.ErrorLog.LogMessage(si, WFScenario)
					Dim selectedMember As String = args.NameValuePairs("SelectedMember")					
					Dim dbToReturn As String = String.Empty
					Dim NoActions As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Mod_Content_Mapping_NoActions_ADM") '04b2bz4_BDF_RP_Dashboard_Content_CGDHS_Mapping_ActionsForAllMembers_NoActions_ADM
					Dim ActionsForMods As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Mod_Content_Mapping_ActionsForMods_ADM") '04b2b2_BDF_RP_Dashboard_Content_CGDHS_Mapping_ActionsForMods_ADM
					Dim ActionsForModsOMBJ_Enacted As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Mod_Content_Mapping_ActionsForMods_ADM_OMBJ_Through_Enacted") '04b2b2_BDF_RP_Dashboard_Content_CGDHS_Mapping_ActionsForMods_ADM_OMBJ_Through_Enacted
					Dim ActionsForParents As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Mod_Content_Mapping_ActionsForParents_ADM") '04b2b1_BDF_RP_Dashboard_Content_CGDHS_Mapping_ActionsForParents_ADM
					Dim isThisOSStandard As Boolean
					Dim isThisOSABV As Boolean
					'Retrieve the Text 8 value from the member
					Dim selectedMemberId As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Flow, selectedMember)
					Dim selectedMemberText8 As String = BRApi.Finance.Flow.Text(si, selectedMemberId, 8, DimConstants.Unknown, DimConstants.Unknown)
					
					'Setting a boolean if it falls under the O&S heirarchy 
					Dim flowPK As DimPk = BRApi.Finance.Dim.GetDimPk(si, "Std_Flow")
					Dim selectedMemberParent As List(Of member) = BRApi.Finance.Members.GetAncestors(si,flowPK,selectedMemberId,False)
					
					For Each parent As Member In selectedMemberParent
						If parent.ToString.Contains("USCG_OS_" & wfTime.Substring(2,2)) Then
							isThisOSStandard = True
						ElseIf parent.ToString.Contains("USCG_ABVOS_" & wfTime.Substring(2,2))
							isThisOSABV = True
						End If 
					Next
					
					'If selectedMember equals USCG_FY##_Mods, USCG_PRI_##, or USCG_PGM_##, return nothing as we shouldn't be adding children underneath these
					Select Case selectedMember
					Case "","None"
						dbToReturn = NoActions
					Case "USCG_FY" & wfTime.Substring(2,2) & "_Mods",
							"USCG_PRI_" & wfTime.Substring(2,2),
							"USCG_PGM_" & wfTime.Substring(2,2),
							"USCG_ABV_FY" & wfTime.Substring(2,2) & "_Mods"
						dbToReturn = NoActions
					Case Else
						'Standard
						'If selectedMemberText8 contains Mod -> is this mod O&S? -> are we in OMBJ? CJ? Enacted?	
						If (selectedMemberText8.XFContainsIgnoreCase("Mod")) And (isThisOSStandard = True) And (WFScenario.Contains("OMBJ") Or WFScenario.Contains("CJ") Or WFScenario.Contains("Enacted"))
							dbToReturn = ActionsForModsOMBJ_Enacted
						
						'ABV
						'If selectedMemberText8 contains Mod -> is this mod O&S? -> are we in OMBJ? 	
						Else If (selectedMemberText8.XFContainsIgnoreCase("Mod")) And (isThisOSABV = True) And (WFScenario.Contains("OMBJ"))
							dbToReturn = ActionsForModsOMBJ_Enacted	
							
						'If selectedMemberText8 contains Mod, show the Mod edits dashboard
						Else If selectedMemberText8.XFContainsIgnoreCase("Mod")
							dbToReturn = ActionsForMods	
							
						'If selectedMemberText8 contains RP, show RP Summary?
						Else If selectedMemberText8.XFContainsIgnoreCase("RP_FY")
							dbToReturn = NoActions
						
						'If not a mod or RP so show AddHierachy member dashboard
						Else 
							dbToReturn = ActionsForParents
						End If
					End Select
					
					Return dbToReturn
					
				End If
								

#End Region 'GetTwoDigitYear

#Region "GetModInfoString" 

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetModInfoString, WFTime=|WFTime|, SelectedMember=|!prm_Mod_SelectedModHierachyName_ADM!|)
				If args.FunctionName.XFEqualsIgnoreCase("GetModInfoString") Then
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim selectedMember As String = args.NameValuePairs("SelectedMember")					
					Dim ReturnString As String = String.Empty

					'Retrieve the Text 8 value from the member
					Dim selectedMemberId As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Flow, selectedMember)
					Dim selectedMemberText8 As String = BRApi.Finance.Flow.Text(si, selectedMemberId, 8, DimConstants.Unknown, DimConstants.Unknown)
					
					Dim isThisOSStandard As Boolean
					'Dim isThisOSABV As Boolean
					
					'Setting a boolean if it falls under the O&S heirarchy 
					Dim flowPK As DimPk = BRApi.Finance.Dim.GetDimPk(si, "Std_Flow")
					Dim selectedMemberParent As List(Of member) = BRApi.Finance.Members.GetAncestors(si,flowPK,selectedMemberId,False)
					
					For Each parent As Member In selectedMemberParent
						If parent.ToString.Contains("USCG_OS_" & wfTime.Substring(2,2)) Then
							isThisOSStandard = True
						End If 
					Next

					If isThisOSStandard Then
						
						ReturnString = "OMBJ - CJ Mod Info"
						
					Else
						
						ReturnString = "OMBJ Mod Info"
						
					End If
					
					Return ReturnString
					
				End If
								

#End Region

#Region "GetFlowMemberDescription" 

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetMemberDescription, MemberName=|!prm_Mod_SelectedModHierachyName_ADM!|)
				If args.FunctionName.XFEqualsIgnoreCase("GetMemberDescription") Then
					Dim memberName As String = args.NameValuePairs("MemberName")
					Dim objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "Std_Flow")
					Dim memberList As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "F#" & memberName, True)
					Dim description As String = String.Empty
					
					If memberList.Count > 0 							
						Select Case memberName
						Case "","None"
							description = ""
						Case Else 
							description = BRApi.Finance.Members.GetMember(si, dimtypeid.Flow, memberName).Description
						End Select
					Else
						description = ""
					End If
						
					Return description
					
				End If
								

#End Region

#Region "GetFlowMemberNameandDescription" 

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetFlowMemberNameandDescription, MemberName=|!prm_ConcReview_UD8_OS!|)
				If args.FunctionName.XFEqualsIgnoreCase("GetFlowMemberNameandDescription") Then
					Dim memberName As String = args.NameValuePairs("MemberName")
					Dim description As String = String.Empty
					
					If memberName.Length = 0
						description = ""
					Else 
						description = memberName & " - " & BRApi.Finance.Members.GetMember(si, dimtypeid.Flow, memberName).Description
					End If
						
					Return description
					
				End If
								

#End Region

#Region "GetUD1MemberDescription" 

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetUD1MemberDescription, MemberName=|!prm_ConcReview_UD8_OS!|)
				If args.FunctionName.XFEqualsIgnoreCase("GetUD1MemberDescription") Then
					Dim memberName As String = args.NameValuePairs("MemberName")
					Dim description As String = String.Empty
					
					If memberName.Length = 0
						description = ""
					Else 
						description = BRApi.Finance.Members.GetMember(si, dimtypeid.UD1, memberName).Description
					End If
						
					Return description
					
				End If
								

#End Region

#Region "GetUD2MemberNameandDescription" 

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetUD2MemberNameandDescription, MemberName=|!prm_ConcReview_UD8_OS!|)
				If args.FunctionName.XFEqualsIgnoreCase("GetUD2MemberNameandDescription") Then
					Dim memberName As String = args.NameValuePairs("MemberName")
					Dim description As String = String.Empty
					
					If memberName.Length = 0
						description = ""
					Else 
						description = memberName & " - " & BRApi.Finance.Members.GetMember(si, dimtypeid.UD2, memberName).Description
					End If
						
					Return description
					
				End If
								

#End Region

#Region "GetUD3MemberDescription" 

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetUD3MemberNameandDescription, MemberName=|!prm_ConcReview_UD8_OS!|)
				If args.FunctionName.XFEqualsIgnoreCase("GetUD3MemberDescription") Then
					Dim memberName As String = args.NameValuePairs("MemberName")
					Dim description As String = String.Empty
					
					If memberName.Length = 0
						description = ""
					Else 
						description = BRApi.Finance.Members.GetMember(si, dimtypeid.UD3, memberName).Description
					End If
						
					Return description
					
				End If
								

#End Region

#Region "GetUD3MemberNameandDescription" 

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetUD3MemberNameandDescription, MemberName=|!prm_ConcReview_UD8_OS!|)
				If args.FunctionName.XFEqualsIgnoreCase("GetUD3MemberNameandDescription") Then
					Dim memberName As String = args.NameValuePairs("MemberName")
					Dim description As String = String.Empty
					
					If memberName.Length = 0
						description = ""
					Else 
						description = memberName & " - " & BRApi.Finance.Members.GetMember(si, dimtypeid.UD3, memberName).Description
					End If
						
					Return description
					
				End If
								

#End Region

#Region "GetUD4MemberDescription" 

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetUD4MemberDescription, MemberName=|!prm_ConcReview_UD8_OS!|)
				If args.FunctionName.XFEqualsIgnoreCase("GetUD4MemberDescription") Then
					Dim memberName As String = args.NameValuePairs("MemberName")
					Dim description As String = String.Empty
					
					If memberName.Length = 0
						description = ""
					Else 
						description = BRApi.Finance.Members.GetMember(si, dimtypeid.UD4, memberName).Description
					End If
						
					Return description
					
				End If
								

#End Region

#Region "GetUD5MemberDescription" 

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetUD5MemberDescription, MemberName=|!prm_ConcReview_UD8_OS!|)
				If args.FunctionName.XFEqualsIgnoreCase("GetUD5MemberDescription") Then
					Dim memberName As String = args.NameValuePairs("MemberName")
					Dim description As String = String.Empty
					
					If memberName.Length = 0
						description = ""
					Else 
						description = BRApi.Finance.Members.GetMember(si, dimtypeid.UD5, memberName).Description
					End If
						
					Return description
					
				End If
								

#End Region

#Region "GetUD6MemberDescription" 

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetUD6MemberDescription, MemberName=|!prm_ConcReview_UD8_OS!|)
				If args.FunctionName.XFEqualsIgnoreCase("GetUD6MemberDescription") Then
					Dim memberName As String = args.NameValuePairs("MemberName")
					Dim description As String = String.Empty
					
					If memberName.Length = 0
						description = ""
					Else 
						description = BRApi.Finance.Members.GetMember(si, dimtypeid.UD6, memberName).Description
					End If
						
					Return description
					
				End If
								

#End Region

#Region "GetUD8MemberDescription" 

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetUD8MemberDescription, MemberName=|!prm_ConcReview_UD8_OS!|)
				If args.FunctionName.XFEqualsIgnoreCase("GetUD8MemberDescription") Then
					Dim memberName As String = args.NameValuePairs("MemberName")
					Dim description As String = String.Empty
					
					If memberName.Length = 0
						description = ""
					Else 
						description = BRApi.Finance.Members.GetMember(si, dimtypeid.UD8, memberName).Description
					End If
						
					Return description
					
				End If
								

#End Region

#Region "GetDataCellUD8Comments"

				'having an account paremeter (eg. prm_CR_ACCT_OS) will allow you to highlight a specific row cell for
				'expansion it was not needed for this instance but decided to keep the account parameters as a future example
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetDataCellUD8Comments, RPName=|!prm_Number_OS!|, AcctMbrName=|!prm_CR_ACCT_OS!|, UD8MbrName=|!prm_CR_UD8_OS!|)
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetDataCellUD8Comments, RPName=|!prm_Number_OS!|, AcctMbrName=Comments_ConcReview, UD8MbrName=|!prm_CR_UD8_OS!|)
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetDataCellUD8Comments, RPName=|!prm_Number_OS!|, AcctMbrName=Resolution_ConcReview, UD8MbrName=|!prm_CR_UD8_OS!|)
				If args.FunctionName.XFEqualsIgnoreCase("GetDataCellUD8Comments") Then
					Dim returnValue As String = ""
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					Dim AcctMbrName As String = args.NameValuePairs.XFGetValue("AcctMbrName")
					Dim UD8MbrName As String = args.NameValuePairs.XFGetValue("UD8MbrName")
					Dim WFTime As Member = args.SubstVarSourceInfo.WFTime
					Dim time As String = WFTime.Name
					Dim year As String = time.Substring(2,2)
					Dim cube As String = args.SubstVarSourceInfo.WFCube
					Dim scenario As String = args.SubstVarSourceInfo.WFScenario.Name
					
					If (Not RPName = "") Then
						Dim rpEntity As String = rpUtils.Get_RP_Entity(si,RPName)
						Dim getcellPOVmbrScript As String = "E#" & rpEntity & ":C#Local:S#" & scenario & ":T#" & time & ":V#Annotation:A#" & AcctMbrName & ":F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#" & UD8MbrName
						Dim cellInfo As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, cube, getcellPOVmbrScript)
						returnValue = cellInfo.DataCellEx.DataCellAnnotation
					End If
					
					Return returnValue
					
				End If

#End Region

#Region "GetAccHist" 

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetAccHist, Profile=CG_Execution_Distribution_Program_Administration.Centralized Bills)
				If args.FunctionName.XFEqualsIgnoreCase("GetAccHist") Then
					Dim profileName As String = args.NameValuePairs("Profile")
					'BrApi.ErrorLog.LogMessage(si, "profileName: "& profileName)	
						
					If ProfileName = "Hisorical Data Load .Update Baseline" Then 
						Return "Carryover_Start"						
					Else
						Return "Historical_RP"
					End If	
							
				End If
						

#End Region

#Region "GetAnnotationValue"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetAnnotationValue, RPName=|!prm_Number_PCI!|, AcctMbrName=PPA_Level1_PCI, Scenario=|WFScenario|)
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetAnnotationValue, RPName=|!prm_Number_PCI!|, AcctMbrName=PPA_Level2_PCI, Scenario=|WFScenario|)
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetAnnotationValue, RPName=|!prm_Number_RD!|, AcctMbrName=PPA, Scenario=|WFScenario|)
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetAnnotationValue, RPName=|!prm_Number_RD!|, AcctMbrName=ATU, Scenario=|WFScenario|)
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetAnnotationValue, RPName=|!prm_Number_RD!|, AcctMbrName=UII, Scenario=|WFScenario|)
				
				If args.FunctionName.XFEqualsIgnoreCase("GetAnnotationValue") Then
					Dim returnValue As String = ""
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					Dim AcctMbrName As String = args.NameValuePairs.XFGetValue("AcctMbrName")
					Dim Scenario As String = args.NameValuePairs.XFGetValue("Scenario")
					Dim WFTime As Member = args.SubstVarSourceInfo.WFTime
					Dim time As String = WFTime.Name
					Dim year As String = time.Substring(2,2)
					Dim cube As String = args.SubstVarSourceInfo.WFCube
					
					If (Not RPName = "") Then
						Dim rpEntity As String = rpUtils.Get_RP_Entity(si,RPName)
						Dim getcellPOVmbrScript As String = "E#" & rpEntity & ":C#Local:S#" & Scenario & ":T#" & time & ":V#Annotation:A#" & AcctMbrName & ":F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
						Dim cellInfo As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, cube, getcellPOVmbrScript)
						returnValue = cellInfo.DataCellEx.DataCellAnnotation
						'BRApi.ErrorLog.LogMessage(si, "DZ----GetAnnotationValue--RPName=" + RPName + "---acct=" + AcctMbrName + "---cell value=" + returnValue)
					End If
					Return returnValue
				End If

#End Region

#Region "Get Base RPs by Year" 

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetBaseRPsByYear, Year = 2024)
				If args.FunctionName.XFEqualsIgnoreCase("GetBaseRPsByYear") Then
					
					Dim wfYear As String = args.NameValuePairs("Year")
					
					Return "FY" & wfYear.Substring(2,2) & "_RPs.Base"
					
				End If
				
#End Region

#Region "GetSourceScenariosToRollForwardFrom"
	If args.FunctionName.XFEqualsIgnoreCase("GetSourceScenariosToRollForwardFrom") Then
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
				ScenarioMemberFilterScript = "S#OMBJ_FY" & wfPriorYearTwoDigit & ", S#CJ_FY" & wfPriorYearTwoDigit & ", S#Enacted_FY" & wfPriorYearTwoDigit
			Else If wfScenario.XFContainsIgnoreCase("OMBJ_")
				' This is the case of rolling forward from within current budget year
				'  Valid options for for source scenarios are current budget year's RAP_<CurrentYear>
				ScenarioMemberFilterScript = "S#RAP_FY" & wfCurrentYearTwoDigit
				
			Else If wfScenario.XFContainsIgnoreCase("CJ_")
				' This is the case of rolling forward from within current budget year
				'  Valid options for for source scenarios are cuurent budget year's OMBJ_<CurrentYear>
				ScenarioMemberFilterScript = "S#OMBJ_FY" & wfCurrentYearTwoDigit
				
			Else If wfScenario.XFContainsIgnoreCase("Enacted_")
				' This is the case of rolling forward from within current budget year
				'  Valid options for for source scenarios are cuurent budget year's CJ_<CurrentYear>
				ScenarioMemberFilterScript = "S#CJ_FY" & wfCurrentYearTwoDigit
			End If
			
		End If
			
		Return ScenarioMemberFilterScript
			
	End If

#End Region

#Region "GetSourceScenariosToRollForwardFrom_BaseAndAnnTerm"
	If args.FunctionName.XFEqualsIgnoreCase("GetSourceScenariosToRollForwardFrom_BaseAndAnnTerm") Then
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
			
		Return ScenarioMemberFilterScript
			
	End If

#End Region

#Region "GetSourceScenariosToRollForwardFrom_BaseAndAnnTerm_Default"
	If args.FunctionName.XFEqualsIgnoreCase("GetBaseandAnnTermDefault") Then

		Dim wfCurrentYear As String = args.NameValuePairs("WFTime")
		Dim wfScenario As String = args.NameValuePairs("WFScenario")
		
		Dim wfCurrentYearTwoDigit As String = wfCurrentYear.Substring(2,2)
		Dim wfPriorYearTwoDigit As String = wfCurrentYearTwoDigit - 1
		
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
		    ' Brapi.ErrorLog.LogMessage(si,textSplit(0).Substring(2))
		     Return textSplit(0).Substring(2)
			
	End If

#End Region

#Region "ShowHideMassBilletDelete" 

				
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, ShowHideMassBilletDelete)
				
				If args.FunctionName.XFEqualsIgnoreCase("ShowHideMassBilletDeleteButton") Then
				
					
					If  BRApi.Security.Authorization.IsUserInGroup(si, "USCG_FERBE_BudFm_r_PowerUser") Then

						   Return "True"
					Else
						    Return "False"
					End If
					
				End If
			
								
#End Region

#Region "GetUserInGroupForReporting"
				
				'This XFBR is built to differentiate which Reporting Workflow Dashboard Office Users See vs. Power Users
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetUserInGroupForReporting)
				If args.FunctionName.XFEqualsIgnoreCase("GetUserInGroupForReporting") Then
					'Replace this with the name of the dashboard parameter containing the literal value of the security group that we want to see this
					Dim securityGroupName1 As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Security_BudFm_r_PowerUser")
					If (BRApi.Security.Authorization.IsUserInGroup(si, securityGroupName1) Or Brapi.Security.Authorization.IsUserInAdminGroup(si)) Then
						Return "02_BDF_RP_Reporting_Content_PowerUsers"
					Else
						Return "02_BDF_RP_Reporting_Content_OfficeUsers"
					End If
				End If 'FunctionName				
					

#End Region

#Region "GetRollForwardSeq"
				'This function retrieves the correct rollforward sequence button based on the WFScenario
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetRollForwardSeq, Scenario=RPSeeding_FY24, Time=2024, SourceScenario=|!prm_Mod_FromScenario_ADM!|, Button=FromPriorYear)
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetRollForwardSeq, Scenario=RPSeeding_FY24, Time=2024, SourceScenario=|!prm_Mod_FromScenario_ADM!|, Button=InYear)
				If args.FunctionName.XFEqualsIgnoreCase("GetRollForwardSeq") Then
					Dim scenario As String = args.NameValuePairs("Scenario")	
					Dim wfCurrentYear As String = args.NameValuePairs("Time")
					Dim wfCurrentYearTwoDigit As Integer = wfCurrentYear.XFConvertToInt - 2000
					Dim sourceScenario As String = args.NameValuePairs("SourceScenario")
					Dim sourceScenarioTwoDigit As Integer
					Dim button As String = args.NameValuePairs("Button")	
					
					If (Not sourceScenario = "")
						sourceScenarioTwoDigit = sourceScenario.Substring(sourceScenario.Length - 2, 2).XFConvertToInt
						If (Not scenario = "")
							If wfCurrentYear >= "2025"							
								If scenario.XFContainsIgnoreCase("RAP_")
									If button.XFEqualsIgnoreCase("FromPriorYear") Then Return "True"
									If button.XFEqualsIgnoreCase("InYear") Then Return "False"
								Else 'must be OMBJ, CJ, Enacted so show the button rollforward in year
									If button.XFEqualsIgnoreCase("FromPriorYear") Then Return "False"
									If button.XFEqualsIgnoreCase("InYear") Then Return "True"
								End If 'rpText3
							Else 'must be 2025 or prior so rollforward depending on the source scenario
								If sourceScenarioTwoDigit = wfCurrentYearTwoDigit
									If button.XFEqualsIgnoreCase("FromPriorYear") Then Return "False"
									If button.XFEqualsIgnoreCase("InYear") Then Return "True"
								Else If sourceScenarioTwoDigit = (wfCurrentYearTwoDigit -1)
									If button.XFEqualsIgnoreCase("FromPriorYear") Then Return "True"
									If button.XFEqualsIgnoreCase("InYear") Then Return "False"
								Else
									If button.XFEqualsIgnoreCase("FromPriorYear") Then Return "False"
									If button.XFEqualsIgnoreCase("InYear") Then Return "False"
								End If
							End If 'time >= "2025"							
						End If 'Not scenario = ""
					Else 
						If button.XFEqualsIgnoreCase("FromPriorYear") Then Return "False"
						If button.XFEqualsIgnoreCase("InYear") Then Return "False"
					End If ' (Not sourceScenario = "")
				End If 'FunctionName					
					

#End Region

#Region "GetRollForwardSrceString"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetRollForwardSrceString, WFScenarioId=31, TextField=1)
				If args.FunctionName.XFEqualsIgnoreCase("GetRollForwardSrceString") Then
					
					Dim WFScenarioId As Integer = args.NameValuePairs("WFScenarioId")		
					Dim textField As String = args.NameValuePairs("TextField")
					If (Not WFScenarioId.ToString = "")
						Dim scenarioText As String = BRApi.Finance.Scenario.Text(si, WFScenarioId, textField)
						If Not scenarioText.Length = 0
							Dim textSplit As List(Of String) = StringHelper.SplitString(scenarioText,"|")
							Dim timeStamp As DateTime = textSplit(0)
							Dim sourceScenario As String = textSplit(1)
							Dim userName As String = textSplit(2)
					    	Dim easternzoneid As String = "Eastern Standard Time"
					    	Dim easternZone As TimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(easternZoneId)
					    	Dim convertTimeEST As String = TimeZoneInfo.ConvertTime(timeStamp, easternzone).ToString & " EST"
						
							Return "Last rolled forward from " & sourceScenario & " by " & userName & " on " & convertTimeEST
						Else 
							Return "This has not been rolled forward yet"
						End If
					Else 
						Return "This has not been rolled forward yet"
					End If 
					
				End If
#End Region

#Region "GetRollForwardSrceScenario"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetRollForwardSrceScenario, WFScenarioId=|WFScenarioId|, TextField=1)
				If args.FunctionName.XFEqualsIgnoreCase("GetRollForwardSrceScenario") Then
					
					Dim WFScenarioId As Integer = args.NameValuePairs("WFScenarioId").XFConvertToInt		
					Dim textField As String = args.NameValuePairs("TextField")
					If (Not WFScenarioId.ToString = "")
						Dim scenarioText As String = BRApi.Finance.Scenario.Text(si, WFScenarioId, textField)
						If Not scenarioText.Length = 0
							Dim textSplit As List(Of String) = StringHelper.SplitString(scenarioText,"|")
							Dim sourceScenario As String = textSplit(1)
							Return sourceScenario
						Else 
							Return "None"
						End If
					Else 
						Return "None"
					End If 
					
				End If
#End Region

#Region "GetRollForwardSrceYear"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetRollForwardSrceYear, WFScenarioId=31, TextField=1)
				If args.FunctionName.XFEqualsIgnoreCase("GetRollForwardSrceYear") Then
					
					Dim WFScenarioId As Integer = args.NameValuePairs("WFScenarioId").XFConvertToInt		
					Dim textField As String = args.NameValuePairs("TextField")
					If (Not WFScenarioId.ToString = "")
						Dim scenarioText As String = BRApi.Finance.Scenario.Text(si, WFScenarioId, textField)
						If Not scenarioText.Length = 0
							Dim textSplit As List(Of String) = StringHelper.SplitString(scenarioText,"|")
							Dim sourceScenario As String = textSplit(1)
							Dim sourceScenarioYear As String = sourceScenario.Substring(sourceScenario.Length -2, 2).XFConvertToInt + 2000
							Return sourceScenarioYear
						Else 
							Return "Root"
						End If
					Else 
						Return "Root"
					End If 
					
				End If
#End Region

#Region "GetIsBudYrRPRollforwardVisible"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetIsBudYrRPRollforwardVisible, Scenario=|WFScenario|, Time=|WFTime|)
				If args.FunctionName.XFEqualsIgnoreCase("GetIsBudYrRPRollforwardVisible") Then
					
					Dim Scenario As String = args.NameValuePairs("Scenario")
					Dim wfCurrentYear As String = args.NameValuePairs("Time")
					Dim dbToReturn As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_ScenarioManagment_Rollforward_BudYearRPData_Content_Value_ADM")
					Dim dbToReturnBlank As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_ScenarioManagment_Rollforward_BudYearRPData_Content_Blank_ADM")
					
						If (Not scenario = "")
							If wfCurrentYear >= "2025"							
								If scenario.XFContainsIgnoreCase("RAP_")
									Return dbToReturnBlank
								Else 'must be OMBJ, CJ or Enacted so return true
									Return dbToReturn
								End If
							Else
								Return dbToReturnBlank
							End If
						Else
							Return dbToReturnBlank
						End If
					
				End If
#End Region

#Region "GetPYBudgetStage" 

                'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetPYBudgetStage, PYScenario=|!prmPYScenario!|)
                If args.FunctionName.XFEqualsIgnoreCase("GetPYBudgetStage") Then
                    Dim PYScenario As String = args.NameValuePairs("PYScenario")
					If Not PYScenario = "" Then
						Dim PYScenarioLen As Integer = PYScenario.Length - 5
						Dim PYScenarioParent As String = PYScenario.Remove(PYScenarioLen)
					
						If PYScenarioParent = "OMBJ" Then
							Return "OMB Justification"
						Else If PYScenarioParent = "CJ" Then
							Return "Congressional Justification"
						Else If PYScenarioParent = "Enacted" Then
							Return "Enacted Budget"
						End If
					
					Else
						
						'Do nothing
						
					End If 
					
                End If 

#End Region 'GetPYBudgetStage

#Region "GetDashboardHelp" 

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetDashboardHelp, WFTime=2024, SelectedDB=|!prm_Content_xxx!|, Filter_Value = xxxAppropriationxxx)
				If args.FunctionName.XFEqualsIgnoreCase("GetDashboardHelp") Then
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim selectedDb As String = args.NameValuePairs("SelectedDb")
					Dim dbToReturn As String = String.Empty
					Dim Filter_Value As String = args.NameValuePairs("Filter_Value")
				
					Select Case Filter_Value
					'If user is in OS, show these Help dashboards
					Case "OS"
						Dim OSparamDict As New Dictionary(Of String, String)
						OSparamDict.Add("CreateRP_OS", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_CreateRP_OS"))
						OSparamDict.Add("EditRP_OS", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_EditRP_OS"))
						OSparamDict.Add("AddEditBillets_OS", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_AddEditBillets_OS"))
						OSparamDict.Add("AddEditNonBillets_OS", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_AddEditNonBillets_OS"))
						OSparamDict.Add("Reporting_OS", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_Reporting_OS"))
						OSparamDict.Add("ConcReview_OS", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_ConcReview_OS"))
						
						For Each dictkey As String In OSparamDict.keys
							If selectedDb.XFContainsIgnoreCase(dictkey)
								Return OSparamDict(dictkey)
							End If
						Next
												
						'BRApi.ErrorLog.LogMessage(si, "Return")
					
					'If user is in PCI, show these Help dashboards
					Case "PCI"	
						Dim PCIparamDict As New Dictionary(Of String, String)
						PCIparamDict.Add("CreateRP_PCI", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_CreateRP_PCI"))
						PCIparamDict.Add("EditRP_PCI", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_EditRP_PCI"))
						PCIparamDict.Add("AddEditExpenses_PCI", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_AddEditExpenses_PCI"))
						PCIparamDict.Add("Reporting_PCI", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_Reporting_PCI"))
						PCIparamDict.Add("ConcReview_PCI", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_ConcReview_PCI"))
						PCIparamDict.Add("History_PCI", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_History_PCI"))
						
						For Each dictkey As String In PCIparamDict.Keys
							If selectedDb.XFContainsIgnoreCase(dictkey)
								Return PCIparamDict(dictkey)
							End If
						Next
									
					'If user is in RD, show these Help dashboards
					Case "RD"		
						Dim RDparamDict As New Dictionary(Of String, String)
						RDparamDict.Add("CreateRP_RD", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_CreateRP_RD"))
						RDparamDict.Add("EditRP_RD",BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_EditRP_RD")) 
						RDparamDict.Add("AddEditExpenses_RD", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_AddEditExpenses_RD"))
						RDparamDict.Add("Reporting_RD", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_Reporting_RD"))
						RDparamDict.Add("ConcReview_RD", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_ConcReview_RD"))
						RDparamDict.Add("History_RD", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_History_RD"))
						
						For Each dictkey As String In RDparamDict.Keys
							If selectedDb.XFContainsIgnoreCase(dictkey)
								Return RDparamDict(dictkey)
							End If
						Next
								
					'If user is in RP, show these Help dashboards
					Case "RP"	
						Dim RPparamDict As New Dictionary(Of String, String)
						RPparamDict.Add("CreateRP_RP", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_CreateRP_RP"))
						RPparamDict.Add("EditRP_RP", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_EditRP_RP"))
						RPparamDict.Add("AddEditExpenses_RP", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_AddEditExpenses_RP"))
						RPparamDict.Add("Reporting_RP", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_Reporting_RP"))
						RPparamDict.Add("ConcReview_RP", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_ConcReview_RP"))
						
						For Each dictkey As String In RPparamDict.Keys
							If selectedDb.XFContainsIgnoreCase(dictkey)
								Return RPparamDict(dictkey)
							End If
						Next
						
					'If user is in MOSP, show these Help dashboards
					Case "MOSP"
						Dim MOSPparamDict As New Dictionary(Of String, String)
						MOSPparamDict.Add("CreateRP_MOSP", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_CreateRP_MOSP"))
						MOSPparamDict.Add("EditRP_MOSP", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_EditRP_MOSP"))
						MOSPparamDict.Add("AddEditExpenses_MOSP", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_AddEditExpenses_MOSP"))
						MOSPparamDict.Add("Reporting_MOSP", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_Reporting_MOSP"))
						MOSPparamDict.Add("ConcReview_MOSP", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_ConcReview_MOSP"))
						
						For Each dictkey As String In MOSPparamDict.Keys
							If selectedDb.XFContainsIgnoreCase(dictkey)
								Return MOSPparamDict(dictkey)
							End If
						Next
															
					'If user is in Funds (F), show these Help dashboards
					Case "F"
						Dim FparamDict As New Dictionary(Of String, String)
						FparamDict.Add("CreateRP_F", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_CreateRP_F"))
						FparamDict.Add("EditRP_F", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_EditRP_F"))
						FparamDict.Add("AddEditExpenses_F", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_AddEditExpenses_F"))
						FparamDict.Add("Reporting_F", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_Reporting_F"))
						FparamDict.Add("ConcReview_F", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_ConcReview_F"))
						
						For Each dictkey As String In FparamDict.Keys
							If selectedDb.XFContainsIgnoreCase(dictkey)
								Return FparamDict(dictkey)
							End If
						Next
																
					'If user is in MERHCF, show these Help dashboards
					Case "MERHCF"	
						Dim MERHCFparamDict As New Dictionary(Of String, String)
						MERHCFparamDict.Add("CreateRP_MERHCF", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_CreateRP_MERHCF"))
						MERHCFparamDict.Add("EditRP_MERHCF", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_EditRP_MERHCF"))
						MERHCFparamDict.Add("AddEditExpenses_MERHCF", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_AddEditExpenses_MERHCF"))
						MERHCFparamDict.Add("Reporting_MERHCF", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_Reporting_MERHCF"))
						MERHCFparamDict.Add("ConcReview_MERHCF", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_ConcReview_MERHCF"))
						
						For Each dictkey As String In MERHCFparamDict.Keys
							If selectedDb.XFContainsIgnoreCase(dictkey)
								Return MERHCFparamDict(dictkey)
							End If
						Next
						
					'If user is in BS, show these Help dashboards
					Case "BS"
						Dim BSparamDict As New Dictionary(Of String, String)
						BSparamDict.Add("CreateRP_BS", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_CreateRP_BS"))
						BSparamDict.Add("EditRP_BS", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_EditRP_BS"))
						BSparamDict.Add("AddEditExpenses_BS", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_AddEditExpenses_BS"))
						BSparamDict.Add("Reporting_BS", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_Reporting_BS"))
						BSparamDict.Add("ConcReview_BS", BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Content_Help_ConcReview_BS"))
						
						For Each dictkey As String In BSparamDict.Keys
							If selectedDb.XFContainsIgnoreCase(dictkey)
								Return BSparamDict(dictkey)
							End If
						Next
						
						Return dbToReturn
					End Select
				End If
								
#End Region 'GetDashboardHelp

#Region "GetRelatedRPStatus"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetRelatedRPStatus, RPName=|!prm_FYRelatedRpX_RP!|)
				If args.FunctionName.XFEqualsIgnoreCase("GetRelatedRPStatus") Then
					
					Dim rpName As String = args.NameValuePairs("RPName")
					If (Not rpName = "")
						Return rpUtils.Get_RP_Status_Description(si, rpName) 
					Else 
						Return ""
						
					End If 'RPName = ""
					
				End If
#End Region 

#Region "GetOlderRelatedRPStatus"			

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetOlderRelatedRPStatus, RPName=|!prm_OlderRelatedRpX_RP!|)
				If args.FunctionName.XFEqualsIgnoreCase("GetOlderRelatedRPStatus") Then
					
					'Get Older Related RP Name
					Dim rpName As String = args.NameValuePairs("RPName")
					If (String.IsNullOrEmpty(rpName)) Then Return String.Empty
					
					'Get TimeID
					Dim budYearYYYY As String = rpUtils.Get_RP_Budget_Year(si, rpName)
					Dim OlderRPTimeid As Integer = BRApi.Finance.Members.GetMemberId(si, DimTypeId.Time, budYearYYYY)
		
					'Get budget year from Older RP field
					Dim budYear As String = rpUtils.Get_RP_Budget_Year_YY(si, rpName)
					
					'Get working scenario using literal parameter
					Dim workingScenario As String = "WorkScen_FY" + budYear										
					Dim workScen As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, workingScenario)
					Dim WorkScenarioId As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, workScen)
					Dim WorkScenarioTypeId As Integer = BRApi.Finance.Scenario.GetScenarioType(si, WorkScenarioId).Id

					'Get Text 1 Value from RP, get member ID for budYear
					Dim flowRPMemID As Integer = BRApi.Finance.Members.GetMemberId(si, dimtypeid.Flow, rpName)
										
					Dim flowText1 As String = BRApi.Finance.Flow.Text(si, flowRPMemID, 1, WorkScenarioTypeId, OlderRPTimeid)
					If (String.IsNullOrEmpty(flowText1)) Then Return "No status set, please contact administrator."
					Dim flowText1Split() As String = flowText1.Split("|")
				
					'Get Older RP Status Description
					Dim olderRelatedStatus As String = flowText1Split(0)
					Dim Get_olderRP_Status_Description As String = BRApi.Finance.Members.GetMemberInfo(si, dimtypeId.UD8, olderRelatedStatus).Description
						
						Return Get_olderRP_Status_Description
				End If
#End Region 

#Region "GetPPAFromRPName"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetPPAFromRPName, RPName=|!prm_Number_PCI!|)
				
				If args.FunctionName.XFEqualsIgnoreCase("GetPPAFromRPName") Then
					Dim returnValue As String = String.Empty
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					Dim WFTime As String = args.NameValuePairs.XFGetValue("WFTime")
					Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,dimtypeid.Time, WFTime)
					
					If (Not String.IsNullOrEmpty(RPName)) Then			
						'Remove the first three digits from the RP as the should be YY_ and that should give you the PPA
						Dim PPA As String = RPName.Substring(3, RPName.Length - 3)
						
						'Get the memberId from the name
						Dim PPAMemberID As Integer = BRApi.Finance.Members.GetMemberInfo(si, dimtypeid.UD1, PPA).Member.MemberId
						Dim objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "Std_PPA")
						Dim PPAMemberHasChildren As Boolean = BRApi.Finance.Members.HasChildren(si, objDimPk, PPAMemberID)
						
						'If the member has children, return a member filter with the children, else return the member itself
						If PPAMemberHasChildren 
							
							Dim UD1MbrList As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Std_PPA",  "U1#" & PPA & ".Children", True)
							Dim loopCounter As Integer = 0
							
							For Each ud1mbr As MemberInfo In UD1MbrList
								
								Dim Ud1mmbrname As String = ud1mbr.Member.Name
								Dim Ud1mmbrId As Integer = BRApi.Finance.Members.GetMemberId(si,dimtypeid.UD1, Ud1mmbrname)
								Dim bValue As Boolean = BRApi.Finance.UD.InUse(si, dimTypeId.UD1, Ud1mmbrId, DimConstants.Unknown, wfTimeId)
								
								If bValue Then
									If loopCounter = 0 Then
										returnValue = "U1#" & Ud1mmbrname
									Else
										returnValue = returnValue & "," & "U1#" & Ud1mmbrname
									End If 
								Else
									'do nothing
								End If 
								
								loopCounter+=1
								
							Next
							
						Else 
							returnValue = "U1#" & PPA
						End If
					End If
					
					Return returnValue
					
				End If

#End Region
		
#Region "GetPPAFromRPNameWODimToken"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetPPAFromRPNameWODimToken, RPName=|!prm_Number_RD!|)
				
				If args.FunctionName.XFEqualsIgnoreCase("GetPPAFromRPNameWODimToken") Then
					Dim returnValue As String = String.Empty
					Dim RPName As String = args.NameValuePairs.XFGetValue("RPName")
					
					If (Not String.IsNullOrEmpty(RPName)) Then			
						'Remove the first three digits from the RP as the should be YY_ and that should give you the PPA
						Dim PPA As String = RPName.Substring(3, RPName.Length - 3)
						
						'Get the memberId from the name
						Dim PPAMemberID As Integer = BRApi.Finance.Members.GetMemberInfo(si, dimtypeid.UD1, PPA).Member.MemberId
						Dim objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "Std_PPA")
						Dim PPAMemberHasChildren As Boolean = BRApi.Finance.Members.HasChildren(si, objDimPk, PPAMemberID)
						
						'If the member has children, return a member filter with the children, else return the member itself
						If PPAMemberHasChildren 
							returnValue = PPA & ".Children"
						Else 
							returnValue = PPA
						End If
					End If
					
					Return returnValue
				End If

#End Region

#Region "GetCurrentWorkingScenario"
				
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetCurrentWorkingScenario, WFTime=|WFTime|, Filter=BYScen)
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetCurrentWorkingScenario, WFTime=|WFTime|, Filter=BYMinusOne)
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetCurrentWorkingScenario, WFTime=|WFTime|, Filter=BYMinusTwo)
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetCurrentWorkingScenario, WFTime=|WFTime|, Filter=BYPlusOne)
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetCurrentWorkingScenario, WFTime=|WFTime|, Filter=BYPlusTwo)
				If args.FunctionName.XFEqualsIgnoreCase("GetCurrentWorkingScenario") Then
					
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim filter As String = args.NameValuePairs("Filter")
					
				Select Case Filter
				
				'If Working Scenario is the Budget Year (BY), return the working scenario of that year.
				Case "BYScen"
					Dim currScenYear As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "CurrentScenarioYear")
					currScenYear = wfTime.Substring(2)
					Dim currScen As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "WorkScen_FY" & currScenYear)
						Return currScen	
								
				'If Scenario is BY minus one year (BY-1), return the working scenario of that year minus 1.
				Case "BYMinusOne"
					Dim currScenYear As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "CurrentScenarioYear")
					currScenYear = wfTime.Substring(2)
					Dim currScenYearMinusOne As String = (currScenYear.XFConvertToInt - 1).ToString
					Dim currScenMinusOne As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "WorkScen_FY" & currScenYearMinusOne)
						Return currScenMinusOne
				
				'If Scenario is BY minus two years (BY-2), return the working scenario of that year minus 2.
				Case "BYMinusTwo"
					Dim currScenYear As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "CurrentScenarioYear")
					currScenYear = wfTime.Substring(2)
					Dim currScenYearMinusTwo As String = (currScenYear.XFConvertToInt - 2).ToString
					Dim currScenMinusTwo As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "WorkScen_FY" & currScenYearMinusTwo)
						Return currScenMinusTwo
				
				'If Scenario is BY plus one year (BY-1), return the working scenario of that year plus 1.
				Case "BYPlusOne"
					Dim currScenYear As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "CurrentScenarioYear")
					currScenYear = wfTime.Substring(2)
					Dim currScenYearPlusOne As String = (currScenYear.XFConvertToInt + 1).ToString
					Dim currScenPlusOne As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "WorkScen_FY" & currScenYearPlusOne)
						Return currScenPlusOne
					
				'If Scenario is BY plus two years (BY-2), return the working scenario of that year plus 2.	
				Case "BYPlusTwo"
					Dim currScenYear As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "CurrentScenarioYear")
					currScenYear = wfTime.Substring(2)
					Dim currScenYearPlusTwo As String = (currScenYear.XFConvertToInt + 2).ToString
					Dim currScenPlusTwo As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "WorkScen_FY" & currScenYearPlusTwo)
						Return currScenPlusTwo

				End Select	

				End If				

#End Region

#Region "GetCurrentWorkingScenarioOptions"
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper_MSN, GetCurrentWorkingScenarioOptions, WFTime=|WFTime|, Filter=BYScen)
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper_MSN, GetCurrentWorkingScenarioOptions, WFTime=|WFTime|, Filter=BYMinusOne)
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper_MSN, GetCurrentWorkingScenarioOptions, WFTime=|WFTime|, Filter=BYMinusTwo)
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper_MSN, GetCurrentWorkingScenarioOptions, WFTime=|WFTime|, Filter=BYPlusOne)
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper_MSN, GetCurrentWorkingScenarioOptions, WFTime=|WFTime|, Filter=BYPlusTwo)
				If args.FunctionName.XFEqualsIgnoreCase("GetCurrentWorkingScenarioOptions") Then
					
					Dim wfTime As String = args.NameValuePairs("WFTime")
					Dim filter As String = args.NameValuePairs("Filter")
					
				Select Case Filter
				Case "BYScen"
					Dim currScenYear As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "CurrentScenarioYear")
					currScenYear = wfTime.Substring(2)
					Dim currScen As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "WorkScen_FY" & currScenYear)
					Dim ScenarioMemberFilterScript As String  = ""
						
						If currScen.XFContainsIgnoreCase("RAP_") Then
							ScenarioMemberFilterScript = "S#OMBJ_FY" & currScenYear & ", S#CJ_FY" & currScenYear & ", S#Enacted_FY" & currScenYear
						Else If currScen.XFContainsIgnoreCase("OMBJ_") Then
							ScenarioMemberFilterScript = "S#RAP_FY" & currScenYear & ", S#CJ_FY" & currScenYear & ", S#Enacted_FY" & currScenYear
						Else If currScen.XFContainsIgnoreCase("CJ_") Then
							ScenarioMemberFilterScript = "S#RAP_FY" & currScenYear & ", S#OMBJ_FY" & currScenYear & ", S#Enacted_FY" & currScenYear
						Else If currScen.XFContainsIgnoreCase("Enacted_")
							ScenarioMemberFilterScript = "S#RAP_FY" & currScenYear & ", S#OMBJ_FY" & currScenYear & ", S#CJ_FY" & currScenYear
						Else If currScen.XFContainsIgnoreCase("Unknown")
								ScenarioMemberFilterScript = "Unknown"
						End If
					Return ScenarioMemberFilterScript	
										
						
					Case "BYMinusOne"
						Dim currScenYear As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "CurrentScenarioYear")
						currScenYear = wfTime.Substring(2)
						Dim currScenYearMinusOne As String = (currScenYear.XFConvertToInt - 1).ToString
						Dim currScenMinusOne As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "WorkScen_FY" & currScenYearMinusOne)
						Dim ScenarioMemberFilterScript As String  = ""
							If currScenMinusOne.XFContainsIgnoreCase("RAP_") Then
								ScenarioMemberFilterScript = "S#OMBJ_FY" & currScenYearMinusOne & ", S#CJ_FY" & currScenYearMinusOne & ", S#Enacted_FY" & currScenYearMinusOne
							Else If currScenMinusOne.XFContainsIgnoreCase("OMBJ_") Then
								ScenarioMemberFilterScript = "S#RAP_FY" & currScenYearMinusOne & ", S#CJ_FY" & currScenYearMinusOne & ", S#Enacted_FY" & currScenYearMinusOne
							Else If currScenMinusOne.XFContainsIgnoreCase("CJ_") Then
								ScenarioMemberFilterScript = "S#RAP_FY" & currScenYearMinusOne & ", S#OMBJ_FY" & currScenYearMinusOne & ", S#Enacted_FY" & currScenYearMinusOne
							Else If currScenMinusOne.XFContainsIgnoreCase("Enacted_")
								ScenarioMemberFilterScript = "S#RAP_FY" & currScenYearMinusOne & ", S#OMBJ_FY" & currScenYearMinusOne & ", S#CJ_FY" & currScenYearMinusOne
							Else If currScenMinusOne.XFContainsIgnoreCase("Unknown")
								ScenarioMemberFilterScript = "Unknown"
							End If
					Return ScenarioMemberFilterScript
					
					
					Case "BYMinusTwo"
						Dim currScenYear As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "CurrentScenarioYear")
						currScenYear = wfTime.Substring(2)
						Dim currScenYearMinusTwo As String = (currScenYear.XFConvertToInt - 2).ToString
						Dim currScenMinusTwo As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "WorkScen_FY" & currScenYearMinusTwo)
						Dim ScenarioMemberFilterScript As String  = ""
							If currScenMinusTwo.XFContainsIgnoreCase("RAP_") Then
								ScenarioMemberFilterScript = "S#OMBJ_FY" & currScenYearMinusTwo & ", S#CJ_FY" & currScenYearMinusTwo & ", S#Enacted_FY" & currScenYearMinusTwo
							Else If currScenMinusTwo.XFContainsIgnoreCase("OMBJ_") Then
								ScenarioMemberFilterScript = "S#RAP_FY" & currScenYearMinusTwo & ", S#CJ_FY" & currScenYearMinusTwo & ", S#Enacted_FY" & currScenYearMinusTwo
							Else If currScenMinusTwo.XFContainsIgnoreCase("CJ_") Then
								ScenarioMemberFilterScript = "S#RAP_FY" & currScenYearMinusTwo & ", S#OMBJ_FY" & currScenYearMinusTwo & ", S#Enacted_FY" & currScenYearMinusTwo
							Else If currScenMinusTwo.XFContainsIgnoreCase("Enacted_")
								ScenarioMemberFilterScript = "S#RAP_FY" & currScenYearMinusTwo & ", S#OMBJ_FY" & currScenYearMinusTwo & ", S#CJ_FY" & currScenYearMinusTwo
							Else If currScenMinusTwo.XFContainsIgnoreCase("Unknown")
								ScenarioMemberFilterScript = "Unknown"	
							End If
					Return ScenarioMemberFilterScript	
					
					
					Case "BYPlusOne"
						Dim currScenYear As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "CurrentScenarioYear")
						currScenYear = wfTime.Substring(2)
						Dim currScenYearPlusOne As String = (currScenYear.XFConvertToInt + 1).ToString
						Dim currScenPlusOne As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "WorkScen_FY" & currScenYearPlusOne)
						Dim ScenarioMemberFilterScript As String  = ""
							If currScenPlusOne.XFContainsIgnoreCase("RAP_") Then
								ScenarioMemberFilterScript = "S#OMBJ_FY" & currScenYearPlusOne & ", S#CJ_FY" & currScenYearPlusOne & ", S#Enacted_FY" & currScenYearPlusOne
							Else If currScenPlusOne.XFContainsIgnoreCase("OMBJ_") Then
								ScenarioMemberFilterScript = "S#RAP_FY" & currScenYearPlusOne & ", S#CJ_FY" & currScenYearPlusOne & ", S#Enacted_FY" & currScenYearPlusOne
							Else If currScenPlusOne.XFContainsIgnoreCase("CJ_") Then
								ScenarioMemberFilterScript = "S#RAP_FY" & currScenYearPlusOne & ", S#OMBJ_FY" & currScenYearPlusOne & ", S#Enacted_FY" & currScenYearPlusOne
							Else If currScenPlusOne.XFContainsIgnoreCase("Enacted_")
								ScenarioMemberFilterScript = "S#RAP_FY" & currScenYearPlusOne & ", S#OMBJ_FY" & currScenYearPlusOne & ", S#CJ_FY" & currScenYearPlusOne
							Else If currScenPlusOne.XFContainsIgnoreCase("Unknown")
								ScenarioMemberFilterScript = "Unknown"
							End If
					Return ScenarioMemberFilterScript					

					
					Case "BYPlusTwo"
						Dim currScenYear As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "CurrentScenarioYear")
						currScenYear = wfTime.Substring(2)
						Dim currScenYearPlusTwo As String = (currScenYear.XFConvertToInt + 2).ToString
						Dim currScenPlusTwo As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "WorkScen_FY" & currScenYearPlusTwo)
						Dim ScenarioMemberFilterScript As String  = ""
							If currScenPlusTwo.XFContainsIgnoreCase("RAP_") Then
								ScenarioMemberFilterScript = "S#OMBJ_FY" & currScenYearPlusTwo & ", S#CJ_FY" & currScenYearPlusTwo & ", S#Enacted_FY" & currScenYearPlusTwo
							Else If currScenPlusTwo.XFContainsIgnoreCase("OMBJ_") Then
								ScenarioMemberFilterScript = "S#RAP_FY" & currScenYearPlusTwo & ", S#CJ_FY" & currScenYearPlusTwo & ", S#Enacted_FY" & currScenYearPlusTwo
							Else If currScenPlusTwo.XFContainsIgnoreCase("CJ_") Then
								ScenarioMemberFilterScript = "S#RAP_FY" & currScenYearPlusTwo & ", S#OMBJ_FY" & currScenYearPlusTwo & ", S#Enacted_FY" & currScenYearPlusTwo
							Else If currScenPlusTwo.XFContainsIgnoreCase("Enacted_")
								ScenarioMemberFilterScript = "S#RAP_FY" & currScenYearPlusTwo & ", S#OMBJ_FY" & currScenYearPlusTwo & ", S#CJ_FY" & currScenYearPlusTwo
							Else If currScenPlusTwo.XFContainsIgnoreCase("Unknown")
								ScenarioMemberFilterScript = "Unknown"
							End If
					Return ScenarioMemberFilterScript						
					
				End Select
				End If

#End Region

#Region "GetRFBaselineCalcYears" 

                'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetRFBaselineCalcYears, WFYear=|WFYear|)
                If args.FunctionName.XFEqualsIgnoreCase("GetRFBaselineCalcYears") Then
                    Dim wfYear As String = args.NameValuePairs("WFYear")
					Dim wfYearNext1 As String = (wfYear.XFConvertToInt + 1).ToString
					Dim wfYearNext2 As String = (wfYear.XFConvertToInt + 2).ToString
					Dim wfYearNext3 As String = (wfYear.XFConvertToInt + 3).ToString
					Dim wfYearNext4 As String = (wfYear.XFConvertToInt + 4).ToString
					Dim appStartYear As String = "2020"
					Dim timeFilter As New Text.StringBuilder
					
					For i As Integer = appStartYear To wfYear
						timefilter.Append("T#" & i & ",")
					Next 
					
					timefilter.Append("T#" & wfYearNext1 & ",")
					timefilter.Append("T#" & wfYearNext2 & ",")
					timefilter.Append("T#" & wfYearNext3 & ",")
					timefilter.Append("T#" & wfYearNext4)
					
					Return timefilter.ToString
					
                End If 

#End Region 'GetPYBudgetStage

#Region "GetRPCompletionStatus"
		'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper_MSN, GetRPCompletionStatus, RPName=|!prm_Number_OS!|, CompletionStatusColor=completionStatusColor)
		If args.FunctionName.XFEqualsIgnoreCase("GetRPCompletionStatus") Then
				
				Dim rpName As String = args.NameValuePairs("RPName")
				If (Not rpName = "")
					
						Dim attributeValueDataAttachmentList As DataAttachmentList = BRApi.Finance.Data.GetDataAttachments(si, "F#" & rpName & ":A#RPCompleteness", False)
						Dim attributeValue As String = String.Empty
						
						For Each attributeValueDataAttachment As DataAttachment In attributeValueDataAttachmentList.Items
							attributeValue = attributeValueDataAttachment.Text
							'brapi.ErrorLog.LogMessage(si, "MSN made it here--item=" + attributeValueDataAttachment.UniqueID.ToString + "---value=" + attributeValue)
						Next
						
						Dim completionStatusColor As String = String.Empty
						If attributeValue.XFEqualsIgnoreCase("Complete")
							completionStatusColor = XFColors.XFWorkflowCompleted.Name
						
						Else If attributeValue.XFEqualsIgnoreCase("Incomplete")
							completionStatusColor = XFColors.Red.Name

						Else If attributeValue.XFEqualsIgnoreCase("Not Calculated")
							completionStatusColor = XFColors.Yellow.Name

						End If
					Return completionStatusColor
				End If
			End If
#End Region

#Region "GetTAFName"

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetTAFName, MFUD8=|MFUD8|, Year=|WFYearPrior2|, Parent=Total_FundYrs_Rem_3YrFunds)
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetTAFName, MFUD8=|MFUD8|, Year=|WFYearPrior2|, Parent=Total_FundYrs_Rem_5YrFunds)
				If args.FunctionName.XFEqualsIgnoreCase("GetTAFName") Then
					
					Dim mfUD8 As String = args.NameValuePairs("MFUD8")					
					Dim year As String = args.NameValuePairs("Year")	
					Dim parent As String = args.NameValuePairs("Parent")
					Dim yearYY As Integer = year.Substring(2,2).XFConvertToInt()
					Dim yearYYPrior4 As Integer = yearYY-4
					Dim yearYYPrior3 As Integer = yearYY-3
					Dim yearYYPrior2 As Integer = yearYY-2
					Dim yearYYPrior1 As Integer = yearYY-1
					Dim yearYYNext1 As Integer = yearYY+1
					Dim yearYYNext2 As Integer = yearYY+2
					Dim yearYYNext3 As Integer = yearYY+3
					Dim yearYYNext4 As Integer = yearYY+4
					Dim yearYYNext5 As Integer = yearYY+5
					
					Dim fundRem_00Description As String = String.Empty
					Dim fundRem_01Description As String = String.Empty
					Dim fundRem_02Description As String = String.Empty	
					Dim fundRem_03Description As String = String.Empty	
					Dim fundRem_04Description As String = String.Empty				
											
					If parent.XFEqualsIgnoreCase("Total_FundYrs_Rem_3YrFunds")								
						fundRem_00Description = yearYYPrior2 & "/" & yearYY & "_TAF "
						fundRem_01Description = yearYYPrior1 & "/" & yearYYNext1 & "_TAF "
						fundRem_02Description = yearYY & "/" & yearYYNext2 & "_TAF "							
						
					Else If parent.XFEqualsIgnoreCase("Total_FundYrs_Rem_5YrFunds")							
						fundRem_00Description = yearYYPrior4 & "/" & yearYY & "_TAF "
						fundRem_01Description = yearYYPrior3 & "/" & yearYYNext1 & "_TAF "
						fundRem_02Description = yearYYPrior2 & "/" & yearYYNext2 & "_TAF "
						fundRem_03Description = yearYYPrior1 & "/" & yearYYNext3 & "_TAF "
						fundRem_04Description = yearYY & "/" & yearYYNext4 & "_TAF "	
						
					End If
						
					If (Not mfUD8 = "")						
							If (mfUD8.XFEqualsIgnoreCase("Total_FundYrs_Rem_3YrFunds") Or mfUD8.XFEqualsIgnoreCase("Total_FundYrs_Rem_5YrFunds"))
								Return "Total TAFS"
							Else If mfUD8.XFContainsIgnoreCase("FundRem_00")
								Return fundRem_00Description
							Else If mfUD8.XFContainsIgnoreCase("FundRem_01")
								Return fundRem_01Description
							Else If mfUD8.XFContainsIgnoreCase("FundRem_02")
								Return fundRem_02Description
							Else If mfUD8.XFContainsIgnoreCase("FundRem_03")
								Return fundRem_03Description
							Else If mfUD8.XFContainsIgnoreCase("FundRem_04")
								Return fundRem_04Description
							End If
					Else 
						Return ""
						
					End If 'RPName = ""
					
				End If
#End Region

#Region "GetRDWorkScensAndDollars"
	'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetRDWorkScensAndDollars, RPName=|!prm_Number_RD!|, Filter=PriorTwoScenario)
	'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetRDWorkScensAndDollars, RPName=|!prm_Number_RD!|, Filter=PriorTwoDollars)
	'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetRDWorkScensAndDollars, RPName=|!prm_Number_RD!|, Filter=PriorOneScenario)
	'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetRDWorkScensAndDollars, RPName=|!prm_Number_RD!|, Filter=PriorOneDollars)
	'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetRDWorkScensAndDollars, RPName=|!prm_Number_RD!|, Filter=CurrScenario)
	'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetRDWorkScensAndDollars, RPName=|!prm_Number_RD!|, Filter=CurrDollars)
	If args.FunctionName.XFEqualsIgnoreCase("GetRDWorkScensAndDollars") Then
		
		Dim rpName As String = args.NameValuePairs.XFGetValue("RPName")
		'Get appropriation From RP Name
		Dim approp As String = rpName.Substring(3, rpName.Length-3)
		Dim filter As String = args.NameValuePairs("Filter")
		Dim wfTime As String = args.SubstVarSourceInfo.WFTime.Name
		Dim wfCube As String = args.SubstVarSourceInfo.WFCube
		Dim priorTwoTime As String = (wftime.XFConvertToInt() -2).ToString
		Dim priorOneTime As String = (wftime.XFConvertToInt() -1).ToString
		Dim timeYY As String = wfTime.Substring(2,2).XFConvertToInt
		Dim priorTwoTimeYY As String = (timeYY.XFConvertToInt() -2).ToString
		Dim priorOneTimeYY As String = (timeYY.XFConvertToInt() -1).ToString
		Dim wfScenario As String = args.SubstVarSourceInfo.WFScenario.Name
		Dim workScenPriorTwo As String = brapi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "WorkScen_FY" & priorTwoTimeYY)
		Dim workScenPriorOne As String = brapi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "WorkScen_FY" & priorOneTimeYY)
										
		'Declare the format string for dollars
		Dim FormatString As String = "$#,##0,."		
		Dim rp_Entity = rpUtils.Get_RP_Entity(si, rpName)
		
		Dim retValue As String = String.Empty		
		
		Select Case filter
		Case "PriorTwoScenario"
			retValue = workScenPriorTwo
		Case "PriorTwoDollars"
			retValue = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "E#" & RP_Entity & ":S#" & workScenPriorTwo & ":T#" & priorTwoTime & ":V#Periodic:A#Funding:F#Total_Flow:O#Top:I#Top:U1#" & approp &":U2#Total_Investment:U3#Total_ObjectClass:U4#Total_ATU:U5#Total_CostLine:U6#Total_Expense_LineItems:U7#None:U8#None").DataCellEx.DataCell.CellAmount.ToString(FormatString)
		Case "PriorOneScenario"
			retValue = workScenPriorOne
		Case "PriorOneDollars"
			retValue = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "E#" & RP_Entity & ":S#" & workScenPriorOne & ":T#" & priorOneTime & ":V#Periodic:A#Funding:F#Total_Flow:O#Top:I#Top:U1#" & approp &":U2#Total_Investment:U3#Total_ObjectClass:U4#Total_ATU:U5#Total_CostLine:U6#Total_Expense_LineItems:U7#None:U8#None").DataCellEx.DataCell.CellAmount.ToString(FormatString)
		Case "CurrScenario"
			retValue = wfScenario
		Case "CurrDollars"
			retValue = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Periodic:A#Funding:F#Total_Flow:O#Top:I#Top:U1#" & approp &":U2#Total_Investment:U3#Total_ObjectClass:U4#Total_ATU:U5#Total_CostLine:U6#Total_Expense_LineItems:U7#None:U8#None").DataCellEx.DataCell.CellAmount.ToString(FormatString)
		End Select
		
		Return retValue	
		
	End If
	
#End Region

#Region "GetPCIUD2Text1"

	'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetPCIUD2Text1, Ud1PPA=[|!prm_EXP_PPA_Selection_PCI!|])
	If args.FunctionName.XFEqualsIgnoreCase("GetPCIUD2Text1") Then

		Dim investmentList As New List (Of String)

		Dim objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "Std_Investment")
		Dim costEstInvestmntInfos As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk,"U2#CostEstimate_Investments.base", True)
		Dim ud1PPA As String = args.NameValuePairs("Ud1PPA")

		For Each Investment As MemberInfo In costEstInvestmntInfos
			
			Dim memberID As Integer = Investment.Member.MemberId
			Dim investmentTextValue As String = BRApi.Finance.UD.Text(si, dimTypeId.UD2, MemberId, 1, DimConstants.Unknown, DimConstants.Unknown)

			investmentTextValue = investmentTextValue.Replace(" ", "")
				
			Dim investmentTextValueSplit() As String = investmentTextValue.split(",")
			For Each ud1Mem As String In investmentTextValueSplit
			
				If ud1Mem = ud1ppa Then
					investmentList.Add("U2#" & Investment.Member.Name)
				End If
			Next
		Next
		Dim uiiAllocDefaults As String = String.Join(",", investmentList)
		
		Return uiiAllocDefaults

	End If
#End Region

#Region "GetHomePageScenarioandTime"
						'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetHomePageScenarioandTime, Filter=Scenario, WFTime=|WFTime|)
						'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetHomePageScenarioandTime, Filter=Time, WFTime=|WFTime|)
						If args.FunctionName.XFEqualsIgnoreCase("GetHomePageScenarioandTime") Then
							
							Dim filter As String = args.NameValuePairs("Filter")
							Dim scenarioToReturn As String = String.Empty
							Dim timeToReturn As String = String.Empty
							Dim wfTime As String = args.NameValuePairs("WFTime")
							Dim workingScenario As String = "WorkScen_FY" + wfTime.Substring(2,2)
							
							If BRApi.Security.Authorization.IsUserInGroup(si, "USCG_FERBE_BudFm_r_Auditor") Then
								
								Dim budExScenario As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, workingScenario)
								scenarioToReturn = budExScenario
								timeToReturn = wfTime
								
							ElseIf BRApi.Security.Authorization.IsUserInGroup(si, "USCG_FERBE_BudFm_r_OfficeUser") Then
								
								Dim budFmScenario As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, workingScenario)
								scenarioToReturn = budFmScenario
							    timeToReturn = wfTime
								
							End If
							
							If filter.XFEqualsIgnoreCase("Time")
								Return timeToReturn
							Else If filter.XFEqualsIgnoreCase("Scenario")
								Return scenarioToReturn
							End If
							
						End If
			

#End Region

#Region "CheckIfPwrUsr" 

				'called from component object: "prm_Billets_Update_CheckIfPwrUsr_OS"
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, CheckIfPwrUsr, FilterValue=DashboardName)
				
				'called from component object: "Embedded OS_Billets_Main_04c1c"
				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, CheckIfPwrUsr, FilterValue=TabDescription)
				
				If args.FunctionName.XFEqualsIgnoreCase("CheckIfPwrUsr") Then
					Dim FilterValue As String = args.NameValuePairs("FilterValue")
					Dim PwrUsr As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Billets_Update_PwrUsr_OS")
					Dim RegUsr As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Billets_Update_RegUsr_OS")
					Brapi.ErrorLog.LogMessage(si, "Code Ran")
					If BRApi.Security.Authorization.IsUserInGroup(si, "USCG_FERBE_BudFm_r_PowerUser") Then
						'BRApi.ErrorLog.LogMessage(si, "DZ--POWER-USER--" + FilterValue)
						If FilterValue = "DashboardName" Then
							Return PwrUsr
						Else
							Return "Update All Billets"
						End If
					Else
						'BRApi.ErrorLog.LogMessage(si, "DZ--REG-USER--" + FilterValue)
						If FilterValue = "DashboardName" Then
							Return RegUsr
							
						Else
							Return " "
						End If
					End If
					
				End If
								
#End Region 'CheckIfPwrUsr

#Region "ScenarioOSDropdown"
'					********************************Change Log*******************
'					Created: 7/30/24 - PF- DHSUSCG-1867 - Created BR to display Scenario Members based on prior year and current year
'					8/23/24 - PF - DHSUSCG-1916 - Added Security Functionality to display scenarios based on the User's Security Group access
					If args.FunctionName.XFEqualsIgnoreCase("ScenarioOSDropdown") Then
						
							Dim myWorkflowUnitPk As WorkflowUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si)
							Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si, myWorkflowUnitPk.TimeKey)
							Dim wfYear As Integer = BRApi.Finance.Time.GetYearFromId(si, myWorkflowUnitPk.TimeKey)
							Dim PriorYear As Integer = wfYear - 1
							Dim futureYear As Integer = wfYear + 1
							Dim currentyearsplit As String = wfTime.Substring(2,2)
							Dim prioryearsplit As String = PriorYear.ToString.Substring(2,2)
							Dim futureyearsplit As String = futureYear.ToString.Substring(2,2)
							Dim scenariowmf As String = "S#RAP_FY" & prioryearsplit & ",S#RAP_FY" & currentyearsplit & ",S#RAP_FY" & futureyearsplit & ",S#OMBJ_FY" & prioryearsplit & ",S#OMBJ_FY" & currentyearsplit & ",S#OMBJ_FY" & futureyearsplit & ",S#CJ_FY" & prioryearsplit & ",S#CJ_FY" & currentyearsplit & ",S#CJ_FY" & futureyearsplit & ",S#Enacted_FY" & prioryearsplit & ",S#Enacted_FY" & currentyearsplit & ",S#Enacted_FY" & futureyearsplit
							
							Dim returnstring As String = " "
							Dim scenariolist As List(Of String) = scenariowmf.Split(",").ToList()
'															
'							Iterate through the list of scenario in scenariolist
							For Each scenario In scenariolist
								
								Dim Value() As String = "S#RAP,S#OMBJ,S#CJ,S#PB_Enacted".Split(",")
'								Retrieve values based on member filter and convert values based on member name
								Dim TrueList As List(Of String) = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"BudFm_Scenario", Value(0) & ".Base, " & Value(1) & ".Base," & Value(2) & ".Base," & Value(3) & ".Base" ,True).ConvertAll(Function(x) x.Member.Name)
'								Split value of scenario string to grab the scenario name without the S#
								Dim Splitvalue As String = scenario.Substring(2)
								
'								If Member Name is in truelist this will grab the members read data group id and check to see if the user is in the read data group of that scenario
								If TrueList.Contains(SplitValue)
									Dim newGuid As Guid = BRApi.Finance.Members.GetMember(si, 2, SplitValue).ReadDataGroupUniqueID
'									If the user is part of that group then we we add the scenario to a retrun string based on if the returnstring is empty...Else we continue for
									If BRApi.Security.Authorization.IsUserInGroup(si, newGuid) = True
										If returnstring = " "
											returnstring &= scenario
										Else
											returnstring &= "," & scenario
										End If
									Else
										Continue For
									End If
								Else
									Continue For
								End If
							Next
							
							Dim returnstringlst As List(Of String) = returnstring.Split(",").ToList()
							Dim scenariolst As String = " "
							
							For Each rtrnscenario In returnstringlst
								
								Dim scenYear As String = rtrnscenario.Substring((rtrnscenario.Length-2),2)
								
								If scenYear >= 26 Then
								
									scenariolst &= "," & rtrnscenario
							
								End If 
							
							Next
							
							
							Return scenariolst
																		
						End If

#End Region

#Region  "Get Time Filter For Copy Attributes"
			If args.FunctionName.XFEqualsIgnoreCase("GetTimeFilterCopyAttributes") Then
			
			'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetTimeFilterCopyAttributes, SourceRP = |!SourceRPName!|, WFTime = |WFTime|)
				Dim sourceRPName As String = args.NameValuePairs("SourceRP")
				Dim wfTime As String = args.NameValuePairs("WFTime")
				
				'Translating two-digit string year into integer year
				Dim sourceRPYear As Integer = CInt("20" & SourceRPName.Substring(0,2))
				Dim wfYear As Integer = CInt(wfTime)
				Dim timeFilter As New Text.StringBuilder
				
				If (wfYear - sourceRPYear = 1) Then
					
				    Dim timeString As String  = "T#" & sourceRPYear + 1 & "," & "T#" & sourceRPYear + 2 & "," & "T#" & sourceRPYear + 3 & "," & "T#" & sourceRPYear + 4 & "," & "T#" & sourceRPYear + 5
					Return timeString
					
				Else If (wfYear - sourceRPYear = -1) Then
					
					Dim timeString As String  = "T#" & sourceRPYear - 1 & "," & "T#" & sourceRPYear & "," & "T#" & sourceRPYear + 1 & "," & "T#" & sourceRPYear + 2 & "," & "T#" & sourceRPYear + 3
					Return timeString
					
				Else If sourceRPYear = wfYear Then
					
					Dim wfYearNext1 As String = "T#|WFTime|"
					Dim wfYearNext2 As String = "T#YearNext1(|WFTime|)" 
					Dim wfYearNext3 As String = "T#YearNext2(|WFTime|)"
					Dim wfYearNext4 As String = "T#YearNext3(|WFTime|)" 
					Dim wfYearNext5 As String = "T#YearNext4(|WFTime|)" 
						
					timefilter.Append( wfYearNext1 & "," & wfYearNext2 & "," & wfYearNext3 & "," & wfYearNext4 & "," & wfYearNext5)

					Return timefilter.ToString()
					
					
					
			   End If
			
				
				
			End If
			

#End Region

#Region "GetModInfoTable" 

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetModInfoTable, WFYear= [|WFTime|]) 
				If args.FunctionName.XFEqualsIgnoreCase("GetModInfoTable") Then
					Dim wfTime As String = args.NameValuePairs("WFTime")
					
					If wfTime >= 2027 Then 
						Return "BDF_Mod_ModInformation_2027"
					Else
						Return "BDF_Mod_ModInformation_2022"
					End If 
					
				End If
								

#End Region 'GetModInfoTable

#Region "GetSupportingDocumentYearNonEdit" 

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetSupportingDocumentYear, WFTime=[|WFTime|])
				If args.FunctionName.XFEqualsIgnoreCase("GetSupportingDocumentYearNonEdit") Then
					Dim wfTime As String = args.NameValuePairs("WFTime")
					
					If wfTime >= 2028 Then 
						Return "04d1a_BDF_RP_Dashboard_Content_AddEditNonBillets_NonEditRP_OS_2028"
					Else
						Return "04d1a_BDF_RP_Dashboard_Content_AddEditNonBillets_NonEditRP_OS_2022"
					End If 
					
				End If
								

#End Region 'GetSupportingDocumentYear

#Region "PwrUserUpdateAllBillets"

	If args.FunctionName.XFEqualsIgnoreCase("PwrUserUpdateAllBillets") Then
		
		If BRApi.Security.Authorization.IsUserInGroup(si, "USCG_FERBE_BudFm_r_PowerUser") Then
        
		         Return True
				  
	     Else 
				 BRAPI.ErrorLog.LogMessage(si, "Reguser")
				  Return False
				
	     End If 
		 
   End If
	 
#End Region


#Region "GetSupportingDocumentYear" 

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetSupportingDocumentYear, WFTime=[|WFTime|])
				If args.FunctionName.XFEqualsIgnoreCase("GetSupportingDocumentYear") Then
					Dim wfTime As String = args.NameValuePairs("WFTime")
					
					If wfTime >= 2028 Then 
						Return "04d1a_BDF_RP_Dashboard_Content_AddEditNonBillets_OS_2028"
					Else
						Return "04d1a_BDF_RP_Dashboard_Content_AddEditNonBillets_OS_2022"
					End If 
					
				End If
								

#End Region 'GetSupportingDocumentYear


#Region "Get ToolBar DB" 

				'XFBR(Workspace.Current.BUDFM_Assembly.BUDFM_StringHelper, GetSupportingDocumentYear, WFTime=[|WFTime|])
				If args.FunctionName.XFEqualsIgnoreCase("GetToolBarDB") Then
				    Dim contentDbValue As String = String.Empty
				    If args.NameValuePairs.ContainsKey("ContentDB") Then
				        contentDbValue = args.NameValuePairs("ContentDB")
				    End If
				    Dim appn As String = ResolveAppnForToolbar(args.NameValuePairs.XFGetValue("APPN_Content", String.Empty), contentDbValue)
				    Dim toolbarCfg As AppnToolbarConfig = ResolveToolbarConfig(appn)
				    Dim resolvedContent As String = NormalizeContentForToolbar(contentDbValue, appn)
				    ' Mode-aware toolbars: Edit only when the param says Edit AND the
				    ' user isn't read-only (same trump as GetModeDashboard/RPControlState).
				    Dim tbMode As String = args.NameValuePairs.XFGetValue("Mode", "View")
				    Dim tbEdit As Boolean = tbMode.XFEqualsIgnoreCase("Edit") AndAlso Not Workspace.GBL.GBL_Assembly.GBL_Helpers.Is_Read_Only(si, "prm_Security_BudFm_r_Auditor")
				
				    Select Case resolvedContent
				        Case appn & "_RP_CreateRP"
				            Return toolbarCfg.CreateRP
				        Case appn & "_RP_Content"
				            ' Edit RP: locked-label toolbar while editing, search toolbar in View
				            Return If(tbEdit, toolbarCfg.RPContentEdit, toolbarCfg.RPContentView)
				        Case appn & "_Billets_Main_04c"
				            Return toolbarCfg.BilletsMain
				        Case appn & "_Billets_AddEditNon_04d"
				            Return toolbarCfg.BilletsAddEdit
				        Case appn & "_Billets_NonAddEditNon_04d"
				            Return toolbarCfg.BilletsView
				        Case appn & "_Rpt_Reporting"
				            Return toolbarCfg.Reporting
				        Case appn & "_RP_ConcReview"
				            Return toolbarCfg.ConcReview
				        Case Else
				            ' default RP toolbar so the embed never errors on an unmapped page
				            Return toolbarCfg.Fallback
				    End Select
				End If
								

#End Region 'GetSupportingDocumentYear

#Region "Unimplemented legacy stubs"
				' Referenced by dashboard bindings but never implemented in the legacy
				' BudFm_ParamHelper either — calls fell through to Return Nothing there.
				' Kept explicit so dispatch coverage documents them.
				If args.FunctionName.XFEqualsIgnoreCase("GetAppropriation_Option") Then
					Return Nothing
				End If
				If args.FunctionName.XFEqualsIgnoreCase("GetUD6NBLT") Then
					Return Nothing
				End If
#End Region 'Unimplemented legacy stubs



				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Private Function ResolveToolbarConfig(ByVal appn As String) As AppnToolbarConfig
			If ToolbarConfigByAppn.ContainsKey(appn) Then Return ToolbarConfigByAppn(appn)
			Return New AppnToolbarConfig(appn & "_RP_ToolbarCreateRP", appn & "_RP_Toolbar_03b", appn & "_RP_Toolbar_03bView", appn & "_Billets_Toolbar", appn & "_NonBillets_Toolbar", appn & "_NonBillets_ToolbarView", appn & "_Rpt_Toolbar", appn & "_RP_ToolbarConcReview", appn & "_RP_Toolbar_03")
		End Function

		Private Function ResolveAppnForToolbar(ByVal appnArg As String, ByVal contentDbValue As String) As String
			If Not String.IsNullOrWhiteSpace(appnArg) Then Return appnArg.Trim().ToUpperInvariant()
			If Not String.IsNullOrWhiteSpace(contentDbValue) Then
				Dim content As String = contentDbValue.Trim()
				Dim lastUnderscore As Integer = content.LastIndexOf("_"c)
				If lastUnderscore >= 0 AndAlso lastUnderscore < content.Length - 1 Then
					Dim suffix As String = content.Substring(lastUnderscore + 1)
					If ToolbarConfigByAppn.ContainsKey(suffix) Then Return suffix.ToUpperInvariant()
				End If
				Dim firstUnderscore As Integer = content.IndexOf("_"c)
				If firstUnderscore > 0 Then Return content.Substring(0, firstUnderscore).ToUpperInvariant()
			End If
			Return "OS"
		End Function

		Private Function NormalizeContentForToolbar(ByVal contentDbValue As String, ByVal appn As String) As String
			If String.IsNullOrWhiteSpace(contentDbValue) Then Return appn & "_RP_Content"
			Dim normalized As String = contentDbValue.Trim()
			If normalized.XFContainsIgnoreCase("_BDF_RP_Dashboard_Content_") Then
				If normalized.XFContainsIgnoreCase("CreateRP") Then Return appn & "_RP_CreateRP"
				If normalized.XFContainsIgnoreCase("NonEditRP") OrElse normalized.XFContainsIgnoreCase("EditRP") Then Return appn & "_RP_Content"
				If normalized.XFContainsIgnoreCase("NonAddEditNonBillets") Then Return appn & "_Billets_NonAddEditNon_04d"
				If normalized.XFContainsIgnoreCase("AddEditNonBillets") Then Return appn & "_Billets_AddEditNon_04d"
				If normalized.XFContainsIgnoreCase("ConcReview") Then Return appn & "_RP_ConcReview"
				If normalized.XFContainsIgnoreCase("Reporting") Then Return appn & "_Rpt_Reporting"
			End If
			Return normalized
		End Function

	End Class
End Namespace
