using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MessageStorage.MessageHandlers;

namespace MessageStorage.Integration.MassTransit
{
    public class IntegrationCommandHandler : BaseMessageHandler<IIntegrationCommand>
    {
        private readonly IBusControl _busControl;

        public IntegrationCommandHandler(IBusControl busControl)
        {
            _busControl = busControl;
        }

        public override async Task HandleAsync(IIntegrationCommand payload, CancellationToken cancellationToken = default)
        {
            await _busControl.Send(payload, payload.GetType(), cancellationToken: cancellationToken);
        }
    }
}