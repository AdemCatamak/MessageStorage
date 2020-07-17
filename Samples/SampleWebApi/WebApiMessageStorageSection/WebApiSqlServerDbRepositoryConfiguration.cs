using MessageStorage.Db.Configurations;

namespace SampleWebApi.WebApiMessageStorageSection
{
    public class WebApiSqlServerDbRepositoryConfiguration : DbRepositoryConfiguration
    {
        public WebApiSqlServerDbRepositoryConfiguration(string connectionStr)
        {
            ConnectionString = connectionStr;
        }
    }
}