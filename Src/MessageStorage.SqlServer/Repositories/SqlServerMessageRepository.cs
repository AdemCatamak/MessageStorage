using System.Threading;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer.Repositories;
using MessageStorage.PayloadSerializers;
using MessageStorage.SqlServer.DbClient;

// ReSharper disable RedundantAnonymousTypePropertyName

namespace MessageStorage.SqlServer.Repositories
{
    internal class SqlServerMessageRepository : IMessageRepository
    {
        private readonly ISqlServerRepositoryFactory _creator;
        private readonly ISqlServerMessageStorageTransaction? _transaction;

        public SqlServerMessageRepository(ISqlServerRepositoryFactory creator, ISqlServerMessageStorageTransaction? transaction = null)
        {
            _creator = creator;
            _transaction = transaction;
        }

        public Task AddAsync(Message message, CancellationToken cancellationToken = default)
        {
            string payloadStr = PayloadSerializer.Serialize(message.Payload);

            string script = $"INSERT INTO [{_creator.RepositoryConfiguration.Schema}].[Messages] (Id, CreatedOn, PayloadTypeName,Payload) VALUES (@Id, @CreatedOn, @PayloadTypeName, @Payload)";
            var parameters = new
                             {
                                 Id = message.Id,
                                 CreatedOn = message.CreatedOn,
                                 PayloadTypeName = message.Payload.GetType().FullName,
                                 Payload = payloadStr,
                             };

            return ExecuteAsync(script, parameters, cancellationToken);
        }

        private async Task ExecuteAsync(string script, object? parameters, CancellationToken cancellationToken)
        {
            if (_transaction != null)
            {
                await _transaction.ExecuteAsync(script, parameters, cancellationToken);
            }
            else
            {
                using ISqlServerMessageStorageConnection connection = _creator.CreateConnection();
                await connection.ExecuteAsync(script, parameters, cancellationToken);
            }
        }
    }
}