using System;
using MessageStorage.AspNetCore;
using MessageStorage.DependencyInjection;
using MessageStorage.Postgres.DependencyInjection;
using MessageStorage.SqlServer.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using SampleWebApi.BackgroundJobs;
using SampleWebApi.EmailService;

namespace SampleWebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "SampleWebApi", Version = "v1"}); });

            services.AddLogging(builder => builder.AddConsole());
            services.AddScoped<IEmailSender, ConsoleEmailSender>();

            var selectedDb = Configuration.GetValue<string>("SelectedDb");
            services.AddForgetty(configurator =>
                     {
                         switch (selectedDb)
                         {
                             case "SqlServer":
                                 configurator.UseSqlServer(provider => provider.GetRequiredService<IConfiguration>()
                                                                               .GetConnectionString("SqlServerConnectionString"));
                                 break;
                             case "Postgres":
                                 configurator.UsePostgres(provider => provider.GetRequiredService<IConfiguration>()
                                                                              .GetConnectionString("PostgresConnectionString"));
                                 break;
                             default:
                                 throw new ArgumentOutOfRangeException(nameof(selectedDb));
                         }

                         configurator.Register<AccountCreated_SendWelcomeEmail>();
                     })
                     // order is important
                    .AddMessageStoragePrerequisiteExecutor()
                    .AddMessageStorageJobDispatcherHostedService(waitAfterJobNotHandled: TimeSpan.FromSeconds(10), concurrentJobDispatchCount: 2);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SampleWebApi v1"));
            }

            app.UseRouting();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}