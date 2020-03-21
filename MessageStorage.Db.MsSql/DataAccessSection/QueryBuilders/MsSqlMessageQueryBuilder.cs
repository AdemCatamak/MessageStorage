using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MessageStorage.Db.DataAccessLayer.QueryBuilders;
using Microsoft.Data.SqlClient;

namespace MessageStorage.Db.MsSql.DataAccessSection.QueryBuilders
{
    public class MsSqlMessageQueryBuilder : IMessageQueryBuilder
    {
        private readonly MessageStorageDbConfiguration _messageStorageDbConfiguration;

        public MsSqlMessageQueryBuilder(MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            _messageStorageDbConfiguration = messageStorageDbConfiguration;
        }

        public (string, IEnumerable<IDbDataParameter>) Add(Message message)
        {
            var sqlParameters = new List<SqlParameter>
                                {
                                    new SqlParameter("@TraceId", (object) message.TraceId ?? DBNull.Value) {SourceColumn = "TraceId"},
                                    new SqlParameter("@CreatedOn", message.CreatedOn) {SourceColumn = "CreatedOn"},
                                    new SqlParameter("@SerializedPayload", message.SerializedPayload) {SourceColumn = "SerializedPayload"},
                                    new SqlParameter("@PayloadClassNamespace", message.PayloadClassNamespace) {SourceColumn = "PayloadClassNamespace"},
                                    new SqlParameter("@PayloadClassName", message.PayloadClassName) {SourceColumn = "PayloadClassName"},
                                };

            string columns = string.Join(",", sqlParameters.Select(p => $"[{p.SourceColumn}]"));
            string parameterNames = string.Join(",", sqlParameters.Select(p => $"{p.ParameterName}"));
            string commandText = $"INSERT INTO [{_messageStorageDbConfiguration.Schema}].[{TableNames.MessageTable}] ({columns}) VALUES ({parameterNames}) SELECT SCOPE_IDENTITY()";
            return (commandText, sqlParameters);
        }
    }
}