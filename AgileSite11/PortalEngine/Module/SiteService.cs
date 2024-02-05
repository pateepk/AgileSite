using CMS.Base;
using CMS.SiteProvider;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Site service
    /// </summary>
    internal class SiteService : ISiteService
    {
        /// <summary>
        /// Current site
        /// </summary>
        public ISiteInfo CurrentSite
        {
            get
            {
                return SiteContext.CurrentSite;
            }
        }


        /// <summary>
        /// Returns true, if the current context executes on live site
        /// </summary>
        public bool IsLiveSite
        {
            get
            {
                return PortalContext.ViewMode.IsLiveSite();
            }
        }
    }
}
