using System;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// Page for SMTP Servers administration.
    /// </summary>
    public abstract class CMSSMTPServersPage : GlobalAdminPage
    {
        /// <summary>
        /// Init event handler
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Check license
            if (!string.IsNullOrEmpty(RequestContext.CurrentDomain))
            {
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.MultipleSMTPServers);
            }
        }
    }
}