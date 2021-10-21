using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.Containers;
using MessageStorage.DataAccessLayer;
using MessageStorage.MessageHandlers;
using MessageStorage.SqlServer.BenchmarkTest.Fixtures;
using MessageStorage.SqlServer.Migrations;
using TestUtility;
using Xunit;
using Xunit.Abstractions;

namespace MessageStorage.SqlServer.BenchmarkTest
{
    [Collection(SqlServerInfraFixture.FIXTURE_KEY)]
    public class AddMessageTest
    {
        private readonly IMessageStorageClient _messageStorageClient;
        private const string SCHEMA = "add_message_load_test";

        private readonly ITestOutputHelper _output;


        public AddMessageTest(SqlServerInfraFixture sqlServerInfraInfraFixture, ITestOutputHelper output)
        {
            _output = output;

            MessageHandlerContainer messageHandlerContainer = new MessageHandlerContainer();
            messageHandlerContainer.Register<DummyMessageHandler>();
            IMessageHandlerProvider messageHandlerProvider = messageHandlerContainer.BuildMessageHandlerProvider();

            var repositoryConfiguration = new RepositoryConfiguration(sqlServerInfraInfraFixture.ConnectionString, SCHEMA);
            var sqlServerRepositoryFactory = new SqlServerRepositoryFactory(repositoryConfiguration);

            var executor = new SqlServerMigrationExecutor(repositoryConfiguration);
            executor.Execute();

            _messageStorageClient = new MessageStorageClient(messageHandlerProvider, sqlServerRepositoryFactory);
        }

        [ReleaseModeTheory]
        [InlineData(2000, 20000)]
        [InlineData(1000, 10000)]
        [InlineData(100, 1000)]
        public void When_AddMessageCalled__ResponseTimeShouldNotExceed(int times, int expectedMilliseconds)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < times; i++)
            {
                DummyMessage dummyMessage = new DummyMessage
                                            {
                    Guid = Guid.NewGuid()
                };
                var task = _messageStorageClient.AddMessageAsync(dummyMessage);
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());
            stopwatch.Stop();

            string message = $"Expected Execution Time : {expectedMilliseconds} ms{Environment.NewLine}" +
                             $"Actual Execution Time : {stopwatch.ElapsedMilliseconds} ms";
            AssertThat.LessThan(expectedMilliseconds, stopwatch.ElapsedMilliseconds, message);
            _output.WriteLine(message);
        }

        public class DummyMessage
        {
            public Guid Guid { get; set; }
        }

        public class DummyMessageHandler : BaseMessageHandler<DummyMessage>
        {
            public override Task HandleAsync(DummyMessage payload, CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }
        }
    }
}