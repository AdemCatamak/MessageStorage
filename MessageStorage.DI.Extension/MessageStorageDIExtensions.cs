using System;
using MessageStorage.Clients;
using MessageStorage.Clients.Imp;
using MessageStorage.DataAccessSection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MessageStorage.DI.Extension
{
    public static class MessageStorageDIExtensions
    {
        public static IServiceCollection AddMessageStorage(this IServiceCollection serviceCollection, Action<IMessageStorageConfigurationBuilder<MessageStorageClient>> messageStorage)
        {
            void MessageStorageConfigurationBuilderWithDefaultConstructor(IMessageStorageConfigurationBuilder<MessageStorageClient> builder)
            {
                builder.Construct((context, manager, configuration) => new MessageStorageClient(context, manager, configuration));
                messageStorage.Invoke(builder);
            }

            return AddMessageStorage<IMessageStorageClient, MessageStorageClient>(serviceCollection, MessageStorageConfigurationBuilderWithDefaultConstructor);
        }


        public static IServiceCollection AddMessageStorage<TMessageStorageClient, TImpMessageStorageClient>(this IServiceCollection serviceCollection, Action<IMessageStorageConfigurationBuilder<TImpMessageStorageClient>> messageStorage)
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

            serviceCollection.AddSingleton<IHostedService>(provider =>
                                                           {
                                                               if (messageStorageConfiguration == null)
                                                               {
                                                                   IServiceScope scope = provider.CreateScope();
                                                                   IServiceProvider localServiceProvider = scope.ServiceProvider;
                                                                   IMessageStorageConfigurationBuilder<TImpMessageStorageClient> builder
                                                                       = new MessageStorageConfigurationBuilder<TImpMessageStorageClient>(localServiceProvider);
                                                                   messageStorage.Invoke(builder);
                                                                   messageStorageConfiguration = builder.Build();
                                                               }

                                                               IBackgroundProcessor backgroundProcessor;
                                                               if (messageStorageConfiguration.RunJobProcessor)
                                                               {
                                                                   backgroundProcessor = new JobProcessor(() => messageStorageConfiguration.MessageStorageRepositoryContextFactory.Invoke(messageStorageConfiguration.MessageStorageRepositoryContextConfiguration),
                                                                                                          messageStorageConfiguration.HandlerManager,
                                                                                                          provider.GetService<ILogger<JobProcessor>>());
                                                               }
                                                               else
                                                               {
                                                                   backgroundProcessor = new DummyBackgroundProcessor(provider.GetService<ILogger<DummyBackgroundProcessor>>());
                                                               }

                                                               IHostedService? backgroundProcessorHostedService = new BackgroundProcessorHostedService(backgroundProcessor);

                                                               return backgroundProcessorHostedService!;
                                                           });
            return serviceCollection;
        }
    }
}