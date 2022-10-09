using ArchAnalyzer.Models;

namespace ArchAnalyzer.Services.Contracts;

public interface IDeserializeAssets
{
    Task<Assets?> DeserializeAsync(CancellationToken cancellationToken);
}