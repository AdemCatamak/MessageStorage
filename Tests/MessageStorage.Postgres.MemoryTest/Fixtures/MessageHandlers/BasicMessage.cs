using System;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.MessageHandlers;

namespace MessageStorage.Postgres.MemoryTest.Fixtures.MessageHandlers;

public record BasicMessage(string Message);

public class BasicMessageHandler : BaseMessageHandler<BasicMessage>
{
    protected override Task HandleAsync(IMessageContext<BasicMessage> messageContext, CancellationToken cancellationToken = default)
    {
        BasicMessage basicMessage = messageContext.Message;
        Console.WriteLine($"{DateTime.UtcNow} | {basicMessage.Message}");
        return Task.CompletedTask;
    }
}