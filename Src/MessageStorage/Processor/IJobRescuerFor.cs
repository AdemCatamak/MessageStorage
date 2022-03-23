using System;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer;

namespace MessageStorage.Processor;

public interface IJobRescuerFor<TMessageStorageClient>
    where TMessageStorageClient : IMessageStorageClient
{
    Task RescueAsync(CancellationToken cancellationToken);
}

internal class JobRescuerFor<TMessageStorageClient> : IJobRescuerFor<TMessageStorageClient>
    where TMessageStorageClient : IMessageStorageClient
{
    private readonly IRepositoryContextFor<TMessageStorageClient> _repositoryContext;
    private readonly JobExecutorOptionsFor<TMessageStorageClient> _jobExecutorOption;

    public JobRescuerFor(IRepositoryContextFor<TMessageStorageClient> repositoryContext, JobExecutorOptionsFor<TMessageStorageClient> jobExecutorOption)
    {
        _repositoryContext = repositoryContext;
        _jobExecutorOption = jobExecutorOption;
    }

    public async Task RescueAsync(CancellationToken cancellationToken)
    {
        double thresholdMilliseconds = _jobExecutorOption.JobExecutionMaxDuration.TotalMilliseconds * 1.1;
        TimeSpan goBack = TimeSpan.FromMilliseconds(thresholdMilliseconds).Negate();

        DateTime lastOperationTimeBeforeThen = DateTime.UtcNow.Add(goBack);

        IJobRepository jobRepository = _repositoryContext.GetJobRepository();
        await jobRepository.RescueJobsAsync(lastOperationTimeBeforeThen, cancellationToken);
    }
}