using MessageStorage.Models.Base;

namespace MessageStorage.DataAccessSection.Repositories.Base
{
    public interface IRepository<in TEntity> where TEntity : Entity
    {
        void Add(TEntity entity);
    }
}