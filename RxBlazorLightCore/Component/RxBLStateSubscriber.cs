using Microsoft.AspNetCore.Components;
using R3;

// ReSharper disable once CheckNamespace -> use same namespace for all components
namespace RxBlazorLightCore;

public class RxBLStateSubscriber<T> : ComponentBase, IDisposable where T : IRxBLStateOwner
{
    [Parameter, EditorRequired]
    public required T Owner { get; init; }

    [Parameter]
    public required Guid[] Filter { get; init; } = [];

    [Parameter]
    public required double SampleRateMS { get; init; } = 100;

#if DEBUG
    [Parameter]
    public bool LogStateChange { get; set; }
#endif

    private IDisposable? _subscription;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        _subscription = Owner.AsObservable
            .Chunk(TimeSpan.FromMilliseconds(SampleRateMS))
            .Where(crList =>
            {
                return Filter.Length == 0 || crList
                    .Select(cr => cr.StateID)
                    .Any(id => Filter.Any(filter => filter == id));
            })
            .SubscribeAwait(async (crList, ct) =>
            {
#if DEBUG
                if (LogStateChange)
                {
                    foreach (var cr in crList)
                    {
                        Console.WriteLine($"StateHasChanged from StateID: {cr.StateID}, OwnerID: {Owner.OwnerID}");
                    }
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