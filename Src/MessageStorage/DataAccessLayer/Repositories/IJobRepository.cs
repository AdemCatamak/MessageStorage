using System;
using System.Threading;
using System.Threading.Tasks;

namespace MessageStorage.DataAccessLayer.Repositories
{
    public interface IJobRepository
    {
        Task AddAsync(Job job, CancellationToken cancellationToken = default);
        Task UpdateJobStatusAsync(Job job, CancellationToken cancellationToken = default);
        Task<Job?> SetFirstQueuedJobToInProgressAsync(CancellationToken cancellationToken = default);
        Task SetInQueued(string messageHandlerTypeName, int maxExecutionAttemptCount, TimeSpan deferTime, CancellationToken cancellationToken = default);
        Task SetInQueued(string messageHandlerTypeName, TimeSpan maxExecutionTime, CancellationToken cancellationToken = default);
        Task<int> GetJobCountAsync(JobStatus jobStatus, CancellationToken cancellationToken = default);
    }
}