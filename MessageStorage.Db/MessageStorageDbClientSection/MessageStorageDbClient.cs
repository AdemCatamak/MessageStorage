using System;
using System.Collections.Generic;
using System.Data;
using MessageStorage.MessageStorageClientSection;

namespace MessageStorage.Db.MessageStorageDbClientSection
{
    public class MessageStorageDbClient : MessageStorageClient, IMessageStorageDbClient
    {
        private readonly IDbStorageAdaptor _dbStorageAdaptor;
        private readonly IHandlerFactory _handlerFactory;

        public MessageStorageDbClient(IDbStorageAdaptor dbStorageAdaptor, IHandlerFactory handlerFactory) : base(dbStorageAdaptor, handlerFactory)
        {
            _dbStorageAdaptor = dbStorageAdaptor;
            _handlerFactory = handlerFactory;
        }

        public void Add<T>(T payload, IDbTransaction dbTransaction, string traceId = null, bool autoJobCreator = true)
        {
            (Message message, IEnumerable<Job> jobs) = PrepareMessageAndJobs(payload, traceId, autoJobCreator);

            _dbStorageAdaptor.Add(message, jobs, dbTransaction);
        }
    }
}