using System.Data;
using MessageStorage.Db.Configurations;
using MessageStorage.Db.DataAccessSection.Repositories.BaseRepository.Imp;
using MessageStorage.Models;

namespace MessageStorage.Db.DataAccessSection.Repositories.Imp
{
    public abstract class DbMessageRepository : DbRepository<Message>,
                                                IDbMessageRepository
    {
        protected DbMessageRepository(IDbConnection dbConnection, DbRepositoryConfiguration dbRepositoryConfiguration) : base(dbConnection, dbRepositoryConfiguration)
        {
        }
    }
}