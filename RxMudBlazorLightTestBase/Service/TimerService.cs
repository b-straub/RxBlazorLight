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

        public override IRxBLScope CreateScope()
        {
            return new TimerScope(this);
        }

        public sealed partial class TimerScope(TimerService service) : IRxBLScope
        {
            public IInput<long> ComponentTimer { get; } = CreateInput(service, 0L);
            public IInput<bool> Suspended { get; } = CreateInput(service, false);

            public IInput<State> TimerState { get; } = CreateInput(service, State.STARTED);

            private IDisposable? _timerDisposable;

            public void EnterScope()
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
