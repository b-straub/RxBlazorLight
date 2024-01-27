
namespace RxBlazorLightCore
{
    public static class RxExtensions
    {
        public static IState<TInterface, TType> CreateState<S, TInterface, TType>(this S service, TType? value, Func<IState<TInterface, TType>, IValueProvider<TType>>? valueProviderFactory = null)
            where S : RxBLService
            where TType : class, TInterface
        {
            return State<S, TInterface, TType>.Create(service, value, valueProviderFactory);
        }

        public static IState<TType> CreateState<S, TType>(this S service, TType? value, Func<IState<TType>, IValueProvider<TType>>? valueProviderFactory = null) where S : RxBLService
        {
            return State<S, TType>.Create(service, value, valueProviderFactory);
        }

        public static IState CreateState<S>(this S service) where S : RxBLService
        {
            return State<S>.Create(service);
        }

        public static bool Providing<T>(this IValueProviderBase<T> valueProvider)
        {
            return valueProvider.Phase is ValueProviderPhase.PROVIDING;
        }

        public static bool Provided<T>(this IValueProviderBase<T> valueProvider)
        {
            return valueProvider.Phase is ValueProviderPhase.PROVIDED;
        }

        public static bool Canceled<T>(this IValueProviderBase<T> valueProvider)
        {
            return valueProvider.Phase is ValueProviderPhase.CANCELED;
        }

        public static bool Exception<T>(this IValueProviderBase<T> valueProvider)
        {
            return valueProvider.Phase is ValueProviderPhase.EXCEPTION;
        }

        public static bool Done<T>(this IValueProviderBase<T> valueProvider)
        {
            return valueProvider.Phase is ValueProviderPhase.PROVIDED ||
                valueProvider.Phase is ValueProviderPhase.CANCELED ||
                valueProvider.Phase is ValueProviderPhase.EXCEPTION;
        }

        internal static void RunValueTask(this ValueTask valueTask)
        {
            if (!valueTask.IsCompleted)
            {
                throw new InvalidOperationException("ValueTask does nor run synchronously!");
            }

            valueTask.GetAwaiter().GetResult();
        }

        internal static TResult RunValueTask<TResult>(this ValueTask<TResult> valueTask)
        {
            if (!valueTask.IsCompleted)
            {
                throw new InvalidOperationException("ValueTask does nor run synchronously!");
            }

            return valueTask.GetAwaiter().GetResult();
        }
    }
}
