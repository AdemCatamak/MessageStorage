using System;
using System.Collections.Generic;
using MessageStorage.Clients;
using MessageStorage.Configurations;
using MessageStorage.DataAccessSection;
using MessageStorage.DI.Extension.Exceptions;

namespace MessageStorage.DI.Extension
{
    public interface IMessageStorageDIConfigurationBuilder<TMessageStorageClient>
        where TMessageStorageClient : class, IMessageStorageClient
    {
        IMessageStorageDIConfigurationBuilder<TMessageStorageClient> RunJob();
        IMessageStorageDIConfigurationBuilder<TMessageStorageClient> RunJob(JobProcessorConfiguration withConfiguration);

        IMessageStorageDIConfigurationBuilder<TMessageStorageClient> UseRepositoryContextConfiguration(MessageStorageRepositoryContextConfiguration configuration);
        IMessageStorageDIConfigurationBuilder<TMessageStorageClient> UseMessageStorageClientConfiguration(MessageStorageClientConfiguration configuration);

        IMessageStorageDIConfigurationBuilder<TMessageStorageClient> UseRepositoryContextFactoryMethod(Func<MessageStorageRepositoryContextConfiguration, IMessageStorageRepositoryContext> factoryMethod);
        IMessageStorageDIConfigurationBuilder<TMessageStorageClient> UseMessageStorageClientFactoryMethod(Func<IMessageStorageRepositoryContext, IHandlerManager, MessageStorageClientConfiguration, TMessageStorageClient> factoryMethod);

        IMessageStorageDIConfigurationBuilder<TMessageStorageClient> AddHandlerDescription(HandlerDescription handlerDescription);
        MessageStorageDIConfiguration<TMessageStorageClient> Build();
    }

    public class MessageStorageDIConfigurationBuilder<TMessageStorageClient> : IMessageStorageDIConfigurationBuilder<TMessageStorageClient>
        where TMessageStorageClient : class, IMessageStorageClient
    {
        private MessageStorageRepositoryContextConfiguration? _repositoryContextConfiguration;
        private Func<MessageStorageRepositoryContextConfiguration, IMessageStorageRepositoryContext>? _messageStorageRepositoryContextFactory;
        private Func<IMessageStorageRepositoryContext, IHandlerManager, MessageStorageClientConfiguration, TMessageStorageClient> _messageStorageClientFactory;
        private readonly List<HandlerDescription> _handlerDescriptions = new List<HandlerDescription>();
        private MessageStorageClientConfiguration? _messageStorageClientConfiguration;
        private JobProcessorConfiguration? _jobProcessorConfiguration;
        private bool _runJobProcessor;

        public IMessageStorageDIConfigurationBuilder<TMessageStorageClient> RunJob()
        {
            _runJobProcessor = true;
            return this;
        }

        public IMessageStorageDIConfigurationBuilder<TMessageStorageClient> RunJob(JobProcessorConfiguration withConfiguration)
        {
            _runJobProcessor = true;
            _jobProcessorConfiguration = withConfiguration;
            return this;
        }

        public IMessageStorageDIConfigurationBuilder<TMessageStorageClient> UseRepositoryContextConfiguration(MessageStorageRepositoryContextConfiguration configuration)
        {
            _repositoryContextConfiguration = configuration;
            return this;
        }

        public IMessageStorageDIConfigurationBuilder<TMessageStorageClient> UseMessageStorageClientConfiguration(MessageStorageClientConfiguration configuration)
        {
            _messageStorageClientConfiguration = configuration;
            return this;
        }

        public IMessageStorageDIConfigurationBuilder<TMessageStorageClient> UseRepositoryContextFactoryMethod(Func<MessageStorageRepositoryContextConfiguration, IMessageStorageRepositoryContext> factoryMethod)
        {
            _messageStorageRepositoryContextFactory = factoryMethod;
            return this;
        }

        public IMessageStorageDIConfigurationBuilder<TMessageStorageClient> UseMessageStorageClientFactoryMethod(Func<IMessageStorageRepositoryContext, IHandlerManager, MessageStorageClientConfiguration, TMessageStorageClient> factoryMethod)
        {
            _messageStorageClientFactory = factoryMethod;
            return this;
        }

        public IMessageStorageDIConfigurationBuilder<TMessageStorageClient> AddHandlerDescription(HandlerDescription handlerDescription)
        {
            _handlerDescriptions.Add(handlerDescription);
            return this;
        }


        public MessageStorageDIConfiguration<TMessageStorageClient> Build()
        {
            if (_repositoryContextConfiguration == null)
            {
                throw new MessageStorageDIConfigurationBuilderException($"{nameof(UseRepositoryContextConfiguration)} method should be used");
            }

            if (_messageStorageRepositoryContextFactory == null)
            {
                throw new MessageStorageDIConfigurationBuilderException($"{nameof(UseRepositoryContextFactoryMethod)} method should be used");
            }

            if (_messageStorageClientFactory == null)
            {
                throw new MessageStorageDIConfigurationBuilderException($"{nameof(UseMessageStorageClientFactoryMethod)} method should be used");
            }

            MessageStorageDIConfiguration<TMessageStorageClient> configuration
                = new MessageStorageDIConfiguration<TMessageStorageClient>(_repositoryContextConfiguration,
                                                                           _messageStorageRepositoryContextFactory,
                                                                           _messageStorageClientFactory,
                                                                           _handlerDescriptions,
                                                                           _messageStorageClientConfiguration,
                                                                           _jobProcessorConfiguration,
                                                                           _runJobProcessor);

            return configuration;
        }
    }
}