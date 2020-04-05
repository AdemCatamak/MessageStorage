using System.Data;
using MessageStorage.DataAccessSection;

namespace MessageStorage.Db.DataAccessLayer
{
    public interface IDbRepository : IRepository
    {
    }

    public interface IDbRepository<in TEntity> : IDbRepository, IRepository<TEntity> where TEntity : class
    {
        void Add(TEntity entity, IDbTransaction dbTransaction);
    }
}