using System.Threading;
using System.Threading.Tasks;
using MessageStorage;
using Microsoft.Extensions.Hosting;

namespace Samples.Db.WebApi.HostedServices
{
    public class JobProcessHostedService : IHostedService
    {
        private readonly IJobProcessServer _jobProcessServer;

        public JobProcessHostedService(IJobProcessServer jobProcessServer)
        {
            _jobProcessServer = jobProcessServer;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _jobProcessServer.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await (_jobProcessServer?.StopAsync() ?? Task.CompletedTask);
        }
    }
}