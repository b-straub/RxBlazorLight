
using RxBlazorLightCore;

namespace RxBlazorLightCoreTest
{
    internal class TestServiceState
    {
        public int Counter { get; set; } = 0;

        public static Action<TestServiceState> Increment => s => s.Counter++;

        public static Action<TestServiceState> AddDirect(int value)
        {
            return s => s.Counter += value;
        }

        public static Action<TestServiceState, int> Add => (s, p) => s.Counter += p;

        public static Func<TestServiceState, Task> IncrementAsync => async s =>
        {
            await Task.Delay(1000);
            s.Counter++;
        };

        public static Func<TestServiceState, int, Task> AddAsync => async (s, p) =>
        {
            await Task.Delay(1000);
            s.Counter += p;
        };
    }

    internal class TestService : RxBLService<TestServiceState>
    {
    }
}
