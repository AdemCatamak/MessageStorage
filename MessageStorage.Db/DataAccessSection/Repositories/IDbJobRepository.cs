using MessageStorage.DataAccessSection.Repositories;
using MessageStorage.Db.Configurations;
using MessageStorage.Db.DataAccessSection.Repositories.BaseRepository;
using MessageStorage.Models;

namespace MessageStorage.Db.DataAccessSection.Repositories
{
    public interface IDbJobRepository<TDbRepositoryConfiguration>
        : IJobRepository<TDbRepositoryConfiguration>, IDbRepository<TDbRepositoryConfiguration, Job>
        where TDbRepositoryConfiguration : DbRepositoryConfiguration
    {
    }
}