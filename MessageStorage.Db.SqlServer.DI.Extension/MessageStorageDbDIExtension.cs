using System.Collections.Generic;
using MessageStorage.Db.Clients;
using MessageStorage.Db.Clients.Imp;
using MessageStorage.Db.Configurations;
using MessageStorage.Db.DataAccessSection;
using MessageStorage.Db.SqlServer.DataAccessSection;
using MessageStorage.DI.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace MessageStorage.Db.SqlServer.DI.Extension
{
    public static class MessageStorageDbDIExtension
    {
        public static IMessageStorageServiceCollection AddMessageStorageDbClient
            (this IMessageStorageServiceCollection messageStorageServiceCollection, DbRepositoryConfiguration dbRepositoryConfiguration, IEnumerable<Handler> handlers, MessageStorageDbConfiguration messageStorageDbConfiguration = null)
        {
            return AddMessageStorageDbClient(messageStorageServiceCollection, dbRepositoryConfiguration, new HandlerManager(handlers), messageStorageDbConfiguration);
        }

        public static IMessageStorageServiceCollection AddMessageStorageDbClient
            (this IMessageStorageServiceCollection messageStorageServiceCollection, DbRepositoryConfiguration sampleSqlServerDbRepositoryConfiguration, IHandlerManager handlerManager, MessageStorageDbConfiguration messageStorageDbConfiguration = null)
        {
            messageStorageDbConfiguration ??= new MessageStorageDbConfiguration();

            messageStorageServiceCollection.AddRepositoryContext<IDbRepositoryContext<DbRepositoryConfiguration>, DbRepositoryConfiguration>
                (provider =>
                 {
                     var sqlServerDbRepositoryContext = new SqlServerDbRepositoryContext<DbRepositoryConfiguration>(sampleSqlServerDbRepositoryConfiguration, new SqlServerDbConnectionFactory());
                     return sqlServerDbRepositoryContext;
                 });

            messageStorageServiceCollection.AddMessageStorageClient<IMessageStorageDbClient>
                (provider =>
                 {
                     var messageStorageDbClient = new MessageStorageDbClient<DbRepositoryConfiguration>(handlerManager, provider.GetRequiredService<IDbRepositoryContext<DbRepositoryConfiguration>>(), messageStorageDbConfiguration);
                     return messageStorageDbClient;
                 });

            return messageStorageServiceCollection;
        }
    }
}