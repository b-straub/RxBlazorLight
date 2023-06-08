
namespace RxBlazorLightCore
{
    public abstract class InputBase<S, T> where S : IRXService
    {
        public T Value { get; internal set; }

        protected S Service { get; }

        protected InputBase(S service, T value)
        {
            Service = service;
            Value = value;
        }

        protected virtual void OnValueChanged(T oldValue, T newValue)
        {
        }

        public void SetValue(T value)
        {
            var oldValue = Value;
            Value = value;
            OnValueChanged(oldValue, Value);
            Service.StateHasChanged();
        }

        public virtual bool CanChange()
        {
            return true;
        }
    }

    public class Input<S, T> : InputBase<S, T>, IInput<T> where S : IRXService
    {
        protected Input(S service, T value) : base(service, value)
        {
        }

        public static IInput<T> Create(S service, T value)
        {
            return new Input<S, T>(service, value);
        }
    }

    public class InputAsync<S, T> : InputBase<S, T>, IInputAsync<T> where S : IRXService
    {
        protected InputAsync(S service, T value) : base(service, value)
        {
        }

        public static IInputAsync<T> Create(S service, T value)
        {
            return new InputAsync<S, T>(service, value);
        }

        protected virtual async Task OnValueChangedAsync(T oldValue, T newValue)
        {
            await Task.CompletedTask;
        }

        public async Task SetValueAsync(T value)
        {
            var oldValue = Value;
            Value = value;
            await OnValueChangedAsync(oldValue, Value);
            Service.StateHasChanged();
        }
    }

    public abstract class InputGroup<S, T> : Input<S, T>, IInputGroup<T> where S : IRXService
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

    public abstract class InputGroup<S, T, P> : InputGroup<S, T>, IInputGroup<T, P> where S : IRXService
    {
        protected P? Parameter { get; private set; }
      
        protected InputGroup(S service, T value) : base(service, value)
        {
        }

        public void SetParameter(P parameter)
        {
            Parameter = parameter;
        }
    }

    public abstract class InputGroupAsync<S, T> : InputAsync<S, T>, IInputGroupAsync<T> where S : IRXService
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

    public abstract class InputGroupAsync<S, T, P> : InputGroupAsync<S, T>, IInputGroupAsync<T, P> where S : IRXService
    {
        protected P? Parameter { get; private set; }

        protected InputGroupAsync(S service, T value) : base(service, value)
        {
        }

        public void SetParameter(P parameter)
        {
            Parameter = parameter;
        }
    }
}