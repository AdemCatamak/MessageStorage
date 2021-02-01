using System;
using MassTransit;
using MessageStorage.Models.Base;

namespace MessageStorage.Models
{
    public class Job : Entity
    {
        public DateTime CreatedOn { get; private set; }
        public string AssignedHandlerName { get; private set; }
        public JobStatus JobStatus { get; private set; }
        public DateTime LastOperationTime { get; private set; }
        public string? LastOperationInfo { get; private set; }
        public Message Message { get; }

        public Job(string assignedHandlerName, Message message)
            : this(NewId.Next().ToString(), DateTime.UtcNow, assignedHandlerName, JobStatus.Waiting, DateTime.UtcNow, lastOperationInfo: null, message)
        {
        }

        public Job(string id, DateTime createdOn, string assignedHandlerName, JobStatus jobStatus, DateTime lastOperationTime, string? lastOperationInfo, Message message)
            : base(id)
        {
            CreatedOn = createdOn;
            AssignedHandlerName = assignedHandlerName;
            JobStatus = jobStatus;
            LastOperationTime = lastOperationTime;
            LastOperationInfo = lastOperationInfo;
            Message = message;
        }

        public void SetInProgress()
        {
            JobStatus = JobStatus.InProgress;
            LastOperationTime = DateTime.UtcNow;
            LastOperationInfo = nameof(SetInProgress);
        }

        public void SetDone()
        {
            JobStatus = JobStatus.Done;
            LastOperationTime = DateTime.UtcNow;
            LastOperationInfo = nameof(SetDone);
        }

        public void SetFailed(string errorMessage)
        {
            JobStatus = JobStatus.Failed;
            LastOperationTime = DateTime.UtcNow;
            LastOperationInfo = errorMessage;
        }
    }

    public enum JobStatus
    {
        Waiting = 1,
        InProgress = 2,
        Done = 3,
        Failed = 4,
    }
}