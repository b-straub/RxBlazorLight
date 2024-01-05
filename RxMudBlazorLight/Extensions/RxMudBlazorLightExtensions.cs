using MudBlazor;
using RxBlazorLightCore;
using RxMudBlazorLight.ButtonBase;

namespace RxMudBlazorLight.Extensions
{
    public static class RxMudBlazorLightExtensions
    {
        internal static string GetCancelIcon(this MBIconVariant? iconVariant)
        {
            var icon = iconVariant switch
            {
                MBIconVariant.Filled => Icons.Material.Filled.Cancel,
                MBIconVariant.Outlined => Icons.Material.Outlined.Cancel,
                MBIconVariant.Sharp => Icons.Material.Sharp.Cancel,
                MBIconVariant.Rounded => Icons.Material.Rounded.Cancel,
                MBIconVariant.TwoTone => Icons.Material.TwoTone.Cancel,
                _ => Icons.Material.Outlined.Cancel
            };

            return icon;
        }

        public static string GetProgressIcon(this MBIconVariant? iconVariant)
        {
            var icon = iconVariant switch
            {
                MBIconVariant.Filled => Icons.Material.Filled.Refresh,
                MBIconVariant.Outlined => Icons.Material.Outlined.Refresh,
                MBIconVariant.Sharp => Icons.Material.Sharp.Refresh,
                MBIconVariant.Rounded => Icons.Material.Rounded.Refresh,
                MBIconVariant.TwoTone => Icons.Material.TwoTone.Refresh,
                _ => Icons.Material.Filled.Refresh
            };

            return icon;
        }

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
    }
}
