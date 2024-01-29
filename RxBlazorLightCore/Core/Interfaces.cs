global using ServiceChangeReason = (RxBlazorLightCore.ChangeReason Reason, System.Guid ID);
global using ServiceException = (System.Exception Exception, System.Guid ID);

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


    public interface IRxBLService
    {
        public ValueTask OnContextReadyAsync();
        public bool Initialized { get; }
        public IEnumerable<ServiceException> Exceptions { get; }
        public void ResetExceptions();
        public IRxBLScope CreateScope();
        public IDisposable Subscribe(Action<ServiceChangeReason> stateHasChanged, double sampleMS);
        public Guid ID { get; }
    }


    public interface IState<TInterface, TType> : IValueProvider<TType> where TType : TInterface
    {
        public TInterface? Value { get; }

        [MemberNotNullWhen(true, nameof(Value))]
        public bool HasValue();
        public bool CanChange();
    }

    public interface IState<TType> : IState<TType, TType>;

    public interface IState : IState<object?, object?>
    {
        public static object? Default { get; } = default;
    }

    public interface IStateGroup<TType> : IState<TType>
    {
        public TType[] GetItems();
        public bool IsItemDisabled(int index);
    }

    public interface IStateGroup<TInterface, TType> : IState<TInterface, TType>
        where TType : TInterface
    {
        public TInterface[] GetItems();
        public bool IsItemDisabled(int index);
    }

    public enum ValueProviderPhase
    {
        NONE,
        PROVIDING,
        PROVIDED,
        CANCELED,
        EXCEPTION
    }

    public interface IValueProviderBase<T>
    {
        public ValueProviderPhase Phase { get; }
        public bool LongRunning();
        public bool CanCancel();
        public void Cancel();
        public Guid ID { get; }
    }

    public interface IValueProvider<T> : IValueProviderBase<T>
    {
        public void Run(T value);
    }

    public interface IValueProviderVoid<T> : IValueProviderBase<object?>
    {
        public void Run();
    }

    public interface IStateProvider : IValueProviderBase<object?>
    {
        public void Run();
    }

    public interface IStateProvider<T> : IValueProviderBase<object?>
    {
        public void Run(T value);
    }
}
