using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using MessageStorage.DataAccessLayer;
using MessageStorage.MySql.IntegrationTest.Fixtures;
using MessageStorage.MySql.Migrations;
using MySqlConnector;
using Xunit;

namespace MessageStorage.MySql.IntegrationTest
{
    public class MonitorClient_GetJobCountAsync_Test : IClassFixture<MonitorClient_GetJobCountAsync_Test.SeedDataFixture>,
                                                       IClassFixture<MySqlInfraFixture>
    {
        private readonly IMonitorClient _sut;
        private readonly MySqlInfraFixture _mySqlInfraFixture;

        private string Schema => _mySqlInfraFixture.Database;

        public MonitorClient_GetJobCountAsync_Test(MySqlInfraFixture mySqlInfraFixture, SeedDataFixture seedDataFixture)
        {
            _mySqlInfraFixture = mySqlInfraFixture;

            var repositoryConfiguration = new RepositoryConfiguration(mySqlInfraFixture.ConnectionString, Schema);
            var postgresRepositoryFactory = new MySqlRepositoryFactory(repositoryConfiguration);

            var executor = new MySqlMigrationExecutor(repositoryConfiguration);
            executor.Execute();

            _sut = new MonitorClient(postgresRepositoryFactory);

            seedDataFixture.Initialize(repositoryConfiguration);
        }

        [Theory]
        [InlineData(JobStatus.Queued, 4)]
        [InlineData(JobStatus.InProgress, 2)]
        [InlineData(JobStatus.Completed, 3)]
        [InlineData(JobStatus.Failed, 0)]
        public async Task When_MonitorClientSendRequest__ResponseShouldBeEquivalentNumberOfJobInStatus(JobStatus desiredJobStatus, int expectedJobCount)
        {
            int result = await _sut.GetJobCountAsync(desiredJobStatus);
            Assert.Equal(expectedJobCount, result);
        }

        public class SeedDataFixture
        {
            private bool _initialized;

            public readonly IReadOnlyCollection<Job> Jobs = new List<Job>
                                                            {
                                                                new Job(Guid.NewGuid(), new Message("dummy-payload"), "dummy-handler", JobStatus.Queued, DateTime.UtcNow, DateTime.UtcNow, null, 5, DateTime.UtcNow),
                                                                new Job(Guid.NewGuid(), new Message("dummy-payload"), "dummy-handler", JobStatus.Queued, DateTime.UtcNow, DateTime.UtcNow, null, 5, DateTime.UtcNow),
                                                                new Job(Guid.NewGuid(), new Message("dummy-payload"), "dummy-handler", JobStatus.Queued, DateTime.UtcNow, DateTime.UtcNow, null, 5, DateTime.UtcNow),
                                                                new Job(Guid.NewGuid(), new Message("dummy-payload"), "dummy-handler", JobStatus.Queued, DateTime.UtcNow, DateTime.UtcNow, null, 5, DateTime.UtcNow),
                                                                new Job(Guid.NewGuid(), new Message("dummy-payload"), "dummy-handler", JobStatus.InProgress, DateTime.UtcNow, DateTime.UtcNow, null, 5, DateTime.UtcNow),
                                                                new Job(Guid.NewGuid(), new Message("dummy-payload"), "dummy-handler", JobStatus.InProgress, DateTime.UtcNow, DateTime.UtcNow, null, 5, DateTime.UtcNow),
                                                                new Job(Guid.NewGuid(), new Message("dummy-payload"), "dummy-handler", JobStatus.Completed, DateTime.UtcNow, DateTime.UtcNow, null, 5, DateTime.UtcNow),
                                                                new Job(Guid.NewGuid(), new Message("dummy-payload"), "dummy-handler", JobStatus.Completed, DateTime.UtcNow, DateTime.UtcNow, null, 5, DateTime.UtcNow),
                                                                new Job(Guid.NewGuid(), new Message("dummy-payload"), "dummy-handler", JobStatus.Completed, DateTime.UtcNow, DateTime.UtcNow, null, 5, DateTime.UtcNow),
                                                            };

            public void Initialize(RepositoryConfiguration repositoryConfiguration)
            {
                if (_initialized) return;

                foreach (Job job in Jobs)
                {
                    string script =
                        $"insert into {repositoryConfiguration.Schema}.jobs (id, created_on, message_id, message_handler_type_name, job_status, last_operation_time, last_operation_info) VALUES (@id, @created_on, @message_id, @message_handler_type_name, @job_status, @last_operation_time, @last_operation_info)";
                    var parameters = new
                                     {
                                         id = job.Id,
                                         created_on = job.CreatedOn,
                                         message_id = job.Message.Id,
                                         message_handler_type_name = job.MessageHandlerTypeName,
                                         job_status = job.JobStatus,
                                         last_operation_time = job.LastOperationTime,
                                         last_operation_info = job.LastOperationInfo
                                     };
                    using var connection = new MySqlConnection(repositoryConfiguration.ConnectionString);
                    connection.Execute(script, parameters);
                }

                _initialized = true;
            }
        }
    }
}