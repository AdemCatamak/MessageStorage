using System;
using MessageStorage.Clients;
using MessageStorage.Clients.Imp;
using MessageStorage.DataAccessSection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;

namespace MessageStorage.DI.Extension
{
    public static class MessageStorageDIExtensions
    {
        public static void AddMessageStorage(this IServiceCollection serviceCollection, Action<IMessageStorageDIConfigurationBuilder<IMessageStorageClient>> options)
        {
            IMessageStorageDIConfigurationBuilder<IMessageStorageClient> builder = new MessageStorageDIConfigurationBuilder<IMessageStorageClient>();
            builder.UseMessageStorageClientFactoryMethod((context, manager, clientConfiguration) => new MessageStorageClient(context, manager, clientConfiguration));
            options(builder);
            MessageStorageDIConfiguration<IMessageStorageClient> configuration = builder.Build();

            AddMessageStorage(serviceCollection, configuration);
        }

        public static void AddMessageStorage<TMessageStorageClient>(this IServiceCollection serviceCollection, Action<IMessageStorageDIConfigurationBuilder<TMessageStorageClient>> options)
            where TMessageStorageClient : class, IMessageStorageClient
        {
            IMessageStorageDIConfigurationBuilder<TMessageStorageClient> builder = new MessageStorageDIConfigurationBuilder<TMessageStorageClient>();
            options(builder);
            MessageStorageDIConfiguration<TMessageStorageClient> configuration = builder.Build();

            AddMessageStorage(serviceCollection, configuration);
        }

        private static void AddMessageStorage<TMessageStorageClient>(this IServiceCollection serviceCollection, MessageStorageDIConfiguration<TMessageStorageClient> configuration)
            where TMessageStorageClient : class, IMessageStorageClient
        {
            var handlerManager = new HandlerManager(configuration.Handlers);

            serviceCollection.AddScoped(provider =>
                                        {
                                            IMessageStorageRepositoryContext messageStorageRepositoryContext = configuration.MessageStorageRepositoryContextFactory.Invoke(configuration.MessageStorageRepositoryContextConfiguration);
                                            TMessageStorageClient messageStorageClient = configuration.MessageStorageClientFactory.Invoke(messageStorageRepositoryContext, handlerManager, configuration.MessageStorageClientConfiguration);
                                            return messageStorageClient;
                                        });

            if (configuration.RunJobProcessor)
            {
                serviceCollection.AddSingleton<IHostedService>(provider =>
                                                                   new JobProcessorHostedService(
                                                                                                 new JobProcessor(() => configuration.MessageStorageRepositoryContextFactory.Invoke(configuration.MessageStorageRepositoryContextConfiguration),
                                                                                                                  handlerManager,
                                                                                                                  NullLogger<IJobProcessor>.Instance,
                                                                                                                  configuration.JobProcessorConfiguration)));
            }
        }
    }
}