using System;
using System.Threading;
using System.Threading.Tasks;

namespace MessageStorage.JobServerSection
{
    public class JobServer : IJobServer, IDisposable
    {
        private readonly IMessageStorageClient _messageStorageClient;
        private readonly JobServerConfiguration _jobServerConfiguration;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public JobServer(IMessageStorageClient messageStorageClient, JobServerConfiguration jobServerConfiguration = null)
        {
            _jobServerConfiguration = jobServerConfiguration ?? new JobServerConfiguration();
            _messageStorageClient = messageStorageClient ?? throw new ArgumentNullException(nameof(messageStorageClient));

            _cancellationTokenSource = new CancellationTokenSource();
            if (_jobServerConfiguration.AutoStart)
                StartAsync();
        }

        public Task StartAsync()
        {
            var task = new Task(Execute, _cancellationTokenSource.Token);
            task.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            _cancellationTokenSource.Cancel();
            return Task.CompletedTask;
        }

        private void Execute()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
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
        }


        public void Dispose()
        {
            _cancellationTokenSource?.Dispose();
        }
    }

    public class JobServerConfiguration
    {
        public TimeSpan WaitWhenMessageNotFound { get; set; } = TimeSpan.FromSeconds(5);
        public bool AutoStart { get; set; } = false;
    }
}