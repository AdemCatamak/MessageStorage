using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MessageStorage.Db.MessageStorageDbClientSection;
using MessageStorage.Db.MsSql;
using MessageStorage.Db.MsSql.Migrations;
using MessageStorage.DI.Extension;
using MessageStorage.HandlerFactorySection;
using Microsoft.Extensions.DependencyInjection;

namespace MessageStorage.Db.DI.Extension
{
    public static class MsSqlMessageStorageInjectionExtensions
    {
        public static IMessageStorageServiceCollection AddMsSqlMessageStorage(this IMessageStorageServiceCollection messageStorageServiceCollection, MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            messageStorageServiceCollection.AddMessageStorageClient<MessageStorageDbClient>(ServiceLifetime.Singleton)
                                           .AddStorageAdaptor<MsSqlDbStorageAdaptor>(ServiceLifetime.Singleton)
                                           .AddHandlerFactory<HandlerFactory>(ServiceLifetime.Singleton);

            // TODO: Singleton Injection block for multiple MessageStorageDbClient
            messageStorageServiceCollection.TryAdd(new ServiceDescriptor(typeof(MessageStorageDbConfiguration), messageStorageDbConfiguration));
            messageStorageServiceCollection.TryAdd(new ServiceDescriptor(typeof(IDbConnectionFactory), typeof(MsSqlDbConnectionFactory), ServiceLifetime.Singleton));

            messageStorageServiceCollection.TryAdd(new ServiceDescriptor(typeof(IMsSqlMigrationRunner), typeof(MsSqlMigrationRunner), ServiceLifetime.Singleton))
                                           .InjectMigrations(new List<Assembly> {typeof(_0001_CreateSchema).Assembly});

            return messageStorageServiceCollection;
        }

        private static IMessageStorageServiceCollection InjectMigrations(this IMessageStorageServiceCollection messageStorageServiceCollection, IEnumerable<Assembly> assemblies)
        {
            IEnumerable<Type> migrations = FindMigrations(assemblies);
            foreach (Type migration in migrations)
            {
                messageStorageServiceCollection.Add(new ServiceDescriptor(typeof(IMigration), migration, ServiceLifetime.Singleton));
            }

            return messageStorageServiceCollection;
        }


        private static IEnumerable<Type> FindMigrations(IEnumerable<Assembly> assemblies)
        {
            var migrationType = typeof(IMigration);
            var result = assemblies.SelectMany(s => s.GetTypes())
                                   .Where(p => migrationType.IsAssignableFrom(p));

            return result;
        }
    }
}