using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer;
using MessageStorage.SqlServer.DbClient;
using MessageStorage.SqlServer.IntegrationTest.Fixtures;
using TestUtility;
using Xunit;

namespace MessageStorage.SqlServer.IntegrationTest
{
    [Collection(SqlServerInfraFixture.FIXTURE_KEY)]
    public class SqlServerRepositoryFactory_CreateConnection_Test
    {
        private readonly SqlServerRepositoryFactory _sut;

        public SqlServerRepositoryFactory_CreateConnection_Test(SqlServerInfraFixture sqlServerInfraFixture)
        {
            _sut = new SqlServerRepositoryFactory(new RepositoryConfiguration(sqlServerInfraFixture.ConnectionString, "CreateConnection_Test_Schema"));
        }

        [Fact]
        public async Task When_SqlServerRepositoryFactoryCreateConnection__ConnectionHasAbleToExecuteScript()
        {
            using ISqlServerMessageStorageConnection sqlServerMessageStorageConnection = _sut.CreateConnection();
            var results = (await sqlServerMessageStorageConnection.QueryAsync("Select GETUTCDATE() as now", null, CancellationToken.None))
               .ToList();

            Assert.NotEmpty(results);
            var dbResponse = (DateTime) results.First().now;

            DateTime now = DateTime.UtcNow;

            AssertThat.LessThan(dbResponse, now.AddMinutes(-1));
            AssertThat.GreaterThan(dbResponse, now.AddMinutes(1));
        }
    }
}