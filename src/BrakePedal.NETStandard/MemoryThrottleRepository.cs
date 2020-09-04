namespace BrakePedal.NETStandard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Internal;

    /// <summary>
    /// 
    /// </summary>
    public partial class MemoryThrottleRepository : IThrottleRepository
    {
        readonly IMemoryCache _store;
        readonly ISystemClock _clock;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cache"></param>
        public MemoryThrottleRepository(IMemoryCache cache, ISystemClock clock)
        {
            _store = cache;
            _clock = clock;
        }

        /// <summary>
        /// 
        /// </summary>
        public MemoryThrottleRepository()
        {
            _store = new MemoryCache(new MemoryCacheOptions());
            _clock = new SystemClock();
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
            string id = CreateThrottleKey(key, limiter);

            var cacheItem = _store.Get(id) as ThrottleCacheItem;
            if (cacheItem != null)
            {
                return cacheItem.Count;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        /// <returns></returns>
        public Task<long?> GetThrottleCountAsync(IThrottleKey key, Limiter limiter)
            => Task.FromResult(GetThrottleCount(key, limiter));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        public void AddOrIncrementWithExpiration(IThrottleKey key, Limiter limiter)
        {
            string id = CreateThrottleKey(key, limiter);
            var cacheItem = _store.Get(id) as ThrottleCacheItem;

            if (cacheItem != null)
            {
                cacheItem.Count = cacheItem.Count + 1;
            }
            else
            {
                cacheItem = new ThrottleCacheItem()
                {
                    Count = 1,
                    Expiration = _clock.UtcNow.UtcDateTime.Add(limiter.Period)
                };
            }

            _store.Set(id, cacheItem, cacheItem.Expiration);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        /// <returns></returns>
        public Task AddOrIncrementWithExpirationAsync(IThrottleKey key, Limiter limiter)
        {
            AddOrIncrementWithExpiration(key, limiter);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        public void SetLock(IThrottleKey key, Limiter limiter)
        {
            var throttleId = CreateThrottleKey(key, limiter);
            _store.Remove(throttleId);

            var lockId = CreateLockKey(key, limiter);
            var expiration = _clock.UtcNow.DateTime.Add(limiter.LockDuration.Value);

            _store.Set(lockId, true, expiration);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        /// <returns></returns>
        public Task SetLockAsync(IThrottleKey key, Limiter limiter)
        {
            SetLock(key, limiter);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        /// <returns></returns>
        public bool LockExists(IThrottleKey key, Limiter limiter)
        {
            var lockId = CreateLockKey(key, limiter);
            return _store.TryGetValue(lockId, out _);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        /// <returns></returns>
        public Task<bool> LockExistsAsync(IThrottleKey key, Limiter limiter)
            => Task.FromResult(LockExists(key, limiter));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        public void RemoveThrottle(IThrottleKey key, Limiter limiter)
        {
            var lockId = CreateThrottleKey(key, limiter);
            _store.Remove(lockId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        /// <returns></returns>
        public Task RemoveThrottleAsync(IThrottleKey key, Limiter limiter)
        {
            RemoveThrottle(key, limiter);
            return Task.CompletedTask;
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
        public Task<string> CreateLockKeyAsync(IThrottleKey key, Limiter limiter)
            => Task.FromResult(CreateLockKey(key, limiter));

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
            Action<double, string> ifNotZeroAppend = (value, key) =>
            {
                if (value != 0)
                {
                    items.Add(string.Concat(value, key));
                }
            };

            ifNotZeroAppend(span.Days, "d");
            ifNotZeroAppend(span.Hours, "h");
            ifNotZeroAppend(span.Minutes, "m");
            ifNotZeroAppend(span.Seconds, "s");

            return string.Join("", items);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private long GetUnixTimestamp()
        {
            TimeSpan timeSpan = (_clock.UtcNow.DateTime - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)timeSpan.TotalSeconds;
        }
    }
}