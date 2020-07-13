using System.Data;
using MessageStorage.Clients;

namespace MessageStorage.Db.Clients
{
    public interface IMessageStorageDbClient
        : IMessageStorageClient
    {
        void UseTransaction(IDbTransaction dbTransaction);
        void ClearTransaction();
    }
}