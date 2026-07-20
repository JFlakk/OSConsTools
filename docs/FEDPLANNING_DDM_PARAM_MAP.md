# FedPlanning DDM — Module Guide & Parameter → Column → Dashboard Object Mapping

Source analyzed: `FedPlanning_ApplicationWorkspaces_20260720_174727Z.xml` (OneStreamXF 9.3.0.18429). This document is built **solely from that export** — it reflects what is deployed to FedPlanning, not what is in this repo's older copies of the same units.

The export contains one workspace, `OS Consultant Tools` (`namespacePrefix="OSConsTools"`, shared to `10 CMD PGM, 00 GBL`), with three maintenance units:

| Unit | Assembly | Contents |
|---|---|---|
| App Objects (Globals) | `GBL_UI_Assembly` | 20 `Std_DB_*.png` icons, 12 `LV_Std_*` format literals, 6 shared SQL/helper classes |
| Dynamic Dashboard Manager | `DDM_UI_Assembly` | 23 params, 29 dashboards, 65 components, 9 C# files — the end-user runtime |
| Dynamic Dashboard Manager (Admin) | `DDM_ConfigUI_Assembly` | 112 params, 103 dashboards, 250 components, 3 adapters, 6 C# files — the config console |

## 1. What this module is

DDM is a **config-driven dynamic dashboard engine**. Admins never build end-user dashboards directly; they write rows into three SQL config tables, and the runtime unit assembles the user-facing dashboard from those rows at render time.

```
DDM Admin console                    SQL config tables                DDM runtime
─────────────────                    ─────────────────                ───────────
DDM_ConfigWFP grid       ──writes──> DDM_DynDBConfig       ──read──>  DDM_LoadDB (menu scoping by WFPKey)
DDM_MenuLayoutConfig UI  ──writes──> DDM_DynDBMenuLayoutConfig ─read─> lbx_DDM_AppMenu, XFBR Get_LayoutDB
DDM_HdrConfig UI         ──writes──> DDM_DynDBHdrConfig (*)  ──read──> DDM_Header (filter buttons/combos)
```

(*) As deployed, the header save actually writes a different table — see §4, defect D1.

Runtime flow: `DDM Dynamic App Dashboard` (TopLevel) fires `DDM_LoadDB` on load → `BL_DDM_AppMenu` (fed by `DDM_DataSets.Get_App_Menu`) drives the left menu → XFBR `DDM_UI.Get_LayoutDB` picks the layout shell (`DDM_App_Content_<code>_DB`) from the selected menu row's `LayoutType` → `DDM_DynDBSvc` (registered via `wsAssemblyService="DDM_UI_Assembly.DDM_SvcFactory"`) fills each `EmbeddedDynamic` pane through `DDM_Content` (embedded dashboard / cube view per pane columns) and `DDM_Header` (one filter/button component set per header row).

**How the mapping machinery works.** `DDM_ConfigHelpers` (Admin assembly) holds two registries — `LayoutRegistry` and `HdrRegistry` — of the shape:

```csharp
ParameterMappings : Dictionary<int /*order*/, Dictionary<string /*parameter*/, string /*DB column*/>>
Config_DashboardName  // which admin form pane renders this type
DashboardName         // which runtime shell renders this type
```

Save extenders (`DDM_ConfigData`) iterate `ParameterMappings` and copy each parameter's value into the mapped DataTable column, then `SQA_GBL_Command_Builder.UpdateTable` upserts the row. The XFBR (`DDM_ConfigUI`) uses `Config_DashboardName` to route the admin UI; the runtime XFBR (`DDM_UI`) uses `DashboardName` to route rendering. **The registries are therefore the single source of truth for the parameter → column → dashboard mapping** — and every place where reality diverges from them is flagged below.

---

## 2. The mapping

### 2.1 Workflow Profile config (table `DDM_DynDBConfig`)

Editor object: **`ted_DDM_DynDBConfig`** (SqlTableEditor) on dashboard **`DDM_ConfigWFP_C2C2R2`**.
STE wiring: `TableName=DDM_DynDBConfig`, `WhereClause = WFPKey = '|!IV_DDM_WFPtrv!|'`, `ColumnNameForBoundParameter=DynDBConfigID` → bound param **`IV_DDM_ConfigID`**, redraws `DDM_ConfigWFP_C2C2R2`.
Grid column formatting comes from XFBR `Get_DDM_ColFormat` → `DDMColFormatter.ConfigColumns["DDM_ConfigWFP"]`:

| Column | Grid behavior | Parameter |
|---|---|---|
| `DynDBConfigID` | hidden, read-only (PK; feeds `IV_DDM_ConfigID`) | — |
| `WFPKey` | visible, read-only, default `|!IV_DDM_WFPtrv!|` | `BL_DDM_WFPNames` (display list) |
| `WFPStepType` | visible, read-only | `DL_DDM_WFPStepType` |
| `Status` | visible, editable | — |
| `CreateDate` / `CreateUser` / `UpdateDate` / `UpdateUser` | visible, read-only | — |

Row creation: `ConfigWFP_SaveAdd` (extender) — reads `IV_DDM_WFPtrv` (set from `trv_DDM_WFP` via adapter `da_DDM_WFP_trv` → `Get_WFP_trv`), checks for an existing row by `WFPKey`, then writes `DynDBType=1`, `WFPKey`, `WFPStepType` (from `WorkflowProfileHierarchy.ProfileType`), `Status=1`, audit quartet. ⚠ See defect D4 — the new row's `DynDBConfigID` is never allocated.

Selection cascade (from `DDM_ConfigLoadDB`): `DL_DDM_SetupOptions` → `DL_DDM_Type` → `BL_DDM_WFPRoot` (`lbx_DDM_RootWFP` @ `DDM_ConfigWFP_C1`) → `BL_DDM_WFPScenType` (`lbx_DDM_ScenType` @ `DDM_ConfigWFP_C1`) → `IV_DDM_WFPtrv` → `IV_DDM_ConfigID` → `BL_DDM_MenuLayoutConfig` → `DL_DDM_LayoutConfigType`. The `BL_ → IV_` pairs are mirrored via `paramMap` (`BL_DDM_WFPtrv→IV_DDM_WFPtrv`, `BL_DDM_ConfigID→IV_DDM_ConfigID`).

### 2.2 Menu / layout config (table `DDM_DynDBMenuLayoutConfig`)

Selector: **`lbx_DDM_MenuLayout`** @ `DDM_MenuLayoutConfig_C1`, bound to **`BL_DDM_MenuLayoutConfig`** (methodQuery `DDM_DataSets.Get_ConfigMenu`, param `IV_DDM_ConfigID`). Layout type picker: **`cbx_DDM_LayoutConfigType`** @ `DDM_MenuLayoutConfig_C2R1` → **`DL_DDM_LayoutConfigType`**. XFBR `Get_MenuDB`/`Get_DDM_LayoutConfigDB` routes `_Blank` vs `_AddUpdate` panes and picks the layout form from `LayoutRegistry.Config_DashboardName`.

**Fixed columns written by `MenuLayoutConfig_Save` on every save:**

| Column | Source parameter | Editing object @ dashboard | Status |
|---|---|---|---|
| `DynDBMenuID` | new via `SQL_GBL_Get_Max_ID` (Add) / `BL_DDM_MenuLayoutConfig` (Update) | — | OK (see D5 for Update) |
| `DynDBConfigID` | `IV_DDM_ConfigID` | supplied from STE selection | OK |
| `Name` | `IV_DDM_MenuLayout_Name` | `txt_DDM_Menu_Name` @ all `DDM_LayoutConfig_*_C2` forms | OK |
| `SortOrder` | `IV_DDM_Menu_SortOrder` (code) **and** `IV_DDM_MenuLayout_SortOrder` (registry) | `txt_DDM_Menu_SortOrder` @ `DDM_LayoutConfig_*_C2` | ⚠ D6 — code reads a parameter that doesn't exist |
| `LayoutType` | `DL_DDM_LayoutConfigType` | `cbx_DDM_LayoutConfigType` | OK |
| `Status` | hardcoded `1` on Add; `DL_DDM_MenuLayout_Status` via registry | `cbx_DDM_MenuLayout_Status` @ `DDM_LayoutConfig_CV_C2`/`_DB_C2`/`_TB_DB_C2` | OK |
| audit quartet | `DateTime.Now` / `si.UserName` | — | OK |

**Per-LayoutType registry mappings** (`LayoutRegistry.Configs`). Legend: ✅ = parameter exists and is wired; ⚠ = broken (see note).

**LayoutType 1 — Dashboard** (admin form `DDM_LayoutConfig_DB`/`_C2`, runtime `DDM_App_Content_DB`)

| # | Parameter | Column | Editor @ dashboard | Status |
|---|---|---|---|---|
| 0 | `IV_DDM_MenuLayout_SortOrder` | `SortOrder` | `txt_DDM_Menu_SortOrder` | ✅ |
| 1 | `IV_DDM_MenuLayout_Name` | `Name` | `txt_DDM_Menu_Name` | ✅ |
| 2 | `IV_DDM_MenuLayout_DB_Name` | `DB_Name` | `txt_DDM_MenuLayout_DB_Name` @ `_DB_C2`, `_2x2_DB_C2`, `_CustomDB_C2` | ✅ |
| 3 | `DL_DDM_MenuLayout_Status` | `Status` | `cbx_DDM_MenuLayout_Status` | ✅ |

**LayoutType 2 — CubeView** (form `DDM_LayoutConfig_CV`/`_C2`, runtime `DDM_App_Content_DB`)
Rows 0/1/3 as above, plus:

| # | Parameter | Column | Editor @ dashboard | Status |
|---|---|---|---|---|
| 2 | `IV_DDM_MenuLayout_CV_Name` | `CV_Name` | `txt_DDM_MenuLayout_CV_Name` @ `DDM_LayoutConfig_CV_C2` | ✅ |

Note: runtime `DashboardName` for CubeView is `DDM_App_Content_DB` in the registry, but the runtime XFBR resolves CubeView to `DDM_App_Content_CV` — the registry value is informational only on the runtime side, so this inconsistency is latent, not live.

**LayoutType 3 — Dashboard_TopBottom** (form `DDM_LayoutConfig_TB_DB`/`_C2`, runtime `DDM_App_Content_TB_DB`)

| # | Registry parameter | Column | Actual editor / actual parameter | Status |
|---|---|---|---|---|
| 0 | `IV_DDM_MenuLayout_SortOrder` | `SortOrder` | `txt_DDM_Menu_SortOrder` | ✅ |
| 1 | `IV_DDM_MenuLayout_Name` | `Name` | `txt_DDM_Menu_Name` | ✅ |
| 2 | `IV_DDM_MenuLayout_T_Height` | `T_Height` | param exists, but `txt_DDM_MenuLayout_T_Height` is bound to `IV_DDM_MenuLayout_SortOrder` | ⚠ D7 |
| 3 | `DL_DDM_MenuLayout_T_Content_Type` | `T_ContentType` | actual param is `DL_DDM_MenuLayout_T_ContentType` (`cbx_DDM_MenuLayout_T_ContentType` @ `_TB_DB_C2`) | ⚠ D8 |
| 4 | `DL_DDM_MenuLayout_T_Name` | `T_Name` | actual param is `IV_DDM_MenuLayout_T_Name` (`txt_DDM_MenuLayout_Name_Top` @ `_TB_DB_C2`) | ⚠ D8 |
| 5 | `DL_DDM_MenuLayout_B_ContentType` | `B_ContentType` | `cbx_DDM_MenuLayout_B_ContentType` @ `_TB_DB_C2` | ✅ |
| 6 | `DL_DDM_MenuLayout_B_Name` | `B_Name` | actual param is `IV_DDM_MenuLayout_B_Name` (`txt_DDM_MenuLayout_B_Name` @ `_TB_DB_C2`) | ⚠ D8 |
| 7 | `DL_DDM_MenuLayout_Status` | `Status` | `cbx_DDM_MenuLayout_Status` | ✅ |

**LayoutType 4 — Dashboard_LeftRight** (form `DDM_LayoutConfig_LR_DB`/`_C2`, runtime `DDM_App_Content_LR_DB`)

| # | Registry parameter | Column | Actual editor / actual parameter | Status |
|---|---|---|---|---|
| 0 | `IV_DDM_MenuLayout_L_Width` | `L_Width` | param exists; no component binds it (see D7) | ⚠ |
| 1 | `DL_DDM_MenuLayout_L_ContentType` | `L_ContentType` | `cbx_DDM_MenuLayout_L_Content_Type` @ `_1L2R_DB_C2` | ✅ |
| 2 | `DL_DDM_MenuLayout_L_Name` | `L_Name` | actual param is `IV_DDM_MenuLayout_L_Name` (`txt_DDM_MenuLayout_Name_Left`) | ⚠ D8 |
| 3 | `DL_DDM_MenuLayout_R_ContentType` | `R_ContentType` | `cbx_DDM_MenuLayout_R_ContentType` (not placed on any dashboard) | ⚠ D9 |
| 4 | `DL_DDM_MenuLayout_R_Name` | `R_Name` | actual param is `IV_DDM_MenuLayout_R_Name` (`txt_DDM_MenuLayout_Name_Right`, not placed) | ⚠ D8/D9 |

Note: this layout's registry omits `SortOrder`/`Name`/`Status` rows — they are still written by the fixed-field code, so only the registry is inconsistent, not the save.

**LayoutType 5 — Dashboard_2Top1Bottom** (form `DDM_LayoutConfig_2T1B_DB`, runtime `DDM_App_Content_2T1B_DB`)

| # | Registry parameter | Column | Status |
|---|---|---|---|
| 0 | `IV_DDM_MenuLayout_L_Width` | `L_Width` | ⚠ no editor bound |
| 1 | `DL_DDM_MenuLayout_TL_ContentType` | `TL_ContentType` | ✅ param + `cbx_DDM_MenuLayout_TL_ContentType` exist (combo not placed on a dashboard — D9) |
| 2 | `DL_DDM_MenuLayout_TL_Name` | `TL_Name` | ⚠ actual param `IV_DDM_MenuLayout_TL_Name` (`txt_DDM_MenuLayout_Name_TopLeft`, not placed) — D8/D9 |
| 3 | `DL_DDM_MenuLayout_CV_Name_Left` | `CV_Name_Left` | ⚠ parameter does not exist — D10 |
| 4 | `DL_DDM_MenuLayout_Right_Content_Type` | `Right_Option_Type` | ⚠ parameter does not exist — D10 |
| 5 | `DL_DDM_MenuLayout_DB_Name_Right` | `DB_Name_Right` | ⚠ parameter does not exist — D10 |
| 6 | `DL_DDM_MenuLayout_CV_Name_Right` | `CV_Name_Right` | ⚠ parameter does not exist — D10 |

**LayoutTypes 6–10 — Dashboard_1Top2Bottom / 2Left1Right / 1Left2Right / 2x2 / CustomDB** (forms `DDM_LayoutConfig_1T2B_DB` / `_2L1R_DB` / `_1L2R_DB` / `_2x2_DB` / `_CustomDB` + `_C2`; runtime `DDM_App_Content_1T2B_DB` / `_2L1R_DB` / `_1L2R_DB` / `_2x2_DB` / `_DB`)

All five share the identical registry block, and **all seven rows reference parameters that do not exist** (D10):

| # | Registry parameter | Column |
|---|---|---|
| 0 | `IV_DDM_MenuLayout_Left_Width` | `L_Width` |
| 1 | `DL_DDM_MenuLayout_Left_Content_Type` | `L_ContentType` |
| 2 | `DL_DDM_MenuLayout_DB_Name_Left` | `DB_Name_Left` |
| 3 | `DL_DDM_MenuLayout_CV_Name_Left` | `CV_Name_Left` |
| 4 | `DL_DDM_MenuLayout_Right_Content_Type` | `R_ContentType` |
| 5 | `DL_DDM_MenuLayout_DB_Name_Right` | `DB_Name_Right` |
| 6 | `DL_DDM_MenuLayout_CV_Name_Right` | `CV_Name_Right` |

What the UI *actually* edits for these layouts (parameters + components that exist but are **never mapped to a column by any registry entry**):

| Parameter | Editor | Placed on |
|---|---|---|
| `DL_DDM_MenuLayout_TL_ContentType` / `IV_DDM_MenuLayout_TL_Name` | `cbx_DDM_MenuLayout_TL_ContentType` / `txt_DDM_MenuLayout_Name_TopLeft` | (not placed) |
| `DL_DDM_MenuLayout_TR_ContentType` / `IV_DDM_MenuLayout_TR_Name` | `cbx_DDM_MenuLayout_TR_ContentType` / `txt_DDM_MenuLayout_TR_Name` | `DDM_LayoutConfig_1L2R_DB_C2` |
| `DL_DDM_MenuLayout_BL_ContentType` / `IV_DDM_MenuLayout_BL_Name` | `cbx_DDM_MenuLayout_BL_ContentType` / `txt_DDM_MenuLayout_Name_BottomLeft` | (not placed) |
| `DL_DDM_MenuLayout_BR_ContentType` / `IV_DDM_MenuLayout_BR_Name` | `cbx_DDM_MenuLayout_BR_ContentType` / `txt_DDM_MenuLayout_Name_BottomRight` | `DDM_LayoutConfig_1L2R_DB_C2` |
| `IV_DDM_MenuLayout_CustomDB_Header_Name` / `_Content_Name` | `txt_DDM_MenuLayout_CustomDB_Header` / `_Content` | `DDM_LayoutConfig_CustomDB` |

**Net effect:** only the single-pane (`Dashboard`, `CubeView`) and partially the `TopBottom`/`LeftRight` layouts can persist their pane configuration today. The three-pane, 2x2, and CustomDB layouts collect input the save never writes, and their registry rows write empty strings into `*_Left`/`*_Right` columns.

### 2.3 Header config (target table `DDM_DynDBHdrConfig`; save currently writes `DDM_HdrConfigs` — D1)

Selector: **`lbx_DDM_Hdr`** @ `DDM_HdrConfig_C1`, bound to **`BL_DDM_HdrConfigs`** (methodQuery `Get_Config_Hdrs`, reads `DDM_DynDBHdrConfig` by `DynDBMenuID`). Type picker: **`cbx_DDM_HdrType`** @ `DDM_HdrConfig_C2R1` → **`DL_DDM_HdrType`** (`Filter, Button`). XFBR `Get_HdrDB`/`Get_ConfigHdrTypeDB` routes `_Blank`/`_AddUpdate` and picks `HdrRegistry.DashboardName` (`DDM_HdrConfig_Fltr` or `DDM_HdrConfig_Btn`).

**Fixed columns written by `HdrConfig_SaveAdd`:**

| Column | Source parameter | Editor @ dashboard | Status |
|---|---|---|---|
| `DDM_HdrID` | new via `Get_Max_ID("DDM_HdrConfigs")` | — | ⚠ legacy table (D1) |
| `DDM_MenuID` | `BL_DDM_MenuLayoutConfig` | `lbx_DDM_MenuLayout` | OK |
| `DDM_ConfigID` | `IV_DDM_ConfigID` | from STE selection | OK |
| `Name` | `IV_DDM_Hdr_Name` | `txt_DDM_Hdr_Name` @ `DDM_HdrConfig_AddUpdate_R1R1C2` | ✅ |
| `SortOrder` | `IV_DDM_Hdr_SortOrder` | `txt_DDM_Hdr_SortOrder` @ `DDM_HdrConfig_AddUpdate_R1R1C2` | ✅ |
| `Option_Type` + `HdrType` | `DL_DDM_Hdr_Type` (code) | actual param is `DL_DDM_HdrType` | ⚠ D2 — always reads 0 |
| `Status` | hardcoded `"In Process"` | — | OK |
| audit quartet | stamped every save | — | ⚠ D11 — CreateDate/User overwritten on updates |

**HdrType 1 — Filter** (`HdrRegistry`, form `DDM_HdrConfig_Fltr`):

| # | Registry parameter | Column | Editor @ dashboard | Status |
|---|---|---|---|---|
| 0 | `IV_DDM_Hdr_Name` | `Name` | `txt_DDM_Hdr_Name` | ✅ |
| 1 | `DL_DDM_Hdr_Fltr_Type` | `Fltr_Type` | `cbx_DDM_Hdr_Fltr_Type` @ `DDM_HdrConfig_Fltr` | ✅ |
| 2 | `DL_DDM_Hdr_Fltr_DimType` | `Fltr_DimType` | `cbx_DDM_Hdr_Fltr_DimType` @ `DDM_HdrConfig_Fltr_R1` | ✅ |
| 3 | `BL_DDM_Hdr_Fltr_DimName` | `Fltr_DimName` | `cbx_DDM_Hdr_Fltr_DimName` @ `DDM_HdrConfig_Fltr_R1` | ✅ |
| 4 | `IV_DDM_Hdr_DependencyTier` | `Fltr_DependencyTier` | `txt_DDM_Hdr_Dependency_Tier` @ `DDM_HdrConfig_Fltr_R1` | ✅ |
| 5,6 | (commented out) | `Fltr_MFB`, `Fltr_Default` | `txt_DDM_Hdr_Fltr_MFB` / `txt_DDM_Hdr_Fltr_Default` exist (bound to `IV_DDM_Hdr_Fltr_MFB` / `_Default`, not placed) | ⚠ member filter + default never saved |
| 7 | `IV_DDM_Hdr_Fltr_Btn` | `Fltr_Btn` | `chk_DDM_Hdr_Fltr_Btn` @ `DDM_HdrConfig_AddUpdates_Filter_R2C2` | ✅ (but `chk_DDM_Hdr_Fltr_Txt` is bound to the same param — D12) |
| 8 | `IV_DDM_Hdr_Fltr_Btn_Lbl` | `Fltr_Btn_Lbl` | actual param is `IV_DDM_Hdr_Fltr_BtnLbl` (`txt_DDM_Hdr_Fltr_Btn_Lbl`) | ⚠ D8 |
| 9 | `IV_DDM_Hdr_Fltr_Btn_ToolTip` | `Fltr_Btn_ToolTip` | actual param is `IV_DDM_Hdr_Fltr_BtnToolTip` (`txt_DDM_Hdr_Fltr_Btn_ToolTip`) | ⚠ D8 |

**HdrType 2 — Button** (form `DDM_HdrConfig_Btn`):

| # | Registry parameter | Column | Editor @ dashboard | Status |
|---|---|---|---|---|
| 0 | `IV_DDM_Hdr_Name` | `Name` | `txt_DDM_Hdr_Name` | ✅ |
| 1 | `DL_DDM_Hdr_Btn_Type` | `Btn_Type` | `cbx_DDM_Hdr_Btn_Type` @ `DDM_HdrConfig_Btn_R1` | ✅ |
| 2 | `IV_DDM_Hdr_Btn_Lbl` | `Btn_Lbl` | `txt_DDM_Hdr_Btn_Lbl` @ `DDM_HdrConfig_Btn_R2` | ✅ |
| 3 | `IV_DDM_Hdr_Btn_ToolTip` | `Btn_ToolTip` | `txt_DDM_Hdr_Btn_ToolTip` @ `DDM_HdrConfig_Btn_R2` | ✅ |

The button **action** sub-form (`DDM_HdrConfig_Action_R1..R5`) collects `DL_DDM_Hdr_ActionSave`, `DL_DDM_Hdr_Btn_ActionPOV`, `DL_DDM_Hdr_Btn_ActionServerTask`, `DL_DDM_Hdr_Btn_ActionUIChanged` (+ ~20 `IV_DDM_Hdr_Action*` / `IV_DDM_Hdr_Btn_FileExp*` / dialog-map params) — **none are in `HdrRegistry`**, so no button action configuration is persisted (⚠ D13). The runtime `DDM_Header.addHeaderItems` expects `Btn_Action*` columns to wire server tasks/save/POV/nav actions.

### 2.4 Runtime consumption (column → runtime dashboard object)

| Config column | Runtime consumer | Runtime object produced |
|---|---|---|
| `DDM_DynDBConfig.WFPKey` | `DDM_LoadDB.setMenuOption`, `Get_App_Menu` | scopes the menu to the user's workflow profile |
| `MenuLayoutConfig.Name` | `BL_DDM_AppMenu` display member | menu entry in `lbx_DDM_AppMenu`; title `lbl_DDM_DynDB_Hdr` (`|!!BL_DDM_AppMenu!!|`) |
| `MenuLayoutConfig.SortOrder` | `Get_App_Menu ORDER BY` | menu ordering |
| `MenuLayoutConfig.LayoutType` | XFBR `DDM_UI.Get_LayoutDB` → `Resolve_Layout_Dashboard` | which `DDM_App_Content_<code>_DB` shell embeds |
| `MenuLayoutConfig.DB_Name` | `DDM_Content.try_BindEmbeddedDashboard` | `Embedded <DB_Name>` component in the pane |
| `MenuLayoutConfig.CV_Name` | `DDM_Content.try_BindCubeView` | `cv_DDM_Dynamic_App_Content*` binding |
| `MenuLayoutConfig.T/B/L/R/TL/TR/BL/BR_ContentType`, `*_Name`, `T_Height`, `L_Width` | `DDM_Support.get_PaneBinding` / `resolve_PaneName` | per-pane dashboard-vs-cubeview choice and sizing |
| `HdrConfig.HdrType` | `DDM_Header.addHeaderItems` | Filter set vs Button in `DDM_App_Hdr_C2C1` |
| `HdrConfig.Fltr_DimType/Fltr_DimName/Fltr_DependencyTier` | `DDM_Header.get_DynamicHdr` | `btn_DDM_App_MbrList<Dim>` member-select buttons + `ML_DDM_App_*` member lists (`~!Mbr_List_*!~` template tokens) |
| `HdrConfig.Fltr_Btn/Fltr_Btn_Lbl/Fltr_Btn_ToolTip` | `DDM_Header.buildButtonXML` | filter apply-button text/tooltip (`~!btn_Text!~`, `~!btn_ToolTip!~`) |
| `HdrConfig.Btn_Type/Btn_Lbl/Btn_ToolTip` + `Btn_Action*` | `DDM_Header.addHeaderItems` | generic header button (`btn_DDM_App_Btn` pattern, `IV_DDM_App_Generic_DBExt_1_*` params) |

Runtime parameter surface (23 params): `BL_DDM_AppMenu` (menu), 6 `IV_DDM_App_*` (menu chrome + `_Copy` holders), 16 `ML_DDM_App_*` member lists — still in three naming generations (`<Dim>MbrList`, `MbrList<Dim>`, `<Dim>_Mbr_List_Copy`), which the runtime name-construction code must chase (see D14).

---

## 3. Defect log (all verified in this export)

| # | Defect | Where | Effect |
|---|---|---|---|
| D1 | Header save writes legacy `DDM_HdrConfigs` (+ reads legacy `DDM_Config` for the parent row), while the header list (`Get_Config_Hdrs`), select handler, and the runtime all read `DDM_DynDBHdrConfig` | `HdrConfig_SaveAdd` | **Saved headers never appear** in the admin list or the runtime header bar |
| D2 | Save reads `DL_DDM_Hdr_Type`; the real parameter is `DL_DDM_HdrType` | `HdrConfig_SaveAdd` | `HdrType`/`Option_Type` always 0 → registry lookup fails → no Filter/Button fields are ever mapped |
| D3 | Header duplicate check re-uses the *menu* dictionaries and error text | `Duplicate_Hdr_Check` usage | wrong duplicate semantics/messages for headers |
| D4 | `newWFPConfigID` is hardcoded 0 and `Get_Max_ID` is never called; `DynDBConfigID` never set on the new row | `ConfigWFP_SaveAdd` | new WFP config rows get no surrogate key (insert fails or writes 0/NULL) |
| D5 | Update path fills with parameter `@Menu_ID` but the SQL declares `@DDM_MenuID` | `MenuLayoutConfig_Save` | menu **updates** throw a SQL parameter error |
| D6 | Fixed-field code reads `IV_DDM_Menu_SortOrder` (nonexistent); registry separately maps `IV_DDM_MenuLayout_SortOrder` to the same column | `MenuLayoutConfig_Save` | sort order silently 0 unless the registry row also fires |
| D7 | Misbound editors: `txt_DDM_MenuLayout_T_Height` and `txt_DDM_LayoutConfig_L_Width` are bound to `IV_DDM_MenuLayout_SortOrder`; `cbx_DDM_DB_Status`/`cbx_DDM_DB_Hdr_Status` bound to `DL_DDM_LayoutConfigType` | components | height/width inputs overwrite sort order; status combos overwrite layout type |
| D8 | Registry ↔ parameter naming drift: `_Content_Type` vs `_ContentType`, `DL_` vs `IV_` for pane names, `Left_`/`Right_` vs `L_`/`R_`, `Btn_Lbl` vs `BtnLbl` | `DDM_ConfigHelpers` | mapped reads return empty → columns saved blank |
| D9 | Editors exist but are placed on no dashboard (or the wrong one — quadrant combos sit on the 1L2R form) | `cbx_DDM_MenuLayout_R/TL/BL_*`, `txt_..._Name_Right/TopLeft/BottomLeft` | fields cannot be entered for the layouts that need them |
| D10 | Registry rows for layouts 5–10 reference 7 parameters that don't exist anywhere (`*_Left`/`*_Right`/`Left_Width` family) | `LayoutRegistry` | 3-pane/2x2/CustomDB pane config is never persisted |
| D11 | `CreateDate`/`CreateUser` stamped on every header save, including updates | `HdrConfig_SaveAdd` | creation audit destroyed on edit |
| D12 | `chk_DDM_Hdr_Fltr_Txt` bound to `IV_DDM_Hdr_Fltr_Btn` (should be `IV_DDM_Hdr_Fltr_Txt`) | components | textbox flag overwrites button flag |
| D13 | Button action params (`DL_DDM_Hdr_*Action*`, `IV_DDM_Hdr_Action*`, FileExp set) collected by `DDM_HdrConfig_Action_R1..R5` are absent from `HdrRegistry` | registry | header button actions never saved, though runtime expects `Btn_Action*` columns |
| D14 | Runtime `ML_` params still in three naming generations; `DDM_Header` builds names dynamically in all three formats | runtime unit | fragile lookups; misses possible per dimension |
| D15 | FMM/MCM copy-paste leftovers inside DDM: `paramMap` holds 6 `BL_FMM_*→IV_FMM_*` pairs, `DDMColFormatter["DDM_ConfigOPDB"]` defines FMM calc-grid columns (`Cube_ID`, `MbrList_*`, `BR_Calc`) with `IV_FMM_*` defaults | `DDM_ConfigLoadDB`, `DDM_ConfigUI` | dead/misleading config; OPDB grid formats a table this module doesn't own |
| D16 | Debug noise in production paths: a loop logging **every** subst var per registry row, `Hit584`-style messages, large commented-out blocks retained | `DDM_ConfigData` | log spam, slower saves, unreadable code |
| D17 | Menu-save duplicate check only guards `runType == "Add"`, and the name check throws even when updating the same row's own name | `MenuLayoutConfig_Save` | renaming to a name that equals its own current value fails on update paths that pass "Add" semantics |

Bright spots vs older copies of this module: `DDM_Config_Migration` is now a real extender with working `Export_DDM_Config`/`Import_DDM_Config`, the runtime XFBR is cleaned up (no undeclared-variable bug), and `Fltr_DependencyTier` (cascading filter tiers) is a genuine new capability.

---

## 4. Wholesale changes recommended for maintainability

Ordered by leverage; the first three eliminate whole *classes* of the defects above rather than individual instances.

1. **Make the registry the only mapping, and make it self-validating.** Today the mapping lives in four places that can drift independently: the registry, the fixed-field save code, the component `boundParameterName`s, and the parameter list. Collapse to one: put *every* column (including `SortOrder`, `Name`, `Status`, FKs, and header `Btn_Action*`) into `ParameterMappings`, drop the hand-written fixed-field blocks, and add a startup/save-time validation that (a) every registry parameter exists in the workspace, and (b) every registry column exists in the target table — log or throw on mismatch. D2, D6, D8, D10, D13 all become impossible or loudly visible instead of silently writing blanks. The ordered `Dictionary<int, Dictionary<string,string>>` adds nothing — a flat ordered list of `(param, column)` records is simpler and removes the double-nesting everywhere it's consumed.

2. **Kill the parameter-name matrix with one naming law and one generator.** Nearly half the broken rows are pure naming drift (`_ContentType` vs `_Content_Type`, `L_` vs `Left_`, `BtnLbl` vs `Btn_Lbl`). Adopt the pane-code convention (`T/B/L/R/TL/TR/BL/BR`) as canonical and generate the per-pane params/columns mechanically: `IV_DDM_MenuLayout_<Pane>_Name`, `DL_DDM_MenuLayout_<Pane>_ContentType`, `IV_DDM_MenuLayout_<Pane>_CV_Name`. Because all multi-pane layouts are just sets of panes, the ten `LayoutRegistry` entries reduce to *one* generic entry plus a `LayoutType → pane-set` table (`2x2 → [TL,TR,BL,BR]`, `1L2R → [L,TR,BR]`, …). That also lets one shared pane sub-dashboard replace the ten near-duplicate `DDM_LayoutConfig_*` forms — the same registry drives which pane editors appear.

3. **Finish the table migration and delete the legacy path.** One generation of tables (`DDM_DynDBConfig` / `DDM_DynDBMenuLayoutConfig` / `DDM_DynDBHdrConfig`), one save path. Point `HdrConfig_SaveAdd` at `DDM_DynDBHdrConfig`, remove all reads of `DDM_Config`/`DDM_Config_Menu`/`DDM_HdrConfigs`, and add a `SolutionTableSetup()` + `<Sol>_TableSetup.txt` DDL resource so installs are deterministic (this unit currently has no DDL anywhere; the migration extender's import assumes the tables exist). D1 is the single highest-impact fix in the module — headers cannot round-trip today.

4. **Extract one generic `SaveConfigRow` routine.** `MenuLayoutConfig_Save` and `HdrConfig_SaveAdd` are the same algorithm (resolve ID → new-or-load row → fixed fields → registry mapping → validate → `UpdateTable`) with independently mutated copies. One shared method taking `(tableName, keyColumn, registryEntry, substVars)` removes the duplicated audit stamping (fixing D11 in one place: set Create* only when `runType == Add`), the duplicated duplicate-checks (give headers their own dictionaries — D3), and the inconsistent value sources (pick **one** of `CustomSubstVars` vs `GetLiteralParameterValue` and use it everywhere).

5. **Adopt a "bind = param name" lint.** Every misbinding in D7/D12 is a component whose `boundParameterName` disagrees with its own name (`txt_DDM_MenuLayout_T_Height` → `IV_DDM_MenuLayout_SortOrder`). Enforce the convention that `<prefix>_DDM_<X>` binds `IV/DL/BL_DDM_<X>` and scan the workspace XML for violations (a 20-line script over `component name` vs `boundParameterName`); run it before every import. The same scan catches unplaced editors (D9) by diffing component definitions against dashboard `componentMember` lists.

6. **Strip debug scaffolding and dead code behind a flag.** Replace the ad-hoc `BRApi.ErrorLog.LogMessage(si,"Hit584")` calls and the per-save loop that dumps every substitution variable with a single `if (isDebug)` guard (an `LV_DDM_Debug` literal), and delete the commented-out blocks — the export/import extender means old versions live in source control/exports, not in comments. This alone shrinks `DDM_ConfigData` by roughly a third and makes the real logic reviewable.

7. **Remove cross-solution bleed.** Move the `BL_FMM_*` pairs out of `paramMap` and delete the FMM `DDM_ConfigOPDB` column set (D15). If OPDB (stand-alone dashboards) is a real DDM feature, give it its own registry entry and grid definition; if not, remove the section until it is — half-present features are the most expensive kind to maintain.

8. **Unify the runtime `ML_` member-list params** to one naming generation (`ML_DDM_App_<Dim>MbrList`), delete the `_Copy` designer artifacts, and change `DDM_Header` to build the name one way. This removes the triple-format name construction (D14) and makes adding a dimension a one-line change.

With items 1–3 in place, the module reaches the state its design intends: adding a layout or header type = one registry entry + one (generated) pane form, with zero new save code — and any future drift fails fast at validation instead of silently writing empty config.
