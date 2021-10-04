using MessageStorage.DataAccessLayer;
using MessageStorage.MySql.DbClient;

namespace MessageStorage.MySql
{
    public interface IMySqlRepositoryFactory : IRepositoryFactory
    {
        new IMySqlMessageStorageConnection CreateConnection();
    }
}