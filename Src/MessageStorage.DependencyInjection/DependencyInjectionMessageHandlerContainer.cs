using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MessageStorage.Containers;
using MessageStorage.Exceptions;
using MessageStorage.MessageHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace MessageStorage.DependencyInjection
{
    public class DependencyInjectionMessageHandlerContainer
        : IMessageHandlerContainer
    {
        private readonly IServiceCollection _serviceCollection;

        public DependencyInjectionMessageHandlerContainer(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        public void Register<TMessageHandler>(Action<MessageHandlerMetadata>? configureMessageHandlerMetadata = null) where TMessageHandler : class, IMessageHandler
        {
            Register(typeof(TMessageHandler), configureMessageHandlerMetadata);
        }

        public void Register(params Assembly[] assemblies)
        {
            Register(null, assemblies);
        }

        public void Register(Action<MessageHandlerMetadata>? configureMessageHandlerMetadata, params Assembly[] assemblies)
        {
            var messageHandlerTypes = assemblies.SelectMany(a => a.GetTypes())
                                                .Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Contains(typeof(IMessageHandler)))
                                                .ToList();

            foreach (Type messageHandlerType in messageHandlerTypes)
            {
                Register(messageHandlerType, configureMessageHandlerMetadata);
            }
        }

        public void Register(Type messageHandlerType, Action<MessageHandlerMetadata>? configureMessageHandlerMetadata)
        {
            if (!typeof(IMessageHandler).IsAssignableFrom(messageHandlerType))
            {
                throw new ArgumentNotCompatibleException(messageHandlerType.FullName ?? string.Empty, typeof(IMessageHandler).FullName);
            }

            var payloadTypes = GetPayloadTypes(messageHandlerType);
            MessageHandlerMetadata messageHandlerMetadata = new MessageHandlerMetadata(messageHandlerType, payloadTypes);
            configureMessageHandlerMetadata?.Invoke(messageHandlerMetadata);

            _serviceCollection.AddSingleton(messageHandlerMetadata);

            _serviceCollection.AddTransient(typeof(IMessageHandler), messageHandlerType);
            _serviceCollection.AddTransient(messageHandlerType);
        }

        private static List<Type> GetPayloadTypes(Type messageHandlerType)
        {
            List<Type> payloadTypes = new List<Type>();
            foreach (Type interfaceType in messageHandlerType.GetInterfaces())
            {
                if (interfaceType.IsGenericType
                 && interfaceType.GetGenericTypeDefinition() == typeof(IMessageHandler<>))
                {
                    payloadTypes.Add(interfaceType.GetGenericArguments()[0]);
                }
            }

            return payloadTypes.Distinct().ToList();
        }
    }
}