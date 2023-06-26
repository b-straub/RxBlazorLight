
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace RxBlazorLightCore
{
    public class CommandBase : IObservable<StateChange>
    {
        public Exception? LastException { get; protected set; }
        public IObservable<Unit> Executed{ get; }

        private readonly Subject<StateChange> _changedSubject;
        private readonly IObservable<StateChange> _changedObservable;

        protected CommandBase()
        {
            _changedSubject = new();
            _changedObservable = _changedSubject.Publish().RefCount();

            Executed = _changedObservable
                .Where(sc => sc is StateChange.CMD_EXECUTED)
                .Select(_ => Unit.Default);
        }

        protected void Changed(StateChange reason)
        {
            _changedSubject.OnNext(reason);
        }

        public IDisposable Subscribe(IObserver<StateChange> observer)
        {
            return _changedObservable.Subscribe(observer);
        }
    }

    public abstract class Command : CommandBase, ICommand
    {
        public Func<ICommand, bool>? PrepareExecution { get; set; }

        protected abstract void DoExecute();

        public virtual bool CanExecute()
        {
            return true;
        }

        public void Execute()
        {
            bool executed = false;
            LastException = null;

            try
            {
                bool execute = true;

                if (PrepareExecution is not null)
                {
                    execute = PrepareExecution(this);
                }

                if (execute && CanExecute())
                {
                    DoExecute();
                    executed = true;
                }
            }
            catch (Exception ex)
            {
                LastException = ex;
            }

            Changed(LastException is not null ? StateChange.EXCEPTION : 
                executed ? StateChange.CMD_EXECUTED : StateChange.CMD_NOT_EXECUTED);
        }
    }

    public abstract class Command<T> : CommandBase, ICommand<T>
    {
        public Func<ICommand<T>, bool>? PrepareExecution { get; set; }
        public T? Parameter { get; private set; }

        public virtual void SetParameter(T? parameter)
        {
            Parameter = parameter;
        }

        protected abstract void DoExecute(T parameter);

        public virtual bool CanExecute(T? parameter)
        {
            return true;
        }

        public virtual void Execute(T parameter)
        {
            Parameter = parameter;
            Execute();
        }

        public virtual void Execute()
        {
            bool executed = false;
            LastException = null;

            try
            {
                bool execute = true;

                if (PrepareExecution is not null)
                {
                    execute = PrepareExecution(this);
                }

                if (execute && CanExecute(Parameter) && Parameter is not null)
                {
                    DoExecute(Parameter);
                    executed = true;
                }
            }
            catch (Exception ex)
            {
                LastException = ex;
            }

            if (executed)
            {
                Parameter = default;
            }

            Changed(LastException is not null ? StateChange.EXCEPTION :
                executed ? StateChange.CMD_EXECUTED : StateChange.CMD_NOT_EXECUTED);
        }
    }

    public abstract class CommandService<S> : Command, IDisposable where S : IRXService
    {
        protected S Service { get; }

        private IDisposable? _serviceDisposable;

        protected CommandService(S service)
        {
            Service = service;
            _serviceDisposable = this.Subscribe(r => Service.StateHasChanged(r, LastException));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _serviceDisposable?.Dispose();
                _serviceDisposable = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public abstract class CommandService<S, T> : Command<T>, IDisposable where S : IRXService
    {
        protected S Service { get; }

        private IDisposable? _serviceDisposable;

        protected CommandService(S service)
        {
            Service = service;
            _serviceDisposable = this.Subscribe(r => Service.StateHasChanged(r, LastException));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _serviceDisposable?.Dispose();
                _serviceDisposable = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }


    public abstract class CommandAsync : CommandBase, ICommandAsync
    {
        public Func<ICommandAsync, CancellationToken, Task<bool>>? PrepareExecutionAsync { get; set; }
        public bool Executing { get; protected set; }

        protected CancellationTokenSource CancellationTokenSource { get; private set; } = new();

        protected CommandAsync()
        {
            Executing = false;
        }

        protected abstract Task DoExecute(CancellationToken cancellationToken);

        public virtual bool CanCancel()
        {
            return false;
        }

        public virtual bool HasProgress()
        {
            return false;
        }

        public virtual bool CanExecute()
        {
            return true;
        }

        public async Task Execute()
        {
            ResetCancel();

            var executed = false;
            LastException = null;
            Executing = true;
            Changed(StateChange.CMD_EXECUTING);

            try
            {
                bool execute = true;

                if (PrepareExecutionAsync is not null)
                {
                    execute = await PrepareExecutionAsync(this, CancellationTokenSource.Token);
                }

                if (execute && CanExecute())
                {
                    await DoExecute(CancellationTokenSource.Token);
                    executed = true;
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() != typeof(TaskCanceledException))
                {
                    LastException = ex;
                }
            }

            Executing = false;
            Changed(LastException is not null ? StateChange.EXCEPTION :
                executed ? StateChange.CMD_EXECUTED : StateChange.CMD_NOT_EXECUTED);
        }

        public void Cancel()
        {
            CancellationTokenSource.Cancel();
            Executing = false;

            Changed(StateChange.CMD_CANCELED);
        }

        protected void ResetCancel()
        {
            if (!CancellationTokenSource.TryReset())
            {
                CancellationTokenSource = new();
            }
        }
    }

    public abstract class CommandAsync<T> : CommandBase, ICommandAsync<T>
    {
        public T? Parameter { get; private set; }

        public virtual void SetParameter(T? parameter)
        {
            Parameter = parameter;
        }

        public Func<ICommandAsync<T>, CancellationToken, Task<bool>>? PrepareExecutionAsync { get; set; }

        public bool Executing { get; protected set; }
        protected CancellationTokenSource CancellationTokenSource { get; private set; } = new();

        protected CommandAsync()
        {
            Executing = false;
        }

        public virtual bool CanCancel()
        {
            return false;
        }

        public virtual bool HasProgress()
        {
            return false;
        }

        protected abstract Task DoExecute(T parameter, CancellationToken cancellationToken);

        public virtual bool CanExecute(T? parameter)
        {
            return true;
        }

        public virtual async Task Execute(T parameter)
        {
            Parameter = parameter;
            await Execute();
        }

        public virtual async Task Execute()
        {
            ResetCancel();

            var executed = false;
            LastException = null;
            Executing = true;
            Changed(StateChange.CMD_EXECUTING);

            try
            {
                bool execute = true;

                if (PrepareExecutionAsync is not null)
                {
                    execute = await PrepareExecutionAsync(this, CancellationTokenSource.Token);
                }

                if (execute && CanExecute(Parameter) && Parameter is not null)
                {
                    await DoExecute(Parameter, CancellationTokenSource.Token);
                    executed = true;
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() != typeof(TaskCanceledException))
                {
                    LastException = ex;
                }
            }

            if (executed)
            {
                Parameter = default;
            }

            Executing = false;
            Changed(LastException is not null ? StateChange.EXCEPTION :
                executed ? StateChange.CMD_EXECUTED : StateChange.CMD_NOT_EXECUTED);
        }

        public void Cancel()
        {
            CancellationTokenSource.Cancel();
            Executing = false;

            Changed(StateChange.CMD_CANCELED);
        }

        protected void ResetCancel()
        {
            if (!CancellationTokenSource.TryReset())
            {
                CancellationTokenSource = new();
            }
        }
    }

    public abstract class CommandLongRunningAsync : CommandAsync
    {
        public override bool CanCancel()
        {
            return true;
        }

        public override bool HasProgress()
        {
            return true;
        }
    }

    public abstract class CommandLongRunningAsync<T> : CommandAsync<T>
    {
        public override bool CanCancel()
        {
            return true;
        }

        public override bool HasProgress()
        {
            return true;
        }
    }

    public abstract class CommandServiceAsync<S> : CommandAsync, IDisposable where S : IRXService
    {
        protected S Service { get; }

        private IDisposable? _serviceDisposable;

        protected CommandServiceAsync(S service)
        {
            Service = service;
            _serviceDisposable = this.Subscribe(r => Service.StateHasChanged(r, LastException));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _serviceDisposable?.Dispose();
                _serviceDisposable = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public abstract class CommandServiceAsync<S, T> : CommandAsync<T>, IDisposable where S : IRXService
    {
        protected S Service { get; }

        private IDisposable? _serviceDisposable;

        protected CommandServiceAsync(S service)
        {
            Service = service;
            _serviceDisposable = this.Subscribe(r => Service.StateHasChanged(r, LastException));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _serviceDisposable?.Dispose();
                _serviceDisposable = null;
            }
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public abstract class CommandLongRunningServiceAsync<S> : CommandServiceAsync<S> where S : IRXService
    {
        protected CommandLongRunningServiceAsync(S service) : base(service)
        {
        }

        public override bool CanCancel()
        {
            return true;
        }

        public override bool HasProgress()
        {
            return true;
        }
    }

    public abstract class CommandLongRunningServiceAsync<S, T> : CommandServiceAsync<S, T> where S : IRXService
    {
        protected CommandLongRunningServiceAsync(S service) : base(service)
        {
        }

        public override bool CanCancel()
        {
            return true;
        }

        public override bool HasProgress()
        {
            return true;
        }
    }
}