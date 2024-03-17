using RxBlazorLightCore;

namespace RxMudBlazorLightTestBase.Service
{
    public sealed partial class TestService
    {
        public static Action<IState<int>> CounterException => _ => throw new InvalidOperationException("Command test exception!");

        public static Action<IState<int>> IncrementCounter => s => s.Value++;

        public static Func<IStateAsync<int>, Task> IncrementCounterAsync => async s =>
        {
            await Task.Delay(1000);
            s.Value++;
        };

        public static Action<IState<int>> AddToCounter(int value)
        {
            return s => s.Value += value;
        }

        public static Func<IStateAsync<int>, CancellationToken, Task> AddToCounterAsync(int value)
        {
            return async (s, ct) =>
            {
                await Task.Delay(2000, ct);
                s.Value += value;
            };
        }

        private static readonly Pizza[] Pizzas =
        [
            new("Cardinale"),
            new("Diavolo"),
            new("Margarita"),
            new("Spinaci")
        ];

        private static readonly TestColor[] Colors =
        [
            new(ColorEnum.RED),
            new(ColorEnum.GREEN),
            new(ColorEnum.BLUE)
        ];

        private bool ColorDisabled(int index)
        {
            return index == 1 && CanIncrementCheck.Value;
        }

        public bool AddModeCanChange(IState<bool> _)
        {
            return !CountState.Changing();
        }

        public bool IncrementStateCanChangeCheck(IState<int> _)
        {
            return CanIncrementCheck.Value;
        }

        public bool IncrementStateCanChange(IStateBase<int> _)
        {
            return _canIncrement;
        }

        public static Func<IStateBase<int>, bool> IntStateCanChangeLowerBound(int lowerBound) => s => s.Value > lowerBound;

        public bool RatingValueCanChange(IState<int> _)
        {
            return GetRadio().Value.Color is ColorEnum.GREEN;
        }

        public static Func<IStateAsync<Pizza>, Pizza, Task> ChangePizzaAsync => async (s, p) =>
            {
                await Task.Delay(1000);
                s.Value = p;
            };

        public static Action<IState<Pizza>, Pizza> ChangePizza(string value)
        {
            return (s, p) =>
            {
                var context = value;
                s.Value = p;
            };
        }

        public static Action<IState<TestColor>, TestColor> ChangeTestColor => (s, p) => s.Value = p;
    }
}
