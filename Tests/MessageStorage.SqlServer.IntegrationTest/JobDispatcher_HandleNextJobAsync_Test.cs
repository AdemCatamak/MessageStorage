using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.BackgroundTasks;
using MessageStorage.Containers;
using MessageStorage.DataAccessLayer;
using MessageStorage.MessageHandlers;
using MessageStorage.SqlServer.IntegrationTest.Fixtures;
using MessageStorage.SqlServer.Migrations;
using TestUtility;
using Xunit;
using Xunit.Abstractions;

namespace MessageStorage.SqlServer.IntegrationTest
{
    [Collection(SqlServerInfraFixture.FIXTURE_KEY)]
    public class JobDispatcher_HandleNextJobAsync_Test
    {
        private readonly IMessageStorageClient _messageStorageClient;
        private readonly JobDispatcher _sut;

        private const string SCHEMA = "handle_message_integration_test";

        private readonly ITestOutputHelper _output;


        public JobDispatcher_HandleNextJobAsync_Test(SqlServerInfraFixture sqlServerInfraInfraFixture, ITestOutputHelper output)
        {
            _output = output;

            MessageHandlerContainer messageHandlerContainer = new MessageHandlerContainer();
            messageHandlerContainer.Register<DummyMessageHandler>();
            messageHandlerContainer.Register<LongProcessHandler>();
            IMessageHandlerProvider messageHandlerProvider = messageHandlerContainer.BuildMessageHandlerProvider();

            var repositoryConfiguration = new RepositoryConfiguration(sqlServerInfraInfraFixture.ConnectionString, SCHEMA);
            var sqlServerRepositoryFactory = new SqlServerRepositoryFactory(repositoryConfiguration);

            var executor = new SqlServerMigrationExecutor(repositoryConfiguration);
            executor.Execute();

            _messageStorageClient = new MessageStorageClient(messageHandlerProvider, sqlServerRepositoryFactory);
            _sut = new JobDispatcher(messageHandlerProvider, sqlServerRepositoryFactory);
        }

        [Theory]
        [InlineData(1, 100)]
        [InlineData(4, 100)]
        [InlineData(8, 100)]
        [InlineData(1, 300)]
        [InlineData(4, 300)]
        [InlineData(8, 300)]
        [InlineData(12, 300)]
        [InlineData(16, 300)]
        [InlineData(1, 1000)]
        [InlineData(4, 1000)]
        [InlineData(8, 1000)]
        [InlineData(12, 1000)]
        [InlineData(16, 1000)]
        public void When_JobHandledCalledWithParallel__InitialJobCountAndJobExecutionCountShouldBeEqual(int parallelJobCount, int initialJobCount)
        {
            List<Task> addTasks = new List<Task>();
            for (var i = 0; i < initialJobCount; i++)
            {
                DummyMessage dummyMessage = new DummyMessage
                {
                    Guid = Guid.NewGuid()
                };
                var task = _messageStorageClient.AddMessageAsync(dummyMessage);
                addTasks.Add(task);
            }

            Task.WaitAll(addTasks.ToArray());

            List<Task> jobProcessorTasks = new List<Task>();
            int actualExecutedJobCount = 0;
            for (var i = 0; i < parallelJobCount; i++)
            {
                Task task = Task.Run(async () =>
                                     {
                                         bool jobHandled;
                                         do
                                         {
                                             jobHandled = await _sut.HandleNextJobAsync();
                                             if (jobHandled)
                                             {
                                                 Interlocked.Increment(ref actualExecutedJobCount);
                                             }
                                         } while (jobHandled);
                                     }
                                    );
                jobProcessorTasks.Add(task);
            }

            Task.WaitAll(jobProcessorTasks.ToArray());

            string message = $"Parallel Job Count : {parallelJobCount}{Environment.NewLine}" +
                             $"Expected Executed Job Count : {initialJobCount}{Environment.NewLine}" +
                             $"Actual Executed Job Count : {actualExecutedJobCount}";
            Assert.Equal(initialJobCount, actualExecutedJobCount);
            _output.WriteLine(message);
        }

        [Fact]
        public void WhenParallelJobIsAllowed__TasksShouldNotWaitEachOther()
        {
            TimeSpan jobConsumeTime = TimeSpan.FromSeconds(5);

            const int jobCount = 8;
            List<Task> addTasks = new List<Task>();
            for (var i = 0; i < jobCount; i++)
            {
                LongProcessRequest longProcessRequest = new LongProcessRequest(jobConsumeTime);
                var task = _messageStorageClient.AddMessageAsync(longProcessRequest);
                addTasks.Add(task);
            }

            Task.WaitAll(addTasks.ToArray());

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            List<Task> jobProcessorTasks = new List<Task>();
            for (var i = 0; i < jobCount; i++)
            {
                var handleTask = _sut.HandleNextJobAsync();
                jobProcessorTasks.Add(handleTask);
            }

            Task.WaitAll(jobProcessorTasks.ToArray());
            stopwatch.Stop();

            double threshold = jobConsumeTime.TotalMilliseconds * 1.5;
            _output.WriteLine($"Threshold : {threshold} ms -- Actual execution time : {stopwatch.ElapsedMilliseconds} ms");
            AssertThat.LessThan(threshold, stopwatch.ElapsedMilliseconds);
        }

        public class LongProcessRequest
        {
            public TimeSpan TimeConsumption { get; private set; }

            public LongProcessRequest(TimeSpan timeConsumption)
            {
                TimeConsumption = timeConsumption;
            }
        }

        public class LongProcessHandler : BaseMessageHandler<LongProcessRequest>
        {
            public override async Task HandleAsync(LongProcessRequest payload, CancellationToken cancellationToken = default)
            {
                await Task.Delay(payload.TimeConsumption, cancellationToken);
            }
        }

        public class DummyMessage
        {
            public Guid Guid { get; set; }
        }

        public class DummyMessageHandler : BaseMessageHandler<DummyMessage>
        {
            public override Task HandleAsync(DummyMessage payload, CancellationToken cancellationToken = default)
            {
                if (payload.Guid == default) throw new ArgumentException();
                return Task.CompletedTask;
            }
        }
    }
}