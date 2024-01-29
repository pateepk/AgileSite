using System;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Membership;
using CMS.UIControls;

namespace CMS.TranslationServices.Web.UI
{
    /// <summary>
    /// Base UI class for translation services modal UI pages.
    /// </summary>
    public class CMSTranslationServiceModalPage : CMSModalPage
    {
        /// <summary>
        /// OnLoad override.
        /// </summary>
        /// <param name="e">Event agrs</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Check the license
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.TranslationServices);
            }

            CurrentUserInfo user = MembershipContext.AuthenticatedUser;

            // Check permissions for CMS Desk -> Tools -> Forums
            if (!user.IsAuthorizedPerUIElement("CMS.TranslationServices", "Translations"))
            {
                RedirectToUIElementAccessDenied("CMS.TranslationServices", "Translations");
            }

            // Check 'Read' permission of CMS.Content
            if (!user.IsAuthorizedPerResource("CMS.Content", "Read"))
            {
                RedirectToAccessDenied("CMS.Content", "Read");
            }
        }
    }
}