using System;
using System.Collections.Generic;
using System.Threading;
using AccountWebApi.AccountApiMessageStorageSection;
using AccountWebApi.AccountApiMessageStorageSection.AccountHandlers;
using AccountWebApi.Controllers;
using AccountWebApi.EntityFrameworkSection;
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

namespace AccountWebApi
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
                    .AddApplicationPart(typeof(AccountController).Assembly);

            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo()); });

            string connectionStr = _configuration.GetConnectionString("SqlServerConnectionStr");

            // Step 1 (DbRepositoryConfiguration)
            var webApiSqlServerDbRepositoryConfiguration = new AccountApiSqlServerDbRepositoryConfiguration(connectionStr);

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
                               new AccountEventHandler(),
                               new AccountCreatedEventHandler()
                           };
            IHandlerManager handlerManager = new HandlerManager(handlers);

            bool withoutExtensions = false;
            if (withoutExtensions)
            {
                // Step 4 (DbConnectionFactory)
                ISqlServerDbConnectionFactory sqlServerDbConnectionFactory = new SqlServerDbConnectionFactory();

                // Step 5 (DbRepositoryContext)
                services.AddScoped<IDbRepositoryContext<AccountApiSqlServerDbRepositoryConfiguration>>(provider => new SqlServerDbRepositoryContext<AccountApiSqlServerDbRepositoryConfiguration>(webApiSqlServerDbRepositoryConfiguration, sqlServerDbConnectionFactory));

                // Step 6 (MessageStorageDbClient)
                services.AddScoped<IMessageStorageDbClient>(provider => new AccountApiDbMessageStorageClient(handlerManager, provider.GetRequiredService<IDbRepositoryContext<AccountApiSqlServerDbRepositoryConfiguration>>()));

                // Step 7 (JobProcessor)
                services.AddSingleton<IJobProcessor>(provider => new JobProcessor<AccountApiSqlServerDbRepositoryConfiguration>(provider.GetRequiredService<IDbRepositoryContext<AccountApiSqlServerDbRepositoryConfiguration>>, handlerManager, provider.GetRequiredService<ILogger<IJobProcessor>>()));

                // Step 8 (JobProcessorHostedService)
                services.AddHostedService<AccountApiJobProcessorHostedService>();
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

            services.AddDbContext<AccountDbContext>(builder => builder.UseSqlServer(connectionStr,
                                                                                    optionsBuilder => optionsBuilder.MigrationsAssembly(typeof(AccountDbContext).Assembly.FullName)));
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