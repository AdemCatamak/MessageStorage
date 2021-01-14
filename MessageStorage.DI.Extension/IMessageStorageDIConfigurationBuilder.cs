using System;
using MessageStorage.Clients;
using MessageStorage.Configurations;
using MessageStorage.DataAccessSection;
using MessageStorage.DI.Extension.Exceptions;

namespace MessageStorage.DI.Extension
{
    public interface IMessageStorageConfigurationBuilder<TMessageStorageClient>
        where TMessageStorageClient : IMessageStorageClient
    {
        IMessageStorageConfigurationBuilder<TMessageStorageClient> WithClientConfiguration(MessageStorageClientConfiguration configuration);
        IMessageStorageConfigurationBuilder<TMessageStorageClient> UseHandlers(Action<IHandlerManager, IServiceProvider> action);
        IMessageStorageConfigurationBuilder<TMessageStorageClient> UseRepositoryContextConfiguration(MessageStorageRepositoryContextConfiguration configuration);
        IMessageStorageConfigurationBuilder<TMessageStorageClient> UseRepositoryContext(Func<MessageStorageRepositoryContextConfiguration, IMessageStorageRepositoryContext> factory);
        IMessageStorageConfigurationBuilder<TMessageStorageClient> WithJobProcessorServer(JobProcessorConfiguration? configuration = null);
        public IMessageStorageConfigurationBuilder<TMessageStorageClient> Construct(Func<IMessageStorageRepositoryContext, IHandlerManager, MessageStorageClientConfiguration, TMessageStorageClient> factory);

        MessageStorageConfiguration<TMessageStorageClient> Build();
    }

    public class MessageStorageConfigurationBuilder<TMessageStorageClient>
        : IMessageStorageConfigurationBuilder<TMessageStorageClient>
        where TMessageStorageClient : IMessageStorageClient
    {
        private readonly IServiceProvider _serviceProvider;

        public MessageStorageConfigurationBuilder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public MessageStorageClientConfiguration MessageStorageClientConfiguration { get; private set; } = new MessageStorageClientConfiguration();
        public bool RunJobProcessor { get; private set; } = false;
        public JobProcessorConfiguration JobProcessorConfiguration { get; private set; } = new JobProcessorConfiguration();
        public MessageStorageRepositoryContextConfiguration? MessageStorageRepositoryContextConfiguration { get; private set; }
        public IHandlerManager HandlerManager { get; private set; } = new HandlerManager();
        public Func<MessageStorageRepositoryContextConfiguration, IMessageStorageRepositoryContext>? MessageStorageRepositoryContextFactory { get; private set; }
        public Func<IMessageStorageRepositoryContext, IHandlerManager, MessageStorageClientConfiguration, TMessageStorageClient>? MessageStorageClientFactory { get; private set; }

        public IMessageStorageConfigurationBuilder<TMessageStorageClient> WithClientConfiguration(MessageStorageClientConfiguration configuration)
        {
            MessageStorageClientConfiguration = configuration;
            return this;
        }

        public IMessageStorageConfigurationBuilder<TMessageStorageClient> UseHandlers(Action<IHandlerManager, IServiceProvider> action)
        {
            action.Invoke(HandlerManager, _serviceProvider);
            return this;
        }

        public IMessageStorageConfigurationBuilder<TMessageStorageClient> UseRepositoryContextConfiguration(MessageStorageRepositoryContextConfiguration configuration)
        {
            MessageStorageRepositoryContextConfiguration = configuration;
            return this;
        }

        public IMessageStorageConfigurationBuilder<TMessageStorageClient> UseRepositoryContext(Func<MessageStorageRepositoryContextConfiguration, IMessageStorageRepositoryContext> factory)
        {
            MessageStorageRepositoryContextFactory = factory;
            return this;
        }

        public IMessageStorageConfigurationBuilder<TMessageStorageClient> WithJobProcessorServer(JobProcessorConfiguration? configuration = null)
        {
            RunJobProcessor = true;
            if (configuration != null)
            {
                JobProcessorConfiguration = configuration;
            }

            return this;
        }

        public IMessageStorageConfigurationBuilder<TMessageStorageClient> Construct(Func<IMessageStorageRepositoryContext, IHandlerManager, MessageStorageClientConfiguration, TMessageStorageClient> factory)
        {
            MessageStorageClientFactory = factory;
            return this;
        }


        public MessageStorageConfiguration<TMessageStorageClient> Build()
        {
            if (MessageStorageRepositoryContextConfiguration == null)
            {
                throw new MessageStorageConfigurationBuilderException($"{nameof(UseRepositoryContextConfiguration)} should be called");
            }

            if (MessageStorageRepositoryContextFactory == null)
            {
                throw new MessageStorageConfigurationBuilderException($"{nameof(UseRepositoryContext)} should be called");
            }

            if (MessageStorageClientFactory == null)
            {
                throw new MessageStorageConfigurationBuilderException($"{nameof(Construct)} should be called");
            }

            MessageStorageConfiguration<TMessageStorageClient> configuration
                = new MessageStorageConfiguration<TMessageStorageClient>(MessageStorageRepositoryContextConfiguration,
                                                                         MessageStorageRepositoryContextFactory,
                                                                         MessageStorageClientFactory,
                                                                         HandlerManager,
                                                                         RunJobProcessor, JobProcessorConfiguration);

            return configuration;
        }
    }
}