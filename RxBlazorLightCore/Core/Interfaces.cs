global using ServiceChangeReason = (RxBlazorLightCore.ChangeReason Reason, System.Guid ID);
global using ServiceException = (System.Exception Exception, System.Guid ID);

using System.Diagnostics.CodeAnalysis;
using System.Reactive;

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


    public interface IState<TInterface, TType> : IStateTransformer<TType> where TType : TInterface
    {
        public TInterface? Value { get; }

        [MemberNotNullWhen(true, nameof(Value))]
        public bool HasValue();
    }

    public interface IState<TType> : IState<TType, TType>;

    public interface IState : IState<Unit, Unit>
    {
        public static Unit Default { get; } = Unit.Default;
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

    public enum StateChangePhase
    {
        NONE,
        CHANGING,
        CHANGED,
        COMPLETED,
        CANCELED,
        EXCEPTION
    }

    public interface IStateProvideTransformBase
    {
        public StateChangePhase Phase { get; }
        public bool LongRunning { get; }
        public bool CanCancel { get; }
        public void Cancel();
        public Guid ID { get; }
    }

    public interface IStateTransformer<T> : IStateProvideTransformBase
    {
        public void Transform(T value);

        public bool CanTransform(T? value);
    }

    public interface IObservableStateProvider<T> : IStateProvideTransformBase, IObserver<T>
    {
        void Provide(T value);
    }

    public interface IObservableStateProvider : IObservableStateProvider<Unit>
    {
        void Provide();
    }

    public interface IStateProvider<T> : IStateProvideTransformBase
    {
        public void Provide();

        public bool CanProvide(T? value);
    }

    public interface IStateProvider : IStateProvider<Unit>
    {
    }
}
