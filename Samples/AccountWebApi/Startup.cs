using System;
using System.Text.Json.Serialization;
using System.Threading;
using AccountWebApi.AccountApiMessageStorageSection.AccountHandlers;
using AccountWebApi.Controllers;
using AccountWebApi.EntityFrameworkSection;
using MessageStorage;
using MessageStorage.AspNetCore;
using MessageStorage.Clients;
using MessageStorage.Db.Configurations;
using MessageStorage.Db.DbMigrationRunners;
using MessageStorage.Db.SqlServer;
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
                    .AddJsonOptions(options => 
                                        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
                    .AddApplicationPart(typeof(HomeController).Assembly);

            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo()); });

            string connectionStr = _configuration.GetConnectionString("SqlServerConnectionStr");

            // Step 1 (DbRepositoryConfiguration)
            var dbRepositoryConfiguration = new DbRepositoryConfiguration(connectionStr);

            // Step 2 (Db Migration)
            IMessageStorageDbMigrationRunner messageStorageDbMigrationRunner = new SqlServerMessageStorageDbMigrationRunner();
            var tryCount = 1;
            DbMigrationTag:
            try
            {
                Console.WriteLine($"Db Migration attempt : {tryCount}");
                messageStorageDbMigrationRunner.MigrateUp(dbRepositoryConfiguration);
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


            // Step 3 (Handlers & HandlerManager)
            services.AddSingleton<Handler, AccountEventHandler>();
            services.AddSingleton<Handler, AccountCreatedEventHandler>();

            // Step 4 (Injection)
            services.AddJobProcessorHostedService()
                    .AddMessageStorage(collection =>
                                       {
                                           collection.AddMessageStorageSqlServerClient(dbRepositoryConfiguration, provider => provider.GetServices<Handler>());
                                           collection.AddSqlServerJobProcessor(dbRepositoryConfiguration, provider => provider.GetServices<Handler>(), provider => provider.GetRequiredService<ILogger<IJobProcessor>>());
                                           collection.AddMessageStorageSqlServerMonitor(dbRepositoryConfiguration);
                                       });


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