using System;
using System.Collections.Generic;
using System.Data;

namespace MessageStorage.Db.DbStorageAdaptorSection
{
    public class DbStorageAdaptor : IDbStorageAdaptor
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public DbStorageAdaptor(IDbConnectionFactory dbConnectionFactory)
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
            throw new System.NotImplementedException();
        }

        public Job SetFirstWaitingJobToInProgress()
        {
            throw new System.NotImplementedException();
        }

        public void Add(Message message, IEnumerable<Job> jobs, IDbTransaction dbTransaction)
        {
            throw new System.NotImplementedException();
        }
    }
}