using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;

namespace RxBlazorLightCore
{
    public sealed partial class RxBLServiceScope<TScope, TService> : ComponentBase, IDisposable where TService : IRxBLService
        where TScope : IRxBLScope
    {
        [CascadingParameter]
        public required TService Service { get; init; }

        [Parameter]
        public required RenderFragment ChildContent { get; init; }

        [NotNull]
        private TScope? Scope { get; set; }

        protected override void OnInitialized()
        {
            ArgumentNullException.ThrowIfNull(Service);
            Scope = (TScope)Service.CreateScope();
            Scope.EnterScope();
            ArgumentNullException.ThrowIfNull(Scope);
            base.OnInitialized();
        }

        public void Dispose()
        {
            Scope.LeaveScope();
        }
    }
}
