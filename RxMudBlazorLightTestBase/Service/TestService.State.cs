using RxBlazorLightCore;

namespace RxMudBlazorLightTestBase.Service
{
    public sealed partial class TestService
    {
        public static Action CounterException => () => throw new InvalidOperationException("Command test exception!");

        public Action IncrementCounter => () => Counter++;

        public static Action IncrementCounterIndirect(int value, Action<int> setter) => () =>
        {
            setter(++value);
        };

        public Func<Task> IncrementCounterAsync => async () =>
        {
            await Task.Delay(1000);
            Counter++;
        };

        public Action AddToCounter(int value) => () =>
        {
            Counter += value;
        };

        public Func<CancellationToken, Task> AddToCounterAsync(int value)
        {
            return async ct =>
            {
                await Task.Delay(2000, ct);
                Counter += value;
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

        public bool AddModeCanChange(bool _)
        {
            return !CounterCMD.Changing() && !CounterCMDAsync.Changing();
        }

        public bool IncrementStateCanChangeCheck()
        {
            return CanIncrementCheck.Value;
        }

        public bool IncrementStateCanChange()
        {
            return _canIncrement;
        }

        public Func<bool> CounterCanChangeLowerBound(int lowerBound) => () => Counter > lowerBound;

        public bool RatingValueCanChange(int _)
        {
            return GetRadio().Value.Color is ColorEnum.GREEN;
        }

        public Func<Pizza, Task> ChangePizzaAsync => async p =>
            {
                await Task.Delay(1000);
                GetPizzas2().Value = p;
            };

        public Action<Pizza> ChangePizza(string value)
        {
            return p =>
            {
                var context = value;
                GetPizzas1().Value = p;
            };
        }

        public Action<TestColor> ChangeTestColor => c => GetRadio().Value = c;
    }
}
