using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MessageStorage.Configurations;
using MessageStorage.DataAccessSection;
using MessageStorage.DataAccessSection.Repositories;
using MessageStorage.Models;

namespace MessageStorage.Clients.Imp
{
    public class MessageStorageClient : IMessageStorageClient
    {
        private readonly IMessageStorageRepositoryContext _messageStorageRepositoryContext;
        private readonly IHandlerManager _handlerManager;
        private readonly MessageStorageClientConfiguration _messageStorageClientConfiguration;

        public MessageStorageClient(IMessageStorageRepositoryContext messageStorageRepositoryContext, IHandlerManager handlerManager, MessageStorageClientConfiguration? messageStorageConfiguration = null)
        {
            _messageStorageRepositoryContext = messageStorageRepositoryContext ?? throw new ArgumentNullException(nameof(messageStorageRepositoryContext));
            _handlerManager = handlerManager ?? throw new ArgumentNullException(nameof(handlerManager));
            _messageStorageClientConfiguration = messageStorageConfiguration ?? new MessageStorageClientConfiguration();
        }

        private Tuple<Message, IEnumerable<Job>> PrepareMessageAndJobs(object payload, bool autoJobCreator)
        {
            var message = new Message(payload);
            var jobs = new List<Job>();
            if (autoJobCreator)
            {
                IEnumerable<string> availableHandlerNames = _handlerManager.GetAvailableHandlerNames(payload);

                jobs = availableHandlerNames.Select(handlerName => new Job(handlerName, message))
                                            .ToList();
            }

            return new Tuple<Message, IEnumerable<Job>>(message, jobs);
        }

        public Tuple<Message, IEnumerable<Job>> Add<T>(T payload)
        {
            return Add(payload, _messageStorageClientConfiguration.AutoJobCreation);
        }

        public Tuple<Message, IEnumerable<Job>> Add<T>(T payload, bool autoJobCreation)
        {
            IMessageStorageTransaction? transaction = null;
            if (!_messageStorageRepositoryContext.HasTransaction)
            {
                transaction = _messageStorageRepositoryContext.BeginTransaction(IsolationLevel.ReadCommitted);
            }

            try
            {
                using var jobRepository = _messageStorageRepositoryContext.GetJobRepository();
                using var messageRepository = _messageStorageRepositoryContext.GetMessageRepository();

                var result = PrepareMessageAndJobs(payload!, autoJobCreation);
                messageRepository.Add(result.Item1);
                foreach (Job job in result.Item2)
                {
                    jobRepository.Add(job);
                }

                transaction?.Commit();

                return result;
            }
            finally
            {
                transaction?.Dispose();
            }
        }

        public int GetJobCountByStatus(JobStatus jobStatus)
        {
            using IJobRepository jobRepository = _messageStorageRepositoryContext.GetJobRepository();
            int result = jobRepository.GetJobCount(jobStatus);
            return result;
        }

        public IMessageStorageTransaction UseTransaction(IDbTransaction dbTransaction)
        {
            IMessageStorageTransaction messageStorageTransaction = _messageStorageRepositoryContext.UseTransaction(dbTransaction);
            return messageStorageTransaction;
        }

        public IMessageStorageTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
             IMessageStorageTransaction messageStorageTransaction = _messageStorageRepositoryContext.BeginTransaction(isolationLevel);
            return messageStorageTransaction;
        }

        public void Dispose()
        {
            // TODO: This object not created by MessageStorageClient. 
            // _messageStorageRepositoryContext.Dispose();
        }
    }
}