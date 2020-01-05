using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MessageStorage.Exceptions;

namespace MessageStorage.MessageStorageAdaptorSection
{
    /// <summary>
    /// This class should only used for test propose.
    /// </summary>
    public class InMemoryMessageStorageAdaptor : IMessageStorageAdaptor
    {
        private readonly ConcurrentDictionary<Guid, Message> _messageStorage;
        private readonly ConcurrentDictionary<Guid, Job> _jobStorage;
        private readonly object _updateJobLockObj;
        private readonly object _setFirstWaitingJobToInProgressLockObj;

        public InMemoryMessageStorageAdaptor()
        {
            _updateJobLockObj = new object();
            _setFirstWaitingJobToInProgressLockObj = new object();
            _messageStorage = new ConcurrentDictionary<Guid, Message>();
            _jobStorage = new ConcurrentDictionary<Guid, Job>();
        }


        /// <summary>
        /// Given payload is stored into memory and return a Message object
        /// </summary>
        /// <param name="message"></param>
        /// <returns>Message is object that is kept relevant info together</returns>
        /// <exception cref="MessageAddOperationFailedException">This exception occurs, when message could not added into ConcurrentDictionary</exception>
        public Message Add(Message message)
        {
            var id = Guid.NewGuid();
            var newMessage = new Message(id, message.SerializedPayload, message.CreatedOn, message.TraceId);
            // ReSharper disable once InconsistentlySynchronizedField
            bool success = _messageStorage.TryAdd(id, newMessage);
            if (!success)
                throw new MessageAddOperationFailedException();
            return newMessage;
        }

        public Message Add(Message message, IEnumerable<Job> jobs)
        {
            var m = Add(message);
            foreach (Job job in jobs)
            {
                var id = Guid.NewGuid();
                var j = new Job(id, m, job.AssignedHandler, job.JobStatus, job.LastOperationTime, job.LastOperationInfo);
                // ReSharper disable once InconsistentlySynchronizedField
                bool success = _jobStorage.TryAdd(j.Id, j);
                if (!success)
                    throw new JobAddOperationFailedException();
            }

            return m;
        }

        public void Update(Job job)
        {
            lock (_updateJobLockObj)
            {
                bool removeSuccess = _jobStorage.TryRemove(job.Id, out Job _);
                if (!removeSuccess)
                    throw new JobRemoveOperationFailed();
                bool addSuccess = _jobStorage.TryAdd(job.Id, job);
                if (!addSuccess)
                    throw new JobAddOperationFailedException();
            }
        }

        public Job SetFirstWaitingJobToInProgress()
        {
            lock (_setFirstWaitingJobToInProgressLockObj)
            {
                KeyValuePair<Guid, Job> jobRow = _jobStorage.OrderByDescending(pair => pair.Value.LastOperationTime).FirstOrDefault(pair => pair.Value.JobStatus == JobStatuses.Waiting);
                if (jobRow.Value == null)
                    return null;
                jobRow.Value.SetInProgress();
                Update(jobRow.Value);
                return jobRow.Value;
            }
        }
    }
}