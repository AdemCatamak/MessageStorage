using System;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.Clients;
using Microsoft.Extensions.Hosting;

namespace MessageStorage.DI.Extension
{
    public class BackgroundProcessorHostedService : IHostedService
    {
        private readonly IBackgroundProcessor _jobProcessor;

        public BackgroundProcessorHostedService(IBackgroundProcessor jobProcessor)
        {
            _jobProcessor = jobProcessor ?? throw new ArgumentNullException(nameof(jobProcessor));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _jobProcessor.StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _jobProcessor.StopAsync(cancellationToken);
        }
    }
}