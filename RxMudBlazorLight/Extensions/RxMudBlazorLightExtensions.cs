using MudBlazor;

namespace RxMudBlazorLight.Extensions
{
    internal static class RxMudBlazorLightExtensions
    {
        public static string GetCancelIcon(this MBIconVariant? iconVariant)
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
    }
}
