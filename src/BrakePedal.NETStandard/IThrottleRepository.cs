namespace BrakePedal.NETStandard
{
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    public interface IThrottleRepository
    {
        /// <summary>
        /// 
        /// </summary>
        object[] PolicyIdentityValues { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        /// <returns></returns>
        long? GetThrottleCount(IThrottleKey key, Limiter limiter);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        /// <returns></returns>
        Task<long?> GetThrottleCountAsync(IThrottleKey key, Limiter limiter);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        void AddOrIncrementWithExpiration(IThrottleKey key, Limiter limiter);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        /// <returns></returns>
        Task AddOrIncrementWithExpirationAsync(IThrottleKey key, Limiter limiter);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        void SetLock(IThrottleKey key, Limiter limiter);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        /// <returns></returns>
        Task SetLockAsync(IThrottleKey key, Limiter limiter);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        /// <returns></returns>
        bool LockExists(IThrottleKey key, Limiter limiter);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        /// <returns></returns>
        Task<bool> LockExistsAsync(IThrottleKey key, Limiter limiter);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        void RemoveThrottle(IThrottleKey key, Limiter limiter);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        /// <returns></returns>
        Task RemoveThrottleAsync(IThrottleKey key, Limiter limiter);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        /// <returns></returns>
        string CreateThrottleKey(IThrottleKey key, Limiter limiter);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limiter"></param>
        /// <returns></returns>
        string CreateLockKey(IThrottleKey key, Limiter limiter);
    }
}