using RxBlazorLightCore;
using System.Reactive.Linq;

namespace RxMudBlazorLightTestBase.Service
{
    public class TimerService : RxBLService
    {
        public enum State
        {
            STARTED,
            OVER20
        }

        public IRxBLStateScope CreateScope()
        {
            return new TimerStateScope(this);
        }

        public sealed partial class TimerStateScope(TimerService service) : RxBLStateScope<TimerService>(service)
        {
            public IState<long> ComponentTimer { get; } = service.CreateState(0L);

            public IState<bool> Suspended { get; } = service.CreateState(false);

            public IState<State> TimerState { get; } = service.CreateState(State.STARTED);

            private IDisposable? _timerDisposable;

            public override ValueTask OnContextReadyAsync()
            {
                _timerDisposable = Observable.Interval(TimeSpan.FromSeconds(1))
                  .StartWith(0)
                  .Subscribe(_ =>
                  {
                      if (!Suspended.Value)
                      {
                          ComponentTimer.Value++;

                          if (ComponentTimer.Value > 20 && TimerState.Value is State.STARTED)
                          {
                              TimerState.Value = State.OVER20;
                          }
                      }
                  });

                Console.WriteLine("TimerScope ContextReady");

                return base.OnContextReadyAsync();
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _timerDisposable?.Dispose();
                    Console.WriteLine("TimerScope Disposed");
                }
            }
        }
    }
}
