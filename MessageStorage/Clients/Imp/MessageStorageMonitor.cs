using MessageStorage.DataAccessSection;
using MessageStorage.DataAccessSection.Repositories;
using MessageStorage.Models;

namespace MessageStorage.Clients.Imp
{
    public class MessageStorageMonitor : IMessageStorageMonitor
    {
        private readonly IRepositoryContext _repositoryContext;

        public MessageStorageMonitor(IRepositoryContext repositoryContext)
        {
            _repositoryContext = repositoryContext;
        }

        private int GetJobCountByStatus(JobStatuses jobStatus)
        {
            IJobRepository jobRepository = _repositoryContext.JobRepository;
            int result = jobRepository.GetJobCountByStatus(jobStatus);
            return result;
        }

        public int GetWaitingJobCount()
        {
            int result = GetJobCountByStatus(JobStatuses.Waiting);
            return result;
        }

        public int GetFailedJobCount()
        {
            int result = GetJobCountByStatus(JobStatuses.Failed);
            return result;
        }

        public void Dispose()
        {
            _repositoryContext?.Dispose();
        }
    }
}