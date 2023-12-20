using System;
using System.Linq;

using CMS;
using CMS.Base.Web.UI;
using CMS.Core;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.SharePoint.Web.UI;
using CMS.SiteProvider;
using CMS.UIControls;

[assembly: RegisterCustomClass("SharePointLibrariesPageExtender", typeof(SharePointLibrariesPageExtender))]

namespace CMS.SharePoint.Web.UI
{
    /// <summary>
    /// SharePoint libraries listing page extender
    /// </summary>
    public class SharePointLibrariesPageExtender : PageExtender<CMSPage>
    {
        /// <summary>
        /// Initializes the page
        /// </summary>
        public override void OnInit()
        {
            Page.LoadComplete += Page_LoadComplete;
        }

        void Page_LoadComplete(object sender, EventArgs e)
        {
            if (!SharePointConnectionInfoProvider.GetSharePointConnections(SiteContext.CurrentSiteID).TopN(1).Any())
            {
                Page.ShowWarning(String.Format(ResHelper.GetString("SharePoint.Library.NoConnectionWarning"), URLHelper.GetAbsoluteUrl(UIContextHelper.GetElementUrl(ModuleName.SHAREPOINT, "SharePoint"))));
            }
        }
    }
}