# CLAUDE.md — OneStream Workspace Conventions (OS Consultant Tools)

This file encodes the structure, naming conventions, and best practices used by the DDM (Dynamic Dashboard Manager) and FMM (Finance Model Manager) tools in this repository. It is written to be portable: replace the solution prefix (`DDM`, `FMM`, `GBL`) with your own and the same rules apply to any OneStream workspace solution.

For the full object-by-object inventory of the existing dashboards, see `docs/DASHBOARD_MAP.md`.

## What this repository is

OneStream workspace source (XML exports + extracted C# business rules) plus a .NET compile harness:

```
OSConsTools/
├── OSConsTools.csproj            # net48 compile project: OS Consultant Tools/**/Assemblies/**/*.cs
├── lib/OneStream/                # OneStream platform DLLs (HintPath refs, not committed to build)
├── OS Consultant Tools/          # the workspace — one folder per maintenance unit
│   ├── App Objects (Globals)/    # shared icons, LV_Std_* format literals, GBL_UI_Assembly helpers
│   ├── Dynamic Dashboard Manager/           # DDM user/runtime unit
│   ├── Dynamic Dashboard Manager (Admin)/   # DDM admin/config unit
│   ├── Finance Model Manager/               # FMM runtime unit (code only, no dashboards)
│   ├── Finance Model Manager (Admin)/       # FMM admin/config unit
│   └── USCG/                     # client-specific VB solution (reference; not compiled)
├── RMW/                          # legacy hardcoded VB rules (what FMM's config-driven design replaces)
├── docs/                         # DLL setup, DASHBOARD_MAP.md, reference implementations
│   └── reference/                # non-compiled reference patterns (header rebuild guard, FMM DDL, cube-load engine, STE patterns)
├── scripts/                      # utility scripts
└── tools/OSConsTools.McpServer/  # stdio MCP server: OneStream API index + repo example search
```

Build check: `dotnet build OSConsTools.csproj` (requires the DLLs listed in `docs/OneStream-DLL-setup.md` in `lib/OneStream/`). Reference code under `docs/reference/` and `.vb` files are intentionally outside the compile glob.

## Solution architecture: the three-unit pattern

Every tool is built as a **config-driven engine** split across maintenance units inside one workspace:

1. **`<Sol>` (user/runtime unit)** — what end users open. Renders dashboards or runs calculations by *reading* the solution's config tables. Assembly: `<Sol>_UI_Assembly`.
2. **`<Sol> (Admin)` (config unit)** — a OnePlace dashboard console admins use to *write* those config tables. No recompile needed to change behavior. Assembly: `<Sol>_ConfigUI_Assembly`.
3. **`App Objects (Globals)`** — shared across all solutions: standard icons (`Std_DB_*.png`), standard format-string literals (`LV_Std_*`), and the `GBL_UI_Assembly` data-access layer.

**The SQL config tables are the contract between admin and runtime.** Admin saves rows; runtime reads rows. Neither side calls the other's assembly at runtime (though they may share enums/registries at compile time via assembly dependencies).

## Maintenance unit anatomy (on disk)

```
<Unit Name>/
├── XML/<UnitExportName>.xml        # OneStream export — source of truth for dashboards,
│                                   #   components, parameters, adapters, file resources
└── Assemblies/<X>_Assembly/        # C# source, byte-parallel to the XML <files> tree
    ├── DB DataSets/                # DashboardDataSet rules (read queries)
    ├── DB Extenders/               # DashboardExtender rules (load logic + saves)
    ├── Dyn DB Components/          # dynamic-dashboard component builders, repositories, POCOs
    ├── Dyn DB Support/             # static resolver/support helpers
    ├── Helper Classes/             # registries, enums, shared helpers
    ├── Factory/                    # <Sol>_SvcFactory.cs (IWsAssemblyServiceFactory)
    │   └── Services/               # IWsas* service implementations
    ├── XFBRs/                      # DashboardStringFunction rules
    └── SQL Adapters/               # (Globals only) SQL data-access classes
```

Rules:
- The XML `<folder name>` / `<file name>` tree must match the on-disk `Assemblies/` tree exactly. When you edit a `.cs` file, the same source must be updated inside the XML's `<sourceCode><![CDATA[...]]>` block before import (they are the same code in two places).
- One maintenance unit per XML export; export root is `<OneStreamXF version="..."><applicationWorkspacesRoot>`.
- Workspace-level settings that matter: `namespacePrefix` (becomes the compiled namespace), `sharedWorkspaceNames` (which application workspaces can see this tool), `wsAssemblyService="<Assembly>.<Sol>_SvcFactory"` on the maintenance unit (registers the service factory).

## Naming conventions

### Solution prefix

Every artifact belonging to a solution carries its prefix: files, classes, tables, dashboards, parameters, components, datasets, adapters. `DDM_*`, `FMM_*`, `GBL_*`. Pick a 3-4 letter prefix for a new solution and never mix prefixes.

Sub-domain infixes narrow the area: `<Sol>_App_*` (user-side runtime objects), `<Sol>_Config*` / `<Sol>_Admin*` (admin side), then feature areas (`_Hdr_`, `_MenuLayout_`, `_CubeConfig_`, `_ModelGrp_`, `_Fltr_`, `_Btn_`).

### Parameters

| Prefix | parameterType | Use |
|---|---|---|
| `BL_` | BoundList | Dynamic dropdown/tree fed by a DataSet BR or SQL; holds the *selected* value |
| `IV_` | InputValue | Free-form/working value; the editable copy of a selection; mode flags |
| `DL_` | DelimitedList | Static option list, usually mirroring a C# enum |
| `ML_` | MemberList | Dimension member picker (one per dim, templated via `~!...!~` tokens) |
| `LV_` | LiteralValue | Shared constants — e.g. the `LV_Std_<comp>_Format` control format strings in Globals |

**The `BL_` ↔ `IV_` pairing pattern:** a `BL_` selection is mirrored into a same-named `IV_` param (`BL_DDM_ConfigID` → `IV_DDM_ConfigID`) so SQL Table Editors and save logic work off a stable input value. Load extenders resolve defaults by swapping the prefix (`param.Replace("IV_","BL_")`). Keep the names identical apart from the prefix.

Common suffixes: `_AddUpdate` (dialog mode flag), `ID` (surrogate-key holders), `_SortOrder`, `_Name`, `_Status`, `_ToolTip`, `_ContentType`, audit quartet `_CreateDate/_CreateUser/_UpdateDate/_UpdateUser`.

### Components

| Prefix | Component type | | Prefix | Component type |
|---|---|---|---|---|
| `btn_` | Button | | `lbx_` | ListBox |
| `cbx_` | ComboBox | | `sp_` | SuppliedParameter |
| `chk_` | CheckBox | | `ted_` | SqlTableEditor |
| `cv_` | CubeView | | `trv_` | TreeView |
| `gv_` | GridView | | `txt_` | TextBox |
| `Img_` | Image | | `SS_` | XFSpreadsheet |
| `lbl_` | Label | | `Embedded <DB name>` / `emb_` | EmbeddedDashboard |

- Name pattern: `<prefix>_<Sol>_<Area>_<Action/Field>` — e.g. `btn_FMM_AcctConfig_Add`, `cbx_DDM_Hdr_Fltr_DimType`, `ted_DDM_DynDBConfig`.
- Embedded-dashboard wrappers are named `Embedded <exact dashboard name>` — one wrapper per dashboard.
- `sp_` supplied parameters are named after the parameter they surface: `sp_BL_DDM_AppMenu`.
- Buttons that route to an extender set `boundParameterName` (usually an `IV_*_AddUpdate` mode param), `paramValueForButtonClick` (`Add`/`Update`/`Delete`), and `selectionChangedTaskType=ExecuteDashboardExtenderBusinessRule`.
- Data adapters: `DA_<Sol>_<Area>` (commandType SQL for direct table reads, Method for DataSet BR calls). Prefer BoundList `methodQuery` bindings for combos; reserve named adapters for TreeViews/grids.

### Dashboards

Two families with distinct grammars:

**Runtime layout shells (user side):** `<Sol>_App_<Part>_<LayoutCode>[_DB]`
- Layout codes describe pane geometry: `L/R/T/B` (single pane), `TL/TR/BL/BR` (quadrant), `2L/2R/2T/2B` (stacked pair), `LR/TB` (split), `1L2R/2L1R/1T2B/2T1B` (three-pane), `2x2` (four-pane), `CV` (cube-view shell), `CustomDB`.
- `Hdr` = header family; `_C#` = column, chained (`_Hdr_C2C1`).
- Entry point is a human-readable TopLevel name (`DDM Dynamic App Dashboard`), everything else is machine-composed.

**Admin config trees:** `<Sol>_<Section>` root + nested-pane coordinates
- `_C#` (column) and `_R#` (row) suffixes, chained arbitrarily deep: `DDM_ConfigWFP_C2C2R2`, `FMM_CalcConfig_Cube_R1R2C2R1C3`.
- State variants: `_Blank` (nothing selected) vs `_AddUpdate` (form populated); `_Add`/`_Update`/`_SaveAdd`/`_SaveUpdate` dialog modes; `Dialog`/`DialogCopy`/`_Copy` wizards; `_T#` tabs.
- Dashboard groups mirror sections: `<Sol> Admin (OnePlace)`, `<Sol> Admin Support`, then one group per config area.

`dashboardType` usage: exactly one `TopLevel` entry per unit (the OnePlace dashboard); shells are `Embedded`; runtime-filled panes are `EmbeddedDynamic`; use `TopLevelWithoutParameterPrompts`/`EmbeddedTopLevelWithoutParameterPrompts` to suppress prompting on parameterized panes.

### SQL config tables

- Name: `<Sol>_<Area>Config` (e.g. `FMM_CubeConfig`, `DDM_DynDBMenuLayoutConfig`).
- Integer surrogate PK named `<Area>ConfigID` (or `<Sol>` domain ID like `DynDBMenuID`), allocated via `SQL_GBL_Get_Max_ID`.
- Every table carries the audit quartet: `CreateDate`, `CreateUser`, `UpdateDate`, `UpdateUser`.
- Parent-child chains use explicit FK columns (`DynDBConfigID` on menu rows, `DynDBMenuID` on header rows; `CubeConfigID → ActConfigID → ModelConfigID → CalcConfigID` in FMM).
- First-time install: a `SolutionTableSetup()` extender function runs DDL from a file resource (`<Sol>_TableSetup.txt`), guarded by `DbSql.DoesTableExist`. Do not scatter `CREATE TABLE` through save logic.
- `dbLocation="Application"` for all solution tables.

### Substitution token styles

| Token | Meaning |
|---|---|
| `|!ParamName!|` | Dashboard parameter value |
| `|!!BL_Param!!|` | Bound-list display member |
| `~!TemplateParam!~` | Dynamic/template substitution var (component templates, repeat items) |
| `|!LV_Std_x_Format!|` | Shared literal (format strings from Globals) |
| `XFBR(<Assembly>, <Function>, ...)` | Inline call to a DashboardStringFunction |

## C# code patterns

### Namespaces (never hardcode the workspace)

```csharp
// Plain helpers, factories, services, POCOs:
namespace Workspace.__WsNamespacePrefix.__WsAssemblyName

// Business-rule entry files add the BR type + rule name:
namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.<RuleName>
namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.<RuleName>
namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardStringFunction.<RuleName>
```

OneStream substitutes the `__WsNamespacePrefix`/`__WsAssemblyName` tokens at deploy (→ `Workspace.OSConsTools.DDM_UI_Assembly`). Cross-assembly calls use the assembly-qualified form: `GBL_UI_Assembly.SQA_GBL_Command_Builder`. Declare assembly dependencies in the XML (`<dependencies>`), e.g. DDM_UI_Assembly depends on DDM_ConfigUI_Assembly (shared enums) and GBL_UI_Assembly (data access).

### Business-rule entry shape

Every BR file has exactly one entry class:

```csharp
public class MainClass
{
    public object Main(SessionInfo si, BRGlobals globals, object api, DashboardExtenderArgs args)
    {
        try
        {
            switch (args.FunctionType)
            {
                case DashboardExtenderFunctionType.ComponentSelectionChanged:
                    if (args.FunctionName.XFEqualsIgnoreCase("CubeConfig_SaveAdd"))
                        return CubeConfig_SaveAdd(si, globals, api, args);
                    break;
                // ...
            }
            return null;
        }
        catch (Exception ex)
        {
            throw ErrorHandler.LogWrite(si, new XFException(si, ex));
        }
    }
}
```

- Dispatch on `args.FunctionType` then `args.FunctionName.XFEqualsIgnoreCase(...)`.
- Wrap everything in try/catch rethrowing `ErrorHandler.LogWrite(si, new XFException(si, ex))`.
- Segment files with `#region "..."` blocks per functional area (`"Global Variables"`, `"Data Validation"`, `"Duplicate Checks"`, `"Menu Layout Save"`).
- Method-name verbs by role: `Get_*` (DataSets/XFBR reads), `Select_*` (selection-changed handlers), `*_SaveAdd`/`*_SaveUpdate`/`*_Save` (persistence), `Val_*` (validation), `Duplicate_*_Check`, `Process_*_Copy` (copy wizards), `Load_*` (load extenders), builder helpers `get_*`/camelCase privates. Extender-level state fields are prefixed `gbl_`.

### Rule-type responsibilities

| BR type | File home | Owns |
|---|---|---|
| DashboardDataSet | `DB DataSets/<Sol>_DataSets.cs` | All read queries (one file, many named datasets `get_<Sol>_*`) |
| DashboardExtender (load) | `DB Extenders/<Sol>_ConfigLoadDB.cs` | On-load param cascade, defaults, collapsible-menu state |
| DashboardExtender (save) | `DB Extenders/<Sol>_ConfigData.cs` | All writes, validation, duplicate checks, copy wizards, `SolutionTableSetup` |
| DashboardStringFunction (XFBR) | `XFBRs/<Sol>_ConfigUI.cs` / `<Sol>_UI.cs` | Pane routing (`_Blank` vs `_AddUpdate`), visibility, grid column format strings |
| Helper registry | `Helper Classes/<Sol>_ConfigHelpers.cs` | Enums + registries mapping types → target dashboards and substVar → DB column (`ParameterMappings`) — the single source of truth shared by save, load, and routing code |
| Service factory | `Factory/<Sol>_SvcFactory.cs` | `IWsAssemblyServiceFactory` switch over `WsAssemblyServiceType` |
| Services | `Factory/Services/<Sol>_DynDBSvc.cs` etc. | `IWsasDynamicDashboardsV800` (fill EmbeddedDynamic panes / repeat components), `IWsasDashboardV800`, `IWsasFinanceCustomCalculateV800` |

### The registry pattern (drive UI→DB mapping from data, not code)

Instead of one save method per field, define a registry once and map generically:

```csharp
public class LayoutConfig
{
    public string DashboardName { get; set; }                       // target render dashboard
    public Dictionary<string, string> ParameterMappings { get; set; } // substVar -> DB column
}
public static readonly Dictionary<LayoutType, LayoutConfig> LayoutRegistry = ...;
```

Save code iterates `ParameterMappings` to move dashboard params into a DataTable row; XFBR routing code reads `DashboardName` to choose the pane. Adding a layout/header/config type = one registry entry + one dashboard, no new save logic.

### Data access (always through Globals)

```csharp
var reader = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);      // parameterized reads
var cmd    = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);   // transactional upserts
int newId  = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection)
                 .Get_Max_ID(si, "FMM_CubeConfig", "CubeConfigID");          // key allocation
cmd.UpdateTable(si, "FMM_CubeConfig", dt, sqa);                              // schema-driven upsert
```

- Never hand-write INSERT/UPDATE statements in extenders; `GBL_SQL_Command_Builder` generates commands from the DataTable schema (PKs/exclusions from its table registry, falling back to `INFORMATION_SCHEMA`).
- Always parameterize (`SqlParameter`), never string-concatenate values into SQL.
- Writes run inside a transaction with rollback on error (built into `UpdateTable`).

### Dynamic dashboards

- Mark runtime-filled panes `EmbeddedDynamic` and leave their layouts empty; the registered `IWsasDynamicDashboardsV800` service fills them.
- The service dispatches on `storedDashboard.Name` / `DynamicDashboard.BasedOnName` and delegates to builder classes (`<Sol>_Header`, `<Sol>_Content`) in `Dyn DB Components/`; pass anything you can't handle to the base `api`.
- Repeat patterns (one component set per config row) inject per-row template subst vars (`~!...!~`) — see `FMM_DynDBSvc` repeating `FMM_SrcCellConfig_Cube_R2`.
- Guard expensive rebuilds: scope redraws with `DashboardsToRedraw` and short-circuit rebuilds when the config signature hasn't changed (`docs/reference/DDM_HeaderRebuildGuard.cs`).

## Best practices

1. **Config over code.** Behavior differences belong in `<Sol>_*Config` table rows, not in per-client code branches. RMW (hardcoded VB per module) is the anti-pattern; FMM is its replacement.
2. **Never hardcode client/application names** (cubes, workspaces, maintenance units, profile keys) in shared assemblies — resolve them from config tables or parameters. (Existing violations: `"Army"`, `"10 CMD PGM"` inside DDM_Header/DDM_Content.)
3. **Keep XML bindings and C# in sync.** Every `methodQuery`/`loadDashboardTaskArgs`/`wsAssemblyService` string in the XML must name an assembly, class, and function that actually exist. Rename drift is the most common breakage (see `docs/DASHBOARD_MAP.md` §6).
4. **One prefix generation per solution.** Don't mix `_Mbr_List_` and `MbrList` styles, don't leave `_Copy` designer artifacts on live objects, don't run old and new table schemas side by side longer than a migration requires.
5. **Validation and duplicate checks live in the save extender** (`Val_*`, `Duplicate_*_Check`) and set flag fields consumed by the UI — save methods must not write invalid or duplicate rows.
6. **`_Blank`/`_AddUpdate` twin panes** for every config form: route between them with an XFBR that checks whether a selection ID exists.
7. **Reuse Globals.** Icons (`Std_DB_*.png`), format strings (`LV_Std_*`), and the `GBL_*` data-access classes exist so solutions don't fork their own copies. New shared helpers go into `GBL_UI_Assembly`, not into a solution assembly.
8. **Audit everything.** Every config row write stamps the audit quartet; use `XFBR Get_Clean_Username`-style helpers for the user name.
9. **Build for the next consultant.** Prefer native OneStream constructs over custom code; when custom code is unavoidable, follow the folder taxonomy and naming so the next person can predict where everything lives from the prefix alone.
10. **Verify before committing:** `dotnet build OSConsTools.csproj` must succeed (with DLLs in `lib/OneStream/`), and any XML you touched must still import cleanly (well-formed, CDATA source matches the on-disk file).

## Quick checklist for a new solution `<Sol>`

- [ ] Two maintenance units: `<Sol>` (runtime) and `<Sol> (Admin)`; assemblies `<Sol>_UI_Assembly` / `<Sol>_ConfigUI_Assembly` with dependencies on `GBL_UI_Assembly`.
- [ ] Folder taxonomy: `DB DataSets`, `DB Extenders`, `Helper Classes`, `XFBRs`, `Factory[/Services]`, `Dyn DB Components` as needed.
- [ ] `<Sol>_SvcFactory` registered via `wsAssemblyService`; services only for the `WsAssemblyServiceType`s you implement.
- [ ] OnePlace entry dashboard + `AdminHdr`/`AdminContent` shells + collapsible menu (`DL_<Sol>_SetupOptions`, `IV_<Sol>_MenuWidth`, show/hide buttons).
- [ ] Config tables `<Sol>_<Area>Config` with surrogate keys + audit quartet; DDL in `<Sol>_TableSetup.txt` behind `SolutionTableSetup()`.
- [ ] Registries in `<Sol>_ConfigHelpers` mapping types → dashboards + `ParameterMappings`.
- [ ] `BL_`/`IV_` pairs for every selectable entity; `DL_` lists mirroring enums.
- [ ] `_Blank`/`_AddUpdate` pane pairs routed by `<Sol>_ConfigUI` XFBR functions.
- [ ] All reads in `<Sol>_DataSets`, all writes in `<Sol>_ConfigData`, load cascade in `<Sol>_ConfigLoadDB`.
