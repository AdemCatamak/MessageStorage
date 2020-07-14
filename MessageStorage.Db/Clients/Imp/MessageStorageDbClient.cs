using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using MessageStorage.Clients.Imp;
using MessageStorage.Db.Configurations;
using MessageStorage.Db.DataAccessSection;
using MessageStorage.Models;

namespace MessageStorage.Db.Clients.Imp
{
    public class MessageStorageDbClient<TDbRepositoryConfiguration>
        : MessageStorageClient,
          IMessageStorageDbClient
        where TDbRepositoryConfiguration : DbRepositoryConfiguration
    {
        private readonly IDbRepositoryContext<TDbRepositoryConfiguration> _dbRepositoryContext;

        public MessageStorageDbClient(IHandlerManager handlerManager, IDbRepositoryContext<TDbRepositoryConfiguration> dbRepositoryContext, MessageStorageDbConfiguration messageStorageConfiguration = null)
            : base(handlerManager, messageStorageConfiguration ?? new MessageStorageDbConfiguration())
        {
            _dbRepositoryContext = dbRepositoryContext ?? throw new ArgumentNullException(nameof(dbRepositoryContext));
        }

        private readonly object _addLock = new object();

        public override Tuple<Message, IEnumerable<Job>> Add<T>(T payload, bool autoJobCreation)
        {
            (Message message, IEnumerable<Job> jobs) = PrepareMessageAndJobs(payload, autoJobCreation);
            List<Job> jobList = jobs.ToList();

            void Add(Message m, IEnumerable<Job> js)
            {
                _dbRepositoryContext.MessageRepository.Add(m);
                foreach (Job j in js)
                {
                    _dbRepositoryContext.JobRepository.Add(j);
                }
            }

            if (_dbRepositoryContext.HasTransaction)
            {
                Add(message, jobList);
            }
            else
            {
                try
                {
                    Monitor.Enter(_addLock);
                    using (IDbTransaction dbTransaction = _dbRepositoryContext.BeginTransaction())
                    {
                        Add(message, jobList);

                        dbTransaction.Commit();
                        _dbRepositoryContext.ClearTransaction();
                    }
                }
                finally
                {
                    Monitor.Exit(_addLock);
                }
            }

            return new Tuple<Message, IEnumerable<Job>>(message, jobList);
        }

        public void UseTransaction(IDbTransaction dbTransaction)
        {
            _dbRepositoryContext.UseTransaction(dbTransaction);
        }

        public void ClearTransaction()
        {
            _dbRepositoryContext.ClearTransaction();
        }
    }
}