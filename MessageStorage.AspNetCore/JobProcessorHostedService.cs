using System.Threading;
using System.Threading.Tasks;
using MessageStorage.Clients;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MessageStorage.AspNetCore
{
    public class JobProcessorHostedService : JobProcessorHostedService<IJobProcessor>
    {
        public JobProcessorHostedService(IJobProcessor jobProcessor) : base(jobProcessor)
        {
        }
    }

    public class JobProcessorHostedService<TJobProcessor> : IHostedService
        where TJobProcessor : IJobProcessor
    {
        private readonly TJobProcessor _jobProcessor;

        public JobProcessorHostedService(TJobProcessor jobProcessor)
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