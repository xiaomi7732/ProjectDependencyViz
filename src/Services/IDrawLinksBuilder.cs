using ArchAnalyzer.Models;

namespace ArchAnalyzer.Services.Contracts;

public interface IDrawLinksBuilder
{
    IEnumerable<DrawLink> Build(string target);
}