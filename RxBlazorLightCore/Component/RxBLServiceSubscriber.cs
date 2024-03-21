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
            .Subscribe(cr =>
            {
                OnServiceStateHasChanged(cr);
                InvokeAsync(StateHasChanged);
            });
    }

    protected virtual void OnServiceStateHasChanged(ServiceChangeReason cr)
    {
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
            .Subscribe(ServiceStateHasChanged);
    }

    protected virtual void ServiceStateHasChanged(ServiceChangeReason cr)
    {
        InvokeAsync(StateHasChanged);
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
            .Subscribe(ServiceStateHasChanged);
    }

    protected virtual void ServiceStateHasChanged(ServiceChangeReason cr)
    {
        InvokeAsync(StateHasChanged);
    }
}
