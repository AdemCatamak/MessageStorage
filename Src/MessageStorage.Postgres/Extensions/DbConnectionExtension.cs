using System.Data;
using MessageStorage.DataAccessLayer;
using MessageStorage.Exceptions;
using MessageStorage.Postgres.DataAccessLayer;
using Npgsql;

namespace MessageStorage.Postgres.Extensions;

public static class DbConnectionExtension
{
    public static IMessageStorageTransaction BeginTransaction(this NpgsqlConnection npgsqlConnection, IMessageStorageClient messageStorageClient)
    {
        return BeginTransaction(npgsqlConnection, IsolationLevel.Unspecified, messageStorageClient);
    }

    public static IMessageStorageTransaction BeginTransaction(this NpgsqlConnection npgsqlConnection, IsolationLevel isolationLevel, IMessageStorageClient messageStorageClient)
    {
        NpgsqlTransaction npgsqlTransaction = npgsqlConnection.BeginTransaction(isolationLevel);
        var postgresMessageStorageTransaction = new PostgresMessageStorageTransaction(npgsqlTransaction, false, messageStorageClient.RepositoryContext.JobQueue);
        messageStorageClient.UseTransaction(postgresMessageStorageTransaction);
        return postgresMessageStorageTransaction;
    }
}