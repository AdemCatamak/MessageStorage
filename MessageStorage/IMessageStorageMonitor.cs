using MessageStorage.DataAccessSection;
using MessageStorage.DataAccessSection.Repositories;

namespace MessageStorage
{
    public interface IJobMonitor
    {
        int GetJobCountByStatus(JobStatuses jobStatus);
    }

    public interface IMessageStorageMonitor : IJobMonitor
    {
        int GetWaitingJobCount();
        int GetFailedJobCount();
    }

    public class MessageStorageMonitor : IMessageStorageMonitor
    {
        private readonly IRepositoryResolver _repositoryResolver;

        public MessageStorageMonitor(IRepositoryResolver repositoryResolver)
        {
            _repositoryResolver = repositoryResolver;
        }

        public int GetJobCountByStatus(JobStatuses jobStatus)
        {
            var jobRepository = _repositoryResolver.Resolve<IJobRepository>();
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