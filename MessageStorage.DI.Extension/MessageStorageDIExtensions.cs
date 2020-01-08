using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace MessageStorage.DI.Extension
{
    public static class MessageStorageDIExtensions
    {
        public static IMessageStorageServiceCollection AddMessageStorage(this IServiceCollection serviceCollection, Action<IMessageStorageServiceCollection> builder)
        {
            IMessageStorageServiceCollection messageStorageServiceCollection = new MessageStorageServiceCollection(serviceCollection);
            builder.Invoke(messageStorageServiceCollection);

            return messageStorageServiceCollection;
        }

        public static IServiceCollection AddHandlers(this IServiceCollection serviceCollection, IEnumerable<Assembly> assemblies)
        {
            IEnumerable<Type> handlerTypes = assemblies.SelectMany(s => s.GetTypes())
                                                       .Where(p => typeof(Handler).IsAssignableFrom(p));


            foreach (Type handlerType in handlerTypes)
            {
                serviceCollection.Add(new ServiceDescriptor(typeof(Handler), handlerType, ServiceLifetime.Singleton));
            }

            return serviceCollection;
        }
    }
}