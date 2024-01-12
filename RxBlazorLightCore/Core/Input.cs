﻿
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace RxBlazorLightCore
{
    public class InputBase<T> : IObservable<T>
    {
        public T Value { get; internal set; }

        private readonly Subject<T> _changedSubject;
        private readonly IObservable<T> _changedObservable;

        protected InputBase(T value)
        {
            Value = value;
            _changedSubject = new();
            _changedObservable = _changedSubject.Publish().RefCount();
        }

        protected virtual void OnValueChanging(T oldValue, T newValue)
        {
        }

        public void SetInitialValue(T value)
        {
            SetValueIntern(value, true);
        }

        public void SetValue(T value)
        {
            SetValueIntern(value, false);
        }

        private void SetValueIntern(T value, bool init)
        {
            if (init || CanChange())
            {
                if (!init)
                {
                    OnValueChanging(Value, value);
                }
                Value = value;
                Changed(Value);
            }
        }

        public virtual bool CanChange()
        {
            return true;
        }

        protected void Changed(T value)
        {
            _changedSubject.OnNext(value);
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return _changedObservable
                .StartWith(Value)
                .Subscribe(observer);
        }
    }

    public class InputBase<S, T> : InputBase<T>, IDisposable where S : IRxService
    {
        protected S Service { get; }
        private IDisposable? _serviceDisposable;

        protected InputBase(S service, T value) : base(value)
        {
            Service = service;
            _serviceDisposable = this.Subscribe(_ => Service.StateHasChanged(ServiceState.INPUT, null));
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

    public class Input<S, T> : InputBase<S, T>, IInput<T> where S : IRxService
    {
        protected Input(S service, T value) : base(service, value)
        {
        }

        public static IInput<T> Create(S service, T value)
        {
            return new Input<S, T>(service, value);
        }
    }

    public class InputAsync<T>(T value) : InputBase<T>(value), IInputAsync<T>
    {
        protected virtual async Task OnValueChangingAsync(T oldValue, T newValue)
        {
            await Task.CompletedTask;
        }

        public async Task SetInitialValueAsync(T value)
        {
            await SetValueAsyncIntern(value, true);
        }

        public async Task SetValueAsync(T value)
        {
            await SetValueAsyncIntern(value, false);
        }

        private async Task SetValueAsyncIntern(T value, bool init)
        {
            if (init || CanChange())
            {
                if (!init)
                {
                    await OnValueChangingAsync(Value, value);
                }
                Value = value;
                Changed(Value);
            }
        }
    }

    public class InputAsync<S, T> : InputBase<S, T>, IInputAsync<T> where S : IRxService
    {
        protected InputAsync(S service, T value) : base(service, value)
        {
        }

        public static IInputAsync<T> Create(S service, T value)
        {
            return new InputAsync<S, T>(service, value);
        }

        protected virtual async Task OnValueChangingAsync(T oldValue, T newValue)
        {
            await Task.CompletedTask;
        }

        public async Task SetInitialValueAsync(T value)
        {
            await SetValueAsyncIntern(value, true);
        }

        public async Task SetValueAsync(T value)
        {
            await SetValueAsyncIntern(value, false);
        }

        private async Task SetValueAsyncIntern(T value, bool init)
        {
            if (init || CanChange())
            {
                if (!init)
                {
                    await OnValueChangingAsync(Value, value);
                }
                Value = value;
                Changed(Value);
            }
        }
    }

    public abstract class InputGroup<T> : Input<T>, IInputGroup<T>
    {

        protected InputGroup(T value) : base(value)
        {
        }

        public abstract T[] GetItems();

        public virtual void Initialize()
        {
        }

        public virtual bool IsItemDisabled(int index)
        {
            return false;
        }
    }

    public abstract class InputGroup<S, T> : Input<S, T>, IInputGroup<T> where S : IRxService
    {

        protected InputGroup(S service, T value) : base(service, value)
        {
        }

        public abstract T[] GetItems();

        public virtual void Initialize()
        {
        }

        public virtual bool IsItemDisabled(int index)
        {
            return false;
        }
    }

    public abstract class InputGroupP<T, P> : InputGroup<T>, IInputGroup<T, P>
    {
        protected P? Parameter { get; private set; }

        protected InputGroupP(T value) : base(value)
        {
        }

        public void SetParameter(P parameter)
        {
            Parameter = parameter;
        }
    }

    public abstract class InputGroupP<S, T, P> : InputGroup<S, T>, IInputGroup<T, P> where S : IRxService
    {
        protected P? Parameter { get; private set; }
      
        protected InputGroupP(S service, T value) : base(service, value)
        {
        }

        public void SetParameter(P parameter)
        {
            Parameter = parameter;
        }
    }

    public abstract class InputGroupAsync<T> : InputAsync<T>, IInputGroupAsync<T>
    {

        protected InputGroupAsync(T value) : base(value)
        {
        }

        public abstract T[] GetItems();

        public virtual void Initialize()
        {
        }

        public virtual Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public virtual bool IsItemDisabled(int index)
        {
            return false;
        }
    }

    public abstract class InputGroupAsync<S, T> : InputAsync<S, T>, IInputGroupAsync<T> where S : IRxService
    {

        protected InputGroupAsync(S service, T value) : base(service, value)
        {
        }

        public abstract T[] GetItems();

        public virtual void Initialize()
        {
        }

        public virtual Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public virtual bool IsItemDisabled(int index)
        {
            return false;
        }
    }

    public abstract class InputGroupPAsync<T, P> : InputGroupAsync<T>, IInputGroupAsync<T, P>
    {
        protected P? Parameter { get; private set; }

        protected InputGroupPAsync(T value) : base(value)
        {
        }

        public void SetParameter(P parameter)
        {
            Parameter = parameter;
        }
    }

    public abstract class InputGroupPAsync<S, T, P> : InputGroupAsync<S, T>, IInputGroupAsync<T, P> where S : IRxService
    {
        protected P? Parameter { get; private set; }

        protected InputGroupPAsync(S service, T value) : base(service, value)
        {
        }

        public void SetParameter(P parameter)
        {
            Parameter = parameter;
        }
    }
}