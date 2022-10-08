using System.Text.Json;
using ArchAnalyzer.Models;
using ArchAnalyzer.Services.Contracts;

namespace ArchAnalyzer.Services;

internal class TextAssetsDeserializer : IDeserializeAssets
{
    private string? _assetJsonText;
    private readonly JsonSerializerOptions _options;

    public TextAssetsDeserializer(JsonSerializerOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public TextAssetsDeserializer WithJsonString(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            throw new ArgumentException($"'{nameof(text)}' cannot be null or empty.", nameof(text));
        }
        _assetJsonText = text;
        return this;
    }

    public Task<Assets?> DeserializeAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_assetJsonText))
        {
            throw new InvalidCastException("Can't deserialize empty asset string.");
        }

        Assets? assets = JsonSerializer.Deserialize<Assets>(_assetJsonText, _options);
        return Task.FromResult(assets);
    }
}