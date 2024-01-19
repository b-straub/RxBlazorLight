using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace RxBlazorLightCore
{
    public class InputBase<T> : IInput<T>
    {
        public T? Value
        {
            get
            {
                return _value;
            }
            set
            {
                SetValue(value);
            }
        }

        public InputState State { get; private set; } = InputState.NONE;

        public Exception? LastException { get; private set; }

        private readonly Subject<T?> _changedSubject;
        private readonly IObservable<T?> _changedObservable;
        private T? _value;
        private CancellationTokenSource _cancellationTokenSource;

        protected InputBase(T? value)
        {
            _value = value;
            _changedSubject = new();
            _changedObservable = _changedSubject.Publish().RefCount();
            _cancellationTokenSource = new();
        }

        public bool HasValue()
        {
            return Value is not null;
        }

        public virtual bool CanChange()
        {
            return true;
        }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        protected void ResetCancel()
        {
            if (!_cancellationTokenSource.TryReset())
            {
                _cancellationTokenSource = new();
            }
        }

        protected virtual void OnValueChanged(T? oldValue, T newValue) { }
        protected virtual ValueTask OnValueChangedAsync(T? oldValue, T newValue, CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
        }

        public IDisposable Subscribe(Action stateHasChanged)
        {
            return _changedObservable
               .StartWith(_value)
               .Select(async newValue =>
               {
                   if (newValue is not null && !newValue.Equals(_value))
                   {
                       LastException = null;
                       ResetCancel();
                       State = InputState.CHANGING;
                       stateHasChanged();
                       var _canceledOrError = false;

                       try
                       {
                           OnValueChanged(_value, newValue);
                           await OnValueChangedAsync(_value, newValue, _cancellationTokenSource.Token);
                       }
                       catch (Exception ex)
                       {
                           if (ex.GetType() == typeof(TaskCanceledException))
                           {
                               State = InputState.CANCELED;
                           }
                           else
                           {
                               LastException = ex;
                               State = InputState.EXCEPTION;
                           }

                           _canceledOrError = true;
                       }

                       if (!_canceledOrError)
                       {
                           _value = newValue;
                           State = InputState.CHANGED;
                       }
                       stateHasChanged();
                   }
               })
               .Subscribe();
        }

        private void SetValue(T? value)
        {
            if (CanChange())
            {
                _changedSubject.OnNext(value);
            }
        }
    }

    public class InputBase<S, T> : InputBase<T>, IDisposable where S : RxBLService
    {
        protected S Service { get; }
        private IDisposable? _serviceDisposable;

        protected InputBase(S service, T? value) : base(value)
        {
            Service = service;
            _serviceDisposable = Subscribe(() => Service.StateHasChanged(State is InputState.EXCEPTION ? ServiceStateChanged.EXCEPTION : ServiceStateChanged.INPUT, LastException));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _serviceDisposable?.Dispose();
                _serviceDisposable = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public class Input<T>(T? value) : InputBase<T>(value)
    {
    }

    public class Input<S, T> : InputBase<S, T> where S : RxBLService
    {
        private readonly Func<bool>? _canChange;

        protected Input(S service, T? value, Func<bool>? canChange = null) : base(service, value)
        {
            _canChange = canChange;
        }

        public override bool CanChange()
        {
            return _canChange is null ? base.CanChange() : _canChange();
        }

        public static IInput<T> Create(S service, T? value, Func<bool>? canChange = null)
        {
            return new Input<S, T>(service, value, canChange);
        }
    }

    public abstract class InputGroup<T> : Input<T>, IInputGroup<T>
    {

        protected InputGroup(T? value) : base(value)
        {
        }

        public abstract T[] GetItems();

        public virtual bool IsItemDisabled(int index)
        {
            return false;
        }
    }

    public abstract class InputGroup<S, T> : Input<S, T>, IInputGroup<T> where S : RxBLService
    {

        protected InputGroup(S service, T? value) : base(service, value)
        {
        }

        public abstract T[] GetItems();

        public virtual bool IsItemDisabled(int index)
        {
            return false;
        }
    }
}