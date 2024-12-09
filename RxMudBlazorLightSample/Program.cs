using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using RxMudBlazorLightSample;
using RxMudBlazorLightTestBase.Service;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices(options =>
{
    options.PopoverOptions.ThrowOnDuplicateProvider = false;
});
builder.Services.AddSingleton<TestService>();
builder.Services.AddSingleton<TimerService>();
builder.Services.AddScoped<StateService>();
builder.Services.AddSingleton<CrudService>();

await builder.Build().RunAsync();
