using System.Data;
using MessageStorage.Db.Configurations;
using MessageStorage.Db.DataAccessSection.Repositories.BaseRepository.Imp;
using MessageStorage.Models;

namespace MessageStorage.Db.DataAccessSection.Repositories.Imp
{
    public abstract class MessageDbRepository : DbRepository<Message>,
                                                IMessageDbRepository
    {
        protected MessageDbRepository(IDbConnection dbConnection, DbRepositoryConfiguration dbRepositoryConfiguration) : base(dbConnection, dbRepositoryConfiguration)
        {
        }
    }
}