using System;
using MessageStorage.AspNetCore;
using MessageStorage.DependencyInjection;
using MessageStorage.MessageHandlers.Options;
using MessageStorage.SqlServer.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using SampleWebApi_UseTransaction.BackgroundJobs;
using SampleWebApi_UseTransaction.DataAccess;
using SampleWebApi_UseTransaction.EmailService;

namespace SampleWebApi_UseTransaction
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "SampleWebApi", Version = "v1"}); });

            services.AddLogging(builder => builder.AddConsole());
            services.AddScoped<IEmailSender, ConsoleEmailSender>();

            services.AddSingleton<IConnectionFactory>(provider => new SqlServerConnectionFactory(provider.GetRequiredService<IConfiguration>()
                                                                                                         .GetConnectionString("SqlServerConnectionString")));

            services.AddForgetty(configurator =>
                     {
                         configurator.UseSqlServer(provider => provider.GetRequiredService<IConfiguration>()
                                                                       .GetConnectionString("SqlServerConnectionString"),
                                                   "use_transaction_schema");

                         configurator.Register<AccountCreated_SendWelcomeEmail>(metaData =>
                         {
                             metaData.UseRetry(5, TimeSpan.FromMinutes(5));
                             metaData.UseRescue(TimeSpan.FromMinutes(30));
                         });
                     })
                    .AddMessageStoragePrerequisiteExecutor()
                    .AddMessageStorageJobDispatcherHostedService(waitAfterJobNotHandled: TimeSpan.FromSeconds(10));
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