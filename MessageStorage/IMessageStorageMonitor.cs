namespace MessageStorage
{
    public interface IMessageStorageMonitor : IJobMonitor
    {
        int GetWaitingJobCount();
        int GetFailedJobCount();
    }

    public class MessageStorageMonitor : IMessageStorageMonitor
    {
        private readonly IStorageAdaptor _storageAdaptor;

        public MessageStorageMonitor(IStorageAdaptor storageAdaptor)
        {
            _storageAdaptor = storageAdaptor;
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

        public int GetJobCountByStatus(JobStatuses jobStatus)
        {
            int result = _storageAdaptor.GetJobCountByStatus(jobStatus);
            return result;
        }
    }
}