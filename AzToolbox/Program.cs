using AzToolbox;
using AzToolbox.Services;
using Blazorise;
using Blazorise.FluentUI2;
using Blazorise.Icons.FluentUI;
using Blazorise.LoadingIndicator;
using KristofferStrube.Blazor.FileSystemAccess;
using KzA.Blazor.PacParser;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;

System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services
    .AddBlazorise(options =>
    {
        options.Debounce = true;
        options.DebounceInterval = 500;
    });
builder.Services
    .AddFluentUI2Providers()
    .AddFluentUIIcons();
builder.Services.AddLoadingIndicator();

builder.Services.AddSingleton(services => (IJSInProcessRuntime)services.GetRequiredService<IJSRuntime>());
builder.Services.AddSingleton<VpnBuildService>();
builder.Services.AddSingleton<PacParserService>();
builder.Services.AddScoped<WinSdService>();
builder.Services.AddScoped<HexehService>();
builder.Services.AddScoped<DnsZoneFileService>();
builder.Services.AddScoped<CryptoService>();

builder.Services.AddFileSystemAccessService();

await builder.Build().RunAsync();
