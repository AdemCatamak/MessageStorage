using System;
using MassTransit;

namespace MessageStorage
{
    public class Job
    {
        public string JobId { get; private set; }
        public string AssignedHandlerName { get; private set; }
        public JobStatuses JobStatus { get; private set; }
        public DateTime LastOperationTime { get; private set; }
        public string LastOperationInfo { get; private set; }
        public Message Message { get; private set; }
        public string MessageId => Message.MessageId;

        public string TraceId => Message.TraceId;

        public Job(string jobId, Message message, string assignedHandlerName, JobStatuses jobStatus, DateTime lastOperationTime, string lastOperationInfo)
        {
            JobId = jobId;
            Message = message;
            AssignedHandlerName = assignedHandlerName;
            JobStatus = jobStatus;
            LastOperationInfo = lastOperationInfo;
            LastOperationTime = lastOperationTime;
        }

        public Job(Message message, string assignedHandlerName)
        {
            JobId = NewId.Next().ToString();
            Message = message;
            AssignedHandlerName = assignedHandlerName;
            JobStatus = JobStatuses.Waiting;
            LastOperationInfo = nameof(Job);
            LastOperationTime = DateTime.UtcNow;
        }

        public void SetInProgress(string info = null)
        {
            LastOperationInfo = info ?? nameof(SetInProgress);
            LastOperationTime = DateTime.UtcNow;
            JobStatus = JobStatuses.InProgress;
        }

        public void SetDone(string info = null)
        {
            LastOperationInfo = info ?? nameof(SetDone);
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