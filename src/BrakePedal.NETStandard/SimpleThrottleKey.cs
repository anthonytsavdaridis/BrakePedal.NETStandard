namespace BrakePedal.NETStandard
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    public class SimpleThrottleKey : IThrottleKey
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        public SimpleThrottleKey(params object[] values)
        {
            Values = values ?? throw new ArgumentNullException(nameof(values));
        }

        /// <summary>
        /// 
        /// </summary>
        public object[] Values { get; private set; }
    }
}