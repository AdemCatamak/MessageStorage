using System.Threading;
using System.Threading.Tasks;

namespace MessageStorage;

public interface IMonitorClient
{
    Task<int> GetJobCountAsync(JobStatus jobStatus, CancellationToken cancellationToken = default);
}

public interface IMonitorClientFor<TMessageStorageClient> : IMonitorClient
    where TMessageStorageClient : IMessageStorageClient
{
}