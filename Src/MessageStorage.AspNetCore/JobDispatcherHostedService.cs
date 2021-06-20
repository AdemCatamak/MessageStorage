using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.BackgroundTasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MessageStorage.AspNetCore
{
    public class JobDispatcherHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _waitAfterJobHandled;
        private readonly TimeSpan _waitAfterJobNotHandled;
        private readonly int _concurrentExecutionCount;
        private readonly ILogger<JobDispatcherHostedService> _logger;

        public JobDispatcherHostedService(IServiceProvider serviceProvider, TimeSpan? waitAfterJobHandled = null, TimeSpan? waitAfterJobNotHandled = null, int concurrentExecutionCount = 1, ILogger<JobDispatcherHostedService>? logger = null)
        {
            _serviceProvider = serviceProvider;
            _waitAfterJobHandled = waitAfterJobHandled ?? TimeSpan.FromMilliseconds(10);
            _waitAfterJobNotHandled = waitAfterJobNotHandled ?? TimeSpan.FromMilliseconds(3000);
            _concurrentExecutionCount = concurrentExecutionCount;
            _logger = logger ?? NullLogger<JobDispatcherHostedService>.Instance;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            List<Task> tasks = new List<Task>();
            for (var i = 0; i < _concurrentExecutionCount; i++)
            {
                Task task = Task.Run(async () => { await InfiniteBackgroundJob(stoppingToken); }, stoppingToken);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks.ToArray());
        }

        private async Task InfiniteBackgroundJob(CancellationToken cancellationToken)
        {
            try
            {
                do
                {
                    await BackgroundJob(cancellationToken);
                } while (!cancellationToken.IsCancellationRequested);
            }
            catch (TaskCanceledException)
            {
                // continue
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{OperationName} closed unexpectedly", nameof(JobDispatcherHostedService) + "." + nameof(InfiniteBackgroundJob));
            }
        }

        private async Task BackgroundJob(CancellationToken cancellationToken)
        {
            using IServiceScope scope = _serviceProvider.CreateScope();

            var jobProcessor = scope.ServiceProvider
                                    .GetRequiredService<IJobDispatcher>();

            _logger.LogInformation("{OperationName} is triggered : {OperationTime}", nameof(IJobDispatcher.HandleNextJobAsync), DateTime.UtcNow);
            var jobHandled = false;
            try
            {
                jobHandled = await jobProcessor.HandleNextJobAsync(cancellationToken);
                _logger.LogInformation("{OperationName} is completed : {OperationTime}", nameof(IJobDispatcher.HandleNextJobAsync), DateTime.UtcNow);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{OperationName} is failed : {OperationTime}", nameof(IJobDispatcher.HandleNextJobAsync), DateTime.UtcNow);
            }

            await (jobHandled
                       ? Task.Delay(_waitAfterJobHandled, cancellationToken)
                       : Task.Delay(_waitAfterJobNotHandled, cancellationToken));
        }
    }
}