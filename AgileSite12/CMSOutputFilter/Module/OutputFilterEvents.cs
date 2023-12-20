using System;
using System.Text;

namespace CMS.OutputFilter
{
    /// <summary>
    /// Output filter events
    /// </summary>
    public class OutputFilterEvents
    {
        /// <summary>
        /// Fires when the output filter saves output to cache
        /// </summary>
        public static OutputCacheHandler SaveOutputToCache = new OutputCacheHandler { Name = "OutputFilterEvents.SaveOutputToCache" };


        /// <summary>
        /// Fires when content from the output cache is about to be send as a response. 
        /// Fires only if cached output is found and gives subscriber the opportunity to change output or to bypass caching altogether.
        /// </summary>
        public static OutputCacheHandler SendCacheOutput = new OutputCacheHandler { Name = "OutputFilterEvents.SendCacheOutput" };
    }
}
