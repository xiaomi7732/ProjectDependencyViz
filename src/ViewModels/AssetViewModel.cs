using ArchAnalyzer.Models;
using ArchAnalyzer.Services;
using ArchAnalyzer.Services.Contracts;
using Microsoft.JSInterop;

namespace ArchAnalyzer.ViewModels;

public class AssetViewModel : ViewModelBase<ArchAnalyzer.Pages.Assets>
{
    private string? _assetJsonText;
    private readonly IJSRuntime _js;
    private readonly TextAssetsDeserializer _textAssetsDeserializer;
    private readonly IAssetService _assetService;
    private readonly FullAssetsDrawLinkBuilder _fullAssetsDrawLinkBuilder;
    private Assets? _assets;

    public AssetViewModel(
        IJSRuntime jsRuntime,
        IDeserializeAssets textAssetsDeserializer,
        IAssetService assetService,
        FullAssetsDrawLinkBuilder fullAssetsDrawLinkBuilder)
    {
        _js = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        _textAssetsDeserializer = textAssetsDeserializer as TextAssetsDeserializer ?? throw new ArgumentNullException(nameof(textAssetsDeserializer));
        _assetService = assetService ?? throw new ArgumentNullException(nameof(assetService));
        _fullAssetsDrawLinkBuilder = fullAssetsDrawLinkBuilder ?? throw new ArgumentNullException(nameof(fullAssetsDrawLinkBuilder));
    }

    public string? AssetJsonText
    {
        get { return _assetJsonText; }
        set
        {
            if (!string.Equals(_assetJsonText, value, StringComparison.Ordinal))
            {
                _assetJsonText = value;
                RaisePropertyChangeAsync();
            }
        }
    }

    public IEnumerable<string> ValidTargets { get; set; } = Enumerable.Empty<string>();

    public string? SelectedTarget { get; set; }

    public void LoadSampleData()
    {

    }

    protected override async Task OnPropertyChangeAsync(string propertyName)
    {
        if (string.Equals(propertyName, nameof(AssetJsonText), StringComparison.Ordinal))
        {
            if (string.IsNullOrEmpty(AssetJsonText))
            {
                SetupDefaults();
                return;
            }

            _assets = await _textAssetsDeserializer.WithJsonString(AssetJsonText).DeserializeAsync(default);
            if (_assets is null)
            {
                SetupDefaults();
                return;
            }

            ValidTargets = await _assetService.GetTargetsAsync(_assets, default);
            SelectedTarget = ValidTargets.FirstOrDefault();
        }
    }

    private void SetupDefaults()
    {
        ValidTargets = Enumerable.Empty<string>();
    }

    public void LoadChart()
    {
        if (_assets is null)
        {
            SetupDefaults();
            return;
        }

        if (_assets.ProjectFileDependencyGroups is null)
        {
            SetupDefaults();
            return;
        }

        if (string.IsNullOrEmpty(SelectedTarget))
        {
            SetupDefaults();
            return;
        }

        // IEnumerable<string>? headers = Enumerable.Empty<string>();
        // if (_assets.ProjectFileDependencyGroups.Keys.Contains(SelectedTarget, StringComparer.OrdinalIgnoreCase))
        // {
        //     headers = _assets.ProjectFileDependencyGroups[SelectedTarget];
        // }

        // List<DrawLink> drawLinks = new List<DrawLink>();
        // foreach (string header in headers)
        // {
        //     ProjectFileDependencyItem dependencyItem = new ProjectFileDependencyItem(header);
        //     string libraryType = _assetService.GetLibraryType(_assets, dependencyItem.Name, dependencyItem.Version) ?? "unknown";
        //     drawLinks.Add(new DrawLink { Source = SelectedTarget, Target = header, Type = libraryType });
        // }
        IEnumerable<DrawLink> graph = _fullAssetsDrawLinkBuilder.WithAssets(_assets).Build(SelectedTarget);
        _js.InvokeVoidAsync("draw", graph);
    }
}