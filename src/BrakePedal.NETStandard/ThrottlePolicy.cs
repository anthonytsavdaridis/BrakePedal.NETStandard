namespace BrakePedal.NETStandard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    public class ThrottlePolicy : IThrottlePolicy
    {
        readonly IThrottleRepository _repository;
        private List<Limiter> _limits;
        private string[] _prefixes;

        /// <summary>
        /// 
        /// </summary>
        public ThrottlePolicy()
            : this(new MemoryThrottleRepository())
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="repository"></param>
        public ThrottlePolicy(IThrottleRepository repository)
        {
            Limiters = new List<Limiter>();
            _repository = repository;
        }

        /// <summary>
        /// 
        /// </summary>
        public long? PerSecond
        {
            get { return GetLimiterCount(TimeSpan.FromSeconds(1)); }
            set { SetLimiter(TimeSpan.FromSeconds(1), value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public long? PerMinute
        {
            get { return GetLimiterCount(TimeSpan.FromMinutes(1)); }
            set { SetLimiter(TimeSpan.FromMinutes(1), value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public long? PerHour
        {
            get { return GetLimiterCount(TimeSpan.FromHours(1)); }
            set { SetLimiter(TimeSpan.FromHours(1), value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public long? PerDay
        {
            get { return GetLimiterCount(TimeSpan.FromDays(1)); }
            set { SetLimiter(TimeSpan.FromDays(1), value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICollection<Limiter> Limiters
        {
            get { return _limits; }
            set { _limits = new List<Limiter>(value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string[] Prefixes
        {
            get { return _prefixes; }
            set
            {
                _prefixes = value;
                _repository.PolicyIdentityValues = _prefixes;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="result"></param>
        /// <param name="increment"></param>
        /// <returns></returns>
        public bool IsThrottled(IThrottleKey key, out CheckResult result, bool increment = true)
        {
            result = Check(key, increment);
            return result.IsThrottled;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="increment"></param>
        /// <returns></returns>
        public async Task<bool> IsThrottledAsync(IThrottleKey key, bool increment = true)
        {
            var result = await CheckAsync(key, increment);
            return result.IsThrottled;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="result"></param>
        /// <param name="increment"></param>
        /// <returns></returns>
        public bool IsLocked(IThrottleKey key, out CheckResult result, bool increment = true)
        {
            result = Check(key, increment);
            return result.IsLocked;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="increment"></param>
        /// <returns></returns>
        public async Task<bool> IsLockedAsync(IThrottleKey key, bool increment = true)
        {
            var result = await CheckAsync(key, increment);
            return result.IsLocked;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="increment"></param>
        /// <returns></returns>
        public CheckResult Check(IThrottleKey key, bool increment = true)
        {
            foreach (Limiter limiter in Limiters)
            {
                var result = new CheckResult
                {
                    IsThrottled = false,
                    IsLocked = false,
                    ThrottleKey = _repository.CreateThrottleKey(key, limiter),
                    Limiter = limiter
                };

                if (limiter.LockDuration.HasValue)
                {
                    result.LockKey = _repository.CreateLockKey(key, limiter);

                    if (_repository.LockExists(key, limiter))
                    {
                        result.IsLocked = true;
                        return result;
                    }
                }

                // Short-circuit this loop if the
                // limit value isn't valid
                if (limiter.Count <= 0)
                {
                    continue;
                }

                long? counter = _repository.GetThrottleCount(key, limiter);

                if (counter.HasValue
                    && counter.Value >= limiter.Count)
                {
                    if (limiter.LockDuration.HasValue)
                    {
                        _repository.SetLock(key, limiter);
                        _repository.RemoveThrottle(key, limiter);
                    }

                    result.IsThrottled = true;
                    return result;
                }

                if (increment)
                    _repository.AddOrIncrementWithExpiration(key, limiter);
            }

            return CheckResult.NotThrottled;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="result"></param>
        /// <param name="increment"></param>
        /// <returns></returns>
        public bool Check(IThrottleKey key, out CheckResult result, bool increment = true)
        {
            result = Check(key, increment);
            return result.IsThrottled;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="increment"></param>
        /// <returns></returns>
        public Task<CheckResult> CheckAsync(IThrottleKey key, bool increment = true)
            => Task.FromResult(Check(key, increment));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="span"></param>
        /// <param name="count"></param>
        private void SetLimiter(TimeSpan span, long? count)
        {
            var item = Limiters.FirstOrDefault(l => l.Period == span);
            if (item != null)
            {
                _limits.Remove(item);
            }

            if (!count.HasValue)
            {
                return;
            }

            item = new Limiter
            {
                Count = count.Value,
                Period = span
            };

            _limits.Add(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        private long? GetLimiterCount(TimeSpan span)
        {
            var item = Limiters.FirstOrDefault(l => l.Period == span);
            var result = default(long?);

            if (item != null)
            {
                result = item.Count;
            }

            return result;
        }
    }
}