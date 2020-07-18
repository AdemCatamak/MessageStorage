using MessageStorage.DataAccessSection.Repositories.Base;
using MessageStorage.Models;

namespace MessageStorage.DataAccessSection.Repositories
{
    public interface IJobRepository : IRepository<Job>
    {
        Job SetFirstWaitingJobToInProgress();
        void Update(Job job);
        int GetJobCountByStatus(JobStatuses jobStatus);
    }
}