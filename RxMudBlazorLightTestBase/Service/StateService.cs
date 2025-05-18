using RxBlazorLightCore;

namespace RxMudBlazorLightTestBase.Service
{
    public class StateService : RxBLService
    {
        public class Scope : RxBLStateScope<StateService>
        {
            public IState<int> Counter { get; }

            public Scope(StateService service) : base(service)
            {
                Counter = this.CreateState(0);
            }
            
            public Func<bool> CounterCanChange => () => Counter.Value < 20;
            public Func<bool> CounterAsyncCanChange => () => Counter.Value < 10;

            public Action  Increment => () => Counter.Value++;

            public Action Add(int value)
            {
                return () => Counter.Value += value;
            }

            public Func<Task> IncrementAsync => async () =>
            {
                await Task.Delay(1000);
                Counter.Value++;
            };

            public Func<IStateCommandAsync, Task> AddAsync(int value)
            {
                return async c =>
                {
                    await Task.Delay(1000, c.CancellationToken);
                    Counter.Value += value;
                };
            }
        }
        
        public IState<int> Counter { get; }
        
        public StateService()
        {
            Counter = this.CreateState(0);
        }

        public IRxBLStateScope<StateService> CreateScope()
        {
            return new Scope(this);
        }

        public Func<bool> CounterCanChange => () => Counter.Value < 20;
        public Func<bool> CounterAsyncCanChange => () => Counter.Value < 10;

        public Action  Increment => () => Counter.Value++;

        public Action Add(int value)
        {
            return () => Counter.Value += value;
        }

        public Func<Task> IncrementAsync => async () =>
        {
            await Task.Delay(1000);
            Counter.Value++;
        };

        public Func<IStateCommandAsync, Task> AddAsync(int value)
        {
            return async c =>
            {
                await Task.Delay(1000, c.CancellationToken);
                Counter.Value += value;
            };
        }
    }
}
