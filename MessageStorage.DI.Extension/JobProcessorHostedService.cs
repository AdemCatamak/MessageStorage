using System;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.Clients;
using Microsoft.Extensions.Hosting;

namespace MessageStorage.DI.Extension
{
    public class JobProcessorHostedService : IHostedService
    {
        private readonly IJobProcessor _jobProcessor;

        public JobProcessorHostedService(IJobProcessor jobProcessor)
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