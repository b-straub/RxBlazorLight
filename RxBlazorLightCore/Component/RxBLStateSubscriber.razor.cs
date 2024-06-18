using Microsoft.AspNetCore.Components;
using System.Reactive.Linq;

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
        
        Service
            .Buffer(TimeSpan.FromMilliseconds(SampleRateMS))
            .SelectMany(crList => crList.Where(cr => Filter.Select(f => f.ID).Contains(cr.ID)))
            .Select(cr => Observable.FromAsync(async () =>
            {
                await OnServiceStateHasChangedAsync(cr);
                OnServiceStateHasChanged(cr);
                await InvokeAsync(StateHasChanged);
            }))
            .Concat()
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
