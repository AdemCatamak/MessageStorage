using System;
using MessageStorage.Clients;
using MessageStorage.Configurations;
using MessageStorage.DataAccessSection;
using Microsoft.Extensions.DependencyInjection;

namespace MessageStorage.DI.Extensions
{
    public interface IMessageStorageServiceCollection
    {
        MessageStorageServiceCollection AddRepositoryContext<TBRepositoryContext, TRepositoryConfiguration>(Func<IServiceProvider, TBRepositoryContext> contextFactory)
            where TBRepositoryContext : class, IRepositoryContext<TRepositoryConfiguration>
            where TRepositoryConfiguration : RepositoryConfiguration;

        MessageStorageServiceCollection AddMessageStorageClient<TMessageStorageClient>(Func<IServiceProvider, TMessageStorageClient> messageStorageClientFactory)
            where TMessageStorageClient : class, IMessageStorageClient;

        MessageStorageServiceCollection AddJobProcessor<TJobProcessor>(Func<IServiceProvider, TJobProcessor> jobProcessorFactory)
            where TJobProcessor : class, IJobProcessor;
    }

    public class MessageStorageServiceCollection : IMessageStorageServiceCollection
    {
        private readonly IServiceCollection _serviceCollection;

        public MessageStorageServiceCollection(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        public MessageStorageServiceCollection AddRepositoryContext<TBRepositoryContext, TRepositoryConfiguration>(Func<IServiceProvider, TBRepositoryContext> contextFactory)
            where TBRepositoryContext : class, IRepositoryContext<TRepositoryConfiguration>
            where TRepositoryConfiguration : RepositoryConfiguration
        {
            _serviceCollection.AddScoped(contextFactory.Invoke);
            return this;
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
            _serviceCollection.AddScoped(jobProcessorFactory.Invoke);
            return this;
        }
    }
}