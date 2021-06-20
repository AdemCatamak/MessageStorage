using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Fasterflect;
using MessageStorage.Exceptions;
using MessageStorage.MessageHandlers;

namespace MessageStorage.Containers
{
    public class MessageHandlerProvider : IMessageHandlerProvider
    {
        private readonly ConcurrentDictionary<string, MessageHandlerMetadata> _messageHandlerAndMessageHandlerDescriptions;

        internal MessageHandlerProvider(IEnumerable<MessageHandlerMetadata> messageHandlerDescriptions)
        {
            _messageHandlerAndMessageHandlerDescriptions = new ConcurrentDictionary<string, MessageHandlerMetadata>();
            foreach (MessageHandlerMetadata messageHandlerDescription in messageHandlerDescriptions)
            {
                _messageHandlerAndMessageHandlerDescriptions.TryAdd(messageHandlerDescription.MessageHandlerType.FullName, messageHandlerDescription);
            }
        }


        public IMessageHandler Create(string messageHandlerTypeName)
        {
            _messageHandlerAndMessageHandlerDescriptions.TryGetValue(messageHandlerTypeName, out MessageHandlerMetadata? messageHandlerDescription);
            if (messageHandlerDescription == null)
            {
                throw new MessageHandlerDescriptionNotFoundException(messageHandlerTypeName);
            }

            object? result;
            try
            {
                result = messageHandlerDescription.MessageHandlerType.CreateInstance();
            }
            catch (Exception e)
            {
                throw new MessageHandlerCreationException(messageHandlerTypeName, e);
            }

            IMessageHandler messageHandler = result as IMessageHandler ?? throw new MessageHandlerCreationException(messageHandlerTypeName);

            return messageHandler;
        }

        public IEnumerable<MessageHandlerMetadata> GetCompatibleMessageHandler(Type payloadType)
        {
            var messageHandlerMetadataList
                = _messageHandlerAndMessageHandlerDescriptions.Values
                                                              .Where(d => d.PayloadTypes.Any(pt => pt.IsAssignableFrom(payloadType)))
                                                              .ToList();

            var distinctMetadataCollection = from metadata in messageHandlerMetadataList
                                             group metadata by metadata.MessageHandlerTypeName
                                             into g
                                             select g.First();

            return distinctMetadataCollection;
        }
    }
}