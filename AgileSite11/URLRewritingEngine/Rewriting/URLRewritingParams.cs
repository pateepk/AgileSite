using CMS.Helpers;
using CMS.PortalEngine;
using CMS.SiteProvider;

namespace CMS.URLRewritingEngine
{
    /// <summary>
    /// Container for URL rewriting parameters
    /// </summary>
    public class URLRewritingParams
    {
        /// <summary>
        /// Rewriting status
        /// </summary>
        public RequestStatusEnum Status
        {
            get;
            set;
        }


        /// <summary>
        /// Excluded status
        /// </summary>
        public ExcludedSystemEnum ExcludedEnum
        {
            get;
            set;
        }


        /// <summary>
        /// Site name
        /// </summary>
        public SiteNameOnDemand SiteName
        {
            get;
            internal set;
        }


        /// <summary>
        /// View mode
        /// </summary>
        public ViewModeOnDemand ViewMode
        {
            get;
            internal set;
        }


        /// <summary>
        /// Relative path
        /// </summary>
        public string RelativePath
        {
            get;
            set;
        }
    }
}
