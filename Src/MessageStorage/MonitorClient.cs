using System.Threading;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer;
using MessageStorage.DataAccessLayer.Repositories;

namespace MessageStorage
{
    public class MonitorClient : IMonitorClient
    {
        private readonly IRepositoryFactory _repositoryFactory;

        public MonitorClient(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public async Task<int> GetJobCountAsync(JobStatus jobStatus, CancellationToken cancellationToken = default)
        {
            IJobRepository jobRepository = _repositoryFactory.CreateJobRepository();
            int result = await jobRepository.GetJobCountAsync(jobStatus, cancellationToken);
            return result;
        }
    }
}