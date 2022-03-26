using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using MessageStorage.Exceptions;
using MessageStorage.Processor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace MessageStorage.BackgroundServices;

internal class JobRetrierHostedServiceFor<TMessageStorageClient> : IHostedService,
                                                                   IDisposable
    where TMessageStorageClient : IMessageStorageClient
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<JobRetrierHostedServiceFor<TMessageStorageClient>> _logger;

    private readonly Timer _timer;

    public JobRetrierHostedServiceFor(IServiceProvider serviceProvider,
                                      JobRetrierHostedServiceOptionFor<TMessageStorageClient> options,
                                      ILogger<JobRetrierHostedServiceFor<TMessageStorageClient>> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        _timer = new Timer(options.Interval.TotalMilliseconds);
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _timer.AutoReset = false;
        _timer.Elapsed += TimerOnElapsed;
        _timer.Enabled = true;
        _timer.Start();

        return Task.CompletedTask;
    }

    private void TimerOnElapsed(object sender, ElapsedEventArgs e)
    {
        _logger.LogInformation("{ServiceName} is started", nameof(JobRetrierHostedServiceFor<TMessageStorageClient>));
        try
        {
            using IServiceScope? scope = _serviceProvider.CreateScope();
            IServiceProvider? serviceProvider = scope.ServiceProvider;
            var jobRetrier = serviceProvider.GetRequiredService<IJobRetrierFor<TMessageStorageClient>>();
            jobRetrier.RetryAsync().GetAwaiter().GetResult();

            _logger.LogInformation("{ServiceName} is completed", nameof(JobRetrierHostedServiceFor<TMessageStorageClient>));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ServiceName} is failed", nameof(JobRetrierHostedServiceFor<TMessageStorageClient>));
        }
        finally
        {
            _timer.Enabled = true;
        }
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _timer.Stop();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer.Dispose();
    }
}

internal record JobRetrierHostedServiceOptionFor<TMessageStorageClient>
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
                throw new ParameterException($"{nameof(JobRetrierHostedServiceOptionFor<TMessageStorageClient>)}.{nameof(Interval)}");
            }

            _interval = value;
        }
    }
}