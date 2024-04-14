
namespace RxBlazorLightCore
{
    public static class RxExtensions
    {
        public static IState<T> CreateState<T>(this RxBLService service, T value)
        {
            return State<T>.Create(service, value);
        }

        public static IStateCommand CreateStateCommand(this RxBLService service)
        {
            return StateCommand.Create(service);
        }

        public static IStateCommandAsync CreateStateCommandAsync(this RxBLService service, bool canCancel = false)
        {
            return StateCommandAsync.Create(service, canCancel);
        }

        public static IStateGroup<T> CreateStateGroup<T>(this RxBLService service, T[] items, T value, Func<int, bool>? itemDisabledDelegate = null)
        {
            return StateGroup<T>.Create(service, items, value, itemDisabledDelegate);
        }

        public static IStateGroupAsync<T> CreateStateGroupAsync<T>(this RxBLService service, T[] items, T value, Func<int, bool>? itemDisabledDelegate = null)
        {
            return StateGroupAsync<T>.Create(service, items, value, itemDisabledDelegate);
        }

        public static bool Changing(this IStateInformation state)
        {
            return state.Phase is StatePhase.CHANGING;
        }

        public static bool Changed(this IStateInformation state)
        {
            return state.Phase is StatePhase.CHANGED;
        }

        public static bool Canceled(this IStateInformation state)
        {
            return state.Phase is StatePhase.CANCELED;
        }

        public static bool Exception(this IStateInformation state)
        {
            return state.Phase is StatePhase.EXCEPTION;
        }

        public static bool Done(this IStateInformation state)
        {
            return state.Phase is StatePhase.CHANGED ||
                state.Phase is StatePhase.CANCELED ||
                state.Phase is StatePhase.EXCEPTION;
        }
    }
}
