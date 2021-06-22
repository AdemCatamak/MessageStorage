using System;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.Containers;
using MessageStorage.DataAccessLayer;
using MessageStorage.DataAccessLayer.Repositories;
using MessageStorage.MessageHandlers;

namespace MessageStorage.BackgroundTasks
{
    public class JobDispatcher : IJobDispatcher
    {
        private readonly IMessageHandlerProvider _messageHandlerProvider;
        private readonly IRepositoryFactory _repositoryFactory;

        public JobDispatcher(IMessageHandlerProvider messageHandlerProvider, IRepositoryFactory repositoryFactory)
        {
            _messageHandlerProvider = messageHandlerProvider;
            _repositoryFactory = repositoryFactory;
        }

        public async Task<bool> HandleNextJobAsync(CancellationToken cancellationToken = default)
        {
            IJobRepository jobRepository = _repositoryFactory.CreateJobRepository();
            Job? job = await jobRepository.SetFirstQueuedJobToInProgressAsync(cancellationToken);

            if (job == null) return false;

            try
            {
                using IMessageHandler messageHandler = _messageHandlerProvider.Create(job.MessageHandlerTypeName);
                await messageHandler.BaseHandleOperationAsync(job.Message.Payload, cancellationToken);
                job.SetCompleted();
            }
            catch (Exception e)
            {
                job.SetFailed(e.ToString());
            }

            await jobRepository.UpdateJobStatusAsync(job, cancellationToken);

            return true;
        }
    }
}