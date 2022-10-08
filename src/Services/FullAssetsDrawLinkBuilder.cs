using ArchAnalyzer.Models;
using ArchAnalyzer.Services.Contracts;
using ArchAnalyzer.ViewModels;

namespace ArchAnalyzer.Services;

public class FullAssetsDrawLinkBuilder : IDrawLinksBuilder
{
    private PackageItem? _interestingPackage;
    private AnalysisDirection _analysisDirection = AnalysisDirection.Up;

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

    public FullAssetsDrawLinkBuilder WithPackageItem(PackageItem packageItem)
    {
        _interestingPackage = packageItem ?? throw new ArgumentNullException(nameof(packageItem));
        return this;
    }

    public FullAssetsDrawLinkBuilder WithDirection(AnalysisDirection direction)
    {
        _analysisDirection = direction;
        return this;
    }

    public FullAssetsDrawLinkBuilder Clear()
    {
        _links.Clear();
        return this;
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

        if (_interestingPackage is null || _analysisDirection == AnalysisDirection.Full)
        {
            BuildFull(target);
            return _links;
        }

        BuildPartial(target, _interestingPackage);
        return _links;
    }

    private void BuildPartial(string target, PackageItem currentItem)
    {
        switch (_analysisDirection)
        {
            case AnalysisDirection.Up:
                BuildUp(target, currentItem);
                break;
            case AnalysisDirection.Down:
                BuildDown(target, currentItem);
                break;
            default:
                throw new NotSupportedException($"Unsupported analysis direction of {_analysisDirection}");
        }
    }

    private void BuildDown(string target, PackageItem currentItem)
    {
        IEnumerable<PackageItem> children = _assetService.GetChildren(_assets!, target, currentItem);
        if (children.Any())
        {
            foreach (PackageItem child in children)
            {
                _links.Add(currentItem.DrawLinkTo(child, _assets!, _assetService));
                BuildDown(target, child);
            }
        }
        else
        {
            _links.Add(currentItem.DrawLinkTo(new PackageItem("End", "0.0.0"), _assets!, _assetService));
        }
    }

    private void BuildUp(string target, PackageItem currentItem)
    {
        IEnumerable<PackageItem> parents = _assetService.GetParents(_assets!, target, currentItem);
        if (parents.Any())
        {
            foreach (PackageItem parent in parents)
            {
                _links.Add(parent.DrawLinkTo(currentItem, _assets!, _assetService));
                BuildUp(target, parent);
            }
        }
        else
        {
            string? projectName = _assets?.Project?.Restore?.ProjectName;
            string sourceNodeName = string.IsNullOrEmpty(projectName) ? target : $"{projectName}/{target}";
            string libType = _assetService.GetLibraryType(_assets!, currentItem) ?? "Unknown";
            _links.Add(new DrawLink()
            {
                Source = sourceNodeName,
                Target = $"{currentItem.Name}/{currentItem.Version}",
                Type = libType,
            });
        }
    }

    private void BuildFull(string target)
    {
        IEnumerable<string>? headers = Enumerable.Empty<string>();
        if (_assets!.ProjectFileDependencyGroups!.Keys.Contains(target, StringComparer.OrdinalIgnoreCase))
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