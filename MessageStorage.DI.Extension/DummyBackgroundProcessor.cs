using System;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.Clients;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MessageStorage.DI.Extension
{
    public class DummyBackgroundProcessor : IBackgroundProcessor
    {
        private readonly ILogger<DummyBackgroundProcessor> _logger;

        public DummyBackgroundProcessor(ILogger<DummyBackgroundProcessor>? logger = null)
        {
            _logger = logger ?? NullLogger<DummyBackgroundProcessor>.Instance;
        }

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug($"{nameof(DummyBackgroundProcessor)}.{nameof(StartAsync)} is called - {DateTime.UtcNow}");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug($"{nameof(DummyBackgroundProcessor)}.{nameof(StopAsync)} is called - {DateTime.UtcNow}");
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _logger.LogDebug($"{nameof(DummyBackgroundProcessor)}.{nameof(Dispose)} is called - {DateTime.UtcNow}");
        }
    }
}