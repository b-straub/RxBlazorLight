
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection.Metadata;

namespace RxBlazorLightCore
{
    public class CommandBase : IObservable<Exception?>
    {
        private readonly Subject<Exception?> _changedSubject;
        private readonly IObservable<Exception?> _changedObservable;

        protected CommandBase()
        {
            _changedSubject = new();
            _changedObservable = _changedSubject.Publish().RefCount();
        }

        protected void Changed(Exception? exception = null)
        {
            _changedSubject.OnNext(exception);
        }

        public IDisposable Subscribe(IObserver<Exception?> observer)
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
            Exception? error = null;

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
                }
            }
            catch (Exception ex)
            {
                error = ex;
            }

            Changed(error);
        }
    }

    public abstract class Command<T> : CommandBase, ICommand<T>
    {
        public Func<ICommand<T>, bool>? PrepareExecution { get; set; }
        public T? Parameter { get; set; }

        protected abstract void DoExecute(T parameter);

        public virtual bool CanExecute(T? parameter)
        {
            return true;
        }

        public void Execute(T parameter)
        {
            Parameter = parameter;
            Execute();
        }

        public void Execute()
        {
            Exception? error = null;

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
                }
            }
            catch (Exception ex)
            {
                error = ex;
            }

            Parameter = default;
            Changed(error);
        }
    }


    public abstract class CommandService<S> : Command, IDisposable where S : IRXService
    {
        protected S Service { get; }

        private IDisposable? _serviceDisposable;

        protected CommandService(S service)
        {
            Service = service;
            _serviceDisposable = this.Subscribe(Service.StateHasChanged);
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
            _serviceDisposable = this.Subscribe(Service.StateHasChanged);
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
            Exception? error = null;
            ResetCancel();

            Executing = true;
            Changed();

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
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() != typeof(TaskCanceledException))
                {
                    error = ex;
                }
            }

            Executing = false;
            Changed(error);
        }

        public void Cancel()
        {
            CancellationTokenSource.Cancel();
            Executing = false;

            Changed();
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
        public T? Parameter { get; set; }

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

        public async Task Execute(T parameter)
        {
            Parameter = parameter;
            await Execute();
        }

        public async Task Execute()
        {
            Exception? error = null;
            ResetCancel();

            Executing = true;
            Changed();

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
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() != typeof(TaskCanceledException))
                {
                    error = ex;
                }
            }

            Executing = false;
            Changed(error);
        }

        public void Cancel()
        {
            CancellationTokenSource.Cancel();
            Executing = false;

            Changed();
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
            _serviceDisposable = this.Subscribe(Service.StateHasChanged);
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
            _serviceDisposable = this.Subscribe(Service.StateHasChanged);
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
