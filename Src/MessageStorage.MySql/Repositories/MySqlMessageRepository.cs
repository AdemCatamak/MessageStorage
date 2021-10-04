using System.Threading;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer.Repositories;
using MessageStorage.MySql.DbClient;
using MessageStorage.PayloadSerializers;

namespace MessageStorage.MySql.Repositories
{
    internal class MySqlMessageRepository : IMessageRepository
    {
        private readonly IMySqlRepositoryFactory _creator;
        private readonly IMySqlMessageStorageTransaction? _transaction;

        public MySqlMessageRepository(IMySqlRepositoryFactory creator, IMySqlMessageStorageTransaction? transaction = null)
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