using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessageStorage.Exceptions;

namespace MessageStorage.MessageStorageClientSection
{
    public class MessageStorageClient : IMessageStorageClient
    {
        private readonly IStorageAdaptor _storageAdaptor;
        private readonly IHandlerFactory _handlerFactory;

        public MessageStorageClient(IStorageAdaptor storageAdaptor, IHandlerFactory handlerFactory)
        {
            _storageAdaptor = storageAdaptor ?? throw new ArgumentNullException(nameof(storageAdaptor));
            _handlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory));
        }

        public Job SetFirstWaitingJobToInProgress()
        {
            return _storageAdaptor.SetFirstWaitingJobToInProgress();
        }

        public void Add<T>(T payload, string traceId = null, bool autoJobCreator = true)
        {
            (Message message, IEnumerable<Job> jobs) = PrepareMessageAndJobs(payload, traceId, autoJobCreator);

            _storageAdaptor.Add(message, jobs);
        }

        public Task Handle(Job job)
        {
            Handler handler = _handlerFactory.GetHandler(job.AssignedAssignedHandlerName);
            if (handler == null)
                throw new HandlerNotFoundException($"Could not found Handler as {job.AssignedAssignedHandlerName}");

            return handler.Handle(job.Message.Payload);
        }

        public void Update(Job job)
        {
            _storageAdaptor.Update(job);
        }

        
        protected Tuple<Message, IEnumerable<Job>> PrepareMessageAndJobs<T>(T payload, string traceId, bool autoJobCreator)
        {
            IEnumerable<string> availableHandlerNames = _handlerFactory.GetAvailableHandlers(payload);
            var message = new Message(payload, traceId);
            var jobs = new List<Job>();
            if (autoJobCreator)
                jobs = availableHandlerNames.Select(handlerName => new Job(message, handlerName))
                                            .ToList();

            return new Tuple<Message, IEnumerable<Job>>(message, jobs);
        }
    }
}