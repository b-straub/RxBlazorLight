
namespace RxBlazorLightCore
{
    public partial class RxBLServiceBase
    {
        #region ICommand
        protected static ICommand CreateCommand(IRXService service, Action execute, Func<bool>? canExecute = null)
        {
            return new Command(service, execute, canExecute);
        }
        #endregion

        #region ICommand<T>
        protected static ICommand<T> CreateCommand<T>(IRXService service, Action<T?> execute, Func<T?, bool>? canExecute = null)
        {
            return new Command<T>(service, execute, canExecute);
        }
        #endregion

        #region ICommandAsync
        protected static ICommandAsync CreateAsyncCommand(IRXService service, Func<Task> execute)
        {
            return new CommandAsync(service, execute, null, null, false);
        }

        protected static ICommandAsync CreateAsyncCommand(IRXService service, Func<Task> execute, bool hasProgress)
        {
            return new CommandAsync(service, execute, null, null, hasProgress);
        }

        protected static ICommandAsync CreateAsyncCommand(IRXService service, Func<Task> execute, Func<bool>? canExecute)
        {
            return new CommandAsync(service, execute, canExecute, null, false);
        }

        protected static ICommandAsync CreateAsyncCommand(IRXService service, Func<Task> execute, Func<bool>? canExecute, bool hasProgress)
        {
            return new CommandAsync(service, execute, canExecute, null, hasProgress);
        }

        protected static ICommandAsync CreateAsyncCommand(IRXService service, Func<Task> execute, Func<bool>? canExecute, Action? cancel)
        {
            return new CommandAsync(service, execute, canExecute, cancel, false);
        }

        protected static ICommandAsync CreateAsyncCommand(IRXService service, Func<Task> execute, Func<bool>? canExecute, Action? cancel, bool hasProgress)
        {
            return new CommandAsync(service, execute, canExecute, cancel, hasProgress);
        }
        #endregion

        #region ICommandAsync<T>
        protected static ICommandAsync<T> CreateAsyncCommand<T>(IRXService service, Func<T?, Task> execute)
        {
            return new CommandAsync<T>(service, execute, null, null, false);
        }

        protected static ICommandAsync<T> CreateAsyncCommand<T>(IRXService service, Func<T?, Task> execute, bool hasProgress)
        {
            return new CommandAsync<T>(service, execute, null, null, hasProgress);
        }

        protected static ICommandAsync<T> CreateAsyncCommand<T>(IRXService service, Func<T?, Task> execute, Func<T?, bool>? canExecute)
        {
            return new CommandAsync<T>(service, execute, canExecute, null, false);
        }

        protected static ICommandAsync<T> CreateAsyncCommand<T>(IRXService service, Func<T?, Task> execute, Func<T?, bool>? canExecute, bool hasProgress)
        {
            return new CommandAsync<T>(service, execute, canExecute, null, hasProgress);
        }

        protected static ICommandAsync<T> CreateAsyncCommand<T>(IRXService service, Func<T?, Task> execute, Func<T?, bool>? canExecute, Action? cancel)
        {
            return new CommandAsync<T>(service, execute, canExecute, cancel, false);
        }

        protected static ICommandAsync<T> CreateAsyncCommand<T>(IRXService service, Func<T?, Task> execute, Func<T?, bool>? canExecute, Action? cancel, bool hasProgress)
        {
            return new CommandAsync<T>(service, execute, canExecute, cancel, hasProgress);
        }
        #endregion
    }
}
