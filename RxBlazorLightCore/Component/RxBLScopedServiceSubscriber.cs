using Microsoft.AspNetCore.Components;
using R3;

// ReSharper disable once CheckNamespace -> use same namespace for all components
namespace RxBlazorLightCore;

public class RxBLScopedServiceSubscriber<T> : OwningComponentBase<T> where T : IRxBLService
{
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

        _subscription = Service.AsObservable
            .Chunk(TimeSpan.FromMilliseconds(SampleRateMS))
            .SubscribeAwait(async (crList, ct) =>
            {
#if DEBUG
                if (LogStateChange)
                {
                    foreach (var cr in crList)
                    {
                        Console.WriteLine($"StateHasChanged from StateID: {cr.StateID}, OwnerID: {Service.StateID}");
                    }
                }
#endif
                await OnServiceStateHasChangedAsync(crList, ct);
                // ReSharper disable once MethodHasAsyncOverloadWithCancellation -> call both sync and async version
                OnServiceStateHasChanged(crList);
                await InvokeAsync(StateHasChanged);
            });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _subscription?.Dispose();
            _subscription = null;
        }

        base.Dispose(disposing);
    }

    protected virtual void OnServiceStateHasChanged(IList<ServiceChangeReason> crList)
    {
    }

    protected virtual Task OnServiceStateHasChangedAsync(IList<ServiceChangeReason> crList, CancellationToken ct)
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