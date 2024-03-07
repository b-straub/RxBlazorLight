
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace RxBlazorLightCore
{
    public static class RxExtensions
    {
        public static IState<TInterface, TType> CreateState<S, TInterface, TType>(this S service, TType? value, Func<IState<TInterface, TType>, IStateTransformer<TType>>? stateProviderTransformerFactory = null)
            where S : RxBLService
            where TType : class, TInterface
        {
            return State<S, TInterface, TType>.Create(service, value, stateProviderTransformerFactory);
        }

        public static IState<TType> CreateState<S, TType>(this S service, TType? value, Func<IState<TType>, IStateTransformer<TType>>? stateProviderTransformerFactory = null) where S : RxBLService
        {
            return State<S, TType>.Create(service, value, stateProviderTransformerFactory);
        }

        public static IObservableStateProvider CreateObservableStateProvider<S>(this S service) where S : RxBLService
        {
            return ObservableStateProvider<S>.Create(service);
        }

        public static IObservableStateProvider<T> CreateObservableStateProvider<S, T>(this S service, IState<T> state) where S : RxBLService
        {
            return ObservableStateProvider<S, T>.Create(service, state);
        }

        public static IObservable<Unit> GetStatePhaseObservable<S>(this S service, IStateProvideTransformBase stateProviderTransformer, 
            StateChangePhase phase = StateChangePhase.CHANGED) where S : RxBLService
        {
            return service
                .Where(sc => sc.ID == stateProviderTransformer.ID && stateProviderTransformer.Phase == phase)
                .Select(_ => Unit.Default);
        }

        public static bool Changing(this IStateProvideTransformBase stateProviderTransformer)
        {
            return stateProviderTransformer.Phase is StateChangePhase.CHANGING;
        }

        public static bool Changed(this IStateProvideTransformBase stateProviderTransformer)
        {
            return stateProviderTransformer.Phase is StateChangePhase.CHANGED;
        }

        public static bool Completed(this IStateProvideTransformBase stateProviderTransformer)
        {
            return stateProviderTransformer.Phase is StateChangePhase.COMPLETED;
        }

        public static bool Canceled(this IStateProvideTransformBase stateProviderTransformer)
        {
            return stateProviderTransformer.Phase is StateChangePhase.CANCELED;
        }

        public static bool Exception(this IStateProvideTransformBase stateProviderTransformer)
        {
            return stateProviderTransformer.Phase is StateChangePhase.EXCEPTION;
        }

        public static bool Done(this IStateProvideTransformBase stateProviderTransformer)
        {
            return stateProviderTransformer.Phase is StateChangePhase.CHANGED ||
                stateProviderTransformer.Phase is StateChangePhase.CANCELED ||
                stateProviderTransformer.Phase is StateChangePhase.EXCEPTION;
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
