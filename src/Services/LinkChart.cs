using ArchAnalyzer.Models;

namespace ArchAnalyzer.Services;

public class LinkChart
{
    private readonly HashSet<DrawLink> _linkHolder = new HashSet<DrawLink>();
    public void AddLink(DrawLink link)
    {
        if(_linkHolder.Contains(link))
        {
            return;
        }
        _linkHolder.Add(link);
    }
}