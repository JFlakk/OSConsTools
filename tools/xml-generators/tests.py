"""
Unit tests for the OneStream XML generators.
Run with:  python -m pytest tools/xml-generators/tests.py -v
"""

import sys
import os
import json
import xml.etree.ElementTree as ET

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
# Helpers
# ---------------------------------------------------------------------------

def parse_xml(fragment: str) -> ET.Element:
    """Parse an XML fragment (possibly with a wrapper for multi-root cases)."""
    try:
        return ET.fromstring(fragment)
    except ET.ParseError:
        return ET.fromstring(f"<root>{fragment}</root>")


# ---------------------------------------------------------------------------
# Parameter tests
# ---------------------------------------------------------------------------

class TestParameterGenerator:
    def test_bound_list_prefix_enforced(self):
        spec = ParameterSpec(parameter_type="BoundList", name="BL_MY_List")
        xml = generate_parameter(spec)
        root = parse_xml(xml)
        assert root.tag == "parameter"
        assert root.attrib["parameterType"] == "BoundList"
        assert root.attrib["name"] == "BL_MY_List"

    def test_wrong_prefix_raises(self):
        import pytest
        spec = ParameterSpec(parameter_type="BoundList", name="IV_WRONG")
        try:
            generate_parameter(spec)
            assert False, "Expected ValueError"
        except ValueError as exc:
            assert "BL_" in str(exc)

    def test_input_value(self):
        spec = ParameterSpec(parameter_type="InputValue", name="IV_MY_Val", default_value="42")
        xml = generate_parameter(spec)
        root = parse_xml(xml)
        assert root.find("defaultValue").text == "42"
        assert root.find("parameterCommandType").text == "Unknown"

    def test_member_list(self):
        spec = ParameterSpec(
            parameter_type="MemberList",
            name="ML_MY_Dim",
            cube_name="~!Cube!~",
            dim_type="Entity",
            dim_name="~!DimName!~",
            member_filter="~!Filter!~",
        )
        xml = generate_parameter(spec)
        root = parse_xml(xml)
        assert root.find("dimType").text == "Entity"
        assert root.find("cubeName").text == "~!Cube!~"

    def test_delimited_list(self):
        spec = ParameterSpec(
            parameter_type="DelimitedList",
            name="DL_MY_Opts",
            display_items="A,B,C",
            value_items="1,2,3",
        )
        xml = generate_parameter(spec)
        root = parse_xml(xml)
        assert root.find("displayItems").text == "A,B,C"
        assert root.find("valueItems").text == "1,2,3"

    def test_literal_value(self):
        spec = ParameterSpec(
            parameter_type="LiteralValue",
            name="LV_MY_Fmt",
            default_value="Height = 30",
        )
        xml = generate_parameter(spec)
        root = parse_xml(xml)
        assert root.find("defaultValue").text == "Height = 30"

    def test_bound_list_with_method(self):
        spec = ParameterSpec(
            parameter_type="BoundList",
            name="BL_MY_List",
            parameter_command_type="Method",
            method_type="BusinessRule",
            method_query="{Workspace.Current.MY.DS}{Get_List}{}",
            results_table_name="MY_Table",
            display_member="Name",
            value_member="ID",
        )
        xml = generate_parameter(spec)
        root = parse_xml(xml)
        assert root.find("methodQuery").text == "{Workspace.Current.MY.DS}{Get_List}{}"
        assert root.find("resultsTableName").text == "MY_Table"
        assert root.find("displayMember").text == "Name"
        assert root.find("valueMember").text == "ID"

    def test_all_required_child_elements_present(self):
        spec = ParameterSpec(parameter_type="InputValue", name="IV_Test")
        xml = generate_parameter(spec)
        root = parse_xml(xml)
        for tag in ["defaultValue", "resultFormatStringType", "resultCustomFormatString",
                    "parameterCommandType", "sqlQuery", "dbLocation", "externalDBConnName",
                    "methodType", "methodQuery", "resultsTableName", "displayMember",
                    "valueMember", "displayItems", "valueItems", "cubeName",
                    "dimType", "dimName", "memberFilter"]:
            assert root.find(tag) is not None, f"Missing child element: {tag}"


# ---------------------------------------------------------------------------
# Dashboard tests
# ---------------------------------------------------------------------------

class TestDashboardGenerator:
    def test_top_level_dashboard(self):
        spec = DashboardSpec(
            name="MY_TopLevel",
            dashboard_type="TopLevel",
            load_dashboard_task_type="ExecuteDashboardExtenderBRAllActions",
            load_dashboard_task_args="{Workspace.Current.A.B}{Load}{}",
            component_members=[ComponentMember("Embedded MY_Hdr"), ComponentMember("Embedded MY_Content")],
        )
        xml = generate_dashboard(spec)
        root = parse_xml(xml)
        assert root.attrib["dashboardType"] == "TopLevel"
        assert root.attrib["loadDashboardTaskType"] == "ExecuteDashboardExtenderBRAllActions"
        cm = root.find("componentMembers")
        assert len(cm.findall("componentMember")) == 2

    def test_embedded_dashboard_grid(self):
        spec = DashboardSpec(
            name="MY_Content",
            dashboard_type="Embedded",
            columns=[ColumnDef(width="Auto"), ColumnDef(width="*")],
            rows=[RowDef(height="*")],
        )
        xml = generate_dashboard(spec)
        root = parse_xml(xml)
        dd = root.find("DashboardDefinition")
        gld = dd.find("GridLayoutDefinition")
        cols = gld.find("ColumnDefinitions").findall("ColumnDefinition")
        assert len(cols) == 2
        assert cols[0].attrib["Width"] == "Auto"
        assert cols[1].attrib["Width"] == "*"

    def test_show_title_default_true(self):
        spec = DashboardSpec(name="MY_DB")
        xml = generate_dashboard(spec)
        root = parse_xml(xml)
        assert root.find("DashboardDefinition/ShowTitle").text == "true"

    def test_empty_component_members(self):
        spec = DashboardSpec(name="MY_DB")
        xml = generate_dashboard(spec)
        assert "<componentMembers />" in xml


# ---------------------------------------------------------------------------
# Component tests
# ---------------------------------------------------------------------------

class TestComponentGenerator:
    def test_standard_button(self):
        spec = ComponentSpec(
            component_type="Button",
            name="btn_MY_Action",
            xf_text="Run",
            button=ButtonSpec(button_type="Standard", image_url="Std_DB_Calc.png"),
        )
        xml = generate_component(spec)
        root = parse_xml(xml)
        assert root.attrib["componentType"] == "Button"
        assert root.find("componentDefinition/XFButtonDefinition").attrib["ButtonType"] == "Standard"
        img = root.find("componentDefinition/XFButtonDefinition/ImageUrlOrFullFileName")
        assert img.text == "Std_DB_Calc.png"

    def test_select_member_dialog_button(self):
        spec = ComponentSpec(
            component_type="Button",
            name="btn_MY_MbrPicker",
            button=ButtonSpec(button_type="SelectMemberDialog", dim_type_name="Entity"),
        )
        xml = generate_component(spec)
        root = parse_xml(xml)
        smi = root.find("componentDefinition/XFButtonDefinition/SelectMemberInfo")
        assert smi is not None
        assert smi.find("DimTypeName").text == "Entity"

    def test_cube_view_component(self):
        spec = ComponentSpec(
            component_type="CubeView",
            name="cv_MY_View",
            cube_view=CubeViewSpec(cube_view_name="MY_CV"),
        )
        xml = generate_component(spec)
        root = parse_xml(xml)
        cv = root.find("componentDefinition/XFCubeViewDefinition")
        assert cv is not None
        assert cv.find("CubeViewName").text == "MY_CV"

    def test_embedded_dashboard_component(self):
        spec = ComponentSpec(
            component_type="EmbeddedDashboard",
            name="Embedded MY_Content",
            embedded_dashboard=EmbeddedDashboardSpec(embedded_dashboard_name="MY_Content"),
        )
        xml = generate_component(spec)
        root = parse_xml(xml)
        assert root.attrib["embeddedDashboardName"] == "MY_Content"

    def test_list_box_component(self):
        spec = ComponentSpec(
            component_type="ListBox",
            name="lbx_MY_Menu",
            xf_text="Menu",
            bound_parameter_name="BL_MY_Menu",
        )
        xml = generate_component(spec)
        root = parse_xml(xml)
        assert root.attrib["componentType"] == "ListBox"
        assert root.attrib["boundParameterName"] == "BL_MY_Menu"

    def test_combobox_component(self):
        spec = ComponentSpec(
            component_type="ComboBox",
            name="cbx_MY_List",
            bound_parameter_name="BL_MY_List",
        )
        xml = generate_component(spec)
        root = parse_xml(xml)
        assert root.attrib["componentType"] == "ComboBox"


# ---------------------------------------------------------------------------
# Business Rule tests
# ---------------------------------------------------------------------------

class TestBusinessRuleGenerator:
    def test_assembly_structure(self):
        file_spec = BRFileSpec(
            name="MY_Class.cs",
            source_code="// C# code here",
            compiler_action_type="Compile",
        )
        folder = BRFolderSpec(name="My Folder", files=[file_spec])
        spec = BusinessRuleSpec(assembly_name="MY_Assembly", folders=[folder])
        xml = generate_business_rule(spec)
        root = parse_xml(xml)
        assert root.tag == "workspaceAssembly"
        assert root.attrib["name"] == "MY_Assembly"
        folder_el = root.find("files/folder")
        assert folder_el.attrib["name"] == "My Folder"
        file_el = folder_el.find("file")
        assert file_el.attrib["name"] == "MY_Class.cs"
        assert file_el.attrib["compilerActionType"] == "Compile"
        assert "// C# code here" in file_el.find("sourceCode").text

    def test_cdata_wrapping(self):
        file_spec = BRFileSpec(name="F.cs", source_code="int x = 1 < 2 ? 3 : 4;")
        folder = BRFolderSpec(name="F", files=[file_spec])
        spec = BusinessRuleSpec(assembly_name="A", folders=[folder])
        xml = generate_business_rule(spec)
        assert "<![CDATA[" in xml
        assert "int x = 1 < 2 ? 3 : 4;" in xml

    def test_empty_dependencies(self):
        spec = BusinessRuleSpec(assembly_name="A", folders=[])
        xml = generate_business_rule(spec)
        assert "<dependencies />" in xml


# ---------------------------------------------------------------------------
# Composer tests
# ---------------------------------------------------------------------------

class TestComposer:
    def _make_mu_spec(self) -> MaintenanceUnitSpec:
        param = ParameterSpec(parameter_type="InputValue", name="IV_TEST_Val")
        comp = ComponentSpec(
            component_type="ListBox",
            name="lbx_TEST_Menu",
            bound_parameter_name="BL_TEST_Menu",
        )
        return MaintenanceUnitSpec(
            name="TEST Module",
            parameters=[param],
            components=[comp],
        )

    def test_maintenance_unit_has_required_sections(self):
        spec = self._make_mu_spec()
        xml = compose_maintenance_unit(spec)
        root = parse_xml(xml)
        assert root.tag == "maintenanceUnit"
        assert root.attrib["name"] == "TEST Module"
        assert root.find("parameters") is not None
        assert root.find("components") is not None
        assert root.find("workspaceAssemblies") is not None
        assert root.find("dashboardGroups") is not None

    def test_full_workspace_xml(self):
        mu = self._make_mu_spec()
        ws = WorkspaceSpec(maintenance_units=[mu])
        xml = compose_workspace(ws)
        assert xml.startswith('<?xml version="1.0" encoding="utf-8"?>')
        assert "<OneStreamXF" in xml
        assert "<applicationWorkspacesRoot>" in xml
        root = ET.fromstring(xml)
        assert root.tag == "OneStreamXF"
        ws_el = root.find("applicationWorkspacesRoot/workspaces/workspace")
        assert ws_el is not None
        assert ws_el.attrib["namespacePrefix"] == "OSConsTools"

    def test_merge_maintenance_unit(self, tmp_path):
        # Create a minimal workspace XML in a temp file
        base_xml = (
            '<?xml version="1.0" encoding="utf-8"?>\n'
            '<OneStreamXF version="9.3.0.18429">\n'
            '    <applicationWorkspacesRoot>\n'
            '        <workspaces>\n'
            '            <workspace name="OS Consultant Tools" namespacePrefix="OSConsTools">\n'
            '                <maintenanceUnits>\n'
            '                </maintenanceUnits>\n'
            '            </workspace>\n'
            '        </workspaces>\n'
            '    </applicationWorkspacesRoot>\n'
            '</OneStreamXF>\n'
        )
        source_file = tmp_path / "workspace.xml"
        source_file.write_text(base_xml, encoding="utf-8")

        mu_xml = compose_maintenance_unit(self._make_mu_spec())
        merged = merge_maintenance_unit_into_file(str(source_file), mu_xml)
        merged_root = ET.fromstring(merged)
        mu_el = merged_root.find(".//maintenanceUnit")
        assert mu_el is not None
        assert mu_el.attrib["name"] == "TEST Module"

    def test_compose_from_json_example(self):
        example_path = os.path.join(
            os.path.dirname(__file__), "examples", "example_module.json"
        )
        with open(example_path, "r") as fh:
            data = json.load(fh)

        # Import _mu_from_dict from generate module
        sys.path.insert(0, os.path.dirname(__file__))
        from generate import _mu_from_dict
        mu_spec = _mu_from_dict(data)
        xml = compose_maintenance_unit(mu_spec)
        root = parse_xml(xml)
        assert root.attrib["name"] == "My New Module"
        # Verify all 5 parameter types are present
        param_types = {p.attrib["parameterType"] for p in root.findall("parameters/parameter")}
        assert param_types == {"BoundList", "InputValue", "DelimitedList", "MemberList", "LiteralValue"}


# ---------------------------------------------------------------------------
# Dashboard Group tests
# ---------------------------------------------------------------------------

class TestDashboardGroupGenerator:
    def test_group_with_dashboards(self):
        db = DashboardSpec(name="MY_DB", dashboard_type="Embedded")
        grp = DashboardGroupSpec(name="MY Group", dashboards=[db])
        xml = generate_dashboard_group(grp)
        root = parse_xml(xml)
        assert root.tag == "dashboardGroup"
        assert root.find("dashboards/dashboard").attrib["name"] == "MY_DB"

    def test_empty_group(self):
        grp = DashboardGroupSpec(name="Empty Group")
        xml = generate_dashboard_group(grp)
        assert "<dashboards />" in xml


if __name__ == "__main__":
    import pytest
    pytest.main([__file__, "-v"])
