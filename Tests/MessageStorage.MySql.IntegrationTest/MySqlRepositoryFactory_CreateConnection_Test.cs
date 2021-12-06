using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer;
using MessageStorage.MySql.IntegrationTest.Fixtures;
using TestUtility;
using Xunit;

namespace MessageStorage.MySql.IntegrationTest
{
    public class MySqlRepositoryFactory_CreateConnection_Test : IClassFixture<MySqlInfraFixture>
    {
        private readonly MySqlRepositoryFactory _sut;

        public MySqlRepositoryFactory_CreateConnection_Test(MySqlInfraFixture mySqlInfraFixture)
        {
            _sut = new MySqlRepositoryFactory(new RepositoryConfiguration(mySqlInfraFixture.ConnectionString, mySqlInfraFixture.Database));
        }

        [Fact]
        public async Task When_PostgresRepositoryFactoryCreateConnection__ConnectionHasAbleToExecuteScript()
        {
            using var mySqlMessageStorageConnection = _sut.CreateConnection();
            var results = (await mySqlMessageStorageConnection.QueryAsync("Select UTC_TIMESTAMP() as now", null, CancellationToken.None))
               .ToList();

            Assert.NotEmpty(results);
            var dbResponse = (DateTime) results.First().now;

            DateTime now = DateTime.UtcNow;

            AssertThat.GreaterThan(dbResponse, now.AddMinutes(1));
            AssertThat.LessThan(dbResponse, now.AddMinutes(-1));
        }
    }
}