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

    public interface IState<T>
    {
        public T Value { get; set; }

        [MemberNotNullWhen(true, nameof(Value))]
        public bool HasValue();
        public Guid ID { get; }
    }

    public interface IStateCommandBase
    {
        public StatePhase Phase { get; }
        public Guid ID { get; }
        public Guid? ChangeCallerID { get; }
    }

    public interface IStateCommandAsyncBase
    {
        public void NotifyChanging();
    }

    public interface IStateCommand : IStateCommandBase
    {
        public void Change();

        public void Change(Action changeDelegate);
    }

    public interface IStateCommandAsync : IStateCommand, IStateCommandAsyncBase
    {
        public Task ChangeAsync(Func<Task> changeDelegateAsync, bool notifyChanging = true, Guid? changeCallerID = null);
        public Task ChangeAsync(Func<CancellationToken, Task> changeDelegateAsync, bool notifyChanging = true, Guid? changeCallerID = null);
        public void Cancel();
    }


    public interface IStateGroup<T> : IStateCommand
    {
        public T Value { get; set; }

        public T[] Items { get; }
        public bool ItemDisabled(int index);
    }

    public interface IStateGroupAsync<T> : IStateCommandAsync
    {
        public T Value { get; set; }

        public T[] Items { get; }
        public bool ItemDisabled(int index);
    }
}
