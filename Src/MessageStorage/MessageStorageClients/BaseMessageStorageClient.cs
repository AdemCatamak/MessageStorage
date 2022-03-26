using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer;
using MessageStorage.MessageHandlers;
using Microsoft.Extensions.Logging;

namespace MessageStorage.MessageStorageClients;

public abstract class BaseMessageStorageClient<TMessageStorageClient> : IMessageStorageClient
    where TMessageStorageClient : IMessageStorageClient
{
    IRepositoryContext IMessageStorageClient.RepositoryContext => _repositoryContext;

    private readonly IRepositoryContextFor<TMessageStorageClient> _repositoryContext;
    private readonly IMessageHandlerMetaDataHolderFor<TMessageStorageClient> _metaDataHolder;
    private readonly ILogger<BaseMessageStorageClient<TMessageStorageClient>> _logger;

    protected BaseMessageStorageClient(IRepositoryContextFor<TMessageStorageClient> repositoryContext, IMessageHandlerMetaDataHolderFor<TMessageStorageClient> metaDataHolder, ILogger<BaseMessageStorageClient<TMessageStorageClient>> logger)
    {
        _repositoryContext = repositoryContext;
        _metaDataHolder = metaDataHolder;
        _logger = logger;
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
        (Message? message, List<Job>? jobList) = CreateEntity(payload);

        if (_repositoryContext.CurrentMessageStorageTransaction == null)
        {
            var stopwatch = Stopwatch.StartNew();
            using IMessageStorageTransaction? transaction = _repositoryContext.StartTransaction();
            await AddMessageInternalAsync(message, jobList, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            stopwatch.Stop();

            _logger.LogDebug("{Now} | ElapsedTime : {ElapsedTime} ms | {MessageId} inserted", DateTime.UtcNow, stopwatch.ElapsedMilliseconds, message.Id);

            return (message, jobList);
        }
        else
        {
            var stopwatch = Stopwatch.StartNew();
            await AddMessageInternalAsync(message, jobList, cancellationToken);
            stopwatch.Stop();

            _logger.LogDebug("{Now} | ElapsedTime : {ElapsedTime} ms | {MessageId} inserted (Borrowed transaction should be committed)", DateTime.UtcNow, stopwatch.ElapsedMilliseconds, message.Id);

            return (message, jobList);
        }
    }

    private (Message, List<Job>) CreateEntity(object payload)
    {
        var message = new Message(payload);
        IReadOnlyList<MessageHandlerMetaData>? availableMessageHandlerMetaDataList = _metaDataHolder.GetAvailableMessageHandlerMetaDataCollection(payload.GetType());
        List<Job>? jobs = availableMessageHandlerMetaDataList.Select(metaData => new Job(message, metaData.MessageHandlerType.FullName!, metaData.MaxRetryCount)).ToList();
        jobs.ForEach(j => j.SetInProgress());
        return (message, jobs);
    }

    private async Task AddMessageInternalAsync(Message message, List<Job> jobList, CancellationToken cancellationToken)
    {
        await _repositoryContext.GetMessageRepository().AddAsync(message, cancellationToken);
        await _repositoryContext.GetJobRepository().AddAsync(jobList, cancellationToken);
    }

    public void Dispose()
    {
        _repositoryContext.Dispose();
    }
}