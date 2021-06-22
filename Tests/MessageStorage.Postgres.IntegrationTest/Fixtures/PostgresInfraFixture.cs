using System;
using DotNet.Testcontainers.Containers.Builders;
using DotNet.Testcontainers.Containers.Configurations.Databases;
using DotNet.Testcontainers.Containers.Modules.Databases;
using Npgsql;
using TestUtility;
using Xunit;

namespace MessageStorage.Postgres.IntegrationTest.Fixtures
{
    [CollectionDefinition(PostgresInfraFixture.FIXTURE_KEY)]
    public class PostgresInfraFixtureDefinition : ICollectionFixture<PostgresInfraFixture>
    {
    }

    public class PostgresInfraFixture : IDisposable
    {
        public const string FIXTURE_KEY = "PostgresInfraFixture";
        public string Host { get; private set; }
        public string Database { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public int Port { get; private set; }

        public string ConnectionString { get; }

        private readonly PostgreSqlTestcontainer _postgreSqlTestContainer;

        public PostgresInfraFixture()
        {
            Host = "127.0.0.1";
            Database = "messagestorage_postgres_functionaldb";
            Username = "postgres";
            Password = "postgres";
            Port = NetworkUtility.GetAvailablePort();

            var connectionStringBuilder
                = new NpgsqlConnectionStringBuilder
                {
                    Host = Host,
                    Database = Database,
                    Port = Port,
                    Username = Username,
                    Password = Password
                };
            ConnectionString = connectionStringBuilder.ConnectionString;

            ITestcontainersBuilder<PostgreSqlTestcontainer> postgresTestContainerBuilder
                = new TestcontainersBuilder<PostgreSqlTestcontainer>()
                   .WithDatabase(new PostgreSqlTestcontainerConfiguration
                    {
                        Database = Database,
                        Username = Username,
                        Password = Password,
                        Port = Port
                    });

            _postgreSqlTestContainer = postgresTestContainerBuilder.Build();
            _postgreSqlTestContainer.StartAsync().GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            _postgreSqlTestContainer?.DisposeAsync().GetAwaiter().GetResult();
        }
    }
}