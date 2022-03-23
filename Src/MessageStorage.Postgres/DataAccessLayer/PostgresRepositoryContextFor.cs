using System.Data;
using System.Runtime.CompilerServices;
using MessageStorage.DataAccessLayer;
using MessageStorage.Exceptions;
using MessageStorage.Postgres.DataAccessLayer.Repositories;
using MessageStorage.Processor;
using Npgsql;

namespace MessageStorage.Postgres.DataAccessLayer;

public class PostgresRepositoryContextFor<TMessageStorageClient> : IRepositoryContextFor<TMessageStorageClient>
    where TMessageStorageClient : IMessageStorageClient
{
    IJobQueue IRepositoryContext.JobQueue => _jobQueueFor;
    IJobQueueFor<TMessageStorageClient> IRepositoryContextFor<TMessageStorageClient>.JobQueueFor => _jobQueueFor;
    public IMessageStorageTransaction? CurrentMessageStorageTransaction => _postgresMessageStorageTransaction;

    private readonly PostgresRepositoryContextConfiguration _repositoryContextConfiguration;
    private readonly IJobQueueFor<TMessageStorageClient> _jobQueueFor;

    private PostgresMessageStorageTransaction? _postgresMessageStorageTransaction;
    private NpgsqlConnection? _npgsqlConnection;

    private readonly object _transactionLockObject;

    public PostgresRepositoryContextFor(PostgresRepositoryContextConfiguration repositoryContextConfiguration, IJobQueueFor<TMessageStorageClient> jobQueueFor)
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

            NpgsqlConnection npgsqlConnection = GetConnection();
            NpgsqlTransaction npgsqlTransaction = npgsqlConnection.BeginTransaction();
            _postgresMessageStorageTransaction = new PostgresMessageStorageTransaction(npgsqlTransaction, false, _jobQueueFor);
        }

        return _postgresMessageStorageTransaction;
    }

    public void UseTransaction(IMessageStorageTransaction borrowedTransaction)
    {
        if (borrowedTransaction is not PostgresMessageStorageTransaction postgresMessageStorageTransaction)
        {
            throw new ArgumentNotCompatibleException(typeof(PostgresMessageStorageTransaction), borrowedTransaction.GetType());
        }

        lock (_transactionLockObject)
        {
            if (IsMessageStorageTransactionUsable())
            {
                throw new TransactionAlreadyStartedException();
            }

            _postgresMessageStorageTransaction = postgresMessageStorageTransaction;
        }
    }

    public IMessageRepository GetMessageRepository()
    {
        NpgsqlConnection connection = IsMessageStorageTransactionUsable() ? _postgresMessageStorageTransaction!.NpgsqlTransaction!.Connection! : GetConnection();
        var postgresMessageRepository = new PostgresMessageRepository(_repositoryContextConfiguration,
                                                                      connection,
                                                                      _postgresMessageStorageTransaction);
        return postgresMessageRepository;
    }

    public IJobRepository GetJobRepository()
    {
        NpgsqlConnection connection = IsMessageStorageTransactionUsable() ? _postgresMessageStorageTransaction!.NpgsqlTransaction!.Connection! : GetConnection();
        var postgresJobRepository = new PostgresJobRepository(_repositoryContextConfiguration,
                                                              connection,
                                                              _postgresMessageStorageTransaction);
        return postgresJobRepository;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private NpgsqlConnection GetConnection()
    {
        if (_npgsqlConnection?.State is ConnectionState.Open)
        {
            return _npgsqlConnection;
        }

        _npgsqlConnection = new NpgsqlConnection(_repositoryContextConfiguration.ConnectionString);
        _npgsqlConnection.Open();
        return _npgsqlConnection;
    }

    private bool IsMessageStorageTransactionUsable()
    {
        if (_postgresMessageStorageTransaction != null && !_postgresMessageStorageTransaction.IsCommitted && !_postgresMessageStorageTransaction.IsDisposed)
        {
            return true;
        }

        return false;
    }

    public void Dispose()
    {
        _postgresMessageStorageTransaction?.Dispose();
        _npgsqlConnection?.Dispose();
    }
}