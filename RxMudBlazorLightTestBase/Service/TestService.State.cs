using R3;
using RxBlazorLightCore;

namespace RxMudBlazorLightTestBase.Service
{
    public sealed partial class TestService
    {
        public static Action CounterException => () => throw new InvalidOperationException("Command test exception!");

        public Action IncrementCounter => () => Counter++;

        public static Action IncrementCounterIndirect(int value, Action<int> setter) => () => { setter(++value); };

        public Func<IStateCommandAsync, Task> IncrementCounterAsync => async _ =>
        {
            await Task.Delay(1000);
            Counter++;
        };

        public Action AddToCounter(int value) => () => { Counter += value; };

        public Func<IStateCommandAsync, Task> AddToCounterAsync(int value)
        {
            return async c =>
            {
                await Task.Delay(2000, c.CancellationToken);
                Counter += value;
            };
        }

        public Func<IStateProgressObserverAsync, IDisposable> IncrementCounterObservable => observer =>
        {
            var stopObservable = Observable
                .FromAsync(async ct =>
                {
                    await Task.Delay(2000, ct);
                    Counter++;
                    return Observable.Timer(TimeSpan.FromSeconds(1));
                });

            return Observable
                .Interval(TimeSpan.FromMilliseconds(100))
                .TakeUntil(stopObservable)
                .Select(_ => IStateProgressObserverAsync.InterminateValue)
                .Subscribe(observer.AsObserver);
        };

        public Func<IStateProgressObserverAsync, IDisposable> IncrementCounterTimeoutObservable => observer =>
        {
            var stopObservable = Observable
                .FromAsync(async ct =>
                {
                    await Task.Delay(2000, ct);
                    Counter++;
                    return Observable.Timer(TimeSpan.FromSeconds(1));
                });

            return Observable
                .Interval(TimeSpan.FromMilliseconds(100))
                .Index()
                .TakeUntil(stopObservable)
                .Select(i => i > 10 ? throw new TimeoutException() : IStateProgressObserverAsync.InterminateValue)
                .Subscribe(observer.AsObserver);
        };

        public Func<IStateProgressObserverAsync, IDisposable> AddCounterObservable => observer =>
        {
            var startObservable = Observable
                .FromAsync(async ct =>
                {
                    await Task.Delay(1000, ct);
                    Counter += 10;
                    return Observable.Return(Unit.Default);
                });

            var stopObservable = Observable
                .FromAsync(async ct =>
                {
                    await Task.Delay(1000, ct);
                    Counter += 10;
                    return Observable.Return(Unit.Default);
                });
            
            var triggerObservable = Observable
                .Interval(TimeSpan.FromMilliseconds(100))
                .Index()
                .Select(i =>
                {
                    Console.WriteLine($"Interval: {i}");
                    return i;
                })
                .TakeUntil(i => i > 20);

            var progress = 0.0;

            return Observable
                .Interval(TimeSpan.FromMilliseconds(40))
                .TakeUntil(startObservable)
                .Concat(
                    Observable
                        .Interval(TimeSpan.FromMilliseconds(40))
                        .TakeUntil(triggerObservable)
                        .Concat(
                            Observable
                                .Interval(TimeSpan.FromMilliseconds(40))
                                .TakeUntil(stopObservable)
                        )
                )
                .Select(_ => progress += 1.0)
                .Subscribe(observer.AsObserver);
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
        
        public Func<IStateCommandAsync, Task> ToggleAddModeDelayedAsync => async _ =>
        {
            await Task.Delay(1000);
            Observable.Interval(TimeSpan.FromMilliseconds(1000))
                .Take(1)
                .Subscribe(_ =>
                {
                    AddMode.Value = !AddMode.Value;
                });
        };

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
                Console.WriteLine($"{o}, {n}");
            };
        }

        public static Action<TestColor, TestColor> ChangeTestColor => (o, n) => Console.WriteLine($"{o}, {n}");
    }
}