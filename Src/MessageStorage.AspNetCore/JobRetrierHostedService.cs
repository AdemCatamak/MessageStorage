using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.BackgroundTasks;
using MessageStorage.BackgroundTasks.Options;
using MessageStorage.MessageHandlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MessageStorage.AspNetCore
{
    public class JobRetrierHostedService : BackgroundService
    {
        private readonly TimeSpan _executionPeriod;

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<JobRetrierHostedService> _logger;

        public JobRetrierHostedService(IServiceProvider serviceProvider, TimeSpan? executionPeriod = null, ILogger<JobRetrierHostedService>? logger = null)
        {
            _serviceProvider = serviceProvider;
            _executionPeriod = executionPeriod ?? TimeSpan.FromSeconds(180);
            _logger = logger ?? NullLogger<JobRetrierHostedService>.Instance;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await BackgroundJob(stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{OperationName} faced unexpected error", $"{nameof(JobRetrierHostedService)}.{nameof(BackgroundJob)}");
            }

            await Task.Delay(_executionPeriod, stoppingToken);
        }

        private async Task BackgroundJob(CancellationToken cancellationToken)
        {
            using IServiceScope scope = _serviceProvider.CreateScope();

            var jobRetrier = scope.ServiceProvider
                                  .GetRequiredService<IJobRetrier>();

            List<RetryOption> retryOptions = scope.ServiceProvider
                                                  .GetServices<MessageHandlerMetadata>()
                                                  .Where(metaData => metaData.RetryOption != null)
                                                  .Select(metaData => metaData.RetryOption)
                                                  .ToList()!;

            List<Task> operationList = new List<Task>();
            foreach (var retryOption in retryOptions)
            {
                Task retryAsync = jobRetrier.RetryAsync(retryOption, cancellationToken);
                operationList.Add(retryAsync);
            }

            await Task.WhenAll(operationList);
        }
    }
}