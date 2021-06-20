using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MessageStorage.MessageHandlers
{
    public interface IMessageHandler
        : IDisposable
    {
        public IEnumerable<Type> PayloadTypes { get; }
        Task BaseHandleOperationAsync(object payload, CancellationToken cancellationToken = default);
    }

    public interface IMessageHandler<in T>
        : IMessageHandler
    {
        Task HandleAsync(T payload, CancellationToken cancellationToken = default);
    }
}