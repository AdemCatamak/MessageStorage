using System;
using MassTransit;
using MessageStorage.Models.Base;

namespace MessageStorage.Models
{
    public class Job : Entity
    {
        public DateTime CreatedOn { get; }
        public string AssignedHandlerName { get; }
        public JobStatuses JobStatus { get; private set; }
        public DateTime LastOperationTime { get; private set; }
        public string LastOperationInfo { get; private set; }
        public Message Message { get; }

        public Job(string assignedHandlerName, Message message)
            : this(id: default, DateTime.UtcNow, assignedHandlerName, JobStatuses.Waiting, DateTime.UtcNow, lastOperationInfo: default, message)
        {
        }

        public Job(string id, DateTime createdOn, string assignedHandlerName, JobStatuses jobStatus, DateTime lastOperationTime, string lastOperationInfo, Message message)
        {
            Id = id ?? NewId.Next().ToString();
            CreatedOn = createdOn;
            AssignedHandlerName = assignedHandlerName;
            JobStatus = jobStatus;
            LastOperationTime = lastOperationTime;
            LastOperationInfo = lastOperationInfo;
            Message = message;
        }

        public void SetInProgress()
        {
            JobStatus = JobStatuses.InProgress;
            LastOperationTime = DateTime.UtcNow;
            LastOperationInfo = nameof(SetInProgress);
        }

        public void SetDone()
        {
            JobStatus = JobStatuses.Done;
            LastOperationTime = DateTime.UtcNow;
            LastOperationInfo = nameof(SetDone);
        }

        public void SetFailed(string errorMessage)
        {
            JobStatus = JobStatuses.Failed;
            LastOperationTime = DateTime.UtcNow;
            LastOperationInfo = errorMessage;
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