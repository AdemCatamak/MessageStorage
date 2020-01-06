using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MessageStorage.Exceptions;

namespace MessageStorage.StorageAdaptorSection
{
    public class InMemoryStorageAdaptor : IStorageAdaptor
    {
        private readonly ConcurrentDictionary<Guid, Message> _messageStorage;
        private readonly ConcurrentDictionary<Guid, Job> _jobStorage;
        private readonly object _updateJobLockObj;

        public InMemoryStorageAdaptor()
        {
            _updateJobLockObj = new object();
            _messageStorage = new ConcurrentDictionary<Guid, Message>();
            _jobStorage = new ConcurrentDictionary<Guid, Job>();
        }

        public void Add(Message message, IEnumerable<Job> jobs)
        {
            Add(message);
            foreach (Job job in jobs ?? new List<Job>())
            {
                Add(job);
            }
        }

        public void Update(Job job)
        {
            lock (_updateJobLockObj)
            {
                UpdateWithoutLock(job);
            }
        }

        public Job SetFirstWaitingJobToInProgress()
        {
            lock (_updateJobLockObj)
            {
                KeyValuePair<Guid, Job> jobRow = _jobStorage.OrderByDescending(pair => pair.Value.LastOperationTime).FirstOrDefault(pair => pair.Value.JobStatus == JobStatuses.Waiting);
                if (jobRow.Value == null)
                    return null;
                jobRow.Value.SetInProgress();
                UpdateWithoutLock(jobRow.Value);
                return jobRow.Value;
            }
        }

        
        private void Add(Message message)
        {
            var id = Guid.NewGuid();
            message.SetId(id);
            // ReSharper disable once InconsistentlySynchronizedField
            bool success = _messageStorage.TryAdd(id, message);
            if (!success)
                throw new MessageAddOperationFailedException();
        }

        private void Add(Job job)
        {
            var id = Guid.NewGuid();
            job.SetId(id);
            // ReSharper disable once InconsistentlySynchronizedField
            bool success = _jobStorage.TryAdd(job.Id, job);
            if (!success)
                throw new JobAddOperationFailedException();
        }

        private void UpdateWithoutLock(Job job)
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