using System.Diagnostics.CodeAnalysis;

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

    public interface IRxBLService : IObservable<ServiceChangeReason>
    {
        public ValueTask OnContextReadyAsync();
        public bool Initialized { get; }
        public IEnumerable<ServiceException> Exceptions { get; }
        public void ResetExceptions();
        public Guid ID { get; }
        public void StateHasChanged();
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
    }

    public interface IState<T> : IStateInformation
    {
        public T Value { get; set; }
        public void Update(T value);

        [MemberNotNullWhen(true, nameof(Value))]
        public bool HasValue();
    }

    public interface IStateCommand : IStateInformation
    {
        public void Execute(Action executeDelegate);
    }

    public interface IStateCommandAsync : IStateCommand
    {
        public bool CanCancel { get; }
        public CancellationToken CancellationToken { get; }
        public Guid? ChangeCallerID { get; }
        public void NotifyChanging();
        public Task ExecuteAsync(Func<IStateCommandAsync, Task> executeDelegateAsync, bool deferredNotification = false, Guid? changeCallerID = null);
        public void Cancel();
    }


    public interface IStateGroup<T> : IStateInformation
    {
        public T Value { get; }
        public T[] Items { get; }
        public bool ItemDisabled(int index);
        public void ChangeValue(T value, Action<T, T>? changingDelegate = null);
    }

    public interface IStateGroupAsync<T> : IStateInformation
    {
        public T Value { get; }
        public T[] Items { get; }
        public bool ItemDisabled(int index);
        public Task ChangeValueAsync(T value, Func<T, T, Task>? changingDelegateAsync = null);
    }
}
