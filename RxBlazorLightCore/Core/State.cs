
using System.Reactive;
using System.Reactive.Linq;

namespace RxBlazorLightCore
{

    public class State<S, TInterface, TType> : IState<TInterface, TType>
        where S : RxBLService
        where TType : TInterface
    {
        public TInterface? Value { get; private set; }
        public ValueProviderPhase Phase => _valueProvider.Phase;
        public Guid ID => _valueProvider.ID;

        private readonly IValueProvider<TType> _valueProvider;

        protected State(S service, TType? value, Func<IState<TInterface, TType>, IValueProvider<TType>>? valueProviderFactory = null)
        {
            Value = value;
            _valueProvider = valueProviderFactory is null ?
                new DefaultValueProvider<S, TInterface, TType>(service, this) : valueProviderFactory(this);
        }

        protected State(S service, TType? value, Func<IState<TType>, IValueProvider<TType>>? valueProviderFactory = null)
        {
            Value = value;
            _valueProvider = valueProviderFactory is null ?
                new DefaultValueProvider<S, TInterface, TType>(service, this) : valueProviderFactory((IState<TType>)this);
        }

        public bool HasValue()
        {
            return Value is not null;
        }

        public virtual bool CanChange()
        {
            return true;
        }

        internal void SetValueIntern(TType? value)
        {
            Value = value;
        }

        public static IState<TInterface, TType> Create(S service, TType? value, Func<IState<TInterface, TType>, IValueProvider<TType>>? valueProviderFactory = null)
        {
            return new State<S, TInterface, TType>(service, value, valueProviderFactory);
        }

        public void Run(TType? value)
        {
            _valueProvider.Run(value);
        }

        public bool LongRunning()
        {
            return _valueProvider.LongRunning();
        }

        public bool CanCancel()
        {
            return _valueProvider.CanCancel();
        }

        public void Cancel()
        {
            _valueProvider.Cancel();
        }
    }

    public class State<S, TType>(S service, TType? value, Func<IState<TType>, IValueProvider<TType>>? valueProviderFactory = null) :
        State<S, TType, TType>(service, value, valueProviderFactory), IState<TType>
        where S : RxBLService
    {
        public static IState<TType> Create(S service, TType? value, Func<IState<TType>, IValueProvider<TType>>? valueProviderFactory = null)
        {
            return new State<S, TType>(service, value, valueProviderFactory);
        }
    }

    public class State<S>(S service) :
        State<S, object?, object?>(service, IState.Default, null), IState
        where S : RxBLService
    {
        public static IState Create(S service)
        {
            return new State<S>(service);
        }
    }

    public abstract class StateGroup<S, TInterface, TType> : State<S, TInterface, TType>, IStateGroup<TInterface, TType>
        where S : RxBLService where
        TType : TInterface
    {
        protected StateGroup(S service, TType? value, Func<IState<TInterface, TType>, IValueProvider<TType>>? valueProviderFactory = null) : 
            base(service, value, valueProviderFactory) { }

        public abstract TInterface[] GetItems();

        public virtual bool IsItemDisabled(int index)
        {
            return false;
        }
    }

    public abstract class StateGroup<S, TType> : State<S, TType, TType> where S : RxBLService
    {
        protected StateGroup(S service, TType? value, Func<IState<TType>, IValueProvider<TType>>? valueProviderFactory = null) : 
            base(service, value, valueProviderFactory) { }

        public abstract TType[] GetItems();

        public virtual bool IsItemDisabled(int index)
        {
            return false;
        }
    }

    public abstract class ValueProviderBase<S, T, TInterface, TType> : IValueProviderBase<T>
        where S : RxBLService
        where TType : TInterface
    {
        public ValueProviderPhase Phase { get; protected set; } = ValueProviderPhase.NONE;
        public Guid ID { get; }

        protected S Service { get; }

        protected State<S, TInterface, TType> State { get; }

        private readonly bool _setState;
        private readonly bool _runAsync;
        private bool _canceled;
        private IDisposable? _runDisposable;

        protected ValueProviderBase(S service, IState<TInterface, TType> state, bool setState, bool runAsync)
        {
            Service = service;
            State = (State<S, TInterface, TType>)state;
            _setState = setState;
            _runAsync = runAsync;
            _canceled = false;

            ID = Guid.NewGuid();
        }

        public virtual bool CanCancel()
        {
            return false;
        }

        public void Cancel()
        {
            if (!CanCancel())
            {
                StateChanged(ValueProviderPhase.EXCEPTION, new InvalidOperationException("CanCancel() returning false!"));
                return;
            }

            if (!_runAsync)
            {
                StateChanged(ValueProviderPhase.EXCEPTION, new InvalidOperationException("Cancel() not allowed for Sync Provider!"));
                return;
            }

            _canceled = true;
            _runDisposable?.Dispose();
        }

        public virtual bool LongRunning()
        {
            return false;
        }

        protected abstract IObservable<TType?> ProvideObervableValueBase(T? value, TType? state);

        protected void RunBase(T? value)
        {
            StateChanged(ValueProviderPhase.PROVIDING);

            _runDisposable = ProvideObervableValueBase(value, (TType?)State.Value)
                .Finally(() =>
                {
                    if (_canceled)
                    {
                        StateChanged(ValueProviderPhase.CANCELED);
                    }
                })
                .Subscribe(v =>
                {
                    if (_setState)
                    {
                        State.SetValueIntern(v);
                    }

                    StateChanged(ValueProviderPhase.PROVIDED);
                },
                ex =>
                {
                    if (ex.GetType() == typeof(TaskCanceledException))
                    {
                        StateChanged(ValueProviderPhase.CANCELED);
                    }
                    else
                    {
                        StateChanged(ValueProviderPhase.EXCEPTION, ex);
                    }
                });
        }

        private void StateChanged(ValueProviderPhase phase, Exception? exception = null)
        {
            Phase = phase;
            Service.StateHasChanged(ID, Phase is ValueProviderPhase.EXCEPTION ? ChangeReason.EXCEPTION : ChangeReason.STATE, exception);
        }

    }

    internal class DefaultValueProvider<S, TInterface, TType>(S service, IState<TInterface, TType> state) :
        ValueProviderBase<S, TType, TInterface, TType>(service, state, true, true), IValueProvider<TType>
        where S : RxBLService
        where TType : TInterface
    {
        public void Run(TType? value)
        {
            RunBase(value);
        }

        protected override IObservable<TType?> ProvideObervableValueBase(TType? value, TType? state)
        {
            return Observable.Return(value);
        }
    }

    public abstract class AsyncValueProvider<S, T, TType>(S service, IState<TType> state) :
       ValueProviderBase<S, T, TType, TType>(service, state, true, true), IValueProvider<T>
       where S : RxBLService
    {
        public void Run(T? value)
        {
            RunBase(value);
        }

        protected override IObservable<TType?> ProvideObervableValueBase(T? value, TType? state)
        {
            return Observable.FromAsync(async ct =>
            {
                return await ProvideValueAsync(value, ct);
            });
        }

        protected abstract Task<TType?> ProvideValueAsync(T? value, CancellationToken cancellationToken);
    }

    public abstract class ValueProvider<S, T, TType>(S service, IState<TType> state) :
     ValueProviderBase<S, T, TType, TType>(service, state, true, false), IValueProvider<T>
     where S : RxBLService
    {
        public void Run(T? value)
        {
            RunBase(value);
        }

        protected override IObservable<TType?> ProvideObervableValueBase(T? value, TType? state)
        {
            return Observable.Return(Unit.Default).Select(_ =>
            {
                return ProvideValue(value);
            });
        }

        protected abstract TType? ProvideValue(T? value);
    }

    public abstract class AsyncValueProvider<S, T>(S service, IState<T> state) :
       ValueProviderBase<S, T, T, T>(service, state, true, true), IValueProviderVoid<T>
       where S : RxBLService
    {
        public void Run()
        {
            RunBase(default);
        }

        protected override IObservable<T?> ProvideObervableValueBase(T? value, T? state)
        {
            return Observable.FromAsync(ProvideValueAsync);
        }

        protected abstract Task<T?> ProvideValueAsync(CancellationToken cancellationToken);
    }

    public abstract class ValueProvider<S, T>(S service, IState<T> state) :
      ValueProviderBase<S, T, T, T>(service, state, true, false), IValueProviderVoid<T>
      where S : RxBLService
    {
        public void Run()
        {
            RunBase(default);
        }

        protected override IObservable<T?> ProvideObervableValueBase(T? value, T? state)
        {
            return Observable.Return(Unit.Default).Select(_ =>
            {
                return ProvideValue();
            });
        }

        protected abstract T? ProvideValue();
    }

    public abstract class AsyncValueRefProvider<S, T, TInterface, TType>(S service, IState<TInterface, TType> state) :
         ValueProviderBase<S, T, TInterface, TType>(service, state, false, true), IValueProvider<T>
         where S : RxBLService
         where TType : class, TInterface
    {
        public void Run(T? value)
        {
            RunBase(value);
        }

        protected override IObservable<TType?> ProvideObervableValueBase(T? value, TType? state)
        {
            return Observable.FromAsync(async ct =>
            {
                ArgumentNullException.ThrowIfNull(state);
                await ProvideValueAsync(value, state, ct);
                return (TType?)default;
            });
        }

        protected abstract Task ProvideValueAsync(T? value, TType stateRef, CancellationToken cancellationToken);
    }

    public abstract class ValueRefProvider<S, T, TInterface, TType>(S service, IState<TInterface, TType> state) :
         ValueProviderBase<S, T, TInterface, TType>(service, state, false, false), IValueProvider<T>
         where S : RxBLService
         where TType : class, TInterface
    {
        public void Run(T? value)
        {
            RunBase(value);
        }

        protected override IObservable<TType?> ProvideObervableValueBase(T? value, TType? state)
        {
            return Observable.Return(Unit.Default).Select(_ =>
            {
                ArgumentNullException.ThrowIfNull(state);
                ProvideState(value, state);
                return (TType?)default;
            });
        }

        protected abstract void ProvideState(T? value, TType stateRef);
    }

    public abstract class AsyncStateProvider<S, T>(S service, IState state) :
       ValueProviderBase<S, T?, object?, object?>(service, state, true, true), IStateProvider<T>
       where T : notnull
       where S : RxBLService
    {
        public void Run(T? value)
        {
            RunBase(value);
        }

        protected override IObservable<object?> ProvideObervableValueBase(T? value, object? state)
        {
            return Observable.FromAsync(async cto =>
            {
                await ProvideStateAsync(value, cto);
                return IState.Default;
            });
        }

        protected abstract Task ProvideStateAsync(T? value, CancellationToken cancellationToken);
    }

    public abstract class StateProvider<S, T>(S service, IState state) :
     ValueProviderBase<S, T?, object?, object?>(service, state, true, false), IStateProvider<T>
     where T : notnull
     where S : RxBLService
    {
        public void Run(T? value)
        {
            RunBase(value);
        }

        protected override IObservable<object?> ProvideObervableValueBase(T? value, object? state)
        {
            return Observable.Return(Unit.Default).Select(_ =>
            {
                ProvideState(value);
                return IState.Default;
            });
        }

        protected abstract void ProvideState(T? value);
    }

    public abstract class StateProviderAsync<S>(S service, IState state) :
       ValueProviderBase<S, object?, object?, object?>(service, state, true, true), IStateProvider
       where S : RxBLService
    {
        public void Run()
        {
            RunBase(IState.Default);
        }

        protected override IObservable<object?> ProvideObervableValueBase(object? value, object? state)
        {
            return Observable.FromAsync(async cto =>
            {
                await ProvideStateAsync(cto);
                return IState.Default;
            });
        }

        protected abstract Task ProvideStateAsync(CancellationToken cancellationToken);
    }

    public abstract class StateProvider<S>(S service, IState state) :
      ValueProviderBase<S, object?, object?, object?>(service, state, false, false), IStateProvider
      where S : RxBLService
    {
        public void Run()
        {
            RunBase(IState.Default);
        }

        protected override IObservable<object?> ProvideObervableValueBase(object? value, object? state)
        {
            return Observable.Return(Unit.Default).Select(_ =>
            {
                ProvideState();
                return IState.Default;
            });
        }

        protected abstract void ProvideState();
    }
}