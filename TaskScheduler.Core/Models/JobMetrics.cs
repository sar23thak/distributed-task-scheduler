namespace TaskScheduler.Core.Models
{
    public class JobMetrics
    {
        public int PendingJobs { get; set; }
        public int RunningJobs { get; set; }
        public int CompletedJobs { get; set; }
        public int FailedJobs { get; set; }
        public int DeadLetterJobs { get; set; }
        public int TotalJobs { get; set; }
    }
}
