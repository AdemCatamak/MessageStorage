using MessageStorage.Db.Configurations;

namespace AccountWebApi.AccountApiMessageStorageSection
{
    public class AccountApiSqlServerDbRepositoryConfiguration : DbRepositoryConfiguration
    {
        public AccountApiSqlServerDbRepositoryConfiguration(string connectionStr)
        {
            ConnectionString = connectionStr;
        }
    }
}