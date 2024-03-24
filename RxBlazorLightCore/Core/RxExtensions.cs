
namespace RxBlazorLightCore
{
    public static class RxExtensions
    {
        public static IState<T> CreateState<T>(this RxBLService service, T value)
        {
            return State<T>.Create(service, value);
        }

        public static IStateAsync<T> CreateStateAsync<T>(this RxBLService service, T value)
        {
            return StateAsync<T>.Create(service, value);
        }

        public static IStateGroup<T> CreateStateGroup<T>(this RxBLService service, T[] items, T inititalItem, Func<int, bool>? itemDisabledDelegate = null)
        {
            return StateGroup<T>.Create(service, items, inititalItem, itemDisabledDelegate);
        }

        public static IStateGroupAsync<T> CreateStateGroupAsync<T>(this RxBLService service, T[] items, T inititalItem, Func<int, bool>? itemDisabledDelegate = null)
        {
            return StateGroupAsync<T>.Create(service, items, inititalItem, itemDisabledDelegate);
        }

        public static bool Changing<T>(this IStateBase<T> state)
        {
            return state.Phase is StatePhase.CHANGING;
        }

        public static bool Changed<T>(this IStateBase<T> state)
        {
            return state.Phase is StatePhase.CHANGED;
        }

        public static bool Canceled<T>(this IStateBase<T> state)
        {
            return state.Phase is StatePhase.CANCELED;
        }

        public static bool Exception<T>(this IStateBase<T> state)
        {
            return state.Phase is StatePhase.EXCEPTION;
        }

        public static bool Done<T>(this IStateBase<T> state)
        {
            return state.Phase is StatePhase.CHANGED ||
                state.Phase is StatePhase.CANCELED ||
                state.Phase is StatePhase.EXCEPTION;
        }
    }
}
