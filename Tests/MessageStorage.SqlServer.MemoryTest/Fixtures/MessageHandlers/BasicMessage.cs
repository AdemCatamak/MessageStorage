using System;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.MessageHandlers;
using Microsoft.Extensions.Logging;

namespace MessageStorage.SqlServer.MemoryTest.Fixtures.MessageHandlers;

public record BasicMessage(string Message);

public class BasicMessageHandler : BaseMessageHandler<BasicMessage>
{
    private readonly ILogger<BasicMessageHandler> _logger;

    public BasicMessageHandler(ILogger<BasicMessageHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleAsync(IMessageContext<BasicMessage> messageContext, CancellationToken cancellationToken)
    {
        BasicMessage payload = messageContext.Message;
        _logger.LogInformation("{DateTime} | {Message}", DateTime.UtcNow, payload.Message);
        return Task.CompletedTask;
    }
}