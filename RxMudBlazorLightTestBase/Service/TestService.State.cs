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

        public Func<IStateCommandAsync, Task> IncrementCounterAsync => async _ =>
        {
            await Task.Delay(1000);
            Counter++;
        };

        public Action AddToCounter(int value) => () =>
        {
            Counter += value;
        };

        public Func<IStateCommandAsync, Task> AddToCounterAsync(int value)
        {
            return async c =>
            {
                await Task.Delay(2000, c.CancellationToken);
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

        public bool ColorDisabled(int index)
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
            return GetRadio().HasValue() && GetRadio().Value!.Color is ColorEnum.GREEN;
        }

        public static Func<Pizza, Pizza, Task> ChangePizzaAsync => async (o, n) =>
            {
                await Task.Delay(1000);
                Console.WriteLine($"{o}, {n}");
            };

        public static Action<Pizza, Pizza> ChangePizza(string value)
        {
            return (o, n) =>
            {
                var context = value;
                Console.WriteLine($"{o}, {n}");
            };
        }

        public static Action<TestColor, TestColor> ChangeTestColor => (o, n) => Console.WriteLine($"{o}, {n}");
    }
}
