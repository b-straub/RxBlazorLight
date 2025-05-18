using Microsoft.AspNetCore.Components;
using R3;

// ReSharper disable once CheckNamespace -> use same namespace for all components
namespace RxBlazorLightCore;

public class RxBLStateSubscriber<T> : ComponentBase, IDisposable where T : IRxBLStateOwner
{
    [Parameter, EditorRequired]
    public required T Owner { get; init; }
    
    [Parameter]
    public required IStateInformation[] Filter { get; init; } = [];

    [Parameter]
    public required RenderFragment ChildContent { get; init; }

    [Parameter]
    public double SampleRateMS { get; set; } = 100;

    private IDisposable? _subscription;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        _subscription = Owner.AsObservable
            .Chunk(TimeSpan.FromMilliseconds(SampleRateMS))
            .Select(crList =>
            {
                return Filter.Length == 0
                    ? crList.ToList()
                    : crList.Where(cr => Filter.Select(s => s.StateID).Contains(cr.StateID)).ToList();
            })
            .SubscribeAwait(async (crList, ct) =>
            {
#if DEBUG
                foreach (var cr in crList)
                {
                    Console.WriteLine($"StateHasChanged from StateID: {cr.StateID}, OwnerID: {Owner.OwnerID}");
                }
#endif
                await OnServiceStateHasChangedAsync(crList, ct);
                // ReSharper disable once MethodHasAsyncOverloadWithCancellation -> call both sync and async version
                OnServiceStateHasChanged(crList);
                await InvokeAsync(StateHasChanged);
            });
    }

    protected virtual void OnServiceStateHasChanged(IList<ServiceChangeReason> crList)
    {
    }

    protected virtual Task OnServiceStateHasChangedAsync(IList<ServiceChangeReason> crList, CancellationToken ct)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _subscription?.Dispose();
            _subscription = null;
        }
    }
}