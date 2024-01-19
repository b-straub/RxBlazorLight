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

    public record StateInfo(string State);

    public sealed partial class TestService : RxBLService
    {
        public IInput<StateInfo> State { get; }

        public int Count { get; private set; }
        public ICommand Simple { get; }
        public ICommand Exception { get; }

        public partial class SubScope(TestService service) : IRxBLScope
        {
            public ICommand Increment = new IncrementCMD(service);

            public ICommandAsync<int> AddAsync = new AddAsyncCMD(service);
        }

        public ICommand Increment { get; }
        public ICommandAsync<int> AddAsync { get; }

        public ICommand<int> EqualsTest { get; }
        public ICommandAsync<int> EqualsTestAsync { get; }
        public ICommand<int> Add { get; }
        public ICommandAsync IncrementAsync { get; }
        public ICommandAsync<int> AddAsyncForm { get; }
        public ICommandAsync<int> AddRemoveAsync { get; }
        public IInput<bool> AddMode { get; }

        public ICommand AddIncrementValue { get; }
        public IInput<int> IncrementValue { get; }
        public IInput<bool> CanIncrementCheck { get; }
        public IInput<string> TextValue { get; }
        public IInput<int> RatingValue { get; }

        private readonly IInputGroup<Pizza> _pizzaInput1;
        private readonly IInputGroup<Pizza> _pizzaInput2;

        private readonly IInputGroup<TestColor> _radioTestExtended;
        private bool _canIncrement = false;

        public TestService(IServiceProvider _)
        {
            Console.WriteLine("TestService Create");

            State = CreateInput(this, (StateInfo?)null);

            Increment = new IncrementCMD(this);
            AddAsync = new AddAsyncCMD(this);

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

            _pizzaInput1 = new PizzaIPG(this, PizzaIPG.Pizzas[0]);
            _pizzaInput2 = new PizzaIPG(this, PizzaIPG.Pizzas[2]);

            _radioTestExtended = new ColorIPGP(this);
        }

        public override IRxBLScope CreateScope()
        {
            return new SubScope(this);
        }

        protected override async ValueTask ContextReadyAsync()
        {
            Console.WriteLine("TestService OnContextInitialized");
            await Task.Delay(3000);
            _canIncrement = true;
            State.Value = new StateInfo("Initialize");
        }

        public void ChangeState(string state)
        { 
            if (State.HasValue())
            {
                State.Value = State.Value with { State = state };
            }
        }

        public IInputGroup<Pizza> GetPizzas1()
        {
            return _pizzaInput1;
        }

        public IInputGroup<Pizza> GetPizzas2()
        {
            return _pizzaInput2;
        }

        public IInputGroup<TestColor> GetRadio()
        {
            return _radioTestExtended;
        }
    }
}
