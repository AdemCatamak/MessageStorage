using System.Threading;
using System.Threading.Tasks;
using MessageStorage.Processor;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MessageStorage.BackgroundServices;

internal class JobQueueConsumeTriggerHostedServiceFor<TMessageStorageClient> : BackgroundService
    where TMessageStorageClient : IMessageStorageClient
{
    private readonly IJobQueueFor<TMessageStorageClient> _jobQueue;
    private readonly ILogger<JobQueueConsumeTriggerHostedServiceFor<TMessageStorageClient>> _logger;

    public JobQueueConsumeTriggerHostedServiceFor(IJobQueueFor<TMessageStorageClient> jobQueue, ILogger<JobQueueConsumeTriggerHostedServiceFor<TMessageStorageClient>> logger)
    {
        _jobQueue = jobQueue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("{ServiceName} is started", nameof(JobQueueConsumeTriggerHostedServiceFor<TMessageStorageClient>));
        await _jobQueue.StartDequeue(stoppingToken);
        _logger.LogInformation("{ServiceName} is stopped", nameof(JobQueueConsumeTriggerHostedServiceFor<TMessageStorageClient>));
    }
}