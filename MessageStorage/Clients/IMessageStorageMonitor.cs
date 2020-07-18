using System;

namespace MessageStorage.Clients
{
    public interface IMessageStorageMonitor : IDisposable
    {
        int GetWaitingJobCount();
        int GetFailedJobCount();
    }
}