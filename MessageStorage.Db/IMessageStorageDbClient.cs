using System;
using System.Collections.Generic;
using System.Data;

namespace MessageStorage.Db
{
    public interface IMessageStorageDbClient : IMessageStorageClient
    {
        void Add<T>(T payload, IDbTransaction dbTransaction, string traceId = null, bool autoJobCreator = true);
    }

    public class MessageStorageDbClient : MessageStorageClient, IMessageStorageDbClient
    {
        private readonly IDbStorageAdaptor _dbStorageAdaptor;

        public MessageStorageDbClient(IDbStorageAdaptor dbStorageAdaptor, IHandlerManager handlerManager) : base(dbStorageAdaptor, handlerManager)
        {
            _dbStorageAdaptor = dbStorageAdaptor;
        }

        public void Add<T>(T payload, IDbTransaction dbTransaction, string traceId = null, bool autoJobCreator = true)
        {
            (Message message, IEnumerable<Job> jobs) = PrepareMessageAndJobs(payload, traceId, autoJobCreator);

            _dbStorageAdaptor.Add(message, jobs, dbTransaction);
        }
    }
}