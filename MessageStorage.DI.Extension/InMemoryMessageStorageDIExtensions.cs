using Microsoft.Extensions.DependencyInjection;

namespace MessageStorage.DI.Extension
{
    public static class InMemoryMessageStorageDIExtensions
    {
        public static IMessageStorageServiceCollection AddInMemoryMessageStorage(this IMessageStorageServiceCollection serviceCollection)
        {
            var inMemoryStorageAdaptor = new InMemoryStorageAdaptor();
            serviceCollection
               .AddHandlerManager<HandlerManager>(ServiceLifetime.Singleton)
               .Add<IMessageStorageMonitor>(provider => new MessageStorageMonitor(inMemoryStorageAdaptor), ServiceLifetime.Singleton)
               .Add<IMessageStorageClient>(provider => new MessageStorageClient(inMemoryStorageAdaptor, provider.GetRequiredService<IHandlerManager>()), ServiceLifetime.Singleton);

            return serviceCollection;
        }
    }
}