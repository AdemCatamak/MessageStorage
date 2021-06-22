using MessageStorage.DataAccessLayer;
using MessageStorage.DataAccessLayer.Repositories;
using MessageStorage.Exceptions;
using MessageStorage.Postgres.DbClient;
using MessageStorage.Postgres.Repositories;

namespace MessageStorage.Postgres
{
    public class PostgresRepositoryFactory : IPostgresRepositoryFactory
    {
        public RepositoryConfiguration RepositoryConfiguration { get; }

        public PostgresRepositoryFactory(RepositoryConfiguration repositoryConfiguration)
        {
            RepositoryConfiguration = repositoryConfiguration;
        }

        IMessageStorageConnection IRepositoryFactory.CreateConnection()
        {
            return CreateConnection();
        }

        public IMessageRepository CreateMessageRepository()
        {
            var postgresMessageRepository = new PostgresMessageRepository(this);
            return postgresMessageRepository;
        }

        public IMessageRepository CreateMessageRepository(IMessageStorageTransaction messageStorageTransaction)
        {
            if (!(messageStorageTransaction is IPostgresMessageStorageTransaction postgresMessageStorageTransaction))
            {
                throw new ArgumentNotCompatibleException(messageStorageTransaction.GetType().FullName, typeof(IPostgresMessageStorageTransaction).FullName);
            }

            var postgresMessageRepository = new PostgresMessageRepository(this, postgresMessageStorageTransaction);
            return postgresMessageRepository;
        }

        public IJobRepository CreateJobRepository()
        {
            var postgresJobRepository = new PostgresJobRepository(this);
            return postgresJobRepository;
        }

        public IJobRepository CreateJobRepository(IMessageStorageTransaction messageStorageTransaction)
        {
            if (!(messageStorageTransaction is IPostgresMessageStorageTransaction postgresMessageStorageTransaction))
            {
                throw new ArgumentNotCompatibleException(messageStorageTransaction.GetType().FullName, typeof(IPostgresMessageStorageTransaction).FullName);
            }

            var postgresJobRepository = new PostgresJobRepository(this, postgresMessageStorageTransaction);
            return postgresJobRepository;
        }

        public IPostgresMessageStorageConnection CreateConnection()
        {
            return new PostgresMessageStorageConnection(RepositoryConfiguration.ConnectionString);
        }
    }
}