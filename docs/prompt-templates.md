# OneStream Workspace XML — Prompt Templates

This document defines the prompt vocabulary and XML skeletons for building OneStream
workspace DB objects. Use these templates with GitHub Copilot (or any LLM) to generate
valid XML fragments, or feed them directly to the CLI in `tools/xml-generators/`.

---

## Naming Conventions

All object names must follow the prefix conventions enforced by the generator:

| Prefix | Object type | Example |
|--------|-------------|---------|
| `BL_` | BoundList parameter | `BL_DDM_App_Menu` |
| `IV_` | InputValue parameter | `IV_DDM_App_SelectedID` |
| `ML_` | MemberList parameter | `ML_DDM_App_EntityList` |
| `LV_` | LiteralValue parameter | `LV_Std_btn_Format` |
| `DL_` | DelimitedList parameter | `DL_FMM_SetupOptions` |
| `btn_` | Button component | `btn_MY_Save` |
| `cbx_` | ComboBox component | `cbx_MY_MbrList` |
| `lbx_` | ListBox component | `lbx_MY_Menu` |
| `cv_` | CubeView component | `cv_MY_MainView` |
| `lbl_` | Label component | `lbl_MY_Header` |
| `Embedded ` | EmbeddedDashboard component | `Embedded MY_Content_DB` |

### Module prefixes

| Module | Prefix |
|--------|--------|
| App Objects (Globals) | `GBL_` |
| Dynamic Dashboard Manager | `DDM_` |
| Dynamic Dashboard Manager (Admin) | `DDM_Admin_` |
| Finance Model Manager | `FMM_` |
| Finance Model Manager (Admin) | `FMM_Admin_` |
| New custom module | Choose a unique 2–5 char abbreviation |

---

## Phase 1 — Parameters

### BoundList parameter (dropdown populated by a Business Rule)

**Prompt**: "Create a BoundList parameter for `[MODULE]` that calls `[AssemblyName].[DataSetsClass]` method `[MethodName]`, stores results in `[TableName]`, displays `[DisplayColumn]`, values from `[ValueColumn]`."

**XML skeleton**:
```xml
<parameter name="BL_[MODULE]_[Name]" description="[description]" userPrompt=""
           parameterType="BoundList" sortOrder="100">
    <defaultValue></defaultValue>
    <resultFormatStringType>Default</resultFormatStringType>
    <resultCustomFormatString></resultCustomFormatString>
    <parameterCommandType>Method</parameterCommandType>
    <sqlQuery></sqlQuery>
    <dbLocation>Application</dbLocation>
    <externalDBConnName></externalDBConnName>
    <methodType>BusinessRule</methodType>
    <methodQuery>{Workspace.Current.[AssemblyName].[DataSetsClass]}{[MethodName]}{}</methodQuery>
    <resultsTableName>[TableName]</resultsTableName>
    <displayMember>[DisplayColumn]</displayMember>
    <valueMember>[ValueColumn]</valueMember>
    <displayItems></displayItems>
    <valueItems></valueItems>
    <cubeName></cubeName>
    <dimType></dimType>
    <dimName></dimName>
    <memberFilter></memberFilter>
</parameter>
```

**CLI**:
```bash
python generate.py parameter \
  --type BoundList \
  --name BL_[MODULE]_[Name] \
  --command-type Method --method-type BusinessRule \
  --method-query "{Workspace.Current.[AssemblyName].[DataSetsClass]}{[MethodName]}{}" \
  --results-table [TableName] \
  --display-member [DisplayColumn] --value-member [ValueColumn]
```

---

### InputValue parameter (stores a single selected or computed value)

**Prompt**: "Create an InputValue parameter for `[MODULE]` named `[Name]` with default value `[DefaultValue]`."

**XML skeleton**:
```xml
<parameter name="IV_[MODULE]_[Name]" description="[description]" userPrompt=""
           parameterType="InputValue" sortOrder="100">
    <defaultValue>[DefaultValue]</defaultValue>
    <resultFormatStringType>Default</resultFormatStringType>
    <resultCustomFormatString></resultCustomFormatString>
    <parameterCommandType>Unknown</parameterCommandType>
    <sqlQuery></sqlQuery>
    <dbLocation>Application</dbLocation>
    <externalDBConnName></externalDBConnName>
    <methodType>Unknown</methodType>
    <methodQuery></methodQuery>
    <resultsTableName></resultsTableName>
    <displayMember></displayMember>
    <valueMember></valueMember>
    <displayItems></displayItems>
    <valueItems></valueItems>
    <cubeName></cubeName>
    <dimType></dimType>
    <dimName></dimName>
    <memberFilter></memberFilter>
</parameter>
```

**CLI**:
```bash
python generate.py parameter --type InputValue --name IV_[MODULE]_[Name] --default-value "[DefaultValue]"
```

---

### MemberList parameter (dimension member picker)

**Prompt**: "Create a MemberList parameter for `[MODULE]` on dimension type `[DimType]` (e.g. Entity, Account, Scenario, Time, View, Consolidation, UD1–UD8)."

**XML skeleton**:
```xml
<parameter name="ML_[MODULE]_[DimType]MbrList" description="" userPrompt=""
           parameterType="MemberList" sortOrder="100">
    <defaultValue>~!Mbr_List_Default!~</defaultValue>
    <resultFormatStringType>Default</resultFormatStringType>
    <resultCustomFormatString></resultCustomFormatString>
    <parameterCommandType>Unknown</parameterCommandType>
    <sqlQuery></sqlQuery>
    <dbLocation>Application</dbLocation>
    <externalDBConnName></externalDBConnName>
    <methodType>Unknown</methodType>
    <methodQuery></methodQuery>
    <resultsTableName></resultsTableName>
    <displayMember></displayMember>
    <valueMember></valueMember>
    <displayItems></displayItems>
    <valueItems></valueItems>
    <cubeName>~!Mbr_List_Cube!~</cubeName>
    <dimType>[DimType]</dimType>
    <dimName>~!Mbr_List_Dim!~</dimName>
    <memberFilter>~!Mbr_List_Filter!~</memberFilter>
</parameter>
```

Valid `dimType` values: `Entity`, `Account`, `Scenario`, `Time`, `View`,
`Consolidation`, `IC`, `Flow`, `Origin`, `UD1`–`UD8`.

**CLI**:
```bash
python generate.py parameter --type MemberList --name ML_[MODULE]_[DimType]MbrList \
  --dim-type [DimType] \
  --cube-name "~!Mbr_List_Cube!~" --dim-name "~!Mbr_List_Dim!~" \
  --member-filter "~!Mbr_List_Filter!~" --default-value "~!Mbr_List_Default!~"
```

---

### LiteralValue parameter (static format or configuration string)

**Prompt**: "Create a LiteralValue parameter for global button formatting named `LV_[MODULE]_[Name]`."

```xml
<parameter name="LV_[MODULE]_[Name]" description="" userPrompt=""
           parameterType="LiteralValue" sortOrder="0">
    <defaultValue>[FormatString]</defaultValue>
    <!-- all other child elements empty / Unknown -->
</parameter>
```

---

### DelimitedList parameter (fixed list of display/value pairs)

**Prompt**: "Create a DelimitedList parameter for `[MODULE]` with options `[DisplayItems]` mapped to values `[ValueItems]`."

```xml
<parameter name="DL_[MODULE]_[Name]" description="[description]" userPrompt=""
           parameterType="DelimitedList" sortOrder="100">
    <defaultValue>[FirstValue]</defaultValue>
    <displayItems>[DisplayItem1],[DisplayItem2],[DisplayItem3]</displayItems>
    <valueItems>[Value1],[Value2],[Value3]</valueItems>
    <!-- all other child elements empty / Unknown -->
</parameter>
```

---

## Phase 2 — Components

### Button component

**Prompt**: "Add a `[Standard|SelectMemberDialog]` button named `btn_[MODULE]_[Action]` that calls Business Rule `[BRMethod]` and redraws `[Dashboard]`."

**XML skeleton (Standard)**:
```xml
<component name="btn_[MODULE]_[Action]" description="" xfText="[ButtonLabel]"
           toolTip="[Tooltip]" componentType="Button" templateParameterValues=""
           brText1="" brText2="" boundParameterName="" paramValueForButtonClick=""
           applyParamValueToCurrentDbrd="true"
           selectionChangedSaveType="NoAction"
           selectionChangedPovActionType="NoAction"
           selectionChangedTaskType="ExecuteDashboardExtenderBRConsServer"
           selectionChangedUIActionType="Refresh"
           selectionChangedNavigationType="NoAction">
    <displayFormat>|!LV_Std_btn_Format!|</displayFormat>
    <selectionChangedTaskArgs>{Workspace.Current.[AssemblyName].[BRClass]}{[BRMethod]}{}</selectionChangedTaskArgs>
    <dashboardsToRedraw>[Dashboard]</dashboardsToRedraw>
    <dashboardForDialog></dashboardForDialog>
    <componentDefinition>
        <XFButtonDefinition ButtonType="Standard">
            <ImageFileSourceType>DashboardFile</ImageFileSourceType>
            <ImageUrlOrFullFileName>[ImageFile.png]</ImageUrlOrFullFileName>
            <PageNumber />
            <ExcelSheet />
            <ExcelNamedRange />
        </XFButtonDefinition>
    </componentDefinition>
    <adapterMembers />
</component>
```

**Common button images** (from file resources):
`Std_DB_Add.png`, `Std_DB_Save.png`, `Std_DB_Cancel.png`, `Std_DB_Calc.png`,
`Std_DB_Copy.png`, `Std_DB_Search.png`, `Std_DB_Aggregate.png`

---

### ListBox component (navigation menu)

**Prompt**: "Add a ListBox navigation menu named `lbx_[MODULE]_Menu` bound to `BL_[MODULE]_Menu`."

```xml
<component name="lbx_[MODULE]_Menu" description="" xfText="[MenuLabel]" toolTip=""
           componentType="ListBox" templateParameterValues="" brText1="" brText2=""
           boundParameterName="BL_[MODULE]_Menu" applyParamValueToCurrentDbrd="true"
           selectionChangedSaveType="NoAction"
           selectionChangedPovActionType="NoAction"
           selectionChangedTaskType="NoTask"
           selectionChangedUIActionType="Refresh"
           selectionChangedNavigationType="NoAction">
    <displayFormat>|!LV_Std_lbx_Format!|</displayFormat>
    <adapterMembers />
</component>
```

---

### ComboBox component

**Prompt**: "Add a ComboBox for `[MODULE]` bound to parameter `BL_[MODULE]_[ListName]`."

```xml
<component name="cbx_[MODULE]_[ListName]" description="" xfText="" toolTip=""
           componentType="ComboBox" templateParameterValues="" brText1="" brText2=""
           boundParameterName="[BL_or_ML_paramName]" applyParamValueToCurrentDbrd="true"
           selectionChangedSaveType="NoAction"
           selectionChangedPovActionType="NoAction"
           selectionChangedTaskType="NoTask"
           selectionChangedUIActionType="NoAction"
           selectionChangedNavigationType="NoAction">
    <displayFormat>|!LV_Std_cbx_Format!|</displayFormat>
    <adapterMembers />
</component>
```

---

### CubeView component

**Prompt**: "Add a CubeView component named `cv_[MODULE]_[Name]` pointing to cube view `[CubeViewName]`."

```xml
<component name="cv_[MODULE]_[Name]" description="" xfText="" toolTip=""
           componentType="CubeView" templateParameterValues="" brText1="" brText2=""
           boundParameterName="" applyParamValueToCurrentDbrd="true"
           selectionChangedSaveType="NoAction"
           selectionChangedPovActionType="NoAction"
           selectionChangedTaskType="NoTask"
           selectionChangedUIActionType="NoAction"
           selectionChangedNavigationType="NoAction">
    <componentDefinition>
        <XFCubeViewDefinition>
            <CubeViewName>[CubeViewName]</CubeViewName>
            <ShowHeader>true</ShowHeader>
            <ShowToggleSizeButton>false</ShowToggleSizeButton>
        </XFCubeViewDefinition>
    </componentDefinition>
    <adapterMembers />
</component>
```

---

### EmbeddedDashboard component

**Prompt**: "Add an EmbeddedDashboard component that embeds `[DashboardName]`."

```xml
<component name="Embedded [DashboardName]" description="" xfText="" toolTip=""
           componentType="EmbeddedDashboard"
           embeddedDashboardName="[DashboardName]" templateNameSuffix=""
           templateParameterValues="" brText1="" brText2="">
    <componentDefinition />
    <adapterMembers />
</component>
```

---

## Phase 3 — Dashboards

### Dashboard types

| `dashboardType` | Use case |
|-----------------|----------|
| `TopLevel` | Main entry-point dashboard shown in the OneStream UI |
| `Embedded` | Static embedded panel inside a TopLevel dashboard |
| `EmbeddedDynamic` | Embedded panel that can be shown/hidden dynamically |
| `EmbeddedTopLevelWithoutParameterPrompts` | Embedded, behaves like TopLevel but no parameter bar |

### Layout types

| `layoutType` | Use case |
|--------------|----------|
| `Grid` | Custom column/row sizing with `*`, `Auto`, or pixel values |
| `Uniform` | Equal-sized cells |
| `VerticalStackPanel` | Stacked vertically |

### TopLevel dashboard with header + content

**Prompt**: "Create a TopLevel dashboard named `[MODULE]_TopLevel_DB` that loads via BR `[AssemblyName].[BRClass]` method `[LoadMethod]`, with a header row (Auto height) and content row (800px)."

```xml
<dashboard name="[MODULE]_TopLevel_DB" description="" pageCaption=""
           dashboardType="TopLevel" layoutType="Grid"
           isInitiallyVisible="true"
           loadDashboardTaskType="ExecuteDashboardExtenderBRAllActions">
    <loadDashboardTaskArgs>{Workspace.Current.[AssemblyName].[BRClass]}{[LoadMethod]}{}</loadDashboardTaskArgs>
    <DashboardDefinition>
        <ShowTitle>true</ShowTitle>
        <Notes />
        <DynamicDashboardDefinition>
            <ComponentTemplateRepeatItems />
        </DynamicDashboardDefinition>
        <CustomControlDbrdDefinition RequiredInputParameters="" />
        <GridLayoutDefinition>
            <ColumnDefinitions>
                <ColumnDefinition ColumnType="Component" Width="*" />
            </ColumnDefinitions>
            <RowDefinitions>
                <RowDefinition RowType="Component" Height="Auto" />
                <RowDefinition RowType="Component" Height="800" />
            </RowDefinitions>
        </GridLayoutDefinition>
        <ScrollPosition>Default</ScrollPosition>
    </DashboardDefinition>
    <componentMembers>
        <componentMember name="Embedded [MODULE]_Hdr_DB" left="" top="" width="" height="" dockPosition="Left" />
        <componentMember name="Embedded [MODULE]_Content_DB" left="" top="" width="" height="" dockPosition="Left" />
    </componentMembers>
</dashboard>
```

---

### Embedded 2-column content dashboard

**Prompt**: "Create an Embedded dashboard named `[MODULE]_Content_DB` with a left nav column (Auto) and a right content column (*)."

```xml
<dashboard name="[MODULE]_Content_DB" description="" pageCaption=""
           dashboardType="Embedded" layoutType="Grid"
           isInitiallyVisible="true" loadDashboardTaskType="NoTask">
    <DashboardDefinition>
        <ShowTitle>true</ShowTitle>
        <Notes />
        <DynamicDashboardDefinition>
            <ComponentTemplateRepeatItems />
        </DynamicDashboardDefinition>
        <CustomControlDbrdDefinition RequiredInputParameters="" />
        <GridLayoutDefinition>
            <ColumnDefinitions>
                <ColumnDefinition ColumnType="Component" Width="Auto" />
                <ColumnDefinition ColumnType="Component" Width="*" />
            </ColumnDefinitions>
            <RowDefinitions>
                <RowDefinition RowType="Component" Height="*" />
            </RowDefinitions>
        </GridLayoutDefinition>
        <ScrollPosition>Default</ScrollPosition>
    </DashboardDefinition>
    <componentMembers>
        <componentMember name="lbx_[MODULE]_Menu" left="" top="" width="" height="" dockPosition="Left" />
        <componentMember name="cv_[MODULE]_MainView" left="" top="" width="" height="" dockPosition="Left" />
    </componentMembers>
</dashboard>
```

---

## Phase 4 — Business Rules / Workspace Assemblies

### Workspace assembly with one C# file

**Prompt**: "Create a workspace assembly named `[MODULE]_Assembly` in CSharp with a folder `[FolderName]` containing file `[FileName].cs` set to Compile."

```xml
<workspaceAssembly name="[MODULE]_Assembly" description="" compilerLanguage="CSharp">
    <dependencies />
    <files>
        <folder name="[FolderName]">
            <file name="[FileName].cs" fileType="CSharp"
                  businessRuleType="Unknown" compilerActionType="Compile"
                  isEncrypted="false">
                <sourceCode><![CDATA[using System;
using OneStreamWorkspacesApi;
using OneStreamWorkspacesApi.V800;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    public class [ClassName]
    {
        // TODO: implement
    }
}]]></sourceCode>
            </file>
        </folder>
    </files>
</workspaceAssembly>
```

**`compilerActionType` values**:
- `Unknown` — helper/support class, not a direct BR entry point
- `Compile` — SQL adapter or business rule entry point
- `CompileAndRun` — executed at compile time

**Namespace pattern** (required for all C# files):
```csharp
namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
```
OneStream substitutes `__WsNamespacePrefix` and `__WsAssemblyName` at runtime.

---

## Phase 5 — Composing a Full Module

### Outer workspace XML wrapper (required for OneStream import)

Every importable workspace XML must have this exact structure:

```xml
<?xml version="1.0" encoding="utf-8"?>
<OneStreamXF version="9.3.0.18429">
    <applicationWorkspacesRoot>
        <workspaces>
            <workspace name="OS Consultant Tools" description="" notes=""
                       accessGroup="Everyone" maintenanceGroup="Everyone"
                       isShareableWorkspace="true" sharedWorkspaceNames=""
                       companyName="" versionNum="" author=""
                       namespacePrefix="OSConsTools"
                       importsNamespace1="" importsNamespace2=""
                       importsNamespace3="" importsNamespace4=""
                       importsNamespace5="" importsNamespace6=""
                       importsNamespace7="" importsNamespace8=""
                       wsAssemblyService=""
                       text1="" text2="" text3="" text4=""
                       text5="" text6="" text7="" text8="">
                <WorkspaceDefinition>
                    <SubstitutionVariableItems />
                </WorkspaceDefinition>
                <maintenanceUnits>
                    <!-- One or more <maintenanceUnit> blocks go here -->
                </maintenanceUnits>
            </workspace>
        </workspaces>
    </applicationWorkspacesRoot>
</OneStreamXF>
```

### maintenanceUnit structure

```xml
<maintenanceUnit name="[Module Name]" description=""
                 accessGroup="Everyone" maintenanceGroup="Everyone"
                 wsAssemblyService="[MODULE]_Assembly.[MODULE]_SvcFactory"
                 category="">
    <fileResources />
    <stringResources />
    <parameters>
        <!-- BL_, IV_, ML_, LV_, DL_ parameters -->
    </parameters>
    <adapters />
    <components>
        <!-- btn_, cbx_, lbx_, cv_, lbl_, Embedded components -->
    </components>
    <workspaceAssemblies>
        <!-- <workspaceAssembly> blocks -->
    </workspaceAssemblies>
    <dashboardGroups>
        <!-- <dashboardGroup> blocks -->
    </dashboardGroups>
</maintenanceUnit>
```

---

## Prompt Examples for GitHub Copilot

Use these prompts in a Copilot chat or inline with the generator:

### Generate a navigation module from scratch
```
Create a OneStream maintenanceUnit named "Report Viewer" with module prefix RPT_.
Include:
- A BoundList parameter BL_RPT_ReportList backed by RPT_Assembly.RPT_DataSets.Get_Reports
  storing results in RPT_ReportConfig, display=Name, value=ReportID
- An InputValue parameter IV_RPT_SelectedReportID
- A TopLevel dashboard RPT_TopLevel_DB with a header row (Auto) and content row (*)
- An Embedded dashboard RPT_Content_DB with left nav (Auto) and right content (*)
- A ListBox lbx_RPT_Menu bound to BL_RPT_ReportList
- A CubeView cv_RPT_MainView pointing to RPT_MainView
- A workspace assembly RPT_Assembly with file RPT_DataSets.cs set to Compile
```

### Add a save button to an existing module
```
Generate a Standard button component btn_FMM_Save for module FMM_ that:
- Displays "Save" with image Std_DB_Save.png
- Calls {Workspace.Current.FMM_Assembly.FMM_BR}{Save}{} on the ConsServer
- Redraws dashboard FMM_Content_DB
- Uses format |!LV_Std_btn_Format!|
```

### Add a member picker for the Entity dimension
```
Generate a MemberList parameter ML_DDM_App_EntityList for module DDM_
bound to dimension type Entity.
```
