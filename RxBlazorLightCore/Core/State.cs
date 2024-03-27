using System.Diagnostics.CodeAnalysis;

namespace RxBlazorLightCore
{
    public class StateBase : IStateCommandBase, IStateCommandAsyncBase
    {
        public StatePhase Phase { get; private set; } = StatePhase.CHANGED;
        public Guid ID { get; } = Guid.NewGuid();
        public Guid? ChangeCallerID { get; protected set; }

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

    public class StateCommandBase : StateBase
    {
        protected StateCommandBase(RxBLService service) : base(service) { }

        public void Change()
        {
            PhaseChanged(true);
        }

        public void Change(Action changeDelegate)
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
    }

    public class StateCommand : StateCommandBase, IStateCommand
    {
        protected StateCommand(RxBLService service) : base(service) { }

        public static IStateCommand Create(RxBLService service)
        {
            return new StateCommand(service);
        }
    }

    public class StateCommandAsync : StateCommandBase, IStateCommandAsync
    {
        private CancellationTokenSource _cancellationTokenSource = new();

        protected StateCommandAsync(RxBLService service) : base(service) { }

        private bool _canCancel;


        public async Task ChangeAsync(Func<CancellationToken, Task> changeDelegateAsync, bool notifyChanging, Guid? changeCallerID = null)
        {
            try
            {
                ChangeCallerID = changeCallerID;

                ResetCancellationToken();
                _canCancel = true;
                PhaseChanged(false, notifyChanging);
                await changeDelegateAsync(_cancellationTokenSource.Token);
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

        public async Task ChangeAsync(Func<Task> changeDelegateAsync, bool notifyChanging, Guid? changeCallerID = null)
        {
            try
            {
                ChangeCallerID = changeCallerID;

                ResetCancellationToken();
                PhaseChanged(false, notifyChanging);
                await changeDelegateAsync();
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

        public static IStateCommandAsync Create(RxBLService service)
        {
            return new StateCommandAsync(service);
        }
    }

    public class StateGroup<T> : StateCommand, IStateGroup<T>, IStateCommand
    {
        public T Value { get; set; }
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

        public static IStateGroup<T> Create(RxBLService service, T[] items, T value, Func<int, bool>? itemDisabledDelegate = null)
        {
            return new StateGroup<T>(service, value, items, itemDisabledDelegate);
        }
    }

    public class StateGroupAsync<T> : StateCommandAsync, IStateGroupAsync<T>, IStateCommandAsync
    {
        public T Value { get; set; }
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

        public static IStateGroupAsync<T> Create(RxBLService service, T[] items, T value, Func<int, bool>? itemDisabledDelegate = null)
        {
            return new StateGroupAsync<T>(service, value, items, itemDisabledDelegate);
        }
    }
}