using MessageStorage.Configurations;
using MessageStorage.Models.Base;

namespace MessageStorage.DataAccessSection.Repositories.Base
{
    public interface IRepository<out TRepositoryConfiguration, in TEntity>
        where TRepositoryConfiguration : RepositoryConfiguration
        where TEntity : Entity
    {
        TRepositoryConfiguration RepositoryConfiguration { get; }
        void Add(TEntity entity);
    }
}