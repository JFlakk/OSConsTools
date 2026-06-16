"""
OneStream Workspace XML Generators
===================================
Each generator class produces an indented XML fragment that can be
composed into a full workspace XML file using composer.py.

Naming conventions enforced:
  BL_   BoundList parameter
  IV_   InputValue parameter
  ML_   MemberList parameter
  LV_   LiteralValue parameter
  DL_   DelimitedList parameter
  btn_  Button component
  cbx_  ComboBox component
  lbx_  ListBox component
  cv_   CubeView component
  lbl_  Label component
"""

from __future__ import annotations

import textwrap
from dataclasses import dataclass, field
from typing import List, Optional

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

INDENT = "    "   # four spaces — matches the existing workspace XMLs

_PARAM_TYPE_PREFIXES = {
    "BoundList":      "BL_",
    "InputValue":     "IV_",
    "MemberList":     "ML_",
    "LiteralValue":   "LV_",
    "DelimitedList":  "DL_",
}


def _ind(depth: int) -> str:
    return INDENT * depth


def _e(tag: str, value: str = "", attrs: Optional[dict] = None, depth: int = 0) -> str:
    """Render a simple element: <tag attr="v">value</tag>."""
    a = ""
    if attrs:
        a = " " + " ".join(f'{k}="{v}"' for k, v in attrs.items())
    prefix = _ind(depth)
    if value == "":
        return f"{prefix}<{tag}{a} />"
    return f"{prefix}<{tag}{a}>{value}</{tag}>"


def _open(tag: str, attrs: Optional[dict] = None, depth: int = 0) -> str:
    a = ""
    if attrs:
        a = " " + " ".join(f'{k}="{v}"' for k, v in attrs.items())
    return f"{_ind(depth)}<{tag}{a}>"


def _close(tag: str, depth: int = 0) -> str:
    return f"{_ind(depth)}</{tag}>"


def _cdata(value: str) -> str:
    return f"<![CDATA[{value}]]>"


# ---------------------------------------------------------------------------
# Parameter Generator
# ---------------------------------------------------------------------------

@dataclass
class ParameterSpec:
    """Specification for a single OneStream workspace parameter."""
    parameter_type: str           # BoundList | InputValue | MemberList | LiteralValue | DelimitedList
    name: str                     # Full parameter name e.g. "BL_DDM_MyList"
    description: str = ""
    user_prompt: str = ""
    sort_order: int = 100
    default_value: str = ""

    # BoundList / SQL fields
    parameter_command_type: str = "Unknown"   # Unknown | Method | SQL
    sql_query: str = ""
    db_location: str = "Application"
    external_db_conn_name: str = ""
    method_type: str = "Unknown"              # Unknown | BusinessRule | SQL
    method_query: str = ""
    results_table_name: str = ""
    display_member: str = ""
    value_member: str = ""

    # DelimitedList fields
    display_items: str = ""
    value_items: str = ""

    # MemberList fields
    cube_name: str = ""
    dim_type: str = ""
    dim_name: str = ""
    member_filter: str = ""

    # Formatting
    result_format_string_type: str = "Default"
    result_custom_format_string: str = ""


def generate_parameter(spec: ParameterSpec, depth: int = 7) -> str:
    """Return indented <parameter> XML fragment."""
    expected_prefix = _PARAM_TYPE_PREFIXES.get(spec.parameter_type, "")
    if expected_prefix and not spec.name.startswith(expected_prefix):
        raise ValueError(
            f"Parameter name '{spec.name}' must start with '{expected_prefix}' "
            f"for type '{spec.parameter_type}'."
        )

    lines: List[str] = []
    lines.append(_open("parameter", {
        "name": spec.name,
        "description": spec.description,
        "userPrompt": spec.user_prompt,
        "parameterType": spec.parameter_type,
        "sortOrder": str(spec.sort_order),
    }, depth))
    lines.append(_e("defaultValue", spec.default_value, depth=depth + 1))
    lines.append(_e("resultFormatStringType", spec.result_format_string_type, depth=depth + 1))
    lines.append(_e("resultCustomFormatString", spec.result_custom_format_string, depth=depth + 1))
    lines.append(_e("parameterCommandType", spec.parameter_command_type, depth=depth + 1))
    lines.append(_e("sqlQuery", spec.sql_query, depth=depth + 1))
    lines.append(_e("dbLocation", spec.db_location, depth=depth + 1))
    lines.append(_e("externalDBConnName", spec.external_db_conn_name, depth=depth + 1))
    lines.append(_e("methodType", spec.method_type, depth=depth + 1))
    lines.append(_e("methodQuery", spec.method_query, depth=depth + 1))
    lines.append(_e("resultsTableName", spec.results_table_name, depth=depth + 1))
    lines.append(_e("displayMember", spec.display_member, depth=depth + 1))
    lines.append(_e("valueMember", spec.value_member, depth=depth + 1))
    lines.append(_e("displayItems", spec.display_items, depth=depth + 1))
    lines.append(_e("valueItems", spec.value_items, depth=depth + 1))
    lines.append(_e("cubeName", spec.cube_name, depth=depth + 1))
    lines.append(_e("dimType", spec.dim_type, depth=depth + 1))
    lines.append(_e("dimName", spec.dim_name, depth=depth + 1))
    lines.append(_e("memberFilter", spec.member_filter, depth=depth + 1))
    lines.append(_close("parameter", depth))
    return "\n".join(lines)


# ---------------------------------------------------------------------------
# Dashboard Generator
# ---------------------------------------------------------------------------

@dataclass
class ColumnDef:
    column_type: str = "Component"   # Component | Spacer
    width: str = "*"                 # *, Auto, or pixel value


@dataclass
class RowDef:
    row_type: str = "Component"      # Component | Spacer
    height: str = "*"                # *, Auto, or pixel value


@dataclass
class ComponentMember:
    name: str
    left: str = ""
    top: str = ""
    width: str = ""
    height: str = ""
    dock_position: str = "Left"


@dataclass
class DashboardSpec:
    """Specification for a single OneStream dashboard."""
    name: str
    dashboard_type: str = "Embedded"   # TopLevel | Embedded | EmbeddedDynamic | EmbeddedTopLevelWithoutParameterPrompts
    layout_type: str = "Grid"          # Grid | Uniform | VerticalStackPanel
    description: str = ""
    page_caption: str = ""
    is_initially_visible: bool = True
    load_dashboard_task_type: str = "NoTask"   # NoTask | ExecuteDashboardExtenderBRAllActions | ...
    load_dashboard_task_args: str = ""
    show_title: bool = True
    display_format: str = ""
    notes: str = ""
    scroll_position: str = "Default"
    columns: List[ColumnDef] = field(default_factory=lambda: [ColumnDef(width="*")])
    rows: List[RowDef] = field(default_factory=lambda: [RowDef(height="*")])
    component_members: List[ComponentMember] = field(default_factory=list)


def generate_dashboard(spec: DashboardSpec, depth: int = 6) -> str:
    """Return indented <dashboard> XML fragment."""
    lines: List[str] = []
    attrs: dict = {
        "name": spec.name,
        "description": spec.description,
        "pageCaption": spec.page_caption,
        "dashboardType": spec.dashboard_type,
        "layoutType": spec.layout_type,
        "isInitiallyVisible": str(spec.is_initially_visible).lower(),
        "loadDashboardTaskType": spec.load_dashboard_task_type,
    }
    lines.append(_open("dashboard", attrs, depth))

    if spec.load_dashboard_task_args:
        lines.append(_e("loadDashboardTaskArgs", spec.load_dashboard_task_args, depth=depth + 1))
    if spec.display_format:
        lines.append(_e("displayFormat", spec.display_format, depth=depth + 1))

    lines.append(_open("DashboardDefinition", depth=depth + 1))
    lines.append(_e("ShowTitle", str(spec.show_title).lower(), depth=depth + 2))
    lines.append(_e("Notes", spec.notes, depth=depth + 2) if spec.notes else f"{_ind(depth+2)}<Notes />")
    lines.append(_open("DynamicDashboardDefinition", depth=depth + 2))
    lines.append(f"{_ind(depth+3)}<ComponentTemplateRepeatItems />")
    lines.append(_close("DynamicDashboardDefinition", depth + 2))
    lines.append(f'{_ind(depth+2)}<CustomControlDbrdDefinition RequiredInputParameters="" />')

    lines.append(_open("GridLayoutDefinition", depth=depth + 2))
    lines.append(_open("ColumnDefinitions", depth=depth + 3))
    for col in spec.columns:
        lines.append(f'{_ind(depth+4)}<ColumnDefinition ColumnType="{col.column_type}" Width="{col.width}" />')
    lines.append(_close("ColumnDefinitions", depth + 3))
    lines.append(_open("RowDefinitions", depth=depth + 3))
    for row in spec.rows:
        lines.append(f'{_ind(depth+4)}<RowDefinition RowType="{row.row_type}" Height="{row.height}" />')
    lines.append(_close("RowDefinitions", depth + 3))
    lines.append(_close("GridLayoutDefinition", depth + 2))

    lines.append(_e("ScrollPosition", spec.scroll_position, depth=depth + 2))
    lines.append(_close("DashboardDefinition", depth + 1))

    if spec.component_members:
        lines.append(_open("componentMembers", depth=depth + 1))
        for cm in spec.component_members:
            lines.append(
                f'{_ind(depth+2)}<componentMember name="{cm.name}" left="{cm.left}" '
                f'top="{cm.top}" width="{cm.width}" height="{cm.height}" '
                f'dockPosition="{cm.dock_position}" />'
            )
        lines.append(_close("componentMembers", depth + 1))
    else:
        lines.append(f"{_ind(depth+1)}<componentMembers />")

    lines.append(_close("dashboard", depth))
    return "\n".join(lines)


# ---------------------------------------------------------------------------
# Business Rule / Workspace Assembly Generator
# ---------------------------------------------------------------------------

@dataclass
class BRFileSpec:
    """A single .cs file within a workspace assembly."""
    name: str                         # e.g. "MyHelper.cs"
    source_code: str                  # Raw C# source
    file_type: str = "CSharp"
    business_rule_type: str = "Unknown"
    compiler_action_type: str = "Compile"   # Unknown | Compile | CompileAndRun
    is_encrypted: bool = False


@dataclass
class BRFolderSpec:
    name: str
    files: List[BRFileSpec] = field(default_factory=list)


@dataclass
class BusinessRuleSpec:
    """Specification for a workspaceAssembly node."""
    assembly_name: str                      # e.g. "DDM_UI_Assembly"
    description: str = ""
    compiler_language: str = "CSharp"
    folders: List[BRFolderSpec] = field(default_factory=list)
    dependencies: List[str] = field(default_factory=list)


def generate_business_rule(spec: BusinessRuleSpec, depth: int = 6) -> str:
    """Return indented <workspaceAssembly> XML fragment."""
    lines: List[str] = []
    lines.append(_open("workspaceAssembly", {
        "name": spec.assembly_name,
        "description": spec.description,
        "compilerLanguage": spec.compiler_language,
    }, depth))

    if spec.dependencies:
        lines.append(_open("dependencies", depth=depth + 1))
        for dep in spec.dependencies:
            lines.append(_e("dependency", dep, depth=depth + 2))
        lines.append(_close("dependencies", depth + 1))
    else:
        lines.append(f"{_ind(depth+1)}<dependencies />")

    lines.append(_open("files", depth=depth + 1))
    for folder in spec.folders:
        lines.append(_open("folder", {"name": folder.name}, depth=depth + 2))
        for f in folder.files:
            lines.append(_open("file", {
                "name": f.name,
                "fileType": f.file_type,
                "businessRuleType": f.business_rule_type,
                "compilerActionType": f.compiler_action_type,
                "isEncrypted": str(f.is_encrypted).lower(),
            }, depth=depth + 3))
            # Embed source code as CDATA
            lines.append(f"{_ind(depth+4)}<sourceCode>{_cdata(f.source_code)}</sourceCode>")
            lines.append(_close("file", depth + 3))
        lines.append(_close("folder", depth + 2))
    lines.append(_close("files", depth + 1))

    lines.append(_close("workspaceAssembly", depth))
    return "\n".join(lines)


# ---------------------------------------------------------------------------
# Component Generator
# ---------------------------------------------------------------------------

@dataclass
class ButtonSpec:
    """Extra properties for a Button component."""
    button_type: str = "Standard"               # Standard | SelectMemberDialog
    image_file_source_type: str = "DashboardFile"
    image_url: str = ""
    dim_type_name: str = ""                     # for SelectMemberDialog
    use_all_dims_for_dim_type: bool = False


@dataclass
class CubeViewSpec:
    """Extra properties for a CubeView component."""
    cube_view_name: str = ""
    show_header: bool = True
    show_toggle_size_button: bool = False


@dataclass
class ListBoxSpec:
    """Extra properties for a ListBox component."""
    pass   # All ListBox config lives on the component attributes


@dataclass
class EmbeddedDashboardSpec:
    """Extra properties for an EmbeddedDashboard component."""
    embedded_dashboard_name: str = ""
    template_name_suffix: str = ""


@dataclass
class ComponentSpec:
    """Specification for a single OneStream dashboard component."""
    component_type: str            # Button | ComboBox | ListBox | CubeView | Label | EmbeddedDashboard
    name: str
    description: str = ""
    xf_text: str = ""
    tool_tip: str = ""
    bound_parameter_name: str = ""
    template_parameter_values: str = ""
    br_text1: str = ""
    br_text2: str = ""
    param_value_for_button_click: str = ""
    apply_param_value_to_current_dbrd: bool = True
    selection_changed_save_type: str = "NoAction"
    selection_changed_pov_action_type: str = "NoAction"
    selection_changed_task_type: str = "NoTask"
    selection_changed_ui_action_type: str = "NoAction"
    selection_changed_navigation_type: str = "NoAction"
    selection_changed_task_args: str = ""
    dashboards_to_redraw: str = ""
    dashboard_for_dialog: str = ""
    display_format: str = ""

    # Type-specific definition — supply exactly one
    button: Optional[ButtonSpec] = None
    cube_view: Optional[CubeViewSpec] = None
    list_box: Optional[ListBoxSpec] = None
    embedded_dashboard: Optional[EmbeddedDashboardSpec] = None


def generate_component(spec: ComponentSpec, depth: int = 6) -> str:
    """Return indented <component> XML fragment."""
    lines: List[str] = []

    attrs: dict = {
        "name": spec.name,
        "description": spec.description,
        "xfText": spec.xf_text,
        "toolTip": spec.tool_tip,
        "componentType": spec.component_type,
        "templateParameterValues": spec.template_parameter_values,
        "brText1": spec.br_text1,
        "brText2": spec.br_text2,
    }
    if spec.component_type not in ("Label", "EmbeddedDashboard"):
        attrs["boundParameterName"] = spec.bound_parameter_name
        attrs["applyParamValueToCurrentDbrd"] = str(spec.apply_param_value_to_current_dbrd).lower()
        attrs["selectionChangedSaveType"] = spec.selection_changed_save_type
        attrs["selectionChangedPovActionType"] = spec.selection_changed_pov_action_type
        attrs["selectionChangedTaskType"] = spec.selection_changed_task_type
        attrs["selectionChangedUIActionType"] = spec.selection_changed_ui_action_type
        attrs["selectionChangedNavigationType"] = spec.selection_changed_navigation_type
    if spec.component_type == "Button":
        attrs["paramValueForButtonClick"] = spec.param_value_for_button_click
    if spec.component_type == "EmbeddedDashboard" and spec.embedded_dashboard:
        attrs["embeddedDashboardName"] = spec.embedded_dashboard.embedded_dashboard_name
        attrs["templateNameSuffix"] = spec.embedded_dashboard.template_name_suffix

    lines.append(_open("component", attrs, depth))

    if spec.display_format:
        lines.append(_e("displayFormat", spec.display_format, depth=depth + 1))
    if spec.selection_changed_task_args:
        lines.append(_e("selectionChangedTaskArgs", spec.selection_changed_task_args, depth=depth + 1))
    if spec.dashboards_to_redraw:
        lines.append(_e("dashboardsToRedraw", spec.dashboards_to_redraw, depth=depth + 1))
    if spec.dashboard_for_dialog:
        lines.append(_e("dashboardForDialog", spec.dashboard_for_dialog, depth=depth + 1))

    # componentDefinition
    if spec.button:
        lines.append(_open("componentDefinition", depth=depth + 1))
        lines.append(_open("XFButtonDefinition", {"ButtonType": spec.button.button_type}, depth=depth + 2))
        lines.append(_e("ImageFileSourceType", spec.button.image_file_source_type, depth=depth + 3))
        lines.append(_e("ImageUrlOrFullFileName", spec.button.image_url, depth=depth + 3))
        lines.append(f"{_ind(depth+3)}<PageNumber />")
        lines.append(f"{_ind(depth+3)}<ExcelSheet />")
        lines.append(f"{_ind(depth+3)}<ExcelNamedRange />")
        if spec.button.button_type == "SelectMemberDialog":
            lines.append(_open("SelectMemberInfo", depth=depth + 3))
            lines.append(_e("DimTypeName", spec.button.dim_type_name, depth=depth + 4))
            lines.append(_e("UseAllDimsForDimType", str(spec.button.use_all_dims_for_dim_type).lower(), depth=depth + 4))
            lines.append(f"{_ind(depth+4)}<DimName />")
            lines.append(f"{_ind(depth+4)}<CubeName />")
            lines.append(f"{_ind(depth+4)}<MemberFilter />")
            lines.append(_close("SelectMemberInfo", depth + 3))
        lines.append(_close("XFButtonDefinition", depth + 2))
        lines.append(_close("componentDefinition", depth + 1))
    elif spec.cube_view:
        lines.append(_open("componentDefinition", depth=depth + 1))
        lines.append(_open("XFCubeViewDefinition", depth=depth + 2))
        lines.append(_e("CubeViewName", spec.cube_view.cube_view_name, depth=depth + 3))
        lines.append(_e("ShowHeader", str(spec.cube_view.show_header).lower(), depth=depth + 3))
        lines.append(_e("ShowToggleSizeButton", str(spec.cube_view.show_toggle_size_button).lower(), depth=depth + 3))
        lines.append(_close("XFCubeViewDefinition", depth + 2))
        lines.append(_close("componentDefinition", depth + 1))
    elif spec.embedded_dashboard:
        lines.append(f"{_ind(depth+1)}<componentDefinition />")
    else:
        # ComboBox, ListBox, Label — no componentDefinition body
        lines.append(f"{_ind(depth+1)}<componentDefinition />") if spec.component_type == "Label" else None

    lines.append(f"{_ind(depth+1)}<adapterMembers />")
    lines.append(_close("component", depth))
    return "\n".join(lines)


# ---------------------------------------------------------------------------
# Dashboard Group Generator
# ---------------------------------------------------------------------------

@dataclass
class DashboardGroupSpec:
    name: str
    description: str = ""
    access_group: str = "Administrators"
    dashboards: List[DashboardSpec] = field(default_factory=list)


def generate_dashboard_group(spec: DashboardGroupSpec, depth: int = 5) -> str:
    """Return indented <dashboardGroup> XML fragment."""
    lines: List[str] = []
    lines.append(_open("dashboardGroup", {
        "name": spec.name,
        "description": spec.description,
        "accessGroup": spec.access_group,
    }, depth))
    if spec.dashboards:
        lines.append(_open("dashboards", depth=depth + 1))
        for db in spec.dashboards:
            lines.append(generate_dashboard(db, depth=depth + 2))
        lines.append(_close("dashboards", depth + 1))
    else:
        lines.append(f"{_ind(depth+1)}<dashboards />")
    lines.append(_close("dashboardGroup", depth))
    return "\n".join(lines)
