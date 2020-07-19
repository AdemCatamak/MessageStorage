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
        private readonly Func<IRepositoryContext> _repositoryContextFactory;
        private readonly IHandlerManager _handlerManager;
        private readonly JobProcessorConfiguration _jobProcessorConfiguration;
        private readonly ILogger<IJobProcessor> _logger;

        private readonly CancellationTokenSource _cancellationTokenSource;

        private Task _executionTask;

        public JobProcessor(Func<IRepositoryContext> repositoryContextFactory, IHandlerManager handlerManager, ILogger<IJobProcessor> logger, JobProcessorConfiguration jobProcessorConfiguration = null)
        {
            _repositoryContextFactory = repositoryContextFactory ?? throw new ArgumentNullException(nameof(repositoryContextFactory));
            _handlerManager = handlerManager ?? throw new ArgumentNullException(nameof(handlerManager));
            _jobProcessorConfiguration = jobProcessorConfiguration ?? new JobProcessorConfiguration();
            _logger = logger ?? NullLogger<IJobProcessor>.Instance;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            LogDebug($"{DateTime.UtcNow} - {nameof(IJobProcessor)}.{nameof(StartAsync)} is called");

            cancellationToken.ThrowIfCancellationRequested();

            _executionTask = new Task(async () => await ExecuteInfinite(_cancellationTokenSource.Token));
            _executionTask.Start();

            LogDebug($"{DateTime.UtcNow} - {nameof(IJobProcessor)}.{nameof(StartAsync)} is completed");

            return Task.CompletedTask;
        }

        private async Task ExecuteInfinite(CancellationToken cancellationToken)
        {
            LogDebug($"{DateTime.UtcNow} - {nameof(IJobProcessor)}.{nameof(ExecuteInfinite)} is called");

            while (!cancellationToken.IsCancellationRequested)
            {
                await ExecuteAsync();
            }

            LogDebug($"{DateTime.UtcNow} - {nameof(IJobProcessor)}.{nameof(ExecuteInfinite)} is stopped");
        }

        public async Task ExecuteAsync()
        {
            LogDebug($"{DateTime.UtcNow} - {nameof(IJobProcessor)}.{nameof(ExecuteAsync)} is called");

            using (IRepositoryContext repositoryContext = _repositoryContextFactory.Invoke())
            {
                IJobRepository jobRepository = repositoryContext.JobRepository;
                try
                {
                    Job job = jobRepository.SetFirstWaitingJobToInProgress();
                    if (job == null)
                    {
                        _logger.LogDebug($"{DateTime.UtcNow} - {nameof(IJobProcessor)}.{nameof(ExecuteAsync)} is finished [Job not found]");
                        Thread.Sleep(_jobProcessorConfiguration.WaitWhenMessageNotFound);
                        return;
                    }

                    await HandleJob(job, jobRepository);
                }
                catch (Exception e)
                {
                    LogError(e);
                }

                LogDebug($"{DateTime.UtcNow} - {nameof(IJobProcessor)}.{nameof(ExecuteAsync)} is finished");
                Thread.Sleep(_jobProcessorConfiguration.WaitAfterMessageHandled);
            }
        }

        private async Task HandleJob(Job job, IJobRepository jobRepository)
        {
            LogDebug($"{DateTime.UtcNow} - {nameof(IJobProcessor)}.{nameof(HandleJob)} is called [JobId: {job.Id}]");

            try
            {
                Handler handler = _handlerManager.GetHandler(job.AssignedHandlerName);
                await handler.BaseHandleOperation(job.Message.GetPayload());
                job.SetDone();
            }
            catch (Exception e)
            {
                job.SetFailed(e.ToString());
            }

            jobRepository.Update(job);

            LogInfo($"{DateTime.UtcNow} - {nameof(IJobProcessor)}.{nameof(HandleJob)} is completed [JobId: {job.Id}, Status: {job.JobStatus}]");
        }

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            LogDebug($"{DateTime.UtcNow} - {nameof(IJobProcessor)}.{nameof(StopAsync)} is called");
            cancellationToken.ThrowIfCancellationRequested();
            _cancellationTokenSource.Cancel();

            bool completed = _executionTask?.Wait(_jobProcessorConfiguration.StopTaskTimeout) ?? true;
            if (!completed)
            {
                _cancellationTokenSource.Cancel(throwOnFirstException: true);
            }

            LogDebug($"{DateTime.UtcNow} - {nameof(IJobProcessor)}.{nameof(StopAsync)} is completed");
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            LogDebug($"{DateTime.UtcNow} - {nameof(IJobProcessor)}.{nameof(Dispose)} is called");
            _cancellationTokenSource?.Dispose();
        }

        private void LogDebug(string message)
        {
            _logger.Log(LogLevel.Debug, eventId: default, typeof(IJobProcessor), exception: null,
                        (type, exception) => $"{nameof(IJobProcessor)} => {message}");
        }

        private void LogInfo(string message)
        {
            _logger.Log(LogLevel.Information, eventId: default, typeof(IJobProcessor), exception: null,
                        (type, exception) => $"{nameof(IJobProcessor)} => {message}");
        }

        private void LogError(Exception e)
        {
            _logger.Log(LogLevel.Error, eventId: default, typeof(IJobProcessor), e,
                        (type, exception) => $"{nameof(IJobProcessor)} => Unexpected error");
        }
    }
}