using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;
using R3;

// ReSharper disable once CheckNamespace -> use same namespace for all components
namespace RxBlazorLightCore;

public sealed partial class RxBLStateScope<TService, TScope> : ComponentBase, IDisposable
{
    [Parameter, EditorRequired]
    public required Func<IRxBLStateScope<TService>> ScopeFactory { get; init; }

    [Parameter]
    public required RenderFragment ChildContent { get; init; }

    [NotNull]
    private TScope? Scope { get; set; }

    [Parameter]
    public double SampleRateMS { get; set; } = 100;

    private IDisposable? _subscription;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        Scope = (TScope)ScopeFactory();
        ArgumentNullException.ThrowIfNull(Scope);

        _subscription = Scope.AsObservable
            .Chunk(TimeSpan.FromMilliseconds(SampleRateMS))
            .SubscribeAwait(async (crList, _) =>
            {
#if DEBUG
                foreach (var cr in crList)
                {
                    Console.WriteLine($"StateHasChanged from StateID: {cr.StateID}, OwnerID: {Scope.OwnerID}");
                }
#endif
                await InvokeAsync(StateHasChanged);
            });
    }

    public void Dispose()
    {
        _subscription?.Dispose();
        _subscription = null;
        Scope.Dispose();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Scope.OnContextReadyAsync();
        }

        await base.OnAfterRenderAsync(firstRender);
    }
}