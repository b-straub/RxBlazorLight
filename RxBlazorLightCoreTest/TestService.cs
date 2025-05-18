
using RxBlazorLightCore;

namespace RxBlazorLightCoreTest
{
    internal class TestService : RxBLService
    {
        public IState<int> Counter { get; }

        public int CounterCommandResult { get; private set; }
        
        public IList<string> StringList { get; }
        public IStateCommand StringListCommand { get; }

        public string? NullString { get; private set; }
        public IStateCommand StringCommand { get; }

        public TestService()
        {
            Counter = this.CreateState(0);
            StringListCommand = this.CreateStateCommand();
            StringCommand = this.CreateStateCommand();

            StringList = [];
        }

        public void IncrementState(IState<int> state)
        {
            Command.Execute(() => state.Value++);
        }

        public bool CanChangeNv => CounterCommandResult < 20;
        public bool CanChangeT(int threshold) => CounterCommandResult < threshold;
        public bool CanChangeS(string compare) => NullString != compare;

        public Action Increment => () => CounterCommandResult++;

        public Action AddString(string value) => () =>
        {
            StringList.Add(value);
        };

        public Action SetString(string value) => () =>
        {
            NullString = value;
        };

        public Action Add(int value) => () =>
        {
            CounterCommandResult += value;
        };

        public async Task IncrementAsync(IStateCommandAsync c)
        {
            await Task.Delay(1000, c.CancellationToken);
            CounterCommandResult++;
        }

        public Func<IStateCommandAsync, Task> AddAsync(int value) => async c => 
        {
            await Task.Delay(1000, c.CancellationToken);
            CounterCommandResult += value;
        };

        public Func<IStateCommandAsync, Task> AddAsyncCancel(int value)
        {
            return async c =>
            {
                await Task.Delay(1000, c.CancellationToken);
                CounterCommandResult = value;
            };
        }
    }
}
