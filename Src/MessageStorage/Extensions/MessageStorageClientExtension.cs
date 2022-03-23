using System;
using System.Threading.Channels;
using MessageStorage.BackgroundServices;
using MessageStorage.MessageHandlers;
using MessageStorage.MessageStorageClients;
using MessageStorage.MonitorClients;
using MessageStorage.Processor;
using Microsoft.Extensions.DependencyInjection;

namespace MessageStorage.Extensions;

public static class MessageStorageClientExtension
{
    public static IServiceCollection AddMessageStorage(this IServiceCollection serviceCollection, Action<MessageStorageOptionsFor<DefaultMessageStorageClient>> messageStorageOptionBuilder)
    {
        AddMessageStorage<IMessageStorageClient, DefaultMessageStorageClient>(serviceCollection, messageStorageOptionBuilder);
        serviceCollection.AddScoped<IMonitorClient, BasicMonitorClientFor<DefaultMessageStorageClient>>();
        return serviceCollection;
    }

    public static IServiceCollection AddMessageStorage<TMessageStorageClientInterface, TMessageStorageClient>
        (this IServiceCollection serviceCollection, Action<MessageStorageOptionsFor<TMessageStorageClient>> messageStorageOptionBuilder)
        where TMessageStorageClientInterface : class, IMessageStorageClient
        where TMessageStorageClient : class, TMessageStorageClientInterface
    {
        var messageStorageOptions = new MessageStorageOptionsFor<TMessageStorageClient>(serviceCollection);
        messageStorageOptionBuilder.Invoke(messageStorageOptions);

        MessageStorageConfiguration<TMessageStorageClientInterface, TMessageStorageClient>(serviceCollection, messageStorageOptions);

        IMessageHandlerMetaDataHolderFor<TMessageStorageClient> metaDataHolder = new MessageHandlerMetaDataHolderFor<TMessageStorageClient>(messageStorageOptions.MessageHandlerMetaDataList);
        serviceCollection.AddSingleton(metaDataHolder);

        serviceCollection.AddScoped<TMessageStorageClientInterface, TMessageStorageClient>();
        serviceCollection.AddScoped<IMonitorClientFor<TMessageStorageClient>, BasicMonitorClientFor<TMessageStorageClient>>();

        serviceCollection.AddSingleton<IJobQueueFor<TMessageStorageClient>>(provider =>
        {
            var channel = Channel.CreateBounded<Job>(new BoundedChannelOptions(messageStorageOptions.JobQueueLength) { SingleReader = true });
            return new JobQueueFor<TMessageStorageClient>(channel.Writer, channel.Reader, provider.GetRequiredService<IJobExecutorFor<TMessageStorageClient>>());
        });
        serviceCollection.AddSingleton<IJobExecutorFor<TMessageStorageClient>, JobExecutorFor<TMessageStorageClient>>();
        serviceCollection.AddSingleton<IJobRescuerFor<TMessageStorageClient>, JobRescuerFor<TMessageStorageClient>>();
        serviceCollection.AddSingleton<IJobRetrierFor<TMessageStorageClient>, JobRetrierFor<TMessageStorageClient>>();

        serviceCollection.AddHostedService<StorageInitializerHostedService>();
        serviceCollection.AddHostedService<JobQueueConsumeTriggerHostedServiceFor<TMessageStorageClient>>();
        serviceCollection.AddHostedService<JobRescuerHostedServiceFor<TMessageStorageClient>>();
        serviceCollection.AddHostedService<JobRetrierHostedServiceFor<TMessageStorageClient>>();

        return serviceCollection;
    }

    private static void MessageStorageConfiguration<TMessageStorageClientInterface, TMessageStorageClient>(IServiceCollection serviceCollection, MessageStorageOptionsFor<TMessageStorageClient> messageStorageOptions)
        where TMessageStorageClientInterface : class, IMessageStorageClient
        where TMessageStorageClient : class, TMessageStorageClientInterface
    {
        var jobRescuerHostedServiceOption = new JobRescuerHostedServiceOptionFor<TMessageStorageClient>
                                            {
                                                Interval = messageStorageOptions.JobRescuerInterval
                                            };
        var jobRetrierHostedServiceOption = new JobRetrierHostedServiceOptionFor<TMessageStorageClient>
                                            {
                                                Interval = messageStorageOptions.JobRetrierInterval
                                            };

        var jobExecutorOptions = new JobExecutorOptionsFor<TMessageStorageClient>()
                                 {
                                     JobExecutionMaxDuration = messageStorageOptions.JobExecutionMaxDuration
                                 };
        var jobRetrierOptions = new JobRetrierOptionsFor<TMessageStorageClient>
                                {
                                    Concurrency = messageStorageOptions.JobHandlingConcurrency,
                                    FetchCount = messageStorageOptions.RetryJobFetchCount,
                                    WaitAfterFullFetch = messageStorageOptions.WaitAfterFullFetch
                                };

        serviceCollection.AddSingleton(jobRescuerHostedServiceOption);
        serviceCollection.AddSingleton(jobRetrierHostedServiceOption);
        serviceCollection.AddSingleton(jobExecutorOptions);
        serviceCollection.AddSingleton(jobRetrierOptions);
    }
}