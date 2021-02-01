using Microsoft.Extensions.DependencyInjection;

namespace MessageStorage.AspNetCore
{
    public static class BackgroundProcessorHostedServiceDIExtensions
    {
        public static IServiceCollection AddMessageStorageHostedService(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddHostedService<BackgroundProcessorHostedService>();
            return serviceCollection;
        }
    }
}