using R3;

// ReSharper disable once CheckNamespace -> use same namespace for implementation
namespace RxBlazorLightCore
{
    internal class StateInformationComparer : IEqualityComparer<IStateInformation>
    {
        public bool Equals(IStateInformation? s1, IStateInformation? s2)
        {
            if (s1 is null && s2 is null)
            {
                return true;
            }

            if (s1 is null | s2 is null)
            {
                return false;
            }

            return s1!.StateID.Equals(s2!.StateID);
        }

        public int GetHashCode(IStateInformation s)
        {
            return s.StateID.GetHashCode();
        }
    }

    public abstract class RxBLStateOwner : IRxBLStateOwner
    {
        public abstract IStateCommand Command { get; }
        public abstract IStateCommandAsync CommandAsync { get; }
        public abstract IStateCommandAsync CancellableCommandAsync { get; }
        public abstract Observable<ServiceChangeReason> AsObservable { get; }
        
        public Guid OwnerID { get; } = Guid.NewGuid();
        public bool Disabled => _ownedStates.Any(s => s.Phase == StatePhase.CHANGING);

        private readonly HashSet<IStateInformation> _ownedStates = new(new StateInformationComparer());

        public bool OwnsState(Guid stateID)
        {
            return stateID == OwnerID || _ownedStates.Select(s => s.StateID).Contains(stateID);
        }

        internal void AddOwnedState(IStateInformation stateInformation)
        {
            _ownedStates.Add(stateInformation);
        }
        
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
    

    public class RxBLStateScope<T> : RxBLStateOwner, IRxBLStateScope<T> where T : RxBLService
    {
        public T Service { get; }
        public override IStateCommand Command { get; }
        public override IStateCommandAsync CommandAsync { get; }
        public override IStateCommandAsync CancellableCommandAsync { get; }
        public override Observable<ServiceChangeReason> AsObservable { get; }
        
        protected RxBLStateScope(T service)
        {
            Service = service;
            Command = this.CreateStateCommand();
            CommandAsync = this.CreateStateCommandAsync();
            CancellableCommandAsync = this.CreateStateCommandAsync(true);
            
            AsObservable = service.AsRawObservable
                .Where(cr => OwnsState(cr.StateID));
        }

        public virtual ValueTask OnContextReadyAsync()
        {
            return ValueTask.CompletedTask;
        }
    }

    public class RxBLService : RxBLStateOwner, IRxBLService
    {
        public bool Initialized { get; private set; }
        public IEnumerable<ServiceException> Exceptions => _serviceExceptions;
        public override IStateCommand Command { get; }
        public override IStateCommandAsync CommandAsync { get; }
        public override IStateCommandAsync CancellableCommandAsync { get; }
        public override Observable<ServiceChangeReason> AsObservable { get; }
        
        internal Observable<ServiceChangeReason> AsRawObservable { get; }
        // ReSharper disable once BuiltInTypeReferenceStyle
        private readonly Object _gate = new();
        private readonly Subject<ServiceChangeReason> _changedSubject = new();
        private readonly HashSet<ServiceException> _serviceExceptions;
     
        protected RxBLService()
        {
            Command = this.CreateStateCommand();
            CommandAsync = this.CreateStateCommandAsync();
            CancellableCommandAsync = this.CreateStateCommandAsync(true);
            AsRawObservable = _changedSubject
                .Synchronize(_gate)
                .Share();

            AsObservable = AsRawObservable
                .Where(cr => OwnsState(cr.StateID));
            
            _serviceExceptions = [];
            Initialized = false;
        }

        public void StateHasChanged(Exception? exception = null)
        {
            lock (_gate)
            {
                StateHasChanged(OwnerID, exception);
            }
        }
        
        internal void StateHasChanged(Guid stateID, Exception? exception = null)
        {
            if (_changedSubject.IsDisposed)
            {
                return;
            }

            if (exception is not null)
            {
                _serviceExceptions.Add(new(stateID, exception));
                _changedSubject.OnNext(new(stateID, ChangeReason.EXCEPTION));
            }
            else
            {
                _changedSubject.OnNext(new(stateID, ChangeReason.STATE));
            }
        }

        public async ValueTask OnContextReadyAsync()
        {
            if (Initialized)
            {
                throw new InvalidOperationException("Nested RxBLService context not allowed!");
            }

            await ContextReadyAsync();
            Initialized = true;
            StateHasChanged(OwnerID);
        }

        protected virtual ValueTask ContextReadyAsync()
        {
            return ValueTask.CompletedTask;
        }

        public void ResetExceptions()
        {
            _serviceExceptions.Clear();
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _changedSubject.Dispose();
            }
        }
    }
}