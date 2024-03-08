using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace RxBlazorLightCore
{
    public class RxBLService : IRxBLService
    {
        public bool Initialized { get; private set; }
        public Guid ID { get; }

        public IEnumerable<ServiceException> Exceptions => _serviceExceptions;

        private readonly Subject<ServiceChangeReason> _changedSubject = new();
        private readonly IObservable<ServiceChangeReason> _changedObservable;
        private readonly List<ServiceException> _serviceExceptions;

        public RxBLService()
        {
            _changedObservable = _changedSubject.Publish().RefCount();
            _serviceExceptions = [];
            Initialized = false;
            ID = Guid.NewGuid();
        }

        public virtual IRxBLScope CreateScope()
        {
            throw new NotImplementedException();
        }

        internal void StateHasChanged(Guid id, ChangeReason reason = ChangeReason.SERVICE, Exception? exception = null)
        {
            if (exception is null && reason is ChangeReason.EXCEPTION ||
                exception is not null && reason is not ChangeReason.EXCEPTION)
            {
                throw new ArgumentException(reason.ToString());
            }

            if (reason is ChangeReason.EXCEPTION && exception is not null)
            {
                _serviceExceptions.Add((exception, id));
            }

            _changedSubject.OnNext((reason, id));
        }

        public IDisposable Subscribe(IObserver<(ChangeReason Reason, Guid ID)> observer)
        {
            return _changedObservable.Subscribe(observer);
        }

        public async ValueTask OnContextReadyAsync()
        {
            if (Initialized)
            {
                throw new InvalidOperationException("Nested RxBLService context not allowed!");
            }
            Initialized = true;
            await ContextReadyAsync();
            StateHasChanged(ID);
        }

        protected virtual ValueTask ContextReadyAsync()
        {
            return ValueTask.CompletedTask;
        }

        public void ResetExceptions()
        {
            _serviceExceptions.Clear();
        }

        public IDisposable Subscribe(Action<(ChangeReason Reason, Guid ID)> stateHasChanged, double sampleMS)
        {
            throw new NotImplementedException();
        }
    }

    public class RxBLService<S> : RxBLService, IRxBLService<S> where S: class, new()
    {
        public S State
        {
            get => _state ??= new S();
        }

        public void SetState(Action<S> action)
        {
            try
            {
                action(State);
                StateHasChanged(ID);
            }
            catch (Exception ex)
            {
                StateHasChanged(ID, ChangeReason.EXCEPTION, ex);
            }
        }


        public async Task SetStateAsync(Func<S, Task> actionAsync, Action<StateChangePhase>? phaseCB = null)
        {
            try
            {
                PhaseChanged(false, phaseCB);
                await actionAsync(State);
                PhaseChanged(true, phaseCB);
            }
            catch (Exception ex)
            {
                PhaseChanged(true, phaseCB, ex);
            }
        }

        public async Task SetStateAsync(Func<S, CancellationToken, Task> actionAsync, CancellationToken cancellationToken, Action<StateChangePhase>? phaseCB = null)
        {
            try
            {
                PhaseChanged(false, phaseCB);
                await actionAsync(State, cancellationToken);
                PhaseChanged(true, phaseCB);
            }
            catch (Exception ex)
            {
                PhaseChanged(true, phaseCB, ex);
            }
        }

        private S? _state;

        private void PhaseChanged(bool changed, Action<StateChangePhase>? phaseCB, Exception? exception = null)
        {
            StateChangePhase phase = changed ? StateChangePhase.CHANGED : StateChangePhase.CHANGING;

            if (exception is not null)
            {
                if (exception?.GetType() == typeof(TaskCanceledException))
                {
                    phase = StateChangePhase.CANCELED;
                    exception = null;
                }
                else
                {
                    phase = StateChangePhase.EXCEPTION;
                }
            }

            if (phaseCB is not null)
            {
                phaseCB(phase);
            }

            StateHasChanged(ID, phase is StateChangePhase.EXCEPTION ? ChangeReason.EXCEPTION : ChangeReason.STATE, exception);
        }

        public IRxObservableState CreateObservableState(Action<S> action)
        {
            var obervableState = RxObervableState<RxBLService<S>, S>.Create(this, action);
            return obervableState;
        }

        public IRxObservableStateAsync CreateObservableStateAsync(Func<S, Task> actionAsync)
        {
            var obervableStateAsync = RxObervableStateAsync<RxBLService<S>, S>.Create(this, actionAsync);
            return obervableStateAsync;
        }

        public IRxObservableStateAsync CreateObservableStateAsync(Func<S, CancellationToken, Task> actionAsync)
        {
            var obervableStateAsync = RxObervableStateAsync<RxBLService<S>, S>.Create(this, actionAsync);
            return obervableStateAsync;
        }
    }

    public class RxObervableStateBase<S, T> : IObservable<StateChangePhase> 
        where S : RxBLService<T> 
        where T : class, new()
    {
        public StateChangePhase Phase => ChangedSubject.Value;

        public bool LongRunning { get; private set; } 
        public bool CanCancel { get; private set; }

        protected S Service { get; }

        protected BehaviorSubject<StateChangePhase> ChangedSubject;
        protected CancellationToken CancellationToken => _cancellationTokenSource.Token;

        private readonly IObservable<StateChangePhase> _changedObservable;
        private CancellationTokenSource _cancellationTokenSource;

        protected RxObervableStateBase(S service)
        {
            Service = service;
            ChangedSubject = new(StateChangePhase.NONE);
            _changedObservable = ChangedSubject.Publish().RefCount();
            _cancellationTokenSource = new();
        }

        public IDisposable Subscribe(IObserver<StateChangePhase> observer)
        {
            return _changedObservable.Subscribe(observer);
        }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        protected void ResetCancellationToken()
        {
            if (!_cancellationTokenSource.TryReset())
            {
                _cancellationTokenSource = new();
            }
        }
    }

    public class RxObervableState<S, T> : RxObervableStateBase<S, T>, IRxObservableState 
        where S : RxBLService<T>
        where T : class, new()
    {
        private readonly Action<T> _action;

        protected RxObervableState(S service, Action<T> action) : base(service)
        {
            _action = action;
        }

        public static RxObervableState<S, T> Create(S service, Action<T> action)
        {
            return new(service, action);
        }

        public void Set()
        {
            Service.SetState(_action);
            ChangedSubject.OnNext(StateChangePhase.CHANGED);
        }
    }

    public class RxObervableStateAsync<S, T> : RxObervableStateBase<S, T>, IRxObservableStateAsync
        where S : RxBLService<T>
        where T : class, new()
    {
        private readonly Func<T, CancellationToken, Task> _actionAsync;

        protected RxObervableStateAsync(S service, Func<T, CancellationToken, Task> actionAsync) : base(service)
        {
            _actionAsync = actionAsync;
        }

        public static RxObervableStateAsync<S, T> Create(S service, Func<T, Task> actionAsync)
        {
            return new(service, (s, ct) => actionAsync(s));
        }

        public static RxObervableStateAsync<S, T> Create(S service, Func<T, CancellationToken, Task> actionAsync)
        {
            return new(service, actionAsync);
        }

        public async Task SetAsync()
        {
            ResetCancellationToken();
            await Service.SetStateAsync(_actionAsync, CancellationToken, ChangedSubject.OnNext);
        }
    }
}
