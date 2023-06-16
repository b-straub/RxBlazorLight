using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;

namespace RxBlazorLightCore.Component
{
    public class RxBLComponent<T> : ComponentBase, IDisposable where T : RxBLServiceBase
    {
        [CascadingParameter]
        [NotNull]
        public T? Service { get; set; }

        [CascadingParameter]
        public double SampleMS { get; set; } = 100;

        private IDisposable? _serviceDisposable;

        protected override void OnInitialized()
        {
            ArgumentNullException.ThrowIfNull(Service);

            base.OnInitialized();
            _serviceDisposable = Service.Subscribe(InvokeStateChanged, SampleMS);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _serviceDisposable?.Dispose();
            }
        }

        private void InvokeStateChanged()
        {
            InvokeAsync(StateHasChanged);
        }
    }
}
