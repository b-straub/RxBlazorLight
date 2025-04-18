using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using RxMudBlazorLightSample;
using RxMudBlazorLightTestBase.Service;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();
builder.Services.AddSingleton(sp => new TestService(sp));
builder.Services.AddSingleton<TimerService>();
builder.Services.AddScoped<StateService>();
builder.Services.AddSingleton<CrudService>();

await builder.Build().RunAsync();
