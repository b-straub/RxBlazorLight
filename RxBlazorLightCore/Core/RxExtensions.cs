
using System.Reactive;

namespace RxBlazorLightCore
{
    public static class RxExtensions
    {
        public static IState<TInterface, TType> CreateState<S, TInterface, TType>(this S service, TType? value, Func<IState<TInterface, TType>, IStateTransformer<TType>>? valueProviderFactory = null)
            where S : RxBLService
            where TType : class, TInterface
        {
            return State<S, TInterface, TType>.Create(service, value, valueProviderFactory);
        }

        public static IState<TType> CreateState<S, TType>(this S service, TType? value, Func<IState<TType>, IStateTransformer<TType>>? valueProviderFactory = null) where S : RxBLService
        {
            return State<S, TType>.Create(service, value, valueProviderFactory);
        }

        public static IObservableStateProvider<Unit> CreateObservableStateProvider<S>(this S service) where S : RxBLService
        {
            return ObservableStateProvider<S, Unit>.Create(service);
        }

        public static IObservableStateProvider<T> CreateObservableStateProvider<S, T>(this S service, IState<T> state) where S : RxBLService
        {
            return ObservableStateProvider<S, T>.Create(service, state);
        }

        public static bool Changing(this IStateProvideTransformBase valueProvider)
        {
            return valueProvider.Phase is StateChangePhase.CHANGING;
        }

        public static bool Changed(this IStateProvideTransformBase valueProvider)
        {
            return valueProvider.Phase is StateChangePhase.CHANGED;
        }

        public static bool Completed(this IStateProvideTransformBase valueProvider)
        {
            return valueProvider.Phase is StateChangePhase.COMPLETED;
        }

        public static bool Canceled(this IStateProvideTransformBase valueProvider)
        {
            return valueProvider.Phase is StateChangePhase.CANCELED;
        }

        public static bool Exception(this IStateProvideTransformBase valueProvider)
        {
            return valueProvider.Phase is StateChangePhase.EXCEPTION;
        }

        public static bool Done(this IStateProvideTransformBase valueProvider)
        {
            return valueProvider.Phase is StateChangePhase.CHANGED ||
                valueProvider.Phase is StateChangePhase.CANCELED ||
                valueProvider.Phase is StateChangePhase.EXCEPTION;
        }

        internal static void RunValueTask(this ValueTask valueTask)
        {
            if (!valueTask.IsCompleted)
            {
                throw new InvalidOperationException("ValueTask does not run synchronously!");
            }

            valueTask.GetAwaiter().GetResult();
        }

        internal static TResult RunValueTask<TResult>(this ValueTask<TResult> valueTask)
        {
            if (!valueTask.IsCompleted)
            {
                throw new InvalidOperationException("ValueTask does not run synchronously!");
            }

            return valueTask.GetAwaiter().GetResult();
        }
    }
}
