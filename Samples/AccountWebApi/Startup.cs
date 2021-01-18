using System;
using System.Text.Json.Serialization;
using System.Threading;
using AccountWebApi.Controllers;
using AccountWebApi.EntityFrameworkSection;
using AccountWebApi.MessageStorageSection.AccountHandlers;
using AccountWebApi.SecondMessageStorageSection;
using MessageStorage;
using MessageStorage.AspNetCore;
using MessageStorage.Configurations;
using MessageStorage.DataAccessSection;
using MessageStorage.DI.Extension;
using MessageStorage.SqlServer.DI.Extension;
using MessageStorage.SqlServer.Migrations;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace AccountWebApi
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private const string ALLOWED_ORIGIN_POLICY = "AllowedOriginPolicy";
        public static string ConnectionStr { get; private set; }

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

            ConnectionStr = _configuration.GetConnectionString("SqlServerConnectionStr");

            // Step 1 (DbRepositoryConfiguration)
            var repositoryConfiguration = new MessageStorageRepositoryContextConfiguration(ConnectionStr);

            // Step 2 (Db Migration)
            IMessageStorageMigrationRunner messageStorageMigrationRunner = new SqlServerMessageStorageMigrationRunner();
            var tryCount = 1;
            DbMigrationTag:
            try
            {
                Console.WriteLine($"Db Migration attempt : {tryCount}");
                messageStorageMigrationRunner.MigrateUp(repositoryConfiguration);
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

            // Step 4 (Injection)
            services.AddMessageStorageHostedService();

            services.AddMessageStorage(messageStorage =>
                                       {
                                           messageStorage.WithClientConfiguration(new MessageStorageClientConfiguration())
                                                         .UseSqlServer(ConnectionStr)
                                                         .UseHandlers((handlerManager, provider) =>
                                                                      {
                                                                          handlerManager.TryAddHandler(new HandlerDescription<AccountEventHandler>
                                                                                                           (() => new AccountEventHandler()));

                                                                          handlerManager.TryAddHandler(new HandlerDescription<AccountCreatedEventHandler>
                                                                                                           (() =>
                                                                                                            {
                                                                                                                var x = provider.GetRequiredService<AccountDbContext>();
                                                                                                                return new AccountCreatedEventHandler(x);
                                                                                                            })
                                                                                                      );
                                                                      })
                                               ;
                                       })
                    .WithJobProcessor();

            services.AddMessageStorage<ISecondMessageStorageClient, SecondMessageStorageClient>
                     (messageStorage =>
                      {
                          messageStorage.WithClientConfiguration(new MessageStorageClientConfiguration())
                                        .UseSqlServer(ConnectionStr)
                                        .UseHandlers((handlerManager, provider) =>
                                                     {
                                                         handlerManager.TryAddHandler(new HandlerDescription<AccountEventHandler>
                                                                                          (() => new AccountEventHandler()));

                                                         handlerManager.TryAddHandler(new HandlerDescription<AccountCreatedEventHandler>
                                                                                          (() =>
                                                                                           {
                                                                                               var x = provider.GetRequiredService<AccountDbContext>();
                                                                                               return new AccountCreatedEventHandler(x);
                                                                                           })
                                                                                     );
                                                     })
                                        .Construct((context, manager, configuration) => new SecondMessageStorageClient(context, manager, configuration));
                      })
                    .WithJobProcessor(new JobProcessorConfiguration())
                ;

            services.AddDbContext<AccountDbContext>(builder => builder.UseSqlServer(ConnectionStr,
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