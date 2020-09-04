using System;
namespace BrakePedal.NETStandard.Redis
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using StackExchange.Redis;

    /// <summary>
    /// 
    /// </summary>
    public class RedisThrottleRepository : IThrottleRepository
    {
        readonly IDatabase _db;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        public RedisThrottleRepository(IDatabase database)
        {
            _db = database;
        }

        /// <summary>
        /// 
        /// </summary>
        public object[] PolicyIdentityValues { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        /// <returns></returns>
        public long? GetThrottleCount(IThrottleKey key, Limiter limiter)
        {
            var id = CreateThrottleKey(key, limiter);
            var value = _db.StringGet(id);

            if (long.TryParse(value, out var convert))
            {
                return convert;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        /// <returns></returns>
        public async Task<long?> GetThrottleCountAsync(IThrottleKey key, Limiter limiter)
        {
            var id = CreateThrottleKey(key, limiter);
            var value = await _db.StringGetAsync(id);

            if (long.TryParse(value, out var convert))
            {
                return convert;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        public void AddOrIncrementWithExpiration(IThrottleKey key, Limiter limiter)
        {
            var id = CreateThrottleKey(key, limiter);

            var result = _db.StringIncrement(id);

            // If we get back 1, that means the key was incremented as it
            // was expiring or it's a new key. Ensure we set the expiration.
            if (result == 1)
            {
                _db.KeyExpire(id, limiter.Period);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        /// <returns></returns>
        public async Task AddOrIncrementWithExpirationAsync(IThrottleKey key, Limiter limiter)
        {
            var id = CreateThrottleKey(key, limiter);

            var result = await _db.StringIncrementAsync(id);

            // If we get back 1, that means the key was incremented as it
            // was expiring or it's a new key. Ensure we set the expiration.
            if (result == 1)
            {
                await _db.KeyExpireAsync(id, limiter.Period);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        /// <returns></returns>
        public bool LockExists(IThrottleKey key, Limiter limiter)
        {
            var id = CreateLockKey(key, limiter);
            return _db.KeyExists(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        /// <returns></returns>
        public Task<bool> LockExistsAsync(IThrottleKey key, Limiter limiter)
        {
            var id = CreateLockKey(key, limiter);
            return _db.KeyExistsAsync(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        public void SetLock(IThrottleKey key, Limiter limiter)
        {
            var id = CreateLockKey(key, limiter);

            var trans = _db.CreateTransaction();
            // TODO: Add Nito
            trans.StringIncrementAsync(id);
            trans.KeyExpireAsync(id, limiter.LockDuration);
            trans.Execute();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        /// <returns></returns>
        public async Task SetLockAsync(IThrottleKey key, Limiter limiter)
        {
            var id = CreateLockKey(key, limiter);

            var trans = _db.CreateTransaction();
            await trans.StringIncrementAsync(id);
            await trans.KeyExpireAsync(id, limiter.LockDuration);
            await trans.ExecuteAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        public void RemoveThrottle(IThrottleKey key, Limiter limiter)
        {
            var id = CreateThrottleKey(key, limiter);
            _db.KeyDelete(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        /// <returns></returns>
        public Task RemoveThrottleAsync(IThrottleKey key, Limiter limiter)
        {
            var id = CreateThrottleKey(key, limiter);
            return _db.KeyDeleteAsync(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        /// <returns></returns>
        public string CreateThrottleKey(IThrottleKey key, Limiter limiter)
        {
            var values = CreateBaseKeyValues(key, limiter);

            var countKey = TimeSpanToFriendlyString(limiter.Period);
            values.Add(countKey);

            // Using the Unix timestamp to the key allows for better
            // precision when querying a key from Redis
            if (limiter.Period.TotalSeconds == 1)
            {
                values.Add(GetUnixTimestamp());
            }

            return string.Join(":", values);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        /// <returns></returns>
        public string CreateLockKey(IThrottleKey key, Limiter limiter)
        {
            var values = CreateBaseKeyValues(key, limiter);

            var lockKeySuffix = TimeSpanToFriendlyString(limiter.LockDuration.Value);
            values.Add("lock");
            values.Add(lockKeySuffix);

            return string.Join(":", values);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        /// <returns></returns>
        private List<object> CreateBaseKeyValues(IThrottleKey key, Limiter limiter)
        {
            var values = key.Values.ToList();

            if (PolicyIdentityValues != null && PolicyIdentityValues.Length > 0)
            {
                values.InsertRange(0, PolicyIdentityValues);
            }

            return values;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        private string TimeSpanToFriendlyString(TimeSpan span)
        {
            var items = new List<string>();

            void ifNotZeroAppend(double value, string key)
            {
                if (value != 0)
                    items.Add(string.Concat(value, key));
            }

            ifNotZeroAppend(span.Days, "d");
            ifNotZeroAppend(span.Hours, "h");
            ifNotZeroAppend(span.Minutes, "m");
            ifNotZeroAppend(span.Seconds, "s");

            return string.Join(string.Empty, items);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private long GetUnixTimestamp()
        {
            var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)timeSpan.TotalSeconds;
        }
    }
}