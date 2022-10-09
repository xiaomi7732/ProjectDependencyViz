namespace ArchAnalyzer.Models;

public record PackageItem
{
    public PackageItem(string name, string version)
    {
        (Name, Version) = (name, version);
    }

    public string Name { get; set; }
    public string Version { get; set; }

    public static PackageItem Parse(string value)
    {
        string[] split = value.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (split.Length != 2)
        {
            throw new InvalidOperationException($"Invalid package name format: {value}");
        }
        return new PackageItem(split[0], split[1]);
    }
}
