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
Imports OneStreamWorkspacesApi
Imports OneStreamWorkspacesApi.V800

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
	Public Class BUDFM_MbrLists
		Implements IWsasFinanceMemberListsV800

		Dim rpUtils As New BUDFM_RP_Utilities

		' BR-name doorway: the dashboards' 50+ CustomMemberList(BRName=
		' Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, ...) filters resolve by
		' BR-name discovery, which invokes Main. Keep this delegate so BOTH
		' invocation paths (BR name and factory service) land on GetMemberList.
'		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
'			Try
'				If api.FunctionType = FinanceFunctionType.MemberList Then
'					Return GetMemberList(si, globals, api, args)
'				End If
'				Return Nothing
'			Catch ex As Exception
'				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
'			End Try
'		End Function

		' NOTE: the parameter is named `globals` (not brGlobals) on purpose — VB
		' matches interface implementations by TYPE signature, and the ported
		' bodies below reference `globals` throughout.
		Public Function GetMemberList(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As MemberList Implements IWsasFinanceMemberListsV800.GetMemberList
			Try
				rpUtils.Main(si, globals, api, New ExtenderArgs())
				'System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")
				Dim startTime As Date = Now
				
				Dim ReturnObject As Object = InnerMain(si, globals, api, args )

				Dim runLength As Global.System.TimeSpan = Now.Subtract(startTime)
				Dim millisecs As Integer = runLength.Milliseconds	
				
				'BrApi.ErrorLog.LogMessage(si, "Perf Test:, " & "USCG_BudFm_MemberLists." & args.MemberListArgs.MemberListName & ", " &	millisecs)
				Return ReturnObject

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function


		Public Function InnerMain(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				Select Case api.FunctionType
											
					Case Is = FinanceFunctionType.MemberList
	
#Region "GetActiveOrReserveList"					
						
						'U2#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetActiveOrReserveList], Billet_Type=|!prm_BLT_BilletType_OS!|)
						If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetActiveOrReserveList") Then
							
							'Get The Billet Type	
							Dim Billet_Type As String = args.MemberListArgs.NameValuePairs.XFGetValue("Billet_Type")
							Dim AD_Reserve As String = args.MemberListArgs.NameValuePairs.XFGetValue("AD_Reserve")
							
							Dim milListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim milListInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "U2#Military_Employment_Type.Children.Remove(NA_Military_Employment_Type)", Nothing)
							Dim milListList As New MemberList(milListHeader, milListInfos)
							Dim naListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim naListInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "U2#NA_Military_Employment_Type", Nothing)
							Dim naListList As New MemberList(naListHeader, naListInfos)
											
							If billet_Type.XFEqualsIgnoreCase("Military")
								Return milListList					
							Else 							
								Return naListList
							End If
							
						End If
						
#End Region
#Region "GetReserveTypeList"					
						
						'U2#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetReserveTypeList], Billet_Type=|!prm_BLT_BilletType_OS!|, AD_Reserve=|!prm_BLT_ADReserve_OS!|)
						If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetReserveTypeList") Then
							
							'Get The Billet Type	
							Dim Billet_Type As String = args.MemberListArgs.NameValuePairs.XFGetValue("Billet_Type")
							Dim AD_Reserve As String = args.MemberListArgs.NameValuePairs.XFGetValue("AD_Reserve")
							
							Dim resListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim resListInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "U2#Reserve.Base.Remove(NA_Reserve)", Nothing)
							Dim resListList As New MemberList(resListHeader, resListInfos)
							Dim naListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim naListInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "U2#NA_Reserve", Nothing)
							Dim naListList As New MemberList(naListHeader, naListInfos)
											
							If billet_Type.XFEqualsIgnoreCase("Civilian") Or (billet_Type.XFEqualsIgnoreCase("Military") And AD_Reserve.XFEqualsIgnoreCase("Active_Duty"))
								Return naListList					
							Else 							
								Return resListList
							End If
							
						End If
						
#End Region

#Region "GetInUseForScenario"

'				E#Total_Lead_Office.Children.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetInUseForScenarios])
'				cbx_GEN_LeadDirectorate_OS
'			    Creating a new RP picking the lead
				
				If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetInUseForScenario") Then
					
					Dim budfm_EntityItemsDimPk As DimPk = api.Dimensions.GetDim("BudFm_Entity").DimPk
				  '  Dim budfm_EntityItemsDimPk As DimPk = api.D.Dim.GetDimPk(si,"BudFm_Entity")
					Dim Total_LeadOffice As Integer = api.Members.GetMemberId(DimtypeID.Entity, "Total_Lead_Office")
					'Dim Total_LeadOffice As Integer = BRApi.Finance.Members.GetMemberId(si,DimtypeID.Entity, "Total_Lead_Office")
					Dim totalEntityLead As List(Of Member) = api.Members.GetBaseMembers(budfm_EntityItemsDimPk, Total_LeadOffice, Nothing)
                  '  Dim totalEntityLead As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si,budfm_EntityItemsDimPk, Total_LeadOffice, Nothing)
					Dim scenarioKey As Integer = si.WorkflowClusterPk.ScenarioKey	
					'Dim ScenarioName As String = BRApi.Finance.Members.GetMemberName(si, "2", scenarioKey)
					Dim LeadList As New List(Of String) 
					Dim leadStringList As String = ""
					Dim wfTime As String = args.MemberListArgs.NameValuePairs.XFGetValue("WFTime")
					
					Dim wfTimeId As Integer = api.Members.GetMemberId(dimtypeid.Time, wfTime)
					Dim objScenarioType As ScenarioType = BRApi.Finance.Scenario.GetScenarioType(si, scenarioKey)
					Dim EntityInuse As Boolean = False
					
					
					 For Each entityLead As Member In totalEntityLead
						 
							Dim entityID As Integer = api.Members.GetMemberId(DimType.Entity.Id, entityLead.Name)
							EntityInuse = api.Entity.InUse(entityID,objScenarioType.Id, wfTimeId )	
							
							If EntityInuse  Then
								LeadList.Add(entityLead.Name.ToString)
							     EntityInuse = False
							End If
							
						 
				 	Next
					
					
					'remove the CG9 not used for OS
					LeadList.Remove("LO_CG9")
					LeadList.Remove("LO_No")
					
					For Each entityV As String  In LeadList
					  If leadStringList = "" Then
						  leadStringList = "E#" & entityV
					  Else 
						  leadStringList = leadStringList & ", E#" & entityV
					  End If
					  
					Next 	
					
				''create the memberlist to return	
				Dim LeadListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
				Dim LeadMemberInfo As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, leadStringList, Nothing)
                Dim LeadDList As New MemberList(LeadListHeader, LeadMemberInfo)
					
				 
			   Return  LeadDList
   
			End If 			
					
#End Region

							
#Region "GetBilletUIIList"					
					
	'U2#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetBilletUIIList], OPFAC=|!prm_BLT_OPFACS_OS!|)
	If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetBilletUIIList") Then
		
		'Get The ATU and OPFAC
		Dim OPFAC As String = args.MemberListArgs.NameValuePairs.XFGetValue("OPFAC")
		Dim OPFACLeft2 As String = String.Empty
		If OPFAC.Length > 0 Then
			OPFACLeft2 = OPFAC.Substring(0,2)
		End If
		
		'If the ATU equals 49 or OPFAC = 98_70098_6, return the full list, otherwise default it to start with no investment
		Dim isFullList As Boolean = OPFACLeft2.XFEqualsIgnoreCase("49") Or OPFAC.XFContainsIgnoreCase("98_70098_6")
		
		If isFullList Then
			
			If globals.GetObject("BilletUIIList_Full") Is Nothing Then
				
				Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
				Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "U2#Billet_Investments.Base", Nothing)
				Dim listList As New MemberList(listHeader, listInfos)
				
				globals.SetObject("BilletUIIList_Full", listList)
				
				Return listList
				
			Else
				
				Return globals.GetObject("BilletUIIList_Full")
				
			End If
			
		Else
			
			If globals.GetObject("BilletUIIList_NA") Is Nothing Then
				
				Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
				Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "U2#NoInvestment,U2#Billet_Investments.Base.Remove(NoInvestment).Remove(024_000006372)", Nothing)
				Dim listList As New MemberList(listHeader, listInfos)
				
				globals.SetObject("BilletUIIList_NA", listList)
				
				Return listList
				
			Else
				
				Return globals.GetObject("BilletUIIList_NA")
				
			End If
			
		End If
		
	End If
					
#End Region


#Region "GetPPETypeList"					

						'U8#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetPPETypeList], Billet_Type=|!prm_BLT_BilletType_OS!|)
						If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetPPETypeList") Then
							
							'Get The Billet Type	
							Dim Billet_Type As String = args.MemberListArgs.NameValuePairs.XFGetValue("Billet_Type")
							
							Dim ppeListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim ppeListInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "U8#Total_PPE.Children", Nothing)
							Dim ppeListList As New MemberList(ppeListHeader, ppeListInfos)

							'DZ--20231212--DHSUSCG-1509--change made per Jennifers request to allow ppe/ppa/atu selections when civilian is selected
							'Dim naListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							'Dim naListInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "U8#NA_PPE_Type", Nothing)
							'Dim naListList As New MemberList(naListHeader, naListInfos)
							
							'If Billet_Type.XFEqualsIgnoreCase("Military")
								Return ppeListList
							'Else
							'	Return naListList
							'End If
							
						End If

#End Region
#Region "GetPPE_PPAList"					
					
						'U1#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetPPE_PPAList], Billet_Type=|!prm_BLT_BilletType_OS!|, PPE_Type=|!prm_BLT_PPEType_OS!|)
						If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetPPE_PPAList") Then
							
							'DZ--20231212--DHSUSCG-1509--change made per Jennifers request to allow ppe/ppa/atu selections
							'when civilian is selected. Billet_Type branching was disabled at that time; only PPE_Type
							'drives the decision now.
							Dim PPE_Type As String = args.MemberListArgs.NameValuePairs.XFGetValue("PPE_Type")
							Dim isNA As Boolean = PPE_Type.XFContainsIgnoreCase("NA_PPE_Type")
							
							If isNA Then
								
								If globals.GetObject("PPE_PPAList_NA") Is Nothing Then
									
									Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
									Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "U1#NA_PPA", Nothing)
									Dim listList As New MemberList(listHeader, listInfos)
									
									globals.SetObject("PPE_PPAList_NA", listList)
									
									Return listList
								Else
									
									Return globals.GetObject("PPE_PPAList_NA")
									
								End If
								
							Else
								
								If globals.GetObject("PPE_PPAList_OS") Is Nothing Then
									
									Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
									Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "U1#OS.Base", Nothing)
									Dim listList As New MemberList(listHeader, listInfos)
									
									globals.SetObject("PPE_PPAList_OS", listList)
									
									Return listList
									
								Else
									
									Return globals.GetObject("PPE_PPAList_OS")
									
								End If
								
							End If
							
						End If

#End Region
			
#Region "GetPPE_ATUList"

				'U4#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetPPE_ATUList], Billet_Type=|!prm_BLT_BilletType_OS!|, PPE_Type=|!prm_BLT_PPEType_OS!|)
				If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetPPE_ATUList") Then
				    
				    ' Get Parameters from the Member List Call
				    Dim Billet_Type As String = args.MemberListArgs.NameValuePairs.XFGetValue("Billet_Type")
				    Dim PPE_Type As String = args.MemberListArgs.NameValuePairs.XFGetValue("PPE_Type")
				    
				    ' Get Workflow Context for the InUse check
				    Dim wfTime As String = api.Workflow.GetWorkflowUnitInfo.TimeName
				    Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si, DimTypeId.Time, wfTime)
				    Dim scenarioKey As Integer = si.WorkflowClusterPk.ScenarioKey
				    Dim scenarioTypeId As Integer = BRApi.Finance.Scenario.GetScenarioType(si, scenarioKey).Id
				    
				    ' Identify the potential members based on PPE_Type
				    Dim memberFilter As String = ""
				    If PPE_Type.XFContainsIgnoreCase("NA_PPE_Type") Then
				        memberFilter = "U4#NA_ATU"
				    Else
				        memberFilter = "U4#Total_ATU.Children.Remove(99_AMMO,99_BF,99_Claims,99_GSA_Rent,99_GSA_Security,99_INDREC,99_Medals,CG_41_ADLM,CG_43_FDLM,CG_45_VDLM,CP,EnvCR,MHC,MP,PCSC,RT,PCI,RD,RP,MERHCF,MOSP,BS,F,CG_833,No_ATU)"
				    End If
				    
				    ' Retrieve potential members from metadata
				    Dim potentialMbrs As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, memberFilter, Nothing)
				    
				    ' Filter the list by In-Use status
				    Dim finalMbrInfoList As New List(Of MemberInfo)()
				    
				    If Not potentialMbrs Is Nothing Then
				        For Each mbrInfo As MemberInfo In potentialMbrs
				            Dim atuId As Integer = mbrInfo.Member.MemberPk.MemberId
				            
				            ' Check if this specific UD4 member is "In Use" in this Scenario and Time
				            Dim bInUse As Boolean = BRApi.Finance.UD.InUse(si, DimTypeId.UD4, atuId, scenarioTypeId, wfTimeId)
				            
				            If bInUse Then
				                finalMbrInfoList.Add(mbrInfo)
				            End If
				        Next
				    End If

				    ' Return the filtered list
				    Dim atuListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
				    Return New MemberList(atuListHeader, finalMbrInfoList)

				End If

#End Region
		
#Region "GetUTL_PPAList"					
						
						'U1#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetUTL_PPAList], Required=|!prm_BLT_Utilities_OS!|)
						If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetUTL_PPAList") Then
							
							'Get The Required	
							Dim Required As String = args.MemberListArgs.NameValuePairs.XFGetValue("Required")
							
							Dim ppaListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim ppaListInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "U1#OS.Base", Nothing)
							Dim ppaListList As New MemberList(ppaListHeader, ppaListInfos)
							Dim naListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim naListInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "U1#NA_PPA", Nothing)
							Dim naListList As New MemberList(naListHeader, naListInfos)
											
							If Required.XFEqualsIgnoreCase("Y")
								Return ppaListList
							Else 'not required		
								Return naListList
							End If
							
						End If
						
#End Region
			
#Region "GetUTL_ATUList"					
    
    ' U4#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetUTL_ATUList], Required=|!prm_BLT_Utilities_OS!|)
    If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetUTL_ATUList") Then
        
        ' Get the "Required" parameter
        Dim Required As String = args.MemberListArgs.NameValuePairs.XFGetValue("Required")
        Dim atuListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
        
        ' Handle the "Not Required" 
        If Not Required.XFEqualsIgnoreCase("Y") Then
            Dim naListInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "U4#NA_ATU", Nothing)
            Return New MemberList(atuListHeader, naListInfos)
        End If

        ' Logic for "Required = Y": Get Workflow and Time context for InUse check
        Dim wfTime As String = api.Workflow.GetWorkflowUnitInfo.TimeName
        Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si, DimTypeId.Time, wfTime)
        
        Dim scenarioKey As Integer = si.WorkflowClusterPk.ScenarioKey
        Dim objScenarioType As ScenarioType = BRApi.Finance.Scenario.GetScenarioType(si, scenarioKey)
        Dim scenarioTypeId As Integer = objScenarioType.Id

        ' Define the potential members
        Dim allPotentialMbrs As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "U4#Total_ATU.Children.Remove(99_AMMO,99_BF,99_Claims,99_GSA_Rent,99_GSA_Security,99_INDREC,99_Medals,CG_41_ADLM,CG_43_FDLM,CG_45_VDLM,CP,EnvCR,MHC,MP,PCSC,RT,PCI,RD,RP,MERHCF,MOSP,BS,F,CG_833,No_ATU)", Nothing)
        
        ' Create a list to store members that pass the InUse check
        Dim filteredMbrs As New List(Of MemberInfo)

        If Not allPotentialMbrs Is Nothing Then
            For Each mbrInfo As MemberInfo In allPotentialMbrs
                Dim atuId As Integer = mbrInfo.Member.MemberPk.MemberId
                
                ' Check if the member has data/is active in the current context
                Dim bInUse As Boolean = BRApi.Finance.UD.InUse(si, DimTypeId.UD4, atuId, scenarioTypeId, wfTimeId)
                
                If bInUse Then
                    filteredMbrs.Add(mbrInfo)
                End If
            Next
        End If

        ' Return the filtered list
        Return New MemberList(atuListHeader, filteredMbrs)
        
    End If
    
#End Region
	
#Region "GetEFlightBagList"

						'U8#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetEFlightBagList], RPName=|!prm_Number_OS!|, Spe_Code_Occu_Series=|!prm_BLT_SpcCodeOccSeries_OS!|)
						If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetEFlightBagList") Then
							
							'Get RP The RP Name and Parse It	
							Dim RPName As String = args.MemberListArgs.NameValuePairs.XFGetValue("RPName")

							' If RP Name is empty, nothing to do 
							If RPName = "" Then
								Return Nothing
							End If					
							Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)	
							Dim wfCube As String = api.Cubes.GetCubeInfo.Cube.Name
							Dim wfTime As String = api.Workflow.GetWorkflowUnitInfo.TimeName
							Dim wfScenario As String = api.Workflow.GetWorkflowUnitInfo.ScenarioName
		                    Dim Specialty_Code As String = args.MemberListArgs.NameValuePairs.XFGetValue("Spe_Code_Occu_Series")
						    Dim CodeId As Integer = api.Members.GetMemberId(dimtypeid.UD3, Specialty_Code)		
							Dim SpecialtyCodeText2 As String = api.UD3.Text(CodeId,2)				
							Dim scriptGenerics As String = "Cb#"& wfCube &":E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":C#USD:V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"	
											
							Dim increase_DecreaseDataAttachmentList As DataAttachmentList = api.Data.GetDataAttachments("A#Increase_Decrease:" & scriptGenerics, False)	
							Dim increase_Decrease As String = String.Empty
							
							For Each increase_DecreaseDataAttachment As DataAttachment In increase_DecreaseDataAttachmentList.Items
								increase_Decrease = increase_DecreaseDataAttachment.Text
							Next	
							
							Dim incListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim incListInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "U8#Total_YesNo.Base.Remove(NA)", Nothing)
							Dim incListList As New MemberList(incListHeader, incListInfos)
							Dim decListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim decListInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "U8#NA", Nothing)
							Dim decListList As New MemberList(decListHeader, decListInfos)
									
							If Increase_Decrease.XFEqualsIgnoreCase("I") And SpecialtyCodeText2.XFEqualsIgnoreCase("Y")
								Return incListList
							Else
								Return decListList
							End If
							
						End If

#End Region
						
#Region "GetTermBilletList"					
						
						'U8#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetTermBilletList], RPName=|!prm_Number_OS!|)
						If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetTermBilletList") Then
							
							'Get RP The RP Name and Parse It	
							Dim RPName As String = args.MemberListArgs.NameValuePairs.XFGetValue("RPName")

							' If RP Name is empty, nothing to do 
							If RPName = "" Then
								Return Nothing
							End If					
							Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)		
							Dim wfCube As String = api.Cubes.GetCubeInfo.Cube.Name
							Dim wfTime As String = api.Workflow.GetWorkflowUnitInfo.TimeName
							Dim wfScenario As String = api.Workflow.GetWorkflowUnitInfo.ScenarioName			
							Dim scriptGenerics As String = "Cb#"& wfCube &":E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":C#USD:V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"	
											
							Dim increase_DecreaseDataAttachmentList As DataAttachmentList = api.Data.GetDataAttachments("A#Increase_Decrease:" & scriptGenerics, False)	
							Dim increase_Decrease As String = String.Empty
							
							For Each increase_DecreaseDataAttachment As DataAttachment In increase_DecreaseDataAttachmentList.Items
								increase_Decrease = increase_DecreaseDataAttachment.Text
							Next	
							
							Dim incListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							'SB 082625 Modified term items return to Remove the NA value and be replaced with Perm
							Dim incListInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "U8#Total_TermBillet.Children.Remove(Term_NA)", Nothing)
							'Dim incListInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "U8#Total_TermBillet.Children", Nothing)
							Dim incListList As New MemberList(incListHeader, incListInfos)
							Dim decListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							'AD 8/12/25 - waiting on Jennifer to tell us what she wants the default Perm/Term to be for Decrease RPs
							'When Increase_Decrease <> 'I' set value to Perm - Permanent
							'Confirmed Jennifer wants the Perm to be default SB 0829
							Dim decListInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "U8#Perm", Nothing)
							'Dim decListInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "U8#Term_NA", Nothing)
							Dim decListList As New MemberList(decListHeader, decListInfos)
											
							If Increase_Decrease.XFEqualsIgnoreCase("I")
								Return incListList
							Else
								Return decListList
							End If
							
						End If
						
#End Region

#Region "GetICASSList"					
					
						'U8#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetICASSList], RPName=|!prm_Number_OS!|)
						If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetICASSList") Then
							
							'Get RP The RP Name and Parse It	
							Dim RPName As String = args.MemberListArgs.NameValuePairs.XFGetValue("RPName")

							' If RP Name is empty, nothing to do 
							If RPName = "" Then
								Return Nothing
							End If	
							Dim increase_Decrease = String.Empty
							Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)	
							Dim wfCube As String = api.Cubes.GetCubeInfo.Cube.Name
							Dim wfTime As String = api.Workflow.GetWorkflowUnitInfo.TimeName
							Dim wfScenario As String = api.Workflow.GetWorkflowUnitInfo.ScenarioName		
							Dim scriptGenerics As String = "Cb#"& wfCube &":E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":C#USD:V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"	
							
							If globals.GetStringValue($"increase_decrease_{scriptGenerics}",String.Empty) = String.Empty					
								'Cache miss -- resolve from the data attachment
								Dim increase_DecreaseDataAttachmentList As DataAttachmentList = api.Data.GetDataAttachments("A#Increase_Decrease:" & scriptGenerics, False)	
								
								'Guard against a Nothing list before iterating it
								If increase_DecreaseDataAttachmentList IsNot Nothing AndAlso increase_DecreaseDataAttachmentList.Items IsNot Nothing Then
									For Each increase_DecreaseDataAttachment As DataAttachment In increase_DecreaseDataAttachmentList.Items
										increase_Decrease = increase_DecreaseDataAttachment.Text
									Next	
								End If
								
								'Cache it for the rest of this session -- use a sentinel for "checked but blank" so
								'a genuinely empty annotation doesn't look identical to "never cached" on the next call
								globals.SetStringValue($"increase_decrease_{scriptGenerics}",increase_Decrease)
							End If
							
							'Decide which filter to use FIRST, then resolve members only once
							Dim mbrFilter As String
							If Increase_Decrease.XFEqualsIgnoreCase("I") Then
								mbrFilter = "U8#Total_ICASS.Children"
							Else
								mbrFilter = "U8#No_ICASS"
							End If
							
							Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, mbrFilter, Nothing)
							
							Return New MemberList(listHeader, listInfos)
							
						End If
					
#End Region
		
#Region "GetBuildOutList"					
						
						'U8#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetBuildOutList], RPName=|!prm_Number_OS!|)
						If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetBuildOutList") Then
							
							'Get RP The RP Name and Parse It	
							Dim RPName As String = args.MemberListArgs.NameValuePairs.XFGetValue("RPName")
							' If RP Name is empty, nothing to do 
							If RPName = "" Then
								Return Nothing
							End If					
							Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)						
							Dim wfCube As String = api.Cubes.GetCubeInfo.Cube.Name
							Dim wfTime As String = api.Workflow.GetWorkflowUnitInfo.TimeName
							Dim wfScenario As String = api.Workflow.GetWorkflowUnitInfo.ScenarioName	
							Dim scriptGenerics As String = "Cb#"& wfCube &":E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":C#USD:V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"	
											
							Dim increase_DecreaseDataAttachmentList As DataAttachmentList = api.Data.GetDataAttachments("A#Increase_Decrease:" & scriptGenerics, False)	
							Dim increase_Decrease As String = String.Empty
							
							For Each increase_DecreaseDataAttachment As DataAttachment In increase_DecreaseDataAttachmentList.Items
								increase_Decrease = increase_DecreaseDataAttachment.Text
							Next	
							
							Dim incListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim incListInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "U8#Total_YesNo.Base.Remove(NA)", Nothing)
							Dim incListList As New MemberList(incListHeader, incListInfos)
							Dim decListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim decListInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "U8#NA", Nothing)
							Dim decListList As New MemberList(decListHeader, decListInfos)
											
							If Increase_Decrease.XFEqualsIgnoreCase("I")
								Return incListList
							Else
								Return decListList
							End If
							
						End If
						
#End Region
			
#Region "GetLeaseList"					
						
						'U8#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetLeaseList], RPName=|!prm_Number_OS!|, Build_Out=|!prm_BLT_Build_Out_OS!|)
						If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetLeaseList") Then
							'Get RP The RP Name and Parse It	
							Dim RPName As String = args.MemberListArgs.NameValuePairs.XFGetValue("RPName")
							' If RP Name is empty, nothing to do 
							If RPName = "" Then
								Return Nothing
							End If					
							Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)	
							Dim Build_Out As String = args.MemberListArgs.NameValuePairs.XFGetValue("Build_Out")	
							
							Dim wfCube As String = api.Cubes.GetCubeInfo.Cube.Name
							Dim wfTime As String = api.Workflow.GetWorkflowUnitInfo.TimeName
							Dim wfScenario As String = api.Workflow.GetWorkflowUnitInfo.ScenarioName			
							Dim scriptGenerics As String = "Cb#"& wfCube &":E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":C#USD:V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"	
											
							Dim increase_DecreaseDataAttachmentList As DataAttachmentList = api.Data.GetDataAttachments("A#Increase_Decrease:" & scriptGenerics, False)	
							Dim increase_Decrease As String = String.Empty
							
							For Each increase_DecreaseDataAttachment As DataAttachment In increase_DecreaseDataAttachmentList.Items
								increase_Decrease = increase_DecreaseDataAttachment.Text
							Next	
							
							Dim totListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim totListInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "U8#Total_Lease.Base", Nothing)
							Dim totListList As New MemberList(totListHeader, totListInfos)
							Dim noListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim noListInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "U8#Lease_No", Nothing)
							Dim noListList As New MemberList(noListHeader, noListInfos)
							
							If Increase_Decrease.XFEqualsIgnoreCase("I")
								If Build_Out.XFEqualsIgnoreCase("Y")							
									Return noListList
								Else 	
									Return totListList
								End If
							Else 	
								Return noListList						
							End If		
							
						End If
						
#End Region
				
#Region "GetLease_PPAList"					
						
						'U1#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetLease_PPAList], Lease_Selection=|!prm_BLT_Lease_OS!|)
						If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetLease_PPAList") Then
							
							'Get lease selection
							Dim lease_Select As String = args.MemberListArgs.NameValuePairs.XFGetValue("Lease_Selection")
							
							Dim ppaListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim ppaListInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "U1#OS.Base", Nothing)
							Dim ppaListList As New MemberList(ppaListHeader, ppaListInfos)
							Dim naListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim naListInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "U1#NA_PPA", Nothing)
							ppaListList.AddMemberInfosToList(naListInfos)
							Dim naListList As New MemberList(naListHeader, naListInfos)		
							If lease_Select.XFEqualsIgnoreCase("Lease_Munro")OrElse lease_Select.XFEqualsIgnoreCase("Lease_No")
								Return naListList
							Else 'not required		
								Return ppaListList
							End If
							
						End If
						
#End Region
					
#Region "GetLease_ATUList"

'U1#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetLease_ATUList], Lease_Selection=|!prm_BLT_Lease_OS!|)
If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetLease_ATUList") Then
    
    ' Get lease selection parameter
    Dim lease_Select As String = args.MemberListArgs.NameValuePairs.XFGetValue("Lease_Selection")
    Dim atuListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
    
    ' Handle the "NA" case
    If lease_Select.XFEqualsIgnoreCase("Lease_Munro") OrElse lease_Select.XFEqualsIgnoreCase("Lease_No") Then
        Dim naListInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "U4#NA_ATU", Nothing)
        Return New MemberList(atuListHeader, naListInfos)
    Else 
        ' Process the "In Use" logic for the full list
        
        ' Get Workflow Context (Time and Scenario Type)
        Dim wfTime As String = api.Workflow.GetWorkflowUnitInfo.TimeName
        Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si, DimTypeId.Time, wfTime)
        Dim scenarioKey As Integer = si.WorkflowClusterPk.ScenarioKey
        Dim objScenarioType As ScenarioType = BRApi.Finance.Scenario.GetScenarioType(si, scenarioKey)
        Dim scenarioTypeId As Integer = objScenarioType.Id
        
        ' Define the ATU filter from your second snippet
        Dim ATUFilter As String = "U4#Total_ATU.Children.Remove(99_AMMO,99_BF,99_Claims,99_GSA_Security,99_INDREC,99_Medals,CG_41_ADLM,CG_43_FDLM,CG_45_VDLM,CP,EnvCR,MHC,MP,PCSC,RT,PCI,RD,RP,MERHCF,MOSP,BS,F,CG_833,No_ATU)"
        Dim allPotentialMbrs As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, ATUFilter, Nothing)
        
        ' List to hold members that pass the InUse check
        Dim finalMbrInfoList As New List(Of MemberInfo)()
        
        ' 4Loop through and check UD4 InUse status
        If Not allPotentialMbrs Is Nothing Then
            For Each mbrInfo As MemberInfo In allPotentialMbrs
                Dim atuId As Integer = mbrInfo.Member.MemberPk.MemberId
                
                ' Perform the InUse check
                Dim bInUse As Boolean = BRApi.Finance.UD.InUse(si, DimTypeId.UD4, atuId, scenarioTypeId, wfTimeId)
                
                If bInUse Then
                    finalMbrInfoList.Add(mbrInfo)
                End If
            Next
        End If
        
        ' Return the filtered list
        Return New MemberList(atuListHeader, finalMbrInfoList)
    End If
    
End If

#End Region
			
#Region "GetFurnitureList"					
						
						'U8#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetFurnitureList], RPName=|!prm_Number_OS!|)
						If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetFurnitureList") Then
							
							'Get RP The RP Name and Parse It	
							Dim RPName As String = args.MemberListArgs.NameValuePairs.XFGetValue("RPName")
							' If RP Name is empty, nothing to do 
							If RPName = "" Then
								Return Nothing
							End If					
							Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)											
													
							Dim wfCube As String = api.Cubes.GetCubeInfo.Cube.Name
							Dim wfTime As String = api.Workflow.GetWorkflowUnitInfo.TimeName
							Dim wfScenario As String = api.Workflow.GetWorkflowUnitInfo.ScenarioName			
							Dim scriptGenerics As String = "Cb#"& wfCube &":E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":C#USD:V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"	
											
							Dim increase_DecreaseDataAttachmentList As DataAttachmentList = api.Data.GetDataAttachments("A#Increase_Decrease:" & scriptGenerics, False)	
							Dim increase_Decrease As String = String.Empty
							
							For Each increase_DecreaseDataAttachment As DataAttachment In increase_DecreaseDataAttachmentList.Items
								increase_Decrease = increase_DecreaseDataAttachment.Text
							Next	
							
							Dim incListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim incListInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "U8#Total_YesNo.Base.Remove(NA)", Nothing)
							Dim incListList As New MemberList(incListHeader, incListInfos)
							Dim decListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim decListInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "U8#NA", Nothing)
							Dim decListList As New MemberList(decListHeader, decListInfos)
											
							If Increase_Decrease.XFEqualsIgnoreCase("I")
								Return incListList
							Else
								Return decListList
							End If
							
						End If
						
#End Region
					
#Region "GetUtilitiesList"					
						
						'U8#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetUtilitiesList], RPName=|!prm_Number_OS!|)
						If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetUtilitiesList") Then
							'Get RP The RP Name and Parse It	
							Dim RPName As String = args.MemberListArgs.NameValuePairs.XFGetValue("RPName")
							' If RP Name is empty, nothing to do 
							If RPName = "" Then
								Return Nothing
							End If					
							Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)	
							Dim wfCube As String = api.Cubes.GetCubeInfo.Cube.Name
							Dim wfTime As String = api.Workflow.GetWorkflowUnitInfo.TimeName
							Dim wfScenario As String = api.Workflow.GetWorkflowUnitInfo.ScenarioName		
							Dim scriptGenerics As String = "Cb#"& wfCube &":E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":C#USD:V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"	
											
							Dim increase_DecreaseDataAttachmentList As DataAttachmentList = api.Data.GetDataAttachments("A#Increase_Decrease:" & scriptGenerics, False)	
							Dim increase_Decrease As String = String.Empty
							
							For Each increase_DecreaseDataAttachment As DataAttachment In increase_DecreaseDataAttachmentList.Items
								increase_Decrease = increase_DecreaseDataAttachment.Text
							Next	
							
							Dim incListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim incListInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "U8#Total_YesNo.Base.Remove(NA)", Nothing)
							Dim incListList As New MemberList(incListHeader, incListInfos)
							Dim decListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim decListInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "U8#N", Nothing)
							Dim decListList As New MemberList(decListHeader, decListInfos)
											
							If Increase_Decrease.XFEqualsIgnoreCase("I")
								Return incListList
							Else
								Return decListList
							End If
							
						End If
						
#End Region

#Region "GetPPAList_Extractor"					
						
						'U3#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetOCList_Extractor])
						If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetPPAList_Extractor") Then

							Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "U1#Total_Appropriations.Base", Nothing)
							Dim listList As New MemberList(listHeader, listInfos)
							
							Return listList
											
							
						End If
						
#End Region

#Region "GetUIIList_Extractor"					
						
						'U3#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetOCList_Extractor])
						If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetUIIList_Extractor") Then

							Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "U2#Total_Investment.Base", Nothing)
							Dim listList As New MemberList(listHeader, listInfos)
							
							Return listList
											
							
						End If
						
#End Region

#Region "GetOCList_Extractor"					
						
						'U3#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetOCList_Extractor])
						If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetOCList_Extractor") Then

							Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "U3#Total_ObjectClass.Base", Nothing)
							Dim listList As New MemberList(listHeader, listInfos)
							
							Return listList
											
							
						End If
						
#End Region

#Region "GetATUList_Extractor"					
						
						'U3#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetOCList_Extractor])
						If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetATUList_Extractor") Then

							Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "U4#Total_ATU.Base", Nothing)
							Dim listList As New MemberList(listHeader, listInfos)
							
							Return listList
											
							
						End If
						
#End Region

#Region "GetRPLineItems"

						'U6#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetRPLineItems], RPName=|!prm_Number_OS!|)
						If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetRPLineItems") Then							
							'Get RP The RP Name and Parse It	
							Dim RPName As String = args.MemberListArgs.NameValuePairs.XFGetValue("RPName")
							' If RP Name is empty, nothing to do 
							If RPName = "" Then
								Return Nothing
							End If					
							
							Dim wfTime As String = api.Workflow.GetWorkflowUnitInfo.TimeName
							Dim wfScenario As String = api.Workflow.GetWorkflowUnitInfo.ScenarioName
							
							'Always pull the current Number_of_Billets value so a billet added mid-session gets picked up
							Dim RP_Entity = rpUtils.Get_RP_Entity(si, RPName)										
							Dim wfCube As String = api.Cubes.GetCubeInfo.Cube.Name
							Dim Number_of_BilletsValueDataAttachmentList As DataAttachmentList = api.Data.GetDataAttachments("Cb#"& wfCube &":E#"& RP_Entity &":C#USD:S#"& wfScenario &":T#"& wfTime &":V#Annotation:A#Number_of_Billets:F#"& RPName &":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None", False)
							Dim Number_of_BilletsValue As String = String.Empty
							
							If Number_of_BilletsValueDataAttachmentList IsNot Nothing AndAlso Number_of_BilletsValueDataAttachmentList.Items IsNot Nothing Then
								BRApi.ErrorLog.LogMessage(si, $"Hit this")
								For Each Number_of_BilletsValueDataAttachment As DataAttachment In Number_of_BilletsValueDataAttachmentList.Items
									Number_of_BilletsValue = Number_of_BilletsValueDataAttachment.Text
								Next
							End If
							
							Dim currentBilletCount As Integer = 0
							If Number_of_BilletsValue <> "" And Number_of_BilletsValue <> "0" Then
								currentBilletCount = Number_of_BilletsValue.XFConvertToInt
							End If
							
							Dim memListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim billetInfos As List(Of MemberInfo)
							
							If globals.GetObject(RPName) Is Nothing Then
								
								'Case 1: nothing cached yet -- resolve every billet and store the MemberInfo list
								BRApi.ErrorLog.LogMessage(si, $"RPLineItems Miss {RPName} - building {currentBilletCount} billets")
								
								If currentBilletCount <= 0 Then
									Return Nothing
								End If
								
								Dim Billets As String = BuildBilletFilter(1, currentBilletCount)
								billetInfos = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, Billets, Nothing)
								
								globals.SetObject(RPName, billetInfos)
								
							Else
								
								billetInfos = CType(globals.GetObject(RPName), List(Of MemberInfo))
								
								If currentBilletCount <> billetInfos.Count Then
									
									'Case 2: cached, but the count changed -- resolve and append just the new billets
									BRApi.ErrorLog.LogMessage(si, $"RPLineItems Hit {RPName} - appending billets {billetInfos.Count + 1} to {currentBilletCount}")
									
									Dim newBilletsFilter As String = BuildBilletFilter(billetInfos.Count + 1, currentBilletCount)
									Dim newMemberInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, newBilletsFilter, Nothing)
									
									If newMemberInfos IsNot Nothing Then
										billetInfos.AddRange(newMemberInfos)
									End If
									
									globals.SetObject(RPName, billetInfos)
									
								Else
									
									'Case 3: cached, same count -- just return it
									BRApi.ErrorLog.LogMessage(si, $"RPLineItems Hit {RPName} - same billet count, returning cached")
									
								End If
								
							End If
							
							If billetInfos.Count = 0 Then
								Return Nothing
							End If
							
							Return New MemberList(memListHeader, billetInfos)
							
						End If
						

#End Region

#Region "GetHistRPList"

						'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetHistRPList], WFTime=|WFTime|, WFTimePrior=|WFTimePrior|)
						If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetHistRPList") Then
						Dim wfTime As String = args.MemberListArgs.NameValuePairs("WFTime")
						Dim wfTimePrior As String = args.MemberListArgs.NameValuePairs("WFTimePrior")
						
							If wfTime.XFConvertToInt < 2026
								Dim memListHeaderRP As New MemberListHeader(args.MemberListArgs.MemberListName)
								Dim memListInfosRP As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "F#Top_Flow,F#Baseline,F#FY" & wfTime.Substring(2,2) & "_RPs,F#FY" & wfTime.Substring(2,2) & "_RP.Base,F#FY" & wfTimePrior.Substring(2,2) & "_AnnTerm.Base", Nothing)
								Dim memListRP As New MemberList(memListHeaderRP, memListInfosRP)	
								'BrApi.ErrorLog.LogMessage(si, "RP")
								Return memListRP	
							Else 
								
							Dim memListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim memListInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "F#Baseline", Nothing)	
							Dim memList As New MemberList(memListHeader, memListInfos)	
							'BrApi.ErrorLog.LogMessage(si, "Baseline")
							Return memList		
							
							End If
										

						End If

#End Region
	
#Region "GetRPNBLTLineItems"

	'U6#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetRPNBLTLineItems], RPName=|!prm_Number_OS!|, MemName=""))											
If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetRPNBLTLineItems") Then
	Dim RPName As String = args.MemberListArgs.NameValuePairs.XFGetValue("RPName")
	Dim MemName As String = args.MemberListArgs.NameValuePairs.XFGetValue("MemName")
		' If RP Name is empty, nothing to do 
		If RPName = "" Then
			Return Nothing
		End If									
						brapi.ErrorLog.LogMessage(si, "Hit 1")		
		Dim wfCube As String = api.Cubes.GetCubeInfo.Cube.Name
		Dim wfTime As String = api.Workflow.GetWorkflowUnitInfo.TimeName
		Dim wfScenario As String = api.Workflow.GetWorkflowUnitInfo.ScenarioName
		Dim UD6NonBillets As String =""
		Dim UD6NonBilletEmpty As Boolean = False
		brapi.ErrorLog.LogMessage(si, "Hit 2")	
		Dim total_NonBillet_Line_ItemsId As Integer = api.Members.GetMemberId(dimtypeId.UD6, MemName)			
		Dim std_LineItemsDimPk As DimPk = api.Dimensions.GetDim("Std_LineItems").DimPk	
		brapi.ErrorLog.LogMessage(si, "Hit 3")
		Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)	
		Dim scriptGenerics As String = "Cb#"& wfCube &":E#" & RP_Entity & ":S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:F#" & RPName & ":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U7#None:U8#None"		
			brapi.ErrorLog.LogMessage(si, "Hit 1")	
		Dim ud6LineItemMems As List(Of Member) = api.Members.GetBaseMembers(std_LineItemsDimPk, total_NonBillet_Line_ItemsId, Nothing)
		If Not ud6lineItemMems Is Nothing Then
			For Each ud6objLineItem As Member In ud6LineItemMems
				If ud6objLineItem.Name= "No_NBLineItem" Then Continue For
				
				'Get the Line Item member Name
				Dim ud6LineItemName As String = ud6objLineItem.Name	
				Dim requested_Item_Tier1DataAttachmentList As DataAttachmentList = api.Data.GetDataAttachments("A#Requested_Item_Tier1:" & scriptGenerics &":U6#" & ud6LineItemName, False)
				Dim requested_Item_Tier1 As String = String.Empty
				For Each requested_Item_Tier1DataAttachment As DataAttachment In requested_Item_Tier1DataAttachmentList.Items
					requested_Item_Tier1 = requested_Item_Tier1DataAttachment.Text
				Next	
					
				If (Not requested_Item_Tier1.XFEqualsIgnoreCase("")) And Not UD6NonBilletEmpty Then	
					UD6NonBillets = UD6NonBillets &",U6#" & ud6LineItemName
				ElseIf requested_Item_Tier1.XFEqualsIgnoreCase("") And Not UD6NonBilletEmpty
					UD6NonBilletEmpty= True		
					UD6NonBillets = UD6NonBillets &",U6#" & ud6LineItemName
				Else
					Continue For		
				End If							
			Next
		End If
		'brapi.ErrorLog.LogMessage(si, "UD6NonBillets: " & UD6NonBillets.Remove(0,1))						
		Dim memListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
		Dim memListInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, UD6NonBillets.Remove(0,1), Nothing)
		Dim memList As New MemberList(memListHeader, memListInfos)	
		
		Return memList
        
	End If

				
#End Region 
	
#Region "GetRPLineItems_Filtered"

						'U6#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetRPLineItems_Filtered], RPName=|!prm_Number_OS!|, LINumberSource=|!prm_BLT_LineItemNumber!|)
						If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetRPLineItems_Filtered") Then
							
							'Get RP The RP Name and Parse It	
							Dim RPName As String = args.MemberListArgs.NameValuePairs.XFGetValue("RPName")
							Dim BilletSource As String = "U6#" & args.MemberListArgs.NameValuePairs.XFGetValue("LINumberSource")
							' If RP Name is empty, nothing to do 
							If RPName = "" Then
								Return Nothing
							End If	
							Dim RP_Entity = rpUtils.Get_Rp_Entity(si, RPName)	
												
							Dim wfCube As String = api.Cubes.GetCubeInfo.Cube.Name
							Dim wfTime As String = api.Workflow.GetWorkflowUnitInfo.TimeName
							Dim wfScenario As String = api.Workflow.GetWorkflowUnitInfo.ScenarioName

							'Dim Number_of_BilletsValue As String = api.Data.GetDataCellEx("Cb#"& wfCube &":E#"& RP_Entity &":C#Local:S#"& wfScenario &":T#"& wfTime &":V#Annotation:A#Number_of_Billets:F#"& RPName &":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").DataCellAnnotation
							
							Dim Number_of_BilletsValueDataAttachmentList As DataAttachmentList = api.Data.GetDataAttachments("Cb#"& wfCube &":E#"& RP_Entity &":C#USD:S#"& wfScenario &":T#"& wfTime &":V#Annotation:A#Number_of_Billets:F#"& RPName &":O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None", False)
							Dim Number_of_BilletsValue As String = String.Empty
							For Each Number_of_BilletsValueDataAttachment As DataAttachment In Number_of_BilletsValueDataAttachmentList.Items
								Number_of_BilletsValue = Number_of_BilletsValueDataAttachment.Text
							Next							
							
							Dim Billets As String= String.Empty	
							
								'If Number_of_BilletsValue <> "" Then
								If (Number_of_BilletsValue <> "" And Number_of_BilletsValue > "1") Then 
									Dim NumberofBillets As Integer = Number_of_BilletsValue.XFConvertToInt
									If NumberofBillets>0 Then 
										While NumberofBillets >0
											If NumberofBillets > 9 Then 
												Billets = "U6#LineItem_" & NumberofBillets & "," & Billets
												NumberofBillets = NumberofBillets-1
											Else
												Billets = "U6#LineItem_0" & NumberofBillets & "," & Billets
												NumberofBillets = NumberofBillets-1
											End If
										End While
										Billets = Billets.Remove(Billets.IndexOf(BilletSource),BilletSource.Length)
										Billets = Billets.Remove(Billets.LastIndexOf(","))					
									End If
								Else 
									
									'O/1 or "" for Billets
									Return Nothing
								
								End If
															
								Dim memListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
								Dim memListInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, Billets, Nothing)
								Dim memList As New MemberList(memListHeader, memListInfos)	
								
								Return memList
								
							End If
							


#End Region

#Region "GetRPMatchList"					
					
						'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetRPMatchList], SearchQuery=[|!prm_SearchQuery_OS!|], Appropriation=[|!prm_Approp_OS!|])
						If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetRPMatchList") Then
							
							Dim wfScenario As String = api.Workflow.GetWorkflowUnitInfo.ScenarioName	
							Dim wfYear As String = api.Workflow.GetWorkflowUnitInfo.TimeName
							Dim SearchQuery As String = args.MemberListArgs.NameValuePairs.XFGetValue("SearchQuery") 
							Dim Appropriation As String = args.MemberListArgs.NameValuePairs.XFGetValue("Appropriation")		
							Dim objUserName As String = api.SI.UserName
							Dim isAdmin As Boolean = BRApi.Security.Authorization.IsUserInAdminGroup(si)
							
							Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							
							'Cache scope deliberately excludes SearchQuery -- so repeated keystrokes in a live search
							'box reuse this same candidate set instead of re-querying and re-checking every RP's status
							Dim cacheKey As String = $"RPMatchCandidates_{Appropriation}_{wfYear}_{wfScenario}_{If(isAdmin, "ADMIN", objUserName)}"
							brapi.ErrorLog.LogMessage(si,$"Hit {cacheKey}")
							Dim candidateList As List(Of Tuple(Of MemberInfo, String))
							
							If globals.GetObject(cacheKey) Is Nothing Then
								
								'Cache miss -- resolve the base filter ONCE (folds the admin/non-admin branching that used
								'to duplicate the whole function into a single filter-string decision)
								Dim MemberFilterScriptWF As String = "F#FY" & wfYear.Substring(2,2) & "_RP.Base"
								Dim MemberFilterScriptWF_WV As String = "F#FY" & wfYear.Substring(2,2) & "_RP_WV.Base"
								Dim ScenarioMbrId As Integer = api.Members.GetMemberId(dimTypeId.Scenario, wfScenario)
								Dim objScenarioType As Integer = api.Scenario.GetScenarioType(ScenarioMbrId).Id
								Dim wftim As String = api.Scenario.GetWorkflowTime(ScenarioMbrId).ToString
								
								Dim mbrFilter As String = String.Empty
								
								If isAdmin Then
									mbrFilter = MemberFilterScriptWF & ".Where(Text8 Contains [_" & Appropriation & "_])," & MemberFilterScriptWF_WV & ".Where(Text8 Contains [_" & Appropriation & "_])"
								Else
									'Resolve the user's data-access-scoped filter once -- GetUser called only here, not twice
									Dim userParentGroupsDict As Dictionary(Of Guid, Group) = BRApi.Security.Authorization.GetUser(si, objUserName).ParentGroups
									Dim objCube As Cube = api.Cubes.GetCubeOrReferencedCubeForDataAccess(api.Pov.Cube.CubeId, api.Pov.EntityDim.DimPk.DimId)
									Dim CubeDataCellAccessItems As List(Of CubeDataAccessItem) = objCube.CubeDataCellAccessItems
									
									For Each Item As CubeDataAccessItem In CubeDataCellAccessItems
										If userParentGroupsDict.ContainsKey(Item.GroupUniqueID) Then
											Dim cubeMemberFilter As String = Item.GetCombinedMemberFilterString
											If cubeMemberFilter.StartsWith("F#") And cubeMemberFilter.Contains(".Where(Name DoesNotContain '_WV')") Then
												'No working-version access -- non-WV RPs only
												mbrFilter = MemberFilterScriptWF & ".Where(Text8 Contains [_" & Appropriation & "_])"
												Exit For
											ElseIf cubeMemberFilter.StartsWith("F#") Then
												'Full flow-member access
												mbrFilter = MemberFilterScriptWF & ".Where(Text8 Contains [_" & Appropriation & "_])," & MemberFilterScriptWF_WV & ".Where(Text8 Contains [_" & Appropriation & "_])"
												Exit For
											End If
										End If
									Next
								End If
								
								'No accessible data-access group matched -- mirrors the original's implicit fall-through-to-Nothing
								If mbrFilter = String.Empty Then
									Return Nothing
								End If
								
								'One GetMembersUsingFilter call, one pass over the results -- no second re-query later
								Dim rawCandidates As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, mbrFilter, Nothing)
								candidateList = New List(Of Tuple(Of MemberInfo, String))
								
								If rawCandidates IsNot Nothing Then
									For Each candidate As MemberInfo In rawCandidates
										Dim flowText1 As String = api.Flow.Text(candidate.Member.MemberId, 1, objScenarioType, wftim.XFConvertToInt())
										If flowText1.Contains("|") Then
											'Grab Text8 here too, once, so SearchQuery matching never needs another round trip
											Dim flowText8 As String = api.Flow.Text(candidate.Member.MemberId, 8, DimConstants.Unknown, DimConstants.Unknown)
											candidateList.Add(Tuple.Create(candidate, flowText8))
										End If
									Next
									candidateList = candidateList.OrderBy(Function(t) t.Item1.Member.Name).ToList()
								End If
								
								globals.SetObject(cacheKey, candidateList)
								
							Else
								candidateList = CType(globals.GetObject(cacheKey), List(Of Tuple(Of MemberInfo, String)))
							End If
							
							'Apply SearchQuery in-memory against the cached candidates -- fast even on every keystroke,
							'since it never touches the database
							Dim finalMembers As List(Of MemberInfo)
							If SearchQuery = "" Then
								finalMembers = candidateList.Select(Function(t) t.Item1).ToList()
							Else
								finalMembers = candidateList.Where(Function(t) t.Item2.XFContainsIgnoreCase(SearchQuery) OrElse t.Item1.Member.Description.XFContainsIgnoreCase(SearchQuery)).Select(Function(t) t.Item1).ToList()
							End If
							
							If finalMembers.Count = 0 Then
								Return Nothing ' was Return " " in the Object-typed BR; a String cannot convert to MemberList
							End If
							
							Return New MemberList(listHeader, finalMembers)
							
						End If
					
#End Region

#Region "GetCostEstimateRows"

	'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetCostEstimateRows])
	If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetCostEstimateRows") Then
		
		'Get time variable
		Dim wfTime As String = api.Workflow.GetWorkflowUnitInfo.TimeName
		Dim wfYY As String = wfTime.Substring(2,2)	
				
		Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
		Dim mbrScriptBuilder As New Text.StringBuilder
			
		'Get the member list Of ATUs To Loop through					
		Dim std_ATUDimPk As DimPk = api.Dimensions.GetDim("std_ATU").DimPk
		Dim total_ATUId As Integer = api.Members.GetMemberId(dimtypeId.UD4, "Total_ATU")		
		
		'Get the member list Of CostLines To Loop through					
		Dim std_CostLineDimPk As DimPk = api.Dimensions.GetDim("std_CostLine").DimPk
		Dim costEstimate_RollupId As Integer = api.Members.GetMemberId(dimtypeId.UD5, "CostEstimate_Rollup")
		
		'Declare variables to be used in the loops		
		Dim startingBuffer As DataBuffer
		Dim atuName As String = String.Empty
		Dim bufferUD4 As String = String.Empty
		Dim bufferUD4Len As Integer
		Dim msbUD4Filter As String = String.Empty
		Dim bufferUD5Id As Integer
		Dim bufferUD5Text6 As String = String.Empty
		Dim msbUD5Filter As String = String.Empty
		Dim costLineAncestors As New List(Of Member)
		Dim msbUD7Filter As String = String.Empty
							
		'The purpose of this buffer is to filter through the RP members for the current WFYear where they are tagged in Text1 with Budget (Status_03) and only show RPs where Funding exists
		startingBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveZeros(T#" & wfTime & ":O#Top:I#Top:A#Funding), F#FY" & wfYY & "_RP.Base, U1#Total_Appropriations.Base, U2#Total_Investment.Base, U3#Total_ObjectClass.Base, U4#Total_ATU.Base, U5#CostEstimate_Rollup.Base, U6#Top_UD6_LineItem.Base)")						
'		startingBuffer.LogDataBuffer(api, "startingBuffer", 1000)
		If Not startingBuffer Is Nothing Then	
			
			For Each startingCell As DataBufferCell In startingBuffer.DataBufferCells.Values									
				If Not startingCell.CellStatus.IsNoData() Then
					'only add Flow members to the list where Text1 contains Status_03 (Budget)
					If (api.Flow.Text(startingcell.DataBufferCellPk.FlowId,1).XFContainsIgnoreCase("Status_03"))
					
						'remove the _NoUnit in the UD4 member so that it only returns the parent member
						bufferUD4 = startingCell.GetUD4Name(api)
						If (Not bufferUD4.XFEqualsIgnoreCase("No_ATU"))
							bufferUD4Len = bufferUD4.Length
							msbUD4filter = bufferUD4.Remove(bufferUD4Len-7) 'remove the _NoUnit
						Else 
							msbUD4filter = bufferUD4
						End If
						
						'Get the ancenstors of the buffer ud5 member and determine which one is a child of CostEstimate_Rollup and return that child for the member filter
						bufferUD5Id = startingCell.DataBufferCellPk.UD5Id
						costLineAncestors = api.Members.GetAncestors(std_CostLineDimPk, bufferUD5Id, False)	
						'loop through the ancestors and determine which one is a child of CostEstimate_Rollup
						If Not costLineAncestors Is Nothing Then
							For Each costLineAncestor As Member In costLineAncestors
								If api.Members.IsChild(std_CostLineDimPk, costEstimate_RollupId, costLineAncestor.MemberId)
									msbUD5Filter = costLineAncestor.Name
								Else 
									'DoNothing
								End If
							Next
						Else
							msbUD5Filter = startingCell.GetUD5Name(api)
						End If
						
						'Get the UD7 Unique name from the text 6 property on the bufferUD5 member and add the UNQ_ prefix and add it to the list
						bufferUD5Text6 = api.UD5.Text(bufferUD5Id, 6)
						If (Not bufferUD5Text6.Length = 0)
							msbUD7Filter = "UNQ_" & bufferUD5Text6
						Else 
							msbud7Filter = "None"
						End If
						
						'build the member script
						mbrScriptBuilder.Append("F#" & startingCell.GetFlowName(api))
						mbrScriptBuilder.Append(":U1#" & startingCell.GetUD1Name(api))
						mbrScriptBuilder.Append(":U2#" & startingCell.GetUD2Name(api))
						mbrScriptBuilder.Append(":U3#" & startingCell.GetUD3Name(api))
						mbrScriptBuilder.Append(":U4#" & msbUD4filter)
						mbrScriptBuilder.Append(":U5#" & msbUD5Filter)
						mbrScriptBuilder.Append(":U6#" & startingCell.GetUD6Name(api))
						mbrScriptBuilder.Append(":U7#" & msbud7Filter & ",")
					Else 
						'Do nothing as the RP is not Status_03
					End If 'RP is status_03
				End If 'rPSourceCell.CellStatus.IsNoData() Then
			Next									
		End If 'startingBuffer
				
		Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, mbrScriptBuilder.ToString, Nothing)
		
		For Each listInfo As MemberInfo In listInfos
			
		Next
		
		 
		Return New MemberList(listHeader, listInfos)
									
		
	End If				

#End Region
							
						
						'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetRelatedRPsList], Appropriation=[|!prm_Approp_OS!|], FilterValue=FYRelated)
						'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetRelatedRPsList], Appropriation=[|!prm_Approp_OS!|], FilterValue=OlderRelated)
						If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetRelatedRPsList") Then
							
							Dim wfYearYY As Integer = api.Workflow.GetWorkflowUnitInfo.TimeName.Substring(2,2).XFConvertToInt
							Dim wfYearPriorYY As Integer = wfYearYY - 1
							Dim wfYearPriorTwoYY As Integer = wfYearYY - 2
							Dim wfYearPriorThreeYY As Integer = wfYearYY - 3
							Dim MemberFilterScriptFY As String = "F#FY" & wfYearYY & "_RP.Base"
							Dim MemberFilterScriptOlder As String = "F#FY" & wfYearPriorYY & "_RP.Base,F#FY" & wfYearPriorTwoYY & "_RP.Base,F#FY" & wfYearPriorThreeYY & "_RP.Base"
							Dim Appropriation As String = args.MemberListArgs.NameValuePairs.XFGetValue("Appropriation")	
							Dim FilterValue As String = args.MemberListArgs.NameValuePairs.XFGetValue("FilterValue") 	
															
							If FilterValue.XFEqualsIgnoreCase("FYRelated") Then
								Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
								Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, MemberFilterScriptFY, Nothing)
								'Sort the members
								Dim objMembers As List(Of Member) = Nothing
							   	If Not listInfos Is Nothing Then
						          	objMembers = (From memberInfo In listInfos Order By memberInfo.Member.Name Ascending Select memberInfo.Member).ToList()
						        End If
								Dim listList As New MemberList(listHeader, objMembers)						
								Return listList		
							Else If FilterValue.XFEqualsIgnoreCase("OlderRelated") Then
								Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
								Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, MemberFilterScriptOlder, Nothing)
								'Sort the members
								Dim objMembers As List(Of Member) = Nothing
							   	If Not listInfos Is Nothing Then
						          	objMembers = (From memberInfo In listInfos Order By memberInfo.Member.Name Ascending Select memberInfo.Member).ToList()
						        End If
								Dim listList As New MemberList(listHeader, objMembers)								
								Return listList		
							End If	
	
							
						End If
#Region "GetBudYearRPsWO_WV_ATList"					
						
						'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetBudYearRPsWO_WV_ATList])
						If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetBudYearRPsWO_WV_ATList") Then
							
							Dim wfYearYY As String = api.Workflow.GetWorkflowUnitInfo.TimeName.Substring(2,2)
							Dim MemberFilterScript As String = "F#FY" & wfYearYY & "_RP.Base.Where(Name DoesNotContain _WV)"
							
							Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, MemberFilterScript, Nothing)
							
							Return New MemberList(listHeader, listInfos)		
							
						End If
						
#End Region
		
#Region "GetRPToModRows"

	'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetRPToModRows], selectedMod=[|!prm_Mod_SelectedModHierachyName_ADM!|])
	If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetRPToModRows") Then
		
		'Get time variable
		Dim wfTime As String = api.Workflow.GetWorkflowUnitInfo.TimeName
		Dim wfYY As String = rpUtils.Get_WFTime_YY(si, wfTime)
		Dim wfYYPrior1 As String = (wfYY - 1).ToString
		Dim wfYYPrior2 As String = (wfYY - 2).ToString
		Dim wfYYPrior3 As String = (wfYY - 3).ToString
		Dim wfYYPrior4 As String = (wfYY - 4).ToString
		Dim wfTimeId As Integer = api.Members.GetMemberId(dimtypeid.Time, wfTime)
		Dim wfScenarioId As Integer = api.Members.GetMemberId(dimtypeid.Scenario, api.Workflow.GetWorkflowUnitInfo.ScenarioName)
		Dim scenarioTypeId As Integer = BRApi.Finance.Scenario.GetScenarioType(si, wfScenarioId).Id
		Dim selectedMod As String = args.MemberListArgs.NameValuePairs("selectedMod")
		
		'Get Whether selected mod is a descendant of OS, PC&I, R&D, etc.		
		Dim std_FlowDimPk As DimPk = api.Dimensions.GetDim("Std_Flow").DimPk
		Dim selectedModId As Integer = api.Members.GetMemberId(dimtypeid.Flow, selectedMod)
		
		'Is OS Descendant?
		Dim OS_ParentId As Integer = api.Members.GetMemberId(dimtypeid.Flow, "USCG_OS_" & wfYY)
		Dim isOSdescendant As Boolean = api.Members.IsDescendant(std_FlowDimPk, OS_ParentId, selectedModId)
				
		'Is PCI Descendant?
		Dim PCI_ParentId As Integer = api.Members.GetMemberId(dimtypeid.Flow, "USCG_PCI_" & wfYY)
		Dim isPCIdescendant As Boolean = api.Members.IsDescendant(std_FlowDimPk, PCI_ParentId, selectedModId)
		
		'Is RD Descendant?
		Dim RD_ParentId As Integer = api.Members.GetMemberId(dimtypeid.Flow, "USCG_RD_" & wfYY)
		Dim isRDdescendant As Boolean = api.Members.IsDescendant(std_FlowDimPk, RD_ParentId, selectedModId)
		
		'Is MERHCF Descendant
		Dim MERHCF_ParentId As Integer = api.Members.GetMemberId(dimtypeid.Flow, "USCG_MERHCF_" & wfYY)
		Dim isMERHCFdescendant As Boolean = api.Members.IsDescendant(std_FlowDimPk, MERHCF_ParentId, selectedModId)
		
		'Is RP Descendant?
		Dim RP_ParentId As Integer = api.Members.GetMemberId(dimtypeid.Flow, "USCG_RP_" & wfYY)
		Dim isRPdescendant As Boolean = api.Members.IsDescendant(std_FlowDimPk, RP_ParentId, selectedModId)
		
		'Is MOSP Descendant?
		Dim MOSP_ParentId As Integer = api.Members.GetMemberId(dimtypeid.Flow, "USCG_MOSP_" & wfYY)
		Dim isMOSPdescendant As Boolean = api.Members.IsDescendant(std_FlowDimPk, MOSP_ParentId, selectedModId)
		
		'Is F Descendant?
		Dim F_ParentId As Integer = api.Members.GetMemberId(dimtypeid.Flow, "USCG_F_" & wfYY)
		Dim isFdescendant As Boolean = api.Members.IsDescendant(std_FlowDimPk, F_ParentId, selectedModId)
		
		'Is BS Descendant?
		Dim BS_ParentId As Integer = api.Members.GetMemberId(dimtypeid.Flow, "USCG_BS_" & wfYY)
		Dim isBSdescendant As Boolean = api.Members.IsDescendant(std_FlowDimPk, BS_ParentId, selectedModId)				
				
		'Above Guidance
		'Is ABVOS Descendant?
		Dim ABVOS_ParentId As Integer = api.Members.GetMemberId(dimtypeid.Flow, "USCG_ABVOS_" & wfYY)
		Dim isABVOSdescendant As Boolean = api.Members.IsDescendant(std_FlowDimPk, ABVOS_ParentId, selectedModId)
		
		'Is ABVPCI Descendant?
		Dim ABVPCI_ParentId As Integer = api.Members.GetMemberId(dimtypeid.Flow, "USCG_ABVPCI_" & wfYY)
		Dim isABVPCIdescendant As Boolean = api.Members.IsDescendant(std_FlowDimPk, ABVPCI_ParentId, selectedModId)
		
		'Is ABVRD Descendant?
		Dim ABVRD_ParentId As Integer = api.Members.GetMemberId(dimtypeid.Flow, "USCG_ABVRD_" & wfYY)
		Dim isABVRDdescendant As Boolean = api.Members.IsDescendant(std_FlowDimPk, ABVRD_ParentId, selectedModId)
		
		'Is ABVMERHCF Descendant
		Dim ABVMERHCF_ParentId As Integer = api.Members.GetMemberId(dimtypeid.Flow, "USCG_ABVMERHCF_" & wfYY)
		Dim isABVMERHCFdescendant As Boolean = api.Members.IsDescendant(std_FlowDimPk, ABVMERHCF_ParentId, selectedModId)
		
		'Is ABVRP Descendant?
		Dim ABVRP_ParentId As Integer = api.Members.GetMemberId(dimtypeid.Flow, "USCG_ABVRP_" & wfYY)
		Dim isABVRPdescendant As Boolean = api.Members.IsDescendant(std_FlowDimPk, ABVRP_ParentId, selectedModId)
		
		'Is ABVMOSP Descendant?
		Dim ABVMOSP_ParentId As Integer = api.Members.GetMemberId(dimtypeid.Flow, "USCG_ABVMOSP_" & wfYY)
		Dim isABVMOSPdescendant As Boolean = api.Members.IsDescendant(std_FlowDimPk, ABVMOSP_ParentId, selectedModId)
		
		'Is ABVF Descendant?
		Dim ABVF_ParentId As Integer = api.Members.GetMemberId(dimtypeid.Flow, "USCG_ABVF_" & wfYY)
		Dim isABVFdescendant As Boolean = api.Members.IsDescendant(std_FlowDimPk, ABVF_ParentId, selectedModId)
		
		'Is ABVBS Descendant?
		Dim ABVBS_ParentId As Integer = api.Members.GetMemberId(dimtypeid.Flow, "USCG_ABVBS_" & wfYY)
		Dim isABVBSdescendant As Boolean = api.Members.IsDescendant(std_FlowDimPk, ABVBS_ParentId, selectedModId)	
		
		'the waterfall switch controls whether to show RPs with a Status_03 (Budget) or Status_04 (Above Guidance)
'		Dim waterfall As String = args.MemberListArgs.NameValuePairs("waterfall")
		
		Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
		Dim mbrScriptBuilder As New Text.StringBuilder
				
		'declare the memberfilter variable to populate and be used in the buffer depending on the appropriation type
		Dim memberFilter As New Text.StringBuilder
		Dim annTermMemberFilter As New Text.StringBuilder
		
		'The purpose of this buffer is to filter through the RP members for the current WFYear and only show RPs where Funding exists		
		Dim startingBuffer As DataBuffer
		
		'Declare and set and Appropriation filter to filter the RPs
		Dim appropFilter As String = String.Empty
		
		Select Case True
		Case isOSdescendant,isABVOSdescendant
			appropFilter = "_OS_"
		Case isPCIdescendant,isABVPCIdescendant
			appropFilter = "_PCI_"
		Case isRDdescendant,isABVRDdescendant
			appropFilter = "_RD_"
		Case isMERHCFdescendant,isABVMERHCFdescendant
			appropFilter = "_MERHCF_"
		Case isRPdescendant,isABVRPdescendant
			appropFilter = "_RP_"
		Case isMOSPdescendant,isABVMOSPdescendant
			appropFilter = "_MOSP_"
		Case isFdescendant,isABVFdescendant
			appropFilter = "_F_"
		Case isBSdescendant,isABVBSdescendant
			appropFilter = "_BS_"
		End Select
		
		
		Select Case True
		Case (isOSdescendant Or isMERHCFdescendant Or isRPdescendant Or isMOSPdescendant Or isFdescendant Or isBSdescendant Or isABVOSdescendant Or isABVMERHCFdescendant Or isABVRPdescendant Or isABVMOSPdescendant Or isABVFdescendant Or isABVBSdescendant)
			memberFilter.Append("F#FY" & wfYY & "_RP.Base,")
			annTermMemberFilter.Append("F#FY" & wfYYPrior1 & "_AnnTerm.Base,")
			
			#Region "Buffer for OS, MERHCF, RP, MOSP, F, BS"
				
				If (Not (selectedMod.XFContainsIgnoreCase("Ann") Or selectedMod.XFContainsIgnoreCase("Trm")))
					startingBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveZeros(T#" & wfTime & ":O#Top:I#Top:A#Funding:U1#Total_Appropriations:U2#Total_Investment:U3#Total_ObjectClass:U4#Total_ATU:U5#Total_CostLine:U6#Top_UD6_LineItem:U7#None:U8#None),  " & memberFilter.ToString & ")")	
	
					If Not startingBuffer Is Nothing Then					
							For Each startingCell As DataBufferCell In startingBuffer.DataBufferCells.Values									
								If Not startingCell.CellStatus.IsNoData() Then
									Dim RPName As String = startingCell.GetFlowName(api)
									Dim RPId As String = startingCell.DataBufferCellPk.FlowId
									'We only want to include RPs that don't have a mod assigned in text 7 or have the selected mod assigned
									Dim modAssigedToRP As String = api.Flow.Text(RPId, 7, scenarioTypeId, wfTimeId)
									If (String.IsNullOrWhiteSpace(modAssigedToRP) Or selectedMod.XFEqualsIgnoreCase(modAssigedToRP))
										'Evaluate the RP status and return only Status_O3 or Status_04 RPs depending on the switch,
										'first need to get the text 1 and parse the value as field 1 holds the Status value
										Dim rpText1 As String = api.Flow.Text(RPId, 1, scenarioTypeId, wfTimeId)
										Dim rpText8 As String = api.Flow.Text(RPId, 8, DimConstants.Unknown, DimConstants.Unknown)
										
										If rpText8.XFContainsIgnoreCase(appropFilter) 'check if OS Appropriation
																				
											If (Not rpText1="")
												Dim RPStatus As String = StringHelper.SplitString(rpText1,"|").Item(0)							
												
												If (Not selectedMod.XFContainsIgnoreCase("ABV") And RPStatus = "Status_03") 'Budget
													mbrScriptBuilder.Append("F#" & RPName & ",")	
												Else If (selectedMod.XFContainsIgnoreCase("ABV") And RPStatus = "Status_04")
													mbrScriptBuilder.Append("F#" & RPName & ",")	
												Else 'Do Nothing
												End If
											Else 'Do Nothing
											End If
										End If
									End If
								End If 'rPSourceCell.CellStatus.IsNoData() Then
							Next		
						Else 
							mbrScriptBuilder.Append("F#No_RPs_Meet_Criteria")				
						End If 'startingBuffer	
				
				Else 'must be AnnTerm so return that DataBuffer filter
					startingBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveZeros(T#" & wfTime & ":O#Top:I#Top:A#Funding:U1#Total_Appropriations:U2#Total_Investment:U3#Total_ObjectClass:U4#Total_ATU:U5#Total_CostLine:U6#Top_UD6_LineItem:U7#None:U8#None),  " & annTermMemberFilter.ToString & ")")	
		 
					If Not startingBuffer.DataBufferCells.Count = 0 Then	
						For Each startingCell As DataBufferCell In startingBuffer.DataBufferCells.Values									
							If Not startingCell.CellStatus.IsNoData() Then
								Dim RPName As String = startingCell.GetFlowName(api)
								Dim RPId As String = startingCell.DataBufferCellPk.FlowId
								'We only want to include RPs that don't have a mod assigned in text 7 or have the selected mod assigned
								Dim modAssigedToRP As String = api.Flow.Text(RPId, 7, scenarioTypeId, wfTimeId)
								If (String.IsNullOrWhiteSpace(modAssigedToRP) Or selectedMod.XFEqualsIgnoreCase(modAssigedToRP))
									Dim rpText8 As String = api.Flow.Text(RPId, 8, DimConstants.Unknown, DimConstants.Unknown)
									
									If rpText8.XFContainsIgnoreCase(appropFilter) 'check if OS Appropriation
										mbrScriptBuilder.Append("F#" & RPName & ",")
									End If
								End If
							End If 'rPSourceCell.CellStatus.IsNoData() Then
						Next		
					Else 'startingbuffercount = 0 so return a default
						mbrScriptBuilder.Append("F#No_RPs_Meet_Criteria")				
					End If 'startingBuffer		
				
				End If
			
			#End Region
			
		Case (isPCIdescendant Or isRDdescendant Or isFdescendant Or isABVPCIdescendant Or isABVRDdescendant Or isABVFdescendant)
			memberFilter.Append("F#FY" & wfYY & "_RP.Base,")	
			memberFilter.Append("F#FY" & wfYYPrior1 & "_RPs.Base,")	
			memberFilter.Append("F#FY" & wfYYPrior2 & "_RPs.Base,")	
			memberFilter.Append("F#FY" & wfYYPrior3 & "_RPs.Base,")	
			memberFilter.Append("F#FY" & wfYYPrior4 & "_RPs.Base,")	
			
'			If selectedMod = "USCG_PGM_2740_27_PC" Then
'				BRApi.ErrorLog.LogMessage(si, "ISVS RP Correct, member filter: " & memberFilter.ToString)
'			End If
						
			#Region "Buffer for PCI, RD, F"
			
				'For PCI since it is Zero Based Funding, get the 5 prior workflow years
				startingBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveZeros(T#" & wfTime & ":O#Top:I#Top:A#Funding:U1#Total_Appropriations:U2#Total_Investment:U3#Total_ObjectClass:U4#Total_ATU:U5#Total_CostLine:U6#Top_UD6_LineItem:U7#None:U8#None), " & memberFilter.ToString & ")")	

				If Not startingBuffer Is Nothing Then					
						For Each startingCell As DataBufferCell In startingBuffer.DataBufferCells.Values									
							If Not startingCell.CellStatus.IsNoData() Then
								Dim RPName As String = startingCell.GetFlowName(api)
								Dim RPId As String = startingCell.DataBufferCellPk.FlowId
								'BRApi.ErrorLog.LogMessage(si, "RP for ISVS Assignment: " & RPName)
								If RPName = "25_PCI_VES_ISVS_YARD" Then
									BRApi.ErrorLog.LogMessage(si, "25 ISVS YARD Debug 1")
								End If
								'We only want to include RPs that don't have a mod assigned in text 7 or have the selected mod assigned
								Dim modAssigedToRP As String = api.Flow.Text(RPId, 7, scenarioTypeId, wfTimeId)
								If (String.IsNullOrWhiteSpace(modAssigedToRP) Or selectedMod.XFEqualsIgnoreCase(modAssigedToRP))
									'Evaluate the RP status and return only Status_O3 or Status_04 RPs depending on the switch,
									'first need to get the text 1 and parse the value as field 1 holds the Status value
									Dim rpText1 As String = api.Flow.Text(RPId, 1, scenarioTypeId, wfTimeId)
									Dim rpText8 As String = api.Flow.Text(RPId, 8, DimConstants.Unknown, DimConstants.Unknown)
									
									If rpText8.XFContainsIgnoreCase(appropFilter) 'check if PCI Appropriation																								
														
										If (Not rpText1="")
											Dim RPStatus As String = StringHelper.SplitString(rpText1,"|").Item(0)	
											
											If (Not selectedMod.XFContainsIgnoreCase("ABV") And RPStatus = "Status_03") 'Budget
												mbrScriptBuilder.Append("F#" & RPName & ",")	
											Else If (selectedMod.XFContainsIgnoreCase("ABV") And RPStatus = "Status_04") 'Above Guidance
												mbrScriptBuilder.Append("F#" & RPName & ",")	
											Else 'Do Nothing
												
											End If
										'Else if Text1 = blank for this Budget Year, pull in a list of prior year RPs for this appropriation that might have funding in this year that was copied forward.
										Else If	(Not selectedMod.XFContainsIgnoreCase("ABV") And (RPName.XFContainsIgnoreCase(wfYYPrior1) Or RPName.XFContainsIgnoreCase(wfYYPrior2) Or RPName.XFContainsIgnoreCase(wfYYPrior3) Or RPName.XFContainsIgnoreCase(wfYYPrior4)))
												mbrScriptBuilder.Append("F#" & RPName & ",") 
										End If
									End If
								End If
							End If 'rPSourceCell.CellStatus.IsNoData() Then
						Next		
					Else 
						mbrScriptBuilder.Append("F#No_RPs_Meet_Criteria")				
					End If 'startingBuffer	
			
			#End Region
			
		End Select
			
		If mbrScriptBuilder.Length = 0
			mbrScriptBuilder.Append("F#No_RPs_Meet_Criteria")
		End If
				
		
		Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, mbrScriptBuilder.ToString, Nothing)
		
		Return New MemberList(listHeader, listInfos)
									
		
	End If
							

#End Region
	
#Region "GetModMoveRelationshipMems"

	'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetModMoveRelationshipMems], selectedMember=[|!prm_Mod_SelectedModHierachyName_ADM!|])
	If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetModMoveRelationshipMems") Then
		
		'Get time variable
		Dim wfTime As String = api.Workflow.GetWorkflowUnitInfo.ScenarioName	
		Dim wfYY As String = wfTime.Substring(2,2)
		Dim selectedMember As String = args.MemberListArgs.NameValuePairs("selectedMember")
		
		If (Not selectedMember.XFEqualsIgnoreCase("None"))				
			Dim selectedMemberId As Integer = api.Members.GetMemberId(dimTypeId.Flow, selectedMember)		
			Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
			Dim mbrScriptBuilder As New Text.StringBuilder			
			Dim std_FlowDimPk As DimPk = api.Dimensions.GetDim("Std_Flow").DimPk
			Dim parents As List(Of Member) = api.Members.GetParents(std_FlowDimPk, selectedMemberId, False)
			'Get the siblings from the first parent of the member in the hierarchy
			Dim siblings As List(Of Member) = api.Members.GetChildren(std_FlowDimPk, parents(0).MemberId)
				
			'add the siblings to a msb
			If siblings.Count > 1
				For Each sibling As Member In siblings					
					'don't add the selected member
					If (Not sibling.Name.XFEqualsIgnoreCase(selectedMember))					
						mbrScriptBuilder.Append("F#" & sibling.Name & ",")	
					End If
				Next 
			Else If siblings.Count = 1
				mbrScriptBuilder.Append(" ")
			End If					
						
			Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, mbrScriptBuilder.ToString, Nothing)
			
			Return New MemberList(listHeader, listInfos)	
		Else 
			Return Nothing
		End If
		
	End If
							

#End Region
			
#Region "GetUSCG_DHSRows"

	#Region "ModDataRows"
	'******This first set of functions will retrieve the dollar rows for the Mods
	
		#Region "Standard ModDataRows"
		'******This function is for Standard Modifications. This is no longer used as of 3/18/24 as we use the individual buffers below to make the cube view faster
	
		'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetUSCG_DHS_ModDataRows])
		If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetUSCG_DHS_ModDataRows") Then
			
			'Get time variable
			Dim wfTime As String = api.Workflow.GetWorkflowUnitInfo.TimeName		
			Dim wfYY As String = wfTime.Substring(2,2)
			
			Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
			Dim mbrScriptBuilder As New Text.StringBuilder
					
			'The purpose of this buffer is to filter through the RP members for the current WFYear and only show RPs where Funding exists
			Dim startingBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveZeros(T#" & wfTime & ":O#Top:I#Top:A#Funding:U4#Total_ATU:U5#Total_CostLine:U6#Top_UD6_LineItem:U7#None:U8#None), F#USCG_FY" & wfYY & "_Mods.Base, U1#Total_Appropriations.Base, U2#Total_Investment.Base, U3#Total_ObjectClass.Base)")	
	'		startingBuffer.LogDataBuffer(api, " Cost Estimate DB", 1000)						
			If Not startingBuffer Is Nothing Then	
				
				For Each startingCell As DataBufferCell In startingBuffer.DataBufferCells.Values									
					If Not startingCell.CellStatus.IsNoData() Then
						Dim flowName As String = startingCell.GetFlowName(api)
						Dim ud1Name As String = startingCell.GetUD1Name(api)
						Dim ud2Name As String = startingCell.GetUD2Name(api)
						Dim ud3Name As String = startingCell.GetUD3Name(api)
						'If the flowName (RP Name) doesn't contain _Ann or _Term or _AT because we don't want Ann/Terms in the data we are importing into DHS because it is already created in DHS
						If (Not flowName.XFContainsIgnoreCase("_Ann") Or flowName.XFContainsIgnoreCase("_Term") Or flowName.XFContainsIgnoreCase("_AT"))
							mbrScriptBuilder.Append("F#" & flowName & ":U1#" & ud1Name & ":U2#" & ud2Name & ":U3#" & ud3Name & ",")	
						End If
					End If 'rPSourceCell.CellStatus.IsNoData() Then
				Next									
			End If 'startingBuffer
			
			If mbrScriptBuilder.Length = 0
				mbrScriptBuilder.Append("F#No_RPs_Meet_Criteria")
			End If
			
			Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, mbrScriptBuilder.ToString, Nothing)
			
			Return New MemberList(listHeader, listInfos)
			
		#End Region
			
		#Region "Standard ModDataRows_BilletDollars"
		'******This function is for Standard Modifications For Billet Dollar Rows
	
		'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetUSCG_DHS_ModDataRows_BilletDollars])
		Else If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetUSCG_DHS_ModDataRows_BilletDollars") Then
			
			'Get time variable
			Dim wfTime As String = api.Workflow.GetWorkflowUnitInfo.TimeName		
			Dim wfYY As String = wfTime.Substring(2,2)
			
			Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
			Dim mbrScriptBuilder As New Text.StringBuilder
			
			'Declare the member script filters
			Dim ud6Name As String = "Total_Billet_Line_Items"
			Dim actName As String = String.Empty
			Dim flowName As String = String.Empty
			Dim ud1Name As String = String.Empty
			Dim ud2Name As String = String.Empty
			Dim ud3Name As String = String.Empty
			
			'The purpose of this buffer is to filter through the RP members for the current WFYear and only show RPs where Funding exists
			Dim startingBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveZeros(T#" & wfTime & ":O#Top:I#Top:U4#Total_ATU:U5#Total_CostLine:U6#" & ud6Name & ":U7#None:U8#None), A#Funding_Recurring_Input, A#Funding_NonRecurring_Input, F#USCG_FY" & wfYY & "_Mods.Base, U1#Total_Appropriations.Base, U2#Total_Investment.Base, U3#Total_ObjectClass.Base)")	
			'startingBuffer.LogDataBuffer(api, " Billets DB", 1000)						
			If Not startingBuffer Is Nothing Then						
				For Each startingCell As DataBufferCell In startingBuffer.DataBufferCells.Values									
					If Not startingCell.CellStatus.IsNoData() Then
						actName = startingCell.GetAccountName(api)
						flowName = startingCell.GetFlowName(api)
						ud1Name = startingCell.GetUD1Name(api)
						ud2Name = startingCell.GetUD2Name(api)
						ud3Name = startingCell.GetUD3Name(api)
						'If the flowName (RP Name) doesn't contain _Ann or _Term or _AT because we don't want Ann/Terms in the data we are importing into DHS because it is already created in DHS
						If (Not flowName.XFContainsIgnoreCase("_Ann") Or flowName.XFContainsIgnoreCase("_Term") Or flowName.XFContainsIgnoreCase("_AT"))
							mbrScriptBuilder.Append("A#" & actName & ":F#" & flowName & ":U1#" & ud1Name & ":U2#" & ud2Name & ":U3#" & ud3Name & ":U6#" & ud6Name & ",")
						End If
					End If 'rPSourceCell.CellStatus.IsNoData() Then
				Next									
			End If 'startingBuffer
			
			If mbrScriptBuilder.Length = 0
				mbrScriptBuilder.Append("F#No_RPs_Meet_Criteria")
			End If
			
			Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, mbrScriptBuilder.ToString, Nothing)
			
			Return New MemberList(listHeader, listInfos)
			
		#End Region
			
		#Region "Standard ModDataRows_GenDetailDollars"
		'******This function is for Standard Modifications For GenDetail Dollar Rows
	
		'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetUSCG_DHS_ModDataRows_GenDetailDollars])
		Else If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetUSCG_DHS_ModDataRows_GenDetailDollars") Then
			
			'Get time variable
			Dim wfTime As String = api.Workflow.GetWorkflowUnitInfo.TimeName		
			Dim wfYY As String = wfTime.Substring(2,2)
			
			Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
			Dim mbrScriptBuilder As New Text.StringBuilder
					
			'Declare the member script filters
			Dim ud6Name As String = "Total_GenDetail_Line_Items"
			Dim actName As String = String.Empty
			Dim flowName As String = String.Empty
			Dim ud1Name As String = String.Empty
			Dim ud2Name As String = String.Empty
			Dim ud3Name As String = String.Empty
			
			'The purpose of this buffer is to filter through the RP members for the current WFYear and only show RPs where Funding exists
			Dim startingBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveZeros(T#" & wfTime & ":O#Top:I#Top:U4#Total_ATU:U5#Total_CostLine:U6#" & ud6Name & ":U7#None:U8#None), A#Funding_Recurring_Input, A#Funding_NonRecurring_Input, F#USCG_FY" & wfYY & "_Mods.Base, U1#Total_Appropriations.Base, U2#Total_Investment.Base, U3#Total_ObjectClass.Base)")	
	'		startingBuffer.LogDataBuffer(api, " Cost Estimate DB", 1000)						
			If Not startingBuffer Is Nothing Then	
				
				For Each startingCell As DataBufferCell In startingBuffer.DataBufferCells.Values									
					If Not startingCell.CellStatus.IsNoData() Then
						actName = startingCell.GetAccountName(api)
						flowName = startingCell.GetFlowName(api)
						ud1Name = startingCell.GetUD1Name(api)
						ud2Name = startingCell.GetUD2Name(api)
						ud3Name = startingCell.GetUD3Name(api)
						'If the flowName (RP Name) doesn't contain _Ann or _Term or _AT because we don't want Ann/Terms in the data we are importing into DHS because it is already created in DHS
						If (Not flowName.XFContainsIgnoreCase("_Ann") Or flowName.XFContainsIgnoreCase("_Term") Or flowName.XFContainsIgnoreCase("_AT"))
							mbrScriptBuilder.Append("A#" & actName & ":F#" & flowName & ":U1#" & ud1Name & ":U2#" & ud2Name & ":U3#" & ud3Name & ":U6#" & ud6Name & ",")	
						End If
					End If 'rPSourceCell.CellStatus.IsNoData() Then
				Next									
			End If 'startingBuffer
			
			If mbrScriptBuilder.Length = 0
				mbrScriptBuilder.Append("F#No_RPs_Meet_Criteria")
			End If
			
			Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, mbrScriptBuilder.ToString, Nothing)
			
			Return New MemberList(listHeader, listInfos)
			
		#End Region
			
		#Region "Standard ModDataRows_CstEstDollars"
		'******This function is for Standard Modifications For CstEstDollars Dollar Rows
	
		'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetUSCG_DHS_ModDataRows_CstEstDollars])
		Else If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetUSCG_DHS_ModDataRows_CstEstDollars") Then
			
			'Get time variable
			Dim wfTime As String = api.Workflow.GetWorkflowUnitInfo.TimeName		
			Dim wfYY As String = wfTime.Substring(2,2)
			
			Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
			Dim mbrScriptBuilder As New Text.StringBuilder
					
			'Declare the member script filters
			Dim ud6Name As String = String.Empty
			Dim actName As String = String.Empty
			Dim flowName As String = String.Empty
			Dim ud1Name As String = String.Empty
			Dim ud2Name As String = String.Empty
			Dim ud3Name As String = String.Empty
			
			'The purpose of this buffer is to filter through the RP members for the current WFYear and only show RPs where Funding exists
			Dim startingBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveZeros(T#" & wfTime & ":O#Top:I#Top:U4#Total_ATU:U5#Total_CostLine:U7#None:U8#None), A#Funding_Recurring_Input, A#Funding_NonRecurring_Input, F#USCG_FY" & wfYY & "_Mods.Base, U1#Total_Appropriations.Base, U2#Total_Investment.Base, U3#Total_ObjectClass.Base, U6#Total_NonBillet_LineItems.Base)")	
	'		startingBuffer.LogDataBuffer(api, " Cost Estimate DB", 1000)						
			If Not startingBuffer Is Nothing Then	
				
				For Each startingCell As DataBufferCell In startingBuffer.DataBufferCells.Values									
					If Not startingCell.CellStatus.IsNoData() Then
						actName = startingCell.GetAccountName(api)
						flowName = startingCell.GetFlowName(api)
						ud1Name = startingCell.GetUD1Name(api)
						ud2Name = startingCell.GetUD2Name(api)
						ud3Name = startingCell.GetUD3Name(api)
						ud6Name = startingCell.GetUD6Name(api)
						'If the flowName (RP Name) doesn't contain _Ann or _Term or _AT because we don't want Ann/Terms in the data we are importing into DHS because it is already created in DHS
						If (Not flowName.XFContainsIgnoreCase("_Ann") Or flowName.XFContainsIgnoreCase("_Term") Or flowName.XFContainsIgnoreCase("_AT"))
							mbrScriptBuilder.Append("A#" & actName & ":F#" & flowName & ":U1#" & ud1Name & ":U2#" & ud2Name & ":U3#" & ud3Name & ":U6#" & ud6Name & ",")	
						End If
					End If 'rPSourceCell.CellStatus.IsNoData() Then
				Next									
			End If 'startingBuffer
			
			If mbrScriptBuilder.Length = 0
				mbrScriptBuilder.Append("F#No_RPs_Meet_Criteria")
			End If
			
			Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, mbrScriptBuilder.ToString, Nothing)
			
			Return New MemberList(listHeader, listInfos)
			
		#End Region
			
		#Region "Standard ModDataRows_ExpDollars"
		'******This function is for Standard Modifications For CstEstDollars Dollar Rows
	
		'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetUSCG_DHS_ModDataRows_ExpDollars])
		Else If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetUSCG_DHS_ModDataRows_ExpDollars") Then
			
			'Get time variable
			Dim wfTime As String = api.Workflow.GetWorkflowUnitInfo.TimeName		
			Dim wfYY As String = wfTime.Substring(2,2)
			
			Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
			Dim mbrScriptBuilder As New Text.StringBuilder
					
			'Declare the member script filters
			Dim ud6Name As String = String.Empty
			Dim actName As String = String.Empty
			Dim flowName As String = String.Empty
			Dim ud1Name As String = String.Empty
			Dim ud2Name As String = String.Empty
			Dim ud3Name As String = String.Empty
			
			'The purpose of this buffer is to filter through the RP members for the current WFYear and only show RPs where Funding exists
			Dim startingBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveZeros(T#" & wfTime & ":O#Top:I#Top:U4#Total_ATU:U5#Total_CostLine:U7#None:U8#None), A#Funding_Recurring_Input, A#Funding_NonRecurring_Input, F#USCG_FY" & wfYY & "_Mods.Base, U1#Total_Appropriations.Base, U2#Total_Investment.Base, U3#Total_ObjectClass.Base, U6#Total_Expense_LineItems.Base)")	
	'		startingBuffer.LogDataBuffer(api, " Cost Estimate DB", 1000)						
			If Not startingBuffer Is Nothing Then	
				
				For Each startingCell As DataBufferCell In startingBuffer.DataBufferCells.Values									
					If Not startingCell.CellStatus.IsNoData() Then
						actName = startingCell.GetAccountName(api)
						flowName = startingCell.GetFlowName(api)
						ud1Name = startingCell.GetUD1Name(api)
						ud2Name = startingCell.GetUD2Name(api)
						ud3Name = startingCell.GetUD3Name(api)
						ud6Name = startingCell.GetUD6Name(api)
						'If the flowName (RP Name) doesn't contain _Ann or _Term or _AT because we don't want Ann/Terms in the data we are importing into DHS because it is already created in DHS
						If (Not flowName.XFContainsIgnoreCase("_Ann") Or flowName.XFContainsIgnoreCase("_Term") Or flowName.XFContainsIgnoreCase("_AT"))
							mbrScriptBuilder.Append("A#" & actName & ":F#" & flowName & ":U1#" & ud1Name & ":U2#" & ud2Name & ":U3#" & ud3Name & ":U6#" & ud6Name & ",")	
						End If
					End If 'rPSourceCell.CellStatus.IsNoData() Then
				Next									
			End If 'startingBuffer
			
			If mbrScriptBuilder.Length = 0
				mbrScriptBuilder.Append("F#No_RPs_Meet_Criteria")
			End If
			
			Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, mbrScriptBuilder.ToString, Nothing)
			
			Return New MemberList(listHeader, listInfos)
			
		#End Region
			
		#Region "Standard ModDataRows_ZeroBaseDollars"
		'******This function is for Standard Modifications For ZeroBaseDollars Dollar Rows
	
		'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetUSCG_DHS_ModDataRows_ZeroBaseDollars])
		Else If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetUSCG_DHS_ModDataRows_ZeroBaseDollars") Then
			
			'Get time variable
			Dim wfTime As String = api.Workflow.GetWorkflowUnitInfo.TimeName		
			Dim wfTimeNext1 As String = wfTime+1		
			Dim wfTimeNext2 As String = wfTime+2	
			Dim wfTimeNext3 As String = wfTime+3	
			Dim wfTimeNext4 As String = wfTime+4
			Dim wfYY As String = wfTime.Substring(2,2)
			
			Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
			Dim mbrScriptBuilder As New Text.StringBuilder
					
			'Declare the member script filters
			Dim ud6Name As String = String.Empty
			Dim actName As String = String.Empty
			Dim flowName As String = String.Empty
			Dim ud1Name As String = String.Empty
			Dim ud2Name As String = String.Empty
			Dim ud3Name As String = String.Empty
			
			'The purpose of this buffer is to filter through the RP members for the current WFYear and only show RPs where Funding exists
			Dim wfTimeBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveZeros(T#" & wfTime & ":O#Top:I#Top:U4#Total_ATU:U5#Total_CostLine:U7#None:U8#None), A#ZeroBase_Funding, F#USCG_FY" & wfYY & "_Mods.Base, U1#Total_Appropriations.Base, U2#Total_Investment.Base, U3#Total_ObjectClass.Base, U6#Total_Expense_LineItems.Base)")	
			Dim wfTimeNext1Buffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveZeros(T#" & wfTimeNext1 & ":O#Top:I#Top:U4#Total_ATU:U5#Total_CostLine:U7#None:U8#None), A#ZeroBase_Funding, F#USCG_FY" & wfYY & "_Mods.Base, U1#Total_Appropriations.Base, U2#Total_Investment.Base, U3#Total_ObjectClass.Base, U6#Total_Expense_LineItems.Base)")	
			Dim wfTimeNext2Buffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveZeros(T#" & wfTimeNext2 & ":O#Top:I#Top:U4#Total_ATU:U5#Total_CostLine:U7#None:U8#None), A#ZeroBase_Funding, F#USCG_FY" & wfYY & "_Mods.Base, U1#Total_Appropriations.Base, U2#Total_Investment.Base, U3#Total_ObjectClass.Base, U6#Total_Expense_LineItems.Base)")	
			Dim wfTimeNext3Buffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveZeros(T#" & wfTimeNext3 & ":O#Top:I#Top:U4#Total_ATU:U5#Total_CostLine:U7#None:U8#None), A#ZeroBase_Funding, F#USCG_FY" & wfYY & "_Mods.Base, U1#Total_Appropriations.Base, U2#Total_Investment.Base, U3#Total_ObjectClass.Base, U6#Total_Expense_LineItems.Base)")	
			Dim wfTimeNext4Buffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveZeros(T#" & wfTimeNext4 & ":O#Top:I#Top:U4#Total_ATU:U5#Total_CostLine:U7#None:U8#None), A#ZeroBase_Funding, F#USCG_FY" & wfYY & "_Mods.Base, U1#Total_Appropriations.Base, U2#Total_Investment.Base, U3#Total_ObjectClass.Base, U6#Total_Expense_LineItems.Base)")	
			Dim superBuffer As DataBuffer = wfTimeBuffer + wfTimeNext1Buffer + wfTimeNext2Buffer + wfTimeNext3Buffer + wfTimeNext4Buffer
			'superBuffer.LogDataBuffer(api, " Cost Estimate DB " , 1000)						
			If Not superBuffer Is Nothing Then	
				
				For Each startingCell As DataBufferCell In superBuffer.DataBufferCells.Values									
					If Not startingCell.CellStatus.IsNoData() Then
						actName = startingCell.GetAccountName(api)
						flowName = startingCell.GetFlowName(api)
						ud1Name = startingCell.GetUD1Name(api)
						ud2Name = startingCell.GetUD2Name(api)
						ud3Name = startingCell.GetUD3Name(api)
						ud6Name = startingCell.GetUD6Name(api)
						'If the flowName (RP Name) doesn't contain _Ann or _Term or _AT because we don't want Ann/Terms in the data we are importing into DHS because it is already created in DHS
						If (Not flowName.XFContainsIgnoreCase("_Ann") Or flowName.XFContainsIgnoreCase("_Term") Or flowName.XFContainsIgnoreCase("_AT"))
							mbrScriptBuilder.Append("A#" & actName & ":F#" & flowName & ":U1#" & ud1Name & ":U2#" & ud2Name & ":U3#" & ud3Name & ":U6#" & ud6Name & ",")	
						End If
					End If 'rPSourceCell.CellStatus.IsNoData() Then
				Next									
			End If 'startingBuffer
			
			
			If mbrScriptBuilder.Length = 0
				mbrScriptBuilder.Append("F#No_RPs_Meet_Criteria")
			End If
			
			Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, mbrScriptBuilder.ToString, Nothing)
			
			Return New MemberList(listHeader, listInfos)
			
		#End Region
			
		#Region "Standard ModDataPositionRows"
		'******This function is for Standard Modifications for Positions
	
		'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetUSCG_DHS_ModDataRows_Positions])
		Else If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetUSCG_DHS_ModDataRows_Positions") Then
			
			'Get time variable
			Dim wfTime As String = api.Workflow.GetWorkflowUnitInfo.TimeName		
			Dim wfYY As String = wfTime.Substring(2,2)
			
			Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
			Dim mbrScriptBuilder As New Text.StringBuilder
					
			'Declare the member script filters
			Dim ud6Name As String = String.Empty
			Dim actName As String = String.Empty
			Dim flowName As String = String.Empty
			Dim ud1Name As String = String.Empty
			Dim ud2Name As String = String.Empty
			Dim ud3Name As String = String.Empty
			
			'The purpose of this buffer is to filter through the RP members for the current WFYear and only show RPs where Funding exists and then use this to generaate the dynamic calc for positions
			Dim startingBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveZeros(T#" & wfTime & ":O#Top:I#Top:U4#Total_ATU:U5#Total_CostLine:U7#None:U8#None), A#Funding_Recurring_Input, F#USCG_FY" & wfYY & "_Mods.Base, U1#Total_Appropriations.Base, U2#Total_Investment.Base, U3#11_1,U3#11_3,U3#11_7, U6#Total_Billet_Line_Items.Base, U6#Total_GenDetail_Line_Items.Base)")	
			'startingBuffer.LogDataBuffer(api, " Positions DB", 1000)						
			If Not startingBuffer Is Nothing Then	
				
				For Each startingCell As DataBufferCell In startingBuffer.DataBufferCells.Values									
					If Not startingCell.CellStatus.IsNoData() Then
						actName = startingCell.GetAccountName(api)
						flowName = startingCell.GetFlowName(api)
						ud1Name = startingCell.GetUD1Name(api)
						ud2Name = startingCell.GetUD2Name(api)
						ud3Name = startingCell.GetUD3Name(api)
						ud6Name = startingCell.GetUD6Name(api)
						'If the flowName (RP Name) doesn't contain _Ann or _Term or _AT because we don't want Ann/Terms in the data we are importing into DHS because it is already created in DHS
						If (Not flowName.XFContainsIgnoreCase("_Ann") Or flowName.XFContainsIgnoreCase("_Term") Or flowName.XFContainsIgnoreCase("_AT"))
							mbrScriptBuilder.Append("A#" & actName & ":F#" & flowName & ":U1#" & ud1Name & ":U2#" & ud2Name & ":U3#" & ud3Name & ":U6#" & ud6Name & ",")	
						End If
					End If 'rPSourceCell.CellStatus.IsNoData() Then
				Next									
			End If 'startingBuffer
			
			If mbrScriptBuilder.Length = 0
				mbrScriptBuilder.Append("F#No_RPs_Meet_Criteria")
			End If
			
			Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, mbrScriptBuilder.ToString, Nothing)
			
			Return New MemberList(listHeader, listInfos)
			
		#End Region
			
		#Region "AboveGuidance ModDataRows"
		'******This function is for Above Guidance Modifications
	
		'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetUSCG_DHS_ABVModDataRows])
		Else If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetUSCG_DHS_ABVModDataRows") Then
			
			'Get time variable
			Dim wfTime As String = api.Workflow.GetWorkflowUnitInfo.TimeName		
			Dim wfYY As String = wfTime.Substring(2,2)
			
			Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
			Dim mbrScriptBuilder As New Text.StringBuilder
					
			'The purpose of this buffer is to filter through the RP members for the current WFYear and only show RPs where Funding exists
			Dim startingBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveZeros(T#" & wfTime & ":O#Top:I#Top:A#Funding:U4#Total_ATU:U5#Total_CostLine:U6#Top_UD6_LineItem:U7#None:U8#None), F#USCG_ABV_FY" & wfYY & "_Mods.Base, U1#Total_Appropriations.Base, U2#Total_Investment.Base, U3#Total_ObjectClass.Base)")	
	'		startingBuffer.LogDataBuffer(api, " Cost Estimate DB", 1000)						
			If Not startingBuffer Is Nothing Then	
				
				For Each startingCell As DataBufferCell In startingBuffer.DataBufferCells.Values									
					If Not startingCell.CellStatus.IsNoData() Then
						Dim flowName As String = startingCell.GetFlowName(api)
						Dim ud1Name As String = startingCell.GetUD1Name(api)
						Dim ud2Name As String = startingCell.GetUD2Name(api)
						Dim ud3Name As String = startingCell.GetUD3Name(api)
						'If the flowName (RP Name) doesn't contain _Ann or _Term or _AT because we don't want Ann/Terms in the data we are importing into DHS because it is already created in DHS
						If (Not flowName.XFContainsIgnoreCase("_Ann") Or flowName.XFContainsIgnoreCase("_Term") Or flowName.XFContainsIgnoreCase("_AT"))
							mbrScriptBuilder.Append("F#" & flowName & ":U1#" & ud1Name & ":U2#" & ud2Name & ":U3#" & ud3Name & ",")	
						End If
					End If 'rPSourceCell.CellStatus.IsNoData() Then
				Next									
			End If 'startingBuffer
			
			If mbrScriptBuilder.Length = 0
				mbrScriptBuilder.Append("F#No_RPs_Meet_Criteria")
			End If
			
			Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, mbrScriptBuilder.ToString, Nothing)
			
			Return New MemberList(listHeader, listInfos)
			
			#End Region
			
		#Region "Above Guidance ModDataRows_BilletDollars"
		'******This function is for Above Guidance Modifications For Billet Dollar Rows
	
		'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetUSCG_DHS_ABVModDataRows_BilletDollars])
		Else If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetUSCG_DHS_ABVModDataRows_BilletDollars") Then
			
			'Get time variable
			Dim wfTime As String = api.Workflow.GetWorkflowUnitInfo.TimeName		
			Dim wfYY As String = wfTime.Substring(2,2)
			
			Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
			Dim mbrScriptBuilder As New Text.StringBuilder
			
			'Declare the member script filters
			Dim ud6Name As String = "Total_Billet_Line_Items"
			Dim actName As String = String.Empty
			Dim flowName As String = String.Empty
			Dim ud1Name As String = String.Empty
			Dim ud2Name As String = String.Empty
			Dim ud3Name As String = String.Empty
			
			'The purpose of this buffer is to filter through the RP members for the current WFYear and only show RPs where Funding exists
			Dim startingBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveZeros(T#" & wfTime & ":O#Top:I#Top:U4#Total_ATU:U5#Total_CostLine:U6#" & ud6Name & ":U7#None:U8#None), A#Funding_Recurring_Input, A#Funding_NonRecurring_Input, F#USCG_ABV_FY" & wfYY & "_Mods.Base, U1#Total_Appropriations.Base, U2#Total_Investment.Base, U3#Total_ObjectClass.Base)")	
			'startingBuffer.LogDataBuffer(api, " Billets DB", 1000)						
			If Not startingBuffer Is Nothing Then						
				For Each startingCell As DataBufferCell In startingBuffer.DataBufferCells.Values									
					If Not startingCell.CellStatus.IsNoData() Then
						actName = startingCell.GetAccountName(api)
						flowName = startingCell.GetFlowName(api)
						ud1Name = startingCell.GetUD1Name(api)
						ud2Name = startingCell.GetUD2Name(api)
						ud3Name = startingCell.GetUD3Name(api)
						'If the flowName (RP Name) doesn't contain _Ann or _Term or _AT because we don't want Ann/Terms in the data we are importing into DHS because it is already created in DHS
						If (Not flowName.XFContainsIgnoreCase("_Ann") Or flowName.XFContainsIgnoreCase("_Term") Or flowName.XFContainsIgnoreCase("_AT"))
							mbrScriptBuilder.Append("A#" & actName & ":F#" & flowName & ":U1#" & ud1Name & ":U2#" & ud2Name & ":U3#" & ud3Name & ":U6#" & ud6Name & ",")
						End If
					End If 'rPSourceCell.CellStatus.IsNoData() Then
				Next									
			End If 'startingBuffer
			
			If mbrScriptBuilder.Length = 0
				mbrScriptBuilder.Append("F#No_RPs_Meet_Criteria")
			End If
			
			Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, mbrScriptBuilder.ToString, Nothing)
			
			Return New MemberList(listHeader, listInfos)
			
		#End Region
			
		#Region "Above Guidance ModDataRows_GenDetailDollars"
		'******This function is for Above Guidance Modifications For GenDetail Dollar Rows
	
		'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetUSCG_DHS_ABVModDataRows_GenDetailDollars])
		Else If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetUSCG_DHS_ABVModDataRows_GenDetailDollars") Then
			
			'Get time variable
			Dim wfTime As String = api.Workflow.GetWorkflowUnitInfo.TimeName		
			Dim wfYY As String = wfTime.Substring(2,2)
			
			Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
			Dim mbrScriptBuilder As New Text.StringBuilder
					
			'Declare the member script filters
			Dim ud6Name As String = "Total_GenDetail_Line_Items"
			Dim actName As String = String.Empty
			Dim flowName As String = String.Empty
			Dim ud1Name As String = String.Empty
			Dim ud2Name As String = String.Empty
			Dim ud3Name As String = String.Empty
			
			'The purpose of this buffer is to filter through the RP members for the current WFYear and only show RPs where Funding exists
			Dim startingBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveZeros(T#" & wfTime & ":O#Top:I#Top:U4#Total_ATU:U5#Total_CostLine:U6#" & ud6Name & ":U7#None:U8#None), A#Funding_Recurring_Input, A#Funding_NonRecurring_Input, F#USCG_ABV_FY" & wfYY & "_Mods.Base, U1#Total_Appropriations.Base, U2#Total_Investment.Base, U3#Total_ObjectClass.Base)")	
	'		startingBuffer.LogDataBuffer(api, " Cost Estimate DB", 1000)						
			If Not startingBuffer Is Nothing Then	
				
				For Each startingCell As DataBufferCell In startingBuffer.DataBufferCells.Values									
					If Not startingCell.CellStatus.IsNoData() Then
						actName = startingCell.GetAccountName(api)
						flowName = startingCell.GetFlowName(api)
						ud1Name = startingCell.GetUD1Name(api)
						ud2Name = startingCell.GetUD2Name(api)
						ud3Name = startingCell.GetUD3Name(api)
						'If the flowName (RP Name) doesn't contain _Ann or _Term or _AT because we don't want Ann/Terms in the data we are importing into DHS because it is already created in DHS
						If (Not flowName.XFContainsIgnoreCase("_Ann") Or flowName.XFContainsIgnoreCase("_Term") Or flowName.XFContainsIgnoreCase("_AT"))
							mbrScriptBuilder.Append("A#" & actName & ":F#" & flowName & ":U1#" & ud1Name & ":U2#" & ud2Name & ":U3#" & ud3Name & ":U6#" & ud6Name & ",")	
						End If
					End If 'rPSourceCell.CellStatus.IsNoData() Then
				Next									
			End If 'startingBuffer
			
			If mbrScriptBuilder.Length = 0
				mbrScriptBuilder.Append("F#No_RPs_Meet_Criteria")
			End If
			
			Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, mbrScriptBuilder.ToString, Nothing)
			
			Return New MemberList(listHeader, listInfos)
			
		#End Region
			
		#Region "Above Guidance ModDataRows_CstEstDollars"
		'******This function is for Above Guidance Modifications For CstEstDollars Dollar Rows
	
		'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetUSCG_DHS_ABVModDataRows_CstEstDollars])
		Else If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetUSCG_DHS_ABVModDataRows_CstEstDollars") Then
			
			'Get time variable
			Dim wfTime As String = api.Workflow.GetWorkflowUnitInfo.TimeName		
			Dim wfYY As String = wfTime.Substring(2,2)
			
			Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
			Dim mbrScriptBuilder As New Text.StringBuilder
					
			'Declare the member script filters
			Dim ud6Name As String = String.Empty
			Dim actName As String = String.Empty
			Dim flowName As String = String.Empty
			Dim ud1Name As String = String.Empty
			Dim ud2Name As String = String.Empty
			Dim ud3Name As String = String.Empty
			
			'The purpose of this buffer is to filter through the RP members for the current WFYear and only show RPs where Funding exists
			Dim startingBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveZeros(T#" & wfTime & ":O#Top:I#Top:U4#Total_ATU:U5#Total_CostLine:U7#None:U8#None), A#Funding_Recurring_Input, A#Funding_NonRecurring_Input, F#USCG_ABV_FY" & wfYY & "_Mods.Base, U1#Total_Appropriations.Base, U2#Total_Investment.Base, U3#Total_ObjectClass.Base, U6#Total_NonBillet_LineItems.Base)")	
	'		startingBuffer.LogDataBuffer(api, " Cost Estimate DB", 1000)						
			If Not startingBuffer Is Nothing Then	
				
				For Each startingCell As DataBufferCell In startingBuffer.DataBufferCells.Values									
					If Not startingCell.CellStatus.IsNoData() Then
						actName = startingCell.GetAccountName(api)
						flowName = startingCell.GetFlowName(api)
						ud1Name = startingCell.GetUD1Name(api)
						ud2Name = startingCell.GetUD2Name(api)
						ud3Name = startingCell.GetUD3Name(api)
						ud6Name = startingCell.GetUD6Name(api)
						'If the flowName (RP Name) doesn't contain _Ann or _Term or _AT because we don't want Ann/Terms in the data we are importing into DHS because it is already created in DHS
						If (Not flowName.XFContainsIgnoreCase("_Ann") Or flowName.XFContainsIgnoreCase("_Term") Or flowName.XFContainsIgnoreCase("_AT"))
							mbrScriptBuilder.Append("A#" & actName & ":F#" & flowName & ":U1#" & ud1Name & ":U2#" & ud2Name & ":U3#" & ud3Name & ":U6#" & ud6Name & ",")	
						End If
					End If 'rPSourceCell.CellStatus.IsNoData() Then
				Next									
			End If 'startingBuffer
			
			If mbrScriptBuilder.Length = 0
				mbrScriptBuilder.Append("F#No_RPs_Meet_Criteria")
			End If
			
			Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, mbrScriptBuilder.ToString, Nothing)
			
			Return New MemberList(listHeader, listInfos)
			
		#End Region
			
		#Region "Above Guidance ModDataRows_ExpDollars"
		'******This function is for Above Guidance Modifications For CstEstDollars Dollar Rows
	
		'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetUSCG_DHS_ABVModDataRows_ExpDollars])
		Else If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetUSCG_DHS_ABVModDataRows_ExpDollars") Then
			
			'Get time variable
			Dim wfTime As String = api.Workflow.GetWorkflowUnitInfo.TimeName		
			Dim wfYY As String = wfTime.Substring(2,2)
			
			Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
			Dim mbrScriptBuilder As New Text.StringBuilder
					
			'Declare the member script filters
			Dim ud6Name As String = String.Empty
			Dim actName As String = String.Empty
			Dim flowName As String = String.Empty
			Dim ud1Name As String = String.Empty
			Dim ud2Name As String = String.Empty
			Dim ud3Name As String = String.Empty
			
			'The purpose of this buffer is to filter through the RP members for the current WFYear and only show RPs where Funding exists
			Dim startingBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveZeros(T#" & wfTime & ":O#Top:I#Top:U4#Total_ATU:U5#Total_CostLine:U7#None:U8#None), A#Funding_Recurring_Input, A#Funding_NonRecurring_Input, F#USCG_ABV_FY" & wfYY & "_Mods.Base, U1#Total_Appropriations.Base, U2#Total_Investment.Base, U3#Total_ObjectClass.Base, U6#Total_Expense_LineItems.Base)")	
	'		startingBuffer.LogDataBuffer(api, " Cost Estimate DB", 1000)						
			If Not startingBuffer Is Nothing Then	
				
				For Each startingCell As DataBufferCell In startingBuffer.DataBufferCells.Values									
					If Not startingCell.CellStatus.IsNoData() Then
						actName = startingCell.GetAccountName(api)
						flowName = startingCell.GetFlowName(api)
						ud1Name = startingCell.GetUD1Name(api)
						ud2Name = startingCell.GetUD2Name(api)
						ud3Name = startingCell.GetUD3Name(api)
						ud6Name = startingCell.GetUD6Name(api)
						'If the flowName (RP Name) doesn't contain _Ann or _Term or _AT because we don't want Ann/Terms in the data we are importing into DHS because it is already created in DHS
						If (Not flowName.XFContainsIgnoreCase("_Ann") Or flowName.XFContainsIgnoreCase("_Term") Or flowName.XFContainsIgnoreCase("_AT"))
							mbrScriptBuilder.Append("A#" & actName & ":F#" & flowName & ":U1#" & ud1Name & ":U2#" & ud2Name & ":U3#" & ud3Name & ":U6#" & ud6Name & ",")	
						End If
					End If 'rPSourceCell.CellStatus.IsNoData() Then
				Next									
			End If 'startingBuffer
			
			If mbrScriptBuilder.Length = 0
				mbrScriptBuilder.Append("F#No_RPs_Meet_Criteria")
			End If
			
			Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, mbrScriptBuilder.ToString, Nothing)
			
			Return New MemberList(listHeader, listInfos)
			
		#End Region
			
		#Region "Above Guidance ModDataRows_ZeroBaseDollars"
		'******This function is for above guidance Modifications For ZeroBaseDollars Dollar Rows
	
		'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetUSCG_DHS_ABVModDataRows_ZeroBaseDollars])
		Else If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetUSCG_DHS_ABVModDataRows_ZeroBaseDollars") Then
			
			'Get time variable
			Dim wfTime As String = api.Workflow.GetWorkflowUnitInfo.TimeName
			Dim wfTimeNext1 As String = wfTime+1		
			Dim wfTimeNext2 As String = wfTime+2	
			Dim wfTimeNext3 As String = wfTime+3	
			Dim wfTimeNext4 As String = wfTime+4		
			Dim wfYY As String = wfTime.Substring(2,2)
			
			Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
			Dim mbrScriptBuilder As New Text.StringBuilder
					
			'Declare the member script filters
			Dim ud6Name As String = String.Empty
			Dim actName As String = String.Empty
			Dim flowName As String = String.Empty
			Dim ud1Name As String = String.Empty
			Dim ud2Name As String = String.Empty
			Dim ud3Name As String = String.Empty
			
			'The purpose of this buffer is to filter through the RP members for the current WFYear and only show RPs where Funding exists
			Dim wfTimeBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveZeros(T#" & wfTime & ":O#Top:I#Top:U4#Total_ATU:U5#Total_CostLine:U7#None:U8#None), A#ZeroBase_Funding, F#USCG_ABV_FY" & wfYY & "_Mods.Base, U1#Total_Appropriations.Base, U2#Total_Investment.Base, U3#Total_ObjectClass.Base, U6#Total_Expense_LineItems.Base)")	
			Dim wfTimeNext1Buffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveZeros(T#" & wfTimeNext1 & ":O#Top:I#Top:U4#Total_ATU:U5#Total_CostLine:U7#None:U8#None), A#ZeroBase_Funding, F#USCG_ABV_FY" & wfYY & "_Mods.Base, U1#Total_Appropriations.Base, U2#Total_Investment.Base, U3#Total_ObjectClass.Base, U6#Total_Expense_LineItems.Base)")	
			Dim wfTimeNext2Buffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveZeros(T#" & wfTimeNext2 & ":O#Top:I#Top:U4#Total_ATU:U5#Total_CostLine:U7#None:U8#None), A#ZeroBase_Funding, F#USCG_ABV_FY" & wfYY & "_Mods.Base, U1#Total_Appropriations.Base, U2#Total_Investment.Base, U3#Total_ObjectClass.Base, U6#Total_Expense_LineItems.Base)")	
			Dim wfTimeNext3Buffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveZeros(T#" & wfTimeNext3 & ":O#Top:I#Top:U4#Total_ATU:U5#Total_CostLine:U7#None:U8#None), A#ZeroBase_Funding, F#USCG_ABV_FY" & wfYY & "_Mods.Base, U1#Total_Appropriations.Base, U2#Total_Investment.Base, U3#Total_ObjectClass.Base, U6#Total_Expense_LineItems.Base)")	
			Dim wfTimeNext4Buffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveZeros(T#" & wfTimeNext4 & ":O#Top:I#Top:U4#Total_ATU:U5#Total_CostLine:U7#None:U8#None), A#ZeroBase_Funding, F#USCG_ABV_FY" & wfYY & "_Mods.Base, U1#Total_Appropriations.Base, U2#Total_Investment.Base, U3#Total_ObjectClass.Base, U6#Total_Expense_LineItems.Base)")	
			Dim superBuffer As DataBuffer = wfTimeBuffer + wfTimeNext1Buffer + wfTimeNext2Buffer + wfTimeNext3Buffer + wfTimeNext4Buffer
	'		startingBuffer.LogDataBuffer(api, " Cost Estimate DB", 1000)						
			If Not superBuffer Is Nothing Then	
				
				For Each startingCell As DataBufferCell In superBuffer.DataBufferCells.Values									
					If Not startingCell.CellStatus.IsNoData() Then
						actName = startingCell.GetAccountName(api)
						flowName = startingCell.GetFlowName(api)
						ud1Name = startingCell.GetUD1Name(api)
						ud2Name = startingCell.GetUD2Name(api)
						ud3Name = startingCell.GetUD3Name(api)
						ud6Name = startingCell.GetUD6Name(api)
						'If the flowName (RP Name) doesn't contain _Ann or _Term or _AT because we don't want Ann/Terms in the data we are importing into DHS because it is already created in DHS
						If (Not flowName.XFContainsIgnoreCase("_Ann") Or flowName.XFContainsIgnoreCase("_Term") Or flowName.XFContainsIgnoreCase("_AT"))
							mbrScriptBuilder.Append("A#" & actName & ":F#" & flowName & ":U1#" & ud1Name & ":U2#" & ud2Name & ":U3#" & ud3Name & ":U6#" & ud6Name & ",")	
						End If
					End If 'rPSourceCell.CellStatus.IsNoData() Then
				Next									
			End If 'startingBuffer
			
			If mbrScriptBuilder.Length = 0
				mbrScriptBuilder.Append("F#No_RPs_Meet_Criteria")
			End If
			
			Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, mbrScriptBuilder.ToString, Nothing)
			
			Return New MemberList(listHeader, listInfos)
			
		#End Region
			
		#Region "Above Guidance ModDataPositionRows"
		'******This function is for Above Guidance Modifications for Positions
	
		'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetUSCG_DHS_ABVModDataRows_Positions])
		Else If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetUSCG_DHS_ABVModDataRows_Positions") Then
			
			'Get time variable
			Dim wfTime As String = api.Workflow.GetWorkflowUnitInfo.TimeName		
			Dim wfYY As String = wfTime.Substring(2,2)
			
			Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
			Dim mbrScriptBuilder As New Text.StringBuilder
					
			'The purpose of this buffer is to filter through the RP members for the current WFYear and only show RPs where Funding exists and then use this to generaate the dynamic calc for positions
			Dim startingBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveZeros(T#" & wfTime & ":O#Top:I#Top:U4#Total_ATU:U5#Total_CostLine:U7#None:U8#None), A#Funding_Recurring_Input, F#USCG_ABV_FY" & wfYY & "_Mods.Base, U1#Total_Appropriations.Base, U2#Total_Investment.Base, U3#11_1,U3#11_3,U3#11_7, U6#Total_Billet_Line_Items.Base, U6#Total_GenDetail_Line_Items.Base)")	
			'startingBuffer.LogDataBuffer(api, " Positions DB", 1000)						
			If Not startingBuffer Is Nothing Then	
				
				For Each startingCell As DataBufferCell In startingBuffer.DataBufferCells.Values									
					If Not startingCell.CellStatus.IsNoData() Then
						Dim actName As String = startingCell.GetAccountName(api)
						Dim flowName As String = startingCell.GetFlowName(api)
						Dim ud1Name As String = startingCell.GetUD1Name(api)
						Dim ud2Name As String = startingCell.GetUD2Name(api)
						Dim ud3Name As String = startingCell.GetUD3Name(api)
						Dim ud6Name As String = startingCell.GetUD6Name(api)
						'If the flowName (RP Name) doesn't contain _Ann or _Term or _AT because we don't want Ann/Terms in the data we are importing into DHS because it is already created in DHS
						If (Not flowName.XFContainsIgnoreCase("_Ann") Or flowName.XFContainsIgnoreCase("_Term") Or flowName.XFContainsIgnoreCase("_AT"))
							mbrScriptBuilder.Append("A#" & actName & ":F#" & flowName & ":U1#" & ud1Name & ":U2#" & ud2Name & ":U3#" & ud3Name & ":U6#" & ud6Name & ",")	
						End If
					End If 'rPSourceCell.CellStatus.IsNoData() Then
				Next									
			End If 'startingBuffer
			
			If mbrScriptBuilder.Length = 0
				mbrScriptBuilder.Append("F#No_RPs_Meet_Criteria")
			End If
			
			Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, mbrScriptBuilder.ToString, Nothing)
			
			Return New MemberList(listHeader, listInfos)
			
		#End Region
									
	#End Region
		
	#Region "ModInfoRows"	
	'*******This second set of functions will retrieve the Mod Info (annotation) rows
	
		#Region "Standard ModInfoRows"
		'******This function is for Standard Modifications
	
		'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetUSCG_DHS_ModInfoRows])
		Else If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetUSCG_DHS_ModInfoRows") Then
			
			'Get time variable
			Dim wfTime As String = api.Workflow.GetWorkflowUnitInfo.TimeName		
			Dim wfYY As String = wfTime.Substring(2,2)
			
			Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
			Dim mbrScriptBuilder As New Text.StringBuilder
			
			'The purpose of this buffer is to filter through the RP members for the current WFYear and only show RPs where Funding exists
			Dim startingBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveZeros(T#" & wfTime & ":O#Top:I#Top:A#Funding:U1#Total_Appropriations:U2#Total_Investment:U3#Total_ObjectClass:U4#Total_ATU:U5#Total_CostLine:U6#Top_UD6_LineItem:U7#None:U8#None), F#USCG_FY" & wfYY & "_Mods.Base)")	
	'		startingBuffer.LogDataBuffer(api, " Cost Estimate DB", 1000)						
			If Not startingBuffer Is Nothing Then	
				
				For Each startingCell As DataBufferCell In startingBuffer.DataBufferCells.Values									
					If Not startingCell.CellStatus.IsNoData() Then
						Dim flowName As String = startingCell.GetFlowName(api)
						'If the flowName (RP Name) doesn't contain _Ann or _Term or _AT because we don't want Ann/Terms in the data we are importing into DHS because it is already created in DHS
						If (Not (flowName.XFContainsIgnoreCase("_Ann") Or flowName.XFContainsIgnoreCase("_Term") Or flowName.XFContainsIgnoreCase("_AT")))
							mbrScriptBuilder.Append("F#" & flowName & ",")	
						End If
					End If 'rPSourceCell.CellStatus.IsNoData() Then
				Next									
			End If 'startingBuffer
				
			If mbrScriptBuilder.Length = 0
				mbrScriptBuilder.Append("F#No_RPs_Meet_Criteria")
			End If
			
			Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, mbrScriptBuilder.ToString, Nothing)
			
			Return New MemberList(listHeader, listInfos)
			
		#End Region
		
		#Region "AboveGuidance ModInfoRows"
		'******This function is for Above Guidance Modifications
	
		'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetUSCG_DHS_ABVModInfoRows])
		Else If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetUSCG_DHS_ABVModInfoRows") Then
			
			'Get time variable
			Dim wfTime As String = api.Workflow.GetWorkflowUnitInfo.TimeName		
			Dim wfYY As String = wfTime.Substring(2,2)
			
			Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
			Dim mbrScriptBuilder As New Text.StringBuilder
			
			'The purpose of this buffer is to filter through the RP members for the current WFYear and only show RPs where Funding exists
			Dim startingBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveZeros(T#" & wfTime & ":O#Top:I#Top:A#Funding:U1#Total_Appropriations:U2#Total_Investment:U3#Total_ObjectClass:U4#Total_ATU:U5#Total_CostLine:U6#Top_UD6_LineItem:U7#None:U8#None), F#USCG_ABV_FY" & wfYY & "_Mods.Base)")	
	'		startingBuffer.LogDataBuffer(api, " Cost Estimate DB", 1000)						
			If Not startingBuffer Is Nothing Then	
				
				For Each startingCell As DataBufferCell In startingBuffer.DataBufferCells.Values									
					If Not startingCell.CellStatus.IsNoData() Then
						Dim flowName As String = startingCell.GetFlowName(api)
						'If the flowName (RP Name) doesn't contain _Ann or _Term or _AT because we don't want Ann/Terms in the data we are importing into DHS because it is already created in DHS
						If (Not (flowName.XFContainsIgnoreCase("_Ann") Or flowName.XFContainsIgnoreCase("_Term") Or flowName.XFContainsIgnoreCase("_AT")))
							mbrScriptBuilder.Append("F#" & flowName & ",")	
						End If
					End If 'rPSourceCell.CellStatus.IsNoData() Then
				Next									
			End If 'startingBuffer
			
			If mbrScriptBuilder.Length = 0
				mbrScriptBuilder.Append("F#No_RPs_Meet_Criteria")
			End If
			
			Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, mbrScriptBuilder.ToString, Nothing)
			
			Return New MemberList(listHeader, listInfos)
			
		#End Region
		
	End If
	
	#End Region
	
#End Region

#Region "GetModTreeItems"

	#Region "GetModTreeItems_Standard"

		'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetModTreeItems])
		If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetModTreeItems") Then
			
			'Get Scenario and Time variables
			Dim wfScenario As String = api.Pov.Scenario.Name
			Dim wfTime As String = api.Pov.Time.Name		
			Dim wfYY As String = wfTime.Substring(2,2)	
			
			Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
			Dim mbrScriptBuilder As New Text.StringBuilder
			
			'Get the parent Flow member on which to base the list of descendants					
			Dim std_FlowDimPk As DimPk = api.Dimensions.GetDim("Std_Flow").DimPk
			Dim total_OSId As Integer = api.Members.GetMemberId(dimtypeId.Flow, "USCG_OS_" & wfYY)
			
			'Define the list of members to loop through
			Dim totalOSDescendantMems As List(Of Member) = api.Members.GetDescendants(std_FlowDimPk, total_OSId, Nothing)
			
			'Define the previous Flow member name to be defined after the first loop
			Dim flowMemPrevName As String = String.Empty
			
			'Loop throught the list of Flow members
			If totalOSDescendantMems IsNot Nothing Then
				For Each flowMem As Member In totalOSDescendantMems

					'Get the Flow member, member name, properties, and Text8 value
					Dim flowMemName As String = flowMem.Name
					Dim flowMemInfo As MemberInfo = BRApi.Finance.Members.GetMemberInfo(si, dimTypeId.Flow, flowMemName, True)
					Dim flowMemProps As FlowVMProperties = flowMemInfo.GetFlowProperties()
					Dim flowMemText8 As String = flowMemProps.Text8.GetStoredValue(DimConstants.Unknown, DimConstants.Unknown)
					
					'If Text8 = "Mod" and the previous member does not contain "Tier", insert a blank row before the current member, else return members based on member name
					If flowMemName = ("USCG_OS_" & wfYY) Then
						Continue For
					Else If flowMemText8.XFEqualsIgnoreCase("Mod") And Not flowMemPrevName.Contains ("Tier") Then
						mbrScriptBuilder.Append("F#None:Name( ),")					
						mbrScriptBuilder.Append("F#" & flowMemName & ",") 
'						Else If flowMemName = ("USCG_OS_" & wfYY) Then
'						Continue For
					Else If flowMemName.XFEqualsIgnoreCase("USCG_TXF_" & wfYY) Then
						mbrScriptBuilder.Append("F#" & flowMemName & ",")
					Else If flowMemName.XFEqualsIgnoreCase("USCG_PRI_" & wfYY) Then
						mbrScriptBuilder.Append("F#" & flowMemName & ",")
					Else If flowMemName.XFEqualsIgnoreCase("USCG_PRI_Incr_" & wfYY) Then
						mbrScriptBuilder.Append("F#" & flowMemName & ",")	
					Else If flowMemName.XFEqualsIgnoreCase("USCG_PRI_Decr_" & wfYY) Then
						mbrScriptBuilder.Append("F#USCG_PRI_Incr_" & wfYY & ":Name(Total Increases),")
						mbrScriptBuilder.Append("F#" & flowMemName & ",")
					Else If flowMemName.XFEqualsIgnoreCase("USCG_PGM_" & wfYY) Then
						mbrScriptBuilder.Append("F#USCG_PRI_Decr_" & wfYY & ":Name(Total Decreases),")
						mbrScriptBuilder.Append("F#USCG_PRI_" & wfYY & ":Name(Total Adjustments-to-Base),")
						mbrScriptBuilder.Append("F#None:Name(  ),")
						mbrScriptBuilder.Append("F#" & flowMemName & ",")
					Else If flowMemName.XFEqualsIgnoreCase("USCG_PGM_Incr_" & wfYY) Then
						mbrScriptBuilder.Append("F#" & flowMemName & ",")
					Else If flowMemName.XFEqualsIgnoreCase("USCG_PGM_Decr_" & wfYY) Then
						mbrScriptBuilder.Append("F#USCG_PGM_Incr_" & wfYY & ":Name(Total Increases),")
						mbrScriptBuilder.Append("F#" & flowMemName & ",")
'					Else If flowMemName = ("USCG_PCI_" & wfYY) Then
'						mbrScriptBuilder.Append("F#USCG_PGM_Decr_" & wfYY & ":Name(Total Decreases),")
'						mbrScriptBuilder.Append("F#USCG_PGM_" & wfYY & ":Name(Total Program Changes),")
'						mbrScriptBuilder.Append("F#None:Name(  ),")
'						mbrScriptBuilder.Append("F#USCG_FY" & wfYY & "_Mods:Name(FY 20" & wfYY & " Operations & Support Request),")
'						mbrScriptBuilder.Append("F#None:Name(FY 20" & wfYY - 1 & " to FY 20" & wfYY & " Operations & Support Total Change),")
'						mbrScriptBuilder.Append("F#None:Name(  ),")
'						mbrScriptBuilder.Append("F#" & flowMemName & ":Name(FY 20" & wfYY & " PC&I),")
'					Else If flowMemName = ("USCG_RD_" & wfYY) Then
'						mbrScriptBuilder.Append("F#" & flowMemName & ":Name(FY 20" & wfYY & " R&D),")
'					Else If flowMemName = ("USCG_RP_" & wfYY) Then
'						mbrScriptBuilder.Append("F#" & flowMemName & ":Name(FY 20" & wfYY & " RP),")
'					Else If flowMemName = ("USCG_MOSP_" & wfYY) Then
'						mbrScriptBuilder.Append("F#" & flowMemName & ":Name(FY 20" & wfYY & " MOSP),")
'					Else If flowMemName = ("USCG_F_" & wfYY) Then
'						mbrScriptBuilder.Append("F#" & flowMemName & ":Name(FY 20" & wfYY & " Funds),")
'					Else If flowMemName = ("USCG_MERHCF_" & wfYY) Then
'						mbrScriptBuilder.Append("F#" & flowMemName & ":Name(FY 20" & wfYY & " MERHCFC),")
'					Else If flowMemName = ("USCG_BS_" & wfYY) Then
'						mbrScriptBuilder.Append("F#" & flowMemName & ":Name(FY 20" & wfYY & " BS),")
'						mbrScriptBuilder.Append("F#USCG_FY" & wfYY & "_Mods:S#" & wfScenario & ":Name(FY 20" & wfYY & " Total Discretionary),")
					Else
						mbrScriptBuilder.Append("F#" & flowMemName & ",")
					End If	
					
					Next
				End If
				
				mbrScriptBuilder.Append("F#USCG_PGM_Decr_" & wfYY & ":Name(Total Decreases),")
				mbrScriptBuilder.Append("F#USCG_PGM_" & wfYY & ":Name(Total Program Changes),")								
			
			'Convert the Text.StringBuilder to a String and strip off the last comma
			Dim mbrFilter As String = mbrScriptBuilder.ToString
			mbrFilter = mbrFilter.Trim().Remove(mbrFilter.Length - 1)
			
			'Define the list and return it
			Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, mbrFilter, Nothing)		 
			Return New MemberList(listHeader, listInfos)
			
		End If
		
	#End Region 'GetModTreeItems_Standard
	
	#Region "GetModTreeItems_AboveGuidance"

		'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetModTreeItemsABV])
		If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetModTreeItemsABV") Then
			
			'Get Scenario and Time variables
			Dim wfScenario As String = api.Pov.Scenario.Name
			Dim wfTime As String = api.Pov.Time.Name		
			Dim wfYY As String = wfTime.Substring(2,2)	
			
			Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
			Dim mbrScriptBuilder As New Text.StringBuilder
			
			'Get the parent Flow member on which to base the list of descendants					
			Dim std_FlowDimPk As DimPk = api.Dimensions.GetDim("Std_Flow").DimPk
			Dim total_ModsId As Integer = api.Members.GetMemberId(dimtypeId.Flow, "USCG_ABV_FY" & wfYY & "_Mods")
			
			If Not total_ModsId = -1 And Not total_ModsId.ToString = "" Then
						
				'Define the list of members to loop through
				Dim totalModDescendantMems As List(Of Member) = api.Members.GetDescendants(std_FlowDimPk, total_ModsId, Nothing)
				
				'Loop through the list of Flow members
				If totalModDescendantMems IsNot Nothing Then
					For Each flowMem As Member In totalModDescendantMems

						'Get the Flow member name
						Dim flowMemName As String = flowMem.Name
						
						If flowMemName = ("USCG_ABVOS_" & wfYY) Then
							mbrScriptBuilder.Append("F#USCG_ABV_FY" & wfYY & "_Mods:Name(USCG Above Guidance),")
							mbrScriptBuilder.Append("F#" & flowMemName & ":Name(O&S Requests),")
						Else If flowMemName = ("USCG_ABVPCI_" & wfYY) Then
							mbrScriptBuilder.Append("F#USCG_ABVOS_" & wfYY & ":Name(Total O&S Requests),")
							mbrScriptBuilder.Append("F#None:Name( ),")
							mbrScriptBuilder.Append("F#" & flowMemName & ":Name(PC&I Requests),")
						Else If flowMemName = ("USCG_ABVRD_" & wfYY) Then
							mbrScriptBuilder.Append("F#USCG_ABVPCI_" & wfYY & ":Name(Total PC&I Requests),")
							mbrScriptBuilder.Append("F#None:Name( ),")
							mbrScriptBuilder.Append("F#" & flowMemName & ":Name(R&D Requests),")
						Else If flowMemName = ("USCG_ABVRP_" & wfYY) Then
							mbrScriptBuilder.Append("F#USCG_ABVRD_" & wfYY & ":Name(Total R&D Requests),")
							mbrScriptBuilder.Append("F#None:Name( ),")
							mbrScriptBuilder.Append("F#" & flowMemName & ":Name(RP Requests),")
						Else If flowMemName = ("USCG_ABVMOSP_" & wfYY) Then
							mbrScriptBuilder.Append("F#USCG_ABVRP_" & wfYY & ":Name(Total RP Requests),")
							mbrScriptBuilder.Append("F#None:Name( ),")
							mbrScriptBuilder.Append("F#" & flowMemName & ":Name(MOSP Requests),")
						Else If flowMemName = ("USCG_ABVF_" & wfYY) Then
							mbrScriptBuilder.Append("F#USCG_ABVMOSP_" & wfYY & ":Name(Total MOSP Requests),")
							mbrScriptBuilder.Append("F#None:Name( ),")
							mbrScriptBuilder.Append("F#" & flowMemName & ":Name(F Requests),")
						Else If flowMemName = ("USCG_ABVMERHCF_" & wfYY) Then
							mbrScriptBuilder.Append("F#USCG_ABVF_" & wfYY & ":Name(Total F Requests),")
							mbrScriptBuilder.Append("F#None:Name( ),")
							mbrScriptBuilder.Append("F#" & flowMemName & ":Name(MERHCF Requests),")
						Else If flowMemName = ("USCG_ABVBS_" & wfYY) Then
							mbrScriptBuilder.Append("F#USCG_ABVMERHCF_" & wfYY & ":Name(Total MERHCF Requests),")
							mbrScriptBuilder.Append("F#None:Name( ),")
							mbrScriptBuilder.Append("F#" & flowMemName & ":Name(BS Requests),")
						Else
							mbrScriptBuilder.Append("F#" & flowMemName & ",")
						End If
						
					Next
				End If
				
			Else 
				mbrScriptBuilder.Append("F#None,")
			End If
				
			
			'Add the final rows to the Text.StringBuilder
			mbrScriptBuilder.Append("F#USCG_ABVBS_" & wfYY & ":Name(Total BS Requests),")
			mbrScriptBuilder.Append("F#None:Name( ),")
			mbrScriptBuilder.Append("F#USCG_ABV_FY" & wfYY & "_Mods:Name(Total Above Guidance Requests)")
			
			'Convert the Text.StringBuilder to a String
			Dim mbrFilter As String = mbrScriptBuilder.ToString
			
			'Define the list and return it
			Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, mbrFilter, Nothing)		 
			Return New MemberList(listHeader, listInfos)
			
		End If
		
	#End Region 'GetModTreeItems_AboveGuidance
				
#End Region

#Region "GetModTreeItemsRolled"

	#Region "GetModTreeItemsRolled_Standard"

		'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetModTreeItemsRolled])
		If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetModTreeItemsRolled") Then
			
			'Get Scenario and Time variables
			Dim wfScenario As String = api.Pov.Scenario.Name
			Dim wfTime As String = api.Pov.Time.Name		
			Dim wfYY As String = wfTime.Substring(2,2)	
			
			Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
			Dim mbrScriptBuilder As New Text.StringBuilder
			
			'Get the parent Flow member on which to base the list of descendants					
			Dim std_FlowDimPk As DimPk = api.Dimensions.GetDim("Std_Flow").DimPk
			Dim total_OSId As Integer = api.Members.GetMemberId(dimtypeId.Flow, "USCG_OS_" & wfYY)
			
			'Define the list of members to loop through
			Dim totalOSDescendantMems As List(Of Member) = api.Members.GetDescendants(std_FlowDimPk, total_OSId, Nothing)
			
			'Loop throught the list of Flow members
			If totalOSDescendantMems IsNot Nothing Then
				For Each flowMem As Member In totalOSDescendantMems

					'Get the Flow member, member name, properties, and Text8 value
					Dim flowMemName As String = flowMem.Name
					Dim flowMemInfo As MemberInfo = BRApi.Finance.Members.GetMemberInfo(si, dimTypeId.Flow, flowMemName, True)
					Dim flowMemProps As FlowVMProperties = flowMemInfo.GetFlowProperties()
					Dim flowMemText8 As String = flowMemProps.Text8.GetStoredValue(DimConstants.Unknown, DimConstants.Unknown)
					
					'If Text8 starts with "RP_", ignore and move to the next descendant, else return members based on member name
					If flowMemText8.StartsWith("RP_") Then
						Continue For
'					Else If flowMemName = ("USCG_DCR_" & wfYY) Or flowMemName = ("USCG_MND_" & wfYY) Then
'						Continue For
					Else If flowMemName.XFEqualsIgnoreCase("USCG_OS_" & wfYY) Then
						Continue For
					Else If flowMemName.XFEqualsIgnoreCase("USCG_TXF_" & wfYY) Then
						mbrScriptBuilder.Append("F#" & flowMemName & ",")
					Else If flowMemName.XFEqualsIgnoreCase("USCG_PRI_" & wfYY) Then
						mbrScriptBuilder.Append("F#" & flowMemName & ",")
					Else If flowMemName.XFEqualsIgnoreCase("USCG_PRI_Incr_" & wfYY) Then
						mbrScriptBuilder.Append("F#" & flowMemName & ",")	
					Else If flowMemName.XFEqualsIgnoreCase("USCG_PRI_Decr_" & wfYY) Then
						mbrScriptBuilder.Append("F#USCG_PRI_Incr_" & wfYY & ":Name(Total Increases),")
						mbrScriptBuilder.Append("F#" & flowMemName & ",")
					Else If flowMemName.XFEqualsIgnoreCase("USCG_PGM_" & wfYY) Then
						mbrScriptBuilder.Append("F#USCG_PRI_Decr_" & wfYY & ":Name(Total Decreases),")
						mbrScriptBuilder.Append("F#USCG_PRI_" & wfYY & ":Name(Total Adjustments-to-Base),")
						mbrScriptBuilder.Append("F#None:Name(  ),")
						mbrScriptBuilder.Append("F#" & flowMemName & ",")
					Else If flowMemName.XFEqualsIgnoreCase("USCG_PGM_Incr_" & wfYY) Then
						mbrScriptBuilder.Append("F#" & flowMemName & ",")
					Else If flowMemName.XFEqualsIgnoreCase("USCG_PGM_Decr_" & wfYY) Then
						mbrScriptBuilder.Append("F#USCG_PGM_Incr_" & wfYY & ":Name(Total Increases),")
						mbrScriptBuilder.Append("F#" & flowMemName & ",")
'					Else If flowMemName = ("USCG_PCI_" & wfYY) Then
'						mbrScriptBuilder.Append("F#USCG_PGM_Decr_" & wfYY & ":Name(Total Decreases),")
'						mbrScriptBuilder.Append("F#USCG_PGM_" & wfYY & ":Name(Total Program Changes),")
'						mbrScriptBuilder.Append("F#None:Name(  ),")
'						mbrScriptBuilder.Append("F#USCG_OS_" & wfYY & ":Name(FY 20" & wfYY & " Operations & Support Request),")
''						mbrScriptBuilder.Append("GetDataCell(F#Top_Flow:S#|WFScenario|:U1#OS-F#Top_Flow:S#|!prmPYScenario!|:U1#OS):Name(FY 20" & wfYY - 1 & " to FY 20" & wfYY & " Operations & Support Total Change),")
'						mbrScriptBuilder.Append("F#None:Name(  ),")
'						mbrScriptBuilder.Append("F#" & flowMemName & ":Name(FY 20" & wfYY & " PC&I),")
'					Else If flowMemName = ("USCG_RD_" & wfYY) Then
'						mbrScriptBuilder.Append("F#" & flowMemName & ":Name(FY 20" & wfYY & " R&D),")
'					Else If flowMemName = ("USCG_MERHCF_" & wfYY) Then
'						mbrScriptBuilder.Append("F#" & flowMemName & ":Name(FY 20" & wfYY & " MERHCFC),")
'						mbrScriptBuilder.Append("F#USCG_DCR_" & wfYY & ":Name(FY 20" & wfYY & " Total Discretionary),")
'						mbrScriptBuilder.Append("F#None:Name(  ),")
'					Else If flowMemName = ("USCG_RP_" & wfYY) Then
'						mbrScriptBuilder.Append("F#" & flowMemName & ":Name(FY 20" & wfYY & " RP),")
'					Else If flowMemName = ("USCG_MOSP_" & wfYY) Then
'						mbrScriptBuilder.Append("F#" & flowMemName & ":Name(FY 20" & wfYY & " MOSP),")
'					Else If flowMemName = ("USCG_F_" & wfYY) Then
'						mbrScriptBuilder.Append("F#" & flowMemName & ":Name(FY 20" & wfYY & " Funds),")
'					Else If flowMemName = ("USCG_BS_" & wfYY) Then
'						mbrScriptBuilder.Append("F#" & flowMemName & ":Name(FY 20" & wfYY & " BS),")
'						mbrScriptBuilder.Append("F#USCG_MND_" & wfYY & ":Name(FY 20" & wfYY & " Total Mandatory),")
					Else
						mbrScriptBuilder.Append("F#" & flowMemName & ",")
					End If
					
				Next
			End If
			
			mbrScriptBuilder.Append("F#USCG_PGM_Decr_" & wfYY & ":Name(Total Decreases),")
			mbrScriptBuilder.Append("F#USCG_PGM_" & wfYY & ":Name(Total Program Changes),")
				
			
			'Convert the Text.StringBuilder to a String and strip off the last comma
			Dim mbrFilter As String = mbrScriptBuilder.ToString
			mbrFilter = mbrFilter.Trim().Remove(mbrFilter.Length - 1)
			
			'Define the list and return it
			Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, mbrFilter, Nothing)		 
			Return New MemberList(listHeader, listInfos)
			
		End If
		
	#End Region 'GetModTreeItemsRolled_Standard
	
	#Region "GetModTreeItemsRolled_AboveGuidance"

		'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetModTreeItemsRolledABV])
		If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetModTreeItemsRolledABV") Then
			
			'Get Scenario and Time variables
			Dim wfScenario As String = api.Pov.Scenario.Name
			Dim wfTime As String = api.Pov.Time.Name		
			Dim wfYY As String = wfTime.Substring(2,2)	
			
			Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
			Dim mbrScriptBuilder As New Text.StringBuilder
			
			'Get the parent Flow member on which to base the list of descendants					
			Dim std_FlowDimPk As DimPk = api.Dimensions.GetDim("Std_Flow").DimPk
			Dim total_ModsId As Integer = api.Members.GetMemberId(dimtypeId.Flow, "USCG_ABV_FY" & wfYY & "_Mods")
			
			If Not total_ModsId = -1 And Not total_ModsId.ToString = "" Then
				
				'Define the list of members to loop through
				Dim totalModDescendantMems As List(Of Member) = api.Members.GetDescendants(std_FlowDimPk, total_ModsId, Nothing)
				
				'Loop through the list of Flow members
				If totalModDescendantMems IsNot Nothing Then
					For Each flowMem As Member In totalModDescendantMems

						'Get the Flow member name
						Dim flowMemName As String = flowMem.Name
						Dim flowMemId As Integer = flowMem.MemberId
						Dim flowText8 As String = api.Flow.Text(flowMemId,8)
						
						If flowMemName = ("USCG_ABVOS_" & wfYY) Then
							mbrScriptBuilder.Append("F#USCG_ABV_FY" & wfYY & "_Mods:Name(USCG Above Guidance),")
							mbrScriptBuilder.Append("F#" & flowMemName & ":Name(O&S Requests),")
						Else If flowMemName = ("USCG_ABVPCI_" & wfYY) Then
							mbrScriptBuilder.Append("F#USCG_ABVOS_" & wfYY & ":Name(Total O&S Requests),")
							mbrScriptBuilder.Append("F#None:Name( ),")
							mbrScriptBuilder.Append("F#" & flowMemName & ":Name(PC&I Requests),")
						Else If flowMemName = ("USCG_ABVRD_" & wfYY) Then
							mbrScriptBuilder.Append("F#USCG_ABVPCI_" & wfYY & ":Name(Total PC&I Requests),")
							mbrScriptBuilder.Append("F#None:Name( ),")
							mbrScriptBuilder.Append("F#" & flowMemName & ":Name(R&D Requests),")
						Else If flowMemName = ("USCG_ABVRP_" & wfYY) Then
							mbrScriptBuilder.Append("F#USCG_ABVRD_" & wfYY & ":Name(Total R&D Requests),")
							mbrScriptBuilder.Append("F#None:Name( ),")
							mbrScriptBuilder.Append("F#" & flowMemName & ":Name(RP Requests),")
						Else If flowMemName = ("USCG_ABVMOSP_" & wfYY) Then
							mbrScriptBuilder.Append("F#USCG_ABVRP_" & wfYY & ":Name(Total RP Requests),")
							mbrScriptBuilder.Append("F#None:Name( ),")
							mbrScriptBuilder.Append("F#" & flowMemName & ":Name(MOSP Requests),")
						Else If flowMemName = ("USCG_ABVF_" & wfYY) Then
							mbrScriptBuilder.Append("F#USCG_ABVMOSP_" & wfYY & ":Name(Total MOSP Requests),")
							mbrScriptBuilder.Append("F#None:Name( ),")
							mbrScriptBuilder.Append("F#" & flowMemName & ":Name(F Requests),")
						Else If flowMemName = ("USCG_ABVMERHCF_" & wfYY) Then
							mbrScriptBuilder.Append("F#USCG_ABVF_" & wfYY & ":Name(Total F Requests),")
							mbrScriptBuilder.Append("F#None:Name( ),")
							mbrScriptBuilder.Append("F#" & flowMemName & ":Name(MERHCF Requests),")
						Else If flowMemName = ("USCG_ABVBS_" & wfYY) Then
							mbrScriptBuilder.Append("F#USCG_ABVMERHCF_" & wfYY & ":Name(Total MERHCF Requests),")
							mbrScriptBuilder.Append("F#None:Name( ),")
							mbrScriptBuilder.Append("F#" & flowMemName & ":Name(BS Requests),")
						Else If flowText8 = "Mod" Then
							mbrScriptBuilder.Append("F#" & flowMemName & ",")
						Else
							'Do Nothing
						End If							
						
					Next
				End If
				
			Else 
				mbrScriptBuilder.Append("F#None,")
			End If
				
			
			'Add the final rows to the Text.StringBuilder
			mbrScriptBuilder.Append("F#USCG_ABVBS_" & wfYY & ":Name(Total BS Requests),")
			mbrScriptBuilder.Append("F#None:Name( ),")
			mbrScriptBuilder.Append("F#USCG_ABV_FY" & wfYY & "_Mods:Name(Total Above Guidance Requests)")
			
			'Convert the Text.StringBuilder to a String
			Dim mbrFilter As String = mbrScriptBuilder.ToString
			
			'Define the list and return it
			Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, mbrFilter, Nothing)		 
			Return New MemberList(listHeader, listInfos)
			
		End If
		
	#End Region 'GetModTreeItemsRolled_AboveGuidance
			
#End Region 
						
#Region "GetRollforwardAnnTermFilter"					
						
		'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetRollforwardAnnTermFilter], SourceScenario=|!prm_Rollforward_BaseAndAnnTerm_Source_CurrentValue_ADM!|, WFTime=|WFTime|)
		If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetRollforwardAnnTermFilter") Then
			
			'Get SourceScenario
			Dim sourceScenario As String = args.MemberListArgs.NameValuePairs.XFGetValue("SourceScenario")
			Dim wfTime As String = args.MemberListArgs.NameValuePairs.XFGetValue("WFTime")							
			
			Dim flowListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
			
			If Not sourceScenario.XFEqualsIgnoreCase("")
				Dim sourceScenarioYear As String = sourceScenario.Substring(sourceScenario.Length-2,2)
				Dim wfTimeYear As String = wfTime.Substring(wfTime.Length-2,2)
				Dim yearPrior As String = (wfTimeYear.XFConvertToInt - 1).XFToString
				
				If sourceScenarioYear = wfTimeYear
					Dim flowListInfos_AnnTerm As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "F#FY" & yearPrior & "_AnnTerm", Nothing)
					Dim flowListList_AnnTerm As New MemberList(flowListHeader, flowListInfos_AnnTerm)	
					Return flowListList_AnnTerm					
				Else 'sourceScenario year is not equal to WF time so return Ann Term data from the prior year RPs as that is the source
					Dim flowListInfos_RP As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "F#FY" & yearPrior & "_RP", Nothing)
					Dim flowListList_RP As New MemberList(flowListHeader, flowListInfos_RP)		
					Return flowListList_RP					
				End If
			Else 'sourceScenario is blank so just return a generic filter
				Dim flowListInfos_None As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "F#None", Nothing)
				Dim flowListList_None As New MemberList(flowListHeader, flowListInfos_None)								
				Return flowListList_None
			End If
			
		End If
						
#End Region
					
#Region "GetRollforwardBudYrRPFilter"					
						
		'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetRollforwardBudYrRPFilter], SourceScenario=|!prm_Rollforward_BaseAndAnnTerm_Source_CurrentValue_ADM!|, WFTime=|WFTime|)
		If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetRollforwardBudYrRPFilter") Then
			
			'Get SourceScenario
			Dim sourceScenario As String = args.MemberListArgs.NameValuePairs.XFGetValue("SourceScenario")
			Dim wfTime As String = args.MemberListArgs.NameValuePairs.XFGetValue("WFTime")							
			
			Dim flowListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
			Dim flowListInfos_None As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "F#None", Nothing)
			Dim flowListList_None As New MemberList(flowListHeader, flowListInfos_None)	
			
			If Not sourceScenario.XFEqualsIgnoreCase("")
				Dim sourceScenarioYear As String = sourceScenario.Substring(sourceScenario.Length-2,2)
				Dim wfTimeYear As String = wfTime.Substring(wfTime.Length-2,2)
				Dim yearPrior As String = (wfTimeYear.XFConvertToInt - 1).XFToString
				
				If sourceScenarioYear = wfTimeYear							
					Return flowListList_None			
				Else 'sourceScenario year is not equal to WF time so return Ann Term data from the prior year RPs as that is the source
					Dim flowListInfos_RP As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "F#FY" & yearPrior & "_RP", Nothing)
					Dim flowListList_RP As New MemberList(flowListHeader, flowListInfos_RP)		
					Return flowListList_RP					
				End If
			Else 'sourceScenario is blank so just return a generic filter						
				Return flowListList_None
			End If
			
		End If
						
#End Region
							
#Region "GetRollforwardBudYrPriorAnnTermFilter"					
						
		'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetRollforwardBudYrPriorAnnTermFilter], SourceScenario=|!prm_Rollforward_BaseAndAnnTerm_Source_CurrentValue_ADM!|, WFTime=|WFTime|)
		If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetRollforwardBudYrPriorAnnTermFilter") Then
			
			'Get SourceScenario
			Dim sourceScenario As String = args.MemberListArgs.NameValuePairs.XFGetValue("SourceScenario")
			Dim wfTime As String = args.MemberListArgs.NameValuePairs.XFGetValue("WFTime")							
			
			Dim flowListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
			Dim flowListInfos_None As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "F#None", Nothing)
			Dim flowListList_None As New MemberList(flowListHeader, flowListInfos_None)	
			
			If Not sourceScenario.XFEqualsIgnoreCase("")
				Dim sourceScenarioYear As String = sourceScenario.Substring(sourceScenario.Length-2,2)
				Dim wfTimeYear As String = wfTime.Substring(wfTime.Length-2,2)
				Dim yearPriorTwo As String = (wfTimeYear.XFConvertToInt - 2).XFToString
				
				If sourceScenarioYear = wfTimeYear							
					Return flowListList_None			
				Else 'sourceScenario year is not equal to WF time so return Ann Term data from the prior year RPs as that is the source
					Dim flowListInfos_RP As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "F#FY" & yearPriorTwo & "_AnnTerm", Nothing)
					Dim flowListList_RP As New MemberList(flowListHeader, flowListInfos_RP)		
					Return flowListList_RP					
				End If
			Else 'sourceScenario is blank so just return a generic filter						
				Return flowListList_None
			End If
			
		End If
						
#End Region

#Region "GetStatusAppropriationStatusRPs"

	'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetStatusAppropriationStatusRPs], Status=[|!prm_RPStatusSelector!|], Appropriation =[|!prm_RPAppropriationSelector!|, BUDCAT=[|!prm_RPBudCatSelector!|]
	
		If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetStatusAppropriationStatusRPs") Then
			Dim wfPk As WorkflowUnitPk = api.Workflow.GetWorkflowUnitInfo.WfUnitPk
			Dim ScenarioTypeId As Integer = api.Scenario.GetScenarioType().Id
			Dim TimeId As Integer = wfPk.TimeKey		
			
			Dim wfYearYY As String = api.Workflow.GetWorkflowUnitInfo.TimeName.Substring(2,2)
			Dim mbrFilter As String = "F#FY" & wfYearYY & "_RP.Base.Where(Name DoesNotContain _WV)"
			Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
			Dim xlistInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, mbrFilter, Nothing)
						
			Dim flowDimPk As DimPk = api.Dimensions.GetDim("Std_Flow").DimPk
			Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(flowDimPk,mbrFilter)
			Dim strMbrList As String = ""

			Dim i As Integer = 0
			For Each mbr In listInfos
				Dim rpText8 As String = api.Flow.Text(mbr.Member.MemberId, 8, DimConstants.Unknown, DimConstants.Unknown)
				Dim rpField8 As List (Of String) = Stringhelper.SplitString(rpText8, "_")
				Dim rpText1 As String = api.Flow.Text(mbr.Member.MemberId, 1, ScenarioTypeId, TimeId)
				Dim rpField1 As List (Of String) = Stringhelper.SplitString(rpText1, "|")
				
				'Dim paramAppropriation As String = args.MemberListArgs.NameValuePairs.XFGetValue("Appropriation")
				Dim paramAppropriation As String = args.MemberListArgs.NameValuePairs.XFGetValue("Appropriation")
				'BrApi.ErrorLog.LogMessage(si, paramAppropriation)
				Dim paramAppropriationList As List (Of String) = paramAppropriation.Replace(" ", "").Split(",").ToList()
															
				Dim paramBUDCAT As String = args.MemberListArgs.NameValuePairs.XFGetValue("BUDCAT")
				Dim paramStatus As String = args.MemberListArgs.NameValuePairs.XFGetValue("Status")
				
				If rpField8.count > 4 Then
					'Filter on selected Appropriation, F#, Text8, field 4
					For Each appropriation In paramAppropriationList
						If appropriation.XFEqualsIgnoreCase(rpField8(4)) Then
							'Filter on selected BUDCAT, F#, Text8, field 5	
							If paramBUDCAT.Contains(rpField8(5)) Then								
								'Filter on selected Status, F#, Text1, field 1															
								If rpField1.count > 2 Then
									If paramStatus.Contains(rpField1(0)) Then	
										strMbrList = strMbrList & " ,F#" & mbr.Member.Name	
									
									End If
								End If	
							End If
						End If
					Next
				End If
								
			i += 1
			Next
			
			If strMbrList.Length = 0 Then
				strMbrList = " "
			Else
				'Do nothing, use member list created above
			End If
			
			Dim outListInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, strMbrList, Nothing)

			Return New MemberList(listHeader, outlistInfos)

	End If
							

#End Region

#Region "GetLeadDirectorateRPs"

	'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetLeadDirectorateRPs], Directorate=[|!prm_LeadDirectorate_Reporting!|], LeadOffice=[|!prm_RPLeadOfficeSelector!|], RPName=[|!prm_RPName!|], ReportFilter=[|!prm_RPSelectionOption!|])
	
		If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetLeadDirectorateRPs") Then
			Dim wfPk As WorkflowUnitPk = api.Workflow.GetWorkflowUnitInfo.WfUnitPk
			Dim TimeId As Integer = wfPk.TimeKey
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
			
			Dim wfYearYY As String = api.Workflow.GetWorkflowUnitInfo.TimeName.Substring(2,2)
			Dim mbrFilter As String = "F#FY" & wfYearYY & "_RP.Base.Where(Name DoesNotContain _WV)"
			Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
			Dim xlistInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, mbrFilter, Nothing)

			Dim flowDimPk As DimPk = api.Dimensions.GetDim("Std_Flow").DimPk
			Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(flowDimPk,mbrFilter)
			Dim strMbrList As String = ""
			
			Dim wfCube As String = api.Cubes.GetCubeInfo.Cube.Name
			Dim wfTime As String = api.Workflow.GetWorkflowUnitInfo.TimeName
			Dim wfScenario As String = api.Workflow.GetWorkflowUnitInfo.ScenarioName
			'brapi.ErrorLog.LogMessage(si, "RP Name " & RPName)

			
			'brapi.ErrorLog.LogMessage(si, "Lead Office " & LeadOffice)
			Dim reportFilter As String = args.MemberListArgs.NameValuePairs.XFGetValue("ReportFilter")
			Dim paramDirectorate As String = args.MemberListArgs.NameValuePairs.XFGetValue("Directorate")
			Dim paramLeadOffice As String = args.MemberListArgs.NameValuePairs.XFGetValue("LeadOffice")
			Dim paramRPName As String = args.MemberListArgs.NameValuePairs.XFGetValue("RPName")
			'brapi.ErrorLog.LogMessage(si, "RP Name Param = " & paramRPName)
			
			'Directorate,LeadOffice,RPName
			If reportFilter = "RPName"
				strMbrList = "F#" & paramRPName
				
			ElseIf reportFilter = "Directorate"

				For Each mbr In listInfos
					Dim rpText8 As String = api.Flow.Text(mbr.Member.MemberId, 8, DimConstants.Unknown, DimConstants.Unknown)
					Dim rpField8 As List (Of String) = Stringhelper.SplitString(rpText8, "_")
					
							
						If rpField8.Count > 4 Then
							'Filter on selected Directorate, F#, Text 8, field 3
							'If paramDirectorate.XFEqualsIgnoreCase("RPScen_" & rpField8(3)) --> outdated now that we're just using the LeadDir member description vs the member name
							If paramDirectorate.XFEqualsIgnoreCase(rpField8(3)) Then
								'brapi.ErrorLog.LogMessage(si, "Field 3 " & Directorate.XFEqualsIgnoreCase("LO_" & rpField8(3)).ToString)
							
																				
											strMbrList = strMbrList & " ,F#" & mbr.Member.Name
											'brapi.ErrorLog.LogMessage(si, "Member List " & strMbrList.ToString)
										
							
							End If
						End If
					
					Next
						
				ElseIf reportFilter = "LeadOffice"
					
					Using dbConnApp As DBConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					
					'SQL CODE--------------------------------
						Dim leadOfficeQuery As New Text.StringBuilder
						Dim leadOfficeDT As New DataTable
						leadOfficeQuery.Append("SELECT Flow, Text ")
						leadOfficeQuery.Append("FROM dbo.DataAttachment ")
						leadOfficeQuery.Append(" WHERE Cube = '" & wfCube & "' ")
						leadOfficeQuery.Append(" AND Time = '" & wfTime & "' ")
						leadOfficeQuery.Append( " AND Flow NOT LIKE '%_WV%' " )
						leadOfficeQuery.Append(" AND Scenario = '" & wfScenario & "' ")
						leadOfficeQuery.Append(" AND Account = 'Lead_Office1' ")
						leadOfficeQuery.Append(" AND Text = '" & paramLeadOffice & "' ")
						leadOfficeQuery.Append(";")
						leadOfficeDT = BRApi.Database.ExecuteSql(dbConnApp, leadOfficeQuery.ToString, False)
					
						For Each leadOfficeRow In leadOfficeDT.Rows
							strMbrList = strMbrList & " ,F#" & leadOfficeRow("Flow")
						Next
					
					End Using
						
				End If
				
				If strMbrList.Length = 0 Then
					strMbrList = " "
				Else 
					'Do nothing, use member list created above
				End If 
			 	'brapi.ErrorLog.LogMessage(si, strMbrList & " " & reportFilter)
						
				Dim outListInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, strMbrList, Nothing)
								
				Return New MemberList(listHeader, outlistInfos)
End If
	
#End Region

#Region "GetStatusBUDCATRPs"

	'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetStatusBUDCATRPs], Status=[|!prm_RPStatusSelector!|], BUDCAT=[|!prm_RPBudCatSelector!|]
	
		If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetStatusBUDCATRPs") Then
			Dim wfPk As WorkflowUnitPk = api.Workflow.GetWorkflowUnitInfo.WfUnitPk
			Dim ScenarioTypeId As Integer = api.Scenario.GetScenarioType().Id
			Dim TimeId As Integer = wfPk.TimeKey		
			
			Dim wfYearYY As String = api.Workflow.GetWorkflowUnitInfo.TimeName.Substring(2,2)
			Dim mbrFilter As String = "F#FY" & wfYearYY & "_RP.Base.Where(Name DoesNotContain _WV)"
			Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
			Dim xlistInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, mbrFilter, Nothing)
						
			Dim flowDimPk As DimPk = api.Dimensions.GetDim("Std_Flow").DimPk
			Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(flowDimPk,mbrFilter)
			Dim strMbrList As String = ""

			Dim i As Integer = 0
			For Each mbr In listInfos
				Dim rpText8 As String = api.Flow.Text(mbr.Member.MemberId, 8, DimConstants.Unknown, DimConstants.Unknown)
				Dim rpField8 As List (Of String) = Stringhelper.SplitString(rpText8, "_")
				Dim rpText1 As String = api.Flow.Text(mbr.Member.MemberId, 1, ScenarioTypeId, TimeId)
				Dim rpField1 As List (Of String) = Stringhelper.SplitString(rpText1, "|")
															
				Dim paramBUDCAT As String = args.MemberListArgs.NameValuePairs.XFGetValue("BUDCAT")
				Dim paramStatus As String = args.MemberListArgs.NameValuePairs.XFGetValue("Status")
				
				If rpField8.count > 4 Then
					'Filter on selected BUDCAT, F#, Text8, field 5	
					If paramBUDCAT.Contains(rpField8(5)) Then								
						'Filter on selected Status, F#, Text1, field 1															
						If rpField1.count > 2 Then
							If paramStatus.Contains(rpField1(0)) Then	
								strMbrList = strMbrList & " ,F#" & mbr.Member.Name	
									
							End If
						End If	
					End If
				End If
								
			i += 1
			Next
			If strMbrList.Length = 0 Then
				strMbrList = " "
			Else
				'Do nothing, use member list created above
			End If
			
			Dim outListInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, strMbrList, Nothing)

			Return New MemberList(listHeader, outlistInfos)

	End If
							

#End Region
				
#Region "GetInUseMbrs"
		'Note: this function is currently limited to work on UD1 through UD8 dimensions only
		
		'U8#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetInUseMbrs], UDdim=[UD8], ParentMbr=[Total_Computer], ExpansionLvl=[Base], InUseValue=[True])
		If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetInUseMbrs") Then

			Dim UDdim As String = args.MemberListArgs.NameValuePairs.XFGetValue("UDdim")
			Dim strUDshort As String = UDdim.Substring(2)
			Dim ParentMbr As String = args.MemberListArgs.NameValuePairs.XFGetValue("ParentMbr")
			Dim ExpansionLvl As String = args.MemberListArgs.NameValuePairs.XFGetValue("ExpansionLvl")
			
			'InUseValue is optional and will default to True (meaning, we desire members with InUse property value = True)
			Dim InUseValue As Boolean = True
			InUseValue = args.MemberListArgs.NameValuePairs.XFGetValue("InUseValue")
			Dim bInUse As Boolean = False

			'BRApi.ErrorLog.LogMessage(si, "GetInUseMbrs parameters:--UDdim=" + UDdim + "--strUDshort=" + strUDshort + "--parentmbr=" + ParentMbr + "--InUseValue=" + InUseValue.ToString)
			
			Dim varyByScenarioId As Integer = api.Pov.Scenario.MemberId
			Dim varyByScenarioTypeId As Integer = api.Scenario.GetScenarioType(varyByScenarioId).Id
			Dim varyByScenarioTypeName As String = api.Scenario.GetScenarioType(varyByScenarioId).Name
			
			Dim wfClusterPk As WorkflowUnitClusterPk = api.Workflow.WFUnitClusterPk
			Dim varyByTimeId As Integer = wfClusterPk.TimeKey
			Dim objScenarioType As ScenarioType = api.Workflow.GetScenarioType()

			
			'create a member list of UD dimension base members to loop through and check the InUse property value
			Dim Children As New List(Of Member)
			'Use the ExpansionLvl to determine whether to get Base or Children members of the ParentMbr
			If ExpansionLvl.XFEqualsIgnoreCase("Base")				
				If strUDshort = "1" Then
					Children = api.Members.GetBaseMembers(api.Pov.UD1Dim.DimPk,api.Members.GetMember(DimType.UD1.Id,ParentMbr).MemberId, Nothing)
				Else If strUDshort = "2"
					Children = api.Members.GetBaseMembers(api.Pov.UD2Dim.DimPk,api.Members.GetMember(DimType.UD2.Id,ParentMbr).MemberId, Nothing)
				Else If strUDshort = "3"
					Children = api.Members.GetBaseMembers(api.Pov.UD3Dim.DimPk,api.Members.GetMember(DimType.UD3.Id,ParentMbr).MemberId, Nothing)
				Else If strUDshort = "4"
					Children = api.Members.GetBaseMembers(api.Pov.UD4Dim.DimPk,api.Members.GetMember(DimType.UD4.Id,ParentMbr).MemberId, Nothing)
				Else If strUDshort = "5"
					Children = api.Members.GetBaseMembers(api.Pov.UD5Dim.DimPk,api.Members.GetMember(DimType.UD5.Id,ParentMbr).MemberId, Nothing)
				Else If strUDshort = "6"
					Children = api.Members.GetBaseMembers(api.Pov.UD6Dim.DimPk,api.Members.GetMember(DimType.UD6.Id,ParentMbr).MemberId, Nothing)
				Else If strUDshort = "7"
					Children = api.Members.GetBaseMembers(api.Pov.UD7Dim.DimPk,api.Members.GetMember(DimType.UD7.Id,ParentMbr).MemberId, Nothing)
				Else If strUDshort = "8"
					Children = api.Members.GetBaseMembers(api.Pov.UD8Dim.DimPk,api.Members.GetMember(DimType.UD8.Id,ParentMbr).MemberId, Nothing)
				End If
			Else If ExpansionLvl.XFEqualsIgnoreCase("Children")				
				If strUDshort = "1" Then
					Children = api.Members.GetChildren(api.Pov.UD1Dim.DimPk,api.Members.GetMember(DimType.UD1.Id,ParentMbr).MemberId, Nothing)
				Else If strUDshort = "2"
					Children = api.Members.GetChildren(api.Pov.UD2Dim.DimPk,api.Members.GetMember(DimType.UD2.Id,ParentMbr).MemberId, Nothing)
				Else If strUDshort = "3"
					Children = api.Members.GetChildren(api.Pov.UD3Dim.DimPk,api.Members.GetMember(DimType.UD3.Id,ParentMbr).MemberId, Nothing)
				Else If strUDshort = "4"
					Children = api.Members.GetChildren(api.Pov.UD4Dim.DimPk,api.Members.GetMember(DimType.UD4.Id,ParentMbr).MemberId, Nothing)
				Else If strUDshort = "5"
					Children = api.Members.GetChildren(api.Pov.UD5Dim.DimPk,api.Members.GetMember(DimType.UD5.Id,ParentMbr).MemberId, Nothing)
				Else If strUDshort = "6"
					Children = api.Members.GetChildren(api.Pov.UD6Dim.DimPk,api.Members.GetMember(DimType.UD6.Id,ParentMbr).MemberId, Nothing)
				Else If strUDshort = "7"
					Children = api.Members.GetChildren(api.Pov.UD7Dim.DimPk,api.Members.GetMember(DimType.UD7.Id,ParentMbr).MemberId, Nothing)
				Else If strUDshort = "8"
					Children = api.Members.GetChildren(api.Pov.UD8Dim.DimPk,api.Members.GetMember(DimType.UD8.Id,ParentMbr).MemberId, Nothing)
				End If
			End If
			
			
			Dim strInUseMbrsTrue As String = ""
			Dim strInUseMbrsFalse As String = ""
			
			'create a string list of members where the chosen UD dimension InUse property is set to True(or False)
			For Each tChild As Member In Children
				If strUDshort = "1" Then
					bInUse = api.UD1.InUse(tChild.MemberId, varyByScenarioTypeId, varyByTimeId)
					If bInUse Then
						strInUseMbrsTrue = strInUseMbrsTrue + ",U1#" + tChild.Name
					Else
						strInUseMbrsFalse = strInUseMbrsFalse + ",U1#" + tChild.Name
					End If
				Else If strUDshort = "2"
					bInUse = api.UD2.InUse(tChild.MemberId, varyByScenarioTypeId, varyByTimeId)
					If bInUse Then
						strInUseMbrsTrue = strInUseMbrsTrue + ",U2#" + tChild.Name
					Else
						strInUseMbrsFalse = strInUseMbrsFalse + ",U2#" + tChild.Name
					End If
				Else If strUDshort = "3"
					bInUse = api.UD3.InUse(tChild.MemberId, varyByScenarioTypeId, varyByTimeId)
					If bInUse Then
						strInUseMbrsTrue = strInUseMbrsTrue + ",U3#" + tChild.Name
					Else
						strInUseMbrsFalse = strInUseMbrsFalse + ",U3#" + tChild.Name
					End If
				Else If strUDshort = "4"
					bInUse = api.UD4.InUse(tChild.MemberId, varyByScenarioTypeId, varyByTimeId)
					If bInUse Then
						strInUseMbrsTrue = strInUseMbrsTrue + ",U4#" + tChild.Name
					Else
						strInUseMbrsFalse = strInUseMbrsFalse + ",U4#" + tChild.Name
					End If
				Else If strUDshort = "5"
					bInUse = api.UD5.InUse(tChild.MemberId, varyByScenarioTypeId, varyByTimeId)
					If bInUse Then
						strInUseMbrsTrue = strInUseMbrsTrue + ",U5#" + tChild.Name
					Else
						strInUseMbrsFalse = strInUseMbrsFalse + ",U5#" + tChild.Name
					End If
				Else If strUDshort = "6"
					bInUse = api.UD6.InUse(tChild.MemberId, varyByScenarioTypeId, varyByTimeId)
					If bInUse Then
						strInUseMbrsTrue = strInUseMbrsTrue + ",U6#" + tChild.Name
					Else
						strInUseMbrsFalse = strInUseMbrsFalse + ",U6#" + tChild.Name
					End If
				Else If strUDshort = "7"
					bInUse = api.UD7.InUse(tChild.MemberId, varyByScenarioTypeId, varyByTimeId)
					If bInUse Then
						strInUseMbrsTrue = strInUseMbrsTrue + ",U7#" + tChild.Name
					Else
						strInUseMbrsFalse = strInUseMbrsFalse + ",U7#" + tChild.Name
					End If
				Else If strUDshort = "8"
					bInUse = api.UD8.InUse(tChild.MemberId, varyByScenarioTypeId, varyByTimeId)
					If bInUse Then
						strInUseMbrsTrue = strInUseMbrsTrue + ",U8#" + tChild.Name
					Else
						strInUseMbrsFalse = strInUseMbrsFalse + ",U8#" + tChild.Name
					End If
				End If
			Next
			
			
			Dim outListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
			If InUseValue Then
				Dim outListInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, strInUseMbrsTrue, Nothing)
				Dim outListList As New MemberList(outListHeader, outListInfos)
				Return outListList
			Else
				Dim outListInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, strInUseMbrsFalse, Nothing)
				Dim outListList As New MemberList(outListHeader, outListInfos)
				Return outListList
			End If
			
		End If
						
#End Region

#Region "GetInUseATUMbrs"

If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetInUseATUMbrs") Then
		
		    ' Get WF Time
		   	Dim wfTime As String = api.Workflow.GetWorkflowUnitInfo.TimeName
			'brapi.ErrorLog.LogMessage(si, "WF Time: " & wfTime)
			Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si, dimtypeid.Time, wfTime)
			'brapi.ErrorLog.LogMessage(si, "WF Time ID: " & wfTimeId)
			
		    ' Get Info from the Workflow and POV
		    ' Get Scenario Type ID from the active Workflow
		    Dim scenarioKey As Integer = si.WorkflowClusterPk.ScenarioKey
		    Dim objScenarioType As ScenarioType = BRApi.Finance.Scenario.GetScenarioType(si, scenarioKey)
		    Dim scenarioTypeId As Integer = objScenarioType.Id
		    
		    ' Define the initial ATU list to check (Your filter)
		    Dim ATUFilter As String = "U4#Total_ATU.Children.Remove(99_AMMO,99_BF,99_Claims,99_GSA_Rent,99_GSA_Security,99_INDREC,99_Medals,CG_41_ADLM,CG_43_FDLM,CG_45_VDLM,CP,EnvCR,MHC,MP,PCSC,RT,PCI,RD,RP,MERHCF,MOSP,BS,F,CG_833,No_ATU)"
		    Dim allPotentialMbrs As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, ATUFilter, Nothing)
		    
		    Dim atuStringList As String = ""
		    Dim ATUInUse As Boolean = True

		    ' Loop through and check UD4 InUse status
		    If Not allPotentialMbrs Is Nothing Then
		        For Each mbrInfo As MemberInfo In allPotentialMbrs
		            Dim atuId As Integer = mbrInfo.Member.MemberPk.MemberId
		            
		            Dim bInUse As Boolean = BRApi.Finance.UD.InUse(si, DimTypeId.UD4, atuId, scenarioTypeId, wfTimeId)
					'brapi.ErrorLog.LogMessage(si, "In Use: " & mbrInfo.Member.Name & " | " & wfTimeId & " | " & objScenarioType.ToString & " | " & bInUse)
		            
		            If bInUse Then
		                If ATUInUse Then
		                    atuStringList = "U4#" & mbrInfo.Member.Name
		                    ATUInUse = False
		                Else
		                    atuStringList = atuStringList & ", U4#" & mbrInfo.Member.Name
		                End If
		            End If
		        Next
		    End If

		    ' Create the final member list to return
		    Dim ATUListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
		    
		    ' Safety check if nothing is in use
		    If String.IsNullOrEmpty(atuStringList) Then
		        Return New MemberList(ATUListHeader, New List(Of MemberInfo)())
		    End If

		    ' Use the built string to get the final MemberInfo list
		    Dim finalAtuMemberInfo As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, atuStringList, Nothing)
		    Return New MemberList(ATUListHeader, finalAtuMemberInfo)

End If

#End Region

#Region "GetInUseOPFACMbrs"
		
		'U4#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetInUseOPFACMbrs], ATU=|!prm_BLT_ATU_OS!|)	
		
                    If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetInUseOPFACMbrs") Then
                        Dim opfacList As MemberList = Nothing
						Dim wfYear As String = api.Workflow.GetWorkflowUnitInfo.TimeName
						'Get the user selected ATU from the combo box selection	
						Dim ATU As String = args.MemberListArgs.NameValuePairs.XFGetValue("ATU")
						If globals.GetObject($"opfacList_{ATU}_{wfYear}") Is Nothing
	                        'Get the current workflow year
													
							Dim ATUFilter As String = "U4#" & ATU & ".Children.Where((Name DoesNotContain 'NoUnit') And (Text8 DoesNotContain 'NotInUse'))"
							Dim inUseListInfo As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, ATUFilter, Nothing)
							                        
	                        'Create the final MemberList using the filtered list
	                        Dim opfacListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
	                        opfacList = New MemberList(opfacListHeader, inUseListInfo)
							globals.SetObject($"opfacList_{ATU}_{wfYear}",opfacList) 
						Else
							opfacList = globals.GetObject($"opfacList_{ATU}_{wfYear}")
						End If

						'Return the final list
                        Return opfacList
                        
                    End If

               ' End If
						
#End Region

#Region "ConcurrentClearanceMatrix"

	'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[ConcurrentClearanceMatrix], RPRowOption = |!prm_RPRowOption!|, RPRowSelector = |!prm_RPRowSelector!|, Status=[|!prm_RPStatusSelector!|], Appropriation =[|!prm_RPAppropriationSelector!|, BUDCAT=[|!prm_RPBudCatSelector!|]
	
		If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("ConcurrentClearanceMatrix") Then
			Dim wfPk As WorkflowUnitPk = api.Workflow.GetWorkflowUnitInfo.WfUnitPk
			Dim ScenarioTypeId As Integer = api.Scenario.GetScenarioType().Id
			Dim TimeId As Integer = wfPk.TimeKey		
			
			Dim wfYearYY As String = api.Workflow.GetWorkflowUnitInfo.TimeName.Substring(2,2)
			Dim rpRowOption As String = args.MemberListArgs.NameValuePairs.XFGetValue("RpRowOption")
			Dim rpRowSelector As String = args.MemberListArgs.NameValuePairs.XFGetValue("RpRowSelector")
			Dim mbrFilter As String = String.Empty
			If rpRowOption = "All"  
						mbrFilter = "F#FY" & wfYearYY & "_RP.Base.Where(Name DoesNotContain _WV)"
					Else 'Is Custom
						mbrFilter = "F#RP.List(" & rpRowSelector & ")"
					End If
'					BrApi.ErrorLog.LogMessage(si, rpRowSelector)
			Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
			Dim xlistInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, mbrFilter, Nothing)
						
			Dim flowDimPk As DimPk = api.Dimensions.GetDim("Std_Flow").DimPk
			Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(flowDimPk,mbrFilter)
			Dim strMbrList As String = ""

			Dim i As Integer = 0
			For Each mbr In listInfos
				Dim rpText8 As String = api.Flow.Text(mbr.Member.MemberId, 8, DimConstants.Unknown, DimConstants.Unknown)
				Dim rpField8 As List (Of String) = Stringhelper.SplitString(rpText8, "_")
				Dim rpText1 As String = api.Flow.Text(mbr.Member.MemberId, 1, ScenarioTypeId, TimeId)
				Dim rpField1 As List (Of String) = Stringhelper.SplitString(rpText1, "|")
				
				'Dim paramAppropriation As String = args.MemberListArgs.NameValuePairs.XFGetValue("Appropriation")
				Dim paramAppropriation As String = args.MemberListArgs.NameValuePairs.XFGetValue("Appropriation")
				'BrApi.ErrorLog.LogMessage(si, paramAppropriation)
				Dim paramAppropriationList As List (Of String) = paramAppropriation.Replace(" ", "").Split(",").ToList()
															
				Dim paramBUDCAT As String = args.MemberListArgs.NameValuePairs.XFGetValue("BUDCAT")
				Dim paramStatus As String = args.MemberListArgs.NameValuePairs.XFGetValue("Status")
				
				If rpField8.count > 4 Then
					'Filter on selected Appropriation, F#, Text8, field 4
					For Each appropriation In paramAppropriationList
						If appropriation.XFEqualsIgnoreCase(rpField8(4)) Then
							'Filter on selected BUDCAT, F#, Text8, field 5	
							If paramBUDCAT.Contains(rpField8(5)) Then								
								'Filter on selected Status, F#, Text1, field 1															
								If rpField1.count > 2 Then
									If paramStatus.Contains(rpField1(0)) Then	
										strMbrList = strMbrList & " ,F#" & mbr.Member.Name	
									
									End If
								End If	
							End If
						End If
					Next
				End If
								
			i += 1
			Next
			
			If strMbrList.Length = 0 Then
				strMbrList = " "
			Else
				'Do nothing, use member list created above
			End If
			
			Dim outListInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, strMbrList, Nothing)

			Return New MemberList(listHeader, outlistInfos)

	End If
	
	
	
#End Region

#Region "RPMaintList"

	'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[RPMaintList], Appropriation =[|!prm_Approp_ADM!|], SearchQuery=[|!prm_SearchQuery_ADM!|])
	
		If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("RPMaintList") Then
			
			
			
			Dim searchQuery As String = args.MemberListArgs.NameValuePairs.XFGetValue("SearchQuery")	
			Dim paramAppropriation As String = args.MemberListArgs.NameValuePairs.XFGetValue("Appropriation")														
			
			Dim wfPk As WorkflowUnitPk = api.Workflow.GetWorkflowUnitInfo.WfUnitPk
			Dim ScenarioTypeId As Integer = api.Scenario.GetScenarioType().Id
			Dim TimeId As Integer = wfPk.TimeKey					
			Dim wfYearYY As String = api.Workflow.GetWorkflowUnitInfo.TimeName.Substring(2,2)
			Dim mbrFilter As String = "F#FY" & wfYearYY & "_RP.Base"
			Dim mbrFilter_WV As String = "F#FY" & wfYearYY &"_RP_WV.Base"
			Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)						
			Dim flowDimPk As DimPk = api.Dimensions.GetDim("Std_Flow").DimPk
			Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(flowDimPk,mbrFilter)
			Dim listInfos_WV As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(flowDimPk,mbrFilter_WV)
			
			Dim strMbrList As List (Of String) = New List (Of String)
			Dim strMbrList_WV As List (Of String) = New List (Of String)
			Dim mbrScriptBuilder As New Text.StringBuilder
			
			If listInfos.Count > 1 Then
				For Each mbr In listInfos
				
					Dim rpText8 As String = api.Flow.Text(mbr.Member.MemberId, 8, DimConstants.Unknown, DimConstants.Unknown)
					Dim rpField8 As List (Of String) = Stringhelper.SplitString(rpText8, "_")
					Dim rpApprop As String = rpField8.Item(4)
					Dim rpText1 As String = api.Flow.Text(mbr.Member.MemberId, 1, ScenarioTypeId, TimeId)
				
					If rpText1.Contains("|") Then
						If (searchQuery.Length = 0)
							If (rpField8.count > 5) Then
								If paramAppropriation.XFEqualsIgnoreCase(rpApprop) Then
									If Not strMbrList.Contains(mbr.Member.Name)	
										strMbrList.Add(mbr.Member.Name)
									End If 
								End If
							End If	
						Else 'search query has input
							If (rpField8.count > 5) Then
								If paramAppropriation.XFEqualsIgnoreCase(rpApprop) Then	
									If rpText8.XFContainsIgnoreCase(searchQuery) Or mbr.Member.Description.XFContainsIgnoreCase(searchQuery)
										If Not strMbrList.Contains(mbr.Member.Name)	
											strMbrList.Add(mbr.Member.Name)
										End If 
									End If
								End If	
							End If	
						End If
					Else 'Do nothing	
					End If	
				Next
			End If
			
			If listInfos_WV.Count > 1 Then
				For Each mbr_WV In listInfos_WV
				
					Dim rpText8_WV As String = api.Flow.Text(mbr_WV.Member.MemberId, 8, DimConstants.Unknown, DimConstants.Unknown)
					Dim rpField8_WV As List (Of String) = Stringhelper.SplitString(rpText8_WV, "_")
					Dim rpApprop_WV As String = rpField8_WV.Item(4)
					Dim rpText1_WV As String = api.Flow.Text(mbr_WV.Member.MemberId, 1, ScenarioTypeId, TimeId)
				
					If rpText1_WV.Contains("|") Then
						If (searchQuery.Length = 0)
							If (rpField8_WV.count > 5)  Then
								If paramAppropriation.XFEqualsIgnoreCase(rpApprop_WV) Then
									If Not strMbrList_WV.Contains(mbr_WV.Member.Name)	
										strMbrList_WV.Add(mbr_WV.Member.Name)
									End If 
								End If
							End If	
						Else 'search query has input
							If (rpField8_WV.count > 5)  Then
								If paramAppropriation.XFEqualsIgnoreCase(rpApprop_WV) Then	
									If rpText8_WV.XFContainsIgnoreCase(searchQuery) Or mbr_WV.Member.Description.XFContainsIgnoreCase(searchQuery)
										If Not strMbrList_WV.Contains(mbr_WV.Member.Name)	
											strMbrList_WV.Add(mbr_WV.Member.Name)
										End If
									End If
								End If	
							End If	
						End If
					Else 'Do nothing	
					End If	
				Next
			End If 
			
			Dim combined As New List (Of String)(Enumerable.Concat(strMbrList,strMbrList_WV))
			
			combined.Sort()

			For Each rp As String In combined
	
				mbrScriptBuilder.Append("F#" & rp & ",")
			
			Next

	
         	If  (mbrScriptBuilder.Length = 0)
			 'Return Nothing
			Else
				
				'Do nothing, use member list created above
				Dim outListInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, mbrScriptBuilder.ToString, Nothing)
				Return New MemberList(listHeader, outlistInfos)
			End If 
		End If
							

#End Region

#Region "GetAllocChklistRPs"

	'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetAllocChklistRPs], Status=[|!prm_RPStatusSelector!|], Appropriation =[|!prm_Approp_ADM!|], SearchQuery=[|!prm_SearchQuery_ADM!|])
	
		If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetAllocChklistRPs") Then
			
			Dim searchQuery As String = args.MemberListArgs.NameValuePairs.XFGetValue("SearchQuery")	
			Dim paramAppropriation As String = args.MemberListArgs.NameValuePairs.XFGetValue("Appropriation")														
			Dim paramStatus As String = args.MemberListArgs.NameValuePairs.XFGetValue("Status")														
			Dim paramBudCat As String = args.MemberListArgs.NameValuePairs.XFGetValue("BudCat")
			
			
			Dim wfPk As WorkflowUnitPk = api.Workflow.GetWorkflowUnitInfo.WfUnitPk
			Dim ScenarioTypeId As Integer = api.Scenario.GetScenarioType().Id
			Dim TimeId As Integer = wfPk.TimeKey					
			Dim wfYearYY As String = api.Workflow.GetWorkflowUnitInfo.TimeName.Substring(2,2)
			Dim mbrFilter As String = "F#FY" & wfYearYY & "_RP.Base.Where(Name DoesNotContain _WV)"
			Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)						
			Dim flowDimPk As DimPk = api.Dimensions.GetDim("Std_Flow").DimPk
			Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(flowDimPk,mbrFilter)
			Dim strMbrList As List (Of String) = New List (Of String)
			Dim mbrScriptBuilder As New Text.StringBuilder
			
			If listInfos.Count > 1 Then
				For Each mbr In listInfos
				
					Dim rpText8 As String = api.Flow.Text(mbr.Member.MemberId, 8, DimConstants.Unknown, DimConstants.Unknown)
					Dim rpField8 As List (Of String) = Stringhelper.SplitString(rpText8, "_")
					Dim rpApprop As String = rpField8.Item(4)
					Dim rpBudCat As String = rpField8.Item(5)
					Dim rpText1 As String = api.Flow.Text(mbr.Member.MemberId, 1, ScenarioTypeId, TimeId)
				
					If rpText1.Contains("|") Then
						Dim rpField1 As List (Of String) = Stringhelper.SplitString(rpText1, "|")
						Dim rpStatus As String = rpField1.Item(0)
						If (searchQuery.Length = 0)
							If rpField8.count > 5 Then
								'Filter on selected Appropriation, F#, Text8, field 4
								If paramAppropriation.XFEqualsIgnoreCase(rpApprop) Then
									'Filter on selected BUDCAT, F#, Text8, field 5	
									If paramBudCat.XFContainsIgnoreCase(rpBudCat) Then
										'Filter on selected Status, F#, Text1, field 1														
										If rpField1.count > 2 Then
											If paramStatus.XFContainsIgnoreCase(rpStatus) Then
												If Not strMbrList.Contains(mbr.Member.Name)	
													strMbrList.Add(mbr.Member.Name)
												End If
											End If
										End If
									End If
								End If
							End If	
						Else 'search query has input
							If rpField8.count > 5 Then
								'Filter on selected Appropriation, F#, Text8, field 4
								If paramAppropriation.XFEqualsIgnoreCase(rpApprop) Then	
									'Filter on selected BUDCAT, F#, Text8, field 5	
									If paramBudCat.XFContainsIgnoreCase(rpBudCat) Then									
										'Filter on selected Status, F#, Text1, field 1		
										If paramStatus.XFContainsIgnoreCase(rpStatus) Then															
											If rpField1.count > 2 Then
												If rpText8.XFContainsIgnoreCase(searchQuery) Or mbr.Member.Description.XFContainsIgnoreCase(searchQuery)
													If Not strMbrList.Contains(mbr.Member.Name)	
														strMbrList.Add(mbr.Member.Name)
													End If
												End If
											End If
										End If
									End If
								End If	
							End If	
						End If
					Else 'Do nothing	
					End If	
				Next
			End if 
			
			strMbrList.Sort()

			For Each rp As String In strMbrList
	
				mbrScriptBuilder.Append("F#" & rp & ",")
			
			Next

			If  (mbrScriptBuilder.Length = 0)
			
			Else
			'Do nothing, use member list created above
			
				Dim outListInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, mbrScriptBuilder.ToString, Nothing)
				Return New MemberList(listHeader, outlistInfos)
			End If 
	End If
							

#End Region

#Region "GetRPByStatus"
			
			'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetRPByStatus], Status=[|!prm_Status!|], RPRowOption=[|!prm_RPRowOption_Extractor!|])

			If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetRPByStatus") Then
	
				Dim rpRowOption As String = args.MemberListArgs.NameValuePairs.XFGetValue("RPRowOption")
	
			    'Dim stpw As Stopwatch = Stopwatch.StartNew
				Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
				Dim mbrScriptBuilder As New Text.StringBuilder

				'WF Time
				Dim wfTime As String = api.Workflow.GetWorkflowUnitInfo.TimeName
				Dim wfYY As String = wfTime.Substring(2,2)

				'Status selection
				Dim Status As String = args.MemberListArgs.NameValuePairs.XFGetValue("Status")

				'Gathering Scenario info to Analyze WFText1
				Dim wfTimeId As Integer = api.Members.GetMemberId(dimtypeid.Time, wfTime)
				Dim wfScenario As String = api.Workflow.GetWorkflowUnitInfo.ScenarioName
				Dim wfScenarioId As Integer = api.Members.GetMemberId(dimtypeid.Scenario, api.Workflow.GetWorkflowUnitInfo.ScenarioName)
				Dim scenarioTypeId As Integer = BRApi.Finance.Scenario.GetScenarioType(si, wfScenarioId).Id

				Dim rpList As List (Of String) = New List (Of String)


				If rpRowOption.Contains("All")
		
					Dim dataBufferFormula As String = "FilterMembers(RemoveZeros(T#" & wfTime & ":O#Top:I#Top:A#Funding), F#FY" & wfYY & "_RP.Base)"
					Dim startingBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula(dataBufferFormula,,False)
		
					'startingBuffer.LogDataBuffer(api,"Starting Data Buffer on: ",1000)

					If Not startingBuffer Is Nothing Then
			
						For Each startingCell As DataBufferCell In startingBuffer.DataBufferCells.Values
				
							If Not startingCell.CellStatus.IsNoData() Then
								
								Dim rpName As String = api.Members.GetMember(dimTypeId.Flow,startingCell.GetFlowName(api)).Name
								
								If Not rpName.Contains("WV") Then
					
									Dim rpID As Integer = api.members.GetMemberId(dimtypeid.Flow, rpName)

									Dim text1 As String = api.Flow.Text(rpID, 1, scenarioTypeId, wfTimeId)
						
									Dim rptextsplit() As String = text1.Split ("|")
									Dim budgetStatus As String = rptextsplit(0)
										
										If budgetStatus = Status
										'If Not mbrScriptBuilder.ToString.Contains(rpname)
											If Not rpList.Contains(rpname) Then
												rpList.Add(rpname)
												'mbrScriptBuilder.Append("F#" & rpname & ",")
											End If 	
						
					     				End If 
								End If	
							End If
						Next
					End If
		
					rpList.Sort()



					For Each rp As String In rpList
						mbrScriptBuilder.Append("F#" & rp & ",")
					Next
		
		If  (mbrScriptBuilder.Length = 0)
			'Do Nothing
		Else	
			Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, mbrScriptBuilder.ToString, Nothing)
		
		'stpw.Stop()
		
		'BRApi.ErrorLog.LogMessage(si, "Elapsed Time: " & stpw.Elapsed.ToString)
		
			Return New MemberList(listHeader, listInfos)
		End If
		
	Else 'Custom RP List
	
	End If	
End If
	
#End Region

#Region "GetRPByStatusCustom"


		'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetRPByStatusCustom], Status=[|!prm_Status!|], RPRowOption=[|!prm_RPRowOption_Extractor!|])

		If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetRPByStatusCustom") Then
	
		Dim rpRowOption As String = args.MemberListArgs.NameValuePairs.XFGetValue("RPRowOption")
	
		'Dim stpw As Stopwatch = Stopwatch.StartNew
		Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
		Dim mbrScriptBuilder As New Text.StringBuilder

		'WF Time
		Dim wfTime As String = api.Workflow.GetWorkflowUnitInfo.TimeName
		Dim wfYY As String = wfTime.Substring(2,2)


		'Status selection
		Dim Status As String = args.MemberListArgs.NameValuePairs.XFGetValue("Status")

		'Gathering Scenario info to Analyze WFText1
		Dim wfTimeId As Integer = api.Members.GetMemberId(dimtypeid.Time, wfTime)
		Dim wfScenario As String = api.Workflow.GetWorkflowUnitInfo.ScenarioName
		Dim wfScenarioId As Integer = api.Members.GetMemberId(dimtypeid.Scenario, api.Workflow.GetWorkflowUnitInfo.ScenarioName)
		Dim scenarioTypeId As Integer = BRApi.Finance.Scenario.GetScenarioType(si, wfScenarioId).Id

		Dim rpList As List (Of String) = New List (Of String)


			If rpRowOption.Contains("Custom")' = "BDF_RP_PPA_Extractor_All_Rows" Then
			
				Dim dataBufferFormula As String = "FilterMembers(RemoveZeros(Cb#BudFm:E#Total_Lead_Office:C#Aggregated:S#" & wfScenario & ":T#" & wfTime & ":O#Top:I#Top:A#Funding), F#FY" & wfYY & "_RP.Base)"
				Dim startingBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula(dataBufferFormula,,False)
				

				If Not startingBuffer Is Nothing Then
				
					For Each startingCell As DataBufferCell In startingBuffer.DataBufferCells.Values
					
						If Not startingCell.CellStatus.IsNoData() Then
						
							Dim rpName As String = api.Members.GetMember(dimTypeId.Flow,startingCell.GetFlowName(api)).Name
								If Not rpName.Contains("WV") Then
						
									Dim rpID As Integer = api.members.GetMemberId(dimtypeid.Flow, rpName)

									Dim text1 As String = api.Flow.Text(rpID, 1, scenarioTypeId, wfTimeId)
							
									Dim rptextsplit() As String = text1.Split ("|")
									Dim budgetStatus As String = rptextsplit(0)
									If budgetStatus = Status
										'If Not mbrScriptBuilder.ToString.Contains(rpname)
										If Not rpList.Contains(rpname) Then
											rpList.Add(rpname)
											'mbrScriptBuilder.Append("F#" & rpname & ",")
										End If 	
							
						    		 End If 
								End If	
						End If
					Next
				End If
			
				rpList.Sort()



				For Each rp As String In rpList
					mbrScriptBuilder.Append("F#" & rp & ",")
				Next

				If  (mbrScriptBuilder.Length = 0)
					
				Else
		 	
					Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, mbrScriptBuilder.ToString, Nothing)
			
					'stpw.Stop()
			
					'BRApi.ErrorLog.LogMessage(si, "Elapsed Time: " & stpw.Elapsed.ToString)
			
					Return New MemberList(listHeader, listInfos)
				End If
			
			 Else 'Standard List
		
			 End If	
		End If
	
#End Region

#Region "GetRPByStatusEUDBoard"
'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetRPByStatusEUDBoard], Status=[|!prm_UX_Status!|])
If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetRPByStatusEUDBoard") Then
	
		'WF Time
		Dim wfTime As String = api.Workflow.GetWorkflowUnitInfo.TimeName
		Dim wfYY As String = wfTime.Substring(2,2)
		Dim objUserName As String = api.SI.UserName

		Dim grpOfficeUsers As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Security_BudFm_r_OfficeUser")
		Dim grpOfficeUsersWV As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Security_BudFm_r_OfficeUserWV")
		Dim grpPowerUsers As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Security_BudFm_r_PowerUser")
	   
		'Status selection	
		Dim Status As String = args.MemberListArgs.NameValuePairs.XFGetValue("Status")
		
	   'Gathering Scenario info to Analyze WFText1
	    Dim wfTimeId As Integer = api.Members.GetMemberId(dimtypeid.Time, wfTime)
		Dim wfScenario As String = api.Workflow.GetWorkflowUnitInfo.ScenarioName
		Dim wfScenarioId As Integer = api.Members.GetMemberId(dimtypeid.Scenario, api.Workflow.GetWorkflowUnitInfo.ScenarioName)
		Dim scenarioTypeId As Integer = BRApi.Finance.Scenario.GetScenarioType(si, wfScenarioId).Id
		
		Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
		Dim mbrScriptBuilder As New Text.StringBuilder

		Dim rpList As List (Of String) = New List (Of String)

		Dim dataBufferFormula As String = "FilterMembers(RemoveZeros(T#" & wfTime & ":O#Top:I#Top:A#Funding), F#FY" & wfYY & "_RP.Base)"
		Dim startingBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula(dataBufferFormula,,False)

		'startingBuffer.LogDataBuffer(api,"Starting Data Buffer on: ",1000)
		
		If Not startingBuffer Is Nothing Then
			
			For Each startingCell As DataBufferCell In startingBuffer.DataBufferCells.Values
				
				If Not startingCell.CellStatus.IsNoData() Then
					Dim rpName As String = api.Members.GetMember(dimTypeId.Flow,startingCell.GetFlowName(api)).Name
					If Not rpName.Contains("WV") Then
						Dim rpID As Integer = api.members.GetMemberId(dimtypeid.Flow, rpName)
						Dim text1 As String = api.Flow.Text(rpID, 1, scenarioTypeId, wfTimeId)
						Dim text8 As String = api.Flow.Text(rpID, 8, DimConstants.Unknown, DimConstants.Unknown)
						Dim rptextsplit() As String = text1.Split ("|")
						Dim budgetStatus As String = rptextsplit(0)
						If budgetStatus = Status
							If (BRApi.Security.Authorization.IsUserInAdminGroup(Si)) Or (BRApi.Security.Authorization.IsUserInGroup(si, objUserName, grpPowerUsers,False))
							'If Not mbrScriptBuilder.ToString.Contains(rpname)
								If Not rpList.Contains(rpname)	
								'mbrScriptBuilder.Append("F#" & rpname & ",")
									rpList.Add(rpname)
								End If
							ElseIf (BRApi.Security.Authorization.IsUserInGroup(si, objUserName, grpOfficeUsers, False)) Or (BRApi.Security.Authorization.IsUserInGroup(si, objUserName, grpOfficeUsersWV, False))
								If (text8.Contains("OS")) Or (text8.Contains("PCI")) Or (text8.Contains("RD"))
									If Not rpList.Contains(rpname)
										rpList.Add(rpname)
									End If 
								End If 	
							End If 
								
						End If 

					End If 
				End If	
			 Next
		 End If

		rpList.Sort()

		For Each rp As String In rpList
	
			mbrScriptBuilder.Append("F#" & rp & ",")
		Next

	
         If  (mbrScriptBuilder.Length = 0)
			 'Return Nothing
         Else	
			Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, mbrScriptBuilder.ToString, Nothing)
			Return New MemberList(listHeader, listInfos)

         End If 	
	
End If
	
#End Region

#Region "GetOSModList"
'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetOSModList])
If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetOSModList") Then
	
		'WF Time
		Dim wfTime As String = api.Workflow.GetWorkflowUnitInfo.TimeName
		Dim wfYY As String = wfTime.Substring(2,2)

		Dim MemberFilterScript As String = "F#USCG_OS_" & wfYY & ".Descendants"
        Dim stringlist As List (Of String) = New List (Of String)

		Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, MemberFilterScript & ".Where(Text8 Contains [Mod])", Nothing)
		
		Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
	
		For Each rp In listInfos
			stringlist.Add("F#" & rp.Member.Name & ",")
		Next

	
        If  stringlist.Count = 0
'			 'Return Nothing
        Else
			Dim FinalString As String = String.Join(",", stringlist)
			Dim newlistInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, FinalString, Nothing)
			Return New MemberList(listHeader, newlistInfos)

         End If 	
	
End If
	
#End Region

#Region "GetRPChangeLog"					
						
						'F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetRPChangeLog],  SearchText=[|!prm_ChangeLog_ADM!|])
						If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetRPChangeLog") Then
							
							Dim wfYear As String = api.Workflow.GetWorkflowUnitInfo.TimeName
							Dim MemberFilterScriptWF As String = "F#FY" & wfYear.Substring(2,2) & "_RP.Base"
							Dim MemberFilterScriptWF_WV As String = "F#FY" & wfYear.Substring(2,2) & "_RP_WV.Base"
							Dim SearchQuery As String = args.MemberListArgs.NameValuePairs.XFGetValue("SearchText") 
							'brapi.ErrorLog.LogMessage(si,"SearchQuery " & SearchQuery)
							If SearchQuery = "" Then
								Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
								Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, MemberFilterScriptWF & "," & MemberFilterScriptWF_WV , Nothing)
								Dim listList As New MemberList(listHeader, listInfos)				
								Return listList		
							Else
								Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
								Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, MemberFilterScriptWF & ".Where((Name Contains [_" & SearchQuery & "_]) Or (Description Contains [" & SearchQuery & "]))," & MemberFilterScriptWF_WV & ".Where((Name Contains [_" & SearchQuery & "_]) Or (Description Contains [" & SearchQuery & "]))", Nothing)
								'api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, MemberFilterScriptWF_WV & ".Where((Name Contains [_" & SearchQuery & "_]) Or (Description Contains [" & SearchQuery & "]))", Nothing)
								Dim listList As New MemberList(listHeader, listInfos)								
								Return listList		
							End If		
								
						End If
						
#End Region

#Region "GetMatchListAttributes" 
' F#Root.CustomMemberList(BRName=Workspace.Current.BUDFM_Assembly.BUDFM_MbrLists, MemberListName=[GetRPMatchListAttributes], Scenario= [|!prm_RPT_SelectScenario_OS!|], SearchQuery=[|!prm_SearchQuery_CopySrc_OS!|], Appropriation=[|!prm_Approp_OS!|])

'						*******************Change-Log***********************************************
'						7/24/24 - PF - DHSUSCG-1867- Updated BR to allow selection for prior year RPs
'						8/14/24 - PF- DHSUSCG-1927 - Updated BR to allow search functionality to work for Source RPs | Added Split Functionality | Added Sort Functionality to Display RPs
'						8/23/24 - PF - DHSUSCG-1916 - Updated BR to sort RP List based on Scenario Chosen in combo box and display RPs based on Scenario Type Text 1 Value

						If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetRPMatchListAttributes") Then
							Dim wfPriorYearNum As Integer = api.Workflow.GetWorkflowUnitInfo.TimeName.XFConvertToInt - 1
							Dim wfPriorYear As String = wfPriorYearNum.ToString
							Dim wfYear As String = api.Workflow.GetWorkflowUnitInfo.TimeName
							Dim Scenariopick As String = args.MemberListArgs.NameValuePairs.XFGetValue("Scenario")
							Dim SearchQuery As String = args.MemberListArgs.NameValuePairs.XFGetValue("SearchQuery") 
							Dim Appropriation As String = args.MemberListArgs.NameValuePairs.XFGetValue("Appropriation")		
							Dim objUserName As String = api.SI.UserName
							Dim objuserInfo As UserInfo = BRApi.Security.Authorization.GetUser(si, objUserName)
'							This does an initial check to see if there is a value in the scenario combo box and if so we grab certain values and store in variables
							If Scenariopick <> ""								
								Dim stringlist As List (Of String) = New List (Of String)
								Dim splitvscenario() As String = Scenariopick.Split("_")
								Dim MemberFilterScriptWF As String = "F#" & splitvscenario(1) & "_RP.Base"
								Dim MemberFilterScriptWF_WV As String = "F#" & splitvscenario(1) & "_RP_WV.Base"
								Dim ScenarioMbrId As Integer = api.Members.GetMemberId(dimTypeId.Scenario, Scenariopick)
								Dim objScenarioType As Integer = api.Scenario.GetScenarioType(ScenarioMbrId).Id
								Dim wftim As String = api.Scenario.GetWorkflowTime(ScenarioMbrId).ToString
'								We then check if the User is and admin user anb based on if the search box is empty we implement some functionality
								If BRApi.Security.Authorization.IsUserInAdminGroup(si)
										If SearchQuery = "" Then
											Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
											Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, MemberFilterScriptWF & ".Where(Text8 Contains [_" & Appropriation & "_])," & MemberFilterScriptWF_WV & ".Where(Text8 Contains [_" & Appropriation & "_])", Nothing)
'											We iterate through the members we retrieved in list info and check to see if the Text 1 value that corresponds with the scenario type of the scenario has a value
'											If it has a value we append/add it to a new list
											For Each Item In listInfos
												Dim flowtext1 As String = api.Flow.Text(Item.Member.MemberId, 1, objScenarioType, wftim.XFConvertToInt())
												If flowtext1.Contains("|") Then
													stringlist.Add("F#" & Item.Member.Name & ",")
												Else
													Continue For
												End If
											Next
'											We then sort the list and and based on it's count we return a value and display the list
											stringlist.Sort()
											If stringlist.Count = 0
												Return Nothing ' was Return " " in the Object-typed BR; a String cannot convert to MemberList
											Else												
												Dim FinalString As String = String.Join(",", stringlist)
												Dim UpdatedMemberInfo As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, FinalString, Nothing)
												Dim listList As New MemberList(listHeader, UpdatedMemberInfo)				
												Return listList
											End If
										Else		'						
											Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
											Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, MemberFilterScriptWF & ".Where((Text8 Contains [_" & Appropriation & "_]) AND ((Text8 Contains [" & SearchQuery & "]) Or (Description Contains [" & SearchQuery & "])))," & MemberFilterScriptWF_WV & ".Where((Text8 Contains [_" & Appropriation & "_]) AND ((Text8 Contains [" & SearchQuery & "]) Or (Description Contains [" & SearchQuery & "])))", Nothing)
											For Each Item In listInfos
												Dim flowtext1 As String = api.Flow.Text(Item.Member.MemberId, 1, objScenarioType, wftim.XFConvertToInt())												
												If flowtext1.Contains("|") Then
													stringlist.Add("F#" & Item.Member.Name & ",")
												Else
													Continue For
												End If
											Next
											stringlist.Sort()
											If stringlist.Count = 0
												Return Nothing ' was Return " " in the Object-typed BR; a String cannot convert to MemberList
											Else												
												Dim FinalString As String = String.Join(",", stringlist)
												Dim UpdatedMemberInfo As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, FinalString, Nothing)
												Dim listList As New MemberList(listHeader, UpdatedMemberInfo)				
												Return listList
											End If	
										End If
		
								Else 'user is an not an admin so return a list based on their DataAccessSecurity
									'First, get a dictionary of their security parent group they are assigned to
									Dim userParentGroupsDict As Dictionary(Of Guid, Group) = BRApi.Security.Authorization.GetUser(si, objUserName).ParentGroups
									
	'								Next - Loop through the list Of data access groups To see If their parent Group matches, And If so, add the member filter From that Group To the member list
									Dim objCube As Cube = api.Cubes.GetCubeOrReferencedCubeForDataAccess(api.Pov.Cube.CubeId, api.Pov.EntityDim.DimPk.DimId)
									Dim CubeDataCellAccessItems As List(Of CubeDataAccessItem) = objCube.CubeDataCellAccessItems
										For Each Item As CubeDataAccessItem In CubeDataCellAccessItems
											If userParentGroupsDict.ContainsKey(Item.GroupUniqueID)
												Dim cubeMemberFilter As String = Item.GetCombinedMemberFilterString
'												Only Return a value If it starts With F# To make sure its a flow specific dimension data access item
'												If cube member filter contains '.Where(Text8 DoesNotContain '_WV') user does not have working version access so only return Non-WV RPs
												If (cubeMemberFilter.StartsWith("F#") And cubeMemberFilter.Contains(".Where(Name DoesNotContain '_WV')"))												
													If SearchQuery = "" Then
														Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
														Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, MemberFilterScriptWF & ".Where(Text8 Contains [_" & Appropriation & "_])", Nothing)
														For Each value In listInfos
															Dim flowtext1 As String = api.Flow.Text(value.Member.MemberId, 1, objScenarioType, wftim.XFConvertToInt())
															If flowtext1.Contains("|") Then
																stringlist.Add("F#" & value.Member.Name & ",")
															End If
														Next
														stringlist.Sort()
														If stringlist.Count = 0
															Return Nothing ' was Return " " in the Object-typed BR; a String cannot convert to MemberList
														Else												
															Dim FinalString As String = String.Join(",", stringlist)
															Dim UpdatedMemberInfo As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, FinalString, Nothing)
															Dim listList As New MemberList(listHeader, UpdatedMemberInfo)				
															Return listList
														End If
													Else
														Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
														Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, MemberFilterScriptWF & ".Where((Text8 Contains [_" & Appropriation & "_]) AND ((Text8 Contains [" & SearchQuery & "]) Or (Description Contains [" & SearchQuery & "])))" , Nothing)
														For Each value In listInfos
															Dim flowtext1 As String = api.Flow.Text(value.Member.MemberId, 1, objScenarioType, wftim.XFConvertToInt())
															If flowtext1.Contains("|") Then
																stringlist.Add("F#" & value.Member.Name & ",")
															End If
														Next
														stringlist.Sort()
														If stringlist.Count = 0
															Return Nothing ' was Return " " in the Object-typed BR; a String cannot convert to MemberList
														Else												
															Dim FinalString As String = String.Join(",", stringlist)
															Dim UpdatedMemberInfo As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, FinalString, Nothing)
															Dim listList As New MemberList(listHeader, UpdatedMemberInfo)				
															Return listList
														End If	
													End If
'												user has access To all flow members so just Return the regular list
												Else If cubeMemberFilter.StartsWith("F#")												
													If SearchQuery = "" Then
														Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
														Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, MemberFilterScriptWF & ".Where(Text8 Contains [_" & Appropriation & "_])," & MemberFilterScriptWF_WV & ".Where(Text8 Contains [_" & Appropriation & "_])", Nothing)
														For Each value In listInfos
															Dim flowtext1 As String = api.Flow.Text(value.Member.MemberId, 1, objScenarioType, wftim.XFConvertToInt())
															If flowtext1.Contains("|") Then
																stringlist.Add("F#" & value.Member.Name & ",")
															End If
														Next
														stringlist.Sort()
														If stringlist.Count = 0
															Return Nothing ' was Return " " in the Object-typed BR; a String cannot convert to MemberList
														Else												
															Dim FinalString As String = String.Join(",", stringlist)
															Dim UpdatedMemberInfo As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, FinalString, Nothing)
															Dim listList As New MemberList(listHeader, UpdatedMemberInfo)				
															Return listList
														End If	
													Else
														Dim listHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
														Dim listInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, MemberFilterScriptWF & ".Where((Text8 Contains [_" & Appropriation & "_]) AND ((Text8 Contains [" & SearchQuery & "]) Or (Description Contains [" & SearchQuery & "])))," & MemberFilterScriptWF_WV & ".Where((Text8 Contains [_" & Appropriation & "_]) AND ((Text8 Contains [" & SearchQuery & "]) Or (Description Contains [" & SearchQuery & "])))" , Nothing)
														For Each value In listInfos
															Dim flowtext1 As String = api.Flow.Text(value.Member.MemberId, 1, objScenarioType, wftim.XFConvertToInt())
															If flowtext1.Contains("|") Then
																stringlist.Add("F#" & value.Member.Name & ",")
															End If
														Next
														stringlist.Sort()
														If stringlist.Count = 0
															Return Nothing ' was Return " " in the Object-typed BR; a String cannot convert to MemberList
														Else												
															Dim FinalString As String = String.Join(",", stringlist)
															Dim UpdatedMemberInfo As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, FinalString, Nothing)
															Dim listList As New MemberList(listHeader, UpdatedMemberInfo)				
															Return listList
														End If	
													End If		
												End If
											End If
										Next
									
								End If
							End If
						End If
#End Region

#Region "GETRPPOA"
			 If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GETRPPOA") Then
			    
			    Dim wfYearYY As String = api.Workflow.GetWorkflowUnitInfo.TimeName.Substring(2,2)
			    Dim flowDimPk As DimPk = api.Dimensions.GetDim("Std_Flow").DimPk
			    
			    Dim filterRP As String = "F#FY" & wfYearYY & "_RP.Base"
			  
			    
			    Dim rpListInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(flowDimPk, filterRP, Nothing)
			    
			    If Not rpListInfos Is Nothing Then
			        Return New MemberList(New MemberListHeader(args.MemberListArgs.MemberListName), rpListInfos)
			    End If
			End If

Return Nothing
#End Region

				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Private Function BuildBilletFilter(ByVal startNum As Integer, ByVal endNum As Integer) As String
			Dim sb As New System.Text.StringBuilder
			For i As Integer = startNum To endNum
				If i > 9 Then
					sb.Append("U6#LineItem_" & i & ",")
				Else
					sb.Append("U6#LineItem_0" & i & ",")
				End If
			Next
			Dim result As String = sb.ToString
			If result.Length > 0 Then
				result = result.Remove(result.Length - 1)
			End If
			Return result
		End Function

	End Class
End Namespace
