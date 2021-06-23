using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer;

namespace MessageStorage
{
    public interface IMessageStorageClient
    {
        void UseTransaction(IMessageStorageTransaction messageStorageTransaction);
        Task<(Message, IEnumerable<Job>)> AddMessageAsync(object payload, CancellationToken cancellationToken = default);
        Task<(Message, IEnumerable<Job>)> AddMessageAsync(object payload, IMessageStorageTransaction messageStorageTransaction, CancellationToken cancellationToken = default);
    }
}