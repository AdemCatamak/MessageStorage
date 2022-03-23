using System;
using System.Collections.Generic;
using MessageStorage.DataAccessLayer;
using MessageStorage.MessageHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace MessageStorage.Extensions;

public class MessageStorageOptionsFor<TMessageStorageClient>
    where TMessageStorageClient : IMessageStorageClient
{
    private readonly IServiceCollection _serviceCollection;
    internal List<MessageHandlerMetaData> MessageHandlerMetaDataList { get; } = new();
   
    public TimeSpan JobRescuerInterval = TimeSpan.FromMinutes(5);
    public TimeSpan JobRetrierInterval = TimeSpan.FromMinutes(5);
    public TimeSpan StorageCleanerInterval = TimeSpan.FromMinutes(30);
    
    public int JobQueueLength = 1000;
    public int JobHandlingConcurrency = 4;
    public int RetryJobFetchCount = 20;
    public TimeSpan WaitAfterFullFetch = TimeSpan.FromSeconds(5);
    public TimeSpan JobExecutionMaxDuration = TimeSpan.FromSeconds(120);
    public TimeSpan FinalizedEntityRemovedAfter = TimeSpan.FromHours(24);
    public bool RemoveOnlySucceededJobs = true;
    public bool RemoveMessages = true;

    internal MessageStorageOptionsFor(IServiceCollection serviceCollection)
    {
        _serviceCollection = serviceCollection;
    }

    public void RegisterHandler<TMessageHandler, TMessage>(int maxRetryCount = 10)
        where TMessageHandler : BaseMessageHandler<TMessage>
        where TMessage : class
    {
        RegisterHandler(typeof(TMessageHandler), typeof(TMessage), maxRetryCount);
    }

    internal void RegisterHandler(Type messageHandlerType, Type messageType, int maxRetryCount)
    {
        _serviceCollection.AddTransient(typeof(IMessageHandler), messageHandlerType);
        _serviceCollection.AddTransient(messageHandlerType);

        var metaData = new MessageHandlerMetaData(messageHandlerType, messageType, maxRetryCount);
        MessageHandlerMetaDataList.RemoveAll(data => data.MessageHandlerType == messageHandlerType && data.MessageType == messageType);
        MessageHandlerMetaDataList.Add(metaData);
    }

    public void RegisterRepositoryContextFor(Func<IStorageInitializeEngine> storageInitializeEngineFactory,
                                             Func<IServiceProvider, IRepositoryContextFor<TMessageStorageClient>> repositoryContextFactory)
    {
        _serviceCollection.AddSingleton(_ => storageInitializeEngineFactory.Invoke());
        _serviceCollection.AddScoped(repositoryContextFactory);
    }
}