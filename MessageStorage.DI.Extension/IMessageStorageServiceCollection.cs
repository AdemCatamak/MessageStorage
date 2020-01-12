using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace MessageStorage.DI.Extension
{
    public interface IMessageStorageServiceCollection
    {
        IMessageStorageServiceCollection AddHandlerManager<T>(ServiceLifetime serviceLifetime) where T : IHandlerManager;
        IMessageStorageServiceCollection AddHandlers(IEnumerable<Assembly> assemblies);

        IMessageStorageServiceCollection AddMessageProcessServer(ServiceLifetime serviceLifetime = ServiceLifetime.Singleton);
        void Add<T>(Func<IServiceProvider, T> messageStorageServiceCollection, ServiceLifetime lifetime) where T : class;
    }

    public class MessageStorageServiceCollection : IMessageStorageServiceCollection
    {
        private readonly IServiceCollection _serviceCollection;

        public MessageStorageServiceCollection(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }


        public IMessageStorageServiceCollection AddHandlerManager<T>(ServiceLifetime serviceLifetime) where T : IHandlerManager
        {
            _serviceCollection.Add(new ServiceDescriptor(typeof(IHandlerManager), typeof(T), serviceLifetime));
            _serviceCollection.Add(new ServiceDescriptor(typeof(T), typeof(T), serviceLifetime));
            return this;
        }

        public IMessageStorageServiceCollection AddHandlers(IEnumerable<Assembly> assemblies)
        {
            IEnumerable<Type> handlerTypes = assemblies.SelectMany(s => s.GetTypes())
                                                       .Where(p => typeof(Handler).IsAssignableFrom(p));


            foreach (Type handlerType in handlerTypes)
            {
                _serviceCollection.Add(new ServiceDescriptor(typeof(Handler), handlerType, ServiceLifetime.Singleton));
            }

            return this;
        }

        public IMessageStorageServiceCollection AddMessageProcessServer(ServiceLifetime serviceLifetime)
        {
            _serviceCollection.Add(new ServiceDescriptor(typeof(IJobServer), typeof(JobServer), serviceLifetime));
            _serviceCollection.Add(new ServiceDescriptor(typeof(JobServer), typeof(JobServer), serviceLifetime));
            return this;
        }

        public void Add<T>(Func<IServiceProvider, T> provider, ServiceLifetime lifetime) where T : class
        {
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    _serviceCollection.AddSingleton<T>(provider);
                    break;
                case ServiceLifetime.Scoped:
                    _serviceCollection.AddScoped<T>(provider);
                    break;
                case ServiceLifetime.Transient:
                    _serviceCollection.AddTransient<T>(provider);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
            }
        }
    }
}