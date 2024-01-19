using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace RxBlazorLightCore
{
    public abstract class RxBLService : IRxBLService
    {
        public bool Initialized { get; private set; }

        public IEnumerable<Exception> Exceptions => _serviceExceptions;

        private readonly Subject<Unit> _changedSubject = new();
        private readonly IObservable<Unit> _changedObservable;
        private readonly List<Exception> _serviceExceptions;
        public RxBLService()
        {
            _changedObservable = _changedSubject.Publish().RefCount();
            _serviceExceptions = [];
            Initialized = false;
        }

        public abstract IRxBLScope CreateScope();

        internal void StateHasChanged(ServiceStateChanged reason = ServiceStateChanged.SERVICE, Exception? exception = null)
        {
            if (exception is null && reason is ServiceStateChanged.EXCEPTION ||
                exception is not null && reason is not ServiceStateChanged.EXCEPTION)
            {
                throw new ArgumentException(reason.ToString());
            }

            if (reason is ServiceStateChanged.EXCEPTION && exception is not null)
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

        public async ValueTask OnContextReadyAsync()
        {
            if (Initialized)
            {
                throw new InvalidOperationException("Nested RxBLService context not allowed!");
            }
            Initialized = true;
            await ContextReadyAsync();
            StateHasChanged();
        }

        protected virtual ValueTask ContextReadyAsync()
        {
            return ValueTask.CompletedTask;
        }

        public void ResetExceptions()
        {
            _serviceExceptions.Clear();
        }

        protected static IInput<T> CreateInput<S, T>(S service, T? value, Func<bool>? canChange = null) where S : RxBLService
        {
            return Input<S, T>.Create(service, value, canChange);
        }
    }
}
