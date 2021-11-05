using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.Containers;
using MessageStorage.DataAccessLayer;
using MessageStorage.MessageHandlers;
using MessageStorage.MySql.BenchmarkTest.Fixtures;
using MessageStorage.MySql.Migrations;
using TestUtility;
using Xunit;
using Xunit.Abstractions;

namespace MessageStorage.MySql.BenchmarkTest
{
    [Collection(MySqlInfraFixture.FIXTURE_KEY)]
    public class AddMessageTest
    {
        private readonly IMessageStorageClient _messageStorageClient;
        private readonly ITestOutputHelper _output;


        public AddMessageTest(MySqlInfraFixture mySqlInfraFixture, ITestOutputHelper output)
        {
            _output = output;

            MessageHandlerContainer messageHandlerContainer = new MessageHandlerContainer();
            messageHandlerContainer.Register<DummyMessageHandler>();
            IMessageHandlerProvider messageHandlerProvider = messageHandlerContainer.BuildMessageHandlerProvider();

            var repositoryConfiguration = new RepositoryConfiguration(mySqlInfraFixture.ConnectionString, mySqlInfraFixture.Database);
            var mySqlRepositoryFactory = new MySqlRepositoryFactory(repositoryConfiguration);

            var executor = new MySqlMigrationExecutor(repositoryConfiguration);
            executor.Execute();

            using var connection = mySqlRepositoryFactory.CreateConnection();
            connection.ExecuteAsync($"DELETE FROM {repositoryConfiguration.Schema}.jobs", null, CancellationToken.None).GetAwaiter().GetResult();

            _messageStorageClient = new MessageStorageClient(messageHandlerProvider, mySqlRepositoryFactory);
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