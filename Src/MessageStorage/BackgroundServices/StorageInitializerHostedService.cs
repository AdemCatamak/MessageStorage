using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MessageStorage.BackgroundServices;

internal class StorageInitializerHostedService : IHostedService
{
    private readonly IEnumerable<IStorageInitializeEngine> _storageInitializeEngines;
    private readonly ILogger<StorageInitializerHostedService> _logger;

    private Task? _initializeAllStorageTask;

    public StorageInitializerHostedService(IEnumerable<IStorageInitializeEngine> storageInitializeEngines,
                                           ILogger<StorageInitializerHostedService> logger)
    {
        _storageInitializeEngines = storageInitializeEngines;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Task[] initializeTasks = _storageInitializeEngines.Select(engine => InitializeAsync(engine, cancellationToken))
                                                          .ToArray();
        _initializeAllStorageTask = Task.WhenAll(initializeTasks);
        return _initializeAllStorageTask;
    }

    private async Task InitializeAsync(IStorageInitializeEngine storageInitializeEngine, CancellationToken cancellationToken)
    {
        var failed = true;
        const int maxRetryCount = 4;
        var tryCount = 0;
        do
        {
            try
            {
                tryCount++;
                _logger.LogInformation("[{StorageInitializer}] TryNumber {TryCount} / {MaxRetryCount} migration attempt is started", storageInitializeEngine.GetType().Name, tryCount, maxRetryCount);
                await storageInitializeEngine.InitializeAsync(cancellationToken);
                failed = false;
                _logger.LogInformation("[{StorageInitializer}] TryNumber {TryCount} / {MaxRetryCount} migration attempt is completed", storageInitializeEngine.GetType().Name, tryCount, maxRetryCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{StorageInitializer}] TryNumber {TryCount} / {MaxRetryCount} migration attempt is failed", storageInitializeEngine.GetType().Name, tryCount, maxRetryCount);
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        } while (failed && tryCount <= maxRetryCount);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _initializeAllStorageTask?.Dispose();
        return Task.CompletedTask;
    }
}