using MessageStorage.DataAccessLayer;
using MessageStorage.DataAccessLayer.Repositories;
using MessageStorage.Exceptions;
using MessageStorage.MySql.DbClient;
using MessageStorage.MySql.Repositories;

namespace MessageStorage.MySql
{
    public class MySqlRepositoryFactory : IMySqlRepositoryFactory
    {
        public RepositoryConfiguration RepositoryConfiguration { get; }

        public MySqlRepositoryFactory(RepositoryConfiguration repositoryConfiguration)
        {
            RepositoryConfiguration = repositoryConfiguration;
        }

        IMessageStorageConnection IRepositoryFactory.CreateConnection()
        {
            return CreateConnection();
        }

        public IMessageRepository CreateMessageRepository()
        {
            var mySqlMessageRepository = new MySqlMessageRepository(this);
            return mySqlMessageRepository;
        }

        public IMessageRepository CreateMessageRepository(IMessageStorageTransaction messageStorageTransaction)
        {
            if (!(messageStorageTransaction is IMySqlMessageStorageTransaction mySqlMessageStorageTransaction))
            {
                throw new ArgumentNotCompatibleException(messageStorageTransaction.GetType().FullName, typeof(IMySqlMessageStorageTransaction).FullName);
            }

            var mySqlMessageRepository = new MySqlMessageRepository(this, mySqlMessageStorageTransaction);
            return mySqlMessageRepository;
        }

        public IJobRepository CreateJobRepository()
        {
            var mySqlJobRepository = new MySqlJobRepository(this);
            return mySqlJobRepository;
        }

        public IJobRepository CreateJobRepository(IMessageStorageTransaction messageStorageTransaction)
        {
            if (!(messageStorageTransaction is IMySqlMessageStorageTransaction mySqlMessageStorageTransaction))
            {
                throw new ArgumentNotCompatibleException(messageStorageTransaction.GetType().FullName, typeof(IMySqlMessageStorageTransaction).FullName);
            }

            var mySqlJobRepository = new MySqlJobRepository(this, mySqlMessageStorageTransaction);
            return mySqlJobRepository;
        }

        public IMySqlMessageStorageConnection CreateConnection()
        {
            return new MySqlMessageStorageConnection(RepositoryConfiguration.ConnectionString);
        }
    }
}