using Microsoft.AspNetCore.Components;
using R3;

// ReSharper disable once CheckNamespace -> use same namespace for all components
namespace RxBlazorLightCore;

public partial class RxBLStateSubscriber<T> : ComponentBase
{
    [Parameter, EditorRequired]
    public required T Service { get; init; }

    [Parameter]
    public required IStateInformation[] Filter { get; init; } = [];

    [Parameter]
    public required RenderFragment ChildContent { get; init; }

    [Parameter]
    public double SampleRateMS { get; set; } = 100;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        
        Service.AsObservable
            .Chunk(TimeSpan.FromMilliseconds(SampleRateMS))
            .Where(crList => crList.Length > 0)
            .SubscribeAwait(async (crList, ct) =>
            {
                foreach (var cr in crList)
                {
                    if (Filter.Length == 0 || Filter.Select(f => f.ID).Contains(cr.ID))
                    {
                        await OnServiceStateHasChangedAsync(cr, ct);
                        OnServiceStateHasChanged(cr);
                        await InvokeAsync(StateHasChanged);
                    }
                }
            });
    }

    protected virtual void OnServiceStateHasChanged(ServiceChangeReason cr)
    {
    }
    
    protected virtual Task OnServiceStateHasChangedAsync(ServiceChangeReason cr, CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}
