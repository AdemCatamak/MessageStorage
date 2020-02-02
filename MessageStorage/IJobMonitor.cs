namespace MessageStorage
{
    public interface IJobMonitor
    {
        int GetJobCountByStatus(JobStatuses jobStatus);
    }
}