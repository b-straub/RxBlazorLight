using RxBlazorLightCore;
using System.Reactive.Linq;

namespace RxMudBlazorLightTestBase.Service
{
    public sealed partial class TimerService : RxBLServiceBase
    {
        public long ComponentTimer { get; private set; }

        private IDisposable? _componentTimerDisposable;

        public TimerService()
        {
            ComponentTimer = 0;
            Console.WriteLine("TimerService Create");
        }

        protected override ValueTask InitializeContext()
        {
            Console.WriteLine("TimerService OnContextInitialized");

            _componentTimerDisposable = Observable.Interval(TimeSpan.FromSeconds(1))
              .StartWith(0)
              .Subscribe(s =>
              {
                  ComponentTimer = s;
                  StateHasChanged();
              });

            return ValueTask.CompletedTask;
        }

        protected override void DisposeContext()
        {
            _componentTimerDisposable?.Dispose();
            _componentTimerDisposable = null;
            Console.WriteLine("TimerService OnContextDisposed");
        }
    }
}
