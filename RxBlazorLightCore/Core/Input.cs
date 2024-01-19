using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace RxBlazorLightCore
{
    public class InputBase<T> : IObservable<T>
    {
        public T Value
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

        private readonly Subject<T> _changedSubject;
        private readonly IObservable<T> _changedObservable;
        private T _value;

        protected InputBase(T value)
        {
            _value = value;
            _changedSubject = new();
            _changedObservable = _changedSubject.Publish().RefCount();

            _changedObservable
                .Select(async newValue =>
                {
                    OnValueChanged(_value, newValue);
                    await OnValueChangedAsync(_value, newValue);
                    _value = newValue;
                    return _value;
                })
                .Subscribe();
        }

        protected virtual void OnValueChanged(T oldValue, T newValue) { }
        protected virtual ValueTask OnValueChangedAsync(T oldValue, T newValue)
        {
            return ValueTask.CompletedTask;
        }

        private void SetValue(T value)
        {
            if (CanChange())
            {
                _changedSubject.OnNext(value);
            }
        }

        public virtual bool CanChange()
        {
            return true;
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return _changedObservable
                .StartWith(_value)
                .Subscribe(observer);
        }
    }

    public class InputBase<S, T> : InputBase<T>, IDisposable where S : RxBLService
    {
        protected S Service { get; }
        private IDisposable? _serviceDisposable;

        protected InputBase(S service, T value) : base(value)
        {
            Service = service;
            _serviceDisposable = this.Subscribe(_ => Service.StateHasChanged(ServiceStateChanged.INPUT, null));
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

    public class Input<T>(T value) : InputBase<T>(value), IInput<T>
    {
    }

    public class Input<S, T> : InputBase<S, T>, IInput<T> where S : RxBLService
    {
        protected Input(S service, T value) : base(service, value)
        {
        }

        public static IInput<T> Create(S service, T value)
        {
            return new Input<S, T>(service, value);
        }
    }

    public abstract class InputGroup<T> : Input<T>, IInputGroup<T>
    {

        protected InputGroup(T value) : base(value)
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

        protected InputGroup(S service, T value) : base(service, value)
        {
        }

        public abstract T[] GetItems();

        public virtual bool IsItemDisabled(int index)
        {
            return false;
        }
    }
}