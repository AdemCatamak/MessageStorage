using System.Collections.Generic;
using System.Data;
using System.Linq;
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
            IEnumerable<string> availableHandlerNames = _handlerFactory.GetAvailableHandlers(payload);
            var message = new Message(payload, traceId);
            var jobs = new List<Job>();
            if (autoJobCreator)
                jobs = availableHandlerNames.Select(handlerName => new Job(message, handlerName))
                                            .ToList();

            _dbStorageAdaptor.Add(message, jobs, dbTransaction);
        }
    }
}