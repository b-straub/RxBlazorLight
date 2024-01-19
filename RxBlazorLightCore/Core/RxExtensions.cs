
using System.Reactive;
using System.Reactive.Linq;

namespace RxBlazorLightCore
{
    public static class RxExtensions
    {
        public static bool Preparing(this ICommandBase commandBase)
        {
            return commandBase.State is CommandState.PREPARING;
        }

        public static bool Executing(this ICommandBase commandBase)
        {
            return commandBase.State is CommandState.EXECUTING;
        }
        public static bool Executed(this ICommandBase commandBase)
        {
            return commandBase.State is CommandState.EXECUTED;
        }
        public static bool NotExecuted(this ICommandBase commandBase)
        {
            return commandBase.State is CommandState.NOT_EXECUTED;
        }

        public static bool Canceled(this ICommandBase commandBase)
        {
            return commandBase.State is CommandState.CANCELED;
        }

        public static bool Exception(this ICommandBase commandBase)
        {
            return commandBase.State is CommandState.EXCEPTION;
        }

        public static bool Running(this ICommandBase commandBase)
        {
            return commandBase.State is CommandState.PREPARING || commandBase.State is CommandState.EXECUTING;
        }

        public static bool Changing<T>(this IInput<T> input)
        {
            return input.State is InputState.CHANGING;
        }
        public static bool Changed<T>(this IInput<T> input)
        {
            return input.State is InputState.CHANGED;
        }
    }
}
