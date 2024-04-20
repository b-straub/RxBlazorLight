
namespace RxBlazorLightCore
{
    public static class RxExtensions
    {
        public static IState<T> CreateState<T>(this RxBLService service, T value)
        {
            return State<T>.Create(service, value, false);
        }

        public static IState<T> CreateIndependentState<T>(this RxBLService service, T value)
        {
            return State<T>.Create(service, value, true);
        }

        public static IStateCommand CreateStateCommand(this RxBLService service)
        {
            return StateCommand.Create(service, false);
        }

        public static IStateCommand CreateIndependentStateCommand(this RxBLService service)
        {
            return StateCommand.Create(service, true);
        }

        public static IStateCommandAsync CreateStateCommandAsync(this RxBLService service, bool canCancel = false)
        {
            return StateCommandAsync.Create(service, canCancel, false);
        }

        public static IStateCommandAsync CreateIndependentStateCommandAsync(this RxBLService service, bool canCancel = false)
        {
            return StateCommandAsync.Create(service, canCancel, true);
        }

        public static IStateGroup<T> CreateStateGroup<T>(this RxBLService service, T[] items, T? value = default)
        {
            return StateGroup<T>.Create(service, items, value, false);
        }

        public static IStateGroup<T> CreateIndependentStateGroup<T>(this RxBLService service, T[] items, T? value = default)
        {
            return StateGroup<T>.Create(service, items, value, true);
        }

        public static IStateGroupAsync<T> CreateStateGroupAsync<T>(this RxBLService service, T[] items, T? value = default)
        {
            return StateGroupAsync<T>.Create(service, items, value, false);
        }

        public static IStateGroupAsync<T> CreateIndependentStateGroupAsync<T>(this RxBLService service, T[] items, T? value = default)
        {
            return StateGroupAsync<T>.Create(service, items, value, true);
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
