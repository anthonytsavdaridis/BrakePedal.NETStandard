namespace BrakePedal.NETStandard
{
    /// <summary>
    /// 
    /// </summary>
    public class CheckResult
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly CheckResult NotThrottled =
            new CheckResult
            {
                IsThrottled = false,
                IsLocked = false
            };

        /// <summary>
        /// 
        /// </summary>
        public string ThrottleKey { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string LockKey { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Limiter Limiter { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsThrottled { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsLocked { get; set; }
    }
}