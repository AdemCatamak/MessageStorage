using System;

namespace MessageStorage
{
    public class Job
    {
        public Guid Id { get; private set; }
        public Message Message { get; private set; }
        public string AssignedHandler { get; private set; }
        public JobStatuses JobStatus { get; private set; }
        public DateTime LastOperationTime { get; private set; }
        public string LastOperationInfo { get; private set; }

        internal Job(Guid id, Message message, string handlerName, JobStatuses jobStatus, DateTime lastOperationTime, string lastOperationInfo)
        {
            Id = id;
            Message = message;
            AssignedHandler = handlerName;
            JobStatus = jobStatus;
            LastOperationInfo = lastOperationInfo;
            LastOperationTime = lastOperationTime;
        }

        public Job(Message message, string handlerName) : this(default, message, handlerName, JobStatuses.Waiting, default, default)
        {
        }

        public void SetInProgress(string info = null)
        {
            LastOperationInfo = info;
            LastOperationTime = DateTime.UtcNow;
            JobStatus = JobStatuses.InProgress;
        }

        public void SetDone(string info = null)
        {
            LastOperationInfo = info;
            LastOperationTime = DateTime.UtcNow;
            JobStatus = JobStatuses.Done;
        }

        public void SetFailed(string failInfo)
        {
            JobStatus = JobStatuses.Failed;
            LastOperationTime = DateTime.UtcNow;
            LastOperationInfo = failInfo;
        }
    }

    public enum JobStatuses
    {
        Waiting = 1,
        InProgress = 2,
        Done = 3,
        Failed = 4,
    }
}