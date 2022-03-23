using System.Data;
using System.Runtime.CompilerServices;
using MessageStorage.DataAccessLayer;
using MessageStorage.Exceptions;
using MessageStorage.Processor;
using MessageStorage.SqlServer.DataAccessLayer.Repositories;
using Microsoft.Data.SqlClient;

namespace MessageStorage.SqlServer.DataAccessLayer;

public class SqlServerRepositoryContextFor<TMessageStorageClient> : IRepositoryContextFor<TMessageStorageClient>
    where TMessageStorageClient : IMessageStorageClient
{
    IJobQueue IRepositoryContext.JobQueue => _jobQueueFor;
    IJobQueueFor<TMessageStorageClient> IRepositoryContextFor<TMessageStorageClient>.JobQueueFor => _jobQueueFor;
    public IMessageStorageTransaction? CurrentMessageStorageTransaction => _sqlServerMessageStorageTransaction;

    private readonly SqlServerRepositoryContextConfiguration _repositoryContextConfiguration;
    private readonly IJobQueueFor<TMessageStorageClient> _jobQueueFor;
    
    private SqlServerMessageStorageTransaction? _sqlServerMessageStorageTransaction;
    private SqlConnection? _sqlConnection;

    private readonly object _transactionLockObject;

    public SqlServerRepositoryContextFor(SqlServerRepositoryContextConfiguration repositoryContextConfiguration, IJobQueueFor<TMessageStorageClient> jobQueueFor)
    {
        _repositoryContextConfiguration = repositoryContextConfiguration;
        _jobQueueFor = jobQueueFor;
        _transactionLockObject = new object();
    }

    public IMessageStorageTransaction StartTransaction()
    {
        lock (_transactionLockObject)
        {
            if (IsMessageStorageTransactionUsable())
            {
                throw new TransactionAlreadyStartedException();
            }

            SqlConnection sqlConnection = GetConnection();
            SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();
            _sqlServerMessageStorageTransaction = new SqlServerMessageStorageTransaction(sqlTransaction, false, _jobQueueFor);
        }

        return _sqlServerMessageStorageTransaction;
    }

    public void UseTransaction(IMessageStorageTransaction borrowedTransaction)
    {
        if (borrowedTransaction is not SqlServerMessageStorageTransaction sqlServerMessageStorageTransaction)
        {
            throw new ArgumentNotCompatibleException(typeof(SqlServerMessageStorageTransaction), borrowedTransaction.GetType());
        }

        lock (_transactionLockObject)
        {
            if (IsMessageStorageTransactionUsable())
            {
                throw new TransactionAlreadyStartedException();
            }

            _sqlServerMessageStorageTransaction = sqlServerMessageStorageTransaction;
        }
    }

    public IMessageRepository GetMessageRepository()
    {
        var connection = IsMessageStorageTransactionUsable() ? _sqlServerMessageStorageTransaction!.SqlTransaction.Connection : GetConnection();
        var sqlServerMessageRepository = new SqlServerMessageRepository(_repositoryContextConfiguration,
                                                                        connection,
                                                                        _sqlServerMessageStorageTransaction);
        return sqlServerMessageRepository;
    }

    public IJobRepository GetJobRepository()
    {
        var connection = IsMessageStorageTransactionUsable() ? _sqlServerMessageStorageTransaction!.SqlTransaction.Connection : GetConnection();
        var sqlServerJobRepository = new SqlServerJobRepository(_repositoryContextConfiguration,
                                                                connection,
                                                                _sqlServerMessageStorageTransaction);
        return sqlServerJobRepository;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private SqlConnection GetConnection()
    {
        if (_sqlConnection?.State is ConnectionState.Open)
        {
            return _sqlConnection;
        }

        _sqlConnection = new SqlConnection(_repositoryContextConfiguration.ConnectionString);
        _sqlConnection.Open();
        return _sqlConnection;
    }

    private bool IsMessageStorageTransactionUsable()
    {
        if (_sqlServerMessageStorageTransaction != null && !_sqlServerMessageStorageTransaction.IsCommitted && !_sqlServerMessageStorageTransaction.IsDisposed)
        {
            return true;
        }

        return false;
    }

}