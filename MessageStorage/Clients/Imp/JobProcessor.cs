using System;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.Configurations;
using MessageStorage.DataAccessSection;
using MessageStorage.DataAccessSection.Repositories;
using MessageStorage.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MessageStorage.Clients.Imp
{
    public class JobProcessor : IJobProcessor
    {
        private readonly Func<IMessageStorageRepositoryContext> _repositoryContextFactory;
        private readonly IHandlerManager _handlerManager;
        private readonly JobProcessorConfiguration _jobProcessorConfiguration;
        private readonly ILogger<IJobProcessor> _logger;

        private Task _backgroundJob;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public JobProcessor(Func<IMessageStorageRepositoryContext> repositoryContextFactory, IHandlerManager handlerManager, ILogger<IJobProcessor>? logger = null, JobProcessorConfiguration? jobProcessorConfiguration = null)
        {
            _repositoryContextFactory = repositoryContextFactory ?? throw new ArgumentNullException(nameof(repositoryContextFactory));
            _handlerManager = handlerManager ?? throw new ArgumentNullException(nameof(handlerManager));
            _jobProcessorConfiguration = jobProcessorConfiguration ?? new JobProcessorConfiguration();
            _logger = logger ?? NullLogger<IJobProcessor>.Instance;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug($"{nameof(IJobProcessor)}.{nameof(StartAsync)} is called - {DateTime.UtcNow}");

            cancellationToken.ThrowIfCancellationRequested();

            _backgroundJob = Task.Run(() => StartBackgroundJob(_cancellationTokenSource.Token), _cancellationTokenSource.Token);

            _logger.LogDebug($"{nameof(IJobProcessor)}.{nameof(StartAsync)} is completed - {DateTime.UtcNow}");

            return Task.CompletedTask;
        }

        private void StartBackgroundJob(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TimeSpan sleep = _jobProcessorConfiguration.WaitWhenJobNotFound;
                try
                {
                    bool jobHandled = StartProcessAsync().GetAwaiter().GetResult();
                    if (jobHandled) sleep = _jobProcessorConfiguration.WaitAfterJobHandled;
                }
                catch (Exception exception)
                {
                    _logger.LogError($"{nameof(IJobProcessor)} has unexpected exception - {DateTime.UtcNow}", exception);
                }
                finally
                {
                    Thread.Sleep(sleep);
                }
            }
        }

        private async Task<bool> StartProcessAsync()
        {
            using var cancellationTokenSource = new CancellationTokenSource(_jobProcessorConfiguration.JobProcessDeadline);
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            using (IMessageStorageRepositoryContext messageStorageRepositoryContext = _repositoryContextFactory.Invoke())
            {
                bool jobHandled = await HandleNextJobAsync(messageStorageRepositoryContext, _handlerManager, cancellationToken);
                return jobHandled;
            }
        }

        private async Task<bool> HandleNextJobAsync(IMessageStorageRepositoryContext messageStorageRepositoryContext, IHandlerManager handlerManager, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"{nameof(IJobProcessor)}.{nameof(HandleNextJobAsync)} is called - {DateTime.UtcNow}");

            IJobRepository jobRepository = messageStorageRepositoryContext.GetJobRepository();
            Job? job = jobRepository.SetFirstWaitingJobToInProgress();

            string message = $"{nameof(IJobProcessor)}.{nameof(HandleNextJobAsync)} is completed - {DateTime.UtcNow}";

            if (job != null)
            {
                await HandleJobAsync(job, handlerManager, cancellationToken);
                jobRepository.UpdateJobStatus(job);

                message += $" - JobId : {job.Id}";
            }
            else
            {
                message += $"JobId: X";
            }

            _logger.LogDebug(message);

            return job != null;
        }

        private async Task HandleJobAsync(Job job, IHandlerManager handlerManager, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"{nameof(IJobProcessor)}.{nameof(HandleJobAsync)} is called - {DateTime.UtcNow} - [JobId: {job.Id}]");

            try
            {
                Handler handler = handlerManager.GetHandler(job.AssignedHandlerName);
                await handler.BaseHandleOperationAsync(job.Message.GetPayload(), cancellationToken);
                job.SetDone();
            }
            catch (Exception e)
            {
                job.SetFailed(e.ToString());
            }

            _logger.LogDebug($"{nameof(IJobProcessor)}.{nameof(HandleJobAsync)} is completed - {DateTime.UtcNow} - [JobId: {job.Id} | JobStatus: {job.JobStatus}]");
        }

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug($"{nameof(IJobProcessor)}.{nameof(StopAsync)} is called - {DateTime.UtcNow}");

            cancellationToken.ThrowIfCancellationRequested();

            _cancellationTokenSource.Cancel();
            TimeSpan timeout = _jobProcessorConfiguration.WaitWhenJobNotFound + _jobProcessorConfiguration.JobProcessDeadline;
            _backgroundJob.Wait(timeout);

            _logger.LogDebug($"{nameof(IJobProcessor)}.{nameof(StopAsync)} is completed - {DateTime.UtcNow}");

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _logger.LogDebug($"{nameof(IJobProcessor)}.{nameof(Dispose)} is called - {DateTime.UtcNow}");
        }
    }
}