using System;
using System.Collections.Generic;
using System.Linq;
using MessageStorage.Containers;
using MessageStorage.Exceptions;
using MessageStorage.MessageHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace MessageStorage.DependencyInjection
{
    public class DependencyInjectionMessageHandlerProvider
        : IMessageHandlerProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public DependencyInjectionMessageHandlerProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IEnumerable<MessageHandlerMetadata> GetCompatibleMessageHandler(Type payloadType)
        {
            var messageHandlerMetadataCollection = _serviceProvider.GetServices<MessageHandlerMetadata>();
            var compatibleMessageHandlerMetadataList = messageHandlerMetadataCollection.Where(handler => handler.PayloadTypes.Any(t => t.IsAssignableFrom(payloadType)))
                                                                                       .ToList();

            var distinctMetadataCollection = from metadata in compatibleMessageHandlerMetadataList
                                             group metadata by metadata.MessageHandlerTypeName
                                             into g
                                             select g.First();

            return distinctMetadataCollection;
        }

        public IMessageHandler Create(string messageHandlerTypeName)
        {
            IMessageHandler? messageHandler = _serviceProvider.GetServices<IMessageHandler>()
                                                              .FirstOrDefault(handler => handler.GetType().FullName == messageHandlerTypeName);

            if (messageHandler == null)
            {
                throw new MessageHandlerCreationException(messageHandlerTypeName);
            }

            return messageHandler;
        }
    }
}