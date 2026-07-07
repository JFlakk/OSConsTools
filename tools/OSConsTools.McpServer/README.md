# OSConsTools MCP Server

`OSConsTools.McpServer` is a small .NET MCP server that helps GitHub Copilot and other MCP clients explore:

- public OneStream API metadata from `lib/OneStream/*.dll`
- repo-specific C# usage examples from `OS Consultant Tools/**/Assemblies/**/*.cs`
- workspace XML bindings from `OS Consultant Tools/**/XML/*.xml`

It is intended as a practical starter server for OneStream-focused coding and maintenance work in this repository.

## Repository assumptions

By default the server scans this repository layout:

- `lib/OneStream`
- `OS Consultant Tools/**/Assemblies/**/*.cs`
- `OS Consultant Tools/**/XML/*.xml`

The repo root is auto-detected by walking up from the built server folder until `OSConsTools.csproj` is found.

You can also override paths:

- `OSCONSTOOLS_REPO_ROOT=/absolute/path/to/OSConsTools`
- `OSCONSTOOLS_ONESTREAM_DLL_DIR=/absolute/path/to/lib/OneStream`

Or pass them as arguments:

```bash
dotnet run --project tools/OSConsTools.McpServer/OSConsTools.McpServer.csproj -- --repo-root /absolute/path/to/OSConsTools
```

## OneStream DLL placement

Place the OneStream DLLs under:

`lib/OneStream`

See `docs/OneStream-DLL-setup.md` for the existing repository setup details.

The MCP server does not require the DLLs to be committed. If the directory is empty or missing, the server still starts and the API metadata tools will simply return no assembly matches.

For the best metadata coverage, also copy any transitive companion assemblies that your OneStream DLLs require. If some dependencies are missing, the server still starts, but `get_index_stats` will report scan warnings and some symbols may only be discoverable through `find_repo_examples` and `trace_workspace_bindings`.

## Build

Build only the MCP server project:

```bash
dotnet build tools/OSConsTools.McpServer/OSConsTools.McpServer.csproj
```

## Run

Run over stdio:

```bash
dotnet run --project tools/OSConsTools.McpServer/OSConsTools.McpServer.csproj --no-build
```

The server implements a small MCP stdio surface directly with JSON-RPC and `tools/list` + `tools/call`. This keeps the MVP lightweight and avoids coupling the repo to a specific external .NET MCP SDK package.

## Available MCP tools

### `search_onestream_api_symbols`

Searches indexed OneStream public types, methods, and properties.

Useful for:

- finding `BRApi`
- finding `DashboardDataSetArgs`
- locating symbols in specific OneStream assemblies

### `get_onestream_symbol_details`

Returns details for a type or member plus matching repo examples.

Useful for:

- inspecting a type’s public members
- checking the signature of a method
- jumping from a symbol to local usage examples

### `find_repo_examples`

Searches the repository’s C# source for matching OneStream usage patterns.

Useful for:

- finding `BRApi.Workflow.General.GetWorkflowUnitPk`
- locating `DashboardExtenderArgs` entrypoints
- locating `IWsAssemblyServiceFactory` implementations

### `trace_workspace_bindings`

Searches workspace XML bindings, including `methodQuery` values and maintenance-unit service bindings.

Useful for:

- tracing `methodQuery` to likely C# files
- locating `wsAssemblyService` factories
- inspecting maintenance-unit parameter bindings

### `get_index_stats`

Shows index counts and scan warnings.

## Example MCP client configuration

Example config for a stdio MCP client:

```json
{
  "mcpServers": {
    "osconstools": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "/absolute/path/to/OSConsTools/tools/OSConsTools.McpServer/OSConsTools.McpServer.csproj",
        "--no-build"
      ],
      "env": {
        "OSCONSTOOLS_REPO_ROOT": "/absolute/path/to/OSConsTools"
      }
    }
  }
}
```

If your MCP client prefers a built binary, publish first and point the client at the generated executable instead.

## Notes

- The server is intentionally repo-specific and optimized for OneStream exploration in this codebase.
- The initial implementation uses reflection-based metadata indexing for public types, methods, and properties.
- XML bindings are parsed from workspace definitions and cross-referenced back to likely C# implementation files.
