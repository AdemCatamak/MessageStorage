using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MessageStorage.MessageHandlers;

namespace MessageStorage.Integration.MassTransit;

public class IntegrationEventHandler : BaseMessageHandler<IIntegrationEvent>
{
    private readonly IBusControl _busControl;

    public IntegrationEventHandler(IBusControl busControl)
    {
        _busControl = busControl;
    }

    protected override async Task HandleAsync(IMessageContext<IIntegrationEvent> messageContext, CancellationToken cancellationToken)
    {
        IIntegrationEvent? message = messageContext.Message;
        var jobId = messageContext.JobId;
        await _busControl.Publish(message, message.GetType(), context => { context.Headers.Set(HeaderConstant.JOB_ID_HEADER, jobId); }, cancellationToken);
    }
}