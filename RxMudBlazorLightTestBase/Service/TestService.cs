using R3;
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

        public override int GetHashCode() => Name.GetHashCode();

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

    public class TestServiceBase : RxBLService
    {
        public StateInfo ServiceState { get; protected set; }
        public IStateCommand ServiceStateCMD { get; }

        public TestServiceBase()
        {
            Console.WriteLine("TestService Create");

            ServiceState = new StateInfo(string.Empty);
            ServiceStateCMD = this.CreateStateCommand();
        }
    }

    public partial class TestService : TestServiceBase
    {
        public sealed class Scope(TestService service) : RxBLStateScope<TestService>(service)
        {
            public int Counter { get; set; }

            public IStateCommand Command = service.CreateStateCommand();

            public IStateCommandAsync CommandAsync = service.CreateStateCommandAsync();
            public IStateCommandAsync CancellableCommandAsync = service.CreateStateCommandAsync(true);

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

            public Action IncrementCounter => () => Counter++;

            public Func<IStateCommandAsync, Task> IncrementCounterAsync => async _ =>
            {
                await Task.Delay(1000);
                Counter++;
            };

            public Func<IStateCommandAsync, Task> AddToCounterAsync(int value)
            {
                return async c =>
                {
                    await Task.Delay(2000, c.CancellationToken);
                    Counter += value;
                };
            }
        }

        public class ColorsStateScope(TestService service) : RxBLStateScope<TestService>(service)
        {
            public readonly IStateGroupAsync<TestColor> TestColors = service.CreateStateGroupAsync(Colors, Colors[0]);

            public Func<TestColor, TestColor, Task> ChangeTestColorAsync(int context)
            {
                return async (_, _) =>
                {
                    await Task.Delay(1000);
                    await TestColors.ChangeValueAsync(Colors[context]);
                };
            }

            public static Func<bool> CanChangeTestColor(int context) => () => context != 1;
        }

        public int Counter { get; set; }

        public IState<bool> AddMode { get; }

        public IState<int> NumericState { get; }

        public IState<bool> CanIncrementCheck { get; }
        public IState<string> TextValue { get; }
        public IState<int> RatingValue { get; }
        public IState<bool> DarkMode { get; }
        
        
        public IStateProgressObserverAsync IncrementObserver { get; }
        public IStateProgressObserverAsync IncrementDialogObserver { get; }
        public IStateProgressObserverAsync AddObserver { get; }
        
        private readonly IStateGroup<Pizza> _pizzaState1;
        private readonly IStateGroupAsync<Pizza> _pizzaState2;
        private readonly IStateGroupAsync<Pizza> _pizzaStateIndependent;
        private readonly IStateGroup<TestColor> _radioTestExtended;
        private bool _canIncrement;

        public TestService()
        {
            Console.WriteLine("TestService Create");
            CanIncrementCheck = this.CreateState(false);
            AddMode = this.CreateState(false);
            NumericState = this.CreateState(10);

            TextValue = this.CreateState("No Text");
            RatingValue = this.CreateState(0);
            DarkMode = this.CreateState(false);

            IncrementObserver = this.CreateStateObserverAsync();
            AddObserver = this.CreateStateObserverAsync();
            IncrementDialogObserver = this.CreateStateObserverAsync();
            
            _pizzaState1 = this.CreateStateGroup(Pizzas);
            _pizzaState2 = this.CreateStateGroupAsync(Pizzas, Pizzas[2]);
            _pizzaStateIndependent = this.CreateStateGroupAsync(Pizzas, Pizzas[2]);
            _radioTestExtended = this.CreateStateGroup(Colors, Colors[0]);
            
            this.AsChangedObservable(TextValue)
                .Take(1)
                .Select(async _ => await _pizzaState2.ChangeValueAsync(Pizzas[1]))
                .Subscribe();
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
            TextValue.Value = "Context Ready";
            ServiceState = new StateInfo("Initialized");
        }

        public Action ChangeServiceState(string state) => () =>
        {
            ServiceState = new StateInfo(State: state);
        };

        public IStateGroup<Pizza> GetPizzas1()
        {
            return _pizzaState1;
        }

        public IStateGroupAsync<Pizza> GetPizzas2()
        {
            return _pizzaState2;
        }

        public IStateGroupAsync<Pizza> GetPizzasIndependent()
        {
            return _pizzaStateIndependent;
        }

        public IStateGroup<TestColor> GetRadio()
        {
            return _radioTestExtended;
        }
    }
}
