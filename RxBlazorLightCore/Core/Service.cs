using System;
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
        private List<IObservable<Unit>> _changeObservables;
        private IDisposable? _changeObservablesDisposable;

        public RxBLService()
        {
            _changedObservable = _changedSubject.Publish().RefCount();
            _serviceExceptions = [];
            _changeObservables = [];
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

        public void RegisterChangeObservables(params IObservable<Unit>[] observables)
        {
            _changeObservables.AddRange(observables);
            _changeObservablesDisposable?.Dispose();

            _changeObservablesDisposable = Observable.Merge(_changeObservables)
                .Subscribe(_ => _changedSubject.OnNext(new(ID, ID, ChangeReason.STATE)));
        }

        public void ClearChangeObservables()
        {
            _changeObservablesDisposable?.Dispose();
            _changeObservablesDisposable = null;
            _changeObservables.Clear();
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
