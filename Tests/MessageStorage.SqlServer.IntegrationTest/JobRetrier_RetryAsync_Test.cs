using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using MessageStorage.BackgroundTasks;
using MessageStorage.BackgroundTasks.Options;
using MessageStorage.DataAccessLayer;
using MessageStorage.DataAccessLayer.Repositories;
using MessageStorage.MessageHandlers;
using MessageStorage.SqlServer.IntegrationTest.Fixtures;
using MessageStorage.SqlServer.Migrations;
using Microsoft.Data.SqlClient;
using Xunit;
using Xunit.Abstractions;

namespace MessageStorage.SqlServer.IntegrationTest
{
    [Collection(SqlServerInfraFixture.FIXTURE_KEY)]
    public class JobRetrier_RetryAsync_Test
    {
        private readonly JobRetrier _sut;

        private readonly IRepositoryFactory _repositoryFactory;
        private readonly ITestOutputHelper _output;

        private const string SCHEMA = "retry_message_integration_test";
        private readonly MessageHandlerMetadata _messageHandlerMetadata;

        public JobRetrier_RetryAsync_Test(SqlServerInfraFixture sqlServerInfraInfraFixture, ITestOutputHelper output)
        {
            _output = output;
            var repositoryConfiguration = new RepositoryConfiguration(sqlServerInfraInfraFixture.ConnectionString, SCHEMA);
            _repositoryFactory = new SqlServerRepositoryFactory(repositoryConfiguration);

            var executor = new SqlServerMigrationExecutor(repositoryConfiguration);
            executor.Execute();

            _messageHandlerMetadata = new MessageHandlerMetadata(typeof(IMessageHandler<string>), new List<Type> {typeof(string)});

            _sut = new JobRetrier(_repositoryFactory);
        }

        [Fact]
        public async Task When_ThereIsNoAnyJobToRetry_JobsStatusNotChange()
        {
            _messageHandlerMetadata.UseRetry(3);

            var jobList = new List<Job>
            {
                new Job(Guid.NewGuid(), new Message("payload"), "handler", JobStatus.Completed, DateTime.UtcNow, DateTime.UtcNow, "", 0, DateTime.UtcNow),
                new Job(Guid.NewGuid(), new Message("payload"), "handler", JobStatus.InProgress, DateTime.UtcNow, DateTime.UtcNow, "", 0, DateTime.UtcNow),
                new Job(Guid.NewGuid(), new Message("payload"), "handler", JobStatus.Queued, DateTime.UtcNow, DateTime.UtcNow, "", 0, DateTime.UtcNow),
                // Retry count exceed
                new Job(Guid.NewGuid(), new Message("payload"), _messageHandlerMetadata.MessageHandlerTypeName, JobStatus.Failed, DateTime.UtcNow, DateTime.UtcNow, "", 5, DateTime.UtcNow),
                // There is no rescue option for handler-type
                new Job(Guid.NewGuid(), new Message("payload"), "message_handler_other", JobStatus.Failed, DateTime.UtcNow, DateTime.UtcNow, "", 0, DateTime.UtcNow)
            };
            IJobRepository jobRepository = _repositoryFactory.CreateJobRepository();
            foreach (Job job in jobList)
            {
                await jobRepository.AddAsync(job);
            }

            await _sut.RetryAsync(_messageHandlerMetadata.RetryOption ?? throw new ArgumentNullException(nameof(_messageHandlerMetadata.RetryOption)));

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
            TimeSpan deferTime = TimeSpan.FromMinutes(1);
            _messageHandlerMetadata.UseRetry(3, deferTime);
            var lastOperationTime = new DateTime(2021, 06, 10, 20, 10, 15);

            var job = new Job(Guid.NewGuid(), new Message("payload"), _messageHandlerMetadata.MessageHandlerTypeName, JobStatus.Failed, DateTime.UtcNow, lastOperationTime, "", 0, DateTime.UtcNow);
            IJobRepository jobRepository = _repositoryFactory.CreateJobRepository();
            await jobRepository.AddAsync(job);

            await _sut.RetryAsync(_messageHandlerMetadata.RetryOption ?? throw new ArgumentNullException(nameof(_messageHandlerMetadata.RetryOption)));

            await using SqlConnection connection = new SqlConnection(_repositoryFactory.RepositoryConfiguration.ConnectionString);
            string script = $"SELECT JobStatus, ExecuteLaterThan FROM {SCHEMA}.jobs WHERE id = @id";
            var tuple = await connection.QueryFirstAsync<(int, DateTime)>(script,
                                                                          new
                                                                          {
                                                                              id = job.Id
                                                                          }
                                                                         );

            Assert.Equal((int) JobStatus.Queued, tuple.Item1);
            Assert.Equal(lastOperationTime.Add(deferTime), tuple.Item2, TimeSpan.FromSeconds(5));
        }
    }
}