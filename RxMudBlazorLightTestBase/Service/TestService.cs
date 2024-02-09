﻿using RxBlazorLightCore;

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
        public IState<StateInfo> StateInfoState { get; }

        public IState<int> CountState { get; }

        public IStateProvider<int> Exception { get; }

        public partial class SubScope(TestService service) : IRxBLScope
        {
            public IStateProvider<int> Increment = new IncrementVP(service, service.CountState);

            public IStateTransformer<int> AddAsync = new AddSPAsync(service, service.CountState);

            public void EnterScope()
            {
                Console.WriteLine("EnterScope");
            }

            public void LiveScope()
            {
                Console.WriteLine("EnterScope");
            }
        }

        public IStateProvider<int> Increment { get; }
        public IStateTransformer<int> AddAsync { get; }

        public IServiceStateProvider EqualsTest { get; }
        public IServiceStateTransformer<int> EqualsTestAsync { get; }
        public IStateTransformer<int> Add { get; }
        public IStateProvider<int> IncrementAsync { get; }
  
        public IStateTransformer<int> AddRemoveAsync { get; }

        public IState<bool> AddMode { get; }

        public IState<int> IncrementState { get; }
        public IStateTransformer<int> IncrementStateAdd { get; }

        public IState<bool> CanIncrementCheck { get; }
        public IState<string> TextValue { get; }
        public IState<int> RatingValue { get; }

        private readonly IStateGroup<Pizza> _pizzaState1;
        private readonly IStateGroup<Pizza> _pizzaState2;

        private readonly IStateGroup<TestColor> _radioTestExtended;
        private bool _canIncrement = false;
        private int _equalTestValue = 0;
        private int _equalTestAsyncValue = 0;

        public TestService(IServiceProvider _)
        {
            Console.WriteLine("TestService Create");

            StateInfoState = this.CreateState((StateInfo?)null);
            CountState = this.CreateState(0);

            Increment = new IncrementVP(this, CountState);
            AddAsync = new AddSPAsync(this, CountState);

            EqualsTest = new EqualsTestVP(this);
            EqualsTestAsync = new EqualsTestSPAsync(this);

            Exception = new ExceptionVP(this, CountState);

            Add = new AddSP(this, CountState);
            IncrementAsync = new IncrementVPAsync(this, CountState);
            AddRemoveAsync = new AddRemoveAsyncSP(this, CountState);

            AddMode = this.CreateState(false, s => new AddModeVP(this, s));
            IncrementState = this.CreateState(10, s => new IncrementStateVP(this, s));
            IncrementStateAdd = new IncrementStateAddVP(this, IncrementState);

            CanIncrementCheck = this.CreateState(false);
            TextValue = this.CreateState("No Text");
            RatingValue = this.CreateState(0, s => new RatingValueVP(this, s));

            _pizzaState1 = new PizzaSG(this, PizzaSG.Pizzas[0]);
            _pizzaState2 = new PizzaSG(this, PizzaSG.Pizzas[2]);

            _radioTestExtended = new ColorSG(this);
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
            StateInfoState.Transform(new StateInfo("Initialize"));
        }

        public void ChangeState(string state)
        { 
            if (StateInfoState.HasValue())
            {
                StateInfoState.Transform(StateInfoState.Value with { State = state });
            }
        }

        public IStateGroup<Pizza> GetPizzas1()
        {
            return _pizzaState1;
        }

        public IStateGroup<Pizza> GetPizzas2()
        {
            return _pizzaState2;
        }

        public IStateGroup<TestColor> GetRadio()
        {
            return _radioTestExtended;
        }
    }
}
