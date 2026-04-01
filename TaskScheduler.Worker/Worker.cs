using TaskScheduler.Core.Enums;
using TaskScheduler.Core.Interfaces;
using TaskScheduler.Infrastructure.Services;

namespace TaskScheduler.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IJobRepository _jobRepository;
    private readonly RedisDistributedLockService _lockService;
    private readonly AdaptiveRateLimiter _rateLimiter;

    public Worker(ILogger<Worker> logger, IJobRepository jobRepository,
        RedisDistributedLockService lockService, AdaptiveRateLimiter rateLimiter)
    {
        _logger = logger;
        _jobRepository = jobRepository;
        _lockService = lockService;
        _rateLimiter = rateLimiter;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var pendingCount = await _jobRepository.GetPendingJobCountAsync();
            await _rateLimiter.WaitAsync(pendingCount, stoppingToken);

            var job = await _jobRepository.GetNextPendingJobAsync();

            if (job is null)
            {
                await Task.Delay(2000, stoppingToken);
                continue;
            }

            var lockAcquired = await _lockService.AcquireLockAsync(job.Id);

            if (!lockAcquired)
            {
                _logger.LogInformation("Job {JobId} is already being processed by another worker. Skipping.", job.Id);
                await Task.Delay(500, stoppingToken);
                continue;
            }

            _logger.LogInformation("Picked up job {JobId} of type {JobType} | Pending jobs: {PendingCount}",
                job.Id, job.Type, pendingCount);

            job.Status = JobStatus.Running;
            job.StartedAt = DateTime.UtcNow;
            await _jobRepository.UpdateAsync(job);

            try
            {
                await ExecuteJobAsync(job);

                job.Status = JobStatus.Completed;
                job.CompletedAt = DateTime.UtcNow;
                await _jobRepository.UpdateAsync(job);

                _logger.LogInformation("Job {JobId} completed successfully.", job.Id);
            }
            catch (Exception ex)
            {
                job.RetryCount++;
                job.LastError = ex.Message;

                if (job.RetryCount >= job.MaxRetries)
                {
                    job.Status = JobStatus.DeadLetter;
                    _logger.LogWarning("Job {JobId} exhausted all retries. Moving to dead letter.", job.Id);
                }
                else
                {
                    job.Status = JobStatus.Pending;
                    _logger.LogWarning("Job {JobId} failed. Retry {RetryCount}/{MaxRetries}.",
                        job.Id, job.RetryCount, job.MaxRetries);
                }

                await _jobRepository.UpdateAsync(job);
            }
            finally
            {
                await _lockService.ReleaseLockAsync(job.Id);
            }
        }
    }

    private async Task ExecuteJobAsync(Core.Models.Job job)
    {
        switch (job.Type)
        {
            case "SendEmail":
                _logger.LogInformation("Sending email with payload: {Payload}", job.Payload);
                await Task.Delay(500);
                break;

            case "GenerateReport":
                _logger.LogInformation("Generating report with payload: {Payload}", job.Payload);
                await Task.Delay(8000);
                break;

            case "AlwaysFail":
                throw new Exception("This job always fails intentionally.");

            default:
                throw new InvalidOperationException($"Unknown job type: {job.Type}");
        }
    }
}