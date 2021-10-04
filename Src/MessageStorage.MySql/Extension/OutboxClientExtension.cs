using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer;
using MessageStorage.MySql.DbClient;
using MySql.Data.MySqlClient;

namespace MessageStorage.MySql.Extension
{
    public static class OutboxClientExtension
    {
        public static void UseTransaction(this IMessageStorageClient messageStorageClient, IDbTransaction dbTransaction)
        {
            IMessageStorageTransaction messageStorageTransaction = dbTransaction.GetMessageStorageTransaction();
            messageStorageClient.UseTransaction(messageStorageTransaction);
        }

        public static void UseTransaction(this IMessageStorageClient messageStorageClient, MySqlTransaction mySqlTransaction)
        {
            IMySqlMessageStorageTransaction mySqlMessageStorageTransaction = mySqlTransaction.GetMessageStorageTransaction();
            messageStorageClient.UseTransaction(mySqlMessageStorageTransaction);
        }

        public static async Task<(Message, IEnumerable<Job>)> AddMessageAsync(this IMessageStorageClient messageStorageClient, object payload, IDbTransaction dbTransaction, CancellationToken cancellationToken = default)
        {
            IMessageStorageTransaction messageStorageTransaction = dbTransaction.GetMessageStorageTransaction();
            var result = await messageStorageClient.AddMessageAsync(payload, messageStorageTransaction, cancellationToken);
            return result;
        }

        public static async Task<(Message, IEnumerable<Job>)> AddMessageAsync(this IMessageStorageClient messageStorageClient, object payload, MySqlTransaction mySqlTransaction, CancellationToken cancellationToken = default)
        {
            IMySqlMessageStorageTransaction mySqlMessageStorageTransaction = mySqlTransaction.GetMessageStorageTransaction();
            var result = await messageStorageClient.AddMessageAsync(payload, mySqlMessageStorageTransaction, cancellationToken);
            return result;
        }
    }
}