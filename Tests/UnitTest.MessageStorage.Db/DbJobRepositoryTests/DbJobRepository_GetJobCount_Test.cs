using System;
using System.Collections.Generic;
using System.Data;
using MessageStorage;
using MessageStorage.Db;
using MessageStorage.Db.DataAccessLayer.QueryBuilders;
using MessageStorage.Db.DataAccessLayer.Repositories;
using Moq;
using NUnit.Framework;

namespace UnitTest.MessageStorage.Db.DbJobRepositoryTests
{
    public class DbJobRepository_GetJobCount_Test
    {
        private class DummyMessageStorageDbConfiguration : MessageStorageDbConfiguration
        {
            public override string ConnectionStr { get; protected set; }
        }

        private DbJobRepository _sut;

        private Mock<IJobQueryBuilder> _jobQueryBuilderMock;
        private Mock<IDbAdaptor> _dbAdaptorMock;

        [SetUp]
        public void SetUp()
        {
            _jobQueryBuilderMock = new Mock<IJobQueryBuilder>();
            _dbAdaptorMock = new Mock<IDbAdaptor>();

            _sut = new DbJobRepository(_jobQueryBuilderMock.Object, _dbAdaptorMock.Object, new DummyMessageStorageDbConfiguration());
        }

        [Test]
        public void WhenDbCommandExecutionThrowsException__TransactionCouldNotCompleted()
        {
            var dbCommandMock = new Mock<IDbCommand>();
            dbCommandMock.Setup(command => command.ExecuteScalar())
                         .Throws<ApplicationException>();
            dbCommandMock.Setup(command => command.Parameters)
                         .Returns(new Mock<IDataParameterCollection>().Object);

            var dbTransactionMock = new Mock<IDbTransaction>();
            var dbConnectionMock = new Mock<IDbConnection>();

            dbConnectionMock.Setup(connection => connection.CreateCommand())
                            .Returns(dbCommandMock.Object);
            dbConnectionMock.Setup(connection => connection.BeginTransaction(It.IsAny<IsolationLevel>()))
                            .Returns(dbTransactionMock.Object);
            dbTransactionMock.Setup(transaction => transaction.Connection)
                             .Returns(dbConnectionMock.Object);

            _dbAdaptorMock.Setup(adaptor => adaptor.CreateConnection(It.IsAny<string>()))
                          .Returns(dbConnectionMock.Object);

            Assert.Throws<ApplicationException>(() => _sut.GetJobCountByStatus(It.IsAny<JobStatuses>()));

            dbConnectionMock.Verify(connection => connection.BeginTransaction(It.IsAny<IsolationLevel>()), Times.Once);
            dbCommandMock.Verify(command => command.ExecuteScalar(), Times.Once);
            dbTransactionMock.Verify(transaction => transaction.Commit(), Times.Never);
        }

        [Test]
        public void WhenDbCommandExecuted__TransactionWillBeDisposed()
        {
            var dbCommandMock = new Mock<IDbCommand>();
            dbCommandMock.Setup(command => command.ExecuteScalar())
                         .Returns(1);
            dbCommandMock.Setup(command => command.Parameters)
                         .Returns(new Mock<IDataParameterCollection>().Object);

            var dbTransactionMock = new Mock<IDbTransaction>();
            var dbConnectionMock = new Mock<IDbConnection>();

            dbConnectionMock.Setup(connection => connection.CreateCommand())
                            .Returns(dbCommandMock.Object);
            dbConnectionMock.Setup(connection => connection.BeginTransaction(It.IsAny<IsolationLevel>()))
                            .Returns(dbTransactionMock.Object);
            dbTransactionMock.Setup(transaction => transaction.Connection)
                             .Returns(dbConnectionMock.Object);

            _dbAdaptorMock.Setup(adaptor => adaptor.CreateConnection(It.IsAny<string>()))
                          .Returns(dbConnectionMock.Object);

            _sut.GetJobCountByStatus(It.IsAny<JobStatuses>());

            dbConnectionMock.Verify(connection => connection.BeginTransaction(It.IsAny<IsolationLevel>()), Times.Once);
            dbCommandMock.Verify(command => command.ExecuteScalar(), Times.Once);
            dbTransactionMock.Verify(transaction => transaction.Dispose(), Times.Once);
        }

        [Test]
        public void WhenDbCommandExecuted_WithParameter__TransactionWillBeCommitted()
        {
            _jobQueryBuilderMock.Setup(builder => builder.GetJobCountByStatus(It.IsAny<JobStatuses>()))
                                .Returns(() => ("query-text", new List<IDbDataParameter>()
                                                              {
                                                                  new Mock<IDbDataParameter>().Object
                                                              }));

            var dbCommandMock = new Mock<IDbCommand>();
            dbCommandMock.Setup(command => command.ExecuteScalar())
                         .Returns(1);
            dbCommandMock.Setup(command => command.Parameters)
                         .Returns(new Mock<IDataParameterCollection>().Object);

            var dbTransactionMock = new Mock<IDbTransaction>();
            var dbConnectionMock = new Mock<IDbConnection>();

            dbConnectionMock.Setup(connection => connection.CreateCommand())
                            .Returns(dbCommandMock.Object);
            dbConnectionMock.Setup(connection => connection.BeginTransaction(It.IsAny<IsolationLevel>()))
                            .Returns(dbTransactionMock.Object);
            dbTransactionMock.Setup(transaction => transaction.Connection)
                             .Returns(dbConnectionMock.Object);

            _dbAdaptorMock.Setup(adaptor => adaptor.CreateConnection(It.IsAny<string>()))
                          .Returns(dbConnectionMock.Object);

            _sut.GetJobCountByStatus(It.IsAny<JobStatuses>());

            dbConnectionMock.Verify(connection => connection.BeginTransaction(It.IsAny<IsolationLevel>()), Times.Once);
            dbConnectionMock.Verify(connection => connection.Dispose(), Times.Once);
            dbTransactionMock.Verify(transaction => transaction.Commit(), Times.Never);
            dbTransactionMock.Verify(transaction => transaction.Dispose(), Times.Once);
            dbCommandMock.Verify(command => command.ExecuteScalar(), Times.Once);
            dbCommandMock.Verify(command => command.Parameters.Add(It.IsAny<IDbDataParameter>()), Times.Once);
        }
    }
}