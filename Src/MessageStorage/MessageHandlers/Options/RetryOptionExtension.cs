using System;

namespace MessageStorage.MessageHandlers.Options
{
    public static class RetryOptionExtension
    {
        public static void UseRetry(this MessageHandlerMetadata messageHandlerMetadata, int retryCount)
        {
            UseRetry(messageHandlerMetadata, retryCount, TimeSpan.FromSeconds(180));
        }

        public static void UseRetry(this MessageHandlerMetadata messageHandlerMetadata, int retryCount, TimeSpan deferTime)
        {
            var retryOption = new RetryOption(messageHandlerMetadata, retryCount, deferTime);
            messageHandlerMetadata.UseRetry(retryOption);
        }
    }
}