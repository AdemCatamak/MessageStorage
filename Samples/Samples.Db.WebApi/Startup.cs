using MessageStorage.Db.MsSql.DI.Extension;
using MessageStorage.DI.Extension;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Samples.Db.WebApi.HostedServices;
using Samples.Db.WebApi.MessageStorageSection;

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
            services.AddMessageStorage(messageStorageServiceCollection =>
                                       {
                                           messageStorageServiceCollection.AddHandlers(new[] {GetType().Assembly})
                                                                          .AddJobProcessServer()
                                                                          .AddMessageStorageDbClient(new MyMessageStorageDbConfiguration());
                                       });
            services.AddHostedService<JobProcessHostedService>();

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