using R3;

// ReSharper disable once CheckNamespace -> use same namespace for extensions
namespace RxBlazorLightCore
{
    public static class RxExtensions
    {
        #region Owner

        public static IState<T> CreateState<T>(this RxBLService rxBLService, T value)
        {
            return State<T>.Create(rxBLService, value, rxBLService);
        }

        public static IStateCommand CreateStateCommand(this RxBLService rxBLService)
        {
            return StateCommand.Create(rxBLService, rxBLService);
        }

        public static IStateCommandAsync CreateStateCommandAsync(this RxBLService rxBLService, bool canCancel = false)
        {
            return StateCommandAsync.Create(rxBLService, canCancel, rxBLService);
        }

        public static IStateProgressObserverAsync CreateStateObserverAsync(this RxBLService rxBLService,
            bool handleError = true)
        {
            return StateProgressObserverAsync.Create(rxBLService, handleError, rxBLService);
        }

        public static IStateGroup<T> CreateStateGroup<T>(this RxBLService rxBLService, T[] items, T? value = default)
        {
            return StateGroup<T>.Create(rxBLService, items, value, rxBLService);
        }

        public static IStateGroupAsync<T> CreateStateGroupAsync<T>(this RxBLService rxBLService, T[] items,
            T? value = default)
        {
            return StateGroupAsync<T>.Create(rxBLService, items, value, rxBLService);
        }

        #endregion
        
        #region Scope

        public static IState<TType> CreateState<TType, TService>(this RxBLStateScope<TService> scope, TType value) where TService : RxBLService
        {
            return State<TType>.Create(scope.Service, value, scope);
        }

        public static IStateCommand CreateStateCommand<TService>(this RxBLStateScope<TService> scope) where TService : RxBLService
        {
            return StateCommand.Create(scope.Service, scope);
        }

        public static IStateCommandAsync CreateStateCommandAsync<TService>(this RxBLStateScope<TService> scope, bool canCancel = false) where TService : RxBLService
        {
            return StateCommandAsync.Create(scope.Service, canCancel, scope);
        }

        public static IStateProgressObserverAsync CreateStateObserverAsync<TService>(this RxBLStateScope<TService> scope,
            bool handleError = true) where TService : RxBLService
        {
            return StateProgressObserverAsync.Create(scope.Service, handleError, scope);
        }

        public static IStateGroup<TType> CreateStateGroup<TType, TService>(this RxBLStateScope<TService> scope, TType[] items,
            TType? value = default) where TService : RxBLService
        {
            return StateGroup<TType>.Create(scope.Service, items, value, scope);
        }

        public static IStateGroupAsync<TType> CreateStateGroupAsync<TType, TService>(this RxBLStateScope<TService> scope, TType[] items,
            TType? value = default) where TService : RxBLService
        {
            return StateGroupAsync<TType>.Create(scope.Service, items, value, scope);
        }

        #endregion

        public static Observable<Unit> AsObservable(this RxBLService rxBLService, IStateInformation state)
        {
            return rxBLService.AsObservable.Where(cr => cr.StateID == state.StateID).Select(_ => Unit.Default);
        }

        public static Observable<T> AsChangedObservable<T>(this RxBLService rxBLService, IState<T> state)
        {
            return rxBLService.AsObservable
                .Where(cr => cr.StateID == state.StateID && state.Changed())
                .Select(_ => state.Value);
        }

        public static IDisposable Subscribe(this Observable<Unit> observable, IRxBLService service)
        {
            return observable.Subscribe(_ => service.StateHasChanged(), service.StateHasChanged,
                r => service.StateHasChanged(r.Exception));
        }

        public static bool OwnsState(this IRxBLStateOwner owner, IStateInformation stateInformation)
        {
            return owner.OwnsState(stateInformation.StateID);
        }
        
        public static bool OwnsState(this IRxBLStateOwner owner, ServiceChangeReason cr)
        {
            return owner.OwnsState(cr.StateID);
        }
        
        public static bool Contains(this IEnumerable<ServiceChangeReason> crList, IStateInformation stateInformation)
        {
            return crList.Any(cr => cr.StateID == stateInformation.StateID);
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