using System;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.MessageHandlers;

namespace MessageStorage.SqlServer.IntegrationTest.Fixtures.MessageHandlers;

public record BasicMessage(string Message);

public class BasicMessageHandler : BaseMessageHandler<BasicMessage>
{
    protected override Task HandleAsync(IMessageContext<BasicMessage> messageContext, CancellationToken cancellationToken)
    {
        BasicMessage? payload = messageContext.Message;
        Console.WriteLine($"{DateTime.UtcNow} | {payload.Message}");
        return Task.CompletedTask;
    }
}