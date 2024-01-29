using CMS.Core;
using CMS.Membership;

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Base page for the E-commerce shipping options.
    /// </summary>
    public class CMSShippingOptionsPage : CMSEcommercePage
    {
        /// <summary>
        /// Checks ecommerce ConfigurationModify permissions. Redirects to access denied page if check fails.
        /// </summary>
        protected void CheckConfigurationModification()
        {
            // Check 'ConfigurationModify' permission
            if (!MembershipContext.AuthenticatedUser.IsAuthorizedPerResource(ModuleName.ECOMMERCE, EcommercePermissions.CONFIGURATION_MODIFY))
            {
                RedirectToAccessDenied(ModuleName.ECOMMERCE, EcommercePermissions.CONFIGURATION_MODIFY);
            }
        }
    }
}