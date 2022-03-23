using System;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer;
using Microsoft.Extensions.DependencyInjection;

namespace MessageStorage.Processor;

public interface IJobRescuerFor<TMessageStorageClient>
    where TMessageStorageClient : IMessageStorageClient
{
    Task RescueAsync();
}

internal class JobRescuerFor<TMessageStorageClient> : IJobRescuerFor<TMessageStorageClient>
    where TMessageStorageClient : IMessageStorageClient
{
    private readonly IServiceProvider _serviceProvider;

    public JobRescuerFor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task RescueAsync()
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        IServiceProvider serviceProvider = scope.ServiceProvider;
        var repositoryContext = serviceProvider.GetRequiredService<IRepositoryContextFor<TMessageStorageClient>>();
        var jobExecutorOption = serviceProvider.GetRequiredService<JobExecutorOptionsFor<TMessageStorageClient>>();

        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await RescueAsync(repositoryContext, jobExecutorOption, cancellationTokenSource.Token);
    }

    private async Task RescueAsync(IRepositoryContextFor<TMessageStorageClient> repositoryContext,
                                   JobExecutorOptionsFor<TMessageStorageClient> jobExecutorOption,
                                   CancellationToken cancellationToken)
    {
        double thresholdMilliseconds = jobExecutorOption.JobExecutionMaxDuration.TotalMilliseconds * 1.1;
        TimeSpan goBack = TimeSpan.FromMilliseconds(thresholdMilliseconds).Negate();

        DateTime lastOperationTimeBeforeThen = DateTime.UtcNow.Add(goBack);

        await repositoryContext.GetJobRepository()
                               .RescueJobsAsync(lastOperationTimeBeforeThen, cancellationToken);
    }
}