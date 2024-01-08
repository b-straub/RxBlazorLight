using RxBlazorLightCore;

namespace RxMudBlazorLightTestBase.Service
{
    public class Pizza(string name)
    {
        public readonly string Name = name;

        public override bool Equals(object? o)
        {
            var other = o as Pizza;
            return other?.Name == Name;
        }

        public override int GetHashCode() => Name?.GetHashCode() ?? 0;

        public override string ToString() => Name;
    }

    public class TestColor(ColorEnum color)
    {
        public ColorEnum Color { get; } = color;

        public override bool Equals(object? o)
        {
            var other = o as TestColor;
            return other?.Color == Color;
        }

        public override int GetHashCode() => Color.GetHashCode();

        public override string ToString() => Color.ToString();
    }

    public enum ColorEnum
    {
        RED,
        GREEN,
        BLUE
    }

    public sealed partial class TestService : RxBLServiceBase
    {
        public int Count { get; private set; }

        public ICommand Simple { get; }
		public ICommand Exception { get; }

        public ICommand<int> EqualsTest { get; }
        public ICommandAsync<int> EqualsTestAsync { get; }

        public ICommand Increment => new IncrementCMD(this);
        public ICommand<int> Add { get; }
        public ICommandAsync IncrementAsync { get; }
        public ICommandAsync<int> AddAsync => new AddAsyncCMD(this);
        public ICommandAsync<int> AddAsyncForm { get; }
        public ICommandAsync<int> AddRemoveAsync { get; }
        public IInput<bool> AddMode { get; }

        public ICommand AddIncrementValue { get; }
        public IInput<int> IncrementValue { get; }
        public IInput<bool> CanIncrementCheck { get; }
        public IInput<string> TextValue { get; }

        public IInput<int> RatingValue { get; }

        private bool _canIncrement = false;

        private readonly IInputGroupAsync<Pizza?> _pizzaIPGAsync;
        private readonly IInputGroup<Pizza> _pizzaIPG;
        private readonly IInputGroup<TestColor, ColorEnum> _radioTestExtended;

        public TestService()
        {
            Count = 0;
            Simple = new SimpleCMD();
            EqualsTest = new EqualsTestCmd();
            EqualsTestAsync = new EqualsTestAsyncCmd();

            Exception = new ExceptionCMD(this);
			Add = new AddCMD(this);
            IncrementAsync = new IncrementAsyncCMD(this);
            AddAsyncForm = new AddAsyncCMDForm(this);
            AddRemoveAsync = new AddRemoveAsyncCMDForm(this);
            AddMode = new AddModeIP(this, false);

            IncrementValue = new IncrementValueIP(this, 0);
            CanIncrementCheck = CreateInput(this, false);
            TextValue = CreateInput(this, "No Text");
            RatingValue = new RatingValueIP(this, 0);
            AddIncrementValue = new AddIncrementValueCMD(this);

            _pizzaIPGAsync = new PizzaIPGAsync(this);
            _pizzaIPG = new PizzaIPG(this, PizzaIPG.Pizzas[0]);
            _radioTestExtended = new ColorIPGP(this);
        }

        public override async Task OnInitializedAsync()
        {
            await Task.Delay(1000);
            _canIncrement = true;
            StateHasChanged();
        }

        public override void OnDisposed()
        {
            Count = 10;
        }

        public IInputGroup<Pizza> GetPizzaInput()
        {
            return _pizzaIPG;
        }

        public IInputGroupAsync<Pizza?> GetPizzaInputAsync()
        {
            return _pizzaIPGAsync;
        }

        public IInputGroup<TestColor, ColorEnum> GetRadio()
        {
            return _radioTestExtended;
        }
    }
}
