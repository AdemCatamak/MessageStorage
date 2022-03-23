using System.Threading.Tasks;
using MassTransit;

namespace MessageStorage.Integration.MassTransit.IntegrationTest.Fixtures.Consumers;

public record BasicIntegrationEvent(string Guid) : IIntegrationEvent;

public class BasicIntegrationEventConsumer : IConsumer<BasicIntegrationEvent>
{
    public Task Consume(ConsumeContext<BasicIntegrationEvent> context)
    {
        BasicIntegrationEvent basicIntegrationEvent = context.Message;
        ConsumedMessageContainer.ConsumedMessageId.Add(basicIntegrationEvent.Guid);
        return Task.CompletedTask;
    }
}