using System;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.Exceptions;

namespace MessageStorage
{
    public abstract class Handler
    {
        public abstract Task BaseHandleOperationAsync(object payload, CancellationToken cancellationToken);

        public virtual string Name => GetType().FullName;

        public virtual Type PayloadType()
        {
            return typeof(object);
        }
    }

    public abstract class Handler<T> : Handler
    {
        protected abstract Task HandleAsync(T payload, CancellationToken cancellationToken);

        public override Task BaseHandleOperationAsync(object payload, CancellationToken cancellationToken)
        {
            object val = payload;
            try
            {
                Type typeT = typeof(T);
                if (typeT == typeof(short))
                {
                    val = Convert.ToInt16(payload.ToString());
                }

                if (typeT == typeof(int))
                {
                    val = Convert.ToInt32(payload.ToString());
                }
            }
            catch
            {
                // ignore
            }

            if (!(val is T t))
            {
                throw new ArgumentNotCompatibleException(payload.GetType().Name, nameof(T));
            }

            return HandleAsync(t, cancellationToken);
        }

        public override Type PayloadType()
        {
            return typeof(T);
        }
    }
}