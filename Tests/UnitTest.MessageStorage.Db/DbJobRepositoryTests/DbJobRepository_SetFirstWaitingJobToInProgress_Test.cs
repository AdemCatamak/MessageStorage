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
    public class DbJobRepository_SetFirstWaitingJobToInProgress_Test
    {
        private class DummyMessageStorageDbConfiguration : MessageStorageDbConfiguration
        {
            public override string ConnectionStr { get; protected set; }
        }

        private DbJobRepository _sut;

        private Mock<IJobQueryBuilder> _jobQueryBuilderMock;
        private Mock<IDbAdaptor> _dbAdaptorMock;
        private DummyMessageStorageDbConfiguration _messageStorageDbConfiguration;

        [SetUp]
        public void SetUp()
        {
            _messageStorageDbConfiguration = new DummyMessageStorageDbConfiguration();
            _jobQueryBuilderMock = new Mock<IJobQueryBuilder>();
            _jobQueryBuilderMock.Setup(builder => builder.MapData(It.IsAny<DataRowCollection>()))
                                .Returns<DataRowCollection>((dataRowCollection) =>
                                                            {
                                                                var jobs = new List<Job>();

                                                                foreach (DataRow dataRow in dataRowCollection)
                                                                {
                                                                    if (dataRow[nameof(Job.Id)] is long id)
                                                                    {
                                                                        var job = new Job(id, It.IsAny<Message>(), It.IsAny<string>(), It.IsAny<JobStatuses>(), It.IsAny<DateTime>(), It.IsAny<string>());
                                                                        jobs.Add(job);
                                                                    }
                                                                }

                                                                return jobs;
                                                            });
            _dbAdaptorMock = new Mock<IDbAdaptor>();

            _sut = new DbJobRepository(_jobQueryBuilderMock.Object, _dbAdaptorMock.Object, _messageStorageDbConfiguration);
        }

        [Test]
        public void WhenDbAdaptorFillThrowsException__TransactionCouldNotCompleted()
        {
            var dbConnectionMock = new Mock<IDbConnection>();
            var dbTransactionMock = new Mock<IDbTransaction>();
            var dbCommandMock = new Mock<IDbCommand>();
            var dataAdaptorMock = new Mock<IDataAdapter>();

            dbConnectionMock.Setup(connection => connection.BeginTransaction(It.IsAny<IsolationLevel>()))
                            .Returns(dbTransactionMock.Object);
            dbConnectionMock.Setup(connection => connection.CreateCommand())
                            .Returns(dbCommandMock.Object);
            dbTransactionMock.Setup(transaction => transaction.Connection)
                             .Returns(dbConnectionMock.Object);

            _dbAdaptorMock.Setup(adaptor => adaptor.CreateConnection(_messageStorageDbConfiguration.ConnectionStr))
                          .Returns(dbConnectionMock.Object);
            _dbAdaptorMock.Setup(adaptor => adaptor.CreateDataAdaptor(dbCommandMock.Object))
                          .Returns(dataAdaptorMock.Object);

            dataAdaptorMock.Setup(adaptor => adaptor.Fill(It.IsAny<DataSet>()))
                           .Throws<ApplicationException>();

            Assert.Throws<ApplicationException>(() => _sut.SetFirstWaitingJobToInProgress());

            dbTransactionMock.Verify(transaction => transaction.Commit(), Times.Never);
            dbTransactionMock.Verify(transaction => transaction.Dispose(), Times.Once);
            dbConnectionMock.Verify(connection => connection.Dispose(), Times.Once);
            dbCommandMock.Verify(dbCommand => dbCommand.Dispose(), Times.Once);
        }

        [Test]
        public void WhenDbAdaptorFillSuccessfully_ThereIsNoData_WithParameter__TransactionCompleted_And_ResultIsNull()
        {
            _jobQueryBuilderMock.Setup(builder => builder.SetFirstWaitingJobToInProgress())
                                .Returns(() => ("query-text", new List<IDbDataParameter>
                                                              {
                                                                  new Mock<IDbDataParameter>().Object
                                                              }));

            var dbConnectionMock = new Mock<IDbConnection>();
            var dbTransactionMock = new Mock<IDbTransaction>();
            var dbCommandMock = new Mock<IDbCommand>();
            var dataAdaptorMock = new Mock<IDataAdapter>();

            dbCommandMock.Setup(command => command.Parameters)
                         .Returns(new Mock<IDataParameterCollection>().Object);
            dbConnectionMock.Setup(connection => connection.BeginTransaction(It.IsAny<IsolationLevel>()))
                            .Returns(dbTransactionMock.Object);
            dbConnectionMock.Setup(connection => connection.CreateCommand())
                            .Returns(dbCommandMock.Object);
            dbTransactionMock.Setup(transaction => transaction.Connection)
                             .Returns(dbConnectionMock.Object);

            _dbAdaptorMock.Setup(adaptor => adaptor.CreateConnection(_messageStorageDbConfiguration.ConnectionStr))
                          .Returns(dbConnectionMock.Object);
            _dbAdaptorMock.Setup(adaptor => adaptor.CreateDataAdaptor(dbCommandMock.Object))
                          .Returns(dataAdaptorMock.Object);

            dataAdaptorMock.Setup(adaptor => adaptor.Fill(It.IsAny<DataSet>()))
                           .Returns<DataSet>(dataSet =>
                                             {
                                                 dataSet.Tables.Add(new DataTable());
                                                 return 1;
                                             });

            Job job = _sut.SetFirstWaitingJobToInProgress();

            Assert.Null(job);
            dbTransactionMock.Verify(transaction => transaction.Commit(), Times.Once);
            dbTransactionMock.Verify(transaction => transaction.Dispose(), Times.Once);
            dbConnectionMock.Verify(connection => connection.Dispose(), Times.Once);
            dbCommandMock.Verify(dbCommand => dbCommand.Dispose(), Times.Once);

            dbCommandMock.Verify(command => command.Parameters.Add(It.IsAny<IDbDataParameter>()), Times.Once);
        }


        [Test]
        public void WhenDbAdaptorFillSuccessfully_ThereIsDataInSet__TransactionCompleted_And_ResultShouldNotBeNull()
        {
            const long id = 3;
            var dbConnectionMock = new Mock<IDbConnection>();
            var dbTransactionMock = new Mock<IDbTransaction>();
            var dbCommandMock = new Mock<IDbCommand>();
            var dataAdaptorMock = new Mock<IDataAdapter>();

            dbConnectionMock.Setup(connection => connection.BeginTransaction(It.IsAny<IsolationLevel>()))
                            .Returns(dbTransactionMock.Object);
            dbConnectionMock.Setup(connection => connection.CreateCommand())
                            .Returns(dbCommandMock.Object);
            dbTransactionMock.Setup(transaction => transaction.Connection)
                             .Returns(dbConnectionMock.Object);

            _dbAdaptorMock.Setup(adaptor => adaptor.CreateConnection(_messageStorageDbConfiguration.ConnectionStr))
                          .Returns(dbConnectionMock.Object);
            _dbAdaptorMock.Setup(adaptor => adaptor.CreateDataAdaptor(dbCommandMock.Object))
                          .Returns(dataAdaptorMock.Object);

            dataAdaptorMock.Setup(adaptor => adaptor.Fill(It.IsAny<DataSet>()))
                           .Returns<DataSet>(dataSet =>
                                             {
                                                 var dataTable = new DataTable();
                                                 dataTable.Clear();
                                                 dataTable.Columns.Add(nameof(Job.Id), id.GetType());
                                                 DataRow dataRow = dataTable.NewRow();
                                                 dataRow[nameof(Job.Id)] = id;
                                                 dataTable.Rows.Add(dataRow);

                                                 dataSet.Tables.Add(dataTable);
                                                 return 1;
                                             });

            Job job = _sut.SetFirstWaitingJobToInProgress();

            Assert.NotNull(job);
            Assert.AreEqual(id, job.Id);
            dbTransactionMock.Verify(transaction => transaction.Commit(), Times.Once);
            dbTransactionMock.Verify(transaction => transaction.Dispose(), Times.Once);
            dbConnectionMock.Verify(connection => connection.Dispose(), Times.Once);
            dbCommandMock.Verify(dbCommand => dbCommand.Dispose(), Times.Once);
        }
    }
}