using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer;
using MessageStorage.Postgres.DbClient;
using Npgsql;

namespace MessageStorage.Postgres.Extension
{
    public static class OutboxClientExtension
    {
        public static void UseTransaction(this IMessageStorageClient messageStorageClient, IDbTransaction dbTransaction)
        {
            IMessageStorageTransaction messageStorageTransaction = dbTransaction.GetMessageStorageTransaction();
            messageStorageClient.UseTransaction(messageStorageTransaction);
        }

        public static void UseTransaction(this IMessageStorageClient messageStorageClient, NpgsqlTransaction npgsqlTransaction)
        {
            IPostgresMessageStorageTransaction postgresMessageStorageTransaction = npgsqlTransaction.GetMessageStorageTransaction();
            messageStorageClient.UseTransaction(postgresMessageStorageTransaction);
        }

        public static async Task<(Message, IEnumerable<Job>)> AddMessageAsync(this IMessageStorageClient messageStorageClient, object payload, IDbTransaction dbTransaction, CancellationToken cancellationToken = default)
        {
            IMessageStorageTransaction messageStorageTransaction = dbTransaction.GetMessageStorageTransaction();
            var result = await messageStorageClient.AddMessageAsync(payload, messageStorageTransaction, cancellationToken);
            return result;
        }

        public static async Task<(Message, IEnumerable<Job>)> AddMessageAsync(this IMessageStorageClient messageStorageClient, object payload, NpgsqlTransaction npgsqlTransaction, CancellationToken cancellationToken = default)
        {
            IPostgresMessageStorageTransaction postgresMessageStorageTransaction = npgsqlTransaction.GetMessageStorageTransaction();
            var result = await messageStorageClient.AddMessageAsync(payload, postgresMessageStorageTransaction, cancellationToken);
            return result;
        }
    }
}