using CMS.Base;
using CMS.PortalEngine;

namespace CMS.OutputFilter
{
    /// <summary>
    /// Arguments for the sending output from cache event.
    /// </summary>
    public class OutputCacheEventArgs : CMSEventArgs
    {
        /// <summary>
        /// View mode of the page.
        /// </summary>
        public ViewModeOnDemand ViewMode
        {
            get;
            set;
        }

        
        /// <summary>
        /// Content retrieved from the output cache.
        /// </summary>
        public CachedOutput Output
        {
            get;
            set;
        }


        /// <summary>
        /// If set to true, content will not be send from output cache, but will be generated regularly.
        /// <remarks>
        /// This is different than calling Cancel() on the event args. Calling Cancel() indicates, that event
        /// subscriber has already sent output to the response. Setting FallbackToRegularLoad indicates, that
        /// content should be generated normally.
        /// </remarks>
        /// </summary>
        public bool FallbackToRegularLoad
        {
            get;
            set;
        }
    }
}