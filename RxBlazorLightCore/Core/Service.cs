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
}
