using System.Globalization;

namespace OSConsTools.McpServer;

internal sealed record ServerOptions(
    string RepoRoot,
    string OneStreamDllDirectory,
    string SourceRoot,
    string XmlRoot)
{
    private const string RepoRootEnvironmentVariable = "OSCONSTOOLS_REPO_ROOT";
    private const string DllDirectoryEnvironmentVariable = "OSCONSTOOLS_ONESTREAM_DLL_DIR";

    public static ServerOptions FromEnvironmentAndArgs(string[] args)
    {
        var parsedArgs = ParseArgs(args);
        var repoRoot = ResolveRepoRoot(parsedArgs);
        var dllDirectory = ResolveDirectory(
            parsedArgs,
            "dll-dir",
            DllDirectoryEnvironmentVariable,
            Path.Combine(repoRoot, "lib", "OneStream"));

        return new ServerOptions(
            RepoRoot: repoRoot,
            OneStreamDllDirectory: dllDirectory,
            SourceRoot: Path.Combine(repoRoot, "OS Consultant Tools"),
            XmlRoot: Path.Combine(repoRoot, "OS Consultant Tools"));
    }

    public string ToRepoRelativePath(string path)
    {
        if (!Path.IsPathRooted(path))
        {
            return path.Replace('\\', '/');
        }

        var relativePath = Path.GetRelativePath(RepoRoot, path);
        return relativePath.Replace('\\', '/');
    }

    private static string ResolveRepoRoot(IReadOnlyDictionary<string, string> parsedArgs)
    {
        return ResolveDirectory(
            parsedArgs,
            "repo-root",
            RepoRootEnvironmentVariable,
            FindRepositoryRootFromBaseDirectory());
    }

    private static string ResolveDirectory(
        IReadOnlyDictionary<string, string> parsedArgs,
        string argumentName,
        string environmentVariable,
        string fallbackPath)
    {
        if (parsedArgs.TryGetValue(argumentName, out var fromArgs) && Directory.Exists(fromArgs))
        {
            return Path.GetFullPath(fromArgs);
        }

        var fromEnvironment = Environment.GetEnvironmentVariable(environmentVariable);
        if (!string.IsNullOrWhiteSpace(fromEnvironment) && Directory.Exists(fromEnvironment))
        {
            return Path.GetFullPath(fromEnvironment);
        }

        return Path.GetFullPath(fallbackPath);
    }

    private static string FindRepositoryRootFromBaseDirectory()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            var csprojPath = Path.Combine(current.FullName, "OSConsTools.csproj");
            var sourceDirectory = Path.Combine(current.FullName, "OS Consultant Tools");

            if (File.Exists(csprojPath) && Directory.Exists(sourceDirectory))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException(
            string.Format(
                CultureInfo.InvariantCulture,
                "Unable to locate the OSConsTools repository root. Set {0} or pass --repo-root.",
                RepoRootEnvironmentVariable));
    }

    private static Dictionary<string, string> ParseArgs(string[] args)
    {
        var parsed = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < args.Length; i++)
        {
            var current = args[i];
            if (!current.StartsWith("--", StringComparison.Ordinal))
            {
                continue;
            }

            var key = current[2..];
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            if (i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal))
            {
                parsed[key] = args[i + 1];
                i++;
            }
        }

        return parsed;
    }
}
