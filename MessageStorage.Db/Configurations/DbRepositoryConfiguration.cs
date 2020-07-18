using System;
using MessageStorage.Configurations;

namespace MessageStorage.Db.Configurations
{
    public class DbRepositoryConfiguration : RepositoryConfiguration
    {
        public string ConnectionString { get; protected set; }
        public string Schema { get; protected set; } = "MessageStorage";

        public DbRepositoryConfiguration(string connectionString, string schema = null)
        {
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            Schema = schema ?? Schema;
        }
    }
}