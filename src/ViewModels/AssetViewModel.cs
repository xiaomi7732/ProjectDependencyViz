using ArchAnalyzer.Models;
using ArchAnalyzer.Services;
using ArchAnalyzer.Services.Contracts;
using Microsoft.AspNetCore.Components.Forms;
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
    private PackageItem? interestingPackage;

    public string? ProjectName { get; set; }

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

    public async Task LoadFilesAsync(InputFileChangeEventArgs e)
    {
        Console.WriteLine(e.File.Name);
        using (Stream readStream = e.File.OpenReadStream(5 * 1024 * 1024, default))
        {
            _assets = await _textAssetsDeserializer.WithStream(readStream).DeserializeAsync(default);
        }

        if (_assets is not null)
        {
            ProjectName = _assets.Project.Restore.ProjectName;

            ValidTargets = await _assetService.GetTargetsAsync(_assets, default);
            SelectedTarget = ValidTargets.FirstOrDefault();
        }
    }

    public IEnumerable<string> ValidTargets { get; set; } = Enumerable.Empty<string>();

    private string? _selectedTarget;
    public string? SelectedTarget
    {
        get { return _selectedTarget; }
        set
        {
            if (!string.Equals(_selectedTarget, value))
            {
                _selectedTarget = value;
                RaisePropertyChangeAsync();
            }
        }
    }

    public IEnumerable<string> PackageList { get; set; } = Enumerable.Empty<string>();
    private string? _selectedPackage;
    public string? SelectedPackage
    {
        get { return _selectedPackage; }
        set
        {
            if (!string.Equals(_selectedPackage, value, StringComparison.Ordinal))
            {
                _selectedPackage = value;
                RaisePropertyChangeAsync();
            }
        }
    }

    public AnalysisDirection AnalysisDirection { get; set; }

    public int SearchLevel { get; set; } = 0;

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

        if (string.Equals(propertyName, nameof(SelectedTarget)))
        {
            _assets = _assets ?? await _textAssetsDeserializer.WithJsonString(AssetJsonText).DeserializeAsync(default);

            if (_assets is null)
            {
                return;
            }

            PackageList = _assets.Targets?[SelectedTarget!]?.Keys?.OrderBy(k => k) ?? Enumerable.Empty<string>();
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

        FullAssetsDrawLinkBuilder builder = _fullAssetsDrawLinkBuilder.Clear().WithAssets(_assets);
        if (!string.IsNullOrEmpty(SelectedPackage))
        {
            builder = builder.WithPackageItem(PackageItem.Parse(SelectedPackage)).WithDirection(AnalysisDirection);
        };

        if (SearchLevel <= 0)
        {
            builder.WithLevel(null);
        }
        else
        {
            builder.WithLevel(SearchLevel);
        }


        IEnumerable<DrawLink> graph = builder.Build(SelectedTarget);
        _js.InvokeVoidAsync("draw", graph, graph.Select(link => link.Type).Distinct());
    }
}