using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using MessageStorage.BackgroundTasks;
using MessageStorage.BackgroundTasks.Options;
using MessageStorage.DataAccessLayer;
using MessageStorage.DataAccessLayer.Repositories;
using MessageStorage.MessageHandlers;
using MessageStorage.MySql.IntegrationTest.Fixtures;
using MessageStorage.MySql.Migrations;
using MySqlConnector;
using Xunit;
using Xunit.Abstractions;

namespace MessageStorage.MySql.IntegrationTest
{
    public class JobRescuer_RescueAsync_Test : IClassFixture<MySqlInfraFixture>
    {
        private readonly JobRescuer _sut;

        private readonly IRepositoryFactory _repositoryFactory;
        private readonly ITestOutputHelper _output;
        private readonly MySqlInfraFixture _mySqlInfraFixture;
        private readonly MessageHandlerMetadata _messageHandlerMetadata;
        
        private string Schema => _mySqlInfraFixture.Database;

        public JobRescuer_RescueAsync_Test(MySqlInfraFixture mySqlInfraFixture, ITestOutputHelper output)
        {
            _mySqlInfraFixture = mySqlInfraFixture;
            _output = output;
            var repositoryConfiguration = new RepositoryConfiguration(mySqlInfraFixture.ConnectionString, Schema);
            _repositoryFactory = new MySqlRepositoryFactory(repositoryConfiguration);

            var executor = new MySqlMigrationExecutor(repositoryConfiguration);
            executor.Execute();

            _messageHandlerMetadata = new MessageHandlerMetadata(typeof(IMessageHandler<string>), new List<Type> { typeof(string) });

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

            await using var connection = new MySqlConnection(_repositoryFactory.RepositoryConfiguration.ConnectionString);
            for (int i = 0; i < jobList.Count; i++)
            {
                var job = jobList[i];
                string script = $"SELECT job_status FROM {Schema}.jobs WHERE id = @id";
                var jobStatus = await connection.QueryFirstAsync<int>(script,
                                                                      new
                                                                      {
                                                                          id = job.Id
                                                                      }
                                                                     );
                string message = $"Expected : {(int)job.JobStatus}{Environment.NewLine}" +
                                 $"Actual : {jobStatus}{Environment.NewLine}" +
                                 $"Job Index : {i}";

                _output.WriteLine(message);
                Assert.Equal((int)job.JobStatus, jobStatus);
            }
        }

        [Fact]
        public async Task When_ThereIsJobToRescue_JobStatusChange()
        {
            TimeSpan timeout = TimeSpan.FromMinutes(60);
            _messageHandlerMetadata.UseRescue(timeout);
            _messageHandlerMetadata.UseRetry(2);

            var lastOperationTime = new DateTime(2020, 06, 10, 20, 10, 15);

            var jobList = new List<Job>
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

                await using var connection = new MySqlConnection(_repositoryFactory.RepositoryConfiguration.ConnectionString);
                string script = $"SELECT job_status FROM {Schema}.jobs WHERE id = @id";
                var jobStatus = await connection.QueryFirstAsync<int>(script,
                                                                      new
                                                                      {
                                                                          id = job.Id
                                                                      }
                                                                     );

                string message = $"Expected : {(int)JobStatus.Queued}{Environment.NewLine}" +
                                 $"Actual : {jobStatus}{Environment.NewLine}" +
                                 $"Job Index : {i}{Environment.NewLine}";
                _output.WriteLine(message);

                Assert.Equal((int)JobStatus.Queued, jobStatus);
            }
        }
    }
}