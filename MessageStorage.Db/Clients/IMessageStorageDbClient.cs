using System;
using System.Data;
using MessageStorage.Clients;

namespace MessageStorage.Db.Clients
{
    public interface IMessageStorageDbClient : IMessageStorageClient, IDisposable
    {
        void UseTransaction(IDbTransaction dbTransaction);
        IDbTransaction BeginTransaction();
        void RemoveTransaction();
    }
}