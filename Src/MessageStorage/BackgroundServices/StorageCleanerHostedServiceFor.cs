using System;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.Exceptions;
using MessageStorage.Processor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MessageStorage.BackgroundServices;

internal class StorageCleanerHostedServiceFor<TMessageStorageClient> : IHostedService,
                                                                       IDisposable
    where TMessageStorageClient : IMessageStorageClient
{
    private readonly IServiceProvider _serviceProvider;
    private readonly StorageCleanerHostedServiceOptionsFor<TMessageStorageClient> _options;
    private readonly ILogger<StorageCleanerHostedServiceFor<TMessageStorageClient>> _logger;

    private Timer? _timer;

    public StorageCleanerHostedServiceFor(IServiceProvider serviceProvider, StorageCleanerHostedServiceOptionsFor<TMessageStorageClient> options, ILogger<StorageCleanerHostedServiceFor<TMessageStorageClient>> logger)
    {
        _serviceProvider = serviceProvider;
        _options = options;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(TryClean, null, TimeSpan.FromMinutes(5), _options.Interval);
        return Task.CompletedTask;
    }

    private void TryClean(object state)
    {
        _logger.LogInformation("{ServiceName} is started", nameof(StorageCleanerHostedServiceFor<TMessageStorageClient>));
        try
        {
            CleanAsync().GetAwaiter().GetResult();
            _logger.LogInformation("{ServiceName} is completed", nameof(StorageCleanerHostedServiceFor<TMessageStorageClient>));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ServiceName} is failed", nameof(StorageCleanerHostedServiceFor<TMessageStorageClient>));
        }
    }

    private async Task CleanAsync()
    {
        if (!_options.FinalizedEntityRemovedAfter.HasValue)
        {
            return;
        }

        IServiceScope scope = _serviceProvider.CreateScope();
        IServiceProvider serviceProvider = scope.ServiceProvider;
        var storageCleaner = serviceProvider.GetRequiredService<IStorageCleanerFor<TMessageStorageClient>>();

        DateTime timeThreshold = DateTime.UtcNow.Add(_options.FinalizedEntityRemovedAfter.Value.Negate());

        using (var cleanJobsCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60)))
        {
            await storageCleaner.CleanJobsAsync(timeThreshold, _options.RemoveOnlySucceedJobs, cleanJobsCancellationTokenSource.Token);
        }

        if (!_options.RemoveMessages) return;

        using (var cleanMessagesCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60)))
        {
            await storageCleaner.CleanMessageAsync(timeThreshold, cleanMessagesCancellationTokenSource.Token);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}

public record StorageCleanerHostedServiceOptionsFor<TMessageStorageClient>
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

    private TimeSpan? _finalizedEntityRemovedAfter = TimeSpan.FromHours(24);

    public TimeSpan? FinalizedEntityRemovedAfter
    {
        get => _finalizedEntityRemovedAfter;
        set
        {
            if (value < TimeSpan.Zero)
            {
                throw new ParameterException($"{nameof(StorageCleanerHostedServiceOptionsFor<TMessageStorageClient>)}.{nameof(FinalizedEntityRemovedAfter)}");
            }

            _finalizedEntityRemovedAfter = value;
        }
    }

    public bool RemoveOnlySucceedJobs { get; set; } = true;
    public bool RemoveMessages { get; set; } = true;
}