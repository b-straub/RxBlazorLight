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

        public IDisposable Subscribe(Action<ServiceChangeReason> stateHasChanged, double sampleMS = 100)
        {
            return _changedObservable
                .Sample(TimeSpan.FromMilliseconds(sampleMS))
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

        protected static IState<TInterface, TType> CreateState<S, TInterface, TType>(S service, TType? value, Func<IState<TInterface, TType>, IValueProvider<TType>>? valueProviderFactory = null)
            where S : RxBLService
            where TType : class, TInterface
        {
            return State<S, TInterface, TType>.Create(service, value, valueProviderFactory);
        }

        protected static IState<TType> CreateState<S, TType>(S service, TType? value, Func<IState<TType>, IValueProvider<TType>>? valueProviderFactory = null) where S : RxBLService
        {
            return State<S, TType>.Create(service, value, valueProviderFactory);
        }

        protected static IState CreateState<S>(S service)
           where S : RxBLService
        {
            return State<S>.Create(service);
        }
    }
}
