using System;
using System.Data;
using JetBrains.dotMemoryUnit;
using MessageStorage;
using MessageStorage.Clients;
using MessageStorage.Clients.Imp;
using MessageStorage.Configurations;
using MessageStorage.DataAccessSection;
using MessageStorage.SqlServer.DataAccessSection;
using Microsoft.Data.SqlClient;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTest.MessageStorage.SqlServer
{
    [DotMemoryUnit(FailIfRunWithoutSupport = false)]
    public class MessageStorageClientDisposeTests : IClassFixture<SqlServerTestFixture>
    {
        private readonly SqlServerTestFixture _sqlServerTestFixture;

        public MessageStorageClientDisposeTests(SqlServerTestFixture sqlServerTestFixture, ITestOutputHelper outputHelper)
        {
            _sqlServerTestFixture = sqlServerTestFixture;
            DotMemoryUnitTestOutput.SetOutputMethod(outputHelper.WriteLine);
        }

        [Fact]
        public void When_BeginTransaction_MessageStorageTransactionCommitted__MessageStorageClientShouldBeDisposed()
        {
            dotMemory.Check(memory => Assert.Equal(0,
                                                   memory.GetObjects(o => o.Type.Is<MessageStorageClient>())
                                                         .ObjectsCount
                                                  )
                           );
            dotMemory.Check(memory => Assert.Equal(0,
                                                   memory.GetObjects(o => o.Type.Is<MessageStorageTransaction>())
                                                         .ObjectsCount
                                                  )
                           );
            dotMemory.Check(memory => Assert.Equal(0,
                                                   memory.GetObjects(o => o.Type.Is<SqlConnection>())
                                                         .ObjectsCount
                                                  )
                           );
            dotMemory.Check(memory => Assert.Equal(0,
                                                   memory.GetObjects(o => o.Type.Is<SqlTransaction>())
                                                         .ObjectsCount
                                                  )
                           );

            object myObj = "dummy-message";
            using (var repositoryContext = new SqlServerMessageStorageRepositoryContext(new MessageStorageRepositoryContextConfiguration(SqlServerTestFixture.CONNECTION_STR)))
            {
                using (IMessageStorageClient localClient = new MessageStorageClient(repositoryContext, new HandlerManager()))
                {
                    dotMemory.Check(memory => Assert.Equal(1,
                                                           memory.GetObjects(o => o.Type.Is<MessageStorageClient>())
                                                                 .ObjectsCount
                                                          )
                                   );
                    using (IMessageStorageTransaction messageStorageTransaction = localClient.BeginTransaction(IsolationLevel.ReadCommitted))
                    {
                        localClient.Add(myObj);
                        messageStorageTransaction.Commit();
                    }
                }
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            dotMemory.Check(memory => Assert.Equal(0,
                                                   memory.GetObjects(o => o.Type.Is<MessageStorageClient>())
                                                         .ObjectsCount
                                                  )
                           );
            dotMemory.Check(memory => Assert.Equal(0,
                                                   memory.GetObjects(o => o.Type.Is<MessageStorageTransaction>())
                                                         .ObjectsCount
                                                  )
                           );
            dotMemory.Check(memory => Assert.Equal(0,
                                                   memory.GetObjects(o => o.Type.Is<SqlConnection>())
                                                         .ObjectsCount
                                                  )
                           );
            dotMemory.Check(memory => Assert.Equal(0,
                                                   memory.GetObjects(o => o.Type.Is<SqlTransaction>())
                                                         .ObjectsCount
                                                  )
                           );
        }

        [Fact]
        public void When_UseTransaction_MessageStorageTransactionCommitted__MessageStorageClientShouldBeDisposed()
        {
            dotMemory.Check(memory => Assert.Equal(0,
                                                   memory.GetObjects(o => o.Type.Is<MessageStorageClient>())
                                                         .ObjectsCount
                                                  )
                           );
            dotMemory.Check(memory => Assert.Equal(0,
                                                   memory.GetObjects(o => o.Type.Is<MessageStorageTransaction>())
                                                         .ObjectsCount
                                                  )
                           );
            dotMemory.Check(memory => Assert.Equal(0,
                                                   memory.GetObjects(o => o.Type.Is<SqlConnection>())
                                                         .ObjectsCount
                                                  )
                           );
            dotMemory.Check(memory => Assert.Equal(0,
                                                   memory.GetObjects(o => o.Type.Is<SqlTransaction>())
                                                         .ObjectsCount
                                                  )
                           );

            object myObj = "dummy-message";
            using (IDbConnection connection = _sqlServerTestFixture.CreateDbConnection())
            {
                using (IMessageStorageClient localClient = new MessageStorageClient(
                                                                                    _sqlServerTestFixture.CreateMessageStorageSqlServerRepositoryContext(),
                                                                                    new HandlerManager()))
                {
                    dotMemory.Check(memory => Assert.Equal(2,
                                                           memory.GetObjects(o => o.Type.Is<MessageStorageClient>())
                                                                 .ObjectsCount
                                                          )
                                   );

                    connection.Open();
                    using (IDbTransaction dbTransaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                    {
                        using (IMessageStorageTransaction messageStorageTransaction =
                            localClient.UseTransaction(dbTransaction))
                        {
                            localClient.Add(myObj);
                            messageStorageTransaction.Commit();
                        }
                    }
                }
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            dotMemory.Check(memory => Assert.Equal(0,
                                                   memory.GetObjects(o => o.Type.Is<MessageStorageClient>())
                                                         .ObjectsCount
                                                  )
                           );
            dotMemory.Check(memory => Assert.Equal(0,
                                                   memory.GetObjects(o => o.Type.Is<MessageStorageTransaction>())
                                                         .ObjectsCount
                                                  )
                           );
            dotMemory.Check(memory => Assert.Equal(0,
                                                   memory.GetObjects(o => o.Type.Is<SqlConnection>())
                                                         .ObjectsCount
                                                  )
                           );
            dotMemory.Check(memory => Assert.Equal(0,
                                                   memory.GetObjects(o => o.Type.Is<SqlTransaction>())
                                                         .ObjectsCount
                                                  )
                           );
        }
    }
}