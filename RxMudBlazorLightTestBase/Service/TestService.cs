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

    public partial class TestServiceBase : RxBLService
    {
        public IState<StateInfo> ServiceState { get; }

        public TestServiceBase(IServiceProvider _)
        {
            Console.WriteLine("TestService Create");

            ServiceState = this.CreateState(new StateInfo(string.Empty));
        }
    }

    public partial class TestService : TestServiceBase
    {
        public sealed class Scope(TestService service) : RxBLStateScope<TestService>(service)
        {
            public IState<int> CountState = service.CreateState(0);

            public IStateAsync<int> CountStateAsync = service.CreateStateAsync(0);

            public override ValueTask OnContextReadyAsync()
            {
                Console.WriteLine("Scope ContextReady");
                return ValueTask.CompletedTask;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    Console.WriteLine("Scope Disposed");
                }
            }
        }

        public class ColorsStateScope(TestService service) : RxBLStateScope<TestService>(service)
        {
            private static readonly TestColor[] Colors =
            [
                new(ColorEnum.RED),
                new(ColorEnum.GREEN),
                new(ColorEnum.BLUE)
            ];

            public IStateGroupAsync<TestColor> TestColors = service.CreateStateGroupAsync(Colors, Colors[0]);

            public static Func<IStateAsync<TestColor>, TestColor, Task> ChangeTestColorAsync(int context)
            {
                return async (s, p) =>
                {
                    var newContext = context;
                    await Task.Delay(1000);
                    s.Value = p;
                };
            }

            public static Func<IStateAsync<TestColor>, bool> CanChangeTestColor(int context)
            {
                return s =>
                {
                    return context != 1;
                };
            }
        }

        public IState<int> CountState { get; }
        public IStateAsync<int> CountStateAsync { get; }

        public IState<bool> AddMode { get; }

        public IState<int> NumericState { get; }

        public IState<bool> CanIncrementCheck { get; }
        public IState<string> TextValue { get; }
        public IState<int> RatingValue { get; }

        private readonly IStateGroup<Pizza> _pizzaState1;
        private readonly IStateGroupAsync<Pizza> _pizzaState2;

        private readonly IStateGroup<TestColor> _radioTestExtended;
        private bool _canIncrement = false;

        public TestService(IServiceProvider sp) : base(sp)
        {
            Console.WriteLine("TestService Create");

            CountState = this.CreateState(0);
            CountStateAsync = this.CreateStateAsync(0);

            CanIncrementCheck = this.CreateState(false);
            AddMode = this.CreateState(false);
            NumericState = this.CreateState(10);

            TextValue = this.CreateState("No Text");
            RatingValue = this.CreateState(0);

            _pizzaState1 = this.CreateStateGroup(Pizzas, Pizzas[0]);
            _pizzaState2 = this.CreateStateGroupAsync(Pizzas, Pizzas[2]);
            _radioTestExtended = this.CreateStateGroup(Colors, Colors[0], ColorDisabled);
        }

        public IRxBLStateScope CreateScope()
        {
            return new Scope(this);
        }

        public IRxBLStateScope CreateColorsScope()
        {
            return new ColorsStateScope(this);
        }

        protected override async ValueTask ContextReadyAsync()
        {
            Console.WriteLine("TestService OnContextInitialized");
            await Task.Delay(3000);
            _canIncrement = true;
            ServiceState.Change(s => s.Value = new StateInfo("Initialized"));
        }

        public void ChangeState(string state)
        {
            ServiceState.Change(s => s.Value = s.Value with { State = state });
        }

        public IStateGroup<Pizza> GetPizzas1()
        {
            return _pizzaState1;
        }

        public IStateGroupAsync<Pizza> GetPizzas2()
        {
            return _pizzaState2;
        }

        public IStateGroup<TestColor> GetRadio()
        {
            return _radioTestExtended;
        }
    }
}
