using ArchAnalyzer.Models;

namespace ArchAnalyzer.Services;

internal static class PackageItemExtensions
{
    public static DrawLink DrawLinkTo(this PackageItem fromPackage, PackageItem toPackage, Assets assets, IAssetService assetService)
    {
        if (fromPackage is null)
        {
            throw new ArgumentNullException(nameof(fromPackage));
        }

        if (toPackage is null)
        {
            throw new ArgumentNullException(nameof(toPackage));
        }

        if (assets is null)
        {
            throw new ArgumentNullException(nameof(assets));
        }

        if (assetService is null)
        {
            throw new ArgumentNullException(nameof(assetService));
        }

        string libType = assetService.GetLibraryType(assets, toPackage.Name, toPackage.Version) ?? "Unknown";

        return new DrawLink()
        {
            Source = $"{fromPackage.Name}/{fromPackage.Version}",
            Target = $"{toPackage.Name}/{toPackage.Version}",
            Type = libType,
        };
    }
}