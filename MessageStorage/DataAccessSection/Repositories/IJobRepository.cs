using MessageStorage.Configurations;
using MessageStorage.DataAccessSection.Repositories.Base;
using MessageStorage.Models;

namespace MessageStorage.DataAccessSection.Repositories
{
    public interface IJobRepository<out TRepositoryConfiguration>
        : IRepository<TRepositoryConfiguration, Job>
        where TRepositoryConfiguration : RepositoryConfiguration
    {
        Job SetFirstWaitingJobToInProgress();
        void Update(Job job);
        int GetJobCountByStatus(JobStatuses jobStatus);
    }
}