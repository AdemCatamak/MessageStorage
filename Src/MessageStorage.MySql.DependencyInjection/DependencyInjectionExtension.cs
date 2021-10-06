using System;
using MessageStorage.DataAccessLayer;
using MessageStorage.DependencyInjection;
using MessageStorage.MySql.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace MessageStorage.MySql.DependencyInjection
{
    public static class DependencyInjectionExtension
    {
        public static IMessageStorageDependencyConfigurator UseMySql(this IMessageStorageDependencyConfigurator messageStorageDependencyConfigurator,
                                                                  string connectionString, string? schema = null)
        {
            return UseMySql(messageStorageDependencyConfigurator, provider => connectionString, provider => schema);
        }

        public static IMessageStorageDependencyConfigurator UseMySql(this IMessageStorageDependencyConfigurator messageStorageDependencyConfigurator,
                                                                     Func<IServiceProvider, string> getConnectionString, string? schema = null)
        {
            return UseMySql(messageStorageDependencyConfigurator, getConnectionString, provider => schema);
        }

        public static IMessageStorageDependencyConfigurator UseMySql(this IMessageStorageDependencyConfigurator messageStorageDependencyConfigurator,
                                                                     Func<IServiceProvider, string> getConnectionString, Func<IServiceProvider, string?> getSchema)
        {
            messageStorageDependencyConfigurator.ServiceCollection.AddSingleton<IPrerequisite, MySqlMigrationExecutor>(provider =>
            {
                RepositoryConfiguration repositoryConfiguration = new RepositoryConfiguration(getConnectionString(provider), getSchema(provider));
                var migrationExecutor = new MySqlMigrationExecutor(repositoryConfiguration);
                return migrationExecutor;
            });
            messageStorageDependencyConfigurator.ServiceCollection.AddScoped<IRepositoryFactory>(provider => provider.GetRequiredService<IMySqlRepositoryFactory>());
            messageStorageDependencyConfigurator.ServiceCollection.AddScoped<IMySqlRepositoryFactory>(provider =>
            {
                string connectionString = getConnectionString(provider);
                string? schema = getSchema(provider);

                var repositoryConfiguration = new RepositoryConfiguration(connectionString, schema);

                var repositoryFactory = new MySqlRepositoryFactory(repositoryConfiguration);
                return repositoryFactory;
            });

            return messageStorageDependencyConfigurator;
        }
    }
}