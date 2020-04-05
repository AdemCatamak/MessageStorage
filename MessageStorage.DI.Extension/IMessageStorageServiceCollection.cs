using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace MessageStorage.DI.Extension
{
    public interface IMessageStorageServiceCollection
    {
        IMessageStorageServiceCollection AddHandlerManager();

        IMessageStorageServiceCollection AddHandlerManager<THandlerManager>()
            where THandlerManager : class, IHandlerManager;

        IMessageStorageServiceCollection AddHandlers(IEnumerable<Assembly> assemblies);

        IMessageStorageServiceCollection AddJobProcessServer();

        IMessageStorageServiceCollection AddJobProcessServer<TJobProcessServer>()
            where TJobProcessServer : class, IJobProcessServer;

        IMessageStorageServiceCollection Add<T>(ServiceLifetime serviceLifetime, Func<IServiceProvider, T> factory)
            where T : class;
    }

    public class MessageStorageServiceCollection : IMessageStorageServiceCollection
    {
        protected readonly IServiceCollection ServiceCollection;

        public MessageStorageServiceCollection(IServiceCollection serviceCollection)
        {
            ServiceCollection = serviceCollection;
        }

        public IMessageStorageServiceCollection AddHandlerManager()
        {
            return AddHandlerManager<HandlerManager>();
        }

        public IMessageStorageServiceCollection AddHandlerManager<THandlerManager>()
            where THandlerManager : class, IHandlerManager
        {
            ServiceCollection.AddSingleton<IHandlerManager, THandlerManager>();
            ServiceCollection.AddSingleton<THandlerManager, THandlerManager>();

            return this;
        }

        public IMessageStorageServiceCollection AddHandlers(IEnumerable<Assembly> assemblies)
        {
            IEnumerable<Type> handlerTypes = assemblies.SelectMany(s => s.GetTypes())
                                                       .Where(p => typeof(Handler).IsAssignableFrom(p));


            foreach (Type handlerType in handlerTypes)
            {
                ServiceCollection.Add(new ServiceDescriptor(typeof(Handler), handlerType, ServiceLifetime.Singleton));
                ServiceCollection.Add(new ServiceDescriptor(handlerType, handlerType, ServiceLifetime.Singleton));
            }

            return this;
        }

        public IMessageStorageServiceCollection AddJobProcessServer()
        {
            return AddJobProcessServer<JobProcessServer>();
        }

        public IMessageStorageServiceCollection AddJobProcessServer<TJobProcessServer>()
            where TJobProcessServer : class, IJobProcessServer
        {
            ServiceCollection.AddSingleton<IJobProcessServer, TJobProcessServer>();
            ServiceCollection.AddSingleton<TJobProcessServer, TJobProcessServer>();

            return this;
        }

        public IMessageStorageServiceCollection Add<T>(ServiceLifetime serviceLifetime, Func<IServiceProvider, T> factory)
            where T : class
        {
            switch (serviceLifetime)
            {
                case ServiceLifetime.Singleton:
                    ServiceCollection.AddSingleton<T>(factory);
                    break;
                case ServiceLifetime.Scoped:
                    ServiceCollection.AddScoped<T>(factory);
                    break;
                case ServiceLifetime.Transient:
                    ServiceCollection.AddTransient(factory);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(serviceLifetime), serviceLifetime, null);
            }

            return this;
        }
    }
}