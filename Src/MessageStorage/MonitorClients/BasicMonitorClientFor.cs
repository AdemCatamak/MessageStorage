using System.Threading;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer;

namespace MessageStorage.MonitorClients;

public class BasicMonitorClientFor<TMessageStorageClient> : IMonitorClientFor<TMessageStorageClient>
    where TMessageStorageClient : IMessageStorageClient
{
    private readonly IRepositoryContextFor<TMessageStorageClient> _repositoryContext;

    public BasicMonitorClientFor(IRepositoryContextFor<TMessageStorageClient> repositoryContext)
    {
        _repositoryContext = repositoryContext;
    }

    public async Task<int> GetJobCountAsync(JobStatus jobStatus, CancellationToken cancellationToken)
    {
        IJobRepository jobRepository = _repositoryContext.GetJobRepository();
        int result = await jobRepository.GetJobCountAsync(jobStatus, cancellationToken);
        return result;
    }

    public void Dispose()
    {
        _repositoryContext.Dispose();
    }
}