using System;
using MassTransit;

namespace MessageStorage
{
    public class Job
    {
        public Guid Id { get; private set; }
        public Message Message { get; private set; }
        public string MessageHandlerTypeName { get; private set; }
        public JobStatus JobStatus { get; private set; }

        public DateTime CreatedOn { get; private set; }
        public DateTime LastOperationTime { get; private set; }
        public string? LastOperationInfo { get; private set; }
        public int CurrentExecutionAttemptCount { get; private set; }
        public DateTime ExecuteLaterThan { get; private set; }

        public Job(Message message, string messageHandlerTypeName, DateTime? executeLaterThan = null)
            : this(NewId.Next().ToSequentialGuid(), message, messageHandlerTypeName, JobStatus.Queued, DateTime.UtcNow, DateTime.UtcNow, JobStatus.Queued.ToString(), 0, executeLaterThan ?? DateTime.UtcNow)
        {
        }

        public Job(Guid id, Message message, string messageHandlerTypeName, JobStatus jobStatus, DateTime createdOn, DateTime lastOperationTime, string? lastOperationInfo, int currentExecutionAttemptCount, DateTime executeLaterThan)
        {
            Id = id;
            Message = message;
            MessageHandlerTypeName = messageHandlerTypeName;
            JobStatus = jobStatus;
            CreatedOn = createdOn;
            LastOperationTime = lastOperationTime;
            LastOperationInfo = lastOperationInfo;
            CurrentExecutionAttemptCount = currentExecutionAttemptCount;
            ExecuteLaterThan = executeLaterThan;
        }

        public void SetQueued()
        {
            ChangeJobStatus(JobStatus.Queued, JobStatus.Queued.ToString());
        }

        public void SetInProgress()
        {
            ChangeJobStatus(JobStatus.InProgress, JobStatus.InProgress.ToString());
        }

        public void SetCompleted()
        {
            ChangeJobStatus(JobStatus.Completed, JobStatus.Completed.ToString());
        }

        public void SetFailed(string errorMessage)
        {
            ChangeJobStatus(JobStatus.Failed, errorMessage);
        }

        private void ChangeJobStatus(JobStatus jobStatus, string lastOperationInfo)
        {
            JobStatus = jobStatus;
            LastOperationInfo = lastOperationInfo;
            LastOperationTime = DateTime.UtcNow;
        }
    }

    public enum JobStatus
    {
        Queued = 1,
        InProgress = 2,
        Completed = 3,
        Failed = 4,
    }
}