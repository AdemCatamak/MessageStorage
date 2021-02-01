using System.Data;
using MessageStorage.Configurations;
using MessageStorage.DataAccessSection;
using MessageStorage.DataAccessSection.Repositories;
using MessageStorage.SqlServer.DataAccessSection.Repositories;
using Microsoft.Data.SqlClient;

namespace MessageStorage.SqlServer.DataAccessSection
{
    public class SqlServerMessageStorageRepositoryContext : MessageStorageRepositoryContext
    {
        public SqlServerMessageStorageRepositoryContext(MessageStorageRepositoryContextConfiguration messageStorageRepositoryContextConfiguration) 
            : base(messageStorageRepositoryContextConfiguration)
        {
        }

        public override IMessageRepository GetMessageRepository()
        {
            IMessageRepository messageRepository = HasTransaction
                                                       ? new SqlServerMessageRepository(MessageStorageTransaction!.ToDbTransaction(), MessageStorageRepositoryContextConfiguration)
                                                       : new SqlServerMessageRepository(DbConnection, MessageStorageRepositoryContextConfiguration);

            return messageRepository;
        }

        public override IJobRepository GetJobRepository()
        {
            IJobRepository jobRepository = HasTransaction
                                               ? new SqlServerJobRepository(MessageStorageTransaction!.ToDbTransaction(), MessageStorageRepositoryContextConfiguration)
                                               : new SqlServerJobRepository(DbConnection, MessageStorageRepositoryContextConfiguration);

            return jobRepository;
        }

        protected override IDbConnection CreateDbConnection()
        {
            return new SqlConnection(MessageStorageRepositoryContextConfiguration.ConnectionStr);
        }
    }
}