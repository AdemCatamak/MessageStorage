using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.Clients;
using Microsoft.Extensions.Hosting;

namespace MessageStorage.DI.Extension
{
    public class BackgroundProcessorHostedService : IHostedService
    {
        private readonly IEnumerable<IBackgroundProcessor> _backgroundProcessors;

        public BackgroundProcessorHostedService(IEnumerable<IBackgroundProcessor>? backgroundProcessors)
        {
            _backgroundProcessors = backgroundProcessors ?? new List<IBackgroundProcessor>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (IBackgroundProcessor backgroundProcessor in _backgroundProcessors)
            {
                await backgroundProcessor.StartAsync(cancellationToken);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (IBackgroundProcessor backgroundProcessor in _backgroundProcessors)
            {
                await backgroundProcessor.StopAsync(cancellationToken);
            }
        }
    }
}