using System;
using System.Threading.Tasks;
using MessageStorage.Exceptions;

namespace MessageStorage
{
    public abstract class Handler
    {
        public abstract Task Handle(object payload);

        public virtual string Name => GetType().FullName;
    }

    public abstract class Handler<T> : Handler
    {
        protected abstract Task Handle(T payload);

        public override Task Handle(object payload)
        {
            if (!(payload is T t))
            {
                throw new ArgumentNotCompatibleException($"{payload.GetType().Name} could not converted {nameof(T)}");
            }

            return Handle(t);
        }
    }
}