using System;
using MessageStorage.BackgroundTasks;
using MessageStorage.Containers;
using Microsoft.Extensions.DependencyInjection;

namespace MessageStorage.DependencyInjection
{
    public static class DependencyInjectionExtension
    {
        public static IServiceCollection AddMessageStorage(this IServiceCollection serviceCollection,
                                                     Action<IMessageStorageDependencyConfigurator> configurationAction)
        {
            serviceCollection.AddScoped<IMonitorClient, MonitorClient>();
            serviceCollection.AddScoped<IMessageStorageClient, MessageStorageClient>();
            serviceCollection.AddScoped<IJobDispatcher, JobDispatcher>();
            serviceCollection.AddScoped<IJobRetrier, JobRetrier>();
            serviceCollection.AddScoped<IJobRescuer, JobRescuer>();

            serviceCollection.AddTransient<IMessageHandlerProvider, DependencyInjectionMessageHandlerProvider>();

            IMessageHandlerContainer messageHandlerContainer = new DependencyInjectionMessageHandlerContainer(serviceCollection);
            var dependencyConfigurator = new MessageStorageDependencyConfigurator(messageHandlerContainer, serviceCollection);
            configurationAction.Invoke(dependencyConfigurator);

            return serviceCollection;
        }
    }
}