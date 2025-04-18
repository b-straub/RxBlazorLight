using System.Diagnostics.CodeAnalysis;
using R3;

// ReSharper disable once CheckNamespace -> use same namespace for implementation
namespace RxBlazorLightCore
{
    public class StateBase : IStateInformation
    {
        public StatePhase Phase { get; private set; } = StatePhase.CHANGED;
        public Guid ID { get; } = Guid.NewGuid();
        public bool Disabled => Independent ? Phase is StatePhase.CHANGING : _service.Changing();
        public bool Independent { get; set; }

        private readonly RxBLService _service;
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
                if (exception.GetType() == typeof(TaskCanceledException))
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

        public void SetValueSilent(T value)
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

        private StateCommandAsync(RxBLService service, bool canCancel) : base(service)
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

        public static IStateCommandAsync Create(RxBLService service, bool canCancel)
        {
            return new StateCommandAsync(service, canCancel);
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

    public class StateProgressObserverAsync : State<double>, IStateProgressObserverAsync
    {
        public Exception? Exception { get; internal set; }
        public Observer<double> AsObserver => _stateObserverCore;
    
        private StateObserverCore _stateObserverCore;
        private readonly bool _handleError;
        private IDisposable? _disposable;

        private StateProgressObserverAsync(RxBLService service, bool handleError) : base(service,
            IStateProgressObserverAsync.InterminateValue)
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

        public static IStateProgressObserverAsync Create(RxBLService service, bool handleError)
        {
            return new StateProgressObserverAsync(service, handleError);
        }
    }

    public class StateGroupBase<T> : StateBase, IStateGroupBase<T>
    {
        public T? Value { get; protected set; }
        public T[] Items { get; }

        protected StateGroupBase(RxBLService service, T? value, T[] items) :
            base(service)
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

    public class StateGroup<T> : StateGroupBase<T>, IStateGroup<T>
    {
        private StateGroup(RxBLService service, T? value, T[] items) :
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
        private StateGroupAsync(RxBLService service, T? value, T[] items) :
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