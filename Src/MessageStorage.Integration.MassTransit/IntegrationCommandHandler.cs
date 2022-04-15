using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Middleware;
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
        string jobId = messageContext.JobId;
        await _busControl.Send(message, message.GetType(), Pipe.Execute<SendContext>(context => { context.Headers.Set(HeaderConstant.JOB_ID_HEADER, jobId); }), cancellationToken);
    }
}