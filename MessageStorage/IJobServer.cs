using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MessageStorage
{
    public interface IJobServer
    {
        Task StartAsync();
        Task StopAsync();
    }

    public class JobServer : IJobServer, IDisposable
    {
        private readonly IMessageStorageClient _messageStorageClient;
        private readonly ILogger<JobServer> _logger;
        private readonly JobServerConfiguration _jobServerConfiguration;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private Task _executionTask;

        public JobServer(IMessageStorageClient messageStorageClient, JobServerConfiguration jobServerConfiguration = null, ILogger<JobServer> logger = null)
        {
            _jobServerConfiguration = jobServerConfiguration ?? new JobServerConfiguration();
            _messageStorageClient = messageStorageClient ?? throw new ArgumentNullException(nameof(messageStorageClient));
            _logger = logger ?? NullLogger<JobServer>.Instance;

            _cancellationTokenSource = new CancellationTokenSource();
            if (_jobServerConfiguration.AutoStart)
                StartAsync();
        }

        public Task StartAsync()
        {
            _logger.LogInformation($"{nameof(JobServer)} is starting");
            _executionTask = new Task(Execute, _cancellationTokenSource.Token);
            _executionTask.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            _logger.LogInformation($"{nameof(JobServer)} is stopping");

            _cancellationTokenSource.Cancel();
            _executionTask.Dispose();
            return Task.CompletedTask;
        }

        private void Execute()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                _logger.LogDebug($"{nameof(JobServer)} => Execute {DateTime.UtcNow}");

                try
                {
                    Job job = _messageStorageClient.SetFirstWaitingJobToInProgress();
                    if (job == null)
                    {
                        Thread.Sleep(_jobServerConfiguration.WaitWhenMessageNotFound);
                        continue;
                    }

                    try
                    {
                        _messageStorageClient.Handle(job)
                                             .GetAwaiter().GetResult();
                        job.SetDone();
                    }
                    catch (Exception e)
                    {
                        job.SetFailed(e.ToString());
                    }

                    _messageStorageClient.Update(job);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Unexpected error");
                }
                
                Thread.Sleep(_jobServerConfiguration.WaitAfterMessageHandled);
            }
        }


        public void Dispose()
        {
            _cancellationTokenSource?.Dispose();
        }
    }

    public class JobServerConfiguration
    {
        public TimeSpan WaitWhenMessageNotFound { get; set; } = TimeSpan.FromSeconds(value: 5);
        public TimeSpan WaitAfterMessageHandled { get; set; } = TimeSpan.Zero;
        public bool AutoStart { get; set; } = false;
    }
}