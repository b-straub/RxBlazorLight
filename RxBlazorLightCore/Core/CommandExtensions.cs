
using System.Reactive;
using System.Reactive.Linq;

namespace RxBlazorLightCore
{
    public static class CommandExtensions
    {
        public static bool Preparing(this ICommandBase commandBase)
        {
            return commandBase.State is CommandState.PREPARING;
        }

        public static IObservable<Unit> PreparingObservable(this ICommandBase commandBase)
        {
            return commandBase.Where(cs => cs is CommandState.PREPARING).Select(_ => Unit.Default);
        }

        public static bool Executing(this ICommandBase commandBase)
        {
            return commandBase.State is CommandState.EXECUTING;
        }

        public static IObservable<Unit> ExecutingObservable(this ICommandBase commandBase)
        {
            return commandBase.Where(cs => cs is CommandState.EXECUTING).Select(_ => Unit.Default);
        }

        public static bool Executed(this ICommandBase commandBase)
        {
            return commandBase.State is CommandState.EXECUTED;
        }

        public static IObservable<Unit> ExecutedObservable(this ICommandBase commandBase)
        {
            return commandBase.Where(cs => cs is CommandState.EXECUTED).Select(_ => Unit.Default);
        }

        public static bool NotExecuted(this ICommandBase commandBase)
        {
            return commandBase.State is CommandState.NOT_EXECUTED;
        }

        public static IObservable<Unit> NotExecutedObservable(this ICommandBase commandBase)
        {
            return commandBase.Where(cs => cs is CommandState.NOT_EXECUTED).Select(_ => Unit.Default);
        }

        public static bool Canceled(this ICommandBase commandBase)
        {
            return commandBase.State is CommandState.CANCELED;
        }

        public static IObservable<Unit> CanceledObservable(this ICommandBase commandBase)
        {
            return commandBase.Where(cs => cs is CommandState.CANCELED).Select(_ => Unit.Default);
        }

        public static bool Exception(this ICommandBase commandBase)
        {
            return commandBase.State is CommandState.EXCEPTION;
        }

        public static IObservable<Unit> ExceptionObservable(this ICommandBase commandBase)
        {
            return commandBase.Where(cs => cs is CommandState.EXCEPTION).Select(_ => Unit.Default);
        }

        public static bool Running(this ICommandBase commandBase)
        {
            return commandBase.State is CommandState.PREPARING || commandBase.State is CommandState.EXECUTING;
        }

        public static IObservable<Unit> RunningObservable(this ICommandBase commandBase)
        {
            return commandBase.Where(cs => cs is CommandState.PREPARING || cs is CommandState.EXECUTING).Select(_ => Unit.Default);
        }
    }
}
