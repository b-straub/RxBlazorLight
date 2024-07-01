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
        
        public static bool TryGet<T>(this DialogResult? result, [NotNullWhen(true)] out T? data)
        {
            data = default;
            
            if (result is not null && !result.Canceled)
            {
                data = (T?)result.Data;
            }

            return data is not null;
        }
    }
}
