using System;
using DotNet.Testcontainers.Containers.Builders;
using DotNet.Testcontainers.Containers.Configurations.Databases;
using DotNet.Testcontainers.Containers.Modules.Databases;
using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Services;
using MySql.Data.MySqlClient;
using TestUtility;

namespace MessageStorage.MySql.FunctionalTest.Fixtures
{
    public class MySqlInfraFixture : IDisposable
    {
        public string ConnectionString { get; }

        private readonly IContainerService _mySqlTestcontainer;

        public MySqlInfraFixture()
        {
            const string? database = "messagestorage_mysql_functional";
            const string? username = "root";
            const string? password = "example";
            int port = TestUtility.NetworkUtility.GetAvailablePort();

            _mySqlTestcontainer = new Builder().UseContainer()
                                               .UseImage("mysql")
                                               .ExposePort(port, 3306)
                                               .WithEnvironment($"MYSQL_ROOT_PASSWORD={password}",
                                                                $"MYSQL_DATABASE={database}")
                                               .WaitForPort($"3306/tcp", 30000 /*30s*/)
                                               .Build()
                                               .Start();

            MySqlConnectionStringBuilder mySqlConnectionStringBuilder = new MySqlConnectionStringBuilder
                                                                        {
                                                                            UserID = username,
                                                                            Password = password,
                                                                            Database = database,
                                                                            Server = "localhost",
                                                                            Port = (uint)port
                                                                        };

            ConnectionString = mySqlConnectionStringBuilder.ConnectionString;
        }

        public void Dispose()
        {
            _mySqlTestcontainer?.Stop();
            _mySqlTestcontainer?.Dispose();
        }
    }
}