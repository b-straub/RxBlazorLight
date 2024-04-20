using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace RxBlazorLightCore;

public class RxBLServiceContext : ComponentBase
{
    [Inject]
    public RxServiceCollector? ServiceCollector { get; init; }

    [Inject]
    public IServiceProvider? ServiceProvider { get; init; }

    protected override void OnInitialized()
    {
        if (ServiceCollector is not null && ServiceProvider is not null)
        {
            foreach (var type in ServiceCollector.ServiceTypes)
            {
                var service = ActivatorUtilities.CreateInstance(ServiceProvider, type) as IRxBLService;
                ArgumentNullException.ThrowIfNull(service);
                ServiceCollector.AddService(service);
            }
        }
        base.OnInitialized();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && ServiceCollector is not null)
        {
            foreach (var service in ServiceCollector.Services)
            {
                if (!service.Initialized)
                {
                    await service.OnContextReadyAsync();
                }
            }
        }

        await base.OnAfterRenderAsync(firstRender);
    }
}
