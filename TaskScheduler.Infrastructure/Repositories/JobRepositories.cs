using Dapper;
using MySqlConnector;
using TaskScheduler.Core.Enums;
using TaskScheduler.Core.Interfaces;
using TaskScheduler.Core.Models;

namespace TaskScheduler.Infrastructure.Repositories;

public class JobRepository : IJobRepository
{
    private readonly string _connectionString;

    public JobRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task AddAsync(Job job)
    {
        const string sql = @"
            INSERT INTO Jobs (Id, Type, Payload, Status, Priority, RetryCount, MaxRetries, CreatedAt, ScheduledAt)
            VALUES (@Id, @Type, @Payload, @Status, @Priority, @RetryCount, @MaxRetries, @CreatedAt, @ScheduledAt)";

        using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, job);
    }

    public async Task<Job?> GetByIdAsync(Guid id)
    {
        const string sql = "SELECT * FROM Jobs WHERE Id = @Id";

        using var connection = new MySqlConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<Job>(sql, new { Id = id });
    }

    public async Task<Job?> GetNextPendingJobAsync()
    {
        const string sql = @"
            SELECT * FROM Jobs 
            WHERE Status = @Status
            AND (ScheduledAt IS NULL OR ScheduledAt <= @Now)
            ORDER BY Priority DESC, CreatedAt ASC
            LIMIT 1";

        using var connection = new MySqlConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<Job>(sql, new
        {
            Status = JobStatus.Pending,
            Now = DateTime.UtcNow
        });
    }

    public async Task UpdateAsync(Job job)
    {
        const string sql = @"
            UPDATE Jobs SET
                Status = @Status,
                RetryCount = @RetryCount,
                LastError = @LastError,
                StartedAt = @StartedAt,
                CompletedAt = @CompletedAt
            WHERE Id = @Id";

        using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, job);
    }
    public async Task<int> GetPendingJobCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM Jobs WHERE Status=@Status";
        using var connection = new MySqlConnection(_connectionString);
        return await connection.ExecuteScalarAsync<int>(sql, new { Status = JobStatus.Pending });
    }
}