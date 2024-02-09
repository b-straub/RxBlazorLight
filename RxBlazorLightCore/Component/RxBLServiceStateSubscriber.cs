using Microsoft.AspNetCore.Components;

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
        Service.Subscribe(cr => ServiceStateHasChanged(cr.ID, cr.Reason), SampleRateMS);
    }

    protected virtual void ServiceStateHasChanged(Guid id, ChangeReason changeReason)
    {
    }
}
