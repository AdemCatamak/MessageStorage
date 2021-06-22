using System;
using System.Data;
using MessageStorage.DataAccessLayer;
using MessageStorage.Exceptions;
using MessageStorage.SqlServer.DbClient;
using Microsoft.Data.SqlClient;

namespace MessageStorage.SqlServer.Extension
{
    public static class DbTransactionExtension
    {
        public static IMessageStorageTransaction GetMessageStorageTransaction(this IDbTransaction dbTransaction)
        {
            if (dbTransaction is SqlTransaction sqlTransaction)
            {
                return GetMessageStorageTransaction(sqlTransaction);
            }

            throw new ArgumentNotCompatibleException(dbTransaction.GetType(), typeof(SqlTransaction));
        }

        public static ISqlServerMessageStorageTransaction GetMessageStorageTransaction(this SqlTransaction sqlTransaction)
        {
            SqlConnection connection = sqlTransaction.Connection ?? throw new ArgumentNullException($"{nameof(sqlTransaction)}.{nameof(sqlTransaction.Connection)}");
            var sqlServerMessageStorageConnection = new SqlServerMessageStorageConnection(connection);
            var messageStorageTransaction = new SqlServerMessageStorageTransaction(sqlServerMessageStorageConnection, sqlTransaction, true);
            return messageStorageTransaction;
        }
    }
}