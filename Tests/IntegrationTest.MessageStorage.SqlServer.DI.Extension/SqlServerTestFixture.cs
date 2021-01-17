using System.Data;
using MessageStorage.Configurations;
using MessageStorage.DataAccessSection;
using MessageStorage.SqlServer.DataAccessSection;
using MessageStorage.SqlServer.Migrations;
using Microsoft.Data.SqlClient;

namespace IntegrationTest.MessageStorage.SqlServer.DI.Extension
{
    public class SqlServerTestFixture
    {
        public const string CONNECTION_STR = "Server=localhost,7784;Database=master;User=sa;Password=Passw0rd;Trusted_Connection=False;";
        public const string SCHEMA = "MessageStorage";

        public SqlServerTestFixture()
        {
            var messageStorageMigrationRunner = new SqlServerMessageStorageMigrationRunner();
            messageStorageMigrationRunner.MigrateUp(new MessageStorageRepositoryContextConfiguration(CONNECTION_STR));
        }

        public IMessageStorageRepositoryContext CreateMessageStorageSqlServerRepositoryContext()
        {
            MessageStorageRepositoryContextConfiguration messageStorageRepositoryContextConfiguration
                = new MessageStorageRepositoryContextConfiguration(CONNECTION_STR);

            IMessageStorageRepositoryContext repositoryContext
                = new SqlServerMessageStorageRepositoryContext(messageStorageRepositoryContextConfiguration);

            return repositoryContext;
        }

        public IDbConnection CreateDbConnection()
        {
            SqlConnection sqlConnection = new SqlConnection(CONNECTION_STR);
            return sqlConnection;
        }
    }
}