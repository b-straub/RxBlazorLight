
using Microsoft.Extensions.DependencyInjection;

namespace RxBlazorLightCore
{
    public class RxServiceCollector
    {
        internal IEnumerable<IRxBLService> Services => _services;

        private readonly List<IRxBLService> _services = [];

        internal IEnumerable<Type> ServiceTypes => _serviceTypes;

        private readonly List<Type> _serviceTypes = [];

        internal void AddService(IRxBLService service)
        {
            _services.Add(service);
        }

        internal void AddServiceType(Type type)
        {
            _serviceTypes.Add(type);
        }
    }

    public static class ServiceExtensions
    {
        public static RxServiceCollector AddRxBLServiceCollector(this IServiceCollection services)
        {
            var collector = new RxServiceCollector();
            services.AddSingleton(collector);
            return collector;
        }

        public static IServiceCollection AddRxBLService<T>(this IServiceCollection services, RxServiceCollector collector) where T : RxBLService, new()
        {
            var service = new T();
            services.AddSingleton(service);
            return services;
        }

        public static IServiceCollection AddRxBLService<T>(this IServiceCollection services, RxServiceCollector collector, Func<IServiceProvider, T> serviceFactory) where T : RxBLService
        {
            collector.AddServiceType(typeof(T));

            services.AddSingleton(sp =>
            {
                var service = serviceFactory(sp);
                return service;
            });

            return services;
        }
    }
}
