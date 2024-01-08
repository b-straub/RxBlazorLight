using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace RxBlazorLightCore
{
    public class RxBLServiceBase : IRXService
    {
        public IEnumerable<Exception> CommandExceptions => _commandExceptions;

        private readonly Subject<Unit> _changedSubject = new();
        private readonly IObservable<Unit> _changedObservable;
        private readonly List<Exception> _commandExceptions;

        public RxBLServiceBase()
        {
            _changedObservable = _changedSubject.Publish().RefCount();
            _commandExceptions = [];
        }

        public void StateHasChanged(ServiceState reason = ServiceState.SERVICE, Exception? exception = null)
        {
            if (exception is null && reason is ServiceState.COMMAND_EXCEPTION ||
                exception is not null && reason is not ServiceState.COMMAND_EXCEPTION)
            {
                throw new ArgumentException(reason.ToString());
            }

            if (reason is ServiceState.COMMAND_EXCEPTION && exception is not null)
            {
                _commandExceptions.Add(exception);
            }

            _changedSubject.OnNext(Unit.Default);
        }

        public IDisposable Subscribe(Action stateHasChanged, double sampleMS = 100)
        {
            return _changedObservable
                .Sample(TimeSpan.FromMilliseconds(sampleMS))
                .Subscribe(_ => stateHasChanged());
        }

        public virtual void OnInitialized() { }

        public virtual Task OnInitializedAsync() { return Task.CompletedTask; }

        public void ResetCommandExceptions()
        {
            _commandExceptions.Clear();
        }

        protected static IInput<T> CreateInput<S, T>(S service, T value) where S : IRXService
        {
            return Input<S, T>.Create(service, value);
        }

        protected static IInputAsync<T> CreateInputAsync<S, T>(S service, T value) where S : IRXService
        {
            return InputAsync<S, T>.Create(service, value);
        }
    }
}
