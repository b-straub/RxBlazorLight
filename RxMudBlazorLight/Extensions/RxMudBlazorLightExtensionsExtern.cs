using MudBlazor;

namespace RxMudBlazorLight.Extensions
{
    public static class RxMudBlazorLightExtensionsExtern
    {
        public static bool OK(this DialogResult? result)
        {
            return result is not null && !result.Canceled;
        }
    }
}
