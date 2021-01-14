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
            AddMessageStorage<IMessageStorageClient>(serviceCollection,
                                                     builder =>
                                                     {
                                                         builder.UseMessageStorageClientFactoryMethod((context, manager, clientConfiguration) => new MessageStorageClient(context, manager, clientConfiguration));
                                                         options.Invoke(builder);
                                                     });
        }

        public static void AddMessageStorage(this IServiceCollection serviceCollection, Action<IMessageStorageDIConfigurationBuilder<IMessageStorageClient>, IServiceProvider> options)
        {
            AddMessageStorage<IMessageStorageClient>(serviceCollection,
                                                     (builder, provider) =>
                                                     {
                                                         builder.UseMessageStorageClientFactoryMethod((context, manager, clientConfiguration) => new MessageStorageClient(context, manager, clientConfiguration));
                                                         options.Invoke(builder, provider);
                                                     });
        }

        public static void AddMessageStorage<TMessageStorageClient>(this IServiceCollection serviceCollection, Action<IMessageStorageDIConfigurationBuilder<TMessageStorageClient>> options)
            where TMessageStorageClient : class, IMessageStorageClient
        {
            AddMessageStorage<TMessageStorageClient>(serviceCollection, (builder, provider) => { options.Invoke(builder); });
        }


        public static void AddMessageStorage<TMessageStorageClient>(this IServiceCollection serviceCollection, Action<IMessageStorageDIConfigurationBuilder<TMessageStorageClient>, IServiceProvider> options)
            where TMessageStorageClient : class, IMessageStorageClient
        {
            HandlerManager? h1 = null;
            MessageStorageDIConfiguration<TMessageStorageClient>? c1 = null;

            IMessageStorageDIConfigurationBuilder<TMessageStorageClient> builder = new MessageStorageDIConfigurationBuilder<TMessageStorageClient>();
            serviceCollection.AddScoped(provider =>
                                        {
                                            if (h1 == null)
                                            {
                                                IServiceScope scope = provider.CreateScope();
                                                IServiceProvider p = scope.ServiceProvider;
                                                options(builder, p);
                                                c1 = builder.Build();
                                                h1 = new HandlerManager(c1.HandlerDescriptions);
                                            }

                                            IMessageStorageRepositoryContext messageStorageRepositoryContext = c1.MessageStorageRepositoryContextFactory.Invoke(c1.MessageStorageRepositoryContextConfiguration);
                                            TMessageStorageClient messageStorageClient = c1.MessageStorageClientFactory.Invoke(messageStorageRepositoryContext, h1, c1.MessageStorageClientConfiguration);
                                            return messageStorageClient;
                                        });

            MessageStorageDIConfiguration<TMessageStorageClient>? c2;
            serviceCollection.AddSingleton<IHostedService>(provider =>
                                                           {
                                                               IServiceScope scope = provider.CreateScope();
                                                               IServiceProvider p = scope.ServiceProvider;
                                                               options(builder, p);
                                                               c2 = builder.Build();

                                                               IBackgroundProcessor backgroundProcessor;
                                                               if (c2.RunJobProcessor)
                                                               {
                                                                   var handlerManager = new HandlerManager(c2.HandlerDescriptions);
                                                                   backgroundProcessor = new JobProcessor(() => c2.MessageStorageRepositoryContextFactory.Invoke(c2.MessageStorageRepositoryContextConfiguration),
                                                                                                          handlerManager,
                                                                                                          NullLogger<JobProcessor>.Instance,
                                                                                                          c2.JobProcessorConfiguration);
                                                               }
                                                               else
                                                               {
                                                                   backgroundProcessor = new DummyBackgroundProcessor();
                                                               }

                                                               var backgroundProcessorHostedService = new BackgroundProcessorHostedService(backgroundProcessor);
                                                               return backgroundProcessorHostedService;
                                                           });
        }
    }
}