using Microsoft.AspNetCore.Mvc;
using TaskScheduler.Core.Interfaces;
using TaskScheduler.Core.Models;

namespace TaskScheduler.Api.Controllers
{
    [ApiController]
    [Route("api/jobs")]
    public class JobsController : ControllerBase
    {
        private readonly IJobRepository _jobRepository;
        public JobsController(IJobRepository jobRepository)
        {
            _jobRepository = jobRepository;
        }
        [HttpPost]
        public async Task<IActionResult> EnqueueJob([FromBody] EnqueueJobRequest request)
        {
            var job = new Job
            {
                Type = request.Type,
                Payload = request.Payload,
                Priority = request.Priority,
                MaxRetries = request.MaxRetries,
                ScheduledAt = request.ScheduledAt
            };

            await _jobRepository.AddAsync(job);

            return CreatedAtAction(nameof(GetJob), new { id = job.Id }, new
            {
                job.Id,
                job.Status,
                job.CreatedAt
            });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetJob(Guid id)
        {
            var job = await _jobRepository.GetByIdAsync(id);

            if (job is null)
                return NotFound(new { message = $"Job {id} not found" });

            return Ok(job);
        }
    }
    public record EnqueueJobRequest(string Type, string Payload, int Priority = 0, 
                                    int MaxRetries = 3, DateTime? ScheduledAt = null);
}
