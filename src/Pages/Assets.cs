using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ArchAnalyzer.Pages;

public partial class Assets : ViewModelComponentBase<Assets>
{

    [Inject]
    private IJSRuntime? _jsRuntime { get; set; }

    public void LoadChart()
    {
        _jsRuntime?.InvokeVoidAsync("draw");
    }
}