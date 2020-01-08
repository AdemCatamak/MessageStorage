using MessageStorage.HandlerFactorySection;
using MessageStorage.MessageStorageClientSection;
using MessageStorage.StorageAdaptorSection;
using Microsoft.Extensions.DependencyInjection;

namespace MessageStorage.DI.Extension
{
    public static class InMemoryMessageStorageDIExtensions
    {
        public static IMessageStorageServiceCollection AddInMemoryMessageStorage(this IMessageStorageServiceCollection serviceCollection)
        {
            serviceCollection.AddMessageStorageClient<MessageStorageClient>(ServiceLifetime.Singleton)
                             .AddStorageAdaptor<InMemoryStorageAdaptor>(ServiceLifetime.Singleton)
                             .AddHandlerFactory<HandlerFactory>(ServiceLifetime.Singleton);
            return serviceCollection;
        }
    }
}