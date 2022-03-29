using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer;

namespace MessageStorage.Processor;

public interface IJobRetrierFor<TMessageStorageClient>
    where TMessageStorageClient : IMessageStorageClient
{
    Task RetryAsync();
}

internal class JobRetrierFor<TMessageStorageClient> : IJobRetrierFor<TMessageStorageClient>
    where TMessageStorageClient : IMessageStorageClient
{
    private readonly IRepositoryContextFor<TMessageStorageClient> _repositoryContext;
    private readonly IJobExecutorFor<TMessageStorageClient> _jobExecutor;
    private readonly JobRetrierOptionsFor<TMessageStorageClient> _options;

    public JobRetrierFor(IRepositoryContextFor<TMessageStorageClient> repositoryContext, IJobExecutorFor<TMessageStorageClient> jobExecutor, JobRetrierOptionsFor<TMessageStorageClient> options)
    {
        _repositoryContext = repositoryContext;
        _jobExecutor = jobExecutor;
        _options = options;
    }

    public async Task RetryAsync()
    {
        DateTime lastOperationTimeBeforeThen = DateTime.UtcNow.AddSeconds(-30);

        var isUnderFetch = true;
        do
        {
            if (!isUnderFetch)
            {
                await Task.Delay(_options.WaitAfterFullFetch);
            }

            using var cancellationTokenSourceForSetInProgress = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            List<Job>? jobList = await _repositoryContext.GetJobRepository()
                                                         .SetInProgressAsync(lastOperationTimeBeforeThen, _options.FetchCount, cancellationTokenSourceForSetInProgress.Token);

            isUnderFetch = jobList.Count < _options.FetchCount;
            Parallel.ForEach(jobList,
                             new ParallelOptions
                             {
                                 MaxDegreeOfParallelism = _options.Concurrency,
                             },
                             job => { _jobExecutor.ExecuteAsync(job).GetAwaiter().GetResult(); });
        } while (!isUnderFetch);
    }
}