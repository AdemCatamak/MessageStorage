using System;
using MassTransit;

namespace MessageStorage;

public class Job
{
    public Guid Id { get; private set; }
    public Message Message { get; private set; }
    public string MessageHandlerTypeName { get; private set; }
    public JobStatus JobStatus { get; private set; }

    public DateTime CreatedOn { get; private set; }
    public DateTime LastOperationTime { get; private set; }
    public string? LastOperationInfo { get; private set; }
    public int CurrentRetryCount { get; private set; }
    public int MaxRetryCount { get; private set; }

    public Job(Message message, string messageHandlerTypeName, int maxExecutionAttemptCount)
        : this(NewId.Next().ToSequentialGuid(), message, messageHandlerTypeName, JobStatus.Queued, DateTime.UtcNow, DateTime.UtcNow, JobStatus.Queued.ToString(), 0, maxExecutionAttemptCount)
    {
    }

    internal Job(Guid id, Message message, string messageHandlerTypeName, JobStatus jobStatus, DateTime createdOn, DateTime lastOperationTime, string? lastOperationInfo, int currentRetryCount, int maxRetryCount)
    {
        Id = id;
        Message = message;
        MessageHandlerTypeName = messageHandlerTypeName;
        JobStatus = jobStatus;
        CreatedOn = createdOn;
        LastOperationTime = lastOperationTime;
        LastOperationInfo = lastOperationInfo;
        CurrentRetryCount = currentRetryCount;
        MaxRetryCount = maxRetryCount;
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