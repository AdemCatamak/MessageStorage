using MessageStorage.Extensions;

namespace MessageStorage.Integration.MassTransit;

public static class MessageStorageOptionsExtension
{
    public static MessageStorageOptionsFor<TMessageStorageClient> RegisterMassTransitMessageHandlers<TMessageStorageClient>(this MessageStorageOptionsFor<TMessageStorageClient> messageStorageOptionsBuilder)
        where TMessageStorageClient : IMessageStorageClient
    {
        messageStorageOptionsBuilder.RegisterHandler<IntegrationEventHandler, IIntegrationEvent>();
        messageStorageOptionsBuilder.RegisterHandler<IntegrationCommandHandler, IIntegrationCommand>();
        return messageStorageOptionsBuilder;
    }
}