using System;

namespace CMS.Base
{
    /// <summary>
    /// Defines a lazy loaded value stored in the request stock helper
    /// </summary>
    public class RequestStockValue<TValue> : CMSLazy<TValue>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="requestStockKey">Request stock key</param>
        /// <param name="initializer">Initializer for the new value</param>
        public RequestStockValue(string requestStockKey, Func<TValue> initializer)
            : base(initializer)
        {
            RequestStockKey = requestStockKey;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="requestStockKey">Request stock key</param>
        /// <param name="defaultValue">Default value</param>
        public RequestStockValue(string requestStockKey, TValue defaultValue = default(TValue))
            : base(() => defaultValue)
        {
            RequestStockKey = requestStockKey;
        }
    }
}
