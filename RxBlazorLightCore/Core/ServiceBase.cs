using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace RxBlazorLightCore
{
    public class RxBLServiceBase : IRxService
    {
        public IEnumerable<Exception> Exceptions => _serviceExceptions;

        private readonly Subject<Unit> _changedSubject = new();
        private readonly IObservable<Unit> _changedObservable;
        private readonly List<Exception> _serviceExceptions;
        private bool _contextInitialized;
        public RxBLServiceBase()
        {
            _changedObservable = _changedSubject.Publish().RefCount();
            _serviceExceptions = [];
            _contextInitialized = false;
        }

        public void StateHasChanged(ServiceState reason = ServiceState.SERVICE, Exception? exception = null)
        {
            if (exception is null && reason is ServiceState.EXCEPTION ||
                exception is not null && reason is not ServiceState.EXCEPTION)
            {
                throw new ArgumentException(reason.ToString());
            }

            if (reason is ServiceState.EXCEPTION && exception is not null)
            {
                _serviceExceptions.Add(exception);
            }

            _changedSubject.OnNext(Unit.Default);
        }

        public IDisposable Subscribe(Action stateHasChanged, double sampleMS = 100)
        {
            return _changedObservable
                .Sample(TimeSpan.FromMilliseconds(sampleMS))
                .Subscribe(_ => stateHasChanged());
        }

        public ValueTask OnContextInitialized() 
        {
            if (_contextInitialized)
            {
                throw new InvalidOperationException("Nested RxBLService context not allowed!");
            }
            _contextInitialized = true;
            return InitializeContext();
        }

        protected virtual ValueTask InitializeContext()
        {
            return ValueTask.CompletedTask;
        }

        public void OnContextDisposed() 
        {
            DisposeContext();
            _contextInitialized = false;
        }

        protected virtual void DisposeContext()
        {
        }

        public void ResetExceptions()
        {
            _serviceExceptions.Clear();
        }

        protected static IInput<T> CreateInput<S, T>(S service, T value) where S : IRxService
        {
            return Input<S, T>.Create(service, value);
        }

        protected static IInputAsync<T> CreateInputAsync<S, T>(S service, T value) where S : IRxService
        {
            return InputAsync<S, T>.Create(service, value);
        }
    }
}
