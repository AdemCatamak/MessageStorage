using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer;
using MessageStorage.Processor;
using Npgsql;

namespace MessageStorage.Postgres.DataAccessLayer;

public class PostgresMessageStorageTransaction : IMessageStorageTransaction
{
    private ConcurrentQueue<Job> _jobToBeDispatched;

    public bool IsCommitted { get; private set; }
    public bool IsDisposed { get; private set; }
    public NpgsqlTransaction NpgsqlTransaction { get; private set; }
    private readonly bool _isBorrowed;
    private readonly IJobQueue _jobQueue;

    public PostgresMessageStorageTransaction(NpgsqlTransaction npgsqlTransaction, bool isBorrowed, IJobQueue jobQueue)
    {
        NpgsqlTransaction = npgsqlTransaction;
        _isBorrowed = isBorrowed;
        _jobQueue = jobQueue;

        _jobToBeDispatched = new ConcurrentQueue<Job>();
    }


    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        await NpgsqlTransaction.CommitAsync(cancellationToken);
        IsCommitted = true;
        while (_jobToBeDispatched.TryDequeue(out Job job))
        {
            _jobQueue.Enqueue(job);
        }
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
            NpgsqlTransaction?.Dispose();
        }
    }
}