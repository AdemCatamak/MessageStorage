using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer;

namespace MessageStorage;

public interface IMessageStorageClient : IDisposable
{
    internal IRepositoryContext RepositoryContext { get; }
    
    void UseTransaction(IMessageStorageTransaction messageStorageTransaction);
    IMessageStorageTransaction StartTransaction();

    Task<(Message, List<Job>)> AddMessageAsync(object payload, CancellationToken cancellationToken = default);
}