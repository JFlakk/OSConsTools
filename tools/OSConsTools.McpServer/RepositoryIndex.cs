using System.Text;
using System.Text.RegularExpressions;

namespace OSConsTools.McpServer;

internal sealed record RepositoryIndex(
    IReadOnlyList<AssemblyRecord> Assemblies,
    IReadOnlyList<ApiTypeRecord> ApiTypes,
    IReadOnlyList<ApiMemberRecord> ApiMembers,
    IReadOnlyList<CodeFileRecord> CodeFiles,
    IReadOnlyList<WorkspaceBindingRecord> WorkspaceBindings,
    IReadOnlyList<string> ScanWarnings)
{
    public object SearchOneStreamApiSymbols(string query, int limit, string? assemblyFilter)
    {
        var normalizedLimit = NormalizeLimit(limit);
        var terms = SearchText.ExpandTerms(query);
        var typeMatches = ApiTypes
            .Select(type => new
            {
                score = SearchText.Score(terms, type.FullName, type.Name, type.Namespace, type.AssemblyName),
                result = (object)new
                {
                    kind = "type",
                    assembly = type.AssemblyName,
                    symbol = type.FullName,
                    namespaceName = type.Namespace,
                    typeKind = type.TypeKind
                },
                assembly = type.AssemblyName,
                symbol = type.FullName
            })
            .Where(result => result.score > 0 && MatchesAssemblyFilter(result.assembly, assemblyFilter));

        var memberMatches = ApiMembers
            .Select(member => new
            {
                score = SearchText.Score(
                    terms,
                    member.FullSymbol,
                    member.MemberName,
                    member.DeclaringTypeFullName,
                    member.Signature,
                    member.AssemblyName),
                result = (object)new
                {
                    kind = member.MemberKind,
                    assembly = member.AssemblyName,
                    symbol = member.FullSymbol,
                    declaringType = member.DeclaringTypeFullName,
                    signature = member.Signature
                },
                assembly = member.AssemblyName,
                symbol = member.FullSymbol
            })
            .Where(result => result.score > 0 && MatchesAssemblyFilter(result.assembly, assemblyFilter));

        var combined = typeMatches
            .Concat(memberMatches)
            .OrderByDescending(result => result.score)
            .ThenBy(result => result.symbol, StringComparer.OrdinalIgnoreCase)
            .Select(result => result.result)
            .Take(normalizedLimit)
            .ToList();

        return new
        {
            query,
            assemblyFilter,
            resultCount = combined.Count,
            results = combined,
            indexStats = GetIndexStats()
        };
    }

    public object GetOneStreamSymbolDetails(string symbol)
    {
        var trimmedSymbol = symbol.Trim();
        var typeMatch = ApiTypes.FirstOrDefault(type =>
            string.Equals(type.FullName, trimmedSymbol, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(type.Name, trimmedSymbol, StringComparison.OrdinalIgnoreCase));

        if (typeMatch is not null)
        {
            var members = ApiMembers
                .Where(member => string.Equals(member.DeclaringTypeFullName, typeMatch.FullName, StringComparison.OrdinalIgnoreCase))
                .OrderBy(member => member.MemberKind, StringComparer.OrdinalIgnoreCase)
                .ThenBy(member => member.MemberName, StringComparer.OrdinalIgnoreCase)
                .Select(member => new
                {
                    kind = member.MemberKind,
                    name = member.MemberName,
                    signature = member.Signature
                })
                .ToList();

            return new
            {
                symbol = trimmedSymbol,
                matchType = "type",
                typeInfo = new
                {
                    assembly = typeMatch.AssemblyName,
                    namespaceName = typeMatch.Namespace,
                    typeName = typeMatch.Name,
                    fullName = typeMatch.FullName,
                    typeKind = typeMatch.TypeKind,
                    baseType = typeMatch.BaseType,
                    implementedInterfaces = typeMatch.Interfaces
                },
                publicMembers = members,
                exampleFiles = FindRepoExamples(trimmedSymbol, 5)
            };
        }

        var memberMatch = ApiMembers.FirstOrDefault(member =>
            string.Equals(member.FullSymbol, trimmedSymbol, StringComparison.OrdinalIgnoreCase) ||
            string.Equals($"{member.DeclaringTypeName}.{member.MemberName}", trimmedSymbol, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(member.MemberName, trimmedSymbol, StringComparison.OrdinalIgnoreCase));

        if (memberMatch is not null)
        {
            return new
            {
                symbol = trimmedSymbol,
                matchType = "member",
                memberInfo = new
                {
                    assembly = memberMatch.AssemblyName,
                    declaringType = memberMatch.DeclaringTypeFullName,
                    kind = memberMatch.MemberKind,
                    name = memberMatch.MemberName,
                    signature = memberMatch.Signature
                },
                exampleFiles = FindRepoExamples(trimmedSymbol, 5)
            };
        }

        var suggestions = SearchOneStreamApiSymbols(trimmedSymbol, 5, null);
        return new
        {
            symbol = trimmedSymbol,
            matchType = "notFound",
            message = "No exact symbol match was found in the indexed OneStream assemblies.",
            suggestions
        };
    }

    public object FindRepoExamples(string query, int limit)
    {
        var normalizedLimit = NormalizeLimit(limit);
        var terms = SearchText.ExpandTerms(query);

        var matches = CodeFiles
            .Select(file =>
            {
                var score = SearchText.Score(terms, file.RelativePath, file.Content);
                if (score == 0)
                {
                    return null;
                }

                var snippets = SnippetBuilder.Build(file.Lines, terms, 3);
                if (snippets.Count == 0)
                {
                    return null;
                }

                return new
                {
                    file = file,
                    score,
                    snippets
                };
            })
            .Where(result => result is not null)
            .OrderByDescending(result => result!.score)
            .ThenBy(result => result!.file.RelativePath, StringComparer.OrdinalIgnoreCase)
            .Take(normalizedLimit)
            .Select(result => new
            {
                path = result!.file.RelativePath,
                score = result.score,
                snippets = result.snippets
            })
            .ToList();

        return new
        {
            query,
            resultCount = matches.Count,
            results = matches
        };
    }

    public object TraceWorkspaceBindings(string? query, int limit)
    {
        var normalizedLimit = NormalizeLimit(limit);
        var terms = SearchText.ExpandTerms(query);

        var matches = WorkspaceBindings
            .Select(binding => new
            {
                Binding = binding,
                Score = terms.Count == 0
                    ? 1
                    : SearchText.Score(
                        terms,
                        binding.FilePath,
                        binding.WorkspaceName,
                        binding.MaintenanceUnitName,
                        binding.ParameterName ?? string.Empty,
                        binding.MethodQuery ?? string.Empty,
                        binding.WorkspaceAssemblyService ?? string.Empty,
                        binding.TargetRule ?? string.Empty,
                        binding.TargetMethod ?? string.Empty)
            })
            .Where(result => result.Score > 0)
            .OrderByDescending(result => result.Score)
            .ThenBy(result => result.Binding.FilePath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(result => result.Binding.LineNumber)
            .Take(normalizedLimit)
            .Select(result => new
            {
                path = result.Binding.FilePath,
                lineNumber = result.Binding.LineNumber,
                workspace = result.Binding.WorkspaceName,
                maintenanceUnit = result.Binding.MaintenanceUnitName,
                parameter = result.Binding.ParameterName,
                parameterCommandType = result.Binding.ParameterCommandType,
                workspaceAssemblyService = result.Binding.WorkspaceAssemblyService,
                methodType = result.Binding.MethodType,
                methodQuery = result.Binding.MethodQuery,
                targetRule = result.Binding.TargetRule,
                targetMethod = result.Binding.TargetMethod,
                likelyCSharpFiles = FindLikelyCodeFiles(result.Binding)
            })
            .ToList();

        return new
        {
            query,
            resultCount = matches.Count,
            results = matches
        };
    }

    public object GetIndexStats()
    {
        return new
        {
            assemblies = Assemblies.Count,
            apiTypes = ApiTypes.Count,
            apiMembers = ApiMembers.Count,
            codeFiles = CodeFiles.Count,
            workspaceBindings = WorkspaceBindings.Count,
            scanWarnings = ScanWarnings
        };
    }

    private IReadOnlyList<string> FindLikelyCodeFiles(WorkspaceBindingRecord binding)
    {
        var searchQuery = string.Join(
            ' ',
            new[]
            {
                binding.TargetMethod,
                binding.TargetRule,
                binding.WorkspaceAssemblyService
            }.Where(value => !string.IsNullOrWhiteSpace(value)));

        if (string.IsNullOrWhiteSpace(searchQuery))
        {
            return Array.Empty<string>();
        }

        var terms = SearchText.ExpandTerms(searchQuery);
        return CodeFiles
            .Select(file => new
            {
                file.RelativePath,
                Score = SearchText.Score(terms, file.RelativePath, file.Content)
            })
            .Where(result => result.Score > 0)
            .OrderByDescending(result => result.Score)
            .ThenBy(result => result.RelativePath, StringComparer.OrdinalIgnoreCase)
            .Take(3)
            .Select(result => result.RelativePath)
            .ToList();
    }

    private static bool MatchesAssemblyFilter(string assemblyName, string? assemblyFilter)
    {
        if (string.IsNullOrWhiteSpace(assemblyFilter))
        {
            return true;
        }

        return assemblyName.Contains(assemblyFilter, StringComparison.OrdinalIgnoreCase);
    }

    private static int NormalizeLimit(int limit) => limit switch
    {
        <= 0 => 10,
        > 25 => 25,
        _ => limit
    };
}

internal sealed record AssemblyRecord(string Name, string Path);

internal sealed record ApiTypeRecord(
    string AssemblyName,
    string Namespace,
    string Name,
    string FullName,
    string TypeKind,
    string? BaseType,
    IReadOnlyList<string> Interfaces);

internal sealed record ApiMemberRecord(
    string AssemblyName,
    string DeclaringTypeName,
    string DeclaringTypeFullName,
    string MemberKind,
    string MemberName,
    string FullSymbol,
    string Signature);

internal sealed record CodeFileRecord(string RelativePath, string Content, IReadOnlyList<string> Lines);

internal sealed record WorkspaceBindingRecord(
    string FilePath,
    int LineNumber,
    string WorkspaceName,
    string MaintenanceUnitName,
    string? ParameterName,
    string? ParameterCommandType,
    string? WorkspaceAssemblyService,
    string? MethodType,
    string? MethodQuery,
    string? TargetRule,
    string? TargetMethod);

internal static class SearchText
{
    private static readonly Regex TokenSplitter = new(@"[^A-Za-z0-9_\.]+", RegexOptions.Compiled);
    private static readonly Regex CamelCaseSplitter = new(@"(?<!^)(?=[A-Z])", RegexOptions.Compiled);
    private const int MinimumLooseTermLength = 5;

    public static IReadOnlyList<string> ExpandTerms(string? value)
    {
        var terms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(value))
        {
            return terms.ToList();
        }

        terms.Add(value.Trim());

        foreach (var token in TokenSplitter.Split(value))
        {
            if (string.IsNullOrWhiteSpace(token) || token.Length < MinimumLooseTermLength)
            {
                continue;
            }

            terms.Add(token);

            foreach (var camelPart in CamelCaseSplitter.Split(token))
            {
                if (camelPart.Length >= MinimumLooseTermLength)
                {
                    terms.Add(camelPart);
                }
            }
        }

        return terms
            .Where(term => !string.IsNullOrWhiteSpace(term))
            .OrderByDescending(term => term.Length)
            .ToList();
    }

    public static int Score(IReadOnlyList<string> terms, params string[] values)
    {
        if (terms.Count == 0)
        {
            return 0;
        }

        var score = 0;
        foreach (var value in values)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            foreach (var term in terms)
            {
                if (value.Equals(term, StringComparison.OrdinalIgnoreCase))
                {
                    score += Math.Max(160, term.Length * 16);
                }
                else if (value.EndsWith("." + term, StringComparison.OrdinalIgnoreCase))
                {
                    score += Math.Max(120, term.Length * 12);
                }
                else if (value.Contains(term, StringComparison.OrdinalIgnoreCase))
                {
                    score += Math.Max(20, term.Length * 4);
                }
            }
        }

        return score;
    }
}

internal static class SnippetBuilder
{
    public static IReadOnlyList<string> Build(IReadOnlyList<string> lines, IReadOnlyList<string> terms, int maxSnippets)
    {
        if (terms.Count == 0)
        {
            return Array.Empty<string>();
        }

        var snippets = new List<string>();
        var usedRanges = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var prioritizedTerms = terms.Where(term => term.Length >= 6).ToList();
        var searchTerms = prioritizedTerms.Count > 0 ? prioritizedTerms : terms;

        for (var i = 0; i < lines.Count; i++)
        {
            if (!searchTerms.Any(term => lines[i].Contains(term, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var start = Math.Max(0, i - 2);
            var end = Math.Min(lines.Count - 1, i + 2);
            var rangeKey = $"{start}:{end}";
            if (!usedRanges.Add(rangeKey))
            {
                continue;
            }

            var builder = new StringBuilder();
            for (var current = start; current <= end; current++)
            {
                builder.Append(current + 1)
                    .Append(": ")
                    .AppendLine(lines[current]);
            }

            snippets.Add(builder.ToString().TrimEnd());
            if (snippets.Count >= maxSnippets)
            {
                break;
            }
        }

        return snippets;
    }
}
