global using ServiceChangeReason = (RxBlazorLightCore.ChangeReason Reason, System.Guid ID);
global using ServiceException = (System.Exception Exception, System.Guid ID);

using System.Reactive;
using System.Diagnostics.CodeAnalysis;

namespace RxBlazorLightCore
{
    public enum ChangeReason
    {
        EXCEPTION,
        SERVICE,
        STATE
    }

    public interface IRxBLScope 
    {
        public void EnterScope() { }
        public void LeaveScope() { }
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
    }

    public interface IState<T> : IStateBase<T>
    {
        public bool CanChange(Func<IState<T>, bool> canChangeDelegate);

        public void Change(Action<IState<T>> changeDelegate, bool notify = true);
    }

    public interface IStateAsync<T> : IStateBase<T>
    {
        public bool CanChange(Func<IStateAsync<T>, bool> canChangeDelegate);
        public Task ChangeAsync(Func<IStateAsync<T>, Task> changeDelegateAsync, bool notify = true);
        public Task ChangeAsync(Func<IStateAsync<T>, CancellationToken, Task> changeDelegateAsync, bool notify = true);
        public bool CanCancel { get; }
        public void Cancel();
    }

    public interface IState: IState<Unit>
    {
        public bool CanChange(Func<IState, bool> canChangeDelegate);
        public void Change(Action<IRxBLService> changeDelegate, bool notify = true);
    }

    public interface IStateAsync : IStateAsync<Unit>
    {
        public bool CanChange(Func<IStateAsync, bool> canChangeDelegate);
        public Task ChangeAsync(Func<IRxBLService, Task> changeDelegateAsync, bool notify = true);
        public Task ChangeAsync(Func<IRxBLService, CancellationToken, Task> changeDelegateAsync, bool notify = true);
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
