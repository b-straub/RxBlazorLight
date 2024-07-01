using System.Diagnostics.CodeAnalysis;
using MudBlazor;

namespace RxMudBlazorLight.Extensions
{
    public static class RxMudBlazorLightExtensionsExtern
    {
        public static bool OK([NotNullWhen(true)] this DialogResult? result)
        {
            return result is not null && !result.Canceled;
        }
    }
}
