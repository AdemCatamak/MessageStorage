using System;
using DotNet.Testcontainers.Containers.Builders;
using DotNet.Testcontainers.Containers.Configurations.Databases;
using DotNet.Testcontainers.Containers.Modules.Databases;
using Npgsql;
using TestUtility;
using Xunit;

namespace MessageStorage.Postgres.IntegrationTest.Fixtures;

[CollectionDefinition(PostgresFixture.FIXTURE_KEY)]
public class PostgresInfraFixtureDefinition : ICollectionFixture<PostgresFixture>
{
}

public class PostgresFixture : IDisposable
{
    public const string FIXTURE_KEY = "PostgresIntegrationTest.PostgresFixture";
    public string ConnectionStr { get; }
    private readonly PostgreSqlTestcontainer _postgreSqlTestcontainer;

    public PostgresFixture()
    {
        const string host = "localhost";
        const string database = "public";
        const string username = "postgres";
        const string password = "postgres";
        int port = NetworkUtility.GetAvailablePort();

        ITestcontainersBuilder<PostgreSqlTestcontainer>? postgreSqlTestContainerBuilder
            = new TestcontainersBuilder<PostgreSqlTestcontainer>()
               .WithDatabase(new PostgreSqlTestcontainerConfiguration
                             {
                                 Database = database,
                                 Port = port,
                                 Username = username,
                                 Password = password
                             });

        _postgreSqlTestcontainer = postgreSqlTestContainerBuilder.Build();
        _postgreSqlTestcontainer.StartAsync().GetAwaiter().GetResult();

        var connectionStringBuilder
            = new NpgsqlConnectionStringBuilder
              {
                  Host = host,
                  Port = port,
                  Username = username,
                  Password = password,
                  Database = database,
              };
        ConnectionStr = connectionStringBuilder.ConnectionString;
    }

    public void Dispose()
    {
        _postgreSqlTestcontainer?.DisposeAsync().GetAwaiter().GetResult();
    }
}