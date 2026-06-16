"""
OneStream Workspace XML Composer
=================================
Combines individual XML snippets (parameters, components, business rules,
dashboard groups) into a complete <maintenanceUnit> block, and optionally
wraps that into a full loadable workspace XML file.

Usage example
-------------
from generators import (
    ParameterSpec, DashboardSpec, BusinessRuleSpec, ComponentSpec,
    BRFileSpec, BRFolderSpec, BusinessRuleSpec,
    generate_parameter, generate_dashboard, generate_business_rule,
    generate_component,
)
from composer import MaintenanceUnitSpec, compose_maintenance_unit, compose_workspace

mu = compose_maintenance_unit(MaintenanceUnitSpec(
    name="My Module",
    parameters=[...],
    components=[...],
    assemblies=[...],
    dashboard_groups=[...],
))
xml = compose_workspace(mu)
print(xml)
"""

from __future__ import annotations

from dataclasses import dataclass, field
from typing import List

from generators import (
    INDENT,
    ParameterSpec,
    ComponentSpec,
    BusinessRuleSpec,
    DashboardGroupSpec,
    generate_parameter,
    generate_component,
    generate_business_rule,
    generate_dashboard_group,
    _ind,
    _open,
    _close,
    _e,
)

WORKSPACE_VERSION = "9.3.0.18429"


# ---------------------------------------------------------------------------
# Maintenance Unit Spec & Composer
# ---------------------------------------------------------------------------

@dataclass
class MaintenanceUnitSpec:
    """Everything that lives inside a single <maintenanceUnit>."""
    name: str
    description: str = ""
    access_group: str = "Everyone"
    maintenance_group: str = "Everyone"
    ws_assembly_service: str = ""
    category: str = ""
    parameters: List[ParameterSpec] = field(default_factory=list)
    components: List[ComponentSpec] = field(default_factory=list)
    assemblies: List[BusinessRuleSpec] = field(default_factory=list)
    dashboard_groups: List[DashboardGroupSpec] = field(default_factory=list)


def compose_maintenance_unit(spec: MaintenanceUnitSpec, depth: int = 3) -> str:
    """Compose a full <maintenanceUnit> XML fragment."""
    lines: List[str] = []
    lines.append(_open("maintenanceUnit", {
        "name": spec.name,
        "description": spec.description,
        "accessGroup": spec.access_group,
        "maintenanceGroup": spec.maintenance_group,
        "wsAssemblyService": spec.ws_assembly_service,
        "category": spec.category,
    }, depth))

    lines.append(f"{_ind(depth+1)}<fileResources />")
    lines.append(f"{_ind(depth+1)}<stringResources />")

    # Parameters
    if spec.parameters:
        lines.append(_open("parameters", depth=depth + 1))
        for p in spec.parameters:
            lines.append(generate_parameter(p, depth=depth + 2))
        lines.append(_close("parameters", depth + 1))
    else:
        lines.append(f"{_ind(depth+1)}<parameters />")

    lines.append(f"{_ind(depth+1)}<adapters />")

    # Components
    if spec.components:
        lines.append(_open("components", depth=depth + 1))
        for c in spec.components:
            lines.append(generate_component(c, depth=depth + 2))
        lines.append(_close("components", depth + 1))
    else:
        lines.append(f"{_ind(depth+1)}<components />")

    # Workspace Assemblies
    if spec.assemblies:
        lines.append(_open("workspaceAssemblies", depth=depth + 1))
        for a in spec.assemblies:
            lines.append(generate_business_rule(a, depth=depth + 2))
        lines.append(_close("workspaceAssemblies", depth + 1))
    else:
        lines.append(f"{_ind(depth+1)}<workspaceAssemblies />")

    # Dashboard Groups
    if spec.dashboard_groups:
        lines.append(_open("dashboardGroups", depth=depth + 1))
        for dg in spec.dashboard_groups:
            lines.append(generate_dashboard_group(dg, depth=depth + 2))
        lines.append(_close("dashboardGroups", depth + 1))
    else:
        lines.append(f"{_ind(depth+1)}<dashboardGroups />")

    lines.append(_close("maintenanceUnit", depth))
    return "\n".join(lines)


# ---------------------------------------------------------------------------
# Workspace Spec & Full-File Composer
# ---------------------------------------------------------------------------

@dataclass
class WorkspaceSpec:
    """Top-level workspace container."""
    name: str = "OS Consultant Tools"
    description: str = ""
    notes: str = ""
    access_group: str = "Everyone"
    maintenance_group: str = "Everyone"
    is_shareable_workspace: bool = True
    shared_workspace_names: str = ""
    namespace_prefix: str = "OSConsTools"
    version: str = WORKSPACE_VERSION
    maintenance_units: List[MaintenanceUnitSpec] = field(default_factory=list)


def compose_workspace(spec: WorkspaceSpec) -> str:
    """
    Compose a fully loadable OneStream workspace XML string.

    The output can be saved as a .xml file and imported directly into
    OneStream via the Workspace Administration screen.
    """
    lines: List[str] = []
    lines.append('<?xml version="1.0" encoding="utf-8"?>')
    lines.append(f'<OneStreamXF version="{spec.version}">')
    lines.append(f"{_ind(1)}<applicationWorkspacesRoot>")
    lines.append(f"{_ind(2)}<workspaces>")

    ws_attrs = {
        "name": spec.name,
        "description": spec.description,
        "notes": spec.notes,
        "accessGroup": spec.access_group,
        "maintenanceGroup": spec.maintenance_group,
        "isShareableWorkspace": str(spec.is_shareable_workspace).lower(),
        "sharedWorkspaceNames": spec.shared_workspace_names,
        "companyName": "",
        "versionNum": "",
        "author": "",
        "namespacePrefix": spec.namespace_prefix,
        "importsNamespace1": "",
        "importsNamespace2": "",
        "importsNamespace3": "",
        "importsNamespace4": "",
        "importsNamespace5": "",
        "importsNamespace6": "",
        "importsNamespace7": "",
        "importsNamespace8": "",
        "wsAssemblyService": "",
        "text1": "",
        "text2": "",
        "text3": "",
        "text4": "",
        "text5": "",
        "text6": "",
        "text7": "",
        "text8": "",
    }
    lines.append(_open("workspace", ws_attrs, depth=3))
    lines.append(f"{_ind(4)}<WorkspaceDefinition>")
    lines.append(f"{_ind(5)}<SubstitutionVariableItems />")
    lines.append(f"{_ind(4)}</WorkspaceDefinition>")

    if spec.maintenance_units:
        lines.append(_open("maintenanceUnits", depth=4))
        for mu in spec.maintenance_units:
            lines.append(compose_maintenance_unit(mu, depth=5))
        lines.append(_close("maintenanceUnits", depth=4))
    else:
        lines.append(f"{_ind(4)}<maintenanceUnits />")

    lines.append(_close("workspace", depth=3))
    lines.append(f"{_ind(2)}</workspaces>")
    lines.append(f"{_ind(1)}</applicationWorkspacesRoot>")
    lines.append("</OneStreamXF>")
    return "\n".join(lines)


# ---------------------------------------------------------------------------
# Merge helper: inject a maintenance unit into an existing workspace XML file
# ---------------------------------------------------------------------------

def merge_maintenance_unit_into_file(xml_path: str, mu_xml: str) -> str:
    """
    Read an existing workspace XML file, append a new <maintenanceUnit>
    block just before </maintenanceUnits>, and return the merged XML string.

    Does NOT write to disk — caller decides what to do with the result.
    """
    with open(xml_path, "r", encoding="utf-8-sig") as fh:
        content = fh.read()

    close_tag = "</maintenanceUnits>"
    if close_tag not in content:
        raise ValueError(
            f"Could not find '{close_tag}' in '{xml_path}'. "
            "Is this a valid OneStream workspace XML?"
        )

    insertion = mu_xml + "\n"
    merged = content.replace(close_tag, insertion + close_tag, 1)
    return merged
