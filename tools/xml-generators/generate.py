#!/usr/bin/env python3
"""
OneStream Workspace XML Generator — CLI
========================================
Generates OneStream workspace XML objects from structured prompt inputs.

Quick-start examples
--------------------
# Generate a BoundList parameter
python generate.py parameter \
    --type BoundList --name BL_MY_List --description "My list" \
    --method-query "{Workspace.Current.MY_Assembly.MY_DataSets}{Get_List}{}" \
    --results-table MY_Table --display-member Name --value-member ID

# Generate an InputValue parameter
python generate.py parameter \
    --type InputValue --name IV_MY_SelectedID

# Generate a dashboard (Embedded, Grid, 2 columns × 1 row)
python generate.py dashboard \
    --name MY_Content --dashboard-type Embedded --layout-type Grid \
    --columns "*,*" --rows "*" \
    --members "lbx_MY_Menu,Embedded MY_Detail_DB"

# Generate a Button component
python generate.py component \
    --type Button --name btn_MY_Action --xf-text "Run" \
    --task-type ExecuteDashboardExtenderBRConsServer \
    --task-args "{Workspace.Current.MY_Assembly.MY_BR}{Run}{}" \
    --image Std_DB_Calc.png

# Generate a CubeView component
python generate.py component \
    --type CubeView --name cv_MY_View --cube-view-name MyCubeView

# Generate a business rule assembly with one file
python generate.py businessrule \
    --assembly-name MY_Assembly --folder "My Classes" \
    --file-name MyClass.cs --compiler-action Compile \
    --source-file /path/to/MyClass.cs

# Compose a full maintenanceUnit from a JSON spec file
python generate.py compose --spec my_module.json --output my_module.xml

# Merge a generated maintenanceUnit into an existing workspace XML
python generate.py merge \
    --source existing_workspace.xml \
    --unit my_module.xml \
    --output merged_workspace.xml

Run `python generate.py <command> --help` for full options on any command.
"""

import argparse
import json
import sys
import os

# Allow running from any directory
sys.path.insert(0, os.path.dirname(__file__))

from generators import (
    ParameterSpec,
    DashboardSpec,
    ColumnDef,
    RowDef,
    ComponentMember,
    ComponentSpec,
    ButtonSpec,
    CubeViewSpec,
    EmbeddedDashboardSpec,
    BusinessRuleSpec,
    BRFolderSpec,
    BRFileSpec,
    DashboardGroupSpec,
    generate_parameter,
    generate_dashboard,
    generate_component,
    generate_business_rule,
    generate_dashboard_group,
)
from composer import (
    MaintenanceUnitSpec,
    WorkspaceSpec,
    compose_maintenance_unit,
    compose_workspace,
    merge_maintenance_unit_into_file,
)


# ---------------------------------------------------------------------------
# Sub-commands
# ---------------------------------------------------------------------------

def cmd_parameter(args) -> str:
    spec = ParameterSpec(
        parameter_type=args.type,
        name=args.name,
        description=args.description or "",
        sort_order=args.sort_order,
        default_value=args.default_value or "",
        parameter_command_type=args.command_type,
        method_type=args.method_type,
        method_query=args.method_query or "",
        results_table_name=args.results_table or "",
        display_member=args.display_member or "",
        value_member=args.value_member or "",
        display_items=args.display_items or "",
        value_items=args.value_items or "",
        cube_name=args.cube_name or "",
        dim_type=args.dim_type or "",
        dim_name=args.dim_name or "",
        member_filter=args.member_filter or "",
        sql_query=args.sql_query or "",
    )
    return generate_parameter(spec, depth=0)


def cmd_dashboard(args) -> str:
    columns = [ColumnDef(width=w.strip()) for w in (args.columns or "*").split(",")]
    rows = [RowDef(height=h.strip()) for h in (args.rows or "*").split(",")]
    members = []
    if args.members:
        for m in args.members.split(","):
            members.append(ComponentMember(name=m.strip()))

    spec = DashboardSpec(
        name=args.name,
        dashboard_type=args.dashboard_type,
        layout_type=args.layout_type,
        description=args.description or "",
        page_caption=args.page_caption or "",
        load_dashboard_task_type=args.task_type,
        load_dashboard_task_args=args.task_args or "",
        show_title=not args.hide_title,
        columns=columns,
        rows=rows,
        component_members=members,
    )
    return generate_dashboard(spec, depth=0)


def cmd_component(args) -> str:
    btn = None
    cv = None
    embedded = None

    if args.type == "Button":
        btn = ButtonSpec(
            button_type=args.button_type or "Standard",
            image_url=args.image or "",
            dim_type_name=args.dim_type_name or "",
        )
    elif args.type == "CubeView":
        cv = CubeViewSpec(cube_view_name=args.cube_view_name or "")
    elif args.type == "EmbeddedDashboard":
        embedded = EmbeddedDashboardSpec(embedded_dashboard_name=args.embedded_name or "")

    spec = ComponentSpec(
        component_type=args.type,
        name=args.name,
        description=args.description or "",
        xf_text=args.xf_text or "",
        tool_tip=args.tool_tip or "",
        bound_parameter_name=args.bound_param or "",
        display_format=args.display_format or "",
        selection_changed_task_type=args.task_type or "NoTask",
        selection_changed_task_args=args.task_args or "",
        selection_changed_ui_action_type=args.ui_action or "NoAction",
        dashboards_to_redraw=args.redraw or "",
        button=btn,
        cube_view=cv,
        embedded_dashboard=embedded,
    )
    return generate_component(spec, depth=0)


def cmd_businessrule(args) -> str:
    source_code = ""
    if args.source_file:
        with open(args.source_file, "r", encoding="utf-8") as fh:
            source_code = fh.read()
    elif args.source_inline:
        source_code = args.source_inline

    br_file = BRFileSpec(
        name=args.file_name,
        source_code=source_code,
        compiler_action_type=args.compiler_action,
    )
    folder = BRFolderSpec(name=args.folder or "Business Rules", files=[br_file])
    spec = BusinessRuleSpec(
        assembly_name=args.assembly_name,
        description=args.description or "",
        folders=[folder],
    )
    return generate_business_rule(spec, depth=0)


def cmd_compose(args) -> str:
    with open(args.spec, "r", encoding="utf-8") as fh:
        data = json.load(fh)
    mu_spec = _mu_from_dict(data)
    if args.full_workspace:
        ws_spec = WorkspaceSpec(maintenance_units=[mu_spec])
        return compose_workspace(ws_spec)
    return compose_maintenance_unit(mu_spec, depth=0)


def cmd_merge(args) -> str:
    with open(args.unit, "r", encoding="utf-8") as fh:
        mu_xml = fh.read()
    return merge_maintenance_unit_into_file(args.source, mu_xml)


# ---------------------------------------------------------------------------
# JSON → Spec helpers
# ---------------------------------------------------------------------------

def _mu_from_dict(d: dict) -> MaintenanceUnitSpec:
    """Build a MaintenanceUnitSpec from a plain JSON dict."""
    params = [_param_from_dict(p) for p in d.get("parameters", [])]
    components = [_component_from_dict(c) for c in d.get("components", [])]
    assemblies = [_br_from_dict(a) for a in d.get("assemblies", [])]
    dashboard_groups = [_dg_from_dict(g) for g in d.get("dashboardGroups", [])]
    return MaintenanceUnitSpec(
        name=d["name"],
        description=d.get("description", ""),
        access_group=d.get("accessGroup", "Everyone"),
        maintenance_group=d.get("maintenanceGroup", "Everyone"),
        ws_assembly_service=d.get("wsAssemblyService", ""),
        parameters=params,
        components=components,
        assemblies=assemblies,
        dashboard_groups=dashboard_groups,
    )


def _param_from_dict(d: dict) -> ParameterSpec:
    return ParameterSpec(
        parameter_type=d["parameterType"],
        name=d["name"],
        description=d.get("description", ""),
        sort_order=d.get("sortOrder", 100),
        default_value=d.get("defaultValue", ""),
        parameter_command_type=d.get("parameterCommandType", "Unknown"),
        method_type=d.get("methodType", "Unknown"),
        method_query=d.get("methodQuery", ""),
        results_table_name=d.get("resultsTableName", ""),
        display_member=d.get("displayMember", ""),
        value_member=d.get("valueMember", ""),
        display_items=d.get("displayItems", ""),
        value_items=d.get("valueItems", ""),
        cube_name=d.get("cubeName", ""),
        dim_type=d.get("dimType", ""),
        dim_name=d.get("dimName", ""),
        member_filter=d.get("memberFilter", ""),
        sql_query=d.get("sqlQuery", ""),
    )


def _component_from_dict(d: dict) -> ComponentSpec:
    ct = d["componentType"]
    btn = None
    cv = None
    embedded = None
    if ct == "Button":
        btn = ButtonSpec(
            button_type=d.get("buttonType", "Standard"),
            image_url=d.get("imageUrl", ""),
            dim_type_name=d.get("dimTypeName", ""),
        )
    elif ct == "CubeView":
        cv = CubeViewSpec(cube_view_name=d.get("cubeViewName", ""))
    elif ct == "EmbeddedDashboard":
        embedded = EmbeddedDashboardSpec(
            embedded_dashboard_name=d.get("embeddedDashboardName", ""),
            template_name_suffix=d.get("templateNameSuffix", ""),
        )
    return ComponentSpec(
        component_type=ct,
        name=d["name"],
        description=d.get("description", ""),
        xf_text=d.get("xfText", ""),
        tool_tip=d.get("toolTip", ""),
        bound_parameter_name=d.get("boundParameterName", ""),
        display_format=d.get("displayFormat", ""),
        selection_changed_task_type=d.get("selectionChangedTaskType", "NoTask"),
        selection_changed_task_args=d.get("selectionChangedTaskArgs", ""),
        selection_changed_ui_action_type=d.get("selectionChangedUIActionType", "NoAction"),
        dashboards_to_redraw=d.get("dashboardsToRedraw", ""),
        button=btn,
        cube_view=cv,
        embedded_dashboard=embedded,
    )


def _br_from_dict(d: dict) -> BusinessRuleSpec:
    folders = []
    for f in d.get("folders", []):
        files = [
            BRFileSpec(
                name=ff["name"],
                source_code=ff.get("sourceCode", ""),
                compiler_action_type=ff.get("compilerActionType", "Compile"),
            )
            for ff in f.get("files", [])
        ]
        folders.append(BRFolderSpec(name=f["name"], files=files))
    return BusinessRuleSpec(
        assembly_name=d["assemblyName"],
        description=d.get("description", ""),
        compiler_language=d.get("compilerLanguage", "CSharp"),
        folders=folders,
        dependencies=d.get("dependencies", []),
    )


def _dg_from_dict(d: dict) -> DashboardGroupSpec:
    dashboards = []
    for db in d.get("dashboards", []):
        cols = [ColumnDef(width=c.get("width", "*")) for c in db.get("columns", [{"width": "*"}])]
        rows = [RowDef(height=r.get("height", "*")) for r in db.get("rows", [{"height": "*"}])]
        members = [ComponentMember(name=m) for m in db.get("componentMembers", [])]
        dashboards.append(DashboardSpec(
            name=db["name"],
            dashboard_type=db.get("dashboardType", "Embedded"),
            layout_type=db.get("layoutType", "Grid"),
            description=db.get("description", ""),
            load_dashboard_task_type=db.get("loadDashboardTaskType", "NoTask"),
            load_dashboard_task_args=db.get("loadDashboardTaskArgs", ""),
            show_title=db.get("showTitle", True),
            columns=cols,
            rows=rows,
            component_members=members,
        ))
    return DashboardGroupSpec(
        name=d["name"],
        description=d.get("description", ""),
        access_group=d.get("accessGroup", "Administrators"),
        dashboards=dashboards,
    )


# ---------------------------------------------------------------------------
# Argument parser
# ---------------------------------------------------------------------------

def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        prog="generate.py",
        description="Generate OneStream workspace XML objects from prompt inputs.",
        formatter_class=argparse.RawDescriptionHelpFormatter,
    )
    sub = parser.add_subparsers(dest="command", required=True)

    # ---- parameter ----
    p_param = sub.add_parser("parameter", help="Generate a <parameter> XML snippet")
    p_param.add_argument("--type", required=True,
                         choices=["BoundList", "InputValue", "MemberList", "LiteralValue", "DelimitedList"],
                         help="Parameter type")
    p_param.add_argument("--name", required=True, help="Full parameter name (e.g. BL_MY_List)")
    p_param.add_argument("--description", help="Description")
    p_param.add_argument("--sort-order", type=int, default=100)
    p_param.add_argument("--default-value", help="Default value")
    p_param.add_argument("--command-type", default="Unknown",
                         choices=["Unknown", "Method", "SQL"])
    p_param.add_argument("--method-type", default="Unknown",
                         choices=["Unknown", "BusinessRule", "SQL"])
    p_param.add_argument("--method-query", help="Method query string")
    p_param.add_argument("--results-table", help="Results table name")
    p_param.add_argument("--display-member", help="Display member column")
    p_param.add_argument("--value-member", help="Value member column")
    p_param.add_argument("--display-items", help="Comma-separated display items (DelimitedList)")
    p_param.add_argument("--value-items", help="Comma-separated value items (DelimitedList)")
    p_param.add_argument("--cube-name", help="Cube name (MemberList)")
    p_param.add_argument("--dim-type", help="Dimension type (MemberList)")
    p_param.add_argument("--dim-name", help="Dimension name (MemberList)")
    p_param.add_argument("--member-filter", help="Member filter (MemberList)")
    p_param.add_argument("--sql-query", help="SQL query (SQL command type)")
    p_param.set_defaults(func=cmd_parameter)

    # ---- dashboard ----
    p_db = sub.add_parser("dashboard", help="Generate a <dashboard> XML snippet")
    p_db.add_argument("--name", required=True)
    p_db.add_argument("--dashboard-type", default="Embedded",
                      choices=["TopLevel", "Embedded", "EmbeddedDynamic",
                               "EmbeddedTopLevelWithoutParameterPrompts"])
    p_db.add_argument("--layout-type", default="Grid",
                      choices=["Grid", "Uniform", "VerticalStackPanel"])
    p_db.add_argument("--description", default="")
    p_db.add_argument("--page-caption", default="")
    p_db.add_argument("--task-type", default="NoTask")
    p_db.add_argument("--task-args", default="")
    p_db.add_argument("--columns", default="*",
                      help="Comma-separated column widths (e.g. 'Auto,*')")
    p_db.add_argument("--rows", default="*",
                      help="Comma-separated row heights (e.g. 'Auto,800')")
    p_db.add_argument("--members", help="Comma-separated component member names")
    p_db.add_argument("--hide-title", action="store_true")
    p_db.set_defaults(func=cmd_dashboard)

    # ---- component ----
    p_comp = sub.add_parser("component", help="Generate a <component> XML snippet")
    p_comp.add_argument("--type", required=True,
                        choices=["Button", "ComboBox", "ListBox", "CubeView",
                                 "Label", "EmbeddedDashboard"])
    p_comp.add_argument("--name", required=True)
    p_comp.add_argument("--description", default="")
    p_comp.add_argument("--xf-text", default="")
    p_comp.add_argument("--tool-tip", default="")
    p_comp.add_argument("--bound-param", default="")
    p_comp.add_argument("--display-format", default="")
    p_comp.add_argument("--task-type", default="NoTask")
    p_comp.add_argument("--task-args", default="")
    p_comp.add_argument("--ui-action", default="NoAction",
                        choices=["NoAction", "Refresh", "RedrawComponents"])
    p_comp.add_argument("--redraw", default="", help="Dashboard(s) to redraw")
    # Button-specific
    p_comp.add_argument("--button-type", default="Standard",
                        choices=["Standard", "SelectMemberDialog"])
    p_comp.add_argument("--image", default="", help="Button image filename")
    p_comp.add_argument("--dim-type-name", default="",
                        help="Dimension type for SelectMemberDialog button")
    # CubeView-specific
    p_comp.add_argument("--cube-view-name", default="")
    # EmbeddedDashboard-specific
    p_comp.add_argument("--embedded-name", default="",
                        help="Name of the dashboard to embed")
    p_comp.set_defaults(func=cmd_component)

    # ---- businessrule ----
    p_br = sub.add_parser("businessrule", help="Generate a <workspaceAssembly> XML snippet")
    p_br.add_argument("--assembly-name", required=True)
    p_br.add_argument("--description", default="")
    p_br.add_argument("--folder", default="Business Rules")
    p_br.add_argument("--file-name", required=True, help="e.g. MyClass.cs")
    p_br.add_argument("--compiler-action", default="Compile",
                      choices=["Unknown", "Compile", "CompileAndRun"])
    p_br.add_argument("--source-file", help="Path to .cs file with source code")
    p_br.add_argument("--source-inline", help="Inline C# source code string")
    p_br.set_defaults(func=cmd_businessrule)

    # ---- compose ----
    p_compose = sub.add_parser("compose",
        help="Compose a full maintenanceUnit (or workspace) from a JSON spec file")
    p_compose.add_argument("--spec", required=True, help="Path to JSON spec file")
    p_compose.add_argument("--full-workspace", action="store_true",
                           help="Wrap in full workspace XML (loadable into OneStream)")
    p_compose.add_argument("--output", help="Output file path (default: stdout)")
    p_compose.set_defaults(func=cmd_compose)

    # ---- merge ----
    p_merge = sub.add_parser("merge",
        help="Merge a maintenanceUnit XML into an existing workspace XML file")
    p_merge.add_argument("--source", required=True,
                         help="Existing workspace XML file")
    p_merge.add_argument("--unit", required=True,
                         help="maintenanceUnit XML fragment file to inject")
    p_merge.add_argument("--output", help="Output file path (default: stdout)")
    p_merge.set_defaults(func=cmd_merge)

    return parser


# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

def main():
    parser = build_parser()
    args = parser.parse_args()

    xml_output = args.func(args)

    output_path = getattr(args, "output", None)
    if output_path:
        with open(output_path, "w", encoding="utf-8") as fh:
            fh.write(xml_output)
        print(f"Written to {output_path}", file=sys.stderr)
    else:
        print(xml_output)


if __name__ == "__main__":
    main()
