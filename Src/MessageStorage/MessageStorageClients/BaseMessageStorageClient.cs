using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer;
using MessageStorage.MessageHandlers;
using MessageStorage.Processor;

namespace MessageStorage.MessageStorageClients;

public abstract class BaseMessageStorageClient<TMessageStorageClient> : IMessageStorageClient
    where TMessageStorageClient : IMessageStorageClient
{
    IRepositoryContext IMessageStorageClient.RepositoryContext => _repositoryContext;

    private readonly IRepositoryContextFor<TMessageStorageClient> _repositoryContext;
    private readonly IMessageHandlerMetaDataHolderFor<TMessageStorageClient> _metaDataHolder;
    
    protected BaseMessageStorageClient(IRepositoryContextFor<TMessageStorageClient> repositoryContext, IMessageHandlerMetaDataHolderFor<TMessageStorageClient> metaDataHolder)
    {
        _repositoryContext = repositoryContext;
        _metaDataHolder = metaDataHolder;
    }


    public void UseTransaction(IMessageStorageTransaction messageStorageTransaction)
    {
        _repositoryContext.UseTransaction(messageStorageTransaction);
    }

    public IMessageStorageTransaction StartTransaction()
    {
        return _repositoryContext.StartTransaction();
    }

    public async Task<(Message, List<Job>)> AddMessageAsync(object payload, CancellationToken cancellationToken)
    {
        if (_repositoryContext.CurrentMessageStorageTransaction == null)
        {
            using IMessageStorageTransaction transaction = _repositoryContext.StartTransaction();
            (Message, List<Job>) result = await AddMessageInternalAsync(payload, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return result;
        }
        else
        {
            (Message, List<Job>) result = await AddMessageInternalAsync(payload, cancellationToken);
            return result;
        }
    }

    private async Task<(Message, List<Job>)> AddMessageInternalAsync(object payload, CancellationToken cancellationToken)
    {
        var message = new Message(payload);
        IReadOnlyList<MessageHandlerMetaData> availableMessageHandlerMetaDataList = _metaDataHolder.GetAvailableMessageHandlerMetaDataCollection(payload.GetType());
        List<Job> jobs = availableMessageHandlerMetaDataList.Select(metaData => new Job(message, metaData.MessageHandlerType.FullName!, metaData.MaxRetryCount)).ToList();
        jobs.ForEach(j => j.SetInProgress());

        await _repositoryContext.GetMessageRepository().AddAsync(message, cancellationToken);
        await _repositoryContext.GetJobRepository().AddAsync(jobs, cancellationToken);

        return (message, jobs);
    }
}