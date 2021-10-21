using System;
using System.Net.NetworkInformation;
using System.Threading;
using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Services;
using MySql.Data.MySqlClient;
using TestUtility;

namespace MessageStorage.MySql.FunctionalTest.Fixtures
{
    public class MySqlInfraFixture : IDisposable
    {
        private readonly IContainerService _mySqlContainer;
        public string Host { get; private set; }
        public string Database { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public int Port { get; private set; }

        public string ConnectionString { get; }


        public MySqlInfraFixture()
        {
            Host = "localhost";
            Database = "message_storage";
            Username = "root";
            Password = "example";
            Port = NetworkUtility.GetAvailablePort();

            var connectionStringBuilder
                = new MySqlConnectionStringBuilder
                  {
                      Server = Host,
                      Database = Database,
                      Port = (uint)Port,
                      UserID = Username,
                      Password = Password
                  };
            ConnectionString = connectionStringBuilder.ConnectionString;

            _mySqlContainer =
                new Builder().UseContainer()
                             .UseImage("mysql:5.7")
                             .ExposePort(Port, 3306)
                             .WithEnvironment($"MYSQL_ROOT_PASSWORD={Password}", $"MYSQL_DATABASE={Database}")
                             .WaitForPort($"{3306}/tcp", 30000 /*30s*/)
                             .Build()
                             .Start();

            MySqlConnection? mySqlConnection = null;
            var tryCount = 0;
            again:
            try
            {
                tryCount++;
                mySqlConnection = new MySqlConnection(ConnectionString);
                mySqlConnection.Open();
            }
            catch (Exception)
            {
                if (tryCount < 5)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                    goto again;
                }
                throw;
            }
            finally
            {
                mySqlConnection?.Dispose();
            }
        }

        public void Dispose()
        {
            _mySqlContainer.Stop();
            _mySqlContainer.Dispose();
        }
    }
}