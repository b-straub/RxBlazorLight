using Microsoft.AspNetCore.Components;
using System.Reactive.Linq;

namespace RxBlazorLightCore;

public class RxBLServiceChangeSubscriber<T> : ComponentBase where T : IRxBLService
{
    [CascadingParameter]
    public required T Service { get; init; }

    [Parameter]
    public double SampleRateMS { get; set; } = 100;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Service
            .Sample(TimeSpan.FromMilliseconds(SampleRateMS))
            .Subscribe(cr => ServiceStateHasChanged(cr.ID, cr.Reason));
    }

    protected virtual void ServiceStateHasChanged(Guid id, ChangeReason changeReason)
    {
    }
}
