# Stage 1 - Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY TaskScheduler.Core/TaskScheduler.Core.csproj TaskScheduler.Core/
COPY TaskScheduler.Infrastructure/TaskScheduler.Infrastructure.csproj TaskScheduler.Infrastructure/
COPY TaskScheduler.Worker/TaskScheduler.Worker.csproj TaskScheduler.Worker/

RUN dotnet restore TaskScheduler.Worker/TaskScheduler.Worker.csproj

COPY TaskScheduler.Core/ TaskScheduler.Core/
COPY TaskScheduler.Infrastructure/ TaskScheduler.Infrastructure/
COPY TaskScheduler.Worker/ TaskScheduler.Worker/

RUN dotnet publish TaskScheduler.Worker/TaskScheduler.Worker.csproj -c Release -o /app/publish

# Stage 2 - Run
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TaskScheduler.Worker.dll"]