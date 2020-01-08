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
            string connectionStr = "";

            services.AddDbContext<NoteDbContext>(builder => builder.UseSqlServer(connectionStr));

            var messageStorageDbConfiguration = MessageStorageDbConfigurationFactory.Create(connectionStr);
            services.AddMessageStorage(builder =>
                                       {
                                           builder.AddMessageProcessServer()
                                                  .AddMsSqlMessageStorage(messageStorageDbConfiguration);
                                       });
            services.AddHandlers(new[] {typeof(Startup).Assembly});

            services.AddHostedService<MessageStorageProcessService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}