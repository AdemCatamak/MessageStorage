using System;
using System.Data;
using System.Linq;
using Dapper;
using MessageStorage.Configurations;
using MessageStorage.DataAccessSection.Repositories.Base;
using MessageStorage.Exceptions;
using MessageStorage.Models;

namespace MessageStorage.DataAccessSection.Repositories
{
    public interface IJobRepository : IRepository<Job>
    {
        Job? SetFirstWaitingJobToInProgress();
        void UpdateJobStatus(Job job);
        int GetJobCount(JobStatus jobStatus);
    }

    public abstract class BaseJobRepository : BaseRepository<Job>,
                                              IJobRepository
    {
        protected BaseJobRepository(IDbTransaction dbTransaction, MessageStorageRepositoryContextConfiguration messageStorageRepositoryContextConfiguration)
            : base(dbTransaction, messageStorageRepositoryContextConfiguration)
        {
        }

        protected BaseJobRepository(IDbConnection dbConnection, MessageStorageRepositoryContextConfiguration messageStorageRepositoryContextConfiguration)
            : base(dbConnection, messageStorageRepositoryContextConfiguration)
        {
        }

        protected abstract string AddStatement { get; }

        public override void Add(Job entity)
        {
            int affectedRowCount = DbConnection.Execute(AddStatement,
                                                        new
                                                        {
                                                            JobId = entity.Id,
                                                            MessageId = entity.Message.Id,
                                                            AssignedHandlerName = entity.AssignedHandlerName,
                                                            JobStatus = entity.JobStatus,
                                                            LastOperationTime = entity.LastOperationTime,
                                                            LastOperationInfo = entity.LastOperationInfo,
                                                            CreatedOn = entity.CreatedOn
                                                        },
                                                        DbTransaction);

            if (affectedRowCount != 1)
            {
                throw new InsertFailedException($"[EntityId: {entity.Id}, EntityType: {nameof(Job)}] insert script return affected row count as {affectedRowCount}");
            }
        }

        protected abstract string SetFirstWaitingJobToInProgressStatement { get; }

        public Job? SetFirstWaitingJobToInProgress()
        {
            var jobs = DbConnection.Query(SetFirstWaitingJobToInProgressStatement, DbTransaction)
                                   .Select(row => new Job(
                                                          (string) row.JobId,
                                                          (DateTime) row.JobCreatedOn,
                                                          (string) row.AssignedHandlerName,
                                                          (JobStatus) row.JobStatus,
                                                          (DateTime) row.LastOperationTime,
                                                          (string) row.LastOperationInfo,
                                                          new Message((string) row.MessageId, (DateTime) row.MessageCreatedOn, (string) row.SerializedPayload)
                                                         )
                                          );

            return jobs.FirstOrDefault();
        }

        protected abstract string UpdateJobStatusStatement { get; }

        public void UpdateJobStatus(Job job)
        {
            int affectedRowCount = DbConnection.Execute(UpdateJobStatusStatement,
                                                        new
                                                        {
                                                            JobId = job.Id,
                                                            JobStatus = job.JobStatus,
                                                            LastOperationTime = job.LastOperationTime,
                                                            LastOperationInfo = job.LastOperationInfo,
                                                        },
                                                        DbTransaction);

            if (affectedRowCount != 1)
            {
                throw new UpdateFailedException($"[JobId: {job.Id}] update script return affected row count as {affectedRowCount}");
            }
        }

        protected abstract string GetJobCountStatement { get; }

        public int GetJobCount(JobStatus jobStatus)
        {
            var result = DbConnection.ExecuteScalar<int>(GetJobCountStatement,
                                                         new
                                                         {
                                                             jobStatus
                                                         },
                                                         DbTransaction);

            return result;
        }
    }
}