using System;
using FluentMigrator.Runner;
using FluentMigrator.Runner.VersionTableInfo;
using MessageStorage.Db.Configurations;
using MessageStorage.Db.DbMigrationRunners;
using MessageStorage.Db.SqlServer.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace MessageStorage.Db.SqlServer
{
    public class SqlServerMessageStorageDbMigrationRunner : IMessageStorageDbMigrationRunner
    {
        public void MigrateUp(DbRepositoryConfiguration dbRepositoryConfiguration)
        {
            IServiceProvider serviceProvider = CreateServices(dbRepositoryConfiguration);

            using (IServiceScope scope = serviceProvider.CreateScope())
            {
                var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                
                runner.MigrateUp();
            }
        }

        private IServiceProvider CreateServices(DbRepositoryConfiguration dbRepositoryConfiguration)
        {
            IServiceCollection serviceCollection = new ServiceCollection()
                                                  .AddFluentMigratorCore()
                                                  .AddLogging(lb => lb.AddFluentMigratorConsole())
                                                  .AddScoped<IVersionTableMetaData, _0001_VersionTable>()
                                                  .AddSingleton(dbRepositoryConfiguration)
                                                  .ConfigureRunner(builder =>
                                                                   {
                                                                       builder.AddSqlServer()
                                                                              .WithGlobalConnectionString(dbRepositoryConfiguration.ConnectionString)
                                                                              .ScanIn(typeof(SqlServerMessageStorageDbMigrationRunner).Assembly).For.Migrations();
                                                                   });

            return serviceCollection.BuildServiceProvider(validateScopes: false);
        }
    }
}