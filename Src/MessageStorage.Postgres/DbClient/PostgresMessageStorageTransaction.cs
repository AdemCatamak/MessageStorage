using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MessageStorage.DataAccessLayer;
using MessageStorage.Exceptions;
using Npgsql;

namespace MessageStorage.Postgres.DbClient
{
    public class PostgresMessageStorageTransaction : IPostgresMessageStorageTransaction
    {
        private readonly NpgsqlTransaction _npgsqlTransaction;
        private readonly bool _transactionBorrowed;

        IMessageStorageConnection IMessageStorageTransaction.Connection => Connection;
        public IPostgresMessageStorageConnection Connection { get; }

        public PostgresMessageStorageTransaction(IPostgresMessageStorageConnection connection, NpgsqlTransaction npgsqlTransaction, bool transactionBorrowed)
        {
            Connection = connection;
            _npgsqlTransaction = npgsqlTransaction;
            _transactionBorrowed = transactionBorrowed;
        }

        public void Dispose()
        {
            if (!_transactionBorrowed)
            {
                _npgsqlTransaction?.Dispose();
            }
        }


        public void Commit()
        {
            _npgsqlTransaction.Commit();
        }

        public Task ExecuteAsync(string script, object? parameters, CancellationToken cancellationToken)
        {
            NpgsqlConnection npgsqlConnection = _npgsqlTransaction.Connection ?? throw new MessageStorageCustomException($"{nameof(NpgsqlTransaction)}'s {nameof(NpgsqlConnection)} is not reachable");
            var commandDefinition = new CommandDefinition(script, parameters, _npgsqlTransaction, cancellationToken: cancellationToken);

            return npgsqlConnection.ExecuteAsync(commandDefinition);
        }
    }
}