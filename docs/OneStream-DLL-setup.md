# OneStream DLL setup

This repository now includes a workspace-level C# project at `OSConsTools.csproj`.
It compiles all C# files under `OS Consultant Tools/**/Assemblies/**/*.cs` and resolves OneStream references from `lib/OneStream`.

The repo also includes a .NET MCP server at `tools/OSConsTools.McpServer/` that scans the same `lib/OneStream` folder for API metadata.

## Add OneStream DLLs

Copy these DLLs into `lib/OneStream`:

- `OneStream.Finance.Database.dll`
- `OneStream.Finance.Engine.dll`
- `OneStream.Shared.Common.dll`
- `OneStream.Shared.Database.dll`
- `OneStream.Shared.Engine.dll`
- `OneStream.Shared.Wcf.dll`
- `OneStream.Stage.Database.dll`
- `OneStream.Stage.Engine.dll`
- `OneStream.Data.DataFrame.dll`
- `OneStream.Data.DataFrame.Abstractions.dll`
- `OneStreamWorkspacesApi.dll`

The references are conditional, so missing DLLs will be skipped until added.

## Validate

If you have the .NET SDK installed, run:

```bash
dotnet build OSConsTools.csproj
```

This checks that all OneStream namespaces are resolved correctly.

To build the MCP server instead, run:

```bash
dotnet build tools/OSConsTools.McpServer/OSConsTools.McpServer.csproj
```
