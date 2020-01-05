using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessageStorage.Exceptions;

namespace MessageStorage.MessageStorageClientSection
{
    public class MessageStorageClient : IMessageStorageClient
    {
        private readonly IMessageStorageAdaptor _messageStorageAdaptor;
        private readonly IHandlerFactory _handlerFactory;

        public MessageStorageClient(IMessageStorageAdaptor messageStorageAdaptor, IHandlerFactory handlerFactory)
        {
            _messageStorageAdaptor = messageStorageAdaptor;
            _handlerFactory = handlerFactory;
        }

        public Job SetFirstWaitingJobToInProgress()
        {
            return _messageStorageAdaptor.SetFirstWaitingJobToInProgress();
        }

        public void Add<T>(T payload, string traceId = null)
        {
            IEnumerable<string> availableHandlerNames = _handlerFactory.GetAvailableHandlers(payload);
            var message = new Message(payload, traceId);
            var jobs = availableHandlerNames.Select(handlerName => new Job(message, handlerName))
                                            .ToList();

            _messageStorageAdaptor.Add(message, jobs);
        }

        public Task Handle(Job job)
        {
            Handler handler = _handlerFactory.GetHandler(job.AssignedHandler);
            if (handler == null)
                throw new HandlerNotFoundException($"Could not found Handler as {job.AssignedHandler}");

            return handler.Handle(job.Message.Payload);
        }

        public void Update(Job job)
        {
            _messageStorageAdaptor.Update(job);
        }
    }
}