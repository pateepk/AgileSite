using System;

using CMS.Modules;
using CMS.SiteProvider;

namespace CMS.Community.Web.UI
{
    /// <summary>
    /// Base page for message boards under groups pages.
    /// </summary>
    public class CMSGroupMessageBoardsPage : CMSGroupPage
    {
        /// <summary>
        /// Page OnInit event.
        /// </summary>
        /// <param name="e">Event args</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Check site availability
            if (!ResourceSiteInfoProvider.IsResourceOnSite("CMS.MessageBoards", SiteContext.CurrentSiteName))
            {
                RedirectToResourceNotAvailableOnSite("CMS.MessageBoards");
            }
        }
    }
}