using System.Diagnostics.CodeAnalysis;
using System.Reactive;

namespace RxBlazorLightCore
{
    internal enum ServiceStateChanged
    {
        COMMAND,
        EXCEPTION,
        INPUT,
        SERVICE
    }

    public interface IRxBLScope
    {
        public void EnterScope() { }

        public void LeaveScope() { }
    }


    public interface IRxBLService
    {
        public ValueTask OnContextReadyAsync();
        public bool Initialized { get; }
        public IEnumerable<Exception> Exceptions { get; }
        public void ResetExceptions();
        public IRxBLScope CreateScope();
        public IDisposable Subscribe(Action stateHasChanged, double sampleMS);
    }

    public enum InputState
    {
        NONE,
        CHANGING,
        CHANGED,
        CANCELED,
        EXCEPTION
    }

    public interface IInput<T>
    {
        public InputState State { get; }
        public Exception? LastException { get; }
        public T? Value { get; set; }

        [MemberNotNullWhen(true, nameof(Value))]
        public bool HasValue();
        public bool CanChange();
        public void Cancel();

        public IDisposable Subscribe(Action stateHasChanged);
    }

    public interface IInputGroup<T> : IInput<T>
    {
        public T[] GetItems();
        public bool IsItemDisabled(int index);
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

    public interface ICommandBase
    {
        public CommandState State { get; }
        public Exception? LastException { get; }
        public bool PrepareModal();
        public IDisposable Subscribe(Action stateHasChanged);
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
