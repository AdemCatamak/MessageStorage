using System;

namespace MessageStorage.MessageHandlers;

public record MessageHandlerMetaData(Type MessageHandlerType, Type MessageType, int MaxRetryCount = 10)
{
    public Type MessageHandlerType { get; } = MessageHandlerType;
    public Type MessageType { get; } = MessageType;
    public int MaxRetryCount { get; } = MaxRetryCount;
}