using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MessageStorage.DataAccessLayer;
using Microsoft.Data.SqlClient;

namespace MessageStorage.SqlServer.DbClient
{
    public class SqlServerMessageStorageTransaction : ISqlServerMessageStorageTransaction
    {
        private readonly SqlTransaction _sqlTransaction;
        private readonly bool _transactionBorrowed;

        public ISqlServerMessageStorageConnection Connection { get; }
        IMessageStorageConnection IMessageStorageTransaction.Connection => Connection;

        public SqlServerMessageStorageTransaction(ISqlServerMessageStorageConnection connection, SqlTransaction sqlTransaction, bool transactionBorrowed)
        {
            Connection = connection;
            _sqlTransaction = sqlTransaction;
            _transactionBorrowed = transactionBorrowed;
        }

        public void Dispose()
        {
            if (!_transactionBorrowed)
            {
                _sqlTransaction?.Dispose();
            }
        }


        public void Commit()
        {
            _sqlTransaction.Commit();
        }

        public Task ExecuteAsync(string script, object? parameters, CancellationToken cancellationToken)
        {
            SqlConnection sqlConnection = _sqlTransaction.Connection;
            var commandDefinition = new CommandDefinition(script, parameters, _sqlTransaction, cancellationToken: cancellationToken);

            return sqlConnection.ExecuteAsync(commandDefinition);
        }
    }
}