using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

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

    public class RxBLService : IRxBLService
    {
        public bool Initialized { get; private set; }
        public Guid ID { get; }
        public IEnumerable<ServiceException> Exceptions => _serviceExceptions;
        public IStateCommand Command { get; }
        public IStateCommandAsync CommandAsync { get; }
        public IStateCommandAsync CancellableCommandAsync { get; }
        public StatePhase Phase { get; private set; } = StatePhase.CHANGED;
        public bool Disabled => Phase is not StatePhase.CHANGING;
        public bool Independent { get; set; }

        private readonly Subject<ServiceChangeReason> _changedSubject = new();
        private readonly IObservable<ServiceChangeReason> _changedObservable;
        private readonly List<ServiceException> _serviceExceptions;

        public RxBLService()
        {
            Command = this.CreateStateCommand();
            CommandAsync = this.CreateStateCommandAsync();
            CancellableCommandAsync = this.CreateStateCommandAsync(true);
            _changedObservable = _changedSubject.Publish().RefCount();
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

            if (!Independent && !stateInfo.Independent && stateInfo.Changing())
            {
                Phase = StatePhase.CHANGING;
            }
            else
            {
                Phase = StatePhase.CHANGED;
            }
        }

        public IDisposable Subscribe(IObserver<ServiceChangeReason> observer)
        {
            return _changedObservable.Subscribe(observer);
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

        public void OnCompleted()
        {

        }

        public void OnError(Exception error)
        {
            _serviceExceptions.Add(new(ID, error));
            _changedSubject.OnNext(new(ID, ChangeReason.EXCEPTION));
        }

        public void OnNext(Unit value)
        {
            _changedSubject.OnNext(new(ID, ChangeReason.STATE));
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
