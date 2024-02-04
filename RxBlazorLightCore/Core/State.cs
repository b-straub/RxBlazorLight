﻿
using System.Reactive;
using System.Reactive.Linq;

namespace RxBlazorLightCore
{

    public class State<S, TInterface, TType> : IState<TInterface, TType>
        where S : RxBLService
        where TType : TInterface
    {
        public TInterface? Value { get; private set; }
        public bool CanRun => _valueProvider.CanRun;
        public bool LongRunning => _valueProvider.LongRunning;
        public bool CanCancel => _valueProvider.CanCancel;
        public StateChangePhase Phase => _valueProvider.Phase;
        public Guid ID => _valueProvider.ID;

        private readonly IStateTransformer<TType> _valueProvider;

        protected internal State(S service, TType? value, Func<IState<TInterface, TType>, IStateTransformer<TType>>? valueProviderFactory = null)
        {
            Value = value;
            _valueProvider = valueProviderFactory is null ?
                new StateTransformDirect<S, TInterface, TType>(service, this) : valueProviderFactory(this);
        }

        protected internal State(S service, TType? value, Func<IState<TType>, IStateTransformer<TType>>? valueProviderFactory = null)
        {
            Value = value;
            _valueProvider = valueProviderFactory is null ?
                new StateTransformDirect<S, TInterface, TType>(service, this) : valueProviderFactory((IState<TType>)this);
        }

        public bool HasValue()
        {
            return Value is not null;
        }

        internal void SetValueIntern(TType? value)
        {
            Value = value;
        }

        public void Transform(TType value)
        {
            _valueProvider.Transform(value);
        }

        public void Cancel()
        {
            _valueProvider.Cancel();
        }

        public static IState<TInterface, TType> Create(S service, TType? value, Func<IState<TInterface, TType>, IStateTransformer<TType>>? valueProviderFactory = null)
        {
            return new State<S, TInterface, TType>(service, value, valueProviderFactory);
        }
    }

    public class State<S, TType> : State<S, TType, TType>, IState<TType>
        where S : RxBLService
    {
        private State(S service, TType? value, Func<IState<TType>, IStateTransformer<TType>>? valueProviderFactory = null) :
            base(service, value, valueProviderFactory)
        {

        }

        public static IState<TType> Create(S service, TType? value, Func<IState<TType>, IStateTransformer<TType>>? valueProviderFactory = null)
        {
            return new State<S, TType>(service, value, valueProviderFactory);
        }
    }

    public class State<S> : State<S, object?, object?>, IState
        where S : RxBLService
    {
        private State(S service) : base (service, IState.Default, null) { }

        public static IState Create(S service)
        {
            return new State<S>(service);
        }
    }

    public class StateGroup<S, TInterface, TType> : State<S, TInterface, TType>, IStateGroup<TInterface, TType>
        where S : RxBLService
        where TType : TInterface
    {
        protected S Service { get; }
        protected StateGroup(S service, TType? value, Func<IState<TInterface, TType>, IStateTransformer<TType>>? valueProviderFactory = null) :
            base(service, value, valueProviderFactory)
        {
            Service = service;
        }

        public virtual TInterface[] GetItems()
        {
            return [];
        }

        public virtual bool IsItemDisabled(int index)
        {
            return false;
        }

        public static IStateGroup<TInterface, TType> CreateSG(S service, TType? value, Func<IState<TInterface, TType>, IStateTransformer<TType>>? valueProviderFactory = null)
        {
            return new StateGroup<S, TInterface, TType>(service, value, valueProviderFactory);
        }
    }

    public class StateGroup<S, TType> : State<S, TType, TType>, IStateGroup<TType>
        where S : RxBLService
    {
        protected S Service { get; }
        protected StateGroup(S service, TType? value, Func<IState<TType>, IStateTransformer<TType>>? valueProviderFactory = null) :
            base(service, value, valueProviderFactory)
        {
            Service = service;
        }

        public virtual TType[] GetItems()
        {
            return [];
        }

        public virtual bool IsItemDisabled(int index)
        {
            return false;
        }

        public static IStateGroup<TType> CreateSG(S service, TType? value, Func<IState<TType>, IStateTransformer<TType>>? valueProviderFactory = null)
        {
            return new StateGroup<S, TType>(service, value, valueProviderFactory);
        }
    }

    public class StateProvideTransformBase<S, T, TInterface, TType> : IStateProvideTransformBase
        where S : RxBLService
        where TType : TInterface
    {
        public virtual bool CanRun => true;
        public virtual bool CanCancel => false;
        public virtual bool LongRunning => false;
        public StateChangePhase Phase { get; protected set; } = StateChangePhase.NONE;
        public Guid ID { get; }

        protected S Service { get; }

        protected State<S, TInterface, TType> State { get; }

        private readonly bool _setState;
        private readonly bool _runAsync;
        private bool _canceled;
        private IDisposable? _runDisposable;

        protected internal StateProvideTransformBase(S service, IState<TInterface, TType>? state, bool setState, bool runAsync)
        {
            Service = service;
            State = state is null ? (State<S, TInterface, TType>)State<S, object?, object?>.Create(service, IState.Default) : (State<S, TInterface, TType>)state;
            _setState = setState;
            _runAsync = runAsync;
            _canceled = false;

            ID = Guid.NewGuid();
        }

        public void Cancel()
        {
            if (!CanCancel)
            {
                StateChanged(StateChangePhase.EXCEPTION, new InvalidOperationException("CanCancel() returning false!"));
                return;
            }

            if (!_runAsync)
            {
                StateChanged(StateChangePhase.EXCEPTION, new InvalidOperationException("Cancel() not allowed for Sync Provider!"));
                return;
            }

            _canceled = true;
            _runDisposable?.Dispose();
        }

        protected virtual IObservable<TType?> ProvideObervableValueBase(T? value, TType? state)
        {
            throw new NotImplementedException();
        }

        protected void TransformBaseSync(TType? value)
        {
            State.SetValueIntern(value);
            StateChanged(StateChangePhase.CHANGED);
        }

        protected void TransformBaseAsync(T? value)
        {
            StateChanged(StateChangePhase.CHANGING);

            _runDisposable = ProvideObervableValueBase(value, (TType?)State.Value)
                .Finally(() =>
                {
                    if (_canceled)
                    {
                        _canceled = false;
                        StateChanged(StateChangePhase.CANCELED);
                    }
                })
                .Subscribe(v =>
                {
                    if (_setState)
                    {
                        State.SetValueIntern(v);
                    }

                    StateChanged(StateChangePhase.CHANGED);
                },
                ex =>
                {
                    if (ex.GetType() == typeof(TaskCanceledException))
                    {
                        StateChanged(StateChangePhase.CANCELED);
                    }
                    else
                    {
                        StateChanged(StateChangePhase.EXCEPTION, ex);
                    }
                });
        }

        protected void StateChanged(StateChangePhase phase, Exception? exception = null)
        {
            Phase = phase;
            Service.StateHasChanged(ID, Phase is StateChangePhase.EXCEPTION ? ChangeReason.EXCEPTION : ChangeReason.STATE, exception);
        }

    }

    public class StateTransformDirect<S, TInterface, TType>(S service, IState<TInterface, TType> state) :
        StateProvideTransformBase<S, TType, TInterface, TType>(service, state, true, true), IStateTransformer<TType>
        where S : RxBLService
        where TType : TInterface
    {
        public void Transform(TType value)
        {
            TransformBaseSync(value);
        }
    }

    public class StateTransformDirect<S, TType>(S service, IState<TType> state) :
       StateProvideTransformBase<S, TType, TType, TType>(service, state, true, true), IStateTransformer<TType>
       where S : RxBLService
    {
        public void Transform(TType value)
        {
            TransformBaseSync(value);
        }
    }

    public abstract class StateTransformAsync<S, T, TType>(S service, IState<TType> state) :
       StateProvideTransformBase<S, T, TType, TType>(service, state, true, true), IStateTransformer<T>
       where S : RxBLService
    {
        public void Transform(T value)
        {
            TransformBaseAsync(value);
        }

        protected override IObservable<TType?> ProvideObervableValueBase(T? value, TType? state)
        {
            ArgumentNullException.ThrowIfNull(value);

            return Observable.FromAsync(async ct =>
            {
                return await TransformStateAsync(value, ct);
            });
        }

        protected abstract Task<TType> TransformStateAsync(T value, CancellationToken cancellationToken);
    }

    public abstract class StateTransform<S, T, TType>(S service, IState<TType> state) :
     StateProvideTransformBase<S, T, TType, TType>(service, state, true, false), IStateTransformer<T>
     where S : RxBLService
    {
        public void Transform(T value)
        {
            try
            {
                TransformBaseSync(TransformState(value));
            }
            catch (Exception ex)
            {
                StateChanged(StateChangePhase.EXCEPTION, ex);
            }
        }

        protected abstract TType? TransformState(T value);
    }

    public abstract class ValueProviderAsync<S, T>(S service, IState<T> state) :
       StateProvideTransformBase<S, T, T, T>(service, state, true, true), IStateProvider<T>
       where S : RxBLService
    {
        public void Provide()
        {
            TransformBaseAsync(default);
        }

        protected override IObservable<T?> ProvideObervableValueBase(T? value, T? state)
        {
            return Observable.FromAsync(ProvideValueAsync);
        }

        protected abstract Task<T?> ProvideValueAsync(CancellationToken cancellationToken);
    }

    public abstract class ValueProvider<S, T>(S service, IState<T> state) :
      StateProvideTransformBase<S, T, T, T>(service, state, true, false), IStateProvider<T>
      where S : RxBLService
    {
        public void Provide()
        {
            try
            {
                TransformBaseSync(ProvideValue());
            }
            catch (Exception ex)
            {
                StateChanged(StateChangePhase.EXCEPTION, ex);
            }
        }

        protected abstract T ProvideValue();
    }

    public abstract class StateRefTransformAsync<S, T, TInterface, TType>(S service, IState<TInterface, TType> state) :
         StateProvideTransformBase<S, T, TInterface, TType>(service, state, false, true), IStateTransformer<T>
         where S : RxBLService
         where TType : class, TInterface
    {
        public void Transform(T value)
        {
            TransformBaseAsync(value);
        }

        protected override IObservable<TType?> ProvideObervableValueBase(T? value, TType? state)
        {
            ArgumentNullException.ThrowIfNull(state);
            ArgumentNullException.ThrowIfNull(value);

            return Observable.FromAsync(async ct =>
            {
                await TransformStateAsync(value, state, ct);
                return (TType?)default;
            });
        }

        protected abstract Task TransformStateAsync(T value, TType stateRef, CancellationToken cancellationToken);
    }

    public abstract class StateRefTransform<S, T, TInterface, TType>(S service, IState<TInterface, TType> state) :
         StateProvideTransformBase<S, T, TInterface, TType>(service, state, false, false), IStateTransformer<T>
         where S : RxBLService
         where TType : class, TInterface
    {
        public void Transform(T value)
        {
            ArgumentNullException.ThrowIfNull(State.Value);
            try
            {
                TransformState(value, (TType)State.Value);
                TransformBaseSync(default);
            }
            catch (Exception ex)
            {
                StateChanged(StateChangePhase.EXCEPTION, ex);
            }
        }

        protected abstract void TransformState(T value, TType stateRef);
    }

    public abstract class ServiceProviderAsync<S, T>(S service) :
       StateProvideTransformBase<S, T?, object?, object?>(service, null, true, true), IServiceStateProvider<T>
       where T : notnull
       where S : RxBLService
    {
        public void Transform(T value)
        {
            TransformBaseAsync(value);
        }

        protected override IObservable<object?> ProvideObervableValueBase(T? value, object? state)
        {
            ArgumentNullException.ThrowIfNull(value);

            return Observable.FromAsync(async cto =>
            {
                await TransformStateAsync(value, cto);
                return IState.Default;
            });
        }

        protected abstract Task TransformStateAsync(T value, CancellationToken cancellationToken);
    }

    public abstract class ServiceProvider<S, T>(S service) :
     StateProvideTransformBase<S, T?, object?, object?>(service, null, true, false), IServiceStateProvider<T>
     where T : notnull
     where S : RxBLService
    {
        public void Transform(T value)
        {
            try
            {
                TransformState(value);
                TransformBaseSync(IState.Default);
            }
            catch (Exception ex)
            {
                StateChanged(StateChangePhase.EXCEPTION, ex);
            }
        }

        protected abstract void TransformState(T value);
    }

    public abstract class ServiceProviderAsync<S>(S service) :
       StateProvideTransformBase<S, object?, object?, object?>(service, null, true, true), IServiceStateProvider
       where S : RxBLService
    {
        public void Provide()
        {
            TransformBaseAsync(IState.Default);
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

    public abstract class ServiceProvider<S>(S service) :
      StateProvideTransformBase<S, object?, object?, object?>(service, null, false, false), IServiceStateProvider
      where S : RxBLService
    {
        public void Provide()
        {
            try
            {
                ProvideState();
                TransformBaseSync(IState.Default);
            }
            catch (Exception ex)
            {
                StateChanged(StateChangePhase.EXCEPTION, ex);
            }
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