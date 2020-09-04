using System;

namespace BrakePedal.NETStandard
{
    public static class LimiterExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static Limiter Over(this Limiter limiter, long seconds)
        {
            return limiter.Over(TimeSpan.FromSeconds(seconds));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public static Limiter Over(this Limiter limiter, TimeSpan span)
        {
            limiter.Period = span;
            return limiter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public static Limiter PerSecond(this Limiter limiter, long count)
        {
            return limiter.Limit(count).Over(1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="limiter"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static Limiter PerMinute(this Limiter limiter, long count)
        {
            return limiter.Limit(count).Over(60);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public static Limiter PerHour(this Limiter limiter, long count)
        {
            return limiter.Limit(count).Over(TimeSpan.FromHours(1));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public static Limiter PerDay(this Limiter limiter, long count)
        {
            return limiter.Limit(count).Over(TimeSpan.FromDays(1));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static Limiter LockFor(this Limiter limiter, long seconds)
        {
            return limiter.LockFor(TimeSpan.FromSeconds(seconds));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public static Limiter LockFor(this Limiter limiter, TimeSpan span)
        {
            limiter.LockDuration = span;
            return limiter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public static Limiter Limit(this Limiter limiter, long count)
        {
            limiter.Count = count;
            return limiter;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Limiter
    {
        /// <summary>
        /// 
        /// </summary>
        public long Count { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public TimeSpan Period { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public TimeSpan? LockDuration { get; set; }
    }
}