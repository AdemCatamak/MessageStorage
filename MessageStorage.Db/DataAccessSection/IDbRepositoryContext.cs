using System.Data;
using MessageStorage.DataAccessSection;
using MessageStorage.Db.DataAccessSection.Repositories;

namespace MessageStorage.Db.DataAccessSection
{
    public interface IDbRepositoryContext : IRepositoryContext
    {
        IDbMessageRepository DbMessageRepository { get; }
        IDbJobRepository DbJobRepository { get; }

        IDbTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
        void UseTransaction(IDbTransaction dbTransaction);
        void ClearTransaction();
        bool HasTransaction { get; }
    }
}