using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace RxBlazorLightCore
{
    public partial class RxBLServiceBase : IRXService
    {
        public IEnumerable<Exception> CommandExceptions => _commandExceptions;

        private readonly Subject<Unit> _changedSubject = new();
        private readonly IObservable<Unit> _changedObservable;
        private readonly List<Exception> _commandExceptions;

        public RxBLServiceBase()
        {
            _changedObservable = _changedSubject.Publish().RefCount();
            _commandExceptions = new List<Exception>();
        }

        public void StateHasChanged(Exception? exception = null)
        {
            if (exception is not null)
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

        protected static IInput<T> CreateInput<T>(IRXService service, T value, Func<bool>? canChange = null)
        {
            return new Input<T>(service, value, canChange);
        }
    }
}
