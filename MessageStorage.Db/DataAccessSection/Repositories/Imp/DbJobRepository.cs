using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MessageStorage.Db.Configurations;
using MessageStorage.Db.DataAccessSection.Repositories.BaseRepository.Imp;
using MessageStorage.Db.Exceptions;
using MessageStorage.Models;

namespace MessageStorage.Db.DataAccessSection.Repositories.Imp
{
    public abstract class DbJobRepository : DbRepository<Job>,
                                            IDbJobRepository
    {
        protected DbJobRepository(IDbConnection dbConnection, DbRepositoryConfiguration dbRepositoryConfiguration)
            : base(dbConnection, dbRepositoryConfiguration)
        {
        }

        public Job SetFirstWaitingJobToInProgress()
        {
            Job job;
            IDbConnection connectionThatUsed = DbTransaction?.Connection ?? DbConnection;

            using (IDbCommand dbCommand = connectionThatUsed.CreateCommand())
            {
                dbCommand.Transaction = DbTransaction;

                (string commandText, IDataParameter[] dataParameters, Func<IDataReader, Job> map) = PrepareSetFirstWaitingJobToInProgressCommand();

                dbCommand.CommandText = commandText;
                foreach (IDataParameter dataParameter in dataParameters)
                {
                    dbCommand.Parameters.Add(dataParameter);
                }

                using (IDataReader reader = dbCommand.ExecuteReader())
                {
                    var jobList = new List<Job>();

                    while (reader.Read())
                    {
                        Job j = map(reader);
                        if (j != null)
                            jobList.Add(j);
                    }

                    if (jobList.Count > 1)
                    {
                        throw new DBConcurrencyException("Some message"); // TODO
                    }

                    job = jobList.FirstOrDefault();
                }
            }

            return job;
        }

        public void Update(Job job)
        {
            IDbConnection connectionThatUsed = DbTransaction?.Connection ?? DbConnection;

            using (IDbCommand dbCommand = connectionThatUsed.CreateCommand())
            {
                dbCommand.Transaction = DbTransaction;

                (string commandText, IDataParameter[] dataParameters) = PrepareUpdateCommand(job);

                dbCommand.CommandText = commandText;
                foreach (IDataParameter dataParameter in dataParameters)
                {
                    dbCommand.Parameters.Add(dataParameter);
                }

                int affectedRowCount = dbCommand.ExecuteNonQuery();
                if (affectedRowCount != 1)
                {
                    throw new UpdateFailedException($"[JobId: {job.Id}] update script return affected row count as {affectedRowCount}");
                }
            }
        }

        public int GetJobCountByStatus(JobStatuses jobStatus)
        {
            IDbConnection connectionThatUsed = DbTransaction?.Connection ?? DbConnection;

            using (IDbCommand dbCommand = connectionThatUsed.CreateCommand())
            {
                dbCommand.Transaction = DbTransaction;

                (string commandText, IDataParameter[] dataParameters) = PrepareGetJobCountByStatusCommand(jobStatus);

                dbCommand.CommandText = commandText;
                foreach (IDataParameter dataParameter in dataParameters)
                {
                    dbCommand.Parameters.Add(dataParameter);
                }

                object countObject = dbCommand.ExecuteScalar();
                var count = Convert.ToInt32(countObject);
                return count;
            }
        }

        protected abstract (string, IDataParameter[]) PrepareUpdateCommand(Job job);
        protected abstract (string, IDataParameter[]) PrepareGetJobCountByStatusCommand(JobStatuses jobStatus);
        protected abstract (string, IDataParameter[], Func<IDataReader, Job>) PrepareSetFirstWaitingJobToInProgressCommand();
    }
}