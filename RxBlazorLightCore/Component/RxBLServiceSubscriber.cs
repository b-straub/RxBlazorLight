﻿using Microsoft.AspNetCore.Components;
using R3;

// ReSharper disable once CheckNamespace -> use same namespace for all components
namespace RxBlazorLightCore;

public class RxBLServiceSubscriber<T> : ComponentBase where T : IRxBLService
{
    [Inject]
    public required T Service { get; init; }

    [Parameter]
    public required double SampleRateMS { get; init; } = 100;

#if DEBUG
    [Parameter]
    public required bool LogStateChange { get; init; }
#endif

    protected override void OnInitialized()
    {
        base.OnInitialized();

        Service.AsObservable
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

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global -> Library
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

        Observable.Merge(Service1.AsObservable, Service2.AsObservable)
            .Chunk(TimeSpan.FromMilliseconds(SampleRateMS))
            .SubscribeAwait(async (crList, ct) =>
            {
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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (!Service1.Initialized)
            {
                await Service1.OnContextReadyAsync();
            }

            if (!Service2.Initialized)
            {
                await Service2.OnContextReadyAsync();
            }
        }

        await base.OnAfterRenderAsync(firstRender);
    }
}

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global -> Library
public class RxBLServiceSubscriber<T1, T2, T3> : ComponentBase
    where T1 : IRxBLService where T2 : IRxBLService where T3 : IRxBLService
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
        Observable.Merge(Service1.AsObservable, Service2.AsObservable, Service3.AsObservable)
            .Chunk(TimeSpan.FromMilliseconds(SampleRateMS))
            .SubscribeAwait(async (crList, ct) =>
            {
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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (!Service1.Initialized)
            {
                await Service1.OnContextReadyAsync();
            }

            if (!Service2.Initialized)
            {
                await Service2.OnContextReadyAsync();
            }

            if (!Service3.Initialized)
            {
                await Service2.OnContextReadyAsync();
            }
        }

        await base.OnAfterRenderAsync(firstRender);
    }
}