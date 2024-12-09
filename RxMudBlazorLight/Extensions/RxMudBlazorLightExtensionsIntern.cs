using MudBlazor;

namespace RxMudBlazorLight.Extensions
{
    internal static class RxMudBlazorLightExtensionsIntern
    {
        public static string GetCancelIcon(this MbIconVariant? iconVariant)
        {
            var icon = iconVariant switch
            {
                MbIconVariant.FILLED => Icons.Material.Filled.Cancel,
                MbIconVariant.OUTLINED => Icons.Material.Outlined.Cancel,
                MbIconVariant.SHARP => Icons.Material.Sharp.Cancel,
                MbIconVariant.ROUNDED => Icons.Material.Rounded.Cancel,
                MbIconVariant.TWO_TONE => Icons.Material.TwoTone.Cancel,
                _ => Icons.Material.Outlined.Cancel
            };

            return icon;
        }

        public static string GetProgressIcon(this MbIconVariant? iconVariant)
        {
            var icon = iconVariant switch
            {
                MbIconVariant.FILLED => Icons.Material.Filled.Refresh,
                MbIconVariant.OUTLINED => Icons.Material.Outlined.Refresh,
                MbIconVariant.SHARP => Icons.Material.Sharp.Refresh,
                MbIconVariant.ROUNDED => Icons.Material.Rounded.Refresh,
                MbIconVariant.TWO_TONE => Icons.Material.TwoTone.Refresh,
                _ => Icons.Material.Filled.Refresh
            };

            return icon;
        }
    }
}
