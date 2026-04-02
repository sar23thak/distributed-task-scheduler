# Stage 1 - Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY TaskScheduler.Core/TaskScheduler.Core.csproj TaskScheduler.Core/
COPY TaskScheduler.Infrastructure/TaskScheduler.Infrastructure.csproj TaskScheduler.Infrastructure/
COPY TaskScheduler.API/TaskScheduler.API.csproj TaskScheduler.API/

RUN dotnet restore TaskScheduler.API/TaskScheduler.API.csproj

COPY TaskScheduler.Core/ TaskScheduler.Core/
COPY TaskScheduler.Infrastructure/ TaskScheduler.Infrastructure/
COPY TaskScheduler.API/ TaskScheduler.API/

RUN dotnet publish TaskScheduler.API/TaskScheduler.API.csproj -c Release -o /app/publish

# Stage 2 - Run
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TaskScheduler.API.dll"]