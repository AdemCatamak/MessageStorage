using System.Data;
using System.Data.Common;
using Dapper;
using MessageStorage;
using Microsoft.Data.SqlClient;
using Npgsql;

namespace TestUtility.DbUtils;

public interface IFetch
{
    Task<Message?> MessageFromPostgresAsync(Guid id);
    Task<Job?> JobFromPostgresAsync(Guid id);

    Task<Message?> MessageFromSqlServerAsync(Guid id);
    Task<Job?> JobFromSqlServerAsync(Guid id);
}

internal class Fetch : IFetch
{
    private string? PostgresConnectionStr { get; set; }
    private string? SqlServerConnectionStr { get; set; }

    public Fetch(string postgresConnectionStr, string sqlServerConnectionStr)
    {
        PostgresConnectionStr = postgresConnectionStr;
        SqlServerConnectionStr = sqlServerConnectionStr;
    }

    public async Task<Message?> MessageFromPostgresAsync(Guid id)
    {
        using var connection = new NpgsqlConnection(PostgresConnectionStr);
        const string getMessageScript = "SELECT * FROM messages WHERE id = @id";
        var getMessageParameters = new
                                   {
                                       id = id
                                   };
        var getMessageCommandDefinition = new CommandDefinition(getMessageScript, getMessageParameters);
        dynamic? messageFromDb = await connection.QueryFirstOrDefaultAsync(getMessageCommandDefinition);

        Message? message = messageFromDb != null
                               ? new Message((Guid)messageFromDb.id, messageFromDb.payload, (DateTime)messageFromDb.created_on)
                               : null;

        return message;
    }

    public async Task<Job?> JobFromPostgresAsync(Guid id)
    {
        using var connection = new NpgsqlConnection(PostgresConnectionStr);
        const string getJobScript = "SELECT * FROM jobs WHERE id = @id";
        var getJobParameters = new
                               {
                                   id = id
                               };
        var getJobCommandDefinition = new CommandDefinition(getJobScript, getJobParameters);
        dynamic? jobFromDb = await connection.QueryFirstOrDefaultAsync(getJobCommandDefinition);

        Job? job = jobFromDb != null
                       ? new Job((Guid)jobFromDb.id, new Message("some-payload"), jobFromDb.message_handler_type_name,
                                 (JobStatus)jobFromDb.job_status, (DateTime)jobFromDb.created_on,
                                 (DateTime)jobFromDb.last_operation_time, jobFromDb.last_operation_info,
                                 (int)jobFromDb.current_retry_count, (int)jobFromDb.max_retry_count)
                       : null;
        return job;
    }


    public async Task<Message?> MessageFromSqlServerAsync(Guid id)
    {
        using var connection = new SqlConnection(SqlServerConnectionStr);
        await connection.OpenAsync();
        using DbTransaction transaction = await connection.BeginTransactionAsync(IsolationLevel.Snapshot);
        const string getMessageScript = "SELECT * FROM Messages WHERE Id = @Id";
        var getMessageParameters = new
                                   {
                                       Id = id
                                   };
        var getMessageCommandDefinition = new CommandDefinition(getMessageScript, getMessageParameters, transaction);
        dynamic? messageFromDb = await transaction.Connection.QueryFirstOrDefaultAsync(getMessageCommandDefinition);

        Message? message = messageFromDb != null ? new Message((Guid)messageFromDb.Id, messageFromDb.Payload, (DateTime)messageFromDb.CreatedOn) : null;
        return message;
    }


    public async Task<Job?> JobFromSqlServerAsync(Guid id)
    {
        using var connection = new SqlConnection(SqlServerConnectionStr);
        await connection.OpenAsync();
        using DbTransaction transaction = await connection.BeginTransactionAsync(IsolationLevel.Snapshot);
        const string getJobScript = "SELECT * FROM Jobs WHERE Id = @Id";
        var getJobParameters = new
                               {
                                   Id = id
                               };
        var getJobCommandDefinition = new CommandDefinition(getJobScript, getJobParameters, transaction);
        dynamic? jobFromDb = await transaction.Connection.QueryFirstOrDefaultAsync(getJobCommandDefinition);

        Job? job = jobFromDb != null
                       ? new Job((Guid)jobFromDb.Id, new Message("some-payload"), jobFromDb.MessageHandlerTypeName,
                                 (JobStatus)jobFromDb.JobStatus, (DateTime)jobFromDb.CreatedOn,
                                 (DateTime)jobFromDb.LastOperationTime, jobFromDb.LastOperationInfo,
                                 (int)jobFromDb.CurrentRetryCount, (int)jobFromDb.MaxRetryCount)
                       : null;
        return job;
    }
}