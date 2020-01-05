using System;
using System.Collections.Generic;
using System.Reflection;
using MessageStorage.HandlerFactorySection;
using MessageStorage.JobServerSection;
using MessageStorage.MessageStorageClientSection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MessageStorage.DI.Extension
{
    public static class DIExtensions
    {
        public static IServiceCollection AddMessageProcessServer(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddSingleton<IJobServer, JobServer>();
            return serviceCollection;
        }

        public static IServiceCollection AddMessageStorageClient<T>(this IServiceCollection serviceCollection, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped) where T : class, IMessageStorageAdaptor
        {
            serviceCollection.TryAdd(new ServiceDescriptor(typeof(IMessageStorageAdaptor), typeof(T), serviceLifetime));
            serviceCollection.TryAdd(new ServiceDescriptor(typeof(IMessageStorageClient), typeof(MessageStorageClient), serviceLifetime));
            serviceCollection.TryAdd(new ServiceDescriptor(typeof(IHandlerFactory), typeof(HandlerFactory), serviceLifetime));
            return serviceCollection;
        }

        public static IServiceCollection AddHandlers(this IServiceCollection serviceCollection, IEnumerable<Assembly> assemblies)
        {
            foreach (Assembly assembly in assemblies)
            {
                var types = assembly.DefinedTypes;
                foreach (TypeInfo typeInfo in types)
                {
                    if (!typeInfo.IsClass || typeInfo.IsAbstract)
                        continue;

                    Type cType = typeInfo;
                    do
                    {
                        var match = BaseTypeCheck(cType);
                        if (match)
                            serviceCollection.TryAddSingleton(typeof(Handler), typeInfo);
                        cType = cType.BaseType;
                    } while (cType != null);
                }
            }

            bool BaseTypeCheck(Type type)
            {
                if (type.BaseType != null && (type.BaseType.IsGenericType && type.BaseType.GetGenericTypeDefinition() == typeof(Handler<>).GetGenericTypeDefinition()))
                {
                    return true;
                }

                return false;
            }

            return serviceCollection;
        }
    }
}