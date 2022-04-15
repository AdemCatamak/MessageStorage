using System;
using System.Threading.Tasks;
using MassTransit;

namespace MessageStorage.Integration.MassTransit.IntegrationTest.Fixtures.Consumers;

public record BasicIntegrationEvent(string Guid) : IIntegrationEvent;

public class BasicIntegrationEventConsumer : IConsumer<BasicIntegrationEvent>
{
    public Task Consume(ConsumeContext<BasicIntegrationEvent> context)
    {
        BasicIntegrationEvent basicIntegrationEvent = context.Message;
        context.Headers.TryGetHeader("ms-job-id", out object? jobId);
        if (jobId == null || !Guid.TryParse(jobId?.ToString(), out Guid _))
        {
            throw new ApplicationException("JobId could not detected");
        }

        ConsumedMessageContainer.ConsumedMessageId.Add(basicIntegrationEvent.Guid);
        return Task.CompletedTask;
    }
}