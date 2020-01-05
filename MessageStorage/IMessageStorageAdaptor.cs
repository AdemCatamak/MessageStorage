using System.Collections.Generic;

namespace MessageStorage
{
    public interface IMessageStorageAdaptor
    {
        Message Add(Message message);
        Message Add(Message message, IEnumerable<Job> jobs);
        void Update(Job job);
        Job SetFirstWaitingJobToInProgress();
    }
}