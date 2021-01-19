using System;
using System.Data;
using System.Threading.Tasks;
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
     
            dotMemory.Check(memory =>
            {
                Assert.Equal(0,
                    memory.GetObjects(o => o.Type.Is<MessageStorageClient>())
                        .ObjectsCount
                );

                Assert.Equal(0,
                    memory.GetObjects(o => o.Type.Is<MessageStorageTransaction>())
                        .ObjectsCount
                );

                Assert.Equal(0,
                    memory.GetObjects(o => o.Type.Is<SqlConnection>())
                        .ObjectsCount
                );

                Assert.Equal(0,
                    memory.GetObjects(o => o.Type.Is<SqlTransaction>())
                        .ObjectsCount
                );

            });

            object myObj = "dummy-message";
            using (var repositoryContext = new SqlServerMessageStorageRepositoryContext(new MessageStorageRepositoryContextConfiguration(SqlServerTestFixture.CONNECTION_STR)))
            {
                using IMessageStorageClient localClient = new MessageStorageClient(repositoryContext, new HandlerManager());
                using var messageStorageTransaction = localClient.BeginTransaction(IsolationLevel.ReadCommitted);
  
                dotMemory.Check(memory =>
                {
                    Assert.Equal(1,
                        memory.GetObjects(o => o.Type.Is<MessageStorageClient>())
                            .ObjectsCount
                    ); 
                
                    Assert.Equal(1,
                        memory.GetObjects(o => o.Type.Is<MessageStorageTransaction>())
                            .ObjectsCount
                    );

                    Assert.Equal(1,
                        memory.GetObjects(o => o.Type.Is<SqlConnection>())
                            .ObjectsCount
                    );

                    Assert.Equal(1,
                        memory.GetObjects(o => o.Type.Is<SqlTransaction>())
                            .ObjectsCount
                    );
                });
                
                localClient.Add(myObj);
                messageStorageTransaction.Commit();
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            for (int j = 0; j < 1000; j++)
            {
                // Wait
            }

      
            dotMemory.Check(memory =>
            {
                Assert.Equal(0,
                    memory.GetObjects(o => o.Type.Is<MessageStorageClient>())
                        .ObjectsCount
                ); 
                
                Assert.Equal(0,
                    memory.GetObjects(o => o.Type.Is<MessageStorageTransaction>())
                        .ObjectsCount
                );

                Assert.Equal(0,
                    memory.GetObjects(o => o.Type.Is<SqlConnection>())
                        .ObjectsCount
                );

                Assert.Equal(0,
                    memory.GetObjects(o => o.Type.Is<SqlTransaction>())
                        .ObjectsCount
                );

            });
        }

        [Fact]
        public void When_UseTransaction_MessageStorageTransactionCommitted__MessageStorageClientShouldBeDisposed()
        {
            dotMemory.Check(memory =>
            {
                Assert.Equal(0,
                    memory.GetObjects(o => o.Type.Is<MessageStorageClient>())
                        .ObjectsCount
                );

                Assert.Equal(0,
                    memory.GetObjects(o => o.Type.Is<MessageStorageTransaction>())
                        .ObjectsCount
                );

                Assert.Equal(0,
                    memory.GetObjects(o => o.Type.Is<SqlConnection>())
                        .ObjectsCount
                );

                Assert.Equal(0,
                    memory.GetObjects(o => o.Type.Is<SqlTransaction>())
                        .ObjectsCount
                );

            });

            object myObj = "dummy-message";
            using (IDbConnection connection = _sqlServerTestFixture.CreateDbConnection())
            {
                using var repositoryContext = _sqlServerTestFixture.CreateMessageStorageSqlServerRepositoryContext();
                using IMessageStorageClient localClient = new MessageStorageClient(repositoryContext, new HandlerManager());

                connection.Open();
                using IDbTransaction dbTransaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
                using IMessageStorageTransaction messageStorageTransaction = localClient.UseTransaction(dbTransaction);
               
                dotMemory.Check(memory =>
                {
                    Assert.Equal(1,
                        memory.GetObjects(o => o.Type.Is<MessageStorageClient>())
                            .ObjectsCount
                    ); 
                
                    Assert.Equal(1,
                        memory.GetObjects(o => o.Type.Is<MessageStorageTransaction>())
                            .ObjectsCount
                    );

                    Assert.Equal(1,
                        memory.GetObjects(o => o.Type.Is<SqlConnection>())
                            .ObjectsCount
                    );

                    Assert.Equal(1,
                        memory.GetObjects(o => o.Type.Is<SqlTransaction>())
                            .ObjectsCount
                    );
                });
                
                localClient.Add(myObj);
                messageStorageTransaction.Commit();
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            for (int j = 0; j < 1000; j++)
            {
                // Wait
            }

            
            dotMemory.Check(memory =>
            {
                Assert.Equal(0,
                    memory.GetObjects(o => o.Type.Is<MessageStorageClient>())
                        .ObjectsCount
                );

                Assert.Equal(0,
                    memory.GetObjects(o => o.Type.Is<MessageStorageTransaction>())
                        .ObjectsCount
                );

                Assert.Equal(0,
                    memory.GetObjects(o => o.Type.Is<SqlConnection>())
                        .ObjectsCount
                );

                Assert.Equal(0,
                    memory.GetObjects(o => o.Type.Is<SqlTransaction>())
                        .ObjectsCount
                );

            });
        }
    }
}