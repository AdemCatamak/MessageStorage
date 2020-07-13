namespace MessageStorage.Clients
{
    public interface IMessageStorageMonitor
    {
        int GetWaitingJobCount();
        int GetFailedJobCount();
    }
}