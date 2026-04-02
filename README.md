# Distributed Task Scheduler

A production-grade distributed job queue and task scheduler built with ASP.NET Core, featuring priority-based execution, automatic retries, distributed locking, and adaptive rate limiting.

## Architecture
```
┌─────────────────┐         ┌─────────────────────────────────────┐
│   HTTP Client   │────────▶│         TaskScheduler.Api           │
└─────────────────┘         │  POST /api/jobs  GET /api/jobs/{id} │
                            │  GET /api/jobs/metrics               │
                            └────────────────┬────────────────────┘
                                             │
                                             ▼
                            ┌─────────────────────────────────────┐
                            │           MySQL Database             │
                            │           Jobs Table                 │
                            └────────────────┬────────────────────┘
                                             │
                                             ▼
                            ┌─────────────────────────────────────┐
                            │       TaskScheduler.Worker          │
                            │  - Polls for pending jobs           │
                            │  - Acquires Redis lock              │
                            │  - Executes job                     │
                            │  - Handles retries                  │
                            │  - Adaptive rate limiting           │
                            └────────────────┬────────────────────┘
                                             │
                                             ▼
                            ┌─────────────────────────────────────┐
                            │           Redis                      │
                            │     Distributed Locking              │
                            └─────────────────────────────────────┘
```

## Tech Stack

- **Backend:** C#, ASP.NET Core (.NET 10), Worker Service
- **Database:** MySQL 8.0 with Dapper ORM
- **Cache/Lock:** Redis 7.2 with StackExchange.Redis
- **Containerization:** Docker, Docker Compose
- **Architecture:** Clean Architecture (Core, Infrastructure, Api, Worker)

## Features

- **Priority Queue** — jobs with higher priority are always picked up first
- **Scheduled Jobs** — enqueue a job to run at a future time
- **Automatic Retries** — failed jobs are retried up to MaxRetries times
- **Dead Letter Queue** — jobs that exhaust all retries are moved to dead letter
- **Distributed Locking** — Redis locks prevent duplicate job execution across multiple worker instances
- **Adaptive Rate Limiting** — worker automatically slows down under heavy load to protect the database
- **Metrics Endpoint** — real-time visibility into queue depth and job statuses
- **Fully Containerized** — entire system runs with a single docker compose command

## Project Structure
```
distributed-task-scheduler/
├── TaskScheduler.Core/           # Models, interfaces, enums (no dependencies)
├── TaskScheduler.Infrastructure/ # JobRepository, Redis locking, rate limiter
├── TaskScheduler.API/            # REST API controllers
├── TaskScheduler.Worker/         # Background job processor
├── db/
│   └── init.sql                  # MySQL schema
└── docker-compose.yml            # Full system orchestration
```

## Running Locally

### Prerequisites
- .NET 10 SDK
- Docker Desktop

### With Docker (recommended)
```bash
git clone https://github.com/sar23thak/distributed-task-scheduler.git
cd distributed-task-scheduler
docker compose up --build
```

API will be available at `http://localhost:5166`

### Without Docker
Start MySQL and Redis containers only:
```bash
docker compose up mysql redis -d
```

Then run Api and Worker separately:
```bash
dotnet run --project TaskScheduler.API
dotnet run --project TaskScheduler.Worker
```

## API Endpoints

### Enqueue a Job
```
POST /api/jobs
Content-Type: application/json

{
    "type": "SendEmail",
    "payload": "{\"to\": \"user@gmail.com\", \"subject\": \"Hello!\"}",
    "priority": 5,
    "maxRetries": 3,
    "scheduledAt": null
}
```

### Get Job Status
```
GET /api/jobs/{id}
```

### Get System Metrics
```
GET /api/jobs/metrics
```

Sample response:
```json
{
    "pendingJobs": 12,
    "runningJobs": 3,
    "completedJobs": 145,
    "failedJobs": 2,
    "deadLetterJobs": 1,
    "totalJobs": 163
}
```

## Job Types

| Type | Description |
|------|-------------|
| `SendEmail` | Simulates sending an email (500ms) |
| `GenerateReport` | Simulates report generation (1000ms) |
| `AlwaysFail` | Test job that always fails (for retry testing) |

## How It Works

1. Client sends `POST /api/jobs` with job type and payload
2. Api saves job to MySQL with `Status=Pending`
3. Worker polls MySQL every 2 seconds for pending jobs
4. Worker acquires a Redis lock for the job (prevents duplicate execution)
5. Worker marks job as `Running` and executes it
6. On success → marks as `Completed`
7. On failure → increments `RetryCount`, resets to `Pending` for retry
8. After `MaxRetries` exhausted → marks as `DeadLetter`