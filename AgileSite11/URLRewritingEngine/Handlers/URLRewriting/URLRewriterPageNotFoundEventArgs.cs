using CMS.Base;
using CMS.PortalEngine;
using CMS.SiteProvider;

namespace CMS.URLRewritingEngine
{
    /// <summary>
    /// Event arguments for <see cref="URLRewriterPageNotFoundHandler"/> event handler
    /// </summary>
    public class URLRewriterPageNotFoundEventArgs : CMSEventArgs
    {
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
            internal set;
        }


        /// <summary>
        /// Indicates whether was 'page not found' handled
        /// </summary>
        /// <remarks>
        /// Set this property to 'true' if you don't want to handle 'page not found' by system
        /// </remarks>
        public bool PageNotFoundHandled
        {
            get;
            set;
        }
    }
}
