using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("RxBlazorLightCoreTests")]

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

        public IDisposable Subscribe(Action<ServiceChangeReason> stateHasChanged, double sampleMS = 100)
        {
            return _changedObservable
                .Sample(TimeSpan.FromMilliseconds(sampleMS))
                .Subscribe(r => stateHasChanged(r));
        }

        internal IDisposable SubscribeMT(Action<ServiceChangeReason> stateHasChanged, double sampleMS = 100)
        {
            return _changedObservable
                .Sample(TimeSpan.FromMilliseconds(sampleMS))
                .ObserveOn(System.Reactive.Concurrency.ThreadPoolScheduler.Instance)
                .Subscribe(r => stateHasChanged(r));
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
    }
}
