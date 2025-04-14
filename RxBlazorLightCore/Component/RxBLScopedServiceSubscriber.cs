using Microsoft.AspNetCore.Components;
using System.Reactive.Linq;

namespace RxBlazorLightCore;

public class RxBLScopedServiceSubscriber<T> : OwningComponentBase<T> where T : IRxBLService
{
    [Parameter]
    public double SampleRateMS { get; set; } = 100;

    private IDisposable? _subscription;
    protected override void OnInitialized()
    {
        base.OnInitialized();
        _subscription = Service
            .Buffer(TimeSpan.FromMilliseconds(SampleRateMS))
            .Where(crList => crList.Count > 0)
            .Select(crList => Observable.FromAsync(async () =>
            {
                await OnServiceStateHasChangedAsync(crList);
                OnServiceStateHasChanged(crList);
                await InvokeAsync(StateHasChanged);
            }))
            .Concat()
            .Subscribe();
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

    protected virtual Task OnServiceStateHasChangedAsync(IList<ServiceChangeReason> crList)
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