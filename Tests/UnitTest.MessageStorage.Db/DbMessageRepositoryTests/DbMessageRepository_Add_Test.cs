using System;
using System.Collections.Generic;
using System.Data;
using MessageStorage;
using MessageStorage.Db;
using MessageStorage.Db.DataAccessLayer.QueryBuilders;
using MessageStorage.Db.DataAccessLayer.Repositories;
using MessageStorage.Exceptions;
using Moq;
using NUnit.Framework;

namespace UnitTest.MessageStorage.Db.DbMessageRepositoryTests
{
    public class DbMessageRepository_Add_Test
    {
        private class DummyMessageStorageDbConfiguration : MessageStorageDbConfiguration
        {
            public override string ConnectionStr { get; protected set; }
        }

        private DbMessageRepository _sut;

        private Mock<IMessageQueryBuilder> _messageQueryBuilderMock;
        private Mock<IDbAdaptor> _dbAdaptorMock;

        [SetUp]
        public void SetUp()
        {
            _messageQueryBuilderMock = new Mock<IMessageQueryBuilder>();
            _dbAdaptorMock = new Mock<IDbAdaptor>();

            _sut = new DbMessageRepository(_messageQueryBuilderMock.Object, _dbAdaptorMock.Object, new DummyMessageStorageDbConfiguration());
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

            Assert.Throws<ApplicationException>(() => _sut.Add(It.IsAny<Message>()));

            dbConnectionMock.Verify(connection => connection.BeginTransaction(It.IsAny<IsolationLevel>()), Times.Once);
            dbCommandMock.Verify(command => command.ExecuteScalar(), Times.Once);
            dbTransactionMock.Verify(transaction => transaction.Commit(), Times.Never);
        }

        [Test]
        public void WhenDbCommandExecuted_ResponseIsNotLong__ArgumentNotCompatibleExceptionOccurs()
        {
            var dbCommandMock = new Mock<IDbCommand>();
            dbCommandMock.Setup(command => command.ExecuteScalar())
                         .Returns("some-message");
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

            var message = new Message(null);
            Assert.Throws<ArgumentNotCompatibleException>(() => _sut.Add(message));

            dbConnectionMock.Verify(connection => connection.BeginTransaction(It.IsAny<IsolationLevel>()), Times.Once);
            dbCommandMock.Verify(command => command.ExecuteScalar(), Times.Once);
            dbTransactionMock.Verify(transaction => transaction.Commit(), Times.Never);
        }

        [Test]
        public void WhenDbCommandExecuted_ResponseIsLessThanZero__ArgumentNotCompatibleExceptionOccurs([Values((long) 0, (long) -1, (long) -42)]
                                                                                                       long result)
        {
            var dbCommandMock = new Mock<IDbCommand>();
            dbCommandMock.Setup(command => command.ExecuteScalar())
                         .Returns(result);
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

            var message = new Message(null);
            Assert.Throws<UnexpectedResponseException>(() => _sut.Add(message));

            dbConnectionMock.Verify(connection => connection.BeginTransaction(It.IsAny<IsolationLevel>()), Times.Once);
            dbCommandMock.Verify(command => command.ExecuteScalar(), Times.Once);
            dbTransactionMock.Verify(transaction => transaction.Commit(), Times.Never);
        }

        [Test]
        public void WhenDbCommandExecuted__TransactionWillBeCommitted()
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

            var message = new Message(null);
            _sut.Add(message);

            dbConnectionMock.Verify(connection => connection.BeginTransaction(It.IsAny<IsolationLevel>()), Times.Once);
            dbCommandMock.Verify(command => command.ExecuteScalar(), Times.Once);
            dbTransactionMock.Verify(transaction => transaction.Commit(), Times.Once);
        }

        [Test]
        public void WhenDbCommandExecuted_WithParameter__TransactionWillBeCommitted()
        {
            _messageQueryBuilderMock.Setup(builder => builder.Add(It.IsAny<Message>()))
                                    .Returns(() => ("query-text", new List<IDbDataParameter>
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

            var message = new Message(null);
            _sut.Add(message);

            dbConnectionMock.Verify(connection => connection.BeginTransaction(It.IsAny<IsolationLevel>()), Times.Once);
            dbCommandMock.Verify(command => command.ExecuteScalar(), Times.Once);
            dbTransactionMock.Verify(transaction => transaction.Commit(), Times.Once);
            dbCommandMock.Verify(command => command.Parameters.Add(It.IsAny<IDbDataParameter>()), Times.Once);
        }
    }
}