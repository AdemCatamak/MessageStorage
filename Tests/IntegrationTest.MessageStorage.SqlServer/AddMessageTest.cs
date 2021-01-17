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

            dotMemory.Check(memory => Assert.Equal(0,
                                                   memory.GetObjects(o => o.Type.Is<IDbConnection>()).ObjectsCount
                                                  )
                           );
            dotMemory.Check(memory => Assert.Equal(0,
                                                   memory.GetObjects(o => o.Type.Is<IDbTransaction>()).ObjectsCount
                                                  )
                           );
        }
    }
}