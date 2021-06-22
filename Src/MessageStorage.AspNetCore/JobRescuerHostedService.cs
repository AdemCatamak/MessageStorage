using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.BackgroundTasks;
using MessageStorage.MessageHandlers;
using MessageStorage.MessageHandlers.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MessageStorage.AspNetCore
{
    public class JobRescuerHostedService : BackgroundService
    {
        private readonly TimeSpan _executionPeriod;

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<JobRescuerHostedService> _logger;

        public JobRescuerHostedService(IServiceProvider serviceProvider, TimeSpan? executionPeriod = null, ILogger<JobRescuerHostedService>? logger = null)
        {
            _serviceProvider = serviceProvider;
            _executionPeriod = executionPeriod ?? TimeSpan.FromSeconds(180);
            _logger = logger ?? NullLogger<JobRescuerHostedService>.Instance;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await BackgroundJob(stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{OperationName} faced unexpected error", $"{nameof(JobRescuerHostedService)}.{nameof(BackgroundJob)}");
            }

            await Task.Delay(_executionPeriod, stoppingToken);
        }

        private async Task BackgroundJob(CancellationToken cancellationToken)
        {
            using IServiceScope scope = _serviceProvider.CreateScope();

            var jobRescuer = scope.ServiceProvider
                                  .GetRequiredService<IJobRescuer>();

            List<RescueOption> rescueOptions = scope.ServiceProvider
                                                    .GetServices<MessageHandlerMetadata>()
                                                    .Select(metaData => metaData.RescueOption)
                                                    .ToList()!;

            List<Task> operationList = new List<Task>();
            foreach (var rescueOption in rescueOptions)
            {
                Task rescueAsync = jobRescuer.RescueAsync(rescueOption, cancellationToken);
                operationList.Add(rescueAsync);
            }

            await Task.WhenAll(operationList);
        }
    }
}