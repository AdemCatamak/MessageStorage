using System;
using System.Data;
using Dapper;
using JetBrains.dotMemoryUnit;
using MessageStorage;
using MessageStorage.Clients;
using MessageStorage.Clients.Imp;
using MessageStorage.DataAccessSection;
using Xunit;

namespace IntegrationTest.MessageStorage.SqlServer
{
    [DotMemoryUnit(FailIfRunWithoutSupport = false)]
    public class AddMessageTest : IClassFixture<SqlServerTestFixture>
    {
        private readonly SqlServerTestFixture _sqlServerTestFixture;

        private readonly IMessageStorageClient _sut;

        public AddMessageTest(SqlServerTestFixture sqlServerTestFixture)
        {
            _sqlServerTestFixture = sqlServerTestFixture;

            IMessageStorageRepositoryContext repositoryContext = _sqlServerTestFixture.CreateMessageStorageSqlServerRepositoryContext();
            IHandlerManager handlerManager = new HandlerManager();
            _sut = new MessageStorageClient(repositoryContext, handlerManager);
        }

        [Fact]
        public void When_AddMessage_NotThrowsException__MessageShouldBePersisted()
        {
            object myObj = "dummy-message";
            var (message, jobs) = _sut.Add(myObj);

            using (IDbConnection connection = _sqlServerTestFixture.CreateDbConnection())
            {
                Assert.Empty(jobs);
                var count = connection.ExecuteScalar<int>($"Select count(*) from [{SqlServerTestFixture.SCHEMA}].[Messages] where MessageId = @messageId",
                                                          new
                                                          {
                                                              messageId = message.Id
                                                          });
                Assert.Equal(1, count);
            }
        }

        [Fact]
        public void When_AddMessage_TransactionNotCommitted__MessageShouldNotBePersisted()
        {
            object myObj = "dummy-message";
            using (IDbConnection connection = _sqlServerTestFixture.CreateDbConnection())
            {
                connection.Open();
                string messageId;
                using (IDbTransaction dbTransaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    _sut.UseTransaction(dbTransaction);
                    var (message, _) = _sut.Add(myObj);
                    messageId = message.Id;

                    var countInTransaction = connection.ExecuteScalar<int>($"Select count(*) from [{SqlServerTestFixture.SCHEMA}].[Messages] where MessageId = @messageId",
                                                                           new
                                                                           {
                                                                               messageId
                                                                           },
                                                                           dbTransaction);
                    Assert.Equal(1, countInTransaction);
                }

                var count = connection.ExecuteScalar<int>($"Select count(*) from [{SqlServerTestFixture.SCHEMA}].[Messages] where MessageId = @messageId",
                                                          new
                                                          {
                                                              messageId
                                                          });
                Assert.Equal(0, count);
            }
        }

        [Fact]
        public void When_AddMessage_MessageStorageTransactionCommitted__MessageShouldBePersisted()
        {
            object myObj = "dummy-message";
            using (IDbConnection connection = _sqlServerTestFixture.CreateDbConnection())
            {
                connection.Open();
                string messageId;
                using (var dbTransaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    IMessageStorageTransaction messageStorageTransaction = _sut.UseTransaction(dbTransaction);
                    var (message, _) = _sut.Add(myObj);
                    messageId = message.Id;
                    messageStorageTransaction.Commit();

                    var countInTransaction = connection.ExecuteScalar<int>($"Select count(*) from [{SqlServerTestFixture.SCHEMA}].[Messages] where MessageId = @messageId",
                                                                           new
                                                                           {
                                                                               messageId
                                                                           },
                                                                           dbTransaction);
                    Assert.Equal(1, countInTransaction);
                }

                var count = connection.ExecuteScalar<int>($"Select count(*) from [{SqlServerTestFixture.SCHEMA}].[Messages] where MessageId = @messageId",
                                                          new
                                                          {
                                                              messageId
                                                          });
                Assert.Equal(1, count);
            }
        }

        [Fact]
        public void When_AddMessage_MessageStorageTransactionCommitted__MessageStorageClientShouldBeAcceptNewTransaction()
        {
            object myObj = "dummy-message";
            using (IDbConnection connection = _sqlServerTestFixture.CreateDbConnection())
            {
                connection.Open();
                string messageId1, messageId2;
                using (IDbTransaction dbTransaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    IMessageStorageTransaction messageStorageTransaction = _sut.UseTransaction(dbTransaction);
                    var (message, _) = _sut.Add(myObj);
                    messageId1 = message.Id;
                    messageStorageTransaction.Commit();
                }

                using (IDbTransaction dbTransaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    IMessageStorageTransaction messageStorageTransaction = _sut.UseTransaction(dbTransaction);
                    var (message, _) = _sut.Add(myObj);
                    messageId2 = message.Id;
                    messageStorageTransaction.Commit();
                }

                var count = connection.ExecuteScalar<int>($"Select count(*) from [{SqlServerTestFixture.SCHEMA}].[Messages] where MessageId in (@messageId1, @messageId2)",
                                                          new
                                                          {
                                                              messageId1, messageId2
                                                          }
                                                         );
                Assert.Equal(2, count);
            }
        }


        [Fact]
        public void When_AddMessage_MessageStorageTransactionCommitted__MessageStorageClientShouldBeDisposed()
        {
            dotMemory.Check(memory => Assert.Equal(1,
                                                   memory.GetObjects(o => o.Type.Is<IMessageStorageClient>())
                                                         .ObjectsCount
                                                  )
                           );

            object myObj = "dummy-message";
            using (IDbConnection connection = _sqlServerTestFixture.CreateDbConnection())
            {
                connection.Open();
                using (IDbTransaction dbTransaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    IMessageStorageTransaction messageStorageTransaction = _sut.UseTransaction(dbTransaction);
                    _sut.Add(myObj);
                    messageStorageTransaction.Commit();
                }
            }

            dotMemory.Check(memory => Assert.Equal(0,
                                                   memory.GetObjects(o => o.Type.Is<IDbConnection>())
                                                         .ObjectsCount
                                                  )
                           );
            dotMemory.Check(memory => Assert.Equal(0,
                                                   memory.GetObjects(o => o.Type.Is<IDbTransaction>())
                                                         .ObjectsCount
                                                  )
                           );
            dotMemory.Check(memory => Assert.Equal(0,
                                                   memory.GetObjects(o => o.Type.Is<IMessageStorageClient>())
                                                         .ObjectsCount
                                                  )
                           );
        }
    }
}