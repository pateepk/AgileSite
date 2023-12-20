using System;

using CMS.Core;
using CMS.Membership;
using CMS.UIControls;

namespace CMS.Reporting.Web.UI
{
    /// <summary>
    /// Base page for the E-commerce reports pages.
    /// </summary>
    [Security(Resource = "CMS.Reporting", ResourceSite = true)]
    public class CMSEcommerceReportsPage : CMSDeskPage
    {
        private bool mIsEcommerceReport = true;


        /// <summary>
        /// Gets or sets a value that indicates if the displayed report is an e-commerce report.
        /// </summary>
        protected bool IsEcommerceReport
        {
            get
            {
                return mIsEcommerceReport;
            }
            set
            {
                mIsEcommerceReport = value;
            }
        }


        /// <summary>
        /// OnInit event
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Check modules availability
            if (!ModuleEntryManager.IsModuleLoaded(ModuleName.REPORTING))
            {
                RedirectToUINotAvailable();
            }

            if (!ModuleEntryManager.IsModuleLoaded(ModuleName.ECOMMERCE))
            {
                RedirectToUINotAvailable();
            }

            // Check permissions
            CheckPermissions(IsEcommerceReport);
        }


        /// <summary>
        /// Checks permissions according to the report type.
        /// </summary>
        public static void CheckPermissions(bool isEcommerceReport)
        {
            var user = MembershipContext.AuthenticatedUser;

            // Check module permissions
            if (isEcommerceReport)
            {
                if (!user.IsAuthorizedPerResource(ModuleName.ECOMMERCE, "EcommerceRead") &&
                    !user.IsAuthorizedPerResource(ModuleName.ECOMMERCE, "ReadReports"))
                {
                    RedirectToAccessDenied(ModuleName.ECOMMERCE, "EcommerceRead OR ReadReports");
                }
            }
            else
            {
                if (!user.IsAuthorizedPerResource(ModuleName.REPORTING, "Read"))
                {
                    RedirectToAccessDenied(ModuleName.REPORTING, "Read");
                }
            }
        }
    }
}