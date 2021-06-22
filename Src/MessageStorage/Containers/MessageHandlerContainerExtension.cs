using System.Collections.Generic;
using MessageStorage.MessageHandlers;

namespace MessageStorage.Containers
{
    public static class MessageHandlerContainerExtension
    {
        public static IMessageHandlerProvider BuildMessageHandlerProvider(this MessageHandlerContainer messageHandlerContainer)
        {
            List<MessageHandlerMetadata> clone = new List<MessageHandlerMetadata>();
            clone.AddRange(messageHandlerContainer.MessageHandlerDescriptions);

            IMessageHandlerProvider messageHandlerProvider = new MessageHandlerProvider(clone);
            return messageHandlerProvider;
        }
    }
}