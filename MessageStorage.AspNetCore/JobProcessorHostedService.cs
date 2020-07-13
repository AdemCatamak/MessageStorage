﻿using System;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.Clients;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MessageStorage.AspNetCore
{
    public class JobProcessorHostedService : IHostedService
    {
        private readonly IJobProcessor _jobProcessor;

        public JobProcessorHostedService(IJobProcessor jobProcessor)
        {
            _jobProcessor = jobProcessor;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _jobProcessor.StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _jobProcessor.StopAsync(cancellationToken);
        }
    }

    public static class JobProcessorHostedServiceExtension
    {
        public static IServiceCollection AddJobProcessorHostedService(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddHostedService<JobProcessorHostedService>();
            return serviceCollection;
        }
    }
}