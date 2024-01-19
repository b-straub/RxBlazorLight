using Microsoft.AspNetCore.Components;

namespace RxBlazorLightCore;

public class RxBLServiceContext : ComponentBase
{
    [Inject]
    public RxServiceCollector? ServiceCollector { get; init; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && ServiceCollector is not null)
        {
            foreach (var service in ServiceCollector.Services)
            {
                await service.OnContextReadyAsync();
            }
        }

        await base.OnAfterRenderAsync(firstRender);
    }
}
