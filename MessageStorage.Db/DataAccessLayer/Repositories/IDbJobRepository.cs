using System.Collections.Generic;
using System.Data;
using System.Linq;
using MessageStorage.DataAccessSection.Repositories;
using MessageStorage.Db.DataAccessLayer.QueryBuilders;
using MessageStorage.Exceptions;

namespace MessageStorage.Db.DataAccessLayer.Repositories
{
    public interface IDbJobRepository : IDbRepository<Job>, IJobRepository
    {
    }

    public class DbJobRepository : IDbJobRepository
    {
        private readonly IJobQueryBuilder _jobQueryBuilder;
        private readonly IDbAdaptor _dbAdaptor;
        private readonly MessageStorageDbConfiguration _messageStorageDbConfiguration;

        public DbJobRepository(IJobQueryBuilder jobQueryBuilder, IDbAdaptor dbAdaptor, MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            _jobQueryBuilder = jobQueryBuilder;
            _dbAdaptor = dbAdaptor;
            _messageStorageDbConfiguration = messageStorageDbConfiguration;
        }

        public void Add(Job entity)
        {
            using (IDbConnection dbConnection = _dbAdaptor.CreateConnection(_messageStorageDbConfiguration.ConnectionStr))
            {
                dbConnection.Open();
                using (IDbTransaction dbTransaction = dbConnection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    Add(entity, dbTransaction);
                    dbTransaction.Commit();
                }
            }
        }

        public void Add(Job entity, IDbTransaction dbTransaction)
        {
            (string commandText, IEnumerable<IDbDataParameter> dbDataParameters) = _jobQueryBuilder.Add(entity);
            using (IDbCommand dbCommand = dbTransaction.Connection.CreateCommand())
            {
                dbCommand.CommandText = commandText;
                dbCommand.Transaction = dbTransaction;
                foreach (IDbDataParameter dbDataParameter in dbDataParameters)
                {
                    dbCommand.Parameters.Add(dbDataParameter);
                }

                object idObj = dbCommand.ExecuteScalar();
                if (long.TryParse(idObj.ToString(), out long id))
                {
                    if (id <= 0)
                    {
                        throw new UnexpectedResponseException($"Insert {nameof(Job)} operation should return Id value which has value is greater than 0");
                    }

                    entity.SetId(id);
                }
                else
                {
                    throw new ArgumentNotCompatibleException(typeof(long), idObj.GetType());
                }
            }
        }

        public Job SetFirstWaitingJobToInProgress()
        {
            (string commandText, IEnumerable<IDbDataParameter> dbDataParameters) = _jobQueryBuilder.SetFirstWaitingJobToInProgress();
            using (IDbConnection dbConnection = _dbAdaptor.CreateConnection(_messageStorageDbConfiguration.ConnectionStr))
            {
                dbConnection.Open();
                using (IDbTransaction dbTransaction = dbConnection.BeginTransaction(IsolationLevel.Serializable))
                {
                    using (IDbCommand dbCommand = dbTransaction.Connection.CreateCommand())
                    {
                        dbCommand.CommandText = commandText;
                        dbCommand.Transaction = dbTransaction;
                        foreach (IDbDataParameter dbDataParameter in dbDataParameters)
                        {
                            dbCommand.Parameters.Add(dbDataParameter);
                        }

                        IDataAdapter dataAdapter = _dbAdaptor.CreateDataAdaptor(dbCommand);
                        var dataset = new DataSet();
                        dataAdapter.Fill(dataset);

                        Job job = _jobQueryBuilder.MapData(dataset.Tables[index: 0].Rows).FirstOrDefault();
                        dbTransaction.Commit();

                        return job;
                    }
                }
            }
        }

        public void Update(Job job)
        {
            (string commandText, IEnumerable<IDbDataParameter> dbDataParameters) = _jobQueryBuilder.Update(job);
            using (IDbConnection dbConnection = _dbAdaptor.CreateConnection(_messageStorageDbConfiguration.ConnectionStr))
            {
                dbConnection.Open();
                using (IDbTransaction dbTransaction = dbConnection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    using (IDbCommand dbCommand = dbTransaction.Connection.CreateCommand())
                    {
                        dbCommand.CommandText = commandText;
                        dbCommand.Transaction = dbTransaction;
                        foreach (IDbDataParameter dbDataParameter in dbDataParameters)
                        {
                            dbCommand.Parameters.Add(dbDataParameter);
                        }

                        dbCommand.ExecuteNonQuery();
                        dbTransaction.Commit();
                    }
                }
            }
        }

        public int GetJobCountByStatus(JobStatuses jobStatus)
        {
            (string commandText, IEnumerable<IDbDataParameter> dbDataParameters) = _jobQueryBuilder.GetJobCountByStatus(jobStatus);
            using (IDbConnection dbConnection = _dbAdaptor.CreateConnection(_messageStorageDbConfiguration.ConnectionStr))
            {
                dbConnection.Open();
                using (IDbTransaction dbTransaction = dbConnection.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    using (IDbCommand dbCommand = dbTransaction.Connection.CreateCommand())
                    {
                        dbCommand.CommandText = commandText;
                        dbCommand.Transaction = dbTransaction;
                        foreach (IDbDataParameter dbDataParameter in dbDataParameters)
                        {
                            dbCommand.Parameters.Add(dbDataParameter);
                        }

                        object result = dbCommand.ExecuteScalar();
                        int jobCountByStatus = int.Parse(result.ToString());
                        return jobCountByStatus;
                    }
                }
            }
        }
    }
}