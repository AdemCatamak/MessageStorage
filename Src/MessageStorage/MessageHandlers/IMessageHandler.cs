using System.Threading;
using System.Threading.Tasks;

namespace MessageStorage.MessageHandlers;

internal interface IMessageHandler
{
    Task BaseHandleOperationAsync(IMessageContext<object> messageContext, CancellationToken cancellationToken);
}