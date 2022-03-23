using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MessageStorage.DataAccessLayer;
using MessageStorage.PayloadSerializers;
using Microsoft.Data.SqlClient;

namespace MessageStorage.SqlServer.DataAccessLayer.Repositories;

internal class SqlServerMessageRepository : IMessageRepository
{
    private readonly SqlServerRepositoryContextConfiguration _repositoryContextConfiguration;
    private readonly SqlConnection _sqlConnection;
    private readonly SqlServerMessageStorageTransaction? _messageStorageTransaction;

    public SqlServerMessageRepository(SqlServerRepositoryContextConfiguration repositoryContextConfiguration, SqlConnection sqlConnection, SqlServerMessageStorageTransaction? messageStorageTransaction)
    {
        _repositoryContextConfiguration = repositoryContextConfiguration;
        _sqlConnection = sqlConnection;
        _messageStorageTransaction = messageStorageTransaction;
    }

    public async Task AddAsync(Message message, CancellationToken cancellationToken)
    {
        string payloadStr = PayloadSerializer.Serialize(message.Payload);

        var scriptBuilder = new StringBuilder("INSERT INTO ");
        if (_repositoryContextConfiguration.Schema != null)
        {
            scriptBuilder.Append($"[{_repositoryContextConfiguration.Schema}].");
        }

        scriptBuilder.Append("[Messages] (Id, CreatedOn, PayloadTypeName,Payload) VALUES (@Id, @CreatedOn, @PayloadTypeName, @Payload)");
        var script = scriptBuilder.ToString();

        var parameters = new
                         {
                             Id = message.Id,
                             CreatedOn = message.CreatedOn,
                             PayloadTypeName = message.Payload.GetType().FullName,
                             Payload = payloadStr,
                         };

        var commandDefinition = new CommandDefinition(script, parameters, _messageStorageTransaction?.SqlTransaction, cancellationToken: cancellationToken);
        await ExecuteAsync(commandDefinition);
    }

    private async Task ExecuteAsync(CommandDefinition commandDefinition)
    {
        if (_messageStorageTransaction != null)
        {
            await _messageStorageTransaction.SqlTransaction.Connection.ExecuteAsync(commandDefinition);
        }
        else
        {
            await _sqlConnection.ExecuteAsync(commandDefinition);
        }
    }
}