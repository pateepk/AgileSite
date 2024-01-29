using System;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page for the Integration pages.
    /// </summary>
    public abstract class CMSIntegrationPage : GlobalAdminPage
    {
        /// <summary>
        /// Page OnInit event.
        /// </summary>
        /// <param name="e">Event args</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, string.Empty) != string.Empty)
            {
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.IntegrationBus);
            }
        }
    }
}