using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core.Services;
using StackExchange.Redis;

namespace Lykke.Service.PayInternal.Services
{
    public class RedisLocksService : IDistributedLocksService
    {
        private readonly string _keyPattern;
        private readonly IDatabase _database;

        public RedisLocksService(
            [NotNull] string keyPattern,
            [NotNull] IConnectionMultiplexer connectionMultiplexer)
        {
            _keyPattern = keyPattern ?? throw new ArgumentNullException(nameof(keyPattern));
            _database = connectionMultiplexer.GetDatabase() ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
        }

        public Task<bool> TryAcquireLockAsync(string key, string data, DateTime expiration)
        {
            TimeSpan expiresIn = expiration - DateTime.UtcNow;

            return _database.LockTakeAsync(GetCacheKey(key), data, expiresIn);
        }

        public Task<bool> ReleaseLockAsync(string key, string token)
        {
            return _database.LockReleaseAsync(GetCacheKey(key), token);
        }

        private string GetCacheKey(string key)
        {
            return string.Format(_keyPattern, key);
        }
    }
}
