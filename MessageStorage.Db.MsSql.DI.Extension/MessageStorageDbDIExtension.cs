using System;
using System.Collections.Generic;
using System.Linq;
using MessageStorage.Db.DataAccessLayer;
using MessageStorage.Db.DataAccessLayer.Repositories;
using MessageStorage.Db.MsSql.DataAccessSection.QueryBuilders;
using MessageStorage.Db.MsSql.Migrations;
using MessageStorage.DI.Extension;
using Microsoft.Extensions.DependencyInjection;

namespace MessageStorage.Db.MsSql.DI.Extension
{
    public static class MessageStorageDbDIExtension
    {
        public static IMessageStorageServiceCollection AddMessageStorageDbClient<TMessageStorageDbConfiguration>
            (this IMessageStorageServiceCollection messageStorageServiceCollection, TMessageStorageDbConfiguration messageStorageDbConfiguration)
            where TMessageStorageDbConfiguration : MessageStorageDbConfiguration
        {
            messageStorageServiceCollection.AddHandlerManager();

            var dbAdaptor = new MsSqlDbAdaptor();

            IEnumerable<IDbRepository> dbRepositories = GetRepositories(messageStorageDbConfiguration, dbAdaptor);
            IEnumerable<IMigration> migrations = GetMigrations();

            messageStorageServiceCollection.Add<IMessageStorageClient>(ServiceLifetime.Singleton,
                                                                         provider => new MessageStorageDbClient<TMessageStorageDbConfiguration>(provider.GetRequiredService<IHandlerManager>()
                                                                                                                                              , new DbRepositoryResolver(dbRepositories)
                                                                                                                                              , new MsSqlMigrationRunner(dbAdaptor)
                                                                                                                                              , migrations
                                                                                                                                              , messageStorageDbConfiguration
                                                                                                                                               )
                                                                        );
            messageStorageServiceCollection.Add<IMessageStorageDbClient>(ServiceLifetime.Singleton,
                                                                       provider => new MessageStorageDbClient<TMessageStorageDbConfiguration>(provider.GetRequiredService<IHandlerManager>()
                                                                                                                                            , new DbRepositoryResolver(dbRepositories)
                                                                                                                                            , new MsSqlMigrationRunner(dbAdaptor)
                                                                                                                                            , migrations
                                                                                                                                            , messageStorageDbConfiguration
                                                                                                                                             )
                                                                      );
            return messageStorageServiceCollection;
        }

        private static IEnumerable<IMigration> GetMigrations()
        {
            IEnumerable<Type> migrationTypes = typeof(_0001_CreateSchema).Assembly.GetTypes()
                                                                         .Where(p => typeof(IMigration).IsAssignableFrom(p));

            IEnumerable<IMigration> migrations = migrationTypes.Select(type => Activator.CreateInstance(type) as IMigration);
            return migrations;
        }

        private static IEnumerable<IDbRepository> GetRepositories<TMessageStorageDbConfiguration>(TMessageStorageDbConfiguration messageStorageDbConfiguration, IDbAdaptor dbAdaptor)
            where TMessageStorageDbConfiguration : MessageStorageDbConfiguration
        {
            var dbMessageRepository = new DbMessageRepository(new MsSqlMessageQueryBuilder(messageStorageDbConfiguration), dbAdaptor, messageStorageDbConfiguration);
            var dbJobRepository = new DbJobRepository(new MsSqlJobQueryBuilder(messageStorageDbConfiguration), dbAdaptor, messageStorageDbConfiguration);

            return new List<IDbRepository>
                   {
                       dbMessageRepository,
                       dbJobRepository
                   };
        }
    }
}