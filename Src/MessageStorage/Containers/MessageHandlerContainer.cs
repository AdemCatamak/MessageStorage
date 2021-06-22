using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MessageStorage.Exceptions;
using MessageStorage.MessageHandlers;

namespace MessageStorage.Containers
{
    public class MessageHandlerContainer : IMessageHandlerContainer
    {
        public IReadOnlyCollection<MessageHandlerMetadata> MessageHandlerDescriptions
            => _messageHandlerAndMessageHandlerDescriptions.Values.ToList().AsReadOnly();

        private readonly ConcurrentDictionary<string, MessageHandlerMetadata> _messageHandlerAndMessageHandlerDescriptions
            = new ConcurrentDictionary<string, MessageHandlerMetadata>();

        public void Register<TMessageHandler>(Action<MessageHandlerMetadata>? configureMessageHandlerMetadata = null) where TMessageHandler : class, IMessageHandler
        {
            Type messageHandlerType = typeof(TMessageHandler);
            Register(messageHandlerType, configureMessageHandlerMetadata);
        }

        public void Register(params Assembly[] assemblies)
        {
            Register(null, assemblies);
        }

        public void Register(Action<MessageHandlerMetadata>? configureMessageHandlerMetadata, params Assembly[] assemblies)
        {
            var messageHandlerTypes = assemblies.SelectMany(a => a.GetTypes())
                                                .Where(t => t.IsClass && !t.IsAbstract)
                                                .SelectMany(t => t.GetInterfaces())
                                                .Where(i => typeof(IMessageHandler) == i)
                                                .ToList();

            foreach (Type messageHandlerType in messageHandlerTypes)
            {
                Register(messageHandlerType, configureMessageHandlerMetadata);
            }
        }

        public void Register(Type messageHandlerType, Action<MessageHandlerMetadata>? configureMessageHandlerMetadata = null)
        {
            if (!typeof(IMessageHandler).IsAssignableFrom(messageHandlerType))
            {
                throw new ArgumentNotCompatibleException(messageHandlerType.FullName ?? string.Empty, nameof(IMessageHandler));
            }

            List<Type> payloadTypes = GetPayloadTypes(messageHandlerType);

            MessageHandlerMetadata messageHandlerMetadata = new MessageHandlerMetadata(messageHandlerType, payloadTypes);
            configureMessageHandlerMetadata?.Invoke(messageHandlerMetadata);

            bool added = _messageHandlerAndMessageHandlerDescriptions.TryAdd(messageHandlerType.FullName, messageHandlerMetadata);
            if (!added)
            {
                throw new DuplicateDependencyInjectionException(messageHandlerType);
            }
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