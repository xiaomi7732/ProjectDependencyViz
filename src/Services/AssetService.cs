using ArchAnalyzer.Models;

namespace ArchAnalyzer.Services;

public interface IAssetService
{
    Task<IEnumerable<string>> GetTargetsAsync(Assets assets, CancellationToken cancellationToken);
    string? GetLibraryType(Assets assets, string name, string version);
}

internal class AssetService : IAssetService
{
    public string? GetLibraryType(Assets assets, string name, string version)
    {
        if (assets?.Libraries is null)
        {
            throw new ArgumentNullException(nameof(assets));
        }

        string key = $"{name}/{version}";
        if (assets.Libraries.ContainsKey(key))
        {
            return assets.Libraries[key].Type;
        }
        return null;
    }

    public Task<IEnumerable<string>> GetTargetsAsync(Assets assets, CancellationToken cancellationToken)
    {
        if (assets.Targets is null || assets.Targets.Count == 0)
        {
            return Task.FromResult(Enumerable.Empty<string>());
        }
        return Task.FromResult(assets.Targets.Keys.AsEnumerable());
    }
}