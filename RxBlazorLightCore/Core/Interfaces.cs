
namespace RxBlazorLightCore
{
    public enum ServiceState
    {
        SERVICE,
        COMMAND,
        COMMAND_EXCEPTION,
        INPUT
    }

    public interface IRXService
    {
        public void OnInitialized();
        public Task OnInitializedAsync();
        public IEnumerable<Exception> CommandExceptions { get; }
        public void ResetCommandExceptions();
        public IDisposable Subscribe(Action stateHasChanged, double sampleMS);
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
        PREPARE,
        EXECUTING,
        EXECUTED,
        NOT_EXECUTED,
        CANCELED,
        EXCEPTION
    }

    public interface ICommandBase : IObservable<ServiceState>
    {
        public CommandState State { get; }
        public bool Executing => State == CommandState.EXECUTING;
        public bool Executed => State == CommandState.EXECUTED;
        public bool Canceled => State == CommandState.CANCELED;
        public Exception? LastException { get; }
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
        public bool Running { get; }
        public bool CanCancel();
        public bool HasProgress();
        public bool PrepareModal();
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
