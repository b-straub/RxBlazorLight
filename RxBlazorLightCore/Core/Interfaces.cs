using System.Diagnostics.CodeAnalysis;
using System.Reactive;

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

    public interface IRxBLService : IObservable<ServiceChangeReason>, IObserver<Unit>, IStateInformation, IDisposable
    {
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

    public interface IStateCommandAsync : IStateCommand
    {
        public bool CanCancel { get; }
        public CancellationToken CancellationToken { get; }
        public Guid? ChangeCallerID { get; }
        public void NotifyChanging();
        public Task ExecuteAsync(Func<IStateCommandAsync, Task> executeCallbackAsync, bool deferredNotification = false, Guid? changeCallerID = null);
        public void Cancel();
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
