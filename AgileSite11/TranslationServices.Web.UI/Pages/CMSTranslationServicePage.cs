using System;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.UIControls;

namespace CMS.TranslationServices.Web.UI
{
    /// <summary>
    /// Base UI class for translation services UI pages.
    /// </summary>
    [Security(Resource = "CMS.Content", Permission = "Read")]
    [Security(Resource = "CMS.TranslationServices", Permission = "Read")]
    [UIElement("CMS.TranslationServices", "Translations")]
    public class CMSTranslationServicePage : CMSDeskPage
    {
        /// <summary>
        /// OnLoad override.
        /// </summary>
        /// <param name="e">Event agrs</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            CheckDocPermissions = false;

            // Check the license
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.TranslationServices);
            }
        }
    }
}