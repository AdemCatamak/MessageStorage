using MessageStorage.Clients;
using Microsoft.Extensions.DependencyInjection;

namespace MessageStorage.AspNetCore
{
    public static class JobProcessorHostedServiceExtension
    {
        public static IServiceCollection AddJobProcessorHostedService(this IServiceCollection serviceCollection)
        {
            return AddJobProcessorHostedService<IJobProcessor>(serviceCollection);
        }

        public static IServiceCollection AddJobProcessorHostedService<TJobProcessor>(this IServiceCollection serviceCollection)
            where TJobProcessor : IJobProcessor
        {
            serviceCollection.AddHostedService<JobProcessorHostedService<TJobProcessor>>();
            return serviceCollection;
        }
    }
}