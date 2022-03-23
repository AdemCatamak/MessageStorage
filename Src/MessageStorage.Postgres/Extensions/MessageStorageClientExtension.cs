using System.Data;
using MessageStorage.DataAccessLayer;
using MessageStorage.Exceptions;
using MessageStorage.Postgres.DataAccessLayer;
using Npgsql;

namespace MessageStorage.Postgres.Extensions;

public static class MessageStorageClientExtension
{
    public static IMessageStorageTransaction UseTransaction(this IMessageStorageClient messageStorageClient, IDbTransaction dbTransaction)
    {
        if (dbTransaction is not NpgsqlTransaction npgsqlTransaction)
        {
            throw new ArgumentNotCompatibleException(dbTransaction.GetType(), typeof(NpgsqlTransaction));
        }

        return UseTransaction(messageStorageClient, npgsqlTransaction);
    }

    public static IMessageStorageTransaction UseTransaction(this IMessageStorageClient messageStorageClient, NpgsqlTransaction npgsqlTransaction)
    {
        var postgresMessageStorageTransaction = new PostgresMessageStorageTransaction(npgsqlTransaction, true, messageStorageClient.RepositoryContext.JobQueue);
        messageStorageClient.UseTransaction(postgresMessageStorageTransaction);
        return postgresMessageStorageTransaction;
    }
}