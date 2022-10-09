namespace ArchAnalyzer.Models;

public class AssetPackageInfo
{
    public string Type { get; set; } = string.Empty;

    public IDictionary<string, string>? Dependencies { get; set; }
}