using System.Reactive;

namespace RxBlazorLightCore
{
    public enum StateChange
    {
        INTERNAL,
        CMD_EXECUTING,
        CMD_EXECUTED,
        CMD_NOT_EXECUTED,
        CMD_CANCELED,
        EXCEPTION,
        INPUT
    }

    public interface IRXService
    {
        public void OnInitialized();
        public Task OnInitializedAsync();
        public IEnumerable<Exception> CommandExceptions { get; }
        public void ResetCommandExceptions();
        public IDisposable Subscribe(Action stateHasChanged, double sampleMS);
        internal void StateHasChanged(StateChange reason, Exception? exception);
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

    public interface ICommand : IObservable<StateChange>
    {
        public Exception? LastException { get; }
        public Func<ICommand, bool>? PrepareExecution { get; set; }
        public bool CanExecute();
        public void Execute();
        public IObservable<Unit> Executed { get; }
    }

    public interface ICommand<T> : IObservable<StateChange>
    {
        public T? Parameter { get; }
        public Func<ICommand<T>, bool>? PrepareExecution { get; set; }
        public void SetParameter(T? parameter);
        public bool CanExecute(T? parameter);
        public void Execute();
        public void Execute(T parameter);
        public IObservable<Unit> Executed { get; }
    }

    public interface ICommandAsyncBase : IObservable<StateChange>
    {
        public Exception? LastException { get; }
        public bool Executing { get; }
        public bool CanCancel();
        public bool HasProgress();
        public void Cancel();
        public IObservable<Unit> Executed { get; }
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
