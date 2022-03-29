using Dapper;
using MessageStorage;
using Microsoft.Data.SqlClient;

namespace TestUtility.DbUtils;

public interface IInsert
{
    public record MessageDto(Guid Id, DateTime CreatedOn, string PayloadTypeName, string Payload);

    public record JobDto(Guid Id, DateTime CreatedOn, Guid MessageId, string MessageHandlerTypeName, JobStatus JobStatus, DateTime LastOperationTime, string LastOperationInfo, int MaxRetryCount, int CurrentRetryCount);

    public Task MessageIntoSqlServerAsync(MessageDto message, CancellationToken cancellationToken = default);
    public Task JobIntoSqlServerAsync(JobDto job, CancellationToken cancellationToken = default);

    public Task MessageIntoPostgresAsync(MessageDto message, CancellationToken cancellationToken = default);
    public Task JobIntoPostgresAsync(JobDto job, CancellationToken cancellationToken = default);
}

public class Insert : IInsert
{
    private string? PostgresConnectionStr { get; set; }
    private string? SqlServerConnectionStr { get; set; }

    public Insert(string postgresConnectionStr, string sqlServerConnectionStr)
    {
        PostgresConnectionStr = postgresConnectionStr;
        SqlServerConnectionStr = sqlServerConnectionStr;
    }

    const string INSERT_MESSAGE_INTO_SQL_SERVER = "INSERT INTO messages (Id, CreatedOn, PayloadTypeName, Payload) VALUES (@Id, @CreatedOn, @PayloadTypeName, @Payload)";

    private const string INSERT_JOB_INTO_SQL_SERVER =
        @"
INSERT INTO jobs (Id, CreatedOn, MessageId, MessageHandlerTypeName, JobStatus, LastOperationTime, LastOperationInfo, MaxRetryCount, CurrentRetryCount)
VALUES (@Id, @CreatedOn, @MessageId, @MessageHandlerTypeName, @JobStatus, @LastOperationTime, @LastOperationInfo, @MaxRetryCount, @CurrentRetryCount)
";

    const string INSERT_MESSAGE_INTO_POSTGRES = "INSERT INTO messages (id, created_on, payload_type_name, payload) VALUES (@Id, @CreatedOn, @PayloadTypeName, @Payload)";

    private const string INSERT_JOB_INTO_POSTGRES =
        @"
INSERT INTO jobs (id, created_on, message_id, message_handler_type_name, job_status, last_operation_time, last_operation_info, max_retry_count, current_retry_count)
VALUES (@Id, @CreatedOn, @MessageId, @MessageHandlerTypeName, @JobStatus, @LastOperationTime, @LastOperationInfo, @MaxRetryCount, @CurrentRetryCount)
";

    public async Task MessageIntoSqlServerAsync(IInsert.MessageDto message, CancellationToken cancellationToken = default)
    {
        using var sqlConnection = new SqlConnection(SqlServerConnectionStr);
        var commandDefinition = new CommandDefinition(INSERT_MESSAGE_INTO_SQL_SERVER, message, cancellationToken: cancellationToken);
        await sqlConnection.ExecuteAsync(commandDefinition);
    }

    public async Task JobIntoSqlServerAsync(IInsert.JobDto job, CancellationToken cancellationToken = default)
    {
        using var sqlConnection = new SqlConnection(SqlServerConnectionStr);
        var commandDefinition = new CommandDefinition(INSERT_JOB_INTO_SQL_SERVER, job, cancellationToken: cancellationToken);
        await sqlConnection.ExecuteAsync(commandDefinition);
    }

    public async Task MessageIntoPostgresAsync(IInsert.MessageDto message, CancellationToken cancellationToken = default)
    {
        using var postgresConnection = new Npgsql.NpgsqlConnection(PostgresConnectionStr);
        var commandDefinition = new CommandDefinition(INSERT_MESSAGE_INTO_POSTGRES, message, cancellationToken: cancellationToken);
        await postgresConnection.ExecuteAsync(commandDefinition);
    }

    public async Task JobIntoPostgresAsync(IInsert.JobDto job, CancellationToken cancellationToken = default)
    {
        using var postgresConnection = new Npgsql.NpgsqlConnection(PostgresConnectionStr);
        var commandDefinition = new CommandDefinition(INSERT_JOB_INTO_POSTGRES, job, cancellationToken: cancellationToken);
        await postgresConnection.ExecuteAsync(commandDefinition);
    }
}