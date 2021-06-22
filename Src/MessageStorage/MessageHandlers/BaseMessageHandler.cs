using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.Exceptions;

namespace MessageStorage.MessageHandlers
{
    public abstract class BaseMessageHandler<T> : IMessageHandler<T>
    {
        public IEnumerable<Type> PayloadTypes => new[] {typeof(T)};

        public Task BaseHandleOperationAsync(object payload, CancellationToken cancellationToken = default)
        {
            T t = Convert(payload);
            return HandleAsync(t, cancellationToken);
        }

        public abstract Task HandleAsync(T payload, CancellationToken cancellationToken = default);

        public void Dispose()
        {
        }

        private static T Convert(object payload)
        {
            object val = payload;
            try
            {
                Type typeT = typeof(T);
                if (typeT == typeof(short))
                {
                    if (short.TryParse(payload.ToString(), out short v))
                    {
                        val = v;
                    }
                }

                if (typeT == typeof(int))
                {
                    if (int.TryParse(payload.ToString(), out int v))
                    {
                        val = v;
                    }
                }

                if (typeT == typeof(long))
                {
                    if (long.TryParse(payload.ToString(), out long v))
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

            return t;
        }
    }
}