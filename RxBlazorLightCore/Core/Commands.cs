
namespace RxBlazorLightCore
{
    public class CommandBase<S>
    {
        protected S Service { get; }

        protected CommandBase(S service)
        {
            Service = service;
        }
    }

    public abstract class Command<S> : CommandBase<S>, ICommand where S : IRXService
    {
        public Func<ICommand, bool>? PrepareExecution { get; set; }

        protected Command(S service) : base(service)
        {
        }

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

            Service.StateHasChanged(error);
        }
    }

    public abstract class Command<S, T> : CommandBase<S>, ICommand<T> where S : IRXService
    {
        public Func<ICommand<T>, bool>? PrepareExecution { get; set; }

        public T? Parameter { get; set; }

        protected Command(S service) : base(service)
        {
        }

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

            Service.StateHasChanged(error);
            Parameter = default;
        }
    }

    public abstract class CommandAsyncBase<S> : CommandBase<S> where S : IRXService
    {
        public bool Executing { get; protected set; }
        protected CancellationTokenSource CancellationTokenSource { get; private set; } = new();

        protected CommandAsyncBase(S service) : base(service)
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


        public void Cancel()
        {
            CancellationTokenSource.Cancel();
            Executing = false;

            Service.StateHasChanged();
        }

        protected void ResetCancel()
        {
            if (!CancellationTokenSource.TryReset())
            {
                CancellationTokenSource = new();
            }
        }
    }

    public abstract class CommandAsync<S> : CommandAsyncBase<S>, ICommandAsync where S : IRXService
    {
        public Func<ICommandAsync, CancellationToken, Task<bool>>? PrepareExecutionAsync { get; set; }

        protected CommandAsync(S service) : base(service)
        {
        }

        protected abstract Task DoExecute(CancellationToken cancellationToken);

        public virtual bool CanExecute()
        {
            return true;
        }

        public async Task Execute()
        {
            Exception? error = null;
            ResetCancel();

            Executing = true;
            Service.StateHasChanged();

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
            Service.StateHasChanged(error);
        }
    }

    public abstract class CommandLongRunningAsync<S> : CommandAsync<S>, ICommandAsync where S : IRXService
    {
        protected CommandLongRunningAsync(S service) : base(service)
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

    public abstract class CommandAsync<S, T> : CommandAsyncBase<S>, ICommandAsync<T> where S : IRXService
    {
        public T? Parameter { get; set; }
        public Func<ICommandAsync<T>, CancellationToken, Task<bool>>? PrepareExecutionAsync { get; set; }

        protected CommandAsync(S service) : base(service)
        {
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
            Service.StateHasChanged();

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
            Service.StateHasChanged(error);
        }
    }

    public abstract class CommandLongRunningAsync<S, T> : CommandAsync<S, T>, ICommandAsync<T> where S : IRXService
    {
        protected CommandLongRunningAsync(S service) : base(service)
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
