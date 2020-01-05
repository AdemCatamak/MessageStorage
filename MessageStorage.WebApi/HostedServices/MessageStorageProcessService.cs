using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace MessageStorage.WebApi.HostedServices
{
    public class MessageStorageProcessService : IHostedService
    {
        private readonly IJobServer _jobServer;

        public MessageStorageProcessService(IJobServer jobServer)
        {
            _jobServer = jobServer;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _jobServer.StartAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _jobServer.StopAsync();
        }
    }
}