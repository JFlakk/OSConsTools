using OSConsTools.McpServer;

try
{
    var options = ServerOptions.FromEnvironmentAndArgs(args);
    var index = RepositoryIndexer.Build(options);

    Console.Error.WriteLine(
        $"OSConsTools MCP indexed {index.Assemblies.Count} assemblies, {index.ApiTypes.Count} public types, {index.ApiMembers.Count} public members, {index.CodeFiles.Count} C# files, and {index.WorkspaceBindings.Count} workspace bindings.");

    var server = new StdioMcpServer(options, index);
    await server.RunAsync(CancellationToken.None);
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex);
    return 1;
}
