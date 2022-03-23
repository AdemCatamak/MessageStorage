using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer;
using MessageStorage.Processor;

namespace MessageStorage;

public interface IMessageStorageClient
{
    internal IRepositoryContext RepositoryContext { get; }
    
    void UseTransaction(IMessageStorageTransaction messageStorageTransaction);
    IMessageStorageTransaction StartTransaction();

    Task<(Message, List<Job>)> AddMessageAsync(object payload, CancellationToken cancellationToken = default);
}