using System;

using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.SiteProvider;
using CMS.Membership;
using CMS.DataEngine;
using CMS.Modules;
using CMS.FormEngine;
using CMS.UIControls;

namespace CMS.OnlineForms.Web.UI
{
    /// <summary>
    /// Base page for the CMS BizForms pages to apply global settings to the pages.
    /// </summary>
    public abstract class CMSBizFormPage : CMSDeskPage
    {
        #region "Methods"

        /// <summary>
        /// Page OnInit event.
        /// </summary>
        /// <param name="e">Event args</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            CheckDocPermissions = false;

            // Check license
            if (!string.IsNullOrEmpty(RequestContext.CurrentDomain))
            {
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.BizForms);
            }

            // Check site availability
            if (!ResourceSiteInfoProvider.IsResourceOnSite("CMS.Form", SiteContext.CurrentSiteName))
            {
                RedirectToResourceNotAvailableOnSite("CMS.Form");
            }

            CurrentUserInfo user = MembershipContext.AuthenticatedUser;

            // Check permissions
            int formId = QueryHelper.GetInteger("formid", 0);
            if (formId <= 0)
            {
                return;
            }

            BizFormInfo bfi = BizFormInfoProvider.GetBizFormInfo(formId);
            if (bfi == null)
            {
                return;
            }

            // Check that the form is on current site and authorized roles for the form
            if (bfi.FormSiteID != SiteContext.CurrentSiteID || !bfi.IsFormAllowedForUser(user.UserName, SiteContext.CurrentSiteName))
            {
                RedirectToAccessDenied(ResHelper.GetString("Bizforms.FormNotAllowedForUserRoles"));
            }

            CheckAlternativeForm(bfi);
        }


        /// <summary>
        /// Checks if form and alternative form have same FormClassID parameter (<see cref="BizFormInfo.FormClassID"/>, <see cref="AlternativeFormInfo.FormClassID"/>).
        /// If not, alternative form does not belong to provided form and user is redirected to "Access Denied" page.
        /// </summary>
        /// <param name="bfi">Online form</param>
        private void CheckAlternativeForm(BizFormInfo bfi)
        {
            int altformid = QueryHelper.GetInteger("altformid", 0);
            if (altformid <= 0)
            {
                return;
            }

            var afi = AlternativeFormInfoProvider.GetAlternativeFormInfo(altformid);
            if (afi == null)
            {
                return;
            }

            // Forms and alternative forms are connected through FormClassID
            if (bfi.FormClassID != afi.FormClassID)
            {
                RedirectToAccessDenied(GetString("general.invalidid"));
            }
        }

        #endregion
    }
}