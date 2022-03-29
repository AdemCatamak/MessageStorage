using System.Threading;
using System.Threading.Tasks;
using MessageStorage.Exceptions;

namespace MessageStorage.MessageHandlers;

public abstract class BaseMessageHandler<TMessage> : IMessageHandler where TMessage : class
{
    public Task BaseHandleOperationAsync(IMessageContext<object> messageContext, CancellationToken cancellationToken)
    {
        TMessage? t = Convert(messageContext.Message);
        var typedMessageContext = new MessageContext<TMessage>(messageContext.JobId, t);
        return HandleAsync(typedMessageContext, cancellationToken);
    }

    protected abstract Task HandleAsync(IMessageContext<TMessage> messageContext, CancellationToken cancellationToken);

    private static TMessage Convert(object payload)
    {
        if (payload is not TMessage t)
        {
            throw new ArgumentNotCompatibleException(payload.GetType(), typeof(TMessage));
        }

        return t;
    }
}