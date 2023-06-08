
namespace RxBlazorLightCore
{
    public interface IRXService
    {
        public void OnInitialized();
        public Task OnInitializedAsync();
        public IEnumerable<Exception> CommandExceptions { get; }
        public void ResetCommandExceptions();
        public IDisposable Subscribe(Action stateHasChanged, double sampleMS);
        internal void StateHasChanged(Exception? exception = null);
    }

    public interface IInput<T>
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

    public interface ICommand
    {
        public Func<ICommand, bool>? PrepareExecution { get; set; }
        public bool CanExecute();
        public void Execute();
    }

    public interface ICommand<T>
    {
        public T? Parameter { get; set; }
        public Func<ICommand<T>, bool>? PrepareExecution { get; set; }
        public bool CanExecute(T? parameter);
        public void Execute();
        public void Execute(T parameter);
    }

    public interface ICommandAsyncBase
    {
        public bool Executing { get; }
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
        public T? Parameter { get; set; }
        public Func<ICommandAsync<T>, CancellationToken, Task<bool>>? PrepareExecutionAsync { get; set; }
        public Task Execute();
        public bool CanExecute(T? parameter);
        public Task Execute(T parameter);
    }
}
