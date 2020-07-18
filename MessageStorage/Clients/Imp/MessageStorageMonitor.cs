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

        public int GetJobCountByStatus(JobStatus jobStatus)
        {
            IJobRepository jobRepository = _repositoryContext.JobRepository;
            int result = jobRepository.GetJobCountByStatus(jobStatus);
            return result;
        }

        public int GetWaitingJobCount()
        {
            int result = GetJobCountByStatus(JobStatus.Waiting);
            return result;
        }

        public int GetFailedJobCount()
        {
            int result = GetJobCountByStatus(JobStatus.Failed);
            return result;
        }

        public void Dispose()
        {
            _repositoryContext?.Dispose();
        }
    }
}