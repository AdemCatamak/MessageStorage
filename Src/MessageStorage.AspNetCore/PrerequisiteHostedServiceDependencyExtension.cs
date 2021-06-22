using Microsoft.Extensions.DependencyInjection;

namespace MessageStorage.AspNetCore
{
    public static class PrerequisiteHostedServiceDependencyExtension
    {
        public static IServiceCollection AddMessageStoragePrerequisiteExecutor(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddHostedService<PrerequisiteExecutorHostedService>();
            return serviceCollection;
        }
    }
}