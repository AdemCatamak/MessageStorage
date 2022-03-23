using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MessageStorage.DataAccessLayer;

public interface IJobRepository
{
    Task AddAsync(List<Job> jobs, CancellationToken cancellationToken);
    Task UpdateStatusAsync(Job job, CancellationToken cancellationToken);
    Task RescueJobsAsync(DateTime lastOperationTimeBeforeThen, CancellationToken cancellationToken);
    Task<List<Job>> SetInProgressAsync(DateTime lastOperationTimeBeforeThen, int fetchCount, CancellationToken cancellationToken);
    Task CleanAsync(DateTime lastOperationTimeBeforeThen, bool removeOnlySucceeded, CancellationToken cancellationToken);
    
    Task<int> GetJobCountAsync(JobStatus jobStatus, CancellationToken cancellationToken);
}