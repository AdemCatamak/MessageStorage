using System.Threading.Tasks;
using MassTransit;

namespace MessageStorage.Integration.MassTransit.IntegrationTest.Fixtures.Consumers;

public record BasicIntegrationCommand(string Guid) : IIntegrationCommand;

public class BasicIntegrationCommandConsumer : IConsumer<BasicIntegrationCommand>
{
    public Task Consume(ConsumeContext<BasicIntegrationCommand> context)
    {
        BasicIntegrationCommand? basicIntegrationCommand = context.Message;
        ConsumedMessageContainer.ConsumedMessageId.Add(basicIntegrationCommand.Guid);
        return Task.CompletedTask;
    }
}

public class BasicIntegrationCommandConsumerDefinition : ConsumerDefinition<BasicIntegrationCommandConsumer>
{
    public static string QueueName = "basic-integration-command-consumer";

    public BasicIntegrationCommandConsumerDefinition()
    {
        Endpoint(configurator => { configurator.Name = QueueName; });
    }
}