using System.Data;
using MessageStorage.Db.Configurations;
using MessageStorage.Db.DataAccessSection.Repositories.Imp;
using MessageStorage.Models;
using Microsoft.Data.SqlClient;

namespace MessageStorage.Db.SqlServer.DataAccessSection.Repositories
{
    public class SqlServerMessageDbRepository : MessageDbRepository
    {
        public SqlServerMessageDbRepository(IDbConnection dbConnection, DbRepositoryConfiguration dbRepositoryConfiguration)
            : base(dbConnection, dbRepositoryConfiguration)
        {
        }

        protected override (string, IDbDataParameter[]) PrepareAddCommand(Message entity)
        {
            object payload = entity.GetPayload();
            string commandText = $"Insert Into [{DbRepositoryConfiguration.Schema}].[{TableNames.MessageTable}] (MessageId, CreatedOn, SerializedPayload, PayloadClassName, PayloadClassFullName) Values (@MessageId, @CreatedOn, @SerializedPayload, @PayloadClassName, @PayloadClassFullName)";
            IDbDataParameter[] dataParameters =
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