using MessageStorage.DataAccessLayer;
using MessageStorage.DataAccessLayer.Repositories;
using MessageStorage.Exceptions;
using MessageStorage.SqlServer.DbClient;
using MessageStorage.SqlServer.Repositories;

namespace MessageStorage.SqlServer
{
    public class SqlServerRepositoryFactory : ISqlServerRepositoryFactory
    {
        public RepositoryConfiguration RepositoryConfiguration { get; }

        public SqlServerRepositoryFactory(RepositoryConfiguration repositoryConfiguration)
        {
            RepositoryConfiguration = repositoryConfiguration;
        }

        IMessageStorageConnection IRepositoryFactory.CreateConnection()
        {
            return CreateConnection();
        }

        public IMessageRepository CreateMessageRepository()
        {
            var sqlServerMessageRepository = new SqlServerMessageRepository(this);
            return sqlServerMessageRepository;
        }

        public IMessageRepository CreateMessageRepository(IMessageStorageTransaction messageStorageTransaction)
        {
            if (!(messageStorageTransaction is ISqlServerMessageStorageTransaction sqlServerMessageStorageTransaction))
            {
                throw new ArgumentNotCompatibleException(messageStorageTransaction.GetType(), typeof(ISqlServerMessageStorageTransaction));
            }

            var sqlServerMessageRepository = new SqlServerMessageRepository(this, sqlServerMessageStorageTransaction);
            return sqlServerMessageRepository;
        }

        public IJobRepository CreateJobRepository()
        {
            var sqlServerJobRepository = new SqlServerJobRepository(this);
            return sqlServerJobRepository;
        }

        public IJobRepository CreateJobRepository(IMessageStorageTransaction messageStorageTransaction)
        {
            if (!(messageStorageTransaction is ISqlServerMessageStorageTransaction sqlServerMessageStorageTransaction))
            {
                throw new ArgumentNotCompatibleException(messageStorageTransaction.GetType(), typeof(ISqlServerMessageStorageTransaction));
            }

            var sqlServerJobRepository = new SqlServerJobRepository(this, sqlServerMessageStorageTransaction);
            return sqlServerJobRepository;
        }

        public ISqlServerMessageStorageConnection CreateConnection()
        {
            return new SqlServerMessageStorageConnection(RepositoryConfiguration.ConnectionString);
        }
    }
}