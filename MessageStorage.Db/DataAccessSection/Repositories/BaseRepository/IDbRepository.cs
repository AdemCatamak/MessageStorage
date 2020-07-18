using System.Data;
using MessageStorage.DataAccessSection.Repositories.Base;
using MessageStorage.Models.Base;

namespace MessageStorage.Db.DataAccessSection.Repositories.BaseRepository
{
    public interface IDbRepository<in TEntity>
        : IRepository<TEntity>
        where TEntity : Entity
    {
        void UseTransaction(IDbTransaction dbTransaction);
        void ClearTransaction();
        bool HasTransaction { get; }
    }
}