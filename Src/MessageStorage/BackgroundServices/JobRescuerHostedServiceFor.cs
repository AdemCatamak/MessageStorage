using System;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.Exceptions;
using MessageStorage.Processor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MessageStorage.BackgroundServices;

internal class JobRescuerHostedServiceFor<TMessageClient> : IHostedService,
                                                            IDisposable
    where TMessageClient : IMessageStorageClient
{
    private readonly IServiceProvider _serviceProvider;
    private readonly JobRescuerHostedServiceOptionFor<TMessageClient> _options;
    private readonly ILogger<JobRescuerHostedServiceFor<TMessageClient>> _logger;

    private Timer? _timer;

    public JobRescuerHostedServiceFor(IServiceProvider serviceProvider,
                                      JobRescuerHostedServiceOptionFor<TMessageClient> options,
                                      ILogger<JobRescuerHostedServiceFor<TMessageClient>> logger)
    {
        _serviceProvider = serviceProvider;
        _options = options;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _timer = new Timer(RescueJobs, null, TimeSpan.FromMinutes(2), _options.Interval);
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
            using IServiceScope? scope = _serviceProvider.CreateScope();
            IServiceProvider? serviceProvider = scope.ServiceProvider;
            var jobRescuer = serviceProvider.GetRequiredService<IJobRescuerFor<TMessageClient>>();
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            jobRescuer.RescueAsync(cancellationTokenSource.Token).GetAwaiter().GetResult();
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