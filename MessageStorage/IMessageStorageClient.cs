using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessageStorage.DataAccessSection;
using MessageStorage.DataAccessSection.Repositories;
using MessageStorage.Exceptions;

namespace MessageStorage
{
    public interface IMessageStorageClient
    {
        Task Handle(Job job);

        (Message, IEnumerable<Job>) Add<T>(T payload, string traceId = null, bool autoJobCreator = true);
        Job SetFirstWaitingJobToInProgress();
        void Update(Job job);
    }

    public abstract class MessageStorageClient : IMessageStorageClient
    {
        protected readonly IHandlerManager HandlerManager;
        protected readonly IRepositoryResolver RepositoryResolver;

        protected MessageStorageClient(IHandlerManager handlerManager, IRepositoryResolver repositoryResolver)
        {
            HandlerManager = handlerManager ?? throw new ArgumentNullException(nameof(handlerManager));
            RepositoryResolver = repositoryResolver;
        }

        public Task Handle(Job job)
        {
            Handler handler = HandlerManager.GetHandler(job.AssignedHandlerName);
            if (handler == null)
                throw new HandlerNotFoundException($"Could not found Handler as {job.AssignedHandlerName}");

            return handler.BaseHandleOperation(job.Message.Payload);
        }

        public (Message, IEnumerable<Job>) Add<T>(T payload, string traceId = null, bool autoJobCreator = true)
        {
            (Message message, IEnumerable<Job> jobs) = PrepareMessageAndJobs(payload, traceId, autoJobCreator);
            var messageRepository = RepositoryResolver.Resolve<IMessageRepository>();
            messageRepository.Add(message);
            List<Job> jobList = jobs.ToList();
            if (jobList.Any())
            {
                var jobRepository = RepositoryResolver.Resolve<IJobRepository>();
                foreach (Job job in jobList)
                {
                    jobRepository.Add(job);
                }
            }

            return (message, jobList);
        }

        public Job SetFirstWaitingJobToInProgress()
        {
            var jobRepository = RepositoryResolver.Resolve<IJobRepository>();
            Job job = jobRepository.SetFirstWaitingJobToInProgress();
            return job;
        }

        public void Update(Job job)
        {
            var jobRepository = RepositoryResolver.Resolve<IJobRepository>();
            jobRepository.Update(job);
        }

        protected Tuple<Message, IEnumerable<Job>> PrepareMessageAndJobs<T>(T payload, string traceId, bool autoJobCreator)
        {
            var message = new Message(payload, traceId);
            var jobs = new List<Job>();
            if (autoJobCreator)
            {
                IEnumerable<string> availableHandlerNames = HandlerManager.GetAvailableHandlerNames(payload);

                jobs = availableHandlerNames.Select(handlerName => new Job(message, handlerName))
                                            .ToList();
            }

            return new Tuple<Message, IEnumerable<Job>>(message, jobs);
        }
    }
}