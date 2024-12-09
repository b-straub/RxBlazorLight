using System.Diagnostics.CodeAnalysis;
using R3;

// ReSharper disable once CheckNamespace -> use same namespace for interface
namespace RxBlazorLightCore
{
    public enum ChangeReason
    {
        EXCEPTION,
        STATE
    }

    public readonly record struct ServiceChangeReason(Guid ID, ChangeReason Reason);
    public readonly record struct ServiceException(Guid ID, Exception Exception);
    public readonly record struct StateValidation(string Message, bool Error);

    public interface IRxBLStateScope : IDisposable
    {
        public ValueTask OnContextReadyAsync();
    }

    public interface IRxBLService : IStateInformation, IDisposable
    {
        public Observable<ServiceChangeReason> AsObservable { get; }
        public Observer<Unit> AsObserver { get; }
        public ValueTask OnContextReadyAsync();
        public bool Initialized { get; }
        public IEnumerable<ServiceException> Exceptions { get; }
        public void ResetExceptions();
        public void StateHasChanged();
        public IStateCommand Command { get; }
        public IStateCommandAsync CommandAsync { get; }
        public IStateCommandAsync CancellableCommandAsync { get; }
    }

    public enum StatePhase
    {
        CHANGING,
        CHANGED,
        CANCELED,
        EXCEPTION
    }

    public interface IStateInformation
    {
        public StatePhase Phase { get; }
        public Guid ID { get; }
        public bool Disabled { get; }
        public bool Independent { get; set; }
    }

    public interface IState<T> : IStateInformation
    {
        public T Value { get; set; }
        public void SetValue(T value);

        [MemberNotNullWhen(true, nameof(Value))]
        public bool HasValue();
    }

    public interface IStateCommand : IStateInformation
    {
        public void Execute(Action executeCallback);
    }
    
    public interface IStateCommandBaseAsync
    {
        public void NotifyChanging();
        public void Cancel();
    }
    
    public interface IStateCommandAsync : IStateCommandBaseAsync, IStateCommand
    {
        public bool CanCancel { get; }
        public Guid? ChangeCallerID { get; }
        public CancellationToken CancellationToken { get; }
        public Task ExecuteAsync(Func<IStateCommandAsync, Task> executeCallbackAsync, bool deferredNotification = false, Guid? changeCallerID = null);
    }
    
    public interface IStateProgressObserverAsync : IStateCommandBaseAsync, IState<double>
    {
        public const double InterminateValue = -1.0;
        public const double MinValue = 0;
        public const double MaxValue = 100.0;
        public Observer<double> AsObserver { get; }
        public Exception? Exception { get; }
        public void ResetException();
        public void ExecuteAsync(Func<IStateProgressObserverAsync, IDisposable> executeCallbackAsync);
    }
    
    public interface IStateGroupBase<T> : IStateInformation
    {
        public T? Value { get; }
        public T[] Items { get; }
        public void Update(T value);

        [MemberNotNullWhen(true, nameof(Value))]
        public bool HasValue();
    }

    public interface IStateGroup<T> : IStateGroupBase<T>
    {
        public void ChangeValue(T value, Action<T, T>? changingCallback = null);
    }

    public interface IStateGroupAsync<T> : IStateGroupBase<T>
    {
        public Task ChangeValueAsync(T value, Func<T, T, Task>? changingCallbackAsync = null);
    }
}
