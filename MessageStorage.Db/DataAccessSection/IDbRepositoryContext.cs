using System.Data;
using MessageStorage.DataAccessSection;
using MessageStorage.Db.Configurations;
using MessageStorage.Db.DataAccessSection.Repositories;

namespace MessageStorage.Db.DataAccessSection
{
    public interface IDbRepositoryContext<TDbRepositoryConfiguration> :
        IRepositoryContext<TDbRepositoryConfiguration>
        where TDbRepositoryConfiguration : DbRepositoryConfiguration
    {
        TDbRepositoryConfiguration DbRepositoryConfiguration { get; }

        IDbMessageRepository<TDbRepositoryConfiguration> DbMessageRepository { get; }
        IDbJobRepository<TDbRepositoryConfiguration> DbJobRepository { get; }
        
        IDbTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
        void UseTransaction(IDbTransaction dbTransaction);
        void ClearTransaction();
        bool HasTransaction { get; }

    }
}