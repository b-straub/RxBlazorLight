using System.Diagnostics.CodeAnalysis;

namespace RxBlazorLightCore
{
    public class StateBase : IStateInformation
    {
        public StatePhase Phase { get; private set; } = StatePhase.CHANGED;
        public Guid ID { get; } = Guid.NewGuid();
        public bool Disabled => Independent ? Phase is StatePhase.CHANGING : _service.Changing();
        public bool Independent { get; set; }

        protected readonly RxBLService _service;
        private bool _notifyChanging = true;

        protected StateBase(RxBLService service)
        {
            _service = service;
            Independent = false;
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
                _service.StateHasChanged(this, exception);
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

        public void Execute(Action changeCallback)
        {
            try
            {
                changeCallback();
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

     
        public async Task ExecuteAsync(Func<IStateCommandAsync, Task> changeCallbackAsync, bool deferredNotification = false, Guid? changeCallerID = null)
        {
            try
            {
                ChangeCallerID = changeCallerID;

                ResetCancellationToken();
                PhaseChanged(false, !deferredNotification);
                await changeCallbackAsync(this);
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

    public class StateGroupBase<T> : StateBase, IStateGroupBase<T>
    {
        public T? Value { get; protected set; }
        public T[] Items => _items;

        private readonly T[] _items;

        protected StateGroupBase(RxBLService service, T? value, T[] items) :
            base(service)
        {
            Value = value;
            _items = items;
        }

        public void Update(T value)
        {
            Value = value; 
        }

        [MemberNotNullWhen(true, nameof(Value))]
        public bool HasValue()
        {
            return Value is not null;
        }
    }

    public class StateGroup<T> : StateGroupBase<T>, IStateGroup<T>
    {
        protected StateGroup(RxBLService service, T? value, T[] items) :
            base(service, value, items)
        {
        }

        public void ChangeValue(T value, Action<T, T>? changingCallback)
        {
            ArgumentNullException.ThrowIfNull(Value);

            try
            {
                if (changingCallback is not null)
                {
                    changingCallback(Value, value);
                }
                Value = value;
                PhaseChanged(true);
            }
            catch (Exception ex)
            {
                PhaseChanged(true, true, ex);
            }
        }

        public static IStateGroup<T> Create(RxBLService service, T[] items, T? value)
        {
            return new StateGroup<T>(service, value, items);
        }
    }

    public class StateGroupAsync<T> : StateGroupBase<T>, IStateGroupAsync<T>
    {
        protected StateGroupAsync(RxBLService service, T? value, T[] items) :
            base(service, value, items)
        {
        }

        public async Task ChangeValueAsync(T value, Func<T, T, Task>? changingCallbackAsync)
        {
            ArgumentNullException.ThrowIfNull(Value);

            try
            {
                PhaseChanged(false);
                if (changingCallbackAsync is not null)
                {
                    await changingCallbackAsync(Value, value);
                }
                Value = value;
                PhaseChanged(true);
            }
            catch (Exception ex)
            {
                PhaseChanged(true, true, ex);
            }
        }

        public static IStateGroupAsync<T> Create(RxBLService service, T[] items, T? value)
        {
            return new StateGroupAsync<T>(service, value, items);
        }
    }
}