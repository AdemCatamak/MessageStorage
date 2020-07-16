using System.Threading;
using System.Threading.Tasks;
using MessageStorage.Clients;
using Microsoft.Extensions.Hosting;

namespace MessageStorage.AspNetCore
{
    public class JobProcessorHostedService<TJobProcessor> : IHostedService
        where TJobProcessor : IJobProcessor
    {
        private readonly TJobProcessor _jobProcessor;

        public JobProcessorHostedService(TJobProcessor jobProcessor)
        {
            _jobProcessor = jobProcessor;
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