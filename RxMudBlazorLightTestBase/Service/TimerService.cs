using RxBlazorLightCore;
using System.Reactive.Linq;

namespace RxMudBlazorLightTestBase.Service
{
    public sealed partial class TimerService : RxBLServiceBase, IDisposable
    {
        public long ComponentTimer { get; private set; }

        private readonly IDisposable _componentTimerDisposable;

        public TimerService()
        {
            _componentTimerDisposable = Observable.Interval(TimeSpan.FromSeconds(1))
              .StartWith(0)
              .Subscribe(s =>
              {
                  ComponentTimer = s;
                  StateHasChanged();
              });
        }

        public void Dispose()
        {
            _componentTimerDisposable.Dispose();
        }
    }
}
