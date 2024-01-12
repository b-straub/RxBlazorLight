
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace RxBlazorLightCore
{
    public static class ServiceExtensions
    {
        public static void AddRxBLService<T>(this IServiceCollection services, double sampleMS = 100) where T : RxBLServiceBase
        {
            services.AddCascadingValue(sp =>
            {
                var service = sp.GetRequiredService<T>();
                return service.CreateCascadingValueSource(sampleMS);
            });
        }

        public static void AddRxBLService<T>(this IServiceCollection services, Func<IServiceProvider, T> serviceFactory, double sampleMS = 100) where T : RxBLServiceBase
        {
            services.AddCascadingValue(sp =>
            {
                var service = serviceFactory(sp);
                return service.CreateCascadingValueSource(sampleMS);
            });
        }

        private static CascadingValueSource<T> CreateCascadingValueSource<T>(this T service, double sampleMS) where T : RxBLServiceBase
        {
            var source = new CascadingValueSource<T>(service, false);
            service.Subscribe(() => source.NotifyChangedAsync(), sampleMS);
            return source;
        }
    }
}
