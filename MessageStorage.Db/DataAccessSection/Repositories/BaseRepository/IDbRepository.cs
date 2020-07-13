using System.Data;
using MessageStorage.DataAccessSection.Repositories.Base;
using MessageStorage.Db.Configurations;
using MessageStorage.Models.Base;

namespace MessageStorage.Db.DataAccessSection.Repositories.BaseRepository
{
    public interface IDbRepository<TDbRepositoryConfiguration, TEntity>
        : IRepository<TDbRepositoryConfiguration, TEntity>
        where TDbRepositoryConfiguration : DbRepositoryConfiguration
        where TEntity : Entity
    {
        TDbRepositoryConfiguration DbRepositoryConfiguration { get; }

        void UseTransaction(IDbTransaction dbTransaction);
        void ClearTransaction();
        bool HasTransaction { get; }
    }
}