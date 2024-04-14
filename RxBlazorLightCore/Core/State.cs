using System.Diagnostics.CodeAnalysis;

namespace RxBlazorLightCore
{
    public class StateBase
    {
        public StatePhase Phase { get; private set; } = StatePhase.CHANGED;
        public Guid ID { get; } = Guid.NewGuid();

        protected readonly RxBLService _service;
        private bool _notifyChanging = true;

        protected StateBase(RxBLService service)
        {
            _service = service;
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

    public class State<T> : StateBase, IState<T>
    {
        public T Value
        {
            get => _value;
            set
            {
                _value = value;
                PhaseChanged(true);
            }
        }

        private T _value;

        protected State(RxBLService service, T value) : base(service)
        {
            _value = value;
        }

        public void Update(T value)
        {
            _value = value;
        }

        [MemberNotNullWhen(true, nameof(Value))]
        public bool HasValue()
        {
            return Value is not null;
        }

        public static IState<T> Create(RxBLService service, T value)
        {
            return new State<T>(service, value);
        }
    }

    public class StateCommand : StateBase, IStateCommand
    {
        protected StateCommand(RxBLService service) : base(service)
        {
        }

        public void Execute(Action changeDelegate)
        {
            try
            {
                changeDelegate();
                PhaseChanged(true);
            }
            catch (Exception ex)
            {
                PhaseChanged(true, true, ex);
            }
        }

        public static IStateCommand Create(RxBLService service)
        {
            return new StateCommand(service);
        }
    }

    public class StateCommandAsync : StateCommand, IStateCommandAsync
    {
        private CancellationTokenSource? _cancellationTokenSource;
        public bool CanCancel { get; }
        public CancellationToken CancellationToken { get; private set; }
        public Guid? ChangeCallerID { get; private set; }

        protected StateCommandAsync(RxBLService service, bool canCancel) : base(service) 
        {
            CanCancel = canCancel;
            if (CanCancel)
            {
                _cancellationTokenSource = new();
                CancellationToken = _cancellationTokenSource.Token;
            }
            else
            {
                CancellationToken = CancellationToken.None;
            }
        }

     
        public async Task ExecuteAsync(Func<IStateCommandAsync, Task> changeDelegateAsync, bool deferredNotification = false, Guid? changeCallerID = null)
        {
            try
            {
                ChangeCallerID = changeCallerID;

                ResetCancellationToken();
                PhaseChanged(false, !deferredNotification);
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
            if (_cancellationTokenSource is null)
            {
                throw new InvalidOperationException(@"Command can not be cancelled. Create with ""CanCancel"" set to true!");
            }

            _cancellationTokenSource.Cancel();
        }

        private void ResetCancellationToken()
        {
            if (_cancellationTokenSource is not null && !_cancellationTokenSource.TryReset())
            {
                _cancellationTokenSource = new();
                CancellationToken = _cancellationTokenSource.Token;
            }
        }

        public static IStateCommandAsync Create(RxBLService service, bool canCancel)
        {
            return new StateCommandAsync(service, canCancel);
        }
    }

    public class StateGroup<T> : StateBase, IStateGroup<T>
    {
        public T Value { get; private set; }
        public T[] Items => _items;

        private readonly T[] _items;
        private readonly Func<int, bool>? _itemDisabledDelegate;

        protected StateGroup(RxBLService service, T value, T[] items, Func<int, bool>? itemDisabledDelegate) :
            base(service)
        {
            Value = value;
            _items = items;

            _itemDisabledDelegate = itemDisabledDelegate;
        }

        public bool ItemDisabled(int index)
        {
            return _itemDisabledDelegate is not null && _itemDisabledDelegate(index);
        }

        public void ChangeValue(T value, Action<T, T>? changingDelegate)
        {
            try
            {
                if (changingDelegate is not null)
                {
                    changingDelegate(Value, value);
                }
                Value = value;
                PhaseChanged(true);
            }
            catch (Exception ex)
            {
                PhaseChanged(true, true, ex);
            }
        }

        public static IStateGroup<T> Create(RxBLService service, T[] items, T value, Func<int, bool>? itemDisabledDelegate = null)
        {
            return new StateGroup<T>(service, value, items, itemDisabledDelegate);
        }
    }

    public class StateGroupAsync<T> : StateBase, IStateGroupAsync<T>
    {
        public T Value { get; private set; }
        public T[] Items => _items;

        private readonly T[] _items;
        private readonly Func<int, bool>? _itemDisabledDelegate;

        protected StateGroupAsync(RxBLService service, T value, T[] items, Func<int, bool>? itemDisabledDelegate) :
            base(service)
        {
            Value = value;
            _items = items;

            _itemDisabledDelegate = itemDisabledDelegate;
        }

        public bool ItemDisabled(int index)
        {
            return _itemDisabledDelegate is not null && _itemDisabledDelegate(index);
        }

        public async Task ChangeValueAsync(T value, Func<T, T, Task>? changingDelegateAsync)
        {
            try
            {
                PhaseChanged(false);
                if (changingDelegateAsync is not null)
                {
                    await changingDelegateAsync(Value, value);
                }
                Value = value;
                PhaseChanged(true);
            }
            catch (Exception ex)
            {
                PhaseChanged(true, true, ex);
            }
        }

        public static IStateGroupAsync<T> Create(RxBLService service, T[] items, T value, Func<int, bool>? itemDisabledDelegate = null)
        {
            return new StateGroupAsync<T>(service, value, items, itemDisabledDelegate);
        }
    }
}