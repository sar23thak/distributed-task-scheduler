using StackExchange.Redis;

namespace TaskScheduler.Infrastructure.Services
{
    public class RedisDistributedLockService
    {
        private readonly IDatabase _redis;
        private readonly TimeSpan _lockExpiry = TimeSpan.FromSeconds(30);
        
        public RedisDistributedLockService(string connectionString)
        {
            var connection=ConnectionMultiplexer.Connect(connectionString);
            _redis = connection.GetDatabase();
        }
        public async Task<bool> AcquireLockAsync(Guid jobId)
        {
            var key = $"job-lock:{jobId}";
            return await _redis.StringSetAsync(key, "locked", _lockExpiry, When.NotExists);
        }
        public async Task ReleaseLockAsync(Guid jobId)
        {
            var key = $"job-lock:{jobId}";
            await _redis.KeyDeleteAsync(key);
        }
    }
}
