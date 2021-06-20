using System;

namespace MessageStorage.MessageHandlers.Options
{
    public class RetryOption
    {
        public MessageHandlerMetadata MessageHandlerMetadata { get; private set; }
        public int RetryCount { get; private set; }
        public TimeSpan DeferTime { get; private set; }

        internal RetryOption(MessageHandlerMetadata messageHandlerMetadata, int retryCount, TimeSpan deferTime)
        {
            MessageHandlerMetadata = messageHandlerMetadata;
            DeferTime = deferTime;
            RetryCount = retryCount;
        }
    }
}