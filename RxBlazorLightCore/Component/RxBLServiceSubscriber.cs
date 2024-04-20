using Microsoft.AspNetCore.Components;
using System.Reactive.Linq;

namespace RxBlazorLightCore;

public class RxBLServiceSubscriber<T> : ComponentBase where T : IRxBLService
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
}

public class RxBLServiceSubscriber<T1, T2> : ComponentBase where T1 : IRxBLService where T2 : IRxBLService
{
    [Inject]
    public required T1 Service1 { get; init; }

    [Inject]
    public required T2 Service2 { get; init; }

    [Parameter]
    public double SampleRateMS { get; set; } = 100;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Observable.Merge(Service1, Service2)
            .Sample(TimeSpan.FromMilliseconds(SampleRateMS))
            .Select(async cr =>
            {
                await ServiceStateHasChangedAsync(cr);
                ServiceStateHasChanged(cr);
                await InvokeAsync(StateHasChanged);
            })
            .Subscribe();
    }

    protected virtual void ServiceStateHasChanged(ServiceChangeReason cr)
    {
    }

    protected virtual Task ServiceStateHasChangedAsync(ServiceChangeReason cr)
    {
        return Task.CompletedTask;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (!Service1.Initialized)
            {
                await Service1.OnContextReadyAsync();
            }
            if (!Service2.Initialized)
            {
                await Service2.OnContextReadyAsync();
            }
        }

        await base.OnAfterRenderAsync(firstRender);
    }
}

public class RxBLServiceSubscriber<T1, T2, T3> : ComponentBase where T1 : IRxBLService where T2 : IRxBLService where T3 : IRxBLService
{
    [Inject]
    public required T1 Service1 { get; init; }

    [Inject]
    public required T2 Service2 { get; init; }

    [Inject]
    public required T3 Service3 { get; init; }

    [Parameter]
    public double SampleRateMS { get; set; } = 100;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Observable.Merge(Service1, Service2, Service3)
            .Sample(TimeSpan.FromMilliseconds(SampleRateMS))
            .Select(async cr =>
             {
                 await ServiceStateHasChangedAsync(cr);
                 ServiceStateHasChanged(cr);
                 await InvokeAsync(StateHasChanged);
             })
            .Subscribe();
    }

    protected virtual void ServiceStateHasChanged(ServiceChangeReason cr)
    {
    }

    protected virtual Task ServiceStateHasChangedAsync(ServiceChangeReason cr)
    {
        return Task.CompletedTask;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (!Service1.Initialized)
            {
                await Service1.OnContextReadyAsync();
            }
            if (!Service2.Initialized)
            {
                await Service2.OnContextReadyAsync();
            }
            if (!Service3.Initialized)
            {
                await Service2.OnContextReadyAsync();
            }
        }

        await base.OnAfterRenderAsync(firstRender);
    }
}
