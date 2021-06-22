using System;
using System.Collections.Generic;
using MessageStorage.MessageHandlers.Options;

namespace MessageStorage.MessageHandlers
{
    public class MessageHandlerMetadata
    {
        public string MessageHandlerTypeName => MessageHandlerType.FullName ?? string.Empty;
        public Type MessageHandlerType { get; }
        public List<Type> PayloadTypes { get; }
        public RetryOption? RetryOption { get; private set; }
        public RescueOption RescueOption { get; private set; }

        public MessageHandlerMetadata(Type messageHandlerType, List<Type> payloadTypes)
        {
            MessageHandlerType = messageHandlerType;
            PayloadTypes = payloadTypes;
            RescueOption = new RescueOption(this, TimeSpan.FromMinutes(30));
        }

        public void UseRetry(RetryOption retryOption)
        {
            RetryOption = retryOption;
        }

        public void UseRescue(RescueOption rescueOption)
        {
            RescueOption = rescueOption;
        }
    }
}