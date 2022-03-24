using FooManagement.DataAccessLayer;
using FooManagement.MessageHandlers;
using FooManagement.SecondaryMessageStorageSection;
using MassTransit;
using MessageStorage.Extensions;
using MessageStorage.Integration.MassTransit;
using MessageStorage.Postgres.Extensions;
using MessageStorage.SqlServer.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(typeof(Program).Assembly);
    x.UsingInMemory((context, cfg) => { cfg.ConfigureEndpoints(context); });
});

builder.Services.AddSingleton<ISqlConnectionFactory>(provider => new SqlConnectionFactory(provider.GetRequiredService<IConfiguration>().GetConnectionString("SqlServer")));
builder.Services.AddSingleton<IPostgresConnectionFactory>(provider => new PostgresConnectionFactory(provider.GetRequiredService<IConfiguration>().GetConnectionString("Postgres")));

builder.Services.AddMessageStorage(options =>
                                   {
                                       options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
                                       options.RegisterHandler<LogFooCreatedMessageHandler, FooCreatedEvent>();

                                       options.JobRescuerInterval = TimeSpan.FromMinutes(5);
                                       options.JobRetrierInterval = TimeSpan.FromMinutes(5);
                                       options.JobQueueLength = 1000;
                                       options.JobHandlingConcurrency = 4;
                                       options.RetryJobFetchCount = 20;
                                       options.WaitAfterFullFetch = TimeSpan.FromSeconds(5);

                                       options.RegisterMassTransitMessageHandlers();
                                   }
                                  );
builder.Services.AddMessageStorage<ISecondaryMessageStorageClient, SecondaryMessageStorageClient>(options =>
{
    options.UsePostgres(builder.Configuration.GetConnectionString("Postgres"));
    options.RegisterHandlers(typeof(Program).Assembly);
});

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();