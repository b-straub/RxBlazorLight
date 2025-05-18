using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using R3;
using RxMudBlazorLightSample;
using RxMudBlazorLightTestBase.Service;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

#if DEBUG
ObservableTracker.EnableTracking = true;
ObservableTracker.EnableStackTrace = true;
ObservableSystem.RegisterUnhandledExceptionHandler(_ =>
{
    Console.WriteLine("Unhandled exception in RxBlazorLightCore");
});
#endif

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices(options => { options.PopoverOptions.ThrowOnDuplicateProvider = false; });
builder.Services.AddSingleton<TimerService>();
builder.Services.AddSingleton<LayoutService>();
builder.Services.AddScoped<TestService>();
builder.Services.AddScoped<StateService>();
builder.Services.AddScoped<CrudService>();

await builder.Build().RunAsync();