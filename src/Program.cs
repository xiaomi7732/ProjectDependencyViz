using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ArchAnalyzer;
using ArchAnalyzer.ViewModels;
using System.Text.Json;
using ArchAnalyzer.Services.Contracts;
using ArchAnalyzer.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<AssetViewModel>();
builder.Services.AddScoped<ViewModelBase<ArchAnalyzer.Pages.Assets>>(p => p.GetRequiredService<AssetViewModel>());

builder.Services.AddScoped<IDeserializeAssets, TextAssetsDeserializer>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<FullAssetsDrawLinkBuilder>();

builder.Services.AddScoped<JsonSerializerOptions>(_ =>
{
    JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    return options;
});

await builder.Build().RunAsync();
