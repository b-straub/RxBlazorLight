using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using static RxBlazorLightCore.State;

namespace RxBlazorLightCore
{
    public class State : State<Unit>, IState
    {
        protected State(RxBLService service) :
            base(service, Unit.Default)
        {
        }

        public bool CanChange(Func<IState, bool> canChangeDelegate)
        {
            return canChangeDelegate(this);
        }

        public void Change(Action<IRxBLService> changeDelegate, bool notify = true)
        {
            base.Change(_ => changeDelegate(_service), notify);
        }

        public static IState Create(RxBLService service)
        {
            return new State(service);
        }
    }

    public class StateAsync : StateAsync<Unit>, IStateAsync
    {
        protected StateAsync(RxBLService service) :
            base(service, Unit.Default)
        {
        }

        public bool CanChange(Func<IStateAsync, bool> canChangeDelegate)
        {
            return canChangeDelegate(this);
        }

        public async Task ChangeAsync(Func<IRxBLService, Task> changeDelegateAsync, bool notify = true)
        {
            await base.ChangeAsync(async _ => await changeDelegateAsync(_service), notify);
        }

        public async Task ChangeAsync(Func<IRxBLService, CancellationToken, Task> changeDelegateAsync, bool notify = true)
        {
            await base.ChangeAsync(async (_, ct) => await changeDelegateAsync(_service, ct), notify);
        }

        public static IStateAsync Create(RxBLService service)
        {
            return new StateAsync(service);
        }
    }

    public class StateBase<T> : IStateBase<T>
    {
        public T Value { get; set; }
        public StatePhase Phase => _changedSubject.Value;
        public Guid ID { get; } = Guid.NewGuid();

        protected readonly RxBLService _service;
        private readonly BehaviorSubject<StatePhase> _changedSubject = new(StatePhase.CHANGED);
        private readonly IObservable<StatePhase> _changedObservable;
        private bool _notify = true;

        protected StateBase(RxBLService service, T value)
        {
            Value = value;
            _service = service;
            _changedObservable = _changedSubject.Publish().RefCount();
        }

        [MemberNotNullWhen(true, nameof(Value))]
        public bool HasValue()
        {
            return Value is not null;
        }

        public void NotifyChanging()
        {
            if (!_notify)
            {
                _notify = true;
                if (_changedSubject.Value is StatePhase.CHANGING)
                {
                    PhaseChanged(false, true);
                }
            }
        }

        public IDisposable Subscribe(IObserver<StatePhase> observer)
        {
            return _changedObservable.Subscribe(observer);
        }

        protected void PhaseChanged(bool changed, bool notify, Exception? exception = null)
        {
            var changePhase = changed ? StatePhase.CHANGED : StatePhase.CHANGING;

            if (!changed && !notify)
            {
                _notify = false;
            }

            if (exception is not null)
            {
                if (exception?.GetType() == typeof(TaskCanceledException))
                {
                    changePhase = StatePhase.CANCELED;
                    exception = null;
                }
                else
                {
                    changePhase = StatePhase.EXCEPTION;
                }
            }

            _changedSubject.OnNext(changePhase);

            if (notify || _notify || exception is not null)
            {
                _service.StateHasChanged(ID, changePhase is StatePhase.EXCEPTION ? ChangeReason.EXCEPTION : ChangeReason.STATE, exception);
                _notify = notify;
            }
        }
    }

    public class State<T> : StateBase<T>, IState<T>
    {
        protected State(RxBLService service, T value) : base(service, value) { }

        public bool CanChange(Func<IState<T>, bool> canChangeDelegate)
        {
            return canChangeDelegate(this);
        }

        public void Change(Action<IState<T>> changeDelegate, bool notify = true)
        {
            try
            {
                PhaseChanged(false, notify);
                changeDelegate(this);
                PhaseChanged(true, notify);
            }
            catch (Exception ex)
            {
                PhaseChanged(true, true, ex);
            }
        }

        public static IState<T> Create(RxBLService service, T value)
        {
            return new State<T>(service, value);
        }
    }

    public class StateAsync<T> : StateBase<T>, IStateAsync<T>
    {
        private CancellationTokenSource _cancellationTokenSource = new();

        protected StateAsync(RxBLService service, T value) : base(service, value) { }

        public bool CanCancel { get; private set; }

        public bool CanChange(Func<IStateAsync<T>, bool> canChangeDelegate)
        {
            return canChangeDelegate(this);
        }

        public async Task ChangeAsync(Func<IStateAsync<T>, CancellationToken, Task> changeDelegateAsync, bool notify = true)
        {
            try
            {
                ResetCancellationToken();
                CanCancel = true;
                PhaseChanged(false, notify);
                await changeDelegateAsync(this, _cancellationTokenSource.Token);
                CanCancel = false;
                PhaseChanged(true, notify);
            }
            catch (Exception ex)
            {
                PhaseChanged(true, true, ex);
            }
        }

        public async Task ChangeAsync(Func<IStateAsync<T>, Task> changeDelegateAsync, bool notify = true)
        {
            try
            {
                ResetCancellationToken();
                PhaseChanged(false, notify);
                await changeDelegateAsync(this);
                PhaseChanged(true, notify);
            }
            catch (Exception ex)
            {
                PhaseChanged(true, true, ex);
            }
        }

        public void Cancel()
        {
            if (CanCancel)
            {
                _cancellationTokenSource.Cancel();
            }
        }

        private void ResetCancellationToken()
        {
            if (!_cancellationTokenSource.TryReset())
            {
                _cancellationTokenSource = new();
            }
        }

        public static IStateAsync<T> Create(RxBLService service, T value)
        {
            return new StateAsync<T>(service, value);
        }
    }

    public class StateGroup<T> : State<T>, IStateGroup<T>
    {
        public T[] Items => _items;

        private readonly T[] _items;
        private readonly Func<int, bool>? _itemDisabledDelegate;

        protected StateGroup(RxBLService service, T inititalItem, T[] items, Func<int, bool>? itemDisabledDelegate) :
            base(service, inititalItem)
        {
            _items = items;
            _itemDisabledDelegate = itemDisabledDelegate;
        }

        public bool ItemDisabled(int index)
        {
            return _itemDisabledDelegate is not null && _itemDisabledDelegate(index);
        }

        public static IStateGroup<T> Create(RxBLService service, T[] items, T inititalItem, Func<int, bool>? itemDisabledDelegate = null)
        {
            return new StateGroup<T>(service, inititalItem, items, itemDisabledDelegate);
        }
    }

    public class StateGroupAsync<T> : StateAsync<T>, IStateGroupAsync<T>
    {
        public T[] Items => _items;

        private readonly T[] _items;
        private readonly Func<int, bool>? _itemDisabledDelegate;

        protected StateGroupAsync(RxBLService service, T inititalItem, T[] items, Func<int, bool>? itemDisabledDelegate) :
            base(service, inititalItem)
        {
            _items = items;
            _itemDisabledDelegate = itemDisabledDelegate;
        }

        public bool ItemDisabled(int index)
        {
            return _itemDisabledDelegate is not null && _itemDisabledDelegate(index);
        }

        public static IStateGroupAsync<T> Create(RxBLService service, T[] items, T inititalItem, Func<int, bool>? itemDisabledDelegate = null)
        {
            return new StateGroupAsync<T>(service, inititalItem, items, itemDisabledDelegate);
        }
    }
}