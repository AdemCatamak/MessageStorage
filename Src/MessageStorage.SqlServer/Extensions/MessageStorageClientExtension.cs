using System.Data;
using MessageStorage.DataAccessLayer;
using MessageStorage.Exceptions;
using MessageStorage.SqlServer.DataAccessLayer;
using Microsoft.Data.SqlClient;

namespace MessageStorage.SqlServer.Extensions;

public static class MessageStorageClientExtension
{
    public static IMessageStorageTransaction UseTransaction(this IMessageStorageClient messageStorageClient, IDbTransaction dbTransaction)
    {
        if (dbTransaction is not SqlTransaction sqlTransaction)
        {
            throw new ArgumentNotCompatibleException(dbTransaction.GetType(), typeof(SqlTransaction));
        }

        return UseTransaction(messageStorageClient, sqlTransaction);
    }

    public static IMessageStorageTransaction UseTransaction(this IMessageStorageClient messageStorageClient, SqlTransaction sqlTransaction)
    {
        var sqlServerMessageStorageTransaction = new SqlServerMessageStorageTransaction(sqlTransaction, true, messageStorageClient.RepositoryContext.JobQueue);
        messageStorageClient.UseTransaction(sqlServerMessageStorageTransaction);
        return sqlServerMessageStorageTransaction;
    }
}