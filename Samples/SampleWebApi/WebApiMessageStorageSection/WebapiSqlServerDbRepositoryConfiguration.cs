using MessageStorage.Db.Configurations;

namespace SampleWebApi.WebApiMessageStorageSection
{
    public class WebapiSqlServerDbRepositoryConfiguration : DbRepositoryConfiguration
    {
        public WebapiSqlServerDbRepositoryConfiguration(string connectionStr)
        {
            ConnectionString = connectionStr;
        }
    }
}