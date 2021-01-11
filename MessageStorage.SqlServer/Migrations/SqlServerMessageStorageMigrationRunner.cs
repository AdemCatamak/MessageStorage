using System;
using FluentMigrator.Runner;
using FluentMigrator.Runner.VersionTableInfo;
using MessageStorage.Configurations;
using MessageStorage.DataAccessSection;
using Microsoft.Extensions.DependencyInjection;

namespace MessageStorage.SqlServer.Migrations
{
    public class SqlServerMessageStorageMigrationRunner : IMessageStorageMigrationRunner
    {
        public void MigrateUp(MessageStorageRepositoryContextConfiguration messageStorageRepositoryContextConfiguration)
        {
            IServiceProvider serviceProvider = CreateServices(messageStorageRepositoryContextConfiguration);

            using (IServiceScope scope = serviceProvider.CreateScope())
            {
                var runner = scope.ServiceProvider.GetRequiredService<FluentMigrator.Runner.IMigrationRunner>();

                runner.MigrateUp();
            }
        }

        private IServiceProvider CreateServices(MessageStorageRepositoryContextConfiguration messageStorageRepositoryContextConfiguration)
        {
            IServiceCollection serviceCollection = new ServiceCollection()
                                                  .AddFluentMigratorCore()
                                                  .AddLogging(lb => lb.AddFluentMigratorConsole())
                                                  .AddScoped<IVersionTableMetaData, _0001_VersionTable>()
                                                  .AddSingleton(messageStorageRepositoryContextConfiguration)
                                                  .ConfigureRunner(builder =>
                                                                   {
                                                                       builder.AddSqlServer()
                                                                              .WithGlobalConnectionString(messageStorageRepositoryContextConfiguration.ConnectionStr)
                                                                              .ScanIn(typeof(SqlServerMessageStorageMigrationRunner).Assembly).For.Migrations();
                                                                   });

            return serviceCollection.BuildServiceProvider(validateScopes: false);
        }
    }
}