using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MessageStorage.Db
{
    public interface IDbStorageAdaptor : IStorageAdaptor, IDbConnectionFactory
    {
        void Add(Message message, IEnumerable<Job> jobs, IDbTransaction dbTransaction);
    }

    public abstract class DbStorageAdaptor : IDbStorageAdaptor
    {
        public MessageStorageDbConfiguration MessageStorageDbConfiguration { get; private set; }


        public void Add(Message message, IEnumerable<Job> jobs)
        {
            using (IDbConnection dbConnection = CreateConnection())
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
            (string commandText, IEnumerable<IDbDataParameter> dbDataParameters) = PrepareUpdateCommand(job, MessageStorageDbConfiguration);

            using (IDbConnection dbConnection = CreateConnection())
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
            (string commandText, IEnumerable<IDbDataParameter> dbDataParameters) = SetFirstWaitingJobToInProgressCommand(MessageStorageDbConfiguration);
            using (IDbConnection dbConnection = CreateConnection())
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

                        Job job = MapData(dataset.Tables[0].Rows).FirstOrDefault();
                        dbTransaction.Commit();

                        return job;
                    }
                }
            }
        }

        public void Add(Message message, IEnumerable<Job> jobs, IDbTransaction dbTransaction)
        {
            using (IDbCommand insertMessageCommand = CreateInsertCommand(message, dbTransaction))
            {
                long messageId = long.Parse(insertMessageCommand.ExecuteScalar().ToString());
                message.SetId(messageId);
            }

            foreach (Job job in jobs)
            {
                using (IDbCommand insertJobCommand = CreateInsertCommand(job, dbTransaction))
                {
                    long jobId = long.Parse(insertJobCommand.ExecuteScalar().ToString());
                    job.SetId(jobId);
                }
            }
        }

        public virtual void SetConfiguration(MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            MessageStorageDbConfiguration = messageStorageDbConfiguration ?? throw new ArgumentNullException(nameof(messageStorageDbConfiguration));
        }

        public abstract IDbConnection CreateConnection();


        private IDbCommand CreateInsertCommand(Job job, IDbTransaction dbTransaction)
        {
            (string commandText, IEnumerable<IDbDataParameter> dbDataParameters) = PrepareInsertCommand(job, MessageStorageDbConfiguration);

            IDbCommand insertJobCommand = dbTransaction.Connection.CreateCommand();
            insertJobCommand.CommandText = commandText;
            dbDataParameters.ToList().ForEach(p => insertJobCommand.Parameters.Add(p));
            insertJobCommand.Transaction = dbTransaction;

            return insertJobCommand;
        }

        private IDbCommand CreateInsertCommand(Message message, IDbTransaction dbTransaction)
        {
            (string commandText, IEnumerable<IDbDataParameter> dbDataParameters) = PrepareInsertCommand(message, MessageStorageDbConfiguration);

            IDbCommand insertMessageCommand = dbTransaction.Connection.CreateCommand();
            insertMessageCommand.CommandText = commandText;
            dbDataParameters.ToList().ForEach(p => insertMessageCommand.Parameters.Add(p));
            insertMessageCommand.Transaction = dbTransaction;

            return insertMessageCommand;
        }


        protected abstract (string, IEnumerable<IDbDataParameter>) PrepareInsertCommand(Message message, MessageStorageDbConfiguration messageStorageDbConfiguration);
        protected abstract (string, IEnumerable<IDbDataParameter>) PrepareInsertCommand(Job job, MessageStorageDbConfiguration messageStorageDbConfiguration);
        protected abstract (string, IEnumerable<IDbDataParameter>) PrepareUpdateCommand(Job job, MessageStorageDbConfiguration messageStorageDbConfiguration);
        protected abstract (string, IEnumerable<IDbDataParameter>) SetFirstWaitingJobToInProgressCommand(MessageStorageDbConfiguration messageStorageDbConfiguration);
        protected abstract IDataAdapter GetDataAdaptor(IDbCommand dbCommand);
        protected abstract IEnumerable<Job> MapData(DataRowCollection dataRowCollection);
    }
}