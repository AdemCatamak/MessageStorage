using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace MessageStorage.WebApi.HostedServices
{
    public class MessageStorageProcessService : IHostedService
    {
        private readonly IJobProcessServer _jobProcessServer;

        public MessageStorageProcessService(IJobProcessServer jobProcessServer)
        {
            _jobProcessServer = jobProcessServer;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _jobProcessServer.StartAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _jobProcessServer.StopAsync();
        }
    }
}