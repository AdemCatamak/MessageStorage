using System.Threading;
using System.Threading.Tasks;
using MessageStorage.Exceptions;

namespace MessageStorage
{
    public abstract class Handler
    {
        public abstract Task BaseHandleOperationAsync(object payload, CancellationToken cancellationToken);

        public virtual string Name => GetType().FullName;
    }

    public abstract class Handler<T> : Handler
    {
        protected abstract Task HandleAsync(T payload, CancellationToken cancellationToken);

        public override Task BaseHandleOperationAsync(object payload, CancellationToken cancellationToken)
        {
            if (!(payload is T t))
            {
                throw new ArgumentNotCompatibleException(payload.GetType().Name, nameof(T));
            }

            return HandleAsync(t, cancellationToken);
        }
    }
}