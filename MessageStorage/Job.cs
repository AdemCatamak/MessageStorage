using System;

namespace MessageStorage
{
    public class Job
    {
        public long Id { get; private set; }
        public long MessageId => Message.Id;
        public Message Message { get; private set; }
        public string AssignedHandlerName { get; private set; }
        public JobStatuses JobStatus { get; private set; }
        public DateTime LastOperationTime { get; private set; }
        public string LastOperationInfo { get; private set; }

        public Job(long id, Message message, string assignedHandlerName, JobStatuses jobStatus, DateTime lastOperationTime, string lastOperationInfo)
        {
            Id = id;
            Message = message;
            AssignedHandlerName = assignedHandlerName;
            JobStatus = jobStatus;
            LastOperationInfo = lastOperationInfo;
            LastOperationTime = lastOperationTime;
        }

        public Job(Message message, string assignedHandlerName)
        {
            Id = default;
            Message = message;
            AssignedHandlerName = assignedHandlerName;
            JobStatus = JobStatuses.Waiting;
            LastOperationInfo = null;
            LastOperationTime = DateTime.UtcNow;
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

        public void SetId(long id)
        {
            if (id == default)
                throw new ArgumentException("id should not be null");
            Id = id;
        }

        public string TraceId => Message.TraceId;
    }

    public enum JobStatuses
    {
        Waiting = 1,
        InProgress = 2,
        Done = 3,
        Failed = 4,
    }
}