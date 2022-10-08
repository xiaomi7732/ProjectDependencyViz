using ArchAnalyzer.Models;
using ArchAnalyzer.Services.Contracts;

namespace ArchAnalyzer.Services;

public class FullAssetsDrawLinkBuilder : IDrawLinksBuilder
{
    public FullAssetsDrawLinkBuilder(IAssetService assetService)
    {
        _assetService = assetService ?? throw new ArgumentNullException(nameof(assetService));
    }

    private Assets? _assets;
    private HashSet<DrawLink> _links = new HashSet<DrawLink>();
    private readonly IAssetService _assetService;

    public FullAssetsDrawLinkBuilder WithAssets(Assets assets)
    {
        _assets = assets ?? throw new ArgumentNullException(nameof(assets));
        return this;
    }

    public void Clear()
    {
        _links.Clear();
    }

    public IEnumerable<DrawLink> Build(string target)
    {
        if (target is null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        if (_assets is null)
        {
            throw new InvalidOperationException($"Call {nameof(WithAssets)}() first.");
        }

        if (_assets.ProjectFileDependencyGroups is null)
        {
            throw new InvalidOperationException("Project dependency is not found.");
        }

        IEnumerable<string>? headers = Enumerable.Empty<string>();
        if (_assets.ProjectFileDependencyGroups.Keys.Contains(target, StringComparer.OrdinalIgnoreCase))
        {
            headers = _assets.ProjectFileDependencyGroups[target];
        }

        // Add headers
        foreach (string header in headers)
        {
            ProjectFileDependencyItem dependencyItem = new ProjectFileDependencyItem(header);
            string libraryType = _assetService.GetLibraryType(_assets, dependencyItem.Name, dependencyItem.Version) ?? "unknown";
            _links.Add(new DrawLink { Source = target, Target = $"{dependencyItem.Name}/{dependencyItem.Version}", Type = libraryType });

            AppendDependencies(target, dependencyItem.Name, dependencyItem.Version);
        }

        return _links;
    }

    private void AppendDependencies(string target, string sourceName, string sourceVersion)
    {
        IDictionary<string, AssetPackageInfo>? targetPackageCollection = _assets?.Targets?[target];
        if (targetPackageCollection is null)
        {
            return;
        }

        string key = $"{sourceName}/{sourceVersion}";
        // Found package
        if (targetPackageCollection.ContainsKey(key))
        {
            AssetPackageInfo assetPackageInfo = targetPackageCollection[key];
            if (assetPackageInfo.Dependencies is null)
            {
                return;
            }
            foreach (KeyValuePair<string, string> dependency in assetPackageInfo.Dependencies)
            {
                string destPackage = $"{dependency.Key}/{dependency.Value}";
                string libraryType = _assetService.GetLibraryType(_assets!, dependency.Key, dependency.Value) ?? "unknown";

                _links.Add(new DrawLink() { Source = key, Target = destPackage, Type = libraryType });

                AppendDependencies(target, dependency.Key, dependency.Value);
            }
        }
    }
}