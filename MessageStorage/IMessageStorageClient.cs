using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessageStorage.Exceptions;

namespace MessageStorage
{
    public interface IMessageStorageClient
    {
        void Add<T>(T payload, string traceId = null, bool autoJobCreator = true);
        Job SetFirstWaitingJobToInProgress();
        Task Handle(Job job);
        void Update(Job job);
    }
    
    public class MessageStorageClient : IMessageStorageClient
    {
        private readonly IStorageAdaptor _storageAdaptor;
        private readonly IHandlerManager _handlerManager;

        public MessageStorageClient(IStorageAdaptor storageAdaptor, IHandlerManager handlerManager)
        {
            _storageAdaptor = storageAdaptor ?? throw new ArgumentNullException(nameof(storageAdaptor));
            _handlerManager = handlerManager ?? throw new ArgumentNullException(nameof(handlerManager));
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
            Handler handler = _handlerManager.GetHandler(job.AssignedHandlerName);
            if (handler == null)
                throw new HandlerNotFoundException($"Could not found Handler as {job.AssignedHandlerName}");

            return handler.Handle(job.Message.Payload);
        }

        public void Update(Job job)
        {
            _storageAdaptor.Update(job);
        }

        
        protected Tuple<Message, IEnumerable<Job>> PrepareMessageAndJobs<T>(T payload, string traceId, bool autoJobCreator)
        {
            IEnumerable<string> availableHandlerNames = _handlerManager.GetAvailableHandlers(payload);
            var message = new Message(payload, traceId);
            var jobs = new List<Job>();
            if (autoJobCreator)
                jobs = availableHandlerNames.Select(handlerName => new Job(message, handlerName))
                                            .ToList();

            return new Tuple<Message, IEnumerable<Job>>(message, jobs);
        }
    }
}