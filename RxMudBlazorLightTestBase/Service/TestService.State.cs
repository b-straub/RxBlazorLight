using System.Reactive.Linq;
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

        public Func<IStateObserverAsync, IDisposable> IncrementCounterObservable => observer =>
        {
            var stopObservable = Observable
                .Return(false)
                .Select(_ => Observable.DeferAsync(async ct =>
                {
                    await Task.Delay(2000, ct);
                    Counter++;
                    return Observable.Timer(TimeSpan.FromSeconds(1));
                }))
                .Switch();

            return Observable
                .Interval(TimeSpan.FromMilliseconds(100))
                .TakeUntil(stopObservable)
                .Select(_ => -1L)
                .Subscribe(observer);
        };
        
        public Func<IStateObserverAsync, IDisposable> IncrementCounterTimeoutObservable => observer =>
        {
            var stopObservable = Observable
                .Return(false)
                .Select(_ => Observable.DeferAsync(async ct =>
                {
                    await Task.Delay(2000, ct);
                    Counter++;
                    return Observable.Timer(TimeSpan.FromSeconds(1));
                }))
                .Switch();

            return Observable
                .Interval(TimeSpan.FromMilliseconds(100))
                .TakeUntil(stopObservable)
                .Select(i => i > 10 ? throw new TimeoutException() : -1L)
                .Subscribe(observer);
        };
        
        public Func<IStateObserverAsync, IDisposable> AddCounterObservable => observer =>
        {
            var startObservable = Observable
                .Return(false)
                .Select(_ => Observable.DeferAsync(async token =>
                {
                    await Task.Delay(1000, token);
                    Counter += 10;
                    return Observable.Timer(TimeSpan.FromSeconds(1));
                }))
                .Switch();
            
            var stopObservable = Observable
                .Return(false)
                .Select(_ => Observable.DeferAsync(async token =>
                {
                    await Task.Delay(1000, token);
                    Counter += 10;
                    return Observable.Return(0);
                }))
                .Switch();

            long progress = 0;
            
            return Observable
                .Interval(TimeSpan.FromMilliseconds(30))
                .TakeUntil(startObservable)
                .Concat(
                    Observable
                        .Interval(TimeSpan.FromMilliseconds(30))
                        .TakeUntil(stopObservable)
                )
                .Select(_ => progress++)
                .Subscribe(observer);
        };

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

        public bool AddModeCanChange()
        {
            return !Command.Changing() && !CommandAsync.Changing();
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
