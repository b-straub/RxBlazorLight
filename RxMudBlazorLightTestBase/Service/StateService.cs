using RxBlazorLightCore;

namespace RxMudBlazorLightTestBase.Service
{
    public class StateService : RxBLService
    {
        public IState<int> Counter { get; }
        public IStateAsync<int> CounterAsync { get; }

        public StateService()
        {
            Counter = this.CreateState(0);
            CounterAsync = this.CreateStateAsync(0);
        }

        public static Func<IState<int>, bool> CounterCanChange => s => s.Value < 20;
        public static Func<IStateAsync<int>, bool> CounterAsyncCanChange => s => s.Value < 10;

        public static Action<IState<int>> Increment => s => s.Value++;

        public static Action<IState<int>> Add(int value)
        {
            return s => s.Value += value;
        }

        public static Func<IStateAsync<int>, Task> IncrementAsync => async s =>
        {
            await Task.Delay(1000);
            s.Value++;
        };

        public static Func<IStateAsync<int>, CancellationToken, Task> AddAsync(int value)
        {
            return async (s, ct) =>
            {
                await Task.Delay(1000, ct);
                s.Value += value;
            };
        }
    }
}
