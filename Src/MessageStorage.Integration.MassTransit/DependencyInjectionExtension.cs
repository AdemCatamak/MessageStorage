using System;
using MessageStorage.DependencyInjection;
using MessageStorage.MessageHandlers;

namespace MessageStorage.Integration.MassTransit
{
    public static class DependencyInjectionExtension
    {
        public static IMessageStorageDependencyConfigurator RegisterMassTransitMessageHandlers(this IMessageStorageDependencyConfigurator configurator, Action<MessageHandlerMetadata>? configureMessageHandlerMetadata = null)
        {
            configurator.Register<IntegrationCommandHandler>(configureMessageHandlerMetadata);
            configurator.Register<IntegrationEventHandler>(configureMessageHandlerMetadata);
            return configurator;
        }
    }
}