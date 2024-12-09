using R3;
using RxBlazorLightCore;
using Xunit.Abstractions;

namespace RxBlazorLightCoreTestBase
{
    public partial class ServiceFixture
    {
        public Func<bool> CanChangeNotZero => () => IntStateResult > 0;
        public Func<bool> CanChangeBelow(int upperBound) => () => IntStateResult < upperBound;

        public Action Reset => () => IntStateResult = 0;

        public Func<IStateCommandAsync, Task<bool>> ResetAsync => _ =>
        {
            IntStateResult = 0;
            return Task.FromResult(true);
        };

        public Action Increment => () => IntStateResult++;

        public Action Add(int value)
        {
            return () => IntStateResult += value;
        }

        public Func<IStateCommandAsync, Task<bool>> AddAsync(int value)
        {
            return async _ =>
            {
                if (IntStateResult > 0)
                {
                    throw new InvalidOperationException("AddAsync");
                }

                await Task.Delay(1000);
                IntStateResult += value;
                return true;
            };
        }

        public Func<IStateCommandAsync, Task<bool>> MultiplyAsync(int value)
        {
            return async c =>
            {
                await Task.Delay(1000, c.CancellationToken);
                IntStateResult *= value;
                return true;
            };
        }

        public void IncrementState(IState<int> state)
        {
            IntCommand.Execute(() => state.Value++);
        }

        public async Task IncrementStateAsync(IState<int> state)
        {
            await IntCommandAsync.ExecuteAsync(async _ =>
            {
                await Task.Delay(10);
                state.Value++;
            });
        }

        public async Task IncrementStateAsyncC(IState<int> state)
        {
            await IntCommandAsync.ExecuteAsync(async c =>
            {
                await Task.Delay(10, c.CancellationToken);
                state.Value++;
            });
        }

        public void ValueChanging(TestEnum oldValue, TestEnum newValue)
        {
            if (oldValue != newValue)
            {
                EnumStateGroupOldValue = oldValue;
            }
        }

        public async Task ValueChangingAsync(TestEnum oldValue, TestEnum newValue)
        {
            await Task.Delay(100);
            if (oldValue != newValue)
            {
                EnumStateGroupAsyncOldValue = oldValue;
            }
        }

        public enum CmdCRUD
        {
            ADD,
            UPDATE,
            DELETE,
            CLEAR
        }

        public Func<IStateCommandAsync, Task> ChangeCrudListAsync((CmdCRUD CMD, CRUDTest? ITEM) value)
        {
            return async c =>
            {
                if (value.CMD is CmdCRUD.ADD)
                {
                    ArgumentNullException.ThrowIfNull(value.ITEM);
                    CRUDList.Add(value.ITEM);
                }
                else if (value.CMD is CmdCRUD.UPDATE)
                {
                    ArgumentNullException.ThrowIfNull(value.ITEM);
                    var item = CRUDList.FirstOrDefault(i => i.Id == value.ITEM.Id);
                    ArgumentNullException.ThrowIfNull(item);

                    CRUDList.Remove(item);
                    CRUDList.Add(value.ITEM);
                }
                else if (value.CMD is CmdCRUD.DELETE)
                {
                    ArgumentNullException.ThrowIfNull(value.ITEM);
                    CRUDList.Remove(value.ITEM);
                }
                else if (value.CMD is CmdCRUD.CLEAR)
                {
                    CRUDList.Clear();
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                await Task.Delay(5, c.CancellationToken);
            };
        }

        public Func<IStateCommandAsync, Task> ChangeCrudDictAsync((CmdCRUD CMD, Guid? ID, CRUDTest? ITEM) value)
        {
            return async c =>
            {
                if (value.CMD is CmdCRUD.ADD)
                {
                    ArgumentNullException.ThrowIfNull(value.ITEM);
                    ArgumentNullException.ThrowIfNull(value.ID);
                    CRUDDict.Add(value.ID.Value, value.ITEM);
                }
                else if (value.CMD is CmdCRUD.UPDATE)
                {
                    ArgumentNullException.ThrowIfNull(value.ITEM);
                    ArgumentNullException.ThrowIfNull(value.ID);
                    CRUDDict[value.ID.Value] = value.ITEM;
                }
                else if (value.CMD is CmdCRUD.DELETE)
                {
                    ArgumentNullException.ThrowIfNull(value.ID);
                    CRUDDict.Remove(value.ID.Value);
                }
                else if (value.CMD is CmdCRUD.CLEAR)
                {
                    CRUDDict.Clear();
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                await Task.Delay(5, c.CancellationToken);
            };
        }

        public IDisposable ObserveState(IStateProgressObserverAsync observer)
        {
            return Observable
                .Interval(TimeSpan.FromSeconds(1))
                .Index()
                .Select(i => (double)i)
                .Take(2)
                .Subscribe(observer.AsObserver);
        }

        public IDisposable ObserveStateComplex(IStateProgressObserverAsync observer, ITestOutputHelper outputHelper)
        {
            var startObservable = Observable
                .FromAsync(async ct =>
                {
                    outputHelper.WriteLine("StartFirstWork");
                    await Task.Delay(1000, ct);
                    IntState.SetValue(10);
                    outputHelper.WriteLine("StopFirstWork");
                    return Observable.Timer(TimeSpan.FromSeconds(1));
                });

            var stopObservable = Observable
                .FromAsync(async ct =>
                {
                    outputHelper.WriteLine("StartLastWork");
                    await Task.Delay(1000, ct);
                    IntState.SetValue(20);
                    outputHelper.WriteLine("StopLastWork");
                    return Observable.Return(0);
                });
            
            return Observable
                .Interval(TimeSpan.FromMilliseconds(500))
                .Index()
                .Select(i => (double)i)
                .TakeUntil(startObservable)
                .Concat(
                    Observable
                        .Interval(TimeSpan.FromMilliseconds(500))
                        .Index()
                        .Select(i => (double)i)
                        .TakeUntil(stopObservable)
                    )
                .Subscribe(observer.AsObserver);
        }

        public static IDisposable ObserveStateThrow(IStateProgressObserverAsync observer)
        {
            return Observable
                .Interval(TimeSpan.FromSeconds(2))
                .Timeout(TimeSpan.FromSeconds(1))
                .Index()
                .Select(i => (double)i)
                .Take(2)
                .Subscribe(observer.AsObserver);
        }
    }
}