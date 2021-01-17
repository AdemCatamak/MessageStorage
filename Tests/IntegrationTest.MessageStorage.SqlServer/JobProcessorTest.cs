using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.dotMemoryUnit;
using MessageStorage;
using MessageStorage.Clients;
using MessageStorage.Clients.Imp;
using MessageStorage.Configurations;
using MessageStorage.DataAccessSection;
using MessageStorage.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTest.MessageStorage.SqlServer
{
    public class JobProcessorTest : IClassFixture<SqlServerTestFixture>,
                                    IDisposable
    {
        private readonly SqlServerTestFixture _sqlServerTestFixture;
        private readonly IMessageStorageClient _messageStorageClient;
        private readonly IBackgroundProcessor _backgroundProcessor;

        private readonly JobProcessorConfiguration _jobProcessorConfiguration = new JobProcessorConfiguration()
                                                                                {
                                                                                    WaitWhenJobNotFound = TimeSpan.FromSeconds(1),
                                                                                    WaitAfterJobHandled = TimeSpan.Zero
                                                                                };

        public JobProcessorTest(SqlServerTestFixture sqlServerTestFixture,ITestOutputHelper outputHelper)
        {
            DotMemoryUnitTestOutput.SetOutputMethod(outputHelper.WriteLine);

            _sqlServerTestFixture = sqlServerTestFixture;

            IMessageStorageRepositoryContext repositoryContext = _sqlServerTestFixture.CreateMessageStorageSqlServerRepositoryContext();
            HandlerDescription dummyHandlerDescription = new HandlerDescription<DummyHandler>(() => new DummyHandler());
            HandlerDescription dummyObjectHandlerDescription = new HandlerDescription<DummyObjectHandler>(() => new DummyObjectHandler());
            IHandlerManager handlerManager = new HandlerManager(new[] {dummyHandlerDescription, dummyObjectHandlerDescription});
            _messageStorageClient = new MessageStorageClient(repositoryContext, handlerManager);
            _backgroundProcessor = new JobProcessor(() => _sqlServerTestFixture.CreateMessageStorageSqlServerRepositoryContext(),
                                                    handlerManager,
                                                    NullLogger<JobProcessor>.Instance,
                                                    _jobProcessorConfiguration);

            _backgroundProcessor.StartAsync().GetAwaiter().GetResult();
        }

        [Fact]
        public async Task WhenJobProcessorStarted_BasicType__HandlerShouldGetHit()
        {
            const int myMessage = 42;
            var (message, jobs) = _messageStorageClient.Add(myMessage);

            TimeSpan wait = TimeSpan.FromMilliseconds(_jobProcessorConfiguration.WaitWhenJobNotFound.TotalMilliseconds * 1.5);
            await Task.Delay(wait);

            IEnumerable<Job> jobList = jobs.ToList();
            Assert.Single(jobList);

            IMessageStorageRepositoryContext repositoryContext = _sqlServerTestFixture.CreateMessageStorageSqlServerRepositoryContext();
            Job? job = repositoryContext.GetJobRepository().GetJob(jobList.First().Id);

            Assert.NotNull(job);
            Assert.Equal(jobList.First().Id, job!.Id);
            Assert.Equal(message.Id, job.Message.Id);
            Assert.Equal(JobStatus.Done, job.JobStatus);
            Assert.Equal(myMessage, DummyHandler.Value);
        }

        [Fact]
        public async Task WhenJobProcessorStarted_ComplexType__HandlerShouldGetHit()
        {
            DummyObject dummyObject = new DummyObject {Value = 42};
            var (message, jobs) = _messageStorageClient.Add(dummyObject);

            TimeSpan wait = TimeSpan.FromMilliseconds(_jobProcessorConfiguration.WaitWhenJobNotFound.TotalMilliseconds * 1.5);
            await Task.Delay(wait);

            IEnumerable<Job> jobList = jobs.ToList();
            Assert.Single(jobList);

            IMessageStorageRepositoryContext repositoryContext = _sqlServerTestFixture.CreateMessageStorageSqlServerRepositoryContext();
            Job? job = repositoryContext.GetJobRepository().GetJob(jobList.First().Id);

            Assert.NotNull(job);
            Assert.Equal(jobList.First().Id, job!.Id);
            Assert.Equal(message.Id, job.Message.Id);
            Assert.Equal(JobStatus.Done, job.JobStatus);
            Assert.Equal(dummyObject.Value, DummyObjectHandler.DummyObject?.Value);
        }


        public class DummyHandler : Handler<int>
        {
            public static int Value;

            protected override Task HandleAsync(int payload, CancellationToken cancellationToken)
            {
                Value = payload;
                return Task.CompletedTask;
            }
        }

        public class DummyObjectHandler : Handler<DummyObject>
        {
            public static DummyObject? DummyObject;

            protected override Task HandleAsync(DummyObject payload, CancellationToken cancellationToken)
            {
                DummyObject = payload;
                return Task.CompletedTask;
            }
        }

        public class DummyObject
        {
            public int Value;
        }

        public void Dispose()
        {
            _backgroundProcessor.StopAsync().GetAwaiter().GetResult();
            _backgroundProcessor.Dispose();
        }
    }
}