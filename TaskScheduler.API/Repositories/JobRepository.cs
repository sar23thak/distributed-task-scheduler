using Dapper;
using MySqlConnector;
using TaskScheduler.Core.Interfaces;
using TaskScheduler.Core.Models;

namespace TaskScheduler.Api.Repositories;

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
}