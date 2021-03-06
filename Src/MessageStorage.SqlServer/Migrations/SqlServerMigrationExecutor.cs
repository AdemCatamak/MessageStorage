using System;
using FluentMigrator.Runner;
using FluentMigrator.Runner.VersionTableInfo;
using MessageStorage.DataAccessLayer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MessageStorage.SqlServer.Migrations
{
    public class SqlServerMigrationExecutor : IPrerequisite
    {
        private readonly RepositoryConfiguration _repositoryConfiguration;

        public SqlServerMigrationExecutor(RepositoryConfiguration repositoryConfiguration)
        {
            _repositoryConfiguration = repositoryConfiguration;
        }

        public void Execute()
        {
            IServiceProvider serviceProvider = CreateServices(_repositoryConfiguration);

            using IServiceScope scope = serviceProvider.CreateScope();
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            runner.MigrateUp();
        }

        private static IServiceProvider CreateServices(RepositoryConfiguration repositoryConfiguration)
        {
            IServiceCollection serviceCollection = new ServiceCollection()
                                                  .AddFluentMigratorCore()
                                                  .AddLogging(lb =>
                                                   {
                                                       lb.AddFluentMigratorConsole();
                                                       lb.SetMinimumLevel(LogLevel.Warning);
                                                   })
                                                  .AddScoped<IVersionTableMetaData, _0001_VersionTable>()
                                                  .AddSingleton(repositoryConfiguration)
                                                  .ConfigureRunner(builder =>
                                                   {
                                                       builder.AddSqlServer()
                                                              .WithGlobalConnectionString(repositoryConfiguration.ConnectionString)
                                                              .ScanIn(typeof(_0001_VersionTable).Assembly).For.Migrations();
                                                   });

            return serviceCollection.BuildServiceProvider(validateScopes: false);
        }
    }
}