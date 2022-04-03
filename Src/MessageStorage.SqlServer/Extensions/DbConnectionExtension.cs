using System.Data;
using MessageStorage.DataAccessLayer;
using MessageStorage.Exceptions;
using MessageStorage.SqlServer.DataAccessLayer;
using Microsoft.Data.SqlClient;

namespace MessageStorage.SqlServer.Extensions;

public static class DbConnectionExtension
{
    public static IMessageStorageTransaction BeginTransaction(this SqlConnection sqlConnection, IMessageStorageClient messageStorageClient)
    {
        return BeginTransaction(sqlConnection, IsolationLevel.Unspecified, messageStorageClient);
    }

    public static IMessageStorageTransaction BeginTransaction(this SqlConnection sqlConnection, IsolationLevel isolationLevel, IMessageStorageClient messageStorageClient)
    {
        var sqlTransaction = sqlConnection.BeginTransaction(isolationLevel);
        var sqlServerMessageStorageTransaction = new SqlServerMessageStorageTransaction(sqlTransaction, false, messageStorageClient.RepositoryContext.JobQueue);
        messageStorageClient.UseTransaction(sqlServerMessageStorageTransaction);
        return sqlServerMessageStorageTransaction;
    }
}