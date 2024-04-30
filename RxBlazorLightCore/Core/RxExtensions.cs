
using System.Reactive;
using System.Reactive.Linq;

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

        public static IStateGroup<T> CreateStateGroup<T>(this RxBLService service, T[] items, T? value = default)
        {
            return StateGroup<T>.Create(service, items, value);
        }

        public static IStateGroupAsync<T> CreateStateGroupAsync<T>(this RxBLService service, T[] items, T? value = default)
        {
            return StateGroupAsync<T>.Create(service, items, value);
        }

        public static IObservable<Unit> AsObservable(this RxBLService service, IStateInformation state)
        {
            return service.Where(cr => cr.ID == state.ID).Select(_ => Unit.Default);
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
