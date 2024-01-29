using System;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// Base class for UI pages of object versioning.
    /// </summary>
    public class CMSObjectVersioningPage : CMSPage
    {
        /// <summary>
        /// Init event handler
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            // Check the license
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.ObjectVersioning);
            }

            base.OnInit(e);
        }
    }
}