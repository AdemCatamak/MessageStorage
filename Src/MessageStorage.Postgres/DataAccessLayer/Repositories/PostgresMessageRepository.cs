using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MessageStorage.DataAccessLayer;
using MessageStorage.PayloadSerializers;
using Npgsql;

namespace MessageStorage.Postgres.DataAccessLayer.Repositories;

internal class PostgresMessageRepository : IMessageRepository
{
    private readonly PostgresRepositoryContextConfiguration _repositoryContextConfiguration;
    private readonly NpgsqlConnection _npgsqlConnection;
    private readonly PostgresMessageStorageTransaction? _postgresMessageStorageTransaction;

    private string SchemaPlaceHolder => _repositoryContextConfiguration.Schema == null ? "" : $"\"{_repositoryContextConfiguration.Schema}\".";

    public PostgresMessageRepository(PostgresRepositoryContextConfiguration repositoryContextConfiguration, NpgsqlConnection npgsqlConnection, PostgresMessageStorageTransaction? postgresMessageStorageTransaction)
    {
        _repositoryContextConfiguration = repositoryContextConfiguration;
        _npgsqlConnection = npgsqlConnection;
        _postgresMessageStorageTransaction = postgresMessageStorageTransaction;
    }

    public async Task AddAsync(Message message, CancellationToken cancellationToken)
    {
        string payloadStr = PayloadSerializer.Serialize(message.Payload);

        var scriptBuilder = new StringBuilder("INSERT INTO ");
        scriptBuilder.Append(SchemaPlaceHolder);
        scriptBuilder.Append("messages (id, created_on, payload_type_name, payload) VALUES (@id, @created_on, @payload_type_name, @payload)");
        var script = scriptBuilder.ToString();

        var parameters = new
                         {
                             id = message.Id,
                             created_on = message.CreatedOn,
                             payload_type_name = message.Payload.GetType().FullName,
                             payload = payloadStr,
                         };

        var commandDefinition = new CommandDefinition(script, parameters, _postgresMessageStorageTransaction?.NpgsqlTransaction, cancellationToken: cancellationToken);
        await ExecuteAsync(commandDefinition);
    }

    public async Task CleanAsync(DateTime createdBeforeThen, CancellationToken cancellationToken)
    {
        var scriptBuilder = new StringBuilder($"DELETE FROM ");
        scriptBuilder.Append(SchemaPlaceHolder);
        scriptBuilder.Append("messages m ");
        scriptBuilder.Append($"LEFT JOIN ");
        scriptBuilder.Append(SchemaPlaceHolder);
        scriptBuilder.Append("jobs j ON j.message_id = m.id ");
        scriptBuilder.Append("WHERE m.created_on < @created_before_then AND j.id IS NULL");
        var script = scriptBuilder.ToString();

        var parameters = new
                         {
                             created_before_then = createdBeforeThen
                         };
        var commandDefinition = new CommandDefinition(script, parameters, cancellationToken: cancellationToken);
        await ExecuteAsync(commandDefinition);
    }

    private async Task ExecuteAsync(CommandDefinition commandDefinition)
    {
        if (_postgresMessageStorageTransaction != null)
        {
            await _postgresMessageStorageTransaction.NpgsqlTransaction.Connection.ExecuteAsync(commandDefinition);
        }
        else
        {
            await _npgsqlConnection.ExecuteAsync(commandDefinition);
        }
    }
}