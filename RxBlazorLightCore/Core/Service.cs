using R3;

// ReSharper disable once CheckNamespace -> use same namespace for implementation
namespace RxBlazorLightCore
{
    public class RxBLStateScope<T>(T service) : IRxBLStateScope where T : IRxBLService
    {
        protected T Service => service;

        public virtual ValueTask OnContextReadyAsync()
        {
            return ValueTask.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposing)
        {
        }
    }

    public class RxBLService : Observer<Unit>, IRxBLService
    {
        public Observable<ServiceChangeReason> AsObservable { get; }
        public Observer<Unit> AsObserver => this;
        public bool Initialized { get; private set; }
        public Guid ID { get; }
        public IEnumerable<ServiceException> Exceptions => _serviceExceptions;
        public IStateCommand Command { get; }
        public IStateCommandAsync CommandAsync { get; }
        public IStateCommandAsync CancellableCommandAsync { get; }
        public StatePhase Phase { get; private set; } = StatePhase.CHANGED;
        public bool Disabled => Phase is not StatePhase.CHANGING;
    
        private readonly Subject<ServiceChangeReason> _changedSubject = new();
        private readonly HashSet<ServiceException> _serviceExceptions;

        protected RxBLService()
        {
            Command = this.CreateStateCommand();
            CommandAsync = this.CreateStateCommandAsync();
            CancellableCommandAsync = this.CreateStateCommandAsync(true);
            AsObservable = _changedSubject.Publish().RefCount();
            _serviceExceptions = [];
            Initialized = false;
            ID = Guid.NewGuid();
        }

        public void StateHasChanged()
        {
            StateHasChanged(this);
        }

        internal void StateHasChanged(IStateInformation stateInfo, Exception? exception = null)
        {
            if (exception is not null)
            {
                _serviceExceptions.Add(new(stateInfo.ID, exception));
                _changedSubject.OnNext(new(stateInfo.ID, ChangeReason.EXCEPTION));
            }
            else
            {
                _changedSubject.OnNext(new(stateInfo.ID, ChangeReason.STATE));
            }

            Phase = stateInfo.Changing() ? StatePhase.CHANGING : StatePhase.CHANGED;
        }
     
        public async ValueTask OnContextReadyAsync()
        {
            if (Initialized)
            {
                throw new InvalidOperationException("Nested RxBLService context not allowed!");
            }
            await ContextReadyAsync();
            Initialized = true;
            StateHasChanged(this);
        }

        protected virtual ValueTask ContextReadyAsync()
        {
            return ValueTask.CompletedTask;
        }

        public void ResetExceptions()
        {
            _serviceExceptions.Clear();
        }
        
        protected override void OnErrorResumeCore(Exception error)
        {
            if (_serviceExceptions.Add(new(ID, error)))
            {
                _changedSubject.OnNext(new(ID, ChangeReason.EXCEPTION));
            }
        }

        protected override void OnCompletedCore(Result result)
        {
            if (result.Exception is not null && _serviceExceptions.Add(new(ID, result.Exception)))
            {
                _changedSubject.OnNext(new(ID, ChangeReason.EXCEPTION));
            }
        }

        protected override void OnNextCore(Unit value)
        {
            _changedSubject.OnNext(new(ID, ChangeReason.STATE));
        }
    }
}
