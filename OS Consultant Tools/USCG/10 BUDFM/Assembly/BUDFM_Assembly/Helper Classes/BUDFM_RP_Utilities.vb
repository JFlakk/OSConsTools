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
Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName

	
'*********************************************EDITING TRACKER************************************************************
'***************************FILL OUT EVERY TIME YOU ARE EDITING RULE, AND SAVE IMMEDIATELY*******************************
'*********************DO NOT FORGET TO CHANGE YOUR STATUS TO CLOSED AND SAVE RULE BEFORE YOU EXIT************************

'Name: 			Monica N.
'Date: 			05/22/2025 
'Time: 			3:44 PM
'Open/Closed:	Open
'
'************************************************************************************************************************	

Public Class BUDFM_RP_Utilities
 
Public Const RP_MODE_EDIT = "Mode_01" 
Public Const RP_MODE_VIEWONLY = "Mode_02" 

Public Const RP_CC_NOT_REQD = "CC_01" 
Public Const RP_CC_REQD = "CC_02" 

Public Const RP_STATUS_CREATE = "Status_01" 
Public Const RP_STATUS_BUDGET = "Status_03" 

Private si As SessionInfo
Public globals As BRGlobals
Private api As Object
Private args As ExtenderArgs


Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
	Try
		Me.si = si
		Me.globals = globals
		Me.api = api
		Me.args = args	
		Select Case args.FunctionType
			
			Case Is = ExtenderFunctionType.Unknown
				
			Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep

				Dim FunctionToRun As String = args.NameValuePairs("FunctionToRun")
				
				If FunctionToRun = "RunTestSuite" Then
					' General test suite for functions in the busines Rule ( for developers to test)
'							BrApi.ErrorLog.LogMessage (si, 
'										 "Input Parameters: 2025, LO_DCMS, OS, 1, 4123, 00")					
'							Dim RPName As String =  Generate_RP_LongName(si, "2025", "LO_DCMS", "OS", "1", "4123", "00" )
'							Dim RPName As String  = "25_4095_00"
'							BrApi.ErrorLog.LogMessage (si, "Generated RP Name: " & RPName)
'							BrApi.ErrorLog.LogMessage (si, "Returned Appropriation: " 	& Get_RP_Appropriation(si, RPName))
'							BrApi.ErrorLog.LogMessage (si, "Returned Entity: " 			& Get_RP_Entity(si, RPName))
'							BrApi.ErrorLog.LogMessage (si, "Returned Budget Year: " 	& Get_RP_Budget_Year(si, RPName))
'							BrApi.ErrorLog.LogMessage (si, "Returned Budget Category: " & Get_RP_Budget_Category(si, RPName))
'							BrApi.ErrorLog.LogMessage (si, "Returned RP Number: " 		& Get_RP_Number(si, RPName))
'							BrApi.ErrorLog.LogMessage (si, "Returned RP Suffix: " 		& Get_RP_Suffix(si, RPName))
'							Copy_RP_Annotations(si, "BudFm", "RAP_FY25", "RAP_FY25", "RP_FY_2025_DCMS_RD_0_4034_00", "RP_FY_2025_DCO_RD_0_2007_00")
'							Delete_RP_Annotations(si, "BudFm", "RAP_FY25", "RP_FY_2025_DCO_RD_0_2007_00")
'							Delete_RP(si, "BudFm", "FY_2025", "RP_FY_2025_DCMS_RD_0_4062_01")
'							Create_New_RP_FromScrartch(si, "2025", "RD", "LO_DCMS", "NA", "Ranga_Test_04_27_2023")
'							Create_New_RP_AsExtention(si,"2025", "25_4095_00", "Ranga_Test_03_20_2023 extension")
'							Create_New_RP_ByCopying(si, "2025",  "RD", "LO_DCMS", "0", "RP By Copy", "RP_FY_2025_DCO_RD_0_2007_00", "RAP_FY25", "RAP_FY25", "BudFm" )
'							Create_WorkingVersion_of_RP(si, "BudFm", "RAP_FY25", "RAP_FY25", "25_4095_00")
'							Rename_RPs_OneTIme(si)
'							If Is_RP_Editable(si, "25_4120_00") Then
'								BrApi.ErrorLog.LogMessage(si, "Editable")
'							Else 
'								BrApi.ErrorLog.LogMessage(si, "Not Editable")
'							End If

'							If Is_RP_CC_Required(si, "25_4120_00") Then
'								BrApi.ErrorLog.LogMessage(si, "Change Comment Required")
'							Else 
'								BrApi.ErrorLog.LogMessage(si, "Change Comment Not Required")
'							End If
'							BrApi.ErrorLog.LogMessage(si, " RP Status: " & Get_RP_Status(si, "25_4120_00"))
					BrApi.ErrorLog.LogMessage(si, "Calling Load Method")
					Load_Form_Data_Exported_By_DM_Job(si)
					
				Else If FunctionToRun = "Copy_RP_Text_Properties" Then
					
					Dim SourceScenario As String = args.NameValuePairs("SourceScenario")
					Dim TargetScenario As String = args.NameValuePairs("TargetScenario")
					Dim SourceYear As String = args.NameValuePairs("SourceYear")
					Dim TargetYear As String = args.NameValuePairs("TargetYear")
					Dim RPParent As String = args.NameValuePairs("RPParent")
					Copy_RP_Text_Properties(si, RPParent, SourceScenario, TargetScenario, SourceYear, TargetYear)
					
				Else If FunctionToRun = "Clear_RP_Text_Properties" Then
					
					Dim TargetScenario As String = args.NameValuePairs("TargetScenario")
					Dim TargetYear As String = args.NameValuePairs("TargetYear")
					Dim RPParent As String = args.NameValuePairs("RPParent")
					
					Clear_RP_Text_Properties(si, RPParent, TargetScenario, TargetYear)
					
				Else If FunctionToRun = "Copy_All_RP_DataAttachments" Then
					
					Dim WFCube As String = args.NameValuePairs("WFCube")
					Dim SourceScenario As String = args.NameValuePairs("SourceScenario")
					Dim TargetScenario As String = args.NameValuePairs("TargetScenario")
					Dim SourceYear As String = args.NameValuePairs("SourceYear")
					Dim TargetYear As String = args.NameValuePairs("TargetYear")
					
					Copy_All_RP_DataAttachments(si, WFCube, SourceScenario, TargetScenario, SourceYear, TargetYear)

				Else If FunctionToRun = "Clear_All_RP_DataAttachments" Then
					
					Dim WFCube As String = args.NameValuePairs("WFCube")
					Dim TargetScenario As String = args.NameValuePairs("TargetScenario")
					Dim TargetYear As String = args.NameValuePairs("TargetYear")
					
					Clear_All_RP_DataAttachments(si, WFCube, TargetScenario, TargetYear)
					
					
				Else If FunctionToRun = "Copy_RP_Attributes" Then
					
					Dim WFCube As String = args.NameValuePairs("WFCube")
					Dim TargetScenario As String = args.NameValuePairs("TargetScenario")
					Dim SourceScenario As String = args.NameValuePairs("SourceScenario")
					Dim SourceRPName As String = args.NameValuePairs("SourceRPName")
					Dim TargetRPName As String = args.NameValuePairs("TargetRPName")
					Dim createWV As Boolean = False
					Copy_RP_Attributes(si, WFCube, SourceScenario, TargetScenario, SourceRPName, TargetRPName, createWV)
					
				
				
					
				
				
				End If
				
			Case Is = ExtenderFunctionType.ExecuteExternalDimensionSource
				'Add External Members
				Dim externalMembers As New List(Of NameValuePair)
				externalMembers.Add(New NameValuePair("YourMember1Name","YourMember1Value"))
				externalMembers.Add(New NameValuePair("YourMember2Name","YourMember2Value"))
				Return externalMembers
		End Select

		Return Nothing
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function


#Region "Public Functions and Subs"

#Region "Generate_RP_LongName"
Public Function Generate_RP_LongName(
						ByVal si As SessionInfo,
						ByVal BudgetYear As String,
						ByVal Entity As String,
						ByVal Appropriation As String,
						ByVal BudgetCategory As String,
						ByVal RPNumber As String,
						ByVal RPSuffix As String
						)
		
' This function returns generates and return RP Name which includes necessry paremeters 
'  Exampe:
' 		Input: 
'			BudgetYear 		(Ex: 2025)
'			Entity  		(Ex: LO_DCMS)
'			Appropriation 	(Ex: OS)
'			BudgetCategory 	(Ex: 1)
'			RPNumber 		(Ex: 4123
'			RPSuffix 		(Ex: 00)
'		Returns:
'			RP_FY_<BudgetYear>_<Entity>_<Appropriation>_<BudgetCategory>_<RPNumber>_<RPSuffix>
'			Ex: RP_FY_2025_DCMS_OS_1_4123_00
'
' 			(**Please note it strips LO_ from Entity Name**)
'
	Try
		' Check to make sure all the input parameters are non-Empty Strings
		If Appropriation = "" Then
			Throw New Exception("Appropriation name is Empty")
		End If 

		If BudgetYear = "" Then
			Throw New Exception("Budget Year is Empty")
		End If 

		If BudgetCategory = "" Then
			Throw New Exception("Budget Category is Empty")
		End If 

		If RPNumber = "" Then
			Throw New Exception("RP Number is Empty")
		End If 
		
		If RPSuffix = "" Then
			Throw New Exception("RP Suffix is Empty")
		End If 

		' Strip LO_ from Entity
		Dim RPSplit As List(Of String) = StringHelper.SplitString(Entity,"_")		
		Dim Entity_without_LO As String = RPSplit(1)
		
		' Construct RPName
		'  RPName = RP_FY_<BudgetYear>_<Entity>_<Appropriation>_<BudgetCategory>_<RPNumber>_<RPSuffix>
		'
		Dim RPName As String = "RP_FY_" & 
							BudgetYear & "_" & 
							Entity_without_LO & "_" &
							Appropriation & "_" & 
							BudgetCategory & "_" &
							RPNumber & "_" &
							RPSuffix
							
		Return RPName
		
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function
#End Region 'Generate_RP_LongName


#Region "Get_RP_Appropriation" 
Public Function Get_RP_Appropriation(ByVal si As SessionInfo, ByVal RPShortName As String) As String
'
' This function returns Appropriation for an given RP 
'  Exampe:
' 		Long Name:  	RP_FY_2025_DCMS_OS_3_4010_00
'		Returns:	OS
'
	Try
		' Check to make sure RP name is not Empty String
		If RPShortName = "" Then
			Throw New Exception("RP Name is Empty")
		End If 
		Dim RPLongName As String = Get_RP_LongName(si, RPShortName)
		' Extract Appropriation and return 
		Dim RPSplit As List(Of String) = StringHelper.SplitString(RPLongName,"_")
		Dim Appropriation As String = RPSplit(4)
		Return Appropriation
		
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function
#End Region 'Get_RP_Appropriation

#Region "Get_RP_Entity"
Public Function Get_RP_Entity(ByVal si As SessionInfo, ByVal RPShortName As String)
'
' This function returns Entity for an given RP 
'  Exampe:
' 		Long Name:  	RP_FY_2025_DCMS_OS_3_4010_00
'		Returns:	LO_DCMS  (Appends LO_ to DCMS)
'
	Try
		
		' Check to make sure RP name is not Empty String
		If RPShortName ="" Then
			Throw New Exception("RP Name is Empty")
		End If 
		
		'If parent RP, then return E#Total_Lead_Office, Else if Baseline return E#LO_No
		If (RPShortName.XFContainsIgnoreCase("Top_Flow") Or RPShortName.XFContainsIgnoreCase("_RP")) Then
			Return "Total_Lead_Office"
		Else If RPShortName.XFEqualsIgnoreCase("Baseline")
			Return "LO_No"
		End If
		Dim RPLongName As String = Get_RP_LongName(si, RPShortName)
		
		' Extract Entity and return 
		
		Dim RPSplit As List(Of String) = StringHelper.SplitString(RPLongName,"_")
		Dim Entity As String = "LO_" & RPSplit(3)

		Return Entity 
		
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function
#End Region 'Get_RP_Entity

#Region "Get_RP_Budget_Category"
Public Function Get_RP_Budget_Category(ByVal si As SessionInfo,ByVal RPShortName As String)
'
' This function returns Budget Category for an given RP 
'  Exampe:
' 		Long Name:  	RP_FY_2025_DCMS_OS_3_4010_00
'		Returns:	3
'
'
	Try
		' Check to make sure RP name is not Empty String
		If RPShortName ="" Then
			Throw New Exception("RP Name is Empty")
		End If 
		Dim RPLongName As String = Get_RP_LongName(si, RPShortName)
		
		' Extract budget Category and return 
		Dim RPSplit As List(Of String) = StringHelper.SplitString(RPLongName,"_")
		Dim BudgetCategory As String = RPSplit(5)
		Return BudgetCategory
		
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function
#End Region 'Get_RP_Budget_Category

#Region "Get_RP_Budget_Year"
Public Function Get_RP_Budget_Year(ByVal si As SessionInfo, ByVal RPShortName As String)
'
' This function returns Budget Year for a given RP 
'  Exampe:
' 		Input:  	RP_FY_2025_DCMS_OS_3_4010_00
'		Returns:	2025
'
'
	Try
		' Check to make sure RP name is not Empty String
		If RPShortName ="" Then
			Throw New Exception("RP Name is Empty")
		End If 

		Dim RPLongName As String = Get_RP_LongName(si, RPShortName)
		
		' Extract Budget Year and return 
		Dim RPSplit As List(Of String) = StringHelper.SplitString(RPLongName,"_")
		Dim BudgetYear As String = RPSplit(2)
		Return BudgetYear
		
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function
#End Region 'Get_RP_Budget_Year

#Region "Get_RP_Budget_Year_YY"
Public Function Get_RP_Budget_Year_YY(ByVal si As SessionInfo, ByVal RPShortName As String)
'
' This function returns two digit Budget Year for a given RP 
'  Exampe:
' 		Input:  	RP_FY_2025_DCMS_OS_3_4010_00
'		Returns:	25
'
'
	Try
		Dim BudgetYear_YYYY = Get_RP_Budget_Year(si, RPShortName)
		Dim Budget_Year_YY = BudgetYear_YYYY - 2000
		Return Budget_Year_YY
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function
#End Region 'Get_RP_Budget_Year_YY

#Region "Get_WFTime_YY"
Public Function Get_WFTime_YY(ByVal si As SessionInfo, ByVal WFTime As String)
'
' This function returns two digit Year for a given WFTime
'  Exampe:
' 		Input:  	2025
'		Returns:	25
'
'
	Try
		
		Dim WFTime_YY = WFTime.Substring(2,2)
		Return WFTime_YY
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function
#End Region 'Get_WFTime_YY

#Region "Get_RP_Number"
Public Function Get_RP_Number(ByVal si As SessionInfo,ByVal RPShortName As String)
'
' This function returns 4 digit RP Number for an given RP
'  Exampe:
' 		Input:  	RP_FY_2025_DCMS_OS_3_4010_00
'		Returns:	4010
'
'

	Try
		' Check to make sure RP name is not Empty String
		If RPShortName ="" Then
			Throw New Exception("RP Name is Empty")
		End If 

		Dim RPLongName As String = Get_RP_LongName(si, RPShortName)
		
		' Extract RP Number and return 
		Dim RPSplit As List(Of String) = StringHelper.SplitString(RPLongName,"_")
		Dim RPNumber As String = RPSplit(6)
		Return RPNumber
		
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function
#End Region 'Get_RP_Number

#Region "Get_RP_Suffix"
Public Function Get_RP_Suffix(ByVal si As SessionInfo, ByVal RPShortName As String)
'
' This function returns 2 digit RP Suffix for an given RP
'  Exampe:
' 		Input:  	RP_FY_2025_DCMS_OS_3_4010_00
'		Returns:	00
'
'
	Try
		' Check to make sure RP name is not Empty String
		If RPShortName ="" Then
			Throw New Exception("RP Name is Empty")
		End If 
		Dim RPLongName As String = Get_RP_LongName(si, RPShortName)
		
		' Extract RP Suffix and return 
		Dim RPSplit As List(Of String) = StringHelper.SplitString(RPLongName,"_")		
		Dim RPSuffix As String = RPSplit(7)
		Return RPSuffix
		
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function
#End Region 'Get_RP_Suffix

#Region "Get_RP_Parent"
Public Function Get_RP_Parent(ByVal si As SessionInfo, ByVal RPYear As String)
	Try
		' Check to make sure RP name is not Empty String
		If RPYear ="" Then
			Throw New Exception("RP Year is Empty")
		End If 
		
		' Extract RP Number and return 
		Return "FY_" & RPYear & "_RPs"		
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function
#End Region 'Get_RP_Parent

#Region "Get_BYRP_Parent"
Public Function Get_BYRP_Parent(ByVal si As SessionInfo, ByVal RPYear As String)
	Try
		' Check to make sure RP name is not Empty String
		If RPYear ="" Or RPYear.Length <> 4 Then
			Throw New Exception("Invalid RP Year " & RPYear)
		End If 
		
		' Extract RP Number and return 
		Return "FY" & RPYear.Substring(2) & "_RP"		
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function
#End Region 'Get_BYRP_Parent

#Region "Get_BYRP_WV_Parent"
Public Function Get_BYRP_WV_Parent(ByVal si As SessionInfo, ByVal RPYear As String)
	Try
		' Check to make sure RP name is not Empty String
		If RPYear ="" Or RPYear.Length <> 4 Then
			Throw New Exception("Invalid RP Year " & RPYear)
		End If 
		
		' Extract RP Number and return 
		Return "FY" & RPYear.Substring(2) & "_RP_WV"		
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function
#End Region 'Get_BYRP_Parent

#Region "Get_ATRP_Parent"
Public Function Get_ATRP_Parent(ByVal si As SessionInfo, ByVal RPYear As String)
	Try
		' Check to make sure RP name is not Empty String
		If RPYear ="" Or RPYear.Length <> 4 Then
			Throw New Exception("Invalid RP Year " & RPYear)
		End If 
		
		' Extract RP Number and return
		Dim TwoDigitYear As Integer = RPYear.Substring(2)
		' Ann / Term RPs are from teh previous year
		TwoDigitYear = TwoDigitYear - 1
		Return "FY" & TwoDigitYear & "_AT"		
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function
#End Region 'Get_ATRP_Parent

#Region "Get_RP_LDName"
Private Function Get_RP_LDName(ByVal si As SessionInfo, ByVal LDEntityName As String)
'Strips LO_ from Entity Name
	Try
		' Check to make sure LD Entity name is not Empty String
		If LDEntityName ="" Then
			Throw New Exception("Lead Directorate is Empty")
		End If
		' Strip LO_ from the entity and retun rest of the String
	    Dim LeadDirect As List(Of String) = StringHelper.SplitString(LDEntityName,"_")	
		Return  LeadDirect(1)
		
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function
#End Region 'Get_RP_LDName

#Region "Generate_RP_ShortName"
Private Function Generate_RP_ShortName (ByVal si As SessionInfo, 
										ByVal BudgetYear As String, 
										ByVal RPNumber As String,
										ByVal Suffix As String)
	Try
		'Generate RP short name,  Example  21_1234_00

		Dim TwoDigitYear As Integer = BudgetYear - 2000
		
		Dim RPShortName As String = TwoDigitYear & "_" & RPNumber & "_" & Suffix
		Return RPShortName
		
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function
#End Region 'Generate_RP_ShortName

#Region "Is_Working_Version"
Public Function Is_Working_Version (ByVal si As SessionInfo, ByVal RPName As String)
	Try
		'Check if RP is a workign version. If so, return true else false
		Return RPName.EndsWith("_WV")
		
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function
#End Region 'Generate_RP_ShortName

#Region "Get_RP_Status_Description"
Public Function Get_RP_Status_Description (ByVal si As SessionInfo, ByVal RPShortName As String) As String
	Try
		'Return Description of RP Status
		Return BRApi.Finance.Members.GetMemberInfo(si, dimtypeId.UD8, Get_RP_Status(si, RPShortName)).Description
		
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function 'Get_RP_Status_Description
#End Region 'Get_RP_Status_Description

#Region "Get_RP_Mode_Description"
Public Function Get_RP_Mode_Description (ByVal si As SessionInfo, ByVal RPShortName As String) As String
	Try
		'Return Description of RP Mode
		Return BRApi.Finance.Members.GetMemberInfo(si, dimtypeId.UD8, Get_RP_Mode(si, RPShortName)).Description
		
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function 'Get_RP_Mode_Description
#End Region 'Get_RP_Mode_Description

#Region "Get_RP_CC_Required_Description"
Public Function Get_RP_CC_Required_Description (ByVal si As SessionInfo, ByVal RPShortName As String) As String
	Try
		'Return Description of RP CC Required flag
		Return BRApi.Finance.Members.GetMemberInfo(si, dimtypeId.UD8, Get_RP_CC_Required(si, RPShortName)).Description
		
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function 'Get_RP_CC_Required_Description
#End Region 'Get_RP_CC_Required_Description

#Region "Get_RP_Description"
Public Function Get_RP_Description (ByVal si As SessionInfo, ByVal RPShortName As String) As String
	Try
		'Return Description of RP 
		Return BRApi.Finance.Members.GetMemberInfo(si, dimtypeId.Flow, RPShortName).Description
		
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function 'Get_RP_Description
#End Region 'Get_RP_Description

#Region "Is_RP_Editable"
Public Function Is_RP_Editable (ByVal si As SessionInfo, ByVal RPShortName As String) As Boolean
	Try

		Return Get_RP_Mode(si, RPShortName).XFEqualsIgnoreCase(RP_MODE_EDIT)
		
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function 'Is_RP_Editable
#End Region 'Is_RP_Editable

#Region "Is_RP_CC_Required"
Public Function Is_RP_CC_Required (ByVal si As SessionInfo, ByVal RPShortName As String) As Boolean
	Try
		Return Get_RP_CC_Required(si, RPShortName).XFEqualsIgnoreCase(RP_CC_REQD)
		
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function 'Is_RP_CC_Required
#End Region 'Is_RP_CC_Required

#Region "Is_Read_Only"
Public Function Is_Read_Only (ByVal si As SessionInfo) As Boolean
	
	'Define auditor role based on the dashboard parameter
	Dim grpAuditors As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Security_BudFm_r_Auditor")
	
	'Define scenario read/write group and determine if user is in the read/write group
	Dim wfPk As WorkflowUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si)
	Dim scenarioReadWriteGrpId As Guid = BRApi.Finance.Members.GetMemberInfo(si, dimTypeId.Scenario, wfPk.ScenarioKey, True).Member.ReadWriteDataGroupUniqueID
	Dim userInScenarioReadWriteGrp As Boolean = BRApi.Security.Authorization.IsUserInGroup(si, scenarioReadWriteGrpId)	
	
	Try 
		If (BRApi.Security.Authorization.IsUserInGroup(si, grpAuditors) And Not BRApi.Security.Authorization.IsUserInAdminGroup(si))
			Return True
		Else If Not userInScenarioReadWriteGrp
			Return True
		Else
			Return False
		End If
		
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function 'Is_Read_Only
#End Region 'Is_Read_Only

#Region "RP_Exists"
Public Function RP_Exists (ByVal si As SessionInfo, ByVal RPShortName As String) As Boolean
	
	Try 
		' Check if an RP already exists with the same name
		Dim MemId As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Flow, RPShortName)
		BrApi.ErrorLog.LogMessage(si, "RP MemId: " & MemId)	
		If Not MemId = -1 Then
			Return True
		Else
			Return False
		End If
		
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function 'RP_Exists
#End Region 'RP_Exists

#Region "Create_New_RP_FromScrartch"
Public Function Create_New_RP_FromScrartch(
					ByVal si As SessionInfo,
					ByVal WFTime As String,
					ByVal RPAppr As String,
					ByVal RPEntity As String,
					ByVal RPBudCat As String,
					ByVal RPTitle As String
					)					
	Try
						
		'Get Lead directorate name from RP Entity 
	    Dim LeadDirectorateName As String = Get_RP_LDName(si, RPEntity)															
		
		' Get RP Parent member name Name 
		Dim RPParentName As String = Get_BYRP_Parent(si, WFTime)

		' Get the next availabe sequence id ( 4 digi RP number to embed into RP name) for the this specific Lead Directorate 
		Dim NextSequenceID = Get_NextSequenceID(si, WFTime, LeadDirectorateName, RPParentName)
		
		
		'Check if budget Category NA, If so set it to 0
		If RPBudCat.XFEqualsIgnoreCase("NA") Then
			RPBudCat = "0"
		End If
		
		'It is new RP, set the suffix to 00
		Dim RPSuffix As String = "00"
	
		' Generate new RP Long Name and Short Name. 
		' Short name Is used As flow member And Long name is stored and text8
		Dim RPLongName As String = Generate_RP_LongName(si, WFTime, RPEntity, RPAppr, RPBudCat, NextSequenceID, RPSuffix)
		Dim RPShortName As String = Generate_RP_ShortName(si, WFTime, NextSequenceID, RPSuffix)
		
		'Create a new RP  i.e Add a new flow memeber and set it's realtionship properties 
		Create_RP(si, RPLongName, RPTitle, RPShortName, RPParentName)	
	
		
		Return RPShortName
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function
#End Region 'Create_New_RP_FromScrartch

#Region "Create_New_RP_AsExtention"
Public Function Create_New_RP_AsExtention(
					ByVal si As SessionInfo,
					ByVal RPYear As String,
					ByVal SourceRPShortName As String,
					ByVal NewRPTitle As String)					
	Try
		
		
		'This RP is being created as extension of an existing RP. 
		' Get the max suffix and bump it up by 1		
		Dim SourceRPSuffix As Integer = Get_RP_Suffix(si, SourceRPShortName)
		If SourceRPSuffix > 0 Then
			Throw New Exception("Please choose an RP with _00 extension as source.")
		End If
		Dim SourceRPNumber As String = Get_RP_Number(si, SourceRPShortName)
		Dim SourceRPTwoDigitYear As Integer = Get_RP_Budget_Year_YY(si, SourceRPShortName)
		Dim SourceRPParentBY As String = Get_BYRP_Parent(si, RPYear)
		
		Dim BudFm_FlowDim As OneStream.Shared.Wcf.Dim = BRApi.Finance.Dim.GetDim(si, "Std_Flow")					
		Dim RPList As List (Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BudFm_FlowDim.DimPk, 
													"F#" & SourceRPParentBY & 
															".Base.Where(Name Contains " & SourceRPTwoDigitYear & "_" & SourceRPNumber & " )", True)
		Dim UsedSuffixes As List (Of Integer) = New List (Of Integer)
		For Each RP As MemberInfo In RPList
			'Get the suffix number from the RP and add it to the list
			Dim Suffix As Integer = Get_RP_Suffix(si, RP.Member.Name)
			BrApi.ErrorLog.LogMessage(si, "Suffix " & Suffix)
			
'				'Add it to the list
			UsedSuffixes.Add(Suffix)
		Next 
		
'			'Sort the list and get the last number in it and add a 1 to this because it will be the next number to assign			
		UsedSuffixes.Sort()
		Dim currLastSuffix As Integer = usedSuffixes.Last()
		Dim NewRPSuffix As Integer = currLastSuffix + 1
		Dim NewRPSuffixStr As String = String.Empty
		
		If NewRPSuffix < 10 Then
			NewRPSuffixStr = "0" & NewRPSuffix
		Else
			NewRPSuffixStr = NewRPSuffix
		End If 
		
'		' Generate new RP Name . In this case RP name remains  the same except suffix
		Dim RPEntity As String = Get_RP_Entity(si, SourceRPShortName)
		Dim RPBudCat As String = Get_RP_Budget_Category(si, SourceRPShortName)
		Dim RPAppr As String = Get_RP_Appropriation(si, SourceRPShortName)
		Dim RPNumber As String = Get_RP_Number(si, SourceRPShortName)

		Dim RPLongName As String = Generate_RP_LongName(si, RPYear, RPEntity, RPAppr, RPBudCat, RPNumber, NewRPSuffixStr)
		Dim RPShortName As String = Generate_RP_ShortName(si, RPYear, RPNumber, NewRPSuffixStr)

		' Check if an RP already exists with the same name
		Dim MemId As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Flow, RPShortName)
		
		If Not MemId = -1 Then
			Throw New Exception("Extension RP already exists for this RP: " & SourceRPShortName)
		End If
				
		' Get RP Parent member name Name 
		Dim RPParentName As String = Get_BYRP_Parent(si, RPYear)
		
		'Create a new RP  i.e Add a new flow memeber and set it's realtionship properties 
		Create_RP(si, RPLongName, NewRPTitle, RPShortName, RPParentName)
		
		Return RPShortName
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function
#End Region 'Create_New_RP_AsExtention

#Region "Copy_RP_Attributes"
Public Function Copy_RP_Attributes(
					ByVal si As SessionInfo,
					ByVal Cube As String,
					ByVal SourceScenario As String,
					ByVal TargetScenario As String,
					ByVal SourceRPName As String,
					ByVal TargetRPName As String,
					ByVal createWV As Boolean
					)					
	Try
		
		Dim SourceYear As String = "20" & sourceScenario.Substring((sourceScenario.length-2),2)
		Dim TargetYear As String = "20" & targetScenario.Substring((targetScenario.length-2),2)

		Dim SourceRPEntity As String = Get_RP_Entity(si, SourceRPName)
		Dim TargetRPEntity As String = Get_RP_Entity(si, TargetRPName)
		
	   	Dim TargetBillets As Integer = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, Cube, "A#Number_of_Billets:E#" & TargetRPEntity & ":S#" & TargetScenario & ":T#" & TargetYear & ":V#Annotation:F#" & TargetRPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").DataCellEx.DataCellAnnotation.XFConvertToInt
		Dim SourceBillets As Integer = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, Cube, "A#Number_of_Billets:E#" & SourceRPEntity & ":S#" & SourceScenario & ":T#" & SourceYear & ":V#Annotation:F#" & SourceRPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").DataCellEx.DataCellAnnotation.XFConvertToInt
	
		Dim args As New Dictionary(Of String, String)
         args.Add("Cube", Cube)
         args.Add("SourceScenario", SourceScenario)
         args.Add("TargetScenario", TargetScenario)
         args.Add("SourceRPName", SourceRPName)
         args.Add("TargetRPName", TargetRPName)
         args.Add("SourceYear", SourceYear)
         args.Add("TargetYear", TargetYear)
         args.Add("SourceRPEntity", SourceRPEntity)
         args.Add("TargetRPEntity", TargetRPEntity)
	     args.Add("TargetBillets", TargetBillets)
		 args.Add("SourceBillets", SourceBillets)
		
		 Dim RPCopyAnnotations As Boolean = False
		
		 Dim WVrpName As Boolean = TargetRPName.Contains("_WV")
		 
		 If(CInt(SourceYear) = CInt(TargetYear)) Then
			 RPCopyAnnotations = True 
		 Else
			 RPCopyAnnotations = False
		 End If
'		 Dim RPCopyAnnotations As Boolean = String.CompareOrdinal(SourceYear,TargetYear)
		 
		'Step 1: Delete Data Attchments (Annotations ..etc) of Target RP 
		' Delete_RP_Annotations(si, Cube, TargetScenario, TargetRPName)

		'Step 2: Copy all Data Attachements (some exclusions apply) from source RP to target RP
		' To copy the annotations check here to see if the Target > or = to the source if it is copy up to the count of source billets.
		' The else calls the RPCopyAnnotations with the TargetBillets being sent as argument to limit the number of source billets copied to the target
'		   Brapi.ErrorLog.LogMessage(si, "Target billets " & TargetBillets)
'		   Brapi.ErrorLog.LogMessage(si, "Source billets " & SourceBillets)

		If TargetBillets >= SourceBillets And WVrpName = False Then 
		   
			Copy_RP_Annotations(si, Cube, SourceScenario, TargetScenario, SourceRPName, TargetRPName,RPCopyAnnotations)

	   Else If SourceBillets > TargetBillets And WVrpName = False Then
		    Copy_RP_Annotations(si, Cube, SourceScenario, TargetScenario, SourceRPName, TargetRPName,TargetBillets,RPCopyAnnotations)
		   
         
	   'Else If createWV = True Then prior
	   Else If WVrpName = True Or createWV = True Then
		   Copy_RP_Annotations(si, Cube, SourceScenario, TargetScenario, SourceRPName, TargetRPName)
	   
	   End If 
	
		'Step 3: Clear and Copy all Data Records Of Target RP
		'added this call below to clear out all out years before copying and altered the Clear single rp alldatarecords to use the 
		'T#|WFTime|,T#YearNext1(|WFTime|),T#YearNext2(|WFTime|),T#YearNext3(|WFTime|),T#YearNext4(|WFTime|)
		 
		BRapi.Utilities.ExecuteDataMgmtSequence(si, "Clear_Single_RP_AllDataRecords", args)
		 If WVrpName = True Or createWV = True Then
		
			 
		 	BRApi.Utilities.ExecuteDataMgmtSequence(si, "Copy_Single_RP_WV_AllDataRecords", args)
			
		Else
			
			BRApi.Utilities.ExecuteDataMgmtSequence(si, "Copy_Single_RP_AllDataRecords", args)
			
		End If
          

		Return Nothing
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function
#End Region 'Copy_RP_Attributes


'#Region "EditBLTLine_OS"

'Public Function EditBLTLine_OS(
'					ByVal si As SessionInfo,
'					ByVal Cube As String,
'					ByVal SourceScenario As String,
'					ByVal TargetScenario As String,
'					ByVal SourceRPName As String,
'					ByVal TargetRPName As String,
'					ByVal createWV As Boolean
'					)					



'				If args.FunctionName.XFEqualsIgnoreCase("EditBLTLine_OS") Then

'					Dim wfTime As String = args.NameValuePairs("WFTime")
'					Dim wfScenario As String = args.NameValuePairs("WFScenario")
'					Dim wfCube As String = args.NameValuePairs("WFCube")
'					Dim RPName As String = args.NameValuePairs("RPName")
'					Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)					
'					Dim LineItemNum As String = args.NameValuePairs("LineItemNum") 
'					Dim description_ChangeLog As String = args.NameValuePairs("Description_ChangeLog")
'					Dim reason_ChangeLog As String = args.NameValuePairs("Reason_ChangeLog")					
'					Dim increase_Decrease As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, "A#Increase_Decrease:E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").DataCellEx.DataCellAnnotation

'					If  String.IsNullOrEmpty (LineItemNum) Then 
'						Throw New Exception("Please choose a Line Item") 
'					End If
					
'					If IsOSPG1Empty(globals, si, wfCube,RP_Entity,wfScenario,wfTime,RPName) Then Throw New Exception("Empty attributes in Page 1. All attributes on Page 1 must have a value to save this page.")

'					Dim billet_Type As String = args.NameValuePairs("Billet_Type") 										'|!prm_BLT_BilletType!|
'					If  String.IsNullOrEmpty(billet_Type) Then Throw New Exception("Please choose Military/Civilian")
'					Dim grade_Type As String = args.NameValuePairs("Grade_Type") 										'|!prm_BLT_GradeType!|
'					Dim grade_Rank As String = args.NameValuePairs("Grade_Rank")  										'|!prm_BLT_GradeRank!|
'					Dim aD_Reserve As String = args.NameValuePairs("AD_Reserve") 										'|!prm_BLT_ADReserve!|
'					Dim reserve_Type As String = args.NameValuePairs("Reserve_Type") 									'|!prm_BLT_ReserveType!|
'					Dim spe_Code_Occu_Series As String = args.NameValuePairs("Spe_Code_Occu_Series") 					'|!prm_BLT_SpcCodeOccSeries!|
'					Dim CodeId As Integer = BRApi.Finance.Members.GetMemberId(si, dimtypeid.UD3, spe_Code_Occu_Series)
'					Dim SpecialtyCodeText2 As String = BRApi.Finance.UD.Text(si, dimtype.UD3.Id, CodeId, 2, DimConstants.Unknown, DimConstants.Unknown)
'					Dim cONUS_OCONUS As String = args.NameValuePairs("ConusOConus") 									'|!prm_BLT_ConusOConus!|
'					Dim pilot As String = SpecialtyCodeText2													        'Assigning Specialty Code Text2 value to Pilot
'					Dim electronic_Flight_Bag As String = args.NameValuePairs("Electronic_Flight_Bag") 					'|!prm_BLT_ElectronicFlightBag!|
'					Dim term_Billet As String = args.NameValuePairs("Term_Billet") 										'|!prm_BLT_TermBillet!|
'					Dim pPE_Type As String = args.NameValuePairs("PPE_Type") 											'|!prm_BLT_PPEType!|
'					Dim	pPE_PPA As String = args.NameValuePairs("PPE_PPA") 												'|!prm_BLT_PPE_PPA!|						
'					Dim pPE_ATU As String = args.NameValuePairs("PPE_ATU") 												'|!prm_BLT_PPE_ATU!|
'					Dim ppe_ATU_NoUnit As String=String.Empty
'					If pPE_ATU <> ""
'						ppe_ATU_NoUnit = pPE_ATU & "_NoUnit" 'Add the ATU and _NoUnit together to get the base level unit to store it at
'					End If
'					Dim build_Out_Choice As String = args.NameValuePairs("Build_Out_Choice") 							'|!prm_BLT_Build_Out!|
'					Dim iCASS_Costs As String = args.NameValuePairs("ICASS_Costs") 										'|!prm_BLT_ICASSType!|
'					Dim position_Number As String = args.NameValuePairs("Position_Number") 								'|!prm_BLT_PositionNumber!|
					
'					'Position number should only be filled out for Decreases in the RAP stage.  If filled out in RAP and Increase, throw and error
'					'If (position_Number.Length > 0 And wfScenario.XFContainsIgnoreCase("RAP_") And increase_Decrease.XFEqualsIgnoreCase("I")) Then Throw New Exception("Position Number should not be filled in for Increase RPs (See Page 1) in the RAP Scenario. Please clear the Position Number and save.")
						
'					Dim position_Title As String = args.NameValuePairs("Position_Title") 								'|!prm_BLT_PositionTitle!|
'					Dim billet_ATU As String = args.NameValuePairs("Billet_ATU") 										'|!prm_BLT_ATU!|
'					Dim billet_ATU_NoUnit As String = String.Empty
'					If billet_ATU <> ""
'						billet_ATU_NoUnit=billet_ATU & "_NoUnit" 'Add the ATU and _NoUnit together to get the base level unit to store it at
'					End If
'					Dim billet_UII As String = args.NameValuePairs("Billet_UII") 										'|!prm_BLT_UII!|
'					Dim billet_Object_Class As String = String.Empty 'leaving this empty because we don't have a parameter for it at this time
'					Dim oPFAC As String = args.NameValuePairs("OPFAC") 													'|!prm_BLT_OPFACS!|						
'					Dim oPFACID As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD4, oPFAC)
'					Dim oPFAC_PPA As String = BRApi.Finance.UD.Text(si, dimTypeId.UD4, oPFACID, 1, 0, 0)
'					Dim detached_Duty As String = args.NameValuePairs("Detached_Duty") 									'|!prm_BLT_DetachedDuty!|
'					Dim detached_Duty_Location As String = args.NameValuePairs("Detached_Duty_Location") 				'|!prm_BLT_DutyLocation!|
'					Dim background_Investigation_Type As String = args.NameValuePairs("Background_Investigation_Type") 	'|!prm_BLT_BIType!|
'					Dim Acquisition_Project As String = args.NameValuePairs("Acquisition_Project") 						'|!prm_BLT_Acq_Project!|
'					Dim lease_Choice As String = args.NameValuePairs("Lease_Choice") 									'|!prm_BLT_Lease!|
'					Dim lease_PPA As String = args.NameValuePairs("Lease_PPA") 											'|!prm_BLT_Lease_PPA_OS!|
'					Dim lease_ATU As String = args.NameValuePairs("Lease_ATU") 											'|!prm_BLT_Lease_ATU_OS!|												'|!prm_BLT_UTL_ATU!|
'					Dim lease_ATU_NoUnit As String=String.Empty
'					If lease_ATU <> ""
'						lease_ATU_NoUnit = lease_ATU & "_NoUnit" 'Add the ATU and _NoUnit together to get the base level unit to store it at
'					End If
'					Dim furniture_Reqd As String = args.NameValuePairs("Furniture_Reqd") 								'|!prm_BLT_Furniture!|
'					Dim utilities_Reqd As String = args.NameValuePairs("Utilities_Reqd") 								'|!prm_BLT_Utilities!|
'					Dim computer_Type As String = args.NameValuePairs("Computer_Type") 									'|!prm_BLT_Computer_Type!|
'					Dim lineItem_Comment As String = args.NameValuePairs("LineItem_Comment") 							'|!prm_BLT_Comment!|
'					Dim UTL_PPA As String = args.NameValuePairs("UTL_PPA") 												'|!prm_BLT_UTL_PPA!|
'					Dim UTL_ATU As String = args.NameValuePairs("UTL_ATU") 												'|!prm_BLT_UTL_ATU!|
'					Dim UTL_ATU_NoUnit As String=String.Empty
'					If UTL_ATU <> ""
'						UTL_ATU_NoUnit = UTL_ATU & "_NoUnit" 'Add the ATU and _NoUnit together to get the base level unit to store it at
'					End If
						
'					If  String.IsNullOrEmpty (term_Billet) Then 
'                        Throw New Exception("Please choose Perm / Term") 
'                    End If

'					RunPreSaveStepsForRP(si, wfCube, wfScenario, wfTime, RPName, reason_ChangeLog, description_ChangeLog, LineItemNum )					
						
'					'Write logic to determine whether to use OPFAC PPA or UII PPA
'					Dim ppa_Option As String
'					Dim billet_UII_ID As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, billet_UII)
'					Dim billet_UII_PPA As String = BRApi.Finance.UD.Text(si, dimTypeId.UD2, billet_UII_ID, 1, 0, 0)
'					If (billet_UII_PPA <> "" And Not billet_UII_PPA.Contains(",")) Then
'						ppa_Option = billet_UII_PPA
'					Else
'						ppa_Option = oPFAC_PPA
'					End If
					
'					'Storing the Annotation text for the attributes in a generic string
'					Dim scriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#" & LineItemNum & ":U7#None:U8#None"								
					
'					'Create a new list of memberscript and value
'					Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)
					
'					'*********Attribute Annotation Storage********
'					'Add the member scripts to the list and store as 0 No data annotations
'					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Billet_Type:" 						& scriptGenerics, 0, True, billet_Type))
'					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Grade_Type:" 						& scriptGenerics, 0, True, grade_Type))
'					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Grade_Rank:" 						& scriptGenerics, 0, True, grade_Rank))
'					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#AD_Reserve:" 						& scriptGenerics, 0, True, aD_Reserve))
'					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Reserve_Type:" 						& scriptGenerics, 0, True, reserve_Type))
'					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Spe_Code_Occu_Series:" 				& scriptGenerics, 0, True, spe_Code_Occu_Series))
'					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Pilot:" 							& scriptGenerics, 0, True, pilot))
'					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Electronic_Flight_Bag:" 			& scriptGenerics, 0, True, electronic_Flight_Bag))
'					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Term_Billet:" 						& scriptGenerics, 0, True, term_Billet))
'					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#PPE_Type:" 							& scriptGenerics, 0, True, pPE_Type))
'					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#PPE_PPA:" 							& scriptGenerics, 0, True, pPE_PPA))
'					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#PPE_ATU:" 							& scriptGenerics, 0, True, ppe_ATU_NoUnit))
'					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Build_Out_Choice:" 					& scriptGenerics, 0, True, build_Out_Choice))
'					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#ICASS_Costs:" 						& scriptGenerics, 0, True, iCASS_Costs))
'					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Position_Number:" 					& scriptGenerics, 0, True, position_Number))
'					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Position_Title:" 					& scriptGenerics, 0, True, position_Title))
'					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Billet_ATU:" 						& scriptGenerics, 0, True, billet_ATU_NoUnit))
'					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Billet_UII:" 						& scriptGenerics, 0, True, billet_UII))
'					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Billet_PPA:" 						& scriptGenerics, 0, True, ppa_Option))
'					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Billet_Object_Class:" 				& scriptGenerics, 0, True, billet_Object_Class))
'					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#CONUS_OCONUS:" 						& scriptGenerics, 0, True, cONUS_OCONUS))
'					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#OPFAC:" 							& scriptGenerics, 0, True, oPFAC))
'					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Detached_Duty:" 					& scriptGenerics, 0, True, detached_Duty))
'					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Detached_Duty_Location:" 			& scriptGenerics, 0, True, detached_Duty_Location))
'					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Background_Investigation_Type:" 	& scriptGenerics, 0, True, background_Investigation_Type))
'					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Lease_Choice:" 						& scriptGenerics, 0, True, lease_Choice))
'					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Lease_PPA:" 						& scriptGenerics, 0, True, lease_PPA))
'					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Lease_ATU:" 						& scriptGenerics, 0, True, lease_ATU_NoUnit))
'					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Furniture_Reqd:" 					& scriptGenerics, 0, True, furniture_Reqd))
'					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Utilities_Reqd:" 					& scriptGenerics, 0, True, utilities_Reqd))
'					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Computer_Type:" 					& scriptGenerics, 0, True, computer_Type))
'					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#LineItem_Comment:" 					& scriptGenerics, 0, True, lineItem_Comment))
'					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Utilities_PPA:" 					& scriptGenerics, 0, True, UTL_PPA))
'					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Utilities_ATU:" 					& scriptGenerics, 0, True, UTL_ATU_NoUnit))
'					lstMemberScriptAndValue.Add(New MemberScriptAndValue(wfCube, "A#Acquisition_Project:" 				& scriptGenerics, 0, True, Acquisition_Project))	
						
					
''							'********Allocation Drivers Storage********									
''							'For those attributes that are also a dimension, we will also store a 1 in that dimension member that is selected so we can find it in a data buffer for the cost calc	
'					Me.AllocationsCalc(si, globals, args, RP_Entity, RPName, wfCube, wfScenario, wfTime, LineItemNum, ppa_Option, billet_UII, billet_Object_Class, billet_ATU_NoUnit, pPE_PPA, ppe_ATU_NoUnit, UTL_PPA, UTL_ATU_NoUnit, lease_PPA, lease_ATU_NoUnit)							
								
						
'					'********Headcount Reporting Storage********
'					Dim hcScriptGenerics As String = "E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Periodic:F#" & RPName & ":O#Forms:I#None:U6#" & LineItemNum & ":U7#None:U8#None"			
					
'					'set the Aviator variable
'					Dim aviator As String = String.Empty
'					If pilot = "Y"
'						aviator = "Aviator"
'					ElseIf pilot = "N"
'						aviator = "NA_Aviator"
'					End If
					
'					'Set the military employment type variable
'					Dim milEmpType As String = String.Empty
'					If aD_Reserve.XFEqualsIgnoreCase("Active_Duty")
'						milEmpType = aD_Reserve
'					ElseIf aD_Reserve.XFEqualsIgnoreCase("Reserve")
'						milEmpType = reserve_Type
'					Else 
'						milEmpType = "NA_Military_Employment_Type"
'					End If
						
					
'					'Run the Headcount Calc
'					Me.HeadcountCalc(si, globals, args, RP_Entity, RPName, wfCube, wfScenario, wfTime, LineItemNum, grade_Rank, milEmpType, spe_Code_Occu_Series, cONUS_OCONUS, aviator)
					
'					'Get PPE Type Description set on save --- Steve B
'					Dim PPE_Typedescription As String = String.Empty
'					Dim loopCounter As Integer = 0
						
'					If pPE_Type.Length = 0
'							PPE_Typedescription = ""
'					Else
							
'						Dim selectedArray() As String = pPE_Type.Replace(" ", "").Split(",")
'						Dim types As List(Of String) = selectedArray.ToList()
						
'						For Each ppetype As String In types
'							If loopCounter = 0 Then
							
'								PPE_Typedescription = BRApi.Finance.Members.GetMember(si, dimtypeid.UD8, ppetype).Description 
							
'							Else
								
'								PPE_Typedescription = PPE_Typedescription & ", " & BRApi.Finance.Members.GetMember(si, dimtypeid.UD8, ppetype).Description
								
'							End If
							
'							loopCounter+=1
						
'						  Next
							
'					End If
					
'					'Write the annotations to the database
'					Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)	
'				 	'Show a message box that the Billet was successfully updated
'					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
'					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_Content_OS","OS_RP_OSDynamicCopy")
'					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue("prm_BLT_PPEType_Descr_OS", 				PPE_Typedescription)
'					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
'					selectionChangedTaskResult.IsOK = True
'					selectionChangedTaskResult.ShowMessageBox = True
'					selectionChangedTaskResult.Message = "" & GetDescription(si,RPname)	 & " " & GetUD6Description(si,LineItemNum) & " Successfully Updated"
'				 	Return selectionChangedTaskResult
					
'				End If 'EditBLTLine					End If ' Edit Mode
					
'End Function


'#End Region 


#Region "Copy_RP_Data"
Public Function Copy_RP_Data(
					ByVal si As SessionInfo,
					ByVal Cube As String,
					ByVal SourceScenario As String,
					ByVal TargetScenario As String,
					ByVal SourceRPName As String,
					ByVal TargetRPName As String)					
	Try
		' First delete any annotations of target RP
		Delete_RP_Annotations(si, Cube, TargetScenario, TargetRPName)

		' Copy  annotations from source RP
		Copy_RP_Annotations(si, Cube, SourceScenario, TargetScenario, SourceRPName, TargetRPName)
		
		Return Nothing
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function
#End Region 'Copy_RP_Attributes

#Region "Create_WorkingVersion_of_RP"
Public Function Create_WorkingVersion_of_RP(
					ByVal si As SessionInfo,
					ByVal Cube As String,
					ByVal SourceScenario As String,
					ByVal TargetScenario As String,
					ByVal RPShortName As String)
	Try
		' There can only be one working version for a given RP 
		' (In future we may open open up For multiple versions. For now we only support one version per RP)
		' It simply has _WV to it's name
		Dim WvRPShortName As String = RPShortName & "_WV"
		Dim WvRPLongName As String =  Get_RP_LongName(si, RPShortName) & "_WV"
		Dim createWV As Boolean = True

		
'		' First check is a working version already exists				
		Dim MemId As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Flow, WvRPShortName)
		If Not MemId = -1 Then
			Throw New Exception("Working version already exists for this RP: " & RPShortName)
		End If
		
		' It does not exist, create one
		' Create RP with _WV extension in the name and keep the title (description) same			
		Dim mbr As Member  = BRApi.Finance.Members.GetMember(si, dimTypeId.Flow, RPShortName)						
		Dim BudgetYear = Get_RP_Budget_Year(si, RPShortName)

		' Get RP Parent member name Name 
		Dim RPParentName As String = Get_BYRP_WV_Parent(si, BudgetYear)
		
		'Create a new RP  i.e Add a new flow memeber and set it's realtionship properties 
		Create_RP(si, WvRPLongName, mbr.Description, WvRPShortName, RPParentName)
		
		' Copy all the RP properties (annotations and data records)
		Copy_RP_Attributes(si, Cube, SourceScenario, TargetScenario, RPShortName, WvRPShortName, createWV)
		Return WvRPShortName
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function
#End Region 'Create_WorkingVersion_of_RP

#Region "Delete_RP"
Public Function Delete_RP(
					ByVal si As SessionInfo,
					ByVal Cube As String,
					ByVal Scenario As String,
					ByVal RPName As String)					
	Try
		'
		' 1. Clear any calculated costs 
		' 2. Delete all annotations
		' 3. Delete Flow Member
		' 

		' Clear costs
		' TO DO 

		'Delete annotations annotations
		Delete_RP_Annotations(si, Cube, Scenario, RPName)

		'Delete Flow member 
		'( TODO: In future check to make sure there is no data in other scenarios,times ..etc
		'   before flow member is deleted)
		Dim FlowDimPK As DimPk = BRApi.Finance.Dim.GetDimPk(si, "Std_Flow")
		Dim FlowMemberId As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Flow.Id, RPName)
		Dim FlowMemberPK As  MemberPk = New MemberPk(DimType.Flow.Id, FlowMemberId)
		BRApi.Finance.MemberAdmin.RemoveMember(si, FlowDimPK, FlowMemberPK)
					
		Return Nothing
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function
#End Region 'Delete_RP

#Region "Create_ModHierMem"
Public Function Create_ModHierMem(
				ByVal si As SessionInfo,
				ByVal modHierMemName As String,
				ByVal modHierMemTitle As String,
				ByVal modHierParentName As String,
				ByVal modHierMemText8 As String)
	Try
		
				Dim objDim As OneStream.Shared.Wcf.Dim = BRApi.Finance.Dim.GetDim(si, "Std_Flow")
				Dim objMemberPk As New MemberPk(DimType.Flow.Id, DimConstants.Unknown)		
										
				'Create New Members
				Dim objModsMember As New Member(objMemberPk, modHierMemName, modHierMemTitle, objDim.DimPk.DimId)		
				Dim objModProperties As New VaryingMemberProperties(DimType.Flow.Id, objModsMember.MemberId, DimConstants.Unknown)
				Dim NewFlowMbrProperties As FlowVMProperties = objModProperties.GetFlowProperties()
				'Setting the Text8 property with the 'Mod' tag
				NewFlowMbrProperties.Text8.SetStoredValue(DimConstants.Unknown,DimConstants.Unknown, modHierMemText8)
		
				'Save
				Dim objModsMemberInfo As New MemberInfo(objModsMember, objModProperties, Nothing, objDim,DimConstants.Unknown)
				Dim isNew As TriStateBool = TriStateBool.TrueValue
				BRApi.Finance.MemberAdmin.SaveMemberInfo(si, objModsMemberInfo, True, True, False, isNew)
				
				'Relationship Assignment
				Dim objModsId As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Flow.Id, modHierMemName)
				Dim ParentID As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Flow.Id, modHierParentName)
				Dim relPk As New RelationshipPk(DimType.Flow.Id, ParentID, objModsId)
				Dim rel As New Relationship(relPk, objDim.DimPk.DimId, RelationshipMovementType.InsertAsLastSibling, 1)
				Dim relInfo As New RelationshipInfo(rel, Nothing)
				Dim relPostionOpt As New RelationshipPositionOptions()
				                                                                                                
				 'Save the member Relationship and its properties.
				 BRApi.Finance.MemberAdmin.SaveRelationshipInfo(si, relInfo, relPostionOpt)
		 
				Return Nothing
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try

End Function
#End Region  'Create_Mod



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
		If  String.IsNullOrEmpty (LineItem) Then 
			Throw New Exception("Please choose a Line Item") 
		End If
'		RunPreSaveStepsForRP(si, Cube, Scenario, RPName, Reason_ChangeLog, Description_Changelog)
		
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Sub

Public Sub RunPreSaveStepsForRP(
					ByVal si As SessionInfo,
					ByVal Cube As String,
					ByVal Scenario As String,
					ByVal Time As String,
					ByVal RPName As String,
					ByVal Reason_ChangeLog As String,
					ByVal Description_Changelog As String)					
	Try
'		' 1. Make sure RP is is edit mode
'		' 2. If change comment is required, make sure comment is entered
'		' 3. Log change comment, if required

'		'Get the RPName and other parameters
		Dim RP_Entity = Get_Rp_Entity(si, RPName)					
		Dim rpId As Integer = BRApi.Finance.Members.GetMemberId(si, dimtypeid.Flow, RPName)
		Dim rpMode As String = BRApi.Finance.Flow.Text(si, rpId, 2, DimConstants.Unknown, DimConstants.Unknown)

		If rpMode.XFEqualsIgnoreCase("Mode_02")	Then
			Throw New Exception (RPName & " is set to View Only. No edits can be made.")
		End If
				
		Dim commentRequired As String = BRApi.Finance.Flow.Text(si, rpId, 3, DimConstants.Unknown, DimConstants.Unknown)
	 
		If commentRequired.XFEqualsIgnoreCase("CC_02") Then															
			
			If (description_ChangeLog.XFEqualsIgnoreCase("") And reason_ChangeLog.XFContainsIgnoreCase("OTH") )  Then
				Throw New Exception("Please enter a description for change comment.")
			End If				
			
'			Me.SetChangeLogComment(si, wfCube, RP_Entity, wfScenario, wfTime, RPName, LineItemNum, reason_ChangeLog, description_ChangeLog)												
							
		End If 'commentRequired		
			
		
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Sub
#End Region  'Public Functions and Subs

#Region "Private Functions and Subs"

#Region "Create_RP"
Public Sub Create_RP(
				ByVal si As SessionInfo,
				ByVal RPLongName As String,
				ByVal RPTitle As String,
				ByVal RPShortName As String,
				ByVal RPParentName As String)
	Try
		
		'First need to check and see if the workflow is locked, if so, then cannot create RPs in the workflow	
 		Dim wfPk As WorkflowUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si)	
 		Dim wfLocked As Boolean = BRApi.Workflow.Status.GetWorkflowStatus(si, wfPk, Nothing).Locked
		
		If wfLocked = True
			Throw New Exception("New RPs cannot be created since the workflow is locked")
		End If
		
		'Next need to check if users is in the scenario write group
		Dim scenarioReadWriteGrpId As Guid = BRApi.Finance.Members.GetMemberInfo(si, dimtypeid.Scenario, wfpk.ScenarioKey, True).Member.ReadWriteDataGroupUniqueID
		Dim userInScenarioReadWriteGrp As Boolean = BRApi.Security.Authorization.IsUserInGroup(si, scenarioReadWriteGrpId)
		
		If userInScenarioReadWriteGrp = False
			Throw New Exception("New RPs cannot be created by this user for this scenario")
		End If
		
		
		'Add RP to the Dimension		
		Dim objDim As OneStream.Shared.Wcf.Dim = BRApi.Finance.Dim.GetDim(si, "Std_Flow")
		Dim objMemberPk As New MemberPk(DimType.Flow.Id, DimConstants.Unknown)		
		
		
		'Create New Member
		Dim objMember As New Member(objMemberPk, RPShortName, RPTitle, objDim.DimPk.DimId)
		
		'Set initial text1 tag for RP ( i.e Create Status, Edit Mode, No change comment required)
		' Below flags correcpond to UD8 members .
		Dim ScenarioTypeId As Integer = BRApi.Finance.Scenario.GetScenarioType(si, wfPk.ScenarioKey).Id
			
		Dim Text1_Value As String = String.Empty
		'If it is an Ann or Trm RP, set the Status to Budget (STATUS_03)
		If (RPShortName.XFContainsIgnoreCase("_Ann") Or RPShortName.XFContainsIgnoreCase("Trm"))
			Text1_Value = RP_STATUS_BUDGET & "|" & RP_MODE_EDIT & "|" & RP_CC_NOT_REQD  
		Else 
			Text1_Value = RP_STATUS_CREATE & "|" & RP_MODE_EDIT & "|" & RP_CC_NOT_REQD 
		End If
		
		Dim objProperties As New VaryingMemberProperties(DimType.Flow.Id, objMember.MemberId, DimConstants.Unknown)
		objProperties.GetFlowProperties.Text1.SetStoredValue(ScenarioTypeId, wfPk.TimeKey, Text1_Value)
		objProperties.GetFlowProperties.Text8.SetStoredValue(DimConstants.Unknown, DimConstants.Unknown, RPLongName)

		
		'Save
		Dim objMemberInfo As New MemberInfo(objMember, objProperties, Nothing, objDim,DimConstants.Unknown)
		Dim isNew As TriStateBool = TriStateBool.TrueValue
		BRApi.Finance.MemberAdmin.SaveMemberInfo(si, objMemberInfo, True, True, False, isNew)
		
		'Relationship Assignment
'       Dim objMemberId As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Flow.Id, RPShortName)
        Dim objMemberId As Integer= BRApi.Finance.Members.GetMemberId(si, DimType.Flow.Id,  objMemberInfo.Member.Name)
		Dim ParentID As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Flow.Id, RPParentName)
        Dim relPk As New RelationshipPk(DimType.Flow.Id, ParentID, objMemberId)
        Dim rel As New Relationship(relPk, objDim.DimPk.DimId, RelationshipMovementType.InsertAsLastSibling, 1)
        Dim relInfo As New RelationshipInfo(rel, Nothing)
        Dim relPostionOpt As New RelationshipPositionOptions()
                                                                                                        
         'Save the member Relationship and its properties.
         BRApi.Finance.MemberAdmin.SaveRelationshipInfo(si, relInfo, relPostionOpt)
		 
		 
'		'Show a message box that the RP was successfully created
'		Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
'		selectionChangedTaskResult.IsOK = True
'		selectionChangedTaskResult.ShowMessageBox = True
'		selectionChangedTaskResult.Message = "Resource Proposal " & RPName & " Successfully Created"
'		Return selectionChangedTaskResult
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try

End Sub
#End Region  'Create_RP

#Region "Copy_RP_Annotations"
' Created another function Copy_RP_Annotations to allow Polymorphism so different actions can occur based off if the source is greater than the target send the extra argument consists of the Target billet count, TargetBillets.
' This overloaded function called when Source is greater than the target.
Private Function Copy_RP_Annotations(
					ByVal si As SessionInfo,
					ByVal Cube As String,
					ByVal SourceScenario As String,
					ByVal TargetScenario As String,
					ByVal SourceRPName As String,
					ByVal TargetRPName As String,
					ByVal TargetBillets As Integer,
					ByVal RPCopyAnnotations As Boolean
	              )
					
	Try
		Dim TargetRPEntity As String = Get_RP_Entity(si, TargetRPName)
		Dim TargetRPTime As String = Get_RP_Budget_Year(si, TargetRPName)
		Dim TargetYear As String = Get_RP_Budget_Year(si, TargetRPName)
	
		Dim wfTime As String = ("WFTime")
		Dim wfScenario As String = ("WFScenario")
		Dim wfCube As String = ("WFCube")
		Dim SourceRPEntity As String = Get_RP_Entity(si, SourceRPName)
	 
		'Step 1: Delte Data Attchments (Annotations ..etc) of Target RP 
         Delete_RP_Annotations(si, Cube, TargetScenario, TargetRPName)
		
		' Create a new list of memberscript and value and add memebers
		
		
		Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)
        'Altered the sql query to limit the items copied from source to equal up to the target count when source > target. 
		Dim sql As New Text.StringBuilder
			sql.Append("SELECT ")
			sql.Append("	Cube, Entity, Parent, Cons, Scenario, Time, ")
			sql.Append("	Account, Flow, Origin, IC,	")
			sql.Append("	UD1, UD2, UD3, UD4, UD5, UD6, UD7, UD8, ")
			sql.Append("	Title, Text, FileName ")
            sql.Append("FROM dbo.DataAttachment  WITH(NOLOCK) ")
            sql.Append("WHERE Cube = '" & Cube & "' ")
            sql.Append("  and Scenario = '" & SourceScenario & "' ")
			sql.Append (" and ((UD6 = 'NONE' OR UD6 like 'NB%' or UD6 like 'Gen%' or UD6 like 'Exp%' or (UD6 like 'LineItem%' and Convert(INT, substring(UD6, 10, 3))  <=" & TargetBillets & ")) and (Flow = '" & SourceRPName & "'))" )
            If RPCopyAnnotations Then
				sql.Append("  and Account not in('Description_ChangeLog','Reference_Doc')")
			Else
				'dont copy Related RPs
				sql.Append("  and Account not in('Description_ChangeLog','Reference_Doc','FY_Related_RP1','FY_Related_RP2','FY_Related_RP3','Older_Related_RP1','Older_Related_RP2','Older_Related_RP3')")
			End If
            sql.Append("  and ( ")
			sql.Append("    not (Account like '%ConcReview%' and UD8 like 'Comment%') ")
			sql.Append("  ) ")
			sql.Append("  and ( ")
			sql.Append("	not (Account = 'CCR_TF') ")
			sql.Append("  ) ")
		
			
			Dim sqlStmt As String = sql.ToString
			
			
			
			Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
	        	Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp, sqlStmt, True)
	           Dim numRows As Integer = dt.Rows.Count
			   
           
				Dim rowIndex As Integer = 0
				If dt IsNot Nothing AndAlso NumRows < 1 Then
					Return Nothing
				End If
				
			    Dim baseScript As String =  
								"E#" & targetRPEntity	& ":" & 
								"S#" & TargetScenario 	& ":" & 
								"T#" & targetRPTime		& ":" & 
								"F#" & targetRPName		& ":" & 
								"V#Annotation"
				Dim specificScript = ""
				Dim memberScript = ""
				Dim TextValue = "" 
				Dim Ud6 = ""
				Dim Account = ""
				Do  
					'Read column values for each row and construct script
					specificScript = 
							"C#"  & dt.Rows(rowIndex)("Cons")	& ":" & 
							"A#"  & dt.Rows(rowIndex)("Account")& ":" & 
							"O#"  & dt.Rows(rowIndex)("Origin") & ":" & 
							"I#"  & dt.Rows(rowIndex)("IC")		& ":" & 
							"U1#" & dt.Rows(rowIndex)("UD1") 	& ":" & 
							"U2#" & dt.Rows(rowIndex)("UD2") 	& ":" & 
							"U3#" & dt.Rows(rowIndex)("UD3") 	& ":" &
							"U4#" & dt.Rows(rowIndex)("UD4") 	& ":" & 
							"U5#" & dt.Rows(rowIndex)("UD5") 	& ":" & 
							"U6#" & dt.Rows(rowIndex)("UD6") 	& ":" & 
							"U7#" & dt.Rows(rowIndex)("UD7") 	& ":" & 
							"U8#" & dt.Rows(rowIndex)("UD8") 	& ":"  
						
							Account = dt.Rows(rowIndex)("Account")
							If Account.Equals("Number_of_Billets")
								textValue = TargetBillets.ToString()
							Else
							     textValue = dt.Rows(rowIndex)("Text")
						   End If 
						
							
							
					memberScript = baseScript & ":" & specificScript
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(Cube, memberScript, 0, True, textValue))						
					
					rowIndex = rowIndex + 1
					specificScript = ""
			
				Loop While rowIndex < numRows
				
           
			  
					
			
			End Using				
		 
			'Write the annotations to the database
			 Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)
		
		Return Nothing	
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function


#End Region  'Copy_RP_Annotations


#Region "Copy_RP_Annotations"
'Overloaded funtion is called when source < target
'Copy_RP_Annotations(si, Cube, SourceScenario, TargetScenario, SourceRPName, TargetRPName,RPCopyAnnotations)

Private Function Copy_RP_Annotations(
					ByVal si As SessionInfo,
					ByVal Cube As String,
					ByVal SourceScenario As String,
					ByVal TargetScenario As String,
					ByVal SourceRPName As String,
					ByVal TargetRPName As String,
					ByVal RPCopyAnnotations As Boolean
					)
					
	Try
		
		Dim TargetRPEntity As String = Get_RP_Entity(si, TargetRPName)
		Dim TargetRPTime As String = Get_RP_Budget_Year(si, TargetRPName)
		Dim TargetYear As String = Get_RP_Budget_Year(si, TargetRPName)
	    Dim SourceYear As String = Get_RP_Budget_Year(si, SourceRPName)
		Dim wfTime As String = ("WFTime")
		Dim wfScenario As String = ("WFScenario")
		Dim wfCube As String = ("WFCube")
		Dim SourceRPEntity As String = Get_RP_Entity(si, SourceRPName)
		' Create a new list of memberscript and value and add memebers
		Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)
		
	
         Dim TargetBillets As Integer = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, Cube, "A#Number_of_Billets:E#" & TargetRPEntity & ":S#" & TargetScenario & ":T#" & TargetYear & ":V#Annotation:F#" & TargetRPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").DataCellEx.DataCellAnnotation.XFConvertToInt
         Dim SourceBillets As Integer = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, Cube, "A#Number_of_Billets:E#" & SourceRPEntity & ":S#" & SourceScenario & ":T#" & SourceYear & ":V#Annotation:F#" & SourceRPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").DataCellEx.DataCellAnnotation.XFConvertToInt

	
		 'clear out target annotations.
		 Delete_RP_Annotations(si, Cube, TargetScenario, TargetRPName)
        
        Dim sql As New Text.StringBuilder
			sql.Append("SELECT ")
			sql.Append("	Cube, Entity, Parent, Cons, Scenario, Time, ")
			sql.Append("	Account, Flow, Origin, IC,	")
			sql.Append("	UD1, UD2, UD3, UD4, UD5, UD6, UD7, UD8, ")
			sql.Append("	Title, Text, FileName ")
            sql.Append("FROM dbo.DataAttachment  WITH(NOLOCK) ")
            sql.Append("WHERE Cube = '" & Cube & "' ")
            sql.Append("  and Scenario = '" & SourceScenario & "' ")
			sql.Append (" and ((UD6 = 'NONE' OR UD6 like 'NB%' or UD6 like 'Gen%' or UD6 like 'Exp%' or (UD6 like 'LineItem%' and Convert(INT, substring(UD6, 10, 3))  <=" & SourceBillets & " ))  And (Flow = '" & SourceRPName & "'))" )
          	If RPCopyAnnotations Then
				sql.Append("  and Account not in('Description_ChangeLog','Reference_Doc')")
			Else
				'dont copy Related RPs
				sql.Append("  and Account not in('Description_ChangeLog','Reference_Doc','FY_Related_RP1','FY_Related_RP2','FY_Related_RP3','Older_Related_RP1','Older_Related_RP2','Older_Related_RP3')")
			End If
            sql.Append("  and ( ")
			sql.Append("    not (Account like '%ConcReview%' and UD8 like 'Comment%') ")
			sql.Append("  ) ")
			sql.Append("  and ( ")
			sql.Append("	not (Account = 'CCR_TF') ")
			sql.Append("  ) ")

			
			Dim sqlStmt As String = sql.ToString
			'BrApi.ErrorLog.LogMessage (si, "sqlStmt:" + sqlStmt)
			
			
			Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
	        	Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp, sqlStmt, True)
	            Dim numRows As Integer = dt.Rows.Count
				Dim rowIndex As Integer = 0
				If dt IsNot Nothing AndAlso NumRows < 1 Then
					Return Nothing
				End If
				
			    Dim baseScript As String =  
								"E#" & targetRPEntity	& ":" & 
								"S#" & TargetScenario 	& ":" & 
								"T#" & targetRPTime		& ":" & 
								"F#" & targetRPName		& ":" & 
								"V#Annotation"
				Dim specificScript = ""
				Dim memberScript = ""
				Dim TextValue = "" 
				Dim Account = ""
				
				Do
					'Read column values for each row and construct script
					specificScript = 
							"C#"  & dt.Rows(rowIndex)("Cons")	& ":" & 
							"A#"  & dt.Rows(rowIndex)("Account")& ":" & 
							"O#"  & dt.Rows(rowIndex)("Origin") & ":" & 
							"I#"  & dt.Rows(rowIndex)("IC")		& ":" & 
							"U1#" & dt.Rows(rowIndex)("UD1") 	& ":" & 
							"U2#" & dt.Rows(rowIndex)("UD2") 	& ":" & 
							"U3#" & dt.Rows(rowIndex)("UD3") 	& ":" &
							"U4#" & dt.Rows(rowIndex)("UD4") 	& ":" & 
							"U5#" & dt.Rows(rowIndex)("UD5") 	& ":" & 
							"U6#" & dt.Rows(rowIndex)("UD6") 	& ":" & 
							"U7#" & dt.Rows(rowIndex)("UD7") 	& ":" & 
							"U8#" & dt.Rows(rowIndex)("UD8") 	& ":"  
				
							Account = dt.Rows(rowIndex)("Account")
							If Account.Equals("Number_of_Billets")
								textValue = TargetBillets.ToString()
							Else
							     textValue = dt.Rows(rowIndex)("Text")
						   End If 	
							
					memberScript = baseScript & ":" & specificScript
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(Cube, memberScript, 0, True, textValue))						
					
					rowIndex = rowIndex + 1
					specificScript = ""
				Loop While rowIndex < numRows 			
			End Using				

			'Write the annotations to the database
			Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)			
		Return Nothing	
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function
#End Region  'Copy_RP_Annotations



#Region "Copy_RP_Annotations"
Private Function Copy_RP_Annotations(
					ByVal si As SessionInfo,
					ByVal Cube As String,
					ByVal SourceScenario As String,
					ByVal TargetScenario As String,
					ByVal SourceRPName As String,
					ByVal TargetRPName As String)
										
	Try

		Dim TargetRPEntity As String = Get_RP_Entity(si, TargetRPName)
		Dim TargetRPTime As String = Get_RP_Budget_Year(si, TargetRPName)
		' Create a new list of memberscript and value and add memebers
		Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)
		
		Delete_RP_Annotations(si, Cube, TargetScenario, TargetRPName)
		
       Dim sql As New Text.StringBuilder
			sql.Append("SELECT ")
			sql.Append("	Cube, Entity, Parent, Cons, Scenario, Time, ")
			sql.Append("	Account, Flow, Origin, IC,	")
			sql.Append("	UD1, UD2, UD3, UD4, UD5, UD6, UD7, UD8, ")
			sql.Append("	Title, Text, FileName ")
            sql.Append("FROM dbo.DataAttachment  WITH(NOLOCK) ")
            sql.Append("WHERE Cube = '" & Cube & "' ")
            sql.Append("  and Scenario = '" & SourceScenario & "' ")
            sql.Append("  and Flow = '" & SourceRPName & "' ")
            sql.Append("  and Account not in ('Description_ChangeLog', 'Reference_Doc')")
            sql.Append("  and ( ")
			sql.Append("    not (Account like '%ConcReview%' and UD8 like 'Comment%') ")
			sql.Append("      or ")
			sql.Append("    (Account = 'CCR_TF') ")
			sql.Append("      or ")
			sql.Append("    (Account like '%ConcReview%' and UD8 not like 'Comment%') ")
			sql.Append("  ) ")

			
			Dim sqlStmt As String = sql.ToString
			
			
			Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
	        	Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp, sqlStmt, True)
	            Dim numRows As Integer = dt.Rows.Count
				Dim rowIndex As Integer = 0
				If dt IsNot Nothing AndAlso NumRows < 1 Then
					Return Nothing
				End If
				
			    Dim baseScript As String =  
								"E#" & targetRPEntity	& ":" & 
								"S#" & TargetScenario 	& ":" & 
								"T#" & targetRPTime		& ":" & 
								"F#" & targetRPName		& ":" & 
								"V#Annotation"
				Dim specificScript = ""
				Dim memberScript = ""
				Dim TextValue = "" 
				
				
				Do
					'Read column values for each row and construct script
					specificScript = 
							"C#"  & dt.Rows(rowIndex)("Cons")	& ":" & 
							"A#"  & dt.Rows(rowIndex)("Account")& ":" & 
							"O#"  & dt.Rows(rowIndex)("Origin") & ":" & 
							"I#"  & dt.Rows(rowIndex)("IC")		& ":" & 
							"U1#" & dt.Rows(rowIndex)("UD1") 	& ":" & 
							"U2#" & dt.Rows(rowIndex)("UD2") 	& ":" & 
							"U3#" & dt.Rows(rowIndex)("UD3") 	& ":" &
							"U4#" & dt.Rows(rowIndex)("UD4") 	& ":" & 
							"U5#" & dt.Rows(rowIndex)("UD5") 	& ":" & 
							"U6#" & dt.Rows(rowIndex)("UD6") 	& ":" & 
							"U7#" & dt.Rows(rowIndex)("UD7") 	& ":" & 
							"U8#" & dt.Rows(rowIndex)("UD8") 	& ":"  
							
							textValue = dt.Rows(rowIndex)("Text")
							
					memberScript = baseScript & ":" & specificScript
					lstMemberScriptAndValue.Add(New MemberScriptAndValue(Cube, memberScript, 0, True, textValue))						
					
					rowIndex = rowIndex + 1
					specificScript = ""
				Loop While rowIndex < numRows 			
			End Using				

			'Write the annotations to the database
			Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)			
		Return Nothing	
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function
#End Region  'Copy_RP_Annotations


#Region "Delete_RP_Annotations"
Private Function Delete_RP_Annotations(
					ByVal si As SessionInfo,
					ByVal Cube As String,
					ByVal Scenario As String,
					ByVal RPName As String)					
	Try
        Dim sql As New Text.StringBuilder
			sql.Append("DELETE FROM dbo.DataAttachment ")
            sql.Append(" WHERE Cube = '" & Cube & "' ")
            sql.Append(" AND Scenario = '" & Scenario & "' ")
            sql.Append(" AND Flow = '" & RPName & "' ")
			sql.Append(" and Not (Account Like '%ConcReview%') ")
			sql.Append(" and Not (Account = 'CCR_TF') ")
			sql.Append(" and Not (UD8 Like 'Comment%') ")
			sql.Append(" and Account NOT IN( 'Description_ChangeLog', 'Reference_Doc' )")
		

			Dim sqlStmt As String = sql.ToString
			BrApi.ErrorLog.LogMessage (si, sqlStmt)
							
			Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
	        	Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp, sqlStmt, True)
			End Using
		Return Nothing
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function
#End Region  'Delete_RP_Annotations

#Region "Get_NextSequenceID"
Private Function Get_NextSequenceID(
					ByVal si As SessionInfo,
					ByVal WFTime As String,
					ByVal LeadDirectorateName As String,
					ByVal RP_ParentName As String)
	Try
'		'Get Time from current Workflow
'		Dim wfTime As String = args.NameValuePairs("WFTime")
'		'retrieve the WF name property from the WF
'		Dim RPLeadDirect As String = args.NameValuePairs("RPLeadDirect")

'	    Dim LeadDirectorateName As String = Get_RP_LDName(si, RPEntity)															
'		Dim RP_ParentName As String = Get_RP_Parent(si, WFTime)
'		Dim RPAppr As String = args.NameValuePairs("RPAppr")
'		Dim RPBudCat As String = args.NameValuePairs("RPBudCat")
'		'Dim RPSuffix As String = args.NameValuePairs("RPSuffix")
'		'Dim RPNumber As String = args.NameValuePairs("RPNumber")
'		Dim RPTitle As String = args.NameValuePairs("RPTitle")

'		Dim RP_String As String 					= "RP"
'		Dim RP_Year As String 						= "FY_" & wfTime
'		Dim RP_EntitySuffix As String 				= LeadDirect(1)
'		Dim RP_Appr As String 						= args.NameValuePairs("RPAppr")
'		Dim RP_BudgetCategory As String 			= args.NameValuePairs("RPBudCat")
		'Dim RP_PlaceHolder_Identifier As String 	= "9999"
		'Dim RP_Number As String 					= "_" & args.NameValuePairs("RPNumber")				
'		Dim RP_Suffix As String						= "00" 'hardcoding this for RP's created from scratch
		
		'Get a unique sequence number to assign to the RP depending on the workflow entity: DCO 200-399, DCMS 400-599, CG9 600-699, CG8 700-799, Overflow 800-990
		'Establis the lead office prfix for the Sequence when starting a new year

		Dim leadOfficePrefix As String = String.Empty
		If LeadDirectorateName.XFEqualsIgnoreCase("DCO")
			leadOfficePrefix = "2"
		Else If LeadDirectorateName.XFEqualsIgnoreCase("DCMS")
			leadOfficePrefix = "4"
		Else If LeadDirectorateName.XFEqualsIgnoreCase("DCS") ''' new lead 
			leadOfficePrefix = "5"	
		Else If LeadDirectorateName.XFEqualsIgnoreCase("CG9") 
			leadOfficePrefix = "6"
		Else If LeadDirectorateName.XFEqualsIgnoreCase("DCP") ''' new lead
			leadOfficePrefix = "6"
		Else If LeadDirectorateName.XFEqualsIgnoreCase("CG8")
			leadOfficePrefix = "7"
		End If
		
		'Establish the list of existing sequences used
		Dim usedSequencesList As New List (Of String)
		Dim BudFm_FlowDim As OneStream.Shared.Wcf.Dim = BRApi.Finance.Dim.GetDim(si, "Std_Flow")			
'		Dim existingRPMemList As List (Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BudFm_FlowDim.DimPk, "F#" & RP_Year & "_RPS.Base.Where(Name Contains " & RP_EntitySuffix & ")", True)
'		Dim existingRPMemList As List (Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BudFm_FlowDim.DimPk, "F#" & RP_parentName & ".Base.Where(Name Contains " & LeadDirectorateName & " )", True)
		Dim existingRPMemList As List (Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BudFm_FlowDim.DimPk, "F#" & RP_ParentName & ".Base.Where(Text8 Contains " & LeadDirectorateName & " )", True)
		Dim NextSequence As String = String.Empty
		
		'If the existing list is not nothing, create the existing list
		If (Not existingRPMemList Is Nothing AndAlso existingRPMemList.Count <> 0) Then
			For Each existingRPMem As MemberInfo In existingRPMemList
				'Get the sequence number from the RP and add it to the list
				Dim existingRPMemSplit() As String = existingRPMem.Member.Name.Split("_")
				Dim uniqueId As String = existingRPMemSplit(1)
				'Add it to the list
				usedSequencesList.Add(uniqueId)
			Next 
			
			'Sort the list and get the last number in it and add a 1 to this because it will be the next number to assign			
			usedSequencesList.Sort()
			Dim currLastSequence As Integer = usedSequencesList.Last().XFConvertToInt()
			NextSequence = (currLastSequence + 1).ToString
			
		Else 'The existing list for this year is nothing so start with an #000 Sequence
			NextSequence = leadOfficePrefix & "000"
		End If
		
		Return NextSequence
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function
#End Region  'Get_NextSequenceID

#Region "Get_RPLongName"
Public Function Get_RP_LongName(
						ByVal si As SessionInfo,
						ByVal RPShortName As String
						) 
					
	Try
		Dim RPLongName As String
		If globals.GetStringValue($"RPShortName_LongName_{RPShortName}",String.Empty) = String.Empty Then
			'BrApi.ErrorLog.LogMessage (si, "RPShort Name 1" & RPShortName)

			Dim RPMemId As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Flow, RPShortName)
			If RPmemId = -1 Then
				Throw New Exception ("RP does not exist " &RPShortName)
			End If
			Dim RPMemberInfo As MemberInfo = BRApi.Finance.Members.GetMemberInfo(si, dimtypeid.Flow, RPMemId, True)
			RPLongName = RPMemberInfo.GetFlowProperties.Text8.GetStoredValue(DimConstants.Unknown, DimConstants.Unknown)
			globals.SetStringValue($"RPShortName_LongName_{RPShortName}",RPLongName)
		Else
			RPLongName = globals.GetStringValue($"RPShortName_LongName_{RPShortName}",String.Empty)
		End If
			
			'BrApi.ErrorLog.LogMessage (si, "RPLong Name 2" & RPLongName)
		Return RPLongName
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function
#End Region 'Get_RPLongName

#Region "Get_RP_Text_Value"
Private Function Get_RP_Text_Value (ByVal si As SessionInfo,
							ByVal RPShortName As String,
							ByVal TextIndex As Integer) As String
	Try
		' Check to make sure RP name is not Empty String
		If RPShortName = "" Then
			Throw New Exception("RP Name is Empty")
		End If 
		
		If (TextIndex < 1) Or (TextIndex > 8) Then
			Throw New Exception("Invalid Text Index, It should be in the range 1 - 8")
		End If 
		
		Dim RPId As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Flow.Id, RPShortName)

		If TextIndex < 8 
			Dim wfPk As WorkflowUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si)
			Dim ScenarioTypeId As Integer = BRApi.Finance.Scenario.GetScenarioType(si, wfPk.ScenarioKey).Id
			Return  BRApi.Finance.Flow.Text(si, RPId, TextIndex, ScenarioTypeId, wfPk.TimeKey)
		Else 
			Return  BRApi.Finance.Flow.Text(si, RPId, TextIndex, DimConstants.Unknown, DimConstants.Unknown)
		End If
		
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function 'Get_RP_Text_Value
#End Region 'Get_RP_Text_Value"

#Region "Parse_RP_Text_Property"
Public Function Parse_RP_Text_Property(ByVal si As SessionInfo,
							ByVal Text As String, 
							ByVal ReturnStringAtIndex As String) As String
	Try
		' If Text1 is empty, nothing to do 
		If (Text  = "") Then
			Return ""
		Else
			Return StringHelper.SplitString(Text,"|")(ReturnStringAtIndex)	
		End If
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function 'Parse_Text
#End Region 'Parse_Text1

#Region "Get_RP_Status"
Private Function Get_RP_Status (ByVal si As SessionInfo, ByVal RPShortName As String) As String
	Try
		' Check to make sure RP name is not Empty String
		If RPShortName ="" Then
			Throw New Exception("RP Name is Empty")
		End If 
		
		' Get the Text tag value stored against Text1 for a given scenarion and time.
		Dim Text1_Value As String = Get_RP_Text_Value(si, RPShortName, 1)
		
		' Parse the text property get the token at index 0
		Dim RPStatus As String = Parse_RP_Text_Property(si, Text1_Value, 0)
		
		Return RPStatus
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function 'Get_RP_Status
#End Region 'Get_RP_Status

#Region "Get_RP_Mode"
Private Function Get_RP_Mode (ByVal si As SessionInfo, ByVal RPShortName As String) As String
	Try
		BRAPI.ErrorLog.LogMessage(si,$"Hit {RPShortName}")
		' Check to make sure RP name is not Empty String
		If RPShortName ="" Then
			Throw New Exception("RP Name is Empty")
		End If 
		
		' Get the Text tag value stored against Text1 for a given scenarion and time.
		Dim Text1_Value As String = Get_RP_Text_Value(si, RPShortName, 1)
		
		' Parse the text property get the token at index 1
		Dim RPMode As String = Parse_RP_Text_Property(si, Text1_Value, 1)
		
		Return RPMode
		
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function 'Get_RP_Mode
#End Region 'Get_RP_Mode

#Region "Get_RP_CC_Required"
Public Function Get_RP_CC_Required (ByVal si As SessionInfo, ByVal RPShortName As String) As String
	Try
		Brapi.ErrorLog.LogMessage(si,$"Hit {RPShortName}")
		' Check to make sure RP name is not Empty String
		If RPShortName ="" Then
			Throw New Exception("RP Name is Empty")
		End If 
		
		' Get the Text tag value stored against Text1 for a given scenarion and time.
		Dim Text1_Value As String = Get_RP_Text_Value(si, RPShortName, 1)
		
		' Parse the text property get the token at index 2
		Dim RPCCReq As String = Parse_RP_Text_Property(si, Text1_Value, 2)
		
		Return RPCCReq
				
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function 'Get_RP_CC_Required
#End Region 'Get_RP_CC_Required

#Region "Rename RPs"
Private Function Rename_RPs_Onetime(
					ByVal si As SessionInfo)
	Try

			Dim RPParent As String = "FY_2025_RPS"
			Dim RPParentId As Integer = BRApi.Finance.Members.GetMemberId(si, dimtypeid.Flow, RPParent)
		
			Dim BudFm_FlowDim As OneStream.Shared.Wcf.Dim = BRApi.Finance.Dim.GetDim(si, "Std_Flow")
			Dim RPNameList As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si, BudFm_FlowDim.DimPk, RPParentId, Nothing)
			
			For Each RPMember As Member In RPNameList	
						
				'Get the RPName
				Dim RPLongName As String = RPMember.Name
				
				If  RPLongName.StartsWith("RP_FY_") Then
								
					Dim RPLongNameSplit() As String = RPLongName.Split("_")
					Dim TwoDigitYear As Integer = RPLongNameSplit(2) - 2000
					Dim RPNumber As String = RPLongNameSplit(6)
					Dim Suffix As String = RPLongNameSplit(7)
					
					Dim RPShortName = TwoDigitYear & "_" & RPNumber & "_" & Suffix
		
					
					'If not blank (status has changed) then proceed
					Dim RPId As Integer = RPMember.MemberId
					Dim RPPk As New MemberPk(BudFm_FlowDim.DimPk.DimTypeId, RPId)
					Dim RPMemberInfo As MemberInfo = BRApi.Finance.Members.GetMemberInfo(si, dimtypeid.Flow, RPId, True)
					Dim RPDesc As String = RPMember.Description
					Dim RPToUpdate As New Member(RPPk,RPShortName,RPDesc,BudFm_FlowDim.DimPk.DimId)
					Dim RPVarProps As VaryingMemberProperties = RPMemberInfo.Properties
					Dim RPToUpdateInfo As New MemberInfo(RPToUpdate,RPVarProps,Nothing,BudFm_FlowDim, DimConstants.Unknown)
					Dim RPMemberProperties As FlowVMProperties = RPToUpdateInfo.GetFlowProperties()
					Dim currentStatus As String = RPMemberProperties.Text1.GetStoredValue(DimConstants.Unknown, DimConstants.Unknown)
																						
					'Set the New Status
					RPMemberProperties.Text8.SetStoredValue(DimConstants.Unknown, DimConstants.Unknown, RPLongName)
					
					'Save the member
					BRapi.Finance.MemberAdmin.SaveMemberInfo(si, RPToUpdateInfo, True, True, False, False)

'					BrApi.ErrorLog.LogMessage (si, " Long Name: " & RPLongName )
'					BrApi.ErrorLog.LogMessage (si, " Short Name: " &  RPShortName )
				End If
			Next 
		Return Nothing
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function
#End Region  'Rename_RPs_OneTime

#Region "Re_Arrange_Text1_Properties"
Private Function Re_Arrange_Text1_Properties(
					ByVal si As SessionInfo)
	Try

			Dim RPParent As String = "FY_2024_RPS"
			Dim RPParentId As Integer = BRApi.Finance.Members.GetMemberId(si, dimtypeid.Flow, RPParent)
		
			Dim BudFm_FlowDim As OneStream.Shared.Wcf.Dim = BRApi.Finance.Dim.GetDim(si, "Std_Flow")
			Dim RPNameList As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si, BudFm_FlowDim.DimPk, RPParentId, Nothing)
			
			For Each RPMember As Member In RPNameList	
						
				'Get the RPName
				Dim RPLongName As String = RPMember.Name
				
'				If  RPLongName.StartsWith("RP_FY_") Then
								
'					Dim RPLongNameSplit() As String = RPLongName.Split("_")
'					Dim TwoDigitYear As Integer = RPLongNameSplit(2) - 2000
'					Dim RPNumber As String = RPLongNameSplit(6)
'					Dim Suffix As String = RPLongNameSplit(7)
					
'					Dim RPShortName = TwoDigitYear & "_" & RPNumber & "_" & Suffix
		
					
					'If not blank (status has changed) then proceed
					Dim RPId As Integer = RPMember.MemberId
					Dim RPShortName As String = RPMember.Name
					Dim RPPk As New MemberPk(BudFm_FlowDim.DimPk.DimTypeId, RPId)
					Dim RPMemberInfo As MemberInfo = BRApi.Finance.Members.GetMemberInfo(si, dimtypeid.Flow, RPId, True)
					Dim RPDesc As String = RPMember.Description
					Dim RPToUpdate As New Member(RPPk,RPShortName,RPDesc,BudFm_FlowDim.DimPk.DimId)
					Dim RPVarProps As VaryingMemberProperties = RPMemberInfo.Properties
					Dim RPToUpdateInfo As New MemberInfo(RPToUpdate,RPVarProps,Nothing,BudFm_FlowDim, DimConstants.Unknown)
					Dim RPMemberProperties As FlowVMProperties = RPToUpdateInfo.GetFlowProperties()
'					Dim currentStatus As String = RPMemberProperties.Text1.GetStoredValue(DimConstants.Unknown, DimConstants.Unknown)
'					Dim CurrentMode As String = RPMemberProperties.Text2.GetStoredValue(DimConstants.Unknown, DimConstants.Unknown)
'					Dim CurrentCCReqdFlag As String = RPMemberProperties.Text3.GetStoredValue(DimConstants.Unknown, DimConstants.Unknown)
																						
'					'Set the New Status
'					Dim New_Text1 As String = RP_STATUS_CREATE & "|" & RP_MODE_EDIT & "|" & RP_CC_NOT_REQD
			 		Dim wfPk As WorkflowUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si)
					Dim ScenarioTypeId As Integer = BRApi.Finance.Scenario.GetScenarioType(si, wfPk.ScenarioKey).Id
'					RPMemberProperties.Text1.SetStoredValue(ScenarioTypeId, wfPk.TimeKey, New_Text1)
					RPMemberProperties.Text1.RemoveStoredPropertyItem(ScenarioTypeId, wfPk.TimeKey)
'					RPMemberProperties.Text1.SetStoredValue(DimConstants.Unknown, DimConstants.Unknown, String.Empty)					
'					RPMemberProperties.Text2.SetStoredValue(DimConstants.Unknown, DimConstants.Unknown, String.Empty)					
'					RPMemberProperties.Text3.SetStoredValue(DimConstants.Unknown, DimConstants.Unknown, String.Empty)					
'					RPMemberProperties.Text4.SetStoredValue(DimConstants.Unknown, DimConstants.Unknown, String.Empty)					
'					RPMemberProperties.Text5.SetStoredValue(DimConstants.Unknown, DimConstants.Unknown, String.Empty)					
'					RPMemberProperties.Text6.SetStoredValue(DimConstants.Unknown, DimConstants.Unknown, String.Empty)					
'					RPMemberProperties.Text7.SetStoredValue(DimConstants.Unknown, DimConstants.Unknown, String.Empty)					
					'Save the member
					BRapi.Finance.MemberAdmin.SaveMemberInfo(si, RPToUpdateInfo, False, True, False, False)

'					BrApi.ErrorLog.LogMessage (si, " Long Name: " & RPLongName )
					BrApi.ErrorLog.LogMessage (si, " Short Name: " &  RPShortName )
'				End If
			Next 
		Return Nothing
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function 'Re_Arrange_Text1_Properties
#End Region  'Re_Arrange_Text1_Properties

#Region "Copy_RP_Text_Properties"
Private Function Copy_RP_Text_Properties(
					ByVal si As SessionInfo,
					ByVal RPParent As String,
					ByVal SourceScenario As String,
					ByVal TargetScenario As String,
					ByVal SourceYear As String,
					ByVal TargetYear As String)
	Try
		
		Dim RPParentId As Integer = BRApi.Finance.Members.GetMemberId(si, dimtypeid.Flow, RPParent)
	
		Dim BudFm_FlowDim As OneStream.Shared.Wcf.Dim = BRApi.Finance.Dim.GetDim(si, "Std_Flow")
		Dim RPNameList As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si, BudFm_FlowDim.DimPk, RPParentId, Nothing)

		' Source Info
		Dim SourceScenarioId As Integer = BRApi.Finance.Members.GetMemberId(si, DimTypeId.Scenario, SourceScenario)
		Dim SourceScenarioTypeId As Integer = BRApi.Finance.Scenario.GetScenarioType(si, SourceScenarioId).Id
		Dim SourceTimeid As Integer =  BRApi.Finance.Members.GetMemberId(si, DimTypeId.Time, SourceYear)
		
		' Target Info
		Dim TargetScenarioId As Integer = BRApi.Finance.Members.GetMemberId(si, DimTypeId.Scenario, TargetScenario)
		Dim TargetScenarioTypeId As Integer = BRApi.Finance.Scenario.GetScenarioType(si, TargetScenarioId).Id
		Dim TargetTimeid As Integer =  BRApi.Finance.Members.GetMemberId(si, DimTypeId.Time, TargetYear)
		
		For Each RPMember As Member In RPNameList	
					
			Dim RPId As Integer = RPMember.MemberId
			Dim RPShortName As String = RPMember.Name
			Dim RPPk As New MemberPk(BudFm_FlowDim.DimPk.DimTypeId, RPId)
			Dim RPMemberInfo As MemberInfo = BRApi.Finance.Members.GetMemberInfo(si, dimtypeid.Flow, RPId, True)
			Dim RPDesc As String = RPMember.Description
			Dim RPToUpdate As New Member(RPPk,RPShortName,RPDesc,BudFm_FlowDim.DimPk.DimId)
			Dim RPVarProps As VaryingMemberProperties = RPMemberInfo.Properties
			Dim RPToUpdateInfo As New MemberInfo(RPToUpdate,RPVarProps,Nothing,BudFm_FlowDim, DimConstants.Unknown)
			Dim RPMemberProperties As FlowVMProperties = RPToUpdateInfo.GetFlowProperties()
			
			' Read Text properties from Source and Set to to Target
			' Text1
			Dim StoredTextProperty As String = ""
			Dim StoredTextModProperty As String = ""
			StoredTextProperty = RPMemberProperties.Text1.GetStoredValue(SourceScenarioTypeId, SourceTimeId)
			StoredTextModProperty = RPMemberProperties.Text7.GetStoredValue(SourceScenarioTypeId, SourceTimeId)
			If Not StoredTextProperty = ""
				RPMemberProperties.Text1.SetStoredValue(TargetScenarioTypeId, TargetTimeId, StoredTextProperty)
			End If
			
			If Not StoredTextModProperty = ""
				RPMemberProperties.Text7.SetStoredValue(TargetScenarioTypeId, TargetTimeId, StoredTextModProperty)
			End If
			
			'Save the member
			BRapi.Finance.MemberAdmin.SaveMemberInfo(si, RPToUpdateInfo, False, True, False, False)
			
		Next 
		
		Return Nothing
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function 'Copy_RP_Text_Properties
#End Region  'Copy_RP_Text_Properties

#Region "Clear_RP_Text_Properties"
Private Function Clear_RP_Text_Properties(
					ByVal si As SessionInfo,
					ByVal RPParent As String,
					ByVal Scenario As String,
					ByVal Year As String)
	Try
		
		Dim RPParentId As Integer = BRApi.Finance.Members.GetMemberId(si, dimtypeid.Flow, RPParent)
	
		Dim BudFm_FlowDim As OneStream.Shared.Wcf.Dim = BRApi.Finance.Dim.GetDim(si, "Std_Flow")
		Dim RPNameList As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si, BudFm_FlowDim.DimPk, RPParentId, Nothing)

		Dim ScenarioId As Integer = BRApi.Finance.Members.GetMemberId(si, DimTypeId.Scenario, Scenario)
		Dim ScenarioTypeId As Integer = BRApi.Finance.Scenario.GetScenarioType(si, ScenarioId).Id
		Dim Timeid As Integer =  BRApi.Finance.Members.GetMemberId(si, DimTypeId.Time, Year)
		
'		' Target Info
'		Dim TargetScenarioId As Integer = BRApi.Finance.Members.GetMemberId(si, DimTypeId.Scenario, TargetScenario)
'		Dim TargetScenarioTypeId As Integer = BRApi.Finance.Scenario.GetScenarioType(si, TargetScenarioId).Id
'		Dim TargetTimeid As Integer =  BRApi.Finance.Members.GetMemberId(si, DimTypeId.Time, TargetYear)
		
		For Each RPMember As Member In RPNameList	
					
			Dim RPId As Integer = RPMember.MemberId
			Dim RPShortName As String = RPMember.Name
			Dim RPPk As New MemberPk(BudFm_FlowDim.DimPk.DimTypeId, RPId)
			Dim RPMemberInfo As MemberInfo = BRApi.Finance.Members.GetMemberInfo(si, dimtypeid.Flow, RPId, True)
			Dim RPDesc As String = RPMember.Description
			Dim RPToUpdate As New Member(RPPk,RPShortName,RPDesc,BudFm_FlowDim.DimPk.DimId)
			Dim RPVarProps As VaryingMemberProperties = RPMemberInfo.Properties
			Dim RPToUpdateInfo As New MemberInfo(RPToUpdate,RPVarProps,Nothing,BudFm_FlowDim, DimConstants.Unknown)
			Dim RPMemberProperties As FlowVMProperties = RPToUpdateInfo.GetFlowProperties()
			
			RPMemberProperties.Text1.RemoveStoredPropertyItem(ScenarioTypeId, TimeId)
			RPMemberProperties.Text2.RemoveStoredPropertyItem(ScenarioTypeId, TimeId)
			RPMemberProperties.Text3.RemoveStoredPropertyItem(ScenarioTypeId, TimeId)
			RPMemberProperties.Text4.RemoveStoredPropertyItem(ScenarioTypeId, TimeId)
			RPMemberProperties.Text5.RemoveStoredPropertyItem(ScenarioTypeId, TimeId)
			RPMemberProperties.Text6.RemoveStoredPropertyItem(ScenarioTypeId, TimeId)
			RPMemberProperties.Text7.RemoveStoredPropertyItem(ScenarioTypeId, TimeId)
			RPMemberProperties.Text8.RemoveStoredPropertyItem(ScenarioTypeId, TimeId)

			'Save the member
			BRapi.Finance.MemberAdmin.SaveMemberInfo(si, RPToUpdateInfo, False, True, False, False)

		Next 
		Return Nothing
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function 'Clear_RP_Text_Properties
#End Region  'Clear_RP_Text_Properties

#Region "Copy_All_RP_DataAttachments"
Private Function Copy_All_RP_DataAttachments(
					ByVal si As SessionInfo,
					ByVal Cube As String,
					ByVal SourceScenario As String,
					ByVal TargetScenario As String,
					ByVal SourceYear As String,
					ByVal TargetYear As String)
	Try
		' First, Clear all data attachements from the TargetScenario & Year
		Clear_All_RP_DataAttachments(si, Cube, TargetScenario, TargetYear)
		' Copy all data attachemnets from Source to Target 
		Dim UserName As String = si.UserName
        Dim sql As New Text.StringBuilder 
		sql.AppendLine("INSERT INTO [dbo].[DataAttachment] " ) 
		sql.AppendLine("	( " )
		sql.AppendLine("		UniqueID,")
		sql.AppendLine("		Cube, Entity, Parent, Cons, ")
		sql.AppendLine("		Scenario, ")
		sql.AppendLine("		Time, ")
		sql.AppendLine("		Account, Flow, Origin, IC,")
		sql.AppendLine("		UD1, UD2, UD3, UD4, UD5, UD6, UD7, UD8,  ")
		sql.AppendLine("		Title, AttachmentType, ")
		sql.AppendLine("		CreatedUserName,  ")
		sql.AppendLine("		CreatedTimestamp,  ")
		sql.AppendLine("		LastEditedUserName,  ")
		sql.AppendLine("		LastEditedTimestamp,  ")
		sql.AppendLine("		Text, FileName, FileBytes  ")
		sql.AppendLine("	) ")
		sql.AppendLine("SELECT ")
		sql.AppendLine("		NEWID(),")
		sql.AppendLine("		Cube, Entity, Parent, Cons, ")
		sql.AppendLine("   		'" & TargetScenario & "',  ")
		sql.AppendLine("   		'" & TargetYear & "',  ")
		sql.AppendLine("		Account, Flow, Origin, IC, ")
		sql.AppendLine("		UD1, UD2, UD3, UD4, UD5, UD6, UD7, UD8,  ")
		sql.AppendLine("		Title, AttachmentType, ")
		sql.AppendLine("   		'" & UserName & "',  ")
		sql.AppendLine("		SYSUTCDATETIME(),  ")
		sql.AppendLine("   		'" & UserName & "',  ")
		sql.AppendLine("		SYSUTCDATETIME(),  ")
		sql.AppendLine("		Text, FileName, FileBytes  ")
		sql.AppendLine("FROM [dbo].[DataAttachment] ")
		sql.AppendLine("WHERE ")
        sql.AppendLine("	 Cube = '" & Cube & "' ")
        sql.AppendLine(" AND Scenario = '" & SourceScenario & "' ")
        sql.AppendLine(" AND Time = '" & SourceYear & "' ")
        sql.AppendLine(" AND Entity <> 'NA' ")
		sql.AppendLine(" AND Flow NOT LIKE '%_WV' ") ' Added this line to exclude flow members ending with _WV
		sql.AppendLine("  AND Account not IN ('Comments_ConcReview','Resolution_ConcReview','Description_ChangeLog','RPAudit','Reference_Doc') ")
		sql.AppendLine(" UNION                                                                                                 ")
		sql.AppendLine(" SELECT                                                                                                ")
		sql.AppendLine(" NEWID(),                                                                                              ")
		sql.AppendLine("   Cube, Entity, Parent, Cons,                                                                         ")
		sql.AppendLine("      '" & TargetScenario & "',                                                                        ")
		sql.AppendLine("      '" & TargetYear & "',                                                                            ")
		sql.AppendLine("     Account, Flow, Origin, IC,                                                                          ")
		sql.AppendLine("     UD1, UD2, UD3, UD4, UD5, UD6, UD7, UD8,                                                             ")
		sql.AppendLine("     Title, AttachmentType,                                                                              ")
		sql.AppendLine("     CreatedUserName,                                                                                    ")
		sql.AppendLine("     CreatedTimestamp,                                                                                    ")
		sql.AppendLine("     LastEditedUserName,                                                                               ")
		sql.AppendLine("     LastEditedTimeStamp,                                                                                  ")
		sql.AppendLine("     Text, FileName, FileBytes                                                                           ")
		sql.AppendLine(" FROM [dbo].[DataAttachment]                                                                           ")
		sql.AppendLine("     where  Cube = '" & Cube & "'                                                                          ")
		sql.AppendLine("     AND Scenario = '" & SourceScenario & "'                                                              ")
		sql.AppendLine("     AND Time = '" & SourceYear & "'                                                                      ")
		sql.AppendLine("     AND Entity <> 'NA'                                                                                   ")
		sql.AppendLine("     AND Flow NOT LIKE '%_WV'                                                                                 ") ' Added this line to exclude flow members ending with _WV
		sql.AppendLine("     AND Account IN ('Comments_ConcReview','Resolution_ConcReview','Description_ChangeLog','RPAudit','Reference_Doc')     ")

		Dim sqlStmt As String = sql.ToString
		'BrApi.ErrorLog.LogMessage(si, "SQLStmt :" & sqlStmt)
			Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
	        	Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp, sqlStmt, True)				
			End Using
		Return Nothing	
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function
#End Region  'Copy_All_RP_DataAttachments

#Region "Clear_All_RP_DataAttachments"
Private Function Clear_All_RP_DataAttachments(
					ByVal si As SessionInfo,
					ByVal Cube As String,
					ByVal Scenario As String,
					ByVal Year As String)
	Try
		Dim UserName As String = si.UserName
        Dim sql As New Text.StringBuilder 

		sql.AppendLine("DELETE FROM [dbo].[DataAttachment] ")
		sql.AppendLine("WHERE ")
        sql.AppendLine("	 Cube = '" & Cube & "' ")
        sql.AppendLine(" AND Scenario = '" & Scenario & "' ")
        sql.AppendLine(" AND Time = '" & Year & "' ")
        sql.AppendLine(" AND Entity <> 'NA' ")
		
		
		Dim sqlStmt As String = sql.ToString
'		BrApi.ErrorLog.LogMessage(si, "SQLStmt :" & sqlStmt)
							
			Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
	        	Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp, sqlStmt, True)
				
			End Using
				
		Return Nothing	
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function
#End Region  'Clear_All_RP_DataAttachments

#Region "Load_Form_Data_Exported_By_DM_Job"
Private Function Load_Form_Data_Exported_By_DM_Job(ByVal si As SessionInfo)
	Try
		BrApi.ErrorLog.LogMessage(si, " Load Method invoked")

		'File To Import
		Dim fileName As String = "BudFm_RAP_FY25_CostTables.csv" '<- Define file to import
		             
		'Import from Network FileShare
'		Dim filePath As String = "\\File Share\Application\FERBE_Development\Groups\Everyone\Ranga\" & fileName
'		Dim filePath As String = "File Share\Applications\FERBE_Development\Groups\Everyone\Ranga\" & fileName
		Dim filePath As String = "\\sguscg.file.core.usgovcloudapi.net\onestreamsharedev1\FileShare\Applications\FERBE_Development\Groups\Everyone\Ranga\" & fileName
		BrApi.ErrorLog.LogMessage(si, " File Path: " &filePath )
		
		'Verify if file exists
		If system.IO.File.Exists(filePath) Then
		    'File exists
			BrApi.ErrorLog.LogMessage(si, " File Exists")
		Else
		    'File does not exist
			BrApi.ErrorLog.LogMessage(si, " File does not exist")
		End If
				
		'Import a csv file containing FORM data
		Dim originFilter As New List(Of String)
		originFilter.Add("Forms")
		Dim delimiter As String = ","                                  '<- Define file delimiter
		Dim targetOriginMember As String = "Forms"                     '<- Define Origin member to extract
		Dim loadZeros As Boolean = False
		   
		'Execute the load
		Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingUsingCsvFile(si,  filePath, delimiter, originFilter, targetOriginMember, loadZeros)
		Return Nothing
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function
#End Region  'Load_Form_Data_Exported_By_DM_Job

#Region "Calc_RP_Allocations"
Private Function Calc_RP_Allocations(
					ByVal si As SessionInfo,
					ByVal Cube As String,
					ByVal Scenario As String,
					ByVal RPName As String,
					ByVal Year As String)					
	Try
		
'		Dim TargetRPEntity As String = Get_RP_Entity(si, TargetRPName)
'		Dim TargetRPTime As String = Get_RP_Budget_Year(si, TargetRPName)
'		' Create a new list of memberscript and value and add memebers
'		Dim lstMemberScriptAndValue As New List(Of memberScriptAndValue)
		
''		Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConn, sqlStatement, useCommandTimeoutLarge)		
'        Dim sql As New Text.StringBuilder
'			sql.Append("SELECT Distint ")
'			sql.Append("	Cube, Entity, Parent, Cons, Scenario, Time, ")
'			sql.Append("	Account, Flow, Origin, IC,	")
'			sql.Append("	UD1, UD2, UD3, UD4, UD5, UD6, UD7, UD8, ")
'			sql.Append("	Title, Text, FileName ")
'            sql.Append("FROM dbo.DataAttachment  WITH(NOLOCK) ")
'            sql.Append("WHERE Cube = '" & Cube & "' ")
'            sql.Append("  AND Scenario = '" & SourceScenario & "' ")
'            sql.Append("  AND Flow = '" & SourceRPName & "' ")
'            sql.Append("  AND account NOT IN( 'Description_ChangeLog', 'Reference_Doc' )")

'			Dim sqlStmt As String = sql.ToString
''			BrApi.ErrorLog.LogMessage (si, sqlStmt)
							
'			Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
'	        	Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp, sqlStmt, True)
'	            Dim numRows As Integer = dt.Rows.Count
'				Dim rowIndex As Integer = 0
'				If dt IsNot Nothing AndAlso NumRows < 1 Then
''					BrApi.ErrorLog.LogMessage (si, " No rows to selected")
'					Return Nothing
'				End If
				
'			    Dim baseScript As String =  
'								"E#" & targetRPEntity	& ":" & 
'								"S#" & TargetScenario 	& ":" & 
'								"T#" & targetRPTime		& ":" & 
'								"F#" & targetRPName		& ":" & 
'								"V#Annotation"
'				Dim specificScript = ""
'				Dim memberScript = ""
'				Dim TextValue = "" 
				
				
'				Do
'					'Read column values for each row and construct script
'					specificScript = 
'							"C#"  & dt.Rows(rowIndex)("Cons")	& ":" & 
'							"A#"  & dt.Rows(rowIndex)("Account")& ":" & 
'							"O#"  & dt.Rows(rowIndex)("Origin") & ":" & 
'							"I#"  & dt.Rows(rowIndex)("IC")		& ":" & 
'							"U1#" & dt.Rows(rowIndex)("UD1") 	& ":" & 
'							"U2#" & dt.Rows(rowIndex)("UD2") 	& ":" & 
'							"U3#" & dt.Rows(rowIndex)("UD3") 	& ":" &
'							"U4#" & dt.Rows(rowIndex)("UD4") 	& ":" & 
'							"U5#" & dt.Rows(rowIndex)("UD5") 	& ":" & 
'							"U6#" & dt.Rows(rowIndex)("UD6") 	& ":" & 
'							"U7#" & dt.Rows(rowIndex)("UD7") 	& ":" & 
'							"U8#" & dt.Rows(rowIndex)("UD8") 	& ":"  
							
'							textValue = dt.Rows(rowIndex)("Text")
							
'					memberScript = baseScript & ":" & specificScript
''					BrApi.ErrorLog.LogMessage (si, memberScript)
''					BrApi.ErrorLog.LogMessage (si, textValue)
''					api.Data.SetDataAttachmentText(memberScript, textValue , False)
'					lstMemberScriptAndValue.Add(New MemberScriptAndValue(Cube, memberScript, 0, True, textValue))						
					
'					rowIndex = rowIndex + 1
'					specificScript = ""
'				Loop While rowIndex < numRows 			
'			End Using				

'			'Write the annotations to the database
'			Dim objXFResult As XFResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, lstMemberScriptAndValue)			
		Return Nothing	
	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function
#End Region  'Copy_RP_Annotations

#End Region 'Private Functions and Subs
End Class
End Namespace
