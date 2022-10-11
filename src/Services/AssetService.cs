using ArchAnalyzer.Models;

namespace ArchAnalyzer.Services;

public interface IAssetService
{
    Task<IEnumerable<string>> GetTargetsAsync(Assets assets, CancellationToken cancellationToken);
    string GetLibraryType(Assets assets, string name);
    string GetLibraryType(Assets assets, PackageItem packageItem);

    /// <summary>
    /// Gets all parents of a given package
    /// </summary>
    IEnumerable<PackageItem> GetParents(Assets assets, string target, PackageItem current);
    IEnumerable<PackageItem> GetChildren(Assets assets, string target, PackageItem currentItem);

    bool IsReferencedByProjectDirectly(Assets assets, string target, PackageItem item);
}

internal class AssetService : IAssetService
{
    public IEnumerable<PackageItem> GetChildren(Assets assets, string target, PackageItem currentItem)
    {
        if (assets is null)
        {
            throw new ArgumentNullException(nameof(assets));
        }

        if (string.IsNullOrEmpty(target))
        {
            throw new ArgumentException($"'{nameof(target)}' cannot be null or empty.", nameof(target));
        }

        if (currentItem is null)
        {
            throw new ArgumentNullException(nameof(currentItem));
        }

        if (assets.Targets is null)
        {
            throw new InvalidDataException("Assets has no targets.");
        }

        if (assets.Targets.ContainsKey(target))
        {
            string searchKey = $"{currentItem.Name}/{currentItem.Version}";
            AssetPackageInfo? located = assets.Targets[target].FirstOrDefault(
                p => string.Equals(p.Key, searchKey, StringComparison.Ordinal)).Value;
            if (located?.Dependencies is not null && located.Dependencies.Any())
            {
                return located.Dependencies.Select(pair =>
                {
                    return new PackageItem(pair.Key, pair.Value);
                });
            }
        }

        return Enumerable.Empty<PackageItem>();
    }

    public string GetLibraryType(Assets assets, string name)
    {
        const string unknown = "Unknown";
        if (assets?.Libraries is null)
        {
            throw new ArgumentNullException(nameof(assets));
        }

        string key = $"{name}/";
        KeyValuePair<string, AssetLibraryInfo>? hit = assets.Libraries.FirstOrDefault(item => item.Key.StartsWith(key, StringComparison.OrdinalIgnoreCase));
        if (hit is null)
        {
            return unknown;
        }
        return hit.Value.Value?.Type ?? unknown;
    }

    public string GetLibraryType(Assets assets, PackageItem packageItem)
        => GetLibraryType(assets, packageItem.Name);

    public IEnumerable<PackageItem> GetParents(Assets assets, string target, PackageItem current)
    {
        if (assets?.Targets is null)
        {
            throw new ArgumentNullException(nameof(assets));
        }

        if (!assets.Targets.ContainsKey(target))
        {
            throw new InvalidOperationException($"Target {target} doesn't exist.");
        }

        IDictionary<string, AssetPackageInfo> allPackages = assets.Targets[target];
        return allPackages.Where(item => item.Value.Dependencies is not null && item.Value.Dependencies.Contains(new KeyValuePair<string, string>(current.Name, current.Version)))
            .Select(item =>
            {
                string[] packageItemString = item.Key.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (packageItemString.Length != 2)
                {
                    throw new InvalidOperationException($"Invalid package name format: {item.Key}");
                }
                return new PackageItem(packageItemString[0], packageItemString[1]);
            });
    }

    public Task<IEnumerable<string>> GetTargetsAsync(Assets assets, CancellationToken cancellationToken)
    {
        if (assets.Targets is null || assets.Targets.Count == 0)
        {
            return Task.FromResult(Enumerable.Empty<string>());
        }
        return Task.FromResult(assets.Targets.Keys.AsEnumerable());
    }

    public bool IsReferencedByProjectDirectly(Assets assets, string target, PackageItem item)
    {
        if (assets is null)
        {
            throw new ArgumentNullException(nameof(assets));
        }

        if (string.IsNullOrEmpty(target))
        {
            throw new ArgumentException($"'{nameof(target)}' cannot be null or empty.", nameof(target));
        }

        if (item is null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        if (assets.ProjectFileDependencyGroups?.ContainsKey(target) == true)
        {
            IEnumerable<string> directlyReferenced = assets.ProjectFileDependencyGroups[target];
            return directlyReferenced.Any(r =>
            {
                ProjectFileDependencyItem directlyRef = new ProjectFileDependencyItem(r);
                return string.Equals(directlyRef.Name, item.Name, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(directlyRef.Version, item.Version, StringComparison.OrdinalIgnoreCase);
            });
        }
        return false;
    }
}