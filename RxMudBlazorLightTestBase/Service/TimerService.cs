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

        public IRxBLScope CreateScope()
        {
            return new TimerScope(this);
        }

        public sealed partial class TimerScope(TimerService service) : IRxBLScope
        {
            public IState<long> ComponentTimer { get; } = service.CreateState(0L);
            public IState<bool> Suspended { get; } = service.CreateState(false);

            public IState<State> TimerState { get; } = service.CreateState(State.STARTED);

            private IDisposable? _timerDisposable;

            public void EnterScope()
            {
                _timerDisposable = Observable.Interval(TimeSpan.FromSeconds(1))
                  .StartWith(0)
                  .Subscribe(_ =>
                  {
                      if (!Suspended.Value)
                      {
                          ComponentTimer.Change(s => s.Value++);

                          if (ComponentTimer.Value > 20 && TimerState.Value is State.STARTED)
                          {
                              TimerState.Change(s => s.Value = State.OVER20);
                          }
                      }
                  });

                Console.WriteLine("TimerScope Entered");
            }

            public void LeaveScope()
            {
                _timerDisposable?.Dispose();
                Console.WriteLine("TimerScope Left");
            }
        }
    }
}
