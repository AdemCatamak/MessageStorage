using System;
using System.Collections.Generic;
using System.Data;
using MessageStorage;
using MessageStorage.Db;
using MessageStorage.Db.DataAccessLayer.QueryBuilders;
using MessageStorage.Db.DataAccessLayer.Repositories;
using Moq;
using NUnit.Framework;

namespace UnitTest.MessageStorage.Db.DbMessageRepositoryTests
{
    public class DbMessageRepository_AddTransactional_Test
    {
        private class DummyMessageStorageDbConfiguration : MessageStorageDbConfiguration
        {
            public override string ConnectionStr { get; protected set; }
        }

        private DbMessageRepository _sut;

        private Mock<IMessageQueryBuilder> _messageQueryBuilderMock;
        private Mock<IDbAdaptor> _dbAdaptorMock;
        private Mock<IDbTransaction> _dbTransactionMock;

        [SetUp]
        public void SetUp()
        {
            _dbTransactionMock = new Mock<IDbTransaction>();

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

            var dbConnectionMock = new Mock<IDbConnection>();

            dbConnectionMock.Setup(connection => connection.CreateCommand())
                            .Returns(dbCommandMock.Object);
            _dbTransactionMock.Setup(transaction => transaction.Connection)
                              .Returns(dbConnectionMock.Object);

            Assert.Throws<ApplicationException>(() => _sut.Add(It.IsAny<Message>(), _dbTransactionMock.Object));

            dbCommandMock.Verify(command => command.ExecuteScalar(), Times.Once);
            _dbAdaptorMock.Verify(adaptor => adaptor.CreateConnection(It.IsAny<string>()), Times.Never);
            dbConnectionMock.Verify(connection => connection.BeginTransaction(It.IsAny<IsolationLevel>()), Times.Never);
            _dbTransactionMock.Verify(transaction => transaction.Commit(), Times.Never);
        }

        [Test]
        public void WhenDbCommandExecuted__OperationCompletedSuccessfully_TransactionWillNotBeCommitted()
        {
            var dbCommandMock = new Mock<IDbCommand>();
            dbCommandMock.Setup(command => command.ExecuteScalar())
                         .Returns(1);
            dbCommandMock.Setup(command => command.Parameters)
                         .Returns(new Mock<IDataParameterCollection>().Object);

            var dbConnectionMock = new Mock<IDbConnection>();
            dbConnectionMock.Setup(connection => connection.CreateCommand())
                            .Returns(dbCommandMock.Object);
            _dbTransactionMock.Setup(transaction => transaction.Connection)
                              .Returns(dbConnectionMock.Object);

            _dbAdaptorMock.Setup(adaptor => adaptor.CreateConnection(It.IsAny<string>()))
                          .Returns(dbConnectionMock.Object);

            var message = new Message(null);
            _sut.Add(message, _dbTransactionMock.Object);

            dbCommandMock.Verify(command => command.ExecuteScalar(), Times.Once);
            dbConnectionMock.Verify(connection => connection.BeginTransaction(It.IsAny<IsolationLevel>()), Times.Never);
            _dbTransactionMock.Verify(transaction => transaction.Commit(), Times.Never);
        }

        [Test]
        public void WhenDbCommandExecuted_WithParameters__OperationCompletedSuccessfully_TransactionWillNotBeCommitted()
        {
            _messageQueryBuilderMock.Setup(builder => builder.Add(It.IsAny<Message>()))
                                    .Returns(() => ("query-text", new List<IDbDataParameter>()
                                                                  {
                                                                      new Mock<IDbDataParameter>().Object,
                                                                      new Mock<IDbDataParameter>().Object
                                                                  }));

            var dbCommandMock = new Mock<IDbCommand>();
            dbCommandMock.Setup(command => command.ExecuteScalar())
                         .Returns(1);
            dbCommandMock.Setup(command => command.Parameters)
                         .Returns(new Mock<IDataParameterCollection>().Object);

            var dbConnectionMock = new Mock<IDbConnection>();
            dbConnectionMock.Setup(connection => connection.CreateCommand())
                            .Returns(dbCommandMock.Object);
            _dbTransactionMock.Setup(transaction => transaction.Connection)
                              .Returns(dbConnectionMock.Object);

            _dbAdaptorMock.Setup(adaptor => adaptor.CreateConnection(It.IsAny<string>()))
                          .Returns(dbConnectionMock.Object);

            var message = new Message(null);
            _sut.Add(message, _dbTransactionMock.Object);

            dbCommandMock.Verify(command => command.ExecuteScalar(), Times.Once);
            dbConnectionMock.Verify(connection => connection.BeginTransaction(It.IsAny<IsolationLevel>()), Times.Never);
            _dbTransactionMock.Verify(transaction => transaction.Commit(), Times.Never);
            dbCommandMock.Verify(command => command.Parameters.Add(It.IsAny<IDbDataParameter>()), Times.Exactly(2));
        }
    }
}