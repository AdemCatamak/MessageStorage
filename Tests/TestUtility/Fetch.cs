using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Npgsql;

namespace TestUtility;

public class Fetch
{
    public static string? PostgresConnectionStr { get; private set; }
    public static string? SqlServerConnectionStr { get; private set; }

    public static void SetPostgresConnectionStr(string connectionStr)
    {
        PostgresConnectionStr = connectionStr;
    }

    public static void SetSqlServerConnectionStr(string connectionStr)
    {
        SqlServerConnectionStr = connectionStr;
    }

    public static async Task<dynamic?> MessageFromPostgresAsync(Guid id)
    {
        using var connection = new NpgsqlConnection(PostgresConnectionStr);
        const string getMessageScript = "SELECT * FROM messages WHERE id = @id";
        var getMessageParameters = new
                                   {
                                       id = id
                                   };
        var getMessageCommandDefinition = new CommandDefinition(getMessageScript, getMessageParameters);
        dynamic? messageFromDb = await connection.QueryFirstOrDefaultAsync(getMessageCommandDefinition);

        return messageFromDb;
    }

    public static async Task<dynamic?> JobFromPostgresAsync(Guid id)
    {
        using var connection = new NpgsqlConnection(PostgresConnectionStr);
        const string getJobScript = "SELECT * FROM jobs WHERE id = @id";
        var getJobParameters = new
                               {
                                   id = id
                               };
        var getJobCommandDefinition = new CommandDefinition(getJobScript, getJobParameters);
        dynamic? jobFromDb = await connection.QueryFirstOrDefaultAsync(getJobCommandDefinition);
        return jobFromDb;
    }

    public static async Task<dynamic?> JobFromSqlServerAsync(Guid id)
    {
        using var connection = new SqlConnection(SqlServerConnectionStr);
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync(IsolationLevel.Snapshot);
        const string getJobScript = "SELECT * FROM Jobs WHERE Id = @Id";
        var getJobParameters = new
                               {
                                   Id = id
                               };
        var getJobCommandDefinition = new CommandDefinition(getJobScript, getJobParameters, transaction);
        dynamic? jobFromDb = await transaction.Connection.QueryFirstOrDefaultAsync(getJobCommandDefinition);
        return jobFromDb;
    }

    public static async Task<dynamic?> MessageFromSqlServerAsync(Guid id)
    {
        using var connection = new SqlConnection(SqlServerConnectionStr);
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync(IsolationLevel.Snapshot);
        const string getMessageScript = "SELECT * FROM Messages WHERE Id = @Id";
        var getMessageParameters = new
                                   {
                                       Id = id
                                   };
        var getMessageCommandDefinition = new CommandDefinition(getMessageScript, getMessageParameters, transaction);
        dynamic? messageFromDb = await transaction.Connection.QueryFirstOrDefaultAsync(getMessageCommandDefinition);

        return messageFromDb;
    }
}