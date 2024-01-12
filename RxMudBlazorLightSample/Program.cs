using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RxMudBlazorLightSample;
using MudBlazor.Services;
using RxMudBlazorLightTestBase.Service;
using RxBlazorLightCore;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();
builder.Services.AddRxBLService(sp => new TestService(sp));
builder.Services.AddSingleton<TimerService>();
builder.Services.AddRxBLService<TimerService>();

await builder.Build().RunAsync();
