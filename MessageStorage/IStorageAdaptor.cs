using System.Collections.Generic;

namespace MessageStorage
{
    public interface IStorageAdaptor
    {
        void Add(Message message, IEnumerable<Job> jobs);
        void Update(Job job);
        Job SetFirstWaitingJobToInProgress();
    }
}