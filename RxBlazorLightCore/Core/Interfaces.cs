
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
        public Action<T> SetValue { get; }
        public bool CanChange();
    }

    public interface ICommandBase
    {
        public bool CanExecute();
        public void Execute();
    }

    public interface ICommand : ICommandBase
    {
        public ICommand Clone();
    }

    public interface ICommand<T> : ICommandBase
    {
        public ICommand<T> Clone();
        public void SetParameter(T? parameter);
    }

    public interface ICommandAsyncBase
    {
        public CancellationToken CancellationToken { get; }
        public bool Executing { get; }
        public bool CanCancel { get; }
        public bool HasProgress { get; }
        public bool CanExecute();
        public Task Execute();
        public void Cancel();
    }

    public interface ICommandAsync : ICommandAsyncBase
    {
        public ICommandAsync Clone();
    }

    public interface ICommandAsync<T> : ICommandAsyncBase
    {
        public ICommandAsync<T> Clone();
        public void SetParameter(T? parameter);

        public void SetParameterAsyncTransformation(Func<T?, Task<T?>>? transformation);
    }
}
