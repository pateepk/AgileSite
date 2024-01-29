using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Chat
{
    /// <summary>
    /// Params for getting messages from database. Those params have to be wrapped in class to be usable by ChatParametrizedCacheWrapper.
    /// </summary>
    public class MessageCacheParams : IChatCacheableParam
    {
        /// <summary>
        /// Maximum number of messages retrieved. Or unlimited if null.
        /// </summary>
        public int? MaxCount { get; set; }


        /// <summary>
        /// Messages sent since when.
        /// </summary>
        public DateTime? SinceWhen { get; set; }


        /// <summary>
        /// Unique key of this params - for caching.
        /// </summary>
        public string CacheKey
        {
            get
            {
                return string.Format("{0}|{1}", MaxCount ?? -1, (SinceWhen ?? DateTime.MinValue).Ticks);
            }
        }
    }
}
