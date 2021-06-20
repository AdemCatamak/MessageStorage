using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MessageStorage.AspNetCore
{
    public static class MessageStorageHostedServiceDependencyExtension
    {
        public static IServiceCollection AddMessageStorageJobDispatcher(this IServiceCollection serviceCollection,
                                                                        TimeSpan? waitAfterJobHandled = null,
                                                                        TimeSpan? waitAfterJobNotHandled = null,
                                                                        int concurrentJobDispatchCount = 1,
                                                                        TimeSpan? retryJobExecutionPeriod = null,
                                                                        TimeSpan? rescueJobExecutionPeriod = null)
        {
            serviceCollection.AddHostedService(provider =>
                                                   new JobDispatcherHostedService(provider,
                                                                                  waitAfterJobHandled,
                                                                                  waitAfterJobNotHandled,
                                                                                  concurrentJobDispatchCount,
                                                                                  provider.GetService<ILogger<JobDispatcherHostedService>>()));

            serviceCollection.AddHostedService(provider =>
                                                   new JobRetrierHostedService(provider,
                                                                               retryJobExecutionPeriod,
                                                                               provider.GetService<ILogger<JobRetrierHostedService>>()));

            serviceCollection.AddHostedService(provider =>
                                                   new JobRescuerHostedService(provider,
                                                                               rescueJobExecutionPeriod,
                                                                               provider.GetService<ILogger<JobRescuerHostedService>>()));

            return serviceCollection;
        }
    }
}