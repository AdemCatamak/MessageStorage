using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MessageStorage.Exceptions;

namespace MessageStorage
{
    public interface IStorageAdaptor
    {
        void Add(Message message, IEnumerable<Job> jobs);
        void Update(Job job);
        Job SetFirstWaitingJobToInProgress();
    }

    public class InMemoryStorageAdaptor : IStorageAdaptor
    {
        private readonly ConcurrentDictionary<long, Message> _messageStorage;
        private readonly ConcurrentDictionary<long, Job> _jobStorage;
        private readonly object _lockObj;

        public InMemoryStorageAdaptor()
        {
            _lockObj = new object();
            _messageStorage = new ConcurrentDictionary<long, Message>();
            _jobStorage = new ConcurrentDictionary<long, Job>();
        }

        public void Add(Message message, IEnumerable<Job> jobs)
        {
            lock (_lockObj)
            {
                AddMessage(message);
                foreach (Job job in jobs ?? new List<Job>())
                {
                    AddJob(job);
                }
            }
        }

        public void Update(Job job)
        {
            lock (_lockObj)
            {
                UpdateJob(job);
            }
        }

        public Job SetFirstWaitingJobToInProgress()
        {
            lock (_lockObj)
            {
                KeyValuePair<long, Job> jobRow = _jobStorage.OrderByDescending(pair => pair.Value.LastOperationTime).FirstOrDefault(pair => pair.Value.JobStatus == JobStatuses.Waiting);
                if (jobRow.Value == null)
                    return null;
                jobRow.Value.SetInProgress();
                UpdateJob(jobRow.Value);
                return jobRow.Value;
            }
        }


        private void AddMessage(Message message)
        {
            int id = _messageStorage.Count + 1;
            message.SetId(id);
            bool success = _messageStorage.TryAdd(message.Id, message);
            if (!success)
                throw new MessageAddOperationFailedException();
        }

        private void AddJob(Job job)
        {
            var id = _jobStorage.Count + 1;
            job.SetId(id);
            bool success = _jobStorage.TryAdd(job.Id, job);
            if (!success)
                throw new JobAddOperationFailedException();
        }
        
        private void UpdateJob(Job job)
        {
            bool removeSuccess = _jobStorage.TryRemove(job.Id, out Job _);
            if (!removeSuccess)
                throw new JobRemoveOperationFailed();
            bool addSuccess = _jobStorage.TryAdd(job.Id, job);
            if (!addSuccess)
                throw new JobAddOperationFailedException();
        }
    }
}