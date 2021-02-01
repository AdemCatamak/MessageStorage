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
                    if(short.TryParse(payload.ToString(), out short v))
                    {
                        val = v;
                    }
                }

                if (typeT == typeof(int))
                {
                    if(int.TryParse(payload.ToString(), out int v))
                    {
                        val = v;
                    }
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