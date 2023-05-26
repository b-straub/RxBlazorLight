
namespace RxBlazorLightCore
{
    internal class CommandBase
    {
        protected IRXService Service { get; }

        protected CommandBase(IRXService service)
        {
            Service = service;
        }
    }

    internal class Command : CommandBase, ICommand
    {
        private readonly Func<bool>? _canExecute;
        private readonly Action _execute;

        public Command(IRXService service, Action execute, Func<bool>? canExecute) : base(service)
        {
            _canExecute = canExecute;
            _execute = execute;
        }

        public Command(Command other) : base(other.Service)
        {
            _canExecute = other._canExecute;
            _execute = other._execute;
        }

        public ICommand Clone()
        {
            return new Command(this);
        }

        public bool CanExecute()
        {
            return _canExecute is null || _canExecute();
        }

        public void Execute()
        {
            Exception? error = null;

            try
            {
                _execute();
            }
            catch (Exception ex)
            {
                if (ex.GetType() != typeof(TaskCanceledException))
                {
                    error = ex;
                }
            }

            Service.StateHasChanged(error);
        }
    }

    internal class Command<T> : CommandBase, ICommand<T>
    {
        private readonly Func<T?, bool>? _canExecute;
        private readonly Action<T?> _execute;
        private T? _parameter;

        public Command(IRXService service, Action<T?> execute, Func<T?, bool>? canExecute) : base(service)
        {
            _canExecute = canExecute;
            _execute = execute;
        }

        public Command(Command<T> other) : base(other.Service)
        {
            _canExecute = other._canExecute;
            _execute = other._execute;
        }

        public ICommand<T> Clone()
        {
            return new Command<T>(this);
        }

        public void SetParameter(T? parameter)
        {
            _parameter = parameter;
        }

        public bool CanExecute()
        {
            return _canExecute is null || _canExecute(_parameter);
        }

        public void Execute()
        {
            Exception? error = null;

            try
            {
                _execute(_parameter);
            }
            catch (Exception ex)
            {
                if (ex.GetType() != typeof(TaskCanceledException))
                {
                    error = ex;
                }
            }

            Service.StateHasChanged(error);
        }
    }

    internal class CommandAsyncBase : CommandBase
    {
        public bool Executing { get; protected set; }
        public bool CanCancel => _cancel is not null;
        public bool HasProgress { get; protected set; }
        public CancellationToken CancellationToken => _cancelTokenSource.Token;

        private CancellationTokenSource _cancelTokenSource = new();
        private readonly Action? _cancel;

        protected CommandAsyncBase(IRXService service, Action? Cancel, bool hasProgress) : base(service)
        {
            Executing = false;
            HasProgress = hasProgress;
            _cancel = Cancel;
        }

        public void Cancel()
        {
            _cancelTokenSource.Cancel();

            if (_cancel is not null)
            {
                _cancel();
                Executing = false;
                Service.StateHasChanged();
            }
        }

        protected void ResetCancel()
        {
            if (!_cancelTokenSource.TryReset())
            {
                _cancelTokenSource = new();
            }
        }
    }

    internal class CommandAsync : CommandAsyncBase, ICommandAsync, ICommandAsyncBase
    {
        private readonly Func<bool>? _canExecute;
        private readonly Func<Task> _execute;

        public CommandAsync(IRXService service, Func<Task> execute, Func<bool>? canExecute, Action? cancel, bool hasProgress) : base(service, cancel, hasProgress)
        {
            _canExecute = canExecute;
            _execute = execute;
        }

        public CommandAsync(CommandAsync other) : base(other.Service, other.Cancel, other.HasProgress)
        {
            _canExecute = other._canExecute;
            _execute = other._execute;
        }

        public ICommandAsync Clone()
        {
            return new CommandAsync(this);
        }

        public bool CanExecute()
        {
            return _canExecute is null || _canExecute();
        }

        public async Task Execute()
        {
            Exception? error = null;
            ResetCancel();

            Executing = true;
            Service.StateHasChanged();

            try
            {
                await _execute();
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

    internal class CommandAsync<T> : CommandAsyncBase, ICommandAsync<T>, ICommandAsyncBase
    {
        private readonly Func<T?, bool>? _canExecute;
        private readonly Func<T?, Task> _execute;
        private T? _parameter;
        private Func<T?, Task<T?>>? _transformation;

        public CommandAsync(IRXService service, Func<T?, Task> execute, Func<T?, bool>? canExecute, Action? cancel, bool hasProgress) : base(service, cancel, hasProgress)
        {
            _canExecute = canExecute;
            _execute = execute;
        }

        public CommandAsync(CommandAsync<T> other) : base(other.Service, other.Cancel, other.HasProgress)
        {
            _canExecute = other._canExecute;
            _execute = other._execute;
        }

        public ICommandAsync<T> Clone()
        {
            return new CommandAsync<T>(this);
        }

        public void SetParameter(T? parameter)
        {
            _parameter = parameter;
        }

        public void SetParameterAsyncTransformation(Func<T?, Task<T?>>? transformation)
        {
            _transformation = transformation;
        }

        public bool CanExecute()
        {
            return _canExecute is null || _canExecute(_parameter);
        }

        public async Task Execute()
        {
            Exception? error = null;
            ResetCancel();

             Executing = true;
            Service.StateHasChanged();

            try
            {                  
                if (_transformation is not null)
                {
                    _parameter = await _transformation(_parameter);
                }

                await _execute(_parameter);
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
}
