using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace RxBlazorLightCore
{
    public class RxBLScope<T>(T service) : IRxBLScope where T : IRxBLService
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

        public IDisposable Subscribe(IObserver<ServiceChangeReason> observer)
        {
            return Service.Subscribe(observer);
        }
    }

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

        internal void StateHasChanged(Guid stateID, Exception? exception = null)
        {
            if (exception is not null)
            {
                _serviceExceptions.Add(new(exception, stateID));
                _changedSubject.OnNext(new(ID, stateID, ChangeReason.EXCEPTION));
            }
            else
            {
                _changedSubject.OnNext(new(ID, stateID, ChangeReason.STATE));
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
    }
}
