using System;
using System.Collections.Generic;
using System.Linq;

namespace MessageStorage.MessageHandlers;

internal class MessageHandlerMetaDataHolderFor<TMessageStorageClient> : IMessageHandlerMetaDataHolderFor<TMessageStorageClient>
    where TMessageStorageClient : IMessageStorageClient
{
    public IReadOnlyList<MessageHandlerMetaData> MetaDataCollection { get; }

    public MessageHandlerMetaDataHolderFor(List<MessageHandlerMetaData> messageHandlerMetaDataList)
    {
        MetaDataCollection = messageHandlerMetaDataList;
    }

    public IReadOnlyList<MessageHandlerMetaData> GetAvailableMessageHandlerMetaDataCollection(Type messageType)
    {
        List<MessageHandlerMetaData> availableMetaDataList = MetaDataCollection.Where(m => m.MessageType.IsAssignableFrom(messageType))
                                                                               .ToList();
        return availableMetaDataList;
    }
}