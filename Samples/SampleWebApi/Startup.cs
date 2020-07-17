using System;
using System.Collections.Generic;
using System.Threading;
using MessageStorage;
using MessageStorage.AspNetCore;
using MessageStorage.Clients;
using MessageStorage.Clients.Imp;
using MessageStorage.Db.Clients;
using MessageStorage.Db.DataAccessSection;
using MessageStorage.Db.DbMigrationRunners;
using MessageStorage.Db.SqlServer;
using MessageStorage.Db.SqlServer.DataAccessSection;
using MessageStorage.Db.SqlServer.DI.Extension;
using MessageStorage.DI.Extension;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using SampleWebApi.Controllers;
using SampleWebApi.EntityFrameworkSection;
using SampleWebApi.WebApiMessageStorageSection;
using SampleWebApi.WebApiMessageStorageSection.SampleHandlers;
using JobProcessorHostedService = SampleWebApi.WebApiMessageStorageSection.JobProcessorHostedService;

namespace SampleWebApi
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private const string ALLOWED_ORIGIN_POLICY = "AllowedOriginPolicy";


        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
                             {
                                 options.AddPolicy(ALLOWED_ORIGIN_POLICY,
                                                   builder =>
                                                   {
                                                       builder.AllowAnyHeader()
                                                              .AllowAnyMethod()
                                                              .AllowAnyOrigin();
                                                   });
                             });

            services.AddControllers()
                    .AddApplicationPart(typeof(SampleController).Assembly);

            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo()); });

            string connectionStr = _configuration.GetConnectionString("SqlServerConnectionStr");

            // Step 1 (DbRepositoryConfiguration)
            var webApiSqlServerDbRepositoryConfiguration = new WebApiSqlServerDbRepositoryConfiguration(connectionStr);

            // Step 2 (Db Migration)
            IMessageStorageDbMigrationRunner messageStorageDbMigrationRunner = new SqlServerMessageStorageDbMigrationRunner();
            var tryCount = 1;
            DbMigrationTag:
            try
            {
                Console.WriteLine($"Db Migration attempt : {tryCount}");
                messageStorageDbMigrationRunner.MigrateUp(webApiSqlServerDbRepositoryConfiguration);
            }
            catch (Exception)
            {
                tryCount++;
                if (tryCount < 4)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(value: 10));
                    goto DbMigrationTag;
                }
            }


            #region Injection

            // Step 3 (Handlers & HandlerManager)
            var handlers = new List<Handler>
                           {
                               new SampleMessageHandler(),
                               new SampleCreatedMessageHandler()
                           };
            IHandlerManager handlerManager = new HandlerManager(handlers);

            bool withoutExtensions = false;
            if (withoutExtensions)
            {
                // Step 4 (DbConnectionFactory)
                ISqlServerDbConnectionFactory sqlServerDbConnectionFactory = new SqlServerDbConnectionFactory();

                // Step 5 (DbRepositoryContext)
                services.AddScoped<IDbRepositoryContext<WebApiSqlServerDbRepositoryConfiguration>>(provider => new SqlServerDbRepositoryContext<WebApiSqlServerDbRepositoryConfiguration>(webApiSqlServerDbRepositoryConfiguration, sqlServerDbConnectionFactory));

                // Step 6 (MessageStorageDbClient)
                services.AddScoped<IMessageStorageDbClient>(provider => new WebApiDbMessageStorageClient(handlerManager, provider.GetRequiredService<IDbRepositoryContext<WebApiSqlServerDbRepositoryConfiguration>>()));

                // Step 7 (JobProcessor)
                services.AddSingleton<IJobProcessor>(provider => new JobProcessor<WebApiSqlServerDbRepositoryConfiguration>(provider.GetRequiredService<IDbRepositoryContext<WebApiSqlServerDbRepositoryConfiguration>>(), handlerManager, provider.GetRequiredService<ILogger<IJobProcessor>>()));

                // Step 8 (JobProcessorHostedService)
                services.AddHostedService<JobProcessorHostedService>();
            }
            else
            {
                services.AddJobProcessorHostedService()
                        .AddMessageStorage(collection =>
                                           {
                                               // collection.AddMessageStorageDbClient(webapiSqlServerDbRepositoryConfiguration, handlerManager);
                                               collection.AddMessageStorageSqlServerClient(webApiSqlServerDbRepositoryConfiguration, handlers);
                                               collection.AddSqlServerJobProcessor(webApiSqlServerDbRepositoryConfiguration, handlers);
                                           });
            }

            #endregion

            services.AddDbContext<SampleDbContext>(builder => builder.UseSqlServer(connectionStr,
                                                                                   optionsBuilder => optionsBuilder.MigrationsAssembly(typeof(SampleDbContext).Assembly.FullName)));
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseStaticFiles();
            app.UseSwagger();
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", ""); });

            app.UseRouting();
            app.UseCors(ALLOWED_ORIGIN_POLICY);
            app.UseEndpoints(builder => { builder.MapControllers(); });
        }
    }
}