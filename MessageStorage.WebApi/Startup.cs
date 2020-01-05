using System.Collections.Generic;
using MessageStorage.DI.Extension;
using MessageStorage.MessageStorageAdaptorSection;
using MessageStorage.WebApi.Handlers;
using MessageStorage.WebApi.HostedServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;

namespace MessageStorage.WebApi
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
            services.AddHostedService<MessageStorageProcessService>();

            // InMemoryStorage should inject singleton
            services.AddMessageStorageClient<InMemoryMessageStorageAdaptor>(ServiceLifetime.Singleton);
            services.AddHandlers(new[] {typeof(NoteCreatedEventHandler).Assembly});
            services.AddMessageProcessServer();

            services.AddSwaggerGen(c =>
                                   {
                                       c.SwaggerDoc("v1", new OpenApiInfo
                                                          {
                                                              Title = $"",
                                                              Version = "v1"
                                                          });
                                   });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            app.UseSwagger();
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", $"v1"); });
            app.UseStaticFiles();
        }
    }
}