using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using RxBlazorLightCore;
using RxMudBlazorLightSample;
using RxMudBlazorLightTestBase.Service;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();

var collector = builder.Services.AddRxBLServiceCollector();
builder.Services.AddRxBLService(collector, sp => new TestService(sp));
builder.Services.AddRxBLService<TimerService>(collector);
builder.Services.AddRxBLService<StateService>(collector);
builder.Services.AddRxBLService<CrudService>(collector);

await builder.Build().RunAsync();
