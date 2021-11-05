using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.BackgroundTasks;
using MessageStorage.Containers;
using MessageStorage.DataAccessLayer;
using MessageStorage.MessageHandlers;
using MessageStorage.MySql.BenchmarkTest.Fixtures;
using MessageStorage.MySql.DbClient;
using MessageStorage.MySql.Migrations;
using TestUtility;
using Xunit;
using Xunit.Abstractions;

namespace MessageStorage.MySql.BenchmarkTest
{
    [Collection(MySqlInfraFixture.FIXTURE_KEY)]
    public class HandleMessageTest
    {
        private readonly IMessageStorageClient _messageStorageClient;
        private readonly JobDispatcher _jobDispatcher;

        private readonly ITestOutputHelper _output;


        public HandleMessageTest(MySqlInfraFixture mySqlInfraFixture, ITestOutputHelper output)
        {
            _output = output;

            MessageHandlerContainer messageHandlerContainer = new MessageHandlerContainer();
            messageHandlerContainer.Register<DummyMessageHandler>();
            IMessageHandlerProvider messageHandlerProvider = messageHandlerContainer.BuildMessageHandlerProvider();

            var repositoryConfiguration = new RepositoryConfiguration(mySqlInfraFixture.ConnectionString, mySqlInfraFixture.Database);
            var mySqlRepositoryFactory = new MySqlRepositoryFactory(repositoryConfiguration);

            var executor = new MySqlMigrationExecutor(repositoryConfiguration);
            executor.Execute();

            using IMySqlMessageStorageConnection? connection = mySqlRepositoryFactory.CreateConnection();
            connection.ExecuteAsync($"DELETE FROM {repositoryConfiguration.Schema}.jobs", null, CancellationToken.None).GetAwaiter().GetResult();

            _messageStorageClient = new MessageStorageClient(messageHandlerProvider, mySqlRepositoryFactory);
            _jobDispatcher = new JobDispatcher(messageHandlerProvider, mySqlRepositoryFactory);
        }

        [ReleaseModeTheory]
        [InlineData(1, 1000, 10000)]
        [InlineData(2, 1000, 7500)]
        [InlineData(4, 1000, 5000)]
        [InlineData(1, 100, 1500)]
        [InlineData(2, 100, 1000)]
        [InlineData(4, 100, 750)]
        public void When_JobHandledCalledWithParallel__ResponseTimeShouldNotExceed(int concurrentJobCount, int times, int expectedMilliseconds)
        {
            List<Task> tasks = new List<Task>();
            for (var i = 0; i < times; i++)
            {
                DummyMessage dummyMessage = new DummyMessage
                                            {
                                                Guid = Guid.NewGuid()
                                            };
                var task = _messageStorageClient.AddMessageAsync(dummyMessage);
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            List<Task> jobProcessorTasks = new List<Task>();
            for (var i = 0; i < concurrentJobCount; i++)
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

            string message = $"Parallel Job Count : {concurrentJobCount}{Environment.NewLine}" +
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