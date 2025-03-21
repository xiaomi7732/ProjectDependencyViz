
// See https://aka.ms/new-console-template for more information
using System.Runtime.CompilerServices;
using System.Text.Json;
using ArchAnalyzer.Models;

CancellationToken cancellationToken = CancellationToken.None;

// Assets file
if (args.Length == 0)
{
    Console.WriteLine("Please provide a path to an asset file. It often lives in the obj folder of the project.");
    return;
}
string assetsFilePath = args[0];
if (!File.Exists(assetsFilePath))
{
    Console.WriteLine($"The file {assetsFilePath} does not exist.");
    return;
}

Console.WriteLine($"Reading {assetsFilePath}...");
JsonSerializerOptions options = new()
{
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true
};
using FileStream fs = File.OpenRead(assetsFilePath);
Assets? assets = await JsonSerializer.DeserializeAsync<Assets>(fs, options, cancellationToken).ConfigureAwait(false);
if (assets == null)
{
    Console.WriteLine($"The file {assetsFilePath} is not a valid asset file.");
    return;
}

// Target framework
string targetFramework = ".NETStandard,Version=v2.1";
if (args.Length > 1)
{
    targetFramework = args[1];
}
Console.WriteLine($"Target framework: {targetFramework}. Press any key to continue...");
Console.ReadKey(true);

// Analysis


if (assets.CentralTransitiveDependencyGroups is null || assets.CentralTransitiveDependencyGroups.Count == 0)
{
    Console.WriteLine($"The file {assetsFilePath} does not contain any transitive dependencies.");
    return;
}

// List<string> candidates = assets.CentralTransitiveDependencyGroups
//     .Where(fx => string.Equals(fx.Key, targetFramework, StringComparison.OrdinalIgnoreCase))
//     .SelectMany(fx => fx.Value.Select(g => g.Key))
//     .Order(StringComparer.OrdinalIgnoreCase)
//     .Distinct()
//     .ToList();

if (assets.Targets is null || !assets.Targets.ContainsKey(targetFramework))
{
    Console.WriteLine($"The file {assetsFilePath} does not contain any targets for the framework {targetFramework}.");
    return;
}

List<string> candidates = assets.Targets[targetFramework]
    .Where(item => item.Value.Type == "package")
    .Select(item => item.Key)
    .Order(StringComparer.OrdinalIgnoreCase)
    .Distinct()
    .ToList();

Console.WriteLine("Candidates:");
foreach (string candidate in candidates)
{
    Console.WriteLine(candidate);
}
Console.WriteLine("====");

IDictionary<string, AssetPackageInfo> dependencyInfo =
    assets.Targets[targetFramework].Where(item => item.Value.Type == "package").ToDictionary(item => item.Key, item => item.Value);

HashSet<string> needed = new(candidates, StringComparer.OrdinalIgnoreCase);
foreach (string candidate in candidates)
{
    // Console.WriteLine("Working on candidate: " + candidate);
    foreach (var package in dependencyInfo)
    {
        if (string.Equals(package.Key, candidate, StringComparison.OrdinalIgnoreCase))
        {
            // Console.WriteLine("Hit self: " + package.Key);
            continue;
        }

        (string candidateName, string candidateVersion) = GetNameVersion(candidate);
        // Not self
        if (package.Value.Dependencies is not null && package.Value.Dependencies.ContainsKey(candidateName) && package.Value.Dependencies[candidateName] == candidateVersion)
        {
            Console.WriteLine($"{candidate} is a transitive dependency of {package.Key}");
            needed.Remove(candidate);
            continue;
        }
    }
}

Console.WriteLine("Required dependencies:");
foreach (string need in needed.Order(StringComparer.OrdinalIgnoreCase))
{
    Console.WriteLine(need);
}

// Specific formatter
Console.WriteLine("Nuspec format:");
foreach (string need in needed.Order(StringComparer.OrdinalIgnoreCase))
{
    (string name, _) = GetNameVersion(need);
    string versionTokenName = "$" + name.Replace(".", string.Empty) + "Version" + "$";
    Console.WriteLine($@"<dependency id=""{name}"" version=""{versionTokenName}"" exclude=""Build,Analyzers"" />");
}

(string name, string version) GetNameVersion(string candidate)
{
    string[] parts = candidate.Split('/');
    string name = parts[0];
    string version = parts[1];
    return (name, version);
}
