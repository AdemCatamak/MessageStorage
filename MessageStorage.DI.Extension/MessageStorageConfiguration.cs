using System;
using MessageStorage.Configurations;
using MessageStorage.DataAccessSection;

namespace MessageStorage.DI.Extension
{
    public class MessageStorageConfiguration<TMessageStorageClient>
    {
        public MessageStorageRepositoryContextConfiguration MessageStorageRepositoryContextConfiguration { get; private set; }
        public Func<MessageStorageRepositoryContextConfiguration, IMessageStorageRepositoryContext> MessageStorageRepositoryContextFactory { get; private set; }
        public Func<IMessageStorageRepositoryContext, IHandlerManager, MessageStorageClientConfiguration, TMessageStorageClient> MessageStorageClientFactory { get; private set; }
        public IHandlerManager HandlerManager { get; private set; }
        public MessageStorageClientConfiguration MessageStorageClientConfiguration { get; private set; } = new MessageStorageClientConfiguration();
        public bool RunJobProcessor { get; private set; }
        public JobProcessorConfiguration JobProcessorConfiguration { get; private set; }

        public MessageStorageConfiguration(MessageStorageRepositoryContextConfiguration messageStorageRepositoryContextConfiguration,
                                           Func<MessageStorageRepositoryContextConfiguration, IMessageStorageRepositoryContext> messageStorageRepositoryContextFactory,
                                           Func<IMessageStorageRepositoryContext, IHandlerManager, MessageStorageClientConfiguration, TMessageStorageClient> messageStorageClientFactory,
                                           IHandlerManager handlerManager,
                                           bool runJobProcessor,
                                           JobProcessorConfiguration jobProcessorConfiguration)
        {
            MessageStorageRepositoryContextConfiguration = messageStorageRepositoryContextConfiguration;
            MessageStorageRepositoryContextFactory = messageStorageRepositoryContextFactory;
            MessageStorageClientFactory = messageStorageClientFactory;
            HandlerManager = handlerManager;
            RunJobProcessor = runJobProcessor;
            JobProcessorConfiguration = jobProcessorConfiguration;
        }
    }
}