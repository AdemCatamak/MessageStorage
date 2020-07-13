using MessageStorage.DataAccessSection.Repositories;
using MessageStorage.Db.Configurations;
using MessageStorage.Db.DataAccessSection.Repositories.BaseRepository;
using MessageStorage.Models;

namespace MessageStorage.Db.DataAccessSection.Repositories
{
    public interface IDbMessageRepository<TDbRepositoryConfiguration>
        : IMessageRepository<TDbRepositoryConfiguration>, IDbRepository<TDbRepositoryConfiguration, Message>
        where TDbRepositoryConfiguration : DbRepositoryConfiguration
    {
    }
}