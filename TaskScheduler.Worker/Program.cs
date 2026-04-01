using TaskScheduler.Core.Interfaces;
using TaskScheduler.Infrastructure.Repositories;
using TaskScheduler.Infrastructure.Services;

var builder = Host.CreateApplicationBuilder(args);

var mySqlConnection = builder.Configuration.GetConnectionString("MySQL")
    ?? throw new InvalidOperationException("MySQL connection string is not configured.");

var redisConnection = builder.Configuration.GetConnectionString("Redis")
    ?? throw new InvalidOperationException("Redis connection string is not configured.");

builder.Services.AddSingleton<IJobRepository>(new JobRepository(mySqlConnection));
builder.Services.AddSingleton(new RedisDistributedLockService(redisConnection));
builder.Services.AddSingleton(new AdaptiveRateLimiter());

builder.Services.AddHostedService<TaskScheduler.Worker.Worker>();

var host = builder.Build();
host.Run();