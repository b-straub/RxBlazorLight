using System.Reactive;
using System.Reactive.Linq;
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

        public enum CMD_CRUD
        {
            ADD,
            UPDATE,
            DELETE,
            CLEAR
        }

        public Func<IStateCommandAsync, Task> ChangeCrudListAsync((CMD_CRUD CMD, CRUDTest? ITEM) value)
        {
            return async c =>
            {
                if (value.CMD is CMD_CRUD.ADD)
                {
                    ArgumentNullException.ThrowIfNull(value.ITEM);
                    CRUDList.Add(value.ITEM);
                }
                else if (value.CMD is CMD_CRUD.UPDATE)
                {
                    ArgumentNullException.ThrowIfNull(value.ITEM);
                    var item = CRUDList.Where(i => i.Id == value.ITEM.Id).FirstOrDefault();
                    ArgumentNullException.ThrowIfNull(item);

                    CRUDList.Remove(item);
                    CRUDList.Add(value.ITEM);
                }
                else if (value.CMD is CMD_CRUD.DELETE)
                {
                    ArgumentNullException.ThrowIfNull(value.ITEM);
                    CRUDList.Remove(value.ITEM);
                }
                else if (value.CMD is CMD_CRUD.CLEAR)
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

        public Func<IStateCommandAsync, Task> ChangeCrudDictAsync((CMD_CRUD CMD, Guid? ID, CRUDTest? ITEM) value)
        {
            return async c =>
            {
                if (value.CMD is CMD_CRUD.ADD)
                {
                    ArgumentNullException.ThrowIfNull(value.ITEM);
                    ArgumentNullException.ThrowIfNull(value.ID);
                    CRUDDict.Add(value.ID.Value, value.ITEM);
                }
                else if (value.CMD is CMD_CRUD.UPDATE)
                {
                    ArgumentNullException.ThrowIfNull(value.ITEM);
                    ArgumentNullException.ThrowIfNull(value.ID);
                    CRUDDict[value.ID.Value] = value.ITEM;
                }
                else if (value.CMD is CMD_CRUD.DELETE)
                {
                    ArgumentNullException.ThrowIfNull(value.ID);
                    CRUDDict.Remove(value.ID.Value);
                }
                else if (value.CMD is CMD_CRUD.CLEAR)
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

        public IDisposable ObserveState(IStateObserverAsync observer)
        {
            return Observable
                .Interval(TimeSpan.FromSeconds(1))
                .Take(2)
                .Subscribe(observer);
        }

        public IDisposable ObserveStateComplex(IStateObserverAsync observer, ITestOutputHelper outputHelper)
        {
            var startObservable = Observable
                .Return(false)
                .Select(_ => Observable.DeferAsync(async token =>
                {
                    outputHelper.WriteLine("StartFirstWork");
                    await Task.Delay(1000, token);
                    IntState.SetValue(10);
                    outputHelper.WriteLine("StopFirstWork");
                    return Observable.Timer(TimeSpan.FromSeconds(1));
                }))
                .Switch();
            
            var stopObservable = Observable
                .Return(false)
                .Select(_ => Observable.DeferAsync(async token =>
                {
                    outputHelper.WriteLine("StartLastWork");
                    await Task.Delay(1000, token);
                    IntState.SetValue(20);
                    outputHelper.WriteLine("StopLastWork");
                    return Observable.Return(0);
                }))
                .Switch();

            long progress = 0;
            
            return Observable
                .Interval(TimeSpan.FromMilliseconds(500))
                .TakeUntil(startObservable)
                .Concat(
                    Observable
                        .Interval(TimeSpan.FromMilliseconds(500))
                        .TakeUntil(stopObservable)
                    )
                .Select(_ => progress++)
                .Subscribe(observer);
        }

        public IDisposable ObserveStateThrow(IStateObserverAsync observer)
        {
            var error = new InvalidOperationException("ObserveStateException");

            return Observable
                .Interval(TimeSpan.FromSeconds(2))
                .Timeout(TimeSpan.FromSeconds(1))
                .Take(2)
                .Subscribe(observer);
        }
    }
}