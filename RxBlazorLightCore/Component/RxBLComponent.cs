using Microsoft.AspNetCore.Components;

namespace RxBlazorLightCore.Component
{
    public class RxBLComponent<T> : ComponentBase, IDisposable where T : RxBLServiceBase
    {
        [CascadingParameter]
        public required T Service { get; init; }

        [CascadingParameter]
        public double SampleMS { get; set; } = 100;

        [CascadingParameter]
        public required bool ServiceInitialized { get; init; }

        private IDisposable? _serviceDisposable;

        protected override void OnInitialized()
        {
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
