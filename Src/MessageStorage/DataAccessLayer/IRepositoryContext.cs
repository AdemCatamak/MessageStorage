using MessageStorage.Processor;

namespace MessageStorage.DataAccessLayer;

public interface IRepositoryContext
{
    internal IJobQueue JobQueue { get; }

    IMessageStorageTransaction? CurrentMessageStorageTransaction { get; }
    IMessageStorageTransaction StartTransaction();
    void UseTransaction(IMessageStorageTransaction borrowedTransaction);

    IMessageRepository GetMessageRepository();
    IJobRepository GetJobRepository();
}

// tag interface for resolving
public interface IRepositoryContextFor<TMessageStorageClient> : IRepositoryContext
    where TMessageStorageClient : IMessageStorageClient
{
    internal IJobQueueFor<TMessageStorageClient> JobQueueFor { get; }
}