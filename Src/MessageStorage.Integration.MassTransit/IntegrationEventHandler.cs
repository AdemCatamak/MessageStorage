using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MessageStorage.MessageHandlers;

namespace MessageStorage.Integration.MassTransit
{
    public class IntegrationEventHandler : BaseMessageHandler<IIntegrationEvent>
    {
        private readonly IBusControl _busControl;

        public IntegrationEventHandler(IBusControl busControl)
        {
            _busControl = busControl;
        }

        public override async Task HandleAsync(IIntegrationEvent payload, CancellationToken cancellationToken = default)
        {
            await _busControl.Publish(payload, payload.GetType(), cancellationToken);
        }
    }
}