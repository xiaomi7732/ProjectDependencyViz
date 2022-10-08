namespace ArchAnalyzer.Models;

public record ProjectFileDependencyItem
{
    public ProjectFileDependencyItem(string value)
    {
        (Name, Version) = Parse(value);
    }

    public string Name { get; init; } = default!;

    public string Version { get; init; } = default!;

    public (string name, string version) Parse(string value)
    {
        // TODO: Make comprehensive parsing
        string[] result = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (result.Length != 3)
        {
            throw new InvalidCastException($"Can't parse value of {value} to {nameof(ProjectFileDependencyItem)}");
        }
        return (result[0], result[2]);
    }
}