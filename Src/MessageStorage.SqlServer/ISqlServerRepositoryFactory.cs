using MessageStorage.DataAccessLayer;
using MessageStorage.SqlServer.DbClient;

namespace MessageStorage.SqlServer
{
    public interface ISqlServerRepositoryFactory : IRepositoryFactory
    {
        new ISqlServerMessageStorageConnection CreateConnection();
    }
}