using System;
using System.Data;
using MessageStorage.DataAccessLayer;
using MessageStorage.Exceptions;
using MessageStorage.Postgres.DbClient;
using Npgsql;

namespace MessageStorage.Postgres.Extension
{
    public static class DbTransactionExtension
    {
        public static IMessageStorageTransaction GetMessageStorageTransaction(this IDbTransaction dbTransaction)
        {
            if (dbTransaction is NpgsqlTransaction npgsqlTransaction)
            {
                return GetMessageStorageTransaction(npgsqlTransaction);
            }

            throw new ArgumentNotCompatibleException(dbTransaction.GetType(), typeof(NpgsqlTransaction));
        }

        public static IPostgresMessageStorageTransaction GetMessageStorageTransaction(this NpgsqlTransaction npgsqlTransaction)
        {
            NpgsqlConnection connection = npgsqlTransaction.Connection ?? throw new ArgumentNullException($"{nameof(npgsqlTransaction)}.{nameof(npgsqlTransaction.Connection)}");
            var postgresMessageStorageConnection = new PostgresMessageStorageConnection(connection);
            var messageStorageTransaction = new PostgresMessageStorageTransaction(postgresMessageStorageConnection, npgsqlTransaction, true);
            return messageStorageTransaction;
        }
    }
}