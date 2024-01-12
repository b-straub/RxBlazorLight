
namespace RxBlazorLightCore
{
    public enum ServiceState
    {
        SERVICE,
        COMMAND,
        EXCEPTION,
        INPUT
    }

    public interface IRxService
    {
        public ValueTask OnContextInitialized();

        public void OnContextDisposed();

        public IEnumerable<Exception> Exceptions { get; }
        public void ResetExceptions();
        internal IDisposable Subscribe(Action stateHasChanged, double sampleMS);
        internal void StateHasChanged(ServiceState reason, Exception? exception);
    }

    public interface IInput<T> : IObservable<T>
    {
        public T Value { get; }
        public void SetValue(T value);
        public bool CanChange();
    }

    public interface IInputAsync<T> : IInput<T>
    {
        public Task SetValueAsync(T value);
    }

    public interface IInputGroup<T> : IInput<T>
    {
        public T[] GetItems();
        public void Initialize();
        public bool IsItemDisabled(int index);
    }

    public interface IInputGroup<T, P> : IInputGroup<T>
    {
        public void SetParameter(P parameter);
    }

    public interface IInputGroupAsync<T> : IInputGroup<T>, IInputAsync<T>
    {
        public Task InitializeAsync();
    }

    public interface IInputGroupAsync<T, P> : IInputGroupAsync<T>
    {
        public void SetParameter(P parameter);
    }

    public enum CommandState
    {
        NONE,
        PREPARING,
        EXECUTING,
        EXECUTED,
        NOT_EXECUTED,
        CANCELED,
        EXCEPTION
    }

    public interface ICommandBase : IObservable<CommandState>
    {
        public CommandState State { get; }
        public Exception? LastException { get; }
        public bool PrepareModal();
    }

    public interface ICommand : ICommandBase
    {
        public Func<ICommand, bool>? PrepareExecution { get; set; }
        public void Execute();
        public bool CanExecute();
    }

    public interface ICommand<T> : ICommandBase
    {
        public T? Parameter { get; }
        public Func<ICommand<T>, bool>? PrepareExecution { get; set; }
        public void SetParameter(T? parameter);
        public bool CanExecute(T? parameter);
        public void Execute();
        public void Execute(T parameter);
    }

    public interface ICommandAsyncBase : ICommandBase
    {
        public bool CanCancel();
        public bool HasProgress();
        public void Cancel();
    }

    public interface ICommandAsync : ICommandAsyncBase
    {
        public Func<ICommandAsync, CancellationToken, Task<bool>>? PrepareExecutionAsync { get; set; }
        public bool CanExecute();
        public Task Execute();
    }

    public interface ICommandAsync<T> : ICommandAsyncBase
    {
        public T? Parameter { get; }
        public Func<ICommandAsync<T>, CancellationToken, Task<bool>>? PrepareExecutionAsync { get; set; }
        public void SetParameter(T? parameter);
        public Task Execute();
        public bool CanExecute(T? parameter);
        public Task Execute(T parameter);
    }
}
