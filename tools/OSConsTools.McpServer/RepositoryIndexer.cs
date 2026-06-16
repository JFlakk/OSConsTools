using System.Reflection;
using System.Runtime.Loader;
using System.Xml;
using System.Xml.Linq;

namespace OSConsTools.McpServer;

internal static class RepositoryIndexer
{
    public static RepositoryIndex Build(ServerOptions options)
    {
        var warnings = new List<string>();
        var assemblies = new List<AssemblyRecord>();
        var apiTypes = new List<ApiTypeRecord>();
        var apiMembers = new List<ApiMemberRecord>();

        ScanAssemblies(options, warnings, assemblies, apiTypes, apiMembers);

        var codeFiles = ScanCodeFiles(options);
        var workspaceBindings = ScanWorkspaceBindings(options, warnings);

        return new RepositoryIndex(
            Assemblies: assemblies,
            ApiTypes: apiTypes,
            ApiMembers: apiMembers,
            CodeFiles: codeFiles,
            WorkspaceBindings: workspaceBindings,
            ScanWarnings: warnings);
    }

    private static void ScanAssemblies(
        ServerOptions options,
        ICollection<string> warnings,
        ICollection<AssemblyRecord> assemblies,
        ICollection<ApiTypeRecord> apiTypes,
        ICollection<ApiMemberRecord> apiMembers)
    {
        if (!Directory.Exists(options.OneStreamDllDirectory))
        {
            warnings.Add($"OneStream DLL directory not found: {options.OneStreamDllDirectory}");
            return;
        }

        var dllPaths = Directory.GetFiles(options.OneStreamDllDirectory, "*.dll", SearchOption.TopDirectoryOnly)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (dllPaths.Length == 0)
        {
            warnings.Add($"No OneStream DLLs found in: {options.OneStreamDllDirectory}");
            return;
        }

        var loadContext = new MetadataAssemblyLoadContext(dllPaths);

        try
        {
            foreach (var dllPath in dllPaths)
            {
                try
                {
                    var assembly = loadContext.LoadFromAssemblyPath(dllPath);
                    assemblies.Add(new AssemblyRecord(assembly.GetName().Name ?? Path.GetFileNameWithoutExtension(dllPath), dllPath));

                    foreach (var exportedType in GetExportedTypes(assembly))
                    {
                        var typeFullName = TypeDisplayFormatter.FormatTypeName(exportedType);
                        var typeRecord = new ApiTypeRecord(
                            AssemblyName: assembly.GetName().Name ?? Path.GetFileNameWithoutExtension(dllPath),
                            Namespace: exportedType.Namespace ?? string.Empty,
                            Name: TypeDisplayFormatter.GetSimpleTypeName(exportedType),
                            FullName: typeFullName,
                            TypeKind: TypeDisplayFormatter.GetTypeKind(exportedType),
                            BaseType: exportedType.BaseType is null ? null : TypeDisplayFormatter.FormatTypeName(exportedType.BaseType),
                            Interfaces: exportedType.GetInterfaces().Select(TypeDisplayFormatter.FormatTypeName).OrderBy(name => name, StringComparer.OrdinalIgnoreCase).ToArray());

                        apiTypes.Add(typeRecord);

                        foreach (var method in exportedType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                        {
                            if (method.IsSpecialName)
                            {
                                continue;
                            }

                            apiMembers.Add(new ApiMemberRecord(
                                AssemblyName: typeRecord.AssemblyName,
                                DeclaringTypeName: typeRecord.Name,
                                DeclaringTypeFullName: typeRecord.FullName,
                                MemberKind: "method",
                                MemberName: method.Name,
                                FullSymbol: $"{typeRecord.FullName}.{method.Name}",
                                Signature: TypeDisplayFormatter.FormatMethodSignature(method)));
                        }

                        foreach (var property in exportedType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                        {
                            apiMembers.Add(new ApiMemberRecord(
                                AssemblyName: typeRecord.AssemblyName,
                                DeclaringTypeName: typeRecord.Name,
                                DeclaringTypeFullName: typeRecord.FullName,
                                MemberKind: "property",
                                MemberName: property.Name,
                                FullSymbol: $"{typeRecord.FullName}.{property.Name}",
                                Signature: TypeDisplayFormatter.FormatPropertySignature(property)));
                        }
                    }
                }
                catch (Exception ex)
                {
                    warnings.Add($"Failed to index {Path.GetFileName(dllPath)}: {ex.Message}");
                }
            }
        }
        finally
        {
            loadContext.Unload();
        }
    }

    private static IReadOnlyList<CodeFileRecord> ScanCodeFiles(ServerOptions options)
    {
        if (!Directory.Exists(options.SourceRoot))
        {
            return Array.Empty<CodeFileRecord>();
        }

        return Directory
            .EnumerateFiles(options.SourceRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => path.Contains($"{Path.DirectorySeparatorChar}Assemblies{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .Select(path =>
            {
                var content = File.ReadAllText(path);
                return new CodeFileRecord(
                    RelativePath: options.ToRepoRelativePath(path),
                    Content: content,
                    Lines: File.ReadAllLines(path));
            })
            .ToList();
    }

    private static IReadOnlyList<WorkspaceBindingRecord> ScanWorkspaceBindings(ServerOptions options, ICollection<string> warnings)
    {
        if (!Directory.Exists(options.XmlRoot))
        {
            return Array.Empty<WorkspaceBindingRecord>();
        }

        var results = new List<WorkspaceBindingRecord>();

        foreach (var xmlPath in Directory.EnumerateFiles(options.XmlRoot, "*.xml", SearchOption.AllDirectories)
                     .Where(path => path.Contains($"{Path.DirectorySeparatorChar}XML{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
                     .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                var document = XDocument.Load(xmlPath, LoadOptions.SetLineInfo);
                var workspaceName = document.Descendants("workspace").Attributes("name").Select(attribute => attribute.Value).FirstOrDefault() ?? string.Empty;

                foreach (var maintenanceUnit in document.Descendants("maintenanceUnit"))
                {
                    var maintenanceUnitName = maintenanceUnit.Attribute("name")?.Value ?? string.Empty;
                    var wsAssemblyService = maintenanceUnit.Attribute("wsAssemblyService")?.Value;

                    foreach (var parameter in maintenanceUnit.Descendants("parameter"))
                    {
                        var methodQuery = parameter.Element("methodQuery")?.Value;
                        if (string.IsNullOrWhiteSpace(methodQuery) && string.IsNullOrWhiteSpace(wsAssemblyService))
                        {
                            continue;
                        }

                        var (targetRule, targetMethod) = ParseMethodQuery(methodQuery);
                        var lineInfo = (IXmlLineInfo)(parameter.Element("methodQuery") ?? parameter);

                        results.Add(new WorkspaceBindingRecord(
                            FilePath: options.ToRepoRelativePath(xmlPath),
                            LineNumber: lineInfo.HasLineInfo() ? lineInfo.LineNumber : 0,
                            WorkspaceName: workspaceName,
                            MaintenanceUnitName: maintenanceUnitName,
                            ParameterName: parameter.Attribute("name")?.Value,
                            ParameterCommandType: parameter.Element("parameterCommandType")?.Value,
                            WorkspaceAssemblyService: wsAssemblyService,
                            MethodType: parameter.Element("methodType")?.Value,
                            MethodQuery: methodQuery,
                            TargetRule: targetRule,
                            TargetMethod: targetMethod));
                    }
                }
            }
            catch (Exception ex)
            {
                warnings.Add($"Failed to index XML {Path.GetFileName(xmlPath)}: {ex.Message}");
            }
        }

        return results;
    }

    private static IReadOnlyList<Type> GetExportedTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetExportedTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(type => type is not null).Cast<Type>().ToArray();
        }
    }

    private static (string? targetRule, string? targetMethod) ParseMethodQuery(string? methodQuery)
    {
        if (string.IsNullOrWhiteSpace(methodQuery))
        {
            return (null, null);
        }

        var segments = methodQuery
            .Split('}', StringSplitOptions.RemoveEmptyEntries)
            .Select(segment => segment.TrimStart('{'))
            .ToArray();

        if (segments.Length < 2)
        {
            return (null, null);
        }

        return (segments[0], segments[1]);
    }
}

internal sealed class MetadataAssemblyLoadContext : AssemblyLoadContext
{
    private readonly Dictionary<string, string> _assemblyPaths;
    private readonly Dictionary<string, string> _frameworkAssemblyPaths;

    public MetadataAssemblyLoadContext(IEnumerable<string> assemblyPaths)
        : base(nameof(MetadataAssemblyLoadContext), isCollectible: true)
    {
        _assemblyPaths = assemblyPaths
            .Select(path => new
            {
                Path = path,
                Name = AssemblyName.GetAssemblyName(path).Name
            })
            .Where(entry => !string.IsNullOrWhiteSpace(entry.Name))
            .ToDictionary(entry => entry.Name!, entry => entry.Path, StringComparer.OrdinalIgnoreCase);

        _frameworkAssemblyPaths = DiscoverFrameworkAssemblyPaths();
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (!string.IsNullOrWhiteSpace(assemblyName.Name) &&
            _assemblyPaths.TryGetValue(assemblyName.Name, out var path))
        {
            return LoadFromAssemblyPath(path);
        }

        if (!string.IsNullOrWhiteSpace(assemblyName.Name) &&
            _frameworkAssemblyPaths.TryGetValue(assemblyName.Name, out var frameworkPath))
        {
            return LoadFromAssemblyPath(frameworkPath);
        }

        return null;
    }

    private static Dictionary<string, string> DiscoverFrameworkAssemblyPaths()
    {
        var paths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") is string trustedPlatformAssemblies)
        {
            foreach (var path in trustedPlatformAssemblies.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                var fileName = Path.GetFileNameWithoutExtension(path);
                if (!paths.ContainsKey(fileName))
                {
                    paths[fileName] = path;
                }
            }
        }

        var sharedRoot = Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location)!, "..", "..");
        if (Directory.Exists(sharedRoot))
        {
            foreach (var path in Directory.EnumerateFiles(sharedRoot, "*.dll", SearchOption.AllDirectories))
            {
                var fileName = Path.GetFileNameWithoutExtension(path);
                if (!paths.ContainsKey(fileName))
                {
                    paths[fileName] = Path.GetFullPath(path);
                }
            }
        }

        return paths;
    }
}

internal static class TypeDisplayFormatter
{
    public static string GetTypeKind(Type type)
    {
        if (type.IsInterface)
        {
            return "interface";
        }

        if (type.IsEnum)
        {
            return "enum";
        }

        if (type.IsValueType)
        {
            return "struct";
        }

        if (type.IsAbstract && type.IsSealed)
        {
            return "static class";
        }

        if (type.IsClass)
        {
            return "class";
        }

        return "type";
    }

    public static string GetSimpleTypeName(Type type)
    {
        var typeName = type.Name;
        var tickIndex = typeName.IndexOf('`');
        if (tickIndex >= 0)
        {
            typeName = typeName[..tickIndex];
        }

        if (type.IsGenericType)
        {
            var genericArguments = type.GetGenericArguments().Select(FormatTypeName);
            return $"{typeName}<{string.Join(", ", genericArguments)}>";
        }

        return typeName;
    }

    public static string FormatTypeName(Type type)
    {
        if (type.IsByRef)
        {
            return $"{FormatTypeName(type.GetElementType()!)}&";
        }

        if (type.IsArray)
        {
            return $"{FormatTypeName(type.GetElementType()!)}[]";
        }

        if (type.IsGenericParameter)
        {
            return type.Name;
        }

        var namespacePrefix = string.IsNullOrWhiteSpace(type.Namespace) ? string.Empty : $"{type.Namespace}.";
        var typeName = GetSimpleTypeName(type);
        return $"{namespacePrefix}{typeName}".Replace('+', '.');
    }

    public static string FormatMethodSignature(MethodInfo method)
    {
        var parameters = method.GetParameters()
            .Select(parameter => $"{FormatTypeName(parameter.ParameterType)} {parameter.Name}")
            .ToArray();

        return $"{FormatTypeName(method.ReturnType)} {FormatTypeName(method.DeclaringType!)}.{method.Name}({string.Join(", ", parameters)})";
    }

    public static string FormatPropertySignature(PropertyInfo property)
    {
        var accessors = new List<string>();
        if (property.CanRead)
        {
            accessors.Add("get;");
        }

        if (property.CanWrite)
        {
            accessors.Add("set;");
        }

        return $"{FormatTypeName(property.PropertyType)} {FormatTypeName(property.DeclaringType!)}.{property.Name} {{ {string.Join(' ', accessors)} }}";
    }
}
