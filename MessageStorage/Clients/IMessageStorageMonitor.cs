using System;
using MessageStorage.Models;

namespace MessageStorage.Clients
{
    public interface IMessageStorageMonitor : IDisposable
    {
        int GetJobCountByStatus(JobStatus jobStatus);
        int GetWaitingJobCount();
        int GetFailedJobCount();
    }
}