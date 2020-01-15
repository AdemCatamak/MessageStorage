using MessageStorage.Db;
using MessageStorage.Db.DI.Extension;
using MessageStorage.DI.Extension;
using MessageStorage.MsSql.WebApi.DbContextSection;
using MessageStorage.MsSql.WebApi.HostedServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace MessageStorage.MsSql.WebApi
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
            var connectionStr = "Server=localhost,1433;Database=TestDb;User=sa;Password=kHyjGp7JH5;Trusted_Connection=False;";

            services.AddDbContext<NoteDbContext>(builder => builder.UseSqlServer(connectionStr));

            MessageStorageDbConfiguration messageStorageDbConfiguration = MessageStorageDbConfigurationFactory.Create(connectionStr);
            services.AddMessageStorage(messageStorageServiceCollection =>
                                       {
                                           messageStorageServiceCollection.AddMessageProcessServer()
                                                                          .AddMsSqlMessageStorage(messageStorageDbConfiguration)
                                                                          .AddHandlers(new[] {typeof(Startup).Assembly});
                                       });

            services.AddHostedService<MessageStorageProcessService>();
            
            services.AddSwaggerGen(c =>
                                   {
                                       c.SwaggerDoc("v1", new OpenApiInfo
                                                          {
                                                              Title = $"",
                                                              Version = "v1"
                                                          });
                                   });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, NoteDbContext noteDbContext)
        {
            noteDbContext.Database.EnsureCreated();

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