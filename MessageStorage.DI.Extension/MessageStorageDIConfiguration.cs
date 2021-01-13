using System;
using System.Collections.Generic;
using MessageStorage.Clients;
using MessageStorage.Configurations;
using MessageStorage.DataAccessSection;

namespace MessageStorage.DI.Extension
{
    public class MessageStorageDIConfiguration<TMessageStorageClient>
        where TMessageStorageClient : class, IMessageStorageClient
    {
        internal MessageStorageDIConfiguration(MessageStorageRepositoryContextConfiguration messageStorageRepositoryContextConfiguration,
                                               Func<MessageStorageRepositoryContextConfiguration, IMessageStorageRepositoryContext> messageStorageRepositoryContext,
                                               Func<IMessageStorageRepositoryContext, IHandlerManager, MessageStorageClientConfiguration, TMessageStorageClient> messageStorageClientFactory,
                                               IEnumerable<HandlerDescription> handlerDescriptions,
                                               MessageStorageClientConfiguration? messageStorageClientConfiguration = null,
                                               JobProcessorConfiguration? jobProcessorConfiguration = null,
                                               bool runJobProcessor = false)
        {
            MessageStorageRepositoryContextConfiguration = messageStorageRepositoryContextConfiguration;
            MessageStorageRepositoryContextFactory = messageStorageRepositoryContext;
            MessageStorageClientFactory = messageStorageClientFactory;
            HandlerDescriptions = handlerDescriptions;
            MessageStorageClientConfiguration = messageStorageClientConfiguration ?? new MessageStorageClientConfiguration();
            JobProcessorConfiguration = jobProcessorConfiguration ?? new JobProcessorConfiguration();
            RunJobProcessor = runJobProcessor;
        }

        public MessageStorageRepositoryContextConfiguration MessageStorageRepositoryContextConfiguration { get; }
        public Func<MessageStorageRepositoryContextConfiguration, IMessageStorageRepositoryContext> MessageStorageRepositoryContextFactory { get; }
        public Func<IMessageStorageRepositoryContext, IHandlerManager, MessageStorageClientConfiguration, TMessageStorageClient> MessageStorageClientFactory { get; }
        public IEnumerable<HandlerDescription> HandlerDescriptions { get; }
        public MessageStorageClientConfiguration MessageStorageClientConfiguration { get; }
        public JobProcessorConfiguration? JobProcessorConfiguration { get; }
        public bool RunJobProcessor { get; }
    }
}