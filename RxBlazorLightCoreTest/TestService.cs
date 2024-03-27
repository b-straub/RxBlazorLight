
using RxBlazorLightCore;

namespace RxBlazorLightCoreTest
{
    internal class TestService : RxBLService
    {
        public IState<int> Counter { get; }

        public int CounterCommandResult { get; private set; }
        public IStateCommand CounterCommand { get; }
        public IStateCommandAsync CounterCommandAsync { get; }


        public IList<string> StringList { get; }
        public IStateCommand StringListCommand { get; }

        public string? NullString { get; private set; }
        public IStateCommand StringCommand { get; }

        public TestService()
        {
            Counter = this.CreateState(0);
            CounterCommand = this.CreateStateCommand();
            CounterCommandAsync = this.CreateStateCommandAsync();

            StringListCommand = this.CreateStateCommand();
            StringCommand = this.CreateStateCommand();

            StringList = [];
        }

        public bool CanChangeNV => CounterCommandResult < 20;
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

        public async Task IncrementAsync()
        {
            await Task.Delay(1000);
            CounterCommandResult++;
        }

        public Func<Task> AddAsync(int value) => async () => 
        {
            await Task.Delay(1000);
            CounterCommandResult += value;
        };

        public Func<CancellationToken, Task> AddAsyncCancel(int value)
        {
            return async ct =>
            {
                await Task.Delay(1000, ct);
                CounterCommandResult = value;
            };
        }
    }
}
