using System;
using System.Collections.Generic;
using System.Text;
using TaskScheduler.Core.Enums;

namespace TaskScheduler.Core.Models
{
    public class Job
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Type { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public JobStatus Status { get; set; } = JobStatus.Pending;
        public int Priority { get; set; } = 0;
        public int RetryCount { get; set; } = 0;
        public int MaxRetries { get; set; } = 3;
        public string? LastError { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ScheduledAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
