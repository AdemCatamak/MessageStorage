using System;
using MessageStorage.Clients;
using Microsoft.Extensions.DependencyInjection;

namespace MessageStorage.DI.Extension
{
    public interface IMessageStorageServiceCollection
    {
        MessageStorageServiceCollection AddMessageStorageClient<TMessageStorageClient>(Func<IServiceProvider, TMessageStorageClient> messageStorageClientFactory)
            where TMessageStorageClient : class, IMessageStorageClient;

        MessageStorageServiceCollection AddJobProcessor<TJobProcessor>(Func<IServiceProvider, TJobProcessor> jobProcessorFactory)
            where TJobProcessor : class, IJobProcessor;

        MessageStorageServiceCollection AddMessageStorageMonitor<TMessageStorageMonitor>(Func<IServiceProvider, TMessageStorageMonitor> messageStorageMonitorFactory)
            where TMessageStorageMonitor : class, IMessageStorageMonitor;
    }

    public class MessageStorageServiceCollection : IMessageStorageServiceCollection
    {
        private readonly IServiceCollection _serviceCollection;

        public MessageStorageServiceCollection(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        public MessageStorageServiceCollection AddMessageStorageClient<TMessageStorageClient>(Func<IServiceProvider, TMessageStorageClient> messageStorageClientFactory)
            where TMessageStorageClient : class, IMessageStorageClient
        {
            _serviceCollection.AddScoped(messageStorageClientFactory.Invoke);
            return this;
        }

        public MessageStorageServiceCollection AddJobProcessor<TJobProcessor>(Func<IServiceProvider, TJobProcessor> jobProcessorFactory)
            where TJobProcessor : class, IJobProcessor
        {
            _serviceCollection.AddSingleton(jobProcessorFactory.Invoke);
            return this;
        }

        public MessageStorageServiceCollection AddMessageStorageMonitor<TMessageStorageMonitor>(Func<IServiceProvider, TMessageStorageMonitor> messageStorageMonitorFactory)
            where TMessageStorageMonitor : class, IMessageStorageMonitor
        {
            _serviceCollection.AddScoped(messageStorageMonitorFactory.Invoke);
            return this;
        }
    }
}