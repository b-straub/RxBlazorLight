using System.Diagnostics.CodeAnalysis;
using R3;

// ReSharper disable once CheckNamespace -> use same namespace for implementation
namespace RxBlazorLightCore
{
    internal class StateBase : IStateInformation
    {
        public StatePhase Phase { get; private set; } = StatePhase.CHANGED;
        public Guid StateID { get; } = Guid.NewGuid();
        public bool Disabled => _owner.OwnsState(this) ? _owner.Disabled : this.Changing();

        private readonly RxBLService _rxBLService;
        private readonly RxBLStateOwner _owner;
        private bool _notifyChanging = true;

        protected StateBase(RxBLService rxBLService, RxBLStateOwner owner)
        {
            _rxBLService = rxBLService;
            _owner = owner;
            _owner.AddOwnedState(this);
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

        internal void PhaseChanged(bool changed, bool notify = true, Exception? exception = null,
            bool submitException = true)
        {
            var changePhase = changed ? StatePhase.CHANGED : StatePhase.CHANGING;

            if (!changed && !notify)
            {
                _notifyChanging = false;
            }

            if (exception is not null)
            {
                if (exception.GetType() == typeof(TaskCanceledException) || exception.GetType() == typeof(OperationCanceledException))
                {
                    changePhase = StatePhase.CANCELED;
                    exception = null;
                }
                else
                {
                    changePhase = StatePhase.EXCEPTION;
                    if (!submitException)
                    {
                        exception = null;
                    }
                }
            }

            Phase = changePhase;

            if (notify || _notifyChanging || exception is not null)
            {
                _rxBLService.StateHasChanged(StateID, exception);
                _notifyChanging = notify;
            }
        }
    }

    internal class State<T> : StateBase, IState<T>
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

        protected State(RxBLService rxBLService, T value, RxBLStateOwner owner) : base(rxBLService, owner)
        {
            _value = value;
        }

        public void SetValueSilent(T value)
        {
            _value = value;
        }

        [MemberNotNullWhen(true, nameof(Value))]
        public bool HasValue()
        {
            return Value is not null;
        }

        public static IState<T> Create(RxBLService rxBLService, T value, RxBLStateOwner owner)
        {
            return new State<T>(rxBLService, value, owner);
        }
    }

    internal class StateCommand : StateBase, IStateCommand
    {
        protected StateCommand(RxBLService rxBLService, RxBLStateOwner owner) : base(rxBLService, owner)
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

        public static IStateCommand Create(RxBLService rxBLService, RxBLStateOwner owner)
        {
            return new StateCommand(rxBLService, owner);
        }
    }

    internal class StateCommandAsync : StateCommand, IStateCommandAsync
    {
        private CancellationTokenSource? _cancellationTokenSource;
        public bool CanCancel { get; }
        public CancellationToken CancellationToken { get; private set; }
        public Guid? ChangeCallerID { get; private set; }

        private StateCommandAsync(RxBLService rxBLService, bool canCancel, RxBLStateOwner owner) : base(rxBLService, owner)
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

        public async Task ExecuteAsync(Func<IStateCommandAsync, Task> changeCallbackAsync,
            bool deferredNotification = false, Guid? changeCallerID = null)
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
                throw new InvalidOperationException(
                    @"Command can not be cancelled. Create with ""CanCancel"" set to true!");
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

        public static IStateCommandAsync Create(RxBLService rxBLService, bool canCancel, RxBLStateOwner owner)
        {
            return new StateCommandAsync(rxBLService, canCancel, owner);
        }
    }

    internal class StateObserverCore(StateProgressObserverAsync stateProgressObserverAsync, bool handleError)
        : Observer<double>
    {
        protected override void OnNextCore(double value)
        {
            stateProgressObserverAsync.SetValueSilent(value);
            stateProgressObserverAsync.PhaseChanged(false);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            stateProgressObserverAsync.Exception = error;
            stateProgressObserverAsync.PhaseChanged(true, true, error, !handleError);
        }

        protected override void OnCompletedCore(Result result)
        {
            stateProgressObserverAsync.Complete();

            if (result.Exception is not null && stateProgressObserverAsync.Exception != result.Exception)
            {
                stateProgressObserverAsync.Exception = result.Exception;
                stateProgressObserverAsync.PhaseChanged(true, true, result.Exception, !handleError);
            }
            else if (stateProgressObserverAsync.Exception is null)
            {
                stateProgressObserverAsync.PhaseChanged(true);
            }
        }
    }

    internal class StateProgressObserverAsync : State<double>, IStateProgressObserverAsync
    {
        public Exception? Exception { get; internal set; }
        public Observer<double> AsObserver => _stateObserverCore;
    
        private StateObserverCore _stateObserverCore;
        private readonly bool _handleError;
        private IDisposable? _disposable;

        private StateProgressObserverAsync(RxBLService rxBLService, bool handleError, RxBLStateOwner owner) : base(rxBLService,
            IStateProgressObserverAsync.InterminateValue, owner)
        {
            _handleError = handleError;
            _stateObserverCore = new(this, _handleError);
        }

        public void ResetException()
        {
            Exception = null;
        }

        public void ExecuteAsync(Func<IStateProgressObserverAsync, IDisposable> executeCallbackAsync)
        {
            Exception = null;
            _stateObserverCore = new(this, _handleError);
            SetValueSilent(IStateProgressObserverAsync.InterminateValue);
            _disposable = executeCallbackAsync(this);
        }

        public void Cancel()
        {
            if (_disposable is null) return;

            _disposable.Dispose();
            Complete();
            PhaseChanged(true, true, new TaskCanceledException());
        }

        internal void Complete()
        {
            _disposable = null;
            SetValueSilent(IStateProgressObserverAsync.MaxValue);
        }

        public static IStateProgressObserverAsync Create(RxBLService rxBLService, bool handleError, RxBLStateOwner owner)
        {
            return new StateProgressObserverAsync(rxBLService, handleError, owner);
        }
    }

    internal class StateGroupBase<T> : StateBase, IStateGroupBase<T>
    {
        public T? Value { get; protected set; }
        public T[] Items { get; }

        protected StateGroupBase(RxBLService rxBLService, T? value, T[] items, RxBLStateOwner owner) :
            base(rxBLService, owner)
        {
            Value = value;
            Items = items;
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

    internal class StateGroup<T> : StateGroupBase<T>, IStateGroup<T>
    {
        private StateGroup(RxBLService rxBLService, T? value, T[] items, RxBLStateOwner owner) :
            base(rxBLService, value, items, owner)
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

        public static IStateGroup<T> Create(RxBLService rxBLService, T[] items, T? value, RxBLStateOwner owner)
        {
            return new StateGroup<T>(rxBLService, value, items, owner);
        }
    }

    internal class StateGroupAsync<T> : StateGroupBase<T>, IStateGroupAsync<T>
    {
        private StateGroupAsync(RxBLService rxBLService, T? value, T[] items, RxBLStateOwner owner) :
            base(rxBLService, value, items, owner)
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

        public static IStateGroupAsync<T> Create(RxBLService rxBLService, T[] items, T? value, RxBLStateOwner owner)
        {
            return new StateGroupAsync<T>(rxBLService, value, items, owner);
        }
    }
}