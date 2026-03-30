using TaskScheduler.Core.Interfaces;
using TaskScheduler.Infrastructure.Repositories;

var builder = Host.CreateApplicationBuilder(args);

//Register Services
var connectionString = builder.Configuration.GetConnectionString("MySQL")
    ?? throw new InvalidOperationException("MySQL coonection string is not configured");

builder.Services.AddSingleton<IJobRepository>(new JobRepository(connectionString));
builder.Services.AddHostedService<TaskScheduler.Worker.Worker>();

var host = builder.Build();
host.Run();