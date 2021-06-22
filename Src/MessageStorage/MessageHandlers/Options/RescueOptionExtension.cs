using System;

namespace MessageStorage.MessageHandlers.Options
{
    public static class RescueOptionExtension
    {
        public static void UseRescue(this MessageHandlerMetadata messageHandlerMetadata, TimeSpan maxExecutionTime)
        {
            var rescueOption = new RescueOption(messageHandlerMetadata, maxExecutionTime);
            messageHandlerMetadata.UseRescue(rescueOption);
        }
    }
}