using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MessageStorage
{
    public interface IJobProcessServer
    {
        Task StartAsync();
        Task StopAsync();
        void Execute();
    }

    public class JobProcessServer : IJobProcessServer, IDisposable
    {
        private readonly IMessageStorageClient _messageStorageClient;
        private readonly CancellationToken _cancellationToken;
        private readonly ILogger<JobProcessServer> _logger;
        private readonly JobServerConfiguration _jobServerConfiguration;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private Task _executionTask;

        public JobProcessServer(IMessageStorageClient messageStorageClient, JobServerConfiguration jobServerConfiguration = null, ILogger<JobProcessServer> logger = null, CancellationToken cancellationToken = default)
        {
            _jobServerConfiguration = jobServerConfiguration ?? new JobServerConfiguration();
            _messageStorageClient = messageStorageClient ?? throw new ArgumentNullException(nameof(messageStorageClient));
            _cancellationToken = cancellationToken;
            _logger = logger ?? NullLogger<JobProcessServer>.Instance;

            _cancellationTokenSource = new CancellationTokenSource();
            if (_jobServerConfiguration.AutoStart)
                StartAsync();
        }

        public Task StartAsync()
        {
            _logger.Log(LogLevel.Information, eventId: default, typeof(JobProcessServer), exception: default,
                        (type, exception) => $"{nameof(JobProcessServer)} is starting");
            _executionTask = new Task(ExecuteInfinite, _cancellationTokenSource.Token);
            _executionTask.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            _logger.Log(LogLevel.Information, eventId: default, typeof(JobProcessServer), exception: default,
                        (type, exception) => $"{nameof(JobProcessServer)} is stopping");

            _cancellationTokenSource.Cancel();
            _executionTask?.Dispose();
            return Task.CompletedTask;
        }

        private void ExecuteInfinite()
        {
            while (!_cancellationTokenSource.IsCancellationRequested && !_cancellationToken.IsCancellationRequested)
            {
                Execute();
            }
        }

        public void Execute()
        {
            _logger.Log(LogLevel.Debug, eventId: default, typeof(JobProcessServer), exception: default,
                        (type, exception) => $"{nameof(JobProcessServer)} => Execute {DateTime.UtcNow}");

            try
            {
                Job job = _messageStorageClient.SetFirstWaitingJobToInProgress();
                if (job == null)
                {
                    Thread.Sleep(_jobServerConfiguration.WaitWhenMessageNotFound);
                    return;
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
                _logger.Log(LogLevel.Error, eventId: default, typeof(JobProcessServer), exception: e,
                            (type, exception) => $"{nameof(JobProcessServer)} => Unexpected error");
            }

            Thread.Sleep(_jobServerConfiguration.WaitAfterMessageHandled);
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