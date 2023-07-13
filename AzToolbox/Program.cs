using AzToolbox;
using AzToolbox.Services;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Blazorise.LoadingIndicator;
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
        options.Immediate = true;
    });
builder.Services.AddBootstrapProviders();
builder.Services.AddFontAwesomeIcons();
builder.Services.AddLoadingIndicator();

builder.Services.AddSingleton(services => (IJSInProcessRuntime)services.GetRequiredService<IJSRuntime>());
builder.Services.AddSingleton<VpnBuildService>();
builder.Services.AddSingleton<PacParserService>();
builder.Services.AddScoped<WinSdService>();

await builder.Build().RunAsync();
