using System;
using System.Data;
using MessageStorage.DataAccessLayer;
using MessageStorage.Exceptions;
using MessageStorage.MySql.DbClient;
using MySqlConnector;

namespace MessageStorage.MySql.Extension
{
    public static class DbTransactionExtension
    {
        public static IMessageStorageTransaction GetMessageStorageTransaction(this IDbTransaction dbTransaction)
        {
            if (dbTransaction is MySqlTransaction mySqlTransaction)
            {
                return GetMessageStorageTransaction(mySqlTransaction);
            }

            throw new ArgumentNotCompatibleException(dbTransaction.GetType(), typeof(MySqlTransaction));
        }

        public static IMySqlMessageStorageTransaction GetMessageStorageTransaction(this MySqlTransaction mySqlTransaction)
        {
            MySqlConnection connection = mySqlTransaction.Connection ?? throw new ArgumentNullException($"{nameof(mySqlTransaction)}.{nameof(mySqlTransaction.Connection)}");
            var mySqlMessageStorageConnection = new MySqlMessageStorageConnection(connection);
            var messageStorageTransaction = new MySqlMessageStorageTransaction(mySqlMessageStorageConnection, mySqlTransaction, true);
            return messageStorageTransaction;
        }
    }
}