using MessageStorage.Configurations;

namespace MessageStorage.Db.Configurations
{
    public abstract class DbRepositoryConfiguration : RepositoryConfiguration
    {
        public string ConnectionString { get; protected set; }
        public string Schema { get; protected set;} = "MessageStorage";
    }
}