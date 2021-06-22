using System;
using DotNet.Testcontainers.Containers.Builders;
using DotNet.Testcontainers.Containers.Configurations.Databases;
using DotNet.Testcontainers.Containers.Modules.Databases;
using Microsoft.Data.SqlClient;
using TestUtility;
using Xunit;

namespace MessageStorage.SqlServer.BenchmarkTest.Fixtures
{
    [CollectionDefinition(SqlServerInfraFixture.FIXTURE_KEY)]
    public class SqlServerInfraFixtureDefinition : ICollectionFixture<SqlServerInfraFixture>
    {
    }

    public class SqlServerInfraFixture : IDisposable
    {
        public const string FIXTURE_KEY = "SqlServerInfraFixture";

        public string Host { get; private set; }
        public string Database { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public int Port { get; private set; }

        public string ConnectionString { get; }

        private readonly MsSqlTestcontainer _sqlTestContainer;

        public SqlServerInfraFixture()
        {
            Host = "127.0.0.1";
            Database = "master";
            Username = "sa";
            Password = "Passw0rd";
            Port = NetworkUtility.GetAvailablePort();

            var connectionStringBuilder
                = new SqlConnectionStringBuilder
                {
                    DataSource = $"{Host},{Port}",
                    InitialCatalog = Database,
                    UserID = Username,
                    Password = Password
                };
            ConnectionString = connectionStringBuilder.ConnectionString;

            ITestcontainersBuilder<MsSqlTestcontainer> sqlServerTestContainerBuilder
                = new TestcontainersBuilder<MsSqlTestcontainer>()
                   .WithDatabase(new MsSqlTestcontainerConfiguration
                    {
                        Password = Password,
                        Port = Port
                    });

            _sqlTestContainer = sqlServerTestContainerBuilder.Build();
            _sqlTestContainer.StartAsync().GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            _sqlTestContainer?.DisposeAsync().GetAwaiter().GetResult();
        }
    }
}