﻿using System.Collections.Generic;
using MessageStorage.Db.Clients;
using MessageStorage.Db.Clients.Imp;
using MessageStorage.Db.Configurations;
using MessageStorage.Db.DataAccessSection;
using MessageStorage.Db.SqlServer.DataAccessSection;
using MessageStorage.DI.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace MessageStorage.Db.SqlServer.DI.Extensions
{
    public static class MessageStorageDbDIExtension
    {
        public static IMessageStorageServiceCollection AddMessageStorageDbClient<TDbRepositoryConfiguration>
            (this IMessageStorageServiceCollection messageStorageServiceCollection, TDbRepositoryConfiguration sampleSqlServerDbRepositoryConfiguration, IEnumerable<Handler> handlers, MessageStorageDbConfiguration messageStorageDbConfiguration = null)
            where TDbRepositoryConfiguration : DbRepositoryConfiguration
        {
            return AddMessageStorageDbClient(messageStorageServiceCollection, sampleSqlServerDbRepositoryConfiguration, new HandlerManager(handlers), messageStorageDbConfiguration);
        }
        
        public static IMessageStorageServiceCollection AddMessageStorageDbClient<TDbRepositoryConfiguration>
            (this IMessageStorageServiceCollection messageStorageServiceCollection, TDbRepositoryConfiguration sampleSqlServerDbRepositoryConfiguration, IHandlerManager handlerManager, MessageStorageDbConfiguration messageStorageDbConfiguration = null)
            where TDbRepositoryConfiguration : DbRepositoryConfiguration
        {
            messageStorageDbConfiguration = messageStorageDbConfiguration ?? new MessageStorageDbConfiguration();

            messageStorageServiceCollection.AddRepositoryContext<IDbRepositoryContext<TDbRepositoryConfiguration>, TDbRepositoryConfiguration>
                (provider =>
                 {
                     var sqlServerDbRepositoryContext = new SqlServerDbRepositoryContext<TDbRepositoryConfiguration>(sampleSqlServerDbRepositoryConfiguration, new SqlServerDbConnectionFactory());
                     return sqlServerDbRepositoryContext;
                 });

            messageStorageServiceCollection.AddMessageStorageClient<IMessageStorageDbClient>
                (provider =>
                 {
                     var messageStorageDbClient = new MessageStorageDbClient<TDbRepositoryConfiguration>(handlerManager, provider.GetRequiredService<IDbRepositoryContext<TDbRepositoryConfiguration>>(), messageStorageDbConfiguration);
                     return messageStorageDbClient;
                 });

            return messageStorageServiceCollection;
        }
    }
}