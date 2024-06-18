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
            .Buffer(TimeSpan.FromMilliseconds(SampleRateMS))
            .Select(crList => Observable.FromAsync(async () =>
            {
                await OnServiceStateHasChangedAsync(crList);
                OnServiceStateHasChanged(crList);
                await InvokeAsync(StateHasChanged);
            }))
            .Concat()
            .Subscribe();
    }

    protected virtual void OnServiceStateHasChanged(IList<ServiceChangeReason> crList)
    {
    }

    protected virtual Task OnServiceStateHasChangedAsync(IList<ServiceChangeReason> crList)
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