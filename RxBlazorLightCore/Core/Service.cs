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

        public void StateHasChanged()
        {
            StateHasChanged(ID);
        }

        internal void StateHasChanged(Guid id, Exception? exception = null)
        {
            if (exception is not null)
            {
                _serviceExceptions.Add(new(id, exception));
                _changedSubject.OnNext(new(id, ChangeReason.EXCEPTION));
            }
            else
            {
                _changedSubject.OnNext(new(id, ChangeReason.STATE));
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
