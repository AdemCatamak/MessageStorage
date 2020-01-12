using Microsoft.Extensions.DependencyInjection;

namespace MessageStorage.DI.Extension
{
    public static class InMemoryMessageStorageDIExtensions
    {
        public static IMessageStorageServiceCollection AddInMemoryMessageStorage(this IMessageStorageServiceCollection serviceCollection)
        {
            serviceCollection
               .AddHandlerManager<HandlerManager>(ServiceLifetime.Singleton)
               .Add<IMessageStorageClient>(provider => new MessageStorageClient(new InMemoryStorageAdaptor(), provider.GetRequiredService<IHandlerManager>()), ServiceLifetime.Singleton);

            return serviceCollection;
        }
    }
}