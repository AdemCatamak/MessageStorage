using System;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer;

namespace MessageStorage.Processor;

public interface IStorageCleanerFor<TMessageStorageClient>
    where TMessageStorageClient : IMessageStorageClient
{
    Task CleanJobsAsync(DateTime lastOperationTimeBeforeThen, bool removeOnlySucceeded, CancellationToken cancellationToken);
    Task CleanMessageAsync(DateTime createdBeforeThen, CancellationToken cancellationToken);
}

public class StorageCleanerFor<TMessageStorageClient> : IStorageCleanerFor<TMessageStorageClient>
    where TMessageStorageClient : IMessageStorageClient
{
    private readonly IRepositoryContextFor<TMessageStorageClient> _repositoryContext;

    public StorageCleanerFor(IRepositoryContextFor<TMessageStorageClient> repositoryContext)
    {
        _repositoryContext = repositoryContext;
    }

    public async Task CleanJobsAsync(DateTime lastOperationTimeBeforeThen, bool removeOnlySucceeded, CancellationToken cancellationToken)
    {
        IJobRepository jobRepository = _repositoryContext.GetJobRepository();
        await jobRepository.CleanAsync(lastOperationTimeBeforeThen, removeOnlySucceeded, cancellationToken);
    }

    public async Task CleanMessageAsync(DateTime createdBeforeThen, CancellationToken cancellationToken)
    {
        IMessageRepository messageRepository = _repositoryContext.GetMessageRepository();
        await messageRepository.CleanAsync(createdBeforeThen, cancellationToken);
    }
}