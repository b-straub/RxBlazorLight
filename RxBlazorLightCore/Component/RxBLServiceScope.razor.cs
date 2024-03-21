using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;

namespace RxBlazorLightCore
{
    public sealed partial class RxBLServiceScope<TScope, TService> : ComponentBase, IDisposable
    {
        [Inject]
        public required TService Service { get; init; }

        [Parameter, EditorRequired]
        public required Func<IRxBLScope> ScopeFactory { get; init; }

        [Parameter]
        public required RenderFragment ChildContent { get; init; }

        [NotNull]
        private TScope? Scope { get; set; }

        protected override void OnInitialized()
        {
            ArgumentNullException.ThrowIfNull(Service);
            Scope = (TScope)ScopeFactory();
            ArgumentNullException.ThrowIfNull(Scope);
            base.OnInitialized();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await Scope.OnContextReadyAsync();
            }
            await base.OnAfterRenderAsync(firstRender);
        }

        public void Dispose()
        {
            Scope.Dispose();
        }
    }
}
