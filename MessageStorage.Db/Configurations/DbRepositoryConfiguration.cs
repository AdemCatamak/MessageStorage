using MessageStorage.Configurations;

namespace MessageStorage.Db.Configurations
{
    public class DbRepositoryConfiguration : RepositoryConfiguration
    {
        public string ConnectionString { get; protected set; }
        public string Schema { get; protected set; } = "MessageStorage";

        public DbRepositoryConfiguration SetConnectionString(string connectionString)
        {
            ConnectionString = connectionString;
            return this;
        }
    }
}