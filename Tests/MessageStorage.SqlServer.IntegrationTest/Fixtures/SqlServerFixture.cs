using System;
using DotNet.Testcontainers.Containers.Builders;
using DotNet.Testcontainers.Containers.Configurations.Databases;
using DotNet.Testcontainers.Containers.Modules.Databases;
using Microsoft.Data.SqlClient;
using TestUtility;
using Xunit;

namespace MessageStorage.SqlServer.IntegrationTest.Fixtures;

[CollectionDefinition(SqlServerFixture.FIXTURE_KEY)]
public class SqlServerInfraFixtureDefinition : ICollectionFixture<SqlServerFixture>
{
}

public class SqlServerFixture : IDisposable
{
    public const string FIXTURE_KEY = "SqlServerIntegrationTest.SqlServerFixture";
    public string ConnectionStr { get; }
    private readonly MsSqlTestcontainer _sqlServerContainer;

    public SqlServerFixture()
    {
        const string host = "localhost";
        const string database = "master";
        const string username = "sa";
        const string password = "Passw0rd";
        int port = NetworkUtility.GetAvailablePort();

        ITestcontainersBuilder<MsSqlTestcontainer> sqlServerTestContainerBuilder
            = new TestcontainersBuilder<MsSqlTestcontainer>()
               .WithDatabase(new MsSqlTestcontainerConfiguration
                             {
                                 // These setters throw exception
                                 // Database = database,
                                 // Username = username,
                                 Password = password,
                                 Port = port
                             });

        _sqlServerContainer = sqlServerTestContainerBuilder.Build();
        _sqlServerContainer.StartAsync().GetAwaiter().GetResult();

        var connectionStringBuilder
            = new SqlConnectionStringBuilder
              {
                  DataSource = $"{host},{port}",
                  UserID = username,
                  Password = password,
                  InitialCatalog = database,
                  TrustServerCertificate = true
              };
        ConnectionStr = connectionStringBuilder.ConnectionString;
    }

    public void Dispose()
    {
        _sqlServerContainer?.DisposeAsync().GetAwaiter().GetResult();
    }
}