using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using JetBrains.dotMemoryUnit;
using MessageStorage;
using MessageStorage.Clients;
using MessageStorage.Clients.Imp;
using MessageStorage.DataAccessSection;
using MessageStorage.Models;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTest.MessageStorage.SqlServer
{
    public class AddMessageWithJobTest : IClassFixture<SqlServerTestFixture>
    {
        private readonly IMessageStorageClient _sut;

        public AddMessageWithJobTest(SqlServerTestFixture sqlServerTestFixture, ITestOutputHelper outputHelper)
        {
            DotMemoryUnitTestOutput.SetOutputMethod(outputHelper.WriteLine);

            IMessageStorageRepositoryContext repositoryContext = sqlServerTestFixture.CreateMessageStorageSqlServerRepositoryContext();
            var handlerDescription = new HandlerDescription<DummyHandler>(() => new DummyHandler());
            IHandlerManager handlerManager = new HandlerManager(new[] {handlerDescription});
            _sut = new MessageStorageClient(repositoryContext, handlerManager);
        }

        [Fact]
        public void WhenAddMessage_HandlerManagerContainsNotMatchingHandler__JobShouldNotBeCreated()
        {
            const string myObj = "some payload";

            var (message, jobs) = _sut.Add(myObj);

            using (IDbConnection connection = SqlServerTestFixture.CreateDbConnection())
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
        public void WhenAddMessage_HandlerManagerContainsMatchingHandler__JobShouldBePersisted()
        {
            const int myObj = 42;

            var (message, jobs) = _sut.Add(myObj);

            using (IDbConnection connection = SqlServerTestFixture.CreateDbConnection())
            {
                IEnumerable<Job> jobList = jobs.ToList();
                Assert.Single(jobList);
                var messageCount = connection.ExecuteScalar<int>($"Select count(*) from [{SqlServerTestFixture.SCHEMA}].[Messages] where MessageId = @messageId",
                                                                 new
                                                                 {
                                                                     messageId = message.Id
                                                                 });
                Assert.Equal(1, messageCount);

                var jobCount = connection.ExecuteScalar<int>($"Select count(*) from [{SqlServerTestFixture.SCHEMA}].[Jobs] where JobId = @jobId",
                                                             new
                                                             {
                                                                 jobId = jobList.First().Id
                                                             });
                Assert.Equal(1, jobCount);
            }
        }

        [Fact]
        public void WhenAddMessage_HandlerManagerContainsMatchingHandler_And_AutoJobCreationIsFalse__JobShouldNotBePersisted()
        {
            const int myObj = 42;

            var (_, jobs) = _sut.Add(myObj, false);

            Assert.Empty(jobs);
        }


        public class DummyHandler : Handler<int>
        {
            protected override Task HandleAsync(int payload, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}