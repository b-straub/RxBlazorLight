
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reactive.Linq;

namespace RxBlazorLightCore
{
    public class RxServiceCollector
    {
        public IEnumerable<IRxBLService> Services => _services;

        private readonly List<IRxBLService> _services = [];

        public IEnumerable<Type> ServiceTypes => _serviceTypes;

        private readonly List<Type> _serviceTypes = [];

        public void AddService<T>(T service) where T : IRxBLService
        {
            _services.Add(service);
        }

        public void AddServiceType(Type type)
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
            collector.AddService(service);

            return services;
        }

        public static IServiceCollection AddRxBLService<T>(this IServiceCollection services, RxServiceCollector collector, Func<IServiceProvider, T> serviceFactory) where T : RxBLService
        {
            collector.AddServiceType(typeof(T));

            services.AddSingleton(sp =>
            {
                var service = serviceFactory(sp);
                collector.AddService(service);
                return service;
            });

            return services;
        }
    }
}
