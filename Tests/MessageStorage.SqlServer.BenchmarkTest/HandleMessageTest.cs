using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.BackgroundTasks;
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
    public class HandleMessageTest
    {
        private readonly IMessageStorageClient _messageStorageClient;
        private readonly JobDispatcher _jobDispatcher;

        private const string SCHEMA = "handle_message_load_test";

        private readonly ITestOutputHelper _output;


        public HandleMessageTest(SqlServerInfraFixture sqlServerInfraInfraFixture, ITestOutputHelper output)
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
            _jobDispatcher = new JobDispatcher(messageHandlerProvider, sqlServerRepositoryFactory);
        }

        [ReleaseModeTheory]
        [InlineData(1, 1000, 10000)]
        [InlineData(2, 1000, 7500)]
        [InlineData(4, 1000, 5000)]
        [InlineData(1, 100, 1500)]
        [InlineData(2, 100, 1000)]
        [InlineData(4, 100, 750)]
        public void When_JobHandledCalledWithParallel__ResponseTimeShouldNotExceed(int parallelJobCount, int times, int expectedMilliseconds)
        {
            List<Task> addTasks = new List<Task>();
            for (var i = 0; i < times; i++)
            {
                DummyMessage dummyMessage = new DummyMessage
                {
                    Guid = Guid.NewGuid()
                };
                var task = _messageStorageClient.AddMessageAsync(dummyMessage);
                addTasks.Add(task);
            }

            Task.WaitAll(addTasks.ToArray());
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            List<Task> jobProcessorTasks = new List<Task>();
            for (var i = 0; i < parallelJobCount; i++)
            {
                Task task = Task.Run(async () =>
                                     {
                                         bool jobHandled;
                                         do
                                         {
                                             jobHandled = await _jobDispatcher.HandleNextJobAsync();
                                         } while (jobHandled);
                                     }
                                    );
                jobProcessorTasks.Add(task);
            }

            Task.WaitAll(jobProcessorTasks.ToArray());

            stopwatch.Stop();

            string message = $"Parallel Job Count : {parallelJobCount}{Environment.NewLine}" +
                             $"Executed Job Count : {times}{Environment.NewLine}" +
                             $"Expected Execution Time : {expectedMilliseconds} ms{Environment.NewLine}" +
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