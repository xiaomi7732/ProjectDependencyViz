using System.Text.Json;
using ArchAnalyzer.Models;
using ArchAnalyzer.Services.Contracts;

namespace ArchAnalyzer.Services;

internal class TextAssetsDeserializer : IDeserializeAssets
{
    private string? _assetJsonText;
    private Stream? _stream;
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

    public TextAssetsDeserializer WithStream(Stream inputStream)
    {
        if (inputStream is null)
        {
            throw new ArgumentNullException(nameof(inputStream));
        }
        _stream = inputStream;
        return this;
    }

    public async Task<Assets?> DeserializeAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_assetJsonText) && _stream is null)
        {
            throw new InvalidCastException("Can't deserialize empty asset string/stream.");
        }

        Assets? assets = null;
        if (_stream is not null)
        {
            assets = await JsonSerializer.DeserializeAsync<Assets>(_stream, _options, cancellationToken);
        }
        else if (!string.IsNullOrEmpty(_assetJsonText))
        {
            assets = JsonSerializer.Deserialize<Assets>(_assetJsonText, _options);
        }
        return assets;
    }
}