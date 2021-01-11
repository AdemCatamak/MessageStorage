using System.Data;
using MessageStorage.Configurations;
using MessageStorage.DataAccessSection.Repositories;

namespace MessageStorage.SqlServer.DataAccessSection.Repositories
{
    public class SqlServerMessageRepository : BaseMessageRepository
    {
        public SqlServerMessageRepository(IDbTransaction dbTransaction, MessageStorageRepositoryContextConfiguration messageStorageRepositoryContextConfiguration)
            : base(dbTransaction, messageStorageRepositoryContextConfiguration)
        {
        }

        public SqlServerMessageRepository(IDbConnection dbConnection, MessageStorageRepositoryContextConfiguration messageStorageRepositoryContextConfiguration)
            : base(dbConnection, messageStorageRepositoryContextConfiguration)
        {
        }

        protected override string AddStatement =>
            $"Insert Into [{MessageStorageRepositoryContextConfiguration.Schema}].[{TableNames.MessageTable}]" +
            $"(MessageId, SerializedPayload, PayloadClassName, PayloadClassFullName, CreatedOn) " +
            $"Values " +
            $"(@MessageId, @SerializedPayload, @PayloadClassName, @PayloadClassFullName, @CreatedOn)";
    }
}