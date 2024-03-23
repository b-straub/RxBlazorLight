using System.Reactive;
using System.Diagnostics.CodeAnalysis;

namespace RxBlazorLightCore
{
    public enum ChangeReason
    {
        EXCEPTION,
        STATE
    }

    public readonly record struct ServiceChangeReason(Guid ServiceID, Guid StateID, ChangeReason Reason);
    public readonly record struct ServiceException(Exception Exception, Guid ID);
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
    }

    public enum StatePhase
    {
        CHANGING,
        CHANGED,
        CANCELED,
        EXCEPTION
    }

    public interface IStateBase<T> : IObservable<StatePhase>
    {
        public T Value { get; set; }

        [MemberNotNullWhen(true, nameof(Value))]
        public bool HasValue();
        public void NotifyChanging();
        public StatePhase Phase { get; }
        public Guid ID { get; }

        public Guid? ChangeCallerID { get; }
    }

    public interface IState<T> : IStateBase<T>
    {
        public bool CanChange(Func<IState<T>, bool> canChangeDelegate);

        public void Change(Action<IState<T>> changeDelegate, bool notify = true);
    }

    public interface IStateAsync<T> : IStateBase<T>
    {
        public bool CanChange(Func<IStateAsync<T>, bool> canChangeDelegate);
        public Task ChangeAsync(Func<IStateAsync<T>, Task> changeDelegateAsync, bool notify = true, Guid? changeCallerID = null);
        public Task ChangeAsync(Func<IStateAsync<T>, CancellationToken, Task> changeDelegateAsync, bool notify = true, Guid? changeCallerID = null);
        public bool CanCancel { get; }
        public void Cancel();
    }

    public interface IState: IState<Unit>
    {
        public bool CanChange(Func<IState, bool> canChangeDelegate);
        public void Change(Action changeDelegate, bool notify = true);
    }

    public interface IStateAsync : IStateAsync<Unit>
    {
        public bool CanChange(Func<IStateAsync, bool> canChangeDelegate);
        public Task ChangeAsync(Func<Task> changeDelegateAsync, bool notify = true, Guid? changeCallerID = null);
        public Task ChangeAsync(Func<CancellationToken, Task> changeDelegateAsync, bool notify = true, Guid? changeCallerID = null);
    }

    public interface IStateGroup<T> : IState<T>
    {
        public T[] Items { get; }
        public bool ItemDisabled(int index);
    }

    public interface IStateGroupAsync<T> : IStateAsync<T>
    {
        public T[] Items { get; }
        public bool ItemDisabled(int index);
    }
}
