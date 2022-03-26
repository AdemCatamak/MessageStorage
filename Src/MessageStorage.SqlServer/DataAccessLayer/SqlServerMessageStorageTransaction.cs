using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer;
using MessageStorage.Processor;
using Microsoft.Data.SqlClient;

namespace MessageStorage.SqlServer.DataAccessLayer;

public class SqlServerMessageStorageTransaction : IMessageStorageTransaction
{
    private readonly ConcurrentQueue<Job> _jobToBeDispatched;

    public bool IsCommitted { get; private set; }
    public bool IsDisposed { get; private set; }
    public SqlTransaction SqlTransaction { get; private set; }
    private readonly bool _isBorrowed;
    private readonly IJobQueue _jobQueue;


    public SqlServerMessageStorageTransaction(SqlTransaction sqlTransaction, bool isBorrowed, IJobQueue jobQueue)
    {
        SqlTransaction = sqlTransaction;
        _isBorrowed = isBorrowed;
        _jobQueue = jobQueue;

        _jobToBeDispatched = new ConcurrentQueue<Job>();
    }


    public Task CommitAsync(CancellationToken cancellationToken)
    {
        SqlTransaction.Commit();
        IsCommitted = true;
        while (_jobToBeDispatched.TryDequeue(out Job? job))
        {
            _jobQueue.Enqueue(job);
        }

        return Task.CompletedTask;
    }

    void IMessageStorageTransaction.AddJobToBeDispatched(Job job)
    {
        _jobToBeDispatched.Enqueue(job);
    }

    public void Dispose()
    {
        IsDisposed = true;
        if (!_isBorrowed)
        {
            SqlTransaction?.Dispose();
        }
    }
}