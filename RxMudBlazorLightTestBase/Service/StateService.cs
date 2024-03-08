
using RxBlazorLightCore;

namespace RxMudBlazorLightTestBase.Service
{
    public class StateServiceState
    {
        public int Counter { get; set; } = 0;

        public static Action<StateServiceState> Increment => s => s.Counter++;

        public static Action<StateServiceState> Add(int value)
        {
            return s => s.Counter += value;
        }

        public bool CanAdd => Counter > 30;

        public static Func<StateServiceState, Task> IncrementAsync => async s =>
        {
            await Task.Delay(3000);
            s.Counter++;
        };

        public static Func<StateServiceState, Task> AddAsync(int value)
        {
            return async s =>
            {
                await Task.Delay(1000);
                s.Counter += value;
            };
        }
    }

    public class StateService : RxBLService<StateServiceState>
    {
    }
}
