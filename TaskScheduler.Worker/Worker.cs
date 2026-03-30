using TaskScheduler.Core.Enums;
using TaskScheduler.Core.Interfaces;

namespace TaskScheduler.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IJobRepository _jobRepository;

    public Worker(ILogger<Worker> logger, IJobRepository jobRepository)
    {
        _logger = logger;
        _jobRepository = jobRepository;
    }
    protected override async Task ExecuteAsync (CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker started.");
        while(!stoppingToken.IsCancellationRequested)
        {
            var job = await _jobRepository.GetNextPendingJobAsync();
            if(job is null)
            {
                await Task.Delay(2000, stoppingToken);
                continue;
            }

            _logger.LogInformation("Picked up job {JobId} of type {JobType}", job.Id, job.Type);
            job.Status=Core.Enums.JobStatus.Running;
            job.StartedAt= DateTime.Now;
            await _jobRepository.UpdateAsync(job);

            try
            {
                await ExecuteJobAsync(job);

                job.Status = Core.Enums.JobStatus.Completed;
                job.CompletedAt = DateTime.UtcNow;
                await _jobRepository.UpdateAsync(job);

                _logger.LogInformation("Job {JobId} completed successfully.", job.Id);
            }
            catch (Exception ex)
            {
                job.RetryCount++;
                job.LastError = ex.Message;
                if(job.RetryCount >= job.MaxRetries)
                {
                    job.Status = JobStatus.DeadLetter;
                    _logger.LogWarning("Job {JobId} exhausted all retries. Moving to dead letter.", job.Id);
                }
                else
                {
                    job.Status=JobStatus.Pending;
                    _logger.LogWarning("Job {JobId} failed. Retry {RetryCount}/{MaxRetries}.", 
                                        job.Id, job.RetryCount, job.MaxRetries);
                }

                await _jobRepository.UpdateAsync(job);
            }
        }
    }
    private async Task ExecuteJobAsync(Core.Models.Job job)
    {
        switch (job.Type)
        {
            case "SendEmail":
                _logger.LogInformation("Sending email with payload: {Payload}", job.Payload);
                await Task.Delay(500); // simulate work
                break;

            case "GenerateReport":
                _logger.LogInformation("Generating report with payload: {Payload}", job.Payload);
                await Task.Delay(1000); // simulate work
                break;

            case "AlwaysFail":
                throw new Exception("This job always fails intentionally.");

            default:
                throw new InvalidOperationException($"Unknown job type: {job.Type}");
        }
    }
}
