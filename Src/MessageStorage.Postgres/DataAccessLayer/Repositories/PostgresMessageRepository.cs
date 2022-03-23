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
        if (_repositoryContextConfiguration.Schema != null)
        {
            scriptBuilder.Append($"\"{_repositoryContextConfiguration.Schema}\".");
        }

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