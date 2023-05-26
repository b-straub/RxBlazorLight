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

        public ICommand Increment;
        public ICommand<int> Add;
        public ICommandAsync IncrementAsync;
        public ICommandAsync<int?> AddAsync;
        public ICommandAsync<int?> AddRemoveAsync;
        public IInput<bool> AddMode;

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
        private CancellationTokenSource _addRemoveCancel = new();

        private bool _canIncrement = false;

        public TestService()
        {
            Count = 0;
            Increment = CreateCommand(this, DoIncrement, () => _canIncrement);
            Add = CreateCommand<int>(this, (a) => { Count += a; }, (i) => Count > 1);
            IncrementAsync = CreateAsyncCommand(this, async () => { await Task.Delay(4000); Count++; }, () => Count > 2, true);
            AddAsync = CreateAsyncCommand<int?>(this, DoAddAsync, DoAddAsyncCanExecute, DoAddAsyncCancel, true);

            AddRemoveAsync = CreateAsyncCommand<int?>(this, DoAddRemoveAsync, DoAddRemoveAsyncCanExecute, DoAddRemoveAsyncCancel, true);
            AddMode = CreateInput(this, false, () => !AddRemoveAsync.Executing);

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
            return Count > 2;
        }

        private void DoAddAsyncCancel()
        {
            _addCancel.Cancel();
        }

        private async Task DoAddRemoveAsync(int? offset)
        {
            if (offset == null)
            {
                throw new ArgumentNullException(nameof(offset));
            }

            if (!_addRemoveCancel.TryReset())
            {
                _addRemoveCancel = new();
            }

            await Task.Delay(5000, _addRemoveCancel.Token);

            int val = AddMode.Value ? (int)offset : -(int)offset;
            Count += val;
        }

        private bool DoAddRemoveAsyncCanExecute(int? value)
        {
            return Count > 5;
        }

        private void DoAddRemoveAsyncCancel()
        {
            _addRemoveCancel.Cancel();
        }

        public void Dispose()
        {

        }
    }
}
