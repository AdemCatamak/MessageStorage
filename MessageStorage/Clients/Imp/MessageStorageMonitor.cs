using MessageStorage.Configurations;
using MessageStorage.DataAccessSection;
using MessageStorage.DataAccessSection.Repositories;
using MessageStorage.Models;

namespace MessageStorage.Clients.Imp
{
    public class MessageStorageMonitor<TRepositoryConfiguration> : IMessageStorageMonitor
        where TRepositoryConfiguration : RepositoryConfiguration
    {
        private readonly IRepositoryContext<TRepositoryConfiguration> _repositoryContext;

        public MessageStorageMonitor(IRepositoryContext<TRepositoryConfiguration> repositoryContext)
        {
            _repositoryContext = repositoryContext;
        }

        private int GetJobCountByStatus(JobStatuses jobStatus)
        {
            IJobRepository<TRepositoryConfiguration> jobRepository = _repositoryContext.JobRepository;
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
    }
}