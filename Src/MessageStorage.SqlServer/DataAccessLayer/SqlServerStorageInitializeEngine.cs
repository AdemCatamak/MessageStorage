using System;
using System.Threading;
using System.Threading.Tasks;
using FluentMigrator.Runner;
using MessageStorage.DataAccessLayer;
using MessageStorage.SqlServer.DataAccessLayer.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MessageStorage.SqlServer.DataAccessLayer;

public class SqlServerStorageInitializeEngine : IStorageInitializeEngine
{
    private readonly SqlServerRepositoryContextConfiguration _repositoryContextConfiguration;

    public SqlServerStorageInitializeEngine(SqlServerRepositoryContextConfiguration repositoryRepositoryContextConfiguration)
    {
        _repositoryContextConfiguration = repositoryRepositoryContextConfiguration;
    }

    public Task InitializeAsync(CancellationToken cancellationToken)
    {
        IServiceProvider serviceProvider = CreateServices(_repositoryContextConfiguration);

        using IServiceScope scope = serviceProvider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
        return Task.CompletedTask;
    }

    private static IServiceProvider CreateServices(SqlServerRepositoryContextConfiguration contextConfiguration)
    {
        IServiceCollection serviceCollection = new ServiceCollection()
                                              .AddFluentMigratorCore()
                                              .AddLogging(lb =>
                                               {
                                                   lb.AddFluentMigratorConsole();
                                                   lb.SetMinimumLevel(LogLevel.Warning);
                                               })
                                              .AddSingleton(contextConfiguration)
                                              .ConfigureRunner(builder =>
                                               {
                                                   builder.AddSqlServer()
                                                          .WithGlobalConnectionString(contextConfiguration.ConnectionString)
                                                          .ScanIn(typeof(M0001VersionTable).Assembly).For.All();
                                               });

        return serviceCollection.BuildServiceProvider(validateScopes: false);
    }
}