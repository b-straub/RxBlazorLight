using RxBlazorLightCore;

namespace RxBlazorLightSample.Service
{
    public class Pizza
    {
        public Pizza(string name)
        {
            Name = name;
        }

        public readonly string Name;

        public override bool Equals(object? o)
        {
            var other = o as Pizza;
            return other?.Name == Name;
        }

        public override int GetHashCode() => Name?.GetHashCode() ?? 0;

        public override string ToString() => Name;
    }

    public enum TestEnum
    {
        RED,
        GREEN,
        BLUE
    }

    public sealed partial class TestService : RxBLServiceBase, IDisposable
    {
        public int Count { get; private set; }

        public bool UseIncrementState { get; private set; } = true;

        public ICommand Increment;
        public ICommand<int> Add;
        public ICommandAsync IncrementAsync;
        public ICommandAsync<int?> AddAsync;
        public ICommand<bool> UseIncrement;

        public ICommand AddIncrementValue;
        public IInput<int> IncrementValue;
        public IInput<bool> CanIncrementCheck;
        public IInput<string> TextValue;
        public IInput<TestEnum> RadioTest;
        public IInput<int> RatingValue;
        public IInput<Pizza> PizzaTest;
        public Pizza[] Pizzas { get; } =
       {
            new Pizza("Cardinale"), new Pizza("Diavolo"), new Pizza("Margarita"), new Pizza("Spinaci")
        };

        private CancellationTokenSource _addCancel = new();
        private bool _canIncrement = false;

        public TestService()
        {
            Count = 0;
            Increment = CreateCommand(this, DoIncrement, () => _canIncrement);
            Add = CreateCommand<int>(this, (a) => { Count += a; }, (i) => Count > 1);
            IncrementAsync = CreateAsyncCommand(this, async () => { await Task.Delay(4000); Count++; }, () => Count > 2, true);
            AddAsync = CreateAsyncCommand<int?>(this, DoAddAsync, DoAddAsyncCanExecute, DoAddAsyncCancel, true);

            UseIncrement = CreateCommand<bool>(this, (s) => { UseIncrementState = s; }, (i) => !AddAsync.Executing);

            IncrementValue = CreateInput(this, 0, () => CanIncrementCheck is not null && CanIncrementCheck.Value);
            CanIncrementCheck = CreateInput(this, false);
            TextValue = CreateInput(this, "No Text");
            RadioTest = CreateInput(this, TestEnum.RED);
            RatingValue = CreateInput(this, 0, () => RadioTest.Value is TestEnum.GREEN);
            PizzaTest = CreateInput(this, Pizzas[0]);

            AddIncrementValue = CreateCommand(this, () => { Count += IncrementValue.Value; }, () => IncrementValue.Value > 5);
        }

        public override async Task OnInitializedAsync()
        {
            await Task.Delay(1000);
            _canIncrement = true;
            StateHasChanged();
        }

        private void DoIncrement()
        {
            Count++;
        }

        private async Task DoAddAsync(int? offset)
        {
            if (offset == null)
            {
                return;
            }

            if (!_addCancel.TryReset())
            {
                _addCancel = new();
            }

            await Task.Delay(5000, _addCancel.Token);

            Count += (int)offset;
        }

        private bool DoAddAsyncCanExecute(int? value)
        {
            return !UseIncrementState && Count > 2;
        }

        private void DoAddAsyncCancel()
        {
            _addCancel.Cancel();
        }

        public void Dispose()
        {

        }
    }
}
