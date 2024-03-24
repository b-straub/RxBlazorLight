using System.Diagnostics.CodeAnalysis;

namespace RxBlazorLightCore
{
    public class StateBase<T> : IStateBase<T>
    {
        public T Value { get; set; }
        public StatePhase Phase { get; private set; } = StatePhase.CHANGED;
        public Guid ID { get; } = Guid.NewGuid();
        public Guid? ChangeCallerID { get; protected set; }

        protected readonly RxBLService _service;
        private bool _notifyChanging = true;

        protected StateBase(RxBLService service, T value)
        {
            Value = value;
            _service = service;
        }

        [MemberNotNullWhen(true, nameof(Value))]
        public bool HasValue()
        {
            return Value is not null;
        }

        public void NotifyChanging()
        {
            if (!_notifyChanging)
            {
                _notifyChanging = true;
                if (Phase is StatePhase.CHANGING)
                {
                    PhaseChanged(false);
                }
            }
        }

        protected void PhaseChanged(bool changed, bool notify = true, Exception? exception = null)
        {
            var changePhase = changed ? StatePhase.CHANGED : StatePhase.CHANGING;

            if (!changed && !notify)
            {
                _notifyChanging = false;
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

            Phase = changePhase;

            if (notify || _notifyChanging || exception is not null)
            {
                _service.StateHasChanged(ID, exception);
                _notifyChanging = notify;
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

        public void Change(Action<IState<T>> changeDelegate)
        {
            try
            {
                PhaseChanged(false);
                changeDelegate(this);
                PhaseChanged(true);
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

        private bool _canCancel;

        public bool CanChange(Func<IStateAsync<T>, bool> canChangeDelegate)
        {
            return canChangeDelegate(this);
        }

        public async Task ChangeAsync(Func<IStateAsync<T>, CancellationToken, Task> changeDelegateAsync, bool notifyChanging, Guid? changeCallerID = null)
        {
            try
            {
                ChangeCallerID = changeCallerID;

                ResetCancellationToken();
                _canCancel = true;
                PhaseChanged(false, notifyChanging);
                await changeDelegateAsync(this, _cancellationTokenSource.Token);
                PhaseChanged(true);
            }
            catch (Exception ex)
            {
                PhaseChanged(true, true, ex);
            }
            finally
            {
                _canCancel = false;
                ChangeCallerID = null;
            }
        }

        public async Task ChangeAsync(Func<IStateAsync<T>, Task> changeDelegateAsync, bool notifyChanging, Guid? changeCallerID = null)
        {
            try
            {
                ChangeCallerID = changeCallerID;

                ResetCancellationToken();
                PhaseChanged(false, notifyChanging);
                await changeDelegateAsync(this);
                PhaseChanged(true);
            }
            catch (Exception ex)
            {
                PhaseChanged(true, true, ex);
            }
            finally
            {
                ChangeCallerID = null;
            }
        }

        public void Cancel()
        {
            if (_canCancel)
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