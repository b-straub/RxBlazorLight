using Microsoft.AspNetCore.Components;
using System.Reactive.Linq;

namespace RxBlazorLightCore;

public partial class RxBLStateSubscriber<T> : ComponentBase
{
    [Parameter, EditorRequired]
    public required T Service { get; init; }

    [Parameter, EditorRequired]
    public required Guid[] IDs { get; init; }

    [Parameter]
    public required RenderFragment ChildContent { get; init; }

    [Parameter]
    public double SampleRateMS { get; set; } = 100;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Service
            .Sample(TimeSpan.FromMilliseconds(SampleRateMS))
            .Where(cr => IDs.Any(i => i == cr.StateID))
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
