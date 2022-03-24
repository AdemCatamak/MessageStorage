using MessageStorage.Integration.MassTransit;
using MessageStorage.MessageHandlers;

namespace FooManagement.MessageHandlers;

public record FooCreatedEvent(string Id, string StrValue);

public record FooCreatedIntegrationEvent(string Id, string StrValue) : IIntegrationEvent;

public class LogFooCreatedMessageHandler : BaseMessageHandler<FooCreatedEvent>
{
    private readonly ILogger<LogFooCreatedMessageHandler> _logger;

    public LogFooCreatedMessageHandler(ILogger<LogFooCreatedMessageHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleAsync(IMessageContext<FooCreatedEvent> messageContext, CancellationToken cancellationToken)
    {
        FooCreatedEvent fooCreatedEvent = messageContext.Message;
        _logger.LogInformation("{EventName} : {Message}", nameof(FooCreatedEvent), fooCreatedEvent);
        return Task.CompletedTask;
    }
}