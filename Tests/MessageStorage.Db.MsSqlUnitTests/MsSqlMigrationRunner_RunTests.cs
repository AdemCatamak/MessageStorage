using System;
using System.Collections.Generic;
using System.Data;
using MessageStorage.Db.MsSql;
using Moq;
using Xunit;

namespace MessageStorage.Db.MsSqlUnitTests
{
    public class MsSqlMigrationRunner_RunTests
    {
        private MsSqlMigrationRunner _sut;

        public MsSqlMigrationRunner_RunTests()
        {
            _sut = new MsSqlMigrationRunner();
        }

        [Fact]
        public void WhenRunMigrationExecutedWithIMigration__TransactionCommitExecutedOnce()
        {
            var mockMsSqlConnectionFactory = new Mock<IMsSqlDbConnectionFactory>();
            var mockMigration = new Mock<IMigration>();

            var mockDbConnection = new Mock<IDbConnection>();
            var mockDbCommand = new Mock<IDbCommand>();
            var mockDbTransaction = new Mock<IDbTransaction>();

            mockMsSqlConnectionFactory.Setup(factory => factory.CreateConnection())
                                      .Returns(mockDbConnection.Object);

            mockDbTransaction.Setup(transaction => transaction.Connection)
                             .Returns(mockDbConnection.Object);

            mockDbConnection.Setup(connection => connection.BeginTransaction())
                            .Returns(mockDbTransaction.Object);

            mockDbConnection.Setup(connection => connection.CreateCommand())
                            .Returns(mockDbCommand.Object);

            mockDbCommand.Setup(command => command.Parameters.Add(It.IsAny<IDbDataParameter>()))
                         .Returns(() => 0);

            _sut.Run(new List<IMigration>() {mockMigration.Object}, mockMsSqlConnectionFactory.Object);


            mockMsSqlConnectionFactory.Verify(factory => factory.CreateConnection(), Times.Once);
            mockDbConnection.Verify(connection => connection.CreateCommand(), Times.Once);
            mockDbCommand.Verify(command => command.ExecuteNonQuery(), Times.Once);
            mockDbTransaction.Verify(transaction => transaction.Commit(), Times.Once);
        }

        [Fact]
        public void WhenRunMigrationExecutedWithTwoObjectOfIMigration__TransactionCommitExecutedTwice()
        {
            var mockMsSqlConnectionFactory = new Mock<IMsSqlDbConnectionFactory>();
            var mockMigration = new Mock<IMigration>();

            var mockDbConnection = new Mock<IDbConnection>();
            var mockDbCommand = new Mock<IDbCommand>();
            var mockDbTransaction = new Mock<IDbTransaction>();

            mockMsSqlConnectionFactory.Setup(factory => factory.CreateConnection())
                                      .Returns(mockDbConnection.Object);

            mockDbTransaction.Setup(transaction => transaction.Connection)
                             .Returns(mockDbConnection.Object);

            mockDbConnection.Setup(connection => connection.BeginTransaction())
                            .Returns(mockDbTransaction.Object);

            mockDbConnection.Setup(connection => connection.CreateCommand())
                            .Returns(mockDbCommand.Object);

            mockDbCommand.Setup(command => command.Parameters.Add(It.IsAny<IDbDataParameter>()))
                         .Returns(() => 0);

            _sut.Run(new List<IMigration>() {mockMigration.Object, mockMigration.Object}, mockMsSqlConnectionFactory.Object);


            mockMsSqlConnectionFactory.Verify(factory => factory.CreateConnection(), Times.Exactly(2));
            mockDbConnection.Verify(connection => connection.CreateCommand(), Times.Exactly(2));
            mockDbCommand.Verify(command => command.ExecuteNonQuery(), Times.Exactly(2));
            mockDbTransaction.Verify(transaction => transaction.Commit(), Times.Exactly(2));
        }

        [Fact]
        public void WhenRunMigrationExecutedWithIVersionedMigration__TransactionCommitExecutedOnce()
        {
            var mockMsSqlConnectionFactory = new Mock<IMsSqlDbConnectionFactory>();
            var mockVersionedMigration = new Mock<IVersionedMigration>();

            var mockDbConnection = new Mock<IDbConnection>();
            var mockDbCommand = new Mock<IDbCommand>();
            var mockDbTransaction = new Mock<IDbTransaction>();

            var messageStorageDbConfiguration = MessageStorageDbConfigurationFactory.Create("connectionStr");
            mockMsSqlConnectionFactory.Setup(factory => factory.MessageStorageDbConfiguration)
                                      .Returns(messageStorageDbConfiguration);

            mockVersionedMigration.Setup(migration => migration.VersionNumber)
                                  .Returns(1);
            
            mockMsSqlConnectionFactory.Setup(factory => factory.CreateConnection())
                                      .Returns(mockDbConnection.Object);

            mockDbTransaction.Setup(transaction => transaction.Connection)
                             .Returns(mockDbConnection.Object);

            mockDbConnection.Setup(connection => connection.BeginTransaction())
                            .Returns(mockDbTransaction.Object);

            mockDbConnection.Setup(connection => connection.CreateCommand())
                            .Returns(mockDbCommand.Object);

            mockDbCommand.Setup(command => command.Parameters.Add(It.IsAny<IDbDataParameter>()))
                         .Returns(() => 0);

            mockDbCommand.Setup(command => command.ExecuteScalar())
                         .Returns(0);

            _sut.Run(new List<IMigration> {mockVersionedMigration.Object}, mockMsSqlConnectionFactory.Object);


            // Last Executed Migration Query Operation || Migration-MigrationHistory Insert Operation
            mockMsSqlConnectionFactory.Verify(factory => factory.CreateConnection(), Times.Exactly(2));
            // Last Executed Migration Query Operation || Migration Execution || MigrationHistory Insert Operation
            mockDbConnection.Verify(connection => connection.CreateCommand(), Times.Exactly(3));
            // Migration Execution || MigrationHistory Insert Operation
            mockDbCommand.Verify(command => command.ExecuteNonQuery(), Times.Exactly(2));
            mockDbTransaction.Verify(transaction => transaction.Commit(), Times.Once);
        }
    }
}