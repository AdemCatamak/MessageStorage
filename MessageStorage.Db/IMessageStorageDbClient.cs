using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MessageStorage.Db.DataAccessLayer;
using MessageStorage.Db.DataAccessLayer.Repositories;

namespace MessageStorage.Db
{
    public interface IMessageStorageDbClient : IMessageStorageClient
    {
        (Message, IEnumerable<Job>) Add<T>(T payload, IDbTransaction dbTransaction, string traceId = null, bool autoJobCreator = true);
    }

    public class MessageStorageDbClient<TMessageStorageDbConfiguration> : MessageStorageClient, IMessageStorageDbClient
        where TMessageStorageDbConfiguration : MessageStorageDbConfiguration
    {
        private readonly TMessageStorageDbConfiguration _messageStorageDbConfiguration;

        public MessageStorageDbClient(IHandlerManager handlerManager, IDbRepositoryResolver dbRepositoryResolver, IMigrationRunner migrationRunner, IEnumerable<IMigration> migrations, TMessageStorageDbConfiguration messageStorageDbConfiguration)
            : base(handlerManager, dbRepositoryResolver)
        {
            migrationRunner.Run(migrations, messageStorageDbConfiguration);
            _messageStorageDbConfiguration = messageStorageDbConfiguration;
        }

        public (Message, IEnumerable<Job>) Add<T>(T payload, IDbTransaction dbTransaction, string traceId = null, bool autoJobCreator = true)
        {
            (Message message, IEnumerable<Job> jobs) = PrepareMessageAndJobs(payload, traceId, autoJobCreator);
            var messageRepository = RepositoryResolver.Resolve<IDbMessageRepository>();

            messageRepository.Add(message, dbTransaction);
            List<Job> jobList = jobs.ToList();
            if (jobList.Any())
            {
                var jobRepository = RepositoryResolver.Resolve<IDbJobRepository>();
                foreach (Job job in jobList)
                {
                    jobRepository.Add(job, dbTransaction);
                }
            }

            return (message, jobList);
        }
    }
}