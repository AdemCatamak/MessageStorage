using System;
using MessageStorage.DataAccessLayer;
using MessageStorage.DependencyInjection;
using MessageStorage.Postgres.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace MessageStorage.Postgres.DependencyInjection
{
    public static class DependencyInjectionExtension
    {
        public static IMessageStorageDependencyConfigurator UsePostgres(this IMessageStorageDependencyConfigurator messageStorageDependencyConfigurator,
                                                                  string connectionString, string? schema = null)
        {
            return UsePostgres(messageStorageDependencyConfigurator, provider => connectionString, provider => schema);
        }

        public static IMessageStorageDependencyConfigurator UsePostgres(this IMessageStorageDependencyConfigurator messageStorageDependencyConfigurator,
                                                                  Func<IServiceProvider, string> getConnectionString, string? schema = null)
        {
            return UsePostgres(messageStorageDependencyConfigurator, getConnectionString, provider => schema);
        }

        public static IMessageStorageDependencyConfigurator UsePostgres(this IMessageStorageDependencyConfigurator messageStorageDependencyConfigurator,
                                                                  Func<IServiceProvider, string> getConnectionString, Func<IServiceProvider, string?> getSchema)
        {
            messageStorageDependencyConfigurator.ServiceCollection.AddSingleton<IPrerequisite, PostgresMigrationExecutor>(provider =>
            {
                RepositoryConfiguration repositoryConfiguration = new RepositoryConfiguration(getConnectionString(provider), getSchema(provider));
                var migrationExecutor = new PostgresMigrationExecutor(repositoryConfiguration);
                return migrationExecutor;
            });
            messageStorageDependencyConfigurator.ServiceCollection.AddScoped<IRepositoryFactory>(provider => provider.GetRequiredService<IPostgresRepositoryFactory>());
            messageStorageDependencyConfigurator.ServiceCollection.AddScoped<IPostgresRepositoryFactory>(provider =>
            {
                string connectionString = getConnectionString(provider);
                string? schema = getSchema(provider);

                var repositoryConfiguration = new RepositoryConfiguration(connectionString, schema);

                var repositoryFactory = new PostgresRepositoryFactory(repositoryConfiguration);
                return repositoryFactory;
            });

            return messageStorageDependencyConfigurator;
        }
    }
}