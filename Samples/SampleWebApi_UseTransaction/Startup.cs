using MessageStorage.Extensions;
using MessageStorage.SqlServer.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using SampleWebApi_UseTransaction.BackgroundJobs;
using SampleWebApi_UseTransaction.DataAccess;
using SampleWebApi_UseTransaction.EmailService;

namespace SampleWebApi_UseTransaction;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "SampleWebApi", Version = "v1" }); });

        services.AddScoped<IEmailSender, ConsoleEmailSender>();

        services.AddSingleton<IConnectionFactory>(provider => new SqlServerConnectionFactory(provider.GetRequiredService<IConfiguration>()
                                                                                                     .GetConnectionString("SqlServerConnectionString")));

        services.AddMessageStorage(configurator =>
        {
            string? sqlConnectionStr = _configuration.GetConnectionString("SqlServerConnectionString");
            configurator.UseSqlServer(sqlConnectionStr, "use_transaction_schema");

            configurator.RegisterHandler<AccountCreated_SendWelcomeEmail, AccountCreated>(20);
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