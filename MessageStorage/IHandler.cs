using System;
using System.Threading.Tasks;

namespace MessageStorage
{
    public interface IHandler
    {
        Task Handle(object payload);

        string Name { get; }
    }

    public abstract class Handler<T> : IHandler where T : class
    {
        protected abstract Task Handle(T payload);

        public Task Handle(object payload)
        {
            if (!(payload is T t))
            {
                throw new ArgumentException($"{payload.GetType().Name} could not converted {nameof(T)}");
            }

            return Handle(t);
        }

        public string Name => GetType().FullName;
    }
}