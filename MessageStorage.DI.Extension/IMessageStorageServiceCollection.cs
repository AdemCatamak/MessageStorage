using Microsoft.Extensions.DependencyInjection;

namespace MessageStorage.DI.Extension
{
    public interface IMessageStorageServiceCollection
    {
        IMessageStorageServiceCollection Add(ServiceDescriptor serviceDescriptor);
        IMessageStorageServiceCollection TryAdd(ServiceDescriptor serviceDescriptor);

        IMessageStorageServiceCollection AddMessageStorageClient<T>(ServiceLifetime serviceLifetime) where T : IMessageStorageClient;
        IMessageStorageServiceCollection AddStorageAdaptor<T>(ServiceLifetime serviceLifetime) where T : IStorageAdaptor;
        IMessageStorageServiceCollection AddHandlerFactory<T>(ServiceLifetime serviceLifetime) where T : IHandlerFactory;
        IMessageStorageServiceCollection AddMessageProcessServer(ServiceLifetime serviceLifetime = ServiceLifetime.Singleton);
    }
}