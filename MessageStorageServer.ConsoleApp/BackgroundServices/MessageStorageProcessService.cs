using System.Threading;
using System.Threading.Tasks;
using MessageStorage;
using Microsoft.Extensions.Hosting;

namespace MessageStorageServer.ConsoleApp.BackgroundServices
{
    public class MessageStorageProcessService : IHostedService
    {
        private readonly IMessageProcessServer _messageProcessServer;

        public MessageStorageProcessService(IMessageProcessServer messageProcessServer)
        {
            _messageProcessServer = messageProcessServer;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _messageProcessServer.StartAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _messageProcessServer.StopAsync();
        }
    }
}