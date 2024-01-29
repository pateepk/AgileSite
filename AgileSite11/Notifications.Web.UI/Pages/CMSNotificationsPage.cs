using System;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.UIControls;

namespace CMS.Notifications.Web.UI
{
    /// <summary>
    /// Base page for the Notifications pages.
    /// </summary>
    public abstract class CMSNotificationsPage : GlobalAdminPage
    {
        /// <summary>
        /// Page OnInit event.
        /// </summary>
        /// <param name="e">Event args</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            string domain = RequestContext.CurrentDomain;
            if (domain != "")
            {
                LicenseHelper.CheckFeatureAndRedirect(domain, FeatureEnum.Notifications);
            }
        }
    }
}