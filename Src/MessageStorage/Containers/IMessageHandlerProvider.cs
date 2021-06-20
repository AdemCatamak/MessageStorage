using System;
using System.Collections.Generic;
using MessageStorage.MessageHandlers;

namespace MessageStorage.Containers
{
    public interface IMessageHandlerProvider
    {
        IMessageHandler Create(string messageHandlerTypeName);
        IEnumerable<MessageHandlerMetadata> GetCompatibleMessageHandler(Type payloadType);
    }
}