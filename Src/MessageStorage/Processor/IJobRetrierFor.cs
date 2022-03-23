using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer;
using Microsoft.Extensions.DependencyInjection;

namespace MessageStorage.Processor;

public interface IJobRetrierFor<TMessageStorageClient>
    where TMessageStorageClient : IMessageStorageClient
{
    Task RetryAsync();
}

internal class JobRetrierFor<TMessageStorageClient> : IJobRetrierFor<TMessageStorageClient>
    where TMessageStorageClient : IMessageStorageClient
{
    private readonly IServiceProvider _serviceProvider;

    public JobRetrierFor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task RetryAsync()
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        IServiceProvider serviceProvider = scope.ServiceProvider;
        var repositoryContext = serviceProvider.GetRequiredService<IRepositoryContextFor<TMessageStorageClient>>();
        var jobExecutorFor = serviceProvider.GetRequiredService<IJobExecutorFor<TMessageStorageClient>>();
        var jobRetrierOptions = serviceProvider.GetRequiredService<JobRetrierOptionsFor<TMessageStorageClient>>();

        DateTime lastOperationTimeBeforeThen = DateTime.UtcNow.AddSeconds(-30);

        bool isUnderFetch;
        do
        {
            using var cancellationTokenSourceForSetInProgress = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            List<Job> jobList = await repositoryContext.GetJobRepository()
                                                       .SetInProgressAsync(lastOperationTimeBeforeThen, jobRetrierOptions.FetchCount, cancellationTokenSourceForSetInProgress.Token);

            isUnderFetch = jobList.Count < jobRetrierOptions.FetchCount;
            Parallel.ForEach(jobList,
                             new ParallelOptions
                             {
                                 MaxDegreeOfParallelism = jobRetrierOptions.Concurrency,
                             },
                             job => { jobExecutorFor.ExecuteAsync(job).GetAwaiter().GetResult(); });

            if (!isUnderFetch)
            {
                await Task.Delay(jobRetrierOptions.WaitAfterFullFetch);
            }
        } while (!isUnderFetch);
    }
}