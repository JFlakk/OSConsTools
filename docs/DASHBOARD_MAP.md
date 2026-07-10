# DDM & FMM Dashboard Map

A complete structural map of the four maintenance units that make up the Dynamic Dashboard Manager (DDM) and Finance Model Manager (FMM) tools, plus the shared App Objects (Globals) unit they depend on. All five units live in the OneStream workspace **`OS Consultant Tools`** (`namespacePrefix="OSConsTools"`), shared into the consuming application workspaces via `sharedWorkspaceNames` (`PPBE Planning Process`, and for DDM also `10 CMD PGM`).

Every unit follows the same on-disk shape:

```
<Unit Name>/
├── XML/<ExportName>.xml                  # OneStream workspace export (source of truth for dashboards)
└── Assemblies/<X>_Assembly/<Folder>/*.cs # C# business rules, byte-parallel to the XML <files> tree
```

| Unit | Assembly | XML export | Role |
|---|---|---|---|
| Dynamic Dashboard Manager | `DDM_UI_Assembly` | `DynamicDashboardManager.xml` (3.7k lines) | End-user runtime: renders configured dynamic dashboards |
| Dynamic Dashboard Manager (Admin) | `DDM_ConfigUI_Assembly` | `DynamicDashboardManagerAdmin.xml` (10.2k lines) | Admin console: authors DDM config into SQL tables |
| Finance Model Manager | `FMM_UI_Assembly` | `FinanceModelManager.xml` (2.4k lines) | Runtime calc engine: no dashboards, pure code |
| Finance Model Manager (Admin) | `FMM_ConfigUI_Assembly` (+ `_Old`, `FMM_Shared_Assembly`) | `FinanceModelManagerAdmin.xml` (33k lines) | Admin console: authors finance-model config |
| App Objects (Globals) | `GBL_UI_Assembly` | `AppObjectsGlobals.xml` | Shared icons, `LV_Std_*` format literals, SQL data-access helpers |

**The core architecture in one sentence:** each tool is a *config-driven engine* — the Admin unit is a dashboard-based authoring front-end that writes rows into custom `DDM_*` / `FMM_*` SQL tables, and the user/runtime unit reads those same tables to render dashboards (DDM) or calculate cube data (FMM). The SQL tables are the contract between the two halves.

---

## 1. Dynamic Dashboard Manager (user/runtime side)

### 1.1 Dashboard groups

| Group | Dashboards | Purpose |
|---|---|---|
| `DDM Dynamic App DB (WorkFlow & OnePlace)` | 1 | Entry point |
| `DDM Dynamic App DB Support` | 29 | Layout shells, header shells, content panes |

### 1.2 Entry point

**`DDM Dynamic App Dashboard`** (TopLevel, Grid: 1 col x 2 rows `Auto`/`800`)
- `loadDashboardTaskType="ExecuteDashboardExtenderBRAllActions"` → fires the **`DDM_LoadDB`** DashboardExtender on load.
- Embeds `DDM_App_Hdr` (header) and `DDM_App_Content` (body), plus supplied parameters for menu state (`sp_BL_DDM_AppMenu`, `sp_IV_DDM_App_ShowHide_MenuBtn`, `sp_IV_DDM_App_MenuWidth`, show/hide button visibility).

### 1.3 Dashboard tree

```
DDM Dynamic App Dashboard (TopLevel)
├── DDM_App_Hdr                       (2 cols: Auto, Auto; row height 80)
│   ├── DDM_App_Hdr_C1                (VerticalStackPanel: lbl_DDM_DynDB_Hdr title,
│   │                                  btn_DDM_App_MenuHide / btn_DDM_App_MenuShow)
│   └── DDM_App_Hdr_C2                (HorizontalStackPanel, template-repeat host)
│       └── DDM_App_Hdr_C2C1          (EmbeddedDynamic — filter buttons / combos /
│                                      textboxes generated at runtime by DDM_Header.cs)
└── DDM_App_Content                   (2 cols: Auto, *)
    ├── lbx_DDM_AppMenu               (left nav ListBox bound to BL_DDM_AppMenu)
    └── DDM_App_Content_C2            (content host; dynamic child chosen by
                                       XFBR DDM_UI.Get_LayoutDB)
        └── one of the layout shells below (EmbeddedDynamic; panes filled at
            runtime by DDM_Content.cs via DDM_DynDBSvc)
```

### 1.4 Layout shell dashboards and suffix codes

The 24 `DDM_App_Content_*` dashboards are layout shells. Suffix codes describe pane geometry:

| Code | Meaning |
|---|---|
| `L` / `R` / `T` / `B` | Single leaf pane: Left / Right / Top / Bottom |
| `TL` / `TR` / `BL` / `BR` | Quadrant leaf pane |
| `2L` / `2R` / `2T` / `2B` | Stacked pair (e.g. `2L` = TL over BL) |
| `LR` / `TB` | Two-pane split (columns / rows) |
| `1L2R` / `2L1R` / `1T2B` / `2T1B` | Three-pane mixes (1 large + 2 stacked) |
| `2x2` | Four quadrants |
| `CV` | Cube View shell (hosts a `cv_*` component instead of an embedded dashboard) |
| `_DB` suffix | "Dashboard content" variant of the layout |
| `Hdr`, `C1`/`C2`/`C2C1` | Header family; `C#` = column index, chained for nesting |

Composition (each composite embeds the leaves):

| Shell | Embeds |
|---|---|
| `DDM_App_Content_DB` | (default single dynamic pane) |
| `DDM_App_Content_CV` | `cv_DDM_Dynamic_App_Content` |
| `DDM_App_Content_LR_DB` | `_L_DB` + `_R_DB` |
| `DDM_App_Content_TB_DB` | `_T_DB` + `_B_DB` |
| `DDM_App_Content_1L2R_DB` | `_L_DB` + `_2R_DB` |
| `DDM_App_Content_2L1R_DB` | `_2L_DB` + `_R_DB` |
| `DDM_App_Content_1T2B_DB` | `_T_DB` + `_2B_DB` |
| `DDM_App_Content_2T1B_DB` | `_2T_DB` + `_B_DB` |
| `DDM_App_Content_2x2_DB` | `_2T_DB` + `_2B_DB` |
| `DDM_App_Content_2T_DB` / `_2B_DB` / `_2L_DB` / `_2R_DB` | pairs of `_TL/_TR/_BL/_BR_DB` leaves |

Each `LayoutType` enum value in `DDM_ConfigHelpers` maps 1:1 to a shell (e.g. `Dashboard_2x2 → DDM_App_Content_2x2_DB`, `CubeView → DDM_App_Content_CV`).

### 1.5 Components (55)

| Prefix | Type | Count | Notes |
|---|---|---|---|
| `Embedded <name>` | EmbeddedDashboard | 28 | one wrapper per dashboard |
| `btn_` | Button | 17 | 13 member-select filter buttons (`btn_DDM_App_MbrList<Dim>`, SelectMemberDialog type), generic `btn_DDM_App_Btn`, menu show/hide |
| `cv_` | CubeView | 9 | empty shells bound at runtime (`cv_DDM_Dynamic_App_Content[_T/_B/_L/_R/_TL/_TR/_BL/_BR]`) |
| `sp_` | SuppliedParameter | 6 | push param values into dashboard scope |
| `cbx_` / `lbl_` / `lbx_` | ComboBox / Label / ListBox | 1 each | `lbx_DDM_AppMenu` is the nav menu |

### 1.6 Parameters (44)

| Prefix | Type | Count | Examples |
|---|---|---|---|
| `BL_` | BoundList | 1 | `BL_DDM_AppMenu` — the menu driver; method query → `DDM_DataSets.Get_App_Menu` |
| `IV_` | InputValue | 7 | `IV_DDM_App_ShowHide_MenuBtn`, `IV_DDM_App_MenuWidth`, `IV_DDM_App_Dashboard_Copy` |
| `ML_` | MemberList | 16 | one per dimension (Account, Cons, Entity, Flow, Origin, Scenario, Time, UD1-8, View); all templated via `~!Mbr_List_*!~` tokens |

### 1.7 Data adapters

None. Data access is via parameter method queries (`BL_DDM_AppMenu`) and direct SQL inside business rules (through `GBL_UI_Assembly` helpers) against: `DDM_DynDBConfig`, `DDM_DynDBMenuLayoutConfig`, `DDM_DynDBMenuConfig`, `DDM_DynDBHdrConfig`, `Cube`.

### 1.8 Business rules (`DDM_UI_Assembly`)

| Folder | File | BR type | Role |
|---|---|---|---|
| Dyn DB Extenders | `DDM_LoadDB.cs` | DashboardExtender | Load-time state: menu defaults, show/hide, menu width |
| Dyn DB DataSets | `DDM_DataSets.cs` | DashboardDataSet | `Get_App_Menu` — menu rows for the current WF profile |
| XFBRs | `DDM_UI.cs` | DashboardStringFunction | `Get_LayoutDB` — resolves which layout shell to embed |
| Dyn DB Support | `DDM_Support.cs` | helper (plain class) | Config-row → pane-binding resolvers, param builders |
| Dyn DB Components | `DDM_Header.cs` | helper (plain class) | Runtime factory for header filter buttons/combos/textboxes |
| Dyn DB Components | `DDM_Content.cs` | helper (plain class) | Runtime factory for content panes (binds embedded DB or CV) |
| Factory | `DDM_SvcFactory.cs` | `IWsAssemblyServiceFactory` | Registered via MU `wsAssemblyService="DDM_UI_Assembly.DDM_SvcFactory"` |
| Factory/Services | `DDM_DynDBSvc.cs` | `IWsasDynamicDashboardsV800` | Fills EmbeddedDynamic panes; dispatches to DDM_Header/DDM_Content |
| Factory/Services | `DDM_DBSvc.cs` | `IWsasDashboardV800` | Standard-dashboard hook (currently a stub) |

### 1.9 Runtime flow

1. `DDM Dynamic App Dashboard` loads → `DDM_LoadDB` seeds menu/show-hide params and defaults `BL_DDM_AppMenu` from `DDM_DynDBConfig ⋈ DDM_DynDBMenuLayoutConfig` (keyed on the workflow profile key).
2. `lbx_DDM_AppMenu` (fed by `DDM_DataSets.Get_App_Menu`) lists the configured menus; selection redraws the dashboard.
3. XFBR `DDM_UI.Get_LayoutDB` reads the selected menu's `LayoutType`/`DB_Name`/`CV_Name` and returns the correct layout shell.
4. The shells are `EmbeddedDynamic`, so the engine calls `DDM_DynDBSvc`, which delegates to `DDM_Content` (fills each pane with the configured embedded dashboard or cube view) and `DDM_Header` (generates header filter/button/textbox components from `DDM_DynDBHdrConfig`).

---

## 2. Dynamic Dashboard Manager (Admin)

### 2.1 Dashboard groups (8 groups, 103 dashboards)

| Group | Count | Purpose |
|---|---|---|
| `DDM Admin (OnePlace)` | 1 | Entry shell `DDM_Admin (OnePlace)` |
| `DDM Admin Support` | 6 | `DDM_AdminHdr[_C1/_C2]`, `DDM_AdminContent[_C2]` shell parts |
| `DDM Admin Config WFP` | 11 | Workflow-profile-bound config (`DDM_ConfigWFP` tree) |
| `DDM Admin Config OPDB` | 7 | Stand-alone / operational dashboard config |
| `DDM Admin Menu Layout Config` | 15 | Menu (page) configuration (`DDM_MenuLayoutConfig` tree) |
| `DDM Admin Layout Config` | 27 | Layout picker: 10 layout templates, each with a `_C2` variant + preview images |
| `DDM Admin Config Header` | 39 | Header item (filter/button) configuration — branches by control type (`_Fltr`, `_Btn`, `_Cbx`, `_Txt`, `_FileExp`) |
| `DDM Admin Bulk Config` | 1 | `DDM_Config_Export_Dialog` (export/migration, WIP) |

Naming inside trees: `_C#` = column, `_R#` = row (chainable, e.g. `DDM_ConfigWFP_C2C2R2`), `_Blank` vs `_AddUpdate` = empty vs populated pane states, `_Add`/`_Update` = dialog modes.

### 2.2 Components (250)

| Prefix | Type | Count | Highlights |
|---|---|---|---|
| `Embedded`/`emb` | EmbeddedDashboard | 110 | |
| `txt_` | TextBox | 53 | config field inputs |
| `cbx_` | ComboBox | 22 | type/status/content-type pickers |
| `btn_` | Button | 22 | Save/Add/Cancel + 6 layout-preview buttons |
| `sp_` | SuppliedParameter | 19 | |
| `lbx_` | ListBox | 8 | `lbx_DDM_SetupOptions` = main admin menu |
| `lbl_` / `chk_` | Label / CheckBox | 5 each | |
| `trv_` | TreeView | 3 | `trv_DDM_WFP`, `trv_DDM_WSMU`, `trv_DDM_WSMU_DB` |
| `Img_` | Image | 2 | logo, separator line |
| `ted_` | SqlTableEditor | 1 | `ted_DDM_DynDBConfig` — the only editable grid |

### 2.3 Parameters (112)

| Prefix | Type | Count | Convention |
|---|---|---|---|
| `IV_` | InputValue | 79 | live edit values; the working copy of a `BL_` selection |
| `DL_` | DelimitedList | 24 | static option lists mirroring C# enums (`DL_DDM_SetupOptions` drives the section menu) |
| `BL_` | BoundList | 9 | dynamic lists fed by `DDM_DataSets` (`BL_DDM_WFPRoot`, `BL_DDM_MenuLayoutConfig`, `BL_DDM_HdrConfigs`, …) |

Core pattern: **`BL_` ↔ `IV_` pairing** — a bound-list selection is mirrored into a same-named `IV_` param for use in SQL Table Editors; `DDM_ConfigLoadDB.getDefaultParam` literally does `param.Replace("IV_","BL_")` to find defaults.

### 2.4 Data adapters (3, all Method/BusinessRule)

| Adapter | Feeds | Rule → dataset |
|---|---|---|
| `da_DDM_WFP_trv` | `trv_DDM_WFP` | `DDM_DataSets → Get_WFP_trv` (WF profile tree; bolds already-configured profiles) |
| `DA_DDM_WSMU_TreeView` | `trv_DDM_WSMU` | `Get_WSMU_TreeView` (workspace → maintenance-unit tree) |
| `DA_DDM_WSMU_DB_TreeView` | `trv_DDM_WSMU_DB` | workspace → MU → dashboard tree |

### 2.5 Config tables (the DDM contract)

| Table | PK | Written by | Read by (user side) |
|---|---|---|---|
| `DDM_DynDBConfig` | `DynDBConfigID` | `ted_DDM_DynDBConfig`, `ConfigWFP_SaveAdd` | `DDM_LoadDB`, `Get_App_Menu` |
| `DDM_DynDBMenuLayoutConfig` | `DynDBMenuID` | `MenuLayoutConfig_Save(Add/Update)` | menu list + layout resolution |
| `DDM_DynDBHdrConfig` | `DynDBHdrID` | header config saves | `DDM_Header` (filter/button generation) |
| `DDM_HdrConfigs` / `DDM_Config` / `DDM_Config_Menu` | legacy | older save paths (mid-migration) | — |

### 2.6 Business rules (`DDM_ConfigUI_Assembly`)

| Folder | File | BR type | Role |
|---|---|---|---|
| DB DataSets | `DDM_DataSets.cs` | DashboardDataSet | All read queries (trees, combos, menu/header lists) |
| DB Extenders | `DDM_ConfigData.cs` | DashboardExtender | All saves, validation (`Val_*`), duplicate checks (`Duplicate_*_Check`) |
| DB Extenders | `DDM_ConfigLoadDB.cs` | DashboardExtender | On-load param cascade (`paramMap`, `HierarchyDict`), collapsible menu |
| DB Extenders | `DDM_Config_Migration.cs` | DashboardStringFunction | Currently a duplicate of `DDM_ConfigUI.cs`; intended export/migration home |
| Helper Classes | `DDM_ConfigHelpers.cs` | helper | **LayoutRegistry / HdrRegistry** — the single source of truth mapping `LayoutType`/`HdrType` → target dashboard + substvar↔column `ParameterMappings`; all shared enums |
| XFBRs | `DDM_ConfigUI.cs` | DashboardStringFunction | Routes `_Blank` vs `_AddUpdate` panes, grid column formats (`DDMColFormatter`) |

### 2.7 Admin flow

Select a WF profile in `trv_DDM_WFP` → create a `DDM_DynDBConfig` row (SqlTableEditor) → define menus (`DDM_MenuLayoutConfig`, layout picked from `LayoutRegistry` templates with preview images) → define header items (`DDM_HdrConfig`, filter/button variants via `HdrRegistry`) → user side renders it all at runtime.

---

## 3. Finance Model Manager (user/runtime side)

**No dashboards, parameters, components, or adapters** — the XML declares only the `FMM_UI_Assembly` code. This unit is a pure calculation engine.

### 3.1 Business rules (`FMM_UI_Assembly`)

| File | BR type | Role |
|---|---|---|
| `Factory/FMM_SvcFactory.cs` | `IWsAssemblyServiceFactory` | Only `DataSet → FMM_DataSetSvc` active; other service types scaffolded/commented |
| `Factory/Services/FMM_CustCalcSvc.cs` | `IWsasFinanceCustomCalculateV800` | CustomCalculate shell (empty try/catch) |
| `Factory/Helper Classes/FMM_StdFinBRHelpers.cs` | Finance BR helper (1.5k lines) | **The engine.** Model-group dispatch (`Proc_ModelGrps` → `Proc_TableModels` / `Proc_CubeModels` / `Proc_ConsolModels` / `Proc_BRTabletoCubeModels` / `Proc_CubetoTableModels`), balanced/unbalanced buffer calcs (`Calc_Balanced_Buffer`, `Calc_UnbalBuffer`, `Calc_UnbalAlloc_Buffer`, …), expression evaluation (`DataTable.Compute` over tokenized formulas), per-dimension cell rewrites, `SetDataBuffer` writes with self-reconciling clears |

The XML also declares 11 additional stub service files (`FMM_DBSvc`, `FMM_DynCVSvc`, `FMM_XFBRSvc`, `FMM_TEDSvc`, …) following the full WsAssembly service taxonomy.

### 3.2 What it reads

`FMM_ModelGrps → FMM_ModelGrpAssign → FMM_Models → FMM_ActConfig → FMM_CalcConfig → FMM_SrcCell / FMM_DestCell` (plus legacy `MCM_Cell`). Source-cell rows carry a full POV (Acct, View, Origin, IC, Flow, UD1-8), buffer filters, per-dim override flags, dynamic-calc scripts, math operators, and parens — the engine walks a model group's calcs in `Sequence` order and turns those rows into cube data.

---

## 4. Finance Model Manager (Admin)

### 4.1 Dashboard groups (17 groups, 261 dashboards)

| Group | Section root | Configures |
|---|---|---|
| `FMM Admin (OnePlace)` | `FMM_Admin (OnePlace)` | Entry shell (AdminHeader + AdminContent + collapsible menu) |
| `FMM Admin Support` | `FMM_AdminHeader`, `FMM_AdminContent` | Shell parts |
| `FMM Cube Config` | `FMM_CubeConfig` | Cubes (`FMM_CubeConfig` table) |
| `FMM Cube Settings` | `FMM_CubeSettings_C2` | Cube-level settings |
| `FMM Unit and Acct Config` | `FMM_UnitAcctConfig` | Units & accounts (`FMM_UnitConfig`, `FMM_AcctConfig`) |
| `FMM Build Model Config` | `FMM_ModelConfig` (+ `Dialog_Add`/`_Copy` wizards with `_T1/_T2/_T3` tabs) | Models |
| `FMM Build Model Group` | `FMM_ModelGrpConfig` | Model groups |
| `FMM Build Model Group Sequence` | `FMM_ModelGrpSeqConfig` | Ordered calc sequences |
| `FMM Calc Table` / `FMM Calc Cube` / `FMM Calc Consol` / `FMM Calc BR Table to Cube` / `FMM Calc BR Cube to Table` | `FMM_CalcConfig_<Type>` | Per-type calc config incl. `FMM_SrcCellConfig_*` / `FMM_DestCellConfig_*` (source-cell rows rendered as a repeated dynamic dashboard `FMM_SrcCellConfig_Cube_R2`) |
| `FMM Approval Config` | `FMM_ApprConfig` | Approval workflow steps |
| `FMM Custom Table Definition` | `FMM_CustTableConfig`, `FMM_CustTableDef` (tabs `_T1_Cols`, `_T2_Indexes`, `_T3_Keys`) | Custom SQL tables |
| `FMM Data Validation Config` | `FMM_DataValConfig` | Validation rules |
| `FMM UI Config` | `FMM_UIConfig` | Grid column formatting |

Same nesting grammar as DDM Admin: `_C#`/`_R#` grid coordinates chained arbitrarily deep (`FMM_CalcConfig_Cube_R1R2C2R1C3`), `_Add`/`_Update`/`_AddUpdate`/`_Blank`/`_SaveAdd`/`_SaveUpdate` states, `Dialog`/`DialogCopy`/`_Copy` wizards, `_T#` tabs.

### 4.2 Components (632 instances / 511 definitions)

| Prefix | Type | Count |
|---|---|---|
| `Embedded`/`emb` | EmbeddedDashboard | 265 |
| `txt_` | TextBox | 129 |
| `btn_` | Button | 76 |
| `cbx_` | ComboBox | 42 |
| `sp_` | SuppliedParameter | 39 |
| `lbx_` | ListBox | 30 |
| `lbl_` | Label | 19 |
| `ted_` | SqlTableEditor | 15 |
| `chk_` | CheckBox | 10 |
| `gv_` | GridView | 4 |
| `trv_` | TreeView | 1 |
| `SS_` | XFSpreadsheet | 1 |
| `Img_` | Image | 1 |

Buttons follow `btn_FMM_<Area>_<Action>` with `boundParameterName` (e.g. `IV_FMM_AcctConfig_AddUpdate`), `paramValueForButtonClick` (`Add`/`Update`/`Delete`), and `selectionChangedTaskType=ExecuteDashboardExtenderBusinessRule` routing into `FMM_ConfigData`.

### 4.3 Parameters (272)

| Prefix | Type | Count | Load-bearing examples |
|---|---|---|---|
| `IV_` | InputValue | ~195 | `IV_FMM_<Area>_AddUpdate` mode flags, `IV_FMM_MenuWidth` |
| `BL_` | BoundList | 48 | the selection chain: `BL_FMM_CubeConfigID → BL_FMM_ActConfigID → BL_FMM_ModelConfigID → BL_FMM_CalcConfigID` |
| `DL_` | DelimitedList | 29 | `DL_FMM_SetupOptions` (section menu), `DL_FMM_CalcType` |

### 4.4 Data adapters

6 named adapters (`DA_FMM_CalcUnitConfig`, `DA_FMM_ModelConfig`, `DA_FMM_ModelGrps`, `DA_FMM_Src_Calc_List` — SQL; `DA_FMM_Fdx_Cube_View`, `DA_FMM_TreeView` — Method → `FMM_DataSets`), plus ~278 component-level embedded adapters (inline `sqlQuery`/`methodQuery` inside combos/lists/grids).

### 4.5 Config tables (the FMM contract)

Actively read/written by the admin UI (integer surrogate keys `<Area>ConfigID`, audit quartet `CreateDate/CreateUser/UpdateDate/UpdateUser`):

`FMM_CubeConfig`, `FMM_ActConfig`, `FMM_AcctConfig`, `FMM_UnitConfig`, `FMM_CalcUnitConfig`, `FMM_CalcConfig`, `FMM_CalcConfig_Config`, `FMM_CalcConfig_Unit_Assign`, `FMM_ModelConfig`, `FMM_ModelGrps`, `FMM_ModelGrpAssign`, `FMM_ModelConfigGrp_Seqs`, `FMM_SrcCell`, `FMM_SrcCellConfig`, `FMM_DestCell`, `FMM_Col_Config`, `FMM_Reg_Config`, `FMM_ApprConfigConfig`, `FMM_ApprConfigStep_Config`, `FMM_CustTable`, `FMM_CustTableDef`, `FMM_UIConfig`.

First-time install: `FMM_ConfigData.SolutionTableSetup()` runs DDL from the `FMM_TableSetup.txt` file resource, guarded by `DbSql.DoesTableExist`. Reference DDL for the next-generation config-driven engines (cube load/extract, workflow states, validation, audit) lives in `docs/reference/FMM_ConfigTables.sql`.

### 4.6 Business rules (`FMM_ConfigUI_Assembly`)

| Folder | File | BR type | Role |
|---|---|---|---|
| DB Extenders | `FMM_ConfigData.cs` (5.6k lines) | DashboardExtender | All saves/copies/deletes; `SolutionTableSetup`; duplicate detection; copy wizards (`Process_Model_Copy`, `Process_Calc_Copy`, …) |
| DB Extenders | `FMM_ConfigLoadDB.cs` | DashboardExtender | `Load_FMM_DB` param cascade — `HierarchyDict` maps each section root to its ordered param dependency chain |
| DB Extenders | `FMM_ConfigMigration.cs` | DashboardExtender | Stub scaffold |
| DB DataSets | `FMM_DataSets.cs` | DashboardDataSet | 32 datasets (`get_FMM_*`) feeding combos/grids/trees |
| XFBRs | `FMM_ConfigUI.cs` | DashboardStringFunction | Pane routing (`Get_<Area>ConfigDB` → `_Blank`/`_AddUpdate`), visibility, grid column formats |
| Helper Classes | `FMM_ConfigHelpers.cs` | helper | `IConfigMappings` registries per area — depth → {substVar → DB column} `ParameterMappings`; `MapConfigValues`, `Set*ConfigParams` |
| Dyn DB Components | `FMM_SrcCellDB.cs` | repository helper | CalcType-aware CRUD over `FMM_SrcCellConfig` |
| Dyn DB Components | `FMM_SrcCellModel.cs` | POCO | Full-POV source-cell model with dimension indexer |
| Factory | `FMM_SvcFactory.cs` | service factory | `DynamicDashboards → FMM_DynDBSvc` |
| Factory/Services | `FMM_DynDBSvc.cs` | `IWsasDynamicDashboardsV800` | Repeats one component set per source-cell row for `FMM_SrcCellConfig_Cube_R2` |

---

## 5. App Objects (Globals)

No dashboards or adapters — purely shared assets:

- **20 `Std_DB_*.png` icons** (Save, Search, Delete, Refresh, HideMenu/ShowMenu, …) used by DDM/FMM buttons.
- **12 `LV_Std_*` LiteralValue parameters** holding standard control format strings (`LV_Std_btn_Format`, `LV_Std_cbx_Format`, `LV_Std_txt_Format`, `LV_Std_Header_Format`, …) referenced as `|!LV_Std_x_Format!|`.
- **`GBL_UI_Assembly`** — the shared data-access layer every DDM/FMM rule uses:

| Folder | File | Role |
|---|---|---|
| SQL Adapters | `SQL_GBL_Get_DataSets.cs` | Parameterized SELECT → DataTable filler |
| SQL Adapters | `SQL_GBL_Get_max_ID.cs` | `MAX(id)+1` surrogate-key allocator |
| SQL Adapters | `SQA_GBL_Command_Builder.cs` | Transactional upsert entry point (`UpdateTable`, `FillDataTable`, merge helpers) |
| GBL Support Classes | `GBL_SQL_Command_Builder.cs` | Schema-driven INSERT/UPDATE/DELETE command generation; PK/exclusion registry (falls back to `INFORMATION_SCHEMA`) |
| GBL Support Classes | `GBL_Helpers.cs` | Custom-subst-var updates, safe DataRow accessors, blank-value coercion |
| GBL Support Classes | `GBL_Import_CSV.cs` | Generic CSV → table bulk loader |

Standard read-then-upsert idiom used everywhere:

```csharp
var reader   = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);   // reads
var cmd      = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection); // writes
var nextId   = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection)
                   .Get_Max_ID(si, "FMM_CubeConfig", "CubeConfigID");       // new keys
cmd.UpdateTable(si, "FMM_CubeConfig", dt, sqa);                             // upsert
```

---

## 6. Known inconsistencies (do not replicate)

These exist in the current source and are the anti-patterns to avoid in new work:

1. **Mixed parameter naming generations** — `ML_DDM_App_<Dim>MbrList` vs `ML_DDM_App_<Dim>_Mbr_List_Copy`; code builds names dynamically in three formats, so some lookups can miss.
2. **Assembly referenced two ways** — `DDM_ConfigUI_Assembly` (source) vs `DDM_Config_UI_Assembly` (some XML methodQueries).
3. **XML references methods that don't exist in source** — e.g. `DDM_Config_Migration.Export_DDM_Config`, `DDM_Config_Data.Save_New_Profile_Config` (rename drift between XML bindings and C#).
4. **`DDM_Config_Migration.cs` is a byte-copy of `DDM_ConfigUI.cs`** rather than actual migration logic.
5. **Dual table generations coexist** — new `DDM_DynDB*` tables alongside legacy `DDM_Config`/`DDM_HdrConfigs` write paths.
6. **Hard-coded client values in shared code** — cube `"Army"`, workspace `"10 CMD PGM"`, maintenance unit `"CMD PGM WF"` inside `DDM_Header`/`DDM_Content`.
7. **Signature drift** — `DDM_UI.Get_LayoutDB` references an undeclared `currDB` and calls `get_PaneBinding` with the wrong arity.
8. **`_Copy` suffixes on live objects** (`IV_DDM_App_Dashboard_Copy`, `sp_..._Copy (2)`) left over from designer copy-paste.
