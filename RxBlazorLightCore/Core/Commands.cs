
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace RxBlazorLightCore
{
    public class CommandBase : IObservable<ServiceState>
    {
        public CommandState State { get; private set; } = CommandState.NONE;

        public Exception? LastException { get; protected set; }

        private readonly Subject<ServiceState> _changedSubject;
        private readonly IObservable<ServiceState> _changedObservable;

        protected CommandBase()
        {
            _changedSubject = new();
            _changedObservable = _changedSubject.Publish().RefCount();
        }

        protected void Changed(CommandState state)
        {
            State = state;
            _changedSubject.OnNext(state is CommandState.EXCEPTION ? ServiceState.COMMAND_EXCEPTION : ServiceState.COMMAND);
        }

        public IDisposable Subscribe(IObserver<ServiceState> observer)
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

            Changed(LastException is not null ? CommandState.EXCEPTION : 
                executed ? CommandState.EXECUTED : CommandState.NOT_EXECUTED);
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

            Changed(LastException is not null ? CommandState.EXCEPTION :
                executed ? CommandState.EXECUTED : CommandState.NOT_EXECUTED);
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
        public bool Running { get; protected set; }

        protected CancellationTokenSource CancellationTokenSource { get; private set; } = new();

        protected CommandAsync()
        {
            Running = false;
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

        public virtual bool PrepareModal()
        {
            return true;
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
            Running = true;

            Changed(PrepareModal() && PrepareExecutionAsync is not null ? CommandState.PREPARE : CommandState.EXECUTING);

            try
            {
                bool execute = true;

                if (PrepareExecutionAsync is not null)
                {
                    execute = await PrepareExecutionAsync(this, CancellationTokenSource.Token);
                }

                if (execute && CanExecute())
                {
                    if (PrepareModal())
                    {
                        Changed(CommandState.EXECUTING);
                    }
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

            Running = false;
            Changed(LastException is not null ? CommandState.EXCEPTION :
                executed ? CommandState.EXECUTED : CommandState.NOT_EXECUTED);
        }

        public void Cancel()
        {
            CancellationTokenSource.Cancel();
            Running = false;

            Changed(CommandState.CANCELED);
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

        public bool Running { get; protected set; }
        protected CancellationTokenSource CancellationTokenSource { get; private set; } = new();

        protected CommandAsync()
        {
            Running = false;
        }

        public virtual bool CanCancel()
        {
            return false;
        }

        public virtual bool HasProgress()
        {
            return false;
        }

        public virtual bool PrepareModal()
        {
            return true;
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
            Running = true;

            Changed(PrepareModal() && PrepareExecutionAsync is not null ? CommandState.PREPARE : CommandState.EXECUTING);

            try
            {
                bool execute = true;

                if (PrepareExecutionAsync is not null)
                {
                    execute = await PrepareExecutionAsync(this, CancellationTokenSource.Token);
                }

                if (execute && CanExecute(Parameter) && Parameter is not null)
                {
                    if (PrepareModal())
                    {
                        Changed(CommandState.EXECUTING);
                    }
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

            Running = false;
            Changed(LastException is not null ? CommandState.EXCEPTION :
                executed ? CommandState.EXECUTED : CommandState.NOT_EXECUTED);
        }

        public void Cancel()
        {
            CancellationTokenSource.Cancel();
            Running = false;

            Changed(CommandState.CANCELED);
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