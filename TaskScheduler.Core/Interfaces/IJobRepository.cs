using TaskScheduler.Core.Models;

namespace TaskScheduler.Core.Interfaces;

public interface IJobRepository
{
    Task AddAsync(Job job);
    Task<Job?> GetByIdAsync(Guid id);
    Task<Job?> GetNextPendingJobAsync();
    Task UpdateAsync(Job job);
}