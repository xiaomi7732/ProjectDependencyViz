using Microsoft.JSInterop;

namespace ArchAnalyzer.Pages;

public partial class Assets
{
    public void LoadChart()
    {
        Console.WriteLine(nameof(LoadChart));

        _js.InvokeVoidAsync("draw");
    }
}