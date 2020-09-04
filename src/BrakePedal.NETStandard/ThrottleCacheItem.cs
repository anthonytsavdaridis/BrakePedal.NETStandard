namespace BrakePedal.NETStandard
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class ThrottleCacheItem
    {
        /// <summary>
        /// 
        /// </summary>
        public long Count { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime Expiration { get; set; }
    }
}