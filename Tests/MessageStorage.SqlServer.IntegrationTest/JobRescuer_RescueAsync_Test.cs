using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using MessageStorage.BackgroundTasks;
using MessageStorage.DataAccessLayer;
using MessageStorage.DataAccessLayer.Repositories;
using MessageStorage.MessageHandlers;
using MessageStorage.MessageHandlers.Options;
using MessageStorage.SqlServer.IntegrationTest.Fixtures;
using MessageStorage.SqlServer.Migrations;
using Microsoft.Data.SqlClient;
using Xunit;
using Xunit.Abstractions;

namespace MessageStorage.SqlServer.IntegrationTest
{
    [Collection(SqlServerInfraFixture.FIXTURE_KEY)]
    public class JobRescuer_RescueAsync_Test
    {
        private readonly JobRescuer _sut;

        private readonly IRepositoryFactory _repositoryFactory;
        private readonly ITestOutputHelper _output;

        private const string SCHEMA = "rescue_message_integration_test";
        private readonly MessageHandlerMetadata _messageHandlerMetadata;

        public JobRescuer_RescueAsync_Test(SqlServerInfraFixture sqlServerInfraInfraFixture, ITestOutputHelper output)
        {
            _output = output;
            var repositoryConfiguration = new RepositoryConfiguration(sqlServerInfraInfraFixture.ConnectionString, SCHEMA);
            _repositoryFactory = new SqlServerRepositoryFactory(repositoryConfiguration);

            var executor = new SqlServerMigrationExecutor(repositoryConfiguration);
            executor.Execute();

            _messageHandlerMetadata = new MessageHandlerMetadata(typeof(IMessageHandler<string>), new List<Type>() {typeof(string)});

            _sut = new JobRescuer(_repositoryFactory);
        }

        [Fact]
        public async Task When_ThereIsNoAnyJobToRescue_JobsStatusNotChange()
        {
            _messageHandlerMetadata.UseRescue(TimeSpan.FromMinutes(30));

            var jobList = new List<Job>
            {
                new Job(Guid.NewGuid(), new Message("payload"), _messageHandlerMetadata.MessageHandlerTypeName, JobStatus.Completed, DateTime.UtcNow, DateTime.UtcNow, "", 0, DateTime.UtcNow),
                new Job(Guid.NewGuid(), new Message("payload"), _messageHandlerMetadata.MessageHandlerTypeName, JobStatus.InProgress, DateTime.UtcNow, DateTime.UtcNow, "", 0, DateTime.UtcNow),
                new Job(Guid.NewGuid(), new Message("payload"), _messageHandlerMetadata.MessageHandlerTypeName, JobStatus.Queued, DateTime.UtcNow, DateTime.UtcNow, "", 0, DateTime.UtcNow),
                new Job(Guid.NewGuid(), new Message("payload"), _messageHandlerMetadata.MessageHandlerTypeName, JobStatus.Failed, DateTime.UtcNow, DateTime.UtcNow, "", 5, DateTime.UtcNow),
                new Job(Guid.NewGuid(), new Message("payload"), "message_handler_other", JobStatus.Failed, new DateTime(2020, 01, 01, 0, 0, 0), DateTime.UtcNow, "", 0, DateTime.UtcNow)
            };
            IJobRepository jobRepository = _repositoryFactory.CreateJobRepository();
            foreach (Job job in jobList)
            {
                await jobRepository.AddAsync(job);
            }

            await _sut.RescueAsync(_messageHandlerMetadata.RescueOption ?? throw new ArgumentNullException(nameof(_messageHandlerMetadata.RetryOption)));

            await using SqlConnection connection = new SqlConnection(_repositoryFactory.RepositoryConfiguration.ConnectionString);
            for (int i = 0; i < jobList.Count; i++)
            {
                var job = jobList[i];

                string script = $"SELECT JobStatus FROM {SCHEMA}.jobs WHERE id = @id";
                var jobStatus = await connection.QueryFirstAsync<int>(script,
                                                                      new
                                                                      {
                                                                          id = job.Id
                                                                      }
                                                                     );

                string message = $"Expected : {(int) job.JobStatus}{Environment.NewLine}" +
                                 $"Actual : {jobStatus}{Environment.NewLine}" +
                                 $"Job Index : {i}";

                _output.WriteLine(message);
                Assert.Equal((int) job.JobStatus, jobStatus);
            }
        }

        [Fact]
        public async Task When_ThereIsJobToRetry_JobStatusChange()
        {
            TimeSpan timeout = TimeSpan.FromMinutes(60);
            _messageHandlerMetadata.UseRescue(timeout);
            _messageHandlerMetadata.UseRetry(2);

            var lastOperationTime = new DateTime(2020, 06, 10, 20, 10, 15);

            var jobList = new List<Job>()
            {
                new Job(Guid.NewGuid(), new Message("payload"), _messageHandlerMetadata.MessageHandlerTypeName, JobStatus.InProgress, DateTime.UtcNow, lastOperationTime, "", 0, DateTime.UtcNow),
                new Job(Guid.NewGuid(), new Message("payload"), _messageHandlerMetadata.MessageHandlerTypeName, JobStatus.InProgress, DateTime.UtcNow, lastOperationTime, "", 5, DateTime.UtcNow),
            };

            IJobRepository jobRepository = _repositoryFactory.CreateJobRepository();
            foreach (Job job in jobList)
            {
                await jobRepository.AddAsync(job);
            }

            await _sut.RescueAsync(_messageHandlerMetadata.RescueOption);

            for (int i = 0; i < jobList.Count; i++)
            {
                Job job = jobList[i];

                await using SqlConnection connection = new SqlConnection(_repositoryFactory.RepositoryConfiguration.ConnectionString);
                string script = $"SELECT JobStatus, ExecuteLaterThan FROM {SCHEMA}.jobs WHERE id = @id";
                var jobStatus = await connection.QueryFirstAsync<int>(script,
                                                                      new
                                                                      {
                                                                          id = job.Id
                                                                      }
                                                                     );

                string message = $"Expected : {(int) JobStatus.Queued}{Environment.NewLine}" +
                                 $"Actual : {jobStatus}{Environment.NewLine}" +
                                 $"Job Index : {i}{Environment.NewLine}";
                _output.WriteLine(message);

                Assert.Equal((int) JobStatus.Queued, jobStatus);
            }
        }
    }
}