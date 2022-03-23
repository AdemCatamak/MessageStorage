using System;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.Exceptions;
using MessageStorage.Processor;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MessageStorage.BackgroundServices;

internal class JobRescuerHostedServiceFor<TMessageClient> : IHostedService,
                                                            IDisposable
    where TMessageClient : IMessageStorageClient
{
    private readonly JobRescuerHostedServiceOptionFor<TMessageClient> _options;
    private readonly IJobRescuerFor<TMessageClient> _jobRescuer;
    private readonly ILogger<JobRescuerHostedServiceFor<TMessageClient>> _logger;

    private Timer? _timer;

    public JobRescuerHostedServiceFor(JobRescuerHostedServiceOptionFor<TMessageClient> options,
                                      IJobRescuerFor<TMessageClient> jobRescuer,
                                      ILogger<JobRescuerHostedServiceFor<TMessageClient>> logger)
    {
        _options = options;
        _jobRescuer = jobRescuer;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _timer = new Timer(RescueJobs, null, TimeSpan.Zero, _options.Interval);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    private void RescueJobs(object? state)
    {
        _logger.LogInformation("{ServiceName} is started", nameof(JobRescuerHostedServiceFor<TMessageClient>));
        try
        {
            _jobRescuer.RescueAsync().GetAwaiter().GetResult();
            _logger.LogInformation("{ServiceName} is completed", nameof(JobRescuerHostedServiceFor<TMessageClient>));
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "{ServiceName} is failed", nameof(JobRescuerHostedServiceFor<TMessageClient>));
        }
    }
}

internal record JobRescuerHostedServiceOptionFor<TMessageStorageClient>
    where TMessageStorageClient : IMessageStorageClient
{
    private TimeSpan _interval;

    public TimeSpan Interval
    {
        get => _interval;
        set
        {
            if (value.CompareTo(TimeSpan.Zero) < 0)
            {
                throw new ParameterException($"{nameof(JobRescuerHostedServiceOptionFor<TMessageStorageClient>)}.{nameof(Interval)}");
            }

            _interval = value;
        }
    }
}