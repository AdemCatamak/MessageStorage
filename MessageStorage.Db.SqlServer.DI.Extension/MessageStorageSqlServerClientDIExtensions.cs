using System.Collections.Generic;
using MessageStorage.Db.Clients;
using MessageStorage.Db.Clients.Imp;
using MessageStorage.Db.Configurations;
using MessageStorage.Db.DI.Extension;
using MessageStorage.Db.SqlServer.DataAccessSection;
using MessageStorage.DI.Extension;

namespace MessageStorage.Db.SqlServer.DI.Extension
{
    public static class MessageStorageSqlServerClientDIExtensions
    {
        public static IMessageStorageServiceCollection AddMessageStorageSqlServerClient
            (this IMessageStorageServiceCollection messageStorageServiceCollection, DbRepositoryConfiguration dbRepositoryConfiguration, IEnumerable<Handler> handlers, MessageStorageDbConfiguration messageStorageDbConfiguration = null)
        {
            return AddMessageStorageSqlServerClient(messageStorageServiceCollection, dbRepositoryConfiguration, new HandlerManager(handlers), messageStorageDbConfiguration);
        }

        public static IMessageStorageServiceCollection AddMessageStorageSqlServerClient
            (this IMessageStorageServiceCollection messageStorageServiceCollection, DbRepositoryConfiguration sampleSqlServerDbRepositoryConfiguration, IHandlerManager handlerManager, MessageStorageDbConfiguration messageStorageDbConfiguration = null)
        {
            messageStorageDbConfiguration ??= new MessageStorageDbConfiguration();

            return messageStorageServiceCollection.AddMessageStorageDbClient<IMessageStorageDbClient>(provider =>
                                                                                                      {
                                                                                                          var sqlServerDbConnectionFactory = new SqlServerDbConnectionFactory();
                                                                                                          var messageStorageDbClient
                                                                                                              = new MessageStorageDbClient(handlerManager,
                                                                                                                                           new SqlServerDbRepositoryContext(sampleSqlServerDbRepositoryConfiguration, sqlServerDbConnectionFactory),
                                                                                                                                           messageStorageDbConfiguration);
                                                                                                          return messageStorageDbClient;
                                                                                                      });
        }
    }
}