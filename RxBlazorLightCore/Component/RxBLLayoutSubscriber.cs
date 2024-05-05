using Microsoft.AspNetCore.Components;
using System.Reactive.Linq;

namespace RxBlazorLightCore;

public class RxBLLayoutSubscriber<T> : LayoutComponentBase where T : IRxBLService
{
    [Inject]
    public required T Service { get; init; }

    [Parameter]
    public double SampleRateMS { get; set; } = 100;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Service
            .Sample(TimeSpan.FromMilliseconds(SampleRateMS))
            .Select(async cr =>
            {
                await OnServiceStateHasChangedAsync(cr);
                OnServiceStateHasChanged(cr);
                await InvokeAsync(StateHasChanged);
            })
            .Subscribe();
    }

    protected virtual void OnServiceStateHasChanged(ServiceChangeReason cr)
    {
    }

    protected virtual Task OnServiceStateHasChangedAsync(ServiceChangeReason cr)
    {
        return Task.CompletedTask;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (!Service.Initialized)
            {
                await Service.OnContextReadyAsync();
            }
        }

        await base.OnAfterRenderAsync(firstRender);
    }
}