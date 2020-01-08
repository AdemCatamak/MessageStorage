using System.ComponentModel;
using System.ComponentModel.Design;
using MessageStorage.JobServerSection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MessageStorage.DI.Extension
{
    public class MessageStorageServiceCollection : IMessageStorageServiceCollection
    {
        private readonly IServiceCollection _serviceCollection;

        public MessageStorageServiceCollection(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        public IMessageStorageServiceCollection TryAdd(ServiceDescriptor serviceDescriptor)
        {
            _serviceCollection.TryAdd(serviceDescriptor);
            return this;
        }

        public IMessageStorageServiceCollection AddMessageStorageClient<T>(ServiceLifetime serviceLifetime) where T : IMessageStorageClient
        {
            _serviceCollection.TryAdd(new ServiceDescriptor(typeof(IMessageStorageClient), typeof(T), serviceLifetime));
            return this;
        }

        public IMessageStorageServiceCollection AddStorageAdaptor<T>(ServiceLifetime serviceLifetime) where T : IStorageAdaptor
        {
            _serviceCollection.TryAdd(new ServiceDescriptor(typeof(IStorageAdaptor), typeof(T), serviceLifetime));
            return this;
        }

        public IMessageStorageServiceCollection AddHandlerFactory<T>(ServiceLifetime serviceLifetime) where T : IHandlerFactory
        {
            _serviceCollection.TryAdd(new ServiceDescriptor(typeof(IHandlerFactory), typeof(T), serviceLifetime));
            return this;
        }

        public IMessageStorageServiceCollection AddMessageProcessServer(ServiceLifetime serviceLifetime)
        {
            _serviceCollection.TryAdd(new ServiceDescriptor(typeof(IJobServer), typeof(JobServer), serviceLifetime));
            return this;
        }
    }
}