using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MessageStorage.MessageHandlers;

namespace MessageStorage.Integration.MassTransit;

public class IntegrationCommandHandler : BaseMessageHandler<IIntegrationCommand>
{
    private readonly IBusControl _busControl;

    public IntegrationCommandHandler(IBusControl busControl)
    {
        _busControl = busControl;
    }

    protected override async Task HandleAsync(IMessageContext<IIntegrationCommand> messageContext, CancellationToken cancellationToken)
    {
        IIntegrationCommand? message = messageContext.Message;
        await _busControl.Send(message, message.GetType(), cancellationToken);
    }
}