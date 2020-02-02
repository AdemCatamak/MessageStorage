using System;
using MessageStorage.Db.MsSql;
using MessageStorage.Exceptions;
using Moq;
using Xunit;

namespace MessageStorage.Db.MsSqlUnitTests.MsSqlDbStorageAdaptorTests
{
    public class MsSqlDbStorageAdaptor_SetConfigurationTests
    {
        private readonly MsSql.MsSqlDbStorageAdaptor _sut;

        public MsSqlDbStorageAdaptor_SetConfigurationTests()
        {
            var mockMsSqlMigrationRunner = new Mock<IMsSqlMigrationRunner>();
            _sut = new MsSql.MsSqlDbStorageAdaptor(mockMsSqlMigrationRunner.Object);
        }

        [Fact]
        public void WhenSetConfigurationExecutedWithNull__ArgumentNullExceptionOccurs()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.SetConfiguration(null));
        }

        [Fact(Skip = "This validation is not executed yet. Maybe MessageStorageDbConfiguration is converted to abstract and concretes created via IMsSqlDbConnection")]
        public void WhenDbConfigurationSetOperationExecuted_InvalidConnectionStr__DbConnectionReturnClosedState()
        {
            const string connectionStr = "dummy-connection-str";
            MessageStorageDbConfiguration messageStorageDbConfiguration = MessageStorageDbConfigurationFactory.Create(connectionStr);
            Assert.Throws<ConnectionStringValidationException>(() => _sut.SetConfiguration(messageStorageDbConfiguration));
        }
    }
}