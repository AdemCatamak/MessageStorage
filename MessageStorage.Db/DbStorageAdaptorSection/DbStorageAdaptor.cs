using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MessageStorage.Db.DbStorageAdaptorSection
{
    public abstract class DbStorageAdaptor : IDbStorageAdaptor
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        protected DbStorageAdaptor(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }

        public void Add(Message message, IEnumerable<Job> jobs)
        {
            using (IDbConnection dbConnection = _dbConnectionFactory.CreateConnection())
            {
                dbConnection.Open();
                using (IDbTransaction dbTransaction = dbConnection.BeginTransaction(IsolationLevel.Serializable))
                {
                    Add(message, jobs, dbTransaction);
                    dbTransaction.Commit();
                }
            }
        }

        public void Update(Job job)
        {
            (string commandText, IEnumerable<IDbDataParameter> dbDataParameters) = PrepareUpdateCommand(job);

            using (IDbConnection dbConnection = _dbConnectionFactory.CreateConnection())
            {
                dbConnection.Open();
                using (IDbTransaction dbTransaction = dbConnection.BeginTransaction(IsolationLevel.Serializable))
                {
                    using (IDbCommand updateJobCommand = dbConnection.CreateCommand())
                    {
                        updateJobCommand.CommandText = commandText;
                        dbDataParameters.ToList().ForEach(p => updateJobCommand.Parameters.Add(p));
                        updateJobCommand.Transaction = dbTransaction;

                        updateJobCommand.ExecuteNonQuery();
                    }

                    dbTransaction.Commit();
                }
            }
        }

        public Job SetFirstWaitingJobToInProgress()
        {
            (string commandText, IEnumerable<IDbDataParameter> dbDataParameters) = SetFirstWaitingJobToInProgressCommand();
            using (IDbConnection dbConnection = _dbConnectionFactory.CreateConnection())
            {
                dbConnection.Open();
                using (IDbTransaction dbTransaction = dbConnection.BeginTransaction(IsolationLevel.Serializable))
                {
                    using (IDbCommand dbCommand = dbConnection.CreateCommand())
                    {
                        dbCommand.CommandText = commandText;
                        dbDataParameters.ToList().ForEach(p => dbCommand.Parameters.Add(p));
                        dbCommand.Transaction = dbTransaction;

                        IDataAdapter dataAdapter = GetDataAdaptor(dbCommand);
                        var dataset = new DataSet();
                        dataAdapter.Fill(dataset);

                        return MapData(dataset.Tables[0].Rows).FirstOrDefault();
                    }
                }
            }
        }

        public void Add(Message message, IEnumerable<Job> jobs, IDbTransaction dbTransaction)
        {
            using (IDbCommand insertMessageCommand = CreateInsertCommand(message, dbTransaction))
            {
                Guid messageId = Guid.Parse(insertMessageCommand.ExecuteScalar().ToString());
                message.SetId(messageId);
            }

            foreach (Job job in jobs)
            {
                using (IDbCommand insertJobCommand = CreateInsertCommand(job, dbTransaction))
                {
                    Guid jobId = Guid.Parse(insertJobCommand.ExecuteScalar().ToString());
                    job.SetId(jobId);
                }
            }
        }

        private IDbCommand CreateInsertCommand(Job job, IDbTransaction dbTransaction)
        {
            (string commandText, IEnumerable<IDbDataParameter> dbDataParameters) = PrepareInsertCommand(job);

            IDbCommand insertJobCommand = dbTransaction.Connection.CreateCommand();
            insertJobCommand.CommandText = commandText;
            dbDataParameters.ToList().ForEach(p => insertJobCommand.Parameters.Add(p));
            insertJobCommand.Transaction = dbTransaction;

            return insertJobCommand;
        }

        private IDbCommand CreateInsertCommand(Message message, IDbTransaction dbTransaction)
        {
            (string commandText, IEnumerable<IDbDataParameter> dbDataParameters) = PrepareInsertCommand(message);

            IDbCommand insertMessageCommand = dbTransaction.Connection.CreateCommand();
            insertMessageCommand.CommandText = commandText;
            dbDataParameters.ToList().ForEach(p => insertMessageCommand.Parameters.Add(p));
            insertMessageCommand.Transaction = dbTransaction;

            return insertMessageCommand;
        }


        protected abstract (string, IEnumerable<IDbDataParameter>) PrepareInsertCommand(Message message);
        protected abstract (string, IEnumerable<IDbDataParameter>) PrepareInsertCommand(Job job);
        protected abstract (string, IEnumerable<IDbDataParameter>) PrepareUpdateCommand(Job job);
        protected abstract (string, IEnumerable<IDbDataParameter>) SetFirstWaitingJobToInProgressCommand();
        protected abstract IDataAdapter GetDataAdaptor(IDbCommand dbCommand);
        protected abstract IEnumerable<Job> MapData(DataRowCollection dataRowCollection);
    }
}