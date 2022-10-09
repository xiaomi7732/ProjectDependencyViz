using System.Diagnostics.CodeAnalysis;

namespace ArchAnalyzer.Models;

public record DrawLink : IEqualityComparer<DrawLink>
{
    public string Source { get; set; } = default!;
    public string Target { get; set; } = default!;
    public string Type { get; set; } = default!;

    public bool Equals(DrawLink? x, DrawLink? y)
    {
        return string.Equals(x?.Source, y?.Source, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x?.Target, y?.Target, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x?.Type, y?.Type, StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode([DisallowNull] DrawLink obj)
    {
        if (string.IsNullOrEmpty(Source) || string.IsNullOrEmpty(Target) || string.IsNullOrEmpty(Type))
        {
            return 0;
        }

        return Source.GetHashCode() ^ Target.GetHashCode() ^ Type.GetHashCode();
    }
}