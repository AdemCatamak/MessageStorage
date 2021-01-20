using System;
using System.Threading;
using System.Threading.Tasks;

namespace MessageStorage.Clients
{
    public interface IBackgroundProcessor : IDisposable
    {
        Task StartAsync(CancellationToken cancellationToken = default);
        Task StopAsync(CancellationToken cancellationToken = default);
    }
}