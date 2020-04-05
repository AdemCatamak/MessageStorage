using MessageStorage.Db;
using MessageStorage.Db.MsSql.DI.Extension;
using MessageStorage.DI.Extension;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Samples.Db.WebApi.HostedServices;

namespace Samples.Db.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            MessageStorageDbConfiguration messageStorageDbConfiguration = MessageStorageDbConfigurationFactory.Create(AppConst.DbConnectionStr);
            services.AddMessageStorage(messageStorageServiceCollection =>
                                       {
                                           messageStorageServiceCollection.AddJobProcessServer()
                                                                          .AddMessageStorageDbClient(messageStorageDbConfiguration)
                                                                          .AddHandlers(new[] {typeof(Startup).Assembly});
                                       });
            
            services.AddHostedService<JobProcessHostedService>();
            
            // VERSION 2.0
            // services.AddMessageStorage(messageStorageServiceCollection =>
            //                            {
            //                                messageStorageServiceCollection.AddHandlers(new[] {GetType().Assembly})
            //                                                               .AddJobProcessServer()
            //                                                               .AddMessageStorageDbClient(new MyMessageStorageDbConfiguration());
            //                            });

            services.AddSwaggerGen(c =>
                                   {
                                       c.SwaggerDoc("v1", new OpenApiInfo
                                                          {
                                                              Title = $"",
                                                              Version = "v1"
                                                          });
                                   });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            app.UseSwagger();
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", $"v1"); });
            app.UseStaticFiles();
        }
    }
}