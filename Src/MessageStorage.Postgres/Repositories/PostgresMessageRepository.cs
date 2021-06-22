using System.Threading;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer.Repositories;
using MessageStorage.PayloadSerializers;
using MessageStorage.Postgres.DbClient;

namespace MessageStorage.Postgres.Repositories
{
    internal class PostgresMessageRepository : IMessageRepository
    {
        private readonly IPostgresRepositoryFactory _creator;
        private readonly IPostgresMessageStorageTransaction? _transaction;

        public PostgresMessageRepository(IPostgresRepositoryFactory creator, IPostgresMessageStorageTransaction? transaction = null)
        {
            _creator = creator;
            _transaction = transaction;
        }

        public Task AddAsync(Message message, CancellationToken cancellationToken = default)
        {
            string payloadStr = PayloadSerializer.Serialize(message.Payload);

            string script = $"insert into \"{_creator.RepositoryConfiguration.Schema}\".\"messages\" (id, created_on, payload_type_name, payload) VALUES (@id, @created_on, @payload_type_name, @payload)";
            var parameters = new
                             {
                                 id = message.Id,
                                 created_on = message.CreatedOn,
                                 payload_type_name = message.Payload.GetType().FullName,
                                 payload = payloadStr,
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
                using var connection = _creator.CreateConnection();
                await connection.ExecuteAsync(script, parameters, cancellationToken);
            }
        }
    }
}