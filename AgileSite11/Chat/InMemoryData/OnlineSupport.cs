using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Chat
{
    using OnlineSupportersDictionary = Dictionary<int, ChatOnlineSupportInfo>;

    /// <summary>
    /// Class holding cache of users online on support.
    /// </summary>
    public class OnlineSupport
    {
        #region "Private fields"

        private readonly ChatCacheWrapper<OnlineSupportersDictionary> onlineSupportersCache;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Dictionary of chat users who are online on support. Dictionary is indexed by chat user ID.
        /// </summary>
        public OnlineSupportersDictionary OnlineSupporters
        {
            get
            {
                return onlineSupportersCache.Data;
            }
        }


        /// <summary>
        /// Invalidates cache of online supporters. This method has to be called after user logs into or leaves support. Otherwise cache can be outdated.
        /// </summary>
        public void InvalidateOnlineSupportCache()
        {
            onlineSupportersCache.Invalidate();
        }


        /// <summary>
        /// Gets count of users online on support.
        /// </summary>
        public int OnlineSupportersCount
        {
            get
            {
                return OnlineSupporters.Count;
            }
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// Constructor. Initializes cache.
        /// </summary>
        /// <param name="parentName">This string is used as a cache key - it has to be unique for every instance of this class</param>
        /// <param name="siteID">Supporters on this site will be handled by this class</param>
        public OnlineSupport(string parentName, int siteID)
        {
            onlineSupportersCache = new ChatCacheWrapper<OnlineSupportersDictionary>(
                parentName + "|OnlineSupporters", 
                () => ChatOnlineSupportInfoProvider.GetAllOnlineSupporters(siteID),
                TimeSpan.FromSeconds(120),
                null);
        }

        #endregion
    }
}
