namespace BrakePedal.NETStandard
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    public interface IThrottlePolicy
    {
        /// <summary>
        /// 
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string[] Prefixes { get; set; }

        /// <summary>
        /// 
        /// </summary>
        ICollection<Limiter> Limiters { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="increment"></param>
        /// <returns></returns>
        CheckResult Check(IThrottleKey key, bool increment = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="increment"></param>
        /// <returns></returns>
        Task<CheckResult> CheckAsync(IThrottleKey key, bool increment = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="result"></param>
        /// <param name="increment"></param>
        /// <returns></returns>
        bool IsThrottled(IThrottleKey key, out CheckResult result, bool increment = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="increment"></param>
        /// <returns></returns>
        Task<bool> IsThrottledAsync(IThrottleKey key, bool increment = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="result"></param>
        /// <param name="increment"></param>
        /// <returns></returns>
        bool IsLocked(IThrottleKey key, out CheckResult result, bool increment = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="increment"></param>
        /// <returns></returns>
        Task<bool> IsLockedAsync(IThrottleKey key, bool increment = true);
    }
}