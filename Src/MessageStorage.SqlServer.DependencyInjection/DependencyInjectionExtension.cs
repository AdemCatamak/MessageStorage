using System;
using MessageStorage.DataAccessLayer;
using MessageStorage.DependencyInjection;
using MessageStorage.SqlServer.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace MessageStorage.SqlServer.DependencyInjection
{
    public static class DependencyInjectionExtension
    {
        public static IMessageStorageDependencyConfigurator UseSqlServer(this IMessageStorageDependencyConfigurator messageStorageDependencyConfigurator,
                                                                         string connectionString, string? schema = null)
        {
            return UseSqlServer(messageStorageDependencyConfigurator, provider => connectionString, provider => schema);
        }

        public static IMessageStorageDependencyConfigurator UseSqlServer(this IMessageStorageDependencyConfigurator messageStorageDependencyConfigurator,
                                                                         Func<IServiceProvider, string> getConnectionString, string? schema = null)
        {
            return UseSqlServer(messageStorageDependencyConfigurator, getConnectionString, provider => schema);
        }

        public static IMessageStorageDependencyConfigurator UseSqlServer(this IMessageStorageDependencyConfigurator messageStorageDependencyConfigurator,
                                                                         Func<IServiceProvider, string> getConnectionString, Func<IServiceProvider, string?> getSchema)
        {
            messageStorageDependencyConfigurator.ServiceCollection.AddSingleton<IPrerequisite, SqlServerMigrationExecutor>(provider =>
            {
                RepositoryConfiguration repositoryConfiguration = new RepositoryConfiguration(getConnectionString(provider), getSchema(provider));
                var migrationExecutor = new SqlServerMigrationExecutor(repositoryConfiguration);
                return migrationExecutor;
            });
            messageStorageDependencyConfigurator.ServiceCollection.AddScoped<IRepositoryFactory>(provider => provider.GetRequiredService<ISqlServerRepositoryFactory>());
            messageStorageDependencyConfigurator.ServiceCollection.AddScoped<ISqlServerRepositoryFactory>(provider =>
            {
                string connectionString = getConnectionString(provider);
                string? schema = getSchema(provider);

                var repositoryConfiguration = new RepositoryConfiguration(connectionString, schema);

                SqlServerRepositoryFactory repositoryFactory = new SqlServerRepositoryFactory(repositoryConfiguration);
                return repositoryFactory;
            });

            return messageStorageDependencyConfigurator;
        }
    }
}