using System;
using System.Collections.Generic;

namespace MessageStorage.MessageHandlers;

public interface IMessageHandlerMetaDataHolderFor<TMessageStorageClient>
    where TMessageStorageClient : IMessageStorageClient
{
    IReadOnlyList<MessageHandlerMetaData> MetaDataCollection { get; }
    IReadOnlyList<MessageHandlerMetaData> GetAvailableMessageHandlerMetaDataCollection(Type messageType);
}