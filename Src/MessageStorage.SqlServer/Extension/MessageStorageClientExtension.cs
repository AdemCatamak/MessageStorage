using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer;
using MessageStorage.SqlServer.DbClient;
using Microsoft.Data.SqlClient;

namespace MessageStorage.SqlServer.Extension
{
    public static class MessageStorageClientExtension
    {
        public static async Task<(Message, IEnumerable<Job>)> AddMessageAsync(this IMessageStorageClient messageStorageClient, object payload, IDbTransaction dbTransaction, CancellationToken cancellationToken = default)
        {
            using IMessageStorageTransaction messageStorageTransaction = dbTransaction.GetMessageStorageTransaction();
            var result = await messageStorageClient.AddMessageAsync(payload, messageStorageTransaction, cancellationToken);
            return result;
        }

        public static async Task<(Message, IEnumerable<Job>)> AddMessageAsync(this IMessageStorageClient messageStorageClient, object payload, SqlTransaction sqlTransaction, CancellationToken cancellationToken = default)
        {
            ISqlServerMessageStorageTransaction sqlServerMessageStorageTransaction = sqlTransaction.GetMessageStorageTransaction();
            var result = await messageStorageClient.AddMessageAsync(payload, sqlServerMessageStorageTransaction, cancellationToken);
            return result;
        }
    }
}