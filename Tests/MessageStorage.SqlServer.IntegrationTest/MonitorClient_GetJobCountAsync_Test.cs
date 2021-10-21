using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using MessageStorage.DataAccessLayer;
using MessageStorage.SqlServer.IntegrationTest.Fixtures;
using MessageStorage.SqlServer.Migrations;
using Microsoft.Data.SqlClient;
using Xunit;

namespace MessageStorage.SqlServer.IntegrationTest
{
    [Collection(SqlServerInfraFixture.FIXTURE_KEY)]
    public class MonitorClient_GetJobCountAsync_Test : IClassFixture<MonitorClient_GetJobCountAsync_Test.SeedDataFixture>
    {
        private readonly IMonitorClient _sut;
        private readonly SqlServerInfraFixture _sqlServerInfraFixture;
        private const string SCHEMA = "monitor_client_getjobcountasync_test";

        public MonitorClient_GetJobCountAsync_Test(SqlServerInfraFixture sqlServerInfraFixture, SeedDataFixture seedDataFixture)
        {
            _sqlServerInfraFixture = sqlServerInfraFixture;

            var repositoryConfiguration = new RepositoryConfiguration(sqlServerInfraFixture.ConnectionString, SCHEMA);
            var sqlServerRepositoryFactory = new SqlServerRepositoryFactory(repositoryConfiguration);

            var executor = new SqlServerMigrationExecutor(repositoryConfiguration);
            executor.Execute();

            _sut = new MonitorClient(sqlServerRepositoryFactory);

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
                        $"INSERT INTO [{repositoryConfiguration.Schema}].[Jobs] (Id, CreatedOn, MessageId, MessageHandlerTypeName, JobStatus, LastOperationTime, LastOperationInfo) VALUES (@Id, @CreatedOn, @MessageId, @MessageHandlerTypeName, @JobStatus, @LastOperationTime, @LastOperationInfo)";
                    var parameters = new
                    {
                        Id = job.Id,
                        CreatedOn = job.CreatedOn,
                        MessageId = job.Message.Id,
                        MessageHandlerTypeName = job.MessageHandlerTypeName,
                        JobStatus = job.JobStatus,
                        LastOperationTime = job.LastOperationTime,
                        LastOperationInfo = job.LastOperationInfo
                    };
                    using var connection = new SqlConnection(repositoryConfiguration.ConnectionString);
                    connection.Execute(script, parameters);
                }

                _initialized = true;
            }
        }
    }
}