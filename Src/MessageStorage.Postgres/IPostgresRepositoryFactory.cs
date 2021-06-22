using MessageStorage.DataAccessLayer;
using MessageStorage.Postgres.DbClient;

namespace MessageStorage.Postgres
{
    public interface IPostgresRepositoryFactory : IRepositoryFactory
    {
        new IPostgresMessageStorageConnection CreateConnection();
    }
}