using RxBlazorLightCore;
using R3;

namespace RxMudBlazorLightTestBase.Service
{
    public class TimerService : RxBLService
    {
        public enum State
        {
            STARTED,
            OVER20
        }

        public IRxBLStateScope<TimerService> CreateScope()
        {
            return new TimerStateScope(this);
        }

        public sealed class TimerStateScope : RxBLStateScope<TimerService>
        {
            public IState<long> ComponentTimer { get; }
            public IState<State> TimerState { get; }
            public IState<bool> Suspended { get; }
            
            private IDisposable? _timerDisposable;

            public TimerStateScope(TimerService service) : base(service)
            {
                ComponentTimer = this.CreateState(0L);
                TimerState = this.CreateState(State.STARTED);
                Suspended = this.CreateState(false);
            }

            public override ValueTask OnContextReadyAsync()
            {
                _timerDisposable = Observable.Interval(TimeSpan.FromSeconds(1))
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

                Console.WriteLine("TimerStateScope ContextReady");
                return base.OnContextReadyAsync();
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _timerDisposable?.Dispose();
                    Console.WriteLine("TimerStateScope Disposed");
                }
            }
        }
    }
}
