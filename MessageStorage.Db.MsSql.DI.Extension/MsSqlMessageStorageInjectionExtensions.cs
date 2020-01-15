using MessageStorage.Db.MsSql;
using MessageStorage.DI.Extension;
using Microsoft.Extensions.DependencyInjection;

namespace MessageStorage.Db.DI.Extension
{
    public static class MsSqlMessageStorageInjectionExtensions
    {
        public static IMessageStorageServiceCollection AddMsSqlMessageStorage(this IMessageStorageServiceCollection messageStorageServiceCollection, MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            messageStorageServiceCollection.AddHandlerManager<HandlerManager>(ServiceLifetime.Singleton);

            var msSqlDbStorageAdaptor = new MsSqlDbStorageAdaptor();
            msSqlDbStorageAdaptor.SetConfiguration(messageStorageDbConfiguration);
            messageStorageServiceCollection.Add<IMessageStorageDbClient>(provider => new MessageStorageDbClient(msSqlDbStorageAdaptor, provider.GetRequiredService<IHandlerManager>()),
                                                                       ServiceLifetime.Singleton);
            messageStorageServiceCollection.Add<IMessageStorageClient>(provider => new MessageStorageDbClient(msSqlDbStorageAdaptor, provider.GetRequiredService<IHandlerManager>()),
                                                                    ServiceLifetime.Singleton);


            return messageStorageServiceCollection;
        }
    }
}