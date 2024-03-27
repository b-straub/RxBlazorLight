using RxBlazorLightCore;

namespace RxMudBlazorLightTestBase.Service
{
    public class StateService : RxBLService
    {
        public IState<int> Counter { get; }
        public int CounterAsync { get; private set; }

        public IStateCommandAsync CounterAsyncCMD { get; }

        public StateService()
        {
            Counter = this.CreateState(0);
            CounterAsyncCMD = this.CreateStateCommandAsync();
        }

        public static Func<int, bool> CounterCanChange => v => v < 20;
        public Func<bool> CounterAsyncCanChange => () => CounterAsync < 10;

        public Action  Increment => () => CounterAsync++;

        public Action Add(int value)
        {
            return () => CounterAsync += value;
        }

        public Func<Task> IncrementAsync => async () =>
        {
            await Task.Delay(1000);
            CounterAsync++;
        };

        public Func<CancellationToken, Task> AddAsync(int value)
        {
            return async ct =>
            {
                await Task.Delay(1000, ct);
                CounterAsync += value;
            };
        }
    }
}
