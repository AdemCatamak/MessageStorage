using System.Data;
using System.Data.SqlClient;
using MessageStorage.Db.Configurations;
using MessageStorage.Db.DataAccessSection.Repositories.Imp;
using MessageStorage.Models;

namespace MessageStorage.Db.SqlServer.DataAccessSection.Repositories
{
    public class SqlServerDbMessageRepository<TDbRepositoryConfiguration>
        : DbMessageRepository<TDbRepositoryConfiguration>
        where TDbRepositoryConfiguration : DbRepositoryConfiguration
    {
        public SqlServerDbMessageRepository(IDbConnection dbConnection, TDbRepositoryConfiguration dbRepositoryConfiguration)
            : base(dbConnection, dbRepositoryConfiguration)
        {
        }

        protected override (string, IDataParameter[]) PrepareAddCommand(Message entity)
        {
            object payload = entity.GetPayload();
            string commandText = $"Insert Into [{DbRepositoryConfiguration.Schema}].[{TableNames.MessageTable}] (MessageId, CreatedOn, SerializedPayload, PayloadClassName, PayloadClassFullName) Values (@MessageId, @CreatedOn, @SerializedPayload, @PayloadClassName, @PayloadClassFullName)";
            IDataParameter[] dataParameters =
            {
                new SqlParameter("@MessageId", entity.Id),
                new SqlParameter("@CreatedOn", entity.CreatedOn),
                new SqlParameter("@SerializedPayload", entity.SerializedPayload),
                new SqlParameter("@PayloadClassName", payload.GetType().Name),
                new SqlParameter("@PayloadClassFullName", payload.GetType().FullName)
            };

            return (commandText, dataParameters);
        }
    }
}