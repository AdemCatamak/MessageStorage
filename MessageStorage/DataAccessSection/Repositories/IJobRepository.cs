namespace MessageStorage.DataAccessSection.Repositories
{
    public interface IJobRepository : IRepository<Job>
    {
        Job SetFirstWaitingJobToInProgress();
        void Update(Job job);
        int GetJobCountByStatus(JobStatuses jobStatus);
    }
}