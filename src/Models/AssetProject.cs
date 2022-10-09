namespace ArchAnalyzer.Models;

public class AssetProject
{
    public string Version { get; set; } = default!;

    public AssetProjectRestore Restore { get; set; } = default!;
}

public class AssetProjectRestore
{
    public string ProjectName { get; set; } = default!;
}