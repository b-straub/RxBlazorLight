
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace RxBlazorLightCore
{
    public class RxServiceCollector
    {
        public IEnumerable<IRxBLService> Services => _services;

        private readonly List<IRxBLService> _services = [];

        public void AddService<T>(T service) where T : IRxBLService
        {
            _services.Add(service);
        }
    }

    public static class ServiceExtensions
    {
        public static IServiceCollection AddRxBLService<T>(this IServiceCollection services, double sampleMS = 100) where T : RxBLService, new()
        {
            services.TryAddSingleton<RxServiceCollector>();

            services.AddCascadingValue(sp =>
            {
                var collector = sp.GetRequiredService<RxServiceCollector>();
                var service = new T();
                collector.AddService(service);
                return service.CreateCascadingValueSource(sampleMS);
            });

            return services;
        }

        public static IServiceCollection AddRxBLService<T>(this IServiceCollection services, Func<System.IServiceProvider, T> serviceFactory, double sampleMS = 100) where T : RxBLService
        {
            services.TryAddSingleton<RxServiceCollector>();

            services.AddCascadingValue(sp =>
            {
                var collector = sp.GetRequiredService<RxServiceCollector>();
                var service = serviceFactory(sp);
                collector.AddService(service);
                return service.CreateCascadingValueSource(sampleMS);
            });

            return services;
        }

        private static CascadingValueSource<T> CreateCascadingValueSource<T>(this T service, double sampleMS) where T : IRxBLService
        {
            var source = new CascadingValueSource<T>(service, false);
            service.Subscribe(_ => source.NotifyChangedAsync(), sampleMS);
            return source;
        }
    }
}
