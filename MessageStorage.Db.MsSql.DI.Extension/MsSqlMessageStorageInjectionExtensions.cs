using MessageStorage.DI.Extension;
using Microsoft.Extensions.DependencyInjection;

namespace MessageStorage.Db.MsSql.DI.Extension
{
    public static class MsSqlMessageStorageInjectionExtensions
    {
        public static IMessageStorageServiceCollection AddMsSqlMessageStorage(this IMessageStorageServiceCollection messageStorageServiceCollection, MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            IMsSqlDbStorageAdaptor msSqlDbStorageAdaptor = CreateMsSqlDbStorageAdaptor(messageStorageDbConfiguration);

            messageStorageServiceCollection.AddHandlerManager<HandlerManager>(ServiceLifetime.Singleton);

            messageStorageServiceCollection.Add<IMessageStorageMonitor>(provider => new MessageStorageMonitor(msSqlDbStorageAdaptor),
                                                                        ServiceLifetime.Singleton);

            messageStorageServiceCollection.Add<IMessageStorageDbClient>(provider => new MessageStorageDbClient(msSqlDbStorageAdaptor, provider.GetRequiredService<IHandlerManager>()),
                                                                         ServiceLifetime.Singleton);
            messageStorageServiceCollection.Add<IMessageStorageClient>(provider => new MessageStorageDbClient(msSqlDbStorageAdaptor, provider.GetRequiredService<IHandlerManager>()),
                                                                       ServiceLifetime.Singleton);


            return messageStorageServiceCollection;
        }

        private static IMsSqlDbStorageAdaptor CreateMsSqlDbStorageAdaptor(MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            IMsSqlMigrationRunner msSqlMigrationRunner = new MsSqlMigrationRunner();
            var msSqlDbStorageAdaptor = new MsSqlDbStorageAdaptor(msSqlMigrationRunner);
            msSqlDbStorageAdaptor.SetConfiguration(messageStorageDbConfiguration);
            return msSqlDbStorageAdaptor;
        }
    }
}