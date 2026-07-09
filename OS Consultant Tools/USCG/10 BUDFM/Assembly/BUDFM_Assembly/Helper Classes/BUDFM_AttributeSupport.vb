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
Imports Newtonsoft.Json

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
	Public Module BUDFM_AttributeSupport
		Private NotInheritable Class AppnRoutingConfig
			Public ReadOnly DefaultContent As String
			Public ReadOnly DefaultPage As String
			Public ReadOnly Frame As String
			Public Sub New(ByVal defaultContent As String, ByVal defaultPage As String, ByVal frame As String)
				Me.DefaultContent = defaultContent
				Me.DefaultPage = defaultPage
				Me.Frame = frame
			End Sub
		End Class
		Private ReadOnly RoutingConfigMap As New Dictionary(Of String, AppnRoutingConfig)(StringComparer.OrdinalIgnoreCase) From {
			{"OS", New AppnRoutingConfig("OS_RP_Content", "OS_RP_Page1", "OS_RP_Frame")}
		}
		Private ReadOnly KnownAppnSuffixes As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
			"OS", "BS", "F", "PCI", "RP", "RD", "MOSP", "MERHCF", "AF", "PC"
		}

		Public Sub SetRPContentRoutingVars(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal vars As Dictionary(Of String, String), ByVal readEdit As String, ByVal content As String, ByVal subcontent As String, ByVal rpAppr As String, ByVal rpNumber As String, ByVal liNumber As String, ByVal wfScenario As String, ByVal wfTime As String, Optional ByVal forceRefresh As Boolean = False)
			Dim appn As String = If(String.IsNullOrWhiteSpace(rpAppr), "OS", rpAppr.Trim().ToUpperInvariant())
			Dim cfg As AppnRoutingConfig = ResolveRoutingConfig(appn)
			' Mode is a param now, not a dashboard swap. Security trumps the request:
			' a user the GBL check deems read-only never lands in Edit.
			Dim mode As String = If(readEdit.XFEqualsIgnoreCase("Edit") AndAlso Not Workspace.GBL.GBL_Assembly.GBL_Helpers.Is_Read_Only(si, "prm_Security_BudFm_r_Auditor"), "Edit", "View")
			vars.XFSetValue("prm_Mode_" & appn, mode)

			' Normalize legacy twin-suffixed names to the single canonical dashboard,
			' so callers still passing twin-token names route correctly during
			' migration (tokens were retired in phase 2).
			If String.IsNullOrWhiteSpace(content) Then content = cfg.DefaultContent
			content = content.Replace("_NonEditRP_", "_").Replace("_EditRP_", "_")
			' Appropriation-agnostic: every appn's canonical objects follow the
			' convention <APPN>_RP_Content / <APPN>_RP_Page1 / <APPN>_RP_Frame.
			If String.IsNullOrEmpty(subcontent) AndAlso content.XFEqualsIgnoreCase(cfg.DefaultContent) Then
				subcontent = cfg.DefaultPage
			End If
			subcontent = subcontent.Replace("_NonEditRP_", "_").Replace("_EditRP_", "_")

			' Single frame regardless of mode (transition shim: the frame param stays
			' populated so existing EmbeddedDashboard bindings don't blank out; it can
			' be dropped once the frame embed is rebound directly).
			vars.XFSetValue("prm_Content_Frame_" & appn, cfg.Frame)
			vars.XFSetValue("prm_Content_" & appn, content)
			vars.XFSetValue("prm_Content_Page_" & appn, subcontent)
			vars.XFSetValue("prm_Content_EditRP_" & appn, subcontent) ' legacy param name — drop once page embeds rebind to _Content_Page_
			If String.IsNullOrEmpty(rpNumber) Then
				vars.XFSetValue("prm_Number_" & appn, String.Empty)
				Return
			End If
			vars.XFSetValue("prm_Number_" & appn, rpNumber)

			Dim entity As String = GetRPEntity(si, rpNumber)
			Dim subjectArea As String = If(content.XFContainsIgnoreCase("AddEditNonBillets"), "NonBillet", If(content.XFContainsIgnoreCase("AddEditBillets"), "Billet", "RP"))
			Dim liKey As String = If(String.IsNullOrEmpty(liNumber), "none", liNumber)
			Dim key As String = String.Format("attr_{0}_{1}_{2}_{3}_{4}_{5}", subjectArea, appn, rpNumber, liKey, wfScenario, wfTime)
			Dim pov As DateTime = GetPovLastEdited(si, entity, wfScenario, wfTime, rpNumber, liKey)
			Dim attrs As Dictionary(Of String, String) = GetAttributes(si, globals, key, pov, entity, wfScenario, wfTime, rpNumber, liKey, appn, subjectArea, forceRefresh)
			For Each kvp As KeyValuePair(Of String, String) In attrs
				For Each targetParam As String In GetAttributeParamTargets(kvp.Key, appn)
					vars.XFSetValue(targetParam, kvp.Value)
				Next
			Next
		End Sub

		Private Function ResolveRoutingConfig(ByVal appn As String) As AppnRoutingConfig
			If RoutingConfigMap.ContainsKey(appn) Then Return RoutingConfigMap(appn)
			Return New AppnRoutingConfig(appn & "_RP_Content", appn & "_RP_Page1", appn & "_RP_Frame")
		End Function

		Private Function GetAttributeParamTargets(ByVal account As String, ByVal appn As String) As List(Of String)
			Dim targets As New List(Of String)
			If Not ParamMap.ContainsKey(account) Then Return targets
			Dim legacyParam As String = ParamMap(account)
			Dim canonicalParam As String = ResolveAppnParamName(legacyParam, appn)
			targets.Add(canonicalParam)
			If Not canonicalParam.XFEqualsIgnoreCase(legacyParam) Then targets.Add(legacyParam)
			Return targets
		End Function

		Private Function ResolveAppnParamName(ByVal paramName As String, ByVal appn As String) As String
			If String.IsNullOrWhiteSpace(paramName) Then Return paramName
			Dim normalizedAppn As String = If(String.IsNullOrWhiteSpace(appn), "OS", appn.Trim().ToUpperInvariant())
			Dim suffixIdx As Integer = paramName.LastIndexOf("_"c)
			If suffixIdx < 0 OrElse suffixIdx >= paramName.Length - 1 Then Return paramName
			Dim suffix As String = paramName.Substring(suffixIdx + 1)
			If Not KnownAppnSuffixes.Contains(suffix) Then Return paramName
			Return paramName.Substring(0, suffixIdx + 1) & normalizedAppn
		End Function

		Public Function GetAttributes(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal key As String, ByVal povStamp As DateTime, ByVal entity As String, ByVal wfScenario As String, ByVal wfTime As String, ByVal rpNumber As String, ByVal liNumber As String, ByVal rpAppr As String, ByVal subjectArea As String, ByVal forceRefresh As Boolean) As Dictionary(Of String, String)
			Dim cachedKey As String = GetCache(si, "key")
			Dim cachedStmp As String = GetCache(si, "stamp")
			Dim cachedJson As String = GetCache(si, "dict")
			Dim fresh As Boolean = (Not forceRefresh) AndAlso cachedKey = key AndAlso Not String.IsNullOrEmpty(cachedStmp) AndAlso povStamp <= DateTime.Parse(cachedStmp, CultureInfo.InvariantCulture)
			If fresh AndAlso Not String.IsNullOrEmpty(cachedJson) Then
				Return JsonConvert.DeserializeObject(Of Dictionary(Of String, String))(cachedJson)
			End If
			Dim dict As Dictionary(Of String, String) = LoadAttributes(si, entity, wfScenario, wfTime, rpNumber, liNumber, rpAppr, subjectArea)
			SetCache(si, "key", key) : SetCache(si, "stamp", povStamp.ToString("o", CultureInfo.InvariantCulture)) : SetCache(si, "dict", JsonConvert.SerializeObject(dict))
			Return dict
		End Function

		' Subject area -> the parent account whose BASE members hold that page's
		' attributes (matches the legacy parAccount values, so each page loads
		' ONLY its own attribute slice instead of everything).
		Private ReadOnly SubjectParentAccount As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase) From {
			{"RP", "RP_Attributes"},
			{"Billet", "Billet_LineItem_Data"},
			{"NonBillet", "NonBillet_LineItem_Data"},
			{"ExpenseRD", "Expense_LineItem_RD"},
			{"ExpenseBS", "Expense_LineItem_BS"}
		}

		' parAccount -> quoted account list for the SQL IN clause; base members
		' rarely change, so resolve once per app domain.
		Private ReadOnly AcctListCache As New Dictionary(Of String, String)

		Private Function GetAccountInList(ByVal si As SessionInfo, ByVal parAccount As String) As String
			SyncLock AcctListCache
				If AcctListCache.ContainsKey(parAccount) Then Return AcctListCache(parAccount)
			End SyncLock
			Dim sb As New Text.StringBuilder
			Try
				Dim acctDimPk As DimPk = BRApi.Finance.Dim.GetDim(si, "BudFm_Account").DimPk
				Dim mbrs As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, acctDimPk, parAccount & ".Base", Nothing)
				If mbrs IsNot Nothing Then
					For Each mi As MemberInfo In mbrs
						sb.Append("'" & mi.Member.Name & "',")
					Next
				End If
			Catch ex As Exception
				' fall through to ParamMap below
			End Try
			If sb.Length = 0 Then
				' metadata lookup failed or parent empty -- fall back to the full
				' ParamMap set (superset; correctness preserved, scoping lost)
				For Each acct As String In ParamMap.Keys
					sb.Append("'" & acct & "',")
				Next
			End If
			Dim result As String = sb.ToString().TrimEnd(","c)
			SyncLock AcctListCache
				AcctListCache(parAccount) = result
			End SyncLock
			Return result
		End Function

		Private Function LoadAttributes(ByVal si As SessionInfo, ByVal entity As String, ByVal scenario As String, ByVal time As String, ByVal rpNumber As String, ByVal liNumber As String, ByVal rpAppr As String, ByVal subjectArea As String) As Dictionary(Of String, String)
			Dim parAccount As String = SubjectParentAccount.XFGetValue(subjectArea, "RP_Attributes")
			Dim ud6 As String = If(String.IsNullOrEmpty(liNumber) OrElse liNumber.XFEqualsIgnoreCase("none"), "None", liNumber)
			Return LoadAttributesByParent(si, parAccount, entity, scenario, time, rpNumber, ud6, "None")
		End Function

		' Ported from USCG_BudFm_Utilities.GetRPAttributes: RankedData query over
		' dbo.DataAttachment, Forms beats Import beats everything else per Account,
		' scoped to the base members of parAccount (incl. the UD8 filter the
		' legacy SQL had).
		Private Function LoadAttributesByParent(ByVal si As SessionInfo, ByVal parAccount As String, ByVal entity As String, ByVal scenario As String, ByVal time As String, ByVal flow As String, ByVal ud6 As String, ByVal ud8 As String) As Dictionary(Of String, String)
			Dim attributeDict As New Dictionary(Of String, String)

			Dim sql As New Text.StringBuilder
			sql.AppendLine("WITH RankedData AS (")
			sql.AppendLine("    SELECT Account, [Text], Origin,")
			sql.AppendLine("    ROW_NUMBER() OVER(PARTITION BY Account ORDER BY ")
			sql.AppendLine("        CASE WHEN Origin = 'Forms' AND [Text] <> '' THEN 1 ")
			sql.AppendLine("             WHEN Origin = 'Import' AND [Text] <> '' THEN 2 ")
			sql.AppendLine("             ELSE 3 END ASC) as PriorityRank")
			sql.AppendLine("    FROM dbo.DataAttachment")
			sql.AppendLine("    WHERE Cube = 'BudFm'")
			sql.AppendFormat("    AND [Time] = '{0}' ", time) : sql.AppendLine("")
			sql.AppendFormat("    AND Scenario = '{0}' ", scenario) : sql.AppendLine("")
			sql.AppendFormat("    AND Entity = '{0}' ", entity) : sql.AppendLine("")
			sql.AppendFormat("    AND Flow = '{0}' ", flow) : sql.AppendLine("")
			sql.AppendLine("    AND UD6 = '" & ud6 & "'")
			sql.AppendLine("    AND UD8 = '" & ud8 & "'")
			sql.AppendLine("    AND Account IN (" & GetAccountInList(si, parAccount) & ")")
			sql.AppendLine(")")
			sql.AppendLine("SELECT Account, [Text], Origin")
			sql.AppendLine("FROM RankedData")
			sql.AppendLine("WHERE PriorityRank = 1")

			Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp, sql.ToString, True)
				For Each dtRow As DataRow In dt.Rows
					attributeDict.Add(dtRow("Account"), dtRow("Text"))
				Next
			End Using

			Return attributeDict
		End Function

		' Drop-in replacement for the legacy USCG_BudFm_Utilities.GetRPAttributes
		' custom calc: same contract (reads scriptGenerics/parAccount from globals,
		' writes attributeDict back to globals) so the 32 extender call sites are a
		' one-line swap -- but parent-scoped, UD8-filtered, and behind the
		' timestamp refresh gate + session cache.
		' Wipes the attribute session cache so the next load hits the database
		' regardless of the timestamp gate (user-driven Refresh).
		Public Sub ClearAttributeCache(ByVal si As SessionInfo)
			SetCache(si, "key", String.Empty)
			SetCache(si, "stamp", String.Empty)
			SetCache(si, "dict", String.Empty)
		End Sub

		Public Sub GetRPAttributes(ByVal si As SessionInfo, ByVal globals As BRGlobals, Optional ByVal forceRefresh As Boolean = False)
			Dim scriptGenerics As String = globals.GetStringValue("scriptGenerics")
			Dim parAccount As String = globals.GetStringValue("parAccount")
			Dim dims As List(Of String) = StringHelper.SplitString(scriptGenerics, ":")
			Dim entity As String = dims(0).Replace("E#", "")
			Dim scenario As String = dims(1).Replace("S#", "")
			Dim time As String = dims(2).Replace("T#", "")
			Dim flow As String = dims(4).Replace("F#", "")
			Dim ud6 As String = dims(12).Replace("U6#", "")
			Dim ud8 As String = dims(14).Replace("U8#", "")

			Dim key As String = String.Format("attr_{0}_{1}_{2}_{3}_{4}_{5}", parAccount, entity, flow, ud6, scenario, time)
			Dim pov As DateTime = GetPovLastEdited(si, entity, scenario, time, flow, If(ud6.XFEqualsIgnoreCase("None"), "none", ud6))

			Dim cachedKey As String = GetCache(si, "key")
			Dim cachedStmp As String = GetCache(si, "stamp")
			Dim cachedJson As String = GetCache(si, "dict")
			Dim fresh As Boolean = (Not forceRefresh) AndAlso cachedKey = key AndAlso Not String.IsNullOrEmpty(cachedStmp) AndAlso pov <= DateTime.Parse(cachedStmp, CultureInfo.InvariantCulture)
			If fresh AndAlso Not String.IsNullOrEmpty(cachedJson) Then
				globals.SetObject("attributeDict", JsonConvert.DeserializeObject(Of Dictionary(Of String, String))(cachedJson))
				Return
			End If

			Dim dict As Dictionary(Of String, String) = LoadAttributesByParent(si, parAccount, entity, scenario, time, flow, ud6, ud8)
			SetCache(si, "key", key) : SetCache(si, "stamp", pov.ToString("o", CultureInfo.InvariantCulture)) : SetCache(si, "dict", JsonConvert.SerializeObject(dict))
			globals.SetObject("attributeDict", dict)
		End Sub

		Public Function GetPovLastEdited(ByVal si As SessionInfo, ByVal entity As String, ByVal scenario As String, ByVal time As String, ByVal flow As String, ByVal liNumber As String) As DateTime
			Dim sql As String = "SELECT MAX(LastEditedTimestamp) S FROM dbo.DataAttachment WHERE Cube='BudFm' AND Scenario='" & scenario & "' AND Entity='" & entity & "' AND [Time]='" & time & "' AND Flow='" & flow & "'"
			If Not String.IsNullOrEmpty(liNumber) AndAlso Not liNumber.XFEqualsIgnoreCase("none") Then sql &= " AND UD6='" & liNumber & "'"
			Using db As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				Dim dt As DataTable = BRApi.Database.ExecuteSql(db, sql, True)
				If dt.Rows.Count > 0 AndAlso Not IsDBNull(dt.Rows(0)("S")) Then Return Convert.ToDateTime(dt.Rows(0)("S"))
			End Using
			Return DateTime.MinValue
		End Function

		Public Function GetRPEntity(ByVal si As SessionInfo, ByVal rpNumber As String) As String
			' Delegates to the ported RP utilities (Get_RP_Entity walks the RP long
			' name: RP_FY_2025_DCMS_OS_3_4010_00 -> LO_DCMS).
			Dim rpUtils As New BUDFM_RP_Utilities
			Return rpUtils.Get_RP_Entity(si, rpNumber)
		End Function

		Private Sub SetCache(ByVal si As SessionInfo, ByVal k As String, ByVal v As String)
			BRApi.State.SetSessionState(si, False, ClientModuleType.Unknown, "", "", "RPAttrCache", k, v, si.XfBytes)
		End Sub
		Private Function GetCache(ByVal si As SessionInfo, ByVal k As String) As String
			Dim st As XFUserState = BRApi.State.GetSessionState(si, False, ClientModuleType.Unknown, "", "", "RPAttrCache", k)
			If st Is Nothing Then Return String.Empty
			Return st.TextValue
		End Function

		' Account -> substitution-var param (full RPAttributeParamMap, 75 entries)
		Public ReadOnly ParamMap As New Dictionary(Of String, String) From {
			{"Number_of_Billets", "prm_BLT_NumberOfBillets_OS"},
			{"Add_General_Detail", "prm_BLT_AutoAddGenDetail_OS"},
			{"Increase_Decrease", "prm_BLT_IncreaseDecrease_OS"},
			{"Part_of_Reprogramming", "prm_BLT_PartOfReprogramming_OS"},
			{"Personnel_Qtrs", "prm_BLT_NumberOfPersonnelQtrs_OS"},
			{"OS_Qtrs", "prm_NBLT_NumberOfOSQtrs_OS"},
			{"Lead_Office1", "prm_LeadOffice1_OS"},
			{"Lead_Office2", "prm_LeadOffice2_OS"},
			{"Lead_Office3", "prm_LeadOffice3_OS"},
			{"Lead_Office_POC1", "prm_LeadOfficePOC1_OS"},
			{"Lead_Office_POC2", "prm_LeadOfficePOC2_OS"},
			{"Lead_Office_POC3", "prm_LeadOfficePOC3_OS"},
			{"Lead_Office_Phone1", "prm_LeadOfficePhone1_OS"},
			{"Lead_Office_Phone2", "prm_LeadOfficePhone2_OS"},
			{"Lead_Office_Phone3", "prm_LeadOfficePhone3_OS"},
			{"Exec_Summary", "prm_ExecSummary_OS"},
			{"Initial_Estimate", "prm_BLT_IE_K_OS"},
			{"Initial_Estimate_MIL_FTP", "prm_BLT_IE_MIL_OS"},
			{"Initial_Estimate_CIV_FTP", "prm_BLT_IE_CIV_OS"},
			{"Base_Funding", "prm_BLT_IE_Base_Funding_OS"},
			{"Base_Funding_MIL_FTP", "prm_BLT_CBF_MIL_OS"},
			{"Base_Funding_CIV_FTP", "prm_BLT_CBF_CIV_OS"},
			{"Base_Funding_Comments", "prm_IE_Base_Funding_Comments_OS"},
			{"Recurring_Base_Estimate", "prm_BLT_IE_R_Base_OS"},
			{"Recurring_Base_Comments", "prm_BLT_R_Base_Comments_OS"},
			{"FY_Related_RP1", "prm_FYRelatedRp1_OS"},
			{"FY_Related_RP2", "prm_FYRelatedRp2_OS"},
			{"FY_Related_RP3", "prm_FYRelatedRp3_OS"},
			{"Older_Related_RP1", "prm_OlderRelatedRp1_OS"},
			{"Older_Related_RP2", "prm_OlderRelatedRp2_OS"},
			{"Older_Related_RP3", "prm_OlderRelatedRp3_OS"},
			{"Affect_Others", "prm_Page3_AffectOthers_OS"},
			{"Alignment", "prm_Page3_Alignment_OS"},
			{"Denial_Impact", "prm_Page3_DenialImpact_OS"},
			{"Funding_Impact", "prm_Page3_FundingImpact_OS"},
			{"Problem", "prm_Page3_Problem_OS"},
			{"ROI", "prm_Page3_ROI_OS"},
			{"Billet_Type", "prm_BLT_BilletType_OS"},
			{"Grade_Type", "prm_BLT_GradeType_OS"},
			{"Grade_Rank", "prm_BLT_GradeRank_OS"},
			{"AD_Reserve", "prm_BLT_ADReserve_OS"},
			{"Reserve_Type", "prm_BLT_ReserveType_OS"},
			{"Spe_Code_Occu_Series", "prm_BLT_SpcCodeOccSeries_OS"},
			{"Pilot", "prm_BLT_Pilot_OS"},
			{"Electronic_Flight_Bag", "prm_BLT_ElectronicFlightBag_OS"},
			{"Position_Number", "prm_BLT_PositionNumber_OS"},
			{"Position_Title", "prm_BLT_PositionTitle_OS"},
			{"Billet_ATU", "prm_BLT_ATU_OS"},
			{"OPFAC", "prm_BLT_OPFACS_OS"},
			{"Billet_UII", "prm_BLT_UII_OS"},
			{"CONUS_OCONUS", "prm_BLT_ConusOConus_OS"},
			{"Detached_Duty", "prm_BLT_DetachedDuty_OS"},
			{"Detached_Duty_Location", "prm_BLT_DutyLocation_OS"},
			{"Term_Billet", "prm_BLT_TermBillet_OS"},
			{"PPE_Type", "prm_BLT_PPEType_OS"},
			{"PPE_PPA", "prm_BLT_PPE_PPA_OS"},
			{"Build_Out_Choice", "prm_BLT_Build_Out_OS"},
			{"ICASS_Costs", "prm_BLT_ICASSType_OS"},
			{"Background_Investigation_Type", "prm_BLT_BIType_OS"},
			{"Acquisition_Project", "prm_BLT_Acq_Project_OS"},
			{"Lease_Choice", "prm_BLT_Lease_OS"},
			{"Lease_PPA", "prm_BLT_Lease_PPA_OS"},
			{"Furniture_Reqd", "prm_BLT_Furniture_OS"},
			{"Utilities_Reqd", "prm_BLT_Utilities_OS"},
			{"Computer_Type", "prm_BLT_Computer_Type_OS"},
			{"LineItem_Comment", "prm_BLT_Comment_OS"},
			{"Utilities_PPA", "prm_BLT_UTL_PPA_OS"},
			{"Requested_Item_Tier1", "prm_NBLT_RequestedItem_Tier1_OS"},
			{"Description_Tier2", "prm_NBLT_Description_Tier2_OS"},
			{"POC", "prm_NBLT_POC_OS"},
			{"DollarK_Value", "prm_NBLT_DollarKValue_OS"},
			{"R_NR", "prm_NBLT_RecurringNonRecurring_OS"},
			{"PPA", "prm_NBLT_PPA_OS"},
			{"UII", "prm_NBLT_UII_OS"},
			{"Object_Class", "prm_NBLT_ObjectClass_OS"}
		}
	End Module
End Namespace
