using System.Data;
using MessageStorage.Db.Configurations;
using MessageStorage.Db.DataAccessSection.Repositories.BaseRepository.Imp;
using MessageStorage.Models;

namespace MessageStorage.Db.DataAccessSection.Repositories.Imp
{
    public abstract class DbMessageRepository<TDbRepositoryConfiguration>
        : DbRepository<TDbRepositoryConfiguration, Message>,
          IDbMessageRepository<TDbRepositoryConfiguration>
        where TDbRepositoryConfiguration : DbRepositoryConfiguration
    {
        protected DbMessageRepository(IDbConnection dbConnection, TDbRepositoryConfiguration dbRepositoryConfiguration) : base(dbConnection, dbRepositoryConfiguration)
        {
        }
    }
}