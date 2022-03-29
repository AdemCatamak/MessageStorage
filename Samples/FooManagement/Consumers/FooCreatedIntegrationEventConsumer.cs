using FooManagement.MessageHandlers;
using MassTransit;

namespace FooManagement.Consumers;

public class FooCreatedIntegrationEventConsumer : IConsumer<FooCreatedIntegrationEvent>
{
    private readonly ILogger<FooCreatedIntegrationEventConsumer> _logger;

    public FooCreatedIntegrationEventConsumer(ILogger<FooCreatedIntegrationEventConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<FooCreatedIntegrationEvent> context)
    {
        FooCreatedIntegrationEvent? message = context.Message;
        _logger.LogInformation("{EventName} is consumed : {Event}", nameof(FooCreatedIntegrationEvent), message);
        
        return Task.CompletedTask;
    }
}