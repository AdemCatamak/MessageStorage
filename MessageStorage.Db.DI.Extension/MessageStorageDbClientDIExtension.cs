using System;
using MessageStorage.Db.Clients;
using MessageStorage.DI.Extension;

namespace MessageStorage.Db.DI.Extension
{
    public static class MessageStorageDbClientDIExtension
    {
        public static IMessageStorageServiceCollection AddMessageStorageDbClient<TMessageStorageDbClient>
            (this IMessageStorageServiceCollection messageStorageServiceCollection, Func<IServiceProvider, TMessageStorageDbClient> messageStorageDbClientFactory)
            where TMessageStorageDbClient : class, IMessageStorageDbClient
        {
            messageStorageServiceCollection.AddMessageStorageClient
                (provider =>
                 {
                     TMessageStorageDbClient messageStorageDbClient = messageStorageDbClientFactory.Invoke(provider);
                     return messageStorageDbClient;
                 });

            return messageStorageServiceCollection;
        }
    }
}