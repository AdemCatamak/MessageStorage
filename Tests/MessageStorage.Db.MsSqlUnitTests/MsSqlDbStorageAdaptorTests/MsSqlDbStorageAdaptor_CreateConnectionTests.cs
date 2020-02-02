using System.Data;
using MessageStorage.Db.MsSql;
using MessageStorage.Exceptions;
using Moq;
using Xunit;

namespace MessageStorage.Db.MsSqlUnitTests.MsSqlDbStorageAdaptorTests
{
    public class MsSqlDbStorageAdaptor_CreateConnectionTests
    {
        private readonly MsSql.MsSqlDbStorageAdaptor _sut;

        public MsSqlDbStorageAdaptor_CreateConnectionTests()
        {
            var mockMsSqlMigrationRunner = new Mock<IMsSqlMigrationRunner>();
            _sut = new MsSql.MsSqlDbStorageAdaptor(mockMsSqlMigrationRunner.Object);
        }

        [Fact]
        public void WhenCreateConnectionExecutedWithoutExecutedSetConfiguration__PreconditionFailedExceptionOccurs()
        {
            Assert.Throws<PreConditionFailedException>(() => _sut.CreateConnection());
        }

        [Fact]
        public void WhenDbConfigurationSetOperationExecuted__CreateConnectionExecuted__DbConnectionReturnClosedState()
        {
            const string connectionStr = "Server=localhost,1433;Database=TestDb;User=sa;Password=Passw0rd;Trusted_Connection=False;";
            MessageStorageDbConfiguration messageStorageDbConfiguration = MessageStorageDbConfigurationFactory.Create(connectionStr);
            _sut.SetConfiguration(messageStorageDbConfiguration);

            IDbConnection dbConnection = _sut.CreateConnection();
            Assert.NotNull(dbConnection);
            Assert.Equal(connectionStr, dbConnection.ConnectionString);
            Assert.Equal(ConnectionState.Closed, dbConnection.State);
        }
    }
}