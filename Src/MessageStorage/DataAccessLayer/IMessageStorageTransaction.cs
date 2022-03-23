using System;
using System.Threading;
using System.Threading.Tasks;

namespace MessageStorage.DataAccessLayer;

public interface IMessageStorageTransaction : IDisposable
{
    public bool IsCommitted { get; }
    public bool IsDisposed { get; }
    Task CommitAsync(CancellationToken cancellationToken = default);

    internal void AddJobToBeDispatched(Job job);
}