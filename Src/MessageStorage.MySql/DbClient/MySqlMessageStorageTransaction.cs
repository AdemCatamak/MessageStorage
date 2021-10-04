using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MessageStorage.DataAccessLayer;
using MessageStorage.Exceptions;
using MySql.Data.MySqlClient;

namespace MessageStorage.MySql.DbClient
{
    public class MySqlMessageStorageTransaction : IMySqlMessageStorageTransaction
    {
        private readonly MySqlTransaction _mySqlTransaction;
        private readonly bool _transactionBorrowed;

        IMessageStorageConnection IMessageStorageTransaction.Connection => Connection;
        public IMySqlMessageStorageConnection Connection { get; }

        public MySqlMessageStorageTransaction(IMySqlMessageStorageConnection connection, MySqlTransaction mySqlTransaction, bool transactionBorrowed)
        {
            Connection = connection;
            _mySqlTransaction = mySqlTransaction;
            _transactionBorrowed = transactionBorrowed;
        }

        public void Dispose()
        {
            if (!_transactionBorrowed)
            {
                _mySqlTransaction?.Dispose();
            }
        }


        public void Commit()
        {
            _mySqlTransaction.Commit();
        }

        public Task ExecuteAsync(string script, object? parameters, CancellationToken cancellationToken)
        {
            MySqlConnection mySqlConnection = _mySqlTransaction.Connection ?? throw new MessageStorageCustomException($"{nameof(MySqlTransaction)}'s {nameof(MySqlConnection)} is not reachable");
            var commandDefinition = new CommandDefinition(script, parameters, _mySqlTransaction, cancellationToken: cancellationToken);

            return mySqlConnection.ExecuteAsync(commandDefinition);
        }
    }
}