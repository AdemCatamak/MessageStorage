using System;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.MessageHandlers;

namespace MessageStorage.SqlServer.IntegrationTest.Fixtures.MessageHandlers;

public record ThrowExMessage(string ExceptionMessage);

public class ThrowExMessageHandler : BaseMessageHandler<ThrowExMessage>
{
    protected override Task HandleAsync(IMessageContext<ThrowExMessage> messageContext, CancellationToken cancellationToken = default)
    {
        throw new ArgumentException(messageContext.Message.ExceptionMessage);
    }
}