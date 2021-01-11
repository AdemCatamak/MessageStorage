using AccountWebApi.EntityFrameworkSection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AccountWebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (var host = CreateHostBuilder(args).Build())
            {
                using (IServiceScope scope = host.Services.CreateScope())
                {
                    var sampleDbContext = scope.ServiceProvider.GetRequiredService<AccountDbContext>();
                    sampleDbContext.Database.Migrate();
                }

                host.Run();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                                           {
                                               config.AddJsonFile("appsettings.json");
                                               if (hostingContext.HostingEnvironment.IsDevelopment())
                                                   config.AddJsonFile("appsettings.dev.json");
                                           })
                .ConfigureWebHostDefaults(webBuilder =>
                                          {
                                              webBuilder.UseStartup<Startup>();
                                              webBuilder.UseUrls("http://*:80");
                                          });
    }
}