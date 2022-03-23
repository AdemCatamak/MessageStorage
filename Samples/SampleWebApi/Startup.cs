using System;
using MessageStorage.Extensions;
using MessageStorage.Postgres.Extensions;
using MessageStorage.SqlServer.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using SampleWebApi.BackgroundJobs;
using SampleWebApi.EmailService;

namespace SampleWebApi;

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
        services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "SampleWebApi", Version = "v1" }); });

        services.AddLogging(builder => builder.AddConsole());
        services.AddScoped<IEmailSender, ConsoleEmailSender>();

        var selectedDb = Configuration.GetValue<string>("SelectedDb");
        services.AddMessageStorage(configurator =>
        {
            switch (selectedDb)
            {
                case "SqlServer":
                    var sqlServerConnectionStr = Configuration.GetConnectionString("SqlServerConnectionString");
                    configurator.UseSqlServer(sqlServerConnectionStr);
                    break;
                case "Postgres":
                    var postgresConnectionStr = Configuration.GetConnectionString("PostgresConnectionString");
                    configurator.UsePostgres(postgresConnectionStr);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(selectedDb));
            }

            configurator.RegisterHandler<AccountCreated_SendWelcomeEmail, AccountCreated>();
        });
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