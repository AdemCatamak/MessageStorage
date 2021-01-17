using System;
using MessageStorage.Clients;
using MessageStorage.Clients.Imp;
using MessageStorage.Configurations;
using MessageStorage.DataAccessSection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MessageStorage.DI.Extension
{
    public static class MessageStorageDIExtensions
    {
        public static (IServiceCollection, Action<IMessageStorageConfigurationBuilder<MessageStorageClient>>) AddMessageStorage(this IServiceCollection serviceCollection, Action<IMessageStorageConfigurationBuilder<MessageStorageClient>> messageStorage)
        {
            void MessageStorageConfigurationBuilderWithDefaultConstructor(IMessageStorageConfigurationBuilder<MessageStorageClient> builder)
            {
                builder.Construct((context, manager, configuration) => new MessageStorageClient(context, manager, configuration));
                messageStorage.Invoke(builder);
            }

            return AddMessageStorage<IMessageStorageClient, MessageStorageClient>(serviceCollection, MessageStorageConfigurationBuilderWithDefaultConstructor);
        }

        public static (IServiceCollection, Action<IMessageStorageConfigurationBuilder<TImpMessageStorageClient>>) AddMessageStorage<TImpMessageStorageClient>(this IServiceCollection serviceCollection, Action<IMessageStorageConfigurationBuilder<TImpMessageStorageClient>> messageStorage)
            where TImpMessageStorageClient : MessageStorageClient
        {
            return AddMessageStorage<TImpMessageStorageClient, TImpMessageStorageClient>(serviceCollection, messageStorage);
        }

        public static (IServiceCollection, Action<IMessageStorageConfigurationBuilder<TImpMessageStorageClient>>) AddMessageStorage<TMessageStorageClient, TImpMessageStorageClient>(this IServiceCollection serviceCollection, Action<IMessageStorageConfigurationBuilder<TImpMessageStorageClient>> messageStorage)
            where TMessageStorageClient : class, IMessageStorageClient
            where TImpMessageStorageClient : MessageStorageClient, TMessageStorageClient
        {
            MessageStorageConfiguration<TImpMessageStorageClient>? messageStorageConfiguration = null;

            serviceCollection.AddScoped<TMessageStorageClient, TImpMessageStorageClient>(provider =>
                                                                                         {
                                                                                             IServiceScope scope = provider.CreateScope();
                                                                                             IServiceProvider localServiceProvider = scope.ServiceProvider;

                                                                                             if (messageStorageConfiguration == null)
                                                                                             {
                                                                                                 IMessageStorageConfigurationBuilder<TImpMessageStorageClient> builder = new MessageStorageConfigurationBuilder<TImpMessageStorageClient>(localServiceProvider);
                                                                                                 messageStorage.Invoke(builder);
                                                                                                 messageStorageConfiguration = builder.Build();
                                                                                             }

                                                                                             IMessageStorageRepositoryContext? messageStorageRepositoryContext = messageStorageConfiguration.MessageStorageRepositoryContextFactory.Invoke(messageStorageConfiguration.MessageStorageRepositoryContextConfiguration);
                                                                                             TImpMessageStorageClient messageStorageClient = messageStorageConfiguration.MessageStorageClientFactory.Invoke(messageStorageRepositoryContext,
                                                                                                                                                                                                            messageStorageConfiguration.HandlerManager,
                                                                                                                                                                                                            messageStorageConfiguration.MessageStorageClientConfiguration);
                                                                                             return messageStorageClient;
                                                                                         });

            return (serviceCollection, messageStorage);
        }

        public static (IServiceCollection, Action<IMessageStorageConfigurationBuilder<TImpMessageStorageClient>>) WithJobProcessor<TImpMessageStorageClient>(this (IServiceCollection, Action<IMessageStorageConfigurationBuilder<TImpMessageStorageClient>>) messageStorage, JobProcessorConfiguration? jobProcessorConfiguration = null)
            where TImpMessageStorageClient : MessageStorageClient
        {
            jobProcessorConfiguration ??= new JobProcessorConfiguration();

            MessageStorageConfiguration<TImpMessageStorageClient>? messageStorageConfiguration = null;

            IServiceCollection serviceCollection = messageStorage.Item1;
            Action<IMessageStorageConfigurationBuilder<TImpMessageStorageClient>>? configure = messageStorage.Item2;

            serviceCollection.AddSingleton<IBackgroundProcessor>(provider =>
                                                                 {
                                                                     if (messageStorageConfiguration == null)
                                                                     {
                                                                         IServiceScope scope = provider.CreateScope();
                                                                         IServiceProvider localServiceProvider = scope.ServiceProvider;
                                                                         IMessageStorageConfigurationBuilder<TImpMessageStorageClient> builder
                                                                             = new MessageStorageConfigurationBuilder<TImpMessageStorageClient>(localServiceProvider);
                                                                         configure(builder);
                                                                         messageStorageConfiguration = builder.Build();
                                                                     }

                                                                     var backgroundProcessor = new JobProcessor(() => messageStorageConfiguration.MessageStorageRepositoryContextFactory.Invoke(messageStorageConfiguration.MessageStorageRepositoryContextConfiguration),
                                                                                                                messageStorageConfiguration.HandlerManager,
                                                                                                                provider.GetService<ILogger<JobProcessor>>(),
                                                                                                                jobProcessorConfiguration);

                                                                     return backgroundProcessor!;
                                                                 });

            return messageStorage;
        }
    }
}