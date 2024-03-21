
using RxBlazorLightCore;

namespace RxBlazorLightCoreTest
{
    internal class TestService : RxBLService
    {
        public IState<int> Counter { get; }
        public IStateAsync<int> CounterAsync { get; }
        public IState<IList<string>> StringList { get; }
        public IState<string?> NullString { get; }

        public TestService()
        {
            Counter = this.CreateState(0);
            CounterAsync = this.CreateStateAsync(10);
            StringList = this.CreateState<IList<string>>([]);
            NullString = this.CreateState<string?>(null);
        }

        public static Func<IStateBase<int>, bool> CanChangeNV => s => s.Value < 20;
        public static Func<IStateBase<int>, bool> CanChangeT(int threshold) => s => s.Value < threshold;
        public static Func<IStateBase<string?>, bool> CanChangeS(string compare) => s => compare != s.Value;

        public static Action<IState<int>> Increment => s => s.Value++;

        public static Action<IState<IList<string>>> AddString(string value)
        {
            return s => s.Value.Add(value);
        }

        public static Action<IState<int>> Add(int value)
        {
            return s => s.Value += value;
        }

        public static Func<IStateAsync<int>, Task> IncrementAsync => async s =>
        {
            await Task.Delay(1000);
            s.Value++;
        };

        public static Func<IStateAsync<int>, Task> AddAsync(int value)
        {
            return async s =>
            {
                await Task.Delay(1000);
                s.Value += value;
            };
        }

        public static Func<IStateAsync<int>, CancellationToken, Task> AddAsyncLR(int value)
        {
            return async (s, ct) =>
            {
                await Task.Delay(1000, ct);
                s.Value += value;
            };
        }
    }
}
