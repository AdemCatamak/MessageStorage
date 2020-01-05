using System;
using System.Threading.Tasks;

namespace MessageStorage
{
    public abstract class Handler
    {
        public abstract Task Handle(object payload);

        public string Name => GetType().FullName;
    }

    /// <summary>
    /// Handle method is executed when MessageDispatcher get Message which has payload is compatible type of T
    /// </summary>
    /// <typeparam name="T">T is payload type</typeparam>
    public abstract class Handler<T> : Handler where T : class
    {
        public abstract Task Handle(T payload);

        public override Task Handle(object payload)
        {
            if (!(payload is T t))
            {
                throw new ArgumentException($"{payload.GetType().Name} could not converted {nameof(T)}");
            }

            return Handle(t);
        }
    }
}