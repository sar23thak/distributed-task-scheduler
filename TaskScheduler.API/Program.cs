using TaskScheduler.Infrastructure.Repositories;
using TaskScheduler.Core.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Services will be registered here
var connectionString = builder.Configuration.GetConnectionString("MySQL")!;
builder.Services.AddSingleton<IJobRepository>(new JobRepository(connectionString));

builder.Services.AddControllers();

var app = builder.Build();
app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();
// Middleware and routes will be configured here

app.Run();