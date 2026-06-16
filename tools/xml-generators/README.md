# OneStream Workspace XML Generator — Usage Guide

This toolset converts structured prompt inputs into valid OneStream `<maintenanceUnit>` XML blocks
that can be imported directly into OneStream through Workspace Administration.

## Quick Start

```bash
cd tools/xml-generators

# Generate a BoundList parameter
python generate.py parameter \
    --type BoundList \
    --name BL_MY_Menu \
    --command-type Method \
    --method-type BusinessRule \
    --method-query "{Workspace.Current.MY_Assembly.MY_DataSets}{Get_Menu}{}" \
    --results-table MY_MenuConfig \
    --display-member Name \
    --value-member MenuID

# Generate a full workspace from a JSON spec (loadable into OneStream)
python generate.py compose \
    --spec examples/example_module.json \
    --full-workspace \
    --output my_workspace.xml

# Merge a new module into an existing workspace XML
python generate.py merge \
    --source "OS Consultant Tools/Dynamic Dashboard Manager/XML/DynamicDashboardManager.xml" \
    --unit my_module.xml \
    --output merged.xml
```

## Commands

| Command | Description |
|---|---|
| `parameter` | Generate a `<parameter>` XML snippet |
| `dashboard` | Generate a `<dashboard>` XML snippet |
| `component` | Generate a `<component>` XML snippet |
| `businessrule` | Generate a `<workspaceAssembly>` XML snippet |
| `compose` | Build a full `<maintenanceUnit>` (or workspace) from a JSON spec |
| `merge` | Inject a module XML into an existing workspace XML file |

Run `python generate.py <command> --help` for full argument documentation.

## JSON Spec Format

The `compose` command takes a JSON file describing the entire module.
See `examples/example_module.json` for a complete working example.

Top-level keys:

```json
{
  "name": "My Module",
  "description": "",
  "accessGroup": "Everyone",
  "maintenanceGroup": "Everyone",
  "wsAssemblyService": "MY_Assembly.MY_SvcFactory",
  "parameters": [...],
  "components": [...],
  "assemblies": [...],
  "dashboardGroups": [...]
}
```

## Running Tests

```bash
cd /path/to/OSConsTools
python -m pytest tools/xml-generators/tests.py -v
```

All 27 unit tests should pass with no dependencies beyond the Python standard library (+ pytest for testing).
