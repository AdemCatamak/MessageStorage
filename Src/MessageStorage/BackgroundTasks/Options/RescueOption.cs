using System;
using MessageStorage.MessageHandlers;

namespace MessageStorage.BackgroundTasks.Options
{
    public class RescueOption
    {
        public MessageHandlerMetadata MessageHandlerMetadata { get; private set; }
        public TimeSpan MaxExecutionTime { get; private set; }

        public RescueOption(MessageHandlerMetadata messageHandlerMetadata, TimeSpan maxExecutionTime)
        {
            MessageHandlerMetadata = messageHandlerMetadata;
            MaxExecutionTime = maxExecutionTime;
        }
    }
}