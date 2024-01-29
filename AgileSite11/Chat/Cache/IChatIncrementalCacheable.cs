using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Chat
{
    /// <summary>
    /// Only classes which implement this interface can be stored in ChatParametrizedCacheWrapper.
    /// </summary>
    public interface IChatIncrementalCacheable
    {
        /// <summary>
        /// Change time of this object.
        /// </summary>
        DateTime ChangeTime { get; }
    }
}
