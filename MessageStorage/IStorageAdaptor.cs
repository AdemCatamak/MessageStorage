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
        private readonly Dictionary<long, Message> _messageStorage;
        private readonly Dictionary<long, Job> _jobStorage;
        private readonly object _lockObj;

        public InMemoryStorageAdaptor()
        {
            _lockObj = new object();
            _messageStorage = new Dictionary<long, Message>();
            _jobStorage = new Dictionary<long, Job>();
        }

        public void Add(Message message, IEnumerable<Job> jobs)
        {
            lock (_lockObj)
            {
                AddMessage(message);

                var filteredJobs = (jobs ?? new List<Job>()).Where(j => j != null);
                foreach (Job job in filteredJobs)
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
            if (message == null)
            {
                throw new MessageNullException();
            }

            long maxKey = _messageStorage.OrderByDescending(pair => pair.Key).FirstOrDefault().Key;
            long id = maxKey + 1;
            message.SetId(id);
            if (_messageStorage.ContainsKey(message.Id))
            {
                throw new MessageAlreadyExistException(message.Id.ToString());
            }

            _messageStorage.Add(message.Id, message);
        }

        private void AddJob(Job job)
        {
            long maxKey = _jobStorage.OrderByDescending(pair => pair.Key).FirstOrDefault().Key;
            long id = maxKey + 1;
            job.SetId(id);
            if (_jobStorage.ContainsKey(job.Id))
            {
                throw new JobAlreadyExistException(job.Id.ToString());
            }

            _jobStorage.Add(job.Id, job);
        }

        private void UpdateJob(Job job)
        {
            if (!_jobStorage.ContainsKey(job.Id))
            {
                throw new JobNotFoundException(job.Id.ToString());
            }

            _jobStorage.Remove(job.Id);
            _jobStorage.Add(job.Id, job);
        }
    }
}