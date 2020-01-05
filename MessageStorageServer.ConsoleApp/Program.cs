using System;
using MessageStorage.DI.Extension;
using MessageStorage.MessageStorageSection;
using MessageStorageServer.ConsoleApp.BackgroundServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MessageStorageServer.ConsoleApp
{
    class Program
    {
        private static IConfiguration _configuration;

        static void Main(string[] args)
        {
            using var host = CreateHost();
            host.Run();
        }

        public static IHost CreateHost()
        {
            return new HostBuilder()
                  .ConfigureAppConfiguration((context, builder) => { _configuration = builder.Build(); })
                  .ConfigureServices((hostContext, services) =>
                                     {
                                         services.Configure<HostOptions>(option => { option.ShutdownTimeout = TimeSpan.FromSeconds(20); });
                                         InjectDependencies(services);
                                     })
                  .ConfigureLogging((host, logging) =>
                                    {
                                        logging.SetMinimumLevel(LogLevel.Information);
                                        logging.ClearProviders();
                                        logging.AddConsole();
                                    })
                  .Build();
        }

        private static void InjectDependencies(IServiceCollection services)
        {
            services.AddHostedService<MessageStorageProcessService>();
           
            services.AddMessageStorageClient<InMemoryMessageStorage>();
            services.AddMessageProcessServer();
        }
    }
}