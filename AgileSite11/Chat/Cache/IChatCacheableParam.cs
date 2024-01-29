using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Chat
{
    /// <summary>
    /// Parametrized cache accepts this interface as a parameter.
    /// </summary>
    public interface IChatCacheableParam
    {
        /// <summary>
        /// This key will be used as a cache key.
        /// </summary>
        string CacheKey { get; }
    }
}
