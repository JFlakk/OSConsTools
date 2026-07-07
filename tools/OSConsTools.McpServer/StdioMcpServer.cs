using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OSConsTools.McpServer;

internal sealed class StdioMcpServer
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly ServerOptions _options;
    private readonly RepositoryIndex _index;
    private readonly IReadOnlyList<McpToolDefinition> _tools;

    public StdioMcpServer(ServerOptions options, RepositoryIndex index)
    {
        _options = options;
        _index = index;
        _tools = CreateToolDefinitions();
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var input = Console.OpenStandardInput();
        var output = Console.OpenStandardOutput();

        while (!cancellationToken.IsCancellationRequested)
        {
            var payload = await ReadMessageAsync(input, cancellationToken);
            if (payload is null)
            {
                break;
            }

            JsonObject? response;
            try
            {
                response = HandleMessage(payload);
            }
            catch (Exception ex)
            {
                response = CreateErrorResponse(null, -32603, ex.Message);
            }

            if (response is not null)
            {
                await WriteMessageAsync(output, response, cancellationToken);
            }
        }
    }

    private JsonObject? HandleMessage(string payload)
    {
        var request = JsonNode.Parse(payload)?.AsObject()
            ?? throw new InvalidOperationException("Invalid JSON-RPC payload.");

        var method = request["method"]?.GetValue<string>();
        var id = request["id"]?.DeepClone();
        var parameters = request["params"] as JsonObject;

        return method switch
        {
            "initialize" => CreateSuccessResponse(
                id,
                new
                {
                    protocolVersion = "2024-11-05",
                    capabilities = new
                    {
                        tools = new
                        {
                            listChanged = false
                        }
                    },
                    serverInfo = new
                    {
                        name = "OSConsTools.McpServer",
                        version = "0.1.0"
                    },
                    instructions = "Use the OneStream symbol, example, and workspace binding tools to inspect the OSConsTools repository and the DLLs in lib/OneStream."
                }),
            "notifications/initialized" => null,
            "ping" => CreateSuccessResponse(id, new { }),
            "shutdown" => CreateSuccessResponse(id, new { }),
            "tools/list" => CreateSuccessResponse(
                id,
                new
                {
                    tools = _tools.Select(tool => new
                    {
                        name = tool.Name,
                        description = tool.Description,
                        inputSchema = tool.InputSchema
                    })
                }),
            "tools/call" => HandleToolCall(id, parameters),
            _ => CreateErrorResponse(id, -32601, $"Unsupported method '{method}'.")
        };
    }

    private JsonObject HandleToolCall(JsonNode? id, JsonObject? parameters)
    {
        var toolName = parameters?["name"]?.GetValue<string>();
        var arguments = parameters?["arguments"] as JsonObject ?? new JsonObject();

        if (string.IsNullOrWhiteSpace(toolName))
        {
            return CreateErrorResponse(id, -32602, "Tool name was not provided.");
        }

        object result = toolName switch
        {
            "search_onestream_api_symbols" => _index.SearchOneStreamApiSymbols(
                GetRequiredString(arguments, "query"),
                GetOptionalInt(arguments, "limit", 10),
                GetOptionalString(arguments, "assembly")),
            "get_onestream_symbol_details" => _index.GetOneStreamSymbolDetails(
                GetRequiredString(arguments, "symbol")),
            "find_repo_examples" => _index.FindRepoExamples(
                GetRequiredString(arguments, "query"),
                GetOptionalInt(arguments, "limit", 5)),
            "trace_workspace_bindings" => _index.TraceWorkspaceBindings(
                GetOptionalString(arguments, "query"),
                GetOptionalInt(arguments, "limit", 10)),
            "get_index_stats" => _index.GetIndexStats(),
            _ => throw new InvalidOperationException($"Unknown tool '{toolName}'.")
        };

        return CreateSuccessResponse(
            id,
            new
            {
                content = new[]
                {
                    new
                    {
                        type = "text",
                        text = JsonSerializer.Serialize(result, JsonOptions)
                    }
                },
                structuredContent = result,
                isError = false
            });
    }

    private static IReadOnlyList<McpToolDefinition> CreateToolDefinitions()
    {
        return new[]
        {
            new McpToolDefinition(
                "search_onestream_api_symbols",
                "Search indexed OneStream public types, methods, and properties from lib/OneStream DLL metadata.",
                new
                {
                    type = "object",
                    properties = new
                    {
                        query = new { type = "string", description = "Type, member, or API term to search for." },
                        assembly = new { type = "string", description = "Optional assembly name filter such as OneStream.FinanceEngine." },
                        limit = new { type = "integer", minimum = 1, maximum = 25 }
                    },
                    required = new[] { "query" }
                }),
            new McpToolDefinition(
                "get_onestream_symbol_details",
                "Get detailed information about an indexed OneStream type or member, including repo examples.",
                new
                {
                    type = "object",
                    properties = new
                    {
                        symbol = new { type = "string", description = "Full or short symbol name." }
                    },
                    required = new[] { "symbol" }
                }),
            new McpToolDefinition(
                "find_repo_examples",
                "Find repository C# usage examples for a OneStream API, workspace service, or business-rule pattern.",
                new
                {
                    type = "object",
                    properties = new
                    {
                        query = new { type = "string", description = "Symbol or pattern to find in OS Consultant Tools assemblies." },
                        limit = new { type = "integer", minimum = 1, maximum = 25 }
                    },
                    required = new[] { "query" }
                }),
            new McpToolDefinition(
                "trace_workspace_bindings",
                "Trace workspace XML bindings, methodQuery values, and likely matching C# implementation files.",
                new
                {
                    type = "object",
                    properties = new
                    {
                        query = new { type = "string", description = "Optional filter for maintenance units, parameters, methodQuery values, or service factories." },
                        limit = new { type = "integer", minimum = 1, maximum = 25 }
                    }
                }),
            new McpToolDefinition(
                "get_index_stats",
                "Show the current DLL, C# source, and XML binding index counts and scan warnings.",
                new
                {
                    type = "object",
                    properties = new { }
                })
        };
    }

    private static async Task<string?> ReadMessageAsync(Stream input, CancellationToken cancellationToken)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var currentHeader = new List<byte>();

        while (true)
        {
            var currentByte = new byte[1];
            var bytesRead = await input.ReadAsync(currentByte, cancellationToken);
            if (bytesRead == 0)
            {
                return null;
            }

            currentHeader.Add(currentByte[0]);
            var count = currentHeader.Count;
            if (count >= 4 &&
                currentHeader[count - 4] == '\r' &&
                currentHeader[count - 3] == '\n' &&
                currentHeader[count - 2] == '\r' &&
                currentHeader[count - 1] == '\n')
            {
                break;
            }
        }

        var headerText = Encoding.ASCII.GetString(currentHeader.ToArray());
        foreach (var line in headerText.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
        {
            var separatorIndex = line.IndexOf(':');
            if (separatorIndex <= 0)
            {
                continue;
            }

            headers[line[..separatorIndex].Trim()] = line[(separatorIndex + 1)..].Trim();
        }

        if (!headers.TryGetValue("Content-Length", out var contentLengthValue) ||
            !int.TryParse(contentLengthValue, out var contentLength) ||
            contentLength < 0)
        {
            throw new InvalidOperationException("Missing or invalid Content-Length header.");
        }

        var bodyBytes = new byte[contentLength];
        var offset = 0;
        while (offset < contentLength)
        {
            var bytesRead = await input.ReadAsync(bodyBytes.AsMemory(offset, contentLength - offset), cancellationToken);
            if (bytesRead == 0)
            {
                throw new InvalidOperationException("Unexpected end of stream while reading MCP payload.");
            }

            offset += bytesRead;
        }

        return Encoding.UTF8.GetString(bodyBytes);
    }

    private static async Task WriteMessageAsync(Stream output, JsonObject payload, CancellationToken cancellationToken)
    {
        var json = payload.ToJsonString(JsonOptions);
        var bodyBytes = Encoding.UTF8.GetBytes(json);
        var headerBytes = Encoding.ASCII.GetBytes($"Content-Length: {bodyBytes.Length}\r\n\r\n");

        await output.WriteAsync(headerBytes, cancellationToken);
        await output.WriteAsync(bodyBytes, cancellationToken);
        await output.FlushAsync(cancellationToken);
    }

    private static JsonObject CreateSuccessResponse(JsonNode? id, object result)
    {
        return new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = id?.DeepClone(),
            ["result"] = JsonSerializer.SerializeToNode(result, JsonOptions)
        };
    }

    private static JsonObject CreateErrorResponse(JsonNode? id, int code, string message)
    {
        return new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = id?.DeepClone(),
            ["error"] = new JsonObject
            {
                ["code"] = code,
                ["message"] = message
            }
        };
    }

    private static string GetRequiredString(JsonObject arguments, string name)
    {
        var value = GetOptionalString(arguments, name);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Argument '{name}' is required.");
        }

        return value;
    }

    private static string? GetOptionalString(JsonObject arguments, string name)
    {
        return arguments[name]?.GetValue<string>();
    }

    private static int GetOptionalInt(JsonObject arguments, string name, int fallback)
    {
        return arguments[name]?.GetValue<int>() ?? fallback;
    }
}

internal sealed record McpToolDefinition(string Name, string Description, object InputSchema);
